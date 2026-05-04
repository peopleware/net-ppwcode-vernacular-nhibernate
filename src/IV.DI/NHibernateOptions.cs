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

using Microsoft.Extensions.DependencyInjection;

using NHibernate;

using PPWCode.Util.Authorization.I;
using PPWCode.Util.Time.I;

namespace PPWCode.Vernacular.NHibernate.IV.DI;

/// <summary>
///     Configuration options used to register NHibernate services with the Microsoft
///     Dependency Injection container via <see cref="ServiceCollectionExtensions.AddNHibernate" />.
/// </summary>
/// <remarks>
///     <para>
///         Each <c>Use*</c> method registers the concrete implementation type that will be bound
///         to the corresponding NHibernate interface in the DI container.  Call
///         <see cref="ApplyDefaultsIfNotGiven" /> (done automatically by
///         <see cref="ServiceCollectionExtensions.AddNHibernate" />) to fill in built-in defaults
///         for every option that was left unset.
///     </para>
///     <para>
///         Four properties are mandatory and have no built-in default:
///         <see cref="MappingAssemblies" /> (set via <see cref="UseMappingAssemblies{T}" />),
///         <see cref="PpwHbmMapping" /> (set via <see cref="UseHbmMapping{T}" />),
///         <see cref="TimeProvider" /> (set via <see cref="UseTimeProvider{T,TTimestamp}" />), and
///         <see cref="IdentityProvider" /> (set via <see cref="UseIdentityProvider{T}" />).
///         Omitting any of them will cause <see cref="ServiceCollectionExtensions.AddNHibernate" /> to
///         throw an <see cref="PPWCode.Vernacular.Exceptions.V.Error" />.
///     </para>
/// </remarks>
public sealed class NHibernateOptions
{
    /// <summary>
    ///     The concrete type that implements <see cref="IExceptionTranslator" />.
    ///     Defaults to the built-in <c>ExceptionTranslator</c> when not set explicitly.
    /// </summary>
    public Type? ExceptionTranslator { get; private set; }

    /// <summary>
    ///     An optional concrete type that implements NHibernate's <see cref="IInterceptor" />.
    ///     When set, the interceptor is registered as a singleton and wired into the
    ///     <c>NhInterceptor</c> wrapper.  When <c>null</c>, no application-level interceptor
    ///     is attached to NHibernate sessions.
    /// </summary>
    public Type? Interceptor { get; private set; }

    /// <summary>
    ///     The transaction isolation level passed to the session provider.
    ///     Defaults to <see cref="System.Data.IsolationLevel.Unspecified" /> when not set
    ///     explicitly, letting the underlying ADO.NET driver choose the level.
    /// </summary>
    public IsolationLevel? IsolationLevel { get; private set; }

    /// <summary>
    ///     The DI <see cref="ServiceLifetime" /> used when registering <see cref="ISession" />
    ///     and <see cref="IStatelessSession" />.
    ///     Defaults to <see cref="ServiceLifetime.Scoped" /> when not set explicitly.
    /// </summary>
    public ServiceLifetime? SessionLifestyle { get; private set; }

    /// <summary>
    ///     The concrete type that implements <see cref="IMappingAssemblies" />.
    ///     <b>Mandatory</b> — must be supplied via <see cref="UseMappingAssemblies{T}" />.
    /// </summary>
    public Type? MappingAssemblies { get; private set; }

    /// <summary>
    ///     The concrete type that implements <see cref="INhConfiguration" />.
    ///     Defaults to the built-in <c>NhConfiguration</c> when not set explicitly.
    /// </summary>
    public Type? NhConfiguration { get; private set; }

    /// <summary>
    ///     The concrete type that implements <see cref="INHibernateSessionFactory" />.
    ///     Defaults to the built-in <c>NHibernateSessionFactory</c> when not set explicitly.
    /// </summary>
    public Type? NHibernateSessionFactory { get; private set; }

    /// <summary>
    ///     The concrete type that implements <see cref="INhProperties" />.
    ///     Defaults to the built-in <c>NhProperties</c> when not set explicitly.
    /// </summary>
    public Type? NhProperties { get; private set; }

    /// <summary>
    ///     The concrete type that implements <see cref="IPpwHbmMapping" />.
    ///     <b>Mandatory</b> — must be supplied via <see cref="UseHbmMapping{T}" />.
    /// </summary>
    public Type? PpwHbmMapping { get; private set; }

