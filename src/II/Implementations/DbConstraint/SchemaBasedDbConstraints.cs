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
using System.Data;
using System.Data.Common;

using JetBrains.Annotations;

using PPWCode.Vernacular.Exceptions.IV;

namespace PPWCode.Vernacular.NHibernate.II.DbConstraint
{
    /// <inheritdoc cref="DbConstraints" />
    public abstract class SchemaBasedDbConstraints : DbConstraints
    {
        /// <summary>
        ///     Get the SQL command necessary to retrieve following columns (order is important!):
        ///     <list type="number">
        ///         <item>
        ///             <description>Constraint Name</description>
        ///         </item>
        ///         <item>
        ///             <description>Table Name</description>
        ///         </item>
        ///         <item>
        ///             <description>Table Schema</description>
        ///         </item>
        ///         <item>
        ///             <description>Constraint Type</description>
        ///         </item>
        ///     </list>
        /// </summary>
        protected abstract string CommandText { get; }

        /// <inheritdoc />
        protected override DbCommand GetCommand(DbConnection connection, DbTransaction transaction)
        {
            DbCommand command = connection.CreateCommand();
            try
            {
                command.CommandText = CommandText;
                command.CommandType = CommandType.Text;
                command.Transaction = transaction;

                // Catalog parameter
                if (command.CommandText.IndexOf("@catalog", StringComparison.InvariantCultureIgnoreCase) > 0)
                {
                    DbParameter catalogParameter = command.CreateParameter();
                    catalogParameter.DbType = DbType.AnsiString;
                    catalogParameter.Direction = ParameterDirection.Input;
                    catalogParameter.ParameterName = "@catalog";
                    catalogParameter.IsNullable = false;
                    catalogParameter.Value = connection.Database;
                    command.Parameters.Add(catalogParameter);
                }
            }
            catch (Exception)
            {
                command.Dispose();
                throw;
            }

            return command;
        }

        /// <inheritdoc />
        protected override DbConstraintMetadata GetDbConstraintMetadata(DbDataReader reader)
        {
            int index = 0;
            string constraintName = GetNullableString(reader, index++);
            string tableName = GetNullableString(reader, index++);
            string tableSchema = GetNullableString(reader, index++);
            string constraintType = GetNullableString(reader, index);

            if ((constraintName == null)
                || (tableName == null)
                || (tableSchema == null)
                || (constraintType == null))
            {
                throw new ProgrammingError($"The query defined by {nameof(CommandText)}, gives incorrect data, all data should be not-null.");
            }

            return
                new DbConstraintMetadataBuilder()
                    .ConstraintName(constraintName)
                    .TableSchema(tableSchema)
                    .TableName(tableName)
                    .DbConstraintType(constraintType);
        }

        [CanBeNull]
        private string GetNullableString([NotNull] DbDataReader reader, int index)
        {
            object value = reader.GetValue(index);
            if ((value == null) || (value == DBNull.Value))
            {
                return null;
            }

            return Convert.ToString(value);
        }
    }
}
