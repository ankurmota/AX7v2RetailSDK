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
    namespace Commerce.Runtime.DataServices
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Reason code data request handler.
        /// </summary>
        public sealed class ReasonCodeDataService : IRequestHandler
        {
            private const int MaxCachedCollectionSize = 500;
    
            private const string ReasonCodeTableSpecificViewName = "INFOCODETABLESPECIFICVIEW";
            private const string ReturnOrderReasonCodesViewName = "RETURNORDERREASONCODESVIEW";
            private const string ReasonCodeSettingsViewName = "INFOCODESETTINGSVIEW";
            private const string InfoCodeIdColumnName = "REASONCODEID";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetReasonCodesByTableRefTypeDataRequest),
                        typeof(GetReturnOrderReasonCodesDataRequest),
                        typeof(GetReasonCodeSettingsDataRequest),
                    };
                }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The request message.</param>
            /// <returns>The response message.</returns>
            public Response Execute(Request request)
            {
                ThrowIf.Null(request, "request");
    
                Response response;
    
                if (request is GetReasonCodesByTableRefTypeDataRequest)
                {
                    response = this.GetReasonCodesByTableRefType((GetReasonCodesByTableRefTypeDataRequest)request);
                }
                else if (request is GetReturnOrderReasonCodesDataRequest)
                {
                    response = this.GetReturnOrderReasonCodes((GetReturnOrderReasonCodesDataRequest)request);
                }
                else if (request is GetReasonCodeSettingsDataRequest)
                {
                    response = this.GetReasonCodeSettings((GetReasonCodeSettingsDataRequest)request);
                }
                else
                {
                    string message = string.Format("Request type '{0}' is not supported", request.GetType().FullName);
                    throw new NotSupportedException(message);
                }
    
                return response;
            }
    
            /// <summary>
            /// Gets the cache accessor for the reason code data service requests.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>An instance of the <see cref="ReasonCodeL2CacheDataStoreAccessor"/> class.</returns>
            private static ReasonCodeL2CacheDataStoreAccessor GetReasonCodeL2CacheDataStoreAccessor(RequestContext context)
            {
                DataStoreManager.InstantiateDataStoreManager(context);
                return new ReasonCodeL2CacheDataStoreAccessor(DataStoreManager.DataStores[DataStoreType.L2Cache], context);
            }
    
            private SingleEntityDataServiceResponse<ReasonCodeSettings> GetReasonCodeSettings(GetReasonCodeSettingsDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                ReasonCodeL2CacheDataStoreAccessor level2CacheDataAccessor = GetReasonCodeL2CacheDataStoreAccessor(request.RequestContext);
    
                bool found;
                bool updateL2Cache;
                ReasonCodeSettings result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetReasonCodeSettings(request.QueryResultSettings.ColumnSet), out found, out updateL2Cache);
    
                if (!found)
                {
                    var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                    {
                        Select = request.QueryResultSettings.ColumnSet,
                        From = ReasonCodeSettingsViewName,
                        Where = "CHANNELID = @channelId",
                    };
    
                    query.Parameters["@channelId"] = request.RequestContext.GetPrincipal().ChannelId;
    
                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<ReasonCodeSettings>(query).SingleOrDefault();
    
                        updateL2Cache &= result != null;
                    }
                }
    
                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutReasonCodeSettings(result);
                }
    
                return new SingleEntityDataServiceResponse<ReasonCodeSettings>(result);
            }
    
            private EntityDataServiceResponse<ReasonCode> GetReturnOrderReasonCodes(GetReturnOrderReasonCodesDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                ReasonCodeL2CacheDataStoreAccessor level2CacheDataAccessor = GetReasonCodeL2CacheDataStoreAccessor(request.RequestContext);
    
                bool found;
                bool updateL2Cache;
                PagedResult<ReasonCode> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetReturnOrderReasonCodes(request.QueryResultSettings), out found, out updateL2Cache);
    
                if (!found)
                {
                    var query = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        From = ReturnOrderReasonCodesViewName,
                        OrderBy = InfoCodeIdColumnName
                    };
    
                    query.Parameters["@DataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
    
                    List<string> whereClauses = new List<string>();
                    whereClauses.Add(@"(DATAAREAID = @DataAreaId)");
                    query.Where = string.Join(" AND ", whereClauses);
    
                    // Load info codes
                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<ReasonCode>(query);
                    }
    
                    updateL2Cache &= result != null
                                     && result.Results.Count < MaxCachedCollectionSize;
                }
    
                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutReturnOrderReasonCodes(request.QueryResultSettings, result);
                }
    
                return new EntityDataServiceResponse<ReasonCode>(result);
            }
    
            private EntityDataServiceResponse<ReasonCode> GetReasonCodesByTableRefType(GetReasonCodesByTableRefTypeDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                ReasonCodeL2CacheDataStoreAccessor level2CacheDataAccessor = GetReasonCodeL2CacheDataStoreAccessor(request.RequestContext);
    
                bool found;
                bool updateL2Cache;
                PagedResult<ReasonCode> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetReasonCodeSpecific(request.TableRefType, request.RefRelation, request.RefRelation2, request.RefRelation3), out found, out updateL2Cache);
    
                if (!found)
                {
                    var query = new SqlPagedQuery(QueryResultSettings.AllRecords)
                    {
                        From = ReasonCodeTableSpecificViewName,
                        OrderBy = InfoCodeIdColumnName
                    };
    
                    var whereClauses = new List<string>();
    
                    whereClauses.Add(@"(REFTABLEID = @RefTableId)");
                    query.Parameters["@RefTableId"] = request.TableRefType;
    
                    // Add query clause for ref relations
                    if (!string.IsNullOrWhiteSpace(request.RefRelation))
                    {
                        whereClauses.Add(@"(REFRELATION = @RefRelation)");
                        query.Parameters["@RefRelation"] = request.RefRelation;
                    }
    
                    if (!string.IsNullOrWhiteSpace(request.RefRelation2))
                    {
                        whereClauses.Add(@"(REFRELATION2 = @RefRelation2)");
                        query.Parameters["@RefRelation2"] = request.RefRelation2;
                    }
    
                    if (!string.IsNullOrWhiteSpace(request.RefRelation3))
                    {
                        whereClauses.Add(@"(REFRELATION3 = @RefRelation3)");
                        query.Parameters["@RefRelation3"] = request.RefRelation3;
                    }
    
                    // Compose the where clause
                    if (whereClauses.Count != 0)
                    {
                        query.Where = string.Join(" AND ", whereClauses);
                    }
    
                    query.OrderBy = "SEQUENCE ASC";
    
                    var reasonCodes = new Collection<ReasonCode>();
    
                    // Load specific reason codes only if at least one refRelation specified with tableRefType
                    if (whereClauses.Count >= 2)
                    {
                        PagedResult<ReasonCodeSpecific> reasonCodesSpecific;
    
                        using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                        {
                            reasonCodesSpecific = databaseContext.ReadEntity<ReasonCodeSpecific>(query);
                        }
    
                        if (reasonCodesSpecific != null)
                        {
                            foreach (var reasonCodeSpecific in reasonCodesSpecific.Results)
                            {
                                GetReasonCodesDataRequest getReasonCodesDataRequest = new GetReasonCodesDataRequest(QueryResultSettings.AllRecords, new string[] { reasonCodeSpecific.ReasonCodeId });
                                var machingReasonCodes = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<ReasonCode>>(getReasonCodesDataRequest, request.RequestContext).PagedEntityCollection.Results;
    
                                foreach (var matchingCode in machingReasonCodes)
                                {
                                    matchingCode.InputRequired = reasonCodeSpecific.InputRequired;
                                    matchingCode.InputRequiredType = (ReasonCodeInputRequiredType)reasonCodeSpecific.InputRequiredTypeValue;
                                }
    
                                reasonCodes.AddRange(machingReasonCodes);
                            }
                        }
                    }
    
                    result = reasonCodes.AsPagedResult();
    
                    updateL2Cache &= result != null && result.Results.Count < MaxCachedCollectionSize;
                }
    
                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutReasonCodeSpecific(request.TableRefType, request.RefRelation, request.RefRelation2, request.RefRelation3, result);
                }
    
                return new EntityDataServiceResponse<ReasonCode>(result);
            }
        }
    }
}
