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
        using System.Collections.Generic;
        using System.Threading.Tasks;
        using System.Web.Mvc;
        using Commerce.RetailProxy;
        using Retail.Ecommerce.Sdk.Core;
        using Retail.Ecommerce.Sdk.Core.OperationsHandlers;
        using Retail.Ecommerce.Web.Storefront.ViewModels;

        /// <summary>
        /// The Products Controller.
        /// </summary>
        public class ProductsController : ActionControllerBase
        {
            private const string ProductSearchResultsViewName = "SearchResults";

            /// <summary>
            /// Get the search results.
            /// </summary>
            /// <param name="searchText">The search text.</param>
            /// <returns>
            /// The View for that product.
            /// </returns>
            public async Task<ActionResult> Search(string searchText)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                ProductOperationsHandler productOperationsHandler = new ProductOperationsHandler(ecommerceContext);

                string sanitizedSearchText = Utilities.GetSanitizedSearchText(searchText);
                PagedResult<ProductSearchResult> productSearchResults = await productOperationsHandler.SearchByText(sanitizedSearchText, catalogId: 0, queryResultSettings: Utilities.DefaultQuerySettings);
                await PopulateProductInfo(productSearchResults, ecommerceContext);

                ProductSearchResultsViewModel viewModel = new ProductSearchResultsViewModel(sanitizedSearchText, productSearchResults);
                
                return this.View(ProductsController.ProductSearchResultsViewName, viewModel);
            }

            private static async Task PopulateProductInfo(IEnumerable<ProductSearchResult> productSearchResults, EcommerceContext ecommerceContext)
            {
                foreach (var productResult in productSearchResults)
                {
                    string extensionPropertyValue = await Utilities.ToCurrencyString((decimal)productResult.Price, ecommerceContext);
                    productResult.ExtensionProperties.SetPropertyValue("FormattedPrice", ExtensionPropertyTypes.String, extensionPropertyValue);
                }
            }
        }
    }
}