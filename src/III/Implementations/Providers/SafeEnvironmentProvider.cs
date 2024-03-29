﻿// Copyright 2018 by PeopleWare n.v..
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

using Common.Logging;

using JetBrains.Annotations;

using PPWCode.Vernacular.NHibernate.III.DbConstraint;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.Providers
{
    /// <inheritdoc />
    public class SafeEnvironmentProvider : ISafeEnvironmentProvider
    {
        [NotNull]
        private static readonly ILog _logger = LogManager.GetLogger<SafeEnvironmentProvider>();

        public SafeEnvironmentProvider([NotNull] IExceptionTranslator exceptionTranslator)
        {
            ExceptionTranslator = exceptionTranslator ?? throw new ArgumentNullException(nameof(exceptionTranslator));
        }

        /// <inheritdoc cref="IExceptionTranslator" />
        [NotNull]
        public IExceptionTranslator ExceptionTranslator { get; }

        /// <inheritdoc />
        public void Run(string requestDescription, Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            Run(requestDescription, ActionToDummyFunc(action));
        }

        /// <inheritdoc />
        public TResult Run<TResult>(string requestDescription, Func<TResult> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            string StartMessage()
                => $"Request {requestDescription} started.";

            string FinishMessage()
                => $"Request {requestDescription} finished.";

            string FailedMessage()
                => $"Request {requestDescription} failed.";

            return Run(StartMessage, FinishMessage, FailedMessage, func);
        }

        /// <inheritdoc />
        public void Run<TEntity, TId>(string requestDescription, Action action, TEntity entity)
            where TEntity : class, IIdentity<TId>
            where TId : IEquatable<TId>
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            Run<TEntity, TId, int>(requestDescription, ActionToDummyFunc(action), entity);
        }

        /// <inheritdoc />
        public TResult Run<TEntity, TId, TResult>(string requestDescription, Func<TResult> func, TEntity entity)
            where TEntity : class, IIdentity<TId>
            where TId : IEquatable<TId>
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
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

            return Run(StartMessage, FinishMessage, FailedMessage, func);
        }

        [NotNull]
        private Func<int> ActionToDummyFunc(Action action)
            => () =>
               {
                   action.Invoke();
                   return default(int);
               };

        [CanBeNull]
        protected virtual TResult Run<TResult>(
            [NotNull] Func<string> startMessage,
            [NotNull] Func<string> finishedMessage,
            [NotNull] Func<string> failedMessage,
            [NotNull] Func<TResult> func)
        {
            Stopwatch sw = null;
            if (_logger.IsInfoEnabled)
            {
                _logger.Info(startMessage());
                sw = new Stopwatch();
                sw.Start();
            }

            TResult result;
            try
            {
                result = func.Invoke();
            }
            catch (Exception e)
            {
                throw ExceptionTranslator.Convert(failedMessage() ?? e.Message, e);
            }
            finally
            {
                sw?.Stop();
            }

            if (_logger.IsInfoEnabled)
            {
                _logger.Info(sw != null ? $"{finishedMessage()}, elapsed {sw.ElapsedMilliseconds} ms." : finishedMessage());
            }

            return result;
        }
    }
}
