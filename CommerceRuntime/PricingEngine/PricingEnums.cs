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
        /// This indicates the method used to calculate the discount. Except for Least Expensive or Line-specific, these will define how
        /// a numeric discount value (defined elsewhere) is to be interpreted. E.g. if this is DiscountPercent, the value represents a
        /// percent off, if this is DealPrice, the value represents the price of the discounted item.
        /// </summary>
        public enum DiscountMethodType
        {
            /// <summary>
            /// Discount value is a price for the discounted item.
            /// </summary>
            DealPrice = 0,
    
            /// <summary>
            /// Discount value is a percent off the item's price.
            /// </summary>
            DiscountPercent = 1,
    
            /// <summary>
            /// Discount value is an amount off the item's price.
            /// </summary>
            DiscountAmount = 2,
    
            /// <summary>
            /// Discount amount off should be the price of the least expensive item on the discount.
            /// </summary>
            LeastExpensive = 3,
    
            /// <summary>
            /// The discount method is deferred to the discount line defined for each item.
            /// </summary>
            LineSpecific = 4,
    
            /// <summary>
            /// Discount value is a price for the discounted item on quantity discount.
            /// </summary>
            MultiplyDealPrice = 5,
    
            /// <summary>
            /// Discount value is a percent off the item's price on quantity discount.
            /// </summary>
            MultiplyDiscountPercent = 6
        }
    }
}
