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

using System.Linq;

using NUnit.Framework;

using PPWCode.Vernacular.NHibernate.IV.Tests.Model.Common;

namespace PPWCode.Vernacular.NHibernate.IV.Tests.IntegrationTests.Sync.QueryOver.Common
{
    // ReSharper disable InconsistentNaming
    public class CompanyBiDirectionalityTests : BaseCompanyTests
    {
        [Test]
        public void Check_BiDirectionality_Add_Child_To_Parent()
        {
            Company company = CreateCompany(CompanyCreationType.NO_CHILDREN);

            // Add child
            CompanyIdentification companyIdentification =
                new CompanyIdentification
                {
                    Identification = "1"
                };
            company.AddIdentification(companyIdentification);
            Assert.That(company.Identifications.Count, Is.EqualTo(1));
            foreach (CompanyIdentification identification in company.Identifications)
            {
                Assert.That(identification.IsTransient, Is.True);
            }

            Company savedCompany = RunInsideTransaction(() => Repository.Merge(company), true);
            Assert.That(savedCompany, Is.Not.Null);
            Assert.That(savedCompany.PersistenceVersion, Is.EqualTo(2));
            Assert.That(savedCompany.Identifications.Count, Is.EqualTo(1));
            foreach (CompanyIdentification identification in savedCompany.Identifications)
            {
                Assert.That(identification.IsTransient, Is.False);
            }
        }

        [Test]
        public void Check_BiDirectionality_Add_Childs_To_Parent()
        {
            Company company = CreateCompany(CompanyCreationType.NO_CHILDREN);

            // Add child
            CompanyIdentification companyIdentification =
                new CompanyIdentification
                {
                    Identification = "1"
                };
            company.AddIdentification(companyIdentification);
            CompanyIdentification companyIdentification2 =
                new CompanyIdentification
                {
                    Identification = "1"
                };
            company.AddIdentification(companyIdentification2);
            Assert.That(company.Identifications.Count, Is.EqualTo(2));
            foreach (CompanyIdentification identification in company.Identifications)
            {
                Assert.That(identification.IsTransient, Is.True);
            }

            Company savedCompany = RunInsideTransaction(() => Repository.Merge(company), true);
            Assert.That(savedCompany, Is.Not.Null);
            Assert.That(savedCompany.PersistenceVersion, Is.EqualTo(2));
            Assert.That(savedCompany.Identifications.Count, Is.EqualTo(2));
            foreach (CompanyIdentification identification in savedCompany.Identifications)
            {
                Assert.That(identification.IsTransient, Is.False);
            }
        }

        [Test]
        public void Check_BiDirectionality_Attach_Parent_To_Child()
        {
            Company company = CreateCompany(CompanyCreationType.NO_CHILDREN);

            // Attach parent to child
            // ReSharper disable once ObjectCreationAsStatement
            new CompanyIdentification
            {
                Identification = "1",
                Company = company
            };
            Assert.That(company.Identifications.Count, Is.EqualTo(1));
            foreach (CompanyIdentification identification in company.Identifications)
            {
                Assert.That(identification.IsTransient, Is.True);
            }

            Company savedCompany = RunInsideTransaction(() => Repository.Merge(company), true);
            Assert.That(savedCompany, Is.Not.Null);
            Assert.That(savedCompany.PersistenceVersion, Is.EqualTo(2));
            Assert.That(savedCompany.Identifications.Count, Is.EqualTo(1));
            foreach (CompanyIdentification identification in savedCompany.Identifications)
            {
                Assert.That(identification.IsTransient, Is.False);
            }
        }

        [Test]
        public void Check_BiDirectionality_Attach_Parent_To_Child2()
        {
            Company company = CreateCompany(CompanyCreationType.NO_CHILDREN);

            // Attach parent to child
            // ReSharper disable once ObjectCreationAsStatement
            new CompanyIdentification
            {
                Identification = "1",
                Company = company
            };

            // ReSharper disable once ObjectCreationAsStatement
            new CompanyIdentification
            {
                Identification = "1",
                Company = company
            };
            Assert.That(company.Identifications.Count, Is.EqualTo(2));
            foreach (CompanyIdentification identification in company.Identifications)
            {
                Assert.That(identification.IsTransient, Is.True);
            }

            Company savedCompany = RunInsideTransaction(() => Repository.Merge(company), true);
            Assert.That(savedCompany, Is.Not.Null);
            Assert.That(savedCompany.PersistenceVersion, Is.EqualTo(2));
            Assert.That(savedCompany.Identifications.Count, Is.EqualTo(2));
            foreach (CompanyIdentification identification in savedCompany.Identifications)
            {
                Assert.That(identification.IsTransient, Is.False);
            }
        }

        [Test]
        public void Check_BiDirectionality_Detach_Parent_From_Child()
        {
            Company company = CreateCompany(CompanyCreationType.WITH_2_CHILDREN);

            Company updatedCompany =
                RunInsideTransaction(
                    () =>
                    {
                        Company mergedCompany = Repository.Merge(company);

                        CompanyIdentification companyIdentification =
                            mergedCompany
                                .Identifications
                                .SingleOrDefault(i => i.Identification == "1");
                        Assert.That(companyIdentification, Is.Not.Null);
                        companyIdentification.Company = null;

                        return mergedCompany;
                    },
                    true);

            Assert.That(updatedCompany, Is.Not.Null);
            Company selectedCompany = RunInsideTransaction(() => Repository.GetById(updatedCompany.Id), false);
            Assert.That(selectedCompany, Is.Not.Null);
            Assert.That(selectedCompany.Identifications.Count, Is.EqualTo(1));
        }

