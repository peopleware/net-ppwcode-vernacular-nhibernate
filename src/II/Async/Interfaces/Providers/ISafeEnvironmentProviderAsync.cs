// Copyright  by PeopleWare n.v..
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

using JetBrains.Annotations;

using PPWCode.Vernacular.NHibernate.II.Providers;
using PPWCode.Vernacular.Persistence.III;

namespace PPWCode.Vernacular.NHibernate.II.Async.Interfaces.Providers
{
    /// <inheritdoc />
    public interface ISafeEnvironmentProviderAsync : ISafeEnvironmentProvider
    {
        [NotNull]
        Task RunAsync(
            [NotNull] string requestDescription,
            [NotNull] Func<CancellationToken, Task> lambda,
            CancellationToken cancellationToken = default);

        [NotNull]
        [ItemCanBeNull]
        Task<TResult> RunAsync<TResult>(
            [NotNull] string requestDescription,
            [NotNull] Func<CancellationToken, Task<TResult>> lambda,
            CancellationToken cancellationToken = default);

        [NotNull]
        Task RunAsync<TEntity, TId>(
            [NotNull] string requestDescription,
            [NotNull] Func<CancellationToken, Task> lambda,
            [CanBeNull] TEntity entity,
            CancellationToken cancellationToken = default)
            where TEntity : class, IIdentity<TId>
            where TId : IEquatable<TId>;

        [NotNull]
        [ItemCanBeNull]
        Task<TResult> RunAsync<TEntity, TId, TResult>(
            [NotNull] string requestDescription,
            [NotNull] Func<CancellationToken, Task<TResult>> lambda,
            [CanBeNull] TEntity entity,
            CancellationToken cancellationToken = default)
            where TEntity : class, IIdentity<TId>
            where TId : IEquatable<TId>;
    }
}