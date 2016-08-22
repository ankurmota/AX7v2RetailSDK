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
        using System;
        using System.Collections.ObjectModel;
        using Commerce.RetailProxy;
    
        /// <summary>
        /// This is a way to showcase selected products in a recommendation bar.
        /// </summary>
        public class SelectedProductGalleryModel2
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SelectedProductGalleryModel2" /> class.
            /// </summary>
            /// <param name="products">Products to be in the selected product gallery.</param>
            /// <param name="heading">Heading for selected product gallery.</param>
            public SelectedProductGalleryModel2(Collection<Product> products, string heading)
            {
                this.Products = products;
                this.Heading = heading;
            }
    
            /// <summary>
            /// Gets or sets products to be in the selected product gallery.
            /// </summary>
            public Collection<Product> Products { get; set; }
    
            /// <summary>
            /// Gets or sets heading for selected product gallery.
            /// </summary>
            public string Heading { get; set; }
        }
    }
}