        [Test]
        public void Check_BiDirectionality_Detach_Parent_From_Child2()
        {
            Company company = CreateCompany(CompanyCreationType.WITH_2_CHILDREN);

            Company updatedCompany =
                RunInsideTransaction(
                    () =>
                    {
                        Company mergedCompany = Repository.Merge(company);

                        foreach (CompanyIdentification identification in mergedCompany.Identifications.ToList())
                        {
                            identification.Company = null;
                        }

                        return mergedCompany;
                    },
                    true);

            Assert.That(updatedCompany, Is.Not.Null);
            Company selectedCompany = RunInsideTransaction(() => Repository.GetById(updatedCompany.Id), false);
            Assert.That(selectedCompany, Is.Not.Null);
            Assert.That(selectedCompany.Identifications.Any(), Is.False);
        }

        [Test]
        public void Check_BiDirectionality_FailedCompany_Add_FailedCompany_To_Company()
        {
            Company company = CreateCompany(CompanyCreationType.NO_CHILDREN);

            Company updatedCompany =
                RunInsideTransaction(
                    () =>
                    {
                        Company mergedCompany = Repository.Merge(company);

                        FailedCompany failedCompany =
                            new FailedCompany
                            {
                                FailingDate = UtcNow
                            };
                        mergedCompany.FailedCompany = failedCompany;

                        return mergedCompany;
                    },
                    true);

            Assert.That(updatedCompany, Is.Not.Null);
            Company selectedCompany = RunInsideTransaction(() => Repository.GetById(updatedCompany.Id), false);
            Assert.That(selectedCompany, Is.Not.Null);
            Assert.That(selectedCompany.FailedCompany, Is.Not.Null);
            Assert.That(selectedCompany.IsFailed, Is.True);
        }

        [Test]
        public void Check_BiDirectionality_FailedCompany_Add_FailedCompany_To_Company2()
        {
            Company company = CreateCompany(CompanyCreationType.NO_CHILDREN);

            Company updatedCompany =
                RunInsideTransaction(
                    () =>
                    {
                        Company mergedCompany = Repository.Merge(company);

                        // ReSharper disable once ObjectCreationAsStatement
                        new FailedCompany
                        {
                            FailingDate = UtcNow,
                            Company = mergedCompany
                        };

                        return mergedCompany;
                    },
                    true);

            Assert.That(updatedCompany, Is.Not.Null);
            Company selectedCompany = RunInsideTransaction(() => Repository.GetById(updatedCompany.Id), false);
            Assert.That(selectedCompany, Is.Not.Null);
            Assert.That(selectedCompany.FailedCompany, Is.Not.Null);
            Assert.That(selectedCompany.IsFailed, Is.True);
        }

        [Test]
        public void Check_BiDirectionality_FailedCompany_Remove_FailedCompany_From_Company()
        {
            Company company = CreateFailedCompany(CompanyCreationType.NO_CHILDREN);

            Company updatedCompany =
                RunInsideTransaction(
                    () =>
                    {
                        Company mergedCompany = Repository.Merge(company);
                        mergedCompany.FailedCompany = null;
                        return mergedCompany;
                    },
                    true);

            Assert.That(updatedCompany, Is.Not.Null);
            Company selectedCompany = RunInsideTransaction(() => Repository.GetById(updatedCompany.Id), false);
            Assert.That(selectedCompany, Is.Not.Null);
            Assert.That(selectedCompany.FailedCompany, Is.Null);
            Assert.That(selectedCompany.IsFailed, Is.False);
        }

        [Test]
        public void Check_BiDirectionality_Remove_Child_From_Parent()
        {
            Company company = CreateCompany(CompanyCreationType.WITH_2_CHILDREN);

            Company updatedCompany =
                RunInsideTransaction(
                    () =>
                    {
                        Company mergedCompany = Repository.Merge(company);

                        CompanyIdentification companyIdentification =
                            mergedCompany
                                .Identifications
                                .SingleOrDefault(i => i.Identification == "1");
                        Assert.That(companyIdentification, Is.Not.Null);
                        mergedCompany.RemoveIdentification(companyIdentification);

                        return mergedCompany;
                    },
                    true);

            Assert.That(updatedCompany, Is.Not.Null);
            Company selectedCompany = RunInsideTransaction(() => Repository.GetById(updatedCompany.Id), false);
            Assert.That(selectedCompany, Is.Not.Null);
            Assert.That(selectedCompany.Identifications.Count, Is.EqualTo(1));
        }

        [Test]
        public void Check_BiDirectionality_Remove_Childs_From_Parent()
        {
            Company company = CreateCompany(CompanyCreationType.WITH_2_CHILDREN);

            Company updatedCompany =
                RunInsideTransaction(
                    () =>
                    {
                        Company mergedCompany = Repository.Merge(company);

                        foreach (CompanyIdentification identification in mergedCompany.Identifications.ToList())
                        {
                            mergedCompany.RemoveIdentification(identification);
                        }

                        return mergedCompany;
                    },
                    true);

            Assert.That(updatedCompany, Is.Not.Null);
            Company selectedCompany = RunInsideTransaction(() => Repository.GetById(updatedCompany.Id), false);
            Assert.That(selectedCompany, Is.Not.Null);
            Assert.That(selectedCompany.Identifications.Any(), Is.False);
        }
    }
}
