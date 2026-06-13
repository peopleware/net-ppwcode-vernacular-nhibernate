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

using System;
using System.Collections.Concurrent;

using NHibernate;
using NHibernate.Type;

using PPWCode.Vernacular.Persistence.V;

namespace PPWCode.Vernacular.NHibernate.IV
{
    /// <summary>
    ///     Represents an abstract base class that serves as an NHibernate interceptor for audit-related functionality.
    ///     This class provides mechanisms to manage auditing of entity changes during insert and update operations.
    /// </summary>
    /// <typeparam name="TId">
    ///     The type of the entity's identifier. Must implement <see cref="System.IEquatable{T}" />.
    /// </typeparam>
    /// <typeparam name="TTimestamp">
    ///     The type representing the timestamp for auditing. Must be a struct that implements
    ///     <see cref="System.IComparable{T}" /> and <see cref="System.IEquatable{T}" />.
    /// </typeparam>
    public abstract class AuditInterceptor<TId, TTimestamp> : EmptyInterceptor
        where TId : IEquatable<TId>
        where TTimestamp : struct, IComparable<TTimestamp>, IEquatable<TTimestamp>
    {
        private const string CreatedAtPropertyName = "CreatedAt";
        private const string CreatedByPropertyName = "CreatedBy";
        private const string LastModifiedAtPropertyName = "LastModifiedAt";
        private const string LastModifiedByPropertyName = "LastModifiedBy";

        protected ConcurrentDictionary<Property, int> IndexCache { get; } = new();

        /// <summary>
        ///     Provides the current timestamp used for auditing purposes.
        ///     This property is utilized to capture the precise moment an action
        ///     is performed, enabling accurate tracking of entity modifications.
        /// </summary>
        protected abstract TTimestamp Now { get; }

        /// <summary>
        ///     Represents the identity name used for auditing purposes.
        ///     This value is typically utilized to capture the user or system
        ///     responsible for performing a specific action on an entity.
        /// </summary>
        protected abstract string IdentityName { get; }

        /// <summary>
        ///     Determines whether the specified entity can be audited.
        /// </summary>
        /// <param name="entity">The entity to evaluate for auditing eligibility.</param>
        /// <param name="id">The identifier of the entity.</param>
        /// <returns>
        ///     A boolean value indicating whether the specified entity can be audited.
        /// </returns>
        protected virtual bool CanAudit(object entity, object id)
            => true;

        /// <summary>
        ///     Sets the specified value for a property on the state array for an entity type.
        /// </summary>
        /// <param name="entityType">
        ///     The type of the entity for which the property value is being set.
        /// </param>
        /// <param name="propertyNames">
        ///     The array of property names corresponding to the entity.
        /// </param>
        /// <param name="state">
        ///     The array representing the state of the entity's properties.
        /// </param>
        /// <param name="propertyName">
        ///     The name of the property to be updated.
        /// </param>
        /// <param name="value">
        ///     The value to set for the specified property.
        /// </param>
        protected virtual void Set(
            Type entityType,
            string[] propertyNames,
            object[] state,
            string propertyName,
            object value)
        {
            int index = IndexCache.GetOrAdd(new Property(entityType, propertyName), k => Array.IndexOf(propertyNames, propertyName));
            if (index >= 0)
            {
                state[index] = value;
            }
        }

        /// <summary>
        ///     Sets audit information for the specified entity during save or update operations.
        /// </summary>
        /// <param name="entity">
        ///     The entity to which the audit information will be applied.
        /// </param>
        /// <param name="currentState">
        ///     The current state of the entity's properties.
        /// </param>
        /// <param name="propertyNames">
        ///     The names of the properties of the entity being audited.
        /// </param>
        /// <param name="onSave">
        ///     A boolean value indicating if the method is invoked during a save operation.
        /// </param>
        /// <returns>
        ///     A boolean value indicating whether audit information was successfully applied to the entity.
        /// </returns>
        protected virtual bool SetAuditInfo(
            object? entity,
            object[] currentState,
            string[] propertyNames,
            bool onSave)
        {
            if (entity is not IPersistentObject<TId> persistentObject)
            {
                return false;
            }

            IInsertAuditable<TTimestamp>? insertAuditable = entity as IInsertAuditable<TTimestamp>;
            IUpdateAuditable<TTimestamp>? updateAuditable = entity as IUpdateAuditable<TTimestamp>;
            if ((insertAuditable == null) && (updateAuditable == null))
            {
                return false;
            }

            TTimestamp time = Now;
            string identityName = IdentityName;
            Type entityType = entity.GetType();

            if ((insertAuditable != null) && (onSave || persistentObject.IdIsTransient))
            {
                string createdAtPropertyName = CreatedAtPropertyName;
                string createdByPropertyName = CreatedByPropertyName;

                Set(entityType, propertyNames, currentState, createdAtPropertyName, time);
                Set(entityType, propertyNames, currentState, createdByPropertyName, identityName);

                insertAuditable.CreatedAt = time;
                insertAuditable.CreatedBy = identityName;
            }

            if (updateAuditable != null)
            {
                string lastModifiedAtPropertyName = LastModifiedAtPropertyName;
                string lastModifiedByPropertyName = LastModifiedByPropertyName;

                Set(entityType, propertyNames, currentState, lastModifiedAtPropertyName, time);
                Set(entityType, propertyNames, currentState, lastModifiedByPropertyName, identityName);

                updateAuditable.LastModifiedAt = time;
                updateAuditable.LastModifiedBy = identityName;
            }

            return true;
        }

        /// <inheritdoc />
        public override bool OnFlushDirty(
            object entity,
            object id,
            object[] currentState,
            object[] previousState,
            string[] propertyNames,
            IType[] types)
            => CanAudit(entity, id) && SetAuditInfo(entity, currentState, propertyNames, false);

        /// <inheritdoc />
        public override bool OnSave(
            object entity,
            object id,
            object[] state,
            string[] propertyNames,
            IType[] types)
            => CanAudit(entity, id) && SetAuditInfo(entity, state, propertyNames, true);

        protected readonly struct Property(Type entityType, string propertyName) : IEquatable<Property>
        {
            private readonly Type _entityType = entityType;
            private readonly string _propertyName = propertyName;

            public static bool operator ==(Property left, Property right)
                => left.Equals(right);

            public static bool operator !=(Property left, Property right)
                => !left.Equals(right);

            /// <summary>
            ///     Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <returns>
            ///     A boolean indicating whether the current object is equal to the <paramref name="other" /> parameter.
            /// </returns>
            /// <param name="other">An object to compare with this object.</param>
            public bool Equals(Property other)
                => ReferenceEquals(_entityType, other._entityType) && string.Equals(_propertyName, other._propertyName);

            /// <summary>
            ///     Indicates whether this instance and a specified object are equal.
            /// </summary>
            /// <returns>
            ///     A boolean indicating whether <paramref name="obj" /> and this instance are the same type and represent the same
            ///     value.
            /// </returns>
            /// <param name="obj">Another object to compare to. </param>
            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                return obj is Property property && Equals(property);
            }

            /// <summary>
            ///     Returns the hash code for this instance.
            /// </summary>
            /// <returns>
            ///     A 32-bit signed integer that is the hash code for this instance.
            /// </returns>
            public override int GetHashCode()
            {
                unchecked
                {
                    return (_entityType.GetHashCode() * 397) ^ _propertyName.GetHashCode();
                }
            }
        }
    }
}
