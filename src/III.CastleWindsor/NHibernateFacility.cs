﻿// Copyright 2018 by PeopleWare n.v..
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

using Castle.Core;
using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;

using NHibernate;
using NHibernate.Mapping;

using PPWCode.Vernacular.Exceptions.IV;
using PPWCode.Vernacular.NHibernate.III.DbConstraint;
using PPWCode.Vernacular.NHibernate.III.Providers;
using PPWCode.Vernacular.Persistence.IV;

using Component = Castle.MicroKernel.Registration.Component;

namespace PPWCode.Vernacular.NHibernate.III.CastleWindsor
{
    public class NHibernateFacility : AbstractFacility
    {
        private Type _exceptionTranslator;
        private Type _interceptor;
        private IsolationLevel? _isolationLevel;
        private LifestyleType? _lifestyleType;
        private Type _mappingAssemblies;
        private Type _nhConfiguration;
        private Type _nhibernateSessionFactory;
        private Type _nhProperties;
        private Type _ppwHbmMapping;
        private Type _queryOverCustomExpressions;
        private Type _safeEnvironmentProvider;
        private Type _sessionProvider;
        private Type _transactionProvider;
        private bool _useCivilizedEventListener = true;

        public NHibernateFacility UseQueryOverCustomExpressions<T>()
            where T : IQueryOverCustomExpressions
        {
            _queryOverCustomExpressions = typeof(T);
            return this;
        }

        public NHibernateFacility UseTransactionProvider<T>()
            where T : ITransactionProvider
        {
            _transactionProvider = typeof(T);
            return this;
        }

        public NHibernateFacility UseSafeEnvironmentProvider<T>()
            where T : ISafeEnvironmentProvider
        {
            _safeEnvironmentProvider = typeof(T);
            return this;
        }

        public NHibernateFacility UseExceptionTranslator<T>()
            where T : IExceptionTranslator
        {
            _exceptionTranslator = typeof(T);
            return this;
        }

        public NHibernateFacility UseSessionProvider<T>(IsolationLevel? isolationLevel)
            where T : ISessionProvider
        {
            _sessionProvider = typeof(T);
            _isolationLevel = isolationLevel;
            return this;
        }

        public NHibernateFacility UseCivilizedEventListener(bool usage)
        {
            _useCivilizedEventListener = usage;
            return this;
        }

        public NHibernateFacility UseHbmMapping<T>()
            where T : IPpwHbmMapping
        {
            _ppwHbmMapping = typeof(T);
            return this;
        }

        public NHibernateFacility UseNhProperties<T>()
            where T : INhProperties
        {
            _nhProperties = typeof(T);
            return this;
        }

        public NHibernateFacility UseInterceptor<T>()
            where T : IInterceptor
        {
            _interceptor = typeof(T);
            return this;
        }

        public NHibernateFacility UseConfiguration<T>()
            where T : INhConfiguration
        {
            _nhConfiguration = typeof(T);
            return this;
        }

        public NHibernateFacility UseNHibernateSessionFactory<T>()
            where T : INHibernateSessionFactory
        {
            _nhibernateSessionFactory = typeof(T);
            return this;
        }

        public NHibernateFacility UseMappingAssemblies<T>()
            where T : IMappingAssemblies
        {
            _mappingAssemblies = typeof(T);
            return this;
        }

        public NHibernateFacility UseLifestyleTypeForSessions(LifestyleType lifestyleType)
        {
            _lifestyleType = lifestyleType;
            return this;
        }

