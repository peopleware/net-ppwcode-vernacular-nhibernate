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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PPWCode.Vernacular.NHibernate.III.Async.Interfaces.Providers;
using PPWCode.Vernacular.NHibernate.III.Tests.IntegrationTests.Async.Linq.Common.Repositories;
using PPWCode.Vernacular.NHibernate.III.Tests.Model.RepositoryWithDtoMapping;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.Tests.IntegrationTests.Async.Linq.DtoMapping.Repositories
{
    public class ShipRepository : TestRepository<Ship>
    {
        public ShipRepository(ISessionProviderAsync sessionProvider)
            : base(sessionProvider)
        {
        }

        public Task<IList<ContainerDto>> FindContainersFromShipsMatchingCodeAsync(string code, CancellationToken cancellationToken)
            => FindAsync(FindContainersQuery(code), cancellationToken);

        public Task<IPagedList<ContainerDto>> FindContainersFromShipsMatchingCodePagedAsync(int pageIndex, int pageSize, string code, CancellationToken cancellationToken)
            => FindPagedAsync(FindContainersQuery(code), pageIndex, pageSize, cancellationToken);

        protected virtual Func<IQueryable<Ship>, IQueryable<ContainerDto>> FindContainersQuery(string code)
            => qry =>
                   qry
                       .SelectMany(ship => ship.CargoContainers, (ship, container) => new { ship, container })
                       .Where(x => x.ship.Code.StartsWith(code))
                       .GroupBy(x => new { shipCode = x.ship.Code, containerCode = x.container.Code, containerLoad = x.container.Load })
                       .OrderBy(g => g.Key.shipCode)
                       .Select(g => new ContainerDto(g.Key.shipCode, g.Key.containerCode, g.Key.containerLoad));
    }
}
