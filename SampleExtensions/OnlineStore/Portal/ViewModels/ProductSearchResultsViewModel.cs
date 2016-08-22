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
    namespace Retail.Ecommerce.Web.Storefront.ViewModels
    {
        using Commerce.RetailProxy;

        /// <summary>
        /// View model representing the product search results.
        /// </summary>
        public class ProductSearchResultsViewModel : ViewModelBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ProductSearchResultsViewModel"/> class.
            /// </summary>
            /// <param name="searchText">The search text.</param>
            /// <param name="productSearchResults">The product search results.</param>
            public ProductSearchResultsViewModel(string searchText, PagedResult<ProductSearchResult> productSearchResults)
            {
                this.SearchText = searchText;
                this.ProductSearchResults = productSearchResults;
            }

            /// <summary>
            /// Gets the search text.
            /// </summary>
            /// <value>
            /// The search text.
            /// </value>
            public string SearchText { get; private set; }

            /// <summary>
            /// Gets the product search results.
            /// </summary>
            /// <value>
            /// The product search results.
            /// </value>
            public PagedResult<ProductSearchResult> ProductSearchResults { get; private set; }
        }
    }
}