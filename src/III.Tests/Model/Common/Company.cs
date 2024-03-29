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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using JetBrains.Annotations;

using NHibernate.Mapping.ByCode;

using PPWCode.Vernacular.NHibernate.III.MappingByCode;
using PPWCode.Vernacular.Persistence.IV;

namespace PPWCode.Vernacular.NHibernate.III.Tests.Model.Common
{
    [Serializable]
    [DataContract(IsReference = true)]
    [AuditLog(AuditLogAction = AuditLogActionEnum.ALL)]
    public class Company : AuditableVersionedPersistentObject<int, int>
    {
        [DataMember]
        private ExtendedCompany _extendedCompany;

        [DataMember]
        private FailedCompany _failedCompany;

        [DataMember]
        private ISet<CompanyIdentification> _identifications = new HashSet<CompanyIdentification>();

        [DataMember]
        private ISet<CompanyIdentification> _parentIdentifications = new HashSet<CompanyIdentification>();

        public Company(int id, int persistenceVersion)
            : base(id, persistenceVersion)
        {
        }

        public Company(int id)
            : base(id)
        {
        }

        public Company()
        {
        }

        [DataMember]
        [Required]
        [StringLength(128)]
        public virtual string Name { get; set; }

        [AuditLogPropertyIgnore]
        public virtual FailedCompany FailedCompany
        {
            get => _failedCompany;
            set
            {
                if (_failedCompany != value)
                {
                    if (_failedCompany != null)
                    {
                        FailedCompany previousFailedCompany = _failedCompany;
                        _failedCompany = null;
                        previousFailedCompany.Company = null;
                    }

                    _failedCompany = value;
                    if (_failedCompany != null)
                    {
                        _failedCompany.Company = this;
                    }
                }
            }
        }

        public virtual bool IsFailed
            => FailedCompany != null;

        [AuditLogPropertyIgnore]
        public virtual ExtendedCompany ExtendedCompany
        {
            get => _extendedCompany;
            set
            {
                if (_extendedCompany != value)
                {
                    if (_extendedCompany != null)
                    {
                        ExtendedCompany previousExtendedCompany = _extendedCompany;
                        _extendedCompany = null;
                        previousExtendedCompany.Company = null;
                    }

                    _extendedCompany = value;
                    if (_extendedCompany != null)
                    {
                        _extendedCompany.Company = this;
                    }
                }
            }
        }

        public virtual bool IsExtended
            => ExtendedCompany != null;

        [AuditLogPropertyIgnore]
        [OtherSidePropertyName(nameof(CompanyIdentification.Company))]
        public virtual ISet<CompanyIdentification> Identifications
            => _identifications;

        [AuditLogPropertyIgnore]
        [OtherSidePropertyName(nameof(CompanyIdentification.ParentCompany))]
        public virtual ISet<CompanyIdentification> ParentIdentifications
            => _parentIdentifications;

        public virtual void RemoveIdentification(CompanyIdentification companyIdentification)
        {
            if ((companyIdentification != null) && Identifications.Remove(companyIdentification))
            {
                companyIdentification.Company = null;
            }
        }

        public virtual void AddIdentification(CompanyIdentification companyIdentification)
        {
            if ((companyIdentification != null) && Identifications.Add(companyIdentification))
            {
                companyIdentification.Company = this;
            }
        }

        public virtual void RemoveParentIdentification(CompanyIdentification companyIdentification)
        {
            if ((companyIdentification != null) && ParentIdentifications.Remove(companyIdentification))
            {
                companyIdentification.ParentCompany = null;
            }
        }

        public virtual void AddParentIdentification(CompanyIdentification companyIdentification)
        {
            if ((companyIdentification != null) && ParentIdentifications.Add(companyIdentification))
            {
                companyIdentification.ParentCompany = this;
            }
        }
    }

    [UsedImplicitly]
    public class CompanyMapper : AuditableVersionedPersistentObjectMapper<Company, int, int>
    {
        public CompanyMapper()
        {
            Property(c => c.Name);

            Set(
                c => c.Identifications,
                c => c.Cascade(Cascade.All.Include(Cascade.DeleteOrphans)),
                r => r.OneToMany());

            Set(
                c => c.ParentIdentifications,
                c => c.Cascade(Cascade.All.Include(Cascade.DeleteOrphans)),
                r => r.OneToMany());

            OneToOne(
                c => c.FailedCompany,
                m =>
                {
                    m.Lazy(LazyRelation.NoLazy);
                    m.ForeignKey(null);
                    m.Cascade(Cascade.All.Include(Cascade.DeleteOrphans));
                });

            ManyToOne(
                c => c.ExtendedCompany,
                m =>
                {
                    m.Index(null);
                    m.Cascade(Cascade.All.Include(Cascade.DeleteOrphans));
                });
        }
    }

    public class UniqueConstraintsForExtendedCompany : UniqueConstraintsForNullableColumn<Company>
    {
        public UniqueConstraintsForExtendedCompany(IPpwHbmMapping ppwHbmMapping)
            : base(ppwHbmMapping)
        {
            ColumnName = c => c.ExtendedCompany;
        }
    }
}
