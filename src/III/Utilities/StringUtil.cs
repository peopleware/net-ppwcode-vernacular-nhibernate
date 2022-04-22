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

using System.Text.RegularExpressions;

using JetBrains.Annotations;

namespace PPWCode.Vernacular.NHibernate.III
{
    public static class StringUtil
    {
        [ContractAnnotation("null => null; notnull => notnull")]
        private static string ConvertFromPascalCaseToUnderscores(string original)
        {
            const string Rgx = @"([A-Z]+)([A-Z][a-z])";
            const string Rgx2 = @"([a-z\d])([A-Z])";

            if (original != null)
            {
                string result = Regex.Replace(original, Rgx, "$1_$2");
                result = Regex.Replace(result, Rgx2, "$1_$2");
                return result;
            }

            return null;
        }

        [ContractAnnotation("null => null; notnull => notnull")]
        public static string ConvertFromPascalCaseToSnakeCase(string original)
            => ConvertFromPascalCaseToUnderscores(original)?.ToLowerInvariant();

        [ContractAnnotation("null => null; notnull => notnull")]
        public static string ConvertFromPascalCaseToScreamingSnakeCase(string original)
            => ConvertFromPascalCaseToUnderscores(original)?.ToUpperInvariant();
    }
}
