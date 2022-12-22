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

using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using NHibernate;

using PPWCode.Vernacular.NHibernate.II.Providers;

namespace PPWCode.Vernacular.NHibernate.II.Async.Interfaces.Providers
{
    /// <inheritdoc />
    public interface ISessionProviderAsync : ISessionProvider
    {
        /// <inheritdoc cref="ITransactionProviderAsync" />
        [NotNull]
        ITransactionProviderAsync TransactionProviderAsync { get; }

        /// <inheritdoc cref="ISafeEnvironmentProviderAsync" />
        [NotNull]
        ISafeEnvironmentProviderAsync SafeEnvironmentProviderAsync { get; }

        /// <inheritdoc cref="ISession.FlushAsync" />
        [NotNull]
        Task FlushAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc cref="ITransaction.CommitAsync" />
        [NotNull]
        Task CommitAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc cref="ITransaction.RollbackAsync" />
        [NotNull]
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
