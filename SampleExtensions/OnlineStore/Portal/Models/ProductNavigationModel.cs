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
        /// Collection of top level categories.
        /// </summary>
        public class ProductNavigationModel
        {
            /// <summary>
            /// Collection of top level categories.
            /// </summary>
            private readonly Collection<ProductNavigationCategory> parentCategoriesValue = new Collection<ProductNavigationCategory>();
    
            /// <summary>
            /// Gets parent categories.
            /// </summary>
            public Collection<ProductNavigationCategory> ParentCategories
            {
                get
                {
                    return this.parentCategoriesValue;
                }
            }
        }
    }
}
