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
    namespace Retail.Ecommerce.Sdk.Core
    {
        /// <summary>
        /// Centralized location for managing all of the constants.
        /// </summary>
        public static class CommonConstants
        {
            /// <summary>
            /// Represents the alternate text of the default product image.
            /// </summary>
            public const string DefaultProductImageAltText = "Product not available.";

            /// <summary>
            /// Represents the url of the default product image.
            /// </summary>
            public const string DefaultProductImageUrl = @"Products/Unavailable_Product_lrg.png";

            /// <summary>
            /// Represent the url of an inactive link.
            /// </summary>
            public const string InactiveLinkUrl = "#";
        }
    }
}
