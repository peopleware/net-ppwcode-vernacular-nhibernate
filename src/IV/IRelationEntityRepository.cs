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
using System.Threading;
using System.Threading.Tasks;

using PPWCode.Vernacular.Persistence.V;

namespace PPWCode.Vernacular.NHibernate.IV;

public interface IRelationEntityRepository<TIdentity>
    where TIdentity : struct, IEquatable<TIdentity>
{
    /// <summary>
    ///     Retrieves an entity of type <typeparamref name="TModel" /> by identifier for use in a relationship to
    ///     another entity.
    /// </summary>
    /// <typeparam name="TModel">The entity type to retrieve.</typeparam>
    /// <param name="id">The identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
    /// <returns>
    ///     The requested entity when <paramref name="id" /> has a value; otherwise, <see langword="null" />.
    /// </returns>
    /// <remarks>Entity retrieval is performed through a stateless session.</remarks>
    Task<TModel?> GetByIdAsync<TModel>(TIdentity? id, CancellationToken cancellationToken = default)
        where TModel : class, IPersistentObject<TIdentity>;
}
