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
    namespace Commerce.Runtime.Data.Sqlite
    {
        using System;
        using System.Collections.Generic;
        using System.Diagnostics.CodeAnalysis;
        using Commerce.Runtime.DataAccess.Sqlite;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Retail.Diagnostics;
        using SQLitePCL;

        /// <summary>
        /// Represents a database connection.
        /// </summary>
        /// <remarks>Sharing a connection between threads at the same time is not supported by this class, thus this class is not thread safe.
        /// Any synchronization must be handled by the caller. To use multithreads with a database connection use <see cref="ConnectionManager"/>.</remarks>
        internal sealed class DatabaseConnection : IDatabaseConnection
        {
            /// <summary>
            /// The maximum number of cached statements.
            /// </summary>
            private readonly int statementCacheSize;

            /// <summary>
            /// The busy timeout in milliseconds.
            /// </summary>
            private readonly int busyTimeout;

            /// <summary>
            /// A cache of compiled statements.
            /// </summary>
            private Dictionary<string, SqlStatement> statementCache;

            /// <summary>
            /// A counter for the number of attached databases.
            /// </summary>
            private int attachedDatabaseCounter;

            /// <summary>
            /// The connection manager that has the connection pool from which this connection has been initialized.
            /// </summary>
            private DataAccess.Sqlite.ConnectionManager connectionManager;

            /// <summary>
            /// A value indicating whether this connection has been disposed or not.
            /// </summary>
            private bool isDisposed;

            /// <summary>
            /// A value indicating whether this connection is being used or not.
            /// </summary>
            private volatile bool inUse;

            /// <summary>
            /// A value indicating whether this connection is marked to be removed from the pool instead of being reused.
            /// </summary>
            private volatile bool markedForRemoval;

            /// <summary>
            /// Initializes a new instance of the <see cref="DatabaseConnection"/> class.
            /// </summary>
            /// <param name="connectionString">The SQLite connection string.</param>
            /// <param name="statementCachingSize">The size of the statement cache pool.</param>
            /// <param name="busyTimeout">The timeout value in milliseconds for waiting on a busy database resource.</param>
            /// <param name="connectionManager">The connection managed that has the connection pool from which this connection has been initialized.</param>
            public DatabaseConnection(string connectionString, int statementCachingSize, int busyTimeout, DataAccess.Sqlite.ConnectionManager connectionManager)
            {
                ThrowIf.NullOrWhiteSpace(connectionString, "sqlConnection");

                if (statementCachingSize < 1)
                {
                    throw new ArgumentOutOfRangeException("statementCachingSize", "Statement caching size must be positive.");
                }

                if (busyTimeout < 0)
                {
                    throw new ArgumentOutOfRangeException("busyTimeout", "Busy timeout must be greater or equal to 0.");
                }

                this.ConnectionString = connectionString;
                this.statementCacheSize = statementCachingSize;
                this.busyTimeout = busyTimeout;
                this.connectionManager = connectionManager;
                this.SqlConnection = null;
                this.isDisposed = false;
                this.inUse = false;
                this.markedForRemoval = false;
                this.attachedDatabaseCounter = 0;
                this.statementCache = new Dictionary<string, SqlStatement>(StringComparer.OrdinalIgnoreCase);
            }

            /// <summary>
            /// Gets a value indicating whether this connection is in use or not.
            /// </summary>
            public bool InUse
            {
                get
                {
                    return this.inUse;
                }
            }

            /// <summary>
            /// Gets the connection string for this connection.
            /// </summary>
            public string ConnectionString
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets a value indicating whether the connection is disposed.
            /// </summary>
            public bool IsDisposed
            {
                get { return this.isDisposed; }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the connection is under a transaction or not.
            /// </summary>
            internal bool IsUnderTransaction
            {
                get;
                set;
            }

            /// <summary>
            /// Gets the actual underlying connection implementation.
            /// </summary>
            internal SQLiteConnection SqlConnection
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets or sets a value indicating whether this connection is marked to be removed from the pool instead of being reused.
            /// </summary>
            /// <remarks>Setting this value to true will remove it from the pool and dispose it completely, instead of keeping it on the pool for reuse.</remarks>
            internal bool MarkedForRemoval
            {
                get
                {
                    return this.markedForRemoval;
                }

                set
                {
                    this.markedForRemoval = value;
                }
            }

            /// <summary>
            /// Disposes the connection.
            /// </summary>
            public void Dispose()
            {
                if (this.isDisposed)
                {
                    return;
                }

                this.connectionManager.ReleaseConnection(this);
            }

            /// <summary>
            /// Begins a transaction on the this connection.
            /// </summary>
            /// <returns>The transaction object.</returns>
            public IDatabaseTransaction BeginTransaction()
            {
                return new SqliteTransaction(this);
            }

            /// <summary>
            /// Opens the connection on the database.
            /// </summary>
            /// <remarks>This operation must be performed on the connection before any other action can be executed using the connection.
            /// The connection should be disposed calling <see cref="Dispose()"/> on this object.</remarks>
            public void Open()
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException(typeof(DatabaseConnection).FullName, "Cannot call Open() on a disposed connection.");
                }

                if (this.SqlConnection == null)
                {
                    SqliteConnectionStringBuilder connectionString = new SqliteConnectionStringBuilder(this.ConnectionString);

                    try
                    {
                        this.SqlConnection = new SQLiteConnection(connectionString.MainDatabase);

                        // attach any other databases
                        this.AttachDatabase(connectionString.AttachedDatabases);

                        // store temp tables in memory
                        this.ExecuteSqlCommand("pragma temp_store=2;");
                        this.ExecuteSqlCommand(string.Format("pragma busy_timeout={0};", this.busyTimeout));
                    }
                    catch (DatabaseException)
                    {
                        this.DiposeSqliteConnection();
                        throw;
                    }
                    catch (SQLiteException exception)
                    {
                        this.DiposeSqliteConnection();
                        SqlTypeHelper.ThrowDatabaseException(exception);
                    }
                    catch (Exception exception)
                    {
                        this.DiposeSqliteConnection();

                        throw new DatabaseException("Error when opening the connection.", exception);
                    }
                }
            }

            /// <summary>
            /// Informs that this connection is not being used anymore.
            /// </summary>
            public void Release()
            {
                this.inUse = false;
            }

            /// <summary>
            /// Informs that this connection is in use.
            /// </summary>
            public void Reserve()
            {
                this.inUse = true;
            }

            /// <summary>
            /// Prepares a statement for execution based on the command string provided.
            /// </summary>
            /// <param name="commandString">The database command to be executed.</param>
            /// <returns>The prepared statement.</returns>
            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "To be disposed by caller.")]
            public SqlStatement PrepareStatement(string commandString)
            {
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException(typeof(DatabaseConnection).FullName, "Cannot call PrepareStatement() on a disposed connection.");
                }

                if (this.SqlConnection == null)
                {
                    throw new DatabaseException(DatabaseErrorCode.OperationNotValid, "Cannot call PrepareStatement() on an unopened connection.");
                }

                SqlStatement statement;

                // tries to get statement from cache
                if (!this.statementCache.TryGetValue(commandString, out statement))
                {
                    // if the statement is not cached yet, create it and add it to the cache
                    try
                    {
                        ISQLiteStatement sqliteStatement = this.SqlConnection.Prepare(commandString);

                        statement = new SqlStatement(sqliteStatement, this);
                        this.CacheStatement(commandString, statement);
                    }
                    catch (SQLiteException exception)
                    {
                        SqlTypeHelper.ThrowDatabaseException(exception);
                    }
                }

                // mark this statement in use so we cannot remove it from the statement cache
                statement.InUse = true;

                // cached statements needs to be reset to start the execution from the begining
                // it is expected that once a result has been consumed the DatabaseResult is disposed reseting the statement
                // reset here handles the case of a bogus application code that did not dispose its database result
                try
                {
                    statement.SqliteStatement.Reset();
                }
                catch (SQLiteException exception)
                {
                    SqlTypeHelper.ThrowDatabaseException(exception);
                }

                return statement;
            }

            /// <summary>
            /// Releases the resources used by the instance.
            /// </summary>
            internal void ReleaseResources()
            {
                // we are disposing the connection
                if (this.statementCache != null)
                {
                    foreach (SqlStatement statement in this.statementCache.Values)
                    {
                        statement.Dispose();
                    }

                    this.statementCache.Clear();
                    this.statementCache = null;
                }

                this.DiposeSqliteConnection();

                this.connectionManager = null;

                this.isDisposed = true;
            }

            /// <summary>
            /// Disposes <see cref="SqlConnection"/>.
            /// </summary>
            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are logged.")]
            private void DiposeSqliteConnection()
            {
                if (this.SqlConnection != null)
                {
                    try
                    {
                        this.SqlConnection.Dispose();
                    }
                    catch (Exception exception)
                    {
                        RetailLogger.Log.CrtDataAccessSqliteConnectionDisposeFailure(exception);
                    }
                    finally
                    {
                        this.SqlConnection = null;
                    }
                }
            }

            /// <summary>
            /// Attaches databases to the current connection.
            /// </summary>
            /// <param name="attachDatabases">One or more databases to be attached.</param>
            private void AttachDatabase(IEnumerable<string> attachDatabases)
            {
                var parameters = new Dictionary<string, object>();

                foreach (string attachDatabase in attachDatabases)
                {
                    parameters["@databaseName"] = attachDatabase;
                    parameters["@databaseAlias"] = "db" + this.attachedDatabaseCounter++;
                    this.ExecuteSqlCommand("ATTACH DATABASE @databaseName AS @databaseAlias;", parameters);
                }
            }

            /// <summary>
            /// Executes the SQL command.
            /// </summary>
            /// <param name="sql">The SQL.</param>
            private void ExecuteSqlCommand(string sql)
            {
                this.ExecuteSqlCommand(sql, new Dictionary<string, object>());
            }

            /// <summary>
            /// Executes the SQL command.
            /// </summary>
            /// <param name="sql">The SQL.</param>
            /// <param name="parameters">The parameters.</param>
            private void ExecuteSqlCommand(string sql, IDictionary<string, object> parameters)
            {
                using (SqlCommand command = new SqlCommand(sql, this, parameters))
                {
                    using (var databaseResult = command.Execute())
                    {
                        do
                        {
                        }
                        while (databaseResult.Read());
                    }
                }
            }

            /// <summary>
            /// Caches a statement for this connection.
            /// </summary>
            /// <param name="commandString">The command string used to prepare the statement.</param>
            /// <param name="statement">The prepared statement.</param>
            private void CacheStatement(string commandString, SqlStatement statement)
            {
                // if we reached our cache limit, we need to take someone out
                if (this.statementCache.Count >= this.statementCacheSize)
                {
                    string keyToRemove = null;

                    foreach (var pair in this.statementCache)
                    {
                        if (!pair.Value.InUse)
                        {
                            keyToRemove = pair.Key;
                            break;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(keyToRemove))
                    {
                        throw new DatabaseException("The connection's statement cache is full. This is usually caused by the caller not disposing statements after they are executed. " +
                                                    "Make sure to call Dispose() on your DatabaseResult object.");
                    }

                    this.RemoveCachedStatement(keyToRemove);
                }

                this.statementCache.Add(commandString, statement);
            }

            /// <summary>
            /// Removes a statement from the cache by its command string.
            /// </summary>
            /// <param name="commandString">The command string used to prepare the statement.</param>
            private void RemoveCachedStatement(string commandString)
            {
                SqlStatement statement = this.statementCache[commandString];
                this.statementCache.Remove(commandString);

                statement.Dispose();
            }
        }
    }
}
