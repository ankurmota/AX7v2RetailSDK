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
        /// This class implements the standard (single-line) discount processing, including the determination of which ways
        /// the discount can apply to the transaction and the value of the discount applied to specific lines.
        /// </summary>
        public class OfferDiscount : DiscountBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="OfferDiscount" /> class.
            /// </summary>
            /// <param name="validationPeriod">Validation period.</param>
            public OfferDiscount(ValidationPeriod validationPeriod)
                : base(validationPeriod)
            {
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
                if (discountableItemGroups == null)
                {
                    throw new ArgumentNullException("discountableItemGroups");
                }
    
                if (remainingQuantities == null)
                {
                    throw new ArgumentNullException("remainingQuantities");
                }
    
                if (priceContext == null)
                {
                    throw new ArgumentNullException("priceContext");
                }
    
                List<DiscountApplication> discountAppliations = new List<DiscountApplication>();
                isInterrupted = false;
    
                // Get the discount code to use for any discount lines, if one is required.
                string discountCodeUsed = this.GetDiscountCodeForDiscount(transaction);
    
                for (int x = 0; x < discountableItemGroups.Length; x++)
                {
                    DiscountableItemGroup discountableItemGroup = discountableItemGroups[x];
                    HashSet<decimal> discountLineNumberSet;
                    if (remainingQuantities[x] != 0M && this.ItemGroupIndexToDiscountLineNumberSetMap.TryGetValue(x, out discountLineNumberSet))
                    {
                        bool hasOverlap = this.HasOverlap(x, itemsWithOverlappingDiscounts);
    
                        foreach (decimal discountLineNumber in discountLineNumberSet)
                        {
                            RetailDiscountLine line = this.DiscountLines[discountLineNumber];
                            DiscountApplication result = new DiscountApplication(this, applyStandalone: !hasOverlap)
                            {
                                RetailDiscountLines = new List<RetailDiscountLineItem>(1) { new RetailDiscountLineItem(x, line) },
                                SortIndex = GetSortIndexForRetailDiscountLine(line),
                                SortValue = GetSortValue(line, discountableItemGroup),
                                DiscountCode = discountCodeUsed
                            };
                            result.ItemQuantities.AddRange(GetItemQuantitiesForDiscountApplication(x, discountableItemGroup.Quantity));
    
                            discountAppliations.Add(result);
                        }
                    }
                }
    
                return discountAppliations;
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
    
                if (itemsWithOverlappingDiscounts == null)
                {
                    throw new ArgumentNullException("itemsWithOverlappingDiscounts");
                }
    
                if (itemsWithOverlappingDiscountsCompoundedOnly == null)
                {
                    throw new ArgumentNullException("itemsWithOverlappingDiscountsCompoundedOnly");
                }
    
                List<DiscountApplication> discountApplications = new List<DiscountApplication>();
                string discountCodeUsed = this.GetDiscountCodeForDiscount(transaction);
    
                foreach (KeyValuePair<int, HashSet<decimal>> pair in this.ItemGroupIndexToDiscountLineNumberSetMap)
                {
                    int itemGroupIndex = pair.Key;
                    bool isOverlappedWithNonCompoundedDiscounts = this.IsItemIndexGroupOverlappedWithNonCompoundedDiscounts(
                        itemGroupIndex,
                        itemsWithOverlappingDiscounts,
                        itemsWithOverlappingDiscountsCompoundedOnly);
    
                    if (!isOverlappedWithNonCompoundedDiscounts)
                    {
                        HashSet<decimal> discountLineNumberSet = pair.Value;
                        decimal quantity = remainingQuantities[itemGroupIndex];
    
                        if (quantity > decimal.Zero && discountLineNumberSet.Count == 1)
                        {
                            RetailDiscountLine discountLineDefinition = this.DiscountLines[discountLineNumberSet.First()];
                            DiscountApplication result = new DiscountApplication(this, applyStandalone: true, removeItemsFromLookupsWhenApplied: true)
                            {
                                RetailDiscountLines = new List<RetailDiscountLineItem>(1) { new RetailDiscountLineItem(itemGroupIndex, discountLineDefinition) },
                                DiscountCode = discountCodeUsed
                            };
                            result.ItemQuantities.Add(itemGroupIndex, quantity);
    
                            discountApplications.Add(result);
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
                if (discountApplication == null || !discountApplication.RetailDiscountLines.Any() || discountableItemGroups == null || remainingQuantities == null)
                {
                    return null;
                }
    
                decimal[] prices = new decimal[discountableItemGroups.Length];
                Dictionary<int, IList<DiscountLineQuantity>> discountDictionary = this.GetExistingDiscountDictionaryAndDiscountedPrices(
                    discountableItemGroups,
                    remainingQuantities,
                    appliedDiscounts,
                    discountApplication,
                    true,
                    true,
                    prices);
    
                RetailDiscountLineItem retailDiscountLineItem = discountApplication.RetailDiscountLines.ElementAt(0);
    
                DiscountOfferMethod discountMethod = (DiscountOfferMethod)retailDiscountLineItem.RetailDiscountLine.DiscountMethod;
                decimal dealPrice = decimal.Zero;
    
                decimal discountValue = decimal.Zero;
                decimal discountAmountForDiscountLine = decimal.Zero;
                switch (discountMethod)
                {
                    case DiscountOfferMethod.DiscountAmount:
                        discountValue = retailDiscountLineItem.RetailDiscountLine.DiscountAmount;
                        discountAmountForDiscountLine = discountValue;
                        break;
    
                    case DiscountOfferMethod.DiscountPercent:
                        discountValue = prices[retailDiscountLineItem.ItemIndex] * (retailDiscountLineItem.RetailDiscountLine.DiscountPercent / 100M);
                        break;
    
                    case DiscountOfferMethod.OfferPrice:
                        dealPrice = retailDiscountLineItem.RetailDiscountLine.OfferPrice;
                        decimal bestExistingDealPrice = 0m;
                        bool hasExistingDealPrice = DiscountBase.TryGetBestExistingDealPrice(discountDictionary, retailDiscountLineItem.ItemIndex, out bestExistingDealPrice);
    
                        // We don't use discounted price here.
                        discountValue = DiscountBase.GetDiscountAmountFromDealPrice(discountableItemGroups[retailDiscountLineItem.ItemIndex].Price, hasExistingDealPrice, bestExistingDealPrice, dealPrice);
                        discountAmountForDiscountLine = discountValue;
                        break;
    
                    default:
                        break;
                }
    
                // When has no competing discounts or compounded, apply all remaining quantity.
                bool applyAllAvailableQuantity = (discountApplication.ApplyStandalone || this.CanCompound) && !discountApplication.HonorQuantity;
                decimal quantityToApply = applyAllAvailableQuantity ?
                    remainingQuantities[retailDiscountLineItem.ItemIndex] : discountApplication.ItemQuantities[retailDiscountLineItem.ItemIndex];
    
                decimal result = discountValue * quantityToApply;
    
                AppliedDiscountApplication newAppliedDiscountApplication = null;
    
                if (result > decimal.Zero)
                {
                    Dictionary<int, decimal> itemQuantities;
    
                    if (applyAllAvailableQuantity)
                    {
                        itemQuantities = new Dictionary<int, decimal>();
                        itemQuantities[retailDiscountLineItem.ItemIndex] = quantityToApply;
                    }
                    else
                    {
                        itemQuantities = discountApplication.ItemQuantities;
                    }
    
                    newAppliedDiscountApplication = new AppliedDiscountApplication(discountApplication, result, itemQuantities, isDiscountLineGenerated: true);
    
                    DiscountLine discountLine = this.NewDiscountLine(discountApplication.DiscountCode, discountableItemGroups[retailDiscountLineItem.ItemIndex].ItemId);
    
                    discountLine.PeriodicDiscountType = PeriodicDiscountOfferType.Offer;
                    discountLine.DealPrice = dealPrice;
                    discountLine.Amount = discountAmountForDiscountLine;
                    discountLine.Percentage = retailDiscountLineItem.RetailDiscountLine.DiscountPercent;
    
                    newAppliedDiscountApplication.AddDiscountLine(retailDiscountLineItem.ItemIndex, new DiscountLineQuantity(discountLine, itemQuantities[retailDiscountLineItem.ItemIndex]));
    
                    if (discountApplication.RemoveItemsFromLookupsWhenApplied)
                    {
                        this.RemoveItemIndexGroupFromLookups(retailDiscountLineItem.ItemIndex);
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
    
            internal static decimal GetUnitDiscountAmount(RetailDiscountLine discountLineDefinition, decimal price)
            {
                bool isFullyCovered = false;
                return GetUnitDiscountAmountAndCheckWhetherItsFullyCovered(discountLineDefinition, price, out isFullyCovered);
            }
    
            internal static decimal GetUnitDiscountAmountAndCheckWhetherItsFullyCovered(RetailDiscountLine discountLineDefinition, decimal price, out bool isFullyCovered)
            {
                isFullyCovered = true;
                decimal unitDiscountAmount = decimal.Zero;
    
                switch ((DiscountOfferMethod)discountLineDefinition.DiscountMethod)
                {
                    case DiscountOfferMethod.DiscountAmount:
                        unitDiscountAmount = discountLineDefinition.DiscountAmount;
                        isFullyCovered = unitDiscountAmount <= price;
                        break;
                    case DiscountOfferMethod.DiscountPercent:
                        unitDiscountAmount = DiscountBase.GetDiscountAmountForPercentageOff(price, discountLineDefinition.DiscountPercent);
                        break;
                    case DiscountOfferMethod.OfferPrice:
                        unitDiscountAmount = DiscountBase.GetDiscountAmountForDealUnitPrice(price, discountLineDefinition.OfferPrice);
                        isFullyCovered = discountLineDefinition.OfferPrice <= price;
                        break;
                    default:
                        break;
                }
    
                unitDiscountAmount = Math.Min(price, unitDiscountAmount);
                unitDiscountAmount = Math.Max(unitDiscountAmount, decimal.Zero);
    
                return unitDiscountAmount;
            }
    
            internal void GetDiscountApplicationHonorQuantityForOneItem(
                List<DiscountApplication> discountApplications,
                int itemGroupIndex,
                SalesTransaction transaction,
                DiscountableItemGroup[] discountableItemGroups,
                decimal quantity)
            {
                string discountCodeUsed = this.GetDiscountCodeForDiscount(transaction);
                HashSet<decimal> discountLineNumberSet = null;
                this.ItemGroupIndexToDiscountLineNumberSetMap.TryGetValue(itemGroupIndex, out discountLineNumberSet);
                this.GetDiscountApplicationHonorQuantityForOneItem(
                    discountApplications,
                    itemGroupIndex,
                    discountLineNumberSet,
                    discountableItemGroups,
                    quantity,
                    discountCodeUsed);
            }
    
            internal void GetDiscountApplicationHonorQuantityForOneItem(
                List<DiscountApplication> discountApplications,
                int itemGroupIndex,
                HashSet<decimal> discountLineNumberSet,
                DiscountableItemGroup[] discountableItemGroups,
                decimal quantity,
                string discountCodeUsed)
            {
                if (quantity > decimal.Zero && discountLineNumberSet != null && discountLineNumberSet.Count > 0)
                {
                    decimal price = discountableItemGroups[itemGroupIndex].Price;
    
                    decimal discountLineNumberWithBestDeal = decimal.Zero;
                    decimal unitDiscountAmountBest = decimal.Zero;
    
                    // Find the best deal from all discount lines.
                    foreach (decimal discountLineNumber in discountLineNumberSet)
                    {
                        RetailDiscountLine discountLineDefinition = this.DiscountLines[discountLineNumber];
                        decimal unitDiscountAmount = GetUnitDiscountAmount(discountLineDefinition, price);
    
                        if (unitDiscountAmount > unitDiscountAmountBest)
                        {
                            unitDiscountAmountBest = unitDiscountAmount;
                            discountLineNumberWithBestDeal = discountLineNumber;
                        }
                    }
    
                    if (unitDiscountAmountBest > decimal.Zero)
                    {
                        RetailDiscountLine discountLineDefinition = this.DiscountLines[discountLineNumberWithBestDeal];
                        DiscountApplication result = new DiscountApplication(this, applyStandalone: true)
                        {
                            // DISCOUNTPERF: replace list with array?
                            RetailDiscountLines = new List<RetailDiscountLineItem>(1) { new RetailDiscountLineItem(itemGroupIndex, discountLineDefinition) },
                            DiscountCode = discountCodeUsed
                        };
                        result.ItemQuantities.Add(itemGroupIndex, quantity);
                        result.HonorQuantity = true;
    
                        discountApplications.Add(result);
                    }
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
                string discountCodeUsed = this.GetDiscountCodeForDiscount(transaction);
                List<DiscountApplication> discountApplications = new List<DiscountApplication>();
    
                // Get one discount application for each item group index with full quantity.
                foreach (KeyValuePair<int, HashSet<decimal>> pair in this.ItemGroupIndexToDiscountLineNumberSetMap)
                {
                    int itemGroupIndex = pair.Key;
                    HashSet<decimal> discountLineNumberSet = pair.Value;
                    decimal quantity = decimal.Zero;
    
                    if (remainingQuantities != null)
                    {
                        quantity = remainingQuantities[itemGroupIndex];
                    }
    
                    if (quantity > decimal.Zero)
                    {
                        this.GetDiscountApplicationHonorQuantityForOneItem(
                            discountApplications,
                            itemGroupIndex,
                            discountLineNumberSet,
                            discountableItemGroups,
                            quantity,
                            discountCodeUsed);
                    }
                }
    
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
                this.RemoveItemGroupIndexesWithZeroQuanttiyFromLookups(remainingQuantities);
    
                this.ReduceWorseDiscountLines(discountableItemGroups);
    
                this.CleanupLookups();
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
    
                decimal totalApplicableQuantityWithOverlapped = decimal.Zero;
                decimal totalDiscountAmountWithOverlapped = decimal.Zero;
                decimal totalApplicableQuantityWithoutOverlapped = decimal.Zero;
                decimal totalDiscountAmountWithoutOverlapped = decimal.Zero;
                Dictionary<int, decimal> itemGroupIndexToQuantityNeededFromOverlappedLookup = new Dictionary<int, decimal>();
    
                foreach (KeyValuePair<int, HashSet<decimal>> pair in this.ItemGroupIndexToDiscountLineNumberSetMap)
                {
                    int itemGroupIndex = pair.Key;
                    HashSet<decimal> discountLineNumberSet = pair.Value;
                    decimal quantity = remainingQuantities[itemGroupIndex];
                    if (quantity > decimal.Zero)
                    {
                        decimal price = discountableItemGroups[itemGroupIndex].Price;
                        decimal unitDiscountAmount = decimal.Zero;
    
                        if (discountLineNumberSet.Any())
                        {
                            unitDiscountAmount = GetUnitDiscountAmount(this.DiscountLines[discountLineNumberSet.First()], price);
                        }
    
                        decimal effectiveDiscountAmount = unitDiscountAmount * quantity;
                        totalApplicableQuantityWithOverlapped += quantity;
                        totalDiscountAmountWithOverlapped += effectiveDiscountAmount;
    
                        if (this.IsItemIndexGroupOverlappedWithNonCompoundedDiscounts(itemGroupIndex, itemsWithOverlappingDiscounts, itemsWithOverlappingDiscountsCompoundedOnly))
                        {
                            itemGroupIndexToQuantityNeededFromOverlappedLookup[itemGroupIndex] = quantity;
                        }
                        else
                        {
                            totalApplicableQuantityWithoutOverlapped += quantity;
                            totalDiscountAmountWithoutOverlapped += effectiveDiscountAmount;
                        }
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
                HashSet<decimal> discountLineNumberSet = null;
                if (this.ItemGroupIndexToDiscountLineNumberSetMap.TryGetValue(itemGroupIndex, out discountLineNumberSet))
                {
                    if (discountLineNumberSet.Count == 1)
                    {
                        RetailDiscountLine discountLineDefinition = this.DiscountLines[discountLineNumberSet.First()];
    
                        result = new SingleItemNonOverlappedDiscountResult(
                            (DiscountOfferMethod)discountLineDefinition.DiscountMethod,
                            discountLineDefinition.DiscountAmount,
                            discountLineDefinition.DiscountPercent,
                            discountLineDefinition.OfferPrice,
                            GetUnitDiscountAmount(discountLineDefinition, price),
                            this.ConcurrencyMode);
                    }
                }
    
                return result;
            }
    
            /// <summary>
            /// Determines if the item is likely evaluated with other items.
            /// </summary>
            /// <param name="itemGroupIndex">Item group index.</param>
            /// <returns>Always false.</returns>
            protected internal override bool IsItemLikelyEvaluatedWithOtherItems(int itemGroupIndex)
            {
                return false;
            }
    
            /// <summary>
            /// Gets the sort value to use for the specified discount line and item group.
            /// </summary>
            /// <param name="discountLine">The discount line used for this application.</param>
            /// <param name="discountableItemGroup">The item group used for this application.</param>
            /// <returns>The sort value.</returns>
            private static decimal GetSortValue(RetailDiscountLine discountLine, DiscountableItemGroup discountableItemGroup)
            {
                switch ((DiscountOfferMethod)discountLine.DiscountMethod)
                {
                    case DiscountOfferMethod.DiscountAmount:
                        return discountLine.DiscountAmount;
                    case DiscountOfferMethod.DiscountPercent:
                        return discountableItemGroup.Price * discountLine.DiscountPercent;
                    case DiscountOfferMethod.OfferPrice:
                        return discountableItemGroup.Price - discountLine.DiscountLinePercentOrValue - discountLine.OfferPrice;
                    default:
                        return 0;
                }
            }
    
            /// <summary>
            /// Gets a decimal array of the specified size with the value for the specified index set to 1.
            /// </summary>
            /// <param name="index">The index for the value that should be set to 1.</param>
            /// <param name="quantity">Item quantity.</param>
            /// <returns>The resulting decimal array.</returns>
            /// <remarks>For each line item, item quantity for discount application is 1 if line item quantity is integer and the whole quantity if fractional.</remarks>
            private static Dictionary<int, decimal> GetItemQuantitiesForDiscountApplication(int index, decimal quantity)
            {
                Dictionary<int, decimal> result = new Dictionary<int, decimal>();
                if (DiscountableItemGroup.IsFraction(quantity))
                {
                    result[index] = Math.Abs(quantity);
                }
                else
                {
                    result[index] = 1M;
                }
    
                return result;
            }
    
            private void ReduceWorseDiscountLines(
                DiscountableItemGroup[] discountableItemGroups)
            {
                // Tested in PricingTests.UnitTestOfferPreOptimization
                foreach (KeyValuePair<int, HashSet<decimal>> pair in this.ItemGroupIndexToDiscountLineNumberSetMap)
                {
                    int itemGroupIndex = pair.Key;
                    HashSet<decimal> discountLineNumberSet = pair.Value;
    
                    if (discountLineNumberSet.Count > 1)
                    {
                        decimal discountLineNumberToKeep = decimal.Zero;
                        decimal bestUnitDiscountAmount = decimal.Zero;
                        bool isFirst = true;
    
                        decimal price = discountableItemGroups[itemGroupIndex].Price;
                        foreach (decimal discountLineNumber in discountLineNumberSet)
                        {
                            RetailDiscountLine discountLine = this.DiscountLines[discountLineNumber];
                            decimal unitDiscountAmount = GetUnitDiscountAmount(discountLine, price);
                            if (isFirst)
                            {
                                discountLineNumberToKeep = discountLineNumber;
                                bestUnitDiscountAmount = unitDiscountAmount;
                                isFirst = false;
                            }
                            else if (unitDiscountAmount > bestUnitDiscountAmount)
                            {
                                discountLineNumberToKeep = discountLineNumber;
                                bestUnitDiscountAmount = unitDiscountAmount;
                            }
                        }
    
                        foreach (decimal discountLineNumber in discountLineNumberSet)
                        {
                            if (discountLineNumber != discountLineNumberToKeep)
                            {
                                HashSet<int> itemGroupIndexSetForDiscountLineNumber = null;
                                if (this.DiscountLineNumberToItemGroupIndexSetMap.TryGetValue(discountLineNumber, out itemGroupIndexSetForDiscountLineNumber))
                                {
                                    itemGroupIndexSetForDiscountLineNumber.Remove(itemGroupIndex);
                                }
                            }
                        }
    
                        discountLineNumberSet.Clear();
                        discountLineNumberSet.Add(discountLineNumberToKeep);
                    }
                }
            }
        }
    }
}
