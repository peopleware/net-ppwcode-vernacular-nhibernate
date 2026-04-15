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

using NHibernate.Mapping.ByCode;

using PPWCode.Vernacular.Persistence.V;

namespace PPWCode.Vernacular.NHibernate.IV.Tests.Model.RepositoryWithDtoMapping
{
    public class Ship : PersistentObject
    {
        private readonly ISet<CargoContainer> _cargoContainers = new HashSet<CargoContainer>();

        public virtual string? Code { get; set; }

        [AuditLogPropertyIgnore]
        public virtual ISet<CargoContainer> CargoContainers
            => _cargoContainers;

        public virtual void AddCargoContainer(CargoContainer? cargoContainer)
        {
            if ((cargoContainer != null) && CargoContainers.Add(cargoContainer))
            {
                cargoContainer.Ship = this;
            }
        }

        public virtual void RemoveCargoContainer(CargoContainer? cargoContainer)
        {
            if ((cargoContainer != null) && CargoContainers.Remove(cargoContainer))
            {
                cargoContainer.Ship = null;
            }
        }

        public class ShipMapper : PersistentObjectMapper<Ship>
        {
            public ShipMapper()
            {
                Property(s => s.Code);

                Set(
                    s => s.CargoContainers,
                    m => m.Cascade(Cascade.All | Cascade.Merge),
                    r => r.OneToMany());
            }
        }
    }
}
