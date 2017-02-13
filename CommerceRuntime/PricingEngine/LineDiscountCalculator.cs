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
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// This class is used to find and populate line discount trade agreement discounts on a transaction.
        /// </summary>
        internal sealed class LineDiscountCalculator
        {
            private DiscountParameters discountParameters;
            private PriceContext priceContext;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="LineDiscountCalculator"/> class with given configurations.
            /// </summary>
            /// <param name="discountParameters">Configuration dictating which trade agreement discount combinations are active.</param>
            /// <param name="priceContext">Price context.</param>
            public LineDiscountCalculator(DiscountParameters discountParameters, PriceContext priceContext)
            {
                this.discountParameters = discountParameters;
                this.priceContext = priceContext;
            }

            /// <summary>
            /// Calculates manual line discount sent from cashier.
            /// Should be called only after other discounts are calculated.
            /// </summary>
            /// <param name="transaction">Transaction to calculate manual total discounts on.</param>
            public void CalculateLineManualDiscount(SalesTransaction transaction)
            {
                if (transaction == null)
                {
                    throw new ArgumentNullException("transaction");
                }

                //DEMO4  //TODO:AM
                if (transaction.CartType == CartType.CustomerOrder)
                {
                    foreach (var salesLine in transaction.ActiveSalesLines)
                    {
                        DiscountLine lineDiscountItem = null;
                        if (salesLine.LineManualDiscountAmount != 0)
                        {
                            // Add a new line discount
                            lineDiscountItem = new DiscountLine
                            {
                                DiscountLineType = DiscountLineType.ManualDiscount,
                                ManualDiscountType = ManualDiscountType.LineDiscountAmount,
                                Amount =
                                    salesLine.Quantity != decimal.Zero
                                        ? salesLine.LineManualDiscountAmount / salesLine.Quantity
                                        : decimal.Zero,
                            };

                            this.AddLineDiscount(transaction, salesLine, lineDiscountItem);
                        }
                    }
                }
                else
                {
                    // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                    foreach (var salesLine in transaction.PriceCalculableSalesLines)
                    {
                        Discount.ClearManualDiscountLinesOfType(salesLine, ManualDiscountType.LineDiscountAmount);
                        Discount.ClearManualDiscountLinesOfType(salesLine, ManualDiscountType.LineDiscountPercent);

                        DiscountLine lineDiscountItem = null;

                        if (salesLine.LineManualDiscountPercentage != 0)
                        {
                            // Add a new line discount
                            lineDiscountItem = new DiscountLine
                            {
                                DiscountLineType = DiscountLineType.ManualDiscount,
                                ManualDiscountType = ManualDiscountType.LineDiscountPercent,
                                Percentage = salesLine.LineManualDiscountPercentage,
                            };

                            this.AddLineDiscount(transaction, salesLine, lineDiscountItem);
                        }

                        if (salesLine.LineManualDiscountAmount != 0)
                        {
                            // Add a new line discount
                            lineDiscountItem = new DiscountLine
                            {
                                DiscountLineType = DiscountLineType.ManualDiscount,
                                ManualDiscountType = ManualDiscountType.LineDiscountAmount,
                                Amount =
                                    salesLine.Quantity != decimal.Zero
                                        ? salesLine.LineManualDiscountAmount/salesLine.Quantity
                                        : decimal.Zero,
                            };

                            this.AddLineDiscount(transaction, salesLine, lineDiscountItem);
                        }
                    }
                }
            }

            /// <summary>
            /// The calculation of a customer line discount.
            /// </summary>
            /// <param name="tradeAgreements">Trade agreement collection to calculate on. If null, uses the pricing data manager to find agreements.</param>
            /// <param name="transaction">The sales transaction which needs line discounts..</param>
            /// <returns>
            /// The sales transaction.
            /// </returns>
            public SalesTransaction CalcLineDiscount(
                List<TradeAgreement> tradeAgreements,
                SalesTransaction transaction)
            {
                // Loop trough all items all calc line discount
                // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                if (tradeAgreements != null && tradeAgreements.Any())
                {
                    foreach (var saleItem in transaction.PriceCalculableSalesLines)
                    {
                        decimal absQty = Math.Abs(saleItem.Quantity);
                        decimal discountAmount = 0m;
                        decimal percent1 = 0m;
                        decimal percent2 = 0m;
                        decimal minQty = 0m;
    
                        this.GetLineDiscountLines(tradeAgreements, saleItem, ref absQty, ref discountAmount, ref percent1, ref percent2, ref minQty);
    
                        decimal totalPercentage = DiscountLine.GetCompoundedPercentage(percent1, percent2);
    
                        if ((totalPercentage != 0m) || (discountAmount != 0m))
                        {
                            DiscountLine discountItem = new DiscountLine
                            {
                                DiscountLineType = DiscountLineType.CustomerDiscount,
                                CustomerDiscountType = CustomerDiscountType.LineDiscount,
                                Percentage = totalPercentage,
                                Amount = discountAmount,
                            };
    
                            Discount.UpdateDiscountLines(saleItem, discountItem);
                        }
                    }
                }
    
                return transaction;
            }
    
            /// <summary>
            /// Calculate the manual line discount.
            /// </summary>
            /// <param name="transaction">The transaction receiving total discount lines.</param>
            /// <param name="saleItem">The sale item that contains the discount lines.</param>
            /// <param name="lineDiscountItem">The line discount amount to discount the transaction.</param>
            private void AddLineDiscount(SalesTransaction transaction, SalesLine saleItem, DiscountLine lineDiscountItem)
            {
                Item item = PriceContextHelper.GetItem(this.priceContext, saleItem.ItemId);
                bool isDiscountAllowed = item != null ? !item.NoDiscountAllowed : true;
                if (isDiscountAllowed)
                {
                    saleItem.DiscountLines.Add(lineDiscountItem);
                    SalesLineTotaller.CalculateLine(transaction, saleItem, d => this.priceContext.CurrencyAndRoundingHelper.Round(d));
                }
            }
    
            private void GetLineDiscountLines(
                List<TradeAgreement> tradeAgreements,
                SalesLine saleItem,
                ref decimal absQty,
                ref decimal discountAmount,
                ref decimal percent1,
                ref decimal percent2,
                ref decimal minQty)
            {
                int idx = 0;
                while (idx < 9)
                {
                    PriceDiscountItemCode itemCode = (PriceDiscountItemCode)(idx % 3);    // Mod divsion
                    PriceDiscountAccountCode accountCode = (PriceDiscountAccountCode)(idx / 3);
    
                    string accountRelation = string.Empty;
                    if (accountCode == PriceDiscountAccountCode.Customer)
                    {
                        accountRelation = this.priceContext.CustomerAccount;
                    }
                    else if (accountCode == PriceDiscountAccountCode.CustomerGroup)
                    {
                        accountRelation = this.priceContext.CustomerLinePriceGroup;
                    }
    
                    accountRelation = accountRelation ?? string.Empty;
    
                    string itemRelation;
                    if (itemCode == PriceDiscountItemCode.Item)
                    {
                        itemRelation = saleItem.ItemId;
                    }
                    else
                    {
                        Item item = PriceContextHelper.GetItem(this.priceContext, saleItem.ItemId);
                        itemRelation = item != null ? item.LineDiscountGroupId : string.Empty;
                    }
    
                    itemRelation = itemRelation ?? string.Empty;
    
                    PriceDiscountType relation = PriceDiscountType.LineDiscountSales; // Sales line discount - 5
    
                    if (this.discountParameters.Activation(relation, accountCode, itemCode))
                    {
                        if (DiscountParameters.ValidRelation(accountCode, accountRelation) &&
                            DiscountParameters.ValidRelation(itemCode, itemRelation))
                        {
                            bool dimensionDiscountFound = false;
    
                            if (saleItem.Variant != null && !string.IsNullOrEmpty(saleItem.Variant.VariantId))
                            {
                                var dimensionPriceDiscTable = Discount.GetPriceDiscData(tradeAgreements, relation, itemRelation, accountRelation, itemCode, accountCode, absQty, this.priceContext, saleItem.Variant, true);
    
                                foreach (TradeAgreement row in dimensionPriceDiscTable)
                                {
                                    bool unitsAreUndefinedOrEqual =
                                        string.IsNullOrEmpty(row.UnitOfMeasureSymbol) ||
                                         string.Equals(row.UnitOfMeasureSymbol, saleItem.SalesOrderUnitOfMeasure, StringComparison.OrdinalIgnoreCase);
    
                                    if (unitsAreUndefinedOrEqual)
                                    {
                                        percent1 += row.PercentOne;
                                        percent2 += row.PercentTwo;
                                        discountAmount += row.Amount;
                                        minQty += row.QuantityAmountFrom;
                                    }
    
                                    if (percent1 > 0M || percent2 > 0M || discountAmount > 0M)
                                    {
                                        dimensionDiscountFound = true;
                                    }
    
                                    if (!row.ShouldSearchAgain)
                                    {
                                        idx = 9;
                                    }
                                }
                            }
    
                            if (!dimensionDiscountFound)
                            {
                                var priceDiscTable = Discount.GetPriceDiscData(tradeAgreements, relation, itemRelation, accountRelation, itemCode, accountCode, absQty, this.priceContext, saleItem.Variant, false);
    
                                foreach (TradeAgreement row in priceDiscTable)
                                {
                                    // Apply default if the unit of measure is not set from the cart.
                                    string unitOfMeasure = Discount.GetUnitOfMeasure(saleItem);
    
                                    bool unitsAreUndefinedOrEqual =
                                        string.IsNullOrEmpty(row.UnitOfMeasureSymbol) ||
                                         string.Equals(row.UnitOfMeasureSymbol, unitOfMeasure, StringComparison.OrdinalIgnoreCase);
    
                                    if (unitsAreUndefinedOrEqual)
                                    {
                                        percent1 += row.PercentOne;
                                        percent2 += row.PercentTwo;
                                        discountAmount += row.Amount;
                                        minQty += row.QuantityAmountFrom;
                                    }
    
                                    if (!row.ShouldSearchAgain)
                                    {
                                        idx = 9;
                                    }
                                }
                            }
                        }
                    }
    
                    idx++;
                }
            }
        }
    }
}
