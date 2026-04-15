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
using System.ComponentModel.DataAnnotations;

using NHibernate.Mapping.ByCode;
using NHibernate.Type;

using PPWCode.Vernacular.Persistence.V;

namespace PPWCode.Vernacular.NHibernate.IV.Tests.Model.Common
{
    public class User : AuditableVersionedPersistentObject
    {
        private readonly ISet<Role> _roles = new HashSet<Role>();

        [Required]
        [StringLength(200)]
        public virtual string? Name { get; set; }

        [Required]
        public virtual Gender? Gender { get; set; }

        public virtual bool? HasBlueEyes { get; set; }

        [AuditLogPropertyIgnore]
        public virtual ISet<Role> Roles
            => _roles;

        public virtual void AddRole(Role? role)
        {
            if ((role != null) && Roles.Add(role))
            {
                role.AddUser(this);
            }
        }

        public virtual void RemoveRole(Role? role)
        {
            if ((role != null) && Roles.Remove(role))
            {
                role.RemoveUser(this);
            }
        }

        public class UserMapper : AuditableVersionedPersistentObjectMapper<User>
        {
            public UserMapper()
            {
                // User is most of the time a reserved word
                Table("`User`");
                Property(
                    u => u.Name,
                    m => m.Unique(true));
                Property(u => u.Gender, m => m.Type<EnumStringType<Gender>>());
                Property(u => u.HasBlueEyes, m => m.Type<YesNoType>());
                Set(
                    u => u.Roles,
                    m => m.Cascade(Cascade.None),
                    c => c.ManyToMany());
            }
        }
    }
}
