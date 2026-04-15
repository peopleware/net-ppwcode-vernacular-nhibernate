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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using NHibernate;
using NHibernate.Criterion;

using PPWCode.Vernacular.NHibernate.IV.Providers;
using PPWCode.Vernacular.Persistence.V;
using PPWCode.Vernacular.Persistence.V.Exceptions;

namespace PPWCode.Vernacular.NHibernate.IV
{
    /// <inheritdoc />
    public abstract class QueryOverRepository<TRoot, TId>
        : Repository<TRoot, TId>
        where TRoot : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        protected QueryOverRepository(ISessionProvider sessionProvider)
            : base(sessionProvider)
        {
        }

        /// <summary>
        ///     Gets an entity by a function.
        /// </summary>
        /// <param name="alias">An additional alias.</param>
        /// <param name="func">The given function.</param>
        /// <returns>The entity that is filtered by the function or null if not found.</returns>
        public virtual TRoot? Get(Expression<Func<TRoot>> alias, Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func)
            => Execute(nameof(Get), () => GetInternal(alias, func));

        /// <summary>
        ///     Gets an entity by a function.
        /// </summary>
        /// <param name="func">The given function.</param>
        /// <returns>The entity that is filtered by the function or null if not found.</returns>
        public virtual TRoot? Get(Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func)
            => Execute(nameof(Get), () => GetInternal(func));

        /// <summary>
        ///     Executes the given query <paramref name="func" /> and returns the entity at position <paramref name="index" />
        ///     in the result.
        /// </summary>
        /// <param name="func">The given function.</param>
        /// <param name="index">The given index.</param>
        /// <returns>The entity that is filtered by the function or null if not found.</returns>
        public virtual TRoot? GetAtIndex(Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func, int index)
            => Execute(nameof(GetAtIndex), () => GetAtIndexInternal(func, index));

        /// <summary>
        ///     Executes the given query <paramref name="func" /> and returns the entity at position <paramref name="index" />
        ///     in the result.
        /// </summary>
        /// <param name="alias">An additional alias.</param>
        /// <param name="func">The given function.</param>
        /// <param name="index">The given index.</param>
        /// <returns>The entity that is filtered by the function or null if not found.</returns>
        public virtual TRoot? GetAtIndex(Expression<Func<TRoot>> alias, Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func, int index)
            => Execute(nameof(GetAtIndex), () => GetAtIndexInternal(alias, func, index));

        /// <summary>
        ///     Find the records complying with the given function.
        /// </summary>
        /// <param name="func">The given function.</param>
        /// <remarks>
        ///     <h3>Extra post-conditions</h3>
        ///     <para>All elements of the resulting set fulfill <paramref name="func" />.</para>
        /// </remarks>
        /// <returns>
        ///     A list of the records satisfying the given <paramref name="func" />.
        /// </returns>
        public virtual IList<TRoot> Find(Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func)
            => Execute(nameof(Find), () => FindInternal(func)) ?? new List<TRoot>();

        /// <summary>
        ///     Find the records complying with the given function.
        /// </summary>
        /// <param name="alias">An additional alias.</param>
        /// <param name="func">The given function.</param>
        /// <remarks>
        ///     <h3>Extra post-conditions</h3>
        ///     <para>All elements of the resulting set fulfill <paramref name="func" />.</para>
        /// </remarks>
        /// <returns>
        ///     A list of the records satisfying the given <paramref name="func" />.
        /// </returns>
        public virtual IList<TRoot> Find(Expression<Func<TRoot>> alias, Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func)
            => Execute(nameof(Find), () => FindInternal(alias, func)) ?? new List<TRoot>();

        /// <summary>
        ///     Find the records complying with the given function. In this result-set, <paramref name="skip" /> tuples are skipped
        ///     and then <paramref name="count" /> are taken as a result-set.
        /// </summary>
        /// <param name="func">The given function.</param>
        /// <param name="skip">Maximum tuples to skip, if <c>null</c> is specified no tuples are skipped.</param>
        /// <param name="count">Maximum tuples to be read from the result-set, if <c>null</c> is specified all records are read.</param>
        /// <remarks>
        ///     <h3>Extra post-conditions.</h3>
        ///     <para>All elements of the resulting set fulfill <paramref name="func" />.</para>
        /// </remarks>
        /// <returns>
        ///     A list of the records satisfying the given <paramref name="func" />.
        /// </returns>
        public virtual IList<TRoot> Find(Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func, int? skip, int? count)
            => Execute(nameof(Find), () => FindInternal(func, skip, count)) ?? new List<TRoot>();

