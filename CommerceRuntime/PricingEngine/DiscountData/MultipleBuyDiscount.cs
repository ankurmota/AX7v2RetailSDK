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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// This class implements the multi-buy (quantity threshold) discount processing, including the determination of which ways
        /// the discount can apply to the transaction and the value of the discount applied to specific lines.
        /// </summary>
        public class MultipleBuyDiscount : DiscountBase
        {
            private decimal lowestQuantityLevel;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="MultipleBuyDiscount" /> class.
            /// </summary>
            /// <param name="validationPeriod">Validation period.</param>
            public MultipleBuyDiscount(ValidationPeriod validationPeriod)
                : base(validationPeriod)
            {
                this.QuantityDiscountLevels = new List<QuantityDiscountLevel>();
                this.ItemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount = new Dictionary<int, decimal>();
                this.DiscountLineNumberSetMakingHighestQuantity = new HashSet<decimal>();
            }
    
            internal enum LevelTrend
            {
                Undecided,
                DealBetterAsLevelIncreases,
                DealWorseAsLevelIncreases,
            }
    
            internal enum OptimizationOverlapType
            {
                TooMuchOverlap,
                NoOverlap,
                OverlappedButRestMadeUpForHighestQuantity,
            }
    
            /// <summary>
            /// Gets the collection of MultiBuy discount tiers.
            /// </summary>
            public IList<QuantityDiscountLevel> QuantityDiscountLevels { get; private set; }
    
            // internal for test only.
            internal LevelTrend QuantityLevelTrend { get; private set; }
    
            // internal for test only.
            internal decimal HighestQuantityLevel { get; private set; }
    
            // internal for test only.
            // Allocate some intra-overlapped item quantity away from discount lines already making up the highest quantity.
            // This is to ensure this item alone gets the best deal, even if it doesn't make up for the highest quantity with other discount lines.
            internal Dictionary<int, decimal> ItemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount { get; private set; }
    
            // internal for test only.
            internal HashSet<decimal> DiscountLineNumberSetMakingHighestQuantity { get; private set; }

            /// <summary>
            /// Gets all of the possible applications of this discount to the specified transaction and line items.
            /// </summary>
            /// <param name="transaction">The transaction to consider for discounts.</param>
            /// <param name="discountableItemGroups">The valid sales line items on the transaction to consider.</param>
            /// <param name="remainingQuantities">The remaining quantities of each of the sales lines to consider.</param>
            /// <param name="priceContext">The pricing context to use.</param>
            /// <param name="appliedDiscounts">Applied discount application.</param>
            /// <param name="itemsWithOverlappingDiscounts">Items with overlapping discounts.</param>
            /// <param name="isInterrupted">A flag indicating whether it's interrupted for too many discount applications.</param>
            /// <returns>The possible permutations of line items that this discount can apply to, or an empty collection if this discount cannot apply.</returns>
            public override IEnumerable<DiscountApplication> GetDiscountApplications(
                SalesTransaction transaction,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                PriceContext priceContext,
                IEnumerable<AppliedDiscountApplication> appliedDiscounts,
                HashSet<int> itemsWithOverlappingDiscounts,
                out bool isInterrupted)
            {
                if (priceContext == null)
                {
                    throw new ArgumentNullException("priceContext");
                }
    
                List<DiscountApplication> discountApplications = new List<DiscountApplication>();
                isInterrupted = false;
    
                // If this discount cannot be applied, return an empty collection.
                if (discountableItemGroups == null ||
                    remainingQuantities == null ||
                    this.IsFinished)
                {
                    return discountApplications;
                }
    
                // Get the discount code to use for any discount lines, if one is required.
                string discountCodeUsed = this.GetDiscountCodeForDiscount(transaction);
    
                // Create a BitSet that can hold the valid items for both positive and negative quantities
                BitSet availableItemGroups = new BitSet((uint)discountableItemGroups.Length);
    
                // [znote] the following can be reworked using ItemGroupIndexToDiscountLineNumberSetLookup.
                for (int x = 0; x < discountableItemGroups.Length; x++)
                {
                    if (remainingQuantities[x] != 0M && this.ItemGroupIndexToDiscountLineNumberSetMap.ContainsKey(x))
                    {
                        availableItemGroups[x] = true;
                    }
                }
    
                discountApplications.AddRange(this.GetDiscountApplicationsForItems(
                    availableItemGroups,
                    discountableItemGroups,
                    remainingQuantities,
                    itemsWithOverlappingDiscounts,
                    discountCodeUsed,
                    priceContext,
                    out isInterrupted));
    
    #if DEBUG
                if (isInterrupted)
                {
                    DiscountCalculator.SetDebugDataOnTransaction(transaction, DiscountCalculator.HasTooManyDiscountApplicationsKey, true);
                }
    #endif
    
                return discountApplications;
            }

            /// <summary>
            /// Gets non-overlapped best-deal discount applications that can be applied right away.
            /// </summary>
            /// <param name="transaction">The transaction to consider for discounts.</param>
            /// <param name="discountableItemGroups">The valid sales line items on the transaction to consider.</param>
            /// <param name="remainingQuantities">The remaining quantities of each of the sales lines to consider.</param>
            /// <param name="priceContext">The pricing context to use.</param>
            /// <param name="appliedDiscounts">Applied discount application.</param>
            /// <param name="itemsWithOverlappingDiscounts">Items with overlapping discounts.</param>
            /// <param name="itemsWithOverlappingDiscountsCompoundedOnly">Hast set of overlapped item group indices, compounded only.</param>
            /// <returns>Non-overlapped discount applications that can be applied right away.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Parameter is correct for this usage.")]
            public override IEnumerable<DiscountApplication> GetDiscountApplicationsNonOverlappedWithBestDeal(
                SalesTransaction transaction,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                PriceContext priceContext,
                IEnumerable<AppliedDiscountApplication> appliedDiscounts,
                HashSet<int> itemsWithOverlappingDiscounts,
                HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly)
            {
                if (priceContext == null)
                {
                    throw new ArgumentNullException("priceContext");
                }
    
                if (discountableItemGroups == null)
                {
                    throw new ArgumentNullException("discountableItemGroups");
                }
    
                if (remainingQuantities == null)
                {
                    throw new ArgumentNullException("remainingQuantities");
                }
    
                List<DiscountApplication> discountApplications = new List<DiscountApplication>();
    
                if (this.IsFinished)
                {
                    return discountApplications;
                }
    
                if (this.QuantityLevelTrend == LevelTrend.DealBetterAsLevelIncreases)
                {
                    string discountCodeUsed = this.GetDiscountCodeForDiscount(transaction);
    
                    // For each discount line, create a discount application
                    //   if it makes the highest quantity level,
                    //   or all items are non-overlapping, internally and externally.
                    foreach (KeyValuePair<decimal, HashSet<int>> pair in this.DiscountLineNumberToItemGroupIndexSetMap)
                    {
                        decimal discountLineNumber = pair.Key;
                        HashSet<int> itemGroupIndexSet = pair.Value;
    
                        // canApplyNonOverlapped is true if the discount line makes the hightest quantity level already.
                        bool canApplyNonOverlapped = true;
                        if (!this.DiscountLineNumberSetMakingHighestQuantity.Contains(discountLineNumber))
                        {
                            // If the discount line doesn't make the highest quantity level, then canApplyNonOverlapped is true
                            //   only if all items in the discount line doesn't overlap externally and internally.
                            foreach (int itemGroupIndex in itemGroupIndexSet)
                            {
                                HashSet<decimal> discountLineNumberSetForItem = this.ItemGroupIndexToDiscountLineNumberSetMap[itemGroupIndex];
                                if (this.IsItemIndexGroupOverlappedWithNonCompoundedDiscounts(itemGroupIndex, itemsWithOverlappingDiscounts, itemsWithOverlappingDiscountsCompoundedOnly))
                                {
                                    canApplyNonOverlapped = false;
                                }
                                else if (discountLineNumberSetForItem.Count > 1)
                                {
                                    canApplyNonOverlapped = false;
                                }
                                else if (discountLineNumberSetForItem.Count == 1 && this.ItemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount.ContainsKey(itemGroupIndex))
                                {
                                    // the item makes the highest quantity level from a different discount line.
                                    canApplyNonOverlapped = false;
                                }
                            }
                        }
    
                        if (canApplyNonOverlapped)
                        {
                            RetailDiscountLine discountLineDefinition = this.DiscountLines[discountLineNumber];
                            Dictionary<int, decimal> quantitiesForApplication = new Dictionary<int, decimal>();
                            List<RetailDiscountLineItem> discountLineItems = new List<RetailDiscountLineItem>();
    
                            decimal totalQuantityForApplication = decimal.Zero;
                            foreach (int itemGroupIndex in itemGroupIndexSet)
                            {
                                // Don't count externally overlapped items yet.
                                if (!this.IsItemIndexGroupOverlappedWithNonCompoundedDiscounts(itemGroupIndex, itemsWithOverlappingDiscounts, itemsWithOverlappingDiscountsCompoundedOnly))
                                {
                                    decimal itemQuantity = remainingQuantities[itemGroupIndex];
                                    if (itemQuantity > decimal.Zero)
                                    {
                                        totalQuantityForApplication += itemQuantity;
                                        quantitiesForApplication[itemGroupIndex] = itemQuantity;
                                        discountLineItems.Add(new RetailDiscountLineItem(itemGroupIndex, discountLineDefinition));
                                    }
                                }
                            }
    
                            if (discountLineItems.Any())
                            {
                                QuantityDiscountLevel quantityLevel = this.GetQuantityLevel(totalQuantityForApplication, discountLineNumber);
    
                                if (quantityLevel != null)
                                {
                                    this.AddDiscountApplicationToAvailableResult(
                                        discountableItemGroups,
                                        discountLineItems,
                                        discountLineDefinition,
                                        quantityLevel.DiscountPriceOrPercent,
                                        quantitiesForApplication,
                                        discountCodeUsed,
                                        discountApplications,
                                        applyStandalone: true,
                                        setNumberOfTimesApplicable: false,
                                        removeItemsFromLookupsWhenApplied: true);
                                }
                            }
                        }
                    }
                }
    
                return discountApplications;
            }

            /// <summary>
            /// Applies the discount application and gets the value, taking into account previously applied discounts.
            /// </summary>
            /// <param name="discountableItemGroups">The transaction line items.</param>
            /// <param name="remainingQuantities">The quantities remaining for each item.</param>
            /// <param name="appliedDiscounts">The previously applied discounts.</param>
            /// <param name="discountApplication">The specific application of the discount to use.</param>
            /// <param name="priceContext">The pricing context to use.</param>
            /// <returns>The value of the discount application.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Parameter is correct for this usage.")]
            public override AppliedDiscountApplication GetAppliedDiscountApplication(
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                IEnumerable<AppliedDiscountApplication> appliedDiscounts,
                DiscountApplication discountApplication,
                PriceContext priceContext)
            {
                decimal result = 0M;
    
                if (discountApplication == null || discountableItemGroups == null || remainingQuantities == null)
                {
                    return null;
                }
    
                decimal[] prices = new decimal[discountableItemGroups.Length];
                bool includeAmountOff = true;
                bool includePercentageOff = true;
                if (this.DiscountType == DiscountMethodType.DealPrice || this.DiscountType == DiscountMethodType.MultiplyDealPrice)
                {
                    includeAmountOff = false;
                    includePercentageOff = false;
                }
    
                Dictionary<int, IList<DiscountLineQuantity>> discountDictionary = this.GetExistingDiscountDictionaryAndDiscountedPrices(
                    discountableItemGroups,
                    remainingQuantities,
                    appliedDiscounts,
                    discountApplication,
                    includeAmountOff,
                    includePercentageOff,
                    prices);
    
                MultipleBuyDiscountMethod discountMethod = (MultipleBuyDiscountMethod)discountApplication.Discount.DiscountType;
                Dictionary<int, decimal> itemIndexToDiscountValueMap = new Dictionary<int, decimal>();
                foreach (RetailDiscountLineItem item in discountApplication.RetailDiscountLines)
                {
                    decimal discountValue = decimal.Zero;
                    switch (discountMethod)
                    {
                        case MultipleBuyDiscountMethod.PercentOff:
                            discountValue = DiscountBase.GetDiscountAmountForPercentageOff(prices[item.ItemIndex], discountApplication.DiscountPercentValue);
                            break;
                        case MultipleBuyDiscountMethod.UnitPrice:
                            decimal dealPrice = discountApplication.DealPriceValue;
    
                            decimal bestExistingDealPrice = 0m;
                            bool hasExistingDealPrice = DiscountBase.TryGetBestExistingDealPrice(discountDictionary, item.ItemIndex, out bestExistingDealPrice);
    
                            discountValue = DiscountBase.GetDiscountAmountFromDealPrice(prices[item.ItemIndex], hasExistingDealPrice, bestExistingDealPrice, dealPrice);
                            itemIndexToDiscountValueMap[item.ItemIndex] = discountValue;
    
                            break;
    
                        default:
                            break;
                    }
    
                    result += discountValue * discountApplication.ItemQuantities[item.ItemIndex];
                }
    
                AppliedDiscountApplication newAppliedDiscountApplication = null;
                if (result > decimal.Zero)
                {
                    Dictionary<int, decimal> itemQuantities = null;
                    if (this.CanCompound &&
                        !discountApplication.ApplyStandalone && discountApplication.ItemQuantities.Sum(p => p.Value) == 1m)
                    {
                        // When it's an optimized simple discount with quantity = 1m and it's compounded, dump all remaining quantity to the applied discount application.
                        RetailDiscountLineItem retailDiscountLineItem = discountApplication.RetailDiscountLines.ElementAt(0);
                        itemQuantities = new Dictionary<int, decimal>();
                        decimal quantity = remainingQuantities[retailDiscountLineItem.ItemIndex];
                        itemQuantities[retailDiscountLineItem.ItemIndex] = quantity;
                        result *= quantity;
                    }
                    else
                    {
                        itemQuantities = discountApplication.ItemQuantities;
                    }
    
                    newAppliedDiscountApplication = new AppliedDiscountApplication(
                        discountApplication,
                        result,
                        itemQuantities,
                        isDiscountLineGenerated: true);
                    foreach (RetailDiscountLineItem retailDiscountLineItem in discountApplication.RetailDiscountLines)
                    {
                        DiscountLine discountLine = this.NewDiscountLine(discountApplication.DiscountCode, discountableItemGroups[retailDiscountLineItem.ItemIndex].ItemId);
    
                        discountLine.PeriodicDiscountType = PeriodicDiscountOfferType.MultipleBuy;
                        discountLine.DealPrice = discountApplication.DealPriceValue;
                        if (discountMethod == MultipleBuyDiscountMethod.UnitPrice)
                        {
                            decimal discountAmount = decimal.Zero;
                            itemIndexToDiscountValueMap.TryGetValue(retailDiscountLineItem.ItemIndex, out discountAmount);
                            discountLine.Amount = discountAmount;
                        }
    
                        discountLine.Percentage = discountApplication.DiscountPercentValue;
    
                        newAppliedDiscountApplication.AddDiscountLine(retailDiscountLineItem.ItemIndex, new DiscountLineQuantity(discountLine, itemQuantities[retailDiscountLineItem.ItemIndex]));
                    }
                }
    
                if (discountApplication.RemoveItemsFromLookupsWhenApplied)
                {
                    foreach (RetailDiscountLineItem discountLineItem in discountApplication.RetailDiscountLines)
                    {
                        decimal discountLineNumber = discountLineItem.RetailDiscountLine.DiscountLineNumber;
                        HashSet<int> itemGroupIndexSetForDiscountLineNumber = null;
                        if (this.DiscountLineNumberToItemGroupIndexSetMap.TryGetValue(discountLineNumber, out itemGroupIndexSetForDiscountLineNumber))
                        {
                            itemGroupIndexSetForDiscountLineNumber.Remove(discountLineItem.ItemIndex);
                        }
    
                        HashSet<decimal> discountLineNumberSetForItemGroupIndex = null;
                        if (this.ItemGroupIndexToDiscountLineNumberSetMap.TryGetValue(discountLineItem.ItemIndex, out discountLineNumberSetForItemGroupIndex))
                        {
                            discountLineNumberSetForItemGroupIndex.Remove(discountLineNumber);
    
                            if (!discountLineNumberSetForItemGroupIndex.Any())
                            {
                                this.ItemGroupIndexToDiscountLineNumberSetMap.Remove(discountLineItem.ItemIndex);
                            }
                        }
                    }
                }
    
                return newAppliedDiscountApplication;
            }
    
            /// <summary>
            /// Generate discount lines for the applied discount application.
            /// </summary>
            /// <param name="appliedDiscountApplication">The applied discount application.</param>
            /// <param name="discountableItemGroups">The discountable item groups.</param>
            /// <param name="priceContext">The price context.</param>
            public override void GenerateDiscountLines(
                AppliedDiscountApplication appliedDiscountApplication,
                DiscountableItemGroup[] discountableItemGroups,
                PriceContext priceContext)
            {
                // Nothing here, already generated in GetAppliedDiscountApplication.
            }
    
            internal LevelTrend InitializeQuantityLevelTrendAndHighestQuantityLevel()
            {
                decimal previousLevelPriceOrPercentage = decimal.Zero;
                this.lowestQuantityLevel = decimal.Zero;
                this.HighestQuantityLevel = decimal.Zero;
                this.QuantityLevelTrend = LevelTrend.Undecided;
    
                if (this.QuantityDiscountLevels.Count == 1)
                {
                    this.QuantityLevelTrend = LevelTrend.DealBetterAsLevelIncreases;
                    this.HighestQuantityLevel = this.QuantityDiscountLevels[0].MinimumQuantity;
                    this.lowestQuantityLevel = this.HighestQuantityLevel;
                }
                else
                {
                    SortedDictionary<decimal, QuantityDiscountLevel> sortedLevels = new SortedDictionary<decimal, QuantityDiscountLevel>(this.QuantityDiscountLevels.ToDictionary(p => p.MinimumQuantity, p => p));
                    MultipleBuyDiscountMethod discountMethod = (MultipleBuyDiscountMethod)this.DiscountType;
    
                    bool isFirst = true;
                    foreach (KeyValuePair<decimal, QuantityDiscountLevel> pair in sortedLevels)
                    {
                        this.HighestQuantityLevel = pair.Key;
    
                        if (isFirst)
                        {
                            isFirst = false;
                            this.lowestQuantityLevel = pair.Key;
                        }
                        else
                        {
                            LevelTrend newLevelTrend = ComputeLevelTrend(discountMethod, previousLevelPriceOrPercentage, pair.Value.DiscountPriceOrPercent);
                            switch (this.QuantityLevelTrend)
                            {
                                case LevelTrend.Undecided:
                                    this.QuantityLevelTrend = newLevelTrend;
                                    break;
                                case LevelTrend.DealBetterAsLevelIncreases:
                                case LevelTrend.DealWorseAsLevelIncreases:
                                    if (this.QuantityLevelTrend != newLevelTrend)
                                    {
                                        this.QuantityLevelTrend = LevelTrend.Undecided;
                                    }
    
                                    break;
                            }
    
                            if (this.QuantityLevelTrend == LevelTrend.Undecided)
                            {
                                break;
                            }
                        }
    
                        previousLevelPriceOrPercentage = pair.Value.DiscountPriceOrPercent;
                    }
                }
    
                return this.QuantityLevelTrend;
            }
    
            /// <summary>
            /// Optimize discount line number to item group index set and item group index to discount line number set lookups.
            /// </summary>
            /// <param name="quantitiesApplicable">Quantities applicable.</param>
            /// <param name="itemsWithOverlappingDiscounts">Items with external overlapping discounts.</param>
            /// <param name="itemsWithOverlappingDiscountsCompoundedOnly">Hast set of overlapped item group indices, compounded only.</param>
            /// <remarks>Private, but internal for test.</remarks>
            internal void OptimizeDiscountLineNumberAndItemGroupIndexLookupsAndReduceRedundantLines(
                decimal[] quantitiesApplicable,
                HashSet<int> itemsWithOverlappingDiscounts,
                HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly)
            {
                if (this.QuantityLevelTrend != LevelTrend.DealBetterAsLevelIncreases)
                {
                    // Optimizate only for quantity discount where deal better as quantity level increases.
                    return;
                }
    
                this.OptimizeDiscountLineNumberAndItemGroupIndexLookups(
                    quantitiesApplicable,
                    itemsWithOverlappingDiscounts,
                    itemsWithOverlappingDiscountsCompoundedOnly);
    
                // [Optimization] 2. remove redundant line, e.g. 2 lines, one for product and one for category, and product belongs to the category,
                //                   then the line of product is redundant discount line.
                if (this.OptimizationReduceRedundantDiscountLines())
                {
                    // if we have removed redundant discount lines, let's go through it one more time.
                    this.OptimizeDiscountLineNumberAndItemGroupIndexLookups(
                        quantitiesApplicable,
                        itemsWithOverlappingDiscounts,
                        itemsWithOverlappingDiscountsCompoundedOnly);
                }
            }
    
            /// <summary>
            /// Optimization: reduce redundant discount lines.
            /// </summary>
            /// <returns>true if reduced, false otherwise.</returns>
            /// <remarks>Private, but internal for test.</remarks>
            internal bool OptimizationReduceRedundantDiscountLines()
            {
                // [Optimization] remove redundant line, e.g. 2 lines, one for product and one for category, and product belongs to the category,
                //                then the line of product is redundant discount line.
                HashSet<decimal> redundantDiscountLineNumberSet = new HashSet<decimal>();
                var discountLineNumbers = this.DiscountLineNumberToItemGroupIndexSetMap.Keys;
    
                foreach (decimal left in discountLineNumbers)
                {
                    foreach (decimal right in discountLineNumbers)
                    {
                        if (left < right)
                        {
                            HashSet<int> leftItemIndexSet = new HashSet<int>(this.DiscountLineNumberToItemGroupIndexSetMap[left]);
                            HashSet<int> rightItemIndexSet = new HashSet<int>(this.DiscountLineNumberToItemGroupIndexSetMap[right]);
    
                            if (leftItemIndexSet.Count <= rightItemIndexSet.Count)
                            {
                                leftItemIndexSet.ExceptWith(rightItemIndexSet);
                                if (leftItemIndexSet.Count == 0)
                                {
                                    redundantDiscountLineNumberSet.Add(left);
                                }
                            }
                            else
                            {
                                rightItemIndexSet.ExceptWith(leftItemIndexSet);
                                if (rightItemIndexSet.Count == 0)
                                {
                                    redundantDiscountLineNumberSet.Add(right);
                                }
                            }
                        }
                    }
                }
    
                this.RemoveDiscountLineNumbersFromLookups(redundantDiscountLineNumberSet);
    
                return redundantDiscountLineNumberSet.Any();
            }
    
            internal QuantityDiscountLevel GetQuantityLevel(decimal quantity, decimal discountLineNumber)
            {
                decimal totalQuantity = this.DiscountLineNumberSetMakingHighestQuantity.Contains(discountLineNumber) ? this.HighestQuantityLevel : quantity;
                return this.GetQuantityLevel(totalQuantity);
            }
    
            internal QuantityDiscountLevel GetQuantityLevel(decimal quantity)
            {
                return this.QuantityDiscountLevels.Where(p => p.MinimumQuantity <= quantity).OrderByDescending(p => p.MinimumQuantity).FirstOrDefault();
            }
    
            /// <summary>
            /// Determines whether the discount can be applied standalone, i.e. not competing with other discounts.
            /// </summary>
            /// <param name="itemsWithOverlappingDiscounts">Items with overlapping discounts.</param>
            /// <param name="itemsWithOverlappingDiscountsCompoundedOnly">Hast set of overlapped item group indices, compounded only.</param>
            /// <returns>true if it can be applied standalone, otherwise false.</returns>
            protected internal override bool CanApplyStandalone(
                HashSet<int> itemsWithOverlappingDiscounts,
                HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly)
            {
                bool isExternallyOverlapped = false;
    
                foreach (KeyValuePair<int, HashSet<decimal>> pair in this.ItemGroupIndexToDiscountLineNumberSetMap)
                {
                    int itemGroupIndex = pair.Key;
                    if (this.IsItemIndexGroupOverlappedWithNonCompoundedDiscounts(itemGroupIndex, itemsWithOverlappingDiscounts, itemsWithOverlappingDiscountsCompoundedOnly))
                    {
                        isExternallyOverlapped = true;
                        break;
                    }
                }
    
                return !isExternallyOverlapped;
            }
    
            /// <summary>
            /// Get discount applications fast.
            /// </summary>
            /// <param name="transaction">Sales transaction.</param>
            /// <param name="discountableItemGroups">Discountable item groups.</param>
            /// <param name="remainingQuantities">Remaining quantities.</param>
            /// <returns>A collection of discount applications.</returns>
            protected internal override IEnumerable<DiscountApplication> GetDiscountApplicationsFastMode(
                SalesTransaction transaction,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities)
            {
                string discountCodeUsed = this.GetDiscountCodeForDiscount(transaction);
    
                IEnumerable<DiscountApplication> result = null;
    
                if (this.QuantityLevelTrend == LevelTrend.DealBetterAsLevelIncreases)
                {
                    result = this.GetDiscountApplicationsForItemsFastModeOptimizedForDeal(
                        discountableItemGroups,
                        remainingQuantities,
                        discountCodeUsed);
                }
                else
                {
                    result = this.GetDiscountApplicationsForItemsFastModeOptimizedForSpeed(
                        discountableItemGroups,
                        remainingQuantities,
                        discountCodeUsed);
                }
    
                return result;
            }
    
            /// <summary>
            /// Pre optimization.
            /// </summary>
            /// <param name="discountableItemGroups">The valid sales line items on the transaction to consider.</param>
            /// <param name="remainingQuantities">The remaining quantities of each of the sales lines to consider.</param>
            /// <param name="itemsWithOverlappingDiscounts">Items with overlapping discounts.</param>
            /// <param name="itemsWithOverlappingDiscountsCompoundedOnly">Hast set of overlapped item group indices, compounded only.</param>
            protected internal override void PreOptimization(
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                HashSet<int> itemsWithOverlappingDiscounts,
                HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly)
            {
                if (remainingQuantities == null)
                {
                    throw new ArgumentNullException("remainingQuantities");
                }
    
                if (itemsWithOverlappingDiscounts == null)
                {
                    throw new ArgumentNullException("itemsWithOverlappingDiscounts");
                }
    
                if (itemsWithOverlappingDiscountsCompoundedOnly == null)
                {
                    throw new ArgumentNullException("itemsWithOverlappingDiscountsCompoundedOnly");
                }
    
                if (this.QuantityDiscountLevels.Count == 0)
                {
                    this.IsFinished = true;
                    return;
                }
    
                this.ItemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount.Clear();
                this.DiscountLineNumberSetMakingHighestQuantity.Clear();
    
                this.InitializeQuantityLevelTrendAndHighestQuantityLevel();
    
                this.RemoveItemGroupIndexesWithZeroQuanttiyFromLookups(remainingQuantities);
    
                // Remove discount line numbers with insufficient quantity.
                HashSet<decimal> discountLineNumbersToRemove = new HashSet<decimal>();
                foreach (KeyValuePair<decimal, HashSet<int>> pair in this.DiscountLineNumberToItemGroupIndexSetMap)
                {
                    decimal discountLineNumber = pair.Key;
                    HashSet<int> itemGroupIndexSetForDiscountLineNumber = pair.Value;
    
                    if (this.lowestQuantityLevel > itemGroupIndexSetForDiscountLineNumber.Sum(p => remainingQuantities[p]))
                    {
                        discountLineNumbersToRemove.Add(discountLineNumber);
                    }
                }
    
                this.RemoveDiscountLineNumbersFromLookups(discountLineNumbersToRemove);
    
                this.OptimizeDiscountLineNumberAndItemGroupIndexLookupsAndReduceRedundantLines(
                    remainingQuantities,
                    itemsWithOverlappingDiscounts,
                    itemsWithOverlappingDiscountsCompoundedOnly);
    
                if (this.ItemGroupIndexToDiscountLineNumberSetMap.Count == 0)
                {
                    this.IsFinished = true;
                }
            }
    
            /// <summary>
            /// Gets the discount deal estimate.
            /// </summary>
            /// <param name="discountableItemGroups">The valid sales line items on the transaction to consider.</param>
            /// <param name="remainingQuantities">The remaining quantities of each of the sales lines to consider.</param>
            /// <param name="itemsWithOverlappingDiscounts">Items with overlapping discounts.</param>
            /// <param name="itemsWithOverlappingDiscountsCompoundedOnly">Hast set of overlapped item group indices, compounded only.</param>
            /// <returns>Discount deal estimate.</returns>
            protected internal override DiscountDealEstimate GetDiscountDealEstimate(
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                HashSet<int> itemsWithOverlappingDiscounts,
                HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly)
            {
                if (discountableItemGroups == null)
                {
                    throw new ArgumentNullException("discountableItemGroups");
                }
    
                if (remainingQuantities == null)
                {
                    throw new ArgumentNullException("remainingQuantities");
                }
    
                if (itemsWithOverlappingDiscounts == null)
                {
                    throw new ArgumentNullException("itemsWithOverlappingDiscounts");
                }
    
                if (itemsWithOverlappingDiscountsCompoundedOnly == null)
                {
                    throw new ArgumentNullException("itemsWithOverlappingDiscounts");
                }
    
                decimal[] remainingQuantitiesLocal = (decimal[])remainingQuantities.Clone();
    
                decimal totalApplicableQuantityWithOverlapped = decimal.Zero;
                decimal totalDiscountAmountWithOverlapped = decimal.Zero;
                decimal totalApplicableQuantityWithoutOverlapped = decimal.Zero;
                decimal totalDiscountAmountWithoutOverlapped = decimal.Zero;
                Dictionary<int, decimal> itemGroupIndexToQuantityNeededFromOverlappedLookup = new Dictionary<int, decimal>();
    
                // Go through each discount line, run all that's available.
                foreach (KeyValuePair<decimal, HashSet<int>> pair in this.DiscountLineNumberToItemGroupIndexSetMap)
                {
                    decimal discountLineNumber = pair.Key;
                    decimal discountLineQuantityWithOverlapped = decimal.Zero;
                    decimal discountLineQuantityWithoutOverlapped = decimal.Zero;
    
                    Dictionary<int, decimal> itemGroupIndexToQuantityNeededFromOverlappedLookupForDiscountLine = new Dictionary<int, decimal>();
    
                    // Use all item quantities available.
                    Dictionary<int, decimal> itemPriceLookup = new Dictionary<int, decimal>();
                    Dictionary<int, decimal> itemQuantityLookup = new Dictionary<int, decimal>();
                    foreach (int itemGroupIndex in pair.Value)
                    {
                        decimal quantity = remainingQuantitiesLocal[itemGroupIndex];
    
                        if (quantity > decimal.Zero)
                        {
                            itemPriceLookup[itemGroupIndex] = discountableItemGroups[itemGroupIndex].Price;
                            itemQuantityLookup[itemGroupIndex] = quantity;
    
                            discountLineQuantityWithOverlapped += quantity;
    
                            if (this.IsItemIndexGroupOverlappedWithNonCompoundedDiscounts(itemGroupIndex, itemsWithOverlappingDiscounts, itemsWithOverlappingDiscountsCompoundedOnly))
                            {
                                itemGroupIndexToQuantityNeededFromOverlappedLookupForDiscountLine[itemGroupIndex] = quantity;
                            }
                            else
                            {
                                discountLineQuantityWithoutOverlapped += quantity;
                            }
    
                            remainingQuantitiesLocal[itemGroupIndex] = decimal.Zero;
                        }
                    }
    
                    QuantityDiscountLevel quantityLevelWithOverlapped = this.GetQuantityLevel(discountLineQuantityWithOverlapped, discountLineNumber);
                    if (quantityLevelWithOverlapped != null)
                    {
                        totalApplicableQuantityWithOverlapped += discountLineQuantityWithOverlapped;
                        totalDiscountAmountWithOverlapped += this.GetEstimatedDiscountAmount(
                            quantityLevelWithOverlapped,
                            itemPriceLookup,
                            itemQuantityLookup,
                            itemsWithOverlappingDiscounts,
                            itemsWithOverlappingDiscountsCompoundedOnly,
                            includeOverlapped: true);
                        itemGroupIndexToQuantityNeededFromOverlappedLookup.AddRange(itemGroupIndexToQuantityNeededFromOverlappedLookupForDiscountLine);
                    }
    
                    QuantityDiscountLevel quantityLevelWithoutOverlapped = this.GetQuantityLevel(discountLineQuantityWithoutOverlapped, discountLineNumber);
                    if (quantityLevelWithoutOverlapped != null)
                    {
                        totalApplicableQuantityWithoutOverlapped += discountLineQuantityWithoutOverlapped;
                        totalDiscountAmountWithoutOverlapped += this.GetEstimatedDiscountAmount(
                            quantityLevelWithoutOverlapped,
                            itemPriceLookup,
                            itemQuantityLookup,
                            itemsWithOverlappingDiscounts,
                            itemsWithOverlappingDiscountsCompoundedOnly,
                            includeOverlapped: false);
                    }
                }
    
                DiscountDealEstimate estimate = new DiscountDealEstimate(
                    this.CanCompound,
                    this.OfferId,
                    totalApplicableQuantityWithOverlapped,
                    totalDiscountAmountWithOverlapped,
                    totalApplicableQuantityWithoutOverlapped,
                    totalDiscountAmountWithoutOverlapped,
                    itemGroupIndexToQuantityNeededFromOverlappedLookup);
    
                return estimate;
            }
    
            /// <summary>
            /// Get single item non-overlapped discount result.
            /// </summary>
            /// <param name="itemGroupIndex">Item group index.</param>
            /// <param name="price">Item price.</param>
            /// <param name="quantity">Item quantity.</param>
            /// <returns>Single item non-overlapped discount result.</returns>
            protected internal override SingleItemNonOverlappedDiscountResult GetSingleItemNonOverlappedDiscountResult(
                int itemGroupIndex,
                decimal price,
                decimal quantity)
            {
                SingleItemNonOverlappedDiscountResult result = SingleItemNonOverlappedDiscountResult.NotApplicable;
    
                if (!this.IsItemLikelyEvaluatedWithOtherItems(itemGroupIndex))
                {
                    decimal quantityToMakeTheLevel = quantity;
                    bool isIndependentOfOverlappedDiscounts = false;
                    if (this.ItemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount.ContainsKey(itemGroupIndex))
                    {
                        // Other items in the discount line make up for the highest quantity already, so this item can be treated as simple discount
                        //   with highest quantity level, without quantity limitation.
                        // While the other case: the item is covered by one discount line and the discount line covers this single item only in the transaction,
                        //   we can't always treat it like simple discount, for example, when overlapped with mix and match.
                        isIndependentOfOverlappedDiscounts = true;
                        quantityToMakeTheLevel = this.HighestQuantityLevel;
                    }
    
                    QuantityDiscountLevel quantityLevel = this.GetQuantityLevel(quantityToMakeTheLevel);
    
                    if (quantityLevel != null)
                    {
                        MultipleBuyDiscountMethod quantityDiscountMethod = (MultipleBuyDiscountMethod)this.DiscountType;
    
                        DiscountOfferMethod discountMethodForSingleItemNonOverlappedDiscountResult = DiscountOfferMethod.OfferPrice;
                        decimal offerPriceForSingleItemNonOverlappedDiscountResult = decimal.Zero;
                        decimal discountAmountForSingleItemNonOverlappedDiscountResult = decimal.Zero;
                        decimal discountPercentageForSingleItemNonOverlappedDiscountResult = decimal.Zero;
                        decimal unitDiscountAmountForSingleItemNonOverlappedDiscountResult = decimal.Zero;
    
                        switch (quantityDiscountMethod)
                        {
                            case MultipleBuyDiscountMethod.UnitPrice:
                                offerPriceForSingleItemNonOverlappedDiscountResult = quantityLevel.DiscountPriceOrPercent;
                                discountMethodForSingleItemNonOverlappedDiscountResult = DiscountOfferMethod.OfferPrice;
                                unitDiscountAmountForSingleItemNonOverlappedDiscountResult = DiscountBase.GetDiscountAmountForDealUnitPrice(price, offerPriceForSingleItemNonOverlappedDiscountResult);
                                break;
                            case MultipleBuyDiscountMethod.PercentOff:
                                discountPercentageForSingleItemNonOverlappedDiscountResult = quantityLevel.DiscountPriceOrPercent;
                                discountMethodForSingleItemNonOverlappedDiscountResult = DiscountOfferMethod.DiscountPercent;
                                unitDiscountAmountForSingleItemNonOverlappedDiscountResult = DiscountBase.GetDiscountAmountForPercentageOff(price, discountPercentageForSingleItemNonOverlappedDiscountResult);
                                break;
                        }
    
                        result = new SingleItemNonOverlappedDiscountResult(
                            discountMethodForSingleItemNonOverlappedDiscountResult,
                            discountAmountForSingleItemNonOverlappedDiscountResult,
                            discountPercentageForSingleItemNonOverlappedDiscountResult,
                            offerPriceForSingleItemNonOverlappedDiscountResult,
                            unitDiscountAmountForSingleItemNonOverlappedDiscountResult,
                            this.ConcurrencyMode,
                            isIndependentOfOverlappedDiscounts);
                    }
                }
    
                return result;
            }
    
            /// <summary>
            /// Determines if the item is likely evaluated with other items.
            /// </summary>
            /// <param name="itemGroupIndex">Item group index.</param>
            /// <returns>true if it's likely evaluated with other items; false otherwise.</returns>
            protected internal override bool IsItemLikelyEvaluatedWithOtherItems(int itemGroupIndex)
            {
                bool isItemLikelyEvaluatedWithOtherItems = true;
                HashSet<decimal> discountLineNumberSet;
                if (this.ItemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount.ContainsKey(itemGroupIndex))
                {
                    // All discount lines related the externally overlapped item make the highest quantity.
                    if (this.ItemGroupIndexToDiscountLineNumberSetMap.TryGetValue(itemGroupIndex, out discountLineNumberSet))
                    {
                        bool allMakesHighestQuantity = true;
                        foreach (decimal discountLineNumber in discountLineNumberSet)
                        {
                            if (!this.DiscountLineNumberSetMakingHighestQuantity.Contains(discountLineNumber))
                            {
                                allMakesHighestQuantity = false;
                            }
                        }
    
                        isItemLikelyEvaluatedWithOtherItems = !allMakesHighestQuantity;
                    }
                }
                else
                {
                    // It belongs to only one discount line and the discount line has only the item.
                    if (this.QuantityLevelTrend == LevelTrend.DealBetterAsLevelIncreases)
                    {
                        if (this.ItemGroupIndexToDiscountLineNumberSetMap.TryGetValue(itemGroupIndex, out discountLineNumberSet))
                        {
                            if (discountLineNumberSet.Count == 1)
                            {
                                decimal discountLineNumberForDiscountApplication = discountLineNumberSet.First();
                                HashSet<int> itemGroupIndexSetForDiscountLineNumber = null;
                                if (this.DiscountLineNumberToItemGroupIndexSetMap.TryGetValue(discountLineNumberForDiscountApplication, out itemGroupIndexSetForDiscountLineNumber))
                                {
                                    if (itemGroupIndexSetForDiscountLineNumber.Contains(itemGroupIndex) && itemGroupIndexSetForDiscountLineNumber.Count == 1)
                                    {
                                        isItemLikelyEvaluatedWithOtherItems = false;
                                    }
                                }
                            }
                        }
                    }
                }
    
                return isItemLikelyEvaluatedWithOtherItems;
            }
    
            /// <summary>
            /// Remove item group index from lookups.
            /// </summary>
            /// <param name="itemGroupIndex">Item group index.</param>
            protected internal override void RemoveItemIndexGroupFromLookups(int itemGroupIndex)
            {
                base.RemoveItemIndexGroupFromLookups(itemGroupIndex);
    
                this.ItemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount.Remove(itemGroupIndex);
            }
    
            /// <summary>
            /// Removes discount line numbers from lookups.
            /// </summary>
            /// <param name="discountLineNumbersToRemove">Discount line numbers to remove.</param>
            protected internal override void RemoveDiscountLineNumbersFromLookups(HashSet<decimal> discountLineNumbersToRemove)
            {
                base.RemoveDiscountLineNumbersFromLookups(discountLineNumbersToRemove);
    
                this.DiscountLineNumberSetMakingHighestQuantity.ExceptWith(discountLineNumbersToRemove);
            }
    
    #if DEBUG
            /// <summary>
            /// Debug discount.
            /// </summary>
            protected internal override void DebugDiscount()
            {
                base.DebugDiscount();
    
                if (DiscountCalculator.LogDiscountDetails)
                {
                    System.Diagnostics.Debug.WriteLine("  Quantity level trend [{0}] highest quantity [{1}]", this.QuantityLevelTrend, this.HighestQuantityLevel);
    
                    StringBuilder itemGroupIndicesMakingHighestQuantity = new StringBuilder("Items making highest quantity: ");
                    foreach (KeyValuePair<int, decimal> pair in this.ItemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount)
                    {
                        itemGroupIndicesMakingHighestQuantity.AppendFormat(" [{0}:{1}]", pair.Key, pair.Value);
                    }
    
                    System.Diagnostics.Debug.WriteLine("  {0}", itemGroupIndicesMakingHighestQuantity.ToString());
    
                    StringBuilder discountLineNumbersMakingHighestQuantity = new StringBuilder("Discount lines making highest quantity: ");
                    foreach (decimal discountLineNumber in this.DiscountLineNumberSetMakingHighestQuantity)
                    {
                        discountLineNumbersMakingHighestQuantity.AppendFormat(" [{0}]", discountLineNumber);
                    }
    
                    System.Diagnostics.Debug.WriteLine("  {0}", discountLineNumbersMakingHighestQuantity.ToString());
                }
            }
    #endif
    
            /// <summary>
            /// Gets the sort index to use for a discount application using the specified discount line.
            /// </summary>
            /// <param name="line">The discount line to determine the sort index on.</param>
            /// <returns>The sort index to use.</returns>
            protected override int GetSortIndexForRetailDiscountLine(RetailDiscountLine line)
            {
                return this.DiscountType == DiscountMethodType.DealPrice ? (int)DiscountOfferMethod.OfferPrice : (int)DiscountOfferMethod.DiscountPercent;
            }
    
            private static LevelTrend ComputeLevelTrend(MultipleBuyDiscountMethod discountMethod, decimal previousPriceOrPercentage, decimal priceOrPercentage)
            {
                bool isBetterLevel;
                if (discountMethod == MultipleBuyDiscountMethod.UnitPrice)
                {
                    isBetterLevel = previousPriceOrPercentage >= priceOrPercentage;
                }
                else
                {
                    isBetterLevel = priceOrPercentage >= previousPriceOrPercentage;
                }
    
                return isBetterLevel ? LevelTrend.DealBetterAsLevelIncreases : LevelTrend.DealWorseAsLevelIncreases;
            }
    
            private static void PrepareItemQuantityForDiscountApplication(
                Dictionary<int, decimal> quantitiesForApplication,
                List<RetailDiscountLineItem> discountLineItems,
                int itemGroupIndex,
                decimal quantityUsed,
                RetailDiscountLine retailDiscountLine,
                decimal[] remainingQuantities)
            {
                quantitiesForApplication[itemGroupIndex] = quantityUsed;
                discountLineItems.Add(new RetailDiscountLineItem(itemGroupIndex, retailDiscountLine));
    
                // Reduce quantity available.
                remainingQuantities[itemGroupIndex] -= quantityUsed;
            }
    
            private void SetNumberOfTimesAppliedForDiscountApplication(DiscountApplication discountApplication)
            {
                // As an example, if quantity level = 4 is the best deal, the you can apply quantity level 2 only once, and quantity level 3 only twice. It works for category as well.
                if (this.QuantityLevelTrend == LevelTrend.DealBetterAsLevelIncreases)
                {
                    decimal quantityLevel = discountApplication.ItemQuantities.Sum(p => p.Value);
                    if (quantityLevel < this.HighestQuantityLevel && quantityLevel > 0)
                    {
                        discountApplication.NumberOfTimesApplicable = (int)Math.Ceiling(this.HighestQuantityLevel / quantityLevel) - 1;
                    }
                    else if (quantityLevel > this.HighestQuantityLevel)
                    {
                        discountApplication.NumberOfTimesApplicable = 1;
                    }
                }
            }
    
            /// <summary>
            /// Gets the sort value to use for the selected items.  This value will be an approximation of the amount or percentage for the discount application.
            /// </summary>
            /// <param name="discountableItemGroups">The item groups on the transaction.</param>
            /// <param name="discountLineItems">Retail discount line plus item index.</param>
            /// <param name="itemQuantities">Item quantities.</param>
            /// <param name="discountUnitPriceOrPercent">Discount unit price or percentage.</param>
            /// <returns>The sort value.</returns>
            private decimal GetSortValue(
                DiscountableItemGroup[] discountableItemGroups,
                List<RetailDiscountLineItem> discountLineItems,
                Dictionary<int, decimal> itemQuantities,
                decimal discountUnitPriceOrPercent)
            {
                decimal sortValue = discountUnitPriceOrPercent;
    
                if (this.DiscountType == DiscountMethodType.DealPrice)
                {
                    decimal totalPrice = decimal.Zero;
                    decimal totalQuantity = decimal.Zero;
    
                    foreach (RetailDiscountLineItem discountLineItem in discountLineItems)
                    {
                        int itemIndex = discountLineItem.ItemIndex;
                        if (discountableItemGroups[itemIndex].Price > discountUnitPriceOrPercent)
                        {
                            totalPrice += discountableItemGroups[itemIndex].Price * itemQuantities[itemIndex];
                            totalQuantity += itemQuantities[itemIndex];
                        }
                    }
    
                    sortValue = totalPrice - (discountUnitPriceOrPercent * totalQuantity);
                }
    
                return sortValue;
            }
    
            /// <summary>
            /// Gets the collection of discount applications for the items on the transaction.
            /// </summary>
            /// <param name="availableItemGroups">The available item groups to consider.</param>
            /// <param name="discountableItemGroups">The item groups on the transaction.</param>
            /// <param name="remainingQuantities">The remaining quantities of the item groups.</param>
            /// <param name="itemsWithOverlappingDiscounts">Items with overlapping discounts.</param>
            /// <param name="discountCodeUsed">The discount code used for triggering this discount.</param>
            /// <param name="priceContext">Price context.</param>
            /// <param name="isInterrupted">A flag indicating whether it's interrupted for too many discount applications.</param>
            /// <returns>The collection of discount applications for this discount on the transaction.</returns>
            private IEnumerable<DiscountApplication> GetDiscountApplicationsForItems(
                BitSet availableItemGroups,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                HashSet<int> itemsWithOverlappingDiscounts,
                string discountCodeUsed,
                PriceContext priceContext,
                out bool isInterrupted)
            {
                // Notes on optimization for quantity discount when the following two are satisfied
                // 1. better the deal, as quantity level increases
                // 2. a) no overlap within the discount and with the rest of discounts, or b) small overlap, but sum(rest of quantities) makes up for the highest quantity level.
                // For a) no overlap, lump all items with all applicable quantities into one discount application.
                // For b) small overlap, for each overlapped item, one discount application, like discount offer, with quantity = 1, and with highest quantity level.
                //        for the rest, lump all with all applicable quantities into one discount application.
                // Details: see GetApplicationsForLine method.
                List<DiscountApplication> result = new List<DiscountApplication>();
                isInterrupted = false;
    
                if (availableItemGroups.IsZero())
                {
                    return result;
                }
    
                int currentIndex = 0;
    
                // Include maxDepth of up to 2 * max tier - 1 (since 2 * max would be 2 separate discounts).
                int minDepth = decimal.ToInt32(this.QuantityDiscountLevels.Min(p => p.MinimumQuantity));
                int maxDepth = (decimal.ToInt32(this.QuantityDiscountLevels.Max(p => p.MinimumQuantity)) * 2) - 1;
    
                decimal[] qtyRemaining = (decimal[])remainingQuantities.Clone();
    
                Stack<int> itemsUsed = new Stack<int>();
    
                // Loop through all of the triggered discount lines
                foreach (KeyValuePair<decimal, HashSet<int>> pair in this.DiscountLineNumberToItemGroupIndexSetMap)
                {
                    RetailDiscountLine discountLine = this.DiscountLines[pair.Key];
                    currentIndex = this.GetApplicationsForLine(
                        availableItemGroups,
                        discountableItemGroups,
                        discountCodeUsed,
                        result,
                        currentIndex,
                        minDepth,
                        maxDepth,
                        qtyRemaining,
                        itemsUsed,
                        discountLine,
                        itemsWithOverlappingDiscounts,
                        priceContext,
                        out isInterrupted);
                }
    
                if (!isInterrupted)
                {
                    // Overlapped items treated as simple discount. See OptimizeDiscountLineNumberAndItemGroupIndexLookupsAndReduceRedundantLines for more details.
                    foreach (KeyValuePair<int, decimal> pair in this.ItemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount)
                    {
                        int itemGroupIndex = pair.Key;
    
                        if (remainingQuantities[itemGroupIndex] > decimal.Zero)
                        {
                            decimal discountLineNumber = pair.Value;
                            RetailDiscountLine retailDiscountLine = this.DiscountLines[discountLineNumber];
                            QuantityDiscountLevel quantityLevel = this.GetQuantityLevel(this.HighestQuantityLevel);
    
                            Dictionary<int, decimal> oneLineItemQuantities = new Dictionary<int, decimal>();
                            oneLineItemQuantities[itemGroupIndex] = 1m;
                            this.AddDiscountApplicationToAvailableResult(
                                discountableItemGroups,
                                new List<RetailDiscountLineItem>() { new RetailDiscountLineItem(itemGroupIndex, retailDiscountLine) },
                                retailDiscountLine,
                                quantityLevel.DiscountPriceOrPercent,
                                oneLineItemQuantities,
                                discountCodeUsed,
                                result,
                                applyStandalone: false,
                                setNumberOfTimesApplicable: false);
                        }
                    }
                }
    
                return result;
            }
    
            /// <summary>
            /// Gets the possible discount applications for the specified discount line.
            /// </summary>
            /// <param name="availableItemGroups">The available item groups to consider.</param>
            /// <param name="discountableItemGroups">The item groups on the transaction.</param>
            /// <param name="discountCodeUsed">The discount code used for triggering this discount.</param>
            /// <param name="result">The collection of DiscountApplication objects that will become the result for this discount.</param>
            /// <param name="currentIndex">The current index within the available item groups.</param>
            /// <param name="minDepth">The minimum number of items required to trigger the discount.</param>
            /// <param name="maxDepth">The maximum number of items required to trigger the discount.</param>
            /// <param name="qtyRemaining">The remaining quantities of the item groups after other items have been used.</param>
            /// <param name="itemsUsed">The items used on the current path.</param>
            /// <param name="line">The discount line to determine the possible applications for.</param>
            /// <param name="itemsWithOverlappingDiscounts">Items with overlapping discounts.</param>
            /// <param name="priceContext">Price context.</param>
            /// <param name="isInterrupted">A flag indicating whether it's interrupted for too many discount applications.</param>
            /// <returns>The currentIndex value to use for the next iteration.</returns>
            private int GetApplicationsForLine(
                BitSet availableItemGroups,
                DiscountableItemGroup[] discountableItemGroups,
                string discountCodeUsed,
                List<DiscountApplication> result,
                int currentIndex,
                int minDepth,
                int maxDepth,
                decimal[] qtyRemaining,
                Stack<int> itemsUsed,
                RetailDiscountLine line,
                HashSet<int> itemsWithOverlappingDiscounts,
                PriceContext priceContext,
                out bool isInterrupted)
            {
                isInterrupted = false;
                bool doFractional = this.HasFractionalLevel();
    
                if (!doFractional)
                {
                    doFractional = this.HasFractionalQuantity(discountableItemGroups, line);
                }
    
                OptimizationOverlapType overlapType = OptimizationOverlapType.TooMuchOverlap;
    
                if (!doFractional && this.QuantityLevelTrend == LevelTrend.DealBetterAsLevelIncreases)
                {
                    overlapType = this.CalculateOptimizationOverlapType(
                        line.DiscountLineNumber,
                        qtyRemaining,
                        itemsWithOverlappingDiscounts);
                }
    
                if (doFractional)
                {
                    this.AddAllQualifiedQuantitiesAsOneDiscountAppliationToResult(
                        discountableItemGroups,
                        line,
                        qtyRemaining,
                        result,
                        discountCodeUsed);
                }
                else if (overlapType == OptimizationOverlapType.NoOverlap)
                {
                    // Optimize for no overlap: lump all into one discount application with all applicable quantities
                    this.AddAllQualifiedQuantitiesAsOneDiscountAppliationToResult(
                        discountableItemGroups,
                        line,
                        qtyRemaining,
                        result,
                        discountCodeUsed);
                }
                else if (overlapType == OptimizationOverlapType.OverlappedButRestMadeUpForHighestQuantity)
                {
                    // DISCOUNTPERF: we probably don't need this anymore, because we have gone through optimization for best deal already. It's only referenced in unit tests.
                    // Optimize for small overlapp, but sum(non-overlapping quantity) >= highest quantity level
                    // 1. One discount application for non-overlapping items with all applicable quantities.
                    // 2. For items with overlapping discounts, treat each one as discount offer, i.e. one discount application with quantity = 1, and with highest quantity level.
                    this.AddNonOverlappingItemsAsOneDiscountAppliationAndMoreToResult(
                        discountableItemGroups,
                        line,
                        result,
                        discountCodeUsed,
                        itemsWithOverlappingDiscounts);
                }
                else
                {
                    while (true)
                    {
                        if (priceContext.ExceedsMaxBestDealAlgorithmStepCount(result.Count))
                        {
                            isInterrupted = true;
                            break;
                        }
    
                        currentIndex = availableItemGroups.GetNextNonZeroBit(currentIndex);
    
                        if (currentIndex == BitSet.UnknownBit || itemsUsed.Count >= maxDepth)
                        {
                            if (itemsUsed.Count > 0)
                            {
                                // No next non-zero bit exists, move to the next item from the previous parent
                                int lastItem = itemsUsed.Pop();
                                qtyRemaining[lastItem] += 1;
                                currentIndex = lastItem + 1;
                            }
                            else
                            {
                                // No next non-zero bit exists and we are at the top level, exit the loop
                                currentIndex = 0;
                                break;
                            }
                        }
                        else if (this.IsDiscountLineCoveringItem(line.DiscountLineNumber, currentIndex) && qtyRemaining[currentIndex] >= 1M)
                        {
                            itemsUsed.Push(currentIndex);
                            qtyRemaining[currentIndex] -= 1;
    
                            // Determine if a tier exists for this depth
                            if (itemsUsed.Count >= minDepth)
                            {
                                this.AddDiscountApplicationToAvailableResult(
                                    discountableItemGroups,
                                    discountCodeUsed,
                                    result,
                                    itemsUsed,
                                    line);
                            }
                        }
                        else
                        {
                            // Not enough items left in this index or this item is not in the correct discount line, move to the next one
                            currentIndex++;
                        }
                    }
                }
    
                return currentIndex;
            }
    
            /// <summary>
            /// Gets the collection of discount applications for the items on the transaction in fast mode, optimized for speed.
            /// Goes through each discount line, uses up all available quantities.
            /// </summary>
            /// <param name="discountableItemGroups">The item groups on the transaction.</param>
            /// <param name="remainingQuantities">The remaining quantities of the item groups.</param>
            /// <param name="discountCodeUsed">The discount code used for triggering this discount.</param>
            /// <returns>The collection of discount applications for this discount on the transaction.</returns>
            private IEnumerable<DiscountApplication> GetDiscountApplicationsForItemsFastModeOptimizedForSpeed(
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                string discountCodeUsed)
            {
                List<DiscountApplication> result = new List<DiscountApplication>();
                decimal[] remainingQuantitiesLocal = (decimal[])remainingQuantities.Clone();
    
                // One application for each discount lines.
                foreach (KeyValuePair<decimal, HashSet<int>> pair in this.DiscountLineNumberToItemGroupIndexSetMap)
                {
                    decimal discountLineNumber = pair.Key;
                    RetailDiscountLine retailDiscountLine = this.DiscountLines[discountLineNumber];
                    decimal totalQuantityForDiscountLine = decimal.Zero;
                    Dictionary<int, decimal> quantitiesForApplication = new Dictionary<int, decimal>();
                    List<RetailDiscountLineItem> discountLineItems = new List<RetailDiscountLineItem>();
    
                    // Use all item quantities available.
                    foreach (int itemGroupIndex in pair.Value)
                    {
                        decimal quantity = remainingQuantitiesLocal[itemGroupIndex];
    
                        if (quantity > decimal.Zero)
                        {
                            totalQuantityForDiscountLine += quantity;
                            PrepareItemQuantityForDiscountApplication(
                                quantitiesForApplication,
                                discountLineItems,
                                itemGroupIndex,
                                quantity,
                                retailDiscountLine,
                                remainingQuantitiesLocal);
                        }
                    }
    
                    QuantityDiscountLevel quantityLevel = this.GetQuantityLevel(totalQuantityForDiscountLine, discountLineNumber);
    
                    if (quantityLevel != null)
                    {
                        // Create discount application.
                        this.AddDiscountApplicationToAvailableResult(
                            discountableItemGroups,
                            discountLineItems,
                            retailDiscountLine,
                            quantityLevel.DiscountPriceOrPercent,
                            quantitiesForApplication,
                            discountCodeUsed,
                            result,
                            applyStandalone: true,
                            setNumberOfTimesApplicable: false);
                    }
                }
    
                return result;
            }
    
            /// <summary>
            /// Gets the collection of discount applications for the items on the transaction in fast mode, optimized for deal.
            /// </summary>
            /// <param name="discountableItemGroups">The item groups on the transaction.</param>
            /// <param name="remainingQuantities">The remaining quantities of the item groups.</param>
            /// <param name="discountCodeUsed">The discount code used for triggering this discount.</param>
            /// <returns>The collection of discount applications for this discount on the transaction.</returns>
            private IEnumerable<DiscountApplication> GetDiscountApplicationsForItemsFastModeOptimizedForDeal(
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                string discountCodeUsed)
            {
                List<DiscountApplication> result = new List<DiscountApplication>();
    
                // Step 1: build 2 lookups from discount line number to item group index set of
                //   1. non-overlapped item group indices
                //   2. overlapped item group indices
                Dictionary<decimal, HashSet<int>> discountLineNumberToNonOverlapItemGroupIndexSetLookup = new Dictionary<decimal, HashSet<int>>();
                Dictionary<decimal, HashSet<int>> discountLineNumberToOverlapItemGroupIndexSetLookup = new Dictionary<decimal, HashSet<int>>();
                foreach (KeyValuePair<decimal, HashSet<int>> pair in this.DiscountLineNumberToItemGroupIndexSetMap)
                {
                    decimal discountLineNumber = pair.Key;
    
                    foreach (int itemGroupIndex in pair.Value)
                    {
                        HashSet<decimal> discountLineNumberSetForItemGroupIndex = null;
                        this.ItemGroupIndexToDiscountLineNumberSetMap.TryGetValue(itemGroupIndex, out discountLineNumberSetForItemGroupIndex);
    
                        // We've already done optimization pre-processing in OptimizeDiscountLineNumberAndItemGroupIndexLookupsAndReduceRedundantLines,
                        // which may put some overlapped items in ItemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount
                        if (this.ItemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount.ContainsKey(itemGroupIndex) ||
                            (discountLineNumberSetForItemGroupIndex != null && discountLineNumberSetForItemGroupIndex.Count > 1))
                        {
                            HashSet<int> itemGroupIndexSetOverlapped = null;
                            if (discountLineNumberToOverlapItemGroupIndexSetLookup.TryGetValue(discountLineNumber, out itemGroupIndexSetOverlapped))
                            {
                                itemGroupIndexSetOverlapped.Add(itemGroupIndex);
                            }
                            else
                            {
                                discountLineNumberToOverlapItemGroupIndexSetLookup.Add(discountLineNumber, new HashSet<int>() { itemGroupIndex });
                            }
                        }
                        else
                        {
                            HashSet<int> itemGroupIndexSetNonOverlapped = null;
                            if (discountLineNumberToNonOverlapItemGroupIndexSetLookup.TryGetValue(discountLineNumber, out itemGroupIndexSetNonOverlapped))
                            {
                                itemGroupIndexSetNonOverlapped.Add(itemGroupIndex);
                            }
                            else
                            {
                                discountLineNumberToNonOverlapItemGroupIndexSetLookup.Add(discountLineNumber, new HashSet<int>() { itemGroupIndex });
                            }
                        }
                    }
                }
    
                // Step 2: construct discount applications for each discount line number
                if (discountLineNumberToOverlapItemGroupIndexSetLookup.Any())
                {
                    // Step 2.1: We have overlapped item group index.
                    // Step 2.1.1: Sort discount line numbers by non-overlapping quantity, descending
                    decimal[] discountLineNumbersSorted = this.DiscountLineNumberToItemGroupIndexSetMap.Keys.ToArray();
                    DiscountLineQuantityComparer discountLineNumberQuantityComparer = new DiscountLineQuantityComparer(
                        discountLineNumberToNonOverlapItemGroupIndexSetLookup,
                        discountLineNumberToOverlapItemGroupIndexSetLookup,
                        remainingQuantities);
                    Array.Sort(discountLineNumbersSorted, discountLineNumberQuantityComparer.CompareQuantityByDiscountLineNumber);
    
                    decimal[] remainingQuantitiesLocal = (decimal[])remainingQuantities.Clone();
                    HashSet<int> itemGroupIndexSetOverlappedQualifiedForHighestQuantityLevel = new HashSet<int>(this.ItemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount.Keys);
    
                    // Step 2.1.2: Go through discount line numbers one by one, and try to make the highest quantity level.
                    // After making the highest quantity level, save the overlapped for other discount lines.
                    for (int discountLineNumberIndex = 0; discountLineNumberIndex < discountLineNumbersSorted.Length; discountLineNumberIndex++)
                    {
                        decimal discountLineNumber = discountLineNumbersSorted[discountLineNumberIndex];
                        RetailDiscountLine retailDiscountLine = this.DiscountLines[discountLineNumber];
                        decimal totalQuantityForDiscountLine = decimal.Zero;
                        Dictionary<int, decimal> quantitiesForApplication = new Dictionary<int, decimal>();
                        List<RetailDiscountLineItem> discountLineItems = new List<RetailDiscountLineItem>();
    
                        HashSet<int> itemGroupIndexSetNonOverlapped = null;
                        if (!discountLineNumberToNonOverlapItemGroupIndexSetLookup.TryGetValue(discountLineNumber, out itemGroupIndexSetNonOverlapped))
                        {
                            itemGroupIndexSetNonOverlapped = new HashSet<int>();
                        }
    
                        HashSet<int> itemGroupIndexSetOverlapped = null;
                        if (!discountLineNumberToOverlapItemGroupIndexSetLookup.TryGetValue(discountLineNumber, out itemGroupIndexSetOverlapped))
                        {
                            itemGroupIndexSetOverlapped = new HashSet<int>();
                        }
    
                        // Step 2.1.2.1: Use all non-overlapped quantities
                        foreach (int itemGroupIndex in itemGroupIndexSetNonOverlapped)
                        {
                            decimal quantity = remainingQuantitiesLocal[itemGroupIndex];
                            totalQuantityForDiscountLine += quantity;
                            PrepareItemQuantityForDiscountApplication(
                                quantitiesForApplication,
                                discountLineItems,
                                itemGroupIndex,
                                quantity,
                                retailDiscountLine,
                                remainingQuantitiesLocal);
                        }
    
                        // Step 2.1.2.2: Use overlapped quantities to get to highest quantity.
                        // For unneeded - after making the highest level - save them for other discount lines in itemGroupIndexSetOverlappedQualifiedForHighestQuantityLevel.
                        decimal totalQuantityOverlappedPending = decimal.Zero;
                        foreach (int itemGroupIndex in itemGroupIndexSetOverlapped)
                        {
                            decimal quantityAvailable = remainingQuantitiesLocal[itemGroupIndex];
                            decimal quantityUsed = decimal.Zero;
                            if (totalQuantityForDiscountLine < this.HighestQuantityLevel)
                            {
                                if (itemGroupIndexSetOverlappedQualifiedForHighestQuantityLevel.Contains(itemGroupIndex))
                                {
                                    // (Overlapped) item group index already qualified for the highest quantity level, add it to pending.
                                    quantityUsed = quantityAvailable;
                                    totalQuantityOverlappedPending += quantityAvailable;
                                }
                                else
                                {
                                    // (Overlapped) item group index not yet qualified for the highest quantity level, use quantity up to highest quantity level.
                                    quantityUsed = Math.Min(quantityAvailable, this.HighestQuantityLevel - totalQuantityForDiscountLine);
                                    totalQuantityForDiscountLine += quantityUsed;
    
                                    PrepareItemQuantityForDiscountApplication(
                                        quantitiesForApplication,
                                        discountLineItems,
                                        itemGroupIndex,
                                        quantityUsed,
                                        retailDiscountLine,
                                        remainingQuantitiesLocal);
                                }
                            }
    
                            quantityAvailable -= quantityUsed;
    
                            // Save unneeded overlapped items for other discount lines in itemGroupIndexSetOverlappedQualifiedForHighestQuantityLevel.
                            if (quantityAvailable > decimal.Zero && !itemGroupIndexSetOverlappedQualifiedForHighestQuantityLevel.Contains(itemGroupIndex))
                            {
                                itemGroupIndexSetOverlappedQualifiedForHighestQuantityLevel.Add(itemGroupIndex);
                            }
                        }
    
                        // Step 2.1.2.3: If adding pending overlapped items makes the highest quantity level, then add them; or else leave them out for now.
                        // Please note that if the overlapped item group index makes the highest quantity level, we always put it in the highest quantity level,
                        // which means we won't sacrifice it for discount line number that won't make the highest level.
                        if (totalQuantityForDiscountLine < this.HighestQuantityLevel &&
                            totalQuantityForDiscountLine + totalQuantityOverlappedPending >= this.HighestQuantityLevel)
                        {
                            foreach (int itemGroupIndex in itemGroupIndexSetOverlapped)
                            {
                                if (itemGroupIndexSetOverlappedQualifiedForHighestQuantityLevel.Contains(itemGroupIndex) && totalQuantityForDiscountLine < this.HighestQuantityLevel)
                                {
                                    decimal quantityAvailable = remainingQuantitiesLocal[itemGroupIndex];
                                    decimal quantityUsed = Math.Min(quantityAvailable, this.HighestQuantityLevel - totalQuantityForDiscountLine);
    
                                    totalQuantityForDiscountLine += quantityUsed;
                                    PrepareItemQuantityForDiscountApplication(
                                        quantitiesForApplication,
                                        discountLineItems,
                                        itemGroupIndex,
                                        quantityUsed,
                                        retailDiscountLine,
                                        remainingQuantitiesLocal);
                                }
                            }
                        }
    
                        QuantityDiscountLevel quantityLevel = this.GetQuantityLevel(totalQuantityForDiscountLine, discountLineNumber);
    
                        if (quantityLevel != null)
                        {
                            this.AddDiscountApplicationToAvailableResult(
                                discountableItemGroups,
                                discountLineItems,
                                retailDiscountLine,
                                quantityLevel.DiscountPriceOrPercent,
                                quantitiesForApplication,
                                discountCodeUsed,
                                result,
                                applyStandalone: true,
                                setNumberOfTimesApplicable: false);
                        }
                    }
    
                    // See DiscountPerformanceTests.PerfMultibuyOverlappingInternalAndExternalExclusiveMakingHighestLevelExtraQuantity()
                    // Step 2.1.3: Create discount applications for overlapped item group indices qualified for the highest quantity level, yet not used.
                    foreach (int itemGroupIndex in itemGroupIndexSetOverlappedQualifiedForHighestQuantityLevel)
                    {
                        decimal quantity = remainingQuantitiesLocal[itemGroupIndex];
                        if (quantity > decimal.Zero)
                        {
                            QuantityDiscountLevel quantityLevel = this.GetQuantityLevel(this.HighestQuantityLevel);
    
                            decimal discountLineNumber = this.ItemGroupIndexToDiscountLineNumberSetMap[itemGroupIndex].First();
                            RetailDiscountLine retailDiscountLine = this.DiscountLines[discountLineNumber];
                            Dictionary<int, decimal> quantitiesForApplication = new Dictionary<int, decimal>();
                            List<RetailDiscountLineItem> discountLineItems = new List<RetailDiscountLineItem>();
                            PrepareItemQuantityForDiscountApplication(
                                quantitiesForApplication,
                                discountLineItems,
                                itemGroupIndex,
                                quantity,
                                retailDiscountLine,
                                remainingQuantitiesLocal);
    
                            this.AddDiscountApplicationToAvailableResult(
                                discountableItemGroups,
                                discountLineItems,
                                retailDiscountLine,
                                quantityLevel.DiscountPriceOrPercent,
                                quantitiesForApplication,
                                discountCodeUsed,
                                result,
                                applyStandalone: true,
                                setNumberOfTimesApplicable: false);
                        }
                    }
                }
                else
                {
                    // Step 2.2: No overlap, no different from fast mode optimized for speed.
                    result.AddRange(this.GetDiscountApplicationsForItemsFastModeOptimizedForSpeed(
                        discountableItemGroups,
                        remainingQuantities,
                        discountCodeUsed));
                }
    
                return result;
            }
    
            /// <summary>
            /// Optimize discount line number to item group index set and item group index to discount line number set lookups.
            /// </summary>
            /// <param name="quantitiesApplicable">Quantities applicable.</param>
            /// <param name="itemsWithOverlappingDiscounts">Items with external overlapping discounts.</param>
            /// <param name="itemsWithOverlappingDiscountsCompoundedOnly">Hast set of overlapped item group indices, compounded only.</param>
            /// <remarks>Private, but internal for test.</remarks>
            private void OptimizeDiscountLineNumberAndItemGroupIndexLookups(
                decimal[] quantitiesApplicable,
                HashSet<int> itemsWithOverlappingDiscounts,
                HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly)
            {
                if (this.QuantityLevelTrend != LevelTrend.DealBetterAsLevelIncreases)
                {
                    // Optimizate only for quantity discount where deal better as quantity level increases.
                    return;
                }
    
                using (SimpleProfiler profiler = new SimpleProfiler("MultibuyDiscount.OptimizeDiscountLineNumberAndItemGroupIndexLookups"))
                {
                    // [Optimization] 1. optimize overlapped item quantity to some needed discount lines.
                    bool keepGoing = true;
                    int maxTries = 100;
                    Dictionary<decimal, decimal> discountLineNumberToNonOverlappedQuantityLookup = new Dictionary<decimal, decimal>();
                    while (keepGoing && maxTries-- > 0)
                    {
                        // [Optimization] count non-overlapping quantity for each discount line.
                        foreach (KeyValuePair<decimal, HashSet<int>> pair in this.DiscountLineNumberToItemGroupIndexSetMap)
                        {
                            decimal discountLineNumber = pair.Key;
                            if (!this.DiscountLineNumberSetMakingHighestQuantity.Contains(discountLineNumber))
                            {
                                // Count non-overlapped (externally and internally) quantity for the discount line.
                                decimal totalQuantityNotOverlapped = decimal.Zero;
                                foreach (int itemGroupIndex in pair.Value)
                                {
                                    bool isItemGroupIndexOverlappedWithNonCompoundedDiscount = this.IsItemIndexGroupOverlappedWithNonCompoundedDiscounts(
                                        itemGroupIndex,
                                        itemsWithOverlappingDiscounts,
                                        itemsWithOverlappingDiscountsCompoundedOnly);
    
                                    if (!isItemGroupIndexOverlappedWithNonCompoundedDiscount)
                                    {
                                        HashSet<decimal> discountLineNumberSet = null;
                                        if (this.ItemGroupIndexToDiscountLineNumberSetMap.TryGetValue(itemGroupIndex, out discountLineNumberSet))
                                        {
                                            if (discountLineNumberSet.Count == 1)
                                            {
                                                totalQuantityNotOverlapped += quantitiesApplicable[itemGroupIndex];
                                            }
                                        }
                                    }
                                }
    
                                discountLineNumberToNonOverlappedQuantityLookup[discountLineNumber] = totalQuantityNotOverlapped;
                            }
                        }
    
                        // [Optimization] some intra-overlapped item has high quantity and all related discount lines can make the highest quantity as a result.
                        // Associate the item with only one discount line, and make all related discount line numbers in DiscountLineNumberSetMakingHighestQuantity.
                        foreach (KeyValuePair<int, HashSet<decimal>> pair in this.ItemGroupIndexToDiscountLineNumberSetMap)
                        {
                            int itemGroupIndex = pair.Key;
                            HashSet<decimal> discountLineNumberSetForItemGroupIndex = pair.Value;
                            if (discountLineNumberSetForItemGroupIndex.Count > 1)
                            {
                                bool isItemGroupIndexOverlappedWithNonCompoundedDiscount = this.IsItemIndexGroupOverlappedWithNonCompoundedDiscounts(
                                    itemGroupIndex,
                                    itemsWithOverlappingDiscounts,
                                    itemsWithOverlappingDiscountsCompoundedOnly);
    
                                // Intra-overlapped only
                                if (!isItemGroupIndexOverlappedWithNonCompoundedDiscount)
                                {
                                    decimal existingQuantityFromOtherItems = decimal.Zero;
                                    foreach (decimal discountLineNumber in discountLineNumberSetForItemGroupIndex)
                                    {
                                        decimal quantityNonOverlappedForDiscountLineNumber = decimal.Zero;
                                        if (discountLineNumberToNonOverlappedQuantityLookup.TryGetValue(discountLineNumber, out quantityNonOverlappedForDiscountLineNumber))
                                        {
                                            existingQuantityFromOtherItems += Math.Min(this.HighestQuantityLevel, quantityNonOverlappedForDiscountLineNumber);
                                        }
                                    }
    
                                    // The item has enough quantity to make all related discount lines get the highest quantity.
                                    // Save all related discount line number in DiscountLineNumberSetMakingHighestQuantity.
                                    if (quantitiesApplicable[itemGroupIndex] + existingQuantityFromOtherItems >= this.HighestQuantityLevel * discountLineNumberSetForItemGroupIndex.Count)
                                    {
                                        this.DiscountLineNumberSetMakingHighestQuantity.AddRange(discountLineNumberSetForItemGroupIndex);
    
                                        // Remove itemGroupIndex and discountLineNumber association except the first one.
                                        bool isFirst = true;
                                        decimal firstDiscountLineNumber = decimal.Zero;
                                        foreach (decimal discountLineNumber in discountLineNumberSetForItemGroupIndex)
                                        {
                                            if (isFirst)
                                            {
                                                isFirst = false;
                                                firstDiscountLineNumber = discountLineNumber;
                                            }
                                            else
                                            {
                                                HashSet<int> itemGroupIndexSetForDiscountLineNumber = null;
    
                                                if (this.DiscountLineNumberToItemGroupIndexSetMap.TryGetValue(discountLineNumber, out itemGroupIndexSetForDiscountLineNumber))
                                                {
                                                    itemGroupIndexSetForDiscountLineNumber.Remove(itemGroupIndex);
                                                }
                                            }
                                        }
    
                                        discountLineNumberSetForItemGroupIndex.Clear();
                                        discountLineNumberSetForItemGroupIndex.Add(firstDiscountLineNumber);
                                    }
                                }
                            }
                        }
    
                        // [Optimization] if overlapping itemGroupIdex from discount line which already makes up for the highest quantity level, so other discount lines can benefit from it.
                        //                in the meantime, save it in itemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount so we don't lose it if it doesn't makes the highest quantity level.
                        bool isOptimized = false;
                        foreach (KeyValuePair<decimal, decimal> pair in discountLineNumberToNonOverlappedQuantityLookup)
                        {
                            decimal discountLineNumber = pair.Key;
                            if (pair.Value >= this.HighestQuantityLevel)
                            {
                                this.DiscountLineNumberSetMakingHighestQuantity.Add(discountLineNumber);
                            }
    
                            if (this.DiscountLineNumberSetMakingHighestQuantity.Contains(discountLineNumber))
                            {
                                HashSet<int> itemGroupIndexSet = this.DiscountLineNumberToItemGroupIndexSetMap[discountLineNumber];
                                HashSet<int> itemGroupIndexSetToRemoveFromThisDiscountLine = new HashSet<int>();
                                foreach (int itemGroupIndex in itemGroupIndexSet)
                                {
                                    bool isItemGroupIndexOverlappedWithNonCompoundedDiscount = this.IsItemIndexGroupOverlappedWithNonCompoundedDiscounts(
                                        itemGroupIndex,
                                        itemsWithOverlappingDiscounts,
                                        itemsWithOverlappingDiscountsCompoundedOnly);
    
                                    if (isItemGroupIndexOverlappedWithNonCompoundedDiscount)
                                    {
                                        // Overlapping with external discounts.
                                        // Save it in itemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount so it can made a simple discount with quantity 1.
                                        if (!this.ItemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount.ContainsKey(itemGroupIndex))
                                        {
                                            this.ItemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount.Add(itemGroupIndex, discountLineNumber);
                                        }
                                    }
                                    else
                                    {
                                        // No overlap with external discounts.
                                        HashSet<decimal> discountLineNumberSet = null;
                                        if (this.ItemGroupIndexToDiscountLineNumberSetMap.TryGetValue(itemGroupIndex, out discountLineNumberSet))
                                        {
                                            if (discountLineNumberSet.Count > 1)
                                            {
                                                // Intra-overlapped, to be removed from this discount line, so it can be used by others.
                                                itemGroupIndexSetToRemoveFromThisDiscountLine.Add(itemGroupIndex);
    
                                                // Save it in itemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount so we don't lose it if it doesn't makes the highest quantity level.
                                                if (!this.ItemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount.ContainsKey(itemGroupIndex))
                                                {
                                                    this.ItemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount.Add(itemGroupIndex, discountLineNumber);
                                                }
                                            }
                                            else
                                            {
                                                // Already fully covered and non-overlapping anymore, remove it from itemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount
                                                if (this.ItemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount.ContainsKey(itemGroupIndex))
                                                {
                                                    this.ItemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount.Remove(itemGroupIndex);
                                                }
                                            }
                                        }
                                    }
                                }
    
                                if (itemGroupIndexSetToRemoveFromThisDiscountLine.Any())
                                {
                                    // Remove it from ItemGroupIndexToDiscountLineNumberSetMap and DiscountLineNumberToItemGroupIndexSetMap
                                    foreach (int itemGroupIndex in itemGroupIndexSetToRemoveFromThisDiscountLine)
                                    {
                                        HashSet<decimal> discountLineNumberSet = null;
                                        if (this.ItemGroupIndexToDiscountLineNumberSetMap.TryGetValue(itemGroupIndex, out discountLineNumberSet))
                                        {
                                            discountLineNumberSet.Remove(discountLineNumber);
                                        }
                                    }
    
                                    itemGroupIndexSet.ExceptWith(itemGroupIndexSetToRemoveFromThisDiscountLine);
                                    isOptimized = true;
                                }
                            }
                        }
    
                        // Only try again if it's optimized;
                        keepGoing = isOptimized;
                    }
                }
            }
    
            private bool HasFractionalLevel()
            {
                bool hasFractionalLevel = false;
    
                foreach (QuantityDiscountLevel level in this.QuantityDiscountLevels)
                {
                    hasFractionalLevel = DiscountableItemGroup.IsFraction(level.MinimumQuantity);
                    if (hasFractionalLevel)
                    {
                        break;
                    }
                }
    
                return hasFractionalLevel;
            }
    
            private bool HasFractionalQuantity(DiscountableItemGroup[] discountableItemGroups, RetailDiscountLine retailDiscountLine)
            {
                bool hasFranctionalQuantity = false;
    
                HashSet<int> itemGroupIndexSet = null;
                if (this.DiscountLineNumberToItemGroupIndexSetMap.TryGetValue(retailDiscountLine.DiscountLineNumber, out itemGroupIndexSet))
                {
                    foreach (int itemGroupIndex in itemGroupIndexSet)
                    {
                        DiscountableItemGroup discountableItemGroup = discountableItemGroups[itemGroupIndex];
                        if (this.IsDiscountLineCoveringItem(retailDiscountLine.DiscountLineNumber, itemGroupIndex) &&
                            DiscountableItemGroup.IsFraction(discountableItemGroup.Quantity))
                        {
                            hasFranctionalQuantity = true;
                            break;
                        }
                    }
                }
    
                return hasFranctionalQuantity;
            }
    
            private void AddNonOverlappingItemsAsOneDiscountAppliationAndMoreToResult(
                DiscountableItemGroup[] discountableItemGroups,
                RetailDiscountLine retailDiscountLine,
                List<DiscountApplication> result,
                string discountCodeUsed,
                HashSet<int> itemsWithOverlappingDiscounts)
            {
                List<RetailDiscountLineItem> discountLineItems = new List<RetailDiscountLineItem>();
                Dictionary<int, decimal> itemQuantities = new Dictionary<int, decimal>();
    
                QuantityDiscountLevel quantityLevel = this.GetQuantityLevel(this.HighestQuantityLevel, retailDiscountLine.DiscountLineNumber);
    
                if (quantityLevel != null)
                {
                    foreach (int itemGroupIndex in this.DiscountLineNumberToItemGroupIndexSetMap[retailDiscountLine.DiscountLineNumber])
                    {
                        if (!this.HasOverlap(itemGroupIndex, itemsWithOverlappingDiscounts))
                        {
                            // For the rest of items without overlap, lump them all in one discount application with all applicable quantities.
                            DiscountableItemGroup item = discountableItemGroups[itemGroupIndex];
                            discountLineItems.Add(new RetailDiscountLineItem(itemGroupIndex, retailDiscountLine));
                            itemQuantities[itemGroupIndex] = item.Quantity;
                        }
                        else if (!this.ItemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount.ContainsKey(itemGroupIndex))
                        {
                            this.ItemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount.Add(itemGroupIndex, retailDiscountLine.DiscountLineNumber);
                        }
                    }
    
                    this.AddDiscountApplicationToAvailableResult(
                        discountableItemGroups,
                        discountLineItems,
                        retailDiscountLine,
                        quantityLevel.DiscountPriceOrPercent,
                        itemQuantities,
                        discountCodeUsed,
                        result,
                        applyStandalone: true,
                        setNumberOfTimesApplicable: false);
                }
            }
    
            private void AddAllQualifiedQuantitiesAsOneDiscountAppliationToResult(
                DiscountableItemGroup[] discountableItemGroups,
                RetailDiscountLine retailDiscountLine,
                decimal[] remainingQuantities,
                List<DiscountApplication> result,
                string discountCodeUsed)
            {
                List<RetailDiscountLineItem> discountLineItems = new List<RetailDiscountLineItem>();
                Dictionary<int, decimal> itemQuantities = new Dictionary<int, decimal>();
                decimal totalQuantity = 0;
    
                HashSet<int> itemGroupIndexSet = null;
                if (this.DiscountLineNumberToItemGroupIndexSetMap.TryGetValue(retailDiscountLine.DiscountLineNumber, out itemGroupIndexSet))
                {
                    foreach (int itemGroupIndex in itemGroupIndexSet)
                    {
                        discountLineItems.Add(new RetailDiscountLineItem(itemGroupIndex, retailDiscountLine));
                        decimal quantity = remainingQuantities[itemGroupIndex];
                        itemQuantities[itemGroupIndex] = quantity;
                        totalQuantity += quantity;
                    }
    
                    QuantityDiscountLevel quantityLevel = this.GetQuantityLevel(totalQuantity, retailDiscountLine.DiscountLineNumber);
    
                    if (quantityLevel != null)
                    {
                        this.AddDiscountApplicationToAvailableResult(
                            discountableItemGroups,
                            discountLineItems,
                            retailDiscountLine,
                            quantityLevel.DiscountPriceOrPercent,
                            itemQuantities,
                            discountCodeUsed,
                            result,
                            applyStandalone: true,
                            setNumberOfTimesApplicable: false);
                    }
                }
            }
    
            /// <summary>
            /// Adds a discount application to the available discount applications result.
            /// </summary>
            /// <param name="discountableItemGroups">The item groups on the transaction.</param>
            /// <param name="discountCodeUsed">The discount code used for triggering this discount.</param>
            /// <param name="result">The collection of DiscountApplication objects that will be included in the result.</param>
            /// <param name="itemsUsed">The items used for this particular application.</param>
            /// <param name="line">The discount line used for this particular application.</param>
            private void AddDiscountApplicationToAvailableResult(
                DiscountableItemGroup[] discountableItemGroups,
                string discountCodeUsed,
                List<DiscountApplication> result,
                Stack<int> itemsUsed,
                RetailDiscountLine line)
            {
                // We don't put duplicate line items in DiscountApplication, instead we rely on DiscountApplication.ItemQuantities to tell applicable quantity for each line item.
                HashSet<int> itemIndexSet = new HashSet<int>();
                Dictionary<int, decimal> itemQuantities = new Dictionary<int, decimal>();
                bool hasItemsNotInOptimizedItemList = false;
                foreach (int x in itemsUsed)
                {
                    itemIndexSet.Add(x);
                    DiscountBase.AddToItemQuantities(itemQuantities, x, 1m);
                    if (!this.ItemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount.ContainsKey(x))
                    {
                        hasItemsNotInOptimizedItemList = true;
                    }
                }
    
                if (!hasItemsNotInOptimizedItemList)
                {
                    // All items have been covered by the optimized items.
                    return;
                }
    
                List<RetailDiscountLineItem> discountLineItems = new List<RetailDiscountLineItem>(itemIndexSet.Count);
                foreach (int itemIndex in itemIndexSet)
                {
                    discountLineItems.Add(new RetailDiscountLineItem(itemIndex, line));
                }
    
                QuantityDiscountLevel quantityLevel = this.GetQuantityLevel(itemsUsed.Count, line.DiscountLineNumber);
    
                if (quantityLevel != null)
                {
                    this.AddDiscountApplicationToAvailableResult(
                        discountableItemGroups,
                        discountLineItems,
                        line,
                        quantityLevel.DiscountPriceOrPercent,
                        itemQuantities,
                        discountCodeUsed,
                        result,
                        applyStandalone: false,
                        setNumberOfTimesApplicable: true);
                }
            }
    
            private void AddDiscountApplicationToAvailableResult(
                DiscountableItemGroup[] discountableItemGroups,
                List<RetailDiscountLineItem> discountLineItems,
                RetailDiscountLine retailDiscountLine,
                decimal discountUnitPriceOrPercent,
                Dictionary<int, decimal> itemQuantites,
                string discountCodeUsed,
                List<DiscountApplication> result,
                bool applyStandalone,
                bool setNumberOfTimesApplicable)
            {
                this.AddDiscountApplicationToAvailableResult(
                    discountableItemGroups,
                    discountLineItems,
                    retailDiscountLine,
                    discountUnitPriceOrPercent,
                    itemQuantites,
                    discountCodeUsed,
                    result,
                    applyStandalone,
                    setNumberOfTimesApplicable,
                    removeItemsFromLookupsWhenApplied: false);
            }
    
            private void AddDiscountApplicationToAvailableResult(
                DiscountableItemGroup[] discountableItemGroups,
                List<RetailDiscountLineItem> discountLineItems,
                RetailDiscountLine retailDiscountLine,
                decimal discountUnitPriceOrPercent,
                Dictionary<int, decimal> itemQuantites,
                string discountCodeUsed,
                List<DiscountApplication> result,
                bool applyStandalone,
                bool setNumberOfTimesApplicable,
                bool removeItemsFromLookupsWhenApplied)
            {
                DiscountApplication app = new DiscountApplication(this, applyStandalone, removeItemsFromLookupsWhenApplied)
                {
                    RetailDiscountLines = discountLineItems,
                    SortIndex = this.GetSortIndexForRetailDiscountLine(retailDiscountLine),
                    SortValue = this.GetSortValue(discountableItemGroups, discountLineItems, itemQuantites, discountUnitPriceOrPercent),
                    DiscountCode = discountCodeUsed,
                    DealPriceValue = this.DiscountType == DiscountMethodType.DealPrice ? discountUnitPriceOrPercent : decimal.Zero,
                    DiscountPercentValue = this.DiscountType == DiscountMethodType.DiscountPercent ? discountUnitPriceOrPercent : decimal.Zero,
                    DiscountAmountValue = decimal.Zero,
                };
                app.ItemQuantities.AddRange(itemQuantites);
    
                if (setNumberOfTimesApplicable)
                {
                    this.SetNumberOfTimesAppliedForDiscountApplication(app);
                }
    
                result.Add(app);
            }
    
            private OptimizationOverlapType CalculateOptimizationOverlapType(
                decimal discountLineNumber,
                decimal[] quantityRemaining,
                HashSet<int> itemsWithOverlappingDiscounts)
            {
                OptimizationOverlapType overlapType = OptimizationOverlapType.TooMuchOverlap;
    
                decimal nonOverlappedItemQuantities = decimal.Zero;
                bool hasOverlap = false;
                bool hasOverlapWithItemsOptimizedForSimpleDiscount = false;
    
                foreach (int itemGroupIndex in this.DiscountLineNumberToItemGroupIndexSetMap[discountLineNumber])
                {
                    if (this.HasOverlap(itemGroupIndex, itemsWithOverlappingDiscounts))
                    {
                        hasOverlap = true;
                    }
                    else
                    {
                        nonOverlappedItemQuantities += quantityRemaining[itemGroupIndex];
                    }
    
                    if (this.ItemGroupIndexToDiscountLineNumberLookupOptimizedForSimpleDiscount.ContainsKey(itemGroupIndex))
                    {
                        hasOverlapWithItemsOptimizedForSimpleDiscount = true;
                    }
                }
    
                if (hasOverlap || hasOverlapWithItemsOptimizedForSimpleDiscount)
                {
                    if (nonOverlappedItemQuantities >= this.HighestQuantityLevel)
                    {
                        overlapType = OptimizationOverlapType.OverlappedButRestMadeUpForHighestQuantity;
                    }
                    else
                    {
                        overlapType = OptimizationOverlapType.TooMuchOverlap;
                    }
                }
                else
                {
                    overlapType = OptimizationOverlapType.NoOverlap;
                }
    
                return overlapType;
            }
    
            private decimal GetEstimatedDiscountAmount(
                QuantityDiscountLevel quantityLevel,
                Dictionary<int, decimal> itemPriceLookup,
                Dictionary<int, decimal> itemQuantityLookup,
                HashSet<int> itemsWithOverlappingDiscounts,
                HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly,
                bool includeOverlapped)
            {
                decimal estimatedDiscountAmount = decimal.Zero;
    
                foreach (KeyValuePair<int, decimal> pair in itemPriceLookup)
                {
                    int itemGroupIndex = pair.Key;
                    if (includeOverlapped || !this.IsItemIndexGroupOverlappedWithNonCompoundedDiscounts(itemGroupIndex, itemsWithOverlappingDiscounts, itemsWithOverlappingDiscountsCompoundedOnly))
                    {
                        decimal price = pair.Value;
                        decimal quantity = itemQuantityLookup[itemGroupIndex];
    
                        decimal estimatedItemDiscountAmount = decimal.Zero;
    
                        if (this.DiscountType == DiscountMethodType.DiscountPercent)
                        {
                            estimatedItemDiscountAmount = DiscountBase.GetDiscountAmountForPercentageOff(price, quantityLevel.DiscountPriceOrPercent) * quantity;
                        }
                        else
                        {
                            estimatedItemDiscountAmount = quantity * (price - quantityLevel.DiscountPriceOrPercent);
                            estimatedItemDiscountAmount = Math.Max(estimatedItemDiscountAmount, decimal.Zero);
                        }
    
                        estimatedDiscountAmount += estimatedItemDiscountAmount;
                    }
                }
    
                return estimatedDiscountAmount;
            }
    
            internal class DiscountLineQuantityComparer
            {
                private Dictionary<decimal, HashSet<int>> discountLineNumberToNonOverlapItemGroupIndexSetLookup;
                private Dictionary<decimal, HashSet<int>> discountLineNumberToOverlapItemGroupIndexSetLookup;
                private decimal[] remainingQuantities;
    
                internal DiscountLineQuantityComparer(
                    Dictionary<decimal, HashSet<int>> discountLineNumberToNonOverlapItemGroupIndexSetLookup,
                    Dictionary<decimal, HashSet<int>> discountLineNumberToOverlapItemGroupIndexSetLookup,
                    decimal[] remainingQuantities)
                {
                    this.discountLineNumberToNonOverlapItemGroupIndexSetLookup = discountLineNumberToNonOverlapItemGroupIndexSetLookup;
                    this.discountLineNumberToOverlapItemGroupIndexSetLookup = discountLineNumberToOverlapItemGroupIndexSetLookup;
                    this.remainingQuantities = remainingQuantities;
                }

                internal int CompareQuantityByDiscountLineNumber(decimal left, decimal right)
                {
                    // It results in descending order by quantity.
                    int ret = Math.Sign(this.GetQuantitySum(this.discountLineNumberToNonOverlapItemGroupIndexSetLookup, right) - this.GetQuantitySum(this.discountLineNumberToNonOverlapItemGroupIndexSetLookup, left));
    
                    if (ret == 0)
                    {
                        ret = Math.Sign(this.GetQuantitySum(this.discountLineNumberToOverlapItemGroupIndexSetLookup, right) - this.GetQuantitySum(this.discountLineNumberToOverlapItemGroupIndexSetLookup, left));
                    }
    
                    return ret;
                }

                private decimal GetQuantitySum(
                    Dictionary<decimal, HashSet<int>> discountLineNumberToItemGroupIndexSetLookup,
                    decimal discountLineNumber)
                {
                    decimal quantitySum = decimal.Zero;
                    HashSet<int> itemGroupIndexSet = null;

                    if (discountLineNumberToItemGroupIndexSetLookup != null &&
                        discountLineNumberToItemGroupIndexSetLookup.TryGetValue(discountLineNumber, out itemGroupIndexSet))
                    {
                        if (itemGroupIndexSet.Count > 0)
                        {
                            quantitySum = itemGroupIndexSet.Sum(p => this.remainingQuantities[p]);
                        }
                    }

                    return quantitySum;
                }
            }
        }
    }
}
