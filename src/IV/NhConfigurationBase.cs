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

using System.Collections.Generic;
using System.Linq;

using NHibernate.Cfg;
using NHibernate.Mapping;

namespace PPWCode.Vernacular.NHibernate.IV
{
    /// <inheritdoc />
    public abstract class NhConfigurationBase(
        INhInterceptor nhInterceptor,
        INhProperties nhProperties,
        IMappingAssemblies mappingAssemblies,
        IPpwHbmMapping ppwHbmMapping,
        IEnumerable<IRegisterEventListener> registerEventListeners,
        IEnumerable<IAuxiliaryDatabaseObject> auxiliaryDatabaseObjects)
        : INhConfiguration
    {
        private readonly object _locker = new();
        private volatile Configuration? _configuration;

        /// <inheritdoc cref="INhProperties" />
        protected INhProperties NhProperties { get; } = nhProperties;

        /// <inheritdoc cref="IRegisterEventListener" />
        protected IRegisterEventListener[] RegisterEventListeners { get; } = registerEventListeners.ToArray();

        /// <inheritdoc cref="GetConfiguration" />
        protected abstract Configuration Configuration { get; }

        /// <inheritdoc cref="INhInterceptor" />
        protected INhInterceptor NhInterceptor { get; } = nhInterceptor;

        /// <inheritdoc cref="IPpwHbmMapping" />
        protected IPpwHbmMapping PpwHbmMapping { get; } = ppwHbmMapping;

        /// <inheritdoc cref="IMappingAssemblies" />
        protected IMappingAssemblies MappingAssemblies { get; } = mappingAssemblies;

        /// <inheritdoc cref="IAuxiliaryDatabaseObject" />
        protected IAuxiliaryDatabaseObject[] AuxiliaryDatabaseObjects { get; } = auxiliaryDatabaseObjects.ToArray();

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
