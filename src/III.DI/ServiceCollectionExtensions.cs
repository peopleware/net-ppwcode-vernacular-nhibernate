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
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using NHibernate;
using NHibernate.Mapping;

using PPWCode.Vernacular.Exceptions.IV;
using PPWCode.Vernacular.NHibernate.III.Async.Interfaces.Providers;
using PPWCode.Vernacular.NHibernate.III.DbConstraint;
using PPWCode.Vernacular.NHibernate.III.Providers;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.DI
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNHibernate(this IServiceCollection services, Action<NHibernateOptions> configure)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            NHibernateOptions options = new NHibernateOptions();
            configure(options);
            options.ApplyDefaultsIfNotGiven();

            if (options.MappingAssemblies == null)
            {
                throw new Error($"You must supply a dependency of type {nameof(IMappingAssemblies)}, using the {nameof(NHibernateOptions.UseMappingAssemblies)} method.");
            }

            if (options.PpwHbmMapping == null)
            {
                throw new Error($"You must supply a dependency of type {nameof(IPpwHbmMapping)}, using the {nameof(NHibernateOptions.UseHbmMapping)} method.");
            }

            services.TryAddSingleton(options.MappingAssemblies);
            services.TryAddSingleton(sp => (IMappingAssemblies)sp.GetRequiredService(options.MappingAssemblies));

            services.TryAddSingleton(options.PpwHbmMapping);
            services.TryAddSingleton(sp => (IPpwHbmMapping)sp.GetRequiredService(options.PpwHbmMapping));

            // Because we called ApplyDefaultsIfNotGiven() on the options, some of them are set and not null
            services.TryAddSingleton(options.ExceptionTranslator!);
            services.TryAddSingleton(sp => (IExceptionTranslator)sp.GetRequiredService(options.ExceptionTranslator));

            services.TryAddSingleton(options.NhProperties!);
            services.TryAddSingleton(sp => (INhProperties)sp.GetRequiredService(options.NhProperties));

            services.TryAddSingleton(options.NhConfiguration!);
            services.TryAddSingleton(sp => (INhConfiguration)sp.GetRequiredService(options.NhConfiguration));

            services.TryAddSingleton(options.NHibernateSessionFactory!);
            services.TryAddSingleton(sp => (INHibernateSessionFactory)sp.GetRequiredService(options.NHibernateSessionFactory));

            if (options.Interceptor != null)
            {
                services.TryAddSingleton(options.Interceptor);
                services.TryAddSingleton(sp => (IInterceptor)sp.GetService(options.Interceptor));
            }

            services.TryAddSingleton<INhInterceptor>(sp =>
            {
                NhInterceptor instance = ActivatorUtilities.CreateInstance<NhInterceptor>(sp);
                instance.Interceptor = sp.GetService<IInterceptor>();
                return instance;
            });

            if (options.TimeProvider != null)
            {
                services.TryAddSingleton(options.TimeProvider);
                services.TryAddSingleton(sp => (ITimeProvider)sp.GetRequiredService(options.TimeProvider));
            }
            else
            {
                services.TryAddSingleton<ITimeProvider, TimeProvider>();
            }

            if (options.IdentityProvider != null)
            {
                services.TryAddSingleton(options.IdentityProvider);
                services.TryAddSingleton(sp => (IIdentityProvider)sp.GetRequiredService(options.IdentityProvider));
            }
            else
            {
                services.TryAddSingleton<IIdentityProvider, IdentityProvider>();
            }

            if (options.CivilizedEventListener)
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton<IRegisterEventListener, CivilizedEventListener>());
            }

            if (options.TransactionProviderAsync != null)
            {
                services.TryAddSingleton(options.TransactionProviderAsync);
                services.TryAddSingleton(sp => (ITransactionProviderAsync)sp.GetRequiredService(options.TransactionProviderAsync));
                services.TryAddSingleton(sp => (ITransactionProvider)sp.GetRequiredService<ITransactionProviderAsync>());
            }
            else
            {
                services.TryAddSingleton(options.TransactionProvider!);
                services.TryAddSingleton(sp => (ITransactionProvider)sp.GetRequiredService(options.TransactionProvider));
            }

            if (options.SafeEnvironmentProviderAsync != null)
            {
                services.TryAddSingleton(options.SafeEnvironmentProviderAsync);
                services.TryAddSingleton(sp => (ISafeEnvironmentProviderAsync)sp.GetRequiredService(options.SafeEnvironmentProviderAsync));
                services.TryAddSingleton(sp => (ISafeEnvironmentProvider)sp.GetRequiredService<ISafeEnvironmentProviderAsync>());
            }
            else
            {
                services.TryAddSingleton(options.SafeEnvironmentProvider!);
                services.TryAddSingleton(sp => (ISafeEnvironmentProvider)sp.GetRequiredService(options.SafeEnvironmentProvider!));
            }

            IsolationLevel isolationLevel = options.IsolationLevel!.Value;
            if (options.SessionProviderAsync != null)
            {
                services.TryAddTransient(options.SessionProviderAsync);
                services.TryAddTransient(sp => (ISessionProviderAsync)ActivatorUtilities.CreateInstance(sp, options.SessionProviderAsync, isolationLevel));
                services.TryAddTransient(sp => (ISessionProvider)sp.GetRequiredService<ISessionProviderAsync>());
            }
            else
            {
                services.TryAddTransient(options.SessionProvider!);
                services.TryAddTransient(sp => (ISessionProvider)ActivatorUtilities.CreateInstance(sp, options.SessionProvider, isolationLevel));
            }

            RegisterNhSessionFactories(services, options);
            RegisterAssemblyBasedComponents(services, options);

            if (options.QueryOverCustomExpressions != null)
            {
                services.TryAddSingleton(options.QueryOverCustomExpressions);
                services.TryAddSingleton(sp =>
                {
                    IQueryOverCustomExpressions instance = (IQueryOverCustomExpressions)sp.GetRequiredService(options.QueryOverCustomExpressions);
                    instance.Initialize();
                    return instance;
                });
            }

            return services;
        }

        private static void RegisterNhSessionFactories(
            IServiceCollection services,
            NHibernateOptions options)
        {
            ServiceLifetime lifetime = options.SessionLifestyle ?? ServiceLifetime.Singleton;

            ServiceDescriptor sessionServiceDescriptor =
                new ServiceDescriptor(
                    typeof(ISession),
                    sp =>
                    {
                        INHibernateSessionFactory sessionFactory = sp.GetRequiredService<INHibernateSessionFactory>();
                        return sessionFactory.SessionFactory.OpenSession();
                    },
                    lifetime);
            services.Add(sessionServiceDescriptor);

            ServiceDescriptor statelessSessionServiceDescriptor =
                new ServiceDescriptor(
                    typeof(IStatelessSession),
                    sp =>
                    {
                        INHibernateSessionFactory sessionFactory = sp.GetRequiredService<INHibernateSessionFactory>();
                        return sessionFactory.SessionFactory.OpenStatelessSession();
                    },
                    lifetime);
            services.Add(statelessSessionServiceDescriptor);
        }

        private static void RegisterAssemblyBasedComponents(
            IServiceCollection services,
            NHibernateOptions options)
        {
            IMappingAssemblies mappingAssemblies = (IMappingAssemblies)Activator.CreateInstance(options.MappingAssemblies!)!;
            foreach (Assembly assembly in mappingAssemblies.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface)
                    {
                        continue;
                    }

                    if (typeof(IRegisterEventListener).IsAssignableFrom(type))
                    {
                        services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IRegisterEventListener), type));
                    }

                    if (typeof(IAuxiliaryDatabaseObject).IsAssignableFrom(type))
                    {
                        services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IAuxiliaryDatabaseObject), type));
                    }
                }
            }
        }
    }
}
