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
    namespace Commerce.Runtime.DataServices.SqlServer
    {
        using System;
        using System.Collections.ObjectModel;
        using System.Diagnostics.CodeAnalysis;
        using Commerce.Runtime.DataAccess.SqlServer;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Framework;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Wraps common operations associated to database access, maintaining single database connection.
        /// </summary>
        internal class SqlServerDatabaseContext : DatabaseContext
        {
            private QueryResultSettings settings;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="SqlServerDatabaseContext"/> class.
            /// </summary>
            /// <param name="request">The data request.</param>
            public SqlServerDatabaseContext(Request request)
                : base(request.RequestContext)
            {
                this.settings = request.QueryResultSettings;
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="SqlServerDatabaseContext"/> class.
            /// </summary>
            /// <param name="context">The data request context.</param>
            public SqlServerDatabaseContext(RequestContext context)
                : this(context, null)
            {
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="SqlServerDatabaseContext"/> class.
            /// </summary>
            /// <param name="context">The data request context.</param>
            /// <param name="requestSettings">The request settings.</param>
            public SqlServerDatabaseContext(RequestContext context, QueryResultSettings requestSettings)
                : base(context)
            {
                this.settings = requestSettings;
            }
    
            /// <summary>
            /// Gets the database provider.
            /// </summary>
            private new SqlServerDatabaseProvider DatabaseProvider
            {
                get
                {
                    return (SqlServerDatabaseProvider)base.DatabaseProvider;
                }
            }
    
            /// <summary>
            /// Executes the stored procedure using the specified parameters.
            /// </summary>
            /// <typeparam name="T">The type of data being returned.</typeparam>
            /// <param name="procedureName">The name of the stored procedures.</param>
            /// <param name="parameters">The set of parameters.</param>
            /// <returns>A paginated collection of <see cref="CommerceEntity"/> objects.</returns>
            public PagedResult<T> ExecuteStoredProcedure<T>(string procedureName, ParameterSet parameters)
                where T : CommerceEntity, new()
            {
                return this.ExecuteStoredProcedure<T>(procedureName, parameters, null);
            }
    
            /// <summary>
            /// Executes the stored procedure using the specified parameters.
            /// </summary>
            /// <typeparam name="T">The type of data being returned.</typeparam>
            /// <param name="procedureName">The name of the stored procedures.</param>
            /// <param name="parameters">The set of parameters.</param>
            /// <param name="returnValue">The return value of the stored procedure.</param>
            /// <returns>A paginated collection of <see cref="CommerceEntity"/> objects.</returns>
            public PagedResult<T> ExecuteStoredProcedure<T>(string procedureName, ParameterSet parameters, out int returnValue)
                where T : CommerceEntity, new()
            {
                return this.ExecuteStoredProcedure<T>(procedureName, parameters, null, out returnValue);
            }
    
            /// <summary>
            /// Executes the stored procedure using the specified parameters, non-paginated.
            /// </summary>
            /// <typeparam name="T">The type of data being returned.</typeparam>
            /// <param name="procedureName">The name of the stored procedures.</param>
            /// <param name="parameters">The set of parameters.</param>
            /// <returns>A collection of <see cref="CommerceEntity"/> objects.</returns>
            public ReadOnlyCollection<T> ExecuteNonPagedStoredProcedure<T>(string procedureName, ParameterSet parameters)
                where T : CommerceEntity, new()
            {
                return this.ExecuteStoredProcedure<T>(procedureName, parameters).Results;
            }
    
            /// <summary>
            /// Executes the specified stored procedure.
            /// </summary>
            /// <typeparam name="T1">Type of first expected result set.</typeparam>
            /// <typeparam name="T2">Type of second expected result set.</typeparam>
            /// <param name="procedureName">Procedure name to call.</param>
            /// <param name="parameters">Parameters to pass to stored procedure.</param>
            /// <returns>The result of executing the stored procedure.</returns>
            public Tuple<PagedResult<T1>, ReadOnlyCollection<T2>> ExecuteStoredProcedure<T1, T2>(string procedureName, ParameterSet parameters)
                where T1 : CommerceEntity, new()
                where T2 : CommerceEntity, new()
            {
                int returnValue = 0;
    
                return this.ExecuteStoredProcedure<T1, T2>(procedureName, parameters, null, out returnValue);
            }
    
            /// <summary>
            /// Executes the specified stored procedure.
            /// </summary>
            /// <typeparam name="T1">Type of first expected result set.</typeparam>
            /// <typeparam name="T2">Type of second expected result set.</typeparam>
            /// <param name="procedureName">Procedure name to call.</param>
            /// <param name="parameters">Parameters to pass to stored procedure.</param>
            /// <param name="outputParameters">Output parameters of the stored procedure.</param>
            /// <param name="returnValue">The return value of the stored procedure.</param>
            /// <returns>The result of executing the stored procedure.</returns>
            public Tuple<PagedResult<T1>, ReadOnlyCollection<T2>> ExecuteStoredProcedure<T1, T2>(string procedureName, ParameterSet parameters, ParameterSet outputParameters, out int returnValue)
                where T1 : CommerceEntity, new()
                where T2 : CommerceEntity, new()
            {
                var resultTuple = this.ExecuteStoredProcedure<T1, T2, T2, T2, T2, T2, T2, T2>(
                        procedureName, parameters, outputParameters, out returnValue);
    
                return new Tuple<PagedResult<T1>, ReadOnlyCollection<T2>>(resultTuple.Item1, resultTuple.Item2);
            }
    
            /// <summary>
            /// Executes the specified stored procedure.
            /// </summary>
            /// <typeparam name="T1">Type of first expected result set.</typeparam>
            /// <typeparam name="T2">Type of second expected result set.</typeparam>
            /// <typeparam name="T3">Type of third expected result set.</typeparam>
            /// <param name="procedureName">Procedure name to call.</param>
            /// <param name="parameters">Parameters to pass to stored procedure.</param>
            /// <returns>The result of executing the stored procedure.</returns>
            public Tuple<PagedResult<T1>, ReadOnlyCollection<T2>, ReadOnlyCollection<T3>>
                ExecuteStoredProcedure<T1, T2, T3>(string procedureName, ParameterSet parameters)
                where T1 : CommerceEntity, new()
                where T2 : CommerceEntity, new()
                where T3 : CommerceEntity, new()
            {
                int returnValue;
                return this.ExecuteStoredProcedure<T1, T2, T3>(procedureName, parameters, null, out returnValue);
            }
    
            /// <summary>
            /// Executes the specified stored procedure.
            /// </summary>
            /// <typeparam name="T1">Type of first expected result set.</typeparam>
            /// <typeparam name="T2">Type of second expected result set.</typeparam>
            /// <typeparam name="T3">Type of third expected result set.</typeparam>
            /// <param name="procedureName">Procedure name to call.</param>
            /// <param name="parameters">Parameters to pass to stored procedure.</param>
            /// <param name="outputParameters">Output parameters of the stored procedure.</param>
            /// <param name="returnValue">The return value of the stored procedure.</param>
            /// <returns>The result of executing the stored procedure.</returns>
            public Tuple<PagedResult<T1>, ReadOnlyCollection<T2>, ReadOnlyCollection<T3>> ExecuteStoredProcedure<T1, T2, T3>(string procedureName, ParameterSet parameters, ParameterSet outputParameters, out int returnValue)
                where T1 : CommerceEntity, new()
                where T2 : CommerceEntity, new()
                where T3 : CommerceEntity, new()
            {
                var resultTuple = this.ExecuteStoredProcedure<T1, T2, T3, T3, T3, T3, T3, T3>(procedureName, parameters, outputParameters, out returnValue);
    
                return new Tuple<PagedResult<T1>, ReadOnlyCollection<T2>, ReadOnlyCollection<T3>>(resultTuple.Item1, resultTuple.Item2, resultTuple.Item3);
            }

            /// <summary>
            /// Executes the specified stored procedure.
            /// </summary>
            /// <typeparam name="T1">Type of first expected result set.</typeparam>
            /// <typeparam name="T2">Type of second expected result set.</typeparam>
            /// <typeparam name="T3">Type of third expected result set.</typeparam>
            /// <typeparam name="T4">Type of fourth expected result set.</typeparam>
            /// <param name="procedureName">Procedure name to call.</param>
            /// <param name="parameters">Parameters to pass to stored procedure.</param>
            /// <returns>The result of executing the stored procedure.</returns>
            public Tuple<PagedResult<T1>, ReadOnlyCollection<T2>, ReadOnlyCollection<T3>, ReadOnlyCollection<T4>>
                ExecuteStoredProcedure<T1, T2, T3, T4>(string procedureName, ParameterSet parameters)
                where T1 : CommerceEntity, new()
                where T2 : CommerceEntity, new()
                where T3 : CommerceEntity, new()
                where T4 : CommerceEntity, new()
            {
                int returnValue;
                return this.ExecuteStoredProcedure<T1, T2, T3, T4>(procedureName, parameters, null, out returnValue);
            }

            /// <summary>
            /// Executes the specified stored procedure.
            /// </summary>
            /// <typeparam name="T1">Type of first expected result set.</typeparam>
            /// <typeparam name="T2">Type of second expected result set.</typeparam>
            /// <typeparam name="T3">Type of third expected result set.</typeparam>
            /// <typeparam name="T4">Type of fourth expected result set.</typeparam>
            /// <param name="procedureName">Procedure name to call.</param>
            /// <param name="parameters">Parameters to pass to stored procedure.</param>
            /// <param name="outputParameters">Output parameters of the stored procedure.</param>
            /// <param name="returnValue">The return value of the stored procedure.</param>
            /// <returns>The result of executing the stored procedure.</returns>
            public Tuple<PagedResult<T1>, ReadOnlyCollection<T2>, ReadOnlyCollection<T3>, ReadOnlyCollection<T4>> ExecuteStoredProcedure<T1, T2, T3, T4>(string procedureName, ParameterSet parameters, ParameterSet outputParameters, out int returnValue)
                where T1 : CommerceEntity, new()
                where T2 : CommerceEntity, new()
                where T3 : CommerceEntity, new()
                where T4 : CommerceEntity, new()
            {
                var resultTuple = this.ExecuteStoredProcedure<T1, T2, T3, T4, T4, T4, T4, T4>(procedureName, parameters, outputParameters, out returnValue);

                return new Tuple<PagedResult<T1>, ReadOnlyCollection<T2>, ReadOnlyCollection<T3>, ReadOnlyCollection<T4>>(resultTuple.Item1, resultTuple.Item2, resultTuple.Item3, resultTuple.Item4);
            }

            /// <summary>
            /// Executes the specified stored procedure.
            /// </summary>
            /// <typeparam name="T1">Type of first expected result set.</typeparam>
            /// <typeparam name="T2">Type of second expected result set.</typeparam>
            /// <typeparam name="T3">Type of third expected result set.</typeparam>
            /// <typeparam name="T4">Type of fourth expected result set.</typeparam>
            /// <typeparam name="T5">Type of fifth expected result set.</typeparam>
            /// <typeparam name="T6">Type of sixth expected result set.</typeparam>
            /// <typeparam name="T7">Type of seventh expected result set.</typeparam>
            /// <typeparam name="T8">Type of eighth expected result set.</typeparam>
            /// <param name="procedureName">Procedure name to call.</param>
            /// <param name="parameters">Parameters to pass to stored procedure.</param>
            /// <returns>The result of executing the stored procedure.</returns>
            [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "As per design.")]
            public Tuple<PagedResult<T1>, ReadOnlyCollection<T2>, ReadOnlyCollection<T3>, ReadOnlyCollection<T4>, ReadOnlyCollection<T5>, ReadOnlyCollection<T6>, ReadOnlyCollection<T7>, Tuple<ReadOnlyCollection<T8>>>
                ExecuteStoredProcedure<T1, T2, T3, T4, T5, T6, T7, T8>(string procedureName, ParameterSet parameters)
                where T1 : CommerceEntity, new()
                where T2 : CommerceEntity, new()
                where T3 : CommerceEntity, new()
                where T4 : CommerceEntity, new()
                where T5 : CommerceEntity, new()
                where T6 : CommerceEntity, new()
                where T7 : CommerceEntity, new()
                where T8 : CommerceEntity, new()
            {
                int returnValue;
                return this.ExecuteStoredProcedure<T1, T2, T3, T4, T5, T6, T7, T8>(procedureName, parameters, null, out returnValue);
            }
    
            /// <summary>
            /// Executes the specified stored procedure.
            /// </summary>
            /// <typeparam name="T1">Type of first expected result set.</typeparam>
            /// <typeparam name="T2">Type of second expected result set.</typeparam>
            /// <typeparam name="T3">Type of third expected result set.</typeparam>
            /// <typeparam name="T4">Type of fourth expected result set.</typeparam>
            /// <typeparam name="T5">Type of fifth expected result set.</typeparam>
            /// <typeparam name="T6">Type of sixth expected result set.</typeparam>
            /// <typeparam name="T7">Type of seventh expected result set.</typeparam>
            /// <typeparam name="T8">Type of eighth expected result set.</typeparam>
            /// <param name="procedureName">Procedure name to call.</param>
            /// <param name="parameters">Parameters to pass to stored procedure.</param>
            /// <param name="outputParameters">Output parameters of the stored procedure.</param>
            /// <param name="returnValue">The return value of the stored procedure.</param>
            /// <returns>The result of executing the stored procedure.</returns>
            public Tuple<PagedResult<T1>, ReadOnlyCollection<T2>, ReadOnlyCollection<T3>, ReadOnlyCollection<T4>, ReadOnlyCollection<T5>, ReadOnlyCollection<T6>, ReadOnlyCollection<T7>, Tuple<ReadOnlyCollection<T8>>>
                ExecuteStoredProcedure<T1, T2, T3, T4, T5, T6, T7, T8>(string procedureName, ParameterSet parameters, ParameterSet outputParameters, out int returnValue)
                where T1 : CommerceEntity, new()
                where T2 : CommerceEntity, new()
                where T3 : CommerceEntity, new()
                where T4 : CommerceEntity, new()
                where T5 : CommerceEntity, new()
                where T6 : CommerceEntity, new()
                where T7 : CommerceEntity, new()
                where T8 : CommerceEntity, new()
            {
                ReadOnlyCollection<T1> resultThe1st = null;
                ReadOnlyCollection<T2> resultThe2nd = null;
                ReadOnlyCollection<T3> resultThe3rd = null;
                ReadOnlyCollection<T4> resultThe4th = null;
                ReadOnlyCollection<T5> resultThe5th = null;
                ReadOnlyCollection<T6> resultThe6th = null;
                ReadOnlyCollection<T7> resultThe7th = null;
                ReadOnlyCollection<T8> resultThe8th = null;
    
                Action<IDatabaseResult> populateEntitiesCallback = (databaseResult) =>
                {
                    using (EntityReader reader = new EntityReader(
                        databaseResult,
                        this.RequestContext.GetChannelConfiguration() == null ? null : this.RequestContext.GetChannelConfiguration().TimeZoneRecords))
                    {
                        resultThe1st = reader.ReadNextEntityPagedResult<T1>(PagingInfo.AllRecords).Results;
                        resultThe2nd = reader.ReadNextEntityPagedResult<T2>(PagingInfo.AllRecords).Results;
                        resultThe3rd = reader.ReadNextEntityPagedResult<T3>(PagingInfo.AllRecords).Results;
                        resultThe4th = reader.ReadNextEntityPagedResult<T4>(PagingInfo.AllRecords).Results;
                        resultThe5th = reader.ReadNextEntityPagedResult<T5>(PagingInfo.AllRecords).Results;
                        resultThe6th = reader.ReadNextEntityPagedResult<T6>(PagingInfo.AllRecords).Results;
                        resultThe7th = reader.ReadNextEntityPagedResult<T7>(PagingInfo.AllRecords).Results;
                        resultThe8th = reader.ReadNextEntityPagedResult<T8>(PagingInfo.AllRecords).Results;
                    }
                };
    
                this.ExecuteStoredProcedure(
                    procedureName,
                    parameters,
                    outputParameters,
                    populateEntitiesCallback,
                    out returnValue);
    
                var rest = Tuple.Create(resultThe8th.AsReadOnly());
                var resultDataSets = new Tuple<PagedResult<T1>, ReadOnlyCollection<T2>, ReadOnlyCollection<T3>, ReadOnlyCollection<T4>, ReadOnlyCollection<T5>, ReadOnlyCollection<T6>, ReadOnlyCollection<T7>, Tuple<ReadOnlyCollection<T8>>>(
                    this.settings == null ? resultThe1st.AsPagedResult() : new PagedResult<T1>(resultThe1st, this.settings.Paging),
                    resultThe2nd.AsReadOnly(),
                    resultThe3rd.AsReadOnly(),
                    resultThe4th.AsReadOnly(),
                    resultThe5th.AsReadOnly(),
                    resultThe6th.AsReadOnly(),
                    resultThe7th.AsReadOnly(),
                    rest);
    
                return resultDataSets;
            }
    
            /// <summary>
            /// Executes a stored procedure.
            /// </summary>
            /// <param name="procedureName">The procedure name.</param>
            /// <param name="parameters">The set of parameters.</param>
            /// <param name="outputParameters">The set of output parameters.</param>
            /// <param name="resultCallback">The result callback.</param>
            /// <param name="returnValue">The procedure result.</param>
            public void ExecuteStoredProcedure(string procedureName, ParameterSet parameters, ParameterSet outputParameters, Action<IDatabaseResult> resultCallback, out int returnValue)
            {
                ThrowIf.NullOrWhiteSpace(procedureName, "procedureName");
                ThrowIf.Null(parameters, "parameters");
    
                int? procedureReturnValue = null;
    
                if (this.settings != null)
                {
                    var settingsParameter = new QueryResultSettingsTableType(this.settings);
                    parameters.Add(QueryResultSettingsTableType.ParameterName, settingsParameter.DataTable);
                }

                RetryPolicy retryPolicy = this.DatabaseProvider.GetRetryPolicy();

                try
                {
                    retryPolicy.ExecuteAction(
                        () => this.DatabaseProvider.ExecuteStoredProcedure(this.ConnectionManager.Connection, procedureName, parameters, outputParameters, resultCallback, out procedureReturnValue),
                        (policy, retryCount, ex) => RetailLogger.Log.CrtDataAccessTransientExceptionOccurred(policy.RetryCount, policy.RetryInterval, retryCount, ex));
                }
                catch (DatabaseException ex)
                {
                    throw new StorageException(
                    StorageErrors.Microsoft_Dynamics_Commerce_Runtime_CriticalStorageError,
                    (int)ex.ErrorCode,
                    ex,
                    "Failed to read from the database. See inner exception for details");
                }
    
                returnValue = procedureReturnValue.GetValueOrDefault(0);
            }
    
            /// <summary>
            /// Executes the specified stored procedure with the specified parameters.
            /// </summary>
            /// <param name="procedureName">The procedure name.</param>
            /// <param name="parameters">The parameters.</param>
            /// <returns>The error code returned from the stored procedure.</returns>
            public int ExecuteStoredProcedureNonQuery(string procedureName, ParameterSet parameters)
            {
                return this.ExecuteStoredProcedureScalar(procedureName, parameters, null);
            }
    
            /// <summary>
            /// Executes the stored procedure using the specified parameters and returns the output
            /// of the stored procedure as a dataset.
            /// </summary>
            /// <param name="procedureName">The name of the stored procedure.</param>
            /// <param name="parameters">The set of parameters.</param>
            /// <returns>A dataset.</returns>
            [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "False positive: This is a parameterized stored proc call.")]
            public DataSet ExecuteStoredProcedureDataSet(string procedureName, ParameterSet parameters)
            {
                int resultValue;
                return this.ExecuteStoredProcedureDataSet(procedureName, parameters, out resultValue);
            }
    
            /// <summary>
            /// Executes the stored procedure using the specified parameters and returns the output
            /// of the stored procedure as a dataset.
            /// </summary>
            /// <param name="procedureName">The name of the stored procedure.</param>
            /// <param name="parameters">The set of parameters.</param>
            /// <param name="returnValue">The return value.</param>
            /// <returns>A dataset.</returns>
            [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "False positive: This is a parameterized stored proc call.")]
            public DataSet ExecuteStoredProcedureDataSet(string procedureName, ParameterSet parameters, out int returnValue)
            {
                DataSet ds = null;
    
                Action<IDatabaseResult> callback = result =>
                {
                    ds = DatabaseContext.CreateDatasetFromResult(result);
                };
    
                this.ExecuteStoredProcedure(procedureName, parameters, null, callback, out returnValue);
    
                return ds;
            }
    
            /// <summary>
            /// Executes the stored procedure using the specified parameters and returns the return value
            /// of the stored procedure as an integer.
            /// </summary>
            /// <param name="procedureName">The name of the stored procedures.</param>
            /// <param name="parameters">The set of parameters.</param>
            /// <returns>A collection of <see cref="CommerceEntity"/> objects.</returns>
            public int ExecuteStoredProcedureScalar(string procedureName, ParameterSet parameters)
            {
                return this.ExecuteStoredProcedureScalar(procedureName, parameters, null);
            }
    
            /// <summary>
            /// Executes the stored procedure using the specified parameters and returns the return value
            /// of the stored procedure as an integer.
            /// </summary>
            /// <param name="procedureName">The name of the stored procedure.</param>
            /// <param name="parameters">The set of parameters.</param>
            /// <param name="outputParameters">The set of output parameters.</param>
            /// <returns>A collection of <see cref="CommerceEntity"/> objects.</returns>
            public int ExecuteStoredProcedureScalar(string procedureName, ParameterSet parameters, ParameterSet outputParameters)
            {
                int returnValue;
    
                this.ExecuteStoredProcedure(procedureName, parameters, outputParameters, DatabaseContext.ConsumeNonQueryResult, out returnValue);
    
                return returnValue;
            }
    
            private PagedResult<T> ExecuteStoredProcedure<T>(string procedureName, ParameterSet parameters, ParameterSet outputParameters)
                where T : CommerceEntity, new()
            {
                int returnValue;
                return this.ExecuteStoredProcedure<T>(procedureName, parameters, outputParameters, out returnValue);
            }
    
            private PagedResult<T> ExecuteStoredProcedure<T>(string procedureName, ParameterSet parameters, ParameterSet outputParameters, out int returnValue)
                where T : CommerceEntity, new()
            {
                return this.ExecuteStoredProcedure<T, T>(procedureName, parameters, outputParameters, out returnValue).Item1;
            }
        }
    }
}
