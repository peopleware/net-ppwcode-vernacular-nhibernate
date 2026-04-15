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
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

using NHibernate.Exceptions;

using PPWCode.Vernacular.NHibernate.IV.DbConstraint;

// MUDO: switch to Microsoft.Data.SqlClient
#pragma warning disable CS0618 // Type or member is obsolete

namespace PPWCode.Vernacular.NHibernate.IV.SqlServer
{
    public class MsSqlViolatedConstraintNameExtracter
        : IViolatedConstraintNameExtracter,
          IDbConstraints
    {
        private readonly IDbConstraints _dbConstraints = new MsSqlDbConstraints();

        /// <inheritdoc />
        public DbConstraintMetadata? GetByConstraintName(string constraintName)
            => _dbConstraints.GetByConstraintName(constraintName);

        /// <inheritdoc />
        public void Initialize(IDictionary<string, string> properties)
        {
            _dbConstraints.Initialize(properties);
        }

        /// <inheritdoc />
        public ISet<DbConstraintMetadata> Constraints
            => _dbConstraints.Constraints;

        /// <inheritdoc />
        public string? ExtractConstraintName(DbException dbException)
        {
            if (ADOExceptionHelper.ExtractDbException(dbException) is SqlException sqle)
            {
                DbConstraintMetadata? dbConstraint =
                    _dbConstraints
                        .Constraints
                        .FirstOrDefault(c => sqle.Message.Contains(c.ConstraintName));
                return dbConstraint?.ConstraintName;
            }

            return null;
        }
    }
}
