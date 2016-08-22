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
    namespace Commerce.Runtime.DataAccess.SqlServer
    {
        using System;
        using System.Collections.Generic;
        using System.Data;
        using System.Data.SqlClient;
        using System.Diagnostics;
        using System.Diagnostics.CodeAnalysis;
        using System.Globalization;
        using System.Linq;
        using System.Reflection;
        using System.Text.RegularExpressions;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Framework;
        using Microsoft.Dynamics.Retail.Diagnostics;
        using DataTable = System.Data.DataTable;

        /// <summary>
        /// Implements the database provider for SQLServer.
        /// </summary>
        public class SqlServerDatabaseProvider : IDatabaseProvider
        {
            internal const string CrtDatabaseSchemaName = "crt";
            private const string StoredProcedureReturnValueParameterName = "@RETURN_VALUE";
            private const int FromClauseNonMatchLengthLimit = 100;
            private const string SchemaGroupName = "schema";
            private const string ObjectGroupName = "object";
            private const string DefaultSchemaName = "dbo";
            private static readonly Regex FromClauseRegex = new Regex(@".*\s+FROM\s+(\[?(?<schema>\w+)\]?\.)?\[?(?<object>@?\w+)\]?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            private readonly SqlServerDatabaseQueryBuilder queryBuilder = new SqlServerDatabaseQueryBuilder();
    
            /// <summary>
            /// Configures the database provider.
            /// </summary>
            /// <param name="configurationDictionary">A dictionary of key pair values for configuring the database provider.</param>
            public void Configure(IDictionary<string, string> configurationDictionary)
            {
            }
    
            /// <summary>
            /// Gets a connection to operate against the database.
            /// </summary>
            /// <param name="connectionString">The connection string to obtain the connection for.</param>
            /// <returns>A connection to operate against the database.</returns>
            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "To be disposed by caller.")]
            public IDatabaseConnection GetConnection(string connectionString)
            {
                return new DatabaseConnection(new SqlConnection(connectionString));
            }

            /// <summary>
            /// Provides the implementation of the retry mechanism for unreliable actions and transient conditions.
            /// </summary>
            /// <returns>The retry policy.</returns>
            public RetryPolicy GetRetryPolicy()
            {
                return new RetryPolicy(new SqlServerTransientErrorDetectionStrategy(), 3, TimeSpan.FromMilliseconds(5));
            }

            /// <summary>
            /// Executes a command against the database.
            /// </summary>
            /// <param name="connection">The database connection in which to execute the command.</param>
            /// <param name="query">The query to be executed against the database.</param>
            /// <returns>The database result object containing the results for the executed command.</returns>
            [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "False positive: This is a parameterized call.")]
            public IDatabaseResult ExecuteQuery(IDatabaseConnection connection, IDatabaseQuery query)
            {
                ThrowIf.Null(query, "query");
                SqlDataReader sqlDataReader = null;
                Guid dataAccessInstrumentationId = Guid.NewGuid();

                using (ChannelDbQueryBoundaryPerfContext perfContext = new ChannelDbQueryBoundaryPerfContext())
                {                    
                    string methodName = GetCallerName();
                    string queryString = query.BuildQuery(this.GetDatabaseQueryBuilder()) ?? string.Empty;
                    string parameterList = string.Join(", ", query.Parameters.Select(p => string.Format("{0}={1}", p.Key, p.Value)));

                    // get the string between the "FROM" and next whitespace
                    Match match = FromClauseRegex.Match(queryString);
                    
                    // when match is not found, take the first FromClauseNonMatchLengthLimit characters or whatever is available
                    string fromClause = match.Success
                        ? string.Format(
                            "[{0}].[{1}]",
                            match.Groups[SchemaGroupName].Success ? match.Groups[SchemaGroupName].Value : DefaultSchemaName,
                            match.Groups[ObjectGroupName].Success ? match.Groups[ObjectGroupName].Value : string.Empty)
                        : queryString.Substring(0, Math.Min(FromClauseNonMatchLengthLimit, queryString.Length));

                    RetailLogger.Log.CrtDataAccessExecuteQueryStarted(fromClause, dataAccessInstrumentationId);

                    DatabaseConnection databaseConnection = GetDatabaseConnection(connection);
                    SqlConnection sqlConnection = databaseConnection.SqlConnection;
    
                    using (SqlCommand command = new SqlCommand(queryString, sqlConnection))
                    {
                        command.CommandTimeout = 0; // infinite wait timeout for command execution
    
                        CreateInputParameters(command, query.Parameters);

                        try
                        {
                            RetailLogger.Log.CrtDataAccessSqlServerSelectStarted(methodName, dataAccessInstrumentationId, queryString, parameterList, fromClause);

                            sqlDataReader = command.ExecuteReader();

                            perfContext.CallWasSuccessful();
                            RetailLogger.Log.CrtDataAccessSqlServerSelectFinished(
                                methodName, 
                                callWasSuccessful: true,
                                correlationId: dataAccessInstrumentationId,
                                sqlQuery: queryString,
                                parameterList: parameterList,
                                fromClause: fromClause);
                        }
                        catch (SqlException sqlException)
                        {
                            RetailLogger.Log.CrtDataAccessSqlServerSelectFinished(
                                methodName,
                                callWasSuccessful: false,
                                correlationId: dataAccessInstrumentationId,
                                sqlQuery: queryString,
                                parameterList: parameterList,
                                fromClause: fromClause);

                            RetailLogger.Log.CrtDataAccessExecuteQueryFinished(numberOfResults: 0, wasSuccessful: false, correlationId: dataAccessInstrumentationId);

                            SqlTypeHelper.HandleException(sqlException);
                        }
                    }
                }

                DatabaseResult databaseResult = new DatabaseResult(sqlDataReader);
                databaseResult.ConfigureMonitoringEvent(dataAccessInstrumentationId);
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
            [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "False positive: This is a parameterized call.")]
            public void ExecuteStoredProcedure(
                IDatabaseConnection connection,
                string procedureName,
                IEnumerable<KeyValuePair<string, object>> parameters,
                IDictionary<string, object> outputParameters,
                Action<IDatabaseResult> resultCallback,
                out int? storeProcedureResultValue)
            {
                if (resultCallback == null)
                {
                    throw new ArgumentNullException("resultCallback");
                }
    
                if (string.IsNullOrWhiteSpace(procedureName))
                {
                    throw new ArgumentNullException("procedureName");
                }

                Guid dataAccessInstrumentationId = Guid.NewGuid();

                using (ChannelDbStoredProcBoundaryPerfContext perfContext = new ChannelDbStoredProcBoundaryPerfContext())
                {                                        
                    string parameterList = string.Join(", ", parameters.Select(p => string.Format("{0}={1}", p.Key, p.Value)));                    
    
                    DatabaseConnection databaseConnection = GetDatabaseConnection(connection);
                    SqlConnection sqlConnection = databaseConnection.SqlConnection;
    
                    SqlDataReader reader = null;
    
                    // in case of stored procedure name doesn't contain schema prefix, we add one
                    if (procedureName.IndexOf(".", StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        procedureName = string.Format("[{0}].{1}", CrtDatabaseSchemaName, procedureName);
                    }

                    RetailLogger.Log.CrtDataAccessExecuteStoredProcedureStarted(procedureName, dataAccessInstrumentationId);

                    using (SqlCommand command = new SqlCommand(procedureName, sqlConnection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 0; // infinite wait timeout for command execution
    
                        if (parameters != null)
                        {
                            CreateInputParameters(command, parameters);
                        }
    
                        if (outputParameters != null)
                        {
                            CreateOutputParameters(command, outputParameters);
                        }
    
                        SqlParameter returnValueParameter = AddReturnValueParameter(command);

                        long numberOfReads = 0;
                        bool success = false;

                        try
                        {                            
                            RetailLogger.Log.CrtDataAccessSqlServerStoredProcedureStarted(procedureName, parameterList, dataAccessInstrumentationId);

                            using (reader = command.ExecuteReader())
                            {
                                perfContext.CallWasSuccessful();
                                RetailLogger.Log.CrtDataAccessSqlServerStoredProcedureFinished(procedureName, parameterList, true, dataAccessInstrumentationId);                                

                                using (DatabaseResult result = new DatabaseResult(reader))
                                {
                                    resultCallback(result);
                                    numberOfReads = result.MonitoringNumberOfReads;
                                }
                            }

                            success = true;
                        }
                        catch (SqlException sqlException)
                        {
                            RetailLogger.Log.CrtDataAccessSqlServerStoredProcedureFinished(procedureName, parameterList, false, dataAccessInstrumentationId);
                            SqlTypeHelper.HandleException(sqlException);
                        }
                        finally
                        {
                            RetailLogger.Log.CrtDataAccessExecuteStoredProcedureFinished(numberOfReads, success, dataAccessInstrumentationId);
                        }
    
                        if (outputParameters != null)
                        {
                            ReadOutputParameters(command, outputParameters);
                        }
    
                        storeProcedureResultValue = (int?)returnValueParameter.Value;
                    }
                }
            }
    
            /// <summary>
            ///  Returns SQL server implementation of <see cref="IDatabaseQueryBuilder"/>.
            /// </summary>
            /// <returns>Returns a specific implementation of <see cref="IDatabaseQueryBuilder"/>.</returns>
            public IDatabaseQueryBuilder GetDatabaseQueryBuilder()
            {
                return this.queryBuilder;
            }
    
            /// <summary>
            /// Creates the input parameters for a SQL command.
            /// </summary>
            /// <param name="command">The SQL command.</param>
            /// <param name="inputParameters">The input parameter collection.</param>
            private static void CreateInputParameters(SqlCommand command, IEnumerable<KeyValuePair<string, object>> inputParameters)
            {
                foreach (var parameter in inputParameters)
                {
                    var parameterValue = parameter.Value;

                    Microsoft.Dynamics.Commerce.Runtime.Data.Types.DataTable table;
                    if (SqlTypeHelper.TryGetTable(parameterValue, out table))
                    {
                        if (string.IsNullOrWhiteSpace(table.TableName))
                        {
                            throw new DatabaseException(DatabaseErrorCode.ParameterNotValid, "The table name of the data table must be provided.");
                        }
    
                        DataTable systemDataTable = SqlTypeHelper.ConvertToSystemDataTable(command.Connection, table);
    
                        var sqlParameter = command.Parameters.AddWithValue(parameter.Key, systemDataTable);
                        sqlParameter.TypeName = string.Format("[{0}].{1}", CrtDatabaseSchemaName, systemDataTable.TableName);
                        continue;
                    }
    
                    object databaseValue = SqlTypeHelper.ConvertToDatabaseValue(parameterValue);
                    command.Parameters.AddWithValue(parameter.Key, databaseValue);
                }
            }
    
            /// <summary>
            /// Creates the output parameters for a SQL command.
            /// </summary>
            /// <param name="command">The SQL command.</param>
            /// <param name="outputParameters">The output parameter collection.</param>
            private static void CreateOutputParameters(SqlCommand command, IEnumerable<KeyValuePair<string, object>> outputParameters)
            {
                SqlParameter param;
                foreach (var parameter in outputParameters)
                {
                    object databaseParameter = SqlTypeHelper.ConvertToDatabaseValue(parameter.Value);
    
                    if (parameter.Value is decimal)
                    {
                        param = new SqlParameter(parameter.Key, SqlDbType.Decimal) { Precision = 32, Scale = 16 };
                        param.Value = parameter.Value;
                    }
                    else
                    {
                        param = new SqlParameter(parameter.Key, databaseParameter);
                    }
    
                    param.Direction = ParameterDirection.InputOutput;
                    command.Parameters.Add(param);
                }
            }
    
            /// <summary>
            /// Reads the output parameters from a  SQL command.
            /// </summary>
            /// <param name="command">The SQL command.</param>
            /// <param name="outputParameters">The output parameter dictionary.</param>
            private static void ReadOutputParameters(SqlCommand command, IDictionary<string, object> outputParameters)
            {
                foreach (SqlParameter parameter in command.Parameters)
                {
                    if (parameter.Direction == ParameterDirection.InputOutput)
                    {
                        outputParameters[parameter.ParameterName] = SqlTypeHelper.ConvertToManagedValue(parameter.Value);
                    }
                }
            }
    
            /// <summary>
            /// Gets the SQL connection based on the instance of the interface connection.
            /// </summary>
            /// <param name="connection">The database connection.</param>
            /// <returns>A SQL server database connection.</returns>
            private static DatabaseConnection GetDatabaseConnection(IDatabaseConnection connection)
            {
                if (connection == null)
                {
                    throw new ArgumentNullException("connection", "Database connection must be provided.");
                }
    
                DatabaseConnection databaseConnection = connection as DatabaseConnection;
    
                if (databaseConnection == null)
                {
                    throw new ArgumentException("Database connection implementation must be of type " + typeof(DatabaseConnection).FullName, "connection");
                }
    
                return databaseConnection;
            }
    
            /// <summary>
            /// Adds the return parameter to a SQL command.
            /// </summary>
            /// <param name="sqlCommand">The SQL command.</param>
            /// <returns>The SQL return parameter added to the command.</returns>
            private static SqlParameter AddReturnValueParameter(SqlCommand sqlCommand)
            {
                SqlParameter returnValueParameter = new SqlParameter(StoredProcedureReturnValueParameterName, SqlDbType.Int);
                returnValueParameter.Direction = ParameterDirection.ReturnValue;
                sqlCommand.Parameters.Add(returnValueParameter);
    
                return returnValueParameter;
            }
    
            /// <summary>
            /// Retrieves the name of the business method higher in stack which indirectly calls this method.
            /// </summary>
            /// <returns>String with the class name and method name.</returns>
            private static string GetCallerName()
            {
                StackTrace stackTrace = new StackTrace();
                MethodBase method = stackTrace.GetFrame(6).GetMethod();
                return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", method.DeclaringType.FullName, method.Name);
            }
        }
    }
}