    /// <summary>
    ///     An optional concrete type that implements <see cref="IQueryOverCustomExpressions" />.
    ///     When set, the instance is resolved and its <c>Initialize</c> method is called once
    ///     during application startup to register custom QueryOver expression handlers.
    /// </summary>
    public Type? QueryOverCustomExpressions { get; private set; }

    /// <summary>
    ///     The concrete type that implements <see cref="ISafeEnvironmentProvider" />.
    ///     Mutually exclusive with <see cref="SafeEnvironmentProviderAsync" />; when neither is
    ///     set, defaults to the built-in async implementation.
    /// </summary>
    public Type? SafeEnvironmentProvider { get; private set; }

    /// <summary>
    ///     The concrete type that implements <see cref="ISafeEnvironmentProviderAsync" />.
    ///     When set, the async interface is registered and the synchronous
    ///     <see cref="ISafeEnvironmentProvider" /> interface is satisfied by the same instance.
    ///     Mutually exclusive with <see cref="SafeEnvironmentProvider" />.
    /// </summary>
    public Type? SafeEnvironmentProviderAsync { get; private set; }

    /// <summary>
    ///     The concrete type that implements <see cref="ISessionProvider" />.
    ///     Mutually exclusive with <see cref="SessionProviderAsync" />; when neither is set,
    ///     defaults to the built-in async implementation.
    /// </summary>
    public Type? SessionProvider { get; private set; }

    /// <summary>
    ///     The concrete type that implements <see cref="ISessionProviderAsync" />.
    ///     When set, the async interface is registered and the synchronous
    ///     <see cref="ISessionProvider" /> interface is satisfied by the same instance.
    ///     Mutually exclusive with <see cref="SessionProvider" />.
    /// </summary>
    public Type? SessionProviderAsync { get; private set; }

    /// <summary>
    ///     The concrete type that implements <see cref="ITimeProvider{T}" />.
    ///     <b>Mandatory</b> — must be supplied via <see cref="UseTimeProvider{T,TTimestamp}" />.
    /// </summary>
    public Type? TimeProvider { get; private set; }

    /// <summary>
    ///     The concrete type that implements <see cref="IIdentityProvider" />.
    ///     <b>Mandatory</b> — must be supplied via <see cref="UseIdentityProvider{T}" />.
    /// </summary>
    public Type? IdentityProvider { get; private set; }

    /// <summary>
    ///     The concrete type that implements <see cref="ITransactionProvider" />.
    ///     Mutually exclusive with <see cref="TransactionProviderAsync" />; when neither is set,
    ///     defaults to the built-in async implementation.
    /// </summary>
    public Type? TransactionProvider { get; private set; }

    /// <summary>
    ///     The concrete type that implements <see cref="ITransactionProviderAsync" />.
    ///     When set, the async interface is registered and the synchronous
    ///     <see cref="ITransactionProvider" /> interface is satisfied by the same instance.
    ///     Mutually exclusive with <see cref="TransactionProvider" />.
    /// </summary>
    public Type? TransactionProviderAsync { get; private set; }

    /// <summary>
    ///     Whether to register the built-in <c>CivilizedEventListener</c> as an
    ///     <see cref="IRegisterEventListener" />.  Defaults to <c>true</c>.
    /// </summary>
    public bool CivilizedEventListener { get; private set; } = true;

    /// <summary>
    ///     Registers a custom <see cref="IQueryOverCustomExpressions" /> implementation.
    ///     The instance is resolved at startup and its <c>Initialize</c> method is invoked once
    ///     to register application-specific QueryOver expression extensions.
    /// </summary>
    /// <typeparam name="T">
    ///     The concrete type that implements <see cref="IQueryOverCustomExpressions" />.
    /// </typeparam>
    /// <returns>The current <see cref="NHibernateOptions" /> instance for fluent chaining.</returns>
    public NHibernateOptions UseQueryOverCustomExpressions<T>()
        where T : IQueryOverCustomExpressions
    {
        QueryOverCustomExpressions = typeof(T);
        return this;
    }

