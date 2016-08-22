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
        using System.Diagnostics;
        using System.Linq;
        using System.Text;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// This class contains methods used to perform the discount calculations on a transaction, 
        /// determining the correct discounts to apply to the transaction.
        /// </summary>
        public class DiscountCalculator
        {
#if DEBUG
            internal const string AppliedMarginalValueAlgorithmKey = "DiscountCalculator.AppliedMarginalValueAlgorithm";
            internal const string FinishedExhaustiveAlgorithmKey = "DiscountCalculator.FinishedExhaustiveAlgorithm";
            internal const string StepCountKey = "DiscountCalculator.StepCount";
            internal const string ForThresholdSubKey = "4Threshold";
            internal const string MaxStepCountKey = "DiscountCalculator.MaxStepCount";
            internal const string HasTooManyDiscountApplicationsKey = "DiscountCalculator.HasTooManyDiscountApplications";
            internal const string HasOverlappingDiscountKey = "DiscountCalculator.HasOverlappingDiscountKey";
            internal const string AllOptimizedForBestDealKey = "DiscountCalculator.AllOptimizedForBestDeal";
            internal const string RemovedUnqualifiedDiscountsKey = "DiscountCalculator.RemovedUnqualifiedDiscounts";
            internal const string IsOptimizedMixMatchOneLineGroupKey = "DiscountCalculator.IsOptimizedMixMatchOneLineGroup";
            internal const string IsPartiallyOptimizedMixMatchOneLineGroupKey = "DiscountCalculator.IsPartiallyOptimizedMixMatchOneLineGroup";
            internal const string QuantitiesAfterPartiallyOptimizedMixMatchOneLineGroupKey = "DiscountCalculator.QuantitiesAfterPartiallyOptimizedMixMatchOneLineGroup";
            internal const string RelativePriceDictionaryKey = "DiscountCalculator.RelativePriceDictionary";
#endif

            private const int InvalidDiscountApplicationIndex = -1;
            private const string KeyDelimiter = "|";

#if DEBUG
            [ThreadStatic]
            private static bool logDiscountDetails;

            [ThreadStatic]
            private static bool logAlgorithmInfoOnTransaction;

            private long stepCount = 0;
            private long pathCount = 0;
#endif

            private Dictionary<long, List<DiscountBase>> itemDiscountsLookup = new Dictionary<long, List<DiscountBase>>();
            private Dictionary<string, DiscountBase> offerIdToDiscountsLookup = new Dictionary<string, DiscountBase>(StringComparer.OrdinalIgnoreCase);
            private PriceContext priceContext;
            private IEnumerable<long> allPriceGroups;
            private IPricingDataAccessor pricingDataManager;
            private bool enableCache = false;

            /// <summary>
            /// Initializes a new instance of the DiscountCalculator class for the specified transaction.
            /// </summary>
            /// <param name="transaction">The transaction to calculate the discounts for.</param>
            /// <param name="priceContext">The pricing context.</param>
            /// <param name="pricingDataManager">The pricing data manager.</param>
            public DiscountCalculator(SalesTransaction transaction, PriceContext priceContext, IPricingDataAccessor pricingDataManager)
            {
                this.priceContext = priceContext;
                this.allPriceGroups = PriceContextHelper.GetAllPriceGroupIdsForDiscount(priceContext);
                this.pricingDataManager = pricingDataManager;
                this.InitializeDiscounts(transaction);
#if DEBUG
                Debug.WriteLine("Max step count [{0}]", priceContext.MaxBestDealAlgorithmStepCount);
#endif
            }

#if DEBUG
            private enum DebugLogHeader : int
            {
                Step = 0,
                Path = 1,
            }
#endif

#if DEBUG
            internal static bool LogDiscountDetails
            {
                get { return logDiscountDetails; }
                set { logDiscountDetails = value; }
            }

            internal static bool LogAlgorithmInfoOnTransaction
            {
                get { return logAlgorithmInfoOnTransaction; }
                set { logAlgorithmInfoOnTransaction = value; }
            }
#endif

            /// <summary>
            /// Determine the best set of discounts to apply to the specified transaction, and apply those discounts.
            /// </summary>
            /// <param name="transaction">The transaction to perform discounting on.</param>
            public void CalculateDiscounts(SalesTransaction transaction)
            {
                if (transaction == null)
                {
                    throw new ArgumentException("A value for transaction must be specified", "transaction");
                }

                DiscountableItemGroup[] discountableItemGroups = DiscountCalculator.GetDiscountableItems(transaction, this.priceContext);

                DiscountableItemGroup[] positiveDiscountableItemGroups = discountableItemGroups.Where(p => p.Quantity > 0).ToArray();
                DiscountableItemGroup[] negativeDiscountableItemGroups = discountableItemGroups.Where(p => p.Quantity < 0).ToArray();

                if (positiveDiscountableItemGroups.Length > 0)
                {
                    this.CalculateAndApplyBestDiscounts(transaction, positiveDiscountableItemGroups, false);
                }

                if (negativeDiscountableItemGroups.Length > 0)
                {
                    foreach (DiscountableItemGroup group in negativeDiscountableItemGroups)
                    {
                        group.NegateQuantity();
                    }

                    this.CalculateAndApplyBestDiscounts(transaction, negativeDiscountableItemGroups, true);
                }
            }

            /// <summary>
            /// Gets the groups of discountable items present on the transaction, grouped by product and variant identifier.
            /// </summary>
            /// <param name="transaction">The current transaction.</param>
            /// <param name="priceContext">Price context.</param>
            /// <returns>The discountable item groups.</returns>
            /// <remarks>It's really private, internal for test.</remarks>
            internal static DiscountableItemGroup[] GetDiscountableItems(SalesTransaction transaction, PriceContext priceContext)
            {
                Dictionary<string, DiscountableItemGroup> items = new Dictionary<string, DiscountableItemGroup>(StringComparer.OrdinalIgnoreCase);
                var itemIds = PriceContextHelper.GetItemIds(transaction);

                if (itemIds.Count != priceContext.ItemCache.Count)
                {
                    var missingItemIds = itemIds.Where(id => !priceContext.ItemCache.ContainsKey(id));
                    ThrowErrorForInvalidItems(missingItemIds);
                }

                foreach (SalesLine line in transaction.PriceCalculableSalesLines)
                {
                    if (!line.IsPriceOverridden)
                    {
                        string key = string.Concat(line.Price, KeyDelimiter, line.ItemId, KeyDelimiter, line.ProductId, KeyDelimiter, line.SalesOrderUnitOfMeasure);

                        if (items.ContainsKey(key))
                        {
                            items[key].Add(line);
                        }
                        else
                        {
                            items.Add(key, new DiscountableItemGroup(line, priceContext) { ExtendedProperties = PriceContextHelper.GetItem(priceContext, line.ItemId) });
                        }
                    }
                }

                return items.Values.ToArray();
            }

            /// <summary>
            /// Build 3 sets of items with overlapping discounts.
            /// </summary>
            /// <param name="itemsWithOverlappingDiscounts">Hast set of overlapped item group indices.</param>
            /// <param name="itemsWithOverlappingDiscountsCompoundedOnly">Hast set of overlapped item group indices, compounded only.</param>
            /// <param name="itemGroupIndexToDiscountsLookup">Item group index to discounts lookup.</param>
            /// <param name="discounts">Discount collection.</param>
            /// <remarks>
            /// Prerequisite: SetupOptimization.
            /// </remarks>
            internal static void BuildItemsWithOverlappingDiscounts(
                HashSet<int> itemsWithOverlappingDiscounts,
                HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly,
                Dictionary<int, List<DiscountBase>> itemGroupIndexToDiscountsLookup,
                IEnumerable<DiscountBase> discounts)
            {
                using (SimpleProfiler profiler = new SimpleProfiler("DiscountCalculator.BuildItemsWithOverlappingDiscounts", 1))
                {
                    itemsWithOverlappingDiscounts.Clear();
                    itemsWithOverlappingDiscountsCompoundedOnly.Clear();
                    itemGroupIndexToDiscountsLookup.Clear();

                    // Build itemGroupIndexToDiscountsLookup.
                    foreach (DiscountBase discount in discounts)
                    {
                        if (!discount.IsFinished)
                        {
                            foreach (KeyValuePair<int, HashSet<decimal>> itemPair in discount.ItemGroupIndexToDiscountLineNumberSetMap)
                            {
                                int itemGroupIndex = itemPair.Key;
                                List<DiscountBase> discountsForItemGroupIndex = null;
                                if (itemGroupIndexToDiscountsLookup.TryGetValue(itemGroupIndex, out discountsForItemGroupIndex))
                                {
                                    discountsForItemGroupIndex.Add(discount);
                                }
                                else
                                {
                                    itemGroupIndexToDiscountsLookup.Add(itemGroupIndex, new List<DiscountBase>() { discount });
                                }
                            }
                        }
                    }

                    // Build itemsWithOverlappingDiscounts and itemsWithOverlappingDiscountsCompoundedOnly.
                    foreach (KeyValuePair<int, List<DiscountBase>> pair in itemGroupIndexToDiscountsLookup)
                    {
                        int itemGroupIndex = pair.Key;
                        List<DiscountBase> discountsForItemGroupIndex = pair.Value;

                        if (discountsForItemGroupIndex.Count > 1)
                        {
                            itemsWithOverlappingDiscounts.Add(itemGroupIndex);

                            bool isCompoundedOnly = true;
                            foreach (DiscountBase discount in discountsForItemGroupIndex)
                            {
                                if (discount.ConcurrencyMode != ConcurrencyMode.Compounded)
                                {
                                    isCompoundedOnly = false;
                                    break;
                                }
                            }

                            if (isCompoundedOnly)
                            {
                                itemsWithOverlappingDiscountsCompoundedOnly.Add(itemGroupIndex);
                            }
                        }
                    }
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals", Justification = "Grandfathered.")]
            internal static bool ReduceOverlappedOfferAndQuantityDiscountsPerItem(
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                Dictionary<int, List<DiscountBase>> itemGroupIndexToDiscountsLookup,
                HashSet<int> itemsWithOverlappingDiscounts)
            {
                // Reducing overlapped discounts (per item) simplifies best-deal algorithm dramatically. 
                // In many cases, we can figure out best-deal (per item) without going through best-deal algorithm.
                //
                // Per method name, it works on discount offer and quantity discount per item if possible.
                // For discount offer, it can always be a candidate to compare for any item.
                // For quantity discount, it may or may not be a candidate to compare. See MultibuyDiscount.GetSingleItemNonOverlappedDiscountResult
                // 
                // Cases (See examples in DiscountCalculatorUnitTests)
                // 1. Multiple discount offers and quantity discounts, with at most one Compounded, and no addition compounded discounts on the item.
                //    + Just pick the best one. If a quantity discount can be a candidate for the item, we'd leave it alone.
                // 2. Multiple compounded discount offers and quantity discounts as candidates, along with non-compounded (best price) ones.
                //    + Non-compounded discounts offer and quantity discounts can compare and reduce as usual.
                //    + Compound multiple compounded candidate into one (the compounded candidate), and compare and reduce it against non-compounded (best price) ones.
                // 3. Has compounded discounts that may take partial quantity from the item, e.g. mix and match, and some quantity discount.
                // 3.1. Discount offers and quantity discounts are non-Compounded, this is actually case 1.
                // 3.2. All discount offers and quantity discounts to compare but one is compounded.
                //      + If compounded is worse, you can't reduce it.
                //      + Compounded if better can reduce non-compouned (best price) discounts for the item.
                //      + Non-compounded discounts offer and quantity discounts can compare and reduce as usual.
                // 3.2. Multiple discount offers and quantity discounts as candidates, along with non-compounded (best price) ones.
                //      + If compounded candidate (See case 2) is worse, you can't reduce it.
                //      + Compounded candidate if better can reduce non-compouned (best price) discounts for the item.
                //      + Non-compounded discounts offer and quantity discounts can compare and reduce as usual.
                using (SimpleProfiler profiler = new SimpleProfiler("DiscountCalculator.ReduceOverlappedOfferAndQuantityDiscounts", 1))
                {
                    bool isReduced = false;
                    foreach (int itemGroupIndex in itemsWithOverlappingDiscounts)
                    {
                        List<DiscountBase> discounts = null;
                        if (itemGroupIndexToDiscountsLookup.TryGetValue(itemGroupIndex, out discounts))
                        {
                            int numberOfNonCompoundedDiscountsExcludingDiscountsLikelyEvaluatedWithOtherItems = 0;

                            // Mix and match may take away partial quantity, like 1 out of 3, which makes comparison of quantity discount tricky.
                            // Same can be true for some quantity discounts where a discount line covers mutliple items.
                            bool hasDiscountsThatMayTakePartialQuantity = false;
                            bool hasCompoundedDiscountsLikelyEvaluatedWithOtherItems = false;

                            // We will later make a compounded candidate from the following variable and compare it against other non-compounded (best price) candidates.
                            List<DiscountBase> discountsCompoundedExcludingDiscountsLikelyEvaluatedWithOtherItems = new List<DiscountBase>();

                            foreach (DiscountBase discount in discounts)
                            {
                                bool isLikelyEvaluatedWithOtherItems = discount.IsItemLikelyEvaluatedWithOtherItems(itemGroupIndex);
                                if (discount.CanCompound)
                                {
                                    if (isLikelyEvaluatedWithOtherItems)
                                    {
                                        hasCompoundedDiscountsLikelyEvaluatedWithOtherItems = true;
                                    }
                                    else
                                    {
                                        // Standalone simple discount for the item.
                                        discountsCompoundedExcludingDiscountsLikelyEvaluatedWithOtherItems.Add(discount);
                                    }
                                }
                                else
                                {
                                    // Exclusive or best price
                                    if (!isLikelyEvaluatedWithOtherItems)
                                    {
                                        numberOfNonCompoundedDiscountsExcludingDiscountsLikelyEvaluatedWithOtherItems++;
                                    }
                                }

                                if (isLikelyEvaluatedWithOtherItems)
                                {
                                    hasDiscountsThatMayTakePartialQuantity = true;
                                }
                            }

                            decimal price = discountableItemGroups[itemGroupIndex].Price;
                            decimal quantity = remainingQuantities[itemGroupIndex];

                            int numberOfCompoundedDiscountsExcludingDiscountsLikelyEvaluatedWithOtherItems = discountsCompoundedExcludingDiscountsLikelyEvaluatedWithOtherItems.Count;

                            // This tells whether to compounded multiple compounded discounts into one, and compare the compounded one with other best price discounts.
                            bool countAllCompoundedExcludingDiscountsLikelyEvaluatedWithOtherItemsAsOne = false;
                            List<SingleItemNonOverlappedDiscountResult> compoundedResultsExcludingDiscountsLikelyEvaluatedWithOtherItemsSorted = new List<SingleItemNonOverlappedDiscountResult>();
                            decimal unitDiscountAmountCompoundedExcludingDiscountsLikelyEvaluatedWithOtherItems = decimal.Zero;
                            bool isCompoundedDiscountAsOneContingentOnOtherDiscounts = true;

                            if (numberOfCompoundedDiscountsExcludingDiscountsLikelyEvaluatedWithOtherItems >= 2)
                            {
                                countAllCompoundedExcludingDiscountsLikelyEvaluatedWithOtherItemsAsOne = true;
                                foreach (DiscountBase discount in discountsCompoundedExcludingDiscountsLikelyEvaluatedWithOtherItems)
                                {
                                    SingleItemNonOverlappedDiscountResult result = discount.GetSingleItemNonOverlappedDiscountResult(itemGroupIndex, price, quantity);
                                    if (result.IsApplicable)
                                    {
                                        compoundedResultsExcludingDiscountsLikelyEvaluatedWithOtherItemsSorted.Add(result);
                                    }
                                    else
                                    {
                                        countAllCompoundedExcludingDiscountsLikelyEvaluatedWithOtherItemsAsOne = false;
                                        break;
                                    }
                                }

                                if (countAllCompoundedExcludingDiscountsLikelyEvaluatedWithOtherItemsAsOne)
                                {
                                    compoundedResultsExcludingDiscountsLikelyEvaluatedWithOtherItemsSorted.Sort(SingleItemNonOverlappedDiscountResult.GetComparison());

                                    bool hasDiscountContigentOnOtherDiscounts = false;
                                    for (int i = 0; i < compoundedResultsExcludingDiscountsLikelyEvaluatedWithOtherItemsSorted.Count; i++)
                                    {
                                        SingleItemNonOverlappedDiscountResult result = compoundedResultsExcludingDiscountsLikelyEvaluatedWithOtherItemsSorted[i];
                                        RetailDiscountLine discountLineDefinition = new RetailDiscountLine();
                                        discountLineDefinition.DiscountMethod = (int)result.DiscountMethod;
                                        discountLineDefinition.OfferPrice = result.OfferPrice;
                                        discountLineDefinition.DiscountAmount = result.DiscountAmount;
                                        discountLineDefinition.DiscountPercent = result.DiscountPercentage;
                                        unitDiscountAmountCompoundedExcludingDiscountsLikelyEvaluatedWithOtherItems += OfferDiscount.GetUnitDiscountAmount(discountLineDefinition, price - unitDiscountAmountCompoundedExcludingDiscountsLikelyEvaluatedWithOtherItems);

                                        if (!result.IsIndependentOfOverlappedDiscounts)
                                        {
                                            hasDiscountContigentOnOtherDiscounts = true;
                                        }
                                    }

                                    if (!hasDiscountContigentOnOtherDiscounts)
                                    {
                                        isCompoundedDiscountAsOneContingentOnOtherDiscounts = false;
                                    }
                                }
                            }

                            bool compareAgainstCompoundedDiscounts = countAllCompoundedExcludingDiscountsLikelyEvaluatedWithOtherItemsAsOne;
                            bool removeCompoundedDiscounts = false;

                            int totalNumberOfDiscountsExcludingMixAndMatch = numberOfCompoundedDiscountsExcludingDiscountsLikelyEvaluatedWithOtherItems + numberOfNonCompoundedDiscountsExcludingDiscountsLikelyEvaluatedWithOtherItems;

                            // We have at least one non-compounded discount.
                            if (totalNumberOfDiscountsExcludingMixAndMatch >= 2 && numberOfNonCompoundedDiscountsExcludingDiscountsLikelyEvaluatedWithOtherItems >= 1)
                            {
                                DiscountBase[] discountsToCompare = null;
                                if (compareAgainstCompoundedDiscounts)
                                {
                                    discountsToCompare = discounts.Where(p => !p.CanCompound && p.PeriodicDiscountType != PeriodicDiscountOfferType.MixAndMatch).ToArray();
                                }
                                else
                                {
                                    // DISCOUNTPERF: what if we have multiple compounded discount? for example, one of them is not applicable.
                                    discountsToCompare = discounts.Where(p => p.PeriodicDiscountType != PeriodicDiscountOfferType.MixAndMatch).ToArray();
                                }

                                HashSet<int> discountIndexToRemoveSet = new HashSet<int>();
                                for (int left = 0; left < discountsToCompare.Length; left++)
                                {
                                    if (discountIndexToRemoveSet.Contains(left))
                                    {
                                        continue;
                                    }

                                    SingleItemNonOverlappedDiscountResult leftResult = discountsToCompare[left].GetSingleItemNonOverlappedDiscountResult(itemGroupIndex, price, quantity);

                                    // (Left) non-compounded (best price) discount to compare against compounded discounts.
                                    if (compareAgainstCompoundedDiscounts)
                                    {
                                        if (leftResult.UnitDiscountAmount < unitDiscountAmountCompoundedExcludingDiscountsLikelyEvaluatedWithOtherItems)
                                        {
                                            // Compounded discounts are better, remove left 
                                            // if no mix and match or other discount can take partial quantity away, or
                                            //    any discount for the item in the compounded candidate is independent.
                                            if (!(hasDiscountsThatMayTakePartialQuantity && isCompoundedDiscountAsOneContingentOnOtherDiscounts))
                                            {
                                                discountIndexToRemoveSet.Add(left);
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            // Compounded discounts are worse, remove them 
                                            // if no additional compounded discount, and 
                                            //   (no mix and match or other discount can take partial quantity away or
                                            //    left result is independent)
                                            if (!hasCompoundedDiscountsLikelyEvaluatedWithOtherItems &&
                                                (leftResult.IsIndependentOfOverlappedDiscounts || !hasDiscountsThatMayTakePartialQuantity))
                                            {
                                                removeCompoundedDiscounts = true;
                                                compareAgainstCompoundedDiscounts = false;
                                            }
                                        }
                                    }

                                    // 1 on 1 discount comparison.
                                    for (int right = left + 1; right < discountsToCompare.Length; right++)
                                    {
                                        if (discountIndexToRemoveSet.Contains(right))
                                        {
                                            // right continue;
                                            continue;
                                        }

                                        SingleItemNonOverlappedDiscountResult rightResult = discountsToCompare[right].GetSingleItemNonOverlappedDiscountResult(itemGroupIndex, price, quantity);

                                        if (leftResult.IsApplicable && rightResult.IsApplicable)
                                        {
                                            bool isRemoved = false;
                                            if (leftResult.UnitDiscountAmount >= rightResult.UnitDiscountAmount)
                                            {
                                                // Don't remove compounded if we have also mix and match compounded
                                                if (!(hasCompoundedDiscountsLikelyEvaluatedWithOtherItems && rightResult.CanCompound) &&
                                                    (leftResult.IsIndependentOfOverlappedDiscounts || !hasDiscountsThatMayTakePartialQuantity))
                                                {
                                                    discountIndexToRemoveSet.Add(right);
                                                    isRemoved = true;
                                                }
                                            }

                                            if (!isRemoved && leftResult.UnitDiscountAmount <= rightResult.UnitDiscountAmount)
                                            {
                                                // Don't remove compounded if we have also mix and match compounded
                                                if (!(hasCompoundedDiscountsLikelyEvaluatedWithOtherItems && leftResult.CanCompound) &&
                                                    (rightResult.IsIndependentOfOverlappedDiscounts || !hasDiscountsThatMayTakePartialQuantity))
                                                {
                                                    discountIndexToRemoveSet.Add(left);
                                                }
                                            }
                                        }
                                    }
                                }

                                foreach (int discountIndexToRemove in discountIndexToRemoveSet)
                                {
                                    DiscountBase discountWithItemToRemove = discountsToCompare[discountIndexToRemove];
                                    discountWithItemToRemove.RemoveItemIndexGroupFromLookups(itemGroupIndex);
                                    discountWithItemToRemove.CleanupLookups();
                                    isReduced = true;
                                }

                                if (removeCompoundedDiscounts)
                                {
                                    foreach (DiscountBase discountWithItemToRemove in discountsCompoundedExcludingDiscountsLikelyEvaluatedWithOtherItems)
                                    {
                                        discountWithItemToRemove.RemoveItemIndexGroupFromLookups(itemGroupIndex);
                                        discountWithItemToRemove.CleanupLookups();
                                        isReduced = true;
                                    }
                                }
                            }
                        }
                    }

                    return isReduced;
                }
            }

            internal static bool RemoveFinishedDiscounts(List<DiscountBase> discounts)
            {
                int originalCount = discounts.Count;
                discounts.RemoveAll(p => p.IsFinished);

                return originalCount > discounts.Count;
            }

#if DEBUG
            internal static void SetDebugDataOnTransaction(SalesTransaction transaction, string key, object value)
            {
                if (LogAlgorithmInfoOnTransaction)
                {
                    transaction.SetProperty(GetInternalDebugKey(key), value);
                }
            }

            internal static object GetDebugDataOnTransaction(SalesTransaction transaction, string key)
            {
                if (LogAlgorithmInfoOnTransaction)
                {
                    return transaction.GetProperty(GetInternalDebugKey(key));
                }

                return null;
            }

            internal static string GetQuantityCountKeyForNonOverlappedBestDeal(string offerId)
            {
                return "NonOverlappedBestDealQuantityCount:" + offerId;
            }

            internal static string GetQuantityCountKeyForNonOverlappedOptimized(string offerId)
            {
                return "NonOverlappedOptimizedQuantityCount:" + offerId;
            }

            internal static string GetStepCountKeyForDiscount(string offerId)
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}:{1}", StepCountKey, offerId);
            }

            internal static string GetStepCountKeyForThreshold()
            {
                return StepCountKey + ForThresholdSubKey;
            }

            internal static string GetAllOptimizedForBestDealKeyForPriority(int priority, bool isExclusive)
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}:P{1}:{2}", AllOptimizedForBestDealKey, priority, isExclusive ? "EX" : "BC");
            }

            internal static string GetFinishedExhaustiveAlgorithmKeyForThreshold()
            {
                return FinishedExhaustiveAlgorithmKey + ForThresholdSubKey;
            }

            internal static string GetAppliedMarginalValueAlgorithmKeyForThreshold()
            {
                return AppliedMarginalValueAlgorithmKey + ForThresholdSubKey;
            }

            internal static string GetStepCountKeyForPriority(int priority, bool isExclusive)
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}:P{1}:{2}", StepCountKey, priority, isExclusive ? "EX" : "BC");
            }

            internal static string GetStepCountKeyForOverlappedGroup(List<string> offerIds, bool isExclusive)
            {
                return GetStepCountKeyForOverlappedGroup(offerIds, isExclusive, 0);
            }

            internal static string GetOfferIdsSubKey(List<string> offerIds)
            {
                offerIds.Sort(StringComparer.OrdinalIgnoreCase);
                StringBuilder offerIdString = new StringBuilder("OfferIds");
                foreach (string offerId in offerIds)
                {
                    offerIdString.Append(":" + offerId);
                }

                return offerIdString.ToString();
            }

            internal static string GetStepCountKeyForOverlappedGroup(List<string> offerIds, bool isExclusive, int priority)
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}:P{1}:{2}:{3}", StepCountKey, priority, isExclusive ? "EX" : "BC", GetOfferIdsSubKey(offerIds));
            }

            internal static string GetStepCountKeyForThreshold(int priority, bool isExclusive)
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}:P{1}:{2}", GetStepCountKeyForThreshold(), priority, isExclusive ? "EX" : "BC");
            }

            internal static string GetFinishedExhaustiveAlgorithmKeyForDiscount(string offerId)
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}:{1}", FinishedExhaustiveAlgorithmKey, offerId);
            }

            internal static string GetFinishedExhaustiveAlgorithmKeyForOverlappedGroup(List<string> offerIds, bool isExclusive, int priority)
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}:P{1}:{2}:{3}", FinishedExhaustiveAlgorithmKey, priority, isExclusive ? "EX" : "BC", GetOfferIdsSubKey(offerIds));
            }

            internal static string GetFinishedExhaustiveAlgorithmKeyForThreshold(int priority, bool isExclusive)
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}:P{1}:{2}", GetFinishedExhaustiveAlgorithmKeyForThreshold(), priority, isExclusive ? "EX" : "BC");
            }

            internal static string GetAppliedMarginalValueAlgorithmKeyForDiscount(string offerId)
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}:OFFER:{1}", AppliedMarginalValueAlgorithmKey, offerId);
            }

            internal static string GetAppliedMarginalValueAlgorithmKeyForOverlappedGroup(List<string> offerIds, bool isExclusive, int priority)
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}:P{1}:{2}:{3}", AppliedMarginalValueAlgorithmKey, priority, isExclusive ? "EX" : "BC", GetOfferIdsSubKey(offerIds));
            }

            internal static string GetAppliedMarginalValueAlgorithmKeyForThreshold(int priority, bool isExclusive)
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}:P{1}:{2}", GetAppliedMarginalValueAlgorithmKeyForThreshold(), priority, isExclusive ? "EX" : "BC");
            }

            internal static string GetIsOptimizedMixMatchOneLineGroupKeyForDiscount(string offerId)
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}:{1}", IsOptimizedMixMatchOneLineGroupKey, offerId);
            }

            internal static string GetIsPartiallyOptimizedMixMatchOneLineGroupKeyForDiscount(string offerId)
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}:{1}", IsPartiallyOptimizedMixMatchOneLineGroupKey, offerId);
            }

            internal static void DebugSetQuantityAppliedOnSalesTransaction(
                string naturalKey,
                SalesTransaction transaction,
                IEnumerable<AppliedDiscountApplication> appliedDiscounts,
                DiscountBase discount)
            {
                decimal quantityApplied = decimal.Zero;
                foreach (AppliedDiscountApplication appliedDiscountApplication in appliedDiscounts)
                {
                    if (appliedDiscountApplication.DiscountApplication.Discount == discount)
                    {
                        foreach (KeyValuePair<int, decimal> pair in appliedDiscountApplication.ItemQuantities)
                        {
                            quantityApplied += pair.Value;
                        }
                    }
                }

                SetDebugDataOnTransaction(transaction, naturalKey, quantityApplied);
            }

            internal static void DebugSetRemainingQuantitiesOnSalesTransaction(
                string naturalKey,
                SalesTransaction transaction,
                decimal[] remainingQuantities)
            {
                SetDebugDataOnTransaction(transaction, naturalKey, remainingQuantities);
            }

            internal static void DebugQuantitites(string header, decimal[] quantities)
            {
                System.Text.StringBuilder quantitiesInString = new System.Text.StringBuilder();
                if (!string.IsNullOrWhiteSpace(header))
                {
                    quantitiesInString.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "  {0}:", header);
                }

                for (int i = 0; i < quantities.Length; i++)
                {
                    int itemGroupIndex = i;
                    decimal quantity = quantities[i];

                    quantitiesInString.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, " {0}x{1}", quantity, itemGroupIndex);
                }

                System.Diagnostics.Debug.WriteLine(quantitiesInString.ToString());
            }
