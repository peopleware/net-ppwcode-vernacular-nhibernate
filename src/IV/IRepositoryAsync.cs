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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NHibernate;

using PPWCode.Vernacular.Persistence.V;
using PPWCode.Vernacular.Persistence.V.Exceptions;

namespace PPWCode.Vernacular.NHibernate.IV
{
    public interface IRepositoryAsync<TRoot, in TId>
        where TRoot : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        /// <inheritdoc cref="ISession.GetAsync{T}(object,System.Threading.CancellationToken)" />
        /// <remarks>
        ///     Runs in an isolated environment. This ensures a transaction is active and exceptions are being triaged.
        /// </remarks>
        Task<TRoot?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

        /// <inheritdoc cref="ISession.LoadAsync{T}(object,System.Threading.CancellationToken)" />
        /// <remarks>
        ///     Runs in an isolated environment. This ensures a transaction is active and exceptions are being triaged.
        /// </remarks>
        Task<TRoot> LoadByIdAsync(TId id, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Find all records of type <typeparamref name="TRoot" />.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        /// <returns>
        ///     A list of records, the list is <b>never</b> a null-reference.
        /// </returns>
        /// <remarks>
        ///     Runs in an isolated environment. This ensures a transaction is active and exceptions are being triaged.
        /// </remarks>
        Task<IList<TRoot>> FindAllAsync(CancellationToken cancellationToken = default);

        /// <summary>Gets a list of entities by their ids.</summary>
        /// <param name="ids">The given primary keys.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        /// <returns>
        ///     The entities with the given ids that could be found, if not found, no entity with the id is returned.
        /// </returns>
        /// <remarks>
        ///     Runs in an isolated environment. This ensures a transaction is active and exceptions are being triaged.
        /// </remarks>
        Task<IList<TRoot>> FindByIdsAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default);

        /// <inheritdoc cref="ISession.MergeAsync{T}(T,System.Threading.CancellationToken)" />
        /// <exception cref="NotFoundException">
        ///     The normal behaviour of nHibernate is that the <c>Merge</c> transforms an UPDATE for a not-found-PK into a
        ///     CREATE. We will not do this in our code-base. In this case we will throw an exception.
        /// </exception>
        /// <remarks>
        ///     Runs in an isolated environment. This ensures a transaction is active and exceptions are being triaged.
        /// </remarks>
        Task<TRoot?> MergeAsync(TRoot? entity, CancellationToken cancellationToken = default);

        /// <inheritdoc cref="ISession.SaveOrUpdateAsync(object,System.Threading.CancellationToken)" />
        /// <exception cref="NotFoundException">
        ///     The normal behaviour of nHibernate is that the <c>SaveOrUpdate</c> transforms an UPDATE for a not-found-PK into a
        ///     CREATE. We will not do this in our code-base. In this case we will throw an exception.
        /// </exception>
        /// <remarks>
        ///     Runs in an isolated environment. This ensures a transaction is active and exceptions are being triaged.
        /// </remarks>
        Task SaveOrUpdateAsync(TRoot? entity, CancellationToken cancellationToken = default);

        /// <inheritdoc cref="ISession.DeleteAsync(object,System.Threading.CancellationToken)" />
        /// <remarks>
        ///     Runs in an isolated environment. This ensures a transaction is active and exceptions are being triaged.
        /// </remarks>
        Task DeleteAsync(TRoot? entity, CancellationToken cancellationToken = default);
    }
}
