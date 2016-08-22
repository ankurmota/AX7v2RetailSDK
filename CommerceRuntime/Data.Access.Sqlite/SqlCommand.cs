/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

namespace Contoso
{
    namespace Commerce.Runtime.DataAccess.Sqlite
    {
        using System;
        using System.Collections.Generic;
        using System.Text.RegularExpressions;
        using Commerce.Runtime.Data.Sqlite;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Retail.Diagnostics;
        using SQLitePCL;

        /// <summary>
        /// Represents a SQLite operation that can be executed.
        /// </summary>
        internal sealed class SqlCommand : IDisposable
        {
            /// <summary>
            /// The schema format used in SQLite.
            /// </summary>
            private const string SchemaFormat = "[{0}_{1}]";

            /// <summary>
            /// Expected schemas separated by |.
            /// </summary>
            private const string PossibleSchemas = "ax|crt";

            /// <summary>
            /// The schema conversion regular expression string.
            /// </summary>
            private const string SchemaConversionRegexString = @"[\[]?(?<schema>" + PossibleSchemas + @")\]?\.[\[]?(?<table>\w*)[\]]?";

            /// <summary>
            /// Regular expression used to convert the schema partition notation '[schemaName].[someObject]' into the SQLite expected format: 'schemaName_someObject'.
            /// </summary>
            private static readonly Regex SchemaConvertionRegex = new Regex(
                SchemaConversionRegexString,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            /// <summary>
            /// Initializes a new instance of the <see cref="SqlCommand"/> class.
            /// </summary>
            /// <param name="commandString">The database command.</param>
            /// <param name="connection">The connection.</param>
            /// <param name="parameters">The parameter collection.</param>
            public SqlCommand(string commandString, DatabaseConnection connection, IDictionary<string, object> parameters)
            {
                this.CommandString = commandString;
                this.Connection = connection;

                // copy parameters to avoid changing the original collection
                this.Parameters = new Dictionary<string, object>(parameters);
            }

            /// <summary>
            /// Gets or sets the command string.
            /// </summary>
            private string CommandString
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the connection to the database.
            /// </summary>
            private DatabaseConnection Connection
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the parameter dictionary.
            /// </summary>
            private IDictionary<string, object> Parameters
            {
                get;
                set;
            }

            /// <summary>
            /// Executes the command statement and returns the database result.
            /// </summary>
            /// <returns>The command statement execution result.</returns>
            public DatabaseResult Execute()
            {
                DatabaseResult result = null;

                var temporaryTables = this.PrepareStatementDependencies();

                SqlStatement statement;

                try
                {
                    statement = this.PrepareStatement();
                    this.BindParameters(statement.SqliteStatement);
                }
                catch (Exception)
                {
                    string tableName = string.Empty;

                    try
                    {
                        // if something goes wrong and we have temporary tables, we need to dispose them
                        foreach (TempTable table in temporaryTables)
                        {
                            tableName = table.TableName;
                            table.Dispose();
                        }
                    }
                    catch (Exception exception)
                    {
                        // avoid throwing another exception inside catch block
                        RetailLogger.Log.CrtDataAccessSqliteTempTableDisposeFailure(tableName, exception);
                    }

                    throw;
                }

                result = new DatabaseResult(statement, temporaryTables);

                return result;
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                this.Connection = null;

                if (this.Parameters != null)
                {
                    this.Parameters.Clear();
                    this.Parameters = null;
                }
            }

            /// <summary>
            /// Converts a command string into a SQLite SQL command string.
            /// </summary>
            /// <param name="commandString">The command string to be converted.</param>
            /// <returns>The SQLite command string result.</returns>
            private static string ConvertToSqliteCommandString(string commandString)
            {
                return SchemaConvertionRegex.Replace(commandString, SqlCommand.SchemaConvertionAction);
            }

            /// <summary>
            /// The action to be executed when replacing the schema token in the command string.
            /// </summary>
            /// <param name="match">The regular expression match result.</param>
            /// <returns>The schema replaced string.</returns>
            private static string SchemaConvertionAction(Match match)
            {
                return string.Format(SchemaFormat, match.Groups[1].Value, match.Groups[2].Value);
            }

            /// <summary>
            /// Bind parameters to the statement.
            /// </summary>
            /// <param name="statement">The statement to bind the parameters to.</param>
            private void BindParameters(ISQLiteStatement statement)
            {
                string currentParameterKey = string.Empty;
                statement.ClearBindings();

                try
                {
                    foreach (var parameter in this.Parameters)
                    {
                        currentParameterKey = parameter.Key;
                        object sqlParameterValue = SqlTypeHelper.ConvertManagedValueToSqlValue(parameter.Value);
                        statement.Bind(currentParameterKey, sqlParameterValue);
                    }
                }
                catch (SQLiteException ex)
                {
                    string message = string.Format("An error occurred when binding parameter '{0}'. Check if parameter is present in the query text. See inner exception for details.", currentParameterKey);
                    throw new SQLiteException(message, ex);
                }
            }

            /// <summary>
            /// Process input parameters and get any dependencies ready.
            /// </summary>
            /// <returns>Returns a list of <see cref="IDisposable"/> elements that are needed to be held until statement execution is finished.</returns>
            private List<IDisposable> PrepareStatementDependencies()
            {
                // queries that provide DataTable as parameters are relying on the TVP-usage
                // we need to convert data tables into temp tables
                List<string> keysToRemove = new List<string>();

                // need to keep trace of temp tables until statement is over
                List<IDisposable> tempTables = new List<IDisposable>();

                foreach (var parameter in this.Parameters)
                {
                    TableType tableType = parameter.Value as TableType;

                    if (tableType != null)
                    {
                        TempTable tempTable = TempTable.CreateTemporaryTable(tableType.DataTable, this.Connection);
                        tempTables.Add(tempTable);

                        // can't remove key during enumeration, store them to remove later
                        keysToRemove.Add(parameter.Key);

                        // update command string to use actual temp table name, instead of parameter value
                        this.CommandString = this.CommandString.Replace(parameter.Key, tempTable.TableName);
                    }
                }

                foreach (string key in keysToRemove)
                {
                    this.Parameters.Remove(key);
                }

                return tempTables;
            }

            /// <summary>
            /// Prepares a SQLite statement that is ready to be bound and executed for this command.
            /// </summary>
            /// <returns>The SQLite statement.</returns>
            private SqlStatement PrepareStatement()
            {
                string commandString = this.CommandString.TrimEnd(' ', '\r', '\n');

                // check for multiple statements in same command
                int index = commandString.IndexOf(';');
                if (index != -1 &&
                    index != commandString.Length - 1)
                {
                    throw new DatabaseException(
                        DatabaseErrorCode.OperationNotValid,
                        "Only one statement execution is supported at a time. Separate statements into different commands.");
                }

                commandString = SqlCommand.ConvertToSqliteCommandString(this.CommandString);
                return this.Connection.PrepareStatement(commandString);
            }
        }
    }
}
