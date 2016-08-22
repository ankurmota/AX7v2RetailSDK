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
        using Commerce.Runtime.Data.Sqlite;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using SQLitePCL;

        /// <summary>
        /// Represents the result of a database operation.
        /// </summary>
        /// <remarks>This class is not thread-safe and should not be used in a thread different from the one holding the database connection.</remarks>
        internal sealed class DatabaseResult : IDatabaseResult
        {
            /// <summary>
            /// The collection of elements that the statement depends on to be executed.
            /// </summary>
            private IEnumerable<IDisposable> dependencyContextCollection;

            /// <summary>
            /// The lookup class for retrieval of field indices by name.
            /// </summary>
            private SqliteFieldNameLookup fieldNameLookup;

            /// <summary>
            /// Initializes a new instance of the <see cref="DatabaseResult"/> class.
            /// </summary>
            /// <param name="statement">The statement.</param>
            /// <param name="dependencyContext">The collection of elements that the statement depends on to be executed.</param>
            public DatabaseResult(SqlStatement statement, IEnumerable<IDisposable> dependencyContext)
            {
                ThrowIf.Null(statement, "statement");

                this.Statement = statement;
                this.dependencyContextCollection = dependencyContext;
            }

            /// <summary>
            /// Gets the number of fields available in the current result set.
            /// </summary>
            public int FieldCount
            {
                get { return this.Statement.SqliteStatement.ColumnCount; }
            }

            /// <summary>
            /// Gets or sets the underlying database statement execute.
            /// </summary>
            private SqlStatement Statement
            {
                get;
                set;
            }

            /// <summary>
            /// Moves to the next result set.
            /// </summary>
            /// <returns>Whether new result set exists or not.</returns>
            /// <remarks>No call to this method is require to read the initial result set.</remarks>
            public bool NextResult()
            {
                // for now, we don't support multiple statements in the same command execution
                return false;
            }

            /// <summary>
            /// Reads the next result set row.
            /// </summary>
            /// <returns>Whether new result set row exists or not.</returns>
            /// <remarks>To read the first row in the result set, as well as all subsequent rows, a call to this method is necessary for each row to be read.</remarks>
            public bool Read()
            {
                SQLiteResult sqlResult = SQLiteResult.OK;

                try
                {
                    sqlResult = this.Statement.SqliteStatement.Step();
                }
                catch (SQLiteException exception)
                {
                    SqlTypeHelper.ThrowDatabaseException(exception);
                }

                switch (sqlResult)
                {
                    case SQLiteResult.ROW:
                        return true;

                    case SQLiteResult.OK:
                    case SQLiteResult.DONE:
                        return false;

                    case SQLiteResult.BUSY:
                        this.HandleDeadlock();
                        return false;

                    default:
                        string message = string.Format(
                            "Database operation could not be completed. Sqlite result: {0}.",
                            sqlResult.ToString());
                        throw new DatabaseException(DatabaseErrorCode.Unknown, message);
                }
            }

            /// <summary>
            /// Gets the value for a specific field in the current result set row.
            /// </summary>
            /// <typeparam name="T">The expected type of the field being read.</typeparam>
            /// <param name="index">The index of the field to be read.</param>
            /// <returns>The field value read.</returns>
            public T GetValue<T>(int index)
            {
                return (T)this.GetValue(index, typeof(T));
            }

            /// <summary>
            /// Gets the value for a specific field in the current result set row.
            /// </summary>
            /// <param name="index">The index of the field to be read.</param>
            /// <param name="valueType">The expected type of the field being read.</param>
            /// <returns>The field value read.</returns>
            public object GetValue(int index, System.Type valueType)
            {
                object sqlValue = this.Statement.SqliteStatement[index];
                return SqlTypeHelper.ConvertSqlValueToManagedValue(sqlValue, valueType);
            }

            /// <summary>
            /// Gets the value for a specific field in the current result set row.
            /// </summary>
            /// <typeparam name="T">The expected type of the field being read.</typeparam>
            /// <param name="fieldName">The name of the field to be read.</param>
            /// <returns>The field value read.</returns>
            public T GetValue<T>(string fieldName)
            {
                int fieldIndex = this.GetFieldIndex(fieldName);
                return this.GetValue<T>(fieldIndex);
            }

            /// <summary>
            /// Gets the value for a specific field in the current result set row.
            /// </summary>
            /// <param name="fieldName">The name of the field to be read.</param>
            /// <param name="valueType">The expected type of the field being read.</param>
            /// <returns>The field value read.</returns>
            public object GetValue(string fieldName, Type valueType)
            {
                int fieldIndex = this.GetFieldIndex(fieldName);
                return this.GetValue(fieldIndex, valueType);
            }

            /// <summary>
            /// Gets the name of the field in a specific index.
            /// </summary>
            /// <param name="index">The index of the field being queried.</param>
            /// <returns>The name of the field.</returns>
            public string GetName(int index)
            {
                return this.Statement.SqliteStatement.ColumnName(index);
            }

            /// <summary>
            /// Gets the index of the field by a specific name.
            /// </summary>
            /// <param name="fieldName">The name of the field.</param>
            /// <returns>The index of the field.</returns>
            public int GetFieldIndex(string fieldName)
            {
                ThrowIf.Null(fieldName, "fieldName");

                if (this.fieldNameLookup == null)
                {
                    this.fieldNameLookup = new SqliteFieldNameLookup(this.Statement.SqliteStatement);
                }

                try
                {
                    // New version of SQLite will support this out of box (see SqliteFieldNameLookup for details)
                    return this.fieldNameLookup.GetOrdinal(fieldName);
                }
                catch (SQLiteException ex)
                {
                    throw new DatabaseException(string.Format("The field '{0}' not found in result set.", fieldName), ex);
                }
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                // statements are cached and disposed by the owner connection
                // reset statement so any kept result is discarted
                try
                {
                    this.Statement.InUse = false;
                    this.Statement.SqliteStatement.Reset();

                    // dispose all dependencies that have been held during statement execution
                    if (this.dependencyContextCollection != null)
                    {
                        foreach (IDisposable dependency in this.dependencyContextCollection)
                        {
                            dependency.Dispose();
                        }

                        this.dependencyContextCollection = null;
                    }
                }
                catch (SQLiteException exception)
                {
                    SqlTypeHelper.ThrowDatabaseException(exception);
                }
            }

            /// <summary>
            /// Handles the situation in which the database is locked and the statement cannot be executed because of race conditions.
            /// </summary>
            private void HandleDeadlock()
            {
                // if there is an open transaction on the connection
                if (this.Statement.Connection.IsUnderTransaction)
                {
                    // roll it back - rolling back a nested transaction forces all previous transactions to be reverted
                    using (IDatabaseTransaction transaction = this.Statement.Connection.BeginTransaction())
                    {
                        transaction.Rollback();
                    }
                }

                try
                {
                    this.Statement.SqliteStatement.Reset();
                }
                catch (SQLiteException)
                {
                    // sqlite.PCL hides result code from Reset() and throws exception instead
                    // when BUSY is returned by sqlite, Reset() will also return Busy for the first call
                    // ignore first time exception so no BUSY exception is returned when disposing this statement
                }

                throw new DatabaseException(
                    DatabaseErrorCode.Deadlock,
                    "This statement has been aborted and the transaction (if any) rolled back to avoid a deadlock. Please try executing your operation again.");
            }
        }
    }
}
