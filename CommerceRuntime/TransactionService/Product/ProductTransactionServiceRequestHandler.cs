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
    namespace Commerce.Runtime.TransactionService
    {
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using System.Linq;
        using System.Xml.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;

        /// <summary>
        /// The product transaction service implementation.
        /// </summary>
        public class ProductTransactionServiceRequestHandler : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetProductDataRealtimeRequest),
                        typeof(GetRemoteProductsByCategoryRealtimeRequest),
                        typeof(GetRemoteProductsByKeywordRealtimeRequest),
                        typeof(RemoteSearchProductsRealtimeRequest)
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
                if (requestType == typeof(GetProductDataRealtimeRequest))
                {
                    response = GetProductData((GetProductDataRealtimeRequest)request);
                }
                else if (requestType == typeof(GetRemoteProductsByCategoryRealtimeRequest))
                {
                    response = GetRemoteProductsByCategory((GetRemoteProductsByCategoryRealtimeRequest)request);
                }
                else if (requestType == typeof(GetRemoteProductsByKeywordRealtimeRequest))
                {
                    response = GetRemoteProductsByKeyword((GetRemoteProductsByKeywordRealtimeRequest)request);
                }
                else if (requestType == typeof(RemoteSearchProductsRealtimeRequest))
                {
                    response = RemoteSearchProducts((RemoteSearchProductsRealtimeRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request type '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            private static ProductSearchRealtimeResponse GetRemoteProductsByKeyword(GetRemoteProductsByKeywordRealtimeRequest request)
            {
                var client = new TransactionServiceClient(request.RequestContext);
                long currentChannelId = request.RequestContext.GetPrincipal().ChannelId;
    
                var products = client.GetProductsByKeyword(
                    currentChannelId,
                    request.ChannelId,
                    request.CatalogId.GetValueOrDefault(),
                    request.Keywords,
                    request.QueryResultSettings.Paging.Skip,
                    request.QueryResultSettings.Paging.Top,
                    request.AttributeIds);
    
                var results = new ProductSearchResultContainer(products);
                return new ProductSearchRealtimeResponse(results);
            }
    
            private static ProductSearchRealtimeResponse GetRemoteProductsByCategory(GetRemoteProductsByCategoryRealtimeRequest request)
            {
                var client = new TransactionServiceClient(request.RequestContext);
                long currentChannelId = request.RequestContext.GetPrincipal().ChannelId;
    
                var products = client.GetProductsByCategory(
                    currentChannelId,
                    request.ChannelId,
                    request.CatalogId.GetValueOrDefault(),
                    request.CategoryId,
                    request.QueryResultSettings.Paging.Skip,
                    request.QueryResultSettings.Paging.Top,
                    request.AttributeIds,
                    request.IncludeProductsFromDescendantCategories);
    
                var results = new ProductSearchResultContainer(products);
    
                return new ProductSearchRealtimeResponse(results);
            }
    
            private static SearchProductsRealtimeResponse RemoteSearchProducts(RemoteSearchProductsRealtimeRequest request)
            {
                var client = new TransactionServiceClient(request.RequestContext);
                PagedResult<ProductSearchResult> searchResults = null;
                long currentChannelId = request.RequestContext.GetPrincipal().ChannelId;
                
                var retrieveAttributeSchemaEntriesDataRequest = new GetProductMediaAttributeSchemaEntriesDataRequest();
                retrieveAttributeSchemaEntriesDataRequest.QueryResultSettings = new QueryResultSettings(new ColumnSet(new string[] { "ATTRIBUTE", "DATATYPE" }), PagingInfo.CreateWithExactCount(2, 0), new SortingInfo("DATATYPE"));
                var attributeSchemaEntries = request.RequestContext.Execute<EntityDataServiceResponse<ProductAttributeSchemaEntry>>(retrieveAttributeSchemaEntriesDataRequest).PagedEntityCollection.Results;
                var imageAttributeId = attributeSchemaEntries.Single(r => r.DataType == AttributeDataType.Image).RecordId;
                var attributesValuesToRetrieve = Convert.ToString(imageAttributeId);

                if (request.CategoryId.HasValue)
                {
                    searchResults = new PagedResult<ProductSearchResult>(client.SearchProductsByCategoryId(currentChannelId, (long)request.CategoryId, request.RequestContext.LanguageId, request.ChannelId, request.CatalogId, attributesValuesToRetrieve, request.QueryResultSettings), request.QueryResultSettings.Paging);
                }
                else if (!string.IsNullOrWhiteSpace(request.SearchText))
                {
                    searchResults = new PagedResult<ProductSearchResult>(client.SearchProductsByText(currentChannelId, request.SearchText, request.ChannelId, request.CatalogId, attributesValuesToRetrieve, request.QueryResultSettings), request.QueryResultSettings.Paging);
                }
                else
                {
                    throw new ArgumentOutOfRangeException("request", "A valid category identfier or search text must be provided to search products.");
                }
    
                return new SearchProductsRealtimeResponse(searchResults);
            }
    
            private static GetProductDataRealtimeResponse GetProductData(GetProductDataRealtimeRequest request)
            {
                var client = new TransactionServiceClient(request.RequestContext);
    
                XDocument productsXml = null;
    
                if (!request.ItemIds.IsNullOrEmpty())
                {
                    productsXml = client.GetProductData(request.ItemIds);
                }
                else if (!request.ProductIds.IsNullOrEmpty())
                {
                    productsXml = client.GetProductData(request.ProductIds);
                }
                else
                {
                    throw new ArgumentException("Either request.ItemIds or request.ProductIds must be populated.");
                }
    
                return new GetProductDataRealtimeResponse(productsXml);
            }
        }
    }
}
