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
    namespace Commerce.Runtime.DataServices.Common
    {
        using System;
        using System.Collections.Generic;
        using System.Text;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Common data request handler for number sequence.
        /// </summary>
        public class ChargeDataService : IRequestHandler
        {
            private const string ChargesView = "CHARGESVIEW";
            private const string ChargeConfigurationViewName = "MARKUPAUTOCONFIGURATIONVIEW";
            private const string SalesParametersViewName = "SALESPARAMETERSVIEW";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetChargeConfigurationsDataRequest),
                        typeof(GetChargeLinesDataRequest),
                        typeof(GetChargeConfigurationsByHeaderDataRequest),
                        typeof(GetSalesParametersDataRequest),
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
    
                if (requestType == typeof(GetChargeConfigurationsDataRequest))
                {
                    response = this.GetChargeConfigurations((GetChargeConfigurationsDataRequest)request);
                }
                else if (requestType == typeof(GetChargeLinesDataRequest))
                {
                    response = this.GetChargeDetails((GetChargeLinesDataRequest)request);
                }
                else if (requestType == typeof(GetChargeConfigurationsByHeaderDataRequest))
                {
                    response = this.GetChargeConfigurationsByHeader((GetChargeConfigurationsByHeaderDataRequest)request);
                }
                else if (requestType == typeof(GetSalesParametersDataRequest))
                {
                    response = this.GetSalesParameters((GetSalesParametersDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            private SingleEntityDataServiceResponse<SalesParameters> GetSalesParameters(GetSalesParametersDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                ChargeL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChargeL2CacheDataStoreAccessor(request.RequestContext);
                bool found;
                bool updateL2Cache;
                string dataAreaId = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                SalesParameters result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetSalesParameters(dataAreaId, request.QueryResultSettings.ColumnSet), out found, out updateL2Cache);
    
                if (!found)
                {
                    var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                    {
                        Select = request.QueryResultSettings.ColumnSet,
                        From = SalesParametersViewName,
                        Where = "DATAAREAID = @DataAreaId"
                    };
    
                    query.Parameters["@DataAreaId"] = dataAreaId;
    
                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<SalesParameters>(query).SingleOrDefault();
                    }
    
                    updateL2Cache &= result != null;
                }
    
                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutSalesParameters(dataAreaId, request.QueryResultSettings.ColumnSet, result);
                }
    
                return new SingleEntityDataServiceResponse<SalesParameters>(result);
            }
    
            private EntityDataServiceResponse<ChargeConfiguration> GetChargeConfigurationsByHeader(GetChargeConfigurationsByHeaderDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                ChargeL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChargeL2CacheDataStoreAccessor(request.RequestContext);
    
                bool found;
                bool updateL2Cache;
                string dataAreaId = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                PagedResult<ChargeConfiguration> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetChargeConfigurationsByHeader(dataAreaId, request.QueryResultSettings, request.ChargeType, request.Header), out found, out updateL2Cache);
    
                if (!found)
                {
                    var query = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        From = ChargeConfigurationViewName,
                        Where = "DATAAREAID = @DataAreaId"
                    };
    
                    query.Parameters["@DataAreaId"] = dataAreaId;
    
                    this.FilterChargeConfigurations(query, request.ChargeType, request.Header);
    
                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<ChargeConfiguration>(query);
                    }
    
                    updateL2Cache &= result != null;
                }
    
                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutChargeConfigurationsByHeader(dataAreaId, request.QueryResultSettings, request.ChargeType, request.Header, result);
                }
    
                return new EntityDataServiceResponse<ChargeConfiguration>(result);
            }
    
            private void FilterChargeConfigurations(SqlPagedQuery query, ChargeLevel chargeType, ChargeConfigurationHeader header)
            {
                StringBuilder whereClause = new StringBuilder("(");
                whereClause.Append("(MODULECATEGORY = @ChargeType)");
    
                if (header.AccountType != ChargeAccountType.None)
                {
                    whereClause.Append(" AND (ACCOUNTCODE = @AccountType AND ACCOUNTRELATION = @AccountRelation) ");
                }
    
                if (header.ItemType != ChargeItemType.None)
                {
                    whereClause.Append(" AND (ITEMCODE = @ItemType AND ITEMRELATION = @ItemRelation AND MODULETYPE = 1) ");
                }
    
                if (header.DeliveryType != ChargeDeliveryType.None)
                {
                    whereClause.Append(" AND (DLVMODECODE = @DeliveryType AND DLVMODERELATION = @DeliveryRelation AND MODULETYPE = 3) ");
                }
    
                whereClause.Append(")");
                query.Where = whereClause.ToString();
    
                query.Parameters["@ChargeType"] = chargeType;
                query.Parameters["@AccountType"] = header.AccountType;
                query.Parameters["@AccountRelation"] = header.AccountRelation;
                query.Parameters["@ItemType"] = header.ItemType;
                query.Parameters["@ItemRelation"] = header.ItemRelation;
                query.Parameters["@DeliveryType"] = header.DeliveryType;
                query.Parameters["@DeliveryRelation"] = header.DeliveryRelation;
            }
    
            private ChargeL2CacheDataStoreAccessor GetChargeL2CacheDataStoreAccessor(RequestContext context)
            {
                DataStoreManager.InstantiateDataStoreManager(context);
                return new ChargeL2CacheDataStoreAccessor(DataStoreManager.DataStores[DataStoreType.L2Cache], context);
            }
    
            private SingleEntityDataServiceResponse<ChargeLine> GetChargeDetails(GetChargeLinesDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
                ThrowIf.Null(request.ChargeCode, "request.ChargeCode");
    
                ChargeL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChargeL2CacheDataStoreAccessor(request.RequestContext);
    
                bool found;
                bool updateL2Cache;
                string dataAreaId = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                ChargeLine result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetChargeDetails(dataAreaId, request.ChargeCode, request.ChargeModule, request.QueryResultSettings), out found, out updateL2Cache);
    
                if (!found)
                {
                    var query = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        From = ChargesView,
                        Where = "MARKUPCODE = @MarkupCode AND MODULETYPE = @ModuleType And DATAAREAID = @DataAreaId"
                    };
    
                    query.Parameters["@MarkupCode"] = request.ChargeCode;
                    query.Parameters["@ModuleType"] = request.ChargeModule;
                    query.Parameters["@DataAreaId"] = dataAreaId;
    
                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<ChargeLine>(query).FirstOrDefault();
                    }
    
                    if (result == null)
                    {
                        result = new ChargeLine();
                    }
    
                    updateL2Cache &= result != null;
                }
    
                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutChargeDetails(dataAreaId, request.ChargeCode, request.ChargeModule, request.QueryResultSettings, result);
                }
    
                return new SingleEntityDataServiceResponse<ChargeLine>(result);
            }
    
            private EntityDataServiceResponse<ChargeConfiguration> GetChargeConfigurations(GetChargeConfigurationsDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                ChargeL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetChargeL2CacheDataStoreAccessor(request.RequestContext);
    
                bool found;
                bool updateL2Cache;
                string dataAreaId = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                PagedResult<ChargeConfiguration> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetChargeConfigurations(dataAreaId, request.QueryResultSettings), out found, out updateL2Cache);
    
                if (!found)
                {
                    var query = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        From = ChargeConfigurationViewName,
                        Where = "DATAAREAID = @DataAreaId"
                    };
    
                    query.Parameters["@DataAreaId"] = dataAreaId;
    
                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<ChargeConfiguration>(query);
                    }
    
                    updateL2Cache &= result != null;
                }
    
                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutChargeConfigurations(dataAreaId, request.QueryResultSettings, result);
                }
    
                return new EntityDataServiceResponse<ChargeConfiguration>(result);
            }
        }
    }
}
