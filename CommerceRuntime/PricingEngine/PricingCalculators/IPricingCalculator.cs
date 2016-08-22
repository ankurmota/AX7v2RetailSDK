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
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// This interface defines a method which will calculate prices for all given
        ///  sales lines and return the price lines keyed by sales line Id.
        /// </summary>
        internal interface IPricingCalculator
        {
            /// <summary>
            /// This method will calculate the prices for all given item lines
            ///  and return the price lines for each item line, keyed by the item line Id.
            /// </summary>
            /// <param name="salesLines">The item lines which need prices.</param>
            /// <param name="priceContext">The configuration of the overall pricing context for the calculation.</param>
            /// <param name="pricingDataManager">Instance of pricing data manager to access pricing data.</param>
            /// <returns>Sets of possible price lines keyed by item line Id.</returns>
            Dictionary<string, IEnumerable<PriceLine>> CalculatePriceLines(
                IEnumerable<SalesLine> salesLines,
                PriceContext priceContext,
                IPricingDataAccessor pricingDataManager);
        }
    }
}
