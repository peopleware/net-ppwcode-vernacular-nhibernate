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

using NHibernate;

namespace PPWCode.Vernacular.NHibernate.IV
{
    /// <inheritdoc />
    public class SessionProvider(
        ISession session,
        ITransactionProvider transactionProvider,
        ISafeEnvironmentProvider safeEnvironmentProvider,
        IsolationLevel isolationLevel)
        : ISessionProvider
    {
        /// <inheritdoc />
        public ISession Session { get; } = session;

        /// <inheritdoc />
        public ITransactionProvider TransactionProvider { get; } = transactionProvider;

        /// <inheritdoc />
        public ISafeEnvironmentProvider SafeEnvironmentProvider { get; } = safeEnvironmentProvider;

        /// <inheritdoc />
        public IsolationLevel IsolationLevel { get; } = isolationLevel;

        /// <inheritdoc />
        public void Flush()
            => TransactionProvider.Run(Session, IsolationLevel, () => SafeEnvironmentProvider.Run(nameof(Flush), () => Session.Flush()));
    }
}