    /// <summary>
    ///     Registers a synchronous <see cref="ITransactionProvider" /> implementation.
    ///     Use <see cref="UseTransactionProviderAsync{T}" /> when an async-capable provider
    ///     is preferred.
    /// </summary>
    /// <typeparam name="T">
    ///     The concrete type that implements <see cref="ITransactionProvider" />.
    /// </typeparam>
    /// <returns>The current <see cref="NHibernateOptions" /> instance for fluent chaining.</returns>
    public NHibernateOptions UseTransactionProvider<T>()
        where T : ITransactionProvider
    {
        TransactionProvider = typeof(T);
        return this;
    }

    /// <summary>
    ///     Registers an asynchronous <see cref="ITransactionProviderAsync" /> implementation.
    ///     The synchronous <see cref="ITransactionProvider" /> interface will be satisfied by
    ///     the same instance.
    /// </summary>
    /// <typeparam name="T">
    ///     The concrete type that implements <see cref="ITransactionProviderAsync" />.
    /// </typeparam>
    /// <returns>The current <see cref="NHibernateOptions" /> instance for fluent chaining.</returns>
    public NHibernateOptions UseTransactionProviderAsync<T>()
        where T : ITransactionProviderAsync
    {
        TransactionProviderAsync = typeof(T);
        return this;
    }

    /// <summary>
    ///     Registers a synchronous <see cref="ISafeEnvironmentProvider" /> implementation.
    ///     Use <see cref="UseSafeEnvironmentProviderAsync{T}" /> when an async-capable provider
    ///     is preferred.
    /// </summary>
    /// <typeparam name="T">
    ///     The concrete type that implements <see cref="ISafeEnvironmentProvider" />.
    /// </typeparam>
    /// <returns>The current <see cref="NHibernateOptions" /> instance for fluent chaining.</returns>
    public NHibernateOptions UseSafeEnvironmentProvider<T>()
        where T : ISafeEnvironmentProvider
    {
        SafeEnvironmentProvider = typeof(T);
        return this;
    }

    /// <summary>
    ///     Registers an asynchronous <see cref="ISafeEnvironmentProviderAsync" /> implementation.
    ///     The synchronous <see cref="ISafeEnvironmentProvider" /> interface will be satisfied
    ///     by the same instance.
    /// </summary>
    /// <typeparam name="T">
    ///     The concrete type that implements <see cref="ISafeEnvironmentProviderAsync" />.
    /// </typeparam>
    /// <returns>The current <see cref="NHibernateOptions" /> instance for fluent chaining.</returns>
    public NHibernateOptions UseSafeEnvironmentProviderAsync<T>()
        where T : ISafeEnvironmentProviderAsync
    {
        SafeEnvironmentProviderAsync = typeof(T);
        return this;
    }

    /// <summary>
    ///     Registers a custom <see cref="IExceptionTranslator" /> implementation that converts
    ///     database-level exceptions into domain-level exceptions.
    /// </summary>
    /// <typeparam name="T">
    ///     The concrete type that implements <see cref="IExceptionTranslator" />.
    /// </typeparam>
    /// <returns>The current <see cref="NHibernateOptions" /> instance for fluent chaining.</returns>
    public NHibernateOptions UseExceptionTranslator<T>()
        where T : IExceptionTranslator
    {
        ExceptionTranslator = typeof(T);
        return this;
    }

    /// <summary>
    ///     Registers a synchronous <see cref="ISessionProvider" /> implementation together with
    ///     the transaction isolation level to use when opening sessions.
    ///     Use <see cref="UseSessionProviderAsync{T}" /> when an async-capable provider is
    ///     preferred.
    /// </summary>
    /// <typeparam name="T">
    ///     The concrete type that implements <see cref="ISessionProvider" />.
    /// </typeparam>
    /// <param name="isolationLevel">
    ///     The isolation level for database transactions, or <c>null</c> to use the default
    ///     (<see cref="System.Data.IsolationLevel.Unspecified" />).
    /// </param>
    /// <returns>The current <see cref="NHibernateOptions" /> instance for fluent chaining.</returns>
    public NHibernateOptions UseSessionProvider<T>(IsolationLevel? isolationLevel)
        where T : ISessionProvider
    {
        SessionProvider = typeof(T);
        IsolationLevel = isolationLevel;
        return this;
    }