        /// <summary>
        ///     Find the records complying with the given function. In this result-set, <paramref name="skip" /> tuples are skipped
        ///     and then <paramref name="count" /> are taken as a result-set.
        /// </summary>
        /// <param name="alias">An additional alias.</param>
        /// <param name="func">The given function.</param>
        /// <param name="skip">Maximum tuples to skip, if <c>null</c> is specified no tuples are skipped.</param>
        /// <param name="count">Maximum tuples to be read from the result-set, if <c>null</c> is specified all records are read.</param>
        /// <remarks>
        ///     <h3>Extra post condition.s</h3>
        ///     <para>All elements of the resulting set fulfill <paramref name="func" />.</para>
        /// </remarks>
        /// <returns>
        ///     A list of the records satisfying the given <paramref name="func" />.
        /// </returns>
        public virtual IList<TRoot> Find(Expression<Func<TRoot>> alias, Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func, int? skip, int? count)
            => Execute(nameof(Find), () => FindInternal(alias, func, skip, count)) ?? new List<TRoot>();

        /// <summary>
        ///     Find a set of records complying with the given function.
        ///     Only a subset of records are returned based on <paramref name="pageSize" /> and <paramref name="pageIndex" />.
        /// </summary>
        /// <param name="pageIndex">The index of the page, indices start from 1.</param>
        /// <param name="pageSize">The size of a page must be greater than 0.</param>
        /// <param name="func">The predicates that the data must fulfill.</param>
        /// <remarks>
        ///     <h3>Extra post-conditions</h3>
        ///     <para>All elements of the resulting set fulfill <paramref name="func" />.</para>
        /// </remarks>
        /// <returns>
        ///     An implementation of <see cref="IPagedList{T}" /> that holds a max. of <paramref name="pageSize" /> records.
        /// </returns>
        public virtual IPagedList<TRoot> FindPaged(int pageIndex, int pageSize, Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>>? func)
            => Execute(nameof(FindPaged), () => FindPagedInternal(pageIndex, pageSize, func)) ?? new PagedList<TRoot>(Enumerable.Empty<TRoot>(), pageIndex, pageSize, 0);

        /// <summary>
        ///     Find a set of records complying with the given function.
        ///     Only a subset of records are returned based on <paramref name="pageSize" /> and <paramref name="pageIndex" />.
        /// </summary>
        /// <param name="pageIndex">The index of the page, indices start from 1.</param>
        /// <param name="pageSize">The size of a page must be greater than 0.</param>
        /// <param name="alias">An additional alias.</param>
        /// <param name="func">The predicates that the data must fulfill.</param>
        /// <remarks>
        ///     <h3>Extra post-conditions</h3>
        ///     <para>All elements of the resulting set fulfill <paramref name="func" />.</para>
        /// </remarks>
        /// <returns>
        ///     An implementation of <see cref="IPagedList{T}" /> that holds a max. of <paramref name="pageSize" /> records.
        /// </returns>
        public virtual IPagedList<TRoot> FindPaged(int pageIndex, int pageSize, Expression<Func<TRoot>> alias, Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func)
            => Execute(nameof(FindPaged), () => FindPagedInternal(pageIndex, pageSize, alias, func)) ?? new PagedList<TRoot>(Enumerable.Empty<TRoot>(), pageIndex, pageSize, 0);

        /// <summary>
        ///     Calculates the number of records complying with the given function.
        /// </summary>
        /// <param name="func">The predicates that the data must fulfill.</param>
        /// <returns>
        ///     Number of records that satisfying the given <paramref name="func" />.
        /// </returns>
        public virtual int Count(Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func)
            => Execute(nameof(Count), () => CountInternal(func));

        /// <summary>
        ///     Calculates the number of records complying with the given function.
        /// </summary>
        /// <param name="alias">An additional alias.</param>
        /// <param name="func">The predicates that the data must fulfill.</param>
        /// <returns>
        ///     Number of records that satisfying the given <paramref name="func" />.
        /// </returns>
        public virtual int Count(Expression<Func<TRoot>> alias, Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func)
            => Execute(nameof(Count), () => CountInternal(alias, func));

        protected virtual TRoot? GetInternal(Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func)
        {
            try
            {
                return func(CreateQueryOver()).SingleOrDefault<TRoot>();
            }
            catch (EmptyResultException)
            {
                return null;
            }
        }

        protected virtual TRoot? GetInternal(Expression<Func<TRoot>> alias, Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func)
        {
            try
            {
                return func(CreateQueryOver(alias)).SingleOrDefault<TRoot>();
            }
            catch (EmptyResultException)
            {
                return null;
            }
        }

        protected virtual TRoot? GetAtIndexInternal(Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func, int index)
        {
            try
            {
                return func(CreateQueryOver()).Skip(index).Take(1).SingleOrDefault<TRoot>();
            }
            catch (EmptyResultException)
            {
                return null;
            }
        }

        protected virtual TRoot? GetAtIndexInternal(Expression<Func<TRoot>>? alias, Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func, int index)
        {
            try
            {
                return func(CreateQueryOver(alias)).Skip(index).Take(1).SingleOrDefault<TRoot>();
            }
            catch (EmptyResultException)
            {
                return null;
            }
        }

        protected override IList<TRoot> FindAllInternal()
            => CreateQueryOver().List<TRoot>();

