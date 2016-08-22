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
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Income Expense Accounts SQL server data service class.
        /// </summary>
        public class IncomeExpenseAccountsSqlServerDataService : IRequestHandler
        {
            private const int MaxCachedCollectionSize = 500;
    
            private const string GetRetailIncomeExpenseAccounts = "GETRETAILINCOMEEXPENSEACCOUNTS";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new Type[]
                    {
                        typeof(GetIncomeExpenseAccountsDataRequest),
                    };
                }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
    
                if (requestType == typeof(GetIncomeExpenseAccountsDataRequest))
                {
                    response = this.GetIncomeExpenseAccounts((GetIncomeExpenseAccountsDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Gets the Income and expense accounts.
            /// </summary>
            /// <param name="request">Get income expense accounts data request.</param>
            /// <returns>Returns the collection of income expense account.</returns>
            private EntityDataServiceResponse<IncomeExpenseAccount> GetIncomeExpenseAccounts(GetIncomeExpenseAccountsDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                ChannelL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChannelL2CacheDataStoreAccessor(request.RequestContext);
    
                bool found;
                bool updateL2Cache;
                PagedResult<IncomeExpenseAccount> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetIncomeExpenseAccounts(request.IncomeExpenseType, request.QueryResultSettings), out found, out updateL2Cache);
    
                if (!found)
                {
                    string dataAreaId = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                    string storeNumber = request.RequestContext.GetOrgUnit().OrgUnitNumber;
    
                    var parameters = new ParameterSet();
    
                    parameters["@nvc_StoreId"] = storeNumber;
                    parameters["@nvc_DataAreaId"] = dataAreaId;
                    parameters["@i_AccountType"] = request.IncomeExpenseType;
    
                    using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                    {
                        result = sqlServerDatabaseContext.ExecuteStoredProcedure<IncomeExpenseAccount>(GetRetailIncomeExpenseAccounts, parameters);
                    }
    
                    updateL2Cache &= result != null
                                     && result.Results.Count < MaxCachedCollectionSize;
                }
    
                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutIncomeExpenseAccounts(request.IncomeExpenseType, request.QueryResultSettings, result);
                }
    
                return new EntityDataServiceResponse<IncomeExpenseAccount>(result);
            }
    
            /// <summary>
            /// Gets the cache accessor for the channel data service requests.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>An instance of the <see cref="ChannelL2CacheDataStoreAccessor"/> class.</returns>
            private ChannelL2CacheDataStoreAccessor GetChannelL2CacheDataStoreAccessor(RequestContext context)
            {
                DataStoreManager.InstantiateDataStoreManager(context);
                return new ChannelL2CacheDataStoreAccessor(DataStoreManager.DataStores[DataStoreType.L2Cache], context);
            }
        }
    }
}
