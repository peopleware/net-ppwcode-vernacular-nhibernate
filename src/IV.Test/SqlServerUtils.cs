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

using System.Collections.Generic;
using System.Data;

using Microsoft.Data.SqlClient;

using PPWCode.Vernacular.Exceptions.V;

// MUDO: switch to Microsoft.Data.SqlClient
#pragma warning disable CS0618 // Type or member is obsolete

namespace PPWCode.Vernacular.NHibernate.IV.Test
{
    public static class SqlServerUtils
    {
        private static string GetConnectionString(
            string sqlConnectionString,
            string? catalog,
            bool pooling)
        {
            SqlConnectionStringBuilder builder =
                new(sqlConnectionString)
                {
                    InitialCatalog = catalog ?? @"master",
                    Pooling = pooling
                };
            return builder.ConnectionString;
        }

        private static SqlConnection GetConnection(string sqlConnectionString, string? catalog, bool pooling)
            => new(GetConnectionString(sqlConnectionString, catalog, pooling));

        private static void ExecuteCommands(
            SqlConnection connection,
            int commandTimeout,
            IEnumerable<string> scripts)
        {
            bool wasOpen = connection.State == ConnectionState.Open;
            if (!wasOpen)
            {
                connection.Open();
            }

            try
            {
                using SqlCommand command = connection.CreateCommand();
                foreach (string script in scripts)
                {
                    if (commandTimeout > 0)
                    {
                        command.CommandTimeout = commandTimeout;
                    }

                    command.CommandText = script;
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                if (!wasOpen)
                {
                    connection.Close();
                }
            }
        }

        private static void ExecuteCommands(
            string sqlConnectionString,
            string? catalog,
            bool pooling,
            int commandTimeout,
            IEnumerable<string> scripts)
        {
            using SqlConnection connection = GetConnection(sqlConnectionString, catalog, pooling);
            ExecuteCommands(connection, commandTimeout, scripts);
        }

        private static bool DataSourceExists(string sqlConnectionString)
            => true;

        public static bool CatalogExists(string sqlConnectionString, string? catalog)
        {
            const string CmdText = @"select null from master.dbo.sysdatabases where name=@name";

            using SqlConnection connection = GetConnection(sqlConnectionString, null, false);
            connection.Open();
            using SqlCommand sqlCommand = new(CmdText, connection);
            SqlParameter param =
                new()
                {
                    ParameterName = "@name",
                    Value = catalog
                };
            sqlCommand.Parameters.Add(param);
            using SqlDataReader dataReader = sqlCommand.ExecuteReader();
            return dataReader.HasRows;
        }

        public static void DropCatalog(string sqlConnectionString, string? catalog)
        {
            if (!DataSourceExists(sqlConnectionString))
            {
                string dataSource = new SqlConnectionStringBuilder(sqlConnectionString).DataSource;
                throw new SemanticException($"datasource={dataSource} unknown");
            }

            // Put database in single user mode and force a disconnect of the other users.
            string[] commands =
            {
                $@"exec msdb.dbo.sp_delete_database_backuphistory @database_name=N'{catalog}'",
                $"alter database [{catalog}] set single_user with rollback immediate",
                $"drop database [{catalog}]"
            };
            ExecuteCommands(sqlConnectionString, null, false, 0, commands);
        }

        public static void CreateCatalog(
            string sqlConnectionString,
            string? catalog,
            bool simpleMode)
        {
            IList<string> commands =
                new List<string>
                {
                    $"create database [{catalog}]"
                };
            if (simpleMode)
            {
                commands.Add($"alter database [{catalog}] set recovery simple with no_wait");
            }

            ExecuteCommands(sqlConnectionString, null, false, 0, commands);
        }
    }
}
