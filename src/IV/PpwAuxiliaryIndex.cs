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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using NHibernate.Dialect;
using NHibernate.Engine;
using NHibernate.Mapping;

using PPWCode.Vernacular.Exceptions.V;

namespace PPWCode.Vernacular.NHibernate.IV
{
    /// <inheritdoc />
    public abstract class PpwAuxiliaryIndex<TEntity>(IPpwHbmMapping ppwHbmMapping)
        : PpwAuxiliaryDatabaseObject(ppwHbmMapping)
        where TEntity : class
    {
        protected Func<IEnumerable<Column>>[]? ColumnDefinitions { get; set; }

        protected Func<IEnumerable<Column>>[]? CoveringColumnDefinitions { get; set; }

        protected virtual string? Filter
            => null;

        protected abstract string GetIndexName();
        protected abstract bool IsUnique();

        protected virtual PersistentClass? GetPersistentClassFor()
            => GetPersistentClassFor(typeof(TEntity));

        protected virtual Column[] GetColumns(Expression<Func<TEntity, object>> propertyLambda)
            => base.GetColumns(propertyLambda);

        protected virtual Column[] GetDiscriminatorColumnsFor(Type type)
            => GetPersistentClassFor(type)
                   ?.Discriminator
                   ?.ColumnIterator
                   .OfType<Column>()
                   .ToArray()
               ?? EmptyColumnArray;

        protected virtual string[] GetDiscriminatorValues()
            => GetDiscriminatorValuesFor(typeof(TEntity));

        public override string SqlCreateString(
            Dialect dialect,
            IMapping mapping,
            string? defaultCatalog,
            string? defaultSchema)
        {
            Context context = new(this, dialect, mapping, defaultCatalog, defaultCatalog);

            string script;
            if (ColumnDefinitions != null)
            {
                if (dialect is MsSql2000Dialect)
                {
                    script = SqlCreateStringSqlServer(context);
                }
                else if (dialect is FirebirdDialect)
                {
                    script = SqlCreateStringFirebird(context);
                }
                else if (dialect is PostgreSQLDialect)
                {
                    script = SqlCreateStringPostgreSQL(context);
                }
                else
                {
                    script = SqlCreateStringGeneric(context);
                }
            }
            else
            {
                script = string.Empty;
            }

            return script;
        }

        protected virtual string SqlCreateStringSqlServer(Context context)
        {
            StringBuilder sb = new();

            sb.Append(
                IsUnique()
                    ? $"create unique index {context.IndexName} on {context.TableName}({context.ColumnNames})"
                    : $"create index {context.IndexName} on {context.TableName}({context.ColumnNames})");

            if (context.CoveringColumnNames != null)
            {
                sb.AppendLine();
                sb.Append($"  include ({context.CoveringColumnNames})");
            }

            if (context.Filter != null)
            {
                sb.AppendLine();
                sb.Append($"  where {context.Filter}");
            }

            sb.AppendLine(";");
            sb.Append("GO");
            return sb.ToString();
        }

        protected virtual string SqlCreateStringFirebird(Context context)
            => SqlCreateStringGeneric(context);

        protected virtual string SqlCreateStringPostgreSQL(Context context)
        {
            StringBuilder sb = new();

            sb.Append(
                IsUnique()
                    ? $"create unique index {context.IndexName} on {context.TableName}({context.ColumnNames})"
                    : $"create index {context.IndexName} on {context.TableName}({context.ColumnNames})");

            if (context.CoveringColumnNames != null)
            {
                sb.AppendLine();
                sb.Append($"  include ({context.CoveringColumnNames})");
            }

            if (context.Filter != null)
            {
                sb.AppendLine();
                sb.Append($"  where {context.Filter}");
            }

            sb.AppendLine(";");
            return sb.ToString();
        }

        protected virtual string SqlCreateStringGeneric(Context context)
        {
            StringBuilder sb = new();

            if (IsUnique())
            {
                sb.AppendLine($"alter table {context.TableName}");
                sb.AppendLine($"  add constraint {context.IndexName} unique({context.ColumnNames});");
            }
            else
            {
                sb.AppendLine($"create index {context.IndexName}");
                sb.AppendLine($"  on {context.TableName} ({context.ColumnNames});");
            }

            if (context.CoveringColumnNames != null)
            {
                throw new ProgrammingError($"Covering columns, {context.CoveringColumnNames}, specified for generic generation of unique constraint {context.IndexName}.");
            }

            if (context.Filter != null)
            {
                throw new ProgrammingError($"Filter, {context.Filter}, specified for generic generation of unique constraint {context.IndexName}.");
            }

            return sb.ToString();
        }

        public override string SqlDropString(Dialect dialect, string? defaultCatalog, string? defaultSchema)
            => string.Empty;

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "reviewed")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "reviewed")]
        protected class Context
        {
            public Context(
                PpwAuxiliaryIndex<TEntity> auxiliaryDatabaseObject,
                Dialect dialect,
                IMapping mapping,
                string? defaultCatalog,
                string? defaultSchema)
            {
                Dialect = dialect;
                Mapping = mapping;
                DefaultCatalog = defaultCatalog;
                DefaultSchema = defaultSchema;
                AuxiliaryDatabaseObject = auxiliaryDatabaseObject;

                Table = auxiliaryDatabaseObject.GetPersistentClassFor()?.Table
                        ?? throw new ProgrammingError($"Unable to determine physical table information for entity type {typeof(TEntity).FullName}");
                string schema = defaultSchema ?? Table.Schema;
                Schema =
                    schema != null
                        ? auxiliaryDatabaseObject.QuoteSchemaName(Dialect, schema)
                        : null;

                TableName = auxiliaryDatabaseObject.QuoteTableName(Dialect, Table.Name);
                if (Schema != null)
                {
                    TableName = $"{Schema}.{TableName}";
                }

                IndexName = auxiliaryDatabaseObject.GetIndexName();

                List<Column>? columns =
                    auxiliaryDatabaseObject
                        .ColumnDefinitions
                        ?.SelectMany(x => x())
                        .ToList();
                if ((columns == null) || !columns.Any())
                {
                    throw new ProgrammingError($"Unable to determine the physical column(s) needed for creating a unique key for entity type {typeof(TEntity).FullName}.");
                }

                Columns = columns;
                IEnumerable<string> columnNames = Columns.Select(c => auxiliaryDatabaseObject.QuoteColumnName(dialect, c.Name));
                ColumnNames = string.Join(",", columnNames);

                CoveringColumns =
                    auxiliaryDatabaseObject
                        .CoveringColumnDefinitions
                        ?.SelectMany(x => x())
                        .ToList();
                if (CoveringColumns != null)
                {
                    IEnumerable<string> coveringColumnNames = CoveringColumns.Select(c => auxiliaryDatabaseObject.QuoteColumnName(dialect, c.Name));
                    CoveringColumnNames = string.Join(",", coveringColumnNames);
                }

                Filter = auxiliaryDatabaseObject.Filter;
            }

            public PpwAuxiliaryIndex<TEntity> AuxiliaryDatabaseObject { get; }
            public Dialect Dialect { get; }
            public IMapping Mapping { get; }
            public string? DefaultCatalog { get; }
            public string? DefaultSchema { get; }
            public string? Schema { get; }
            public Table Table { get; }
            public List<Column> Columns { get; }
            public List<Column>? CoveringColumns { get; }
            public string TableName { get; }
            public string IndexName { get; }
            public string ColumnNames { get; }
            public string? CoveringColumnNames { get; }
            public string? Filter { get; }
        }
    }
}