        protected virtual IList<TRoot> FindInternal(Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>>? func)
        {
            try
            {
                IQueryOver<TRoot> queryOver = func != null ? func(CreateQueryOver()) : CreateQueryOver();
                return queryOver.List<TRoot>();
            }
            catch (EmptyResultException)
            {
                return new List<TRoot>();
            }
        }

        protected virtual IList<TRoot> FindInternal(Expression<Func<TRoot>>? alias, Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>>? func)
        {
            try
            {
                IQueryOver<TRoot> queryOver = func != null ? func(CreateQueryOver(alias)) : CreateQueryOver(alias);
                return queryOver.List<TRoot>();
            }
            catch (EmptyResultException)
            {
                return new List<TRoot>();
            }
        }

        protected virtual IList<TRoot> FindInternal(Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>>? func, int? skip, int? count)
        {
            try
            {
                IQueryOver<TRoot> queryOver = func != null ? func(CreateQueryOver()) : CreateQueryOver();

                if (skip.HasValue)
                {
                    queryOver = queryOver.Skip(skip.Value);
                }

                if (count.HasValue)
                {
                    queryOver = queryOver.Take(count.Value);
                }

                return queryOver.List<TRoot>();
            }
            catch (EmptyResultException)
            {
                return new List<TRoot>();
            }
        }

        protected virtual IList<TRoot> FindInternal(Expression<Func<TRoot>>? alias, Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>>? func, int? skip, int? count)
        {
            try
            {
                IQueryOver<TRoot> queryOver = func != null ? func(CreateQueryOver(alias)) : CreateQueryOver(alias);

                if (skip.HasValue)
                {
                    queryOver = queryOver.Skip(skip.Value);
                }

                if (count.HasValue)
                {
                    queryOver = queryOver.Take(count.Value);
                }

                return queryOver.List<TRoot>();
            }
            catch (EmptyResultException)
            {
                return new List<TRoot>();
            }
        }

        protected virtual PagedList<TRoot> FindPagedInternal(int pageIndex, int pageSize, Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>>? func)
            => FindPagedInternal(pageIndex, pageSize, () => func != null ? func(CreateQueryOver()) : CreateQueryOver());

        protected virtual PagedList<TRoot> FindPagedInternal(int pageIndex, int pageSize, Expression<Func<TRoot>>? alias, Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>>? func)
            => FindPagedInternal(pageIndex, pageSize, () => func != null ? func(CreateQueryOver(alias)) : CreateQueryOver(alias));

        protected virtual PagedList<TRoot> FindPagedInternal(int pageIndex, int pageSize, Func<IQueryOver<TRoot, TRoot>> queryFactory)
            => FindPagedInternal<TRoot, TRoot>(pageIndex, pageSize, queryFactory);

        protected virtual PagedList<TSubType> FindPagedInternal<TSubType>(int pageIndex, int pageSize, Func<IQueryOver<TRoot, TRoot>> queryFactory)
            => FindPagedInternal<TSubType, TRoot>(pageIndex, pageSize, queryFactory);

        protected virtual PagedList<TDto> FindPagedInternal<TDto, TSubType>(int pageIndex, int pageSize, Func<IQueryOver<TRoot, TSubType>> queryFactory)
        {
            try
            {
                IQueryOver<TRoot, TSubType> rowCountQueryOver = queryFactory();
                IFutureValue<int> rowCount =
                    rowCountQueryOver
                        .ToRowCountQuery()
                        .FutureValue<int>();

                IQueryOver<TRoot, TSubType> pagingQueryOver = queryFactory();
                IList<TDto> qryResult =
                    pagingQueryOver
                        .Skip((pageIndex - 1) * pageSize)
                        .Take(pageSize)
                        .Future<TDto>()
                        .ToList();

                PagedList<TDto> result = new PagedList<TDto>(qryResult, pageIndex, pageSize, rowCount.Value);

                return result;
            }
            catch (EmptyResultException)
            {
                return new PagedList<TDto>(Enumerable.Empty<TDto>(), 1, pageSize, 0);
            }
        }

        protected virtual int CountInternal(Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>>? func)
            => CountInternal(null, func);

        protected virtual int CountInternal(Expression<Func<TRoot>>? alias, Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>>? func)
        {
            try
            {
                IQueryOver<TRoot, TRoot> queryOver =
                    alias != null
                        ? CreateQueryOver(alias)
                        : CreateQueryOver();
                return func?.Invoke(queryOver).RowCount() ?? queryOver.RowCount();
            }
            catch (EmptyResultException)
            {
                return 0;
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<TRoot> FindByIdsInternal(IEnumerable<TId> ids)
            => FindInternal(qry => qry.Where(e => e.Id.IsIn((ICollection)ids)));

        protected virtual IQueryOver<TRoot, TRoot> CreateQueryOver()
            => Session.QueryOver<TRoot>();

        protected virtual IQueryOver<TRoot, TRoot> CreateQueryOver(Expression<Func<TRoot>>? alias)
            => Session.QueryOver(alias);
    }
}
