﻿// Copyright 2017 by PeopleWare n.v..
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using NHibernate.Mapping.ByCode;

using PPWCode.Vernacular.NHibernate.I.MappingByCode;

namespace PPWCode.Vernacular.NHibernate.I.Tests.Models.Mapping
{
    public class FailedCompanyMapper : InsertAuditablePersistentObjectMapper<FailedCompany, int>
    {
        public FailedCompanyMapper()
        {
            Id(fc => fc.Id, m => m.Generator(Generators.Foreign<FailedCompany>(fc => fc.Company)));
            Property(fc => fc.FailingDate);
            OneToOne(fc => fc.Company, m => m.Constrained(true));
        }
    }
}