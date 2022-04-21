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

using NUnit.Framework;

namespace PPWCode.Vernacular.NHibernate.III.Tests.Utilities
{
    [TestFixture]
    public class StringUtilTests
    {
        [TestCase("MyTestIdentifier", "my_test_identifier")]
        [TestCase("KBONumber", "kbo_number")]
        [TestCase("C4Model", "c4_model")]
        [TestCase("F15Jet", "f15_jet")]
        [TestCase("AMD64InstructionSet", "amd64_instruction_set")]
        public void ConversionSnakeCase(string original, string transformed)
        {
            Assert.AreEqual(transformed, StringUtil.ConvertFromPascalCaseToSnakeCase(original));
        }

        [TestCase("MyTestIdentifier", "MY_TEST_IDENTIFIER")]
        [TestCase("KBONumber", "KBO_NUMBER")]
        [TestCase("C4Model", "C4_MODEL")]
        [TestCase("F15Jet", "F15_JET")]
        [TestCase("AMD64InstructionSet", "AMD64_INSTRUCTION_SET")]
        public void ConversionScreamingSnakeCase(string original, string transformed)
        {
            Assert.AreEqual(transformed, StringUtil.ConvertFromPascalCaseToScreamingSnakeCase(original));
        }
    }
}
