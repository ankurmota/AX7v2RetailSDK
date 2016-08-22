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
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// This class manages compounded quantities.
        /// 1. Within non-threshold discounts, no compound across priorities.
        /// 2. Within threshold discounts, no compound across priorities.
        /// 3. Threshold discounts can be compounded on top of non-threshold discount regardless.
        /// </summary>
        /// <remarks>
        /// The instance is stateful with additional data after each round for the following two private lookups:
        ///     itemGroupIndexToPriorityToSettledCompoundedQuantityForNonThreshold
        ///     itemGroupIndexToPriorityToSettledCompoundedQuantityForThreshold
        /// See DiscountCalculator.CalculateAndApplyBestDiscounts and DiscountCalculator.SetAppliedDiscountApplicationStackAndReduceRemainingQuantities.
        /// </remarks>
        internal class ItemCompoundedOfferQuantityLookup
        {
            private Dictionary<int, Dictionary<int, decimal>> itemGroupIndexToPriorityToSettledCompoundedQuantityForNonThreshold;
            private Dictionary<int, Dictionary<int, decimal>> itemGroupIndexToPriorityToSettledCompoundedQuantityForThreshold;

            internal ItemCompoundedOfferQuantityLookup()
            {
                this.itemGroupIndexToPriorityToSettledCompoundedQuantityForNonThreshold = new Dictionary<int, Dictionary<int, decimal>>();
                this.itemGroupIndexToPriorityToSettledCompoundedQuantityForThreshold = new Dictionary<int, Dictionary<int, decimal>>();
                this.ItemGroupIndexToCompoundedOfferToQuantityLookup = new Dictionary<int, Dictionary<string, decimal>>();
            }

            /// <summary>
            /// Gets item group index to compounded offer to quantity lookup.
            /// </summary>
            /// <remarks>
            /// It's a work-in-progress lookup for the current round. At the beginning of each round, it's empty.
            /// </remarks>
            internal Dictionary<int, Dictionary<string, decimal>> ItemGroupIndexToCompoundedOfferToQuantityLookup { get; private set; }

            /// <summary>
            /// Refreshes item compounded quantity lookup after each round of calculating discounts
            ///     of exclusive or non-exclusive non-thresholds discounts, of the same priority number.
            /// Prepares remaining quantities for the next round.
            /// </summary>
            /// <param name="bestDiscountPath">Best discount path of application discount applications so far.</param>
            /// <param name="lastPriorityNumber">Priority number of the last round.</param>
            /// <param name="remainingQuantities">Remaining quantities for the next round.</param>
            /// <param name="remainingQuantitiesForCompound">Remaining quantities for compound for the next round.</param>
            /// <param name="reserveCompoundedQuantityForLeastExpensiveFavorRetailer">
            /// If it's followed by least expensive favoring retail of the same priority, then reserve the compounded quantity of the same priority.
            /// </param>
            internal void RefreshLookupAndPrepareRemainingQuantitiesForNonThresholds(
                List<AppliedDiscountApplication> bestDiscountPath,
                int lastPriorityNumber,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompound,
                bool reserveCompoundedQuantityForLeastExpensiveFavorRetailer)
            {
                // Step 1: fill in this.ItemGroupIndexToCompoundedOfferToQuantityLookup from the last round.
                //         reduce remaining quantitied for non-compounded discounts.
                this.ItemGroupIndexToCompoundedOfferToQuantityLookup.Clear();
                foreach (AppliedDiscountApplication appliedDiscountApplication in bestDiscountPath)
                {
                    DiscountBase discount = appliedDiscountApplication.DiscountApplication.Discount;

                    if (discount.CanCompound)
                    {
                        // We've already processed the lookups for the previous round. Here we only processed the most recent round.
                        // Ignore mix and match least expensive favoring retailer
                        MixAndMatchDiscount mixAndMatch = discount as MixAndMatchDiscount;
                        if (discount.PricingPriorityNumber == lastPriorityNumber &&
                            discount.PeriodicDiscountType != PeriodicDiscountOfferType.Threshold &&
                            (mixAndMatch == null || mixAndMatch.DiscountType != DiscountMethodType.LeastExpensive || mixAndMatch.LeastExpensiveMode == LeastExpensiveMode.FavorCustomer))
                        {
                            this.FillLookup(appliedDiscountApplication);
                        }
                    }
                    else
                    {
                        ReduceQuantitiesFromNonCompounded(appliedDiscountApplication, remainingQuantities, remainingQuantitiesForCompound);
                    }
                }

                // Step 2: move the data to settled lookups.
                this.MoveToSettledCompoundedQuantityLookup(lastPriorityNumber, this.itemGroupIndexToPriorityToSettledCompoundedQuantityForNonThreshold);

                int? priorityNumberToSkipForCompounded = null;
                if (reserveCompoundedQuantityForLeastExpensiveFavorRetailer)
                {
                    priorityNumberToSkipForCompounded = lastPriorityNumber;
                }

                this.ReduceQuantitiesFromCompoundedForNonThresholds(
                    remainingQuantities,
                    remainingQuantitiesForCompound,
                    priorityNumberToSkipForCompounded);

                // Clear current lookup.
                this.ItemGroupIndexToCompoundedOfferToQuantityLookup.Clear();
            }

            /// <summary>
            /// Refreshes item compounded quantity lookup after each round of calculating discounts
            ///     of exclusive or non-exclusive least-expensive-favor-retail discounts, of the same priority number.
            /// Prepares remaining quantities for the next round.
            /// </summary>
            /// <param name="bestDiscountPath">Best discount path of application discount applications so far.</param>
            /// <param name="lastPriorityNumber">Priority number of the last round.</param>
            /// <param name="remainingQuantities">Remaining quantities for the next round.</param>
            /// <param name="remainingQuantitiesForCompound">Remaining quantities for compound for the next round.</param>
            internal void RefreshLookupAndPrepareRemainingQuantitiesForLeastExpensiveFavoRetailer(
                List<AppliedDiscountApplication> bestDiscountPath,
                int lastPriorityNumber,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompound)
            {
                // Step 1: fill in this.ItemGroupIndexToCompoundedOfferToQuantityLookup from the last round.
                //         reduce remaining quantitied for non-compounded discounts.
                this.ItemGroupIndexToCompoundedOfferToQuantityLookup.Clear();
                foreach (AppliedDiscountApplication appliedDiscountApplication in bestDiscountPath)
                {
                    DiscountBase discount = appliedDiscountApplication.DiscountApplication.Discount;

                    if (discount.CanCompound)
                    {
                        // We've already processed the lookups for the previous round. Here we only processed the most recent round.
                        // Only mix and match least expensive favoring retailer
                        MixAndMatchDiscount mixAndMatch = discount as MixAndMatchDiscount;
                        if (discount.PricingPriorityNumber == lastPriorityNumber &&
                            (mixAndMatch != null && mixAndMatch.DiscountType == DiscountMethodType.LeastExpensive && mixAndMatch.LeastExpensiveMode == LeastExpensiveMode.FavorRetailer))
                        {
                            this.FillLookup(appliedDiscountApplication);
                        }
                    }
                    else
                    {
                        ReduceQuantitiesFromNonCompounded(appliedDiscountApplication, remainingQuantities, remainingQuantitiesForCompound);
                    }
                }

                // Step 2: move the data to settled lookups.
                foreach (KeyValuePair<int, Dictionary<string, decimal>> pair in this.ItemGroupIndexToCompoundedOfferToQuantityLookup)
                {
                    int itemGroupIndex = pair.Key;
                    Dictionary<string, decimal> offerIdToQuantityLookup = pair.Value;
                    decimal compoundedQuantity = offerIdToQuantityLookup.Max(p => p.Value);
                    AddToOrUpdateItemGroupIndexToPriorityToSettledCompoundedQuantityLookup(
                        itemGroupIndex,
                        lastPriorityNumber,
                        compoundedQuantity,
                        this.itemGroupIndexToPriorityToSettledCompoundedQuantityForNonThreshold);
                }

                this.ReduceQuantitiesFromCompoundedForNonThresholds(
                    remainingQuantities,
                    remainingQuantitiesForCompound,
                    null);

                // Clear current lookup.
                this.ItemGroupIndexToCompoundedOfferToQuantityLookup.Clear();
            }

            /// <summary>
            /// Prepares remaining quantities for thresholds for the first time.
            /// </summary>
            /// <param name="bestDiscountPath">Best discount path of application discount applications so far.</param>
            /// <param name="remainingQuantities">Remaining quantities for the next round.</param>
            /// <param name="remainingQuantitiesForCompound">Remaining quantities for compound for the next round.</param>
            /// <remarks>Threshold can be compounded on top on non-thresholds, so we have to reset remaining quantities before we evaluate threshold discounts.</remarks>
            internal void PrepareRemainingQuantitiesForThresholdsFirstTime(
                List<AppliedDiscountApplication> bestDiscountPath,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompound)
            {
                // Step 1: fill in this.ItemGroupIndexToCompoundedOfferToQuantityLookup from the last round.
                //         reduce remaining quantitied for non-compounded discounts.
                this.ItemGroupIndexToCompoundedOfferToQuantityLookup.Clear();
                foreach (AppliedDiscountApplication appliedDiscountApplication in bestDiscountPath)
                {
                    DiscountBase discount = appliedDiscountApplication.DiscountApplication.Discount;
                    if (!discount.CanCompound)
                    {
                        ReduceQuantitiesFromNonCompounded(appliedDiscountApplication, remainingQuantities, remainingQuantitiesForCompound);
                    }
                }

                // Finish off items with threshold discounts.
                foreach (KeyValuePair<int, Dictionary<int, decimal>> pair in this.itemGroupIndexToPriorityToSettledCompoundedQuantityForThreshold)
                {
                    int itemGroupIndex = pair.Key;

                    // Compounded threshold always take the whole item.
                    remainingQuantities[itemGroupIndex] = decimal.Zero;
                    remainingQuantitiesForCompound[itemGroupIndex] = decimal.Zero;
                }

                // Process non-threshold compounded quantities.
                this.ReduceQuantitiesFromCompoundedNonThresholdForThreshold(remainingQuantities);

                // Clear current lookup.
                this.ItemGroupIndexToCompoundedOfferToQuantityLookup.Clear();
            }

            /// <summary>
            /// Refreshes item compounded quantity lookup after each round of calculating discounts
            ///     of exclusive or non-exclusive thresholds discounts, of the same priority number.
            /// Prepares remaining quantities for the next round.
            /// </summary>
            /// <param name="bestDiscountPath">Best discount path of application discount applications so far.</param>
            /// <param name="lastPriorityNumber">Priority number of the last round.</param>
            /// <param name="remainingQuantities">Remaining quantities for the next round.</param>
            /// <param name="remainingQuantitiesForCompound">Remaining quantities for compound for the next round.</param>
            internal void RefreshLookupAndPrepareRemainingQuantitiesForThresholds(
                List<AppliedDiscountApplication> bestDiscountPath,
                int lastPriorityNumber,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompound)
            {
                // Step 1: fill in this.ItemGroupIndexToCompoundedOfferToQuantityLookup from the last round.
                //         reduce remaining quantitied for non-compounded discounts.
                this.ItemGroupIndexToCompoundedOfferToQuantityLookup.Clear();
                foreach (AppliedDiscountApplication appliedDiscountApplication in bestDiscountPath)
                {
                    DiscountBase discount = appliedDiscountApplication.DiscountApplication.Discount;

                    if (discount.CanCompound)
                    {
                        // We've already processed the lookups for the previous round. Here we only processed the most recent round.
                        if (discount.PricingPriorityNumber == lastPriorityNumber &&
                            discount.PeriodicDiscountType == PeriodicDiscountOfferType.Threshold)
                        {
                            this.FillLookup(appliedDiscountApplication);
                        }
                    }
                    else
                    {
                        ReduceQuantitiesFromNonCompounded(appliedDiscountApplication, remainingQuantities, remainingQuantitiesForCompound);
                    }
                }

                // Step 2: move the data to settled lookups.
                this.MoveToSettledCompoundedQuantityLookup(lastPriorityNumber, this.itemGroupIndexToPriorityToSettledCompoundedQuantityForThreshold);

                // Step 3: figure out remain quantities for threshold evaluation.
                this.ReduceQuantitiesFromCompoundedThresholdsForThresholds(remainingQuantities, remainingQuantitiesForCompound);
                this.ReduceQuantitiesFromCompoundedNonThresholdForThreshold(remainingQuantities);

                // Clear current lookup.
                this.ItemGroupIndexToCompoundedOfferToQuantityLookup.Clear();
            }

            internal void FillLookup(
                DiscountBase discount,
                int itemGroupIndex,
                decimal quantity)
            {
                // Build the lookup from itemGroupId to OfferId to Quantity.
                Dictionary<string, decimal> offerIdToQuantityLookup = null;
                if (this.ItemGroupIndexToCompoundedOfferToQuantityLookup.TryGetValue(itemGroupIndex, out offerIdToQuantityLookup))
                {
                    decimal existingQuantity = decimal.Zero;
                    if (offerIdToQuantityLookup.TryGetValue(discount.OfferId, out existingQuantity))
                    {
                        offerIdToQuantityLookup[discount.OfferId] = quantity + existingQuantity;
                    }
                    else
                    {
                        offerIdToQuantityLookup.Add(discount.OfferId, quantity);
                    }
                }
                else
                {
                    offerIdToQuantityLookup = new Dictionary<string, decimal>();
                    offerIdToQuantityLookup.Add(discount.OfferId, quantity);
                    this.ItemGroupIndexToCompoundedOfferToQuantityLookup.Add(itemGroupIndex, offerIdToQuantityLookup);
                }
            }

            internal void FillLookup(AppliedDiscountApplication appliedDiscountApplication)
            {
                foreach (KeyValuePair<int, decimal> pair in appliedDiscountApplication.ItemQuantities)
                {
                    int itemGroupIndex = pair.Key;
                    decimal quantity = pair.Value;

                    this.FillLookup(appliedDiscountApplication.DiscountApplication.Discount, itemGroupIndex, quantity);
                }
            }

            internal decimal GetCompoundedQuantityForNonThreshold(int itemGroupIndex)
            {
                decimal compoundedQuantity = decimal.Zero;
                Dictionary<int, decimal> priorityToSettledCompoundedQuantityForNonThreshold = null;
                if (this.itemGroupIndexToPriorityToSettledCompoundedQuantityForNonThreshold.TryGetValue(itemGroupIndex, out priorityToSettledCompoundedQuantityForNonThreshold))
                {
                    compoundedQuantity = priorityToSettledCompoundedQuantityForNonThreshold.Sum(p => p.Value);
                }

                return compoundedQuantity;
            }

            internal decimal GetCompoundedQuantityForThreshold(int itemGroupIndex)
            {
                decimal compoundedQuantity = decimal.Zero;
                Dictionary<int, decimal> priorityToSettledCompoundedQuantityForThreshold = null;
                if (this.itemGroupIndexToPriorityToSettledCompoundedQuantityForThreshold.TryGetValue(itemGroupIndex, out priorityToSettledCompoundedQuantityForThreshold))
                {
                    compoundedQuantity = priorityToSettledCompoundedQuantityForThreshold.Sum(p => p.Value);
                }

                return compoundedQuantity;
            }

            internal decimal GetCurrentCompoundedQuantity(int itemGroupIndex)
            {
                Dictionary<string, decimal> offerIdToQuantityLookup = null;
                decimal compoundedQuantity = decimal.Zero;
                if (this.ItemGroupIndexToCompoundedOfferToQuantityLookup.TryGetValue(itemGroupIndex, out offerIdToQuantityLookup))
                {
                    if (offerIdToQuantityLookup.Any())
                    {
                        compoundedQuantity = offerIdToQuantityLookup.Max(p => p.Value);
                    }
                }

                return compoundedQuantity;
            }

            private static void AddToItemGroupIndexToPriorityToSettledCompoundedQuantityLookup(
                int itemGroupIndex,
                int priorityNumber,
                decimal compoundedQuantity,
                Dictionary<int, Dictionary<int, decimal>> itemGroupIndexToPriorityToSettledCompoundedQuantity)
            {
                Dictionary<int, decimal> priorityToSettledCompoundedQuantity = null;
                if (!itemGroupIndexToPriorityToSettledCompoundedQuantity.TryGetValue(itemGroupIndex, out priorityToSettledCompoundedQuantity))
                {
                    priorityToSettledCompoundedQuantity = new Dictionary<int, decimal>();
                    itemGroupIndexToPriorityToSettledCompoundedQuantity.Add(itemGroupIndex, priorityToSettledCompoundedQuantity);
                }

                priorityToSettledCompoundedQuantity.Add(priorityNumber, compoundedQuantity);
            }

            private static void AddToOrUpdateItemGroupIndexToPriorityToSettledCompoundedQuantityLookup(
                int itemGroupIndex,
                int priorityNumber,
                decimal compoundedQuantity,
                Dictionary<int, Dictionary<int, decimal>> itemGroupIndexToPriorityToSettledCompoundedQuantity)
            {
                Dictionary<int, decimal> priorityToSettledCompoundedQuantity = null;
                decimal existingCompoundedQuantity = decimal.Zero;
                if (itemGroupIndexToPriorityToSettledCompoundedQuantity.TryGetValue(itemGroupIndex, out priorityToSettledCompoundedQuantity))
                {
                    // If we already have item->priority in the lookup, then update the quantity with the max of new and existing.
                    priorityToSettledCompoundedQuantity.TryGetValue(priorityNumber, out existingCompoundedQuantity);
                }
                else
                {
                    priorityToSettledCompoundedQuantity = new Dictionary<int, decimal>();
                    itemGroupIndexToPriorityToSettledCompoundedQuantity.Add(itemGroupIndex, priorityToSettledCompoundedQuantity);
                }

                priorityToSettledCompoundedQuantity[priorityNumber] = Math.Max(compoundedQuantity, existingCompoundedQuantity);
            }

            private static void ReduceQuantitiesFromNonCompounded(
                AppliedDiscountApplication appliedDiscountApplication,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompound)
            {
                // Non-compounded. Just reduce the quantities.
                foreach (KeyValuePair<int, decimal> pair in appliedDiscountApplication.ItemQuantities)
                {
                    int itemGroupIndex = pair.Key;
                    decimal quantity = pair.Value;
                    remainingQuantities[itemGroupIndex] -= quantity;
                    remainingQuantitiesForCompound[itemGroupIndex] -= quantity;
                }
            }

            /// <summary>
            /// Reduce quantity from applied compounded discounts for remaining non thresholds.
            /// </summary>
            /// <param name="remainingQuantities">Remaining quantities.</param>
            /// <param name="remainingQuantitiesForCompounded">Remaining quantities for compounded.</param>
            /// <param name="priorityNumberToSkipForCompounded">Do not reduce compounded quantity of this priority number, for least expensive favoring retailer.</param>
            private void ReduceQuantitiesFromCompoundedForNonThresholds(
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompounded,
                int? priorityNumberToSkipForCompounded)
            {
                // Process compounded quantities by reducing highest quantity for each offer.
                foreach (KeyValuePair<int, Dictionary<int, decimal>> pair in this.itemGroupIndexToPriorityToSettledCompoundedQuantityForNonThreshold)
                {
                    int itemGroupIndex = pair.Key;
                    decimal compoundedQuantityToReduce = decimal.Zero;
                    decimal compoundedQuantityToReduceForQuantititesCompounded = decimal.Zero;
                    foreach (KeyValuePair<int, decimal> priorityCompoundeQuantityPair in pair.Value)
                    {
                        decimal priority = priorityCompoundeQuantityPair.Key;
                        decimal compoundedQuantityForPriority = priorityCompoundeQuantityPair.Value;
                        compoundedQuantityToReduce += compoundedQuantityForPriority;

                        // We decided to run mix and matches of least expensive favoring retail after other non-threshold discounts,
                        // and the special least expensive mix and matches can compound on top of other non-threshold discounts of the same priority.
                        if (priorityNumberToSkipForCompounded == null || priorityNumberToSkipForCompounded.Value != priority)
                        {
                            compoundedQuantityToReduceForQuantititesCompounded += compoundedQuantityForPriority;
                        }
                    }

                    // We can't compound across priorities.
                    remainingQuantities[itemGroupIndex] = Math.Max(decimal.Zero, remainingQuantities[itemGroupIndex] - compoundedQuantityToReduce);
                    remainingQuantitiesForCompounded[itemGroupIndex] = Math.Max(decimal.Zero, remainingQuantitiesForCompounded[itemGroupIndex] - compoundedQuantityToReduceForQuantititesCompounded);
                }
            }

            private void ReduceQuantitiesFromCompoundedNonThresholdForThreshold(decimal[] remainingQuantities)
            {
                // Process non-threshold compounded quantities.
                foreach (KeyValuePair<int, Dictionary<int, decimal>> pair in this.itemGroupIndexToPriorityToSettledCompoundedQuantityForNonThreshold)
                {
                    int itemGroupIndex = pair.Key;

                    if (!this.itemGroupIndexToPriorityToSettledCompoundedQuantityForThreshold.ContainsKey(itemGroupIndex))
                    {
                        decimal compoundedQuantity = pair.Value.Sum(p => p.Value);

                        // Leave out remainingQuantitiesForCompound for threshold to compound.
                        remainingQuantities[itemGroupIndex] = Math.Max(decimal.Zero, remainingQuantities[itemGroupIndex] - compoundedQuantity);
                    }
                }
            }

            private void ReduceQuantitiesFromCompoundedThresholdsForThresholds(
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompound)
            {
                foreach (KeyValuePair<int, Dictionary<int, decimal>> pair in this.itemGroupIndexToPriorityToSettledCompoundedQuantityForThreshold)
                {
                    int itemGroupIndex = pair.Key;

                    // Compounded threshold always take the whole item.
                    remainingQuantities[itemGroupIndex] = decimal.Zero;
                    remainingQuantitiesForCompound[itemGroupIndex] = decimal.Zero;
                }
            }

            private void MoveToSettledCompoundedQuantityLookup(
                int lastPriorityNumber,
                Dictionary<int, Dictionary<int, decimal>> itemGroupIndexToPriorityToSettledCompoundedQuantity)
            {
                foreach (KeyValuePair<int, Dictionary<string, decimal>> pair in this.ItemGroupIndexToCompoundedOfferToQuantityLookup)
                {
                    int itemGroupIndex = pair.Key;
                    Dictionary<string, decimal> offerIdToQuantityLookup = pair.Value;
                    decimal compoundedQuantity = offerIdToQuantityLookup.Max(p => p.Value);
                    AddToItemGroupIndexToPriorityToSettledCompoundedQuantityLookup(
                        itemGroupIndex,
                        lastPriorityNumber,
                        compoundedQuantity,
                        itemGroupIndexToPriorityToSettledCompoundedQuantity);
                }
            }
        }
    }
}
