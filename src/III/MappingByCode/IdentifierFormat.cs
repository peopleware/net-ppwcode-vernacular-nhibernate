// Copyright 2022 by PeopleWare n.v..
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace PPWCode.Vernacular.NHibernate.III.MappingByCode
{
    public enum IdentifierFormat
    {
        /// <summary>
        ///     Keep the names of the identifiers as defined for class names and
        ///     property names.
        /// </summary>
        /// <example>
        ///     MyTestIdentifier => MyTestIdentifier
        /// </example>
        AS_IS = 1,

        /// <summary>
        ///     Convert the names of the identifiers as defined for class names
        ///     and property names to snake case.  Assume that the original
        ///     identifier is in pascal case.
        /// </summary>
        /// <example>
        ///     MyTestIdentifier => my_test_identifier
        /// </example>
        PASCAL_CASE_TO_SNAKE_CASE,

        /// <summary>
        ///     Convert the names of the identifiers as defined for class names
        ///     and property names to screaming snake case.  Assume that the
        ///     original identifier is in pascal case.
        /// </summary>
        /// <example>
        ///     MyTestIdentifier => MY_TEST_IDENTIFIER
        /// </example>
        PASCAL_CASE_TO_SCREAMING_SNAKE_CASE
    }
}
