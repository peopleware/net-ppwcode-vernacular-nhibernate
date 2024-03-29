﻿// Copyright 2020 by PeopleWare n.v..
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

using PPWCode.Vernacular.NHibernate.III.Test;

namespace PPWCode.Vernacular.NHibernate.III.Tests.IntegrationTests.Sync.QueryOver.Common
{
    public class DirtyCheckingExampleTests : BaseCompanyTests
    {
        [Test]
        [Explicit]
        public void DirtyCheckingTest()
        {
            /* Set Number to null (but Number is defined as int) */
            /*
            using (ISession session = NhConfigurator.SessionFactory.OpenSession())
            {
                using (ITransaction trans = session.BeginTransaction())
                {
                    session.CreateQuery(@"update CompanyIdentification set Number = null").ExecuteUpdate();
                    trans.Commit();
                }
            }
            */
            new DirtyChecking(Configuration, SessionFactory, Assert.Fail, Assert.Inconclusive).Test();
        }
    }
}