#endif

#if DEBUG
            private static void WriteDebugInfoForPossibleDiscountApplications(DiscountableItemGroup[] discountableItemGroups, DiscountApplication[] possibleDiscountApplications)
            {
                Debug.WriteLine("Total possible discount applications: {0}", possibleDiscountApplications.Length);
                for (int debugDaIndex = 0; debugDaIndex < possibleDiscountApplications.Length; debugDaIndex++)
                {
                    DiscountApplication debugDa = possibleDiscountApplications[debugDaIndex];
                    Debug.WriteLine("DA{0}: DiscountOfferId={1}. DiscountType={2}. DiscountConcurrency={3}", debugDaIndex, debugDa.Discount.OfferId, debugDa.Discount.PeriodicDiscountType, debugDa.Discount.ConcurrencyMode);
                    Debug.WriteLine("Quantities: Length={0}", debugDa.ItemQuantities.Count);
                    foreach (KeyValuePair<int, decimal> currentQuantity in debugDa.ItemQuantities)
                    {
                        DiscountableItemGroup itemGroup = discountableItemGroups[currentQuantity.Key];
                        Debug.WriteLine("    Item {0}({1}) quantity = {2}", itemGroup.ItemId, currentQuantity.Key, currentQuantity.Value);
                    }
                }
            }

            private static string GetInternalDebugKey(string naturalKey)
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "!DBG@{0}#DBG$", naturalKey);
            }

            private static void WriteDebugInfoForDiscountApplications(string header, DiscountApplication[] exclusiveOrBestDiscountApplications, DiscountableItemGroup[] discountableItemGroups)
            {
                if (LogDiscountDetails)
                {
                    Debug.WriteLine("[{0}] possible discount applications: {1}", header, exclusiveOrBestDiscountApplications.Count());

                    foreach (DiscountApplication discountApplication in exclusiveOrBestDiscountApplications)
                    {
                        Debug.WriteLine("  [{0}] deal [{1:0.##}] $ off [{2:0.##}] % off [{3:0.##}] sort [{4}] v [{5:0.##}]  ", discountApplication.Discount.OfferId, discountApplication.DealPriceValue, discountApplication.DiscountAmountValue, discountApplication.DiscountPercentValue, discountApplication.SortIndex, discountApplication.SortValue);

                        foreach (KeyValuePair<int, decimal> pair in discountApplication.ItemQuantities)
                        {
                            int itemGroupIndex = pair.Key;
                            decimal quantity = pair.Value;
                            Debug.WriteLine("    [{0}] for [{1}]-[{2}] q [{3}]", itemGroupIndex, discountableItemGroups[itemGroupIndex].ItemId, discountableItemGroups[itemGroupIndex].ProductId, quantity);
                        }
                    }
                }
            }
