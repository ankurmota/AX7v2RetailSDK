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
    namespace Commerce.Runtime.Services.PricingEngine
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// This class is used to find and populate manual loyalty discount on a transaction.
        /// </summary>
        internal sealed class LoyaltyDiscountCalculator
        {
            private PriceContext priceContext;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="LoyaltyDiscountCalculator"/> class.
            /// </summary>
            /// <param name="priceContext">Price context.</param>
            public LoyaltyDiscountCalculator(PriceContext priceContext)
            {
                this.priceContext = priceContext;
            }
    
            /// <summary>
            /// Calculates distribution of manual loyalty discounts across the transaction.
            /// Should be called only after other discounts are calculated.
            /// </summary>
            /// <param name="transaction">Transaction to calculate manual loyalty discounts on.</param>
            public void CalculateLoyaltyManualDiscount(SalesTransaction transaction)
            {
                if (transaction == null)
                {
                    throw new ArgumentNullException("transaction");
                }
    
                Discount.ClearDiscountLinesOfType(transaction, DiscountLineType.LoyaltyDiscount);
    
                if (transaction.LoyaltyManualDiscountAmount.HasValue)
                {
                    this.AddLoyaltyDiscAmountLines(transaction, transaction.LoyaltyManualDiscountAmount.Value);
                }
            }
    
            /// <summary>
            /// This method will distribute the amountToDiscount across all the sale items in the transaction
            ///     proportionally except for the line item with the largest amount.  The remainder will be distributed
            ///     to the line item with the largest amount to ensure the amount to discount is exactly applied.
            /// This method currently works when the redeem loyalty points button is applied.
            /// </summary>
            /// <param name="transaction">The transaction receiving loyalty discount lines.</param>
            /// <param name="amountToDiscount">The amount to discount the transaction.</param>
            private void AddLoyaltyDiscAmountLines(
                SalesTransaction transaction,
                decimal amountToDiscount)
            {
                decimal totalAmtAvailableForDiscount = decimal.Zero;
    
                // Build a list of the discountable items with the largest value item last
                var discountableSaleItems = (from s in transaction.SalesLines
                                             where ((s.IsEligibleForDiscount() && PriceContextHelper.IsDiscountAllowed(this.priceContext, s.ItemId))
                                                || s.IsLoyaltyDiscountApplied)
                                             orderby Math.Abs(s.NetAmount), s.LineId
                                             select s).ToList();
    
                // Iterate through all non voided items whether we are going to discount or not so that they get added
                // back to the totals
                foreach (SalesLine salesLine in transaction.SalesLines.Where(sl => !sl.IsVoided))
                {
                    Discount.ClearDiscountLinesOfType(salesLine, DiscountLineType.LoyaltyDiscount);
                    SalesLineTotaller.CalculateLine(transaction, salesLine, d => this.priceContext.CurrencyAndRoundingHelper.Round(d));
    
                    if (salesLine.IsEligibleForDiscount() || salesLine.IsLoyaltyDiscountApplied)
                    {
                        // Calculate the total amount that is available for discount
                        totalAmtAvailableForDiscount += Math.Abs(salesLine.NetAmountWithAllInclusiveTax);
                    }
                }
    
                // Calculate the percentage (as a fraction) that we should attempt to discount each discountable item
                // to reach the total
                decimal discountFactor = totalAmtAvailableForDiscount != decimal.Zero ? (amountToDiscount / totalAmtAvailableForDiscount) : decimal.Zero;
    
                decimal totalAmtDistributed = decimal.Zero;
    
                // Iterate through all discountable items.
                foreach (SalesLine salesLine in discountableSaleItems)
                {
                    decimal amountToDiscountForThisItem = decimal.Zero;
    
                    if (salesLine != discountableSaleItems.Last())
                    {
                        // for every item except for the last in the list (which will have the largest value)
                        // discount by the rounded amount that is closest to the percentage desired for the transaction
                        decimal itemPrice = salesLine.NetAmount;
                        amountToDiscountForThisItem = this.priceContext.CurrencyAndRoundingHelper.Round(discountFactor * Math.Abs(itemPrice));
                        totalAmtDistributed += amountToDiscountForThisItem;
                    }
                    else
                    {
                        // Discount the last item by the remainder to ensure that the exact desired discount is applied
                        amountToDiscountForThisItem = amountToDiscount - totalAmtDistributed;
                    }
    
                    DiscountLine discountLine;
                    if (amountToDiscountForThisItem != decimal.Zero)
                    {
                        // Add a new loyalty points discount item
                        discountLine = new DiscountLine();
                        discountLine.Amount = salesLine.Quantity != 0 ? amountToDiscountForThisItem / salesLine.Quantity : amountToDiscountForThisItem;
                        discountLine.DiscountLineType = DiscountLineType.LoyaltyDiscount;
    
                        salesLine.DiscountLines.Add(discountLine);
                        salesLine.IsLoyaltyDiscountApplied = true;
                    }
    
                    SalesLineTotaller.CalculateLine(transaction, salesLine, d => this.priceContext.CurrencyAndRoundingHelper.Round(d));
                }
            }
        }
    }
}
