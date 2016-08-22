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
        /// This class is used to find and populate total discount trade agreement discounts and manual total discounts on a transaction.
        /// </summary>
        internal sealed class TotalDiscountCalculator
        {
            private DiscountParameters discountParameters;
            private PriceContext priceContext;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="TotalDiscountCalculator"/> class with given configurations.
            /// </summary>
            /// <param name="discountParameters">Configuration dictating which trade agreement discount combinations are active.</param>
            /// <param name="priceContext">Price context.</param>
            public TotalDiscountCalculator(DiscountParameters discountParameters, PriceContext priceContext)
            {
                this.discountParameters = discountParameters;
                this.priceContext = priceContext;
            }
    
            /// <summary>
            /// Calculates distribution of manual total discounts across the transaction.
            /// Should be called only after other discounts are calculated.
            /// </summary>
            /// <param name="transaction">Transaction to calculate manual total discounts on.</param>
            public void CalculateTotalManualDiscount(SalesTransaction transaction)
            {
                if (transaction == null)
                {
                    throw new ArgumentNullException("transaction");
                }
    
                Discount.ClearManualDiscountLinesOfType(transaction, ManualDiscountType.TotalDiscountPercent);
                Discount.ClearManualDiscountLinesOfType(transaction, ManualDiscountType.TotalDiscountAmount);
    
                // It's either $ off or % off, not both. In case of a bug where both are present, $ off wins.
                if (transaction.TotalManualDiscountAmount != 0)
                {
                    this.AddTotalDiscAmountLines(transaction, DiscountLineType.ManualDiscount, transaction.TotalManualDiscountAmount);
                }
                else if (transaction.TotalManualDiscountPercentage != 0)
                {
                    this.AddTotalDiscPctLines(transaction);
                }
            }
    
            /// <summary>
            /// The calculation of the total customer discount.
            /// </summary>
            /// <param name="tradeAgreements">Trade agreement collection to calculate on. If null, uses the pricing data manager to find agreements.</param>
            /// <param name="retailTransaction">The retail transaction which needs total discounts.</param>
            /// <returns>
            /// The retail transaction.
            /// </returns>
            public SalesTransaction CalcTotalCustomerDiscount(
                List<TradeAgreement> tradeAgreements,
                SalesTransaction retailTransaction)
            {
                if (tradeAgreements != null && tradeAgreements.Any())
                {
                    decimal totalAmount = 0;
    
                    // Find the total amount as a basis for the total discount
                    // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                    var clonedTransaction = retailTransaction.Clone<SalesTransaction>();
    
                    foreach (var clonedSalesLine in clonedTransaction.PriceCalculableSalesLines)
                    {
                        if (this.IsTotalDiscountAllowed(clonedSalesLine.ItemId))
                        {
                            SalesLineTotaller.CalculateLine(clonedTransaction, clonedSalesLine, d => this.priceContext.CurrencyAndRoundingHelper.Round(d));
                            totalAmount += clonedSalesLine.NetAmountWithAllInclusiveTax;
                        }
                    }
    
                    decimal absTotalAmount = Math.Abs(totalAmount);
    
                    // Find the total discounts.
                    PriceDiscountType relation = PriceDiscountType.EndDiscountSales; // Total sales discount - 7
                    PriceDiscountItemCode itemCode = PriceDiscountItemCode.AllItems; // All items - 2
                    PriceDiscountAccountCode accountCode = 0;
                    string itemRelation = string.Empty;
                    decimal percent1 = 0m;
                    decimal percent2 = 0m;
                    decimal discountAmount = 0m;
                    ProductVariant dimension = new ProductVariant();
    
                    int idx = 0;
                    while (idx < /* Max(PriceDiscAccountCode) */ 3)
                    {   // Check discounts for Store Currency
                        accountCode = (PriceDiscountAccountCode)idx;
    
                        string accountRelation = string.Empty;
                        if (accountCode == PriceDiscountAccountCode.Customer)
                        {
                            accountRelation = retailTransaction.CustomerId;
                        }
                        else if (accountCode == PriceDiscountAccountCode.CustomerGroup)
                        {
                            accountRelation = this.priceContext.CustomerTotalPriceGroup;
                        }
    
                        accountRelation = accountRelation ?? string.Empty;
    
                        // Only get Active discount combinations
                        if (this.discountParameters.Activation(relation, (PriceDiscountAccountCode)accountCode, (PriceDiscountItemCode)itemCode))
                        {
                            var priceDiscTable = Discount.GetPriceDiscData(tradeAgreements, relation, itemRelation, accountRelation, itemCode, accountCode, absTotalAmount, this.priceContext, dimension, false);
    
                            foreach (TradeAgreement row in priceDiscTable)
                            {
                                percent1 += row.PercentOne;
                                percent2 += row.PercentTwo;
                                discountAmount += row.Amount;
    
                                if (!row.ShouldSearchAgain)
                                {
                                    idx = 3;
                                }
                            }
                        }
    
                        idx++;
                    }
    
                    decimal totalPercentage = DiscountLine.GetCompoundedPercentage(percent1, percent2);
    
                    if (discountAmount != decimal.Zero)
                    {
                        this.AddTotalDiscAmountLines(retailTransaction, DiscountLineType.CustomerDiscount, discountAmount);
                    }
    
                    if (totalPercentage != 0)
                    {
                        // Update the sale items.
                        // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                        foreach (var saleItem in retailTransaction.PriceCalculableSalesLines)
                        {
                            if (this.IsTotalDiscountAllowed(saleItem.ItemId))
                            {
                                DiscountLine discountItem = GetCustomerDiscountItem(saleItem, CustomerDiscountType.TotalDiscount, DiscountLineType.CustomerDiscount);
                                discountItem.Percentage = totalPercentage;
                            }
                        }
                    }
                }
    
                return retailTransaction;
            }
    
            /// <summary>
            /// Retrieves a customer discount item of the indicated type if it exists and creates one if not.
            /// </summary>
            /// <param name="salesLine">The sales line from which to find customer discount lines.</param>
            /// <param name="customerDiscountType">The customer discount type.</param>
            /// <param name="lineDiscountType">The line discount type.</param>
            /// <returns>
            /// The discount line.
            /// </returns>
            private static DiscountLine GetCustomerDiscountItem(SalesLine salesLine, CustomerDiscountType customerDiscountType, DiscountLineType lineDiscountType)
            {
                DiscountLine discount;
                var discounts = from d in salesLine.DiscountLines
                                where
                                d.DiscountLineType == lineDiscountType &&
                                d.CustomerDiscountType == customerDiscountType
                                select d;
    
                // If the discount doesn't exist create a new one
                if (discounts.Count() == 0)
                {
                    discount = new DiscountLine
                    {
                        DiscountLineType = lineDiscountType,
                        CustomerDiscountType = customerDiscountType,
                    };
    
                    salesLine.DiscountLines.Add(discount);
                }
                else
                {
                    // otherwise select it.
                    discount = discounts.First();
                }
    
                return discount;
            }
    
            /// <summary>
            /// Adds total discount lines to the item lines.
            /// </summary>
            /// <param name="transaction">The transaction receiving total discount lines.</param>
            private void AddTotalDiscPctLines(SalesTransaction transaction)
            {
                // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                // Add the total discount to each item.
                foreach (var saleItem in transaction.PriceCalculableSalesLines)
                {
                    if (PriceContextHelper.IsDiscountAllowed(this.priceContext, saleItem.ItemId) && saleItem.Quantity > 0)
                    {
                        // Add a new total discount
                        DiscountLine totalDiscountItem = new DiscountLine
                        {
                            DiscountLineType = DiscountLineType.ManualDiscount,
                            ManualDiscountType = ManualDiscountType.TotalDiscountPercent,
                            Percentage = transaction.TotalManualDiscountPercentage,
                        };
                        saleItem.DiscountLines.Add(totalDiscountItem);
                        SalesLineTotaller.CalculateLine(transaction, saleItem, d => this.priceContext.CurrencyAndRoundingHelper.Round(d));
                    }
                }
            }
    
            /// <summary>
            /// This method will distribute the amountToDiscount across all the sale items in the transaction
            ///   proportionally except for the line item with the largest amount.  The remainder will be distributed
            ///   to the line item with the largest amount to ensure the amount to discount is exactly applied.
            /// This method currently works for either the customer discount or when the total discount button is applied.
            /// </summary>
            /// <param name="transaction">The transaction receiving total discount lines.</param>
            /// <param name="discountType">Whether this discount is for a customer or for the total discount item.</param>
            /// <param name="amountToDiscount">The amount to discount the transaction.</param>
            private void AddTotalDiscAmountLines(
                SalesTransaction transaction,
                DiscountLineType discountType,
                decimal amountToDiscount)
            {
                decimal totalAmtAvailableForDiscount = decimal.Zero;
    
                // Build a list of the discountable items with the largest value item last.
                // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                var discountableSaleItems = (from s in transaction.PriceCalculableSalesLines
                                             where s.IsEligibleForDiscount() && s.Quantity > 0
                                                 && PriceContextHelper.IsDiscountAllowed(this.priceContext, s.ItemId)
                                             orderby Math.Abs(s.NetAmount), s.LineId
                                             select s).ToList();
    
                // Iterate through all non voided items whether we are going to discount or not so that they get added
                // back to the totals
                // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                foreach (var saleItem in transaction.PriceCalculableSalesLines)
                {
                    // We can clear the discount line for total discount because a total manual amount discount
                    // will override a total manual percent discount, whereas customer discount can have both
                    // amount and percentage applied simultaneously.
                    if (discountType == DiscountLineType.ManualDiscount)
                    {
                        Discount.ClearManualDiscountLinesOfType(saleItem, ManualDiscountType.TotalDiscountAmount);
                        Discount.ClearManualDiscountLinesOfType(saleItem, ManualDiscountType.TotalDiscountPercent);
                    }
    
                    SalesLineTotaller.CalculateLine(transaction, saleItem, d => this.priceContext.CurrencyAndRoundingHelper.Round(d));
    
                    if (saleItem.IsEligibleForDiscount() && saleItem.Quantity > 0)
                    {
                        // Calculate the total amount that is available for discount
                        totalAmtAvailableForDiscount += Math.Abs(saleItem.NetAmountWithAllInclusiveTax);
                    }
                }
    
                // Calculate the percentage (as a fraction) that we should attempt to discount each discountable item
                // to reach the total.
                decimal discountFactor = totalAmtAvailableForDiscount != decimal.Zero ? (amountToDiscount / totalAmtAvailableForDiscount) : decimal.Zero;
    
                decimal totalAmtDistributed = decimal.Zero;
    
                // Iterate through all discountable items.
                foreach (var saleItem in discountableSaleItems)
                {
                    decimal amountToDiscountForThisItem = decimal.Zero;
    
                    if (saleItem != discountableSaleItems.Last())
                    {
                        // for every item except for the last in the list (which will have the largest value)
                        // discount by the rounded amount that is closest to the percentage desired for the transaction
                        decimal itemPrice = saleItem.NetAmount;
                        amountToDiscountForThisItem = this.priceContext.CurrencyAndRoundingHelper.Round(discountFactor * Math.Abs(itemPrice));
                        totalAmtDistributed += amountToDiscountForThisItem;
                    }
                    else
                    {
                        // Discount the last item by the remainder to ensure that the exact desired discount is applied
                        amountToDiscountForThisItem = amountToDiscount - totalAmtDistributed;
                    }
    
                    DiscountLine discountItem;
                    if (amountToDiscountForThisItem != decimal.Zero)
                    {
                        if (discountType == DiscountLineType.ManualDiscount)
                        {
                            // Add a new total discount item
                            discountItem = new DiscountLine();
                            discountItem.DiscountLineType = DiscountLineType.ManualDiscount;
                            discountItem.ManualDiscountType = ManualDiscountType.TotalDiscountAmount;
                            saleItem.DiscountLines.Add(discountItem);
                        }
                        else
                        {
                            // for customer discounts we need to either update the existing one, or add a new one.
                            discountItem = GetCustomerDiscountItem(saleItem, CustomerDiscountType.TotalDiscount, DiscountLineType.CustomerDiscount);
                        }
    
                        discountItem.Amount = saleItem.Quantity != 0 ? amountToDiscountForThisItem / saleItem.Quantity : amountToDiscountForThisItem;
                    }
    
                    SalesLineTotaller.CalculateLine(transaction, saleItem, d => this.priceContext.CurrencyAndRoundingHelper.Round(d));
                }
            }
    
            private bool IsTotalDiscountAllowed(string itemId)
            {
                Item item = PriceContextHelper.GetItem(this.priceContext, itemId);
                bool isTotalDiscountAllowed = item != null && item.IsTotalDiscountAllowed && !item.NoDiscountAllowed;
    
                return isTotalDiscountAllowed;
            }
        }
    }
}
