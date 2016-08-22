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
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// This class finds all base prices for items and creates a base price price line
        ///  for each item line, keyed by item line line Id.
        /// </summary>
        internal class BasePriceCalculator : IPricingCalculator
        {
            /// <summary>
            /// Prevents a default instance of the BasePriceCalculator class from being created.
            /// </summary>
            private BasePriceCalculator()
            {
            }
    
            /// <summary>
            /// Factory method to get an instance of the base price calculator.
            /// </summary>
            /// <returns>Instance of a base price calculator.</returns>
            public static BasePriceCalculator CreateBasePriceCalculator()
            {
                return new BasePriceCalculator();
            }
    
            /// <summary>
            /// Implements the IPricingCalculator interface to calculate item base prices.
            /// </summary>
            /// <param name="salesLines">The item lines which need prices.</param>
            /// <param name="priceContext">The configuration of the overall pricing context for the calculation.</param>
            /// <param name="pricingDataManager">Instance of pricing data manager to access pricing data.</param>
            /// <returns>Sets of possible price lines keyed by item line Id.</returns>
            public Dictionary<string, IEnumerable<PriceLine>> CalculatePriceLines(
                IEnumerable<SalesLine> salesLines,
                PriceContext priceContext,
                IPricingDataAccessor pricingDataManager)
            {
                // Include item ID, product, and price here (even though we only need item ID and price, so that we match the cached entry from pulling the product).
                var priceDictionary = priceContext.ItemCache.ToDictionary(i => i.Key, i => i.Value.BasePrice, StringComparer.OrdinalIgnoreCase);
    
                var priceLines = new Dictionary<string, IEnumerable<PriceLine>>(StringComparer.OrdinalIgnoreCase);
    
                ChannelPriceConfiguration priceConfiguration = pricingDataManager.GetChannelPriceConfiguration();
                bool needCurrencyConversion = !string.Equals(priceConfiguration.CompanyCurrency, priceContext.CurrencyCode, StringComparison.OrdinalIgnoreCase);
    
                foreach (var salesLine in salesLines)
                {
                    if (!priceLines.ContainsKey(salesLine.LineId))
                    {
                        decimal price = 0;
                        decimal basePrice;
                        if (priceDictionary.TryGetValue(salesLine.ItemId, out basePrice))
                        {
                            if (needCurrencyConversion)
                            {
                                price = priceContext.CurrencyAndRoundingHelper.ConvertCurrency(priceConfiguration.CompanyCurrency, priceContext.CurrencyCode, basePrice);
                            }
                            else
                            {
                                price = basePrice;
                            }
                        }
    
                        var priceLine = new BasePriceLine
                        {
                            Value = price * GetUnitQuantityOfMeasure(salesLine),
                            PriceMethod = PriceMethod.Fixed,
                        };
    
                        priceLines.Add(salesLine.LineId, new BasePriceLine[1] { priceLine });
                    }
                }
    
                return priceLines;
            }
    
            private static decimal GetUnitQuantityOfMeasure(SalesLine salesLine)
            {
                return salesLine.UnitOfMeasureConversion != null
                           ? salesLine.UnitOfMeasureConversion.GetFactorForQuantity(salesLine.Quantity)
                           : 1m;
            }
        }
    }
}
