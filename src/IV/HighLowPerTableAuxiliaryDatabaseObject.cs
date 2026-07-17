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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Engine;
using NHibernate.Mapping.ByCode;

namespace PPWCode.Vernacular.NHibernate.IV
{
    /// <inheritdoc />
    public abstract class HighLowPerTableAuxiliaryDatabaseObject : PpwAuxiliaryDatabaseObject
    {
        protected HighLowPerTableAuxiliaryDatabaseObject(IPpwHbmMapping ppwHbmMapping)
            : base(ppwHbmMapping)
        {
        }

        protected abstract string GeneratorTableName { get; }
        protected abstract string GeneratorEntityNameColumnName { get; }
        protected abstract string GeneratorNextHiColumnName { get; }
        protected abstract string GeneratorTableNameColumnName { get; }

        protected virtual IEnumerable<IGeneratorDef> GeneratorDefs
        {
            get { yield return Generators.HighLow; }
        }

        protected virtual IEnumerable<string> SchemaNames
            => HbmClasses.Select(c => c.schema).Distinct();

        protected virtual IEnumerable<HbmClass> HbmClasses
        {
            get
            {
                ISet<string> generatorClasses = new HashSet<string>(GeneratorDefs.Select(g => g.Class));

                return
                    PpwHbmMapping
                        .HbmMapping
                        .RootClasses
                        .Where(c => generatorClasses.Contains(c.Id.generator.@class));
            }
        }

        protected abstract int GeneratorEntityNameColumnLength(Dialect dialect);

        protected abstract int GeneratorTableNameColumnLength(Dialect dialect);

        public override string SqlCreateString(
            Dialect dialect,
            IMapping mapping,
            string? defaultCatalog,
            string? defaultSchema)
        {
            Context context = new(this, dialect, mapping, defaultCatalog, defaultCatalog);

            string script;
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

            return script;
        }

        public virtual string SqlCreateStringSqlServer(Context context)
        {
            StringBuilder script = new();
            foreach (string schemaName in SchemaNames)
            {
                string generatorTableName = context.GetTableName(schemaName);
                script.AppendLine($"DELETE FROM {generatorTableName};");
                script.AppendLine("GO");
                script.AppendLine($"ALTER TABLE {generatorTableName} ADD {context.EntityNameColumnName} VARCHAR({GeneratorEntityNameColumnLength(context.Dialect)}) NOT NULL;");
                script.AppendLine("GO");
                script.AppendLine($"ALTER TABLE {generatorTableName} ADD {context.TableNameColumnName} VARCHAR({GeneratorTableNameColumnLength(context.Dialect)}) NOT NULL;");
                script.AppendLine("GO");
                script.AppendLine($"ALTER TABLE {generatorTableName} ADD PRIMARY KEY ({context.EntityNameColumnName});");
                script.AppendLine("GO");

                HashSet<HbmClass> hbmClasses = new(HbmClasses.Where(c => c.schema == schemaName));
                foreach (HbmClass hbmClass in hbmClasses)
                {
                    string[] segments = hbmClass.Name.Split(',');
                    string fullClassName = segments[0];
                    string tableName = RemoveBackTicks(hbmClass.table);

                    script.AppendLine($"INSERT INTO {generatorTableName} ({context.NextHiColumnName}, {context.EntityNameColumnName}, {context.TableNameColumnName})");
                    script.AppendLine($"VALUES (0, '{fullClassName}', '{tableName}');");
                }

                script.AppendLine("GO");
            }

            return script.ToString();
        }

        public virtual string SqlCreateStringFirebird(Context context)
        {
            StringBuilder script = new();

            script.AppendLine("execute block");
            script.AppendLine("as");
            script.AppendLine("begin");

            foreach (string schemaName in SchemaNames)
            {
                string generatorTableName = context.GetTableName(schemaName);
                script.AppendLine($"  DELETE FROM {generatorTableName};");
                script.AppendLine($"  execute statement 'ALTER TABLE {generatorTableName} ADD {context.EntityNameColumnName} VARCHAR({GeneratorEntityNameColumnLength(context.Dialect)}) NOT NULL';");
                script.AppendLine($"  execute statement 'ALTER TABLE {generatorTableName} ADD {context.TableNameColumnName} VARCHAR({GeneratorTableNameColumnLength(context.Dialect)}) NOT NULL';");
                script.AppendLine($"  execute statement 'ALTER TABLE {generatorTableName} ADD PRIMARY KEY ({context.EntityNameColumnName})';");
            }

            script.AppendLine("end");
            return script.ToString();
        }

