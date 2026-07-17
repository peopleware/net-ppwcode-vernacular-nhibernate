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
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using Microsoft.Extensions.Logging;

using NHibernate;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;

namespace PPWCode.Vernacular.NHibernate.IV;

/// <summary>
///     Base implementation for database creation and schema initialization.
/// </summary>
/// <remarks>
///     This class coordinates optional database creation or recreation, schema export, and diagnostic logging.
///     Derived types can override the protected hooks to customize how the schema is created.
/// </remarks>
public abstract class DatabaseManager : IDatabaseManager
{
    private readonly ILogger _logger;
    private readonly INhConfiguration _nhConfiguration;
    private readonly INHibernateSessionFactory _nHibernateSessionFactory;
    private readonly IPpwHbmMapping _ppwHbmMapping;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DatabaseManager" /> class.
    /// </summary>
    /// <param name="logger">The logger used to write diagnostic information.</param>
    /// <param name="nHibernateSessionFactory">Provides access to NHibernate sessions.</param>
    /// <param name="nhConfiguration">Provides the NHibernate configuration used for schema creation.</param>
    /// <param name="ppwHbmMapping">Provides the mapping metadata that can be logged during schema creation.</param>
    protected DatabaseManager(
        ILogger logger,
        INHibernateSessionFactory nHibernateSessionFactory,
        INhConfiguration nhConfiguration,
        IPpwHbmMapping ppwHbmMapping)
    {
        _logger = logger;
        _nHibernateSessionFactory = nHibernateSessionFactory;
        _nhConfiguration = nhConfiguration;
        _ppwHbmMapping = ppwHbmMapping;
    }

    /// <inheritdoc />
    public virtual void ExecuteScript(string script)
    {
        using ISession session = _nHibernateSessionFactory.SessionFactory.OpenSession();
        using ITransaction transaction = session.BeginTransaction(IsolationLevel.Serializable);
        IEnumerable<string> commands = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        foreach (string command in commands.Where(c => !string.IsNullOrWhiteSpace(c)))
        {
            session
                .CreateSQLQuery(command)
                .ExecuteUpdate();
        }

        transaction.Commit();
    }

    /// <inheritdoc />
    public virtual void Create(
        bool canCreateDatabase,
        bool canAskAcknowledge,
        bool useNHibernateSchemaExport)
    {
        if (canCreateDatabase)
        {
            if (canAskAcknowledge)
            {
                int timeoutSeconds = 10;
                Console.WriteLine($" --- Answer the next questions in {timeoutSeconds} seconds ---");
                Console.WriteLine(" --- Otherwise startup will continue assuming 'N'          ---");
                Console.Write("Recreate database (includes the core data)? (Y/N) : ");
                try
                {
                    string? recreateDatabaseAnswer = TimeOutConsoleReader.ReadLine(timeoutSeconds * 1000);
                    if ((recreateDatabaseAnswer != null) && recreateDatabaseAnswer.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine();
                        Console.WriteLine();

                        CreateInternal(useNHibernateSchemaExport);
                    }
                }
                catch (TimeoutException)
                {
                    Console.WriteLine();
                    Console.WriteLine("Waited too long. The database was not recreated.");
                }
            }
            else
            {
                CreateInternal(useNHibernateSchemaExport);
            }
        }
    }

    /// <summary>
    ///     Creates the schema by using the configured creation strategy.
    /// </summary>
    /// <param name="useNHibernateSchemaExport">
    ///     Indicates whether schema creation should use NHibernate schema export instead of the FluentMigrator path.
    /// </param>
    protected virtual void CreateInternal(bool useNHibernateSchemaExport)
    {
        if (useNHibernateSchemaExport)
        {
            _logger.LogInformation("Creating schema using NHibernate schema export.");
            CreateUsingHbm2Ddl();
        }
        else
        {
            _logger.LogInformation("Creating schema using external Migrator.");
            CreateUsingExternalMigrator();
        }
    }

    /// <summary>
    ///     Creates the schema by executing the optional drop script and then running NHibernate schema export.
    /// </summary>
    protected virtual void CreateUsingHbm2Ddl()
    {
        string? dropAllScript = GetDropAllScript();
        if (dropAllScript != null)
        {
            ExecuteScript(dropAllScript);
        }
        else
        {
            _logger.LogWarning("DropAll.sql not found.");
        }

        ShowMappings(_ppwHbmMapping.HbmMapping);
        SchemaExport schemaExport = new(_nhConfiguration.GetConfiguration());
        schemaExport.Create(false, true);
    }

    /// <summary>
    ///     Creates the schema by using the FluentMigrator-based path.
    /// </summary>
    /// <remarks>
    ///     The base implementation is empty. Derived classes can override this method to provide migrations.
    /// </remarks>
    protected abstract void CreateUsingExternalMigrator();

    /// <summary>
    ///     Gets the SQL script that drops all database objects before schema recreation.
    /// </summary>
    /// <returns>
    ///     The drop script; otherwise, <see langword="null" />.
    /// </returns>
    protected abstract string? GetDropAllScript();

    /// <summary>
    ///     Logs the generated NHibernate mapping as formatted XML.
    /// </summary>
    /// <param name="hbmMapping">The mapping to log.</param>
    protected virtual void ShowMappings(HbmMapping hbmMapping)
    {
        string xmlMapping =
            new StringBuilder()
                .AppendLine()
                .AppendLine(hbmMapping.AsString())
                .ToString();
        _logger.LogInformation(xmlMapping);
    }

    private static class TimeOutConsoleReader
    {
        private static readonly AutoResetEvent _getInput;
        private static readonly AutoResetEvent _gotInput;
        private static string? _input;

        static TimeOutConsoleReader()
        {
            _getInput = new AutoResetEvent(false);
            _gotInput = new AutoResetEvent(false);
            Thread inputThread = new(DoReadLine) { IsBackground = true };
            inputThread.Start();
        }

        [SuppressMessage("ReSharper", "FunctionNeverReturns", Justification = "Should wait for user input")]
        private static void DoReadLine()
        {
            while (true)
            {
                _getInput.WaitOne();
                _input = Console.ReadLine();
                _gotInput.Set();
            }
        }

        public static string? ReadLine(int timeOutMillisecs)
        {
            _getInput.Set();
            bool success = _gotInput.WaitOne(timeOutMillisecs);
            if (success)
            {
                return _input;
            }

            throw new TimeoutException("User did not provide input within the time limit.");
        }
    }
}