    /// <summary>
    ///     Registers an asynchronous <see cref="ISessionProviderAsync" /> implementation together
    ///     with the transaction isolation level to use when opening sessions.
    ///     The synchronous <see cref="ISessionProvider" /> interface will be satisfied by the
    ///     same instance.
    /// </summary>
    /// <typeparam name="T">
    ///     The concrete type that implements <see cref="ISessionProviderAsync" />.
    /// </typeparam>
    /// <param name="isolationLevel">
    ///     The isolation level for database transactions, or <c>null</c> to use the default
    ///     (<see cref="System.Data.IsolationLevel.Unspecified" />).
    /// </param>
    /// <returns>The current <see cref="NHibernateOptions" /> instance for fluent chaining.</returns>
    public NHibernateOptions UseSessionProviderAsync<T>(IsolationLevel? isolationLevel)
        where T : ISessionProviderAsync
    {
        SessionProviderAsync = typeof(T);
        IsolationLevel = isolationLevel;
        return this;
    }

    /// <summary>
    ///     Controls whether the built-in <c>CivilizedEventListener</c> is registered.
    ///     Pass <c>false</c> to opt out, for example when the application provides its own
    ///     NHibernate event listeners that subsume the civilized-check behaviour.
    /// </summary>
    /// <param name="usage"><c>true</c> to enable (the default); <c>false</c> to disable.</param>
    /// <returns>The current <see cref="NHibernateOptions" /> instance for fluent chaining.</returns>
    public NHibernateOptions UseCivilizedEventListener(bool usage)
    {
        CivilizedEventListener = usage;
        return this;
    }

    /// <summary>
    ///     Registers the <see cref="IPpwHbmMapping" /> implementation that builds the HBM
    ///     (Hibernate Mapping by Code) configuration used to map domain entities.
    ///     <b>Mandatory.</b>
    /// </summary>
    /// <typeparam name="T">
    ///     The concrete type that implements <see cref="IPpwHbmMapping" />.
    /// </typeparam>
    /// <returns>The current <see cref="NHibernateOptions" /> instance for fluent chaining.</returns>
    public NHibernateOptions UseHbmMapping<T>()
        where T : IPpwHbmMapping
    {
        PpwHbmMapping = typeof(T);
        return this;
    }

    /// <summary>
    ///     Registers a custom <see cref="INhProperties" /> implementation that supplies
    ///     low-level NHibernate configuration properties (connection string, dialect, etc.).
    /// </summary>
    /// <typeparam name="T">
    ///     The concrete type that implements <see cref="INhProperties" />.
    /// </typeparam>
    /// <returns>The current <see cref="NHibernateOptions" /> instance for fluent chaining.</returns>
    public NHibernateOptions UseNhProperties<T>()
        where T : INhProperties
    {
        NhProperties = typeof(T);
        return this;
    }

    /// <summary>
    ///     Registers a custom NHibernate <see cref="IInterceptor" /> that will be attached to
    ///     every session opened by the session factory.
    /// </summary>
    /// <typeparam name="T">
    ///     The concrete type that implements <see cref="IInterceptor" />.
    /// </typeparam>
    /// <returns>The current <see cref="NHibernateOptions" /> instance for fluent chaining.</returns>
    public NHibernateOptions UseInterceptor<T>()
        where T : IInterceptor
    {
        Interceptor = typeof(T);
        return this;
    }

    /// <summary>
    ///     Registers a custom <see cref="INhConfiguration" /> implementation that builds and
    ///     supplies the <c>NHibernate.Cfg.Configuration</c> object.
    /// </summary>
    /// <typeparam name="T">
    ///     The concrete type that implements <see cref="INhConfiguration" />.
    /// </typeparam>
    /// <returns>The current <see cref="NHibernateOptions" /> instance for fluent chaining.</returns>
    public NHibernateOptions UseConfiguration<T>()
        where T : INhConfiguration
    {
        NhConfiguration = typeof(T);
        return this;
    }

    /// <summary>
    ///     Registers a custom <see cref="INHibernateSessionFactory" /> implementation that
    ///     wraps or replaces the built-in NHibernate <c>ISessionFactory</c> creation logic.
    /// </summary>
    /// <typeparam name="T">
    ///     The concrete type that implements <see cref="INHibernateSessionFactory" />.
    /// </typeparam>
    /// <returns>The current <see cref="NHibernateOptions" /> instance for fluent chaining.</returns>
    public NHibernateOptions UseNHibernateSessionFactory<T>()
        where T : INHibernateSessionFactory
    {
        NHibernateSessionFactory = typeof(T);
        return this;
    }

