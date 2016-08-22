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
        using System.Diagnostics.CodeAnalysis;
        using System.Linq;
        using System.Text;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// This class implements the mix and match discount processing, including the determination of which ways 
        /// the discount can apply to the transaction and the value of the discount applied to specific lines.
        /// </summary>
        public class MixAndMatchDiscount : DiscountBase
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Silly code analysis.")]
            private const decimal QuantityOne = 1m;

            /// <summary>
            /// Initializes a new instance of the <see cref="MixAndMatchDiscount" /> class.
            /// </summary>
            /// <param name="validationPeriod">Validation period.</param>
            public MixAndMatchDiscount(ValidationPeriod validationPeriod)
                : base(validationPeriod)
            {
                this.LineGroupToNumberOfItemsMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                this.LineGroupToItemGroupIndexSetLookup = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);
                this.LineGroupToDiscountLineNumberSetLookup = new Dictionary<string, HashSet<decimal>>(StringComparer.OrdinalIgnoreCase);
                this.ItemGroupIndexToLineGroupSetLookup = new Dictionary<int, HashSet<string>>();
            }

            internal enum OptimizationOverlapType
            {
                ExternalOverlap,
                InternalOverlap,
                NoOverlap,
            }

            /// <summary>
            /// Gets or sets the number of least-expensive lines to use in triggering this offer.
            /// </summary>
            public int NumberOfLeastExpensiveLines { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the least expensive favors retailer.
            /// </summary>
            public LeastExpensiveMode LeastExpensiveMode { get; set; }

            /// <summary>
            /// Gets the map of mix and match line group to number of items.
            /// </summary>
            public IDictionary<string, int> LineGroupToNumberOfItemsMap { get; private set; }

            /// <summary>
            /// Gets the line group to item group index set lookup.
            /// </summary>
            /// <remarks>This will be re-initialized for every GetDiscountApplications call, and we have sales and return.</remarks>
            internal IDictionary<string, HashSet<int>> LineGroupToItemGroupIndexSetLookup { get; private set; }

            /// <summary>
            /// Gets the line group to discount line number set lookup.
            /// </summary>
            /// <remarks>This will be re-initialized for every GetDiscountApplications call, and we have sales and return.</remarks>
            internal IDictionary<string, HashSet<decimal>> LineGroupToDiscountLineNumberSetLookup { get; private set; }

            /// <summary>
            /// Gets the item group index to the line group set lookup.
            /// </summary>
            /// <remarks>This will be re-initialized for every GetDiscountApplications call, and we have sales and return.</remarks>
            internal IDictionary<int, HashSet<string>> ItemGroupIndexToLineGroupSetLookup { get; private set; }

            internal bool IsLeastExpensiveAmountOff
            {
                get
                {
                    // %-off first, then $-off, and deal price last. See GetValueFromDiscountApplicationAndPrices.
                    return this.DiscountType == DiscountMethodType.LeastExpensive && this.DiscountPercentValue == decimal.Zero && this.DiscountAmountValue > decimal.Zero;
                }
            }

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

                isInterrupted = false;
                List<DiscountApplication> discountApplications = new List<DiscountApplication>();

                // If this discount cannot be applied, return an empty collection.
                if (discountableItemGroups == null
                    || remainingQuantities == null
                    || this.IsFinished)
                {
                    return discountApplications;
                }

                // Get the discount code to use for any discount lines, if one is required.
                string discountCodeUsed = this.GetDiscountCodeForDiscount(transaction);

                // Create a BitSet that can hold the valid items for both positive and negative quantities
                BitSet availableItemGroups = new BitSet((uint)discountableItemGroups.Length);

                for (int x = 0; x < discountableItemGroups.Length; x++)
                {
                    if (remainingQuantities[x] != decimal.Zero && this.ItemGroupIndexToDiscountLineNumberSetMap.ContainsKey(x))
                    {
                        availableItemGroups[x] = true;
                    }
                }

                // Now we have the different groups that could get a discount from this, as well as the discount lines that can apply.
                discountApplications.AddRange(this.GetDiscountApplicationsForItems(
                    availableItemGroups,
                    discountableItemGroups,
                    remainingQuantities,
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

                // If this discount cannot be applied, return an empty collection.
                if (discountableItemGroups == null
                    || remainingQuantities == null
                    || this.IsFinished)
                {
                    return discountApplications;
                }

                // Get the discount code to use for any discount lines, if one is required.
                string discountCodeUsed = this.GetDiscountCodeForDiscount(transaction);
                IEnumerable<DiscountApplication> optimizedDiscountApplications;
                if (this.TryOpitimizeDiscountApplicationsForItems(
                    discountableItemGroups,
                    remainingQuantities,
                    appliedDiscounts,
                    itemsWithOverlappingDiscounts,
                    discountCodeUsed,
                    out optimizedDiscountApplications))
                {
                    discountApplications.AddRange(optimizedDiscountApplications);
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
                if (discountApplication == null || discountableItemGroups == null)
                {
                    return null;
                }

                bool includeAmountOff = true;
                bool includePercentageOff = true;
                this.FigureOutCompoundedPriceFlags(out includeAmountOff, out includePercentageOff);

                decimal[] prices = new decimal[discountableItemGroups.Length];
                Dictionary<int, IList<DiscountLineQuantity>> discountDictionary = this.GetExistingDiscountDictionaryAndDiscountedPrices(
                    discountableItemGroups,
                    remainingQuantities,
                    appliedDiscounts,
                    discountApplication,
                    includeAmountOff,
                    includePercentageOff,
                    prices);

                decimal result = this.GetValueFromDiscountApplicationAndPrices(discountApplication, prices, discountDictionary);

                AppliedDiscountApplication currentDiscountApplication = null;

                if (result > decimal.Zero)
                {
                    // Only generate the discount lines dictionary now if it's compounded.
                    bool isDiscountLineGenerated = this.CanCompound;
                    currentDiscountApplication = new AppliedDiscountApplication(
                        discountApplication,
                        result,
                        discountApplication.ItemQuantities,
                        isDiscountLineGenerated);
                    if (isDiscountLineGenerated)
                    {
                        currentDiscountApplication.ItemGroupIndexToDiscountLineQuantitiesLookup = this.GenerateDiscountLinesDictionary(
                            discountableItemGroups,
                            currentDiscountApplication,
                            priceContext);
                    }
                }

                return currentDiscountApplication;
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
                ThrowIf.Null(appliedDiscountApplication, "appliedDiscountApplication");
                ThrowIf.Null(discountableItemGroups, "discountableItemGroups");
                ThrowIf.Null(priceContext, "priceContext");

                appliedDiscountApplication.ItemGroupIndexToDiscountLineQuantitiesLookup = this.GenerateDiscountLinesDictionary(
                    discountableItemGroups,
                    appliedDiscountApplication,
                    priceContext);
            }

            internal static Comparison<MixAndMatchDiscount> GetComparisonForLeastExpensiveFavorRetailer()
            {
                return new Comparison<MixAndMatchDiscount>(MixAndMatchDiscount.CompareLeastExpensiveFavorRetailer);
            }

            internal static int CompareLeastExpensiveFavorRetailer(MixAndMatchDiscount x, MixAndMatchDiscount y)
            {
                int ret = -1;
                if (x != null && y != null)
                {
                    // First: concurrency Exclusive < Best price < Compounded
                    ret = x.ConcurrencyMode.CompareTo(y.ConcurrencyMode);

                    // Second: deal price, the lower the better.
                    if (ret == 0)
                    {
                        if (x.DealPriceValue > decimal.Zero && y.DealPriceValue > decimal.Zero)
                        {
                            ret = x.DealPriceValue.CompareTo(y.DealPriceValue);
                        }
                        else if (x.DealPriceValue > decimal.Zero)
                        {
                            ret = -1;
                        }
                        else if (y.DealPriceValue > decimal.Zero)
                        {
                            ret = 1;
                        }
                    }

                    // Third: $-off
                    if (ret == 0)
                    {
                        if (x.DiscountAmountValue > decimal.Zero && y.DiscountAmountValue > decimal.Zero)
                        {
                            ret = y.DiscountAmountValue.CompareTo(x.DiscountAmountValue);
                        }
                        else if (x.DiscountAmountValue > decimal.Zero)
                        {
                            ret = -1;
                        }
                        else if (y.DiscountAmountValue > decimal.Zero)
                        {
                            ret = 1;
                        }
                    }

                    // Fourth: %-off
                    if (ret == 0)
                    {
                        ret = y.DiscountPercentValue.CompareTo(x.DiscountPercentValue);
                    }
                }
                else if (y == null)
                {
                    ret = 1;
                }

                return ret;
            }

            internal IEnumerable<DiscountApplication> GetDiscountApplicationsForLeastExpensiveFavorRetailer(
                SalesTransaction transaction,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                IEnumerable<AppliedDiscountApplication> appliedDiscounts)
            {
                List<DiscountApplication> discountApplicationList = new List<DiscountApplication>();

                if (this.IsFinished ||
                    this.DiscountType != DiscountMethodType.LeastExpensive ||
                    this.LeastExpensiveMode == LeastExpensiveMode.FavorCustomer ||
                    this.LineGroupToNumberOfItemsMap.Min(p => p.Value) < this.NumberOfLeastExpensiveLines)
                {
                    return discountApplicationList;
                }

                // Step 0: get discounted prices
                decimal[] discountedPrices = new decimal[discountableItemGroups.Length];
                bool includeAmountOff = true;
                bool includePercentageOff = true;
                this.FigureOutCompoundedPriceFlags(out includeAmountOff, out includePercentageOff);

                decimal[] remainingQuantitiesLocal = (decimal[])remainingQuantities.Clone();
                this.GetExistingDiscountDictionaryAndDiscountedPrices(
                    discountableItemGroups,
                    remainingQuantitiesLocal,
                    appliedDiscounts,
                    new HashSet<int>(this.ItemGroupIndexToLineGroupSetLookup.Keys),
                    includeAmountOff,
                    includePercentageOff,
                    discountedPrices);

                Dictionary<int, decimal> discountedPricesLookup = new Dictionary<int, decimal>();
                for (int itemGroupIndex = 0; itemGroupIndex < discountedPrices.Length; itemGroupIndex++)
                {
                    discountedPricesLookup.Add(itemGroupIndex, discountedPrices[itemGroupIndex]);
                }

                // Step 1: construct line-group-overlapped item group index set.
                HashSet<int> overlappedItemGroupIndexSet = this.GetOverlappedItemGroupIndexSet(remainingQuantities);

                // Step 2: distribute overlapped quantities if any to lineGroupToItemGroupIndexToAllocatedQuantityLookup
                Dictionary<string, decimal> lineGroupToQuantityLookup = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, Dictionary<int, decimal>> lineGroupToItemGroupIndexToAllocatedQuantityLookup = new Dictionary<string, Dictionary<int, decimal>>(StringComparer.OrdinalIgnoreCase);
                if (overlappedItemGroupIndexSet.Any())
                {
                    this.AllocateOverlappedItemsToLineGroups(
                        lineGroupToQuantityLookup,
                        lineGroupToItemGroupIndexToAllocatedQuantityLookup,
                        overlappedItemGroupIndexSet,
                        remainingQuantitiesLocal);
                }

                // Step 3: build line group to sorted item group index array (by item price descending) lookup. 
                // Later in final step, we'll use items from highest price to lowest for each line group.
                Dictionary<string, int[]> lineGroupToSortedItemGroupIndexArrayLookup = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
                this.BuildLineGroupToSortedItemGroupIndexArrayLookup(
                    lineGroupToSortedItemGroupIndexArrayLookup,
                    lineGroupToItemGroupIndexToAllocatedQuantityLookup,
                    overlappedItemGroupIndexSet,
                    discountedPricesLookup);

                // Step 4: Construct discount applications, each time with the least priced items, repeatatively until no more.
                string discountCodeUsed = this.GetDiscountCodeForDiscount(transaction);
                discountApplicationList = this.ConstructDiscountApplicationsForLeastExpensiveFavorRetailer(
                    lineGroupToSortedItemGroupIndexArrayLookup,
                    discountableItemGroups,
                    remainingQuantitiesLocal,
                    lineGroupToItemGroupIndexToAllocatedQuantityLookup,
                    overlappedItemGroupIndexSet,
                    discountedPrices,
                    discountCodeUsed);

                return discountApplicationList;
            }

            /// <summary>
            /// Prepare line group to discount line number set lookup, line group to item group index set lookup, and item group index to line group set lookup.
            /// </summary>
            /// <param name="discountableItemGroups">The discountable item groups.</param>
            /// <param name="remainingQuantities">Remaining quantities.</param>
            /// <remarks>It's private, exposed for testing.</remarks>
            internal void PrepareLineGroupLookups(DiscountableItemGroup[] discountableItemGroups, decimal[] remainingQuantities)
            {
                using (SimpleProfiler profiler = new SimpleProfiler("MixAndMatch.PrepareLineGroupLookups", 2))
                {
                    // DISCOUNTPERF: In mix and match setup in AX, make it impossible for an item to belong to the same line group of 2 different discount lines.
                    this.LineGroupToItemGroupIndexSetLookup.Clear();
                    this.LineGroupToDiscountLineNumberSetLookup.Clear();
                    this.ItemGroupIndexToLineGroupSetLookup.Clear();

                    // Prepare 3 lookups: 
                    //   line group to discount line number set
                    //   line group to item group index set
                    //   item group index set to line group set
                    for (int itemGroupIndex = 0; itemGroupIndex < discountableItemGroups.Length; itemGroupIndex++)
                    {
                        decimal itemQuantity = remainingQuantities[itemGroupIndex];
                        if (itemQuantity > decimal.Zero)
                        {
                            HashSet<decimal> discountLineNumberSet = null;
                            if (this.ItemGroupIndexToDiscountLineNumberSetMap.TryGetValue(itemGroupIndex, out discountLineNumberSet))
                            {
                                foreach (decimal discountLineNumber in discountLineNumberSet)
                                {
                                    RetailDiscountLine discountLine = this.DiscountLines[discountLineNumber];

                                    HashSet<decimal> discountLineNumberSetForLineGroup = null;
                                    if (this.LineGroupToDiscountLineNumberSetLookup.TryGetValue(discountLine.MixAndMatchLineGroup, out discountLineNumberSetForLineGroup))
                                    {
                                        discountLineNumberSetForLineGroup.Add(discountLine.DiscountLineNumber);
                                    }
                                    else
                                    {
                                        this.LineGroupToDiscountLineNumberSetLookup.Add(discountLine.MixAndMatchLineGroup, new HashSet<decimal>() { discountLine.DiscountLineNumber });
                                    }

                                    HashSet<int> itemGroupIndexSetForLineGroup = null;
                                    if (this.LineGroupToItemGroupIndexSetLookup.TryGetValue(discountLine.MixAndMatchLineGroup, out itemGroupIndexSetForLineGroup))
                                    {
                                        itemGroupIndexSetForLineGroup.Add(itemGroupIndex);
                                    }
                                    else
                                    {
                                        this.LineGroupToItemGroupIndexSetLookup.Add(discountLine.MixAndMatchLineGroup, new HashSet<int>() { itemGroupIndex });
                                    }

                                    HashSet<string> lineGroupSet = null;
                                    if (this.ItemGroupIndexToLineGroupSetLookup.TryGetValue(itemGroupIndex, out lineGroupSet))
                                    {
                                        lineGroupSet.Add(discountLine.MixAndMatchLineGroup);
                                    }
                                    else
                                    {
                                        this.ItemGroupIndexToLineGroupSetLookup.Add(itemGroupIndex, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { discountLine.MixAndMatchLineGroup });
                                    }
                                }
                            }
                        }
                    }

                    // Try merging discount lines for line specific: if two discount lines of the same line group has identical type and value. In other words, remove duplicate.
                    if (this.DiscountType == DiscountMethodType.LineSpecific)
                    {
                        foreach (KeyValuePair<string, HashSet<decimal>> pair in this.LineGroupToDiscountLineNumberSetLookup)
                        {
                            HashSet<decimal> discountLineNumberSet = pair.Value;

                            int discountLineCount = discountLineNumberSet.Count;
                            if (discountLineCount >= 2)
                            {
                                // Build discount line to discount line replacements.
                                Dictionary<decimal, decimal> discountLineReplacements = new Dictionary<decimal, decimal>();
                                decimal[] discountLineArray = discountLineNumberSet.ToArray();

                                for (int left = 0; left < discountLineCount; left++)
                                {
                                    decimal leftDiscountLineNumber = discountLineArray[left];
                                    if (!discountLineReplacements.ContainsKey(leftDiscountLineNumber))
                                    {
                                        RetailDiscountLine leftDiscountLine = this.DiscountLines[leftDiscountLineNumber];
                                        for (int right = left + 1; right < discountLineCount; right++)
                                        {
                                            decimal rightDiscountLineNumber = discountLineArray[right];
                                            if (!discountLineReplacements.ContainsKey(rightDiscountLineNumber))
                                            {
                                                RetailDiscountLine rightDiscountLine = this.DiscountLines[rightDiscountLineNumber];
                                                if (leftDiscountLine.DiscountMethod == rightDiscountLine.DiscountMethod &&
                                                    leftDiscountLine.DiscountLinePercentOrValue == rightDiscountLine.DiscountLinePercentOrValue)
                                                {
                                                    discountLineReplacements.Add(rightDiscountLineNumber, leftDiscountLineNumber);
                                                }
                                            }
                                        }
                                    }
                                }

                                // Move itemGroupIndexSet for to-be-replaced discount line to the replacement discount line.
                                foreach (KeyValuePair<decimal, decimal> replacementPair in discountLineReplacements)
                                {
                                    decimal discountLineNumberToReplace = replacementPair.Key;
                                    decimal discountLineNumberReplacement = replacementPair.Value;

                                    // Move itemGroupIndexSet from discountLineToReplace to discountLineReplacement in this.DiscountLineNumberToItemGroupIndexSetMap
                                    HashSet<int> itemGroupIndexSetToMove = null;
                                    if (this.DiscountLineNumberToItemGroupIndexSetMap.TryGetValue(discountLineNumberToReplace, out itemGroupIndexSetToMove))
                                    {
                                        HashSet<int> itemGroupIndexSetDestination = null;

                                        if (this.DiscountLineNumberToItemGroupIndexSetMap.TryGetValue(discountLineNumberReplacement, out itemGroupIndexSetDestination))
                                        {
                                            itemGroupIndexSetDestination.AddRange(itemGroupIndexSetToMove);
                                        }
                                        else
                                        {
                                            this.DiscountLineNumberToItemGroupIndexSetMap.Add(discountLineNumberReplacement, itemGroupIndexSetToMove);
                                        }

                                        this.DiscountLineNumberToItemGroupIndexSetMap.Remove(discountLineNumberToReplace);
                                    }

                                    // Replace discount line number in this.ItemGroupIndexToDiscountLineNumberSetMap 
                                    foreach (KeyValuePair<int, HashSet<decimal>> itemGroupIndexDiscountLinesPair in this.ItemGroupIndexToDiscountLineNumberSetMap)
                                    {
                                        HashSet<decimal> discountLineNumberSetForItem = itemGroupIndexDiscountLinesPair.Value;
                                        if (discountLineNumberSetForItem.Contains(discountLineNumberToReplace))
                                        {
                                            discountLineNumberSetForItem.Remove(discountLineNumberToReplace);
                                            discountLineNumberSetForItem.Add(discountLineNumberReplacement);
                                        }
                                    }

                                    // Remove discount line number in this.LineGroupToDiscountLineNumberSetLookup
                                    discountLineNumberSet.Remove(discountLineNumberToReplace);
                                }
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Gets lookup of line group to item group index array order by price descending.
            /// </summary>
            /// <param name="discountedPrices">Discounted prices.</param>
            /// <returns>Lookup of line group to item group index array order by price descending.</returns>
            /// <remarks>It's private, exposed for testing.</remarks>
            internal IDictionary<string, int[]> GetLineGroupToItemGroupIndexDescendingByPriceArrayLookup(decimal[] discountedPrices)
            {
                IDictionary<string, int[]> lineGroupToOrderedItemGroupIndexArrayLookup = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);

                foreach (KeyValuePair<string, HashSet<int>> pair in this.LineGroupToItemGroupIndexSetLookup)
                {
                    HashSet<int> itemGroupIndexSet = pair.Value;
                    int[] itemGroupIndicesSorted = new int[itemGroupIndexSet.Count];
                    Dictionary<int, decimal> itemPriceLookupForSorting = new Dictionary<int, decimal>();
                    int i = 0;
                    foreach (int itemGroupIndex in itemGroupIndexSet)
                    {
                        itemGroupIndicesSorted[i] = itemGroupIndex;
                        itemPriceLookupForSorting[itemGroupIndex] = discountedPrices[itemGroupIndex];
                        i++;
                    }

                    ItemPriceComparer itemPriceComparer = new ItemPriceComparer(itemPriceLookupForSorting);

                    Array.Sort(itemGroupIndicesSorted, itemPriceComparer.CompareItemPriceByItemGroupIndexDescending);

                    lineGroupToOrderedItemGroupIndexArrayLookup.Add(pair.Key, itemGroupIndicesSorted);
                }

                return lineGroupToOrderedItemGroupIndexArrayLookup;
            }

            /// <summary>
            /// Remove low priced items that won't make discount applications.
            /// </summary>
            /// <param name="discountedPrices">Discounted prices.</param>
            /// <param name="quantitiesRemaining">Quantities remaining.</param>
            /// <param name="lineGroupToOrderedItemGroupIndexArrayLookup">Line group to ordered item group index array lookup.</param>
            /// <remarks>It's private, exposed for testing.</remarks>
            internal void RemoveLowPricedItemsThatWontMakeDiscountApplications(
                decimal[] discountedPrices,
                decimal[] quantitiesRemaining,
                IDictionary<string, int[]> lineGroupToOrderedItemGroupIndexArrayLookup)
            {
                using (SimpleProfiler profiler = new SimpleProfiler("MixAndMatch.RemoveLowPricedItemsThatWontMakeDiscountApplications", 2))
                {
                    // Figure out number of discount applications, which is the least of number of discount applications possible for each line group.
                    int numberOfDiscountApplications = 0;
                    bool isInitializedumberOfDiscountApplications = false;
                    Dictionary<string, decimal> lineGroupToQuantityLookup = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                    foreach (KeyValuePair<string, int[]> pair in lineGroupToOrderedItemGroupIndexArrayLookup)
                    {
                        string lineGroup = pair.Key;
                        decimal quantity = decimal.Zero;
                        foreach (int itemGroupIndex in pair.Value)
                        {
                            quantity += quantitiesRemaining[itemGroupIndex];
                        }

                        decimal numberOfItemsNeededForLineGroup = this.LineGroupToNumberOfItemsMap[lineGroup];

                        int numberOfDiscountApplicationsPossible = numberOfItemsNeededForLineGroup > decimal.Zero ? (int)Math.Floor(quantity / numberOfItemsNeededForLineGroup) : 0;

                        if (numberOfDiscountApplicationsPossible == 0)
                        {
                            numberOfDiscountApplications = 0;
                            break;
                        }
                        else
                        {
                            lineGroupToQuantityLookup.Add(lineGroup, quantity);
                            if (!isInitializedumberOfDiscountApplications || numberOfDiscountApplicationsPossible < numberOfDiscountApplications)
                            {
                                numberOfDiscountApplications = numberOfDiscountApplicationsPossible;
                                isInitializedumberOfDiscountApplications = true;
                            }
                        }
                    }

                    if (numberOfDiscountApplications == 0 || lineGroupToQuantityLookup.Count < this.LineGroupToNumberOfItemsMap.Count)
                    {
                        // Not applicable.
                        for (int itemGroupIndex = 0; itemGroupIndex < quantitiesRemaining.Length; itemGroupIndex++)
                        {
                            // It's okay to remove everything, including items not covered by the mix and match discount, as quantitiesRemaining is local for the mix and match discount.
                            quantitiesRemaining[itemGroupIndex] = decimal.Zero;
                        }
                    }
                    else
                    {
                        // Remove not-needed quantities of low priced items for each line group by the number of discount applications.
                        foreach (KeyValuePair<string, decimal> pair in lineGroupToQuantityLookup)
                        {
                            string lineGroup = pair.Key;
                            decimal totalQuantityForLineGroup = pair.Value;

                            int[] itemGroupIndexSetOrdered = null;
                            if (lineGroupToOrderedItemGroupIndexArrayLookup.TryGetValue(lineGroup, out itemGroupIndexSetOrdered))
                            {
                                decimal quantityNeeded = numberOfDiscountApplications * this.LineGroupToNumberOfItemsMap[lineGroup];
                                decimal quantityToRemove = totalQuantityForLineGroup - quantityNeeded;
                                if (quantityToRemove > decimal.Zero)
                                {
                                    for (int indexLast = itemGroupIndexSetOrdered.Length - 1; indexLast >= 0; indexLast--)
                                    {
                                        int itemGroupIndex = itemGroupIndexSetOrdered[indexLast];
                                        decimal possibleQuantityToRemove = Math.Min(quantityToRemove, quantitiesRemaining[itemGroupIndex]);
                                        if (possibleQuantityToRemove > decimal.Zero)
                                        {
                                            quantitiesRemaining[itemGroupIndex] -= possibleQuantityToRemove;
                                            quantityToRemove -= possibleQuantityToRemove;
                                        }

                                        if (quantityToRemove == decimal.Zero)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        // Remove low price items that won't make the deal price
                        if (this.DiscountType == DiscountMethodType.DealPrice)
                        {
                            decimal[] quantitiesForDiscountApplication = new decimal[quantitiesRemaining.Length];
                            HashSet<int> itemGroupIndexUsed = new HashSet<int>();
                            decimal totalPriceInDiscountApplication = decimal.Zero;

                            // Need indexed enumeration (lineGroupList) to keep track of indexStarting
                            int numberOfLineGroups = this.LineGroupToNumberOfItemsMap.Count;
                            List<string> lineGroupList = new List<string>(this.LineGroupToNumberOfItemsMap.Keys);
                            int[] indexStartingList = new int[numberOfLineGroups];

                            bool keepGoing = true;
                            while (keepGoing)
                            {
                                // Get a discount application combination from the lowest priced items from each line group.
                                for (int indexLineGroup = 0; indexLineGroup < numberOfLineGroups; indexLineGroup++)
                                {
                                    string lineGroup = lineGroupList[indexLineGroup];
                                    decimal quantityNeededForLineGroup = this.LineGroupToNumberOfItemsMap[lineGroup];
                                    int index = indexStartingList[indexLineGroup];

                                    int[] itemGroupIndexSortedByPriceDescending = null;
                                    if (!lineGroupToOrderedItemGroupIndexArrayLookup.TryGetValue(lineGroup, out itemGroupIndexSortedByPriceDescending))
                                    {
                                        // missing items for the line group.
                                        keepGoing = false;
                                        break;
                                    }

                                    int itemGroupCount = itemGroupIndexSortedByPriceDescending.Length;

                                    // Fill in items from lowest price for the line group.
                                    while (quantityNeededForLineGroup > decimal.Zero && index < itemGroupCount)
                                    {
                                        int indexFromLast = (itemGroupCount - 1) - index;
                                        int itemGroupIndex = itemGroupIndexSortedByPriceDescending[indexFromLast];
                                        decimal quantityUsed = Math.Min(quantityNeededForLineGroup, quantitiesRemaining[itemGroupIndex]);

                                        if (quantityUsed < quantityNeededForLineGroup)
                                        {
                                            // Quantity used up, move to next one.
                                            index++;
                                            indexStartingList[indexLineGroup] = index;
                                        }

                                        if (quantityUsed > decimal.Zero)
                                        {
                                            totalPriceInDiscountApplication += quantityUsed * discountedPrices[itemGroupIndex];
                                            itemGroupIndexUsed.Add(itemGroupIndex);

                                            quantityNeededForLineGroup -= quantityUsed;
                                            quantitiesForDiscountApplication[itemGroupIndex] += quantityUsed;
                                        }
                                    }

                                    if (quantityNeededForLineGroup > decimal.Zero)
                                    {
                                        // Quit if we no longer have quantity for a line group.
                                        keepGoing = false;
                                        break;
                                    }
                                }

                                if (keepGoing)
                                {
                                    if (totalPriceInDiscountApplication < this.DealPriceValue)
                                    {
                                        // Found the combination, but it's below deal price, which means no discount. Remove it.
                                        foreach (int itemGroupIndex in itemGroupIndexUsed)
                                        {
                                            quantitiesRemaining[itemGroupIndex] -= quantitiesForDiscountApplication[itemGroupIndex];
                                        }

                                        quantitiesForDiscountApplication = new decimal[quantitiesRemaining.Length];
                                        itemGroupIndexUsed.Clear();
                                        totalPriceInDiscountApplication = decimal.Zero;
                                    }
                                    else
                                    {
                                        // Found the combination, and it's above deal price, we're done.
                                        keepGoing = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            internal HashSet<int> GetOverlappedItemGroupIndexSet(decimal[] remainingQuantities)
            {
                HashSet<int> overlappedItemGroupIndexSet = new HashSet<int>();
                foreach (KeyValuePair<int, HashSet<string>> pair in this.ItemGroupIndexToLineGroupSetLookup)
                {
                    int itemGroupIndex = pair.Key;

                    // Don't count items with 0 remaining quantity.
                    if (remainingQuantities[itemGroupIndex] > decimal.Zero)
                    {
                        HashSet<string> lineGroupSet = pair.Value;

                        if (lineGroupSet.Count > 1)
                        {
                            overlappedItemGroupIndexSet.Add(itemGroupIndex);
                        }
                    }
                }

                return overlappedItemGroupIndexSet;
            }

            internal void GetDiscountApplicationsSequentiallyOptimizedForOneLineGroup(
                List<DiscountApplication> discountApplicationList,
                SalesTransaction transaction,
                DiscountableItemGroup[] discountableItemGroups,
                Dictionary<int, decimal> mixAndMatchRelativeValueLookup,
                decimal[] remainingQuantities)
            {
                // One line group only
                if (this.LineGroupToNumberOfItemsMap.Count > 1 || this.IsFinished || this.LineGroupToItemGroupIndexSetLookup.Count == 0)
                {
                    return;
                }

                string discountCodeUsed = this.GetDiscountCodeForDiscount(transaction);
                var lineGroupItemGroupIndexSetPair = this.LineGroupToItemGroupIndexSetLookup.First();
                string lineGroup = lineGroupItemGroupIndexSetPair.Key;
                HashSet<int> itemGroupIndexSetForLineGroup = lineGroupItemGroupIndexSetPair.Value;
                int numberOfItemsNeeded = this.LineGroupToNumberOfItemsMap[lineGroup];

                // DISCOUNTPERF: mixAndMatchRelativeValueLookup.Keys.OrderBy?
                // Sort the item group index array, order by relative price descending.
                int[] itemGroupIndexArraySorted = mixAndMatchRelativeValueLookup.Keys.ToArray();
                ItemPriceComparer itemPriceComparer = new ItemPriceComparer(mixAndMatchRelativeValueLookup);
                Array.Sort(itemGroupIndexArraySorted, itemPriceComparer.CompareItemPriceByItemGroupIndexDescending);

                // The following 3 need to be reset after a new discount application.
                List<RetailDiscountLineItem> discountLineItems = new List<RetailDiscountLineItem>();
                Dictionary<int, decimal> quantitiesForDiscountApplication = new Dictionary<int, decimal>();
                HashSet<int> itemGroupIndexUsed = new HashSet<int>();

                // Construct discount applications sequentially from the item with highest relative price, to the lowest.
                // Quit when it runs out of quantity, or relative price (comparing to simple discounts) for the application is negative.
                decimal[] remainingQuantitiesLocal = (decimal[])remainingQuantities.Clone();
                bool keepGoing = true;
                int itemGroupIndexPosition = 0;
                while (keepGoing && itemGroupIndexPosition < itemGroupIndexArraySorted.Length)
                {
                    decimal quantityNeededForLineGroup = numberOfItemsNeeded;

                    decimal relativePrice = decimal.Zero;

                    while (quantityNeededForLineGroup > decimal.Zero && itemGroupIndexPosition < itemGroupIndexArraySorted.Length)
                    {
                        int itemGroupIndex = itemGroupIndexArraySorted[itemGroupIndexPosition];
                        decimal quantityUsed = decimal.Zero;
                        if (itemGroupIndexSetForLineGroup.Contains(itemGroupIndex))
                        {
                            decimal quantityAvailable = remainingQuantitiesLocal[itemGroupIndex];
                            quantityUsed = Math.Min(quantityNeededForLineGroup, quantityAvailable);
                            relativePrice += mixAndMatchRelativeValueLookup[itemGroupIndex] * quantityUsed;
                        }

                        if (quantityUsed < quantityNeededForLineGroup)
                        {
                            // Quantity used up, move to next one.
                            itemGroupIndexPosition++;
                        }

                        if (quantityUsed > decimal.Zero)
                        {
                            // Will reduce quantitiesRemaining later.
                            itemGroupIndexUsed.Add(itemGroupIndex);

                            quantityNeededForLineGroup -= quantityUsed;
                            DiscountBase.AddToItemQuantities(quantitiesForDiscountApplication, itemGroupIndex, quantityUsed);

                            RetailDiscountLine discountLine = this.GetDiscountLineByLineGroupAndItemGroupIndex(lineGroup, itemGroupIndex);
                            for (decimal j = decimal.Zero; j < quantityUsed; j++)
                            {
                                discountLineItems.Add(new RetailDiscountLineItem(itemGroupIndex, discountLine));
                            }
                        }
                    }

                    // Quit if not enough quantity, or relative price (comparing to simple discounts) is negative.
                    if (quantityNeededForLineGroup > decimal.Zero || relativePrice < decimal.Zero)
                    {
                        keepGoing = false;
                        break;
                    }
                    else
                    {
                        // We've got a discount application
                        DiscountApplication app = this.ConstructDiscountApplication(
                            discountableItemGroups,
                            discountLineItems,
                            quantitiesForDiscountApplication,
                            discountCodeUsed,
                            applyStandalone: true);
                        discountApplicationList.Add(app);

                        // Reduce remaining quantities.
                        foreach (int itemGroupIndex in itemGroupIndexUsed)
                        {
                            remainingQuantitiesLocal[itemGroupIndex] -= quantitiesForDiscountApplication[itemGroupIndex];
                        }

                        // reset discountLineGroups, quantitiesForDiscountApplication
                        discountLineItems = new List<RetailDiscountLineItem>();
                        quantitiesForDiscountApplication.Clear();
                        itemGroupIndexUsed.Clear();
                    }
                }
            }

            internal void GetDiscountApplicationsPartiallyOptimizedForOneLineGroupLeastExpensive(
                List<DiscountApplication> discountApplicationList,
                SalesTransaction transaction,
                DiscountableItemGroup[] discountableItemGroups,
                Dictionary<int, decimal> mixAndMatchRelativeValueLookup,
                List<HashSet<int>> consolidatedListOfItemsWithSamePriceAndRelativeValuesDescending,
                decimal[] remainingQuantities)
            {
                // For items with positive relative values, apply mix and match until the quantity is no more than 2 * (quantityNeeded - 1).
                // For items with the best positive value, apply mix and match until the quantity is the quantity to no more than quantityNeeded - 1.
                // For relative value, see OverlappedDiscounts.IsOkayForMixAndMatchOneLineGroupOptimizationAndFillupValueLookups.
                if (this.LineGroupToNumberOfItemsMap.Count > 1 ||
                    this.IsFinished ||
                    this.LineGroupToItemGroupIndexSetLookup.Count == 0 ||
                    this.DiscountType != DiscountMethodType.LeastExpensive ||
                    this.IsLeastExpensiveAmountOff)
                {
                    // One line group only, least expensive with deal price or %-off.
                    return;
                }

                string discountCodeUsed = this.GetDiscountCodeForDiscount(transaction);
                var lineGroupItemGroupIndexSetPair = this.LineGroupToItemGroupIndexSetLookup.First();
                string lineGroup = lineGroupItemGroupIndexSetPair.Key;
                HashSet<int> itemGroupIndexSetForLineGroup = lineGroupItemGroupIndexSetPair.Value;
                int numberOfItemsNeeded = this.LineGroupToNumberOfItemsMap[lineGroup];

                bool isHighestRelativePrice = true;
                decimal[] remainingQuantitiesLocal = (decimal[])remainingQuantities.Clone();
                foreach (HashSet<int> itemGroupIndexSetWithSameRelativePriceAndPrice in consolidatedListOfItemsWithSamePriceAndRelativeValuesDescending)
                {
                    int itemGroupIndexFirst = itemGroupIndexSetWithSameRelativePriceAndPrice.First();
                    decimal numberOfApplications = decimal.Zero;
                    decimal relativeValue = decimal.Zero;

                    // Figure out how many discount applications to apply. Roughly (total-quantity - quantity-to-keep) / #-of-items-for-1-application.
                    if (mixAndMatchRelativeValueLookup.TryGetValue(itemGroupIndexFirst, out relativeValue))
                    {
                        if (relativeValue > decimal.Zero)
                        {
                            decimal maxQuantityNotToApply = numberOfItemsNeeded - 1;
                            if (isHighestRelativePrice)
                            {
                                isHighestRelativePrice = false;
                            }
                            else
                            {
                                maxQuantityNotToApply *= 2;
                            }

                            decimal totalQuantity = itemGroupIndexSetWithSameRelativePriceAndPrice.Sum(p => remainingQuantitiesLocal[p]);
                            decimal remainder = totalQuantity % numberOfItemsNeeded;
                            if (totalQuantity > maxQuantityNotToApply)
                            {
                                decimal remainderPlusOneRound = remainder + numberOfItemsNeeded;
                                decimal quantityNotToApply = remainderPlusOneRound > maxQuantityNotToApply ? remainder : remainderPlusOneRound;

                                numberOfApplications = (totalQuantity - quantityNotToApply) / numberOfItemsNeeded;
                            }
                        }
                        else
                        {
                            // Stop when relative value is no longer favorite for mix and match.
                            break;
                        }
                    }

                    // The following 3 need to be reset after a new discount application.
                    List<RetailDiscountLineItem> discountLineItems = new List<RetailDiscountLineItem>();
                    Dictionary<int, decimal> quantitiesForDiscountApplication = new Dictionary<int, decimal>();
                    HashSet<int> itemGroupIndexUsed = new HashSet<int>();

                    // Apply the discount applications.
                    int[] itemGroupIndexArray = itemGroupIndexSetWithSameRelativePriceAndPrice.ToArray();
                    int arrayIndex = 0;
                    int itemCount = itemGroupIndexSetWithSameRelativePriceAndPrice.Count;
                    while (numberOfApplications > decimal.Zero && arrayIndex < itemCount)
                    {
                        decimal quantityNeededForLineGroup = numberOfItemsNeeded;

                        while (quantityNeededForLineGroup > decimal.Zero && arrayIndex < itemCount)
                        {
                            int itemGroupIndex = itemGroupIndexArray[arrayIndex];
                            if (itemGroupIndexSetForLineGroup.Contains(itemGroupIndex))
                            {
                                decimal quantityAvailable = remainingQuantitiesLocal[itemGroupIndex];
                                decimal quantityUsed = Math.Min(quantityNeededForLineGroup, quantityAvailable);

                                if (quantityUsed < quantityNeededForLineGroup)
                                {
                                    // Quantity used up, move to next one.
                                    arrayIndex++;
                                }

                                if (quantityUsed > decimal.Zero)
                                {
                                    // Will reduce quantitiesRemaining later.
                                    itemGroupIndexUsed.Add(itemGroupIndex);

                                    quantityNeededForLineGroup -= quantityUsed;
                                    DiscountBase.AddToItemQuantities(quantitiesForDiscountApplication, itemGroupIndex, quantityUsed);

                                    RetailDiscountLine discountLine = this.GetDiscountLineByLineGroupAndItemGroupIndex(lineGroup, itemGroupIndex);
                                    for (decimal j = decimal.Zero; j < quantityUsed; j++)
                                    {
                                        discountLineItems.Add(new RetailDiscountLineItem(itemGroupIndex, discountLine));
                                    }
                                }
                            }
                        }

                        if (quantityNeededForLineGroup == decimal.Zero)
                        {
                            // We've got a discount application
                            DiscountApplication app = this.ConstructDiscountApplication(
                                discountableItemGroups,
                                discountLineItems,
                                quantitiesForDiscountApplication,
                                discountCodeUsed,
                                applyStandalone: true);
                            discountApplicationList.Add(app);

                            // Reduce remaining quantities.
                            foreach (int itemGroupIndex in itemGroupIndexUsed)
                            {
                                remainingQuantitiesLocal[itemGroupIndex] -= quantitiesForDiscountApplication[itemGroupIndex];
                            }

                            // reset discountLineGroups, quantitiesForDiscountApplication
                            discountLineItems = new List<RetailDiscountLineItem>();
                            quantitiesForDiscountApplication.Clear();
                            itemGroupIndexUsed.Clear();

                            numberOfApplications--;
                        }
                        else
                        {
                            // not enough quantity.
                            break;
                        }
                    }
                }
            }

            internal void AllocateOverlappedItemsToLineGroups(
                Dictionary<string, decimal> lineGroupToQuantityLookup,
                Dictionary<string, Dictionary<int, decimal>> lineGroupToItemGroupIndexToAllocatedQuantityLookup,
                HashSet<int> overlappedItemGroupIndexSet,
                decimal[] remainingQuantitiesLocal)
            {
                // Test: MixAndMatchDiscountUnitTests.TestAllocateOverlappedItemsToLineGroups
                using (SimpleProfiler profiler = new SimpleProfiler("AllocateOverlappedItemsToLineGroups", 2))
                {
                    // Step 1: build lineGroup to non-overlapped quantity lookup, and to overlapped, and to overlapped item group index set lookup.
                    Dictionary<string, decimal> lineGroupToQuantityNonOverlappedLookup = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                    Dictionary<string, decimal> lineGroupToQuantityOverlappedLookup = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                    Dictionary<string, HashSet<int>> lineGroupToOverlappedItemGroupIndexSetLookup = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);
                    this.BuildLineGroupLookupsInFastMode(
                        lineGroupToQuantityNonOverlappedLookup,
                        lineGroupToQuantityOverlappedLookup,
                        lineGroupToOverlappedItemGroupIndexSetLookup,
                        remainingQuantitiesLocal,
                        overlappedItemGroupIndexSet);

                    // Step 2: sort line groups by how many discount application it can make.
                    // We will use the order to distribute overlapped quantities later.
                    lineGroupToQuantityLookup.AddRange(lineGroupToQuantityNonOverlappedLookup);
                    string[] lineGroupsSorted = this.LineGroupToNumberOfItemsMap.Keys.ToArray();
                    LineGroupByQuantityNeedComparer lineGroupComparer = new LineGroupByQuantityNeedComparer(
                        lineGroupToQuantityNonOverlappedLookup,
                        lineGroupToQuantityOverlappedLookup,
                        this.LineGroupToNumberOfItemsMap);
                    Array.Sort(lineGroupsSorted, lineGroupComparer.CompareLineGroupByQuantityNeeded);
                    Dictionary<string, int> lineGroupToSortedIndexLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    for (int lineGroupIndex = 0; lineGroupIndex < lineGroupsSorted.Length; lineGroupIndex++)
                    {
                        lineGroupToSortedIndexLookup.Add(lineGroupsSorted[lineGroupIndex], lineGroupIndex);
                    }

                    // Step 3: sort overlapped item group indices by how much the item quantity is needed, related to lineGroupsSorted.
                    int[] overlappedItemGroupIndicesSorted = overlappedItemGroupIndexSet.ToArray();
                    OverlappedItemGroupIndexComparer itemOverlappedComparer = new OverlappedItemGroupIndexComparer(
                        lineGroupsSorted,
                        this.ItemGroupIndexToLineGroupSetLookup);
                    Array.Sort(overlappedItemGroupIndicesSorted, itemOverlappedComparer.CompareOverlappedItemByMostHelpfulFirst);

                    // Step 4: distribute intra-overlapped items with lest significant first.
                    for (int i = 0; i < overlappedItemGroupIndicesSorted.Length; i++)
                    {
                        int itemGroupIndex = overlappedItemGroupIndicesSorted[i];
                        HashSet<string> itemLineGroupSet = this.ItemGroupIndexToLineGroupSetLookup[itemGroupIndex];

                        // Item line groups sorted by how many discount applications they can make, ascending.
                        string[] itemLineGroupSorted = itemLineGroupSet.ToArray();
                        LineGroupByQuantityNeedComparer itemlineGroupComparer = new LineGroupByQuantityNeedComparer(
                            lineGroupToQuantityLookup,
                            this.LineGroupToNumberOfItemsMap);
                        Array.Sort(itemLineGroupSorted, itemlineGroupComparer.CompareLineGroupByQuantityNeeded);

                        decimal quantityNeededForOneMoreApplication = decimal.Zero;
                        decimal numberOfDiscountApplicationsForLineGroupAchieved = decimal.Zero;
                        decimal numberOfDiscountApplicationsForLineGroupFirst = decimal.Zero;
                        for (int itemLineGroupSortedIndex = 0; itemLineGroupSortedIndex < itemLineGroupSorted.Length; itemLineGroupSortedIndex++)
                        {
                            string lineGroup = itemLineGroupSorted[itemLineGroupSortedIndex];
                            quantityNeededForOneMoreApplication += this.LineGroupToNumberOfItemsMap[lineGroup];
                            decimal numberOfDiscountApplicationsForLineGroup = Math.Floor(lineGroupToQuantityLookup[lineGroup] / this.LineGroupToNumberOfItemsMap[lineGroup]);
                            if (itemLineGroupSortedIndex == 0)
                            {
                                numberOfDiscountApplicationsForLineGroupFirst = numberOfDiscountApplicationsForLineGroup;
                            }

                            int itemLineGroupSortedIndexNext = itemLineGroupSortedIndex + 1;
                            bool isLast = itemLineGroupSortedIndexNext == itemLineGroupSorted.Length;
                            decimal incrementalDiscountApplications = decimal.Zero;
                            if (isLast)
                            {
                                incrementalDiscountApplications = Math.Floor(remainingQuantitiesLocal[itemGroupIndex] / quantityNeededForOneMoreApplication);
                            }
                            else
                            {
                                // Try to make to the number of discount applications of the next line group.
                                string lineGroupNext = itemLineGroupSorted[itemLineGroupSortedIndexNext];
                                decimal numberOfDiscountApplicationsForLineGroupNext = Math.Floor(lineGroupToQuantityLookup[lineGroupNext] / this.LineGroupToNumberOfItemsMap[lineGroupNext]);

                                decimal maxIncrementalDiscountApplications = numberOfDiscountApplicationsForLineGroupNext - numberOfDiscountApplicationsForLineGroup;
                                if (maxIncrementalDiscountApplications > decimal.Zero)
                                {
                                    decimal itemQuantity = remainingQuantitiesLocal[itemGroupIndex];
                                    incrementalDiscountApplications = Math.Min(Math.Floor(itemQuantity / quantityNeededForOneMoreApplication), maxIncrementalDiscountApplications);
                                }
                            }

                            if (incrementalDiscountApplications > decimal.Zero)
                            {
                                decimal numberOfDiscountApplicationsToAchieve = incrementalDiscountApplications + numberOfDiscountApplicationsForLineGroup;
                                this.AllocateOverlappedQuantityToLineGroups(
                                    lineGroupToItemGroupIndexToAllocatedQuantityLookup,
                                    lineGroupToQuantityLookup,
                                    itemLineGroupSorted,
                                    remainingQuantitiesLocal,
                                    itemGroupIndex,
                                    itemLineGroupSortedIndex,
                                    numberOfDiscountApplicationsToAchieve);
                                numberOfDiscountApplicationsForLineGroupAchieved = Math.Max(numberOfDiscountApplicationsForLineGroupAchieved, numberOfDiscountApplicationsToAchieve);
                            }
                        }

                        // For the remaining, at most twice more.
                        decimal finalNumberOfDiscountApplicationsToAchieve = numberOfDiscountApplicationsForLineGroupAchieved > decimal.Zero ? numberOfDiscountApplicationsForLineGroupAchieved : numberOfDiscountApplicationsForLineGroupFirst;
                        for (int x = 0; x < 2 && remainingQuantitiesLocal[itemGroupIndex] > decimal.Zero; x++)
                        {
                            finalNumberOfDiscountApplicationsToAchieve++;
                            this.AllocateOverlappedQuantityToLineGroups(
                                lineGroupToItemGroupIndexToAllocatedQuantityLookup,
                                lineGroupToQuantityLookup,
                                itemLineGroupSorted,
                                remainingQuantitiesLocal,
                                itemGroupIndex,
                                itemLineGroupSorted.Length - 1,
                                finalNumberOfDiscountApplicationsToAchieve);
                        }
                    }
                }

#if DEBUG
                foreach (KeyValuePair<string, decimal> pair in lineGroupToQuantityLookup)
                {
                    string lineGroup = pair.Key;
                    decimal lineGroupQuantity = pair.Value;
                    StringBuilder lineGroupString = new StringBuilder("[line group quantity]");
                    lineGroupString.AppendFormat(" Line group [{0}] Total quantity [{1}] Allocated:", lineGroup, lineGroupQuantity);
                    Dictionary<int, decimal> itemGroupIndexToAllocatedQuantityLookup = null;
                    if (lineGroupToItemGroupIndexToAllocatedQuantityLookup.TryGetValue(lineGroup, out itemGroupIndexToAllocatedQuantityLookup))
                    {
                        foreach (KeyValuePair<int, decimal> pairItemGroupIndexToAllocatedQuantity in itemGroupIndexToAllocatedQuantityLookup)
                        {
                            lineGroupString.AppendFormat(" [{0}:{1}]", pairItemGroupIndexToAllocatedQuantity.Key, pairItemGroupIndexToAllocatedQuantity.Value);
                        }
                    }

                    System.Diagnostics.Debug.WriteLine(lineGroupString.ToString());
                }
#endif
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
                    if (itemsWithOverlappingDiscounts != null && itemsWithOverlappingDiscounts.Contains(itemGroupIndex))
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
                if (transaction == null)
                {
                    throw new ArgumentNullException("transaction");
                }

                if (discountableItemGroups == null)
                {
                    throw new ArgumentNullException("discountableItemGroups");
                }

                if (remainingQuantities == null)
                {
                    throw new ArgumentNullException("remainingQuantities");
                }

                string discountCodeUsed = this.GetDiscountCodeForDiscount(transaction);

                // Step 1: construct line-group-overlapped item group index set.
                HashSet<int> overlappedItemGroupIndexSet = this.GetOverlappedItemGroupIndexSet(remainingQuantities);

                decimal[] remainingQuantitiesLocal = (decimal[])remainingQuantities.Clone();

                // Step 2: distribute overlapped quantities if any to lineGroupToItemGroupIndexToAllocatedQuantityLookup
                Dictionary<string, decimal> lineGroupToQuantityLookup = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, Dictionary<int, decimal>> lineGroupToItemGroupIndexToAllocatedQuantityLookup = new Dictionary<string, Dictionary<int, decimal>>(StringComparer.OrdinalIgnoreCase);
                if (overlappedItemGroupIndexSet.Any())
                {
                    this.AllocateOverlappedItemsToLineGroups(
                        lineGroupToQuantityLookup,
                        lineGroupToItemGroupIndexToAllocatedQuantityLookup,
                        overlappedItemGroupIndexSet,
                        remainingQuantitiesLocal);
                }

                // Step 3: build line group to sorted item group index array (by item price descending) lookup. 
                // Later in final step, we'll use items from highest price to lowest for each line group.
                Dictionary<string, int[]> lineGroupToSortedItemGroupIndexArrayLookup = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
                this.BuildLineGroupToSortedItemGroupIndexArrayLookup(
                    lineGroupToSortedItemGroupIndexArrayLookup,
                    lineGroupToItemGroupIndexToAllocatedQuantityLookup,
                    discountableItemGroups,
                    overlappedItemGroupIndexSet);

                // Step 4: construct discount applications one by one from lineGroupToSortedItemGroupIndexArrayLookup
                List<DiscountApplication> discountApplications = this.ConstructDiscountApplicationsFastMode(
                    lineGroupToSortedItemGroupIndexArrayLookup,
                    discountableItemGroups,
                    remainingQuantitiesLocal,
                    lineGroupToItemGroupIndexToAllocatedQuantityLookup,
                    overlappedItemGroupIndexSet,
                    discountCodeUsed);

                return discountApplications;
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
                if (discountableItemGroups == null)
                {
                    throw new ArgumentNullException("discountableItemGroups");
                }

                if (remainingQuantities == null)
                {
                    throw new ArgumentNullException("remainingQuantities");
                }

                if (this.LineGroupToNumberOfItemsMap.Sum(p => p.Value) < 2m)
                {
                    this.IsFinished = true;
                }

                // All line group has needed items >= 1
                if (!this.IsFinished && this.LineGroupToNumberOfItemsMap.Where(p => p.Value < 1).Any())
                {
                    this.IsFinished = true;
                }

                if (!this.IsFinished)
                {
                    this.RemoveItemGroupIndexesWithZeroQuanttiyFromLookups(remainingQuantities);
                    this.PrepareLineGroupLookups(discountableItemGroups, remainingQuantities);

                    // Not enough line group(s) with items.
                    if (this.LineGroupToNumberOfItemsMap.Count > this.LineGroupToItemGroupIndexSetLookup.Count)
                    {
                        this.IsFinished = true;
                    }
                }

                if (!this.IsFinished)
                {
                    // Make sure each line group has adequate quantity.
                    foreach (KeyValuePair<string, HashSet<int>> pair in this.LineGroupToItemGroupIndexSetLookup)
                    {
                        string lineGroup = pair.Key;
                        HashSet<int> itemGroupIndexSetForLineGroup = pair.Value;
                        decimal quantityAvailableForLineGroup = itemGroupIndexSetForLineGroup.Sum(p => remainingQuantities[p]);

                        if (quantityAvailableForLineGroup < this.LineGroupToNumberOfItemsMap[lineGroup])
                        {
                            this.IsFinished = true;
                            break;
                        }
                    }
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
                    throw new ArgumentNullException("itemsWithOverlappingDiscountsCompoundedOnly");
                }

                HashSet<int> itemsWithOverlappingDiscountsExcludingCompoundedOnly = new HashSet<int>(itemsWithOverlappingDiscounts);
                itemsWithOverlappingDiscountsExcludingCompoundedOnly.ExceptWith(itemsWithOverlappingDiscountsCompoundedOnly);

                decimal totalNumberOfDiscountApplicationsNonOverlapped;
                decimal totalDiscountAmountNonOverlapped;
                decimal totalApplicableQuantityNonOverlapped;
                this.CalculateEstimate(
                    false,
                    discountableItemGroups,
                    remainingQuantities,
                    itemsWithOverlappingDiscountsExcludingCompoundedOnly,
                    out totalNumberOfDiscountApplicationsNonOverlapped,
                    out totalDiscountAmountNonOverlapped,
                    out totalApplicableQuantityNonOverlapped);

                decimal totalNumberOfDiscountApplicationsOverlapped = decimal.Zero;
                decimal totalDiscountAmountOverlapped = decimal.Zero;
                decimal totalApplicableQuantityOverlapped = decimal.Zero;
                Dictionary<int, decimal> itemGroupIndexToQuantityNeededFromOverlappedLookup = new Dictionary<int, decimal>();
                if (itemsWithOverlappingDiscountsExcludingCompoundedOnly.Any())
                {
                    this.CalculateEstimate(
                        true,
                        discountableItemGroups,
                        remainingQuantities,
                        itemsWithOverlappingDiscountsExcludingCompoundedOnly,
                        out totalNumberOfDiscountApplicationsOverlapped,
                        out totalDiscountAmountOverlapped,
                        out totalApplicableQuantityOverlapped);

                    foreach (int itemGroupIndex in itemsWithOverlappingDiscountsExcludingCompoundedOnly)
                    {
                        HashSet<string> lineGroupSet = null;
                        decimal quantityNeededEstimate = decimal.Zero;
                        if (this.ItemGroupIndexToLineGroupSetLookup.TryGetValue(itemGroupIndex, out lineGroupSet))
                        {
                            quantityNeededEstimate = totalNumberOfDiscountApplicationsOverlapped * lineGroupSet.Sum(p => this.LineGroupToNumberOfItemsMap[p]);
                            quantityNeededEstimate = Math.Min(quantityNeededEstimate, remainingQuantities[itemGroupIndex]);
                        }

                        itemGroupIndexToQuantityNeededFromOverlappedLookup[itemGroupIndex] = quantityNeededEstimate;
                    }
                }
                else
                {
                    totalNumberOfDiscountApplicationsOverlapped = totalNumberOfDiscountApplicationsNonOverlapped;
                    totalDiscountAmountOverlapped = totalDiscountAmountNonOverlapped;
                    totalApplicableQuantityOverlapped = totalApplicableQuantityNonOverlapped;
                }

                totalDiscountAmountNonOverlapped = Math.Max(decimal.Zero, totalDiscountAmountNonOverlapped);
                totalDiscountAmountOverlapped = Math.Max(totalDiscountAmountOverlapped, totalDiscountAmountNonOverlapped);

                DiscountDealEstimate estimate = new DiscountDealEstimate(
                    this.CanCompound,
                    this.OfferId,
                    totalApplicableQuantityOverlapped,
                    totalDiscountAmountOverlapped,
                    totalApplicableQuantityNonOverlapped,
                    totalDiscountAmountNonOverlapped,
                    itemGroupIndexToQuantityNeededFromOverlappedLookup);

                return estimate;
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
                    System.Diagnostics.Debug.WriteLine("  line group to discount line number set lookup.");
                    foreach (KeyValuePair<string, HashSet<decimal>> pair in this.LineGroupToDiscountLineNumberSetLookup)
                    {
                        StringBuilder discountLineNumbers = new StringBuilder();
                        discountLineNumbers.AppendFormat("    line group [{0}] Discount line numbers:", pair.Key);
                        foreach (decimal discountLineNumber in pair.Value)
                        {
                            discountLineNumbers.AppendFormat(" [{0}]", discountLineNumber);
                        }

                        System.Diagnostics.Debug.WriteLine(discountLineNumbers.ToString());
                    }

                    System.Diagnostics.Debug.WriteLine("  line group to item group index set lookup.");
                    foreach (KeyValuePair<string, HashSet<int>> pair in this.LineGroupToItemGroupIndexSetLookup)
                    {
                        StringBuilder itemGroupIndices = new StringBuilder();
                        itemGroupIndices.AppendFormat("    line group [{0}] item group indices:", pair.Key);
                        foreach (int itemGroupIndex in pair.Value)
                        {
                            itemGroupIndices.AppendFormat(" [{0}]", itemGroupIndex);
                        }

                        System.Diagnostics.Debug.WriteLine(itemGroupIndices.ToString());
                    }
                }
            }
#endif

            /// <summary>
            /// Initializes lookups, etc.
            /// </summary>
            protected override void InitializeLookups()
            {
                base.InitializeLookups();

                this.LineGroupToDiscountLineNumberSetLookup.Clear();
                this.LineGroupToItemGroupIndexSetLookup.Clear();
                this.ItemGroupIndexToLineGroupSetLookup.Clear();
            }

            /// <summary>
            /// Translates a line discount method from <see cref="DiscountOfferMethod"/> to <see cref="DiscountMethodType"/>.
            /// </summary>
            /// <param name="lineDiscountMethod">The value of the discount method.</param>
            /// <returns>The corresponding discount method enumeration value.</returns>
            private static DiscountMethodType TranslateLineDiscountMethod(int lineDiscountMethod)
            {
                switch (lineDiscountMethod)
                {
                    case (int)DiscountOfferMethod.DiscountAmount:
                        return DiscountMethodType.DiscountAmount;
                    case (int)DiscountOfferMethod.DiscountPercent:
                        return DiscountMethodType.DiscountPercent;
                    case (int)DiscountOfferMethod.OfferPrice:
                        return DiscountMethodType.DealPrice;
                    default:
                        return DiscountMethodType.DiscountPercent;
                }
            }

            // Allocate overlapped quantities from remainingQuantities to lineGroupToItemGroupIndexToAllocatedQuantityLookup
            private static void AllocateOverlappedQuantityToLineGroup(
                Dictionary<string, Dictionary<int, decimal>> lineGroupToItemGroupIndexToAllocatedQuantityLookup,
                decimal[] remainingQuantities,
                string lineGroup,
                int itemGroupIndex,
                decimal quantityUsed)
            {
                if (quantityUsed > decimal.Zero)
                {
                    remainingQuantities[itemGroupIndex] -= quantityUsed;

                    // Allocate quantity to the line group, in lineGroupToItemGroupIndexToAllocatedQuantityLookup
                    Dictionary<int, decimal> itemGroupIndexToAllocatedQuantityLookup = null;
                    if (lineGroupToItemGroupIndexToAllocatedQuantityLookup.TryGetValue(lineGroup, out itemGroupIndexToAllocatedQuantityLookup))
                    {
                        decimal existingQuantity = decimal.Zero;
                        itemGroupIndexToAllocatedQuantityLookup.TryGetValue(itemGroupIndex, out existingQuantity);
                        itemGroupIndexToAllocatedQuantityLookup[itemGroupIndex] = existingQuantity + quantityUsed;
                    }
                    else
                    {
                        itemGroupIndexToAllocatedQuantityLookup = new Dictionary<int, decimal>();
                        itemGroupIndexToAllocatedQuantityLookup.Add(itemGroupIndex, quantityUsed);
                        lineGroupToItemGroupIndexToAllocatedQuantityLookup.Add(lineGroup, itemGroupIndexToAllocatedQuantityLookup);
                    }
                }
            }

            private static bool IsItemQuantitiesCoveredByRemainingQuantities(Dictionary<int, decimal> itemQuantities, decimal[] remainingQuantities)
            {
                if (itemQuantities == null || remainingQuantities == null)
                {
                    return false;
                }

                foreach (KeyValuePair<int, decimal> pair in itemQuantities)
                {
                    int itemGroupIndex = pair.Key;
                    decimal quantity = pair.Value;

                    if (itemGroupIndex >= remainingQuantities.Length)
                    {
                        return false;
                    }
                    else if (quantity > remainingQuantities[itemGroupIndex])
                    {
                        return false;
                    }
                }

                return true;
            }

            private static void ReduceQuantitiesFromRemainingQuantities(
                string lineGroup,
                int itemGroupIndex,
                decimal quantityUsed,
                decimal[] remainingQuantities,
                HashSet<int> overlappedItemGroupIndexSet,
                Dictionary<string, Dictionary<int, decimal>> lineGroupToItemGroupIndexToAllocatedQuantityLookup)
            {
                // Reduce available quantity now.
                if (overlappedItemGroupIndexSet.Contains(itemGroupIndex))
                {
                    lineGroupToItemGroupIndexToAllocatedQuantityLookup[lineGroup][itemGroupIndex] -= quantityUsed;
                }
                else
                {
                    remainingQuantities[itemGroupIndex] -= quantityUsed;
                }
            }

            private static decimal GetAvailableQuantityForItem(
                string lineGroup,
                int itemGroupIndex,
                decimal[] remainingQuantities,
                HashSet<int> overlappedItemGroupIndexSet,
                Dictionary<string, Dictionary<int, decimal>> lineGroupToItemGroupIndexToAllocatedQuantityLookup)
            {
                decimal quantityAvailable = decimal.Zero;
                if (overlappedItemGroupIndexSet.Contains(itemGroupIndex))
                {
                    Dictionary<int, decimal> itemQuantityAllocatedToLineGroup = null;

                    if (lineGroupToItemGroupIndexToAllocatedQuantityLookup.TryGetValue(lineGroup, out itemQuantityAllocatedToLineGroup))
                    {
                        itemQuantityAllocatedToLineGroup.TryGetValue(itemGroupIndex, out quantityAvailable);
                    }
                }
                else
                {
                    quantityAvailable = remainingQuantities[itemGroupIndex];
                }

                return quantityAvailable;
            }

            /// <summary>
            /// Generate discount lines dictionary from the applied discount application.
            /// </summary>
            /// <param name="discountableItemGroups">The transaction line items.</param>
            /// <param name="discountApplicationToApply">The specific application of the discount to use.</param>
            /// <param name="priceContext">The pricing context to use.</param>
            /// <returns>A dictionary containing a list of DiscountLine objects for each index of discountableItemGroups that has at least one discount.</returns>
            private IDictionary<int, IList<DiscountLineQuantity>> GenerateDiscountLinesDictionary(
                DiscountableItemGroup[] discountableItemGroups,
                AppliedDiscountApplication discountApplicationToApply,
                PriceContext priceContext)
            {
                IDictionary<int, IList<DiscountLineQuantity>> result = new Dictionary<int, IList<DiscountLineQuantity>>();

                if (discountApplicationToApply == null || discountableItemGroups == null)
                {
                    return result;
                }

                // Keep track of which lines have been considered in the amount so that we don't double-count them.
                List<int> linesWithDiscount = new List<int>();

                decimal totalPrice = decimal.Zero;
                decimal discountAmount = this.DiscountAmountValue;
                decimal dealPrice = this.DealPriceValue;

                decimal[] prices = discountApplicationToApply.DiscountApplication.GetPrices(discountableItemGroups);

                if (this.DealPriceValue != decimal.Zero || this.DiscountAmountValue != decimal.Zero)
                {
                    foreach (KeyValuePair<int, decimal> pair in discountApplicationToApply.DiscountApplication.ItemQuantities)
                    {
                        int itemGroupIndex = pair.Key;
                        decimal quantity = pair.Value;
                        totalPrice += prices[itemGroupIndex] * quantity;
                    }
                }

                foreach (var group in discountApplicationToApply.DiscountApplication.RetailDiscountLines.GroupBy(p => p.RetailDiscountLine.MixAndMatchLineGroup))
                {
                    this.GetDiscountLinesToApplyForGroup(
                        discountableItemGroups,
                        prices,
                        discountApplicationToApply.DiscountApplication,
                        priceContext,
                        result,
                        linesWithDiscount,
                        ref totalPrice,
                        ref discountAmount,
                        ref dealPrice,
                        group);
                }

                return result;
            }

            /// <summary>
            /// Arranges the items into mix-and-match line groups to determine the possible combinations of lines that are valid for this discount.
            /// </summary>
            /// <param name="availableItemGroups">The available item groups.</param>
            /// <param name="currentItemGroupIndex">The current index in the available items.</param>
            /// <param name="qtyRemaining">The remaining quantities of items after other groups have been used.</param>
            /// <param name="matchedLineGroups">The dictionary containing matching sets of items for each mix-and-match line group.</param>
            private void ArrangeLineGroups(
                BitSet availableItemGroups,
                int currentItemGroupIndex,
                decimal[] qtyRemaining,
                Dictionary<string, List<List<RetailDiscountLineItem>>> matchedLineGroups)
            {
                Stack<int> itemsUsed = new Stack<int>();
                Stack<RetailDiscountLine> discountLinesUsed = new Stack<RetailDiscountLine>();

                foreach (string lineGroup in this.LineGroupToNumberOfItemsMap.Keys)
                {
                    // [znote] For each line group, gather all possible item combinations.
                    int numberOfItemsNeededForLineGroup = this.LineGroupToNumberOfItemsMap[lineGroup];
                    HashSet<decimal> discountLineNumberSetForLineGroup = null;

                    if (!this.LineGroupToDiscountLineNumberSetLookup.TryGetValue(lineGroup, out discountLineNumberSetForLineGroup))
                    {
                        matchedLineGroups.Clear();
                        return;
                    }

                    // [znote] example in a {0, 1, 2} and b {3} where a needs 2 and b 1. qty for each line = 1. a only.
                    // currentItemGroupIndex = 0: push{0}
                    // currentItemGroupIndex = 0: currentItemGroupIndex++ as we ran out of quantity for 0.
                    // currentItemGroupIndex = 1: push{1} - got a match {0, 1}
                    // currentItemGroupIndex = 1: pop{1} & currentItemGroupIndex = 1(from pop) + 1 = 2
                    // currentItemGroupIndex = 2: push{2} - got a match {0, 2}
                    // currentItemGroupIndex = 2: pop{2} & currentItemGroupIndex = 2(from pop) + 1 = 3
                    // currentItemGroupIndex = 3: no match: currentItemGroupIndex++
                    // currentItemGroupIndex = 4: pop{0} & currentItemGroupIndex = 0(from pop) + 1 = 1
                    // currentItemGroupIndex = 1: push{1}
                    // currentItemGroupIndex = 1: currentItemGroupIndex++ as we ran out of quantity for 0.
                    // currentItemGroupIndex = 2: push{2} - got a match {1, 2}
                    // currentItemGroupIndex = 2: pop{2} & currentItemGroupIndex = 2(from pop) + 1 = 3
                    // currentItemGroupIndex = 3: no match: currentItemGroupIndex++
                    // currentItemGroupIndex = 4: pop{1} & currentItemGroupIndex = 1(from pop) + 1 = 2
                    // currentItemGroupIndex = 2: push{2}
                    // currentItemGroupIndex = 2: currentItemGroupIndex++ as we ran out of quantity for 2.
                    // currentItemGroupIndex = 3: no match: currentItemGroupIndex++
                    // currentItemGroupIndex = 4: pop{2} & currentItemGroupIndex = 2(from pop) + 1 = 3
                    // currentItemGroupIndex = 3: no match: currentItemGroupIndex++
                    while (true)
                    {
                        currentItemGroupIndex = availableItemGroups.GetNextNonZeroBit(currentItemGroupIndex);

                        if (currentItemGroupIndex == BitSet.UnknownBit || itemsUsed.Count >= numberOfItemsNeededForLineGroup)
                        {
                            if (itemsUsed.Count > 0)
                            {
                                // No next non-zero bit exists, move to the next item from the previous parent
                                int lastItem = itemsUsed.Pop();
                                discountLinesUsed.Pop();
                                qtyRemaining[lastItem] += 1;
                                currentItemGroupIndex = lastItem + 1;
                            }
                            else
                            {
                                // No next non-zero bit exists and we are at the top level, exit the loop.
                                currentItemGroupIndex = 0;
                                break;
                            }
                        }
                        else
                        {
                            // [znote] it makes no difference for an item to which discount line it belongs.
                            RetailDiscountLine matchedDiscountLineForItemAndLineGroup = null;
                            HashSet<decimal> discountLineNumberSetForItem = null;
                            if (this.ItemGroupIndexToDiscountLineNumberSetMap.TryGetValue(currentItemGroupIndex, out discountLineNumberSetForItem))
                            {
                                foreach (decimal discountLineNumber in discountLineNumberSetForItem)
                                {
                                    if (discountLineNumberSetForLineGroup.Contains(discountLineNumber))
                                    {
                                        matchedDiscountLineForItemAndLineGroup = this.DiscountLines[discountLineNumber];
                                        break;
                                    }
                                }
                            }

                            if (matchedDiscountLineForItemAndLineGroup != null && qtyRemaining[currentItemGroupIndex] >= 1M)
                            {
                                // [znote] We've got enough for the line group, add it to matchedLineGroups. 
                                itemsUsed.Push(currentItemGroupIndex);
                                discountLinesUsed.Push(matchedDiscountLineForItemAndLineGroup);
                                qtyRemaining[currentItemGroupIndex] -= 1;

                                // Check to see if we have all of the required items for this group.
                                if (itemsUsed.Count == numberOfItemsNeededForLineGroup)
                                {
                                    List<RetailDiscountLineItem> lineGroupComposition = new List<RetailDiscountLineItem>(itemsUsed.Count);
                                    int[] matchedItems = itemsUsed.ToArray();
                                    RetailDiscountLine[] matchedDiscountLines = discountLinesUsed.ToArray();

                                    for (int x = 0; x < matchedItems.Length; x++)
                                    {
                                        lineGroupComposition.Add(new RetailDiscountLineItem(matchedItems[x], matchedDiscountLines[x]));
                                    }

                                    // Now that we have a valid number of lines, save off this path with the matched line in the matched groups.
                                    if (matchedLineGroups.ContainsKey(lineGroup))
                                    {
                                        matchedLineGroups[lineGroup].Add(lineGroupComposition);
                                    }
                                    else
                                    {
                                        matchedLineGroups.Add(lineGroup, new List<List<RetailDiscountLineItem>>(1) { lineGroupComposition });
                                    }
                                }
                            }
                            else
                            {
                                // Not enough items left in this index or this item is not in the correct discount line, move to the next one.
                                currentItemGroupIndex++;
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Determines the sort value to use for a specific discount application.  This value is an approximation of the amount or percentage that this discount application will use.
            /// </summary>
            /// <param name="discountLineItems">The items used by this application.</param>
            /// <param name="discountableItemGroups">The item groups on the transaction.</param>
            /// <returns>The sort value to use.</returns>
            private decimal GetSortValue(IEnumerable<RetailDiscountLineItem> discountLineItems, DiscountableItemGroup[] discountableItemGroups)
            {
                decimal result = decimal.Zero;

                if (this.DiscountType == DiscountMethodType.DiscountAmount)
                {
                    return this.DiscountAmountValue;
                }

                if (this.DiscountType == DiscountMethodType.LeastExpensive)
                {
                    if (this.DiscountPercentValue != decimal.Zero)
                    {
                        return this.DiscountPercentValue;
                    }
                    else if (this.DiscountAmountValue != decimal.Zero)
                    {
                        return this.DiscountAmountValue;
                    }
                }

                decimal totalPrice = decimal.Zero;

                foreach (RetailDiscountLineItem item in discountLineItems)
                {
                    totalPrice += discountableItemGroups[item.ItemIndex].Price;
                }

                foreach (RetailDiscountLineItem item in discountLineItems)
                {
                    decimal price = discountableItemGroups[item.ItemIndex].Price;

                    switch ((DiscountOfferMethod)item.RetailDiscountLine.DiscountMethod)
                    {
                        case DiscountOfferMethod.DiscountAmount:
                            result += item.RetailDiscountLine.DiscountAmount;
                            break;
                        case DiscountOfferMethod.DiscountPercent:
                            result += price * (this.DiscountPercentValue + item.RetailDiscountLine.DiscountPercent + item.RetailDiscountLine.DiscountLinePercentOrValue) / 100M;
                            break;
                        case DiscountOfferMethod.OfferPrice:
                            result += price - (item.RetailDiscountLine.DiscountLinePercentOrValue * price / totalPrice);
                            break;
                        default:
                            result += 0;
                            break;
                    }
                }

                return result;
            }

            /// <summary>
            /// Gets the possible mix-and-match discount applications for the item groups on the transaction.
            /// </summary>
            /// <param name="availableItemGroups">The available item groups to consider.</param>
            /// <param name="discountableItemGroups">The item groups on the transaction.</param>
            /// <param name="remainingQuantities">The remaining quantities of the item groups.</param>
            /// <param name="discountCodeUsed">The discount code used for triggering this discount.</param>
            /// <param name="priceContext">Price context.</param>
            /// <param name="isInterrupted">A flag indicating whether it's interrupted for too many discount applications.</param>
            /// <returns>The collection of discount applications for this discount on the transaction.</returns>
            private IEnumerable<DiscountApplication> GetDiscountApplicationsForItems(
                BitSet availableItemGroups,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                string discountCodeUsed,
                PriceContext priceContext,
                out bool isInterrupted)
            {
                List<DiscountApplication> result = new List<DiscountApplication>();
                isInterrupted = false;

                Dictionary<string, List<List<RetailDiscountLineItem>>> matchedLineGroups = new Dictionary<string, List<List<RetailDiscountLineItem>>>(StringComparer.OrdinalIgnoreCase);

                if (availableItemGroups.IsZero())
                {
                    return result;
                }

                int currentIndex = 0;

                decimal[] qtyRemaining = (decimal[])remainingQuantities.Clone();

                // Loop through all of the triggered discount lines to arrange them into line groups
                this.ArrangeLineGroups(
                    availableItemGroups,
                    currentIndex,
                    qtyRemaining,
                    matchedLineGroups);

                // Now that we have all of the possibilities for each group, add all of the possible combinations of groups matches to the result.
                // Check the original lines to make sure that we get all of the required groups.
                List<string> requiredGroups = this.LineGroupToNumberOfItemsMap.Keys.ToList();

                // Since we don't know how many groups we will have, we use a pair of stacks here to do a depth-first search on the different 
                // permutations of line groups, where the first stack contains the index into the groups and the second stack contains the selected group.
                Stack<int> groupIndices = new Stack<int>(requiredGroups.Count);
                Stack<List<RetailDiscountLineItem>> groupMember = new Stack<List<RetailDiscountLineItem>>(requiredGroups.Count);

                bool isLeastExpensiveWithOneLineGroup = this.DiscountType == DiscountMethodType.LeastExpensive && this.LineGroupToNumberOfItemsMap.Count == 1;

                // If any required group doesn't exist in the matches, then we won't add any results, just return the empty list of matches.
                if (matchedLineGroups.Count == requiredGroups.Count)
                {
                    // Initialize the stacks by pushing the first element from each group up to the last one.
                    // Then we will do the foreach, then iterate on each available option.
                    for (int x = 0; x < requiredGroups.Count - 1; x++)
                    {
                        groupIndices.Push(0);
                        groupMember.Push(matchedLineGroups[requiredGroups[x]][0]);
                    }

                    while (true)
                    {
                        if (priceContext.ExceedsMaxBestDealAlgorithmStepCount(result.Count))
                        {
                            isInterrupted = true;
                            return result;
                        }

                        Dictionary<int, decimal> itemQuantities = new Dictionary<int, decimal>();

                        foreach (List<RetailDiscountLineItem> groupMatch in groupMember)
                        {
                            foreach (RetailDiscountLineItem item in groupMatch)
                            {
                                DiscountBase.AddToItemQuantities(itemQuantities, item.ItemIndex, 1);
                            }
                        }

                        // Verify that there are enough remaining items to use this amount (before looking at the last group).
                        if (MixAndMatchDiscount.IsItemQuantitiesCoveredByRemainingQuantities(itemQuantities, remainingQuantities))
                        {
                            // Save off a discount line for each member in the last required group
                            foreach (List<RetailDiscountLineItem> lastGroupLine in matchedLineGroups[requiredGroups[groupIndices.Count]])
                            {
                                Dictionary<int, decimal> lastGroupQuantities = new Dictionary<int, decimal>(itemQuantities);
                                foreach (RetailDiscountLineItem item in lastGroupLine)
                                {
                                    DiscountBase.AddToItemQuantities(lastGroupQuantities, item.ItemIndex, 1);
                                }

                                // Verify that there are enough remaining items to use this amount (including the last group).
                                if (MixAndMatchDiscount.IsItemQuantitiesCoveredByRemainingQuantities(lastGroupQuantities, remainingQuantities))
                                {
                                    // Add discount line, using the groupMember stack contents plus this line.
                                    groupMember.Push(lastGroupLine);

                                    List<RetailDiscountLineItem> discountLineItems = groupMember.SelectMany(p => p).ToList();
                                    DiscountApplication app = this.ConstructDiscountApplication(
                                        discountableItemGroups,
                                        discountLineItems,
                                        lastGroupQuantities,
                                        discountCodeUsed,
                                        applyStandalone: false);
                                    if (isLeastExpensiveWithOneLineGroup && lastGroupQuantities.Count > 1)
                                    {
                                        app.NumberOfTimesApplicable = 1;
                                    }

                                    result.Add(app);

                                    groupMember.Pop();
                                }
                            }
                        }

                        // This would only happen if there is only a single group required.
                        if (groupIndices.Count == 0)
                        {
                            break;
                        }
                        else
                        {
                            currentIndex = groupIndices.Pop() + 1;
                            groupMember.Pop();

                            while (matchedLineGroups[requiredGroups[groupIndices.Count]].Count <= currentIndex && groupIndices.Count != 0)
                            {
                                currentIndex = groupIndices.Pop() + 1;
                                groupMember.Pop();
                            }

                            // Check to see if we have already examined every group
                            if (groupIndices.Count == 0 && matchedLineGroups[requiredGroups[0]].Count <= currentIndex)
                            {
                                break;
                            }
                            else
                            {
                                groupIndices.Push(currentIndex);
                                groupMember.Push(matchedLineGroups[requiredGroups[groupIndices.Count - 1]][currentIndex]);

                                // Fill in the rest of the stack with the left-hand nodes of the tree
                                for (int x = groupIndices.Count; x < requiredGroups.Count - 1; x++)
                                {
                                    groupIndices.Push(0);
                                    groupMember.Push(matchedLineGroups[requiredGroups[x]][0]);
                                }
                            }
                        }
                    }
                }

                return result;
            }

            private void GetDiscountLinesToApplyForGroup(
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] prices,
                DiscountApplication discountApplication,
                PriceContext priceContext,
                IDictionary<int, IList<DiscountLineQuantity>> result,
                List<int> linesWithDiscount,
                ref decimal totalPrice,
                ref decimal discountAmount,
                ref decimal dealPrice,
                IGrouping<string, RetailDiscountLineItem> group)
            {
                RetailDiscountLine firstDiscountLine = group.Select(p => p.RetailDiscountLine).First();

                // Line specific deal price. Note it's not firstDiscountLine.OfferPrice.
                if (this.DiscountType == DiscountMethodType.LineSpecific && firstDiscountLine.DiscountMethod == (int)DiscountOfferMethod.OfferPrice)
                {
                    totalPrice = group.Sum(p => prices[p.ItemIndex]);
                    dealPrice = firstDiscountLine.DiscountLinePercentOrValue;
                }

                foreach (var item in group)
                {
                    DiscountLine discountItem = this.NewDiscountLine(discountApplication.DiscountCode, discountableItemGroups[item.ItemIndex].ItemId);

                    bool discountApplied = true;

                    if (this.DiscountType == DiscountMethodType.LeastExpensive)
                    {
                        discountApplied = this.GetDiscountLinesForRetailDiscountLineItemForLeastExpensive(
                            discountableItemGroups,
                            prices,
                            discountApplication,
                            linesWithDiscount,
                            item,
                            discountItem);
                    }
                    else
                    {
                        this.GetDiscountLinesForRetailDiscountLineItem(
                            prices,
                            priceContext,
                            ref totalPrice,
                            ref discountAmount,
                            ref dealPrice,
                            item,
                            discountItem);
                    }

                    if (discountApplied)
                    {
                        linesWithDiscount.Add(item.ItemIndex);

                        if (result.ContainsKey(item.ItemIndex))
                        {
                            result[item.ItemIndex].Add(new DiscountLineQuantity(discountItem, 1m));
                        }
                        else
                        {
                            result.Add(item.ItemIndex, new List<DiscountLineQuantity>() { new DiscountLineQuantity(discountItem, 1m) });
                        }
                    }
                }
            }

            private void GetDiscountLinesForRetailDiscountLineItem(
                decimal[] prices,
                PriceContext priceContext,
                ref decimal totalPrice,
                ref decimal discountAmount,
                ref decimal dealPrice,
                RetailDiscountLineItem item,
                DiscountLine discountItem)
            {
                RetailDiscountLine discountLine = item.RetailDiscountLine;

                // Handle scaling and rounding loss for discount on multiple lines.
                decimal thisDiscountAmount = decimal.Zero;

                if (this.DiscountType == DiscountMethodType.DiscountAmount && discountAmount > decimal.Zero)
                {
                    // $ off is distributed proportionally to all items that make the mix and match.
                    discountItem.Percentage = 0;
                    thisDiscountAmount = totalPrice != decimal.Zero ? priceContext.CurrencyAndRoundingHelper.Round(discountAmount * prices[item.ItemIndex] / totalPrice) : decimal.Zero;

                    totalPrice -= prices[item.ItemIndex];
                    discountAmount -= thisDiscountAmount;
                    discountItem.Amount = thisDiscountAmount;
                }
                else if (this.DiscountType == DiscountMethodType.DiscountPercent && this.DiscountPercentValue > decimal.Zero)
                {
                    // % off
                    discountItem.Percentage = Math.Min(100m, this.DiscountPercentValue);
                    discountItem.Amount = 0m;
                }
                else if (this.DiscountType == DiscountMethodType.LineSpecific && discountLine.DiscountMethod == (int)DiscountOfferMethod.DiscountPercent && discountLine.DiscountLinePercentOrValue > decimal.Zero)
                {
                    // Line specific % off
                    discountItem.Percentage = Math.Min(100m, discountLine.DiscountLinePercentOrValue);
                    discountItem.Amount = 0m;
                }
                else if ((this.DiscountType == DiscountMethodType.DealPrice || discountLine.DiscountMethod == (int)DiscountOfferMethod.OfferPrice) && dealPrice != decimal.Zero)
                {
                    // Deal price or line specific deal price
                    discountItem.Percentage = 0m;
                    thisDiscountAmount = prices[item.ItemIndex] - (totalPrice != decimal.Zero ? priceContext.CurrencyAndRoundingHelper.Round(dealPrice * (prices[item.ItemIndex] / totalPrice)) : decimal.Zero);
                    thisDiscountAmount = Math.Max(thisDiscountAmount, 0);
                    totalPrice -= prices[item.ItemIndex];
                    dealPrice -= prices[item.ItemIndex] - thisDiscountAmount;

                    discountItem.Amount = thisDiscountAmount;
                    discountItem.DealPrice = prices[item.ItemIndex] - thisDiscountAmount;
                }
            }

            private bool GetDiscountLinesForRetailDiscountLineItemForLeastExpensive(
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] prices,
                DiscountApplication discountApplication,
                List<int> linesWithDiscount,
                RetailDiscountLineItem item,
                DiscountLine discountItem)
            {
                List<Tuple<int, decimal>> lowestPrices = this.GetLeastExpensiveLines(discountApplication, prices);

                bool applyDiscount = (lowestPrices.Count(p => p.Item1 == item.ItemIndex) - linesWithDiscount.Count(p => p == item.ItemIndex)) > 0;

                if (applyDiscount)
                {
                    if (this.DiscountPercentValue != decimal.Zero)
                    {
                        discountItem.Percentage = Math.Min(100m, this.DiscountPercentValue);
                    }
                    else if (this.DiscountAmountValue != decimal.Zero)
                    {
                        discountItem.Amount = Math.Min(this.DiscountAmountValue, discountableItemGroups[item.ItemIndex].Price);
                    }
                    else if (this.DealPriceValue != decimal.Zero)
                    {
                        discountItem.DealPrice = this.DealPriceValue;
                        discountItem.Amount = Math.Max(discountableItemGroups[item.ItemIndex].Price - this.DealPriceValue, 0);
                    }
                }

                return applyDiscount;
            }

            private List<Tuple<int, decimal>> GetLeastExpensiveLines(
                DiscountApplication discountApplication,
                decimal[] prices)
            {
                List<Tuple<int, decimal>> lowestPrices = new List<Tuple<int, decimal>>();
                foreach (KeyValuePair<int, decimal> pair in discountApplication.ItemQuantities)
                {
                    int itemGroupIndex = pair.Key;
                    decimal quantity = pair.Value;

                    for (int y = 0; y < (int)quantity; y++)
                    {
                        lowestPrices.Add(new Tuple<int, decimal>(itemGroupIndex, prices[itemGroupIndex]));
                    }
                }

                lowestPrices = lowestPrices.OrderBy(p => p.Item2).ThenByDescending(p => p.Item1).Take(this.NumberOfLeastExpensiveLines).ToList();
                return lowestPrices;
            }

            private decimal GetValueFromDiscountApplicationAndPrices(
                DiscountApplication discountApplication,
                decimal[] prices,
                Dictionary<int, IList<DiscountLineQuantity>> discountDictionary)
            {
                decimal result = 0M;
                decimal totalPrice;

                decimal numberOfDiscountLines = new decimal(discountApplication.RetailDiscountLines.Count());

                switch (this.DiscountType)
                {
                    case DiscountMethodType.DealPrice:
                        totalPrice = discountApplication.RetailDiscountLines.Sum(p => prices[p.ItemIndex]);
                        result = totalPrice - this.DealPriceValue;
                        break;
                    case DiscountMethodType.DiscountAmount:
                        result = this.DiscountAmountValue;
                        break;
                    case DiscountMethodType.DiscountPercent:
                        totalPrice = discountApplication.RetailDiscountLines.Sum(p => prices[p.ItemIndex]);
                        result = totalPrice * (this.DiscountPercentValue / 100m);
                        break;
                    case DiscountMethodType.LineSpecific:
                        Dictionary<decimal, decimal> retailDiscountLineNumberToDiscountAmoutMapForDealPrice = new Dictionary<decimal, decimal>();
                        foreach (RetailDiscountLineItem retailDiscountLineItem in discountApplication.RetailDiscountLines)
                        {
                            DiscountMethodType discountMethod = TranslateLineDiscountMethod(retailDiscountLineItem.RetailDiscountLine.DiscountMethod);
                            switch (discountMethod)
                            {
                                case DiscountMethodType.DiscountPercent:
                                    result += prices[retailDiscountLineItem.ItemIndex] * (retailDiscountLineItem.RetailDiscountLine.DiscountLinePercentOrValue / 100m);
                                    break;
                                case DiscountMethodType.DealPrice:
                                    decimal dealPrice = retailDiscountLineItem.RetailDiscountLine.DiscountLinePercentOrValue;

                                    // Use the deal price divided by the number of items for this line or for the mix and match group
                                    int numberOfItemsForLine = retailDiscountLineItem.RetailDiscountLine.MixAndMatchLineNumberOfItemsNeeded != 0 ? retailDiscountLineItem.RetailDiscountLine.MixAndMatchLineNumberOfItemsNeeded : discountApplication.RetailDiscountLines.Count(p => p.RetailDiscountLine == retailDiscountLineItem.RetailDiscountLine);

                                    if (numberOfItemsForLine >= 1m)
                                    {
                                        dealPrice /= new decimal(numberOfItemsForLine);
                                    }

                                    decimal bestExistingDealPrice = 0m;
                                    bool hasExistingDealPrice = DiscountBase.TryGetBestExistingDealPrice(discountDictionary, retailDiscountLineItem.ItemIndex, out bestExistingDealPrice);

                                    decimal discountAmount;

                                    // Please note dealPrice is the average one, so we may have negagive result for some items.
                                    if (hasExistingDealPrice && bestExistingDealPrice > dealPrice)
                                    {
                                        discountAmount = bestExistingDealPrice - dealPrice;
                                    }
                                    else
                                    {
                                        discountAmount = prices[retailDiscountLineItem.ItemIndex] - dealPrice;
                                    }

                                    decimal retailDiscountLineDiscountAmount = decimal.Zero;
                                    decimal lineNumber = retailDiscountLineItem.RetailDiscountLine.DiscountLineNumber;
                                    retailDiscountLineNumberToDiscountAmoutMapForDealPrice.TryGetValue(lineNumber, out retailDiscountLineDiscountAmount);
                                    retailDiscountLineNumberToDiscountAmoutMapForDealPrice[lineNumber] = retailDiscountLineDiscountAmount + discountAmount;

                                    break;
                            }
                        }

                        foreach (KeyValuePair<decimal, decimal> pair in retailDiscountLineNumberToDiscountAmoutMapForDealPrice)
                        {
                            // If deal price is over the actual price, take 0 as discount amount, instead of negative discount amount.
                            if (pair.Value > decimal.Zero)
                            {
                                result += pair.Value;
                            }
                        }

                        break;
                    case DiscountMethodType.LeastExpensive:
                        // Find the [NumberOfLeastExpensive] least expensive items by sorting prices and taking the lowest ones.
                        List<Tuple<int, decimal>> lowestPrices = this.GetLeastExpensiveLines(discountApplication, prices);

                        // We support $ off or deal price for least expensive only when number of least expensive items = 1.
                        if (discountApplication.DiscountPercentValue > decimal.Zero)
                        {
                            result = lowestPrices.Sum(p => ((p.Item2 * discountApplication.DiscountPercentValue) / 100m));
                        }
                        else if (discountApplication.DiscountAmountValue > decimal.Zero)
                        {
                            if (lowestPrices.Count >= 1)
                            {
                                result = Math.Min(lowestPrices[0].Item2, discountApplication.DiscountAmountValue);
                            }
                        }
                        else if (discountApplication.DealPriceValue > decimal.Zero)
                        {
                            if (lowestPrices.Count >= 1)
                            {
                                result = lowestPrices[0].Item2 - Math.Min(lowestPrices[0].Item2, discountApplication.DealPriceValue);
                            }
                        }

                        break;

                    default:
                        break;
                }

                return result;
            }

            private DiscountApplication ConstructDiscountApplication(
                DiscountableItemGroup[] discountableItemGroups,
                List<RetailDiscountLineItem> discountLineItems,
                Dictionary<int, decimal> quantitiesForDiscountApplication,
                string discountCodeUsed,
                bool applyStandalone)
            {
                return this.ConstructDiscountApplication(
                    discountableItemGroups,
                    discountLineItems,
                    quantitiesForDiscountApplication,
                    null,
                    discountCodeUsed,
                    applyStandalone);
            }

            private DiscountApplication ConstructDiscountApplication(
                DiscountableItemGroup[] discountableItemGroups,
                List<RetailDiscountLineItem> discountLineItems,
                Dictionary<int, decimal> quantitiesForDiscountApplication,
                decimal[] prices,
                string discountCodeUsed,
                bool applyStandalone)
            {
                DiscountApplication app = new DiscountApplication(this, prices, applyStandalone)
                {
                    RetailDiscountLines = discountLineItems,
                    SortIndex = discountLineItems.Select(p => p.RetailDiscountLine).Distinct().Max(p => this.GetSortIndexForRetailDiscountLine(p)),
                    SortValue = this.GetSortValue(discountLineItems, discountableItemGroups),
                    DiscountCode = discountCodeUsed,
                    NumberOfTimesApplicable = applyStandalone ? 1 : 0,
                };
                app.ItemQuantities.AddRange(quantitiesForDiscountApplication);

                if (this.DiscountType == DiscountMethodType.LeastExpensive)
                {
                    app.DealPriceValue = this.DealPriceValue;
                    app.DiscountAmountValue = this.DiscountAmountValue;
                    app.DiscountPercentValue = this.DiscountPercentValue;
                }

                return app;
            }

            private bool TryOpitimizeDiscountApplicationsForItems(
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] quantitiesApplicable,
                IEnumerable<AppliedDiscountApplication> appliedDiscounts,
                HashSet<int> itemsWithOverlappingDiscounts,
                string discountCodeUsed,
                out IEnumerable<DiscountApplication> discountApplications)
            {
                using (SimpleProfiler profiler = new SimpleProfiler("MixAndMatch.TryOpitimizeDiscountApplicationsForItems", 2))
                {
                    bool isOptimized = false;
                    List<DiscountApplication> discountApplicationList = new List<DiscountApplication>();
                    discountApplications = discountApplicationList;

                    OptimizationOverlapType overlapType = this.CalculateOptimizationOverlapType(
                        quantitiesApplicable,
                        itemsWithOverlappingDiscounts);

                    if (overlapType == OptimizationOverlapType.NoOverlap)
                    {
                        // For now, we only optimize mix and match discount without any overlaps, including between its line groups.
                        // First, get discounted prices
                        decimal[] discountedPrices = new decimal[discountableItemGroups.Length];
                        bool includeAmountOff = true;
                        bool includePercentageOff = true;
                        this.FigureOutCompoundedPriceFlags(out includeAmountOff, out includePercentageOff);

                        this.GetExistingDiscountDictionaryAndDiscountedPrices(
                            discountableItemGroups,
                            quantitiesApplicable,
                            appliedDiscounts,
                            new HashSet<int>(this.ItemGroupIndexToLineGroupSetLookup.Keys),
                            includeAmountOff,
                            includePercentageOff,
                            discountedPrices);

                        IDictionary<string, int[]> lineGroupToOrderedItemGroupIndexArrayLookup = this.GetLineGroupToItemGroupIndexDescendingByPriceArrayLookup(discountedPrices);
                        if (lineGroupToOrderedItemGroupIndexArrayLookup.Count < this.LineGroupToNumberOfItemsMap.Count)
                        {
                            return isOptimized;
                        }

                        decimal[] quantitiesRemainingLocal = quantitiesApplicable.Clone() as decimal[];

                        if (this.DiscountType == DiscountMethodType.DiscountPercent)
                        {
                            this.GetDiscountApplicationsSequentially(
                                discountApplicationList,
                                discountableItemGroups,
                                quantitiesRemainingLocal,
                                discountedPrices,
                                lineGroupToOrderedItemGroupIndexArrayLookup,
                                discountCodeUsed,
                                false);
                            isOptimized = true;
                        }
                        else if (this.DiscountType == DiscountMethodType.DealPrice)
                        {
                            // Optimize for splitting. First, removed low priced items that won't make discount applications, i.e. redundant items and lowest prices items that won't make deal price.
                            this.RemoveLowPricedItemsThatWontMakeDiscountApplications(
                                discountedPrices,
                                quantitiesRemainingLocal,
                                lineGroupToOrderedItemGroupIndexArrayLookup);

                            // Now, all combination would make the deal price. First round, whole item for each line group.
                            this.GetDiscountApplicationsSequentially(
                                discountApplicationList,
                                discountableItemGroups,
                                quantitiesRemainingLocal,
                                discountedPrices,
                                lineGroupToOrderedItemGroupIndexArrayLookup,
                                discountCodeUsed,
                                oneItemGroupForOneLineGroup: true);

                            // Second round, the rest.
                            this.GetDiscountApplicationsSequentially(
                                discountApplicationList,
                                discountableItemGroups,
                                quantitiesRemainingLocal,
                                discountedPrices,
                                lineGroupToOrderedItemGroupIndexArrayLookup,
                                discountCodeUsed,
                                false);

                            isOptimized = true;
                        }
                        else if (this.DiscountType == DiscountMethodType.LineSpecific)
                        {
                            // For line specific, each line can be either deal price or %-off.
                            // If discount line is 1-1 mapping to line group, then we can apply sequentially from highest priced items to lowest.
                            if (this.LineGroupToNumberOfItemsMap.Count == this.DiscountLineNumberToItemGroupIndexSetMap.Count)
                            {
                                this.GetDiscountApplicationsSequentially(
                                    discountApplicationList,
                                    discountableItemGroups,
                                    quantitiesRemainingLocal,
                                    discountedPrices,
                                    lineGroupToOrderedItemGroupIndexArrayLookup,
                                    discountCodeUsed,
                                    false);
                                isOptimized = true;
                            }
                        }
                        else if (this.LineGroupToNumberOfItemsMap.Count == 1)
                        {
                            int[] itemGroupIndexSortedByPriceDescending = lineGroupToOrderedItemGroupIndexArrayLookup.First().Value;

                            if (this.DiscountType == DiscountMethodType.LeastExpensive)
                            {
                                // 1 line group & least expensive: Going from best price down sequentially gives the best deal in any of deal price, $ off or % off.
                                this.GetDiscountApplicationsSequentially(
                                    discountApplicationList,
                                    discountableItemGroups,
                                    quantitiesRemainingLocal,
                                    discountedPrices,
                                    lineGroupToOrderedItemGroupIndexArrayLookup,
                                    discountCodeUsed,
                                    false);
                                isOptimized = true;
                            }
                            else if (this.DiscountType == DiscountMethodType.DiscountAmount)
                            {
                                this.RemoveLowPricedItemsThatWontMakeDiscountApplications(
                                    discountedPrices,
                                    quantitiesRemainingLocal,
                                    lineGroupToOrderedItemGroupIndexArrayLookup);
                                decimal lowestPrice = this.GetLowestPriceMakingMixAndMatch(
                                    discountedPrices,
                                    lineGroupToOrderedItemGroupIndexArrayLookup,
                                    quantitiesRemainingLocal);
                                if (lowestPrice >= this.DiscountAmountValue)
                                {
                                    // Any combination covers the full discount amount.
                                    this.GetDiscountApplicationsSequentially(
                                        discountApplicationList,
                                        discountableItemGroups,
                                        quantitiesRemainingLocal,
                                        discountedPrices,
                                        lineGroupToOrderedItemGroupIndexArrayLookup,
                                        discountCodeUsed,
                                        false);
                                    isOptimized = true;
                                }
                                else
                                {
                                    decimal quantityNeeded = this.LineGroupToNumberOfItemsMap.First().Value;
                                    if (quantityNeeded == 2m)
                                    {
                                        // 1 line group with quantity 2: matching best price and worse price sequentially gives best deal.
                                        this.GetDiscountApplicationsMatchBestPriceWithWorsePriceForOneLineGroupWithQuantity2(
                                            discountApplicationList,
                                            discountableItemGroups,
                                            quantitiesRemainingLocal,
                                            discountedPrices,
                                            itemGroupIndexSortedByPriceDescending,
                                            discountCodeUsed);
                                        isOptimized = true;
                                    }
                                    else if (quantityNeeded > 2m)
                                    {
                                        // 1 line group with quantity 3+, and worse price won't cover full discount: try matching best price with worse price and see if it works.
                                        isOptimized = this.TryGetDiscountApplicationsAmountOffForOneLineGroup(
                                            discountApplicationList,
                                            discountableItemGroups,
                                            quantitiesRemainingLocal,
                                            discountedPrices,
                                            itemGroupIndexSortedByPriceDescending,
                                            discountCodeUsed);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // 2+ line groups
                            if (this.DiscountType == DiscountMethodType.DiscountAmount)
                            {
                                this.RemoveLowPricedItemsThatWontMakeDiscountApplications(
                                    discountedPrices,
                                    quantitiesRemainingLocal,
                                    lineGroupToOrderedItemGroupIndexArrayLookup);
                                decimal lowestPriceMakingMixAndMatch = this.GetLowestPriceMakingMixAndMatch(
                                    discountedPrices,
                                    lineGroupToOrderedItemGroupIndexArrayLookup,
                                    quantitiesRemainingLocal);
                                if (lowestPriceMakingMixAndMatch >= this.DiscountAmountValue)
                                {
                                    // Any combination covers the full amount.
                                    this.GetDiscountApplicationsSequentially(
                                        discountApplicationList,
                                        discountableItemGroups,
                                        quantitiesRemainingLocal,
                                        discountedPrices,
                                        lineGroupToOrderedItemGroupIndexArrayLookup,
                                        discountCodeUsed,
                                        false);
                                    isOptimized = true;
                                }
                            }
                        }
                    }

                    return isOptimized;
                }
            }

            /// <summary>
            /// Get discount application sequentially.
            /// </summary>
            /// <param name="discountApplicationList">Discount application list to be filled.</param>
            /// <param name="discountableItemGroups">Discountable item groups.</param>
            /// <param name="quantitiesRemaining">Quantities remaining.</param>
            /// <param name="prices">Prices, discounted in compounded case.</param>
            /// <param name="lineGroupToOrderedItemGroupIndexArrayLookup">Line group to ordered item group index array lookup.</param>
            /// <param name="discountCodeUsed">Discount code used.</param>
            /// <param name="oneItemGroupForOneLineGroup">A value indicating whether to use one item group for one line group.</param>
            private void GetDiscountApplicationsSequentially(
                List<DiscountApplication> discountApplicationList,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] quantitiesRemaining,
                decimal[] prices,
                IDictionary<string, int[]> lineGroupToOrderedItemGroupIndexArrayLookup,
                string discountCodeUsed,
                bool oneItemGroupForOneLineGroup)
            {
                // The following 3 need to be reset after a new discount application.
                List<RetailDiscountLineItem> discountLineItems = new List<RetailDiscountLineItem>();
                Dictionary<int, decimal> quantitiesForDiscountApplication = new Dictionary<int, decimal>();
                HashSet<int> itemGroupIndexUsed = new HashSet<int>();

                // Need indexed enumeration (lineGroupList) to keep track of indexStarting
                int numberOfLineGroups = this.LineGroupToNumberOfItemsMap.Count;
                List<string> lineGroupList = new List<string>(this.LineGroupToNumberOfItemsMap.Keys);
                int[] indexStartingList = new int[numberOfLineGroups];

                bool keepGoing = true;
                while (keepGoing)
                {
                    for (int indexLineGroup = 0; indexLineGroup < numberOfLineGroups; indexLineGroup++)
                    {
                        string lineGroup = lineGroupList[indexLineGroup];
                        int numberOfItemsNeeded = this.LineGroupToNumberOfItemsMap[lineGroup];
                        decimal quantityNeededForLineGroup = numberOfItemsNeeded;
                        int index = indexStartingList[indexLineGroup];

                        int[] itemGroupIndexSortedByPriceDescending = null;
                        if (!lineGroupToOrderedItemGroupIndexArrayLookup.TryGetValue(lineGroup, out itemGroupIndexSortedByPriceDescending))
                        {
                            // missing items for the line group.
                            keepGoing = false;
                            break;
                        }

                        while (quantityNeededForLineGroup > decimal.Zero && index < itemGroupIndexSortedByPriceDescending.Length)
                        {
                            int itemGroupIndex = itemGroupIndexSortedByPriceDescending[index];
                            decimal quantityUsed = Math.Min(quantityNeededForLineGroup, quantitiesRemaining[itemGroupIndex]);

                            if (quantityUsed < quantityNeededForLineGroup)
                            {
                                // Quantity used up, move to next one.
                                index++;
                                indexStartingList[indexLineGroup] = index;
                            }

                            if (quantityUsed == quantityNeededForLineGroup || (!oneItemGroupForOneLineGroup && quantityUsed > decimal.Zero))
                            {
                                // Will reduce quantitiesRemaining later.
                                itemGroupIndexUsed.Add(itemGroupIndex);

                                quantityNeededForLineGroup -= quantityUsed;
                                DiscountBase.AddToItemQuantities(quantitiesForDiscountApplication, itemGroupIndex, quantityUsed);

                                RetailDiscountLine discountLine = this.GetDiscountLineByLineGroupAndItemGroupIndex(lineGroup, itemGroupIndex);
                                for (decimal j = decimal.Zero; j < quantityUsed; j++)
                                {
                                    discountLineItems.Add(new RetailDiscountLineItem(itemGroupIndex, discountLine));
                                }
                            }
                        }

                        if (quantityNeededForLineGroup > decimal.Zero)
                        {
                            // Quit if we no longer have quantity for a line group.
                            keepGoing = false;
                            break;
                        }
                    }

                    if (keepGoing)
                    {
                        // We've got a discount application
                        DiscountApplication app = this.ConstructDiscountApplication(
                            discountableItemGroups,
                            discountLineItems,
                            quantitiesForDiscountApplication,
                            prices,
                            discountCodeUsed,
                            applyStandalone: true);
                        discountApplicationList.Add(app);

                        // Reduce remaining quantities.
                        foreach (int itemGroupIndex in itemGroupIndexUsed)
                        {
                            quantitiesRemaining[itemGroupIndex] -= quantitiesForDiscountApplication[itemGroupIndex];
                        }

                        // reset discountLineGroups, quantitiesForDiscountApplication
                        discountLineItems = new List<RetailDiscountLineItem>();
                        quantitiesForDiscountApplication.Clear();
                        itemGroupIndexUsed.Clear();
                    }
                }
            }

            private bool TryGetDiscountApplicationsAmountOffForOneLineGroup(
                List<DiscountApplication> discountApplicationList,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] quantitiesRemainingLocal,
                decimal[] prices,
                int[] itemGroupIndexSortedByPriceDescending,
                string discountCodeUsed)
            {
                // Essentially, we're trying to match best price with worse price and see if it works.
                bool isOptimized = true;
                List<DiscountApplication> discountApplicationsLocal = new List<DiscountApplication>();

                bool isHighestPriceBelowWaterIndexSet = false;
                decimal totalApplicableQuantity = decimal.Zero;
                decimal totalQuantityAboveWater = decimal.Zero;
                decimal totalPrice = decimal.Zero;
                decimal quantityNeeded = this.LineGroupToNumberOfItemsMap.First().Value;

                for (int i = 0; i < itemGroupIndexSortedByPriceDescending.Length; i++)
                {
                    int itemGroupIndex = itemGroupIndexSortedByPriceDescending[i];
                    decimal itemQuantity = quantitiesRemainingLocal[itemGroupIndex];
                    if (itemQuantity > decimal.Zero)
                    {
                        totalApplicableQuantity += itemQuantity;
                        decimal itemPrice = prices[itemGroupIndex];
                        totalPrice += itemPrice * itemQuantity;

                        if (!isHighestPriceBelowWaterIndexSet)
                        {
                            if (itemPrice * quantityNeeded >= this.DiscountAmountValue)
                            {
                                totalQuantityAboveWater += itemQuantity;
                            }
                            else
                            {
                                isHighestPriceBelowWaterIndexSet = true;
                            }
                        }
                    }
                }

                decimal numberOfDiscountApplications = Math.Floor(totalApplicableQuantity / quantityNeeded);

                // First rough estimate
                // don't optimize if total price isn't above 120% of discount amount, because we'd see waste in price.
                // don't optimize if total quantity above water makes up less than 60% of total quantity.
                if (totalApplicableQuantity == decimal.Zero ||
                    totalPrice == decimal.Zero ||
                    totalPrice < (numberOfDiscountApplications * this.DiscountAmountValue) * 1.2m ||
                    totalQuantityAboveWater < totalApplicableQuantity * 0.6m)
                {
                    isOptimized = false;
                }

                if (isOptimized)
                {
                    // We'll try match highest with lowest, but it's no guarantee that we can optimize it in such a way that it maximize discount amount.
                    int indexHighestPrice = 0;
                    int indexLowestPrice = itemGroupIndexSortedByPriceDescending.Length - 1;

                    int quantityNeededHalf = (int)Math.Floor(quantityNeeded / 2m);
                    bool isEvenQuantityNeeded = quantityNeeded == (quantityNeededHalf * 2m);

                    // Temporary item group indices holder for constructing a discount application.
                    int[] itemGroupIndicesUsed = new int[(int)quantityNeeded];

                    // The following five need to be reset after every match.
                    decimal quantityHighestNeededLocal = quantityNeededHalf;
                    decimal quantityLowestNeededLocal = quantityNeededHalf;
                    decimal quantityOneMoreNeededLocal = isEvenQuantityNeeded ? decimal.Zero : QuantityOne;
                    decimal amountAccumulated = decimal.Zero;
                    int indexItemGroupIndicesUsed = 0;

                    // Now try matching highest with lowest, with each round fully covering discount amount. Or else optimization fails.
                    while (indexHighestPrice <= indexLowestPrice)
                    {
                        // half of items come from highest price
                        if (quantityHighestNeededLocal > decimal.Zero)
                        {
                            int itemGroupIndexHighest = itemGroupIndexSortedByPriceDescending[indexHighestPrice];
                            decimal quantityHighestUsed = Math.Min(quantitiesRemainingLocal[itemGroupIndexHighest], quantityHighestNeededLocal);
                            if (quantityHighestUsed > decimal.Zero)
                            {
                                quantityHighestNeededLocal -= quantityHighestUsed;
                                quantitiesRemainingLocal[itemGroupIndexHighest] -= quantityHighestUsed;
                                while (quantityHighestUsed > decimal.Zero)
                                {
                                    amountAccumulated += prices[itemGroupIndexHighest];
                                    itemGroupIndicesUsed[indexItemGroupIndicesUsed++] = itemGroupIndexHighest;
                                    quantityHighestUsed -= QuantityOne;
                                }
                            }
                            else
                            {
                                indexHighestPrice++;
                            }
                        }

                        // half come from lowest price
                        if (quantityLowestNeededLocal > decimal.Zero)
                        {
                            int itemGroupIndexLowest = itemGroupIndexSortedByPriceDescending[indexLowestPrice];
                            decimal quantityLowestUsed = Math.Min(quantitiesRemainingLocal[itemGroupIndexLowest], quantityLowestNeededLocal);

                            if (quantityLowestUsed > decimal.Zero)
                            {
                                quantityLowestNeededLocal -= quantityLowestUsed;
                                quantitiesRemainingLocal[itemGroupIndexLowest] -= quantityLowestUsed;
                                while (quantityLowestUsed > decimal.Zero)
                                {
                                    amountAccumulated += prices[itemGroupIndexLowest];
                                    itemGroupIndicesUsed[indexItemGroupIndicesUsed++] = itemGroupIndexLowest;
                                    quantityLowestUsed -= QuantityOne;
                                }
                            }
                            else
                            {
                                indexLowestPrice--;
                            }
                        }

                        if (quantityHighestNeededLocal == decimal.Zero && quantityLowestNeededLocal == decimal.Zero)
                        {
                            // When number of items required is odd number, we need to get one more to make mix and match.
                            if (quantityOneMoreNeededLocal > decimal.Zero)
                            {
                                if (amountAccumulated >= this.DiscountAmountValue)
                                {
                                    // We've got enough price to cover discount amount fully, get one from lowest price
                                    int itemGroupIndexLowest = itemGroupIndexSortedByPriceDescending[indexLowestPrice];
                                    if (quantitiesRemainingLocal[itemGroupIndexLowest] >= QuantityOne)
                                    {
                                        // no need to accumulate more amountAccumulated as it covers discount amount already.
                                        itemGroupIndicesUsed[indexItemGroupIndicesUsed++] = itemGroupIndexLowest;
                                        quantitiesRemainingLocal[itemGroupIndexLowest] -= QuantityOne;
                                        quantityOneMoreNeededLocal -= QuantityOne;
                                    }
                                    else
                                    {
                                        indexLowestPrice--;
                                    }
                                }
                                else
                                {
                                    // Not enough price to cover discount amount fully, get one from highest price
                                    int itemGroupIndexHighest = itemGroupIndexSortedByPriceDescending[indexHighestPrice];
                                    if (quantitiesRemainingLocal[itemGroupIndexHighest] >= QuantityOne)
                                    {
                                        amountAccumulated += prices[itemGroupIndexHighest];
                                        itemGroupIndicesUsed[indexItemGroupIndicesUsed++] = itemGroupIndexHighest;
                                        quantitiesRemainingLocal[itemGroupIndexHighest] -= QuantityOne;
                                        quantityOneMoreNeededLocal -= QuantityOne;
                                    }
                                    else
                                    {
                                        indexHighestPrice++;
                                    }
                                }
                            }

                            if (quantityOneMoreNeededLocal == decimal.Zero)
                            {
                                // Now we've got all the items for mix and match. It's time to make or break.
                                if (amountAccumulated >= this.DiscountAmountValue)
                                {
                                    // The item combination covers the discount amount fully, let's make it a discount application.
                                    List<RetailDiscountLineItem> discountLineItems = new List<RetailDiscountLineItem>();
                                    Dictionary<int, decimal> quantitiesForDiscountApplication = new Dictionary<int, decimal>();
                                    foreach (int itemGroupIndex in itemGroupIndicesUsed)
                                    {
                                        discountLineItems.Add(this.CreateRetailDiscountLineItem(itemGroupIndex));
                                        DiscountBase.AddToItemQuantities(quantitiesForDiscountApplication, itemGroupIndex, QuantityOne);
                                    }

                                    DiscountApplication app = this.ConstructDiscountApplication(
                                        discountableItemGroups,
                                        discountLineItems,
                                        quantitiesForDiscountApplication,
                                        prices,
                                        discountCodeUsed,
                                        applyStandalone: true);
                                    discountApplicationsLocal.Add(app);

                                    quantityHighestNeededLocal = quantityNeededHalf;
                                    quantityLowestNeededLocal = quantityNeededHalf;
                                    quantityOneMoreNeededLocal = isEvenQuantityNeeded ? decimal.Zero : QuantityOne;
                                    amountAccumulated = decimal.Zero;
                                    indexItemGroupIndicesUsed = 0;
                                }
                                else
                                {
                                    // Optimization fails, back to regular processing.
                                    isOptimized = false;
                                    break;
                                }
                            }
                        }
                    }

                    if (isOptimized)
                    {
                        discountApplicationList.AddRange(discountApplicationsLocal);
                    }
                }

                return isOptimized;
            }

            private void GetDiscountApplicationsMatchBestPriceWithWorsePriceForOneLineGroupWithQuantity2(
                List<DiscountApplication> discountApplicationList,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] quantitiesRemainingLocal,
                decimal[] prices,
                int[] itemGroupIndexSortedByPriceDescending,
                string discountCodeUsed)
            {
                // Theorem 1: a>b>c>d, then min(a+d, 0) + min(b+c, 0) >= min(a+c, 0) + min(b+d, 0). In other words, the former combination of matching highest and lowest achieves the best result. 
                // Theorem: (even numbers) a>b>c>d>e>f...., then we can use previous theorem to exchange elements to achieve the best result of sum (min(x+y, 0)) by pairing best and worse one by one.
                // We're matching highest price with lowest price step by step. Note: redundant items have already removed.
                int left = 0;
                int right = itemGroupIndexSortedByPriceDescending.Length - 1;

                while (left <= right)
                {
                    int leftItemGroupIndex = itemGroupIndexSortedByPriceDescending[left];
                    int rightItemGroupIndex = itemGroupIndexSortedByPriceDescending[right];
                    decimal leftQuantity = quantitiesRemainingLocal[leftItemGroupIndex];
                    decimal rightQuantity = quantitiesRemainingLocal[rightItemGroupIndex];

                    if (leftQuantity >= QuantityOne && rightQuantity >= QuantityOne)
                    {
                        List<RetailDiscountLineItem> discountLineItems = new List<RetailDiscountLineItem>();
                        discountLineItems.Add(this.CreateRetailDiscountLineItem(leftItemGroupIndex));
                        discountLineItems.Add(this.CreateRetailDiscountLineItem(rightItemGroupIndex));
                        Dictionary<int, decimal> quantitiesForDiscountApplication = new Dictionary<int, decimal>();
                        if (leftItemGroupIndex != rightItemGroupIndex)
                        {
                            quantitiesForDiscountApplication[leftItemGroupIndex] = QuantityOne;
                            quantitiesForDiscountApplication[rightItemGroupIndex] = QuantityOne;
                        }
                        else
                        {
                            quantitiesForDiscountApplication[leftItemGroupIndex] = QuantityOne + QuantityOne;
                        }

                        DiscountApplication app = this.ConstructDiscountApplication(
                            discountableItemGroups,
                            discountLineItems,
                            quantitiesForDiscountApplication,
                            prices,
                            discountCodeUsed,
                            applyStandalone: true);
                        discountApplicationList.Add(app);

                        quantitiesRemainingLocal[leftItemGroupIndex] -= QuantityOne;
                        quantitiesRemainingLocal[rightItemGroupIndex] -= QuantityOne;
                    }
                    else
                    {
                        if (leftQuantity < QuantityOne)
                        {
                            left++;
                        }

                        if (rightQuantity < QuantityOne)
                        {
                            right--;
                        }
                    }
                }
            }

            private void CalculateEstimate(
                bool includeExternallyOverlappedItems,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                HashSet<int> itemsWithOverlappingDiscountsExcludingCompoundedOnly,
                out decimal totalNumberOfDiscountApplications,
                out decimal totalDiscountAmount,
                out decimal totalApplicableQuantity)
            {
                decimal[] remainingQuantitiesLocal = (decimal[])remainingQuantities.Clone();

                if (!includeExternallyOverlappedItems)
                {
                    // Exclude externally overlapped items.
                    foreach (int itemGroupIndex in itemsWithOverlappingDiscountsExcludingCompoundedOnly)
                    {
                        remainingQuantitiesLocal[itemGroupIndex] = decimal.Zero;
                    }
                }

                // Step 1: construct line-group-overlapped item group index set.
                HashSet<int> overlappedItemGroupIndexSet = this.GetOverlappedItemGroupIndexSet(remainingQuantitiesLocal);
                Dictionary<string, decimal> lineGroupToQuantityLookup = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, Dictionary<int, decimal>> lineGroupToItemGroupIndexToAllocatedQuantityLookup = new Dictionary<string, Dictionary<int, decimal>>(StringComparer.OrdinalIgnoreCase);
                if (overlappedItemGroupIndexSet.Any())
                {
                    this.AllocateOverlappedItemsToLineGroups(
                        lineGroupToQuantityLookup,
                        lineGroupToItemGroupIndexToAllocatedQuantityLookup,
                        overlappedItemGroupIndexSet,
                        remainingQuantitiesLocal);
                }
                else
                {
                    // No overlap, sum up lineGroupToQuantityLookup quickly
                    foreach (KeyValuePair<string, HashSet<int>> pair in this.LineGroupToItemGroupIndexSetLookup)
                    {
                        string lineGroup = pair.Key;
                        HashSet<int> itemGroupIndexSetForLineGroup = pair.Value;

                        lineGroupToQuantityLookup[lineGroup] = itemGroupIndexSetForLineGroup.Sum(p => remainingQuantitiesLocal[p]);
                    }
                }

                // Step 3: get number of discount applications;
                totalNumberOfDiscountApplications = decimal.Zero;
                Dictionary<string, decimal> lineGroupToAveragePriceLookup = new Dictionary<string, decimal>();
                foreach (KeyValuePair<string, int> pair in this.LineGroupToNumberOfItemsMap)
                {
                    string lineGroup = pair.Key;
                    decimal quantityNeededForLineGroup = pair.Value;
                    decimal netPrice = decimal.Zero;
                    decimal quantityForLineGroup = lineGroupToQuantityLookup[lineGroup];

                    decimal numberOfDiscountApplicationsForLineGroup = Math.Floor(quantityForLineGroup / quantityNeededForLineGroup);
                    if (numberOfDiscountApplicationsForLineGroup > decimal.Zero)
                    {
                        if (totalNumberOfDiscountApplications == decimal.Zero)
                        {
                            totalNumberOfDiscountApplications = numberOfDiscountApplicationsForLineGroup;
                        }
                        else
                        {
                            totalNumberOfDiscountApplications = Math.Min(totalNumberOfDiscountApplications, numberOfDiscountApplicationsForLineGroup);
                        }
                    }
                    else
                    {
                        totalNumberOfDiscountApplications = decimal.Zero;
                        break;
                    }

                    HashSet<int> itemGroupIndexSetForLineGroup = this.LineGroupToItemGroupIndexSetLookup[lineGroup];
                    foreach (int itemGroupInex in itemGroupIndexSetForLineGroup)
                    {
                        decimal itemQuantity = decimal.Zero;
                        if (overlappedItemGroupIndexSet.Contains(itemGroupInex))
                        {
                            Dictionary<int, decimal> itemGroupIndexToQuantityAllocated = null;
                            if (lineGroupToItemGroupIndexToAllocatedQuantityLookup.TryGetValue(lineGroup, out itemGroupIndexToQuantityAllocated))
                            {
                                itemGroupIndexToQuantityAllocated.TryGetValue(itemGroupInex, out itemQuantity);
                            }
                        }
                        else
                        {
                            itemQuantity = remainingQuantitiesLocal[itemGroupInex];
                        }

                        netPrice += discountableItemGroups[itemGroupInex].Price * itemQuantity;
                    }

                    lineGroupToAveragePriceLookup[lineGroup] = quantityForLineGroup > decimal.Zero ? netPrice / quantityForLineGroup : decimal.Zero;
                }

                decimal quantityPerDiscountApplication = this.LineGroupToNumberOfItemsMap.Sum(p => p.Value);

                // Step 4: estimate (on average) single discount application value.
                totalApplicableQuantity = quantityPerDiscountApplication * totalNumberOfDiscountApplications;
                decimal singleDiscountApplicationValue = decimal.Zero;
                if (totalNumberOfDiscountApplications > decimal.Zero)
                {
                    switch (this.DiscountType)
                    {
                        case DiscountMethodType.DiscountAmount:
                            singleDiscountApplicationValue = this.DiscountAmountValue;
                            break;
                        case DiscountMethodType.DiscountPercent:
                            singleDiscountApplicationValue = DiscountBase.GetDiscountAmountForPercentageOff(
                                lineGroupToAveragePriceLookup.Sum(p => p.Value * this.LineGroupToNumberOfItemsMap[p.Key]),
                                this.DiscountPercentValue);
                            break;
                        case DiscountMethodType.DealPrice:
                            singleDiscountApplicationValue = lineGroupToAveragePriceLookup.Sum(p => p.Value * this.LineGroupToNumberOfItemsMap[p.Key]) - this.DealPriceValue;
                            break;
                        case DiscountMethodType.LeastExpensive:
                            decimal leastAveragePrice = lineGroupToAveragePriceLookup.Min(p => p.Value);
                            if (this.DiscountPercentValue > decimal.Zero)
                            {
                                singleDiscountApplicationValue = DiscountBase.GetDiscountAmountForPercentageOff(leastAveragePrice * this.NumberOfLeastExpensiveLines, this.DiscountPercentValue);
                            }
                            else if (this.DiscountAmountValue > decimal.Zero)
                            {
                                singleDiscountApplicationValue = this.DiscountAmountValue;
                            }
                            else if (this.DealPriceValue > decimal.Zero)
                            {
                                singleDiscountApplicationValue = leastAveragePrice - this.DealPriceValue;
                            }

                            break;
                        case DiscountMethodType.LineSpecific:
                            foreach (KeyValuePair<string, HashSet<decimal>> pair in this.LineGroupToDiscountLineNumberSetLookup)
                            {
                                string lineGroup = pair.Key;
                                HashSet<decimal> discountLineNumberSet = pair.Value;
                                if (discountLineNumberSet.Any())
                                {
                                    RetailDiscountLine discountLineDefinition = this.DiscountLines[discountLineNumberSet.First()];
                                    if (discountLineDefinition.MixAndMatchLineSpecificDiscountType == (int)DiscountOfferMethod.OfferPrice)
                                    {
                                        singleDiscountApplicationValue += (lineGroupToAveragePriceLookup[lineGroup] * this.LineGroupToNumberOfItemsMap[lineGroup]) - discountLineDefinition.DiscountLinePercentOrValue;
                                    }
                                    else
                                    {
                                        singleDiscountApplicationValue = DiscountBase.GetDiscountAmountForPercentageOff(lineGroupToAveragePriceLookup[lineGroup] * this.LineGroupToNumberOfItemsMap[lineGroup], discountLineDefinition.DiscountLinePercentOrValue);
                                    }
                                }
                            }

                            break;
                    }
                }

                // Step 5: estimate total discount amount.
                totalDiscountAmount = singleDiscountApplicationValue * totalNumberOfDiscountApplications;
            }

            private RetailDiscountLineItem CreateRetailDiscountLineItem(int itemGroupIndex)
            {
                RetailDiscountLine discountLine = this.DiscountLines[this.ItemGroupIndexToDiscountLineNumberSetMap[itemGroupIndex].First()];
                return new RetailDiscountLineItem(itemGroupIndex, discountLine);
            }

            private RetailDiscountLine GetDiscountLineByLineGroupAndItemGroupIndex(string lineGroup, int itemGroupIndex)
            {
                RetailDiscountLine discountLine = null;

                HashSet<decimal> discountLineNumberSet = this.ItemGroupIndexToDiscountLineNumberSetMap[itemGroupIndex];
                foreach (decimal discountLineNumber in discountLineNumberSet)
                {
                    RetailDiscountLine discountLineInLoop = this.DiscountLines[discountLineNumber];

                    if (string.Equals(lineGroup, discountLineInLoop.MixAndMatchLineGroup, StringComparison.OrdinalIgnoreCase))
                    {
                        discountLine = discountLineInLoop;
                        break;
                    }
                }

                return discountLine;
            }

            private OptimizationOverlapType CalculateOptimizationOverlapType(
                decimal[] quantityRemaining,
                HashSet<int> itemsWithOverlappingDiscounts)
            {
                using (SimpleProfiler profiler = new SimpleProfiler("MixAndMatch.CalculateOptimizationOverlapType", 2))
                {
                    OptimizationOverlapType overlapType = OptimizationOverlapType.NoOverlap;

                    foreach (KeyValuePair<int, HashSet<string>> pair in this.ItemGroupIndexToLineGroupSetLookup)
                    {
                        if (quantityRemaining[pair.Key] > decimal.Zero)
                        {
                            if (itemsWithOverlappingDiscounts.Contains(pair.Key))
                            {
                                overlapType = OptimizationOverlapType.ExternalOverlap;
                                break;
                            }
                            else if (overlapType != OptimizationOverlapType.ExternalOverlap && pair.Value.Count > 1)
                            {
                                overlapType = OptimizationOverlapType.InternalOverlap;
                            }
                        }
                    }

                    return overlapType;
                }
            }

            private decimal GetLowestPriceMakingMixAndMatch(
                decimal[] discountedPrices,
                IDictionary<string, int[]> lineGroupToOrderedItemGroupIndexArrayLookup,
                decimal[] quantitiesRemaining)
            {
                using (SimpleProfiler profiler = new SimpleProfiler("MixAndMatch.GetLowestPriceMakingMixAndMatch", 2))
                {
                    // Note: we can optimize this further by removing redundant (unused) items.
                    decimal lowestPriceSummarized = decimal.Zero;
                    decimal[] quantitiesRemainingLocal = quantitiesRemaining.Clone() as decimal[];

                    foreach (KeyValuePair<string, int> pair in this.LineGroupToNumberOfItemsMap)
                    {
                        string lineGroup = pair.Key;
                        int numberOfItemsNeeded = pair.Value;
                        decimal quantityNeeded = numberOfItemsNeeded;

                        int[] itemGroupIndexSortedByPriceDescending = null;
                        if (lineGroupToOrderedItemGroupIndexArrayLookup.TryGetValue(lineGroup, out itemGroupIndexSortedByPriceDescending))
                        {
                            // index starts at the end for the lowest price.
                            int indexFromLast = itemGroupIndexSortedByPriceDescending.Length - 1;

                            // stop when we reached quantity needed or index out of range.
                            while (quantityNeeded > decimal.Zero && indexFromLast >= 0)
                            {
                                int itemGroupIndex = itemGroupIndexSortedByPriceDescending[indexFromLast];

                                decimal quantityUsed = Math.Min(quantityNeeded, quantitiesRemainingLocal[itemGroupIndex]);

                                if (quantityUsed > decimal.Zero)
                                {
                                    quantitiesRemainingLocal[itemGroupIndex] -= quantityUsed;
                                    quantityNeeded -= quantityUsed;
                                    lowestPriceSummarized += quantityUsed * discountedPrices[itemGroupIndex];
                                }
                                else
                                {
                                    indexFromLast--;
                                }
                            }
                        }

                        if (quantityNeeded > decimal.Zero)
                        {
                            // no match found.
                            lowestPriceSummarized = decimal.Zero;
                            break;
                        }
                    }

                    return lowestPriceSummarized;
                }
            }

            private void AllocateOverlappedQuantityToLineGroups(
                Dictionary<string, Dictionary<int, decimal>> lineGroupToItemGroupIndexToAllocatedQuantityLookup,
                Dictionary<string, decimal> lineGroupToQuantityLookup,
                string[] itemLineGroupSorted,
                decimal[] remainingQuantitiesLocal,
                int itemGroupIndex,
                int lastItemLineGroupIndex,
                decimal numberOfDiscountApplicationsToAchieve)
            {
                for (int lineGroupIndexToDistribute = lastItemLineGroupIndex; lineGroupIndexToDistribute >= 0; lineGroupIndexToDistribute--)
                {
                    string lineGroupToDistribute = itemLineGroupSorted[lineGroupIndexToDistribute];
                    decimal existingQuantity = lineGroupToQuantityLookup[lineGroupToDistribute];
                    decimal quantityNeeded = (numberOfDiscountApplicationsToAchieve * this.LineGroupToNumberOfItemsMap[lineGroupToDistribute]) - existingQuantity;
                    if (quantityNeeded > decimal.Zero)
                    {
                        decimal quantityUsed = Math.Min(quantityNeeded, remainingQuantitiesLocal[itemGroupIndex]);
                        if (quantityUsed > decimal.Zero)
                        {
                            AllocateOverlappedQuantityToLineGroup(
                                lineGroupToItemGroupIndexToAllocatedQuantityLookup,
                                remainingQuantitiesLocal,
                                lineGroupToDistribute,
                                itemGroupIndex,
                                quantityUsed);
                            lineGroupToQuantityLookup[lineGroupToDistribute] += quantityUsed;
                        }
                    }
                }
            }

            // Build 3 lookups from line group to
            //   quantity non overlapped
            //   quantity overlapped
            //   overlapped item group index set, available if needed.
            private void BuildLineGroupLookupsInFastMode(
                Dictionary<string, decimal> lineGroupToQuantityNonOverlappedLookup,
                Dictionary<string, decimal> lineGroupToQuantityOverlappedLookup,
                Dictionary<string, HashSet<int>> lineGroupToOverlappedItemGroupIndexSetLookup,
                decimal[] remainingQuantities,
                HashSet<int> overlappedItemGroupIndexSet)
            {
                foreach (KeyValuePair<string, HashSet<int>> pair in this.LineGroupToItemGroupIndexSetLookup)
                {
                    string lineGroup = pair.Key;
                    HashSet<int> itemGroupIndexSetForLineGroup = pair.Value;

                    decimal quantityNonOverlapped = decimal.Zero;
                    decimal quantityOverlapped = decimal.Zero;
                    HashSet<int> overlappedItemGroupIndexSetForLineGroup = null;
                    foreach (int itemGroupIndex in itemGroupIndexSetForLineGroup)
                    {
                        if (overlappedItemGroupIndexSet.Contains(itemGroupIndex))
                        {
                            quantityOverlapped += remainingQuantities[itemGroupIndex];
                            if (overlappedItemGroupIndexSetForLineGroup != null)
                            {
                                overlappedItemGroupIndexSetForLineGroup.Add(itemGroupIndex);
                            }
                            else
                            {
                                overlappedItemGroupIndexSetForLineGroup = new HashSet<int>() { itemGroupIndex };
                                lineGroupToOverlappedItemGroupIndexSetLookup[lineGroup] = overlappedItemGroupIndexSetForLineGroup;
                            }
                        }
                        else
                        {
                            quantityNonOverlapped += remainingQuantities[itemGroupIndex];
                        }
                    }

                    lineGroupToQuantityNonOverlappedLookup[lineGroup] = quantityNonOverlapped;
                    lineGroupToQuantityOverlappedLookup[lineGroup] = quantityOverlapped;
                }
            }

            // Build line group to sorted-by-price item group index array look up: lineGroupToSortedItemGroupIndexArrayLookup
            // Later, when we build discount applications, we start from highest priced item for each line group for better deal.
            private void BuildLineGroupToSortedItemGroupIndexArrayLookup(
                Dictionary<string, int[]> lineGroupToSortedItemGroupIndexArrayLookup,
                Dictionary<string, Dictionary<int, decimal>> lineGroupToItemGroupIndexToQuantityLookup,
                DiscountableItemGroup[] discountableItemGroups,
                HashSet<int> overlappedItemGroupIndexSet)
            {
                Dictionary<int, decimal> itemPriceLookupForSorting = new Dictionary<int, decimal>();
                for (int itemGroupIndex = 0; itemGroupIndex < discountableItemGroups.Length; itemGroupIndex++)
                {
                    // This is for fast mode, no need to use discounted prices.
                    itemPriceLookupForSorting[itemGroupIndex] = discountableItemGroups[itemGroupIndex].Price;
                }

                this.BuildLineGroupToSortedItemGroupIndexArrayLookup(
                    lineGroupToSortedItemGroupIndexArrayLookup,
                    lineGroupToItemGroupIndexToQuantityLookup,
                    overlappedItemGroupIndexSet,
                    itemPriceLookupForSorting);
            }

            // Build line group to sorted-by-price item group index array look up: lineGroupToSortedItemGroupIndexArrayLookup
            private void BuildLineGroupToSortedItemGroupIndexArrayLookup(
                Dictionary<string, int[]> lineGroupToSortedItemGroupIndexArrayLookup,
                Dictionary<string, Dictionary<int, decimal>> lineGroupToItemGroupIndexToQuantityLookup,
                HashSet<int> overlappedItemGroupIndexSet,
                Dictionary<int, decimal> itemPriceLookupForSorting)
            {
                foreach (KeyValuePair<string, HashSet<int>> pair in this.LineGroupToItemGroupIndexSetLookup)
                {
                    string lineGroup = pair.Key;
                    HashSet<int> itemGroupIndexSetForLineGroup = new HashSet<int>(pair.Value);

                    // Remove all overlapped item group indices first.
                    itemGroupIndexSetForLineGroup.ExceptWith(overlappedItemGroupIndexSet);

                    // Add allocated overlapped item group indices.
                    Dictionary<int, decimal> itemGroupIndexToOverlappedQuantityLookup = null;
                    if (lineGroupToItemGroupIndexToQuantityLookup.TryGetValue(lineGroup, out itemGroupIndexToOverlappedQuantityLookup))
                    {
                        itemGroupIndexSetForLineGroup.AddRange(itemGroupIndexToOverlappedQuantityLookup.Keys);
                    }

                    int[] itemGroupIndicesSorted = itemGroupIndexSetForLineGroup.ToArray();
                    ItemPriceComparer itemPriceComparer = new ItemPriceComparer(itemPriceLookupForSorting);
                    Array.Sort(itemGroupIndicesSorted, itemPriceComparer.CompareItemPriceByItemGroupIndexDescending);
                    lineGroupToSortedItemGroupIndexArrayLookup[lineGroup] = itemGroupIndicesSorted;
                }
            }

            // It will use (reduce) quantities in remainingQuantitiesLocal and lineGroupToItemGroupIndexToAllocatedQuantityLookup
            private List<DiscountApplication> ConstructDiscountApplicationsFastMode(
                Dictionary<string, int[]> lineGroupToSortedItemGroupIndexArrayLookup,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantitiesLocal,
                Dictionary<string, Dictionary<int, decimal>> lineGroupToItemGroupIndexToAllocatedQuantityLookup,
                HashSet<int> overlappedItemGroupIndexSet,
                string discountCodeUsed)
            {
                // This is the last step of fast mode: construction discount applications.
                // Pre-requsite: quantities of overlapped items allocated to each item in lineGroupToItemGroupIndexToAllocatedQuantityLookup.
                List<DiscountApplication> discountApplications = new List<DiscountApplication>();
                bool keepGoing = true;

                // Main Step: get discount application one by one, and used up all available quantities.
                while (keepGoing)
                {
                    List<RetailDiscountLineItem> discountLineItems = new List<RetailDiscountLineItem>();
                    Dictionary<int, decimal> quantitiesForDiscountApplication = new Dictionary<int, decimal>();

                    foreach (KeyValuePair<string, int> pair in this.LineGroupToNumberOfItemsMap)
                    {
                        string lineGroup = pair.Key;
                        decimal quantityNeededForLineGroup = pair.Value;

                        int[] itemGroupIndices = null;
                        if (!lineGroupToSortedItemGroupIndexArrayLookup.TryGetValue(lineGroup, out itemGroupIndices))
                        {
                            keepGoing = false;
                            break;
                        }

                        for (int index = 0; index < itemGroupIndices.Length; index++)
                        {
                            int itemGroupIndex = itemGroupIndices[index];
                            decimal quantityAvailableForItemGroupIndex = GetAvailableQuantityForItem(
                                lineGroup,
                                itemGroupIndex,
                                remainingQuantitiesLocal,
                                overlappedItemGroupIndexSet,
                                lineGroupToItemGroupIndexToAllocatedQuantityLookup);

                            decimal quantityUsed = Math.Min(quantityAvailableForItemGroupIndex, quantityNeededForLineGroup);

                            if (quantityUsed > decimal.Zero)
                            {
                                quantityNeededForLineGroup -= quantityUsed;
                                this.PrepareDiscountApplicationForLineGroupAndItemAndReduceQuantity(
                                    lineGroup,
                                    itemGroupIndex,
                                    quantityUsed,
                                    discountLineItems,
                                    quantitiesForDiscountApplication,
                                    remainingQuantitiesLocal,
                                    overlappedItemGroupIndexSet,
                                    lineGroupToItemGroupIndexToAllocatedQuantityLookup);
                            }

                            // I've got enough, break foreach(index in itemGroupIndices)
                            if (quantityNeededForLineGroup == decimal.Zero)
                            {
                                break;
                            }
                        }

                        if (quantityNeededForLineGroup > decimal.Zero)
                        {
                            // Quit if we no longer have quantity for a line group.
                            keepGoing = false;
                            break;
                        }
                    }

                    if (keepGoing)
                    {
                        // We've got a discount application
                        DiscountApplication app = this.ConstructDiscountApplication(
                            discountableItemGroups,
                            discountLineItems,
                            quantitiesForDiscountApplication,
                            discountCodeUsed,
                            applyStandalone: true);
                        discountApplications.Add(app);
                    }
                }

                return discountApplications;
            }

            // It will use (reduce) quantities in remainingQuantitiesLocal and lineGroupToItemGroupIndexToAllocatedQuantityLookup
            private List<DiscountApplication> ConstructDiscountApplicationsForLeastExpensiveFavorRetailer(
                Dictionary<string, int[]> lineGroupToSortedItemGroupIndexArrayLookup,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantitiesLocal,
                Dictionary<string, Dictionary<int, decimal>> lineGroupToItemGroupIndexToAllocatedQuantityLookup,
                HashSet<int> overlappedItemGroupIndexSet,
                decimal[] discountedPrices,
                string discountCodeUsed)
            {
                // Pre-requsite: quantities of overlapped items allocated to each item in lineGroupToItemGroupIndexToAllocatedQuantityLookup.
                List<DiscountApplication> discountApplications = new List<DiscountApplication>();
                bool keepGoing = true;

                // Main Step: get discount application one by one with least priced items, and used up all available quantities.
                while (keepGoing)
                {
                    // Get the least expensive items (to discount) first
                    Dictionary<string, Dictionary<int, decimal>> leastExpensiveLookup = new Dictionary<string, Dictionary<int, decimal>>(StringComparer.OrdinalIgnoreCase);
                    keepGoing = this.TryGetLeastExpensiveLookup(
                        lineGroupToSortedItemGroupIndexArrayLookup,
                        remainingQuantitiesLocal,
                        overlappedItemGroupIndexSet,
                        lineGroupToItemGroupIndexToAllocatedQuantityLookup,
                        discountedPrices,
                        leastExpensiveLookup);

                    if (keepGoing)
                    {
                        DiscountApplication discountApplication = this.TryConstructOneDiscountApplicationForLeastExpensiveFavorRetailer(
                            lineGroupToSortedItemGroupIndexArrayLookup,
                            discountableItemGroups,
                            remainingQuantitiesLocal,
                            lineGroupToItemGroupIndexToAllocatedQuantityLookup,
                            overlappedItemGroupIndexSet,
                            leastExpensiveLookup,
                            discountedPrices,
                            discountCodeUsed);

                        if (discountApplication != null)
                        {
                            discountApplications.Add(discountApplication);
                        }
                        else
                        {
                            keepGoing = false;
                        }
                    }
                }

                return discountApplications;
            }

            private DiscountApplication TryConstructOneDiscountApplicationForLeastExpensiveFavorRetailer(
                Dictionary<string, int[]> lineGroupToSortedItemGroupIndexArrayLookup,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantitiesLocal,
                Dictionary<string, Dictionary<int, decimal>> lineGroupToItemGroupIndexToAllocatedQuantityLookup,
                HashSet<int> overlappedItemGroupIndexSet,
                Dictionary<string, Dictionary<int, decimal>> leastExpensiveLookup,
                decimal[] discountedPrices,
                string discountCodeUsed)
            {
                List<RetailDiscountLineItem> discountLineItems = new List<RetailDiscountLineItem>();
                Dictionary<int, decimal> quantitiesForDiscountApplication = new Dictionary<int, decimal>();

                bool hasEnoughQuantity = true;
                foreach (KeyValuePair<string, int> pair in this.LineGroupToNumberOfItemsMap)
                {
                    string lineGroup = pair.Key;
                    decimal quantityNeededForLineGroup = pair.Value;

                    Dictionary<int, decimal> leastExpensiveItemQuantityForLineGroup = null;
                    if (leastExpensiveLookup.TryGetValue(lineGroup, out leastExpensiveItemQuantityForLineGroup))
                    {
                        decimal quantityCoveredForLineGroup = leastExpensiveItemQuantityForLineGroup.Sum(p => p.Value);
                        quantityNeededForLineGroup -= quantityCoveredForLineGroup;
                    }

                    if (quantityNeededForLineGroup > decimal.Zero)
                    {
                        int[] itemGroupIndices = null;
                        if (!lineGroupToSortedItemGroupIndexArrayLookup.TryGetValue(lineGroup, out itemGroupIndices))
                        {
                            hasEnoughQuantity = false;
                            break;
                        }

                        for (int index = 0; index < itemGroupIndices.Length; index++)
                        {
                            int itemGroupIndex = itemGroupIndices[index];
                            decimal quantityAvailableForItemGroupIndex = GetAvailableQuantityForItem(
                                lineGroup,
                                itemGroupIndex,
                                remainingQuantitiesLocal,
                                overlappedItemGroupIndexSet,
                                lineGroupToItemGroupIndexToAllocatedQuantityLookup);

                            decimal quantityUsed = Math.Min(quantityAvailableForItemGroupIndex, quantityNeededForLineGroup);
                            if (quantityUsed > decimal.Zero)
                            {
                                quantityNeededForLineGroup -= quantityUsed;
                                this.PrepareDiscountApplicationForLineGroupAndItemAndReduceQuantity(
                                    lineGroup,
                                    itemGroupIndex,
                                    quantityUsed,
                                    discountLineItems,
                                    quantitiesForDiscountApplication,
                                    remainingQuantitiesLocal,
                                    overlappedItemGroupIndexSet,
                                    lineGroupToItemGroupIndexToAllocatedQuantityLookup);
                            }

                            // I've got enough, break foreach(index in itemGroupIndices)
                            if (quantityNeededForLineGroup == decimal.Zero)
                            {
                                break;
                            }
                        }
                    }

                    if (quantityNeededForLineGroup > decimal.Zero)
                    {
                        hasEnoughQuantity = false;
                        break;
                    }
                }

                DiscountApplication app = null;
                if (hasEnoughQuantity)
                {
                    // Add least expensive items
                    foreach (KeyValuePair<string, Dictionary<int, decimal>> pair in leastExpensiveLookup)
                    {
                        string lineGroup = pair.Key;
                        foreach (KeyValuePair<int, decimal> itemQuantityPair in pair.Value)
                        {
                            int itemGroupIndex = itemQuantityPair.Key;
                            decimal quantity = itemQuantityPair.Value;

                            this.PrepareDiscountApplicationForLineGroupAndItem(
                                lineGroup,
                                itemGroupIndex,
                                quantity,
                                discountLineItems,
                                quantitiesForDiscountApplication);
                        }
                    }

                    app = this.ConstructDiscountApplication(
                        discountableItemGroups,
                        discountLineItems,
                        quantitiesForDiscountApplication,
                        discountedPrices,
                        discountCodeUsed,
                        applyStandalone: true);
                }

                return app;
            }

            private void PrepareDiscountApplicationForLineGroupAndItemAndReduceQuantity(
                string lineGroup,
                int itemGroupIndex,
                decimal quantityUsed,
                List<RetailDiscountLineItem> discountLineItems,
                Dictionary<int, decimal> quantitiesForDiscountApplication,
                decimal[] remainingQuantitiesLocal,
                HashSet<int> overlappedItemGroupIndexSet,
                Dictionary<string, Dictionary<int, decimal>> lineGroupToItemGroupIndexToAllocatedQuantityLookup)
            {
                this.PrepareDiscountApplicationForLineGroupAndItem(
                    lineGroup,
                    itemGroupIndex,
                    quantityUsed,
                    discountLineItems,
                    quantitiesForDiscountApplication);

                ReduceQuantitiesFromRemainingQuantities(
                    lineGroup,
                    itemGroupIndex,
                    quantityUsed,
                    remainingQuantitiesLocal,
                    overlappedItemGroupIndexSet,
                    lineGroupToItemGroupIndexToAllocatedQuantityLookup);
            }

            private void PrepareDiscountApplicationForLineGroupAndItem(
                string lineGroup,
                int itemGroupIndex,
                decimal quantityUsed,
                List<RetailDiscountLineItem> discountLineItems,
                Dictionary<int, decimal> quantitiesForDiscountApplication)
            {
                DiscountBase.AddToItemQuantities(quantitiesForDiscountApplication, itemGroupIndex, quantityUsed);

                RetailDiscountLine discountLine = this.GetDiscountLineByLineGroupAndItemGroupIndex(lineGroup, itemGroupIndex);
                for (decimal j = decimal.Zero; j < quantityUsed; j++)
                {
                    discountLineItems.Add(new RetailDiscountLineItem(itemGroupIndex, discountLine));
                }
            }

            private bool TryGetLeastExpensiveLookup(
                Dictionary<string, int[]> lineGroupToSortedItemGroupIndexArrayLookup,
                decimal[] remainingQuantities,
                HashSet<int> overlappedItemGroupIndexSet,
                Dictionary<string, Dictionary<int, decimal>> lineGroupToItemGroupIndexToAllocatedQuantityLookup,
                decimal[] discountedPrices,
                Dictionary<string, Dictionary<int, decimal>> leastExpensiveLookup)
            {
                bool keepGoing = true;
                decimal leastExpensiveQuantityNeeded = this.NumberOfLeastExpensiveLines;

                while (leastExpensiveQuantityNeeded > decimal.Zero && keepGoing)
                {
                    decimal lowestPrice = decimal.Zero;
                    string lineGroupForLowestPrice = string.Empty;
                    int itemGroupIndexForLowestPrice = 0;
                    foreach (KeyValuePair<string, int[]> pair in lineGroupToSortedItemGroupIndexArrayLookup)
                    {
                        string lineGroup = pair.Key;
                        int[] sortedItemGroupIndexArray = pair.Value;

                        for (int i = sortedItemGroupIndexArray.Length - 1; i >= 0; i--)
                        {
                            int itemGroupIndex = sortedItemGroupIndexArray[i];
                            decimal availableQuantity = GetAvailableQuantityForItem(
                                lineGroup,
                                itemGroupIndex,
                                remainingQuantities,
                                overlappedItemGroupIndexSet,
                                lineGroupToItemGroupIndexToAllocatedQuantityLookup);
                            if (availableQuantity > decimal.Zero)
                            {
                                decimal myPrice = discountedPrices[itemGroupIndex];
                                if (string.IsNullOrWhiteSpace(lineGroupForLowestPrice) || myPrice < lowestPrice)
                                {
                                    lineGroupForLowestPrice = lineGroup;
                                    lowestPrice = myPrice;
                                    itemGroupIndexForLowestPrice = itemGroupIndex;
                                }

                                // We need lowest priced available item only.
                                break;
                            }
                        }
                    }

                    if (string.IsNullOrWhiteSpace(lineGroupForLowestPrice))
                    {
                        keepGoing = false;
                    }
                    else
                    {
                        Dictionary<int, decimal> itemQuantityLookup = null;
                        if (leastExpensiveLookup.TryGetValue(lineGroupForLowestPrice, out itemQuantityLookup))
                        {
                            decimal quantity = decimal.Zero;
                            itemQuantityLookup.TryGetValue(itemGroupIndexForLowestPrice, out quantity);
                            itemQuantityLookup[itemGroupIndexForLowestPrice] = quantity + 1m;
                        }
                        else
                        {
                            itemQuantityLookup = new Dictionary<int, decimal>();
                            itemQuantityLookup.Add(itemGroupIndexForLowestPrice, 1m);
                            leastExpensiveLookup.Add(lineGroupForLowestPrice, itemQuantityLookup);
                        }

                        ReduceQuantitiesFromRemainingQuantities(
                            lineGroupForLowestPrice,
                            itemGroupIndexForLowestPrice,
                            1m,
                            remainingQuantities,
                            overlappedItemGroupIndexSet,
                            lineGroupToItemGroupIndexToAllocatedQuantityLookup);

                        leastExpensiveQuantityNeeded--;
                    }
                }

                return keepGoing;
            }

            private void FigureOutCompoundedPriceFlags(
                out bool includeAmountOff,
                out bool includePercentageOff)
            {
                includeAmountOff = true;
                includePercentageOff = true;
                if (this.DiscountType == DiscountMethodType.DealPrice ||
                    (this.DiscountType == DiscountMethodType.LeastExpensive && this.DiscountPercentValue == decimal.Zero && this.DiscountAmountValue == decimal.Zero && this.DealPriceValue > decimal.Zero))
                {
                    includeAmountOff = false;
                    includePercentageOff = false;
                }
                else if (this.DiscountType == DiscountMethodType.DiscountAmount ||
                    (this.DiscountType == DiscountMethodType.LeastExpensive && this.DiscountPercentValue == decimal.Zero && this.DiscountAmountValue > decimal.Zero))
                {
                    includePercentageOff = false;
                }
            }

            internal class OverlappedItemGroupIndexComparer
            {
                private string[] lineGroupsSorted;
                private IDictionary<int, HashSet<string>> itemGroupIndexToLineGroupSetLookup;

                internal OverlappedItemGroupIndexComparer(
                    string[] lineGroupsSorted,
                    IDictionary<int, HashSet<string>> itemGroupIndexToLineGroupSetLookup)
                {
                    this.lineGroupsSorted = lineGroupsSorted;
                    this.itemGroupIndexToLineGroupSetLookup = itemGroupIndexToLineGroupSetLookup;
                }

                internal int CompareOverlappedItemByMostHelpfulFirst(int left, int right)
                {
                    int result = 0;

                    // The earlier the item shows up in the sorted line group array, the latter.
                    // In short, process items that show up late in the sorted line group array first.
                    for (int i = 0; i < this.lineGroupsSorted.Length; i++)
                    {
                        string lineGroup = this.lineGroupsSorted[i];
                        bool lineGroupHasLeft = this.itemGroupIndexToLineGroupSetLookup[left].Contains(lineGroup);
                        bool lineGroupHasRight = this.itemGroupIndexToLineGroupSetLookup[right].Contains(lineGroup);

                        if (lineGroupHasLeft && !lineGroupHasRight)
                        {
                            result = 1;
                            break;
                        }
                        else if (!lineGroupHasLeft && lineGroupHasRight)
                        {
                            result = -1;
                            break;
                        }
                    }

                    return result;
                }
            }

            /// <summary>
            /// Line group compare by who needs quantity more.
            /// </summary>
            internal class LineGroupByQuantityNeedComparer
            {
                private bool checkOverlapped = false;
                private IDictionary<string, decimal> lineGroupNonOverlappedQuantityLookup;
                private IDictionary<string, decimal> lineGroupToQuantityOverlappedLookup;
                private IDictionary<string, int> lineGroupToNumberOfItemsLookup;

                internal LineGroupByQuantityNeedComparer(
                    IDictionary<string, decimal> lineGroupNonOverlappedQuantityLookup,
                    IDictionary<string, decimal> lineGroupToQuantityOverlappedLookup,
                    IDictionary<string, int> lineGroupToNumberOfItemsLookup)
                {
                    this.lineGroupNonOverlappedQuantityLookup = lineGroupNonOverlappedQuantityLookup;
                    this.lineGroupToQuantityOverlappedLookup = lineGroupToQuantityOverlappedLookup;
                    this.lineGroupToNumberOfItemsLookup = lineGroupToNumberOfItemsLookup;
                    this.checkOverlapped = true;
                }

                internal LineGroupByQuantityNeedComparer(
                    IDictionary<string, decimal> lineGroupNonOverlappedQuantityLookup,
                    IDictionary<string, int> lineGroupToNumberOfItemsLookup)
                {
                    this.lineGroupNonOverlappedQuantityLookup = lineGroupNonOverlappedQuantityLookup;
                    this.lineGroupToNumberOfItemsLookup = lineGroupToNumberOfItemsLookup;
                    this.checkOverlapped = false;
                }

                internal int CompareLineGroupByQuantityNeeded(string left, string right)
                {
                    int ret = 0;

                    decimal leftQuantityNeededForNext = decimal.Zero;
                    decimal leftNumberOfAppliations = this.GetNumberOfApplicationsQualifiedByLineGroup(left, out leftQuantityNeededForNext);
                    decimal rightQuantityNeededForNext = decimal.Zero;
                    decimal rightNumberOfAppliations = this.GetNumberOfApplicationsQualifiedByLineGroup(right, out rightQuantityNeededForNext);

                    // It results in ascending order by number of applications quantified.
                    if (leftNumberOfAppliations != rightNumberOfAppliations)
                    {
                        // Round 1: who has less # of discount applications
                        ret = Math.Sign(leftNumberOfAppliations - rightNumberOfAppliations);
                    }
                    else if (rightQuantityNeededForNext != leftQuantityNeededForNext)
                    {
                        // Round 2: who needs more quantity for next discount application
                        ret = Math.Sign(rightQuantityNeededForNext - leftQuantityNeededForNext);
                    }
                    else if (this.checkOverlapped)
                    {
                        // Round 3: who has less overlapped quantity.
                        ret = Math.Sign(this.lineGroupToQuantityOverlappedLookup[left] - this.lineGroupToQuantityOverlappedLookup[right]);
                    }

                    return ret;
                }

                private decimal GetNumberOfApplicationsQualifiedByLineGroup(string lineGroup, out decimal quantityNeededForNext)
                {
                    decimal nonOverlappedQuantity = this.lineGroupNonOverlappedQuantityLookup[lineGroup];
                    int numberOfItemsNeeded = this.lineGroupToNumberOfItemsLookup[lineGroup];

                    decimal numberOfApplicationsQualified = Math.Floor(numberOfItemsNeeded != 0 ? nonOverlappedQuantity / numberOfItemsNeeded : decimal.Zero);
                    decimal quantityLeft = nonOverlappedQuantity - (numberOfItemsNeeded * numberOfApplicationsQualified);

                    quantityNeededForNext = Math.Max(decimal.Zero, numberOfItemsNeeded - quantityLeft);

                    return numberOfApplicationsQualified;
                }
            }
        }
    }
}
