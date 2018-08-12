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
using System.Linq.Expressions;

using JetBrains.Annotations;

using NHibernate;

using PPWCode.Vernacular.Persistence.III;

namespace PPWCode.Vernacular.NHibernate.II
{
    public interface IQueryOverRepository<TRoot, in TId> : IRepository<TRoot, TId>
        where TRoot : class, IIdentity<TId>
        where TId : IEquatable<TId>
    {
        /// <summary>
        ///     Gets an entity by a function.
        /// </summary>
        /// <param name="func">The given function.</param>
        /// <returns>The entity that is filtered by the function or null if not found.</returns>
        [CanBeNull]
        TRoot Get([NotNull] Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func);

        /// <summary>
        ///     Gets an entity by a function.
        /// </summary>
        /// <param name="alias">An additional alias.</param>
        /// <param name="func">The given function.</param>
        /// <returns>The entity that is filtered by the function or null if not found.</returns>
        [CanBeNull]
        TRoot Get([CanBeNull] Expression<Func<TRoot>> alias, [NotNull] Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func);

        /// <summary>
        ///     Executes the given query <paramref name="func" /> and returns the entity at position <paramref name="index" />
        ///     in the result.
        /// </summary>
        /// <param name="func">The given function.</param>
        /// <param name="index">The given index.</param>
        /// <returns>The entity that is filtered by the function or null if not found.</returns>
        [CanBeNull]
        TRoot GetAtIndex([NotNull] Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func, int index);

        /// <summary>
        ///     Executes the given query <paramref name="func" /> and returns the entity at position <paramref name="index" />
        ///     in the result.
        /// </summary>
        /// <param name="alias">An additional alias.</param>
        /// <param name="func">The given function.</param>
        /// <param name="index">The given index.</param>
        /// <returns>The entity that is filtered by the function or null if not found.</returns>
        [CanBeNull]
        TRoot GetAtIndex([CanBeNull] Expression<Func<TRoot>> alias, [NotNull] Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func, int index);

        /// <summary>
        ///     Find the records complying with the given function.
        /// </summary>
        /// <param name="func">The given function.</param>
        /// <remarks>
        ///     <h3>Extra post conditions</h3>
        ///     <para>All elements of the resulting set fulfill <paramref name="func" />.</para>
        /// </remarks>
        /// <returns>
        ///     A list of the records satisfying the given <paramref name="func" />.
        /// </returns>
        [NotNull]
        IList<TRoot> Find([CanBeNull] Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func);

        /// <summary>
        ///     Find the records complying with the given function.
        /// </summary>
        /// <param name="alias">An additional alias.</param>
        /// <param name="func">The given function.</param>
        /// <remarks>
        ///     <h3>Extra post conditions</h3>
        ///     <para>All elements of the resulting set fulfill <paramref name="func" />.</para>
        /// </remarks>
        /// <returns>
        ///     A list of the records satisfying the given <paramref name="func" />.
        /// </returns>
        [NotNull]
        IList<TRoot> Find([CanBeNull] Expression<Func<TRoot>> alias, [CanBeNull] Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func);

        /// <summary>
        ///     Find the records complying with the given function. In this result-set, <paramref name="skip" /> tuples are skipped
        ///     and then <paramref name="count" /> are taken as a result-set.
        /// </summary>
        /// <param name="func">The given function.</param>
        /// <param name="skip">Maximum tuples to skip, if <c>null</c> is specified no tuples are skipped.</param>
        /// <param name="count">Maximum tuples to be read from the result-set, if <c>null</c> is specified all records are read.</param>
        /// <remarks>
        ///     <h3>Extra post conditions.</h3>
        ///     <para>All elements of the resulting set fulfill <paramref name="func" />.</para>
        /// </remarks>
        /// <returns>
        ///     A list of the records satisfying the given <paramref name="func" />.
        /// </returns>
        [NotNull]
        IList<TRoot> Find([CanBeNull] Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func, int? skip, int? count);

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
        [NotNull]
        IList<TRoot> Find([CanBeNull] Expression<Func<TRoot>> alias, [CanBeNull] Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func, int? skip, int? count);

        /// <summary>
        ///     Find a set of records complying with the given function.
        ///     Only a subset of records are returned based on <paramref name="pageSize" /> and <paramref name="pageIndex" />.
        /// </summary>
        /// <param name="pageIndex">The index of the page, indices start from 1.</param>
        /// <param name="pageSize">The size of a page, must be greater then 0.</param>
        /// <param name="func">The predicates that the data must fulfill.</param>
        /// <remarks>
        ///     <h3>Extra post conditions</h3>
        ///     <para>All elements of the resulting set fulfill <paramref name="func" />.</para>
        /// </remarks>
        /// <returns>
        ///     An implementation of <see cref="IPagedList{T}" /> that holds a max. of <paramref name="pageSize" /> records.
        /// </returns>
        [NotNull]
        IPagedList<TRoot> FindPaged(int pageIndex, int pageSize, [CanBeNull] Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func);

        /// <summary>
        ///     Find a set of records complying with the given function.
        ///     Only a subset of records are returned based on <paramref name="pageSize" /> and <paramref name="pageIndex" />.
        /// </summary>
        /// <param name="pageIndex">The index of the page, indices start from 1.</param>
        /// <param name="pageSize">The size of a page, must be greater then 0.</param>
        /// <param name="alias">An additional alias.</param>
        /// <param name="func">The predicates that the data must fulfill.</param>
        /// <remarks>
        ///     <h3>Extra post conditions</h3>
        ///     <para>All elements of the resulting set fulfill <paramref name="func" />.</para>
        /// </remarks>
        /// <returns>
        ///     An implementation of <see cref="IPagedList{T}" /> that holds a max. of <paramref name="pageSize" /> records.
        /// </returns>
        [NotNull]
        IPagedList<TRoot> FindPaged(int pageIndex, int pageSize, [CanBeNull] Expression<Func<TRoot>> alias, [CanBeNull] Func<IQueryOver<TRoot, TRoot>, IQueryOver<TRoot, TRoot>> func);
    }
}
