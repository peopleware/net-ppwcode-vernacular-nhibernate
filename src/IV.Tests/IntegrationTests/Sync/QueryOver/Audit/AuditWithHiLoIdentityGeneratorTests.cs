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

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.III.Tests.IntegrationTests.Sync.QueryOver.Common;
using PPWCode.Vernacular.NHibernate.III.Tests.Model.Common;

namespace PPWCode.Vernacular.NHibernate.III.Tests.IntegrationTests.Sync.QueryOver.Audit
{
    // ReSharper disable InconsistentNaming
    public class AuditWithHiLoIdentityGeneratorTests : BaseCompanyTests
    {
        [Test]
        public void Created_Audit_Fields_Should_be_Set_After_Save()
        {
            Company company = CreateCompany(CompanyCreationType.NO_CHILDREN);

            Assert.That(IdentityName, Is.EqualTo(company.CreatedBy));
            Assert.That(UtcNow, Is.EqualTo(company.CreatedAt));
        }

        [Test]
        public void Created_Audit_Fields_Should_be_Set_After_Save_With_Children()
        {
            Company company = CreateCompany(CompanyCreationType.WITH_2_CHILDREN);

            Assert.That(IdentityName, Is.EqualTo(company.CreatedBy));
            Assert.That(UtcNow, Is.EqualTo(company.CreatedAt));

            foreach (CompanyIdentification companyIdentification in company.Identifications)
            {
                Assert.That(IdentityName, Is.EqualTo(companyIdentification.CreatedBy));
                Assert.That(UtcNow, Is.EqualTo(companyIdentification.CreatedAt));
            }
        }

        [Test]
        public void LastModified_Audit_Fields_Should_be_Not_Null_After_Save()
        {
            Company company = CreateCompany(CompanyCreationType.NO_CHILDREN);

            Assert.That(company.LastModifiedAt, Is.Not.Null);
            Assert.That(company.LastModifiedBy, Is.Not.Null);
        }

        [Test]
        public void LastModified_Audit_Fields_Should_be_Not_Null_After_Save_With_Children()
        {
            Company company = CreateCompany(CompanyCreationType.WITH_2_CHILDREN);

            Assert.That(company.LastModifiedAt, Is.Not.Null);
            Assert.That(company.LastModifiedBy, Is.Not.Null);

            Assert.That(company.Identifications.Count, Is.EqualTo(2));
            foreach (CompanyIdentification companyIdentification in company.Identifications)
            {
                Assert.That(companyIdentification.LastModifiedAt, Is.Not.Null);
                Assert.That(companyIdentification.LastModifiedBy, Is.Not.Null);
            }
        }
    }
}
