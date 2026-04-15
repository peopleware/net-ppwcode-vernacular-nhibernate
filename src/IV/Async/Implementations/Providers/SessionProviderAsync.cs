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

using System.Data;
using System.Threading;
using System.Threading.Tasks;

using NHibernate;

using PPWCode.Vernacular.NHibernate.IV.Async.Interfaces.Providers;
using PPWCode.Vernacular.NHibernate.IV.Providers;

namespace PPWCode.Vernacular.NHibernate.IV.Async.Implementations.Providers
{
    /// <inheritdoc cref="ISessionProviderAsync" />
    public class SessionProviderAsync(
        ISession session,
        ITransactionProviderAsync transactionProviderAsync,
        ISafeEnvironmentProviderAsync safeEnvironmentProviderAsync,
        IsolationLevel isolationLevel)
        : SessionProvider(session, transactionProviderAsync, safeEnvironmentProviderAsync, isolationLevel),
          ISessionProviderAsync
    {
        /// <inheritdoc />
        public ITransactionProviderAsync TransactionProviderAsync { get; } = transactionProviderAsync;

        /// <inheritdoc />
        public ISafeEnvironmentProviderAsync SafeEnvironmentProviderAsync { get; } = safeEnvironmentProviderAsync;

        /// <inheritdoc />
        public Task FlushAsync(CancellationToken cancellationToken)
        {
            Task NHibernateFlushAsync(CancellationToken can)
                => Session.FlushAsync(can);

            Task SafeFlushAsync(CancellationToken can)
                => SafeEnvironmentProviderAsync.RunAsync(nameof(FlushAsync), NHibernateFlushAsync, can);

            return TransactionProviderAsync.RunAsync(Session, IsolationLevel, SafeFlushAsync, cancellationToken);
        }
    }
}
