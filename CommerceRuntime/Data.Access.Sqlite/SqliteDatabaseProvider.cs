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
        using Commerce.Runtime.DataAccess.Sqlite;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Framework;

        /// <summary>
        /// Implements the database provider for SQLite.
        /// </summary>
        public sealed class SqliteDatabaseProvider : IDatabaseProvider, IDisposable
        {
            private static readonly IDatabaseQueryBuilder QueryBuilder = new SqliteDatabaseQueryBuilder();
            private volatile DataAccess.Sqlite.ConnectionManager connectionManager;

            /// <summary>
            /// Gets the instance of <see cref="ConnectionManager"/>.
            /// </summary>
            private DataAccess.Sqlite.ConnectionManager ConnectionManager
            {
                get
                {
                    if (this.connectionManager == null)
                    {
                        throw new InvalidOperationException("SqliteDatabaseProvider has not been configured. Please call SqliteDatabaseProvider.Configure before any calls to an instance of the class.");
                    }

                    return this.connectionManager;
                }
            }

            /// <summary>
            /// Update configuration parameters.
            /// </summary>
            /// <param name="configuration">The data access configuration.</param>
            public void Configure(SqliteConfiguration configuration)
            {
                ThrowIf.Null(configuration, "dataAccessConfiguration");
                configuration.Validate();

                lock (this)
                {
                    if (this.connectionManager != null)
                    {
                        throw new InvalidOperationException("This instance has already been configured and its configuration values cannot be reset.");
                    }

                    this.connectionManager = new DataAccess.Sqlite.ConnectionManager(configuration);
                }
            }

            /// <summary>
            /// Configures the database provider.
            /// </summary>
            /// <param name="configurationDictionary">A dictionary of key pair values for configuring the database provider.</param>
            public void Configure(IDictionary<string, string> configurationDictionary)
            {
                this.Configure(new SqliteConfiguration(configurationDictionary));
            }

            /// <summary>
            /// Gets a connection to operate against the database.
            /// </summary>
            /// <param name="connectionString">The connection string to obtain the connection for.</param>
            /// <returns>A connection to operate against the database.</returns>
            public IDatabaseConnection GetConnection(string connectionString)
            {
                return this.ConnectionManager.GetConnection(connectionString);
            }

            /// <summary>
            /// Gets table schema info for a table in SQLite databases.
            /// </summary>
            /// <param name="connection">Connection to SQLite databases.</param>
            /// <param name="tableName">The SQLite table name.</param>
            /// <returns><c>SqliteTableSchema</c> object.</returns>
            public SqliteTableSchema GetTableSchemaInfo(IDatabaseConnection connection, string tableName)
            {
                this.ValidateTableExistence(connection, tableName);

                const int IndexOfName = 1, IndexOfType = 2;
                Dictionary<string, SqliteTableColumn> tableColumns = new Dictionary<string, SqliteTableColumn>();

                string queryString = string.Format("PRAGMA table_info({0});", tableName);
                using (var result = this.ExecuteQuery(connection, new SqlQuery(queryString)))
                {
                    while (result.Read())
                    {
                        string columnName = result.GetValue<string>(IndexOfName).ToUpperInvariant();
                        SqlType sqlType = SqlTypeHelper.ConvertStringToSqlType(result.GetValue<string>(IndexOfType));
                        Type managedType = SqlTypeHelper.ConvertSqlTypeToManagedType(sqlType);

                        tableColumns.Add(columnName, new SqliteTableColumn(columnName, sqlType, managedType));
                    }
                }

                return new SqliteTableSchema(tableName, tableColumns);
            }

            /// <summary>
            /// Provides the implementation of the retry mechanism for unreliable actions and transient conditions.
            /// </summary>
            /// <returns>The retry policy.</returns>
            public RetryPolicy GetRetryPolicy()
            {
                // SQLite implementation does not retry.
                return RetryPolicy.NoRetry;
            }

            /// <summary>
            /// Executes a command against the database.
            /// </summary>
            /// <param name="connection">The database connection in which to execute the command.</param>
            /// <param name="query">The query to be executed against the database.</param>
            /// <returns>The database result object containing the results for the executed command.</returns>
            public IDatabaseResult ExecuteQuery(IDatabaseConnection connection, IDatabaseQuery query)
            {
                ThrowIf.Null(query, "query");

                DatabaseConnection databaseConnection = GetSqlConnection(connection);
                string queryString = query.BuildQuery(this.GetDatabaseQueryBuilder());
                DatabaseResult databaseResult;

                using (SqlCommand command = new SqlCommand(queryString, databaseConnection, query.Parameters))
                {
                    databaseResult = command.Execute();
                }

                return databaseResult;
            }

            /// <summary>
            /// Executes a store procedure against the database.
            /// </summary>
            /// <param name="connection">The database connection.</param>
            /// <param name="procedureName">The store procedure name.</param>
            /// <param name="parameters">The set of input parameters for the procedure.</param>
            /// <param name="outputParameters">The set of output parameters for the procedure.</param>
            /// <param name="resultCallback">A callback action to be executed to consume the database result. The procedure results must be fully consumed before the output parameter values are accessed.</param>        
            /// <param name="storeProcedureResultValue">The return value of the stored procedure.</param>
            public void ExecuteStoredProcedure(
                IDatabaseConnection connection,
                string procedureName,
                IEnumerable<KeyValuePair<string, object>> parameters,
                IDictionary<string, object> outputParameters,
                Action<IDatabaseResult> resultCallback,
                out int? storeProcedureResultValue)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Gets the database query builder.
            /// </summary>
            /// <returns>The database query builder.</returns>
            public IDatabaseQueryBuilder GetDatabaseQueryBuilder()
            {
                return SqliteDatabaseProvider.QueryBuilder;
            }

            /// <summary>
            /// Indicates that a connection string is not going to be used any more.
            /// </summary>
            /// <param name="connectionString">A connection string.</param>
            /// <returns>True if the connection string is released successfully otherwise false.</returns>
            /// <remarks>If the method returns false it just means that some connections cannot be released right now because they are in use.</remarks>
            public bool ReleaseConnection(string connectionString)
            {
                ThrowIf.NullOrWhiteSpace(connectionString, "connectionString");

                return this.ConnectionManager.ClearPool(connectionString);
            }

            /// <summary>
            /// Disposes the database provider including its connection manager.
            /// </summary>
            public void Dispose()
            {
                if (this.connectionManager != null)
                {
                    lock (this)
                    {
                        if (this.connectionManager != null)
                        {
                            this.connectionManager.Dispose();
                            this.connectionManager = null;
                        }
                    }
                }
            }
            
            /// <summary>
            /// Executes a command against the database that has no output.
            /// </summary>
            /// <param name="connection">The database connection in which to execute the command.</param>
            /// <param name="query">The query to be executed against the database.</param>
            internal void ExecuteNonQuery(IDatabaseConnection connection, IDatabaseQuery query)
            {
                // exhausts the result set to avoid unexecuted statements
                using (IDatabaseResult result = this.ExecuteQuery(connection, query))
                {
                    do
                    {
                        while (result.Read())
                        {
                        }
                    }
                    while (result.NextResult());
                }
            }

            /// <summary>
            /// Executes a command against the database that returns a single result.
            /// </summary>
            /// <param name="connection">The database connection in which to execute the command.</param>
            /// <param name="query">The query to be executed against the database.</param>
            /// <param name="managedType">The managed type that the database scalar result should be converted to.</param>
            /// <returns>The first column of the first row in the result set, or a null reference if the result set is empty.</returns>
            internal object ExecuteScalar(IDatabaseConnection connection, IDatabaseQuery query, Type managedType)
            {
                object scalar = null;

                using (IDatabaseResult result = this.ExecuteQuery(connection, query))
                {
                    if (result.Read())
                    {
                        scalar = result.GetValue(0, managedType);
                    }
                }

                return scalar;
            }

            /// <summary>
            /// Gets the SQL connection based on the instance of the interface connection.
            /// </summary>
            /// <param name="connection">The database connection.</param>
            /// <returns>A SQL lite database connection.</returns>
            private static DatabaseConnection GetSqlConnection(IDatabaseConnection connection)
            {
                DatabaseConnection databaseConnection = connection as DatabaseConnection;

                if (databaseConnection == null)
                {
                    throw new ArgumentException("Invalid database connection.", "connection");
                }

                return databaseConnection;
            }

            // In order to prevent SQL injection, validate if a given table exists in databases associated with the connection.
            // Since we split online database into 2 databases, namely offline channel database and offline transaction database, therefore for each table,
            // it only exists in one database.
            private void ValidateTableExistence(IDatabaseConnection connection, string tableName)
            {
                const string TableNameParameter = "@TableName";
                const int IndexOfName = 1;
                string queryDatabaseList = "PRAGMA database_list;";
                List<string> databaseList = new List<string>();
                using (var result = this.ExecuteQuery(connection, new SqlQuery(queryDatabaseList)))
                {
                    while (result.Read())
                    {
                        databaseList.Add(result.GetValue<string>(IndexOfName));
                    }
                }

                string queryTableListInRegularDbTemplate =
                    @"SELECT COUNT(*)
                  FROM [{0}].[SQLITE_MASTER]
                  WHERE TYPE = 'table' AND NAME = {1} COLLATE NOCASE;";

                string queryTableListInTempDbTemplate =
                    @"SELECT COUNT(*)
                  FROM [TEMP].[SQLITE_TEMP_MASTER]
                  WHERE TYPE = 'table' AND NAME = {0} COLLATE NOCASE;";

                bool doesTableExist = false;
                foreach (string databaseName in databaseList)
                {
                    SqlQuery query = null;
                    if (string.Compare(databaseName, "temp", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        query = new SqlQuery(string.Format(queryTableListInTempDbTemplate, TableNameParameter));
                    }
                    else
                    {
                        query = new SqlQuery(string.Format(queryTableListInRegularDbTemplate, databaseName, TableNameParameter));
                    }

                    query.Parameters.Add(TableNameParameter, tableName);

                    int count = 0;
                    using (IDatabaseResult result = this.ExecuteQuery(connection, query))
                    {
                        if (result.Read())
                        {
                            count = result.GetValue<int>(0);
                        }
                    }

                    // In current database, table names must be unique.
                    if (count == 1)
                    {
                        doesTableExist = true;
                        break;
                    }
                }

                if (!doesTableExist)
                {
                    throw new ArgumentException(string.Format("Table {0} does not exist in databases associated with the connection.", tableName));
                }
            }
        }
    }
}
