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
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// This class implements the threshold (by amount) discount processing, including the determination of which ways
        /// the discount can apply to the transaction and the value of the discount applied to specific lines.
        /// </summary>
        public class ThresholdDiscount : DiscountBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ThresholdDiscount" /> class.
            /// </summary>
            /// <param name="validationPeriod">Validation period.</param>
            public ThresholdDiscount(ValidationPeriod validationPeriod)
                : base(validationPeriod)
            {
                this.ThresholdDiscountTiers = new List<ThresholdDiscountTier>();
            }

            /// <summary>
            /// Gets the threshold discount tiers for this discount.
            /// </summary>
            public IList<ThresholdDiscountTier> ThresholdDiscountTiers { get; private set; }

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
                ThrowIf.Null(discountableItemGroups, "discountableItemGroups");
                ThrowIf.Null(remainingQuantities, "remainingQuantities");

                // Get the discount code to use for any discount lines, if one is required.
                string discountCodeUsed = this.GetDiscountCodeForDiscount(transaction);
                List<DiscountApplication> applications = new List<DiscountApplication>();
                isInterrupted = false;

                // Set of lines from the given discount which correspond to all items in the cart.
                HashSet<RetailDiscountLine> lines = new HashSet<RetailDiscountLine>();

                // This variable holds total amount from all items (quantities are taken into the account) and will be used towards end of the function
                // to figure out whether we have a tier in this discount which can be triggered for the given items in the cart.
                decimal totalAmountForTriggeredLines = 0M;

                // Iterating over every item in the cart (the items are groupped by quantity).
                for (int x = 0; x < discountableItemGroups.Length; x++)
                {
                    DiscountableItemGroup item = discountableItemGroups[x];
                    IList<RetailDiscountLine> triggeredLines = null;

                    // See if this discount contains a line for the given product.
                    this.TryGetRetailDiscountLines(item.ProductId, item.MasterProductId, item.SalesOrderUnitOfMeasure, out triggeredLines);

                    // As an optimization, we get the total amount here for each product or variant, so that we can determine if this discount is possible.
                    if (triggeredLines.Count > 0)
                    {
                        // The discount line was found therefore adding the corresponding item's amount to the total amount.
                        totalAmountForTriggeredLines += item.Price * remainingQuantities[x];
                    }

                    foreach (RetailDiscountLine line in triggeredLines)
                    {
                        // Need to avoid adding duplicates which could take place if a line on discount is defined as a category
                        // and that category covers more than 1 item in the cart.
                        if (!lines.Contains(line))
                        {
                            lines.Add(line);
                        }
                    }
                }

                HashSet<string> discountPriceGroups = new HashSet<string>(ConvertPriceDiscountGroupIdsToGroups(this.PriceDiscountGroupIds, priceContext));

                // Adding amount for possible non discountable items if they can participate in threshold discount calculation.
                totalAmountForTriggeredLines += this.GetThresholdAmountFromDiscountedItems(
                    discountableItemGroups,
                    appliedDiscounts,
                    discountPriceGroups,
                    priceContext);

                // Figuring out whether the discount's definition contains a tier which will be triggered given items in the cart.
                // In other words we are figuring out whether it will be possible to apply this discount against the cart or not.
                if (lines.Count > 0 && this.ThresholdDiscountTiers.Any(p => p.AmountThreshold <= totalAmountForTriggeredLines))
                {
                    List<RetailDiscountLineItem> retailDiscountLines = new List<RetailDiscountLineItem>(lines.Count);

                    foreach (RetailDiscountLine line in lines)
                    {
                        retailDiscountLines.Add(new RetailDiscountLineItem(InvalidIndex, line));
                    }

                    DiscountApplication result = new DiscountApplication(this)
                    {
                        RetailDiscountLines = retailDiscountLines,
                        SortIndex = -1,
                        SortValue = GetSortValue(lines.First()),
                        DiscountCode = discountCodeUsed
                    };

                    applications.Add(result);
                }

                return applications;
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
                // Not applicable.
                return new List<DiscountApplication>();
            }

            /// <summary>
            /// Gets the value of the specified discount application on the transaction, possibly including previously applied discounts.
            /// </summary>
            /// <param name="discountableItemGroups">The transaction line items.</param>
            /// <param name="remainingQuantities">The quantities remaining for each item.</param>
            /// <param name="appliedDiscounts">The previously applied discounts.</param>
            /// <param name="discountApplication">The specific application of the discount to use.</param>
            /// <param name="priceContext">The pricing context to use.</param>
            /// <returns>The value of the discount application.</returns>
            public override AppliedDiscountApplication GetAppliedDiscountApplication(
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                IEnumerable<AppliedDiscountApplication> appliedDiscounts,
                DiscountApplication discountApplication,
                PriceContext priceContext)
            {
                decimal totalAmount = 0M;

                // Return null and do not use this discount if any of the parameters are null or if another threshold discount has already been applied.
                if (discountableItemGroups == null ||
                    discountApplication == null ||
                    remainingQuantities == null ||
                    appliedDiscounts.Any(p => p.DiscountApplication.Discount.PeriodicDiscountType == PeriodicDiscountOfferType.Threshold && string.Equals(p.DiscountApplication.Discount.OfferId, this.OfferId, StringComparison.OrdinalIgnoreCase)))
                {
                    return null;
                }

                decimal[] itemPrices = new decimal[discountableItemGroups.Length];

                HashSet<int> discountedItems = new HashSet<int>();

                // Need to get the total amount for the lines that are covered by this discount.
                // The value will be used later to find the best applicable tier.
                totalAmount = this.GetTotalThresholdAmountForCoveredLines(
                    discountableItemGroups,
                    remainingQuantities,
                    appliedDiscounts,
                    itemPrices,
                    discountedItems,
                    priceContext);

                // Now that we have the total and the item prices, calculate the amount.
                ThresholdDiscountTier tier =
                    this.ThresholdDiscountTiers.Where(p => p.AmountThreshold <= Math.Abs(totalAmount))
                        .OrderByDescending(p => p.AmountThreshold)
                        .FirstOrDefault();

                Dictionary<int, decimal> appliedQuantities = new Dictionary<int, decimal>();

                // Since we have already calculated totalAmount it means that the items were already "taken" or "used" and we need to reflect
                // that by initializing appliedQuantities.
                foreach (int itemIndex in discountedItems)
                {
                    appliedQuantities[itemIndex] = remainingQuantities[itemIndex];
                }

                // Just total price for the items covered by this discount without any discount applied.
                decimal totalPriceToBeDiscounted = discountedItems.Sum(x => itemPrices[x] * appliedQuantities[x]);

                decimal value = decimal.Zero;

                // The tier could be null, if, for instance we have 2 competing TH discount and one of them was already processed earlier
                // This will result that this (2nd discount) will not have enough quantities and as a result the totalAmount will not be able
                // to accumulate enough value to find any tier even if the tier exists from discount definition point of view.
                if (tier != null)
                {
                    // Calculating the amount of the discount based on Discount Method.
                    if (tier.DiscountMethod == ThresholdDiscountMethod.AmountOff)
                    {
                        // If item's price is smaller than discount we return just item price which will mean 
                        // that the item is for free. 
                        // The check for Min is needed because otherwise we would end-up owing money to a customer.
                        value = Math.Min(tier.DiscountValue, totalPriceToBeDiscounted);
                    }
                    else if (tier.DiscountMethod == ThresholdDiscountMethod.PercentOff)
                    {
                        value = totalPriceToBeDiscounted * tier.DiscountValue / 100m;
                    }
                }

                AppliedDiscountApplication currentDiscountApplication = null;

                if (value > decimal.Zero)
                {
                    currentDiscountApplication = new AppliedDiscountApplication(discountApplication, value, appliedQuantities, totalAmount, itemPrices, false);
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

                using (SimpleProfiler profiler = new SimpleProfiler("ThresholdDiscount.GenerateDiscountLines", 2))
                {
                    appliedDiscountApplication.ItemGroupIndexToDiscountLineQuantitiesLookup = this.GenerateDiscountLinesDictionary(
                        discountableItemGroups,
                        appliedDiscountApplication,
                        priceContext);
                }
            }

            internal static DiscountBase[] GetSortedDiscountsToApplyInFastMode(
              IList<DiscountBase> unsortedDiscounts,
              DiscountableItemGroup[] discountableItemGroups,
              decimal[] remainingQuantities,
              decimal[] remainingQuantitiesForCompound,
              HashSet<int> itemsWithOverlappingDiscounts,
              HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly)
            {
                using (SimpleProfiler profiler = new SimpleProfiler("ThresholdDiscounts.GetDiscountsToApplyInFastMode", 2))
                {
                    Dictionary<string, DiscountDealEstimate> offerIdToEstimateNonCompoundedLookup = new Dictionary<string, DiscountDealEstimate>(StringComparer.OrdinalIgnoreCase);

                    // Consolidate all compounded discounts into one estimate, to be sorted with the rest later.
                    List<DiscountBase> compoundedDiscounts = new List<DiscountBase>();
                    DiscountDealEstimate combinedEstimateForCompounded = null;

                    Dictionary<string, DiscountBase> offerIdtoDiscountLookup = unsortedDiscounts.ToDictionary(d => d.OfferId);

                    combinedEstimateForCompounded = BuildEstimates(
                        offerIdToEstimateNonCompoundedLookup,
                        compoundedDiscounts,
                        offerIdtoDiscountLookup,
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
                    DiscountBase[] discountsSorted = new DiscountBase[unsortedDiscounts.Count];
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

                            if (offerIdtoDiscountLookup.TryGetValue(estimate.OfferId, out discount))
                            {
                                discountsSorted[discountIndex] = discount;
                                discountIndex++;
                            }
                        }
                    }

                    return discountsSorted;
                }
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
                return new List<DiscountApplication>();
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
                    throw new ArgumentNullException(nameof(discountableItemGroups));
                }

                if (remainingQuantities == null)
                {
                    throw new ArgumentNullException(nameof(remainingQuantities));
                }

                if (itemsWithOverlappingDiscounts == null)
                {
                    throw new ArgumentNullException(nameof(itemsWithOverlappingDiscounts));
                }

                if (itemsWithOverlappingDiscountsCompoundedOnly == null)
                {
                    throw new ArgumentNullException(nameof(itemsWithOverlappingDiscounts));
                }

                decimal totalApplicableQuantityWithOverlapped = decimal.Zero;
                decimal totalDiscountAmountWithOverlapped = decimal.Zero;
                decimal totalApplicableQuantityWithoutOverlapped = decimal.Zero;
                decimal totalDiscountAmountWithoutOverlapped = decimal.Zero;
                Dictionary<int, decimal> itemGroupIndexToQuantityNeededFromOverlappedLookup = new Dictionary<int, decimal>();

                decimal quantityWithOverlappedTemp = 0M;
                decimal quantityWithoutOverlappedTemp = 0M;

                decimal transactionPriceAmountWithOverlapped = 0M;
                decimal transactionPriceAmountWithoutOverlapped = 0M;

                // Iterating over every item in the cart (the items are groupped by quantity)
                // to find out total transaction's price amount for all covered (by this discount) items
                // the results are stored in totalTransactionPriceAmountWithOverlapped and totalTransactionPriceAmountWithoutOverlapped which will be analyzed later
                // to find out whether we have a triggering threshold for them.
                for (int itemIndex = 0; itemIndex < discountableItemGroups.Length; itemIndex++)
                {
                    DiscountableItemGroup item = discountableItemGroups[itemIndex];
                    IList<RetailDiscountLine> triggeredLines = null;

                    decimal quantity = remainingQuantities[itemIndex];
                    if (quantity > 0)
                    {
                        // See if this discount contains a line for the given product.
                        this.TryGetRetailDiscountLines(item.ProductId, item.MasterProductId, item.SalesOrderUnitOfMeasure, out triggeredLines);
                        if (triggeredLines.Count > 0)
                        {
                            decimal effectivePrice = item.Price * quantity;

                            // The discount line was found therefore adding the corresponding item's amount to the accumulators.
                            transactionPriceAmountWithOverlapped += effectivePrice;
                            quantityWithOverlappedTemp += quantity;

                            if (this.IsItemIndexGroupOverlappedWithNonCompoundedDiscounts(itemIndex, itemsWithOverlappingDiscounts, itemsWithOverlappingDiscountsCompoundedOnly))
                            {
                                itemGroupIndexToQuantityNeededFromOverlappedLookup[itemIndex] = quantity;
                            }
                            else
                            {
                                quantityWithoutOverlappedTemp += quantity;
                                transactionPriceAmountWithoutOverlapped += effectivePrice;
                            }
                        }
                    }
                }

                // Now looking for the best tier (and also finding out whether a tier exists at all given the threshold)
                // a) for items with overlappings
                // b) for items without overlappings
                ThresholdDiscountTier tierWithOverlapping =
                    this.ThresholdDiscountTiers.Where(p => p.AmountThreshold <= Math.Abs(transactionPriceAmountWithOverlapped))
                        .OrderByDescending(p => p.AmountThreshold)
                        .FirstOrDefault();

                ThresholdDiscountTier tierWithoutOverlapping =
                    this.ThresholdDiscountTiers.Where(p => p.AmountThreshold <= Math.Abs(transactionPriceAmountWithoutOverlapped))
                        .OrderByDescending(p => p.AmountThreshold)
                        .FirstOrDefault();

                // Getting discount value taking into account the discount method (Amount or Percentage).
                decimal discountValueWithOverlapping;
                if (tierWithOverlapping != null)
                {
                    discountValueWithOverlapping = tierWithOverlapping.DiscountMethod == ThresholdDiscountMethod.AmountOff ? tierWithOverlapping.DiscountValue : DiscountBase.GetDiscountAmountForPercentageOff(transactionPriceAmountWithOverlapped, tierWithOverlapping.DiscountValue);
                }
                else
                {
                    discountValueWithOverlapping = 0M;
                }

                decimal discountValueWithoutOverlapping;
                if (tierWithoutOverlapping != null)
                {
                    discountValueWithoutOverlapping = tierWithoutOverlapping.DiscountMethod == ThresholdDiscountMethod.AmountOff ? tierWithoutOverlapping.DiscountValue : DiscountBase.GetDiscountAmountForPercentageOff(transactionPriceAmountWithOverlapped, tierWithoutOverlapping.DiscountValue);
                }
                else
                {
                    discountValueWithoutOverlapping = 0M;
                }

                totalApplicableQuantityWithOverlapped = (tierWithOverlapping != null) ? quantityWithOverlappedTemp : 0M;
                totalDiscountAmountWithOverlapped = discountValueWithOverlapping;

                totalApplicableQuantityWithoutOverlapped = (tierWithoutOverlapping != null) ? quantityWithoutOverlappedTemp : 0M;
                totalDiscountAmountWithoutOverlapped = discountValueWithoutOverlapping;

                DiscountDealEstimate estimate = new DiscountDealEstimate(
                    this.CanCompound,
                    this.OfferId,
                    totalApplicableQuantityWithOverlapped,
                    totalDiscountAmountWithOverlapped,
                    totalApplicableQuantityWithoutOverlapped,
                    totalDiscountAmountWithoutOverlapped,
                    itemGroupIndexToQuantityNeededFromOverlappedLookup);

                // itemGroupIndexToQuantityNeededFromOverlappedLookup could be empty if all threshold discounts are compounded which means we don't care about overlapped discounts
                // because even if they exist they will not reduce quantities available for other compounded discounts.
                return estimate;
            }

            private static DiscountDealEstimate BuildEstimates(
                Dictionary<string, DiscountDealEstimate> offerIdToEstimateNonCompoundedLookupHolder,
                List<DiscountBase> compoundedDiscountsHolder,
                Dictionary<string, DiscountBase> discounts,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                decimal[] remainingQuantitiesForCompound,
                HashSet<int> itemsWithOverlappingDiscounts,
                HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly)
            {
                using (SimpleProfiler profiler = new SimpleProfiler("OverlappedDiscounts.BuildEstimates", 2))
                {
                    DiscountDealEstimate combinedEstimateForCompounded = null;
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

            /// <summary>
            /// Gets the sort value to use for the specified discount line.  For threshold discounts, this is either the value of the discount or zero for the percentage.
            /// </summary>
            /// <param name="discountLine">The discount line triggered for this application.</param>
            /// <returns>The sort value.</returns>
            private static decimal GetSortValue(RetailDiscountLine discountLine)
            {
                switch ((DiscountOfferMethod)discountLine.DiscountMethod)
                {
                    case DiscountOfferMethod.DiscountAmount:
                        return discountLine.DiscountAmount;
                    case DiscountOfferMethod.DiscountPercent:
                        return 0;
                    case DiscountOfferMethod.OfferPrice:
                        return discountLine.DiscountLinePercentOrValue;
                    default:
                        return 0;
                }
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
                if (priceContext == null)
                {
                    throw new ArgumentNullException("priceContext");
                }

                Dictionary<int, IList<DiscountLineQuantity>> result = new Dictionary<int, IList<DiscountLineQuantity>>();

                if (discountableItemGroups == null || discountApplicationToApply == null)
                {
                    return result;
                }

                decimal totalAmount = discountApplicationToApply.TotalAmountForCoveredLines;
                decimal[] itemPrices = discountApplicationToApply.ItemPrices;

                // Now that we have the total and the item prices, calculate the discount lines for each discountable item.
                ThresholdDiscountTier tier =
                    this.ThresholdDiscountTiers.Where(p => p.AmountThreshold <= Math.Abs(totalAmount))
                        .OrderByDescending(p => p.AmountThreshold)
                        .FirstOrDefault();

                if (tier != null)
                {
                    if (tier.DiscountMethod == ThresholdDiscountMethod.AmountOff)
                    {
                        decimal totalPrice = discountApplicationToApply.ItemQuantities.Sum(x => itemPrices[x.Key] * x.Value);
                        decimal discountAmount = Math.Min(tier.DiscountValue, totalPrice);

                        //// We have to deal with rounding for $-off threshold discount to make sure total discount matches what's defined on the tier.
                        //// 1. Avoid split, i.e. if last one to process has fractional quantity or quantity = 1 (smallest unit).
                        //// 2. Avoid overallocation before the last one, e.g. $2 to 2 items: A of $10 with qty = 101, and B of $10 with qty = 1.
                        ////    If we process A first, then unit amount = $2 / 102 rounded to $0.02, so A got $2.02, more than it's allowed.
                        ////    In this case, we have to process A last, which will involve split A into 2 lines.
                        const int InitialItemIndex = -1;
                        int itemIndexSmallestIntegerQuantity = InitialItemIndex;
                        int itemIndexLargestNetPriceFractionalQuantity = InitialItemIndex;
                        int itemIndexLargestNetPrice = InitialItemIndex;

                        decimal largestNetPriceFractionalQuantity = decimal.Zero;
                        decimal largestNetPrice = decimal.Zero;
                        decimal smallestIntegerQuantity = decimal.Zero;
                        decimal smallestIntegerPrice = decimal.Zero;

                        // Step 1: get item index with smallest integer quantity and item index with largest fraction quantity net price.
                        foreach (KeyValuePair<int, decimal> pair in discountApplicationToApply.ItemQuantities)
                        {
                            int itemGroupIndex = pair.Key;
                            decimal quantity = pair.Value;

                            if (itemPrices[itemGroupIndex] > decimal.Zero)
                            {
                                if (DiscountableItemGroup.IsFraction(quantity))
                                {
                                    if (itemIndexLargestNetPriceFractionalQuantity == InitialItemIndex || (itemPrices[itemGroupIndex] * quantity) > largestNetPriceFractionalQuantity)
                                    {
                                        itemIndexLargestNetPriceFractionalQuantity = itemGroupIndex;
                                        largestNetPriceFractionalQuantity = itemPrices[itemGroupIndex] * quantity;
                                    }
                                }
                                else
                                {
                                    if (itemIndexSmallestIntegerQuantity == InitialItemIndex ||
                                        quantity < smallestIntegerQuantity ||
                                        (quantity == smallestIntegerQuantity && itemPrices[itemGroupIndex] > smallestIntegerPrice))
                                    {
                                        itemIndexSmallestIntegerQuantity = itemGroupIndex;
                                        smallestIntegerQuantity = quantity;
                                        smallestIntegerPrice = itemPrices[itemGroupIndex];
                                    }
                                }

                                if (largestNetPrice == InitialItemIndex || (itemPrices[itemGroupIndex] * quantity) > largestNetPrice)
                                {
                                    itemIndexLargestNetPrice = itemGroupIndex;
                                    largestNetPrice = itemPrices[itemGroupIndex] * quantity;
                                }
                            }
                        }

                        int itemIndexLastOneToProcess = largestNetPriceFractionalQuantity > (smallestIntegerQuantity * smallestIntegerPrice) ? itemIndexLargestNetPriceFractionalQuantity : itemIndexSmallestIntegerQuantity;
                        int itemIndexSecondLastOneToProcess = itemIndexLargestNetPrice;

                        // Step 2: allocate discount amount to all except two items with largest net price and either largest fraction net price or smallest integer quantity.
                        foreach (KeyValuePair<int, decimal> pair in discountApplicationToApply.ItemQuantities)
                        {
                            int itemGroupIndex = pair.Key;
                            decimal quantityApplied = pair.Value;

                            if (itemGroupIndex != itemIndexLastOneToProcess && itemGroupIndex != itemIndexSecondLastOneToProcess)
                            {
                                decimal unitDiscountAmount = priceContext.CurrencyAndRoundingHelper.Round(discountAmount * itemPrices[itemGroupIndex] / totalPrice);
                                decimal thisDiscountAmount = priceContext.CurrencyAndRoundingHelper.Round(unitDiscountAmount * quantityApplied);
                                totalPrice -= itemPrices[itemGroupIndex] * quantityApplied;
                                discountAmount -= thisDiscountAmount;
                                this.AddAmountOffDiscountLineToDictionary(
                                    discountApplicationToApply,
                                    discountableItemGroups,
                                    result,
                                    itemGroupIndex,
                                    unitDiscountAmount,
                                    quantityApplied);
                            }
                        }

                        // Step 3: try allocating discount amount the item with largest net price, if discount overflows, move it to the last.
                        if (itemIndexSecondLastOneToProcess != itemIndexLastOneToProcess && discountApplicationToApply.ItemQuantities.ContainsKey(itemIndexSecondLastOneToProcess))
                        {
                            decimal quantityApplied = discountApplicationToApply.ItemQuantities[itemIndexSecondLastOneToProcess];
                            decimal unitDiscountAmount = priceContext.CurrencyAndRoundingHelper.Round(discountAmount * itemPrices[itemIndexSecondLastOneToProcess] / totalPrice);
                            decimal thisDiscountAmount = priceContext.CurrencyAndRoundingHelper.Round(unitDiscountAmount * quantityApplied);

                            if (thisDiscountAmount > discountAmount && discountApplicationToApply.ItemQuantities.ContainsKey(itemIndexLastOneToProcess))
                            {
                                int itemIndexSwap = itemIndexLastOneToProcess;
                                itemIndexLastOneToProcess = itemIndexSecondLastOneToProcess;
                                itemIndexSecondLastOneToProcess = itemIndexSwap;
                            }

                            // Step 4: process the second one to last.
                            quantityApplied = discountApplicationToApply.ItemQuantities[itemIndexSecondLastOneToProcess];
                            unitDiscountAmount = priceContext.CurrencyAndRoundingHelper.Round(discountAmount * itemPrices[itemIndexSecondLastOneToProcess] / totalPrice);
                            thisDiscountAmount = priceContext.CurrencyAndRoundingHelper.Round(unitDiscountAmount * quantityApplied);
                            totalPrice -= itemPrices[itemIndexSecondLastOneToProcess] * quantityApplied;
                            discountAmount -= thisDiscountAmount;
                            this.AddAmountOffDiscountLineToDictionary(
                                discountApplicationToApply,
                                discountableItemGroups,
                                result,
                                itemIndexSecondLastOneToProcess,
                                unitDiscountAmount,
                                quantityApplied);
                        }

                        // Step 5: dole out the remaining to the last one
                        if (discountAmount > decimal.Zero && discountApplicationToApply.ItemQuantities.ContainsKey(itemIndexLastOneToProcess))
                        {
                            decimal quantityToApply = discountApplicationToApply.ItemQuantities[itemIndexLastOneToProcess];
                            if (DiscountableItemGroup.IsFraction(quantityToApply))
                            {
                                // Last one is fractional quantity, dump leftover fully by not rounding unit discount amount.
                                decimal unitDiscountAmount = discountAmount / quantityToApply;
                                this.AddAmountOffDiscountLineToDictionary(discountApplicationToApply, discountableItemGroups, result, itemIndexLastOneToProcess, unitDiscountAmount, quantityToApply);
                            }
                            else
                            {
                                // Last one is integer quantity, split discount amount into 2 groups.
                                decimal price = itemPrices[itemIndexLastOneToProcess];
                                decimal smallestAmount = PriceContextHelper.GetSmallestNonNegativeAmount(priceContext, Math.Min(discountAmount, price));
                                if (smallestAmount > decimal.Zero && quantityToApply > decimal.Zero)
                                {
                                    int totalDiscountAmountInSmallestAmount = (int)(discountAmount / smallestAmount);
                                    int averageDiscountAmountRoundingDownInSmallestAmount = totalDiscountAmountInSmallestAmount / (int)quantityToApply;
                                    decimal quantityForHigherDiscountAmount = totalDiscountAmountInSmallestAmount - (averageDiscountAmountRoundingDownInSmallestAmount * (int)quantityToApply);
                                    decimal quantityForLowerDiscountAmount = quantityToApply - quantityForHigherDiscountAmount;
                                    decimal unitDiscountAmountForLowerDiscountAmount = averageDiscountAmountRoundingDownInSmallestAmount * smallestAmount;

                                    if (quantityForHigherDiscountAmount > decimal.Zero)
                                    {
                                        this.AddAmountOffDiscountLineToDictionary(
                                            discountApplicationToApply,
                                            discountableItemGroups,
                                            result,
                                            itemIndexLastOneToProcess,
                                            unitDiscountAmountForLowerDiscountAmount + smallestAmount,
                                            quantityForHigherDiscountAmount);
                                    }

                                    if (quantityForLowerDiscountAmount > decimal.Zero && unitDiscountAmountForLowerDiscountAmount > decimal.Zero)
                                    {
                                        this.AddAmountOffDiscountLineToDictionary(
                                            discountApplicationToApply,
                                            discountableItemGroups,
                                            result,
                                            itemIndexLastOneToProcess,
                                            unitDiscountAmountForLowerDiscountAmount,
                                            quantityForLowerDiscountAmount);
                                    }
                                }
                            }
                        }
                    }
                    else if (tier.DiscountMethod == ThresholdDiscountMethod.PercentOff)
                    {
                        foreach (KeyValuePair<int, decimal> pair in discountApplicationToApply.ItemQuantities)
                        {
                            int itemGroupIndex = pair.Key;
                            decimal quantity = pair.Value;
                            DiscountLine discountItem = this.NewDiscountLine(discountApplicationToApply.DiscountApplication.DiscountCode, discountableItemGroups[itemGroupIndex].ItemId);
                            discountItem.Percentage = Math.Max(decimal.Zero, Math.Min(tier.DiscountValue, 100m));
                            discountItem.Amount = decimal.Zero;

                            result.Add(itemGroupIndex, new List<DiscountLineQuantity>() { new DiscountLineQuantity(discountItem, quantity) });
                        }
                    }
                }

                return result;
            }

            private void AddAmountOffDiscountLineToDictionary(
                AppliedDiscountApplication discountApplicationToApply,
                DiscountableItemGroup[] discountableItemGroups,
                Dictionary<int, IList<DiscountLineQuantity>> itemIndexToDiscountLinesDictionary,
                int itemIndex,
                decimal unitDiscountAmount,
                decimal quantityApplied)
            {
                DiscountLine discountItem = this.NewDiscountLine(discountApplicationToApply.DiscountApplication.DiscountCode, discountableItemGroups[itemIndex].ItemId);
                discountItem.Amount = unitDiscountAmount;
                discountItem.Percentage = 0;

                IList<DiscountLineQuantity> discountLineQuantityList = null;
                if (itemIndexToDiscountLinesDictionary.TryGetValue(itemIndex, out discountLineQuantityList))
                {
                    discountLineQuantityList.Add(new DiscountLineQuantity(discountItem, quantityApplied));
                }
                else
                {
                    itemIndexToDiscountLinesDictionary.Add(itemIndex, new List<DiscountLineQuantity>() { new DiscountLineQuantity(discountItem, quantityApplied) });
                }
            }

            /// <summary>
            /// Gets the total amount on the transaction for the lines that may be used by this threshold discount.
            /// </summary>
            /// <param name="discountableItemGroups">The discountable item groups on the transaction.</param>
            /// <param name="remainingQuantities">The remaining quantities of the item groups.</param>
            /// <param name="appliedDiscounts">The previously applied discounts.</param>
            /// <param name="itemPrices">The average prices of items on the transaction after previously applied compounding discounts.</param>
            /// <param name="discountedItems">The items that will be discounted by this application of the threshold discount.</param>
            /// <param name="priceContext">The pricing context to use.</param>
            /// <returns>The total amount for the lines specified.</returns>
            private decimal GetTotalThresholdAmountForCoveredLines(
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                IEnumerable<AppliedDiscountApplication> appliedDiscounts,
                decimal[] itemPrices,
                ISet<int> discountedItems,
                PriceContext priceContext)
            {
                HashSet<string> discountPriceGroups = new HashSet<string>(ConvertPriceDiscountGroupIdsToGroups(this.PriceDiscountGroupIds, priceContext));
                decimal totalThresholdAmount = 0;
                Dictionary<int, IList<DiscountLineQuantity>> itemGroupIndexToDiscountLineQuantitiesLookup = new Dictionary<int, IList<DiscountLineQuantity>>();
                if (this.CanCompound)
                {
                    this.InitializeDiscountDictionary(appliedDiscounts, itemGroupIndexToDiscountLineQuantitiesLookup, false);
                }

                for (int x = 0; x < discountableItemGroups.Length; x++)
                {
                    if (remainingQuantities[x] != 0M
                        && (this.ShouldCountNonDiscountItems || DiscountBase.IsDiscountAllowedForDiscountableItemGroup(discountableItemGroups[x]))
                        && DiscountBase.IsDiscountAllowedForCatalogIds(priceContext, discountPriceGroups, discountableItemGroups[x].CatalogIds)
                        && this.ItemGroupIndexToDiscountLineNumberSetMap.ContainsKey(x))
                    {
                        itemPrices[x] = discountableItemGroups[x].Price;

                        // If this is a compounding discount, we need to determine any previous discounts that could have applied to it, unless it is an amount discount.
                        if (this.CanCompound)
                        {
                            // Determine the value of the previously applied concurrent discounts so that we can get the proper value here.
                            // We group them together here to get the correct amount when there is a quantity greater than one and a percentage discount.
                            if (itemGroupIndexToDiscountLineQuantitiesLookup.ContainsKey(x))
                            {
                                List<List<DiscountLineQuantity>> sortedConcurrentDiscounts =
                                    itemGroupIndexToDiscountLineQuantitiesLookup[x].GroupBy(p => p.DiscountLine.OfferId)
                                                         .OrderByDescending(p => p.First().DiscountLine.Amount)
                                                         .Select(concurrentDiscountGroup => concurrentDiscountGroup.ToList())
                                                         .ToList();

                                foreach (List<DiscountLineQuantity> concurrentDiscount in sortedConcurrentDiscounts)
                                {
                                    decimal startingPrice = itemPrices[x];

                                    foreach (DiscountLineQuantity discLine in concurrentDiscount)
                                    {
                                        itemPrices[x] -= (((startingPrice * discLine.DiscountLine.Percentage / 100M) + discLine.DiscountLine.Amount) * discLine.Quantity) / Math.Abs(discountableItemGroups[x].Quantity);
                                    }
                                }
                            }
                        }

                        itemPrices[x] = Math.Max(decimal.Zero, itemPrices[x]);
                        totalThresholdAmount += remainingQuantities[x] * itemPrices[x];

                        if (DiscountBase.IsDiscountAllowedForDiscountableItemGroup(discountableItemGroups[x]))
                        {
                            discountedItems.Add(x);
                        }
                    }
                }

                totalThresholdAmount += this.GetThresholdAmountFromDiscountedItems(
                    discountableItemGroups,
                    appliedDiscounts,
                    discountPriceGroups,
                    priceContext);

                return totalThresholdAmount;
            }

            private decimal GetThresholdAmountFromDiscountedItems(
                DiscountableItemGroup[] discountableItemGroups,
                IEnumerable<AppliedDiscountApplication> appliedDiscounts,
                HashSet<string> discountPriceGroups,
                PriceContext priceContext)
            {
                // If count non discountable items, we should add items discounted by exclusive/best-price non threshold discounts.
                decimal thresholdAmountFromDiscountedItems = decimal.Zero;
                if (this.ShouldCountNonDiscountItems)
                {
                    foreach (AppliedDiscountApplication appliedDiscountApplication in appliedDiscounts)
                    {
                        DiscountBase discount = appliedDiscountApplication.DiscountApplication.Discount;
                        if (!discount.CanCompound && discount.PeriodicDiscountType != PeriodicDiscountOfferType.Threshold)
                        {
                            foreach (KeyValuePair<int, IList<DiscountLineQuantity>> pair in appliedDiscountApplication.ItemGroupIndexToDiscountLineQuantitiesLookup)
                            {
                                int itemGroupIndex = pair.Key;

                                // The item has to be qualified for the threshold discount first.
                                if (this.ItemGroupIndexToDiscountLineNumberSetMap.ContainsKey(itemGroupIndex) &&
                                    DiscountBase.IsDiscountAllowedForCatalogIds(priceContext, discountPriceGroups, discountableItemGroups[itemGroupIndex].CatalogIds))
                                {
                                    decimal price = discountableItemGroups[itemGroupIndex].Price;
                                    foreach (DiscountLineQuantity discountLineQuantity in pair.Value)
                                    {
                                        if (discountLineQuantity.DiscountLine.Amount > decimal.Zero)
                                        {
                                            thresholdAmountFromDiscountedItems += discountLineQuantity.Quantity * Math.Max(decimal.Zero, price - discountLineQuantity.DiscountLine.Amount);
                                        }
                                        else if (discountLineQuantity.DiscountLine.Percentage > decimal.Zero)
                                        {
                                            thresholdAmountFromDiscountedItems += discountLineQuantity.Quantity * Math.Max(decimal.Zero, price * (1m - (discountLineQuantity.DiscountLine.Percentage / 100m)));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return thresholdAmountFromDiscountedItems;
            }
        }
    }
}
