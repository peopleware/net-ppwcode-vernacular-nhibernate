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

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace PPWCode.Vernacular.NHibernate.I.Tests.Models
{
    [DataContract(IsReference = true), Serializable]
    public class AllRoundIctCompany : IctCompany
    {
        [DataMember]
        private string m_AllRound;

        public AllRoundIctCompany(int id, int persistenceVersion)
            : base(id, persistenceVersion)
        {
        }

        public AllRoundIctCompany(int id)
            : base(id)
        {
        }

        public AllRoundIctCompany()
        {
        }

        [StringLength(-1)]
        public virtual string AllRound
        {
            get { return m_AllRound; }
            set
            {
                Contract.Ensures(AllRound == value);

                m_AllRound = value;
            }
        }
    }
}
