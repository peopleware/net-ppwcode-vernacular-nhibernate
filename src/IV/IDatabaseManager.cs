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

namespace PPWCode.Vernacular.NHibernate.IV;

/// <summary>
///     Manages database initialization tasks for an NHibernate-backed application.
/// </summary>
public interface IDatabaseManager
{
    /// <summary>
    ///     Executes the provided database script.
    /// </summary>
    /// <param name="script">The SQL or database-specific script to execute.</param>
    void ExecuteScript(string script);

    /// <summary>
    ///     Creates the database and schema according to the supplied creation options.
    /// </summary>
    /// <param name="canCreateDatabase">
    ///     Indicates whether the implementation is allowed to create the target database when it does not yet exist.
    /// </param>
    /// <param name="canAskAcknowledge">
    ///     Indicates whether the implementation may require explicit acknowledgement before performing creation work.
    /// </param>
    /// <param name="useNHibernateSchemaExport">
    ///     Indicates whether schema creation should be performed through NHibernate schema export facilities.
    /// </param>
    void Create(
        bool canCreateDatabase,
        bool canAskAcknowledge,
        bool useNHibernateSchemaExport);
}
