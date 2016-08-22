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
    
        /// <summary>
        /// A specific top-level category in the product navigation, it contains a parent category link and then a list of links to all of its child categories.
        /// </summary>
        public class ProductNavigationCategory
        {
            /// <summary>
            /// Gets or sets Actual Collection of children categories.
            /// </summary>
            private readonly Collection<CustomLink> childCategoriesValue = new Collection<CustomLink>();
    
            /// <summary>
            /// Initializes a new instance of the <see cref="ProductNavigationCategory" /> class.
            /// </summary>
            public ProductNavigationCategory()
            {
            }
    
            /// <summary>
            /// Gets or sets Link to parent category.
            /// </summary>
            public CustomLink ParentCategory { get; set; }
    
            /// <summary>
            /// Gets list of children categories.
            /// </summary>
            public Collection<CustomLink> ChildCategories
            {
                get
                {
                    return this.childCategoriesValue;
                }
            }
        }
    }
}
