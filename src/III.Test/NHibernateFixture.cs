﻿// Copyright 2017 by PeopleWare n.v..
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Common.Logging;

using HibernatingRhinos.Profiler.Appender;

using JetBrains.Annotations;

using Microsoft.Extensions.Configuration;

using Moq;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

using PPWCode.Log4Net.Adapter;
using PPWCode.Vernacular.NHibernate.III.Async.Implementations.Providers;
using PPWCode.Vernacular.NHibernate.III.Async.Interfaces.Providers;
using PPWCode.Vernacular.NHibernate.III.DbConstraint;
using PPWCode.Vernacular.NHibernate.III.Providers;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.Test
{
    public abstract class NHibernateFixture<TId>
        : BaseFixture
        where TId : IEquatable<TId>
    {
        [CanBeNull]
        private ISessionFactory _sessionFactory;

        [CanBeNull]
        private ISessionProvider _sessionProvider;

        [CanBeNull]
        private ISessionProviderAsync _sessionProviderAsync;

        [CanBeNull]
        private AppSettings _appSettings;

        protected abstract Configuration Configuration { get; }
        protected abstract string IdentityName { get; }
        protected abstract DateTime UtcNow { get; }

        /// <inheritdoc />
        protected override void OnFixtureSetup()
        {
            IConfiguration config =
                new ConfigurationBuilder()
                    .AddJsonFile(@"appsettings.json", false, false)
                    .AddEnvironmentVariables(@"PPWCODE_TESTS")
                    .Build();
            _appSettings = new AppSettings();
            config
                .GetSection(@"appSettings")
                .Bind(_appSettings);

            log4net.Util.LogLog.InternalDebugging = true;
            LogManager.Adapter =
                new Log4NetLoggerFactoryAdapter(
                    Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly(),
                    null);
        }

        /// <inheritdoc />
        protected override void OnFixtureTeardown()
        {
            _appSettings = null;
        }

        protected virtual bool UseProfiler
            => _appSettings?.UseProfiler ?? false;

        protected virtual bool SuppressProfilingWhileCreatingSchema
            => _appSettings?.SuppressProfilingWhileCreatingSchema ?? true;

        protected virtual bool ShowSql
            => _appSettings?.ShowSql ?? false;

        protected virtual bool FormatSql
            => _appSettings?.FormatSql ?? false;

        protected virtual bool GenerateStatistics
            => _appSettings?.GenerateStatistics ?? true;

        [CanBeNull]
        protected virtual string FixedConnectionString
            => _appSettings?.FixedConnectionString;

        [NotNull]
        protected virtual ISessionFactory SessionFactory
            => _sessionFactory ?? (_sessionFactory = Configuration.BuildSessionFactory());

        [NotNull]
        protected virtual ISession OpenSession()
        {
            Mock<IIdentityProvider> identityProvider = new Mock<IIdentityProvider>();
            identityProvider
                .Setup(ip => ip.IdentityName)
                .Returns(IdentityName);

            Mock<ITimeProvider> timeProvider = new Mock<ITimeProvider>();
            timeProvider
                .Setup(tp => tp.Now)
                .Returns(UtcNow.ToLocalTime);
            timeProvider
                .Setup(tp => tp.UtcNow)
                .Returns(UtcNow);

            AuditInterceptor<TId> sessionLocalInterceptor = new AuditInterceptor<TId>(identityProvider.Object, timeProvider.Object, true);

            return
                SessionFactory
                    .WithOptions()
                    .Interceptor(sessionLocalInterceptor)
                    .OpenSession();
        }

        [NotNull]
        protected virtual ISessionProvider SessionProvider
            => _sessionProvider
               ?? (_sessionProvider =
                       new SessionProvider(
                           OpenSession(),
                           new TransactionProvider(),
                           new SafeEnvironmentProvider(new ExceptionTranslator()),
                           IsolationLevel.ReadCommitted));

        [NotNull]
        protected virtual ITransactionProvider TransactionProvider
            => SessionProvider.TransactionProvider;

        [NotNull]
        protected virtual ISessionProviderAsync SessionProviderAsync
            => _sessionProviderAsync
               ?? (_sessionProviderAsync =
                       new SessionProviderAsync(
                           OpenSession(),
                           new TransactionProviderAsync(),
                           new SafeEnvironmentProviderAsync(new ExceptionTranslator()),
                           IsolationLevel.ReadCommitted));

        [NotNull]
        protected virtual ITransactionProviderAsync TransactionProviderAsync
            => SessionProviderAsync.TransactionProviderAsync;

        protected virtual void BuildSchema()
        {
            SchemaExport schemaExport = new SchemaExport(Configuration);
            if (UseProfiler && SuppressProfilingWhileCreatingSchema)
            {
                using (ProfilerIntegration.IgnoreAll())
                {
                    schemaExport.Create(false, true);
                }
            }
            else
            {
                schemaExport.Create(false, true);
            }
        }

        protected virtual void CloseSessionFactory()
        {
            _sessionFactory?.Close();
            _sessionFactory = null;
        }

        protected virtual void CloseSession()
        {
            _sessionProvider?.Session.Close();
            _sessionProvider = null;

            _sessionProviderAsync?.Session.Close();
            _sessionProviderAsync = null;
        }

        [CanBeNull]
        protected T RunInsideTransaction<T>([NotNull] Func<T> func, bool clearSession)
        {
            T result =
                TransactionProvider
                    .Run(SessionProvider.Session, SessionProvider.IsolationLevel, func);

            if (clearSession)
            {
                SessionProvider.Session.Clear();
            }

            return result;
        }

        protected void RunInsideTransaction([NotNull] Action action, bool clearSession)
        {
            TransactionProvider
                .Run(SessionProvider.Session, SessionProvider.IsolationLevel, action);

            if (clearSession)
            {
                SessionProvider.Session.Clear();
            }
        }

        [NotNull]
        [ItemCanBeNull]
        protected async Task<T> RunInsideTransactionAsync<T>(
            [NotNull] Func<CancellationToken, Task<T>> lambda,
            bool clearSession,
            CancellationToken cancellationToken)
        {
            T result =
                await TransactionProviderAsync
                    .RunAsync(
                        SessionProviderAsync.Session,
                        SessionProviderAsync.IsolationLevel,
                        lambda,
                        cancellationToken)
                    .ConfigureAwait(false);

            if (clearSession)
            {
                SessionProviderAsync.Session.Clear();
            }

            return result;
        }

        [NotNull]
        protected async Task RunInsideTransactionAsync(
            [NotNull] Func<CancellationToken, Task> lambda,
            bool clearSession,
            CancellationToken cancellationToken)
        {
            await TransactionProviderAsync
                .RunAsync(
                    SessionProviderAsync.Session,
                    SessionProviderAsync.IsolationLevel,
                    lambda,
                    cancellationToken)
                .ConfigureAwait(false);

            if (clearSession)
            {
                SessionProviderAsync.Session.Clear();
            }
        }

        protected class AppSettings
        {
            [UsedImplicitly]
            public bool UseProfiler { get; set; }

            [UsedImplicitly]
            public bool SuppressProfilingWhileCreatingSchema { get; set; }

            [UsedImplicitly]
            public bool ShowSql { get; set; }

            [UsedImplicitly]
            public bool FormatSql { get; set; }

            [UsedImplicitly]
            public bool GenerateStatistics { get; set; }

            [UsedImplicitly]
            [CanBeNull]
            public string FixedConnectionString { get; set; }

            /// <inheritdoc />
            public override string ToString()
                => $"{nameof(UseProfiler)}: {UseProfiler}, {nameof(SuppressProfilingWhileCreatingSchema)}: {SuppressProfilingWhileCreatingSchema}, {nameof(ShowSql)}: {ShowSql}, {nameof(FormatSql)}: {FormatSql}, {nameof(GenerateStatistics)}: {GenerateStatistics}";
        }
    }
}
