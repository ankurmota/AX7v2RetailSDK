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
        using System.Diagnostics.CodeAnalysis;
        using System.IO;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// This class encapsulates all logic for totaling a sales line once all the
        /// prices, discounts, charges, taxes, etc. have been set on it.
        /// </summary>
        public static class SalesLineTotaller
        {
            /// <summary>
            /// Populates the fields for total amount, total discount, and total taxes on the sales line.
            /// </summary>
            /// <param name="salesTransaction">The parent transaction for the sales line.</param>
            /// <param name="salesLine">The sales line to total.</param>
            /// <param name="salesRoundingRule">Delegate which can do sales rounding.</param>
            public static void CalculateLine(SalesTransaction salesTransaction, SalesLine salesLine, RoundingRule salesRoundingRule)
            {
                if (salesTransaction == null)
                {
                    throw new ArgumentNullException("salesTransaction");
                }
    
                CalculateLine(salesTransaction.BeginDateTime, salesTransaction.LineDiscountCalculationType, salesLine, salesRoundingRule, true);
            }
    
            /// <summary>
            /// Populates the fields for total amount, total discount, and total taxes on the sales line.
            /// </summary>
            /// <param name="transactionBeginDateTime">Transaction begin date time.</param>
            /// <param name="lineDiscountCalculationType">Line discount calculation type.</param>
            /// <param name="salesLine">The sales line to total.</param>
            /// <param name="salesRoundingRule">Delegate which can do sales rounding.</param>
            public static void CalculateLine(DateTime transactionBeginDateTime, LineDiscountCalculationType lineDiscountCalculationType, SalesLine salesLine, RoundingRule salesRoundingRule)
            {
                CalculateLine(transactionBeginDateTime, lineDiscountCalculationType, salesLine, salesRoundingRule, true);
            }
    
            private static void CalculateLine(DateTimeOffset transactionBeginDateTime, LineDiscountCalculationType lineDiscountCalculationType, SalesLine salesLine, RoundingRule salesRoundingRule, bool compareDiscounts)
            {
                if (salesLine == null)
                {
                    throw new ArgumentNullException("salesLine");
                }
    
                if (salesRoundingRule == null)
                {
                    throw new ArgumentNullException("salesRoundingRule");
                }
    
                decimal discountAmount = decimal.Zero;
    
                SalesLineTotaller.CalculateTax(salesLine);
    
                if ((salesLine.Blocked == false) && (salesLine.DateToActivateItem <= transactionBeginDateTime))
                {
                    salesLine.GrossAmount = salesRoundingRule(salesLine.Price * salesLine.Quantity);
    
                    if (!salesLine.IsPriceLocked || salesLine.QuantityOrdered != salesLine.Quantity)
                    {
                        salesLine.LineDiscount = 0;
                        salesLine.LinePercentageDiscount = 0;
                        salesLine.PeriodicDiscount = 0;
                        salesLine.PeriodicPercentageDiscount = 0;
                        salesLine.TotalDiscount = decimal.Zero;
                        salesLine.TotalPercentageDiscount = decimal.Zero;
                        salesLine.LoyaltyDiscountAmount = decimal.Zero;
                        salesLine.LoyaltyPercentageDiscount = decimal.Zero;
    
                        if (salesLine.IsVoided || salesLine.IsPriceOverridden)
                        {
                            salesLine.DiscountLines.Clear();
                        }
    
                        if (compareDiscounts)
                        {
                            ComparingDiscounts(transactionBeginDateTime, lineDiscountCalculationType, salesLine, salesRoundingRule);
                        }
    
                        AllocateDiscountAmountToDiscountLines(salesLine, lineDiscountCalculationType, salesRoundingRule);
                    }
                    else
                    {
                        FixDiscountAmountsOnSalesLine(salesLine, salesRoundingRule);
                    }
    
                    discountAmount = salesLine.PeriodicDiscount + salesLine.LineDiscount + salesLine.TotalDiscount + salesLine.LoyaltyDiscountAmount;
    
                    salesLine.NetAmountWithAllInclusiveTax = salesLine.GrossAmount - discountAmount;
    
                    // Removing exempt inclusive taxes.
                    // Update - Bug 938614, - not deducting anymore. If PM's later decide to deduct this inclusive tax then uncomment line below.
                    salesLine.NetAmount = salesLine.NetAmountWithAllInclusiveTax; // -salesLine.TaxAmountExemptInclusive;
    
                    if (salesLine.Quantity != 0)
                    {
                        salesLine.NetAmountPerUnit = salesLine.NetAmountWithNoTax() / salesLine.Quantity;
                    }
    
                    salesLine.UnitQuantity = salesLine.UnitOfMeasureConversion.Convert(salesLine.Quantity);
    
                    salesLine.DiscountAmount = discountAmount;
    
                    salesLine.TotalAmount = salesLine.NetAmountWithTax();
                    salesLine.NetAmountWithoutTax = salesLine.NetAmount - salesLine.TaxAmountInclusive;
                }
            }
    
            /// <summary>
            /// Fix discount amounts on SalesLine.
            /// </summary>
            /// <param name="salesLine">Sales line.</param>
            /// <param name="roundingRule">Rounding rule delegate.</param>
            /// <remarks>Recalled customer order may not have discount amount fields filled.</remarks>
            private static void FixDiscountAmountsOnSalesLine(SalesLine salesLine, RoundingRule roundingRule)
            {
                if (salesLine.IsPriceLocked && salesLine.DiscountAmount == decimal.Zero && salesLine.DiscountLines.Any())
                {
                    decimal periodicDiscount = decimal.Zero;
                    decimal lineDiscount = decimal.Zero;
                    decimal totalDiscount = decimal.Zero;
                    decimal loyaltyDiscount = decimal.Zero;
                    foreach (DiscountLine discountLine in salesLine.DiscountLines)
                    {
                        if (discountLine.DiscountLineType == DiscountLineType.PeriodicDiscount)
                        {
                            periodicDiscount += discountLine.EffectiveAmount;
                        }
                        else if (discountLine.DiscountLineType == DiscountLineType.CustomerDiscount &&
                            (discountLine.CustomerDiscountType == CustomerDiscountType.LineDiscount ||
                            discountLine.CustomerDiscountType == CustomerDiscountType.MultilineDiscount))
                        {
                            lineDiscount += discountLine.EffectiveAmount;
                        }
                        else if (discountLine.DiscountLineType == DiscountLineType.CustomerDiscount &&
                            discountLine.CustomerDiscountType == CustomerDiscountType.TotalDiscount)
                        {
                            totalDiscount += discountLine.EffectiveAmount;
                        }
                        else if (discountLine.DiscountLineType == DiscountLineType.ManualDiscount &&
                            (discountLine.ManualDiscountType == ManualDiscountType.LineDiscountAmount ||
                            discountLine.ManualDiscountType == ManualDiscountType.LineDiscountPercent))
                        {
                            lineDiscount += discountLine.EffectiveAmount;
                        }
                        else if (discountLine.DiscountLineType == DiscountLineType.ManualDiscount &&
                            (discountLine.ManualDiscountType == ManualDiscountType.TotalDiscountAmount ||
                            discountLine.ManualDiscountType == ManualDiscountType.TotalDiscountAmount))
                        {
                            totalDiscount += discountLine.EffectiveAmount;
                        }
                        else if (discountLine.DiscountLineType == DiscountLineType.LoyaltyDiscount)
                        {
                            loyaltyDiscount += discountLine.EffectiveAmount;
                        }
                        else
                        {
                            // Dump everything else into lineDiscount. We really shouldn't be here.
                            lineDiscount += discountLine.EffectiveAmount;
                        }
                    }
    
                    if (salesLine.GrossAmount == decimal.Zero)
                    {
                        salesLine.GrossAmount = roundingRule(salesLine.Price * salesLine.Quantity);
                    }
    
                    salesLine.PeriodicDiscount = periodicDiscount;
                    decimal grossAmount = salesLine.GrossAmount;
                    if (grossAmount != 0)
                    {
                        salesLine.PeriodicPercentageDiscount = Math.Round((periodicDiscount / grossAmount) * 100m, 2);
                    }
    
                    salesLine.LineDiscount = lineDiscount;
                    grossAmount -= salesLine.PeriodicDiscount;
                    if (grossAmount != 0)
                    {
                        salesLine.LinePercentageDiscount = Math.Round((lineDiscount / grossAmount) * 100m, 2);
                    }
    
                    salesLine.TotalDiscount = totalDiscount;
                    grossAmount -= salesLine.LineDiscount;
                    if (grossAmount != 0)
                    {
                        salesLine.TotalPercentageDiscount = Math.Round((totalDiscount / grossAmount) * 100m, 2);
                    }
    
                    salesLine.LoyaltyDiscountAmount = loyaltyDiscount;
                    grossAmount -= salesLine.TotalDiscount;
                    if (grossAmount != 0)
                    {
                        salesLine.LoyaltyPercentageDiscount = Math.Round((loyaltyDiscount / grossAmount) * 100m, 2);
                    }
    
                    salesLine.DiscountAmount = periodicDiscount + lineDiscount + totalDiscount + loyaltyDiscount;
                }
            }
    
            /// <summary>
            /// Calculates the tax amount properties on this taxable item.
            /// </summary>
            /// <param name="taxableItem">The taxable item.</param>
            private static void CalculateTax(TaxableItem taxableItem)
            {
                taxableItem.TaxAmount = taxableItem.TaxLines.Sum(t => t.IsExempt ? decimal.Zero : t.Amount);
                taxableItem.TaxAmountExclusive = taxableItem.TaxLines.Sum(t => (t.IsExempt || t.IsIncludedInPrice) ? decimal.Zero : t.Amount);
                taxableItem.TaxAmountInclusive = taxableItem.TaxLines.Sum(t => (t.IsExempt || !t.IsIncludedInPrice) ? decimal.Zero : t.Amount);
                taxableItem.TaxAmountExemptInclusive = taxableItem.TaxLines.Sum(t => (t.IsExempt && t.IsIncludedInPrice) ? t.Amount : decimal.Zero);
            }
    
            /// <summary>
            /// Compares the discounts on each of the sale line items:
            ///  If the sale line has both a customer and a periodic discount (other then Mix and Match) then
            ///     the discounts are compared and the better one is chosen and the other one taken of the sale line.
            ///  If the sale line has a customer discount and a Mix &amp; Match discount then the M&amp;M discount is always
            ///     chosen because there is no way to know the total discount of the M&amp;M because it consists of 2 or more sale lines.
            ///     So we assume that the M&amp;M is always better.
            /// </summary>
            /// <param name="transactionBeginDateTime">The transaction start date.</param>
            /// <param name="lineDiscountCalculationType">The line discount calculation type.</param>
            /// <param name="salesLine">The sales line.</param>
            /// <param name="salesRoundingRule">The rounding rule to use for the sales line.</param>
            private static void ComparingDiscounts(DateTimeOffset transactionBeginDateTime, LineDiscountCalculationType lineDiscountCalculationType, SalesLine salesLine, RoundingRule salesRoundingRule)
            {
                bool periodicDiscFound = false;
                bool customerDiscFound = false;
                bool mixAndMatchFound = false;
                bool thresholdFound = false;
                bool customerTotalDiscFound = false;
    
                // Go through all the discount lines and figure out what type of discount lines are available.
                foreach (var discountLine in salesLine.DiscountLines)
                {
                    if (discountLine.DiscountLineType == DiscountLineType.PeriodicDiscount)
                    {
                        if (discountLine.PeriodicDiscountType == PeriodicDiscountOfferType.MixAndMatch)
                        {
                            mixAndMatchFound = true;
                        }
                        else if (discountLine.PeriodicDiscountType == PeriodicDiscountOfferType.Threshold)
                        {
                            thresholdFound = true;
                        }
                        else
                        {
                            periodicDiscFound = true;
                        }
                    }
    
                    if (discountLine.DiscountLineType == DiscountLineType.CustomerDiscount)
                    {
                        if (discountLine.CustomerDiscountType != CustomerDiscountType.TotalDiscount)
                        {
                            customerDiscFound = true;
                        }
                        else
                        {
                            customerTotalDiscFound = true;
                        }
                    }
                }
    
                bool discardCustomerDiscounts = mixAndMatchFound || thresholdFound;
                bool discardTotalCustomerDiscount = thresholdFound;
    
                // If (Mix & Match or Threshold) and Customer Total Discount were found
                // then they will override any other discounts that are found on the sale line
                // no need to compare prices or discounts.
                if (discardCustomerDiscounts && customerTotalDiscFound)
                {
                    ClearCustomerDiscountLines(salesLine, discardTotalCustomerDiscount);
                    customerTotalDiscFound = false;
                    customerDiscFound = false;
                }
    
                // If (Mix & Match or Threshold) and Customer discount both found on the sale item
                // then the Mix & Match will override any customer discount
                if (discardCustomerDiscounts && customerDiscFound)
                {
                    ClearCustomerDiscountLines(salesLine, discardTotalCustomerDiscount);
                    customerDiscFound = false;
                }
    
                // If a Customer Discount is found and either a Multibuy or a Discount offer
                // the best price is found from either discount and the better one chosen.
                if (customerDiscFound && periodicDiscFound)
                {
                    // clone line and keep only customer discounts
                    SalesLine custSaleLineItem = salesLine.Clone<SalesLine>();
                    ClearDiscountLinesOfType(custSaleLineItem, DiscountLineType.PeriodicDiscount);
    
                    // clone line and keep only periodic discounts
                    SalesLine periodicSaleLineItem = salesLine.Clone<SalesLine>();
                    ClearDiscountLinesOfType(periodicSaleLineItem, DiscountLineType.CustomerDiscount);
    
                    CalculateLine(transactionBeginDateTime, lineDiscountCalculationType, custSaleLineItem, salesRoundingRule, false);
                    CalculateLine(transactionBeginDateTime, lineDiscountCalculationType, periodicSaleLineItem, salesRoundingRule, false);
    
                    if (Math.Abs(custSaleLineItem.NetAmount) >= Math.Abs(periodicSaleLineItem.NetAmount))
                    {
                        ClearCustomerDiscountLines(salesLine, false);
                    }
                    else
                    {
                        ClearDiscountLinesOfType(salesLine, DiscountLineType.PeriodicDiscount);
                    }
                }
            }
    
            /// <summary>
            /// Clear all customer discount lines.
            /// </summary>
            /// <param name="salesLine">The sales line.</param>
            /// <param name="deleteTotalCustomerDiscount">A value indicating whether the customer total discount should be deleted as well.</param>
            private static void ClearCustomerDiscountLines(SalesLine salesLine, bool deleteTotalCustomerDiscount)
            {
                if (salesLine == null)
                {
                    throw new ArgumentNullException("salesLine");
                }
    
                // Create a list for items to be removed
                var deleteList = new List<DiscountLine>();
                foreach (DiscountLine discountLine in salesLine.DiscountLines)
                {
                    if (discountLine.DiscountLineType == DiscountLineType.CustomerDiscount)
                    {
                        if (discountLine.CustomerDiscountType == CustomerDiscountType.TotalDiscount && deleteTotalCustomerDiscount)
                        {
                            deleteList.Add(discountLine);
                        }
                        else if (discountLine.CustomerDiscountType != CustomerDiscountType.TotalDiscount)
                        {
                            deleteList.Add(discountLine);
                        }
                    }
                }
    
                // Remove peridic discounts.
                foreach (DiscountLine discountLine in deleteList)
                {
                    salesLine.DiscountLines.Remove(discountLine);
                }
            }
    
            private static void ClearDiscountLinesOfType(SalesLine salesLine, DiscountLineType lineType)
            {
                var remainingDiscounts = salesLine.DiscountLines.Where(l => l.DiscountLineType != lineType).ToList();
                salesLine.DiscountLines.Clear();
                foreach (var discount in remainingDiscounts)
                {
                    salesLine.DiscountLines.Add(discount);
                }
    
                if (lineType == DiscountLineType.PeriodicDiscount)
                {
                    salesLine.PeriodicDiscountPossibilities.Clear();
                    salesLine.QuantityDiscounted = 0;
                }
    
                if (lineType == DiscountLineType.CustomerDiscount)
                {
                    salesLine.LineMultilineDiscOnItem = LineMultilineDiscountOnItem.None;
                }
            }
    
            private static void AllocateDiscountAmountToDiscountLines(SalesLine salesLine, LineDiscountCalculationType lineDiscountCalculationType, RoundingRule roundingRule)
            {
                if (salesLine.DiscountLines.Count == 0)
                {
                    return;
                }
    
                // customerLineAmountForMixOrMax and customerLinePercentForMixOrMax are for LineDiscountCalculationType.MaxLineMultiline and MinLineMultiline
                decimal customerLineAmountForMixOrMax = 0;
                decimal customerLinePercentForMixOrMax = 0;
    
                List<DiscountLine> periodicDiscountItemList = new List<DiscountLine>();
                List<DiscountLine> periodicThresholdDiscountItemList = new List<DiscountLine>();
                List<DiscountLine> customerLineDiscountItemList = new List<DiscountLine>();
    
                // Manual line discount at most one.
                DiscountLine manualLineDiscountItem = null;
                List<DiscountLine> totalDiscountItemList = new List<DiscountLine>();
                List<DiscountLine> loyaltyDiscountLineList = new List<DiscountLine>();
    
                //// Step 1: split discount lines into 5 groups: Periodic less threshold, Periodic threshold, Customer line, Manual line and Total and Loyalty
                ////         and figure out customerLineAmountForMixOrMax & customerLinePercentForMixOrMax along the way.
                foreach (DiscountLine discountLineItem in salesLine.DiscountLines)
                {
                    discountLineItem.FixInvalidAmountAndPercentage();
    
                    if (discountLineItem.DiscountLineType == DiscountLineType.CustomerDiscount)
                    {
                        // Customer Total
                        if (discountLineItem.CustomerDiscountType == CustomerDiscountType.TotalDiscount)
                        {
                            totalDiscountItemList.Add(discountLineItem);
                        }
                        else
                        {
                            // Customer Line
                            if (salesLine.LineMultilineDiscOnItem == LineMultilineDiscountOnItem.Both)
                            {
                                if (customerLineAmountForMixOrMax == decimal.Zero ||
                                    (lineDiscountCalculationType == LineDiscountCalculationType.MaxLineMultiline && discountLineItem.Amount > customerLineAmountForMixOrMax) ||
                                    (lineDiscountCalculationType == LineDiscountCalculationType.MinLineMultiline && discountLineItem.Amount > 0 && discountLineItem.Amount < customerLineAmountForMixOrMax))
                                {
                                    customerLineAmountForMixOrMax = discountLineItem.Amount;
                                }
    
                                if (customerLinePercentForMixOrMax == decimal.Zero ||
                                    (lineDiscountCalculationType == LineDiscountCalculationType.MaxLineMultiline && discountLineItem.Percentage > customerLinePercentForMixOrMax) ||
                                    (lineDiscountCalculationType == LineDiscountCalculationType.MinLineMultiline && discountLineItem.Percentage > 0  && discountLineItem.Percentage < customerLinePercentForMixOrMax))
                                {
                                    customerLinePercentForMixOrMax = discountLineItem.Percentage;
                                }
                            }
    
                            customerLineDiscountItemList.Add(discountLineItem);
                        }
                    }
                    else if (discountLineItem.DiscountLineType == DiscountLineType.PeriodicDiscount)
                    {
                        // Periodic a.k.a. Retail
                        if (discountLineItem.PeriodicDiscountType == PeriodicDiscountOfferType.Threshold)
                        {
                            periodicThresholdDiscountItemList.Add(discountLineItem);
                        }
                        else
                        {
                            periodicDiscountItemList.Add(discountLineItem);
                        }
                    }
                    else if (discountLineItem.DiscountLineType == DiscountLineType.ManualDiscount &&
                        (discountLineItem.ManualDiscountType == ManualDiscountType.LineDiscountAmount ||
                        discountLineItem.ManualDiscountType == ManualDiscountType.LineDiscountPercent))
                    {
                        // Line Manual
                        manualLineDiscountItem = discountLineItem;
                    }
                    else if (discountLineItem.DiscountLineType == DiscountLineType.ManualDiscount &&
                        (discountLineItem.ManualDiscountType == ManualDiscountType.TotalDiscountAmount ||
                        discountLineItem.ManualDiscountType == ManualDiscountType.TotalDiscountPercent))
                    {
                        // Total manual
                        totalDiscountItemList.Add(discountLineItem);
                    }
                    else if (discountLineItem.DiscountLineType == DiscountLineType.LoyaltyDiscount)
                    {
                        // Loyalty discount
                        loyaltyDiscountLineList.Add(discountLineItem);
                    }
                }
    
                salesLine.PeriodicDiscount = 0;
                salesLine.PeriodicPercentageDiscount = 0;
    
                // Step 2: allocate effective discount amount for periodic less threshold discount lines.
                AllocatePeriodicDiscountLines(salesLine, periodicDiscountItemList, roundingRule);
    
                // Step 3: allocate effective discount amount for periodic threshold discount lines.
                AllocatePeriodicDiscountLines(salesLine, periodicThresholdDiscountItemList, roundingRule);
    
                // Stpe 4: allocate effective discount amount for customer and manual line discounts.
                AllocateLineDiscountLines(salesLine, customerLineDiscountItemList, manualLineDiscountItem, lineDiscountCalculationType, customerLineAmountForMixOrMax, customerLinePercentForMixOrMax, roundingRule);
    
                // Stpe 4: allocate effective discount amount for total line discounts.
                AllocateTotalDiscountLines(salesLine, totalDiscountItemList, roundingRule);
    
                // Stpe 5: allocate effective discount amount for loyalty line discounts
                AllocateLoyaltyDiscountLines(salesLine, loyaltyDiscountLineList, roundingRule);
            }
    
            private static decimal AllocatePeriodicDiscountLines(SalesLine salesLine, List<DiscountLine> periodicDiscountItemList, RoundingRule roundingRule)
            {
                decimal periodicDiscountEffectiveAmount = 0;
                decimal periodicDiscountPercentage = salesLine.PeriodicPercentageDiscount;
                decimal quantityDiscounted = salesLine.Quantity;
                decimal grossAmountDiscountable = (salesLine.Price * quantityDiscounted) - salesLine.PeriodicDiscount;
                decimal totalPeriodicDiscountEffectiveAmount = salesLine.PeriodicDiscount;
    
                if (periodicDiscountItemList.Any() && grossAmountDiscountable != decimal.Zero)
                {
                    // Round 0: figure out amount for deal price, in which best price wins.
                    DiscountLine lineWithBestDealPrice = null;
                    foreach (DiscountLine periodicLine in periodicDiscountItemList)
                    {
                        if (periodicLine.DealPrice > decimal.Zero)
                        {
                            if (lineWithBestDealPrice == null)
                            {
                                lineWithBestDealPrice = periodicLine;
                            }
                            else if (periodicLine.DealPrice < lineWithBestDealPrice.DealPrice)
                            {
                                // Sets the amount = 0 on existing lineWithBestDealPrice, and then resets lineWithBestDealPrice.
                                lineWithBestDealPrice.Amount = 0;
                                lineWithBestDealPrice = periodicLine;
                            }
                            else
                            {
                                periodicLine.Amount = decimal.Zero;
                            }
                        }
                    }
    
                    if (lineWithBestDealPrice != null)
                    {
                        lineWithBestDealPrice.SetAmountForDealPrice(grossAmountDiscountable - periodicDiscountEffectiveAmount, quantityDiscounted, roundingRule);
                    }
    
                    // Round 1: amount off second.
                    foreach (DiscountLine periodicLine in periodicDiscountItemList)
                    {
                        periodicDiscountEffectiveAmount += periodicLine.SetEffectiveAmountForAmountOff(grossAmountDiscountable - periodicDiscountEffectiveAmount, quantityDiscounted, roundingRule);
                    }
    
                    // Round 2: percent off. If we have 2 periodic discounts applied to a single line, they much be compounded.
                    foreach (DiscountLine periodicLine in periodicDiscountItemList)
                    {
                        if (grossAmountDiscountable != periodicDiscountEffectiveAmount)
                        {
                            decimal grossAmountBase = grossAmountDiscountable - periodicDiscountEffectiveAmount;
                            decimal maxDiscountAmount = grossAmountBase;
                            periodicDiscountEffectiveAmount += periodicLine.AddEffectiveAmountForPercentOff(grossAmountBase, maxDiscountAmount, roundingRule);
                        }
                    }
    
                    totalPeriodicDiscountEffectiveAmount += periodicDiscountEffectiveAmount;
                    periodicDiscountPercentage = (totalPeriodicDiscountEffectiveAmount / grossAmountDiscountable) * 100m;
                }
    
                salesLine.PeriodicDiscount = totalPeriodicDiscountEffectiveAmount;
                salesLine.PeriodicPercentageDiscount = Math.Round(periodicDiscountPercentage, 2);
    
                return periodicDiscountEffectiveAmount;
            }
    
            private static decimal AllocateLineDiscountLines(
                SalesLine salesLine,
                List<DiscountLine> customerLineDiscountItemList,
                DiscountLine manualLineDiscountItem,
                LineDiscountCalculationType lineDiscountCalculationType,
                decimal customerLineAmountForMixOrMax,
                decimal customerLinePercentForMixOrMax,
                RoundingRule roundingRule)
            {
                decimal lineDiscountEffectiveAmount = 0;
                decimal lineDiscountEffectivePercentage = 0;
                decimal grossAmountDiscountable = (salesLine.Price * salesLine.Quantity) - salesLine.PeriodicDiscount;
    
                if (grossAmountDiscountable != decimal.Zero)
                {
                    // Round 1: amount off first for customer line discounts and manual line discount, in that order.
                    if (customerLineDiscountItemList.Any())
                    {
                        foreach (DiscountLine customerLine in customerLineDiscountItemList)
                        {
                            if (salesLine.LineMultilineDiscOnItem != LineMultilineDiscountOnItem.Both ||
                                lineDiscountCalculationType == LineDiscountCalculationType.LinePlusMultiline ||
                                lineDiscountCalculationType == LineDiscountCalculationType.LineMultiplyMultiline ||
                                (lineDiscountCalculationType == LineDiscountCalculationType.MaxLineMultiline && customerLine.Amount == customerLineAmountForMixOrMax) ||
                                (lineDiscountCalculationType == LineDiscountCalculationType.MinLineMultiline && customerLine.Amount == customerLineAmountForMixOrMax) ||
                                (lineDiscountCalculationType == LineDiscountCalculationType.Line && customerLine.CustomerDiscountType == CustomerDiscountType.LineDiscount) ||
                                (lineDiscountCalculationType == LineDiscountCalculationType.Multiline && customerLine.CustomerDiscountType == CustomerDiscountType.MultilineDiscount))
                            {
                                customerLine.SetEffectiveAmountForAmountOff(grossAmountDiscountable - lineDiscountEffectiveAmount, salesLine.Quantity, roundingRule);
                                lineDiscountEffectiveAmount += customerLine.EffectiveAmount;
    
                                // In case of Min and Max, customerLineAmountForMixOrMax can only be applied once.
                                customerLineAmountForMixOrMax = decimal.Zero;
                            }
                            else
                            {
                                customerLine.EffectiveAmount = decimal.Zero;
                            }
                        }
                    }
    
                    if (manualLineDiscountItem != null)
                    {
                        manualLineDiscountItem.SetEffectiveAmountForAmountOff(grossAmountDiscountable - lineDiscountEffectiveAmount, salesLine.Quantity, roundingRule);
                        lineDiscountEffectiveAmount += manualLineDiscountItem.EffectiveAmount;
                    }
    
                    decimal grossAmountDiscountableLessAmountOff = grossAmountDiscountable - lineDiscountEffectiveAmount;
                    decimal lineDiscountEffectiveAmountForPercentageOnly = 0;
    
                    // Round 2: percentage off for customer line discounts and manual line discount, in that order.
                    if (customerLineDiscountItemList.Any())
                    {
                        foreach (DiscountLine customerLine in customerLineDiscountItemList)
                        {
                            if (salesLine.LineMultilineDiscOnItem != LineMultilineDiscountOnItem.Both ||
                                lineDiscountCalculationType == LineDiscountCalculationType.LinePlusMultiline ||
                                lineDiscountCalculationType == LineDiscountCalculationType.LineMultiplyMultiline ||
                                (lineDiscountCalculationType == LineDiscountCalculationType.MaxLineMultiline && customerLine.Percentage == customerLinePercentForMixOrMax) ||
                                (lineDiscountCalculationType == LineDiscountCalculationType.MinLineMultiline && customerLine.Percentage == customerLinePercentForMixOrMax) ||
                                (lineDiscountCalculationType == LineDiscountCalculationType.Line && customerLine.CustomerDiscountType == CustomerDiscountType.LineDiscount) ||
                                (lineDiscountCalculationType == LineDiscountCalculationType.Multiline && customerLine.CustomerDiscountType == CustomerDiscountType.MultilineDiscount))
                            {
                                // Compound only when LineDiscountCalculationType.LineMultiplyMultiline.
                                decimal grossAmountBase = grossAmountDiscountableLessAmountOff;
                                if (lineDiscountCalculationType == LineDiscountCalculationType.LineMultiplyMultiline)
                                {
                                    grossAmountBase = grossAmountDiscountableLessAmountOff - lineDiscountEffectiveAmountForPercentageOnly;
                                }
    
                                decimal maxDiscountAmount = grossAmountDiscountable - lineDiscountEffectiveAmount;
                                decimal discountAmountAdded = customerLine.AddEffectiveAmountForPercentOff(grossAmountBase, maxDiscountAmount, roundingRule);
    
                                lineDiscountEffectiveAmount += discountAmountAdded;
                                lineDiscountEffectiveAmountForPercentageOnly += discountAmountAdded;
    
                                // In case of Min and Max, customerLineAmountForMixOrMax can only be applied once.
                                customerLineAmountForMixOrMax = decimal.Zero;
                            }
                        }
                    }
    
                    if (manualLineDiscountItem != null)
                    {
                        // Whether to add or to compound for manual discount follows the same rule for customer line and multiline discount.
                        decimal grossAmountBase = grossAmountDiscountableLessAmountOff;
                        if (lineDiscountCalculationType == LineDiscountCalculationType.LineMultiplyMultiline)
                        {
                            grossAmountBase = grossAmountDiscountableLessAmountOff - lineDiscountEffectiveAmountForPercentageOnly;
                        }
    
                        decimal maxDiscountAmount = grossAmountDiscountable - lineDiscountEffectiveAmount;
                        decimal discountAmountAdded = manualLineDiscountItem.AddEffectiveAmountForPercentOff(grossAmountBase, maxDiscountAmount, roundingRule);
    
                        lineDiscountEffectiveAmount += discountAmountAdded;
                        lineDiscountEffectiveAmountForPercentageOnly += discountAmountAdded;
                    }
    
                    lineDiscountEffectivePercentage = (lineDiscountEffectiveAmount / grossAmountDiscountable) * 100m;
                }
    
                salesLine.LineDiscount = lineDiscountEffectiveAmount;
                salesLine.LinePercentageDiscount = Math.Round(lineDiscountEffectivePercentage, 2);
    
                return lineDiscountEffectiveAmount;
            }
    
            private static decimal AllocateTotalDiscountLines(SalesLine salesLine, List<DiscountLine> totalDiscountItemList, RoundingRule roundingRule)
            {
                decimal totalDiscountEffectiveAmount = 0;
                decimal totalDiscountPercentage = 0;
                decimal grossAmountDiscountable = (salesLine.Price * salesLine.Quantity) - salesLine.PeriodicDiscount - salesLine.LineDiscount;
    
                if (totalDiscountItemList.Any() && grossAmountDiscountable != decimal.Zero)
                {
                    // Round 1: amount off fist.
                    foreach (DiscountLine totalLine in totalDiscountItemList)
                    {
                        totalDiscountEffectiveAmount += totalLine.SetEffectiveAmountForAmountOff(grossAmountDiscountable - totalDiscountEffectiveAmount, salesLine.Quantity, roundingRule);
                    }
    
                    // Round 2: percent off, plus percentage.
                    decimal grossAmountBase = grossAmountDiscountable - totalDiscountEffectiveAmount;
                    foreach (DiscountLine totalLine in totalDiscountItemList)
                    {
                        if (grossAmountDiscountable != totalDiscountEffectiveAmount)
                        {
                            decimal maxDiscountAmount = grossAmountDiscountable - totalDiscountEffectiveAmount;
                            totalDiscountEffectiveAmount += totalLine.AddEffectiveAmountForPercentOff(grossAmountBase, maxDiscountAmount, roundingRule);
                        }
                    }
    
                    totalDiscountPercentage = (totalDiscountEffectiveAmount / grossAmountDiscountable) * 100m;
                }
    
                salesLine.TotalDiscount = totalDiscountEffectiveAmount;
                salesLine.TotalPercentageDiscount = Math.Round(totalDiscountPercentage, 2);
    
                return totalDiscountEffectiveAmount;
            }
    
            private static decimal AllocateLoyaltyDiscountLines(SalesLine salesLine, IEnumerable<DiscountLine> loyaltyDiscountLineList, RoundingRule roundingRule)
            {
                decimal loyaltyDiscountEffectiveAmount = decimal.Zero;
                decimal loyaltyDiscountPercentage = decimal.Zero;
                decimal grossAmountDiscountable = (salesLine.Price * salesLine.Quantity) - salesLine.PeriodicDiscount - salesLine.LineDiscount - salesLine.TotalDiscount;
    
                if (loyaltyDiscountLineList.Any() && grossAmountDiscountable != decimal.Zero)
                {
                    // Round 1: amount off fist
                    foreach (DiscountLine loyaltyLine in loyaltyDiscountLineList)
                    {
                        loyaltyDiscountEffectiveAmount += loyaltyLine.SetEffectiveAmountForAmountOff(grossAmountDiscountable - loyaltyDiscountEffectiveAmount, salesLine.Quantity, roundingRule);
                    }
    
                    // Round 2: percent off, plus percentage
                    decimal grossAmountBase = grossAmountDiscountable - loyaltyDiscountEffectiveAmount;
                    foreach (DiscountLine loyaltyLine in loyaltyDiscountLineList)
                    {
                        if (grossAmountDiscountable != loyaltyDiscountEffectiveAmount)
                        {
                            decimal maxDiscountAmount = grossAmountDiscountable - loyaltyDiscountEffectiveAmount;
                            loyaltyDiscountEffectiveAmount += loyaltyLine.AddEffectiveAmountForPercentOff(grossAmountBase, maxDiscountAmount, roundingRule);
                        }
                    }
    
                    loyaltyDiscountPercentage = (loyaltyDiscountEffectiveAmount / grossAmountDiscountable) * 100m;
                }
    
                salesLine.LoyaltyDiscountAmount = loyaltyDiscountEffectiveAmount;
                salesLine.LoyaltyPercentageDiscount = Math.Round(loyaltyDiscountPercentage, 2);
    
                return loyaltyDiscountEffectiveAmount;
            }
        }
    }
}
