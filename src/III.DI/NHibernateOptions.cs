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

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

using NHibernate;

using PPWCode.Vernacular.NHibernate.III.Async.Implementations.Providers;
using PPWCode.Vernacular.NHibernate.III.Async.Interfaces.Providers;
using PPWCode.Vernacular.NHibernate.III.DbConstraint;
using PPWCode.Vernacular.NHibernate.III.Providers;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.DI;

public sealed class NHibernateOptions
{
    [CanBeNull]
    public Type ExceptionTranslator { get; private set; }

    [CanBeNull]
    public Type Interceptor { get; private set; }

    public IsolationLevel? IsolationLevel { get; private set; }
    public ServiceLifetime? SessionLifestyle { get; private set; }

    [CanBeNull]
    public Type MappingAssemblies { get; private set; }

    [CanBeNull]
    public Type NhConfiguration { get; private set; }

    [CanBeNull]
    public Type NHibernateSessionFactory { get; private set; }

    [CanBeNull]
    public Type NhProperties { get; private set; }

    [CanBeNull]
    public Type PpwHbmMapping { get; private set; }

    [CanBeNull]
    public Type QueryOverCustomExpressions { get; private set; }

    [CanBeNull]
    public Type SafeEnvironmentProvider { get; private set; }

    [CanBeNull]
    public Type SafeEnvironmentProviderAsync { get; private set; }

    [CanBeNull]
    public Type SessionProvider { get; private set; }

    [CanBeNull]
    public Type SessionProviderAsync { get; private set; }

    [CanBeNull]
    public Type TimeProvider { get; private set; }

    [CanBeNull]
    public Type IdentityProvider { get; private set; }

    [CanBeNull]
    public Type TransactionProvider { get; private set; }

    [CanBeNull]
    public Type TransactionProviderAsync { get; private set; }

    public bool CivilizedEventListener { get; private set; } = true;

    public NHibernateOptions UseQueryOverCustomExpressions<T>()
        where T : IQueryOverCustomExpressions
    {
        QueryOverCustomExpressions = typeof(T);
        return this;
    }

    public NHibernateOptions UseTransactionProvider<T>()
        where T : ITransactionProvider
    {
        TransactionProvider = typeof(T);
        return this;
    }

    public NHibernateOptions UseTransactionProviderAsync<T>()
        where T : ITransactionProviderAsync
    {
        TransactionProviderAsync = typeof(T);
        return this;
    }

    public NHibernateOptions UseSafeEnvironmentProvider<T>()
        where T : ISafeEnvironmentProvider
    {
        SafeEnvironmentProvider = typeof(T);
        return this;
    }

    public NHibernateOptions UseSafeEnvironmentProviderAsync<T>()
        where T : ISafeEnvironmentProviderAsync
    {
        SafeEnvironmentProviderAsync = typeof(T);
        return this;
    }

    public NHibernateOptions UseExceptionTranslator<T>()
        where T : IExceptionTranslator
    {
        ExceptionTranslator = typeof(T);
        return this;
    }

    public NHibernateOptions UseSessionProvider<T>(IsolationLevel? isolationLevel)
        where T : ISessionProvider
    {
        SessionProvider = typeof(T);
        IsolationLevel = isolationLevel;
        return this;
    }

    public NHibernateOptions UseSessionProviderAsync<T>(IsolationLevel? isolationLevel)
        where T : ISessionProviderAsync
    {
        SessionProviderAsync = typeof(T);
        IsolationLevel = isolationLevel;
        return this;
    }

    public NHibernateOptions UseCivilizedEventListener(bool usage)
    {
        CivilizedEventListener = usage;
        return this;
    }

    public NHibernateOptions UseHbmMapping<T>()
        where T : IPpwHbmMapping
    {
        PpwHbmMapping = typeof(T);
        return this;
    }

    public NHibernateOptions UseNhProperties<T>()
        where T : INhProperties
    {
        NhProperties = typeof(T);
        return this;
    }

    public NHibernateOptions UseInterceptor<T>()
        where T : IInterceptor
    {
        Interceptor = typeof(T);
        return this;
    }

    public NHibernateOptions UseConfiguration<T>()
        where T : INhConfiguration
    {
        NhConfiguration = typeof(T);
        return this;
    }

    public NHibernateOptions UseNHibernateSessionFactory<T>()
        where T : INHibernateSessionFactory
    {
        NHibernateSessionFactory = typeof(T);
        return this;
    }

    public NHibernateOptions UseMappingAssemblies<T>()
        where T : IMappingAssemblies
    {
        MappingAssemblies = typeof(T);
        return this;
    }

    public NHibernateOptions UseLifestyleTypeForSessions(ServiceLifetime lifestyleType)
    {
        SessionLifestyle = lifestyleType;
        return this;
    }

    public NHibernateOptions UseTimeProvider<T>()
        where T : ITimeProvider
    {
        TimeProvider = typeof(T);
        return this;
    }

    public NHibernateOptions UseIdentityProvider<T>()
        where T : IIdentityProvider
    {
        IdentityProvider = typeof(T);
        return this;
    }

    public void ApplyDefaultsIfNotGiven()
    {
        if ((TransactionProviderAsync == null) && (TransactionProvider == null))
        {
            TransactionProviderAsync = typeof(TransactionProviderAsync);
        }

        if ((SafeEnvironmentProviderAsync == null) && (SafeEnvironmentProvider == null))
        {
            SafeEnvironmentProviderAsync = typeof(SafeEnvironmentProviderAsync);
        }

        if ((SessionProviderAsync == null) && (SessionProvider == null))
        {
            SessionProviderAsync = typeof(SessionProviderAsync);
        }

        ExceptionTranslator ??= typeof(ExceptionTranslator);
        NhProperties ??= typeof(NhProperties);
        NhConfiguration ??= typeof(NhConfiguration);
        NHibernateSessionFactory ??= typeof(NHibernateSessionFactory);
        SessionLifestyle ??= ServiceLifetime.Scoped;
        IsolationLevel ??= System.Data.IsolationLevel.Unspecified;
    }
}
