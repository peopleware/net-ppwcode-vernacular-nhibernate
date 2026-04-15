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
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using PPWCode.Vernacular.Exceptions.V;
using PPWCode.Vernacular.NHibernate.IV.Providers;
using PPWCode.Vernacular.Persistence.V;
using PPWCode.Vernacular.Persistence.V.Exceptions;

namespace PPWCode.Vernacular.NHibernate.IV
{
    /// <inheritdoc cref="IRepository{T,TId}" />
    public abstract class Repository<TRoot, TId>(ISessionProvider sessionProvider)
        : RepositoryBase<TRoot, TId>(sessionProvider),
          IRepository<TRoot, TId>
        where TRoot : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        /// <inheritdoc />
        public virtual TRoot? GetById(TId? id)
            => Execute(nameof(GetById), () => GetByIdInternal(id));

        /// <inheritdoc />
        public virtual TRoot LoadById(TId? id)
            => Execute(nameof(LoadById), () => LoadByIdInternal(id))
               ?? throw new ProgrammingError($"Unexpected null result while load<{typeof(TRoot).FullName}>({id}).");

        /// <inheritdoc />
        public virtual IList<TRoot> FindAll()
            => Execute(nameof(FindAll), FindAllInternal) ?? new List<TRoot>();

        /// <inheritdoc />
        public virtual IList<TRoot> FindByIds(IEnumerable<TId> ids)
            => Execute(
                   nameof(FindByIds),
                   () =>
                   {
                       List<TRoot> result = new List<TRoot>();
                       foreach (TId[] segment in GetSegmentedIds(ids).Where(s => s.Length > 0))
                       {
                           result.AddRange(FindByIdsInternal(segment));
                       }

                       return result;
                   }) ?? new List<TRoot>();

        /// <inheritdoc />
        [return: NotNullIfNotNull(nameof(entity))]
        public virtual TRoot? Merge(TRoot? entity)
            => Execute(nameof(Merge), () => MergeInternal(entity));

        /// <inheritdoc />
        public virtual void SaveOrUpdate(TRoot? entity)
            => Execute(nameof(SaveOrUpdate), () => SaveOrUpdateInternal(entity));

        /// <inheritdoc />
        public virtual void Delete(TRoot? entity)
            => Execute(nameof(Delete), () => DeleteInternal(entity));

        protected virtual TRoot? GetByIdInternal(TId? id)
            => Session.Get<TRoot>(id);

        protected virtual TRoot LoadByIdInternal(TId? id)
            => Session.Load<TRoot>(id);

        /// <inheritdoc cref="FindAll" />
        protected abstract IList<TRoot> FindAllInternal();

        /// <inheritdoc cref="FindByIds" />
        protected abstract IEnumerable<TRoot> FindByIdsInternal(IEnumerable<TId> segment);

        /// <inheritdoc cref="Merge" />
        /// <remarks>Runs in an isolated environment. This ensures a transaction is active and exceptions are being triaged.</remarks>
        [return: NotNullIfNotNull(nameof(entity))]
        protected virtual TRoot? MergeInternal(TRoot? entity)
        {
            if (entity != null)
            {
                // Note: Prevent a CREATE for something that was assumed to be an UPDATE.
                // NHibernate MERGE transforms an UPDATE for a not-found-PK into a CREATE
                if (!entity.IdIsTransient && (GetById(entity.Id) == null))
                {
                    throw new NotFoundException("Merge executed for an entity that no longer exists in the database.");
                }

                return Session.Merge(entity);
            }

            return null;
        }

        /// <inheritdoc cref="SaveOrUpdate" />
        /// <remarks>Runs in an isolated environment. This ensures a transaction is active and exceptions are being triaged.</remarks>
        protected virtual void SaveOrUpdateInternal(TRoot? entity)
        {
            if (entity != null)
            {
                // Note: Prevent a CREATE for something that was assumed to be an UPDATE.
                if (!entity.IdIsTransient && (GetById(entity.Id) == null))
                {
                    throw new NotFoundException("SaveOrUpdate executed for an entity that no longer exists in the database.");
                }

                Session.SaveOrUpdate(entity);
            }
        }

        /// <inheritdoc cref="Delete" />
        protected virtual void DeleteInternal(TRoot? entity)
        {
            if (entity is { IdIsTransient: false })
            {
                // Check if entity exists
                TRoot fetchedEntity = Session.Get<TRoot>(entity.Id);
                if (fetchedEntity != null)
                {
                    // Handle stale objects
                    TRoot mergedEntity = Session.Merge(entity);

                    // finally, delete none-transient not stale existing entity
                    Session.Delete(mergedEntity);
                }
            }
        }
    }
}