        public virtual string SqlCreateStringPostgreSQL(Context context)
            => SqlCreateStringGeneric(context);

        public virtual string SqlCreateStringGeneric(Context context)
        {
            StringBuilder script = new();
            foreach (string schemaName in SchemaNames)
            {
                string generatorTableName = context.GetTableName(schemaName);
                script.AppendLine($"DELETE FROM {generatorTableName};");
                script.AppendLine($"ALTER TABLE {generatorTableName} ADD {context.EntityNameColumnName} VARCHAR({GeneratorEntityNameColumnLength(context.Dialect)}) NOT NULL;");
                script.AppendLine($"ALTER TABLE {generatorTableName} ADD {context.TableNameColumnName} VARCHAR({GeneratorTableNameColumnLength(context.Dialect)}) NOT NULL;");
                script.AppendLine($"ALTER TABLE {generatorTableName} ADD PRIMARY KEY ({context.EntityNameColumnName});");

                HashSet<HbmClass> hbmClasses = new(HbmClasses.Where(c => c.schema == schemaName));
                foreach (HbmClass hbmClass in hbmClasses)
                {
                    string[] segments = hbmClass.Name.Split(',');
                    string fullClassName = segments[0];
                    string tableName = RemoveBackTicks(hbmClass.table);

                    script.AppendLine($"INSERT INTO {generatorTableName} ({context.NextHiColumnName}, {context.EntityNameColumnName}, {context.TableNameColumnName})");
                    script.AppendLine($"VALUES (0, '{fullClassName}', '{tableName}');");
                }
            }

            return script.ToString();
        }

        public override string SqlDropString(
            Dialect dialect,
            string? defaultCatalog,
            string? defaultSchema)
            => string.Empty;

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "reviewed")]
        public class Context
        {
            public Context(
                HighLowPerTableAuxiliaryDatabaseObject auxiliaryDatabaseObject,
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
                NextHiColumnName = auxiliaryDatabaseObject.PpwHbmMapping.GetIdentifier(auxiliaryDatabaseObject.GeneratorNextHiColumnName);
                EntityNameColumnName = auxiliaryDatabaseObject.PpwHbmMapping.GetIdentifier(auxiliaryDatabaseObject.GeneratorEntityNameColumnName);
                TableNameColumnName = auxiliaryDatabaseObject.PpwHbmMapping.GetIdentifier(auxiliaryDatabaseObject.GeneratorTableNameColumnName);
            }

            public HighLowPerTableAuxiliaryDatabaseObject AuxiliaryDatabaseObject { get; }

            public Dialect Dialect { get; }

            public IMapping Mapping { get; }

            public string? DefaultCatalog { get; }

            public string? DefaultSchema { get; }

            public string NextHiColumnName { get; }

            public string EntityNameColumnName { get; }

            public string TableNameColumnName { get; }

            public string GetTableName(string schemaName)
            {
                string quotedSchemaName = AuxiliaryDatabaseObject.QuoteSchemaName(Dialect, AuxiliaryDatabaseObject.RemoveBackTicks(schemaName));
                string quotedGeneratorTableName = AuxiliaryDatabaseObject.QuoteTableName(Dialect, AuxiliaryDatabaseObject.PpwHbmMapping.GetIdentifier(AuxiliaryDatabaseObject.GeneratorTableName));
                return
                    string.IsNullOrEmpty(quotedSchemaName)
                        ? quotedGeneratorTableName
                        : $"{quotedSchemaName}.{quotedGeneratorTableName}";
            }
        }
    }
}
