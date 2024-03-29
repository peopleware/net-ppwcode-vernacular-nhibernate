// Copyright 2020 by PeopleWare n.v..
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

using NHibernate.Mapping;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.III.Test;
using PPWCode.Vernacular.NHibernate.III.Tests.Model.Common;

namespace PPWCode.Vernacular.NHibernate.III.Tests.IntegrationTests
{
    [Parallelizable(ParallelScope.Fixtures)]
    public abstract class BaseQueryTests : BaseRepositoryFixture<int, TestIntAuditLog>
    {
        private IPpwHbmMapping _ppwHbmMapping;

        protected override string CatalogName
            => $"Test.{GetType().Assembly.GetName().Name}";

        protected override IPpwHbmMapping PpwHbmMapping
            => _ppwHbmMapping ?? (_ppwHbmMapping = new TestsSimpleModelMapper(new TestsMappingAssemblies()));

        protected override string IdentityName
            => "Test - IdentityName";

        protected override IEnumerable<IAuxiliaryDatabaseObject> AuxiliaryDatabaseObjects
        {
            get
            {
                yield return new TestHighLowPerTableAuxiliaryDatabaseObject(PpwHbmMapping);
                yield return new UniqueConstraintsForExtendedCompany(PpwHbmMapping);
            }
        }
    }
}
