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
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// This class handles handles calculating the actual prices for each
        ///  item line based on the price lines for each item line line Id.
        /// </summary>
        internal static class PriceLineResolver
        {
            /// <summary>
            /// For the given item lines calculate and set their prices based on the set
            ///  of price lines provided (keyed by item line line Id).
            /// </summary>
            /// <param name="salesLines">Item lines to have their prices set.</param>
            /// <param name="priceLines">Set of price lines used to set the item line prices.</param>
            /// <param name="currencyAndRoundingHelper">Currency and rounding helper.</param>
            public static void ResolveAndApplyPriceLines(IEnumerable<SalesLine> salesLines, Dictionary<string, IEnumerable<PriceLine>> priceLines, ICurrencyOperations currencyAndRoundingHelper)
            {
                foreach (var sl in salesLines)
                {
                    IEnumerable<PriceLine> prices;
                    if (!priceLines.TryGetValue(sl.LineId, out prices))
                    {
                        prices = new PriceLine[0];
                    }
    
                    ResolveAndApplyPriceForSalesLine(sl, prices, currencyAndRoundingHelper);
                }
            }
    
            private static void ResolveAndApplyPriceForSalesLine(SalesLine item, IEnumerable<PriceLine> itemPriceLines, ICurrencyOperations currencyAndRoundingHelper)
            {
                var agreementLine = itemPriceLines.OfType<TradeAgreementPriceLine>().FirstOrDefault();
                var baseLine = itemPriceLines.OfType<BasePriceLine>().FirstOrDefault();
    
                bool hasTradeAgreementPrice = agreementLine != null;
                bool hasBasePrice = baseLine != null;
    
                item.AgreementPrice = hasTradeAgreementPrice ? agreementLine.Value : 0m;
                item.BasePrice = hasBasePrice ? baseLine.Value : 0m;
    
                // use the trade agreement price if any, otherwise use the base price
                if (hasTradeAgreementPrice)
                {
                    SetPriceOnSalesLine(item, item.AgreementPrice);
                    item.TradeAgreementPriceGroup = agreementLine.CustPriceGroup;
                }
                else if (hasBasePrice)
                {
                    SetPriceOnSalesLine(item, item.BasePrice);
                    item.AgreementPrice = item.BasePrice;
                }
                else
                {
                    SetPriceOnSalesLine(item, 0);
                }
    
                // now try to apply any price adjustments
                var adjustmentLines = itemPriceLines.OfType<PriceAdjustmentPriceLine>();
                item.AdjustedPrice = PriceAdjustmentCalculator.CalculatePromotionPrice(adjustmentLines, item.Price);
    
                if (Math.Abs(item.AdjustedPrice) < Math.Abs(item.Price))
                {
                    SetPriceOnSalesLine(item, item.AdjustedPrice);
                    item.TradeAgreementPriceGroup = null;
                }
    
                // round prices
                item.Price = currencyAndRoundingHelper.Round(item.Price);
                if (item.OriginalPrice.HasValue)
                {
                    item.OriginalPrice = currencyAndRoundingHelper.Round(item.OriginalPrice.Value);
                }
    
                item.BasePrice = currencyAndRoundingHelper.Round(item.BasePrice);
                item.AgreementPrice = currencyAndRoundingHelper.Round(item.AgreementPrice);
                item.AdjustedPrice = currencyAndRoundingHelper.Round(item.AdjustedPrice);
            }
    
            /// <summary>
            /// Set the price or original price depending on if price is overridden or keyed-in.
            /// </summary>
            /// <param name="line">Sales line to set price on.</param>
            /// <param name="priceToSet">Price to set.</param>
            private static void SetPriceOnSalesLine(SalesLine line, decimal priceToSet)
            {
                if (line.IsPriceOverridden)
                {
                    line.OriginalPrice = priceToSet;
                }
                else
                {
                    line.Price = priceToSet;
                }
            }
        }
    }
}
