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
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using Commerce.RetailProxy;
    
        /// <summary>
        /// Product Gallery View Model.
        /// </summary>
        public class ProductGalleryModel
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ProductGalleryModel" /> class.
            /// </summary>
            /// <param name="catId">Category that products are in.</param>
            /// <param name="inputtedList">List of products to show.</param>
            /// <param name="links">Breadcrumb Navigation Links.</param>
            /// <param name="mapping">Mapping of CategoryId to the name of the category.</param>
            /// <param name="filt">List of filters for products.</param>
            public ProductGalleryModel(long catId, IEnumerable<Product> inputtedList, Collection<CustomLink> links, Dictionary<long, string> mapping, Dictionary<string, string[]> filt)
            {
                this.CategoryId = catId;
                this.ProductList = inputtedList;
                this.BreadcrumbNavLinks = links;
                this.CategoryIdToNameMapping = mapping;
                this.Filters = filt;
            }
    
            /// <summary>
            /// Gets or sets List of products to show.
            /// </summary>
            public IEnumerable<Product> ProductList { get; set; }
    
            /// <summary>
            /// Gets or sets Category that products are in.
            /// </summary>
            public long CategoryId { get; set; }
    
            /// <summary>
            /// Gets or sets Breadcrumb Navigation Links.
            /// </summary>
            public Collection<CustomLink> BreadcrumbNavLinks { get; set; }
    
            /// <summary>
            /// Gets or sets a Mapping of CategoryId to the name of the category.
            /// </summary>
            public Dictionary<long, string> CategoryIdToNameMapping { get; set; }
    
            /// <summary>
            /// Gets or sets a list of filters for products.
            /// </summary>
            public Dictionary<string, string[]> Filters { get; set; }
        }
    }
}
