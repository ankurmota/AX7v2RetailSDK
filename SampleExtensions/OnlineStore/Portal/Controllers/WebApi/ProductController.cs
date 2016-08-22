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
    namespace Retail.Ecommerce.Web.Storefront.Controllers
    {
        using System;
        using System.Collections.Generic;
        using System.Threading.Tasks;
        using System.Web.Mvc;
        using Commerce.RetailProxy;
        using Retail.Ecommerce.Sdk.Core;
        using Retail.Ecommerce.Sdk.Core.OperationsHandlers;

        /// <summary>
        /// Product Controller.
        /// </summary>
        public class ProductController : WebApiControllerBase
        {
            /// <summary>
            /// Gets the products.
            /// </summary>
            /// <param name="productIds">The product ids.</param>
            /// <returns>Response containing simple products.</returns>
            public async Task<ActionResult> GetSimpleProducts(IEnumerable<long> productIds)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                ProductOperationsHandler productOperationsHandler = new ProductOperationsHandler(ecommerceContext);
                IEnumerable<SimpleProduct> simpleProducts = await productOperationsHandler.GetSimpleProducts(productIds);

                return this.Json(simpleProducts);
            }

            /// <summary>
            /// Gets the active prices of the specified products.
            /// </summary>
            /// <param name="recordIds">Products' record IDs.</param>
            /// <param name="catalogId">The catalog ID.</param>
            /// <param name="affiliationLoyaltyTierIds">Affiliation Loyalty Tier IDs.</param>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>The prices.</returns>
            public async Task<ActionResult> GetActivePrices(IEnumerable<long> recordIds, long? catalogId, IEnumerable<long> affiliationLoyaltyTierIds, QueryResultSettings queryResultSettings)
            {
                if (queryResultSettings == null)
                {
                    throw new ArgumentNullException(nameof(queryResultSettings));
                }

                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                ProductOperationsHandler productOperationsHandler = new ProductOperationsHandler(ecommerceContext);
                PagedResult<ProductPrice> products = await productOperationsHandler.GetActivePrices(recordIds, catalogId, affiliationLoyaltyTierIds, queryResultSettings);

                return this.Json(products.Results);
            }

            /// <summary>
            /// Gets the product availability.
            /// </summary>
            /// <param name="productIds">The product ids.</param>
            /// <param name="channelId">The channel identifier.</param>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>Response containing available quantities for the listings inquired.</returns>
            public async Task<ActionResult> GetProductAvailability(IEnumerable<long> productIds, long channelId, QueryResultSettings queryResultSettings)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                ProductOperationsHandler productOperationsHandler = new ProductOperationsHandler(ecommerceContext);
                PagedResult<ProductAvailableQuantity> productAvailableQuantities = await productOperationsHandler.GetProductAvailability(productIds, channelId, queryResultSettings);

                return this.Json(productAvailableQuantities.Results);
            }
        }
    }
}