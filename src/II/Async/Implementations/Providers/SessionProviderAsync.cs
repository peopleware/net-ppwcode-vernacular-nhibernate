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

using System.Data;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using NHibernate;

using PPWCode.Vernacular.NHibernate.II.Async.Interfaces.Providers;
using PPWCode.Vernacular.NHibernate.II.Providers;

namespace PPWCode.Vernacular.NHibernate.II.Async.Implementations.Providers
{
    /// <inheritdoc cref="ISessionProviderAsync" />
    [UsedImplicitly]
    public class SessionProviderAsync
        : SessionProvider,
          ISessionProviderAsync
    {
        public SessionProviderAsync(
            [NotNull] ISession session,
            [NotNull] ITransactionProviderAsync transactionProviderAsync,
            [NotNull] ISafeEnvironmentProviderAsync safeEnvironmentProviderAsync,
            IsolationLevel isolationLevel)
            : base(session, transactionProviderAsync, safeEnvironmentProviderAsync, isolationLevel)
        {
            TransactionProviderAsync = transactionProviderAsync;
            SafeEnvironmentProviderAsync = safeEnvironmentProviderAsync;
        }

        /// <inheritdoc />
        public ITransactionProviderAsync TransactionProviderAsync { get; }

        /// <inheritdoc />
        public ISafeEnvironmentProviderAsync SafeEnvironmentProviderAsync { get; }

        /// <inheritdoc />
        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            Task NHibernateFlushAsync(CancellationToken can)
                => Session.FlushAsync(can);

            Task SafeFlushAsync(CancellationToken can)
                => SafeEnvironmentProviderAsync.RunAsync(nameof(FlushAsync), NHibernateFlushAsync, can);

            return TransactionProviderAsync.RunAsync(Session, IsolationLevel, SafeFlushAsync, cancellationToken);
        }

        /// <inheritdoc />
        public Task CommitAsync(CancellationToken cancellationToken = default)
            => SafeEnvironmentProviderAsync.RunAsync(
                nameof(CommitAsync),
                async can =>
                {
                    ITransaction tran = Session.GetCurrentTransaction();
                    if (tran?.IsActive == true)
                    {
                        await Session.FlushAsync(can).ConfigureAwait(false);
                        await tran.CommitAsync(can).ConfigureAwait(false);
                    }
                },
                cancellationToken);

        /// <inheritdoc />
        public Task RollbackAsync(CancellationToken cancellationToken = default)
            => SafeEnvironmentProviderAsync.RunAsync(
                nameof(CommitAsync),
                async can =>
                {
                    ITransaction tran = Session.GetCurrentTransaction();
                    if (tran?.IsActive == true)
                    {
                        await tran.RollbackAsync(can).ConfigureAwait(false);
                    }
                },
                cancellationToken);
    }
}
