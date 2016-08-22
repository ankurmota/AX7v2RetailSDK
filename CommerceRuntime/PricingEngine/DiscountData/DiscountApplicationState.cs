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
        using System.Collections;
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
    
        /// <summary>
        /// Class representing the state of the discount application process at a particular node of the possible discounts calculation tree.
        /// This class is used to store state information on the stack when the code proceeds down to the next level during the depth-first search of the tree
        /// so that it can be restored when the code pops back up to the level where this node occurred.
        /// </summary>
        internal class DiscountApplicationState
        {
            /// <summary>
            /// Gets or sets the remaining eligible discount applications at this node.
            /// </summary>
            public BitSet RemainingApplications { get; set; }
    
            /// <summary>
            /// Gets or sets the remaining line item quantities on the transaction at this node.
            /// </summary>
            public decimal[] RemainingQuantities { get; set; }
    
            /// <summary>
            /// Gets or sets the remaining line item quantities for compound on the transaction at this node.
            /// </summary>
            public decimal[] RemainingQuantitiesForCompound { get; set; }
    
            /// <summary>
            /// Gets or sets the counter value of which of the remaining applications we are processing at this node.
            /// </summary>
            public int AppliedDiscountApplication { get; set; }
    
            /// <summary>
            /// Gets or sets the value of the discounts applied up to this node.
            /// </summary>
            public decimal Value { get; set; }
    
            /// <summary>
            /// Gets or sets the number of times this discount was applied to these lines.
            /// </summary>
            public int NumberOfTimesApplied { get; set; }
        }
    }
}
