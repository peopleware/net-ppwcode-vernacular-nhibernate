﻿// Copyright 2018-2022 by PeopleWare n.v..
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

using JetBrains.Annotations;

using NHibernate.Exceptions;

using PPWCode.Vernacular.NHibernate.III.DbConstraint;

namespace PPWCode.Vernacular.NHibernate.III.DbExceptionConverters
{
    /// <inheritdoc cref="ISQLExceptionConverter" />
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1033", Justification = "Reviewed: Explicit interface implementation is done on purpose")]
    public abstract class BaseExceptionConverter
        : ISQLExceptionConverter,
          IConfigurable
    {
        private IDbConstraints _dbConstraints;

        protected BaseExceptionConverter([NotNull] IViolatedConstraintNameExtracter violatedConstraintNameExtracter)
        {
            ViolatedConstraintNameExtracter = violatedConstraintNameExtracter ?? throw new ArgumentNullException(nameof(violatedConstraintNameExtracter));
        }

        /// <inheritdoc cref="IViolatedConstraintNameExtracter" />
        [NotNull]
        public IViolatedConstraintNameExtracter ViolatedConstraintNameExtracter { get; }

        [CanBeNull]
        protected IDictionary<string, string> Configuration { get; private set; }

        /// <inheritdoc cref="IDbConstraints" />
        [CanBeNull]
        protected IDbConstraints DbConstraints
            => _dbConstraints ?? (_dbConstraints = ViolatedConstraintNameExtracter as IDbConstraints);

        /// <inheritdoc cref="IConfigurable.Configure" />
        void IConfigurable.Configure([NotNull] IDictionary<string, string> properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            Configuration = new Dictionary<string, string>(properties);
            DbConstraints?.Initialize(Configuration);
        }

        /// <inheritdoc cref="ISQLExceptionConverter.Convert" />
        Exception ISQLExceptionConverter.Convert([NotNull] AdoExceptionContextInfo adoExceptionContextInfo)
            => OnConvert(adoExceptionContextInfo);

        /// <inheritdoc cref="ISQLExceptionConverter.Convert" />
        [NotNull]
        protected abstract Exception OnConvert([NotNull] AdoExceptionContextInfo adoExceptionContextInfo);

        [CanBeNull]
        protected virtual string GetConstraintName([NotNull] AdoExceptionContextInfo adoExceptionContextInfo)
            => adoExceptionContextInfo.SqlException is DbException sqle
                   ? ViolatedConstraintNameExtracter.ExtractConstraintName(sqle)
                   : null;
    }
}
