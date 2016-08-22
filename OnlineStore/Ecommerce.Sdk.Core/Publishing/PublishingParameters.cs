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
    namespace Retail.Ecommerce.Sdk.Core.Publishing
    {
        using System.Collections.Generic;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Encapsulates set of properties required for publishing.
        /// </summary>
        public class PublishingParameters
        {
            /// <summary>
            /// Gets Channel Categories.
            /// </summary>
            public IEnumerable<Category> Categories
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets map of categories and their attributes.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "The nesting naturally fits into the business purpose of this method.")]
            public Dictionary<long, IEnumerable<AttributeCategory>> CategoriesAttributes
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets Product Attributes.
            /// </summary>
            public IEnumerable<AttributeProduct> ProductsAttributes
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets Channel's default culture.
            /// </summary>
            public CultureInfo ChannelDefaultCulture
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets Gift Card Item ID.
            /// </summary>
            public string GiftCartItemId
            {
                get;
                internal set;
            }
        }
    }
}
