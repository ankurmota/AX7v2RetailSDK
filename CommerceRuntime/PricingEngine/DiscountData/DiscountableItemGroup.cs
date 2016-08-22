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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// Class containing groupings of SalesLine objects so that identical items that are on multiple lines can be considered as a single line.
        /// </summary>
        public class DiscountableItemGroup
        {
            private List<SalesLine> salesLines;
            private List<DiscountLineQuantity> discountLineQuantitiesNonCompounded;
            private List<DiscountLineQuantity> discountLineQuantitiesCompounded;
            private PriceContext priceContext;
    
            /// <summary>
            /// Initializes a new instance of the DiscountableItemGroup class with the specified SalesLine object included.
            /// </summary>
            /// <param name="line">The line item to add.</param>
            /// <param name="priceContext">Price context.</param>
            public DiscountableItemGroup(SalesLine line, PriceContext priceContext)
                : this(priceContext)
            {
                this.Add(line);
            }
    
            /// <summary>
            /// Initializes a new instance of the DiscountableItemGroup class with the specified SalesLine object included.
            /// </summary>
            /// <param name="lines">The line items to add.</param>
            /// <param name="priceContext">Price context.</param>
            public DiscountableItemGroup(IEnumerable<SalesLine> lines, PriceContext priceContext)
                : this(priceContext)
            {
                this.salesLines = new List<SalesLine>(lines);
                this.Quantity += this.salesLines.Sum(p => p.Quantity);
            }
    
            /// <summary>
            /// Initializes a new instance of the DiscountableItemGroup class with no SalesLine items included.
            /// </summary>
            /// <param name="priceContext">Price context.</param>
            private DiscountableItemGroup(PriceContext priceContext)
            {
                this.discountLineQuantitiesNonCompounded = new List<DiscountLineQuantity>();
                this.discountLineQuantitiesCompounded = new List<DiscountLineQuantity>();
                this.salesLines = new List<SalesLine>();
                this.Quantity = 0M;
                this.priceContext = priceContext;
            }
    
            /// <summary>
            /// Gets the item ID from the collection of SalesLines.
            /// </summary>
            public string ItemId
            {
                get { return this.salesLines.Any() ? this.salesLines[0].ItemId : string.Empty; }
            }
    
            /// <summary>
            /// Gets the price from the collection of SalesLines.
            /// </summary>
            public decimal Price
            {
                get { return this.salesLines.Any() ? this.salesLines[0].Price : 0M; }
            }
    
            /// <summary>
            /// Gets the product ID from the collection of SalesLines.
            /// </summary>
            public long ProductId
            {
                get { return this.salesLines.Any() ? this.salesLines[0].ProductId : 0; }
            }
    
            /// <summary>
            /// Gets product id or the master product id of the variant.
            /// </summary>
            /// <remarks>This is RecId in EcoResProduct table.</remarks>
            public long MasterProductId
            {
                get { return this.salesLines.Any() ? this.salesLines[0].MasterProductId : 0; }
            }
    
            /// <summary>
            /// Gets the unit of measure from the collection of SalesLines.
            /// </summary>
            public string SalesOrderUnitOfMeasure
            {
                get { return this.salesLines.Any() ? this.salesLines[0].SalesOrderUnitOfMeasure : string.Empty; }
            }
    
            /// <summary>
            /// Gets the catalog identifier from the collection of SalesLines.
            /// </summary>
            public ISet<long> CatalogIds
            {
                get { return this.salesLines.Any() ? this.salesLines[0].CatalogIds : new HashSet<long>(); }
            }
    
            /// <summary>
            /// Gets the number of SalesLines included in this group.
            /// </summary>
            public int Count
            {
                get { return this.salesLines.Count; }
            }
    
            /// <summary>
            /// Gets or sets the Item extended properties for this item group.
            /// </summary>
            public Item ExtendedProperties { get; set; }
    
            /// <summary>
            /// Gets the total quantity for the line items in this group.
            /// </summary>
            public decimal Quantity { get; private set; }
    
            /// <summary>
            /// Gets the SalesLine object at the specified index within this group.
            /// </summary>
            /// <param name="index">The index of the line to retrieve.</param>
            /// <returns>The SalesLine object.</returns>
            public SalesLine this[int index]
            {
                get { return this.salesLines[index]; }
            }
    
            /// <summary>
            /// Adds the specified SalesLine object to the collection of lines included in this group.
            /// </summary>
            /// <param name="line">The line item to add.</param>
            public void Add(SalesLine line)
            {
                if (line != null)
                {
                    this.salesLines.Add(line);
                    this.Quantity += line.Quantity;
                }
            }
    
            /// <summary>
            /// Adds a discount line to this group.
            /// </summary>
            /// <param name="discountLineQuantity">The discount line quantity to add to the group.</param>
            public void AddDiscountLine(DiscountLineQuantity discountLineQuantity)
            {
                ThrowIf.Null(discountLineQuantity, "discountLineQuantity");
                ThrowIf.Null(discountLineQuantity.DiscountLine, "discountLineQuantity.DiscountLine");
    
                List<DiscountLineQuantity> discountLineQuantitiesToAdd = discountLineQuantity.DiscountLine.ConcurrencyMode == ConcurrencyMode.Compounded ? this.discountLineQuantitiesCompounded : this.discountLineQuantitiesNonCompounded;
                DiscountLineQuantity existingItem = discountLineQuantitiesToAdd.FirstOrDefault(p => p.DiscountLine.OfferId == discountLineQuantity.DiscountLine.OfferId && p.DiscountLine.Amount == discountLineQuantity.DiscountLine.Amount && p.DiscountLine.Percentage == discountLineQuantity.DiscountLine.Percentage);
    
                if (existingItem != null)
                {
                    existingItem.Quantity += discountLineQuantity.Quantity;
                }
                else
                {
                    discountLineQuantitiesToAdd.Add(discountLineQuantity);
                }
            }
    
            /// <summary>
            /// Applies the discount lines for this group to the transaction, splitting lines if necessary.
            /// </summary>
            /// <param name="transaction">The transaction that contains the lines in this group.</param>
            /// <param name="isReturn">True if it's return.</param>
            [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals", Justification = "Grandfathered.")]
            public void ApplyDiscountLines(SalesTransaction transaction, bool isReturn)
            {
                ThrowIf.Null(transaction, "transaction");
    
                if (!this.salesLines.Any())
                {
                    return;
                }
    
                // Keep track of available line items that have non-compoundable discounts applied to them.
                HashSet<int> availableLines = new HashSet<int>();
    
                // Get the lines in reverse order, so that BOGO scenarios apply to the last item by default.
                for (int x = this.salesLines.Count - 1; x >= 0; x--)
                {
                    availableLines.Add(x);
                }
    
                // Finish up exclusive or best price first.
                foreach (DiscountLineQuantity discountLineQuantity in this.discountLineQuantitiesNonCompounded)
                {
                    this.ApplyOneDiscountLine(discountLineQuantity, transaction, availableLines, isReturn);
                }
    
                //// Discounts will be in order of non-compoundable first, then compoundable, we should maintain that order.
                //// In addition, for each concurrency, threhold is the last.
                bool reallocateDiscountAmountForCompoundedThresholdDiscountAmount = false;
    
                // Prepare offer id to discount amount dictionary for compounded threshold discount with amount off.
                Dictionary<string, decimal> offerIdToDiscountAmountDictionary = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, int> offerIdToPriorityDictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (DiscountLineQuantity discount in this.discountLineQuantitiesCompounded)
                {
                    if (discount.DiscountLine.PeriodicDiscountType == PeriodicDiscountOfferType.MixAndMatch && discount.DiscountLine.IsCompoundable)
                    {
                        reallocateDiscountAmountForCompoundedThresholdDiscountAmount = true;
                    }
    
                    if (reallocateDiscountAmountForCompoundedThresholdDiscountAmount &&
                        discount.DiscountLine.PeriodicDiscountType == PeriodicDiscountOfferType.Threshold
                        && discount.DiscountLine.IsCompoundable && discount.DiscountLine.Amount > decimal.Zero)
                    {
                        decimal thresholdDiscountAmount = decimal.Zero;
                        offerIdToDiscountAmountDictionary.TryGetValue(discount.DiscountLine.OfferId, out thresholdDiscountAmount);
    
                        // Effective amount was calculated earlier in ThresholdDiscount.cs for threshold discount with amount off.
                        offerIdToDiscountAmountDictionary[discount.DiscountLine.OfferId] = thresholdDiscountAmount +
                            this.priceContext.CurrencyAndRoundingHelper.Round(discount.DiscountLine.Amount * discount.Quantity);
    
                        if (!offerIdToPriorityDictionary.ContainsKey(discount.DiscountLine.OfferId))
                        {
                            offerIdToPriorityDictionary.Add(discount.DiscountLine.OfferId, discount.DiscountLine.PricingPriorityNumber);
                        }
                    }
                    else
                    {
                        if (!discount.DiscountLine.IsCompoundable || discount.DiscountLine.PeriodicDiscountType == PeriodicDiscountOfferType.Threshold)
                        {
                            this.ApplyOneDiscountLine(discount, transaction, availableLines, isReturn);
                        }
                        else
                        {
                            // See comment for method ApplyOneDiscountLineToCompoundedOnly.
                            decimal quantityNotApplied = this.ApplyOneDiscountLineToCompoundedOnly(discount, transaction, availableLines, isReturn);
    
                            if (quantityNotApplied > decimal.Zero)
                            {
                                this.ApplyOneDiscountLine(discount, transaction, availableLines, isReturn, quantityNotApplied);
                            }
                        }
                    }
                }
    
                //// We need to reallocate amount off for compounded threshold discount amount in some cases.
                //// Earlier we calculate discount amount right for the item group as a whole, but we weren't able to allocate it properly.
                //// E.g. compounded mix and match discount of bug one get one 1c, and compounded threshold discount of $10.
                ////      Transaction has a item of quantity 2, one get 1c deal price, the other one gets nothing.
                ////      Threshold discount amount would like $5 for both items, but the effective amount for the 1st one is just 1c.
                ////      As such, we need to reallocate threshold discount $10 (total), 1c to 1st item, and $9.99 to the 2nd one.
                if (reallocateDiscountAmountForCompoundedThresholdDiscountAmount)
                {
                    foreach (KeyValuePair<string, decimal> offerIdDiscountAmountPair in offerIdToDiscountAmountDictionary)
                    {
                        // First, build item index to discounted price dictionary, exclusing threshold percentage off.
                        Dictionary<int, decimal> itemIndexToDiscountedPriceDictionary = new Dictionary<int, decimal>();
                        availableLines.Clear();
                        decimal totalPrice = decimal.Zero;
                        for (int x = this.salesLines.Count - 1; x >= 0; x--)
                        {
                            IList<DiscountLine> existingDiscountLines = this.salesLines[x].DiscountLines;
                            decimal salesLineQuantity = Math.Abs(this.salesLines[x].Quantity);
    
                            // Ignore non-compounded sales lines.
                            if (salesLineQuantity != decimal.Zero && !existingDiscountLines.Where(p => !p.IsCompoundable).Any())
                            {
                                decimal discountedPrice = this.Price;
                                availableLines.Add(x);
    
                                // non-threshold discount $-off first
                                foreach (DiscountLine discountLineAmountOff in existingDiscountLines)
                                {
                                    if (discountLineAmountOff.Amount > decimal.Zero &&
                                        discountLineAmountOff.PeriodicDiscountType != PeriodicDiscountOfferType.Threshold)
                                    {
                                        discountedPrice -= discountLineAmountOff.Amount;
                                    }
                                }
    
                                // non-threshold discount %-off
                                foreach (DiscountLine discountLinePercentOff in existingDiscountLines)
                                {
                                    if (discountLinePercentOff.Amount == decimal.Zero &&
                                        discountLinePercentOff.PeriodicDiscountType != PeriodicDiscountOfferType.Threshold)
                                    {
                                        discountedPrice -= discountedPrice * (discountLinePercentOff.Percentage / 100m);
                                    }
                                }
    
                                // threshold discount $-off
                                foreach (DiscountLine discountLineAmountOff in existingDiscountLines)
                                {
                                    if (discountLineAmountOff.Amount > decimal.Zero &&
                                        discountLineAmountOff.PeriodicDiscountType == PeriodicDiscountOfferType.Threshold)
                                    {
                                        discountedPrice -= discountLineAmountOff.Amount;
                                    }
                                }
    
                                discountedPrice = Math.Max(decimal.Zero, discountedPrice);
                                itemIndexToDiscountedPriceDictionary[x] = discountedPrice;
                                totalPrice += discountedPrice * salesLineQuantity;
                            }
                        }
    
                        decimal offerDiscountAmount = offerIdDiscountAmountPair.Value;
                        int priority = 0;
                        offerIdToPriorityDictionary.TryGetValue(offerIdDiscountAmountPair.Key, out priority);
    
                        int[] salesLineIndicesSorted = itemIndexToDiscountedPriceDictionary.Keys.ToArray();
                        SalesLineDiscountedPriceComparer comparer = new SalesLineDiscountedPriceComparer(itemIndexToDiscountedPriceDictionary, this.salesLines);
                        Array.Sort(salesLineIndicesSorted, comparer.CompareNetPrice);
    
                        for (int index = 0; index < salesLineIndicesSorted.Length; index++)
                        {
                            int salesLineIndex = salesLineIndicesSorted[index];
                            SalesLine salesLine = this.salesLines[salesLineIndex];
                            decimal salesLineQuantity = Math.Abs(salesLine.Quantity);
                            decimal price = itemIndexToDiscountedPriceDictionary[salesLineIndex];
                            if (DiscountableItemGroup.IsFraction(salesLineQuantity))
                            {
                                decimal myDiscountAmount = offerDiscountAmount * ((price * salesLineQuantity) / totalPrice);
                                myDiscountAmount = this.priceContext.CurrencyAndRoundingHelper.Round(myDiscountAmount);
                                decimal unitDiscountAmount = myDiscountAmount / salesLineQuantity;
                                offerDiscountAmount -= myDiscountAmount;
                                salesLine.DiscountLines.Add(NewDiscountLineCompoundedThreshold(
                                    offerIdDiscountAmountPair.Key,
                                    salesLine.LineNumber,
                                    unitDiscountAmount,
                                    priority));
                            }
                            else
                            {
                                if (index == (salesLineIndicesSorted.Length - 1))
                                {
                                    // Last one.
                                    decimal smallestAmount = PriceContextHelper.GetSmallestNonNegativeAmount(this.priceContext, Math.Min(offerDiscountAmount, price));
                                    if (smallestAmount > decimal.Zero && salesLineQuantity > decimal.Zero)
                                    {
                                        int totalDiscountAmountInSmallestAmount = (int)(offerDiscountAmount / smallestAmount);
                                        int averageDiscountAmountRoundingDownInSmallestAmount = totalDiscountAmountInSmallestAmount / (int)salesLineQuantity;
                                        decimal quantityForHigherDiscountAmount = totalDiscountAmountInSmallestAmount - (averageDiscountAmountRoundingDownInSmallestAmount * (int)salesLineQuantity);
                                        decimal quantityForLowerDiscountAmount = salesLineQuantity - quantityForHigherDiscountAmount;
                                        decimal unitDiscountAmountForLowerDiscountAmount = averageDiscountAmountRoundingDownInSmallestAmount * smallestAmount;
    
                                        if (quantityForHigherDiscountAmount > decimal.Zero)
                                        {
                                            SalesLine salesLineToAddDiscount = salesLine;
                                            if (quantityForLowerDiscountAmount > decimal.Zero)
                                            {
                                                SalesLine newLine = this.SplitLine(salesLine, quantityForHigherDiscountAmount);
    
                                                // Add the new line to the transaction and to the available lines for discounts.
                                                // Set the line number to the next available number.
                                                newLine.LineNumber = transaction.SalesLines.Max(p => p.LineNumber) + 1;
                                                transaction.SalesLines.Add(newLine);
                                                this.salesLines.Add(newLine);
                                                salesLineToAddDiscount = newLine;
                                            }
    
                                            DiscountLine discountLine = NewDiscountLineCompoundedThreshold(
                                                offerIdDiscountAmountPair.Key,
                                                salesLineToAddDiscount.LineNumber,
                                                unitDiscountAmountForLowerDiscountAmount + smallestAmount,
                                                priority);
                                            salesLineToAddDiscount.DiscountLines.Add(discountLine);
                                        }
    
                                        if (quantityForLowerDiscountAmount > decimal.Zero && unitDiscountAmountForLowerDiscountAmount > decimal.Zero)
                                        {
                                            DiscountLine discountLine = NewDiscountLineCompoundedThreshold(
                                                offerIdDiscountAmountPair.Key,
                                                salesLine.LineNumber,
                                                unitDiscountAmountForLowerDiscountAmount,
                                                priority);
                                            salesLine.DiscountLines.Add(discountLine);
                                        }
                                    }
                                }
                                else
                                {
                                    decimal myDiscountAmount = offerDiscountAmount * ((price * salesLineQuantity) / totalPrice);
                                    decimal unitDiscountAmount = this.priceContext.CurrencyAndRoundingHelper.Round(myDiscountAmount / salesLineQuantity);
                                    if (unitDiscountAmount > decimal.Zero)
                                    {
                                        offerDiscountAmount -= unitDiscountAmount * salesLineQuantity;
                                        salesLine.DiscountLines.Add(NewDiscountLineCompoundedThreshold(
                                            offerIdDiscountAmountPair.Key,
                                            salesLine.LineNumber,
                                            unitDiscountAmount,
                                            priority));
                                    }
                                }
                            }
                        }
                    }
                }
            }
    
            /// <summary>
            /// Multiplies the quantities for each line by -1, for use in processing negative quantities just as positive quantities are processed.
            /// </summary>
            public void NegateQuantity()
            {
                this.Quantity *= -1;
            }
    
            internal static bool IsFraction(decimal number)
            {
                decimal fraction = number % 1m;
    
                return fraction != decimal.Zero;
            }
    
            private static bool IsQuantityMatchSalesOrReturn(bool isReturn, decimal quantity)
            {
                return (isReturn && quantity < decimal.Zero) || (!isReturn && quantity > decimal.Zero);
            }
    
            private static DiscountLine NewDiscountLineCompoundedThreshold(string offerId, decimal salesLineNumber, decimal amount, int priorityNumber)
            {
                DiscountLine discountLine = new DiscountLine()
                {
                    OfferId = offerId,
                    SaleLineNumber = salesLineNumber,
                    DiscountLineType = Microsoft.Dynamics.Commerce.Runtime.DataModel.DiscountLineType.PeriodicDiscount,
                    PeriodicDiscountType = PeriodicDiscountOfferType.Threshold,
                    ConcurrencyMode = ConcurrencyMode.Compounded,
                    IsCompoundable = true,
                    PricingPriorityNumber = priorityNumber,
                    Amount = amount,
                };
    
                return discountLine;
            }
    
            private static bool CanApplyCompoundedDiscount(
                IEnumerable<DiscountLine> existingDiscountLines,
                DiscountLine newDiscountLine,
                bool requiresExistingCompounded)
            {
                bool canApplyCompoundedDiscount = true;
    
                if (newDiscountLine.PeriodicDiscountType == PeriodicDiscountOfferType.Threshold ||
                    newDiscountLine.PeriodicDiscountType == PeriodicDiscountOfferType.Offer)
                {
                    // Threshold and discount offer don't discount for partial quantity, so no need to worry about requiresExistingCompounded;
                    canApplyCompoundedDiscount = CanCompoundTogether(existingDiscountLines, newDiscountLine);
                }
                else
                {
                    // This is really for multiple compounded mix and match discounts, each of which taking partial quantity.
                    // We'd compound them together as much as possible.
                    if (existingDiscountLines.Any())
                    {
                        canApplyCompoundedDiscount = CanCompoundTogether(existingDiscountLines, newDiscountLine);
                    }
                    else
                    {
                        // No existing discount
                        if (requiresExistingCompounded)
                        {
                            canApplyCompoundedDiscount = false;
                        }
                    }
                }
    
                return canApplyCompoundedDiscount;
            }
    
            private static bool CanCompoundTogether(IEnumerable<DiscountLine> existingDiscountLines, DiscountLine newDiscountLine)
            {
                bool canCompoundTogether = true;
    
                if (!newDiscountLine.IsCompoundable)
                {
                    canCompoundTogether = false;
                }
    
                if (canCompoundTogether)
                {
                    if (newDiscountLine.PeriodicDiscountType == PeriodicDiscountOfferType.Threshold)
                    {
                        // threshold compounded discounts: can NOT compound across priorities within threshold discounts.
                        //                                 can compound on top of non-theshold discounts.
                        if (existingDiscountLines.Where(p => string.Equals(newDiscountLine.OfferId, p.OfferId, StringComparison.OrdinalIgnoreCase) ||
                                                            (p.PeriodicDiscountType == PeriodicDiscountOfferType.Threshold && newDiscountLine.PricingPriorityNumber != p.PricingPriorityNumber) ||
                                                            !p.IsCompoundable).Any())
                        {
                            canCompoundTogether = false;
                        }
                    }
                    else
                    {
                        // non-threshold compounded discounts: can NOT compound across priorities.
                        if (existingDiscountLines.Where(p => string.Equals(newDiscountLine.OfferId, p.OfferId, StringComparison.OrdinalIgnoreCase) ||
                                                        newDiscountLine.PricingPriorityNumber != p.PricingPriorityNumber ||
                                                        !p.IsCompoundable).Any())
                        {
                            canCompoundTogether = false;
                        }
                    }
                }
    
                return canCompoundTogether;
            }
    
            /// <summary>
            /// Apply one discount line on top of compounded discounts only.
            /// </summary>
            /// <param name="discountLineQuantity">Discount line and quantity.</param>
            /// <param name="transaction">Sales transaction.</param>
            /// <param name="availableLines">Available sales lines.</param>
            /// <param name="isReturn">A flag indicating whether it's for return.</param>
            /// <returns>Quantity not applied yet.</returns>
            /// <remarks>
            /// This is to deal with multiple mix and match discounts, each taking only partial quantity.
            /// A simple example of one sales line of quantity 3, where compounded mix and mach 1 (mm1) takes 1 quantity and mm2 takes 2.
            ///   We'd split the line into 3:
            ///      quantity 1 - mm1 and mm2 compounded
            ///      quantity 1 - mm2 only
            ///      quantity 1 - no discount
            /// The alternative is to split the line into 2, one of quantity 1 with mm1 and there other one of quantity 2 with mm2.
            /// The reason for the decision is that with multiple rounds of discount calculation, by compounding them together, we could leave
            /// quantity for the next round.
            /// It's a rare scenario. The bottom line is that we need to ensure consistent behaviors.
            /// </remarks>
            private decimal ApplyOneDiscountLineToCompoundedOnly(
                DiscountLineQuantity discountLineQuantity,
                SalesTransaction transaction,
                HashSet<int> availableLines,
                bool isReturn)
            {
                decimal quantityNotApplied = decimal.Zero;
                if (discountLineQuantity.Quantity != decimal.Zero)
                {
                    decimal quantityNeeded = discountLineQuantity.Quantity;
    
                    bool discountFullyApplied = false;
                    bool keepGoing = true;
    
                    while (keepGoing)
                    {
                        bool discountPartiallyApplied = false;
    
                        // Look for an exact match first within lines
                        discountFullyApplied = this.ApplyDiscountLineForDirectMatch(availableLines, quantityNeeded, discountLineQuantity, isReturn, requiresExistingCompoundedDiscounts: true);
    
                        // If that was not found, look for a combination of lower-quantity lines by selecting the largest lower-quantity line and repeating this loop.
                        if (!discountFullyApplied)
                        {
                            discountPartiallyApplied = this.ApplyDiscountLineForSmallerMatch(availableLines, ref quantityNeeded, discountLineQuantity, isReturn, requiresExistingCompoundedDiscounts: true);
                        }
    
                        // If we still have not found a match, find the smallest higher-quantity line and split it.
                        if (!discountFullyApplied && !discountPartiallyApplied)
                        {
                            discountFullyApplied = this.ApplyDiscountLineForLargerMatch(transaction, availableLines, ref quantityNeeded, discountLineQuantity, isReturn, requiresExistingCompoundedDiscounts: true);
                        }
    
                        // Keep going if discount is only partial - not fully - applied.
                        keepGoing = !discountFullyApplied && discountPartiallyApplied;
                    }
    
                    quantityNotApplied = discountFullyApplied ? decimal.Zero : quantityNeeded;
                }
    
                return quantityNotApplied;
            }
    
            private void ApplyOneDiscountLine(
                DiscountLineQuantity discountLineQuantity,
                SalesTransaction transaction,
                HashSet<int> availableLines,
                bool isReturn)
            {
                this.ApplyOneDiscountLine(
                    discountLineQuantity,
                    transaction,
                    availableLines,
                    isReturn,
                    discountLineQuantity.Quantity);
            }
    
            private void ApplyOneDiscountLine(
                DiscountLineQuantity discountLineQuantity,
                SalesTransaction transaction,
                HashSet<int> availableLines,
                bool isReturn,
                decimal quantityNotApplied)
            {
                if (discountLineQuantity.Quantity != decimal.Zero)
                {
                    bool discountFullyApplied = false;
                    bool keepGoing = true;
    
                    while (keepGoing)
                    {
                        bool discountPartiallyApplied = false;
    
                        // Look for an exact match first within lines
                        discountFullyApplied = this.ApplyDiscountLineForDirectMatch(availableLines, quantityNotApplied, discountLineQuantity, isReturn);
    
                        // If that was not found, look for a combination of lower-quantity lines by selecting the largest lower-quantity line and repeating this loop.
                        if (!discountFullyApplied)
                        {
                            discountPartiallyApplied = this.ApplyDiscountLineForSmallerMatch(availableLines, ref quantityNotApplied, discountLineQuantity, isReturn);
                        }
    
                        // If we still have not found a match, find the smallest higher-quantity line and split it.
                        if (!discountFullyApplied && !discountPartiallyApplied)
                        {
                            discountFullyApplied = this.ApplyDiscountLineForLargerMatch(transaction, availableLines, ref quantityNotApplied, discountLineQuantity, isReturn);
                        }
    
                        // Keep going if discount is only partial - not fully - applied.
                        keepGoing = !discountFullyApplied && discountPartiallyApplied;
                    }
                }
            }
    
            /// <summary>
            /// Splits a SalesLine into two sales lines in cases where a discount only applies to a portion of the quantity.
            /// </summary>
            /// <param name="salesLine">The line to split.</param>
            /// <param name="quantityNeeded">The quantity to split away from this line.</param>
            /// <returns>The new SalesLine containing the needed quantity.</returns>
            private SalesLine SplitLine(SalesLine salesLine, decimal quantityNeeded)
            {
                if (salesLine.Quantity < 0)
                {
                    quantityNeeded *= -1;
                }
    
                // Create the duplicate sale line.
                SalesLine newLine = salesLine.Clone<SalesLine>();
                newLine.Quantity = quantityNeeded;
                newLine.QuantityDiscounted = 0m;
                newLine.LineId = Guid.NewGuid().ToString("N");
                newLine.OriginLineId = salesLine.OriginLineId;
    
                if (salesLine.LineManualDiscountAmount != decimal.Zero && salesLine.Quantity > 0)
                {
                    newLine.LineManualDiscountAmount = this.priceContext.CurrencyAndRoundingHelper.Round((salesLine.LineManualDiscountAmount / salesLine.Quantity) * newLine.Quantity);
                    salesLine.LineManualDiscountAmount -= newLine.LineManualDiscountAmount;
                }
    
                // Set the new quantity on the orgininal sale line item.
                salesLine.Quantity -= quantityNeeded;
    
                return newLine;
            }
    
            private bool ApplyDiscountLineForLargerMatch(
                SalesTransaction transaction,
                HashSet<int> availableLines,
                ref decimal quantityNeeded,
                DiscountLineQuantity discount,
                bool isReturn)
            {
                return this.ApplyDiscountLineForLargerMatch(
                    transaction,
                    availableLines,
                    ref quantityNeeded,
                    discount,
                    isReturn,
                    requiresExistingCompoundedDiscounts: false);
            }
    
            /// <summary>
            /// Attempts to apply a DiscountLine to a SalesLine that has a larger quantity than the required quantity, splitting the line if it is found.
            /// </summary>
            /// <param name="transaction">The current transaction containing the lines.</param>
            /// <param name="availableLines">The available line indices on the transaction.</param>
            /// <param name="quantityNeeded">The quantity needed for the DiscountLine.</param>
            /// <param name="discount">The DiscountLine and original quantity needed.</param>
            /// <param name="isReturn">True if it's return.</param>
            /// <param name="requiresExistingCompoundedDiscounts">A flag indicating whether it requires existing compounded discounts on the line to compound on top of.</param>
            /// <returns>True if a match was found and the discount was applied, false otherwise.</returns>
            private bool ApplyDiscountLineForLargerMatch(
                SalesTransaction transaction,
                HashSet<int> availableLines,
                ref decimal quantityNeeded,
                DiscountLineQuantity discount,
                bool isReturn,
                bool requiresExistingCompoundedDiscounts)
            {
                bool discountApplied = false;
    
                foreach (int x in availableLines.ToList().OrderBy(p => this.salesLines[p].Quantity))
                {
                    SalesLine salesLine = this.salesLines[x];
    
                    if (!IsQuantityMatchSalesOrReturn(isReturn, salesLine.Quantity))
                    {
                        continue;
                    }
    
                    decimal lineQuantity = Math.Abs(salesLine.Quantity);
                    if (lineQuantity > quantityNeeded)
                    {
                        if (discount.DiscountLine.ConcurrencyMode != ConcurrencyMode.Compounded
                            || CanApplyCompoundedDiscount(this.salesLines[x].DiscountLines, discount.DiscountLine, requiresExistingCompoundedDiscounts))
                        {
                            // Perform the split of this line
                            SalesLine newLine = this.SplitLine(this.salesLines[x], quantityNeeded);
    
                            // Add the new line to the transaction and to the available lines for discounts.  Set the line number to the next available number.
                            newLine.LineNumber = transaction.SalesLines.Max(p => p.LineNumber) + 1;
                            transaction.SalesLines.Add(newLine);
                            this.salesLines.Add(newLine);
    
                            DiscountLine discountLine = discount.DiscountLine.Clone<DiscountLine>();
                            discountLine.SaleLineNumber = newLine.LineNumber;
                            newLine.DiscountLines.Add(discountLine);
                            newLine.QuantityDiscounted = quantityNeeded * Math.Sign(salesLine.Quantity);
    
                            // If this is a compounding discount, add the new line to the available lines.
                            if (discount.DiscountLine.ConcurrencyMode == ConcurrencyMode.Compounded)
                            {
                                availableLines.Add(this.salesLines.Count - 1);
                            }
    
                            discountApplied = true;
                            quantityNeeded = 0;
                            break;
                        }
                    }
                }
    
                return discountApplied;
            }
    
            private bool ApplyDiscountLineForSmallerMatch(
                HashSet<int> availableLines,
                ref decimal quantityNeeded,
                DiscountLineQuantity discount,
                bool isReturn)
            {
                return this.ApplyDiscountLineForSmallerMatch(
                    availableLines,
                    ref quantityNeeded,
                    discount,
                    isReturn,
                    requiresExistingCompoundedDiscounts: false);
            }
    
            /// <summary>
            /// Attempts to apply a DiscountLine to a SalesLine that has a smaller quantity than the required quantity.
            /// </summary>
            /// <param name="availableLines">The available line indices on the transaction.</param>
            /// <param name="quantityNeeded">The quantity needed for the DiscountLine.</param>
            /// <param name="discount">The DiscountLine and original quantity needed.</param>
            /// <param name="isReturn">True if it's return.</param>
            /// <param name="requiresExistingCompoundedDiscounts">A flag indicating whether it requires existing compounded discounts on the line to compound on top of.</param>
            /// <returns>True if the discount was applied to a line, false otherwise.</returns>
            private bool ApplyDiscountLineForSmallerMatch(
                HashSet<int> availableLines,
                ref decimal quantityNeeded,
                DiscountLineQuantity discount,
                bool isReturn,
                bool requiresExistingCompoundedDiscounts)
            {
                bool discountPartiallyApplied = false;
    
                foreach (int x in availableLines.ToList().OrderByDescending(p => this.salesLines[p].Quantity))
                {
                    SalesLine salesLine = this.salesLines[x];
    
                    if (!IsQuantityMatchSalesOrReturn(isReturn, salesLine.Quantity))
                    {
                        continue;
                    }
    
                    decimal lineQuantity = Math.Abs(salesLine.Quantity);
                    if (lineQuantity < quantityNeeded)
                    {
                        if (discount.DiscountLine.ConcurrencyMode != ConcurrencyMode.Compounded
                            || CanApplyCompoundedDiscount(this.salesLines[x].DiscountLines, discount.DiscountLine, requiresExistingCompoundedDiscounts))
                        {
                            DiscountLine discountLine = discount.DiscountLine.Clone<DiscountLine>();
                            discountLine.SaleLineNumber = salesLine.LineNumber;
                            salesLine.DiscountLines.Add(discountLine);
                            salesLine.QuantityDiscounted = salesLine.Quantity;
    
                            if (discount.DiscountLine.ConcurrencyMode != ConcurrencyMode.Compounded)
                            {
                                availableLines.Remove(x);
                            }
    
                            discountPartiallyApplied = true;
                            quantityNeeded -= lineQuantity;
                            break;
                        }
                    }
                }
    
                return discountPartiallyApplied;
            }
    
            private bool ApplyDiscountLineForDirectMatch(
                HashSet<int> availableLines,
                decimal quantityNeeded,
                DiscountLineQuantity discount,
                bool isReturn)
            {
                return this.ApplyDiscountLineForDirectMatch(
                    availableLines,
                    quantityNeeded,
                    discount,
                    isReturn,
                    requiresExistingCompoundedDiscounts: false);
            }
    
            /// <summary>
            /// Attempts to apply a DiscountLine to a SalesLine that has exactly the required quantity.
            /// </summary>
            /// <param name="availableLines">The available line indices on the transaction.</param>
            /// <param name="quantityNeeded">The quantity needed for the DiscountLine.</param>
            /// <param name="discount">The DiscountLine and original quantity needed.</param>
            /// <param name="isReturn">True if it's return.</param>
            /// <param name="requiresExistingCompoundedDiscounts">A flag indicating whether it requires existing compounded discounts on the line to compound on top of.</param>
            /// <returns>True if an exact match was found and the discount was applied, false otherwise.</returns>
            private bool ApplyDiscountLineForDirectMatch(
                HashSet<int> availableLines,
                decimal quantityNeeded,
                DiscountLineQuantity discount,
                bool isReturn,
                bool requiresExistingCompoundedDiscounts)
            {
                bool discountApplied = false;
    
                foreach (int x in availableLines.ToList())
                {
                    SalesLine salesLine = this.salesLines[x];
    
                    if (!IsQuantityMatchSalesOrReturn(isReturn, salesLine.Quantity))
                    {
                        continue;
                    }
    
                    decimal lineQuantity = Math.Abs(salesLine.Quantity);
                    if (lineQuantity == quantityNeeded)
                    {
                        if (discount.DiscountLine.ConcurrencyMode != ConcurrencyMode.Compounded
                            || CanApplyCompoundedDiscount(this.salesLines[x].DiscountLines, discount.DiscountLine, requiresExistingCompoundedDiscounts))
                        {
                            DiscountLine discountLine = discount.DiscountLine.Clone<DiscountLine>();
                            discountLine.SaleLineNumber = salesLine.LineNumber;
                            salesLine.DiscountLines.Add(discountLine);
                            salesLine.QuantityDiscounted = quantityNeeded * Math.Sign(salesLine.Quantity);
    
                            if (discount.DiscountLine.ConcurrencyMode != ConcurrencyMode.Compounded)
                            {
                                availableLines.Remove(x);
                            }
    
                            discountApplied = true;
                            break;
                        }
                    }
                }
    
                return discountApplied;
            }
    
            private class SalesLineDiscountedPriceComparer
            {
                private Dictionary<int, decimal> salesLineIndexToDiscountedPriceDictionary;
                private List<SalesLine> salesLines;
    
                internal SalesLineDiscountedPriceComparer(
                    Dictionary<int, decimal> salesLineIndexToDiscountedPriceDictionary,
                    List<SalesLine> salesLines)
                {
                    this.salesLineIndexToDiscountedPriceDictionary = salesLineIndexToDiscountedPriceDictionary;
                    this.salesLines = salesLines;
                }
    
                internal int CompareNetPrice(int left, int right)
                {
                    // It results in ascending order by discountedPrice.
                    int ret = Math.Sign((this.salesLineIndexToDiscountedPriceDictionary[left] * this.salesLines[left].Quantity) -
                        (this.salesLineIndexToDiscountedPriceDictionary[right] * this.salesLines[right].Quantity));
    
                    return ret;
                }
            }
        }
    }
}
