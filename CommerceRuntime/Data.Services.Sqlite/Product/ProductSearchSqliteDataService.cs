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
    namespace Commerce.Runtime.DataServices.Sqlite
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Product;
    
        /// <summary>
        /// The SQL server data request handler to search for products.
        /// </summary>
        public class ProductSearchSqliteDataService : IRequestHandler
        {
            private const string CatalogIdVariableName = "@bi_CatalogId";
            private const string CategoryIdVariableName = "@bi_CategoryId";
            private const string LocaleVariableName = "@nvc_Locale";
    
            private const string SearchProductsByCategoryIdSprocName = "SEARCHPRODUCTSBYCATEGORYID";
    
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
                    response = ProductSearchSqliteDataService.ProcessProductSearchRequest((GetProductSearchResultsDataRequest)request);
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
    
                if (request.ChannelId <= 0)
                {
                    throw new ArgumentOutOfRangeException("request", request.ChannelId, "Channel identifier cannot be less than zero.");
                }
    
                if (request.CatalogId < 0)
                {
                    throw new ArgumentOutOfRangeException("request", request.CatalogId, "Catalog identifier cannot be less than zero.");
                }
    
                if (request.RequestContext.GetPrincipal().ChannelId != request.ChannelId)
                {
                    throw new ArgumentOutOfRangeException("request", "GetProductSearchResultsDataRequest can only search for products in the current channel.");
                }
    
                PagedResult<ProductSearchResult> results;
    
                if (request.CategoryId > 0)
                {
                    results = new SearchProductsByCategoryIdProcedure(request).Execute();
    
                    return new EntityDataServiceResponse<ProductSearchResult>(results);
                }
    
                if (!string.IsNullOrWhiteSpace(request.SearchText))
                {
                    throw new NotImplementedException("Will be implemented soon.");
                }
    
                throw new InvalidOperationException("Only product search by category id and text is supported. Please provide a valid category id or search text.");
            }
        }
    }
}
