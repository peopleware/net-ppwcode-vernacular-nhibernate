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

using NHibernate.Dialect;
using NHibernate.Exceptions;

namespace PPWCode.Vernacular.NHibernate.III.SqlServer
{
    public class MsSqlDialect : MsSql2012Dialect
    {
        private IViolatedConstraintNameExtracter _violatedConstraintNameExtracter;

        /// <inheritdoc />
        public override IViolatedConstraintNameExtracter ViolatedConstraintNameExtracter
            => _violatedConstraintNameExtracter
               ?? (_violatedConstraintNameExtracter = new MsSqlViolatedConstraintNameExtracter());

        /// <inheritdoc />
        public override ISQLExceptionConverter BuildSQLExceptionConverter()
            => new MsSqlConverter(ViolatedConstraintNameExtracter);
    }
}
