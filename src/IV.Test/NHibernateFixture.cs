// Copyright 2026 by PeopleWare n.v..
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
using System.Threading;
using System.Threading.Tasks;

using HibernatingRhinos.Profiler.Appender;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Moq;

using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

using PPWCode.Util.Authorization.I;
using PPWCode.Util.Time.I;

using Serilog;

namespace PPWCode.Vernacular.NHibernate.IV.Test
{
    public abstract class NHibernateFixture<TId>
        : BaseFixture
        where TId : IEquatable<TId>
    {
        private AppSettings? _appSettings;
        private ISessionFactory? _sessionFactory;
        private ISessionProvider? _sessionProvider;
        private ISessionProviderAsync? _sessionProviderAsync;

        protected abstract Configuration Configuration { get; }
        protected abstract string IdentityName { get; }
        protected abstract DateTime UtcNow { get; }

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

        protected virtual string? FixedConnectionString
            => _appSettings?.FixedConnectionString;

        protected virtual ISessionFactory SessionFactory
            => _sessionFactory ??= Configuration.BuildSessionFactory();

        protected virtual ISessionProvider SessionProvider
            => _sessionProvider ??=
                   new SessionProvider(
                       OpenSession(),
                       new TransactionProvider(),
                       new SafeEnvironmentProvider(new ExceptionTranslator()),
                       IsolationLevel.ReadCommitted);

        protected virtual ITransactionProvider TransactionProvider
            => SessionProvider.TransactionProvider;

        protected virtual ISessionProviderAsync SessionProviderAsync
            => _sessionProviderAsync ??=
                   new SessionProviderAsync(
                       OpenSession(),
                       new TransactionProviderAsync(),
                       new SafeEnvironmentProviderAsync(new ExceptionTranslator()),
                       IsolationLevel.ReadCommitted);

        protected virtual ITransactionProviderAsync TransactionProviderAsync
            => SessionProviderAsync.TransactionProviderAsync;

        /// <inheritdoc />
        protected override void OnFixtureSetup()
        {
            // 1. Build configuration
            IConfiguration config =
                new ConfigurationBuilder()
                    .AddJsonFile(@"appsettings.json", false, false)
                    .AddEnvironmentVariables(@"PPWCODE_")
                    .Build();
            _appSettings = new AppSettings();
            config
                .GetSection(@"appSettings")
                .Bind(_appSettings);

            // 2. Setup Serilog to talk to NUnit
            Log.Logger = new LoggerConfiguration()
                .ReadFrom
                .Configuration(config) // Reads levels from JSON
                .Enrich.FromLogContext()
                .CreateLogger();

            // 3. Wire our nHibernate packages logging to Serilog
            PPWLogging.Factory = LoggerFactory.Create(builder =>
            {
                // Tell Microsoft Logging to just pass everything to Serilog
                builder.AddSerilog(dispose: true);
            });

            // 4 Wire nHibernate itself to use the same logger factory
            PPWLogging.Factory.UseAsNHibernateLoggerProvider();
        }

        /// <inheritdoc />
        protected override void OnFixtureTeardown()
        {
            _appSettings = null;
        }

        protected virtual ISession OpenSession()
        {
            Mock<IIdentityProvider> identityProvider = new();
            identityProvider
                .Setup(ip => ip.IdentityName)
                .Returns(IdentityName);

            Mock<ITimeProvider<DateTime>> timeProvider = new();
            timeProvider
                .Setup(tp => tp.Now)
                .Returns(UtcNow.ToLocalTime);
            timeProvider
                .Setup(tp => tp.UtcNow)
                .Returns(UtcNow);

            TestAuditInterceptor<TId> sessionLocalInterceptor = new(timeProvider.Object, true, identityProvider.Object);

            return
                SessionFactory
                    .WithOptions()
                    .Interceptor(sessionLocalInterceptor)
                    .OpenSession();
        }

        protected virtual void BuildSchema()
        {
            SchemaExport schemaExport = new(Configuration);
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

        protected T? RunInsideTransaction<T>(Func<T> func, bool clearSession)
        {
            T? result = TransactionProvider.Run(SessionProvider.Session, SessionProvider.IsolationLevel, func);

            if (clearSession)
            {
                SessionProvider.Session.Clear();
            }

            return result;
        }

        protected void RunInsideTransaction(Action action, bool clearSession)
        {
            TransactionProvider.Run(SessionProvider.Session, SessionProvider.IsolationLevel, action);

            if (clearSession)
            {
                SessionProvider.Session.Clear();
            }
        }

        protected async Task<T?> RunInsideTransactionAsync<T>(
            Func<CancellationToken, Task<T?>> lambda,
            bool clearSession,
            CancellationToken cancellationToken)
        {
            T? result =
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

        protected async Task RunInsideTransactionAsync(
            Func<CancellationToken, Task> lambda,
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
            public bool UseProfiler { get; set; }
            public bool SuppressProfilingWhileCreatingSchema { get; set; }
            public bool ShowSql { get; set; }
            public bool FormatSql { get; set; }
            public bool GenerateStatistics { get; set; }
            public string? FixedConnectionString { get; set; }

            /// <inheritdoc />
            public override string ToString()
                => $"{nameof(UseProfiler)}: {UseProfiler}, {nameof(SuppressProfilingWhileCreatingSchema)}: {SuppressProfilingWhileCreatingSchema}, {nameof(ShowSql)}: {ShowSql}, {nameof(FormatSql)}: {FormatSql}, {nameof(GenerateStatistics)}: {GenerateStatistics}";
        }
    }
}
