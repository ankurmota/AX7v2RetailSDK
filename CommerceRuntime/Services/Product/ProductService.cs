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
        using System.Linq;
        using System.Xml;
        using System.Xml.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// Implementation for product service.
        /// </summary>
        public class ProductService : IRequestHandler
        {
            private static readonly Type[] SupportedRequestTypesArray = new Type[]
            {
                typeof(ProductSearchServiceRequest),
                typeof(GetProductServiceRequest),
                typeof(GetProductRefinersRequest)
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

                if (request is ProductSearchServiceRequest)
                {
                    response = SearchProducts((ProductSearchServiceRequest)request);
                }
                else if (request is GetProductServiceRequest)
                {
                    response = GetProducts((GetProductServiceRequest)request);
                }
                else if (request is GetProductRefinersRequest)
                {
                    response = GetProductRefiners((GetProductRefinersRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request type '{0}' is not supported.", request.GetType()));
                }

                return response;
            }

            private static ProductSearchServiceResponse SearchProducts(ProductSearchServiceRequest request)
            {
                ProductSearchServiceResponse response;

                long currentChannelId = request.RequestContext.GetPrincipal().ChannelId;
                if (request.QueryCriteria.Context.IsRemoteLookup(currentChannelId))
                {
                    response = SearchRemoteProducts(request);
                }
                else
                {
                    response = SearchProductsLocal(request);
                    var results = response.ProductSearchResult;

                    // Download product data from AX if it is not found locally and the DownloadProductData flag is set.
                    if (results.Results.Count < request.QueryCriteria.ItemIds.Distinct().Count() && request.QueryCriteria.DownloadProductData == true)
                    {
                        List<string> itemIds = new List<string>();

                        foreach (ProductLookupClause productLookupClause in request.QueryCriteria.ItemIds)
                        {
                            itemIds.Add(productLookupClause.ItemId);
                        }

                        var getProductDataServiceRequest = new GetProductDataRealtimeRequest(itemIds);

                        var getProductDataServiceResponse = request.RequestContext.Runtime.Execute<GetProductDataRealtimeResponse>(getProductDataServiceRequest, request.RequestContext);
                        var productsXml = getProductDataServiceResponse.ProductDataXml;

                        var manager = new ProductDataManager(request.RequestContext);
                        manager.SaveProductData(productsXml);

                        // Reset the channel context since the remote product data has been saved locally.
                        request.QueryCriteria.Context.ChannelId = request.RequestContext.GetPrincipal().ChannelId;
                        request.QueryCriteria.Context.CatalogId = 0;

                        response = SearchProductsLocal(request);
                    }
                }

                return response;
            }

            private static ProductSearchServiceResponse SearchProductsLocal(ProductSearchServiceRequest request)
            {
                var queryCriteria = request.QueryCriteria;

                if (queryCriteria.Context == null || queryCriteria.Context.ChannelId.HasValue == false)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "The channel identifier on the query criteria context must be specified.");
                }

                GetProductServiceRequest getProductsServiceRequest = new GetProductServiceRequest(
                    queryCriteria,
                    request.RequestContext.GetChannelConfiguration().DefaultLanguageId,
                    false /*FetchProductsAvailableInFuture*/,
                    request.QueryResultSettings,
                    queryCriteria.IsOnline);

                ProductSearchResultContainer result = request.RequestContext.Runtime.Execute<ProductSearchServiceResponse>(
                    getProductsServiceRequest,
                    request.RequestContext).ProductSearchResult;

                // populate the active variant product id if this was a search by item or barcode
                if ((!queryCriteria.ItemIds.IsNullOrEmpty() || !queryCriteria.Barcodes.IsNullOrEmpty()) && !queryCriteria.SkipVariantExpansion)
                {
                    SetActiveVariants(queryCriteria, result);
                }

                if (!queryCriteria.Refinement.IsNullOrEmpty())
                {
                    result.Results = ProductRefinement.RefineProducts(result.Results, queryCriteria.Refinement);
                }

                return new ProductSearchServiceResponse(result);
            }

            /// <summary>
            /// Get products based on the <paramref name="serviceRequest"/>.
            /// </summary>
            /// <param name="serviceRequest">The product retrieval request.</param>
            /// <returns>The product retrieval response.</returns>
            private static ProductSearchServiceResponse GetProducts(GetProductServiceRequest serviceRequest)
            {
                GetProductsHelper getProductsHandler = new GetProductsHelper(serviceRequest);
                return new ProductSearchServiceResponse(getProductsHandler.GetProducts());
            }

            private static ProductSearchServiceResponse SearchRemoteProducts(ProductSearchServiceRequest request)
            {
                ProductSearchResultContainer results = null;

                bool searchByProduct = request.QueryCriteria.Ids != null && request.QueryCriteria.Ids.Any();
                bool searchByCategory = request.QueryCriteria.CategoryIds != null && request.QueryCriteria.CategoryIds.Any();
                bool searchByKeyword = !string.IsNullOrEmpty(request.QueryCriteria.SearchCondition);

                if (searchByCategory && request.QueryCriteria.CategoryIds.HasMultiple())
                {
                    throw new NotSupportedException("Only a single category identifier can be specified when searching remotely.");
                }

                if (!(searchByProduct ^ searchByCategory ^ searchByKeyword))
                {
                    throw new NotSupportedException("When searching remotely you can search only by id, category or keyword.");
                }

                string attributeIds = string.Concat(RetailProductChannelProductAttributeId.ProductName, ",", RetailProductChannelProductAttributeId.Image);

                if (searchByProduct)
                {
                    GetProductDataRealtimeRequest getProductDataServiceRequest = new GetProductDataRealtimeRequest(request.QueryCriteria.Ids);
                    var getProductDataResponse = request.RequestContext.Execute<GetProductDataRealtimeResponse>(getProductDataServiceRequest);
                    var productsXml = getProductDataResponse.ProductDataXml;

                    var manager = new ProductDataManager(request.RequestContext);
                    manager.SaveProductData(productsXml);

                    // Checks if the downloaded product contains a kit product and retrieve the component and substitutes of the kit product.
                    RetrieveComponentProducts(request, productsXml);

                    // Reset the channel context since the remote product data has been saved locally.
                    request.QueryCriteria.Context.ChannelId = request.RequestContext.GetPrincipal().ChannelId;
                    request.QueryCriteria.Context.CatalogId = 0;

                    results = SearchProductsLocal(request).ProductSearchResult;
                }
                else if (searchByCategory)
                {
                    var getProductsByCategoryRequest = new GetRemoteProductsByCategoryRealtimeRequest(
                        request.QueryCriteria.Context.ChannelId.Value,
                        request.QueryCriteria.Context.CatalogId,
                        request.QueryCriteria.CategoryIds.First(),
                        attributeIds,
                        request.QueryCriteria.IncludeProductsFromDescendantCategories);

                    getProductsByCategoryRequest.QueryResultSettings = request.QueryResultSettings;

                    var response = request.RequestContext.Execute<ProductSearchRealtimeResponse>(getProductsByCategoryRequest);
                    results = response.ProductSearchResult;
                }
                else if (searchByKeyword)
                {
                    var getProductsByKeywordRequest = new GetRemoteProductsByKeywordRealtimeRequest(
                        request.QueryCriteria.Context.ChannelId.Value,
                        request.QueryCriteria.Context.CatalogId,
                        request.QueryCriteria.SearchCondition,
                        attributeIds);

                    getProductsByKeywordRequest.QueryResultSettings = request.QueryResultSettings;

                    var response = request.RequestContext.Execute<ProductSearchRealtimeResponse>(getProductsByKeywordRequest);
                    results = response.ProductSearchResult;
                }

                return new ProductSearchServiceResponse(results);
            }

            /// <summary>
            /// Checks if the downloaded product contains a kit product and retrieves the component and substitutes of the kit product.
            /// </summary>
            /// <param name="request">The product retrieval request.</param>
            /// <param name="productsXml">The product xml data remotely retrieved from AX.</param>
            private static void RetrieveComponentProducts(ProductSearchServiceRequest request, XDocument productsXml)
            {
                // For kit products, components and substitutes of the kit should also be downloaded from AX using transaction services.
                // Hence identify the list of components and substitutes of the kit and download the corresponding product data from AX.
                // This should be done before reading the virtual kit product from the store db.
                var components = productsXml.Descendants("RetailKitComponent");

                // if there are any components or substitutes of a kit then download the corresponding product data from AX and save it in local db.
                if (components.Any())
                {
                    var substitutes = productsXml.Descendants("SubstituteProduct");

                    List<long> ids = new List<long>();
                    foreach (var component in components)
                    {
                        ids.Add(long.Parse(component.Element("Component").Value));
                    }

                    foreach (var substitute in substitutes)
                    {
                        ids.Add(long.Parse(substitute.Value));
                    }

                    var manager = new ProductDataManager(request.RequestContext);

                    GetProductDataRealtimeRequest getProductDataServiceRequestForComponent = new GetProductDataRealtimeRequest(ids);
                    var getProductDataResponseForComponent = request.RequestContext.Execute<GetProductDataRealtimeResponse>(getProductDataServiceRequestForComponent);

                    var componentsXml = getProductDataResponseForComponent.ProductDataXml;
                    manager.SaveProductData(componentsXml);
                }
            }

            private static GetProductRefinersServiceResponse GetProductRefiners(GetProductRefinersRequest request)
            {
                return ProductRefinement.GetProductRefiners(request);
            }

            /// <summary>
            /// Set active variants.
            /// </summary>
            /// <param name="searchCriteria">The search criteria.</param>
            /// <param name="searchResult">The search result.</param>
            private static void SetActiveVariants(ProductSearchCriteria searchCriteria, ProductSearchResultContainer searchResult)
            {
                ThrowIf.Null(searchCriteria, "searchCriteria");
                ThrowIf.Null(searchResult, "searchResult");

                // Barcode searches are translated at the higher level into searches by item id. We'll ignore the barcode collection for now,
                // until we've added support for "native" retrieval of products by barcode.
                if (searchCriteria.ItemIds == null
                    || searchCriteria.ItemIds.Count == 0
                    || searchResult.Results.Count == 0)
                {
                    return;
                }

                // It is possible that the search criteria includes several invent dim ids of the same item id. In that case we
                // pick the first one as the active variant, and the client will have to traverse the composition information to
                // do a proper matching. However, that case should be rare. The more typical case when this code path is being hit
                // is the scanning of a barcode. Since we don't support batching of barcode-based searches, there will be exactly
                // one variant being returned.
                Dictionary<string, string> itemIdLookup = new Dictionary<string, string>();

                // pre-populate the lookup map
                foreach (var entry in searchCriteria.ItemIds)
                {
                    // skip over empty invent dim ids
                    if (string.IsNullOrWhiteSpace(entry.InventDimensionId))
                    {
                        continue;
                    }

                    if (!itemIdLookup.ContainsKey(entry.ItemId))
                    {
                        itemIdLookup.Add(entry.ItemId, entry.InventDimensionId);
                    }
                }

                // walk the results, setting the active variant as needed.
                foreach (var product in searchResult.Results)
                {
                    // skip over non-masters or those whose item id was not part of the search criteria
                    if (!product.IsMasterProduct
                        || !itemIdLookup.ContainsKey(product.ItemId))
                    {
                        continue;
                    }

                    product.CompositionInformation.VariantInformation.ActiveVariantProductId
                        = product.CompositionInformation.VariantInformation.IndexedInventoryDimensionIds[itemIdLookup[product.ItemId]];
                }
            }
        }
    }
}
