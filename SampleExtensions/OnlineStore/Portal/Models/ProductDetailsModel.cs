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
    namespace Retail.Ecommerce.Web.Storefront.Models
    {
        using System.Collections.ObjectModel;
        using Commerce.RetailProxy;
    
        /// <summary>
        /// View Model for Product Details page.
        /// </summary>
        public class ProductDetailsModel
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ProductDetailsModel" /> class.
            /// </summary>
            /// <param name="p">Product the page is for.</param>
            /// <param name="links">Navigation breadcrumb links.</param>
            public ProductDetailsModel(Product p, Collection<CustomLink> links)
            {
                this.Product = p;
                this.BreadcrumbNavLinks = links;
            }
    
            /// <summary>
            /// Gets or sets Product page is showing details of.
            /// </summary>
            public Product Product { get; set; }
    
            /// <summary>
            /// Gets or sets Links for navigation breadcrumb.
            /// </summary>
            public Collection<CustomLink> BreadcrumbNavLinks { get; set; }
        }
    }
}
