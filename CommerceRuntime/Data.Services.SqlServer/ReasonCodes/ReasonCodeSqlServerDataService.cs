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
    namespace Commerce.Runtime.DataServices.SqlServer.DataServices
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Reason code data request handler for SQLServer.
        /// </summary>
        public sealed class ReasonCodeSqlServerDataService : IRequestHandler
        {
            private const int MaxCachedCollectionSize = 500;
    
            private const string ReasonCodeFunctionName = "GETRETAILINFOCODESTRANSLATED(@languageId, @defaultlanguageId, @DataAreaId, @tvp_groupIds)";
            private const string ReasonSubCodeFunctionName = "GETRETAILINFOSUBCODESTRANSLATED(@languageId, @defaultlanguageId, @DataAreaId)";
    
            private const string InfoSubCodeIdColumnName = "INFOSUBCODEID";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[] { typeof(GetReasonCodesDataRequest) };
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
    
                if (request is GetReasonCodesDataRequest)
                {
                    response = GetReasonCodes((GetReasonCodesDataRequest)request);
                }
                else
                {
                    string message = string.Format("Request type '{0}' is not supported", request.GetType().FullName);
                    throw new NotSupportedException(message);
                }
    
                return response;
            }
    
            private static EntityDataServiceResponse<ReasonCode> GetReasonCodes(GetReasonCodesDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                string channelLanguageId = GetDefaultLanguageId(request.RequestContext);
                string employeeLanguageId = GetEmployeeLanguageId(request.RequestContext);
    
                ReasonCodeL2CacheDataStoreAccessor level2CacheDataAccessor = GetReasonCodeL2CacheDataStoreAccessor(request.RequestContext);
    
                bool found;
                bool updateL2Cache;
                PagedResult<ReasonCode> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetReasonCodes(request.ReasonCodeIds, channelLanguageId, employeeLanguageId, request.QueryResultSettings), out found, out updateL2Cache);
    
                if (!found)
                {
                    SortingInfo sortingInfo = request.QueryResultSettings.Sorting;
                    if (!sortingInfo.IsSpecified)
                    {
                        sortingInfo = new SortingInfo(ReasonCode.PriorityColumn, false);
                    }
    
                    var query = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        OrderBy = sortingInfo.ToString(),
                        From = ReasonCodeFunctionName,
                        Aliased = true,
                    };
    
                    AddLanguageIdToQuery(query, request.RequestContext);
                    AddDataAreaIdToQuery(query, request.RequestContext);
    
                    // Load info codes
                    using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                    using (StringIdTableType groupIds = new StringIdTableType(request.ReasonCodeIds, "GROUPID"))
                    {
                        // the view sets the INFOCODEID to GROUPID when the reason code is not part of a group, so we always query by GROUPID
                        query.Parameters["@tvp_groupIds"] = groupIds.DataTable;
    
                        result = sqlServerDatabaseContext.ReadEntity<ReasonCode>(query);
                    }
    
                    // Load sub codes
                    if (result.Results.Any())
                    {
                        QueryResultSettings subCodeSettings = QueryResultSettings.AllRecords;
                        var subcodes = GetReasonSubCodes(result.Results.Select(x => x.ReasonCodeId), null, subCodeSettings, request).Results.ToLookup(x => x.ReasonCodeId);
    
                        foreach (var infoCode in result.Results)
                        {
                            infoCode.ReasonSubCodes.Clear();
                            infoCode.ReasonSubCodes.AddRange(subcodes[infoCode.ReasonCodeId]);
                        }
                    }
    
                    updateL2Cache &= result != null && result.Results.Count < MaxCachedCollectionSize;
                }
    
                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutReasonCodes(request.ReasonCodeIds, channelLanguageId, employeeLanguageId, request.QueryResultSettings, result);
                }
    
                return new EntityDataServiceResponse<ReasonCode>(result);
            }
    
            /// <summary>
            /// Gets the reason sub codes with given reason code identifier or reason sub code identifier.
            /// </summary>
            /// <param name="reasonCodeIds">The reason code identifier.</param>
            /// <param name="reasonSubCodeId">The reason sub code identifier.</param>
            /// <param name="settings">The query result settings.</param>
            /// <param name="request">The get reason codes data request.</param>
            /// <returns>The info sub codes.</returns>
            private static PagedResult<ReasonSubCode> GetReasonSubCodes(IEnumerable<string> reasonCodeIds, string reasonSubCodeId, QueryResultSettings settings, GetReasonCodesDataRequest request)
            {
                ThrowIf.Null(reasonCodeIds, "reasonCodeIds");
                ThrowIf.Null(settings, "settings");
    
                var query = new SqlPagedQuery(settings)
                {
                    From = ReasonSubCodeFunctionName,
                    Aliased = true
                };
    
                BuildSubReasonCodesQuery(reasonCodeIds, reasonSubCodeId, query, request.RequestContext);
    
                PagedResult<ReasonSubCode> reasonSubcodes;
    
                using (StringIdTableType type = new StringIdTableType(reasonCodeIds, "REASONCODEID"))
                {
                    query.Parameters["@TVP_INFOCODEIDTABLETYPE"] = type;
    
                    using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                    {
                        reasonSubcodes = sqlServerDatabaseContext.ReadEntity<ReasonSubCode>(query);
                    }
    
                    return reasonSubcodes;
                }
            }
    
            /// <summary>
            /// Builds the query for getting sub reason codes.
            /// </summary>
            /// <param name="reasonCodeIds">The reason code identifiers.</param>
            /// <param name="reasonSubCodeId">The reason sub code identifier.</param>
            /// <param name="query">The query object.</param>
            /// <param name="context">The request context.</param>
            private static void BuildSubReasonCodesQuery(IEnumerable<string> reasonCodeIds, string reasonSubCodeId, SqlPagedQuery query, RequestContext context)
            {
                ThrowIf.Null(query, "query");
    
                // Add query clause for info code ids
                if (reasonCodeIds.Any(reasonCodeId => string.IsNullOrWhiteSpace(reasonCodeId)))
                {
                    throw new ArgumentException("Empty reason code id(s) were encountered.", "reasonCodeIds");
                }
    
                var whereClauses = new List<string>();
    
                // Add query clause for info subcode id (primary key is a combination of infoCodeId and subCodeId).
                if (!string.IsNullOrWhiteSpace(reasonSubCodeId))
                {
                    whereClauses.Add(string.Format(@"({0} = @InfoSubcodeId)", InfoSubCodeIdColumnName));
                    query.Parameters["@InfoSubcodeId"] = reasonSubCodeId;
                }
    
                AddLanguageIdToQuery(query, context);
                AddDataAreaIdToQuery(query, context);
    
                query.Where = string.Join(" AND ", whereClauses);
            }
    
            /// <summary>
            /// Gets the default language id for the channel.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>Language identifier.</returns>
            private static string GetDefaultLanguageId(RequestContext context)
            {
                var getDefaultLanguageIdDataRequest = new GetDefaultLanguageIdDataRequest();
                string defaultLanguageId = context.Runtime.Execute<SingleEntityDataServiceResponse<string>>(getDefaultLanguageIdDataRequest, context).Entity;
    
                return defaultLanguageId;
            }
    
            /// <summary>
            /// Gets the language identifier for current employee.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>Language identifier.</returns>
            private static string GetEmployeeLanguageId(RequestContext context)
            {
                GetEmployeeDataRequest dataRequest = new GetEmployeeDataRequest(context.GetPrincipal().UserId, QueryResultSettings.SingleRecord);
                var currentEmployee = context.Execute<SingleEntityDataServiceResponse<Employee>>(dataRequest).Entity;
                return currentEmployee.CultureName;
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
    
            private static void AddLanguageIdToQuery(SqlPagedQuery query, RequestContext context)
            {
                query.Parameters["@defaultlanguageId"] = GetDefaultLanguageId(context);
                query.Parameters["@languageId"] = GetEmployeeLanguageId(context);
            }
    
            private static void AddDataAreaIdToQuery(SqlPagedQuery query, RequestContext context)
            {
                query.Parameters["@DataAreaId"] = context.GetChannelConfiguration().InventLocationDataAreaId;
            }
        }
    }
}
