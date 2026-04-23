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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PPWCode.Vernacular.Persistence.V;

namespace PPWCode.Vernacular.NHibernate.IV
{
    /// <inheritdoc cref="ISafeEnvironmentProviderAsync" />
    public class SafeEnvironmentProviderAsync(IExceptionTranslator exceptionTranslator)
        : SafeEnvironmentProvider(exceptionTranslator),
          ISafeEnvironmentProviderAsync
    {
        // Use the static bridge to create the logger
        private static readonly ILogger _logger = PPWLogging.GetLogger<SafeEnvironmentProviderAsync>();

        /// <inheritdoc />
        public Task RunAsync(
            string requestDescription,
            Func<CancellationToken, Task> lambda,
            CancellationToken cancellationToken)
        {
            async Task<int> WrapperAsync(CancellationToken can)
            {
                await lambda(can).ConfigureAwait(false);
                return 0;
            }

            return RunAsync(requestDescription, WrapperAsync, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TResult?> RunAsync<TResult>(
            string requestDescription,
            Func<CancellationToken, Task<TResult?>> lambda,
            CancellationToken cancellationToken)
        {
            string StartMessage()
                => $"Request {requestDescription} started.";

            string FinishMessage()
                => $"Request {requestDescription} finished.";

            string FailedMessage()
                => $"Request {requestDescription} failed.";

            return RunAsync(StartMessage, FinishMessage, FailedMessage, lambda, cancellationToken);
        }

        /// <inheritdoc />
        public Task RunAsync<TEntity, TId>(
            string requestDescription,
            Func<CancellationToken, Task> lambda,
            TEntity? entity,
            CancellationToken cancellationToken)
            where TEntity : class, IIdentity<TId>
            where TId : IEquatable<TId>
        {
            async Task<int> WrapperAsync(CancellationToken can)
            {
                await lambda(can).ConfigureAwait(false);
                return 0;
            }

            return RunAsync<TEntity, TId, int>(requestDescription, WrapperAsync, entity, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TResult?> RunAsync<TEntity, TId, TResult>(
            string requestDescription,
            Func<CancellationToken, Task<TResult?>> lambda,
            TEntity? entity,
            CancellationToken cancellationToken)
            where TEntity : class, IIdentity<TId>
            where TId : IEquatable<TId>
        {
            if (lambda == null)
            {
                throw new ArgumentNullException(nameof(lambda));
            }

            string StartMessage()
                => entity != null
                       ? $"Request {requestDescription} for class {typeof(TEntity).Name}, entity={entity} started"
                       : $"Request {requestDescription} for class {typeof(TEntity).Name} started";

            string FinishMessage()
                => entity != null
                       ? $"Request {requestDescription} for class {typeof(TEntity).Name}, entity={entity} finished"
                       : $"Request {requestDescription} for class {typeof(TEntity).Name} finished";

            string FailedMessage()
                => entity != null
                       ? $"Request {requestDescription} for class {typeof(TEntity).Name}, entity={entity} failed"
                       : $"Request {requestDescription} for class {typeof(TEntity).Name} failed";

            return RunAsync(StartMessage, FinishMessage, FailedMessage, lambda, cancellationToken);
        }

        protected virtual async Task<TResult?> RunAsync<TResult>(
            Func<string> startMessage,
            Func<string> finishedMessage,
            Func<string> failedMessage,
            Func<CancellationToken, Task<TResult?>> lambda,
            CancellationToken cancellationToken)
        {
            Stopwatch? sw = null;
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(startMessage());
                sw = new Stopwatch();
                sw.Start();
            }

            TResult? result;
            try
            {
                result = await lambda(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw ExceptionTranslator.Convert(failedMessage(), e);
            }
            finally
            {
                sw?.Stop();
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(sw != null ? $"{finishedMessage()}, elapsed {sw.ElapsedMilliseconds} ms." : finishedMessage());
            }

            return result;
        }
    }
}
