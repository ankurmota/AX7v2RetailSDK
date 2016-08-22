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
    namespace Commerce.Runtime.Services
    {
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Implementation for product search service.
        /// </summary>
        public class SearchProductsService : IRequestHandler
        {
            private static readonly Type[] SupportedRequestTypesArray = new Type[]
            {
                typeof(SearchProductsServiceRequest)
            };
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get { return SupportedRequestTypesArray; }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                ThrowIf.Null(request, "request");
                Response response;
    
                if (request is SearchProductsServiceRequest)
                {
                    response = SearchProducts((SearchProductsServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request type '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            private static SearchProductsServiceResponse SearchProducts(SearchProductsServiceRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                if (request.ChannelId < 0)
                {
                    throw new ArgumentOutOfRangeException("request", request.ChannelId, "Channel identifier cannot be less than zero.");
                }
    
                if (request.CatalogId < 0)
                {
                    throw new ArgumentOutOfRangeException("request", request.CatalogId, "Catalog identifier cannot be less than zero.");
                }
    
                long currentChannelId = request.RequestContext.GetPrincipal().ChannelId;
    
                if (request.ChannelId != currentChannelId && request.CategoryId.HasValue)
                {
                    RemoteSearchProductsRealtimeRequest realtimeRequest = new RemoteSearchProductsRealtimeRequest(request.ChannelId, request.CatalogId, request.CategoryId.Value, request.QueryResultSettings);
                    SearchProductsRealtimeResponse realtimeResponse = request.RequestContext.Execute<SearchProductsRealtimeResponse>(realtimeRequest);

                    return new SearchProductsServiceResponse(realtimeResponse.ProductSearchResults);
                }
                else if (request.ChannelId != currentChannelId && !string.IsNullOrWhiteSpace(request.SearchText))
                {
                    var realtimeRequest = new RemoteSearchProductsRealtimeRequest(request.ChannelId, request.CatalogId, request.SearchText, request.QueryResultSettings);
                    SearchProductsRealtimeResponse realtimeResponse = request.RequestContext.Execute<SearchProductsRealtimeResponse>(realtimeRequest);

                    return new SearchProductsServiceResponse(realtimeResponse.ProductSearchResults);
                }

                GetProductSearchResultsDataRequest dataRequest = null;
    
                if (request.ChannelId == currentChannelId && request.CategoryId.HasValue)
                {
                    dataRequest = new GetProductSearchResultsDataRequest(request.ChannelId, request.CatalogId, request.CategoryId.Value, request.QueryResultSettings);
                }
                else if (request.ChannelId == currentChannelId && !string.IsNullOrWhiteSpace(request.SearchText))
                {
                    GetProductBarcodeDataRequest productBarcodeRequest = new GetProductBarcodeDataRequest(request.SearchText);
                    GetProductBarcodeDataResponse productBarcodeResponse = request.RequestContext.Execute<GetProductBarcodeDataResponse>(productBarcodeRequest);

                    if (productBarcodeResponse != null && productBarcodeResponse.Barcode != null && !string.IsNullOrWhiteSpace(productBarcodeResponse.Barcode.ItemId))
                    {
                        dataRequest = new GetProductSearchResultsDataRequest(request.ChannelId, request.CatalogId, productBarcodeResponse.Barcode.ItemId, request.QueryResultSettings);
                        dataRequest.UseFuzzySearch = false;
                    }
                    else
                    {
                        dataRequest = new GetProductSearchResultsDataRequest(request.ChannelId, request.CatalogId, request.SearchText, request.QueryResultSettings);
                    }
                }
                else
                {
                    throw new NotSupportedException("A valid category identifier or search text has not been specified. One of these values need to be set correctly.");
                }
    
                var dataResponse = request.RequestContext.Execute<EntityDataServiceResponse<ProductSearchResult>>(dataRequest);
    
                return new SearchProductsServiceResponse(dataResponse.PagedEntityCollection);
            }
        }
    }
}
