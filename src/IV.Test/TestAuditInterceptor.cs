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

using PPWCode.Util.Authorization.I;
using PPWCode.Util.Time.I;

namespace PPWCode.Vernacular.NHibernate.IV.Test;

public class TestAuditInterceptor<TId>(ITimeProvider<DateTime> timeProvider, bool useUtc, IIdentityProvider identityProvider)
    : AuditInterceptor<TId, DateTime>
    where TId : IEquatable<TId>
{
    /// <inheritdoc />
    protected override DateTime Now
        => useUtc ? timeProvider.UtcNow : timeProvider.Now;

    /// <inheritdoc />
    protected override string IdentityName
        => identityProvider.IdentityName;
}
