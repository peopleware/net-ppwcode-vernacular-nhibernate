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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Mapping;

using PPWCode.Vernacular.Exceptions.V;

namespace PPWCode.Vernacular.NHibernate.IV
{
    /// <inheritdoc cref="IPpwAuxiliaryDatabaseObject" />
    public abstract class PpwAuxiliaryDatabaseObject(IPpwHbmMapping ppwHbmMapping)
        : AbstractAuxiliaryDatabaseObject,
          IPpwAuxiliaryDatabaseObject
    {
        protected static readonly string[] EmptyStringArray = [];
        protected static readonly Column[] EmptyColumnArray = [];

        private Configuration? _configuration;

        /// <inheritdoc cref="IPpwHbmMapping" />
        public IPpwHbmMapping PpwHbmMapping { get; } = ppwHbmMapping;

        protected Configuration? Configuration
            => _configuration;

        public void SetConfiguration(Configuration configuration)
        {
            _configuration = configuration;
        }

        protected virtual PersistentClass? GetPersistentClassFor(Type type)
            => Configuration
                ?.ClassMappings
                .SingleOrDefault(m => m.MappedClass == type);

        protected virtual string? GetTableNameFor(Type type)
            => GetPersistentClassFor(type)?.Table.Name;

        protected virtual string[] GetDiscriminatorColumnNameFor(Type type)
            => GetPersistentClassFor(type)
                   ?.Discriminator
                   ?.ColumnIterator
                   .OfType<Column>()
                   .Select(c => c.Name)
                   .ToArray()
               ?? EmptyStringArray;

        protected virtual string[] GetDiscriminatorValuesFor(Type type)
            => Configuration != null
                   ? Configuration
                       .ClassMappings
                       .SelectMany(classMapping => classMapping.DirectSubclasses, (classMapping, subclass) => new { classMapping, subclass })
                       .Where(t => (t.classMapping.MappedClass == type) && !t.subclass.MappedClass.IsAbstract)
                       .Select(t => t.subclass.DiscriminatorValue)
                       .Union(
                           Configuration
                               .ClassMappings
                               .Where(m => (m.MappedClass == type) && !m.MappedClass.IsAbstract)
                               .Select(m => m.DiscriminatorValue))
                       .ToArray()
                   : [];

        protected virtual Column[] GetColumns<TSource>(Expression<Func<TSource, object>> propertyLambda)
        {
            PropertyInfo propInfo = GetPropertyInfo(propertyLambda);
            PersistentClass? persistentClass = GetPersistentClassFor(typeof(TSource));
            Property? nhProperty =
                persistentClass
                    ?.PropertyIterator
                    .SingleOrDefault(p => string.Equals(p.Name, propInfo.Name, StringComparison.Ordinal));
            return
                nhProperty != null
                    ? nhProperty
                        .Value
                        .ColumnIterator
                        .OfType<Column>()
                        .ToArray()
                    : EmptyColumnArray;
        }

        protected virtual string[] GetColumnNames<TSource>(Expression<Func<TSource, object>> propertyLambda)
            => GetColumns(propertyLambda)
                .Select(c => c.Name)
                .ToArray();

        protected virtual string[] GetIdentifierColumnNames(Type type)
        {
            PersistentClass? persistentClass = GetPersistentClassFor(type);
            if (persistentClass != null)
            {
                return
                    persistentClass
                        .Identifier
                        .ColumnIterator
                        .OfType<Column>()
                        .Select(c => c.Name)
                        .ToArray();
            }

            return EmptyStringArray;
        }

        protected virtual PropertyInfo GetPropertyInfo<TSource>(Expression<Func<TSource, object>> propertyLambda)
        {
            Expression body = propertyLambda.Body;
            MemberExpression? member = body as MemberExpression;
            if ((member == null) && !(body is UnaryExpression unary && ((member = unary.Operand as MemberExpression) != null)))
            {
                throw new ProgrammingError($"Expression \'{propertyLambda}\' does not refer to a property.");
            }

            PropertyInfo? propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ProgrammingError($"Expression \'{propertyLambda}\' refers to a field, not a property.");
            }

            Type type = typeof(TSource);
            if ((propInfo.DeclaringType != null) && !propInfo.DeclaringType.IsAssignableFrom(type))
            {
                throw new ProgrammingError($"Expression \'{propertyLambda}\' refers to a property that is not from type \'{{type}}\'.");
            }

            return propInfo;
        }

        [return: NotNullIfNotNull(nameof(identifier))]
        protected virtual string? RemoveBackTicks(string? identifier)
            => identifier?.Replace("`", string.Empty);

        [return: NotNullIfNotNull(nameof(columnName))]
        protected virtual string? QuoteColumnName(Dialect dialect, string? columnName)
            => !string.IsNullOrEmpty(columnName) && !dialect.IsQuoted(columnName) && PpwHbmMapping.QuoteIdentifiers
                   ? dialect.QuoteForColumnName(columnName)
                   : columnName;

        [return: NotNullIfNotNull(nameof(tableName))]
        protected virtual string? QuoteTableName([NotNull] Dialect dialect, string? tableName)
            => !string.IsNullOrEmpty(tableName) && !dialect.IsQuoted(tableName) && PpwHbmMapping.QuoteIdentifiers
                   ? dialect.QuoteForTableName(tableName)
                   : tableName;

        [return: NotNullIfNotNull(nameof(schemaName))]
        protected virtual string? QuoteSchemaName([NotNull] Dialect dialect, string? schemaName)
            => !string.IsNullOrEmpty(schemaName) && !dialect.IsQuoted(schemaName) && PpwHbmMapping.QuoteIdentifiers
                   ? dialect.QuoteForSchemaName(schemaName)
                   : schemaName;
    }
}
