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
        /// Result from a price lookup, contains both the Price value and whether or not that price definition has specified an value for 'Price includes tax'.
        /// </summary>
        internal struct PriceResult
        {
            /// <summary>
            /// Price value.
            /// </summary>
            public readonly decimal Price;
    
            /// <summary>
            /// Customer Price Group.
            /// </summary>
            public readonly string CustPriceGroup;
    
            /// <summary>
            /// Whether or not the price includes taxes.
            /// </summary>
            public readonly PriceGroupIncludesTax IncludesTax;
    
            /// <summary>
            /// Maximum retail price.
            /// </summary>
            public decimal MaximumRetailPriceIndia;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="PriceResult"/> struct.
            /// </summary>
            /// <param name="price">Price of the result.</param>
            /// <param name="includesTax">Does the result include tax.</param>
            /// <param name="maximumRetailPriceIndia">Maximum retail price.</param>
            /// <param name="custPriceGroup">Customer price group.</param>
            public PriceResult(decimal price, PriceGroupIncludesTax includesTax, decimal maximumRetailPriceIndia = decimal.Zero, string custPriceGroup = null)
            {
                this.Price = price;
                this.IncludesTax = includesTax;
                this.MaximumRetailPriceIndia = maximumRetailPriceIndia;
                this.CustPriceGroup = custPriceGroup;
            }
        }
    }
}
