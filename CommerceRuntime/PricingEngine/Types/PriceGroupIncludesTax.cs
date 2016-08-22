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
    namespace Commerce.Runtime.Services.PricingEngine
    {
        /// <summary>
        /// Enumeration to capture whether a customer or price-group price has specified an override value for Price-includes-tax.
        /// </summary>
        internal enum PriceGroupIncludesTax
        {
            /// <summary>
            /// 0 if we don't know whether price group includes tax.
            /// </summary>
            NotSpecified = 0,
    
            /// <summary>
            /// 1 if the price group excludes tax from price.
            /// </summary>
            PriceExcludesTax = 1,
    
            /// <summary>
            /// 2 if the price group includes tax in price.
            /// </summary>
            PriceIncludesTax = 2
        }
    }
}
