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

using NHibernate.Cfg;

namespace PPWCode.Vernacular.NHibernate.I.Interfaces
{
    [ContractClass(typeof(INhPropertiesContract))]
    public interface INhProperties
    {
        IEnumerable<KeyValuePair<string, string>> GetProperties(Configuration configuration);
    }

    // ReSharper disable once InconsistentNaming
    [ContractClassFor(typeof(INhProperties))]
    public abstract class INhPropertiesContract : INhProperties
    {
        public IEnumerable<KeyValuePair<string, string>> GetProperties(Configuration configuration)
        {
            Contract.Ensures(Contract.Result<IEnumerable<KeyValuePair<string, string>>>() != null);

            return default(IEnumerable<KeyValuePair<string, string>>);
        }
    }
}