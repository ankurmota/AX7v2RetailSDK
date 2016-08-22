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
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Retrieves the collection of products.
        /// </summary>
        public sealed class GetChangedProductsRequestHandler : SingleRequestHandler<ChangedProductsSearchRequest, ChangedProductsSearchResponse>
        {
            /// <summary>
            /// Executes the workflow to retrieve changed products.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override ChangedProductsSearchResponse Process(ChangedProductsSearchRequest request)
            {
                ThrowIf.Null(request, "request");
    
                if (request.RequestForChanges == null)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "The query criteria must be specified.");
                }
    
                long channelId = this.Context.GetPrincipal().ChannelId;
                if (request.RequestForChanges.Context.IsRemoteLookup(channelId))
                {
                    string message = string.Format(
                        CultureInfo.InvariantCulture,
                        "The specified context (Channel={0}, Catalog={1}) is not supported when retrieving changed products.",
                        request.RequestForChanges.Context.ChannelId,
                        request.RequestForChanges.Context.CatalogId);
    
                    throw new NotSupportedException(message);
                }
    
                GetChangedProductsDataRequest dataRequest = new GetChangedProductsDataRequest(request.RequestForChanges, request.QueryResultSettings);
                var products = this.Context.Runtime.Execute<SingleEntityDataServiceResponse<ChangedProductsSearchResult>>(dataRequest, this.Context).Entity;
    
                if (products.Results.Count > 0)
                {
                    // retrieve and update prices
                    var priceRequest = new GetProductPricesServiceRequest(products.Results);
                    var priceResponse = this.Context.Execute<GetProductPricesServiceResponse>(priceRequest);
                    var productMap = new Dictionary<long, Product>(products.Results.Count);
                    var productIdLookupMap = new Dictionary<long, long>();
    
                    foreach (var product in products.Results)
                    {
                        productMap[product.RecordId] = product;
                        productIdLookupMap[product.RecordId] = product.RecordId;
                        if (product.IsMasterProduct)
                        {
                            foreach (var variant in product.GetVariants())
                            {
                                productIdLookupMap[variant.DistinctProductVariantId] = product.RecordId;
                            }
                        }
                    }
    
                    // update prices on the products
                    SearchProductsRequestHandler.SetProductPrices(priceResponse.ProductPrices.Results, productMap, productIdLookupMap);
                }
    
                return new ChangedProductsSearchResponse(products);
            }
        }
    }
}
