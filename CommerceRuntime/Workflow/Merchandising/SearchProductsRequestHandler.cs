/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

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
    namespace Commerce.Runtime.Workflow
    {
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// Retrieves the collection of products.
        /// </summary>
        public sealed class SearchProductsRequestHandler : SingleRequestHandler<ProductSearchRequest, ProductSearchResponse>
        {
            /// <summary>
            /// Sets the specified product prices on the product collection.
            /// </summary>
            /// <param name="productPrices">The product prices, calculated by the pricing service.</param>
            /// <param name="productMap">Products by product id.</param>
            /// <param name="productLookupIdMap">Product lookup map, including variant ids.</param>
            internal static void SetProductPrices(
                ReadOnlyCollection<ProductPrice> productPrices,
                Dictionary<long, Product> productMap,
                Dictionary<long, long> productLookupIdMap)
            {
                foreach (var price in productPrices)
                {
                    long targetProdId = price.ProductId;
                    long crtProdId;
                    Product crtProduct;
                    
                    if (!productLookupIdMap.TryGetValue(targetProdId, out crtProdId)
                        || !productMap.TryGetValue(productLookupIdMap[targetProdId], out crtProduct))
                    {
                        NetTracer.Warning("price data returned for unknown target product id {0}.", targetProdId);
                        continue;
                    }

                    ProductVariant crtVariant;
                    if (price.IsVariantPrice
                        && crtProduct.IsMasterProduct
                        && crtProduct.TryGetVariant(targetProdId, out crtVariant))
                    {
                        crtVariant.SetPrice(price);
                        continue;
                    }
    
                    crtProduct.SetPrice(price);
                }
            }
    
            /// <summary>
            /// Executes the workflow to retrieve products using the specified criteria.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override ProductSearchResponse Process(ProductSearchRequest request)
            {
                ThrowIf.Null(request, "request");
    
                if (request.QueryCriteria == null)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "The query criteria must be specified.");
                }
    
                if (request.QueryCriteria.Context == null || request.QueryCriteria.Context.ChannelId.HasValue == false)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "The channel identifier on the query criteria context must be specified.");
                }
    
                long currentChannelId = this.Context.GetPrincipal().ChannelId;
    
                var searchRequest = new ProductSearchServiceRequest(request.QueryCriteria, request.QueryResultSettings);
                ProductSearchServiceResponse searchResponse = this.Context.Runtime.Execute<ProductSearchServiceResponse>(searchRequest, this.Context);
    
                ProductSearchResultContainer results = searchResponse.ProductSearchResult;
                if (results.Results.Any() && !request.QueryCriteria.Context.IsRemoteLookup(currentChannelId))
                {
                    // retrieve and update prices
                    var priceRequest = new GetProductPricesServiceRequest(results.Results);
                    var priceResponse = this.Context.Execute<GetProductPricesServiceResponse>(priceRequest);
                    var productMap = new Dictionary<long, Product>(results.Results.Count);
    
                    foreach (var product in results.Results)
                    {
                        productMap[product.RecordId] = product;
                    }
    
                    // update prices on the products
                    SearchProductsRequestHandler.SetProductPrices(priceResponse.ProductPrices.Results, productMap, results.ProductIdLookupMap);
                }
    
                return new ProductSearchResponse(results);
            }
        }
    }
}
