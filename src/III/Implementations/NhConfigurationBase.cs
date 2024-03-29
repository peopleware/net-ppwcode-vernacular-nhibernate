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

using System.Collections.Generic;

using JetBrains.Annotations;

using NHibernate.Cfg;
using NHibernate.Mapping;

namespace PPWCode.Vernacular.NHibernate.III
{
    /// <inheritdoc />
    public abstract class NhConfigurationBase : INhConfiguration
    {
        private readonly object _locker = new object();
        private volatile Configuration _configuration;

        protected NhConfigurationBase(
            [NotNull] INhInterceptor nhInterceptor,
            [NotNull] INhProperties nhProperties,
            [NotNull] IMappingAssemblies mappingAssemblies,
            [NotNull] IPpwHbmMapping ppwHbmMapping,
            [NotNull] IRegisterEventListener[] registerEventListeners,
            [NotNull] IAuxiliaryDatabaseObject[] auxiliaryDatabaseObjects)
        {
            NhInterceptor = nhInterceptor;
            NhProperties = nhProperties;
            MappingAssemblies = mappingAssemblies;
            PpwHbmMapping = ppwHbmMapping;
            RegisterEventListeners = registerEventListeners;
            AuxiliaryDatabaseObjects = auxiliaryDatabaseObjects;
        }

        /// <inheritdoc cref="INhProperties" />
        [NotNull]
        protected INhProperties NhProperties { get; }

        /// <inheritdoc cref="IRegisterEventListener" />
        [NotNull]
        protected IEnumerable<IRegisterEventListener> RegisterEventListeners { get; }

        /// <inheritdoc cref="GetConfiguration" />
        [NotNull]
        protected abstract Configuration Configuration { get; }

        /// <inheritdoc cref="INhInterceptor" />
        [NotNull]
        protected INhInterceptor NhInterceptor { get; }

        /// <inheritdoc cref="IPpwHbmMapping" />
        [NotNull]
        protected IPpwHbmMapping PpwHbmMapping { get; }

        /// <inheritdoc cref="IMappingAssemblies" />
        [NotNull]
        protected IMappingAssemblies MappingAssemblies { get; }

        /// <inheritdoc cref="IAuxiliaryDatabaseObject" />
        [NotNull]
        protected IAuxiliaryDatabaseObject[] AuxiliaryDatabaseObjects { get; }

        /// <inheritdoc />
        public Configuration GetConfiguration()
        {
            if (_configuration == null)
            {
                lock (_locker)
                {
                    if (_configuration == null)
                    {
                        _configuration = Configuration;
                    }
                }
            }

            return _configuration;
        }
    }
}
