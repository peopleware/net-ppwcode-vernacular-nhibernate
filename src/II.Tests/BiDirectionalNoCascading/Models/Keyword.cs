﻿// Copyright 2017 by PeopleWare n.v..
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
using System.Runtime.Serialization;

using NHibernate.Mapping.ByCode;

using PPWCode.Vernacular.NHibernate.II.MappingByCode;
using PPWCode.Vernacular.Persistence.III;

namespace PPWCode.Vernacular.NHibernate.II.Tests.BiDirectionalNoCascading.Models
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Keyword : PersistentObject<int>
    {
        public Keyword()
        {
        }

        public Keyword(int id)
            : base(id)
        {
        }

        [DataMember]
        public virtual string Name { get; set; }

        [DataMember]
        public virtual ISet<Book> Books { get; } = new HashSet<Book>();

        public virtual void AddBook(Book book)
        {
            if ((book != null) && Books.Add(book))
            {
                book.AddKeyword(this);
            }
        }

        public virtual void RemoveBook(Book book)
        {
            if ((book != null) && Books.Remove(book))
            {
                book.RemoveKeyword(this);
            }
        }
    }

    public class KeywordMapper : PersistentObjectMapper<Keyword, int>
    {
        public KeywordMapper()
        {
            Property(k => k.Name);

            Set(
                k => k.Books,
                m => { m.Cascade(Cascade.None); },
                r => r.ManyToMany());
        }
    }
}