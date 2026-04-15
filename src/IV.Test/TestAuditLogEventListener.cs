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

using NHibernate.Event;

using PPWCode.Util.Authorization.I;
using PPWCode.Util.Time.I;
using PPWCode.Vernacular.Persistence.V;

namespace PPWCode.Vernacular.NHibernate.IV.Test;

public class TestAuditLogEventListener<TId, TAuditEntity> : AuditLogEventListener<TId, DateTime, TAuditEntity, AuditLogEventContext>
    where TId : IEquatable<TId>
    where TAuditEntity : AuditLog<TId, DateTime>, new()
{
    private readonly IIdentityProvider _identityProvider;
    private readonly ITimeProvider<DateTime> _timeProvider;
    private readonly bool _useUtc;

    public TestAuditLogEventListener(
        ITimeProvider<DateTime> timeProvider,
        bool useUtc,
        IIdentityProvider identityProvider)
    {
        _identityProvider = identityProvider;
        _timeProvider = timeProvider;
        _useUtc = useUtc;
    }

    /// <inheritdoc />
    protected override DateTime Now
        => _useUtc ? _timeProvider.UtcNow : _timeProvider.Now;

    /// <inheritdoc />
    protected override string IdentityName
        => _identityProvider.IdentityName;

    /// <inheritdoc />
    protected override bool CanAuditLogFor(AbstractEvent @event, AuditLogItem auditLogItem, AuditLogActionEnum requestedLogAction)
        => true;

    /// <inheritdoc />
    protected override void OnAddAuditEntities(AuditLogEventContext context)
    {
        // NOP
    }

    /// <inheritdoc />
    protected override AuditLogEventContext CreateContext(IPostDatabaseOperationEventArgs postDatabaseOperationEventArgs)
        => new AuditLogEventContext(postDatabaseOperationEventArgs);
}
