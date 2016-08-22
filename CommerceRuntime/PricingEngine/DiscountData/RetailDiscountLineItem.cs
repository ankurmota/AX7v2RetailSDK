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
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// This class contains the item index and discount line for a matching line within a discount application.
        /// </summary>
        public class RetailDiscountLineItem
        {
            /// <summary>
            /// Initializes a new instance of the RetailDiscountLineItem class.
            /// </summary>
            public RetailDiscountLineItem()
            {
            }
    
            /// <summary>
            /// Initializes a new instance of the RetailDiscountLineItem class, with the values set to the specified parameters.
            /// </summary>
            /// <param name="itemIndex">The ItemIndex value.</param>
            /// <param name="discountLine">The RetailDiscountLine value.</param>
            public RetailDiscountLineItem(int itemIndex, RetailDiscountLine discountLine)
            {
                this.ItemIndex = itemIndex;
                this.RetailDiscountLine = discountLine;
            }
    
            /// <summary>
            /// Gets or sets the index of the matching item.
            /// </summary>
            public int ItemIndex { get; set; }
    
            /// <summary>
            /// Gets or sets the discount line that the item belongs to for this application.
            /// </summary>
            public RetailDiscountLine RetailDiscountLine { get; set; }
        }
    }
}