#endif

            /// <summary>
            /// Gets the remaining possible applications after using a compounding discount.
            /// </summary>
            /// <param name="appliedDiscountsStack">The currently applied discounts in this path.</param>
            /// <param name="possibleDiscountApplications">The possible discount applications.</param>
            /// <param name="nextIndex">The next index in the possible applications to consider.</param>
            /// <param name="remainingQuantitiesForCoumpound">The remaining quantities for compound.</param>
            /// <param name="remainingApplications">The remaining applications that are possible.</param>
            private static void GetRemainingApplicationsForCompoundedDiscount(
                Stack<AppliedDiscountApplication> appliedDiscountsStack,
                DiscountApplication[] possibleDiscountApplications,
                int nextIndex,
                decimal[] remainingQuantitiesForCoumpound,
                ref BitSet remainingApplications)
            {
                // For compounding discounts, it's a bit more complicated, since this particular discount can only apply to each unit once, 
                // and each unit should also remove any non-compounding discounts from the possibilities.
                // Due to the order that we do processing in, this particular discount must be at the top of the stack, so once we encounter a different application, we can stop.
                // We also should allow the current discount to be processed again as long as the remaining quantities are still sufficient to trigger it.  
                // If they are not, the current discount cannot be applied again, so it should be removed from the remaining applications.
                // Also, since compounding discounts will be at the end of the remaining applications, we know that no discounts other than the current one can 
                // be eliminated by applying this discount (since a compounding discount cannot exclude any other compounding discount from applying.
                // The match needs to be on offer ID to prevent a compound discount that has multiple tiers (quantity, etc.) from applying more than once.
                AppliedDiscountApplication[] currentApplications = appliedDiscountsStack.ToArray();

                for (int remainingIndex = nextIndex; remainingIndex < possibleDiscountApplications.Length; remainingIndex++)
                {
                    decimal[] remainingQuantitiesAfterDiscount = (decimal[])remainingQuantitiesForCoumpound.Clone();

                    for (int x = 0; x < currentApplications.Length; x++)
                    {
                        if (remainingApplications[remainingIndex] && !currentApplications[x].DiscountApplication.CanCompound(possibleDiscountApplications[remainingIndex]))
                        {
                            for (int y = 0; y < remainingQuantitiesAfterDiscount.Length; y++)
                            {
                                decimal quantity = decimal.Zero;
                                if (currentApplications[x].ItemQuantities.TryGetValue(y, out quantity))
                                {
                                    remainingQuantitiesAfterDiscount[y] -= quantity;
                                }

                                decimal quantityNeeded = decimal.Zero;
                                if (possibleDiscountApplications[remainingIndex].ItemQuantities.TryGetValue(y, out quantityNeeded) &&
                                    remainingQuantitiesAfterDiscount[y] < quantityNeeded)
                                {
                                    remainingApplications[remainingIndex] = false;
                                }
                            }
                        }
                    }
                }
            }

            private static void FixRemainingQuantitiesAfterApplyingDiscountApplication(
                AppliedDiscountApplication appliedDiscountApplication,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCoumpound,
                ItemCompoundedOfferQuantityLookup itemCompoundedOfferQuantityLookup)
            {
                // This works for optimized discount applications. When you get an applied discount application, you really apply it.
                // For true best-value algorithm, which tries all possible combination of discount applications, it doesn't use this.
                // The best-value algorithm is the last one in each round of exclusive non-threshold, best price or compounded non-threshold,
                // exclusive threshold, best price or compounded threshold. After each round,
                // we refresh itemCompoundedOfferQuantityLookup in SetAppliedDiscountApplicationStackAndReduceRemainingQuantities.
                foreach (KeyValuePair<int, decimal> pair in appliedDiscountApplication.ItemQuantities)
                {
                    int itemGroupIndex = pair.Key;
                    decimal quantity = pair.Value;

                    if (appliedDiscountApplication.DiscountApplication.Discount.CanCompound)
                    {
                        itemCompoundedOfferQuantityLookup.FillLookup(appliedDiscountApplication.DiscountApplication.Discount, itemGroupIndex, quantity);

                        Dictionary<string, decimal> compoundedOfferToQuantityLookup = null;
                        decimal highestQuantityForOneCompoundedOffer = decimal.Zero;
                        if (itemCompoundedOfferQuantityLookup.ItemGroupIndexToCompoundedOfferToQuantityLookup.TryGetValue(itemGroupIndex, out compoundedOfferToQuantityLookup))
                        {
                            highestQuantityForOneCompoundedOffer = compoundedOfferToQuantityLookup.Max(p => p.Value);
                        }

                        remainingQuantities[itemGroupIndex] = Math.Max(decimal.Zero, remainingQuantitiesForCoumpound[itemGroupIndex] - highestQuantityForOneCompoundedOffer);
                    }
                    else
                    {
                        remainingQuantities[itemGroupIndex] -= quantity;
                        remainingQuantitiesForCoumpound[itemGroupIndex] -= quantity;
                    }
                }
            }

            /// <summary>
            /// Gets the remaining possible applications after using a non-compounding discount.
            /// </summary>
            /// <param name="possibleDiscountApplications">The possible discount applications.</param>
            /// <param name="newAppliedDiscountApplication">The newly applied discount application.</param>
            /// <param name="nextIndex">The next index in the possible applications to consider.</param>
            /// <param name="remainingQuantities">The remaining quantities of items.</param>
            /// <param name="remainingQuantitiesForCoumpound">The remaining quantities for compound.</param>
            /// <param name="remainingApplications">The remaining applications that are possible.</param>
            /// <param name="availableItemDiscounts">The discounts that are available for each discountable item group.</param>
            private static void GetRemainingApplicationsForNonCompoundedDiscount(
                DiscountApplication[] possibleDiscountApplications,
                AppliedDiscountApplication newAppliedDiscountApplication,
                int nextIndex,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCoumpound,
                ref BitSet remainingApplications,
                BitSet[] availableItemDiscounts)
            {
                foreach (KeyValuePair<int, decimal> pair in newAppliedDiscountApplication.ItemQuantities)
                {
                    int itemGroupIndex = pair.Key;
                    decimal quantity = pair.Value;
                    remainingQuantities[itemGroupIndex] -= quantity;
                    remainingQuantitiesForCoumpound[itemGroupIndex] -= quantity;

                    // As a slight performance benefit, if this remaining quantity went to zero, remove any remaining discount applications that needed this item
                    if (remainingQuantities[itemGroupIndex] == 0M && possibleDiscountApplications[nextIndex].ItemQuantities.ContainsKey(itemGroupIndex))
                    {
                        remainingApplications = remainingApplications.And(availableItemDiscounts[itemGroupIndex].Not());
                    }
                }

                // Now remove any remaining discount applications that can no longer be applied
                for (int x = 0; x < remainingApplications.Length; x++)
                {
                    if (remainingApplications[x])
                    {
                        foreach (KeyValuePair<int, decimal> pair in possibleDiscountApplications[x].ItemQuantities)
                        {
                            int itemGroupIndex = pair.Key;
                            decimal quantity = pair.Value;

                            if (remainingQuantities[itemGroupIndex] < quantity)
                            {
                                // This discount can't be applied
                                remainingApplications[x] = false;
                            }
                        }
                    }
                }
            }

            private static void PrepareAvailableItemDiscountBitSes(BitSet[] availableItemDiscounts, DiscountApplication[] discountApplications)
            {
                // Populate the listing of discounts applications that require each specific item
                for (int itemGroupIndex = 0; itemGroupIndex < availableItemDiscounts.Length; itemGroupIndex++)
                {
                    availableItemDiscounts[itemGroupIndex] = new BitSet(discountApplications.Select(p => p.ItemQuantities.ContainsKey(itemGroupIndex)).ToArray());
                }
            }

            /// <summary>
            /// Throws error and logs the invalid item Ids on the transaction.
            /// </summary>
            /// <param name="invalidItemIds">Item Ids for items not in the database.</param>
            /// <exception cref="DataValidationException">Thrown if the invalid item Ids are non-empty.</exception>
            private static void ThrowErrorForInvalidItems(IEnumerable<string> invalidItemIds)
            {
                if (invalidItemIds.Any())
                {
                    var ex = new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound,
                        "Sales line item could not be found. Try running product jobs and ensure item is assorted to current channel. See tracing log for detailed errors.");

                    var formattedIds = invalidItemIds.Aggregate(
                        new StringBuilder("Items not found for item ids on transaction: "),
                        (sb, id) => sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "ItemId: {0}; ", id),
                        sb => sb.ToString());

                    RetailLogger.Log.CrtPricingEngineDiscountCalculatorInvalidItems(formattedIds, ex);

                    throw ex;
                }
            }

            private static void GetDiscountsByPriority(
                List<DiscountBase> possibleDiscounts,
                int pricingPriorityNumber,
                List<DiscountBase> possibleExclusiveDiscounts,
                List<MixAndMatchDiscount> possibleExclusiveSpecialMixAndMatches,
                List<DiscountBase> possibleNonExclusiveDiscounts,
                List<MixAndMatchDiscount> possibleNonExclusiveSpecialMixAndMatches)
            {
                // Fill in possibleExclusiveDiscounts and possibleNonExclusiveDiscounts, filtered by priority number.
                possibleExclusiveDiscounts.Clear();
                possibleNonExclusiveDiscounts.Clear();

                foreach (DiscountBase discount in possibleDiscounts)
                {
                    if (discount.PricingPriorityNumber == pricingPriorityNumber)
                    {
                        MixAndMatchDiscount mixAndMatch = discount as MixAndMatchDiscount;

                        if (mixAndMatch != null && mixAndMatch.DiscountType == DiscountMethodType.LeastExpensive && mixAndMatch.LeastExpensiveMode == LeastExpensiveMode.FavorRetailer)
                        {
                            if (mixAndMatch.ConcurrencyMode == ConcurrencyMode.Exclusive)
                            {
                                possibleExclusiveSpecialMixAndMatches.Add(mixAndMatch);
                            }
                            else
                            {
                                possibleNonExclusiveSpecialMixAndMatches.Add(mixAndMatch);
                            }
                        }
                        else
                        {
                            if (discount.ConcurrencyMode == ConcurrencyMode.Exclusive)
                            {
                                possibleExclusiveDiscounts.Add(discount);
                            }
                            else
                            {
                                possibleNonExclusiveDiscounts.Add(discount);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Gets the priority number list from discounts, sorted by priority descending.
            /// </summary>
            /// <param name="discounts">Discount list.</param>
            /// <returns>Priority number list, sorted by priority descending.</returns>
            private static List<int> GetSortedPriorityListFromDiscounts(List<DiscountBase> discounts)
            {
                HashSet<int> prioritySet = new HashSet<int>();

                foreach (DiscountBase discount in discounts)
                {
                    prioritySet.Add(discount.PricingPriorityNumber);
                }

                List<int> sortedPriorityList = new List<int>(prioritySet);
                if (sortedPriorityList.Any())
                {
                    sortedPriorityList.Sort();
                    sortedPriorityList.Reverse();
                }

                return sortedPriorityList;
            }

            private bool ApplyNonOverlappingDiscountsWithBestDeal(
                List<DiscountBase> possibleDiscounts,
                SalesTransaction transaction,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompound,
                ItemCompoundedOfferQuantityLookup itemCompoundedOfferQuantityLookup,
                HashSet<int> itemsWithOverlappingDiscounts,
                HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly,
                ref List<AppliedDiscountApplication> bestDiscountPath,
                ref decimal bestDiscountValue)
            {
                using (SimpleProfiler profiler = new SimpleProfiler("DiscountCalculator.ApplyNonOverlappingDiscountsWithBestDeal", 1))
                {
                    bool isCompoundedDiscountApplied = false;
                    foreach (DiscountBase discount in possibleDiscounts)
                    {
                        decimal[] remainingQuantitiesToApply = discount.CanCompound ? remainingQuantitiesForCompound : remainingQuantities;
                        IEnumerable<DiscountApplication> discountApplications = discount.GetDiscountApplicationsNonOverlappedWithBestDeal(
                            transaction,
                            discountableItemGroups,
                            remainingQuantitiesToApply,
                            this.priceContext,
                            bestDiscountPath,
                            itemsWithOverlappingDiscounts,
                            itemsWithOverlappingDiscountsCompoundedOnly);

#if DEBUG
                        Debug.WriteLine("GetDiscountApplicationsNonOverlappedWithBestDeal returned {0} discount applications for discount {1}.", discountApplications.Count(), discount.OfferName);
#endif

                        this.ApplyStandaloneDiscountApplications(
                            discountApplications,
                            discountableItemGroups,
                            remainingQuantities,
                            remainingQuantitiesForCompound,
                            itemCompoundedOfferQuantityLookup,
                            ref bestDiscountPath,
                            ref bestDiscountValue);

                        if (discount is MixAndMatchDiscount && discountApplications.Any())
                        {
                            // Mix and match is one shot, i.e. we don't apply partially here.
                            discount.IsFinished = true;
                        }

                        if (discountApplications.Any())
                        {
                            discount.CleanupLookups();
                            if (discount.CanCompound)
                            {
                                isCompoundedDiscountApplied = true;
                            }

#if DEBUG
                            DebugSetQuantityAppliedOnSalesTransaction(
                                GetQuantityCountKeyForNonOverlappedBestDeal(discount.OfferId),
                                transaction,
                                bestDiscountPath,
                                discount);
#endif
                        }

                        if (!discount.IsFinished)
                        {
                            // Finish the discount if no more quantity available.
                            bool hasQuantityToDiscount = false;

                            foreach (KeyValuePair<int, HashSet<decimal>> pair in discount.ItemGroupIndexToDiscountLineNumberSetMap)
                            {
                                int itemGroupIndex = pair.Key;
                                decimal quantityToDiscount = discount.CanCompound ? remainingQuantitiesForCompound[itemGroupIndex] : remainingQuantities[itemGroupIndex];
                                if (quantityToDiscount > decimal.Zero)
                                {
                                    hasQuantityToDiscount = true;
                                    break;
                                }
                            }

                            if (!hasQuantityToDiscount)
                            {
                                discount.IsFinished = true;
                            }
                        }
                    }

                    RemoveFinishedDiscounts(possibleDiscounts);

                    return isCompoundedDiscountApplied;
                }
            }

            private void ApplyNonOverlappingDiscountsOptimized(
                List<DiscountBase> possibleDiscounts,
                SalesTransaction transaction,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompound,
                ItemCompoundedOfferQuantityLookup itemCompoundedOfferQuantityLookup,
                HashSet<int> itemsWithOverlappingDiscounts,
                HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly,
                ref List<AppliedDiscountApplication> bestDiscountPath,
                ref decimal bestDiscountValue)
            {
                using (SimpleProfiler profiler = new SimpleProfiler("DiscountCalcuator.ApplyNonOverlappingDiscountsOptimized", 1))
                {
                    foreach (DiscountBase discount in possibleDiscounts)
                    {
                        if (discount is MixAndMatchDiscount || discount is MultipleBuyDiscount)
                        {
                            if (discount.CanApplyStandalone(itemsWithOverlappingDiscounts, itemsWithOverlappingDiscountsCompoundedOnly))
                            {
                                bool isInterrupted = false;

                                DiscountApplication[] applications = discount.GetDiscountApplications(
                                                    transaction,
                                                    discountableItemGroups,
                                                    discount.CanCompound ? remainingQuantities : remainingQuantitiesForCompound,
                                                    this.priceContext,
                                                    bestDiscountPath,
                                                    itemsWithOverlappingDiscounts,
                                                    out isInterrupted).ToArray();

                                if (!isInterrupted)
                                {
                                    decimal discountValue = bestDiscountValue;

                                    // Get the best path within the exclusive discounts
                                    isInterrupted = this.CalculateBestDiscountApplications(
                                        discountableItemGroups,
                                        ref bestDiscountPath,
                                        ref bestDiscountValue,
                                        applications,
                                        discountValue,
                                        remainingQuantities,
                                        remainingQuantitiesForCompound);
                                }
                                else
                                {
#if DEBUG
                                    SetDebugDataOnTransaction(transaction, HasTooManyDiscountApplicationsKey, true);
#endif
                                }

                                if (isInterrupted)
                                {
                                    IEnumerable<DiscountApplication> discountApplications = discount.GetDiscountApplicationsFastMode(
                                        transaction,
                                        discountableItemGroups,
                                        discount.CanCompound ? remainingQuantities : remainingQuantitiesForCompound);

                                    if (discountApplications.Any())
                                    {
                                        foreach (DiscountApplication discountApplication in discountApplications)
                                        {
                                            if (discountApplication.ApplyStandalone)
                                            {
                                                this.ApplyStandaloneDiscountApplication(
                                                    discountApplication,
                                                    discountableItemGroups,
                                                    remainingQuantities,
                                                    remainingQuantitiesForCompound,
                                                    itemCompoundedOfferQuantityLookup,
                                                    ref bestDiscountPath,
                                                    ref bestDiscountValue);
                                            }
                                        }

#if DEBUG
                                        SetDebugDataOnTransaction(transaction, GetAppliedMarginalValueAlgorithmKeyForDiscount(discount.OfferId), true);
#endif
                                    }
                                }
#if DEBUG
                                else
                                {
                                    SetDebugDataOnTransaction(transaction, GetFinishedExhaustiveAlgorithmKeyForDiscount(discount.OfferId), true);
                                }
#endif

#if DEBUG
                                SetDebugDataOnTransaction(transaction, GetStepCountKeyForDiscount(discount.OfferId), this.stepCount);
                                DebugSetQuantityAppliedOnSalesTransaction(
                                    GetQuantityCountKeyForNonOverlappedBestDeal(discount.OfferId),
                                    transaction,
                                    bestDiscountPath,
                                    discount);
#endif

                                discount.IsFinished = true;
                            }
                        }
                    }

                    RemoveFinishedDiscounts(possibleDiscounts);
                }
            }

            private void ApplyOverlappingDiscountsOptimized(
                List<DiscountBase> possibleDiscounts,
                SalesTransaction transaction,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompound,
                ItemCompoundedOfferQuantityLookup itemCompoundedOfferQuantityLookup,
                HashSet<int> itemsWithOverlappingDiscounts,
                HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly,
                ref List<AppliedDiscountApplication> bestDiscountPath,
                ref decimal bestDiscountValue)
            {
                if (possibleDiscounts.Count == 0)
                {
                    return;
                }

                using (SimpleProfiler profiler = new SimpleProfiler("DiscountCalcuator.ApplyNonExclusiveOverlappingDiscountsOptimized", 1))
                {
                    IEnumerable<OverlapppedDiscounts> overlappedDiscountsList = OverlapppedDiscounts.BuildOverlappedDiscountsGroup(possibleDiscounts);

#if DEBUG
                    if (overlappedDiscountsList.Any())
                    {
                        SetDebugDataOnTransaction(transaction, HasOverlappingDiscountKey, true);
                    }
#endif

                    foreach (OverlapppedDiscounts overlappedDiscounts in overlappedDiscountsList)
                    {
                        if (this.TryOptimizeForOneMixAndMatchWithOneLineGroup(
                            transaction,
                            discountableItemGroups,
                            overlappedDiscounts,
                            remainingQuantities,
                            remainingQuantitiesForCompound,
                            itemCompoundedOfferQuantityLookup,
                            ref bestDiscountPath,
                            ref bestDiscountValue))
                        {
                            continue;
                        }

                        this.TryPartiallyOptimizeForOneMixAndMatchWithOneLineGroupLeastExpensive(
                            transaction,
                            discountableItemGroups,
                            overlappedDiscounts,
                            remainingQuantities,
                            remainingQuantitiesForCompound,
                            itemCompoundedOfferQuantityLookup,
                            ref bestDiscountPath,
                            ref bestDiscountValue);

                        // Offer discount can appear in multiple instances of overlappedDiscounts.
                        // We have to limit the quantities to only applicable item group indiced in the overlappedDiscounts.
                        decimal[] remainingQuantitiesForThisGroupOfOverlappedDiscountsOnly = new decimal[remainingQuantities.Length];
                        decimal[] remainingQuantitiesForCompoundForThisGroupOfOverlappedDiscountsOnly = new decimal[remainingQuantities.Length];
                        foreach (int itemGroupIndex in overlappedDiscounts.CoveredItemGroupIndexSet)
                        {
                            remainingQuantitiesForThisGroupOfOverlappedDiscountsOnly[itemGroupIndex] = remainingQuantities[itemGroupIndex];
                            remainingQuantitiesForCompoundForThisGroupOfOverlappedDiscountsOnly[itemGroupIndex] = remainingQuantitiesForCompound[itemGroupIndex];
                        }

                        // Try best price algorithm first, if it's interrupted for taking too many steps, switch to marginal-value algorithm.
                        bool isInterrupted = false;

                        List<DiscountBase> discountsToApply = new List<DiscountBase>(overlappedDiscounts.MixAndMatchAndQuantityDiscounts.Values);
                        discountsToApply.AddRange(overlappedDiscounts.OfferDiscounts.Values);

                        List<DiscountApplication> discountApplicationList = new List<DiscountApplication>();

                        foreach (DiscountBase discount in discountsToApply)
                        {
                            bool isInterruptedForDiscount = false;
                            discountApplicationList.AddRange(discount.GetDiscountApplications(
                                                    transaction,
                                                    discountableItemGroups,
                                                    discount.CanCompound ? remainingQuantitiesForCompoundForThisGroupOfOverlappedDiscountsOnly : remainingQuantitiesForThisGroupOfOverlappedDiscountsOnly,
                                                    this.priceContext,
                                                    bestDiscountPath,
                                                    itemsWithOverlappingDiscounts,
                                                    out isInterruptedForDiscount));

                            if (isInterruptedForDiscount || this.priceContext.ExceedsMaxBestDealAlgorithmStepCount(discountApplicationList.Count))
                            {
#if DEBUG
                                SetDebugDataOnTransaction(transaction, HasTooManyDiscountApplicationsKey, true);
#endif
                                isInterrupted = true;
                                break;
                            }
                        }

#if DEBUG
                        List<string> offerIds = new List<string>();
                        foreach (KeyValuePair<string, DiscountBase> pair in overlappedDiscounts.MixAndMatchAndQuantityDiscounts)
                        {
                            offerIds.Add(pair.Key);
                        }
#endif

                        if (!isInterrupted && discountApplicationList.Any())
                        {
                            DiscountApplication[] nonExclusiveDiscountApplications = discountApplicationList
                                                        .OrderBy(p => p.Discount.ConcurrencyMode)
                                                        .ThenByDescending(p => p.SortIndex)
                                                        .ThenByDescending(p => p.SortValue)
                                                        .ToArray();

                            // When applying the discount, we still use original remainingQuantities and remainingQuantitiesForCompound
                            decimal discountValue = bestDiscountValue;

                            // Get the best path within the exclusive discounts
                            isInterrupted = this.CalculateBestDiscountApplications(
                                discountableItemGroups,
                                ref bestDiscountPath,
                                ref bestDiscountValue,
                                nonExclusiveDiscountApplications,
                                discountValue,
                                remainingQuantities,
                                remainingQuantitiesForCompound);

#if DEBUG
                            string stepCountKey = GetStepCountKeyForOverlappedGroup(offerIds, possibleDiscounts[0].ConcurrencyMode == ConcurrencyMode.Exclusive, possibleDiscounts[0].PricingPriorityNumber);
                            SetDebugDataOnTransaction(transaction, stepCountKey, this.stepCount);

                            if (!isInterrupted)
                            {
                                string finishedExhaustiveAlgorithmKey = GetFinishedExhaustiveAlgorithmKeyForOverlappedGroup(offerIds, possibleDiscounts[0].ConcurrencyMode == ConcurrencyMode.Exclusive, possibleDiscounts[0].PricingPriorityNumber);
                                SetDebugDataOnTransaction(transaction, finishedExhaustiveAlgorithmKey, true);
                            }
#endif
                        }

                        if (isInterrupted)
                        {
#if DEBUG
                            string appliedMarginalValueAlgorithmKey = GetAppliedMarginalValueAlgorithmKeyForOverlappedGroup(offerIds, possibleDiscounts[0].ConcurrencyMode == ConcurrencyMode.Exclusive, possibleDiscounts[0].PricingPriorityNumber);
                            SetDebugDataOnTransaction(transaction, appliedMarginalValueAlgorithmKey, true);
#endif

                            DiscountBase[] discountsSorted = overlappedDiscounts.GetSortedDiscountsToApplyInFastMode(
                                discountableItemGroups,
                                remainingQuantitiesForThisGroupOfOverlappedDiscountsOnly,
                                remainingQuantitiesForCompoundForThisGroupOfOverlappedDiscountsOnly,
                                itemsWithOverlappingDiscounts,
                                itemsWithOverlappingDiscountsCompoundedOnly);

                            for (int i = 0; i < discountsSorted.Length; i++)
                            {
                                DiscountBase discount = discountsSorted[i];

                                // Use remaining quantities only from the covered group to get discount applications.
                                IEnumerable<DiscountApplication> discountApplications = discount.GetDiscountApplicationsFastMode(
                                    transaction,
                                    discountableItemGroups,
                                    discount.CanCompound ? remainingQuantitiesForCompoundForThisGroupOfOverlappedDiscountsOnly : remainingQuantitiesForThisGroupOfOverlappedDiscountsOnly);

                                if (discountApplications.Any())
                                {
                                    // Apply the discount applications using remaining quantities for the whole transaction.
                                    this.ApplyStandaloneDiscountApplications(
                                        discountApplications,
                                        discountableItemGroups,
                                        remainingQuantities,
                                        remainingQuantitiesForCompound,
                                        itemCompoundedOfferQuantityLookup,
                                        ref bestDiscountPath,
                                        ref bestDiscountValue);

                                    // Reset remaining quantities from the covered group.
                                    foreach (int itemGroupIndex in overlappedDiscounts.CoveredItemGroupIndexSet)
                                    {
                                        remainingQuantitiesForThisGroupOfOverlappedDiscountsOnly[itemGroupIndex] = remainingQuantities[itemGroupIndex];
                                        remainingQuantitiesForCompoundForThisGroupOfOverlappedDiscountsOnly[itemGroupIndex] = remainingQuantitiesForCompound[itemGroupIndex];
                                    }
                                }

                                discount.CleanupLookups();

                                // A discount offer may cover multiple overlapped discount groups.
                                if (discount is MixAndMatchDiscount || discount is MultipleBuyDiscount)
                                {
                                    discount.IsFinished = true;
                                }
                            }
                        }
                    }
                }
            }

            private void ApplyStandaloneDiscountApplications(
                IEnumerable<DiscountApplication> discountApplications,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompound,
                ItemCompoundedOfferQuantityLookup itemCompoundedOfferQuantityLookup,
                ref List<AppliedDiscountApplication> bestDiscountPath,
                ref decimal bestDiscountValue)
            {
                foreach (DiscountApplication discountApplication in discountApplications)
                {
                    if (discountApplication.ApplyStandalone)
                    {
                        this.ApplyStandaloneDiscountApplication(
                            discountApplication,
                            discountableItemGroups,
                            remainingQuantities,
                            remainingQuantitiesForCompound,
                            itemCompoundedOfferQuantityLookup,
                            ref bestDiscountPath,
                            ref bestDiscountValue);
                    }
                }
            }

            private void ApplyStandaloneDiscountApplication(
                DiscountApplication discountApplication,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompound,
                ItemCompoundedOfferQuantityLookup itemCompoundedOfferQuantityLookup,
                ref List<AppliedDiscountApplication> bestDiscountPath,
                ref decimal bestDiscountValue)
            {
                AppliedDiscountApplication newAppliedDiscountApplication = discountApplication.Apply(
                    discountableItemGroups,
                    discountApplication.Discount.CanCompound ? remainingQuantitiesForCompound : remainingQuantities,
                    bestDiscountPath,
                    this.priceContext);

                if (newAppliedDiscountApplication != null)
                {
                    bestDiscountValue += newAppliedDiscountApplication.Value;
                    bestDiscountPath.Add(newAppliedDiscountApplication);

                    FixRemainingQuantitiesAfterApplyingDiscountApplication(
                        newAppliedDiscountApplication,
                        remainingQuantities,
                        remainingQuantitiesForCompound,
                        itemCompoundedOfferQuantityLookup);
                }
            }

            private bool TryOptimizeForOneMixAndMatchWithOneLineGroup(
                SalesTransaction transaction,
                DiscountableItemGroup[] discountableItemGroups,
                OverlapppedDiscounts overlappedDiscounts,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompound,
                ItemCompoundedOfferQuantityLookup itemCompoundedOfferQuantityLookup,
                ref List<AppliedDiscountApplication> bestDiscountPath,
                ref decimal bestDiscountValue)
            {
                Dictionary<int, decimal> mixAndMatchRelativeValueLookup = new Dictionary<int, decimal>();
                Dictionary<int, OfferDiscount> simpleDiscountOfferLookup = new Dictionary<int, OfferDiscount>();

                bool isOptimized = false;
                if (overlappedDiscounts.IsOkayForMixAndMatchOneLineGroupOptimizationAndFillupValueLookups(
                    mixAndMatchRelativeValueLookup,
                    simpleDiscountOfferLookup,
                    discountableItemGroups,
                    remainingQuantities))
                {
                    // Algorithm: (say mix and match buy 3 items for $10.
                    // 1. For each item with quantity 3, calculate relative value of mix and match against discount offer: M(AAA) - 3xS(A)
                    //    See IsOkayForMixAndMatchOneLineGroupOptimizationAndFillupValueLookups
                    // 2. Order items by relative values
                    // 3. Loop through items order by relative values descending, with quantity 3 in a group
                    //    if sum(relative value) for items in the group is positive, then apply mix and match discount,
                    //    else stop.
                    // 4. Apply discount offers for all the remaining items.
                    MixAndMatchDiscount mixAndMatchDiscount = overlappedDiscounts.MixAndMatchAndQuantityDiscounts.First().Value as MixAndMatchDiscount;

#if DEBUG
                    SetDebugDataOnTransaction(transaction, RelativePriceDictionaryKey, mixAndMatchRelativeValueLookup);
#endif

                    List<DiscountApplication> discountApplicationsForMixAndMatch = new List<DiscountApplication>();
                    mixAndMatchDiscount.GetDiscountApplicationsSequentiallyOptimizedForOneLineGroup(
                        discountApplicationsForMixAndMatch,
                        transaction,
                        discountableItemGroups,
                        mixAndMatchRelativeValueLookup,
                        remainingQuantities);

                    if (discountApplicationsForMixAndMatch.Count > 0)
                    {
                        this.ApplyStandaloneDiscountApplications(
                            discountApplicationsForMixAndMatch,
                            discountableItemGroups,
                            remainingQuantities,
                            remainingQuantitiesForCompound,
                            itemCompoundedOfferQuantityLookup,
                            ref bestDiscountPath,
                            ref bestDiscountValue);

                        mixAndMatchDiscount.IsFinished = true;
                    }

                    List<DiscountApplication> discountApplicationsForSimpleDiscounts = new List<DiscountApplication>();
                    foreach (KeyValuePair<int, decimal> pair in mixAndMatchRelativeValueLookup)
                    {
                        int itemGroupIndex = pair.Key;
                        decimal quantity = remainingQuantities[itemGroupIndex];
                        if (quantity > decimal.Zero)
                        {
                            OfferDiscount offerDiscount = null;
                            if (simpleDiscountOfferLookup.TryGetValue(itemGroupIndex, out offerDiscount))
                            {
                                offerDiscount.GetDiscountApplicationHonorQuantityForOneItem(
                                    discountApplicationsForSimpleDiscounts,
                                    itemGroupIndex,
                                    transaction,
                                    discountableItemGroups,
                                    quantity);
                            }
                        }
                    }

                    if (discountApplicationsForSimpleDiscounts.Count > 0)
                    {
                        this.ApplyStandaloneDiscountApplications(
                            discountApplicationsForSimpleDiscounts,
                            discountableItemGroups,
                            remainingQuantities,
                            remainingQuantitiesForCompound,
                            itemCompoundedOfferQuantityLookup,
                            ref bestDiscountPath,
                            ref bestDiscountValue);
                    }

                    isOptimized = true;

#if DEBUG
                    SetDebugDataOnTransaction(transaction, GetIsOptimizedMixMatchOneLineGroupKeyForDiscount(overlappedDiscounts.MixAndMatchAndQuantityDiscounts.First().Key), true);
#endif
                }

                return isOptimized;
            }

            private bool TryPartiallyOptimizeForOneMixAndMatchWithOneLineGroupLeastExpensive(
                SalesTransaction transaction,
                DiscountableItemGroup[] discountableItemGroups,
                OverlapppedDiscounts overlappedDiscounts,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompound,
                ItemCompoundedOfferQuantityLookup itemCompoundedOfferQuantityLookup,
                ref List<AppliedDiscountApplication> bestDiscountPath,
                ref decimal bestDiscountValue)
            {
                // Algorithm: (say mix and match least expensive: buy 2 items get 1 50% off.
                // 1. For each item with quantity 3, calculate relative value of mix and match against discount offer: M(AAA) - 3xS(A)
                //    See IsOkayForMixAndMatchLeastExpensiveOneLineGroupPartialOptimizationAndFillupValueLookups
                // 2. Group items by relative and price and order the group by relative price, in consolidatedListOfItemsWithSamePriceAndRelativeValuesDescending.
                //    See IsOkayForMixAndMatchLeastExpensiveOneLineGroupPartialOptimizationAndFillupValueLookups
                // 3. Loop through item groups order by relative values descending.
                //    if relative price is positive, then apply mix and match for them, until total quantity remaining for them goes under or equal to 4,
                //    else (i.e. relative price is zero or negative), then apply simple discounts for them, until total quantity remaining form them goes under or equal to 2.
                Dictionary<int, decimal> mixAndMatchRelativeValueLookup = new Dictionary<int, decimal>();
                List<int> itemGroupIndexListSortedByRelativePriceDescending = new List<int>();
                List<HashSet<int>> consolidatedListOfItemsWithSamePriceAndRelativeValuesDescending = new List<HashSet<int>>();
                Dictionary<int, OfferDiscount> simpleDiscountOfferLookup = new Dictionary<int, OfferDiscount>();

                bool isPartiallyOptimized = false;
                if (overlappedDiscounts.IsOkayForMixAndMatchLeastExpensiveOneLineGroupPartialOptimizationAndFillupValueLookups(
                    mixAndMatchRelativeValueLookup,
                    itemGroupIndexListSortedByRelativePriceDescending,
                    consolidatedListOfItemsWithSamePriceAndRelativeValuesDescending,
                    simpleDiscountOfferLookup,
                    discountableItemGroups,
                    remainingQuantities))
                {
#if DEBUG
                    SetDebugDataOnTransaction(transaction, RelativePriceDictionaryKey, mixAndMatchRelativeValueLookup);
#endif

                    MixAndMatchDiscount mixAndMatchDiscount = overlappedDiscounts.MixAndMatchAndQuantityDiscounts.First().Value as MixAndMatchDiscount;

                    List<DiscountApplication> discountApplicationsForMixAndMatch = new List<DiscountApplication>();
                    mixAndMatchDiscount.GetDiscountApplicationsPartiallyOptimizedForOneLineGroupLeastExpensive(
                        discountApplicationsForMixAndMatch,
                        transaction,
                        discountableItemGroups,
                        mixAndMatchRelativeValueLookup,
                        consolidatedListOfItemsWithSamePriceAndRelativeValuesDescending,
                        remainingQuantities);

                    if (discountApplicationsForMixAndMatch.Count > 0)
                    {
                        this.ApplyStandaloneDiscountApplications(
                            discountApplicationsForMixAndMatch,
                            discountableItemGroups,
                            remainingQuantities,
                            remainingQuantitiesForCompound,
                            itemCompoundedOfferQuantityLookup,
                            ref bestDiscountPath,
                            ref bestDiscountValue);
                    }

                    List<DiscountApplication> discountApplicationsForSimpleDiscounts = new List<DiscountApplication>();

                    // Starting from the worse value;
                    int numberOfItemsNeededForMixAndMatch = mixAndMatchDiscount.LineGroupToNumberOfItemsMap.First().Value;
                    for (int indexForConsolidatedList = consolidatedListOfItemsWithSamePriceAndRelativeValuesDescending.Count - 1; indexForConsolidatedList >= 0; indexForConsolidatedList--)
                    {
                        HashSet<int> itemGroupIndexSetWithSameRelativePriceAndPrice = consolidatedListOfItemsWithSamePriceAndRelativeValuesDescending[indexForConsolidatedList];
                        int itemGroupIndexFirst = itemGroupIndexSetWithSameRelativePriceAndPrice.First();
                        decimal relativeValue = decimal.Zero;
                        decimal quantityToApply = decimal.Zero;
                        if (mixAndMatchRelativeValueLookup.TryGetValue(itemGroupIndexFirst, out relativeValue))
                        {
                            if (relativeValue <= decimal.Zero)
                            {
                                decimal totalQuantity = itemGroupIndexSetWithSameRelativePriceAndPrice.Sum(p => remainingQuantities[p]);
                                decimal remainder = totalQuantity % numberOfItemsNeededForMixAndMatch;

                                quantityToApply = totalQuantity - remainder;
                            }
                        }

                        if (quantityToApply > decimal.Zero)
                        {
                            // Multiple items share the same price and relative price.
                            foreach (int itemGroupIndex in itemGroupIndexSetWithSameRelativePriceAndPrice)
                            {
                                decimal quantityAvailable = remainingQuantities[itemGroupIndex];
                                decimal quantityUsed = Math.Min(quantityAvailable, quantityToApply);
                                if (quantityUsed > decimal.Zero)
                                {
                                    OfferDiscount offerDiscount = null;
                                    if (simpleDiscountOfferLookup.TryGetValue(itemGroupIndex, out offerDiscount))
                                    {
                                        offerDiscount.GetDiscountApplicationHonorQuantityForOneItem(
                                            discountApplicationsForSimpleDiscounts,
                                            itemGroupIndex,
                                            transaction,
                                            discountableItemGroups,
                                            quantityUsed);
                                    }
                                }

                                quantityToApply -= quantityUsed;

                                if (quantityToApply <= decimal.Zero)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    if (discountApplicationsForSimpleDiscounts.Count > 0)
                    {
                        this.ApplyStandaloneDiscountApplications(
                            discountApplicationsForSimpleDiscounts,
                            discountableItemGroups,
                            remainingQuantities,
                            remainingQuantitiesForCompound,
                            itemCompoundedOfferQuantityLookup,
                            ref bestDiscountPath,
                            ref bestDiscountValue);
                    }

                    isPartiallyOptimized = true;

#if DEBUG
                    SetDebugDataOnTransaction(transaction, GetIsPartiallyOptimizedMixMatchOneLineGroupKeyForDiscount(overlappedDiscounts.MixAndMatchAndQuantityDiscounts.First().Key), true);
                    SetDebugDataOnTransaction(transaction, QuantitiesAfterPartiallyOptimizedMixMatchOneLineGroupKey, remainingQuantities.Clone());
#endif
                }

                return isPartiallyOptimized;
            }

            /// <summary>
            /// Calculates the best path for application of the available discounts.
            /// </summary>
            /// <param name="discountableItemGroups">The discountable items on the transaction.</param>
            /// <param name="bestDiscountPath">The current best discount path.</param>
            /// <param name="bestDiscountValue">The current best discount value.</param>
            /// <param name="possibleDiscountApplications">The set of possible discount applications.</param>
            /// <param name="discountValue">The current value of this path.</param>
            /// <param name="remainingQuantities">The remaining quantities of each item in the discountableItemGroups parameter.</param>
            /// <param name="remainingQuantitiesForCoumpound">The remaining quantities for compound.</param>
            /// <returns>true if finished, false otherwise.</returns>
            private bool CalculateBestDiscountApplications(
                DiscountableItemGroup[] discountableItemGroups,
                ref List<AppliedDiscountApplication> bestDiscountPath,
                ref decimal bestDiscountValue,
                DiscountApplication[] possibleDiscountApplications,
                decimal discountValue,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCoumpound)
            {
#if DEBUG
                DateTime startDateTime = DateTime.UtcNow;
                this.stepCount = 0;
                this.pathCount = 0;
#endif

                int discountApplicationNextIndex = 0;
                BitSet remainingApplications = new BitSet((uint)possibleDiscountApplications.Length, true);
                BitSet[] availableItemDiscounts = new BitSet[discountableItemGroups.Length];

                PrepareAvailableItemDiscountBitSes(availableItemDiscounts, possibleDiscountApplications);

                int numberOfTimesApplied = 1;

                decimal bestDiscountValueHere = decimal.Zero;

                int stepCountForBestDealAlgorithm = 0;
                bool isInterrupted = false;

                // When interrupted, we need to keep the original quantities.
                decimal[] remainingQuantitiesOriginal = remainingQuantities;
                decimal[] remainingQuantitiesForCoumpoundOriginal = remainingQuantitiesForCoumpound;
                decimal[] remainingQuantitiesOriginalCopy = (decimal[])remainingQuantities.Clone();
                decimal[] remainingQuantitiesForCoumpoundOriginalCopy = (decimal[])remainingQuantitiesForCoumpound.Clone();
                List<AppliedDiscountApplication> bestDiscountPathOriginal = new List<AppliedDiscountApplication>(bestDiscountPath);

                if (possibleDiscountApplications.Length == 0)
                {
                    return isInterrupted;
                }

#if DEBUG
                WriteDebugInfoForPossibleDiscountApplications(discountableItemGroups, possibleDiscountApplications);
#endif

                Stack<AppliedDiscountApplication> appliedDiscountsStack = new Stack<AppliedDiscountApplication>();
                foreach (AppliedDiscountApplication appliedDiscountApplication in bestDiscountPath)
                {
                    appliedDiscountsStack.Push(appliedDiscountApplication);
                }

                Stack<DiscountApplicationState> discountApplicationStateStack = new Stack<DiscountApplicationState>();

                while (discountApplicationNextIndex != InvalidDiscountApplicationIndex)
                {
                    // Couldn't finished in max number of steps allowed. Will switch to marginal-value backup algorithm.
                    if (this.priceContext.ExceedsMaxBestDealAlgorithmStepCount(stepCountForBestDealAlgorithm))
                    {
                        isInterrupted = true;
                        break;
                    }

                    stepCountForBestDealAlgorithm++;

                    DiscountApplication discountApplication = possibleDiscountApplications[discountApplicationNextIndex];

                    // Get the value of this application.  If the value is null (threshold discounts could do this), do not apply this discount, move to the next index.
                    AppliedDiscountApplication appliedDiscountApplication = discountApplication.Apply(
                        discountableItemGroups,
                        discountApplication.Discount.CanCompound ? remainingQuantitiesForCoumpound : remainingQuantities,
                        appliedDiscountsStack,
                        this.priceContext);

                    // null means the DA was not applied which could happen because we don't have enough quantities in remainingQuanitites
                    // for the items covered by the DA's discount.
                    if (appliedDiscountApplication == null)
                    {
                        remainingApplications[discountApplicationNextIndex] = false;
                    }
                    else
                    {
                        decimal thisAmount = appliedDiscountApplication.Value;

                        // Before we start processing a discount, we need to save the current state for when we pop back up.
                        discountApplicationStateStack.Push(new DiscountApplicationState()
                        {
                            Value = discountValue,
                            AppliedDiscountApplication = discountApplicationNextIndex,
                            RemainingApplications = new BitSet(remainingApplications),
                            RemainingQuantities = (decimal[])remainingQuantities.Clone(),
                            RemainingQuantitiesForCompound = (decimal[])remainingQuantitiesForCoumpound.Clone(),
                            NumberOfTimesApplied = numberOfTimesApplied
                        });

                        // Start processing this specific discount application.
                        discountValue += thisAmount;

                        // Add the application to the applied discounts stack
                        appliedDiscountsStack.Push(appliedDiscountApplication);

                        // We can't rely on FixRemainingQuantitiesAfterApplyingDiscountApplication,
                        // because itemCompoundedOfferQuantityLookup is not reliable, when it goes
                        // back and forth, trying all permutations to get the best deal.
                        // Remove the quantities affected by this discount from the cart.
                        if (!discountApplication.Discount.CanCompound)
                        {
                            GetRemainingApplicationsForNonCompoundedDiscount(
                                possibleDiscountApplications,
                                appliedDiscountApplication,
                                discountApplicationNextIndex,
                                remainingQuantities,
                                remainingQuantitiesForCoumpound,
                                ref remainingApplications,
                                availableItemDiscounts);
                        }
                        else
                        {
                            GetRemainingApplicationsForCompoundedDiscount(
                                appliedDiscountsStack,
                                possibleDiscountApplications,
                                discountApplicationNextIndex,
                                remainingQuantitiesForCoumpound,
                                ref remainingApplications);
                        }

                        // If applying the current discount again would be more than the number of times this discount can be applied, 
                        // the current discount should be removed from the possible discounts.
                        if (possibleDiscountApplications[discountApplicationNextIndex].NumberOfTimesApplicable != DiscountBase.UnlimitedNumberOfTimesApplicable
                            && (numberOfTimesApplied + 1) > possibleDiscountApplications[discountApplicationNextIndex].NumberOfTimesApplicable)
                        {
                            remainingApplications[discountApplicationNextIndex] = false;
                        }

#if DEBUG
                        this.WriteDebugInfoForPath(DebugLogHeader.Step, thisAmount, discountValue, appliedDiscountsStack, discountApplicationStateStack, discountApplicationNextIndex);
#endif
                    }

                    // We now have applied this discount and have determined which other discounts can apply (including itself), so we should continue processing those.
                    // If none are left, then we should check the value against the previous best-known value and replace it if needed.
                    bool discountsRemain = false;
                    int newIndex = InvalidDiscountApplicationIndex;

                    // Find the next available discount.
                    for (int x = discountApplicationNextIndex; x < remainingApplications.Length; x++)
                    {
                        if (remainingApplications[x])
                        {
                            newIndex = x;
                            discountsRemain = true;
                            break;
                        }
                    }

                    if (discountsRemain)
                    {
                        // Reset the next index to the next available application found in the step above.
                        if (discountApplicationNextIndex == newIndex)
                        {
                            // We are still dealing with the same DA so need to increase its applied counter.
                            numberOfTimesApplied++;
                        }
                        else
                        {
                            // We are about to deal with new DA so need to initialize the applied counter.
                            discountApplicationNextIndex = newIndex;
                            numberOfTimesApplied = 1;
                        }
                    }
                    else
                    {
#if DEBUG
                        this.WriteDebugInfoForPath(DebugLogHeader.Path, 0, discountValue, appliedDiscountsStack, discountApplicationStateStack, discountApplicationNextIndex);
#endif

                        // No remaining discounts to process, check the value of this path and pop back up.
                        if (discountValue > bestDiscountValueHere)
                        {
                            bestDiscountValueHere = discountValue;
                            bestDiscountPath = appliedDiscountsStack.Reverse().ToList();
                        }

                        // Set the nextIndex, just in case we will not be popping back up (this will be -1 in that case).
                        discountApplicationNextIndex = newIndex;

                        while (discountApplicationStateStack.Count > 0 && newIndex == InvalidDiscountApplicationIndex)
                        {
                            DiscountApplicationState discountApplicationState = discountApplicationStateStack.Pop();
                            appliedDiscountsStack.Pop();
                            remainingQuantities = discountApplicationState.RemainingQuantities;
                            remainingQuantitiesForCoumpound = discountApplicationState.RemainingQuantitiesForCompound;
                            remainingApplications = discountApplicationState.RemainingApplications;
                            discountValue = discountApplicationState.Value;

                            // All paths for that index should have been covered to this point, start at the next index value (otherwise this would be an infinite loop).
                            // If this index is a compounding discount, continue up the stack, since there are no alternate paths with compounding discounts
                            // except for the possibility of the same offer ID for compounding having multiple possible applications.
                            if (!possibleDiscountApplications[discountApplicationState.AppliedDiscountApplication].Discount.CanCompound
                                || possibleDiscountApplications.Skip(discountApplicationState.AppliedDiscountApplication + 1).Any(p => !p.CanCompound(possibleDiscountApplications[discountApplicationState.AppliedDiscountApplication]))
                                || possibleDiscountApplications[discountApplicationState.AppliedDiscountApplication].Discount.PeriodicDiscountType == PeriodicDiscountOfferType.Threshold)
                            {
                                for (int x = discountApplicationState.AppliedDiscountApplication + 1; x < remainingApplications.Length; x++)
                                {
                                    if (remainingApplications[x])
                                    {
                                        newIndex = x;
                                        break;
                                    }
                                }
                            }

                            discountApplicationNextIndex = newIndex;
                        }
                    }
                }

                if (isInterrupted)
                {
                    // Restore best discount path and original quantities when interrupted.
                    for (int i = 0; i < remainingQuantities.Length; i++)
                    {
                        bestDiscountPath.Clear();
                        bestDiscountPath.AddRange(bestDiscountPathOriginal);
                        remainingQuantitiesOriginal[i] = remainingQuantitiesOriginalCopy[i];
                        remainingQuantitiesForCoumpoundOriginal[i] = remainingQuantitiesForCoumpoundOriginalCopy[i];
                    }
                }
                else
                {
                    bestDiscountValue += bestDiscountValueHere;
                }

#if DEBUG
                this.WriteDebugInfoSummary(startDateTime);
#endif

                return isInterrupted;
            }

            /// <summary>
            /// Calculates and applies the best path for discounts on a transaction and a set of discountable item groups.
            /// </summary>
            /// <param name="transaction">The current transaction to calculate discounts for.</param>
            /// <param name="discountableItemGroups">The groups of discountable sales lines on the transaction.</param>
            /// <param name="isReturn">True if it's return.</param>
            private void CalculateAndApplyBestDiscounts(SalesTransaction transaction, DiscountableItemGroup[] discountableItemGroups, bool isReturn)
            {
#if DEBUG
                SetDebugDataOnTransaction(transaction, MaxStepCountKey, this.priceContext.MaxBestDealAlgorithmStepCount);
#endif

                using (SimpleProfiler profiler = new SimpleProfiler("DiscountCalcuator.CalculateAndApplyBestDiscounts"))
                {
                    // Step 0: Set up optimization: for each discount, prepare discount line number to item group index set lookup, and item group idex to discount line number set lookup.
                    this.SetupOptimization(discountableItemGroups);

                    List<AppliedDiscountApplication> bestDiscountPath = new List<AppliedDiscountApplication>();
                    decimal bestDiscountValue = 0.0M;
                    decimal[] originalQuantities = discountableItemGroups.Select(p => p.Quantity).ToArray();
                    decimal[] remainingQuantities = (decimal[])originalQuantities.Clone();
                    decimal[] remainingQuantitiesForCompound = (decimal[])originalQuantities.Clone();
                    ItemCompoundedOfferQuantityLookup itemCompoundedOfferQuantityLookup = new ItemCompoundedOfferQuantityLookup();

                    // Step 1: Process non threshold discounts by priority.
                    List<DiscountBase> possibleNonThresholdDiscounts = this.GetPossibleDiscountsForLineItems(
                        transaction,
                        this.allPriceGroups,
                        this.priceContext.CurrencyCode,
                        isReturn,
                        this.priceContext,
                        remainingQuantities,
                        remainingQuantitiesForCompound,
                        false);

                    if (this.priceContext.IsDiagnosticsCollected && possibleNonThresholdDiscounts.Any())
                    {
                        this.priceContext.PricingEngineDiagnosticsObject.AddDiscountsConsidered(possibleNonThresholdDiscounts.Select(x => x.OfferId).ToList());
                    }

                    List<int> sortedPriorityList = GetSortedPriorityListFromDiscounts(possibleNonThresholdDiscounts);
                    decimal discountValue = decimal.Zero;

                    // Process non-threshold discounts by priority, from highest to lowest, by exclusive and by non-exclusive.
                    foreach (int priorityNumber in sortedPriorityList)
                    {
                        List<DiscountBase> possibleExclusiveDiscounts = new List<DiscountBase>();
                        List<MixAndMatchDiscount> possibleExclusiveSpecialMixAndMatches = new List<MixAndMatchDiscount>();
                        List<DiscountBase> possibleNonExclusiveDiscounts = new List<DiscountBase>();
                        List<MixAndMatchDiscount> possibleNonExclusiveSpecialMixAndMatches = new List<MixAndMatchDiscount>();
                        GetDiscountsByPriority(
                            possibleNonThresholdDiscounts,
                            priorityNumber,
                            possibleExclusiveDiscounts,
                            possibleExclusiveSpecialMixAndMatches,
                            possibleNonExclusiveDiscounts,
                            possibleNonExclusiveSpecialMixAndMatches);

                        // Step 1.1 Exclusive non-threshold per priority
                        if (possibleExclusiveDiscounts.Any())
                        {
                            this.CalculateNonThresholdDiscounts(
                                transaction,
                                possibleExclusiveDiscounts,
                                discountableItemGroups,
                                ref bestDiscountPath,
                                ref bestDiscountValue,
                                remainingQuantities,
                                remainingQuantitiesForCompound,
                                itemCompoundedOfferQuantityLookup);

                            this.GenerateDiscountLines(bestDiscountPath, discountableItemGroups);

                            discountValue = bestDiscountValue;
                            remainingQuantities = (decimal[])originalQuantities.Clone();
                            remainingQuantitiesForCompound = (decimal[])originalQuantities.Clone();
                            itemCompoundedOfferQuantityLookup.RefreshLookupAndPrepareRemainingQuantitiesForNonThresholds(
                                bestDiscountPath,
                                priorityNumber,
                                remainingQuantities,
                                remainingQuantitiesForCompound,
                                reserveCompoundedQuantityForLeastExpensiveFavorRetailer: false);
                        }

                        // Step 1.2 Exclusive non-threshold special mix and math per priority
                        if (possibleExclusiveSpecialMixAndMatches.Any())
                        {
                            this.CalculateMixAndMatchLeastExpensiveFavorRetailer(
                                transaction,
                                possibleExclusiveSpecialMixAndMatches,
                                discountableItemGroups,
                                ref bestDiscountPath,
                                ref bestDiscountValue,
                                remainingQuantities,
                                remainingQuantitiesForCompound,
                                itemCompoundedOfferQuantityLookup);

                            this.GenerateDiscountLines(bestDiscountPath, discountableItemGroups);

                            discountValue = bestDiscountValue;
                            remainingQuantities = (decimal[])originalQuantities.Clone();
                            remainingQuantitiesForCompound = (decimal[])originalQuantities.Clone();
                            itemCompoundedOfferQuantityLookup.RefreshLookupAndPrepareRemainingQuantitiesForLeastExpensiveFavoRetailer(
                                bestDiscountPath,
                                priorityNumber,
                                remainingQuantities,
                                remainingQuantitiesForCompound);
                        }

                        // Step 1.3 Non exclusive non-threshold per priority
                        if (possibleNonExclusiveDiscounts.Any())
                        {
                            this.CalculateNonThresholdDiscounts(
                                transaction,
                                possibleNonExclusiveDiscounts,
                                discountableItemGroups,
                                ref bestDiscountPath,
                                ref bestDiscountValue,
                                remainingQuantities,
                                remainingQuantitiesForCompound,
                                itemCompoundedOfferQuantityLookup);

                            this.GenerateDiscountLines(bestDiscountPath, discountableItemGroups);

                            discountValue = bestDiscountValue;
                            remainingQuantities = (decimal[])originalQuantities.Clone();
                            remainingQuantitiesForCompound = (decimal[])originalQuantities.Clone();
                            itemCompoundedOfferQuantityLookup.RefreshLookupAndPrepareRemainingQuantitiesForNonThresholds(
                                bestDiscountPath,
                                priorityNumber,
                                remainingQuantities,
                                remainingQuantitiesForCompound,
                                reserveCompoundedQuantityForLeastExpensiveFavorRetailer: possibleNonExclusiveSpecialMixAndMatches.Any());
                        }

                        // Step 1.4 Non exclusive non-threshold special mix and math per priority
                        if (possibleNonExclusiveSpecialMixAndMatches.Any())
                        {
                            this.CalculateMixAndMatchLeastExpensiveFavorRetailer(
                                transaction,
                                possibleNonExclusiveSpecialMixAndMatches,
                                discountableItemGroups,
                                ref bestDiscountPath,
                                ref bestDiscountValue,
                                remainingQuantities,
                                remainingQuantitiesForCompound,
                                itemCompoundedOfferQuantityLookup);

                            this.GenerateDiscountLines(bestDiscountPath, discountableItemGroups);

                            discountValue = bestDiscountValue;
                            remainingQuantities = (decimal[])originalQuantities.Clone();
                            remainingQuantitiesForCompound = (decimal[])originalQuantities.Clone();
                            itemCompoundedOfferQuantityLookup.RefreshLookupAndPrepareRemainingQuantitiesForLeastExpensiveFavoRetailer(
                                bestDiscountPath,
                                priorityNumber,
                                remainingQuantities,
                                remainingQuantitiesForCompound);
                        }
                    }

                    // The last one of non-threshold discounts may not get anything.
                    // We need to reset remaining quantities for compounded to start threshold discount calculation.
                    remainingQuantities = (decimal[])originalQuantities.Clone();
                    remainingQuantitiesForCompound = (decimal[])originalQuantities.Clone();
                    itemCompoundedOfferQuantityLookup.PrepareRemainingQuantitiesForThresholdsFirstTime(
                        bestDiscountPath,
                        remainingQuantities,
                        remainingQuantitiesForCompound);

                    // Step 2 Process threshold discounts by priority.
                    List<DiscountBase> possibleThresholdDiscounts = this.GetPossibleDiscountsForLineItems(
                        transaction,
                        this.allPriceGroups,
                        this.priceContext.CurrencyCode,
                        isReturn,
                        this.priceContext,
                        remainingQuantities,
                        remainingQuantitiesForCompound,
                        true);
                    sortedPriorityList = GetSortedPriorityListFromDiscounts(possibleThresholdDiscounts);

                    if (this.priceContext.IsDiagnosticsCollected && possibleThresholdDiscounts.Any())
                    {
                        this.priceContext.PricingEngineDiagnosticsObject.AddDiscountsConsidered(possibleThresholdDiscounts.Select(x => x.OfferId).ToList());
                    }

                    // Process threshold discounts by priority, from highest to lowest, by exclusive and by non-exclusive.
                    foreach (int priorityNumber in sortedPriorityList)
                    {
                        // Mix and match discounts will be ignored.
                        List<DiscountBase> possibleExclusiveDiscounts = new List<DiscountBase>();
                        List<DiscountBase> possibleNonExclusiveDiscounts = new List<DiscountBase>();
                        List<MixAndMatchDiscount> possibleExclusiveSpecialMixAndMatches = new List<MixAndMatchDiscount>();
                        List<MixAndMatchDiscount> possibleNonExclusiveSpecialMixAndMatches = new List<MixAndMatchDiscount>();
                        GetDiscountsByPriority(
                            possibleThresholdDiscounts,
                            priorityNumber,
                            possibleExclusiveDiscounts,
                            possibleExclusiveSpecialMixAndMatches,
                            possibleNonExclusiveDiscounts,
                            possibleNonExclusiveSpecialMixAndMatches);

                        // Step 2.1 Exclusive threshold discounts per priority.
                        if (possibleExclusiveDiscounts.Any())
                        {
                            this.CalculateThresholdDiscounts(
                                transaction,
                                possibleExclusiveDiscounts,
                                discountableItemGroups,
                                ref bestDiscountPath,
                                ref bestDiscountValue,
                                discountValue,
                                remainingQuantities,
                                remainingQuantitiesForCompound,
                                itemCompoundedOfferQuantityLookup);

                            this.GenerateDiscountLines(bestDiscountPath, discountableItemGroups);

                            discountValue = bestDiscountValue;
                            remainingQuantities = (decimal[])originalQuantities.Clone();
                            remainingQuantitiesForCompound = (decimal[])originalQuantities.Clone();
                            itemCompoundedOfferQuantityLookup.RefreshLookupAndPrepareRemainingQuantitiesForThresholds(
                                bestDiscountPath,
                                priorityNumber,
                                remainingQuantities,
                                remainingQuantitiesForCompound);
                        }

                        // Step 2.2 Non exclusive threshold discounts per priority.
                        if (possibleNonExclusiveDiscounts.Any())
                        {
                            this.CalculateThresholdDiscounts(
                                transaction,
                                possibleNonExclusiveDiscounts,
                                discountableItemGroups,
                                ref bestDiscountPath,
                                ref bestDiscountValue,
                                discountValue,
                                remainingQuantities,
                                remainingQuantitiesForCompound,
                                itemCompoundedOfferQuantityLookup);

                            this.GenerateDiscountLines(bestDiscountPath, discountableItemGroups);

                            discountValue = bestDiscountValue;
                            remainingQuantities = (decimal[])originalQuantities.Clone();
                            remainingQuantitiesForCompound = (decimal[])originalQuantities.Clone();
                            itemCompoundedOfferQuantityLookup.RefreshLookupAndPrepareRemainingQuantitiesForThresholds(
                                bestDiscountPath,
                                priorityNumber,
                                remainingQuantities,
                                remainingQuantitiesForCompound);
                        }
                    }

                    using (SimpleProfiler p = new SimpleProfiler("DiscountCalcuator.CalculateAndApplyBestDiscounts.ApplyDiscountLines"))
                    {
                        foreach (AppliedDiscountApplication appliedDiscountApplication in bestDiscountPath)
                        {
                            appliedDiscountApplication.Apply(discountableItemGroups);
                        }

                        // Apply discount lines to the groups, which may require splitting line items.
                        foreach (DiscountableItemGroup group in discountableItemGroups)
                        {
                            // Loop through the DiscountLineQuantities to apply the proper number, split compound and non-compound discounts.
                            group.ApplyDiscountLines(transaction, isReturn);
                        }
                    }
                }
            }

            private void GenerateDiscountLines(
                IEnumerable<AppliedDiscountApplication> appliedDiscounts,
                DiscountableItemGroup[] discountableItemGroups)
            {
                using (SimpleProfiler profiler = new SimpleProfiler("DiscountCalcuator.GenerateDiscountLines", 1))
                {
                    foreach (AppliedDiscountApplication currentApplied in appliedDiscounts)
                    {
                        currentApplied.GenerateDiscountLines(discountableItemGroups, this.priceContext);
                    }
                }
            }

            private void SetupOptimization(DiscountableItemGroup[] discountableItemGroups)
            {
                using (SimpleProfiler profiler = new SimpleProfiler("DiscountCalculator.SetupOptimization"))
                {
                    // Pre-optimization: for each discount, prepare discount line number to item group index set lookup, and item group idex to discount line number set lookup.
                    // Overall, we calculate discounts twice, one for sales, one for return.
                    this.offerIdToDiscountsLookup.Clear();
                    foreach (KeyValuePair<long, List<DiscountBase>> pair in this.itemDiscountsLookup)
                    {
                        foreach (DiscountBase discount in pair.Value)
                        {
                            if (!this.offerIdToDiscountsLookup.ContainsKey(discount.OfferId))
                            {
                                this.offerIdToDiscountsLookup.Add(discount.OfferId, discount);
                            }
                        }
                    }

                    foreach (KeyValuePair<string, DiscountBase> pair in this.offerIdToDiscountsLookup)
                    {
                        pair.Value.InitializeAndPrepareDiscountLineNumberAndItemGroupIndexLookups(discountableItemGroups, this.priceContext);
                    }
                }
            }

            /// <summary>
            /// Initializes the internal variables that hold all of the possible discounts.
            /// </summary>
            /// <param name="transaction">The sales transaction to initialize discounts for.</param>
            private void InitializeDiscounts(SalesTransaction transaction)
            {
                if (this.enableCache)
                {
                    this.itemDiscountsLookup = RetailDiscountStore.GetProductOrVarintToDiscountMapFromCache(this.pricingDataManager, this.priceContext, transaction);
                }
                else
                {
                    this.itemDiscountsLookup = RetailDiscountStore.GetProductOrVariantToDiscountMapLive(transaction, this.priceContext, this.pricingDataManager);
                }
            }

            private void CalculateMixAndMatchLeastExpensiveFavorRetailer(
                SalesTransaction transaction,
                List<MixAndMatchDiscount> possibleMixAndMatches,
                DiscountableItemGroup[] discountableItemGroups,
                ref List<AppliedDiscountApplication> bestDiscountPath,
                ref decimal bestDiscountValue,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompound,
                ItemCompoundedOfferQuantityLookup itemCompoundedOfferQuantityLookup)
            {
                foreach (MixAndMatchDiscount mixAndMatch in possibleMixAndMatches)
                {
                    decimal[] remainingQuantitiesToUser = mixAndMatch.CanCompound ? remainingQuantitiesForCompound : remainingQuantities;

                    mixAndMatch.PreOptimization(
                        discountableItemGroups,
                        remainingQuantitiesToUser,
                        new HashSet<int>(),
                        new HashSet<int>());
                }

                possibleMixAndMatches.RemoveAll(p => p.IsFinished);

                HashSet<int> itemsWithOverlappingDiscounts = new HashSet<int>();
                HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly = new HashSet<int>();
                Dictionary<int, List<DiscountBase>> itemGroupIndexToDiscountsLookup = new Dictionary<int, List<DiscountBase>>();
                List<DiscountBase> possibleDiscounts = new List<DiscountBase>(possibleMixAndMatches);

                BuildItemsWithOverlappingDiscounts(
                    itemsWithOverlappingDiscounts,
                    itemsWithOverlappingDiscountsCompoundedOnly,
                    itemGroupIndexToDiscountsLookup,
                    possibleDiscounts);

                foreach (MixAndMatchDiscount mixAndMatch in possibleMixAndMatches)
                {
                    if (mixAndMatch.CanApplyStandalone(itemsWithOverlappingDiscounts, itemsWithOverlappingDiscountsCompoundedOnly))
                    {
                        this.CalculateOneMixAndMatchLeastExpensiveFavorRetailer(
                            transaction,
                            mixAndMatch,
                            discountableItemGroups,
                            ref bestDiscountPath,
                            ref bestDiscountValue,
                            remainingQuantities,
                            remainingQuantitiesForCompound,
                            itemCompoundedOfferQuantityLookup);
                    }
                }

                possibleMixAndMatches.RemoveAll(p => p.IsFinished);

                // Overlapped mix and match discounts (least expensive favoring retailers)
                if (possibleMixAndMatches.Any())
                {
                    possibleMixAndMatches.Sort(MixAndMatchDiscount.GetComparisonForLeastExpensiveFavorRetailer());

                    foreach (MixAndMatchDiscount mixAndMatch in possibleMixAndMatches)
                    {
                        this.CalculateOneMixAndMatchLeastExpensiveFavorRetailer(
                            transaction,
                            mixAndMatch,
                            discountableItemGroups,
                            ref bestDiscountPath,
                            ref bestDiscountValue,
                            remainingQuantities,
                            remainingQuantitiesForCompound,
                            itemCompoundedOfferQuantityLookup);
                    }
                }
            }

            private void CalculateOneMixAndMatchLeastExpensiveFavorRetailer(
                SalesTransaction transaction,
                MixAndMatchDiscount mixAndMatch,
                DiscountableItemGroup[] discountableItemGroups,
                ref List<AppliedDiscountApplication> bestDiscountPath,
                ref decimal bestDiscountValue,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompound,
                ItemCompoundedOfferQuantityLookup itemCompoundedOfferQuantityLookup)
            {
                decimal[] remainingQuantitiesToUser = mixAndMatch.CanCompound ? remainingQuantitiesForCompound : remainingQuantities;

                IEnumerable<DiscountApplication> discountApplications = mixAndMatch.GetDiscountApplicationsForLeastExpensiveFavorRetailer(
                    transaction,
                    discountableItemGroups,
                    remainingQuantitiesToUser,
                    bestDiscountPath);

                if (discountApplications.Any())
                {
#if DEBUG
                    Debug.WriteLine("CalculateOneMixAndMatchLeastExpensiveFavorRetailer returned {0} discount applications for discount {1}.", discountApplications.Count(), mixAndMatch.OfferName);
#endif

                    this.ApplyStandaloneDiscountApplications(
                        discountApplications,
                        discountableItemGroups,
                        remainingQuantities,
                        remainingQuantitiesForCompound,
                        itemCompoundedOfferQuantityLookup,
                        ref bestDiscountPath,
                        ref bestDiscountValue);

                    mixAndMatch.IsFinished = true;
                }
            }

            /// <summary>
            /// Calculates non-threshold discounts for a transaction and discountable item groups.
            /// </summary>
            /// <param name="transaction">The current transaction.</param>
            /// <param name="possibleDiscounts">Possible discounts.</param>
            /// <param name="discountableItemGroups">The discountable items on the transaction.</param>
            /// <param name="bestDiscountPath">The current best discount path.</param>
            /// <param name="bestDiscountValue">The current best discount value.</param>
            /// <param name="remainingQuantities">The remaining quantities of each item in the discountableItemGroups parameter.</param>
            /// <param name="remainingQuantitiesForCompound">The remaining quantities for compounded discounts.</param>
            /// <param name="itemCompoundedOfferQuantityLookup">Item group index to compounded offer to quantity lookup.</param>
            private void CalculateNonThresholdDiscounts(
                SalesTransaction transaction,
                List<DiscountBase> possibleDiscounts,
                DiscountableItemGroup[] discountableItemGroups,
                ref List<AppliedDiscountApplication> bestDiscountPath,
                ref decimal bestDiscountValue,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompound,
                ItemCompoundedOfferQuantityLookup itemCompoundedOfferQuantityLookup)
            {
                using (SimpleProfiler profiler = new SimpleProfiler("DiscountCalcuator.CalculateNonThresholdDiscounts"))
                {
                    if (!possibleDiscounts.Any())
                    {
                        return;
                    }

                    HashSet<int> itemsWithOverlappingDiscounts = new HashSet<int>();
                    HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly = new HashSet<int>();
                    Dictionary<int, List<DiscountBase>> itemGroupIndexToDiscountsLookup = new Dictionary<int, List<DiscountBase>>();
                    this.OptimizeAndApplyNonOverlappedDiscountsBestDeal(
                        transaction,
                        discountableItemGroups,
                        ref bestDiscountPath,
                        ref bestDiscountValue,
                        remainingQuantities,
                        remainingQuantitiesForCompound,
                        itemCompoundedOfferQuantityLookup,
                        possibleDiscounts,
                        itemsWithOverlappingDiscounts,
                        itemsWithOverlappingDiscountsCompoundedOnly,
                        itemGroupIndexToDiscountsLookup);

                    this.ApplyNonOverlappingDiscountsOptimized(
                        possibleDiscounts,
                        transaction,
                        discountableItemGroups,
                        remainingQuantities,
                        remainingQuantitiesForCompound,
                        itemCompoundedOfferQuantityLookup,
                        itemsWithOverlappingDiscounts,
                        itemsWithOverlappingDiscountsCompoundedOnly,
                        ref bestDiscountPath,
                        ref bestDiscountValue);

                    this.ApplyOverlappingDiscountsOptimized(
                        possibleDiscounts,
                        transaction,
                        discountableItemGroups,
                        remainingQuantities,
                        remainingQuantitiesForCompound,
                        itemCompoundedOfferQuantityLookup,
                        itemsWithOverlappingDiscounts,
                        itemsWithOverlappingDiscountsCompoundedOnly,
                        ref bestDiscountPath,
                        ref bestDiscountValue);
                }
            }

            private void CalculateThresholdDiscounts(
                SalesTransaction transaction,
                List<DiscountBase> possibleDiscounts,
                DiscountableItemGroup[] discountableItemGroups,
                ref List<AppliedDiscountApplication> bestDiscountPath,
                ref decimal bestDiscountValue,
                decimal discountValue,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompound,
                ItemCompoundedOfferQuantityLookup itemCompoundedOfferQuantityLookup)
            {
                using (SimpleProfiler profiler = new SimpleProfiler("DiscountCalcuator.CalculateExclusiveThresholdDiscounts"))
                {
                    if (!possibleDiscounts.Any())
                    {
                        return;
                    }

                    // itemsWithOverlappingDiscounts doesn't really matter for threshold.
                    HashSet<int> itemsWithOverlappingDiscounts = new HashSet<int>();
                    HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly = new HashSet<int>();
                    Dictionary<int, List<DiscountBase>> itemGroupIndexToDiscountsLookup = new Dictionary<int, List<DiscountBase>>();

                    BuildItemsWithOverlappingDiscounts(
                        itemsWithOverlappingDiscounts,
                        itemsWithOverlappingDiscountsCompoundedOnly,
                        itemGroupIndexToDiscountsLookup,
                        possibleDiscounts);
                    List<DiscountApplication> discountApplications = new List<DiscountApplication>();
                    bool isInterrupted = false;
                    foreach (DiscountBase discount in possibleDiscounts)
                    {
                        decimal[] quantitites = discount.CanCompound ? remainingQuantitiesForCompound : remainingQuantities;
                        IEnumerable<DiscountApplication> discountApplicationsForOne = discount.GetDiscountApplications(
                            transaction,
                            discountableItemGroups,
                            quantitites,
                            this.priceContext,
                            bestDiscountPath,
                            itemsWithOverlappingDiscounts,
                            out isInterrupted);

                        if (isInterrupted)
                        {
                            break;
                        }

                        discountApplications.AddRange(discountApplicationsForOne);
                    }

                    if (!isInterrupted)
                    {
                        DiscountApplication[] discountApplicationsSorted = discountApplications.OrderBy(p => p.Discount.ConcurrencyMode)
                                                                                    .ThenByDescending(p => p.SortIndex)
                                                                                    .ThenByDescending(p => p.SortValue)
                                                                                    .ToArray();
#if DEBUG
                        WriteDebugInfoForDiscountApplications("Threshold discount applications", discountApplicationsSorted, discountableItemGroups);
#endif

                        if (discountApplicationsSorted.Length > 0)
                        {
                            isInterrupted = this.CalculateBestDiscountApplications(
                                discountableItemGroups,
                                ref bestDiscountPath,
                                ref bestDiscountValue,
                                discountApplicationsSorted,
                                discountValue,
                                remainingQuantities,
                                remainingQuantitiesForCompound);

#if DEBUG
                            SetDebugDataOnTransaction(transaction, GetStepCountKeyForThreshold(), this.stepCount);
                            string stepCountKey = GetStepCountKeyForThreshold(possibleDiscounts[0].PricingPriorityNumber, possibleDiscounts[0].ConcurrencyMode == ConcurrencyMode.Exclusive);
                            SetDebugDataOnTransaction(transaction, stepCountKey, this.stepCount);
                            if (!isInterrupted)
                            {
                                string finishedExhaustiveAlgorithmKey = GetFinishedExhaustiveAlgorithmKeyForThreshold(possibleDiscounts[0].PricingPriorityNumber, possibleDiscounts[0].ConcurrencyMode == ConcurrencyMode.Exclusive);
                                SetDebugDataOnTransaction(transaction, finishedExhaustiveAlgorithmKey, true);
                            }
#endif
                        }
                    }

                    if (isInterrupted)
                    {
                        this.ApplyThresholdsFast(
                            transaction,
                            possibleDiscounts,
                            discountableItemGroups,
                            remainingQuantities,
                            remainingQuantitiesForCompound,
                            itemCompoundedOfferQuantityLookup,
                            itemsWithOverlappingDiscounts,
                            itemsWithOverlappingDiscountsCompoundedOnly,
                            ref bestDiscountPath,
                            ref bestDiscountValue);

#if DEBUG
                        // DISCOUNTPERF: Fix tests
                        string appliedMarginalValueAlgorithmKey = GetAppliedMarginalValueAlgorithmKeyForThreshold(possibleDiscounts[0].PricingPriorityNumber, possibleDiscounts[0].ConcurrencyMode == ConcurrencyMode.Exclusive);
                        SetDebugDataOnTransaction(transaction, appliedMarginalValueAlgorithmKey, true);
#endif
                    }
                }
            }

            private void ApplyThresholdsFast(
                SalesTransaction transaction,
                IList<DiscountBase> possibleDiscounts,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompound,
                ItemCompoundedOfferQuantityLookup itemCompoundedOfferQuantityLookup,
                HashSet<int> itemsWithOverlappingDiscounts,
                HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly,
                ref List<AppliedDiscountApplication> bestDiscountPath,
                ref decimal bestDiscountValue)
            {
                DiscountBase[] discountsSorted = ThresholdDiscount.GetSortedDiscountsToApplyInFastMode(
                    possibleDiscounts,
                    discountableItemGroups,
                    remainingQuantities,
                    remainingQuantitiesForCompound,
                    itemsWithOverlappingDiscounts,
                    itemsWithOverlappingDiscountsCompoundedOnly);

                for (int i = 0; i < discountsSorted.Length; i++)
                {
                    DiscountBase discount = discountsSorted[i];

#if DEBUG
                    discount.DebugDiscount();
#endif

                    bool isGetDiscountApplicationsInterrupted;

                    // Use remaining quantities only from the covered group to get discount applications.
                    IEnumerable<DiscountApplication> fastDiscountApplications = discount.GetDiscountApplications(
                        transaction,
                        discountableItemGroups,
                        discount.CanCompound ? remainingQuantitiesForCompound : remainingQuantities,
                        this.priceContext,
                        bestDiscountPath,
                        itemsWithOverlappingDiscounts,
                        out isGetDiscountApplicationsInterrupted);

                    foreach (DiscountApplication discountApplication in fastDiscountApplications)
                    {
                        discountApplication.ApplyStandalone = true;
                    }

                    if (fastDiscountApplications.Any())
                    {
                        // Apply the discount applications using remaining quantities for the whole transaction.
                        this.ApplyStandaloneDiscountApplications(
                            fastDiscountApplications,
                            discountableItemGroups,
                            remainingQuantities,
                            remainingQuantitiesForCompound,
                            itemCompoundedOfferQuantityLookup,
                            ref bestDiscountPath,
                            ref bestDiscountValue);
                    }

                    discount.CleanupLookups();
                }
            }

            private List<DiscountBase> GetPossibleDiscountsForLineItems(
                SalesTransaction transaction,
                IEnumerable<long> storePriceGroups,
                string currencyCode,
                bool isReturn,
                PriceContext priceContext,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompound,
                bool isThreshold)
            {
                List<DiscountBase> possibleDiscounts = new List<DiscountBase>();
                foreach (KeyValuePair<string, DiscountBase> pair in this.offerIdToDiscountsLookup)
                {
                    DiscountBase discount = pair.Value;
                    if ((isThreshold && discount.PeriodicDiscountType == PeriodicDiscountOfferType.Threshold) ||
                        (!isThreshold && discount.PeriodicDiscountType != PeriodicDiscountOfferType.Threshold))
                    {
                        if (discount.CanDiscountApply(transaction, storePriceGroups, currencyCode, isReturn, priceContext))
                        {
                            foreach (int itemIndexGroup in pair.Value.ItemGroupIndexToDiscountLineNumberSetMap.Keys)
                            {
                                decimal[] quantities = discount.CanCompound ? remainingQuantitiesForCompound : remainingQuantities;
                                if (quantities[itemIndexGroup] > decimal.Zero)
                                {
                                    possibleDiscounts.Add(pair.Value);
                                    break;
                                }
                            }
                        }
                    }
                }

                return possibleDiscounts;
            }

            // Non Overlapped means those products which have just 1 applicable discount.
            private void OptimizeAndApplyNonOverlappedDiscountsBestDeal(
                SalesTransaction transaction,
                DiscountableItemGroup[] discountableItemGroups,
                ref List<AppliedDiscountApplication> bestDiscountPath,
                ref decimal bestDiscountValue,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompound,
                ItemCompoundedOfferQuantityLookup itemCompoundedOfferQuantityLookup,
                List<DiscountBase> possibleDiscounts,
                HashSet<int> itemsWithOverlappingDiscounts,
                HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly,
                Dictionary<int, List<DiscountBase>> itemGroupIndexToDiscountsLookup)
            {
                if (!possibleDiscounts.Any())
                {
                    return;
                }

#if DEBUG
                int priority = possibleDiscounts[0].PricingPriorityNumber;
                bool isExclusive = possibleDiscounts[0].ConcurrencyMode == ConcurrencyMode.Exclusive;
#endif

                BuildItemsWithOverlappingDiscounts(
                    itemsWithOverlappingDiscounts,
                    itemsWithOverlappingDiscountsCompoundedOnly,
                    itemGroupIndexToDiscountsLookup,
                    possibleDiscounts);

                // Step 1: pre-optimization and remove unqualified discounts.
                using (SimpleProfiler profilerPreOptimize = new SimpleProfiler("DiscountCalcuator.OptimizeAndApplyNonOverlappedDiscounts.PreOptimization"))
                {
                    foreach (DiscountBase discount in possibleDiscounts)
                    {
                        discount.PreOptimization(
                            discountableItemGroups,
                            discount.CanCompound ? remainingQuantitiesForCompound : remainingQuantities,
                            itemsWithOverlappingDiscounts,
                            itemsWithOverlappingDiscountsCompoundedOnly);
                    }
                }

                if (RemoveFinishedDiscounts(possibleDiscounts))
                {
#if DEBUG
                    SetDebugDataOnTransaction(transaction, RemovedUnqualifiedDiscountsKey, true);
#endif

                    BuildItemsWithOverlappingDiscounts(
                        itemsWithOverlappingDiscounts,
                        itemsWithOverlappingDiscountsCompoundedOnly,
                        itemGroupIndexToDiscountsLookup,
                        possibleDiscounts);

                    // If some discounts are removed, then we may optimize quantity discount further.
                    foreach (DiscountBase discount in possibleDiscounts)
                    {
                        if (discount is MultipleBuyDiscount)
                        {
                            discount.PreOptimization(
                                discountableItemGroups,
                                discount.CanCompound ? remainingQuantitiesForCompound : remainingQuantities,
                                itemsWithOverlappingDiscounts,
                                itemsWithOverlappingDiscountsCompoundedOnly);
                        }
                    }
                }

#if DEBUG
                DiscountBase.DebugDiscounts("Optimized possible discounts", possibleDiscounts);
#endif

                // Step 2: reduce overlapped discount offers and quantity discounts per item.
                if (ReduceOverlappedOfferAndQuantityDiscountsPerItem(
                    discountableItemGroups,
                    remainingQuantities,
                    itemGroupIndexToDiscountsLookup,
                    itemsWithOverlappingDiscounts))
                {
                    RemoveFinishedDiscounts(possibleDiscounts);

                    BuildItemsWithOverlappingDiscounts(
                        itemsWithOverlappingDiscounts,
                        itemsWithOverlappingDiscountsCompoundedOnly,
                        itemGroupIndexToDiscountsLookup,
                        possibleDiscounts);
                }

#if DEBUG
                DiscountBase.DebugDiscounts("Reduced", possibleDiscounts);
#endif

                // Step 3: apply non-overlapping discounts with best deal.
                if (this.ApplyNonOverlappingDiscountsWithBestDeal(
                    possibleDiscounts,
                    transaction,
                    discountableItemGroups,
                    remainingQuantities,
                    remainingQuantitiesForCompound,
                    itemCompoundedOfferQuantityLookup,
                    itemsWithOverlappingDiscounts,
                    itemsWithOverlappingDiscountsCompoundedOnly,
                    ref bestDiscountPath,
                    ref bestDiscountValue))
                {
                    BuildItemsWithOverlappingDiscounts(
                        itemsWithOverlappingDiscounts,
                        itemsWithOverlappingDiscountsCompoundedOnly,
                        itemGroupIndexToDiscountsLookup,
                        possibleDiscounts);

                    // One more time.
                    // For example, we may finish off compounded quantity discount, now we can work on compounded mix and match.
                    this.ApplyNonOverlappingDiscountsWithBestDeal(
                        possibleDiscounts,
                        transaction,
                        discountableItemGroups,
                        remainingQuantities,
                        remainingQuantitiesForCompound,
                        itemCompoundedOfferQuantityLookup,
                        itemsWithOverlappingDiscounts,
                        itemsWithOverlappingDiscountsCompoundedOnly,
                        ref bestDiscountPath,
                        ref bestDiscountValue);
                }

                BuildItemsWithOverlappingDiscounts(
                    itemsWithOverlappingDiscounts,
                    itemsWithOverlappingDiscountsCompoundedOnly,
                    itemGroupIndexToDiscountsLookup,
                    possibleDiscounts);

#if DEBUG
                SetDebugDataOnTransaction(transaction, GetAllOptimizedForBestDealKeyForPriority(priority, isExclusive), !possibleDiscounts.Any());
#endif

                // Later
                // Step 4: apply non-overlapped discounts, which may be optimized for speed, i.e. plan b.
                // Step 5: apply overlapped discounts, which again may be optimized for speed, i.e. plan b.
            }

#if DEBUG
            private void WriteDebugInfoSummary(DateTime startDateTime)
            {
                Debug.WriteLine("Starts [{0:MM/dd/yyyy hh:mm:ss.fff}] Ends [{1:MM/dd/yyyy hh:mm:ss.fff}] Paths {2} Steps {3}.", startDateTime, DateTime.UtcNow, this.pathCount, this.stepCount);
            }

            private void WriteDebugInfoForPath(
                DebugLogHeader header,
                decimal newAmount,
                decimal discountValue,
                Stack<AppliedDiscountApplication> appliedDiscountsStack,
                Stack<DiscountApplicationState> discountApplicationStateStack,
                int nextIndex = 0)
            {
                if (header == DebugLogHeader.Path)
                {
                    this.pathCount++;
                }
                else
                {
                    this.stepCount++;
                }

                if (LogDiscountDetails)
                {
                    Debug.WriteLine("[{0}] next index {3} got value of {1} with new amount {2} for path: ", header, discountValue, newAmount, nextIndex);

                    List<AppliedDiscountApplication> appliedDiscountsList = appliedDiscountsStack.ToList();
                    appliedDiscountsList.Reverse();

                    AppliedDiscountApplication.DebugAppliedDiscountApplicationList(appliedDiscountsList);

                    Debug.WriteLine("Application state stack: ");

                    List<DiscountApplicationState> applicationStates = discountApplicationStateStack.ToList();
                    applicationStates.Reverse();

                    foreach (DiscountApplicationState applicationState in applicationStates)
                    {
                        Debug.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "  App [{0}], # [{1}], v [{2:0.##}], r q: ", applicationState.AppliedDiscountApplication, applicationState.NumberOfTimesApplied, applicationState.Value));
                        for (int x = 0; x < applicationState.RemainingQuantities.Length; x++)
                        {
                            if (applicationState.RemainingQuantities[x] != 0M)
                            {
                                Debug.WriteLine("  " + applicationState.RemainingQuantities[x] + "x" + x + ", ");
                            }
                        }

                        Debug.WriteLine(string.Empty);
                    }
                }
            }
#endif
        }
    }
}
