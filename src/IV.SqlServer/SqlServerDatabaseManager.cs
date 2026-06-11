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

using System.IO;
using System.Reflection;

using Microsoft.Extensions.Logging;

namespace PPWCode.Vernacular.NHibernate.IV.SqlServer;

/// <inheritdoc />
public class SqlServerDatabaseManager : DatabaseManager
{
    /// <inheritdoc />
    public SqlServerDatabaseManager(
        ILogger logger,
        INHibernateSessionFactory nHibernateSessionFactory,
        INhConfiguration nhConfiguration,
        IPpwHbmMapping ppwHbmMapping)
        : base(
            logger,
            nHibernateSessionFactory,
            nhConfiguration,
            ppwHbmMapping)
    {
    }

    /// <inheritdoc />
    protected override void CreateUsingExternalMigrator()
    {
        // NOP
    }

    /// <inheritdoc />
    protected override string? GetDropAllScript()
    {
        Assembly assembly = typeof(SqlServerDatabaseManager).Assembly;
        string resource = $"{typeof(SqlServerDatabaseManager).Namespace}.DropAll.sql";
        Stream? stream = assembly.GetManifestResourceStream(resource);
        if (stream == null)
        {
            return null;
        }

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}
