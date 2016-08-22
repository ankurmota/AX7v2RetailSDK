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
    namespace Commerce.Runtime.Services.PricingEngine.DiscountData
    {
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Class containing the discount line and the quantity of items required for that discount line for groups of line items.
        /// </summary>
        public class DiscountLineQuantity
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DiscountLineQuantity" /> class.
            /// </summary>
            /// <param name="discountLine">Discount line.</param>
            /// <param name="quantity">The quantity.</param>
            public DiscountLineQuantity(DiscountLine discountLine, decimal quantity)
            {
                this.DiscountLine = discountLine;
                this.Quantity = quantity;
            }
    
            /// <summary>
            /// Gets or sets the discount line that belongs to the discount.
            /// </summary>
            public DiscountLine DiscountLine { get; set; }
    
            /// <summary>
            /// Gets or sets the quantity of items required for this discount line.
            /// </summary>
            public decimal Quantity { get; set; }
        }
    }
}
