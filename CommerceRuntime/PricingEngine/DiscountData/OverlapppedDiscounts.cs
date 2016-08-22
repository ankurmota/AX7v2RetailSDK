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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// Overlapped discounts.
        /// </summary>
        /// <remarks>See BuildOverlappedDiscountsGroup method.</remarks>
        internal class OverlapppedDiscounts
        {
            internal OverlapppedDiscounts(int itemGroupIndex, DiscountBase discount)
            {
                this.OverlapId = Guid.NewGuid();
                this.MixAndMatchAndQuantityDiscounts = new Dictionary<string, DiscountBase>(StringComparer.OrdinalIgnoreCase);
                this.MixAndMatchAndQuantityDiscounts.Add(discount.OfferId, discount);
                this.OfferDiscounts = new Dictionary<string, DiscountBase>(StringComparer.OrdinalIgnoreCase);
                this.CoveredItemGroupIndexSet = new HashSet<int>() { itemGroupIndex };
            }
    
            internal Guid OverlapId { get; private set; }
    
            /// <summary>
            /// Gets the mix and match discounts and quantity discounts.
            /// </summary>
            /// <remarks>
            /// Mix and match and quantity discounts are the master of each group of overlapped discounts.
            /// </remarks>
            internal Dictionary<string, DiscountBase> MixAndMatchAndQuantityDiscounts { get; private set; }
    
            /// <summary>
            /// Gets the offer discounts.
            /// </summary>
            /// <remarks>
            /// One offer discount can be in multiple groups of overlapped discounts because each item is independent.
            /// </remarks>
            internal Dictionary<string, DiscountBase> OfferDiscounts { get; private set; }
    
            /// <summary>
            /// Gets the covered item group index set.
            /// </summary>
            /// <remarks>
            /// We may split items in one transaction into multiple groups of overlapped discounts.
            /// </remarks>
            internal HashSet<int> CoveredItemGroupIndexSet { get; private set; }
    
            /// <summary>
            /// Splits all discounts into multiple groups of overlapped discounts with each group non-overlapping with each other.
            /// </summary>
            /// <param name="possibleDiscounts">Possible discounts.</param>
            /// <returns>A collection of overlapped discounts.</returns>
            /// <remarks>By isolating discounts into groups, we can calculate and optimize discounts for each group.</remarks>
            internal static IEnumerable<OverlapppedDiscounts> BuildOverlappedDiscountsGroup(List<DiscountBase> possibleDiscounts)
            {
                // DISCOUNTPERF: add filtering by remaining quantities
                // Build lookup for mix and match and quantity discounts. Ignore offer discounts for now.
                Dictionary<int, OverlapppedDiscounts> itemGroupIndexToOverlapppedDiscountsLookup = new Dictionary<int, OverlapppedDiscounts>();
                foreach (DiscountBase discount in possibleDiscounts)
                {
                    if (discount is MixAndMatchDiscount || discount is MultipleBuyDiscount)
                    {
                        OverlapppedDiscounts myOverlapppedDiscounts = null;
                        foreach (KeyValuePair<int, HashSet<decimal>> pair in discount.ItemGroupIndexToDiscountLineNumberSetMap)
                        {
                            int itemGroupIndex = pair.Key;
                            OverlapppedDiscounts overlappedDiscounts = null;
                            if (itemGroupIndexToOverlapppedDiscountsLookup.TryGetValue(itemGroupIndex, out overlappedDiscounts))
                            {
                                // Item covered in an overlappedDiscounts already.
                                if (myOverlapppedDiscounts == null)
                                {
                                    // myOverlapppedDiscounts not set yet, add current discount to it.
                                    myOverlapppedDiscounts = overlappedDiscounts;
                                    myOverlapppedDiscounts.MixAndMatchAndQuantityDiscounts.Add(discount.OfferId, discount);
                                }
                                else if (myOverlapppedDiscounts.OverlapId != overlappedDiscounts.OverlapId)
                                {
                                    // Item covered in a different overlappedDiscounts, myOverlapppedDiscounts acquires it.
                                    myOverlapppedDiscounts.Aquire(itemGroupIndexToOverlapppedDiscountsLookup, overlappedDiscounts);
                                    itemGroupIndexToOverlapppedDiscountsLookup[itemGroupIndex] = myOverlapppedDiscounts;
                                }
                            }
                            else
                            {
                                // Item not covered yet.
                                if (myOverlapppedDiscounts == null)
                                {
                                    // New overlappedDiscounts.
                                    myOverlapppedDiscounts = new OverlapppedDiscounts(itemGroupIndex, discount);
                                }
                                else
                                {
                                    // Add item to the overlappedDiscounts.
                                    myOverlapppedDiscounts.CoveredItemGroupIndexSet.Add(itemGroupIndex);
                                }
    
                                itemGroupIndexToOverlapppedDiscountsLookup[itemGroupIndex] = myOverlapppedDiscounts;
                            }
                        }
                    }
                }
    
                // Add offer discounts to the lookup.
                foreach (DiscountBase discount in possibleDiscounts)
                {
                    if (discount is OfferDiscount)
                    {
                        foreach (KeyValuePair<int, HashSet<decimal>> pair in discount.ItemGroupIndexToDiscountLineNumberSetMap)
                        {
                            int itemGroupIndex = pair.Key;
                            OverlapppedDiscounts overlappedDiscounts = null;
                            if (itemGroupIndexToOverlapppedDiscountsLookup.TryGetValue(itemGroupIndex, out overlappedDiscounts))
                            {
                                overlappedDiscounts.OfferDiscounts[discount.OfferId] = discount;
                            }
                        }
                    }
                }
    
                // Convert itemGroupIndexToOverlapppedDiscountsLookup to List<OverlapppedDiscounts>.
                HashSet<Guid> overlappedIdSet = new HashSet<Guid>();
                List<OverlapppedDiscounts> overlappedDiscountsList = new List<OverlapppedDiscounts>();
                foreach (KeyValuePair<int, OverlapppedDiscounts> pair in itemGroupIndexToOverlapppedDiscountsLookup)
                {
                    OverlapppedDiscounts overlappedDiscounts = pair.Value;
    
                    if (!overlappedIdSet.Contains(overlappedDiscounts.OverlapId))
                    {
                        overlappedIdSet.Add(overlappedDiscounts.OverlapId);
                        overlappedDiscountsList.Add(overlappedDiscounts);
                    }
                }
    
                return overlappedDiscountsList;
            }
    
            internal bool IsOkayForMixAndMatchOneLineGroupOptimizationAndFillupValueLookups(
                Dictionary<int, decimal> mixAndMatchRelativeValueLookup,
                Dictionary<int, OfferDiscount> simpleDiscountOfferLookup,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities)
            {
                // Special optimization for this overlapped group when there is
                //   only one mix and match 
                //   with only one line group
                //   with any of deal price, $ off, % off, or least expensive $-off.
                bool isOkay = this.MixAndMatchAndQuantityDiscounts.Count == 1;
    
                MixAndMatchDiscount mixAndMatchDiscount = null;
                if (isOkay)
                {
                    mixAndMatchDiscount = this.MixAndMatchAndQuantityDiscounts.First().Value as MixAndMatchDiscount;
    
                    isOkay = mixAndMatchDiscount != null;
                }
    
                if (isOkay)
                {
                    isOkay = mixAndMatchDiscount.DiscountType == DiscountMethodType.DealPrice ||
                        mixAndMatchDiscount.DiscountType == DiscountMethodType.DiscountPercent ||
                        mixAndMatchDiscount.DiscountType == DiscountMethodType.DiscountAmount ||
                        mixAndMatchDiscount.IsLeastExpensiveAmountOff;
                }
    
                if (isOkay)
                {
                    isOkay = mixAndMatchDiscount.LineGroupToNumberOfItemsMap.Count == 1;
                }
    
                if (isOkay)
                {
                    // Mix and match example: buy 3 for $10.
                    // For each item, calculate relative value of mix and match against discount offer with quantity 3 as a group.
                    //     relative value of item A = M(AAA) - 3S(A)
                    // See DiscountCalculator.TryOptimizeForOneMixAndMatchWithOneLineGroup
                    var lineGroupNumberOfItemsPair = mixAndMatchDiscount.LineGroupToNumberOfItemsMap.First();
                    string lineGroup = lineGroupNumberOfItemsPair.Key;
                    decimal quantityNeeded = lineGroupNumberOfItemsPair.Value;
                    HashSet<int> mixAndMatchItemIndexGroupSet = null;
                    if (mixAndMatchDiscount.LineGroupToItemGroupIndexSetLookup.TryGetValue(lineGroup, out mixAndMatchItemIndexGroupSet))
                    {
                        foreach (int itemGroupIndex in mixAndMatchItemIndexGroupSet)
                        {
                            if (remainingQuantities[itemGroupIndex] > decimal.Zero)
                            {
                                // Move it to mix and match
                                decimal mixAndMatchValue = decimal.Zero;
    
                                decimal price = discountableItemGroups[itemGroupIndex].Price;
                                decimal totalPrice = price * quantityNeeded;
                                switch (mixAndMatchDiscount.DiscountType)
                                {
                                    case DiscountMethodType.DealPrice:
                                        mixAndMatchValue = totalPrice - mixAndMatchDiscount.DealPriceValue;
                                        isOkay = mixAndMatchValue >= decimal.Zero;
                                        break;
                                    case DiscountMethodType.DiscountAmount:
                                        mixAndMatchValue = mixAndMatchDiscount.DiscountAmountValue;
                                        isOkay = totalPrice >= mixAndMatchDiscount.DiscountAmountValue;
                                        break;
                                    case DiscountMethodType.DiscountPercent:
                                        mixAndMatchValue = DiscountBase.GetDiscountAmountForPercentageOff(totalPrice, mixAndMatchDiscount.DiscountPercentValue);
                                        break;
                                    default:
                                        isOkay = false;
                                        if (mixAndMatchDiscount.IsLeastExpensiveAmountOff && price >= mixAndMatchDiscount.DiscountAmountValue)
                                        {
                                            isOkay = true;
                                            mixAndMatchValue = mixAndMatchDiscount.DiscountAmountValue;
                                        }
    
                                        break;
                                }
    
                                if (!isOkay)
                                {
                                    break;
                                }
    
                                int discountOfferCount = 0;
                                decimal simpleDiscountValue = decimal.Zero;
                                foreach (KeyValuePair<string, DiscountBase> pair in this.OfferDiscounts)
                                {
                                    OfferDiscount offer = pair.Value as OfferDiscount;
    
                                    if (offer != null)
                                    {
                                        HashSet<decimal> discountLineNumberSetForItem = null;
                                        if (offer.ItemGroupIndexToDiscountLineNumberSetMap.TryGetValue(itemGroupIndex, out discountLineNumberSetForItem))
                                        {
                                            if (discountLineNumberSetForItem.Count >= 1)
                                            {
                                                decimal discountLineNumber = discountLineNumberSetForItem.First();
                                                RetailDiscountLine discountLineDefinition = offer.DiscountLines[discountLineNumber];
    
                                                // For now, it works for fully covered items.
                                                // Fully covered, as an example, buy 3 for $10, then if item price is $3, it won't get discount
                                                //     or buy 3 for $5 off, then if item price is $1, it won't get full discount.
                                                bool isFullyCovered = false;
                                                simpleDiscountValue = OfferDiscount.GetUnitDiscountAmountAndCheckWhetherItsFullyCovered(discountLineDefinition, price, out isFullyCovered) * quantityNeeded;
                                                if (!isFullyCovered)
                                                {
                                                    isOkay = false;
                                                }
    
                                                if (!isOkay)
                                                {
                                                    break;
                                                }
    
                                                simpleDiscountOfferLookup[itemGroupIndex] = offer;
                                                discountOfferCount++;
                                            }
                                        }
                                    }
                                }
    
                                // For now, we don't handle multiple discount offers.
                                // With optimization earlier, that's mostly the case. See DiscountCalcuator.ReduceOverlappedOfferAndQuantityDiscountsPerItem.
                                if (discountOfferCount > 1)
                                {
                                    isOkay = false;
                                }
    
                                if (!isOkay)
                                {
                                    break;
                                }
    
                                mixAndMatchRelativeValueLookup.Add(itemGroupIndex, mixAndMatchValue - simpleDiscountValue);
                            }
                        }
                    }
                }
    
                return isOkay;
            }
    
            internal bool IsOkayForMixAndMatchLeastExpensiveOneLineGroupPartialOptimizationAndFillupValueLookups(
                Dictionary<int, decimal> mixAndMatchRelativeValueLookup,
                List<int> itemGroupIndexListSortedByRelativePriceDescending,
                List<HashSet<int>> consolidatedListOfItemsWithSamePriceAndRelativeValuesDescending,
                Dictionary<int, OfferDiscount> simpleDiscountOfferLookup,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities)
            {
                // Special optimization for this overlapped group when there is
                //   only one mix and match 
                //   with only one line group
                //   with least expensive deal price and %-off
                bool isOkay = this.MixAndMatchAndQuantityDiscounts.Count == 1;
                consolidatedListOfItemsWithSamePriceAndRelativeValuesDescending.Clear();
    
                MixAndMatchDiscount mixAndMatchDiscount = null;
                if (isOkay)
                {
                    mixAndMatchDiscount = this.MixAndMatchAndQuantityDiscounts.First().Value as MixAndMatchDiscount;
    
                    isOkay = mixAndMatchDiscount != null;
                }
    
                if (isOkay)
                {
                    isOkay = mixAndMatchDiscount.LineGroupToNumberOfItemsMap.Count == 1;
                }
    
                if (isOkay)
                {
                    isOkay = mixAndMatchDiscount.DiscountType == DiscountMethodType.LeastExpensive &&
                       !mixAndMatchDiscount.IsLeastExpensiveAmountOff;
                }
    
                if (isOkay)
                {
                    var lineGroupNumberOfItemsPair = mixAndMatchDiscount.LineGroupToNumberOfItemsMap.First();
                    string lineGroup = lineGroupNumberOfItemsPair.Key;
                    decimal quantityNeeded = lineGroupNumberOfItemsPair.Value;
                    HashSet<int> mixAndMatchItemIndexGroupSet = null;
                    if (mixAndMatchDiscount.LineGroupToItemGroupIndexSetLookup.TryGetValue(lineGroup, out mixAndMatchItemIndexGroupSet))
                    {
                        foreach (int itemGroupIndex in mixAndMatchItemIndexGroupSet)
                        {
                            if (remainingQuantities[itemGroupIndex] > decimal.Zero)
                            {
                                // Move it to mix and match
                                decimal mixAndMatchValue = decimal.Zero;
    
                                decimal price = discountableItemGroups[itemGroupIndex].Price;
                                decimal totalPriceForDiscount = price * mixAndMatchDiscount.NumberOfLeastExpensiveLines;
    
                                if (mixAndMatchDiscount.DiscountPercentValue > decimal.Zero)
                                {
                                    mixAndMatchValue = DiscountBase.GetDiscountAmountForPercentageOff(totalPriceForDiscount, mixAndMatchDiscount.DiscountPercentValue);
                                }
                                else if (mixAndMatchDiscount.DiscountAmountValue == decimal.Zero && mixAndMatchDiscount.DealPriceValue > decimal.Zero)
                                {
                                    mixAndMatchValue = totalPriceForDiscount - mixAndMatchDiscount.DealPriceValue;
                                    if (mixAndMatchValue < decimal.Zero)
                                    {
                                        isOkay = false;
                                    }
                                }
    
                                if (!isOkay)
                                {
                                    break;
                                }
    
                                decimal simpleDiscountValue = decimal.Zero;
                                int discountOfferCount = 0;
    
                                // If an item has compounded discounts only, then we have already processed compounded discount offers, so we won't see them here.
                                //    And this won't affect algorithm.
                                // If we modify optimization before overlapped discounts, then this may not be true.
                                // See DiscountCalcuator.ReduceOverlappedOfferAndQuantityDiscountsPerItem.
                                foreach (KeyValuePair<string, DiscountBase> pair in this.OfferDiscounts)
                                {
                                    OfferDiscount offer = pair.Value as OfferDiscount;
    
                                    if (offer != null)
                                    {
                                        HashSet<decimal> discountLineNumberSetForItem = null;
                                        if (offer.ItemGroupIndexToDiscountLineNumberSetMap.TryGetValue(itemGroupIndex, out discountLineNumberSetForItem))
                                        {
                                            if (discountLineNumberSetForItem.Count >= 1)
                                            {
                                                decimal discountLineNumber = discountLineNumberSetForItem.First();
                                                RetailDiscountLine discountLineDefinition = offer.DiscountLines[discountLineNumber];
    
                                                // For now, it works for fully covered items.
                                                // Fully covered, as an example, buy 3 for $10, then if item price is $3, it won't get discount
                                                //     or buy 3 for $5 off, then if item price is $1, it won't get full discount.
                                                bool isFullyCovered = false;
                                                simpleDiscountValue = OfferDiscount.GetUnitDiscountAmountAndCheckWhetherItsFullyCovered(discountLineDefinition, price, out isFullyCovered) * quantityNeeded;
                                                if (!isFullyCovered)
                                                {
                                                    isOkay = false;
                                                }
    
                                                if (!isOkay)
                                                {
                                                    break;
                                                }
    
                                                simpleDiscountOfferLookup[itemGroupIndex] = offer;
    
                                                discountOfferCount++;
                                            }
                                        }
                                    }
                                }
    
                                // For now, we don't handle multiple discount offers.
                                // With optimization earlier, that's mostly the case. See DiscountCalcuator.ReduceOverlappedOfferAndQuantityDiscountsPerItem.
                                if (discountOfferCount > 1)
                                {
                                    isOkay = false;
                                }
    
                                if (!isOkay)
                                {
                                    break;
                                }
    
                                mixAndMatchRelativeValueLookup.Add(itemGroupIndex, mixAndMatchValue - simpleDiscountValue);
                            }
                        }
                    }
                }
    
                if (isOkay)
                {
                    // Now group items by relative price and price, and order them by relative price, in consolidatedListOfItemsWithSamePriceAndRelativeValuesDescending.
                    // See tests in OverlappedDiscountsUnitTests.
                    itemGroupIndexListSortedByRelativePriceDescending.AddRange(mixAndMatchRelativeValueLookup.Keys);
                    ItemPriceComparer itemPriceComparer = new ItemPriceComparer(mixAndMatchRelativeValueLookup);
                    itemGroupIndexListSortedByRelativePriceDescending.Sort(itemPriceComparer.GetComparison());
    
                    decimal previousRelativePrice = decimal.Zero;
                    Dictionary<decimal, HashSet<int>> itemPriceToItemGroupIndexSetLookup = new Dictionary<decimal, HashSet<int>>();
                    for (int listIndex = 0; listIndex < itemGroupIndexListSortedByRelativePriceDescending.Count; listIndex++)
                    {
                        int itemGroupIndex = itemGroupIndexListSortedByRelativePriceDescending[listIndex];
                        decimal currentPrice = discountableItemGroups[itemGroupIndex].Price;
                        decimal currentRelativePrice = mixAndMatchRelativeValueLookup[itemGroupIndex];
                        if (listIndex == 0)
                        {
                            previousRelativePrice = currentRelativePrice;
                            itemPriceToItemGroupIndexSetLookup.Add(currentPrice, new HashSet<int>() { itemGroupIndex });
                        }
                        else
                        {
                            if (currentRelativePrice == previousRelativePrice)
                            {
                                HashSet<int> itemGroupIndexSetWithSameRelativePriceAndPrice = null;
                                if (itemPriceToItemGroupIndexSetLookup.TryGetValue(currentPrice, out itemGroupIndexSetWithSameRelativePriceAndPrice))
                                {
                                    itemGroupIndexSetWithSameRelativePriceAndPrice.Add(itemGroupIndex);
                                }
                                else
                                {
                                    itemPriceToItemGroupIndexSetLookup.Add(currentPrice, new HashSet<int>() { itemGroupIndex });
                                }
                            }
    
                            if (currentRelativePrice != previousRelativePrice)
                            {
                                foreach (KeyValuePair<decimal, HashSet<int>> pair in itemPriceToItemGroupIndexSetLookup)
                                {
                                    HashSet<int> itemGroupIndexSetWithSameRelativePriceAndPrice = pair.Value;
    
                                    consolidatedListOfItemsWithSamePriceAndRelativeValuesDescending.Add(new HashSet<int>(itemGroupIndexSetWithSameRelativePriceAndPrice));
                                }
    
                                previousRelativePrice = currentRelativePrice;
                                itemPriceToItemGroupIndexSetLookup.Clear();
                                itemPriceToItemGroupIndexSetLookup.Add(currentPrice, new HashSet<int>() { itemGroupIndex });
                            }
    
                            if (listIndex == itemGroupIndexListSortedByRelativePriceDescending.Count - 1)
                            {
                                foreach (KeyValuePair<decimal, HashSet<int>> pair in itemPriceToItemGroupIndexSetLookup)
                                {
                                    HashSet<int> itemGroupIndexSetWithSameRelativePriceAndPrice = pair.Value;
                                    consolidatedListOfItemsWithSamePriceAndRelativeValuesDescending.Add(new HashSet<int>(itemGroupIndexSetWithSameRelativePriceAndPrice));
                                }
    
                                itemPriceToItemGroupIndexSetLookup.Clear();
                            }
                        }
                    }
                }
    
                return isOkay;
            }
    
            internal DiscountBase[] GetSortedDiscountsToApplyInFastMode(
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompound,
                HashSet<int> itemsWithOverlappingDiscounts,
                HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly)
            {
                using (SimpleProfiler profiler = new SimpleProfiler("OverlappedDiscounts.GetDiscountsToApplyInFastMode", 2))
                {
                    Dictionary<string, DiscountDealEstimate> offerIdToEstimateNonCompoundedLookup = new Dictionary<string, DiscountDealEstimate>(StringComparer.OrdinalIgnoreCase);
    
                    // Consolidate all compounded discounts into one estimate, to be sorted with the rest later.
                    List<DiscountBase> compoundedDiscounts = new List<DiscountBase>();
                    DiscountDealEstimate combinedEstimateForCompounded = null;
    
                    // Build estimates for offer discounts.
                    combinedEstimateForCompounded = OverlapppedDiscounts.BuildEstimates(
                        offerIdToEstimateNonCompoundedLookup,
                        compoundedDiscounts,
                        combinedEstimateForCompounded,
                        this.OfferDiscounts,
                        discountableItemGroups,
                        remainingQuantities,
                        remainingQuantitiesForCompound,
                        itemsWithOverlappingDiscounts,
                        itemsWithOverlappingDiscountsCompoundedOnly);
    
                    // Build estimates for mix and match and quantity discounts.
                    combinedEstimateForCompounded = OverlapppedDiscounts.BuildEstimates(
                        offerIdToEstimateNonCompoundedLookup,
                        compoundedDiscounts,
                        combinedEstimateForCompounded,
                        this.MixAndMatchAndQuantityDiscounts,
                        discountableItemGroups,
                        remainingQuantities,
                        remainingQuantitiesForCompound,
                        itemsWithOverlappingDiscounts,
                        itemsWithOverlappingDiscountsCompoundedOnly);
    
                    List<DiscountDealEstimate> estimatedSorted = new List<DiscountDealEstimate>(offerIdToEstimateNonCompoundedLookup.Values);
                    if (combinedEstimateForCompounded != null)
                    {
                        estimatedSorted.Add(combinedEstimateForCompounded);
                    }
    
                    estimatedSorted.Sort(DiscountDealEstimate.GetComparison());
    
    #if DEBUG
                    foreach (DiscountDealEstimate estimate in estimatedSorted)
                    {
                        estimate.DebugDisplay();
                    }
    #endif
    
                    DiscountBase[] discountsSorted = new DiscountBase[this.MixAndMatchAndQuantityDiscounts.Count + this.OfferDiscounts.Count];
                    int discountIndex = 0;
                    for (int i = estimatedSorted.Count - 1; i >= 0; i--)
                    {
                        DiscountDealEstimate estimate = estimatedSorted[i];
    
                        if (estimate.CanCompound)
                        {
                            for (int compoundedIndex = 0; compoundedIndex < compoundedDiscounts.Count; compoundedIndex++)
                            {
                                discountsSorted[discountIndex] = compoundedDiscounts[compoundedIndex];
                                discountIndex++;
                            }
                        }
                        else
                        {
                            DiscountBase discount = null;
    
                            if (this.MixAndMatchAndQuantityDiscounts.TryGetValue(estimate.OfferId, out discount))
                            {
                                discountsSorted[discountIndex] = discount;
                                discountIndex++;
                            }
                            else if (this.OfferDiscounts.TryGetValue(estimate.OfferId, out discount))
                            {
                                discountsSorted[discountIndex] = discount;
                                discountIndex++;
                            }
                        }
                    }
    
                    return discountsSorted;
                }
            }
    
            private static DiscountDealEstimate BuildEstimates(
                Dictionary<string, DiscountDealEstimate> offerIdToEstimateNonCompoundedLookupHolder,
                List<DiscountBase> compoundedDiscountsHolder,
                DiscountDealEstimate existingCombinedEstimatesForCompounded,
                Dictionary<string, DiscountBase> discounts,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompound,
                HashSet<int> itemsWithOverlappingDiscounts,
                HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly)
            {
                using (SimpleProfiler profiler = new SimpleProfiler("OverlappedDiscounts.BuildEstimates", 2))
                {
                    DiscountDealEstimate combinedEstimateForCompounded = existingCombinedEstimatesForCompounded;
                    foreach (KeyValuePair<string, DiscountBase> pair in discounts)
                    {
                        DiscountBase discount = pair.Value;
                        DiscountDealEstimate estimate = discount.GetDiscountDealEstimate(
                            discountableItemGroups,
                            discount.CanCompound ? remainingQuantitiesForCompound : remainingQuantities,
                            itemsWithOverlappingDiscounts,
                            itemsWithOverlappingDiscountsCompoundedOnly);
    
                        if (discount.CanCompound)
                        {
                            if (combinedEstimateForCompounded == null)
                            {
                                combinedEstimateForCompounded = estimate;
                            }
                            else
                            {
                                combinedEstimateForCompounded = DiscountDealEstimate.Combine(combinedEstimateForCompounded, estimate);
                            }
    
                            compoundedDiscountsHolder.Add(discount);
                        }
                        else
                        {
                            offerIdToEstimateNonCompoundedLookupHolder[discount.OfferId] = estimate;
                        }
                    }
    
                    // returns combined estimate for compounded
                    return combinedEstimateForCompounded;
                }
            }
    
            private void Aquire(
                Dictionary<int, OverlapppedDiscounts> itemGroupIndexToOverlapppedDiscountsLookup,
                OverlapppedDiscounts overlapppedDiscountsToAcquire)
            {
                using (SimpleProfiler profiler = new SimpleProfiler("OverlappedDiscounts.Aquire", 4))
                {
                    this.MixAndMatchAndQuantityDiscounts.AddRange(overlapppedDiscountsToAcquire.MixAndMatchAndQuantityDiscounts);
                    this.CoveredItemGroupIndexSet.AddRange(overlapppedDiscountsToAcquire.CoveredItemGroupIndexSet);
    
                    foreach (KeyValuePair<string, DiscountBase> pairOfferIdToDiscount in overlapppedDiscountsToAcquire.MixAndMatchAndQuantityDiscounts)
                    {
                        DiscountBase discount = pairOfferIdToDiscount.Value;
                        foreach (KeyValuePair<int, HashSet<decimal>> pair in discount.ItemGroupIndexToDiscountLineNumberSetMap)
                        {
                            int itemGroupIndex = pair.Key;
                            itemGroupIndexToOverlapppedDiscountsLookup[itemGroupIndex] = this;
                        }
                    }
                }
            }
        }
    }
}
