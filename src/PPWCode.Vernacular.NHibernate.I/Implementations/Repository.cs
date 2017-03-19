﻿// Copyright 2017 by PeopleWare n.v..
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using NHibernate;

using PPWCode.Vernacular.NHibernate.I.Interfaces;
using PPWCode.Vernacular.Persistence.II;

namespace PPWCode.Vernacular.NHibernate.I.Implementations
{
    public abstract class Repository<T, TId>
        : RepositoryBase<T, TId>,
          IRepository<T, TId>
        where T : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        protected Repository(ISession session)
            : base(session)
        {
        }

        public virtual T GetById(TId id)
        {
            return Execute("GetById", () => GetByIdInternal(id));
        }

        public virtual T Get(Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return Execute("Get", () => GetInternal<T>(alias, func));
        }

        public virtual R Get<R>(Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return Execute("Get", () => GetInternal<R>(alias, func));
        }

        public virtual T Get(Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return Execute("Get", () => GetInternal<T>(func));
        }

        public virtual R Get<R>(Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return Execute("Get", () => GetInternal<R>(func));
        }

        public virtual T GetAtIndex(Func<IQueryOver<T, T>, IQueryOver<T, T>> func, int index)
        {
            return Execute("GetAtIndex", () => GetAtIndexInternal<T>(func, index));
        }

        public virtual R GetAtIndex<R>(Func<IQueryOver<T, T>, IQueryOver<T, T>> func, int index)
        {
            return Execute("GetAtIndex", () => GetAtIndexInternal<R>(func, index));
        }

        public virtual T GetAtIndex(Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func, int index)
        {
            return Execute("GetAtIndex", () => GetAtIndexInternal<T>(alias, func, index));
        }

        public virtual R GetAtIndex<R>(Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func, int index)
        {
            return Execute("GetAtIndex", () => GetAtIndexInternal<R>(alias, func, index));
        }

        public virtual IList<T> FindAll()
        {
            return Execute("FindAll", () => FindAllInternal());
        }

        public virtual IList<T> Find(Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return Execute("Find", () => FindInternal<T>(func));
        }

        public virtual IList<R> Find<R>(Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return Execute("Find", () => FindInternal<R>(func));
        }

        public virtual IList<T> Find(Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return Execute("Find", () => FindInternal<T>(alias, func));
        }

        public virtual IList<R> Find<R>(Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return Execute("Find", () => FindInternal<R>(alias, func));
        }

        public virtual IPagedList<T> FindPaged(int pageIndex, int pageSize, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return Execute("FindPaged", () => FindPagedInternal<T>(pageIndex, pageSize, func));
        }

        public virtual IPagedList<R> FindPaged<R>(int pageIndex, int pageSize, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return Execute("FindPaged", () => FindPagedInternal<R>(pageIndex, pageSize, func));
        }

        public virtual IPagedList<T> FindPaged(int pageIndex, int pageSize, Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return Execute("FindPaged", () => FindPagedInternal<T>(pageIndex, pageSize, alias, func));
        }

        public virtual IPagedList<R> FindPaged<R>(int pageIndex, int pageSize, Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            return Execute("FindPaged", () => FindPagedInternal<R>(pageIndex, pageSize, alias, func));
        }

        public virtual IList<T> Find(Func<IQueryOver<T, T>, IQueryOver<T, T>> func, int? skip, int? count)
        {
            return Execute("Find", () => FindInternal<T>(func, skip, count));
        }

        public virtual IList<R> Find<R>(Func<IQueryOver<T, T>, IQueryOver<T, T>> func, int? skip, int? count)
        {
            return Execute("Find", () => FindInternal<R>(func, skip, count));
        }

        public virtual IList<T> Find(Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func, int? skip, int? count)
        {
            return Execute("Find", () => FindInternal<T>(alias, func, skip, count));
        }

        public virtual IList<R> Find<R>(Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func, int? skip, int? count)
        {
            return Execute("Find", () => FindInternal<R>(alias, func, skip, count));
        }

        public virtual T Merge(T entity)
        {
            return Execute("Merge", () => MergeInternal(entity));
        }

        public virtual void Delete(T entity)
        {
            Execute("Delete", () => DeleteInternal(entity));
        }

        protected virtual T GetByIdInternal(TId id)
        {
            T result = Session.Get<T>(id);

            return result;
        }

        protected virtual R GetInternal<R>(Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            R result = func(CreateQueryOver()).SingleOrDefault<R>();

            return result;
        }

        protected virtual R GetInternal<R>(Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            R result = func(CreateQueryOver(alias)).SingleOrDefault<R>();

            return result;
        }

        protected virtual R GetAtIndexInternal<R>(Func<IQueryOver<T, T>, IQueryOver<T, T>> func, int index)
        {
            R result = func(CreateQueryOver()).Skip(index).Take(1).SingleOrDefault<R>();

            return result;
        }

