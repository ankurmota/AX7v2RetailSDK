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
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Single item non-overlapped discount result, for offer discounts and quantity discounts.
        /// </summary>
        /// <remarks>
        /// It's used for comparing and reducing overlapped discount offers and quantity discounts per item.
        /// See: DiscountCalculator.ReduceOverlappedOfferAndQuantityDiscountsPerItem.
        /// </remarks>
        public class SingleItemNonOverlappedDiscountResult
        {
            internal SingleItemNonOverlappedDiscountResult(
                DiscountOfferMethod discountMethod,
                decimal discountAmount,
                decimal discountPercentage,
                decimal offerPrice,
                decimal unitDiscountAmount,
                ConcurrencyMode concurrencyMode)
                : this(
                    discountMethod,
                    discountAmount,
                    discountPercentage,
                    offerPrice,
                    unitDiscountAmount,
                    concurrencyMode,
                    isIndependentOfOverlappedDiscounts: true)
            {
            }
    
            internal SingleItemNonOverlappedDiscountResult(
                DiscountOfferMethod discountMethod,
                decimal discountAmount,
                decimal discountPercentage,
                decimal offerPrice,
                decimal unitDiscountAmount,
                ConcurrencyMode concurrencyMode,
                bool isIndependentOfOverlappedDiscounts)
                : this(true)
            {
                this.DiscountMethod = discountMethod;
                this.DiscountAmount = discountAmount;
                this.DiscountPercentage = discountPercentage;
                this.OfferPrice = offerPrice;
                this.UnitDiscountAmount = unitDiscountAmount;
                this.CanCompound = concurrencyMode == ConcurrencyMode.Compounded;
                this.IsIndependentOfOverlappedDiscounts = isIndependentOfOverlappedDiscounts;
            }
    
            private SingleItemNonOverlappedDiscountResult(bool isApplicable)
            {
                this.IsApplicable = isApplicable;
            }
    
            internal static SingleItemNonOverlappedDiscountResult NotApplicable
            {
                get
                {
                    return new SingleItemNonOverlappedDiscountResult(false);
                }
            }
    
            internal bool IsApplicable { get; private set; }
    
            internal DiscountOfferMethod DiscountMethod { get; private set; }
    
            internal decimal DiscountAmount { get; private set; }
    
            internal decimal DiscountPercentage { get; private set; }
    
            internal decimal OfferPrice { get; private set; }
    
            internal decimal UnitDiscountAmount { get; private set; }
    
            internal bool CanCompound { get; private set; }
    
            internal bool IsIndependentOfOverlappedDiscounts { get; private set; }
    
            /// <summary>
            /// Compare two compounded results by application sequence.
            /// </summary>
            /// <param name="x">First single item non-overlapped discount result.</param>
            /// <param name="y">Second single item non-overlapped discount result.</param>
            /// <returns>Comparison result.</returns>
            public static int CompareCompoundedApplicationSequence(SingleItemNonOverlappedDiscountResult x, SingleItemNonOverlappedDiscountResult y)
            {
                int result = 0;
    
                if (x != null && y != null)
                {
                    if (x.IsApplicable && x.CanCompound && y.IsApplicable && y.CanCompound)
                    {
                        // The sort would put OfferPrice first, then DiscountAmount, and finally Percentage
                        result = Math.Sign(((int)y.DiscountMethod) - ((int)x.DiscountMethod));
    
                        if (result == 0)
                        {
                            switch (x.DiscountMethod)
                            {
                                case DiscountOfferMethod.OfferPrice:
                                    result = x.OfferPrice.CompareTo(y.OfferPrice);
                                    break;
                                case DiscountOfferMethod.DiscountAmount:
                                    result = y.DiscountAmount.CompareTo(x.DiscountAmount);
                                    break;
                                case DiscountOfferMethod.DiscountPercent:
                                    result = y.DiscountPercentage.CompareTo(x.DiscountPercentage);
                                    break;
                            }
                        }
                    }
                }
                else if (y == null)
                {
                    result = 1;
                }
                else
                {
                    result = -1;
                }
    
                return result;
            }
    
            internal static Comparison<SingleItemNonOverlappedDiscountResult> GetComparison()
            {
                return new Comparison<SingleItemNonOverlappedDiscountResult>(SingleItemNonOverlappedDiscountResult.CompareCompoundedApplicationSequence);
            }
    
    #if DEBUG
            internal void DebugDisplay()
            {
                System.Diagnostics.Debug.WriteLine(
                    "  Is applicable? [{0})] method [{1}] unit discount amount [{2}] independent? [{3}] compounded? [{4}]",
                    this.IsApplicable,
                    this.DiscountAmount,
                    this.UnitDiscountAmount,
                    this.IsIndependentOfOverlappedDiscounts,
                    this.CanCompound);
            }
    #endif
        }
    }
}
