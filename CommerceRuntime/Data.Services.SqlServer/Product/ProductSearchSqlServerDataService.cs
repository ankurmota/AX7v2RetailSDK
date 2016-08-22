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
        /// The SQL server data request handler to search for products.
        /// </summary>
        public class ProductSearchSqlServerDataService : IRequestHandler
        {
            // Function names.
            private const string GetProductResultsByCategoryIdFunctionName = "GETPRODUCTSEARCHRESULTSBYCATEGORYID(@bi_ChannelId, @bi_CatalogId, @dt_ChannelDate, @nvc_Locale, @bi_CategoryId)";
            private const string SearchProductsBytextFunctionName = "GETPRODUCTSEARCHRESULTSBYTEXT(@bi_ChannelId, @bi_CatalogId, @dt_ChannelDate, @i_MaxTop, @nvc_Locale, @nvc_SearchText)";
    
            // Variable names.
            private const string CatalogIdVariableName = "@bi_CatalogId";
            private const string CategoryIdVariableName = "@bi_CategoryId";
            private const string LocaleVariableName = "@nvc_Locale";
            private const string SearchTextVariableName = "@nvc_SearchText";
            private const string MaxTopVariableName = "@i_MaxTop";

            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new Type[] { typeof(GetProductSearchResultsDataRequest) };
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
    
                if (requestType == typeof(GetProductSearchResultsDataRequest))
                {
                    response = ProductSearchSqlServerDataService.ProcessProductSearchRequest((GetProductSearchResultsDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Re-directs product search requests to appropriate handlers based on search type.
            /// </summary>
            /// <param name="request">The request to perform a search operation.</param>
            /// <returns>A collection of search results representative of a product.</returns>
            private static EntityDataServiceResponse<ProductSearchResult> ProcessProductSearchRequest(GetProductSearchResultsDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                if (request.RequestContext.GetPrincipal().ChannelId != request.ChannelId)
                {
                    throw new ArgumentOutOfRangeException("request", "GetProductSearchResultsDataRequest can only search for products in the current channel.");
                }
    
                PagedResult<ProductSearchResult> results = null;
                var channelDateTime = request.RequestContext.GetNowInChannelTimeZone().DateTime;
    
                if (request.CategoryId.HasValue)
                {
                    results = SearchByCategoryId(request.ChannelId, request.CatalogId, channelDateTime, request.RequestContext.LanguageId, (long)request.CategoryId, request.RequestContext, request.QueryResultSettings);
                }
    
                if (!string.IsNullOrWhiteSpace(request.SearchText))
                {
                    results = SearchByText(request.ChannelId, request.CatalogId, channelDateTime, request.RequestContext.LanguageId, request.SearchText, request.UseFuzzySearch, request.RequestContext, request.QueryResultSettings);
                }
    
                return new EntityDataServiceResponse<ProductSearchResult>(results);
            }
    
            private static PagedResult<ProductSearchResult> SearchByCategoryId(long channelId, long catalogId, DateTime channelDateTime, string locale, long categoryId, RequestContext context, QueryResultSettings settings)
            {
                var defaultSort = new SortingInfo("RECID");

                var query = new SqlPagedQuery(settings, defaultSort)
                {
                    From = GetProductResultsByCategoryIdFunctionName
                };

                // This hint ensures efficient parameter sniffing resulting in only parts of the SQL code relevant to this specific request being executed.
                // In this case, when catalog id is zero, all catalog related SQL operation(s) are completely eliminated from the execution plan.
                query.AddHints("RECOMPILE");
                query.Parameters[DatabaseAccessor.ChannelIdVariableName] = channelId;
                query.Parameters[CatalogIdVariableName] = catalogId;
                query.Parameters[DatabaseAccessor.ChannelDateVariableName] = channelDateTime;
                query.Parameters[LocaleVariableName] = locale;
                query.Parameters[CategoryIdVariableName] = categoryId;

                using (DatabaseContext databaseContext = new DatabaseContext(context))
                {
                    return databaseContext.ReadEntity<ProductSearchResult>(query);
                }
            }

            private static PagedResult<ProductSearchResult> SearchByText(long channelId, long catalogId, DateTime channelDateTime, string locale, string searchText, bool useFuzzySearch, RequestContext context, QueryResultSettings settings)
            {
                var query = new SqlPagedQuery(settings)
                {
                    From = SearchProductsBytextFunctionName
                };

                query.Parameters[DatabaseAccessor.ChannelIdVariableName] = channelId;
                query.Parameters[CatalogIdVariableName] = catalogId;
                query.Parameters[DatabaseAccessor.ChannelDateVariableName] = channelDateTime;
                query.Parameters[MaxTopVariableName] = (int)(settings.Paging.NumberOfRecordsToFetch + settings.Paging.Skip);
                query.Parameters[LocaleVariableName] = locale;
                query.Parameters[SearchTextVariableName] = new FreeTextSearchFormatter(searchText) { UseFuzzySearch = useFuzzySearch }.GetFormattedSearchText();

                // This hint ensures efficient parameter sniffing resulting in only parts of the SQL code relevant to this specific request being executed.
                // In this case, when catalog id is zero, all catalog related SQL operation(s) are completely eliminated from the execution plan.
                query.AddHints("RECOMPILE");

                using (DatabaseContext databaseContext = new DatabaseContext(context))
                {
                    return databaseContext.ReadEntity<ProductSearchResult>(query);
                }
            }
        }
    }
}
