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

using System.Diagnostics;

using PPWCode.Vernacular.NHibernate.IV.Exceptions;

namespace PPWCode.Vernacular.NHibernate.IV
{
    public class DbConstraintMetadataBuilder(
        string constraintName,
        string tableSchema,
        string tableName)
    {
        private string _constraintName = constraintName;
        private DbConstraintTypeEnum _constraintType = DbConstraintTypeEnum.UNKNOWN;
        private string _tableName = tableName;
        private string _tableSchema = tableSchema;

        [DebuggerStepThrough]
        public DbConstraintMetadataBuilder Merge(DbConstraintMetadata metadata)
            => DbConstraintType(metadata.ConstraintType)
                .ConstraintName(metadata.ConstraintName)
                .TableSchema(metadata.TableSchema)
                .TableName(metadata.TableName);

        [DebuggerStepThrough]
        public DbConstraintMetadataBuilder Merge(DbConstraintMetadataBuilder builder)
            => DbConstraintType(builder._constraintType)
                .ConstraintName(builder._constraintName)
                .TableSchema(builder._tableSchema)
                .TableName(builder._tableName);

        [DebuggerStepThrough]
        public DbConstraintMetadataBuilder ConstraintName(string constraintName)
        {
            _constraintName = constraintName;
            return this;
        }

        [DebuggerStepThrough]
        public DbConstraintMetadataBuilder TableName(string tableName)
        {
            _tableName = tableName;
            return this;
        }

        [DebuggerStepThrough]
        public DbConstraintMetadataBuilder TableSchema(string tableSchema)
        {
            _tableSchema = tableSchema;
            return this;
        }

        [DebuggerStepThrough]
        public DbConstraintMetadataBuilder DbConstraintType(DbConstraintTypeEnum dbConstraintType)
        {
            _constraintType = dbConstraintType;
            return this;
        }

        [DebuggerStepThrough]
        public DbConstraintMetadataBuilder DbConstraintType(string constraintType)
        {
            switch (constraintType)
            {
                case "PRIMARY KEY":
                    _constraintType = DbConstraintTypeEnum.PRIMARY_KEY;
                    break;

                case "UNIQUE":
                    _constraintType = DbConstraintTypeEnum.UNIQUE;
                    break;

                case "FOREIGN KEY":
                    _constraintType = DbConstraintTypeEnum.FOREIGN_KEY;
                    break;

                case "CHECK":
                    _constraintType = DbConstraintTypeEnum.CHECK;
                    break;

                case "NOT NULL":
                    _constraintType = DbConstraintTypeEnum.NOT_NULL;
                    break;

                default:
                    _constraintType = DbConstraintTypeEnum.UNKNOWN;
                    break;
            }

            return this;
        }

        [DebuggerStepThrough]
        public DbConstraintMetadata Build()
            => this;

        [DebuggerStepThrough]
        public static implicit operator DbConstraintMetadata(DbConstraintMetadataBuilder builder)
            => new(builder._constraintName, builder._tableName, builder._tableSchema, builder._constraintType);
    }
}
