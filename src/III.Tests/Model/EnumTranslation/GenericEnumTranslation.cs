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
using System.Runtime.Serialization;

using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;

using PPWCode.Vernacular.Exceptions.IV;

namespace PPWCode.Vernacular.NHibernate.III.Tests.Model.EnumTranslation
{
    [Serializable]
    [DataContract(IsReference = true)]
    public abstract class GenericEnumTranslation<T> : EnumTranslation
        where T : struct, IComparable, IConvertible, IFormattable
    {
        protected GenericEnumTranslation()
        {
            if (!typeof(T).IsEnum)
            {
                throw new ProgrammingError("Invalid generic type.");
            }
        }

        protected GenericEnumTranslation(int id)
            : base(id)
        {
        }

        [DataMember]
        public virtual T? Code { get; set; }
    }

    public abstract class GenericEnumTranslationMapper<T> : SubclassMapping<GenericEnumTranslation<T>>
        where T : struct, IComparable, IConvertible, IFormattable
    {
        protected GenericEnumTranslationMapper()
        {
            Property(
                e => e.Code,
                m => { m.Type<EnumStringType<T>>(); });
        }
    }
}
