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
    
        /// <summary>
        /// Discount deal estimate.
        /// </summary>
        public class DiscountDealEstimate
        {
            private bool isDiscountAmountPerOverlappedQuantitySet = false;
    
            private decimal discountAmountPerOverlappedQuantity;
    
            internal DiscountDealEstimate()
            {
            }
    
            internal DiscountDealEstimate(
                bool canCompound,
                string offerId,
                decimal totalApplicableQuantityWithOverlapped,
                decimal totalDiscountAmountWithOverlapped,
                decimal totalApplicableQuantityWithoutOverlapped,
                decimal totalDiscountAmountWithoutOverlapped,
                Dictionary<int, decimal> itemGroupIndexToQuantityNeededFromOverlappedLookup)
            {
                this.CanCompound = canCompound;
                this.OfferId = offerId;
    
                this.TotalApplicableQuantityWithOverlapped = totalApplicableQuantityWithOverlapped;
                this.TotalDiscountAmountWithOverlapped = totalDiscountAmountWithOverlapped;
    
                this.TotalApplicableQuantityWithoutOverlapped = totalApplicableQuantityWithoutOverlapped;
                this.TotalDiscountAmountWithoutOverlapped = totalDiscountAmountWithoutOverlapped;
    
                this.ItemGroupIndexToQuantityNeededFromOverlappedLookup = itemGroupIndexToQuantityNeededFromOverlappedLookup;
            }
    
            internal bool CanCompound { get; private set; }
    
            internal string OfferId { get; private set; }
    
            internal decimal TotalApplicableQuantityWithOverlapped { get; private set; }
    
            internal decimal TotalDiscountAmountWithOverlapped { get; private set; }
    
            internal decimal TotalApplicableQuantityWithoutOverlapped { get; private set; }
    
            internal decimal TotalDiscountAmountWithoutOverlapped { get; private set; }
    
            internal Dictionary<int, decimal> ItemGroupIndexToQuantityNeededFromOverlappedLookup { get; private set; }
    
            internal decimal TotalQuantityNeededFromOverlapped
            {
                get
                {
                    decimal quantityNeeded = this.ItemGroupIndexToQuantityNeededFromOverlappedLookup != null ?
                                this.ItemGroupIndexToQuantityNeededFromOverlappedLookup.Sum(p => p.Value) :
                                decimal.Zero;
    
                    quantityNeeded = Math.Min(quantityNeeded, this.TotalApplicableQuantityWithOverlapped);
    
                    return quantityNeeded;
                }
            }
    
            /// <summary>
            /// Gets the marginal value of overlapped items.
            /// </summary>
            /// <remarks>
            /// The value of overlapped items is the difference of total discount amount with and without overlapped items,
            ///   divided by (estimated) overlapped quantity needed.
            /// </remarks>
            internal decimal MarginalDiscountAmountPerOverlappedQuantity
            {
                get
                {
                    if (!this.isDiscountAmountPerOverlappedQuantitySet)
                    {
                        decimal totalDiscountAmountFromOverlappedQuantity = this.TotalDiscountAmountWithOverlapped - this.TotalDiscountAmountWithoutOverlapped;
    
                        this.discountAmountPerOverlappedQuantity = this.TotalQuantityNeededFromOverlapped > decimal.Zero ?
                                    totalDiscountAmountFromOverlappedQuantity / this.TotalQuantityNeededFromOverlapped :
                                    decimal.MaxValue;
    
                        this.isDiscountAmountPerOverlappedQuantitySet = true;
                    }
    
                    return this.discountAmountPerOverlappedQuantity;
                }
            }
    
            /// <summary>
            /// Compare two estimates.
            /// </summary>
            /// <param name="x">First estimate.</param>
            /// <param name="y">Second estimate.</param>
            /// <returns>Comparison result.</returns>
            internal static int Compare(DiscountDealEstimate x, DiscountDealEstimate y)
            {
                if (x != null && y != null)
                {
                    return x.MarginalDiscountAmountPerOverlappedQuantity.CompareTo(y.MarginalDiscountAmountPerOverlappedQuantity);
                }
                else if (y == null)
                {
                    return 1;
                }
    
                return -1;
            }
    
            internal static DiscountDealEstimate Combine(DiscountDealEstimate x, DiscountDealEstimate y)
            {
                if (!x.CanCompound || !y.CanCompound)
                {
                    return x;
                }
    
                Dictionary<int, decimal> combinedLookup = new Dictionary<int, decimal>(x.ItemGroupIndexToQuantityNeededFromOverlappedLookup);
    
                foreach (KeyValuePair<int, decimal> pair in y.ItemGroupIndexToQuantityNeededFromOverlappedLookup)
                {
                    int itemGroupIndex = pair.Key;
                    decimal quantity = pair.Value;
    
                    decimal existingQuantity = decimal.Zero;
    
                    // Takes the max quantity of the two.
                    if (combinedLookup.TryGetValue(itemGroupIndex, out existingQuantity))
                    {
                        combinedLookup[itemGroupIndex] = Math.Max(quantity, existingQuantity);
                    }
                    else
                    {
                        combinedLookup[itemGroupIndex] = quantity;
                    }
                }
    
                return new DiscountDealEstimate(
                    true,
                    string.Empty,
                    x.TotalApplicableQuantityWithOverlapped + y.TotalApplicableQuantityWithOverlapped,
                    x.TotalDiscountAmountWithOverlapped + y.TotalDiscountAmountWithOverlapped,
                    x.TotalApplicableQuantityWithoutOverlapped + y.TotalApplicableQuantityWithoutOverlapped,
                    x.TotalDiscountAmountWithoutOverlapped + y.TotalDiscountAmountWithoutOverlapped,
                    combinedLookup);
            }
    
            internal static Comparison<DiscountDealEstimate> GetComparison()
            {
                return new Comparison<DiscountDealEstimate>(DiscountDealEstimate.Compare);
            }
    
    #if DEBUG
            internal static void DebugDisplayList(List<DiscountDealEstimate> estimates)
            {
                foreach (DiscountDealEstimate estimate in estimates)
                {
                    estimate.DebugDisplay();
                }
            }
    
            internal void DebugDisplay()
            {
                StringBuilder overlappedItemQuantities = new StringBuilder();
                bool isFirst = true;
                foreach (KeyValuePair<int, decimal> pair in this.ItemGroupIndexToQuantityNeededFromOverlappedLookup)
                {
                    if (isFirst)
                    {
                        overlappedItemQuantities.AppendFormat("{0}x{1}", pair.Key, pair.Value);
                        isFirst = false;
                    }
                    else
                    {
                        overlappedItemQuantities.AppendFormat(",{0}x{1}", pair.Key, pair.Value);
                    }
                }
    
                System.Diagnostics.Debug.WriteLine(
                    "  [Estimate({0})] Overlapped: qty [{1}] amount [{2}] Non-overlapped: qty [{3}] amount [{4}] average $ [{5}] QTY [{6}] Can Compound? [{7}]",
                    this.OfferId,
                    this.TotalApplicableQuantityWithOverlapped,
                    this.TotalDiscountAmountWithOverlapped,
                    this.TotalApplicableQuantityWithoutOverlapped,
                    this.TotalDiscountAmountWithoutOverlapped,
                    this.MarginalDiscountAmountPerOverlappedQuantity,
                    overlappedItemQuantities.ToString(),
                    this.CanCompound);
            }
    #endif
        }
    }
}
