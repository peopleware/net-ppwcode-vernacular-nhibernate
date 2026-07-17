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
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using NHibernate;

using PPWCode.Vernacular.Persistence.V;

namespace PPWCode.Vernacular.NHibernate.IV;

/// <inheritdoc cref="IRelationEntityRepository{TIdentity}" />
public class RelationEntityRepository<TIdentity>
    : IRelationEntityRepository<TIdentity>
    where TIdentity : struct, IEquatable<TIdentity>
{
    private readonly ISessionProviderAsync _sessionProvider;

    /// <summary>
    ///     Initializes a new relation entity repository that executes NHibernate access in a safe transactional
    ///     environment.
    /// </summary>
    /// <param name="sessionProvider">Provides the current NHibernate session and related execution services.</param>
    public RelationEntityRepository(ISessionProviderAsync sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    private ISession Session
        => _sessionProvider.Session;

    private ITransactionProviderAsync TransactionProviderAsync
        => _sessionProvider.TransactionProviderAsync;

    private ISafeEnvironmentProviderAsync SafeEnvironmentProviderAsync
        => _sessionProvider.SafeEnvironmentProviderAsync;

    private IsolationLevel IsolationLevel
        => _sessionProvider.IsolationLevel;

    /// <inheritdoc />
    public async Task<TModel?> GetByIdAsync<TModel>(TIdentity? id, CancellationToken cancellationToken)
        where TModel : class, IPersistentObject<TIdentity>
        => id == null
               ? null
               : await ExecuteAsync(
                         nameof(GetByIdAsync),
                         async can =>
                         {
                             TModel result = await GetByIdInternalAsync<TModel>(id.Value, can).ConfigureAwait(false);
                             return result;
                         },
                         cancellationToken)
                     .ConfigureAwait(false);

    /// <summary>
    ///     Executes repository work in a safe environment with an active transaction.
    /// </summary>
    /// <typeparam name="TResult">The type of the asynchronous operation result.</typeparam>
    /// <param name="requestDescription">A short description of the request for diagnostics and error handling.</param>
    /// <param name="lambda">The repository operation to execute.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
    /// <returns>The task representing the asynchronous repository operation.</returns>
    protected virtual Task<TResult?> ExecuteAsync<TResult>(
        string requestDescription,
        Func<CancellationToken, Task<TResult?>> lambda,
        CancellationToken cancellationToken)
    {
        Task<TResult?> SafeAsync(CancellationToken can)
            => SafeEnvironmentProviderAsync.RunAsync(requestDescription, lambda, can);

        return TransactionProviderAsync.RunAsync(Session, IsolationLevel, SafeAsync, cancellationToken);
    }

    /// <summary>
    ///     Retrieves an entity by its identifier from the current NHibernate session.
    /// </summary>
    /// <typeparam name="TModel">The entity type to retrieve.</typeparam>
    /// <param name="id">The identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
    /// <returns>The entity if it exists; otherwise, <see langword="null" />.</returns>
    protected virtual Task<TModel> GetByIdInternalAsync<TModel>(TIdentity id, CancellationToken cancellationToken)
        where TModel : class, IPersistentObject<TIdentity>
        => Session.GetAsync<TModel>(id, cancellationToken);
}