        protected virtual R GetAtIndexInternal<R>(Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func, int index)
        {
            R result = func(CreateQueryOver(alias)).Skip(index).Take(1).SingleOrDefault<R>();

            return result;
        }

        protected virtual IList<T> FindAllInternal()
        {
            IQueryOver<T> queryOver = CreateQueryOver();
            IList<T> result = queryOver.List<T>();

            return result;
        }

        protected virtual IList<R> FindInternal<R>(Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            IQueryOver<T> queryOver = func != null ? func(CreateQueryOver()) : CreateQueryOver();
            IList<R> result = queryOver.List<R>();

            return result;
        }

        protected virtual IList<R> FindInternal<R>(Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            IQueryOver<T> queryOver = func != null ? func(CreateQueryOver(alias)) : CreateQueryOver(alias);
            IList<R> result = queryOver.List<R>();

            return result;
        }

        protected virtual IList<R> FindInternal<R>(Func<IQueryOver<T, T>, IQueryOver<T, T>> func, int? skip, int? count)
        {
            IQueryOver<T> queryOver = func != null ? func(CreateQueryOver()) : CreateQueryOver();

            if (skip.HasValue)
            {
                queryOver = queryOver.Skip(skip.Value);
            }

            if (count.HasValue)
            {
                queryOver = queryOver.Take(count.Value);
            }

            IList<R> result = queryOver.List<R>();

            return result;
        }

        protected virtual IList<R> FindInternal<R>(Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func, int? skip, int? count)
        {
            IQueryOver<T> queryOver = func != null ? func(CreateQueryOver(alias)) : CreateQueryOver(alias);

            if (skip.HasValue)
            {
                queryOver = queryOver.Skip(skip.Value);
            }

            if (count.HasValue)
            {
                queryOver = queryOver.Take(count.Value);
            }

            IList<R> result = queryOver.List<R>();

            return result;
        }

        protected virtual PagedList<R> FindPagedInternal<R>(int pageIndex, int pageSize, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            IQueryOver<T> rowCountQueryOver = func != null ? func(CreateQueryOver()) : CreateQueryOver();
            IFutureValue<int> rowCount =
                rowCountQueryOver
                    .ToRowCountQuery()
                    .FutureValue<int>();

            IQueryOver<T> pagingQueryOver = func != null ? func(CreateQueryOver()) : CreateQueryOver();
            IList<R> qryResult =
                pagingQueryOver
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Future<R>()
                    .ToList();

            PagedList<R> result = new PagedList<R>(qryResult, pageIndex, pageSize, rowCount.Value);

            return result;
        }

        protected virtual PagedList<R> FindPagedInternal<R>(int pageIndex, int pageSize, Expression<Func<T>> alias, Func<IQueryOver<T, T>, IQueryOver<T, T>> func)
        {
            IQueryOver<T> rowCountQueryOver = func != null ? func(CreateQueryOver(alias)) : CreateQueryOver(alias);
            IFutureValue<int> rowCount =
                rowCountQueryOver
                    .ToRowCountQuery()
                    .FutureValue<int>();

            IQueryOver<T> pagingQueryOver = func != null ? func(CreateQueryOver(alias)) : CreateQueryOver(alias);
            IList<R> qryResult =
                pagingQueryOver
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Future<R>()
                    .ToList();

            PagedList<R> result = new PagedList<R>(qryResult, pageIndex, pageSize, rowCount.Value);

            return result;
        }

        protected virtual T MergeInternal(T entity)
        {
            // Note: Prevent a CREATE for something that was assumed to be an UPDATE.
            // NHibernate MERGE transforms an UPDATE for a not-found-PK into a CREATE
            if (entity != null && !entity.IsTransient && GetById(entity.Id) == null)
            {
                throw new NotFoundException("Merge executed for an entity that no longer exists in the database.");
            }

            T result = Session.Merge(entity);

            return result;
        }

        protected virtual void DeleteInternal(T entity)
        {
            if (!entity.IsTransient)
            {
                // Check if entity exists
                T fetchedEntity = Session.Get<T>(entity.Id);
                if (fetchedEntity != null)
                {
                    // Handle stale objects
                    Session.Merge(entity);
                    // finally delete none-transient not stale existing entity
                    Session.Delete(fetchedEntity);
                }
            }
        }

        protected virtual IQueryOver<T, T> CreateQueryOver()
        {
            T rootAlias = null;
            return Session.QueryOver(() => rootAlias);
        }

        protected virtual IQueryOver<T, T> CreateQueryOver(Expression<Func<T>> alias)
        {
            return Session.QueryOver(alias);
        }
    }
}
