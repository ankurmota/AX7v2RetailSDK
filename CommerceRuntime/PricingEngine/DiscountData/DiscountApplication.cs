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
        using System.Collections;
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Class representing a specific application of a discount to a set of line items on a transaction.
        /// </summary>
        public class DiscountApplication
        {
            private readonly decimal[] prices;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="DiscountApplication" /> class.
            /// </summary>
            /// <param name="discount">The discount.</param>
            public DiscountApplication(DiscountBase discount)
                : this(discount, false)
            {
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="DiscountApplication" /> class.
            /// </summary>
            /// <param name="discount">The discount.</param>
            /// <param name="applyStandalone">Whether to apply it standalone.</param>
            public DiscountApplication(DiscountBase discount, bool applyStandalone)
                : this(discount, applyStandalone, false)
            {
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="DiscountApplication" /> class.
            /// </summary>
            /// <param name="discount">The discount.</param>
            /// <param name="prices">Item prices, maybe discounted.</param>
            /// <param name="applyStandalone">Whether to apply it standalone.</param>
            public DiscountApplication(DiscountBase discount, decimal[] prices, bool applyStandalone)
                : this(discount, applyStandalone, false)
            {
                this.prices = prices;
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="DiscountApplication" /> class.
            /// </summary>
            /// <param name="discount">The discount.</param>
            /// <param name="applyStandalone">Whether to apply it standalone.</param>
            /// <param name="removeItemsFromLookupsWhenApplied">Whether to remove items from lookups when applied.</param>
            public DiscountApplication(DiscountBase discount, bool applyStandalone, bool removeItemsFromLookupsWhenApplied)
            {
                this.Discount = discount;
                this.ApplyStandalone = applyStandalone;
                this.RemoveItemsFromLookupsWhenApplied = removeItemsFromLookupsWhenApplied;
                this.NumberOfTimesApplicable = discount != null ? discount.NumberOfTimesApplicable : 0;
                this.ItemQuantities = new Dictionary<int, decimal>();
            }
    
            /// <summary>
            /// Gets the quantities of line items affected by this application of the discount.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Performance critical code.")]
            public Dictionary<int, decimal> ItemQuantities { get; private set; }
    
            /// <summary>
            /// Gets or the discount associated with this application.
            /// </summary>
            public DiscountBase Discount { get; private set; }
    
            /// <summary>
            /// Gets or sets the specific discount lines for the discount associated with this application.
            /// </summary>
            public IEnumerable<RetailDiscountLineItem> RetailDiscountLines { get; set; }
    
            /// <summary>
            /// Gets or sets the priority value to use when sorting this discount.
            /// </summary>
            public int SortIndex { get; set; }
    
            /// <summary>
            /// Gets or sets the amount value to use when sorting this discount.
            /// </summary>
            public decimal SortValue { get; set; }
    
            /// <summary>
            /// Gets or sets the discount code used for this application.
            /// </summary>
            public string DiscountCode { get; set; }
    
            /// <summary>
            /// Gets or sets the application-specific discount amount value.
            /// </summary>
            public decimal DiscountAmountValue { get; set; }
    
            /// <summary>
            /// Gets or sets the application-specific discount percentage value.
            /// </summary>
            public decimal DiscountPercentValue { get; set; }
    
            /// <summary>
            /// Gets or sets the application-specific deal or unit price value.
            /// </summary>
            public decimal DealPriceValue { get; set; }
    
            /// <summary>
            /// Gets or sets the number of times this offer can be applied to a transaction.
            /// </summary>
            public int NumberOfTimesApplicable { get; set; }
    
            internal bool ApplyStandalone { get; set; }
    
            internal bool HonorQuantity { get; set; }
    
            internal bool RemoveItemsFromLookupsWhenApplied { get; set; }

            /// <summary>
            /// Apply the discount applications, taking into account previously applied discounts.
            /// </summary>
            /// <param name="discountableItemGroups">The line items in the transaction.</param>
            /// <param name="remainingQuantities">The quantities remaining for each item.</param>
            /// <param name="appliedDiscounts">The previously applied discounts.</param>
            /// <param name="priceContext">The pricing context to use.</param>
            /// <returns>The value of this application of the discount.</returns>
            public AppliedDiscountApplication Apply(
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                IEnumerable<AppliedDiscountApplication> appliedDiscounts,
                PriceContext priceContext)
            {
                return this.Discount.GetAppliedDiscountApplication(
                    discountableItemGroups,
                    remainingQuantities,
                    appliedDiscounts,
                    this,
                    priceContext);
            }
    
            internal bool CanCompound(DiscountApplication discountApplication)
            {
                bool canCompound = true;
    
                if (this.Discount is OfferDiscount)
                {
                    canCompound = !object.ReferenceEquals(this, discountApplication);
                }
                else
                {
                    canCompound = !string.Equals(this.Discount.OfferId, discountApplication.Discount.OfferId, StringComparison.OrdinalIgnoreCase);
                }
    
                return canCompound;
            }
    
            internal decimal[] GetPrices(DiscountableItemGroup[] discountableItemGroups)
            {
                decimal[] prices = this.prices;
    
                if (prices == null)
                {
                    prices = new decimal[discountableItemGroups.Length];
    
                    foreach (KeyValuePair<int, decimal> pair in this.ItemQuantities)
                    {
                        int itemGroupIndex = pair.Key;
                        prices[itemGroupIndex] = discountableItemGroups[itemGroupIndex].Price;
                    }
                }
    
                return prices;
            }
        }
    }
}
