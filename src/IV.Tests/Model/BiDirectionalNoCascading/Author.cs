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
    public class Author : PersistentObject
    {
        private readonly ISet<Book> _books = new HashSet<Book>();

        public virtual string? Name { get; set; }

        [AuditLogPropertyIgnore]
        public virtual ISet<Book> Books
            => _books;

        public virtual void AddBook(Book? book)
        {
            if ((book != null) && Books.Add(book))
            {
                book.Author = this;
            }
        }

        public virtual void RemoveBook(Book? book)
        {
            if ((book != null) && Books.Remove(book))
            {
                book.Author = null;
            }
        }
    }

    public class AuthorMapper : PersistentObjectMapper<Author>
    {
        public AuthorMapper()
        {
            Property(a => a.Name);

            Set(
                c => c.Books,
                m => m.Cascade(Cascade.None),
                r => r.OneToMany());
        }
    }
}
