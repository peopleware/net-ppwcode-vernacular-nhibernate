﻿// Copyright 2014 by PeopleWare n.v..
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

using NHibernate.Cfg.MappingSchema;

namespace PPWCode.Vernacular.NHibernate.I.Interfaces
{
    /// <summary>
    ///     If <see cref="GetHbmMappings" /> returns a not-null
    ///     <see cref="HbmMapping" />, it will be used instead of <see cref="IMappingAssemblies.GetAssemblies" /> to determine
    ///     the mappings.
    /// </summary>
    [ContractClass(typeof(IHbmMappingContract))]
    public interface IHbmMapping
    {
        /// <summary>
        ///     Get a <see cref="HbmMapping" /> instance or null, this can be used by hbmMapping by code.
        /// </summary>
        /// <returns>
        ///     A <see cref="HbmMapping" /> instance or null.
        /// </returns>
        IEnumerable<HbmMapping> GetHbmMappings();
    }

    // ReSharper disable once InconsistentNaming
    [ContractClassFor(typeof(IHbmMapping))]
    public abstract class IHbmMappingContract : IHbmMapping
    {
        public IEnumerable<HbmMapping> GetHbmMappings()
        {
            Contract.Ensures(Contract.Result<IEnumerable<HbmMapping>>() != null);

            return default(IEnumerable<HbmMapping>);
        }
    }
}