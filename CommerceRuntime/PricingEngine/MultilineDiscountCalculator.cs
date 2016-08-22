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
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// This class is used to find and populate multiline trade agreement discounts on a transaction.
        /// </summary>
        internal sealed class MultilineDiscountCalculator
        {
            private DiscountParameters discountParameters;
            private PriceContext priceContext;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="MultilineDiscountCalculator"/> class with given configurations.
            /// </summary>
            /// <param name="discountParameters">Configuration dictating which trade agreement discount combinations are active.</param>
            /// <param name="priceContext">Price context.</param>
            public MultilineDiscountCalculator(DiscountParameters discountParameters, PriceContext priceContext)
            {
                this.discountParameters = discountParameters;
                this.priceContext = priceContext;
            }
    
            /// <summary>
            /// The calculation of a customer multiline discount.
            /// </summary>
            /// <remarks>
            /// Calculation of multiline discount is done as follows:
            ///   1. Create working table for calculation.
            ///   2. Populate working table with total quantities for all the multiline groups encountered on the sales lines.
            ///   3. For all rows (and therefore multiline groups) found, search for trade agreements in the database.
            ///      a. The search is first for customer-specific, then customer multiline discount group, then all customers.
            ///      b. The search stops when a trade agreement is encountered with "Find next" unmarked.
            ///      c. If nothing is found for the store currency the search is attempted again with the company accounting currency.
            ///   4. All found agreements are summed in the working table and applied to each sales line which matches the multiline groups.
            ///   5. If there are sales lines which weren't discounted with a multiline discount.
            ///      a. Find their total quantity and search for any multiline trade agreements marked for "All items".
            ///      b. If any agreements were found apply them to any lines that weren't already discounted with a multiline discount.
            /// </remarks>
            /// <param name="tradeAgreements">Trade agreement collection to calculate on. If null, uses the pricing data manager to find agreements.</param>
            /// <param name="transaction">The sales transaction which needs multiline discounts attached.</param>
            /// <returns>
            /// The sales transaction.
            /// </returns>
            public SalesTransaction CalcMultiLineDiscount(List<TradeAgreement> tradeAgreements, SalesTransaction transaction)
            {
                if (tradeAgreements != null && tradeAgreements.Any())
                {
                    // collection of salesLine not discounted by multiline discount group
                    // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                    var nondiscountedSalesLines = new List<SalesLine>(transaction.PriceCalculableSalesLines);
    
                    Dictionary<string, decimal> priceGroupSalesQuantityLookup = this.GetPriceGroupSalesQuantityLookup(transaction);
    
                    decimal percent1 = decimal.Zero;
                    decimal percent2 = decimal.Zero;
                    decimal discountAmount = decimal.Zero;
    
                    // Find discounts for the different multiline discount groups
                    foreach (KeyValuePair<string, decimal> priceGroupQuantityPair in priceGroupSalesQuantityLookup)
                    {
                        // we've found some multiline discount groups, so clear non-discounted lines from the default of "all"
                        nondiscountedSalesLines.Clear();
    
                        // find multiline discounts for this multiline discount row
                        this.GetMultiLineDiscountLine(tradeAgreements, PriceDiscountItemCode.ItemGroup, transaction, priceGroupQuantityPair.Key, priceGroupQuantityPair.Value, out percent1, out percent2, out discountAmount);
    
                        // Update the sale items.
                        // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                        foreach (var saleItem in transaction.PriceCalculableSalesLines)
                        {
                            Item item = PriceContextHelper.GetItem(this.priceContext, saleItem.ItemId);
                            string discountGroupId = item != null ? item.MultilineDiscountGroupId : string.Empty;
                            if (string.Equals(discountGroupId, priceGroupQuantityPair.Key, StringComparison.OrdinalIgnoreCase))
                            {
                                // if line is part of discounted item group, apply the discount
                                ApplyMultilineDiscount(saleItem, percent1, percent2, discountAmount);
                            }
                            else
                            {
                                // otherwise, add to non-discounted lines
                                nondiscountedSalesLines.Add(saleItem);
                            }
                        }
                    }
    
                    // find total quantity of items on lines still eligible for multiline discount
                    decimal lineSum = nondiscountedSalesLines.Aggregate(0M, (acc, sl) => acc + sl.Quantity);
    
                    // find any multiline discounts to apply to "all items"
                    this.GetMultiLineDiscountLine(tradeAgreements, PriceDiscountItemCode.AllItems, transaction, string.Empty, lineSum, out percent1, out percent2, out discountAmount);
    
                    // Update the sale items.
                    foreach (var saleItem in nondiscountedSalesLines)
                    {
                        ApplyMultilineDiscount(saleItem, percent1, percent2, discountAmount);
                    }
                }
    
                return transaction;
            }
    
            /// <summary>
            /// Apply the given multiline discount row to the given sales line if discount amounts have been specified.
            /// </summary>
            /// <param name="salesLine">The sales line which will receive the discount.</param>
            /// <param name="percent1">Percentage one.</param>
            /// <param name="percent2">Percentage two.</param>
            /// <param name="discountAmount">Discount amount.</param>
            private static void ApplyMultilineDiscount(SalesLine salesLine, decimal percent1, decimal percent2, decimal discountAmount)
            {
                if (percent1 > decimal.Zero || percent2 > decimal.Zero || discountAmount > decimal.Zero)
                {
                    DiscountLine discountItem = new DiscountLine
                    {
                        DiscountLineType = DiscountLineType.CustomerDiscount,
                        CustomerDiscountType = CustomerDiscountType.MultilineDiscount,
                        Percentage = DiscountLine.GetCompoundedPercentage(percent1, percent2),
                        Amount = discountAmount,
                    };
    
                    Discount.UpdateDiscountLines(salesLine, discountItem);
                }
            }
    
            /// <summary>
            /// Find and total all multiline discount trade agreements that match the given relations and quantity.
            /// </summary>
            /// <param name="tradeAgreements">Trade agreement collection to calculate on.</param>
            /// <param name="itemCode">The item code to search by (item group or all).</param>
            /// <param name="retailTransaction">The transaction context with Id and customer Id.</param>
            /// <param name="priceGroup">Multiline price group.</param>
            /// <param name="salesQuantity">Aggregated quantity for multiline price group.</param>
            /// <param name="percent1">Percentage one.</param>
            /// <param name="percent2">Percentage two.</param>
            /// <param name="discountAmount">Discount amount.</param>
            private void GetMultiLineDiscountLine(
                List<TradeAgreement> tradeAgreements,
                PriceDiscountItemCode itemCode,
                SalesTransaction retailTransaction,
                string priceGroup,
                decimal salesQuantity,
                out decimal percent1,
                out decimal percent2,
                out decimal discountAmount)
            {
                PriceDiscountType relation = PriceDiscountType.MultilineDiscountSales; // Sales multiline discount - 6
                ProductVariant dimension = new ProductVariant();
    
                percent1 = decimal.Zero;
                percent2 = decimal.Zero;
                discountAmount = decimal.Zero;
    
                bool searchAgain = true;
                var codes = new PriceDiscountAccountCode[] { PriceDiscountAccountCode.Customer, PriceDiscountAccountCode.CustomerGroup, PriceDiscountAccountCode.AllCustomers };
                foreach (var accountCode in codes)
                {
                    // skip to next configuration if this one isn't enabled
                    if (!this.discountParameters.Activation(relation, accountCode, itemCode))
                    {
                        continue;
                    }
    
                    // get item relation based on item code
                    string itemRelation = (itemCode == PriceDiscountItemCode.ItemGroup) ? priceGroup : string.Empty;
                    itemRelation = itemRelation ?? string.Empty;
    
                    // get customer relation based on account code
                    string accountRelation = string.Empty;
                    if (accountCode == PriceDiscountAccountCode.Customer)
                    {
                        accountRelation = retailTransaction.CustomerId;
                    }
                    else if (accountCode == PriceDiscountAccountCode.CustomerGroup)
                    {
                        accountRelation = this.priceContext.CustomerMultipleLinePriceGroup;
                    }
    
                    accountRelation = accountRelation ?? string.Empty;
    
                    // if both relations are valid for the given item and account codes, look for trade agreements matching these relations
                    if (DiscountParameters.ValidRelation(accountCode, accountRelation) &&
                        DiscountParameters.ValidRelation(itemCode, itemRelation))
                    {
                        // get any active multiline discount trade agreement matching relations and quantity
                        var priceDiscTable = Discount.GetPriceDiscData(tradeAgreements, relation, itemRelation, accountRelation, itemCode, accountCode, salesQuantity, this.priceContext, dimension, false);
    
                        // compute running sum of discount values found
                        foreach (TradeAgreement row in priceDiscTable)
                        {
                            percent1 += row.PercentOne;
                            percent2 += row.PercentTwo;
                            discountAmount += row.Amount;
    
                            // stop search when we find a trade agreement set to not find next trade agreement
                            if (!row.ShouldSearchAgain)
                            {
                                searchAgain = false;
                            }
                        }
                    }
    
                    // stop search if we found a discount without "find next" marked
                    if (!searchAgain)
                    {
                        break;
                    }
                }
            }
    
            private Dictionary<string, decimal> GetPriceGroupSalesQuantityLookup(SalesTransaction transaction)
            {
                // Sum up all the linegroup discount lines in the same group
                // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                Dictionary<string, decimal> priceGroupSalesQuantityLookup = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                foreach (SalesLine salesLine in transaction.PriceCalculableSalesLines)
                {
                    Item item = PriceContextHelper.GetItem(this.priceContext, salesLine.ItemId);
                    if (item != null && !string.IsNullOrEmpty(item.MultilineDiscountGroupId))
                    {
                        decimal quantity = decimal.Zero;
    
                        if (priceGroupSalesQuantityLookup.TryGetValue(item.MultilineDiscountGroupId, out quantity))
                        {
                            priceGroupSalesQuantityLookup[item.MultilineDiscountGroupId] = quantity + salesLine.Quantity;
                        }
                        else
                        {
                            priceGroupSalesQuantityLookup.Add(item.MultilineDiscountGroupId, salesLine.Quantity);
                        }
                    }
                }
    
                return priceGroupSalesQuantityLookup;
            }
        }
    }
}