    /// <summary>
    ///     Registers the <see cref="IMappingAssemblies" /> implementation that enumerates the
    ///     assemblies scanned for NHibernate mappings and event listeners.
    ///     <b>Mandatory.</b>
    /// </summary>
    /// <typeparam name="T">
    ///     The concrete type that implements <see cref="IMappingAssemblies" />.
    /// </typeparam>
    /// <returns>The current <see cref="NHibernateOptions" /> instance for fluent chaining.</returns>
    public NHibernateOptions UseMappingAssemblies<T>()
        where T : IMappingAssemblies
    {
        MappingAssemblies = typeof(T);
        return this;
    }

    /// <summary>
    ///     Overrides the DI lifetime used when registering <see cref="ISession" /> and
    ///     <see cref="IStatelessSession" />.
    ///     The default is <see cref="ServiceLifetime.Scoped" />, which is appropriate for
    ///     web/API applications where one session per HTTP request is desired.
    /// </summary>
    /// <param name="lifestyleType">The desired <see cref="ServiceLifetime" />.</param>
    /// <returns>The current <see cref="NHibernateOptions" /> instance for fluent chaining.</returns>
    public NHibernateOptions UseLifestyleTypeForSessions(ServiceLifetime lifestyleType)
    {
        SessionLifestyle = lifestyleType;
        return this;
    }

    /// <summary>
    ///     Registers the <see cref="ITimeProvider{TTimestamp}" /> implementation that supplies the
    ///     current timestamp for auditing purposes.
    ///     <b>Mandatory.</b>
    /// </summary>
    /// <typeparam name="T">
    ///     The concrete type that implements <see cref="ITimeProvider{TTimestamp}" />.
    /// </typeparam>
    /// <typeparam name="TTimestamp">
    ///     The timestamp type used by the <see cref="ITimeProvider{TTimestamp}" /> implementation.
    /// </typeparam>
    /// <returns>The current <see cref="NHibernateOptions" /> instance for fluent chaining.</returns>
    public NHibernateOptions UseTimeProvider<T, TTimestamp>()
        where T : ITimeProvider<TTimestamp>
        where TTimestamp : struct, IComparable<TTimestamp>, IEquatable<TTimestamp>
    {
        TimeProvider = typeof(T);
        return this;
    }

    /// <summary>
    ///     Registers the <see cref="IIdentityProvider" /> implementation that supplies the
    ///     current user identity for auditing purposes.
    ///     <b>Mandatory.</b>
    /// </summary>
    /// <typeparam name="T">
    ///     The concrete type that implements <see cref="IIdentityProvider" />.
    /// </typeparam>
    /// <returns>The current <see cref="NHibernateOptions" /> instance for fluent chaining.</returns>
    public NHibernateOptions UseIdentityProvider<T>()
        where T : IIdentityProvider
    {
        IdentityProvider = typeof(T);
        return this;
    }

    /// <summary>
    ///     Fills in built-in defaults for every option that was not set explicitly.
    ///     This method is called automatically by
    ///     <see cref="ServiceCollectionExtensions.AddNHibernate" /> and does not normally need
    ///     to be called by application code directly.
    /// </summary>
    /// <remarks>
    ///     Applied defaults (in order of precedence):
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="TransactionProviderAsync" /> — <c>TransactionProviderAsync</c>
    ///                 (async), unless a sync or async provider was already configured.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="SafeEnvironmentProviderAsync" /> — <c>SafeEnvironmentProviderAsync</c>
    ///                 (async), unless a sync or async provider was already configured.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="SessionProviderAsync" /> — <c>SessionProviderAsync</c> (async),
    ///                 unless a sync or async provider was already configured.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ExceptionTranslator" />, <see cref="NhProperties" />,
    ///                 <see cref="NhConfiguration" />, <see cref="NHibernateSessionFactory" /> —
    ///                 each falls back to its corresponding built-in implementation.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="SessionLifestyle" /> — <see cref="ServiceLifetime.Scoped" />.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="IsolationLevel" /> — <see cref="System.Data.IsolationLevel.Unspecified" />.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
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
