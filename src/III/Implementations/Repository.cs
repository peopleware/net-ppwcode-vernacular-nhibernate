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

using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using PPWCode.Vernacular.Exceptions.IV;
using PPWCode.Vernacular.NHibernate.III.Providers;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III
{
    /// <inheritdoc cref="IRepository{T,TId}" />
    public abstract class Repository<TRoot, TId>
        : RepositoryBase<TRoot, TId>,
          IRepository<TRoot, TId>
        where TRoot : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        protected Repository([NotNull] ISessionProvider sessionProvider)
            : base(sessionProvider)
        {
        }

        /// <inheritdoc />
        public virtual TRoot GetById(TId id)
            => Execute(nameof(GetById), () => GetByIdInternal(id));

        /// <inheritdoc />
        public virtual TRoot LoadById(TId id)
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
        public virtual TRoot Merge(TRoot entity)
            => Execute(nameof(Merge), () => MergeInternal(entity));

        /// <inheritdoc />
        public virtual void SaveOrUpdate(TRoot entity)
            => Execute(nameof(SaveOrUpdate), () => SaveOrUpdateInternal(entity));

        /// <inheritdoc />
        public virtual void Delete(TRoot entity)
            => Execute(nameof(Delete), () => DeleteInternal(entity));

        [CanBeNull]
        protected virtual TRoot GetByIdInternal([NotNull] TId id)
            => Session.Get<TRoot>(id);

        [NotNull]
        protected virtual TRoot LoadByIdInternal([NotNull] TId id)
            => Session.Load<TRoot>(id);

        /// <inheritdoc cref="FindAll" />
        [NotNull]
        [ItemNotNull]
        protected abstract IList<TRoot> FindAllInternal();

        /// <inheritdoc cref="FindByIds"/>
        [NotNull]
        [ItemNotNull]
        protected abstract IEnumerable<TRoot> FindByIdsInternal([NotNull] [ItemNotNull] IEnumerable<TId> segment);

        /// <inheritdoc cref="Merge" />
        /// <remarks>Runs in an isolated environment. This ensures a transaction is active and exceptions are being triaged.</remarks>
        [ContractAnnotation("entity:null => null; entity:notnull => notnull")]
        protected virtual TRoot MergeInternal(TRoot entity)
        {
            if (entity != null)
            {
                // Note: Prevent a CREATE for something that was assumed to be an UPDATE.
                // NHibernate MERGE transforms an UPDATE for a not-found-PK into a CREATE
                if (!entity.IsTransient && (GetById(entity.Id) == null))
                {
                    throw new NotFoundException("Merge executed for an entity that no longer exists in the database.");
                }

                return Session.Merge(entity);
            }

            return default;
        }

        /// <inheritdoc cref="SaveOrUpdate" />
        /// <remarks>Runs in an isolated environment. This ensures a transaction is active and exceptions are being triaged.</remarks>
        protected virtual void SaveOrUpdateInternal([CanBeNull] TRoot entity)
        {
            if (entity != null)
            {
                // Note: Prevent a CREATE for something that was assumed to be an UPDATE.
                if (!entity.IsTransient && (GetById(entity.Id) == null))
                {
                    throw new NotFoundException("SaveOrUpdate executed for an entity that no longer exists in the database.");
                }

                Session.SaveOrUpdate(entity);
            }
        }

        /// <inheritdoc cref="Delete" />
        protected virtual void DeleteInternal([CanBeNull] TRoot entity)
        {
            if ((entity != null) && !entity.IsTransient)
            {
                // Check if entity exists
                TRoot fetchedEntity = Session.Get<TRoot>(entity.Id);
                if (fetchedEntity != null)
                {
                    // Handle stale objects
                    TRoot mergedEntity = Session.Merge(entity);

                    // finally delete none-transient not stale existing entity
                    Session.Delete(mergedEntity);
                }
            }
        }
    }
}
