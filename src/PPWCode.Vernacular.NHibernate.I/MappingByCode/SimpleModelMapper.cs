﻿// Copyright 2014 by PeopleWare n.v..
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
using System.Reflection;
using System.Text.RegularExpressions;

using NHibernate.Mapping.ByCode;

using PPWCode.Vernacular.NHibernate.I.Interfaces;

namespace PPWCode.Vernacular.NHibernate.I.MappingByCode
{
    /// <summary>
    ///     Simple ModelMapper
    /// </summary>
    public abstract class SimpleModelMapper : ModelMapperBase
    {
        protected SimpleModelMapper(IMappingAssemblies mappingAssemblies)
            : base(mappingAssemblies)
        {
            ModelMapper.BeforeMapSet += OnBeforeMappingCollectionConvention;
            ModelMapper.BeforeMapBag += OnBeforeMappingCollectionConvention;
            ModelMapper.BeforeMapList += OnBeforeMappingCollectionConvention;
            ModelMapper.BeforeMapIdBag += OnBeforeMappingCollectionConvention;
            ModelMapper.BeforeMapMap += OnBeforeMappingCollectionConvention;
        }

        protected abstract bool UseCamelCaseUnderScoreForDbObjects { get; }

        protected override string DefaultAccess
        {
            get { return "field.pascalcase-m-underscore"; }
        }

        protected override void OnBeforeMapClass(IModelInspector modelInspector, Type type, IClassAttributesMapper classCustomizer)
        {
            classCustomizer.DynamicUpdate(true);
            classCustomizer.Id(m => m.Generator(Generators.HighLow));
        }

        protected override void OnBeforeMapProperty(IModelInspector modelInspector, PropertyPath member, IPropertyMapper propertyCustomizer)
        {
            Type declaringType = member.LocalMember.DeclaringType;
            if (declaringType == null)
            {
                return;
            }

            string memberName = member.ToColumnName();
            if (UseCamelCaseUnderScoreForDbObjects)
            {
                propertyCustomizer.Column(CamelCaseToUnderscore(memberName));
            }

            // Getting type of reflected object
            Type propertyType;
            switch (member.LocalMember.MemberType)
            {
                case MemberTypes.Field:
                    propertyType = ((FieldInfo)member.LocalMember).FieldType;
                    break;
                case MemberTypes.Property:
                    propertyType = ((PropertyInfo)member.LocalMember).PropertyType;
                    break;
                default:
                    propertyType = null;
                    break;
            }

            if ((propertyType != null && propertyType.IsPrimitive)
                || propertyType == typeof(DateTime))
            {
                propertyCustomizer.NotNullable(true);
            }
        }

        protected override void OnBeforeMapJoinedSubclass(IModelInspector modelInspector, Type type, IJoinedSubclassAttributesMapper joinedSubclassCustomizer)
        {
            // ReSharper disable once PossibleNullReferenceException
            joinedSubclassCustomizer.Key(k => k.Column(string.Format("{0}Id", type.BaseType.Name)));
        }

        protected override void OnBeforeMapManyToMany(IModelInspector modelInspector, PropertyPath member, IManyToManyMapper collectionRelationManyToManyCustomizer)
        {
            collectionRelationManyToManyCustomizer.Column(string.Format("{0}Id", member.CollectionElementType().Name));
        }

        protected override void OnBeforeMapManyToOne(IModelInspector modelInspector, PropertyPath member, IManyToOneMapper propertyCustomizer)
        {
            propertyCustomizer.Column(string.Format("{0}Id", member.LocalMember.Name));
            propertyCustomizer.ForeignKey(string.Format("FK_{0}_{1}", member.Owner().Name, member.LocalMember.Name));
        }

        protected override void OnBeforeMapOneToOne(IModelInspector modelInspector, PropertyPath member, IOneToOneMapper propertyCustomizer)
        {
            propertyCustomizer.ForeignKey(string.Format("FK_{0}_{1}", member.Owner().Name, member.LocalMember.Name));
        }

        protected virtual void OnBeforeMappingCollectionConvention(IModelInspector modelinspector, PropertyPath member, ICollectionPropertiesMapper collectionPropertiesCustomizer)
        {
            if (modelinspector.IsManyToMany(member.LocalMember))
            {
                collectionPropertiesCustomizer.Table(member.ManyToManyIntermediateTableName("To"));
            }

            collectionPropertiesCustomizer.Key(k => k.Column(DetermineKeyColumnName(modelinspector, member)));
        }

        private static string CamelCaseToUnderscore(string camelCase)
        {
            const string Rgx = @"([A-Z]+)([A-Z][a-z])";
            const string Rgx2 = @"([a-z\d])([A-Z])";

            string result = Regex.Replace(camelCase, Rgx, "$1_$2");
            result = Regex.Replace(result, Rgx2, "$1_$2");
            return result.ToUpper();
        }

        private static string DetermineKeyColumnName(IModelInspector inspector, PropertyPath member)
        {
            MemberInfo otherSideProperty = member.OneToManyOtherSideProperty();
            if (inspector.IsOneToMany(member.LocalMember) && otherSideProperty != null)
            {
                return string.Format("{0}Id", otherSideProperty.Name);
            }

            return string.Format("{0}Id", member.Owner().Name);
        }
    }
}