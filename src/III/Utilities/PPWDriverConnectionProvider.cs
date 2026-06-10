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
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

using NHibernate.Connection;

namespace PPWCode.Vernacular.NHibernate.III
{
    /// <inheritdoc />
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Castle Windsor usage")]
#if NETSTANDARD2_0 || NET462_OR_GREATER
    [Serializable]
#endif
    public class PPWDriverConnectionProvider : DriverConnectionProvider
    {
        [CanBeNull]
        private ILogger _logger;

        [JetBrains.Annotations.NotNull]
        public ILogger Logger
            => _logger ??= PPWLogging.GetLogger(GetType());

        /// <summary>
        ///     Closes and Disposes of the <see cref="T:System.Data.IDbConnection" />.
        ///     In some cases we have seen that the <paramref name="conn" /> was not known (aka null), the result was
        ///     that the exception <see cref="NullReferenceException" /> was thrown in the abstract class
        ///     <see cref="ConnectionProvider" />.
        ///     Usage :
        ///     <property name="connection.provider">
        ///         PPWCode.Vernacular.NHibernate.I.Utilities.PPWDriverConnectionProvider,
        ///         PPWCode.Vernacular.NHibernate.I
        ///     </property>
        /// </summary>
        /// <param name="conn">The <see cref="T:System.Data.IDbConnection" /> to clean up.</param>
        public override void CloseConnection([CanBeNull] DbConnection conn)
        {
            if (conn != null)
            {
                base.CloseConnection(conn);
            }
            else if (Logger.IsEnabled(LogLevel.Warning))
            {
                StringBuilder sb =
                    new StringBuilder()
                        .AppendLine("CloseConnection called with <null> conn.")
                        .AppendLine()
                        .AppendLine("Stack Trace :")
                        .AppendLine()
                        .AppendLine(Environment.StackTrace)
                        .AppendLine();
                Logger.LogWarning(sb.ToString());
            }
        }
    }
}
