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

using NHibernate.Mapping.ByCode;

using PPWCode.Vernacular.Persistence.V;

namespace PPWCode.Vernacular.NHibernate.IV.Tests.Model.BiDirectionalNoCascading
{
    public class Book : PersistentObject
    {
        private readonly ISet<Keyword> _keywords = new HashSet<Keyword>();
        private Author? _author;
        public virtual string? Name { get; set; }

        public virtual Author? Author
        {
            get => _author;
            set
            {
                if (_author != value)
                {
                    if (_author != null)
                    {
                        Author previousAuthor = _author;
                        _author = null;
                        previousAuthor.RemoveBook(this);
                    }

                    _author = value;
                    _author?.AddBook(this);
                }
            }
        }

        [AuditLogPropertyIgnore]
        public virtual ISet<Keyword> Keywords
            => _keywords;

        public virtual void AddKeyword(Keyword? keyword)
        {
            if ((keyword != null) && Keywords.Add(keyword))
            {
                keyword.AddBook(this);
            }
        }

        public virtual void RemoveKeyword(Keyword? keyword)
        {
            if ((keyword != null) && Keywords.Remove(keyword))
            {
                keyword.RemoveBook(this);
            }
        }

        public class BookMapper : PersistentObjectMapper<Book>
        {
            public BookMapper()
            {
                Property(b => b.Name);

                ManyToOne(b => b.Author);

                Set(
                    b => b.Keywords,
                    m =>
                    {
                        m.Inverse(true);
                        m.Cascade(Cascade.None);
                    },
                    r => r.ManyToMany());
            }
        }
    }
}
