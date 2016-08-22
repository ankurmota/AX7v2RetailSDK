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
        using System.Collections.ObjectModel;
        using System.Diagnostics.CodeAnalysis;
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;        

        /// <summary>
        /// This class finds all possible price adjustment lines for items and creates a set
        ///  of price adjustment price lines for each item line, keyed by item line line Id.
        /// </summary>
        internal class PriceAdjustmentCalculator : IPricingCalculator
        {
            /// <summary>
            /// Prevents a default instance of the PriceAdjustmentCalculator class from being created.
            /// </summary>
            private PriceAdjustmentCalculator()
            {
            }
    
            /// <summary>
            /// Factory method to get an instance of the price adjustment calculator.
            /// </summary>
            /// <returns>Instance of a price adjustment calculator.</returns>
            public static PriceAdjustmentCalculator CreatePriceAdjustmentCalculator()
            {
                return new PriceAdjustmentCalculator();
            }
    
            /// <summary>
            /// Implements the IPricingCalculator interface to calculate item price adjustment prices.
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
                // get pairs of product Id and distinct variant Id from lines to search by
                var productIds = salesLines
                    .Select(sl => new ItemUnit
                        {
                            ItemId = sl.ItemId,
                            VariantInventoryDimensionId = sl.InventoryDimensionId ?? string.Empty,
                            DistinctProductVariant = sl.Variant != null ? sl.Variant.DistinctProductVariantId : 0,
                            Product = sl.MasterProductId == 0 ? sl.ProductId : sl.MasterProductId
                        })
                    .Distinct().ToList();
    
                Tuple<DateTimeOffset, DateTimeOffset> dateRange = PricingEngine.GetMinAndMaxActiveDates(salesLines, priceContext.ActiveDate);
    
                // datetime to use depends on listing/cart scenario
                if (priceContext.PriceCalculationMode == PricingCalculationMode.Independent)
                {
                    dateRange = new Tuple<DateTimeOffset, DateTimeOffset>(dateRange.Item1, dateRange.Item2);
                }
    
                // fetch all price adjustments (and any related validation periods)
                ISet<string> allPriceGroups = PriceContextHelper.GetAllPriceGroupsForPrice(priceContext);
                IEnumerable<PriceAdjustment> adjustments = pricingDataManager.ReadPriceAdjustments(
                    productIds,
                    allPriceGroups,
                    dateRange.Item1,
                    dateRange.Item2,
                    QueryResultSettings.AllRecords) as IEnumerable<PriceAdjustment>;

                if (priceContext.IsDiagnosticsCollected && adjustments.Any())
                {
                    priceContext.PricingEngineDiagnosticsObject.AddPriceAdjustmentsConsidered(adjustments.Select(x => x.OfferId).ToList());
                }
    
                HashSet<string> offerIds = new HashSet<string>();
                foreach (PriceAdjustment adjustment in adjustments)
                {
                    offerIds.Add(adjustment.OfferId);
                }
    
                IDictionary<string, IList<PriceGroup>> adjustmentPriceGroupDictionary = PriceContextHelper.GetRetailDiscountPriceGroupDictionaryFilteredByTransaction(
                    pricingDataManager,
                    offerIds,
                    allPriceGroups);
    
                FixAdjustmentPriorityFromPriceGroups(adjustments, adjustmentPriceGroupDictionary);
    
                // "index" adjustments by item Id & dimensions
                var adjustmentDict = adjustments
                    .GroupBy(a => GetItemIdInventDimIdKey(a.ItemId, a.InventoryDimensionId))
                    .ToDictionary(grp => grp.Key, grp => grp.ToList(), StringComparer.OrdinalIgnoreCase);
    
                var promotionLines = new Dictionary<string, IList<PriceAdjustment>>(StringComparer.OrdinalIgnoreCase);
                foreach (var line in salesLines)
                {
                    // get item price adjustments, continue to next item if none found
                    var key = GetItemIdInventDimIdKey(line.ItemId, line.InventoryDimensionId);
    
                    List<PriceAdjustment> itemAdjustments;
                    if (!adjustmentDict.TryGetValue(key, out itemAdjustments))
                    {
                        continue;
                    }
    
                    List<PriceAdjustment> allApplicablePriceAdjustmentsForItem = new List<PriceAdjustment>();
                    foreach (var a in itemAdjustments)
                    {
                        IList<PriceGroup> priceGroups = null;
                        adjustmentPriceGroupDictionary.TryGetValue(a.OfferId, out priceGroups);
    
                        HashSet<string> priceGroupIdSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        priceGroupIdSet.AddRange(priceGroups.Select(p => p.GroupId));
                        if (PriceContextHelper.IsApplicableForPrice(priceContext, priceGroupIdSet, line.CatalogIds)
                            && IsAdjustmentActiveOnSalesLine(line, a, priceContext.ActiveDate)
                            && IsMatchUnitOfMeasure(line, a)
                            && IsMatchCurrency(priceContext.CurrencyCode, a))
                        {
                            allApplicablePriceAdjustmentsForItem.Add(a);
                        }
                    }
    
                    if (allApplicablePriceAdjustmentsForItem.Any())
                    {
                        // We apply price adjustment from only those with highest priority.
                        int highestPriority = allApplicablePriceAdjustmentsForItem.Max(p => p.PricingPriorityNumber);
                        IEnumerable<PriceAdjustment> priceAdjustmentsWithHighestPriorityForItem = allApplicablePriceAdjustmentsForItem.Where(p => p.PricingPriorityNumber == highestPriority);
    
                        // filter price adjustments by active validation periods before adding to dictionary
                        foreach (var a in priceAdjustmentsWithHighestPriorityForItem)
                        {
                            IList<PriceAdjustment> promos;
                            if (!promotionLines.TryGetValue(line.LineId, out promos))
                            {
                                promos = new List<PriceAdjustment>();
                                promotionLines.Add(line.LineId, promos);
                            }
    
                            promos.Add(a);
                        }
                    }
                }
    
                // convert price adjustments to price lines for the sales lines
                var priceLines = PriceAdjustmentsToPriceLines(salesLines, promotionLines);
    
                return priceLines;
            }
    
            internal static IEnumerable<PriceAdjustmentPriceLine> PriceAdjustmentsToPriceLines(IEnumerable<PriceAdjustment> priceAdjustments)
            {
                var promoPrices = new List<PriceAdjustmentPriceLine>();
    
                foreach (var promo in priceAdjustments)
                {
                    var promoLine = new PriceAdjustmentPriceLine();
                    promoLine.OriginId = promo.OfferId;
                    promoLine.Concurrency = promo.ConcurrencyMode;
                    promoLine.IsCompoundable = false;
    
                    switch (promo.DiscountMethod)
                    {
                        case DiscountOfferMethod.DiscountPercent:
                            promoLine.Value = promo.DiscountPercent;
                            promoLine.PriceMethod = PriceMethod.PercentOff;
                            promoLine.IsCompoundable = true;
                            promoPrices.Add(promoLine);
                            break;
    
                        case DiscountOfferMethod.OfferPrice:
                            promoLine.Value = promo.OfferPrice;
                            promoLine.PriceMethod = PriceMethod.Fixed;
                            promoPrices.Add(promoLine);
    
                            break;
    
                        case DiscountOfferMethod.DiscountAmount:
                            promoLine.Value = promo.DiscountAmount;
                            promoLine.PriceMethod = PriceMethod.AmountOff;
                            promoLine.IsCompoundable = true;
                            promoPrices.Add(promoLine);
                            break;
                    }
                }
    
                return promoPrices;
            }
    
            /// <summary>
            /// Given a set of promotion lines, tentative item price, and item, calculate the price after promotions are applied.
            /// </summary>
            /// <param name="priceLines">List of possible adjustments and methods active for this item.</param>
            /// <param name="price">Price of the item before the promotion, derived from trade agreement or base item price.</param>
            /// <returns>
            /// Unrounded price after applying all promotions.
            /// </returns>
            internal static decimal CalculatePromotionPrice(IEnumerable<PriceAdjustmentPriceLine> priceLines, decimal price)
            {
                if (priceLines == null || !priceLines.Any())
                {
                    return price;
                }
    
                decimal promoPrice = price;
                IList<PromoPrice> promoPrices = new List<PromoPrice>();
    
                foreach (var promo in priceLines)
                {
                    PromoPrice promoLine = new PromoPrice();
                    promoLine.PromoId = promo.OriginId;
                    promoLine.Concurrency = promo.Concurrency;
                    promoLine.IsCompoundable = false;
    
                    switch (promo.PriceMethod)
                    {
                        case PriceMethod.PercentOff:
                            promoLine.PercentOff = promo.Value;
                            promoLine.IsCompoundable = true;
                            break;
    
                        case PriceMethod.Fixed:
                            promoLine.AmountOff = Math.Max(0m, price - promo.Value);
                            break;
    
                        case PriceMethod.AmountOff:
                            promoLine.AmountOff = promo.Value;
                            promoLine.IsCompoundable = true;
                            break;
    
                        default:
                            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Behavior not defined for price method: {0}", promo.PriceMethod));
                    }
    
                    promoPrices.Add(promoLine);
                }
    
                promoPrice = price - FindConcurrentPromoAmount(promoPrices, price);
    
                return promoPrice;
            }
    
            private static void FixAdjustmentPriorityFromPriceGroups(
                IEnumerable<PriceAdjustment> adjustments,
                IDictionary<string, IList<PriceGroup>> offerIdToPriceGroupsLookup)
            {
                foreach (PriceAdjustment priceAdjustment in adjustments)
                {
                    if (priceAdjustment.PricingPriorityNumber <= 0)
                    {
                        IList<PriceGroup> priceGroups = null;
                        if (offerIdToPriceGroupsLookup.TryGetValue(priceAdjustment.OfferId, out priceGroups))
                        {
                            if (priceGroups.Any())
                            {
                                // Ideally, we should have a new field like EffectivePricingPriorityNumber. This is good enough for our purpose.
                                priceAdjustment.PricingPriorityNumber = Math.Max(0, priceGroups.Max(p => p.PricingPriorityNumber));
                            }
                        }
                    }
                }
            }
    
            private static string GetItemIdInventDimIdKey(string itemId, string inventDimId)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}-@-{1}", itemId, inventDimId);
            }
    
            private static bool IsMatchUnitOfMeasure(SalesLine line, PriceAdjustment adjustment)
            {
                bool isMatch = string.IsNullOrWhiteSpace(adjustment.UnitOfMeasure);
    
                if (!isMatch)
                {
                    isMatch = string.Equals(adjustment.UnitOfMeasure, line.SalesOrderUnitOfMeasure, StringComparison.OrdinalIgnoreCase);
                }
    
                return isMatch;
            }
    
            private static bool IsMatchCurrency(string storeCurrencyCode, PriceAdjustment adjustment)
            {
                return string.IsNullOrEmpty(adjustment.CurrencyCode) ? false : string.Equals(storeCurrencyCode, adjustment.CurrencyCode, StringComparison.OrdinalIgnoreCase);
            }
    
            private static bool IsAdjustmentActiveOnSalesLine(
                SalesLine line,
                PriceAdjustment adjustment,
                DateTimeOffset defaultDate)
            {
                var activeDate = line.SalesDate ?? defaultDate;
    
                return InternalValidationPeriod.ValidateDateAgainstValidationPeriod(
                    (DateValidationType)adjustment.DateValidationType,
                    adjustment.ValidationPeriod,
                    adjustment.ValidFromDate,
                    adjustment.ValidToDate,
                    activeDate);
            }
    
            /// <summary>
            /// Find the total promotion discount amount taken off the item price.
            /// </summary>
            /// <param name="promoPrices">List of promotion prices to consider.</param>
            /// <param name="price">Tentative price of the item before promotions are applied.</param>
            /// <returns>The final promotion amount to remove from given price.</returns>
            private static decimal FindConcurrentPromoAmount(IEnumerable<PromoPrice> promoPrices, decimal price)
            {
                Func<PromoPrice, decimal> overallPromotionAmount =
                    p => Math.Max(p.AmountOff, p.PercentOff * price / 100);
    
                IEnumerable<PromoPrice> activePromotions = new List<PromoPrice>();
                decimal totalPromoAmount = 0;
    
                if (promoPrices.Any())
                {
                    var exclusivePromos = promoPrices.Where(p => p.Concurrency == ConcurrencyMode.Exclusive);
                    if (exclusivePromos.Any())
                    {
                        // if there are exclusive promotions, take only the best exclusive promotion
                        activePromotions = exclusivePromos.OrderByDescending(overallPromotionAmount).Take(1);
                    }
                    else if (promoPrices.Any(p => p.Concurrency == ConcurrencyMode.BestPrice))
                    {
                        // if there are best price promotions, find only the best price from all discounts (i.e. from best price and compounded)
                        activePromotions = promoPrices.OrderByDescending(overallPromotionAmount).Take(1);
                    }
                    else
                    {
                        // otherwise, apply compounded promotions, taking the best non-compoundable promotion first
                        activePromotions = promoPrices.Where(p => !p.IsCompoundable)
                            .OrderByDescending(overallPromotionAmount).Take(1)
                            .Concat(promoPrices.Where(p => p.IsCompoundable));
                    }
    
                    // combine all active promotions
                    totalPromoAmount = CombinePromotionPrices(activePromotions, price);
                }
    
                return totalPromoAmount;
            }
    
            /// <summary>
            /// Given a list of promotion prices, crunch them down into a single discounted amount.
            /// </summary>
            /// <param name="promoPrices">Prices to crunch.</param>
            /// <param name="price">Tentative item price before promotions.</param>
            /// <returns>Gives the overall discounted amount from collection of adjustments for given price.</returns>
            private static decimal CombinePromotionPrices(IEnumerable<PromoPrice> promoPrices, decimal price)
            {
                decimal totalAmount = 0;
    
                // only first non-compoundable promotion will be applied
                if (promoPrices.Any(p => !p.IsCompoundable))
                {
                    PromoPrice nonCompoundable = promoPrices.Where(p => !p.IsCompoundable).First();
                    totalAmount = Math.Max(nonCompoundable.AmountOff, nonCompoundable.PercentOff * price / 100);
                }
    
                // apply compoundable promotions in order of offer Id, $ off first and % off last.
                foreach (var promo in promoPrices.Where(p => p.IsCompoundable).OrderBy(p => p.AmountOff * (-1m)))
                {
                    // otherwise add the amount or percent to the total amount discounted
                    if (promo.AmountOff != 0)
                    {
                        totalAmount += promo.AmountOff;
                    }
                    else if (promo.PercentOff != 0)
                    {
                        totalAmount += (price - totalAmount) * promo.PercentOff / 100;
                    }
                }
    
                // don't discount more than the price
                if (totalAmount > price)
                {
                    totalAmount = price;
                }
    
                return totalAmount;
            }
    
            private static Dictionary<string, IEnumerable<PriceLine>> PriceAdjustmentsToPriceLines(
                IEnumerable<SalesLine> salesLines,
                IDictionary<string, IList<PriceAdjustment>> priceAdjustments)
            {
                var itemPriceLines = new Dictionary<string, IEnumerable<PriceLine>>(StringComparer.OrdinalIgnoreCase);
    
                foreach (var item in salesLines)
                {
                    IList<PriceAdjustment> itemPriceAdjustments;
                    if (!priceAdjustments.TryGetValue(item.LineId, out itemPriceAdjustments))
                    {
                        itemPriceAdjustments = new PriceAdjustment[0];
                    }
    
                    var promoPrices = PriceAdjustmentsToPriceLines(itemPriceAdjustments);
    
                    // add set of price lines to the item map
                    if (!itemPriceLines.ContainsKey(item.LineId))
                    {
                        itemPriceLines.Add(item.LineId, promoPrices);
                    }
                }
    
                return itemPriceLines;
            }
    
            /// <summary>
            /// Struct to hold related fields for defining a promotion price. These are
            ///  processed by the promotion concurrency code.
            /// </summary>
            private struct PromoPrice
            {
                /// <summary>
                /// Offer Id of the promotion, used to sort compounded promos.
                /// </summary>
                public string PromoId;
    
                /// <summary>
                /// Amount off given by the promotion.
                /// </summary>
                public decimal AmountOff;
    
                /// <summary>
                /// Percent off given by the promotion.
                /// </summary>
                public decimal PercentOff;
    
                /// <summary>
                /// Concurrency setting of the promotion.
                /// </summary>
                public ConcurrencyMode Concurrency;
    
                /// <summary>
                /// Whether the current promotion can be compounded. If this was derived from an offer price,
                ///   it can not be compounded, but the best can be chosen before compounding more amounts and percent.
                /// </summary>
                public bool IsCompoundable;
            }
        }
    }
}