        /// <inheritdoc />
        protected override void Init()
        {
            if (_ppwHbmMapping == null)
            {
                throw new Error($"You must supply a dependency of type {nameof(IPpwHbmMapping)}, using the {nameof(UseHbmMapping)} method.");
            }

            if (_mappingAssemblies == null)
            {
                throw new Error($"You must supply a dependency of type {nameof(IMappingAssemblies)}, using the {nameof(UseMappingAssemblies)} method.");
            }

            Kernel.AddSubResolverConditionally(k => new CollectionResolver(k, true));

            Kernel
                .Register(
                    Component
                        .For<IMappingAssemblies>()
                        .ImplementedBy(_mappingAssemblies)
                        .LifeStyle.Singleton,
                    Component
                        .For<IPpwHbmMapping>()
                        .ImplementedBy(_ppwHbmMapping)
                        .LifeStyle.Singleton,
                    Component
                        .For<INhInterceptor>()
                        .ImplementedBy<NhInterceptor>()
                        .LifeStyle.Singleton);

            IMappingAssemblies mappingAssemblies = Kernel.Resolve<IMappingAssemblies>();
            foreach (Assembly assembly in mappingAssemblies.GetAssemblies())
            {
                Kernel
                    .Register(
                        Classes
                            .FromAssembly(assembly)
                            .BasedOn<IRegisterEventListener>()
                            .WithService.Base()
                            .LifestyleSingleton(),
                        Classes
                            .FromAssembly(assembly)
                            .BasedOn<IAuxiliaryDatabaseObject>()
                            .WithService.Base()
                            .LifestyleSingleton());
            }

            Type transactionProvider = _transactionProvider ?? typeof(TransactionProvider);
            Kernel
                .Register(
                    Component
                        .For<ITransactionProvider>()
                        .ImplementedBy(transactionProvider)
                        .LifeStyle.Singleton);

            Type exceptionTranslator = _exceptionTranslator ?? typeof(ExceptionTranslator);
            Kernel
                .Register(
                    Component
                        .For<IExceptionTranslator>()
                        .ImplementedBy(exceptionTranslator)
                        .LifeStyle.Singleton);

            Type safeEnvironmentProvider = _safeEnvironmentProvider ?? typeof(SafeEnvironmentProvider);
            Kernel
                .Register(
                    Component
                        .For<ISafeEnvironmentProvider>()
                        .ImplementedBy(safeEnvironmentProvider)
                        .LifeStyle.Singleton);

            if (_useCivilizedEventListener && !Kernel.HasComponent(typeof(CivilizedEventListener)))
            {
                Kernel
                    .Register(
                        Component
                            .For<IRegisterEventListener>()
                            .ImplementedBy<CivilizedEventListener>()
                            .LifeStyle.Singleton);
            }

            Type nhProperties = _nhProperties ?? typeof(EmptyNhProperties);
            Kernel
                .Register(
                    Component
                        .For<INhProperties>()
                        .ImplementedBy(nhProperties)
                        .LifeStyle.Singleton);

            if (_interceptor != null)
            {
                Kernel
                    .Register(
                        Component
                            .For<IInterceptor>()
                            .ImplementedBy(_interceptor)
                            .LifeStyle.Singleton);
            }

            Type configuration = _nhConfiguration ?? typeof(NhConfiguration);
            Kernel
                .Register(
                    Component
                        .For<INhConfiguration>()
                        .ImplementedBy(configuration)
                        .LifeStyle.Singleton);

            LifestyleType lifestyleType = _lifestyleType ?? LifestyleType.Singleton;
            Type nhibernateSessionFactory = _nhibernateSessionFactory ?? typeof(NHibernateSessionFactory);
            Kernel
                .Register(
                    Component
                        .For<INHibernateSessionFactory>()
                        .ImplementedBy(nhibernateSessionFactory)
                        .LifeStyle.Singleton,
                    Component
                        .For<ISession>()
                        .UsingFactoryMethod(
                            _ =>
                            {
                                INHibernateSessionFactory sessionFactoryFactory = Kernel.Resolve<INHibernateSessionFactory>();
                                return sessionFactoryFactory.SessionFactory.OpenSession();
                            })
                        .LifeStyle.Is(lifestyleType),
                    Component
                        .For<IStatelessSession>()
                        .UsingFactoryMethod(
                            _ =>
                            {
                                INHibernateSessionFactory sessionFactoryFactory = Kernel.Resolve<INHibernateSessionFactory>();
                                return sessionFactoryFactory.SessionFactory.OpenStatelessSession();
                            })
                        .LifeStyle.Is(lifestyleType));

            Type sessionProvider = _sessionProvider ?? typeof(SessionProvider);
            IsolationLevel isolationLevel = _isolationLevel ?? IsolationLevel.Unspecified;
            Kernel
                .Register(
                    Component
                        .For<ISessionProvider>()
                        .ImplementedBy(sessionProvider)
                        .DependsOn(Dependency.OnValue<IsolationLevel>(isolationLevel))
                        .LifeStyle.Transient);

            if (_queryOverCustomExpressions != null)
            {
                Kernel
                    .Register(
                        Component
                            .For<IQueryOverCustomExpressions>()
                            .ImplementedBy(_queryOverCustomExpressions)
                            .LifestyleSingleton());
                IQueryOverCustomExpressions queryOverCustomExpressions = Kernel.Resolve<IQueryOverCustomExpressions>();
                queryOverCustomExpressions.Initialize();
            }

            if (!Kernel.HasComponent(typeof(ITimeProvider)))
            {
                Kernel
                    .Register(
                        Component
                            .For<ITimeProvider>()
                            .ImplementedBy<TimeProvider>()
                            .IsFallback()
                            .LifeStyle.Singleton);
            }

            if (!Kernel.HasComponent(typeof(IIdentityProvider)))
            {
                Kernel
                    .Register(
                        Component
                            .For<IIdentityProvider>()
                            .ImplementedBy<IdentityProvider>()
                            .IsFallback()
                            .LifeStyle.Singleton);
            }
        }
    }
}
