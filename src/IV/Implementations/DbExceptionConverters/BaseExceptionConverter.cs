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
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

using NHibernate.Exceptions;

using PPWCode.Vernacular.NHibernate.IV.DbConstraint;

namespace PPWCode.Vernacular.NHibernate.IV.DbExceptionConverters
{
    /// <inheritdoc cref="ISQLExceptionConverter" />
    [SuppressMessage("Design", "CA1033", Justification = "Reviewed: Explicit interface implementation is done on purpose")]
    public abstract class BaseExceptionConverter(IViolatedConstraintNameExtracter violatedConstraintNameExtracter)
        : ISQLExceptionConverter,
          IConfigurable
    {
        private IDbConstraints? _dbConstraints;

        /// <inheritdoc cref="IViolatedConstraintNameExtracter" />
        public IViolatedConstraintNameExtracter ViolatedConstraintNameExtracter { get; }
            = violatedConstraintNameExtracter ?? throw new ArgumentNullException(nameof(violatedConstraintNameExtracter));

        protected IDictionary<string, string>? Configuration { get; private set; }

        /// <inheritdoc cref="IDbConstraints" />
        protected IDbConstraints? DbConstraints
            => _dbConstraints ??= ViolatedConstraintNameExtracter as IDbConstraints;

        /// <inheritdoc cref="IConfigurable.Configure" />
        void IConfigurable.Configure(IDictionary<string, string> properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            Configuration = new Dictionary<string, string>(properties);
            DbConstraints?.Initialize(Configuration);
        }

        /// <inheritdoc cref="ISQLExceptionConverter.Convert" />
        Exception ISQLExceptionConverter.Convert(AdoExceptionContextInfo adoExceptionContextInfo)
            => OnConvert(adoExceptionContextInfo);

        /// <inheritdoc cref="ISQLExceptionConverter.Convert" />
        protected abstract Exception OnConvert(AdoExceptionContextInfo adoExceptionContextInfo);

        protected virtual string? GetConstraintName(AdoExceptionContextInfo adoExceptionContextInfo)
            => adoExceptionContextInfo.SqlException is DbException sqle
                   ? ViolatedConstraintNameExtracter.ExtractConstraintName(sqle)
                   : null;
    }
}
