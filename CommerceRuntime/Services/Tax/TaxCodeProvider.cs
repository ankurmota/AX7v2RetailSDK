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
    namespace Commerce.Runtime.Services
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Diagnostics;
        using System.Diagnostics.CodeAnalysis;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Tax Provider for default AX Tax definitions.
        /// </summary>
        [DebuggerDisplay("Identifier: {Identifier}")]
        internal class TaxCodeProvider
        {
            /// <summary>
            /// The maximum priority of a tax code.
            /// </summary>
            protected const int MaxPriorityTaxCode = 4;
            private static readonly DateTimeOffset NoDate = DateTimeOffset.MinValue;
            private Collection<TaxCode> transactionTaxCodes;
            private TaxCodeAmountRounder taxCodeAmountRounder;

            /// <summary>
            /// Initializes a new instance of the <see cref="TaxCodeProvider"/> class.
            /// </summary>
            /// <param name="taxContext">Tax context.</param>
            internal TaxCodeProvider(TaxContext taxContext)
            {
                this.TaxContext = taxContext;
                this.Identifier = "LSRetailPosis.TaxService.DefaultTaxProvider";
                this.transactionTaxCodes = new Collection<TaxCode>();
            }

            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public string Identifier { get; protected set; }

            /// <summary>
            /// Gets the tax context.
            /// </summary>
            protected TaxContext TaxContext { get; private set; }

            /// <summary>
            /// Gets the base price for tax included.
            /// </summary>
            /// <param name="taxableItem">The taxable item.</param>
            /// <param name="codes">The codes.</param>
            /// <param name="taxContext">The tax context.</param>
            /// <returns>The base price for tax included.</returns>
            public static decimal GetBasePriceForTaxIncluded(TaxableItem taxableItem, ReadOnlyCollection<TaxCode> codes, TaxContext taxContext)
            {
                // check to see if we can do the 'simple' Inclusive algorithm
                bool simpleBasis = codes.All(c =>
                    (c.TaxBase == TaxBase.PercentPerNet || c.TaxBase == TaxBase.PercentGrossOnNet)
                    && (c.TaxLimitMin == decimal.Zero && c.TaxLimitMax == decimal.Zero));
                bool collectLimits = codes.Any(c => (c.CollectLimitMax != decimal.Zero || c.CollectLimitMin != decimal.Zero));
                bool multiplePercentage = codes.Any(c => (c.TaxIntervals.Count > 1));

                if (simpleBasis && !collectLimits && !multiplePercentage)
                {
                    // Get base price for Simple TaxInclusive calculation
                    return GetBasePriceSimpleTaxIncluded(taxableItem, codes);
                }
                else
                {
                    // Get base price for Full TaxInclusive calculation
                    return GetBasePriceAdvancedTaxIncluded(taxableItem, codes, collectLimits, taxContext);
                }
            }

            /// <summary>
            /// Calculate taxes for the transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Transaction to calculate taxes for.</param>
            public void CalculateTax(RequestContext context, SalesTransaction transaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(transaction, "transaction");

                // at different we have different implementations of Itaxable. Flatten them into a list and loop.
                List<TaxableItem> taxableItems = new List<TaxableItem>();

                // Order level charges
                taxableItems.AddRange(transaction.ChargeLines);

                // Line Level
                // Consider active (non-void) lines for tax.
                // Need to recalculate tax on return-by-receipt lines because we cannot reconstruct tax lines from return transaction lines alone.
                // A few key information like IsExempt, IsTaxInclusive, TaxCode are not available on return transaction line.
                foreach (var lineItem in transaction.ActiveSalesLines)
                {
                    if (!lineItem.IsGiftCardLine
                        && !lineItem.IsInvoiceLine)
                    {
                        // lineitem itself
                        taxableItems.Add(lineItem);

                        // associated charges
                        taxableItems.AddRange(lineItem.ChargeLines);
                    }
                }

                this.FixTaxCodeIntervalsLookup(context, taxableItems);
                this.CalculateTax(context, taxableItems, transaction);
            }

            internal void CalculateTax(RequestContext context, List<TaxableItem> taxableItems, SalesTransaction transaction)
            {
                // Reset cached value for tax code / tax code combination totals.
                this.taxCodeAmountRounder = new TaxCodeAmountRounder();

                // Calculate tax on order-level miscellaneous charges
                foreach (var taxableItem in taxableItems)
                {
                    this.CalculateTax(taxableItem, context, transaction);
                }
            }

            /// <summary>
            /// Gets the priority of the specified tax code.
            /// </summary>
            /// <param name="code">The tax code.</param>
            /// <returns>A value indicating the priority of the tax code.</returns>
            protected static int TaxCodePriority(TaxCode code)
            {
                if (code == null)
                {
                    throw new ArgumentNullException("code");
                }

                // Return codes to be processed in the following order:
                // 1. Amount per unit & Percent of net & Percent Gross on net
                // 2. Percent of tax
                // 3. Percent of gross (single tax)
                // 4. Percent of gross (all taxes)
                switch (code.TaxBase)
                {
                    case TaxBase.AmountByUnit:
                    case TaxBase.PercentPerNet:
                    case TaxBase.PercentGrossOnNet:
                        return 1;

                    case TaxBase.PercentPerTax:
                        return 2;

                    case TaxBase.PercentPerGross:
                        return string.IsNullOrEmpty(code.TaxOnTax) ? MaxPriorityTaxCode : 3;

                    default:
                        return 0;
                }
            }

            /// <summary>
            /// Sets the line item tax rate.
            /// </summary>
            /// <param name="taxableItem">The taxable item.</param>
            /// <param name="taxAmount">The tax amount.</param>
            protected static void SetLineItemTaxRate(TaxableItem taxableItem, decimal taxAmount)
            {
                if (taxableItem == null)
                {
                    throw new ArgumentNullException("taxableItem");
                }

                decimal extendedPrice = taxableItem.Price * taxableItem.Quantity;
                if (extendedPrice == decimal.Zero)
                {
                    extendedPrice = 1;
                }

                taxableItem.TaxRatePercent += (taxAmount * 100) / extendedPrice;
            }

            /// <summary>
            /// Sets the line item tax rate.
            /// </summary>
            /// <param name="taxableItem">The taxable item.</param>
            /// <param name="lineTaxResult">The line tax result.</param>
            protected static void SetLineItemTaxRate(TaxableItem taxableItem, LineTaxResult lineTaxResult)
            {
                if (taxableItem == null)
                {
                    throw new ArgumentNullException("taxableItem");
                }

                if (lineTaxResult == null)
                {
                    throw new ArgumentNullException("lineTaxResult");
                }

                // Ignore any portion of the TaxAmount that is 'Exempt' when computing the rate.
                decimal amount = lineTaxResult.TaxAmount - lineTaxResult.ExemptAmount;
                SetLineItemTaxRate(taxableItem, amount);
            }

            /// <summary>
            /// Sorts the specified tax codes by priority.
            /// </summary>
            /// <param name="codes">The tax codes.</param>
            /// <returns>An ordered collection of tax codes.</returns>
            protected virtual ReadOnlyCollection<TaxCode> SortCodes(Dictionary<string, TaxCode> codes)
            {
                if (codes == null)
                {
                    throw new ArgumentNullException("codes");
                }

                return new ReadOnlyCollection<TaxCode>(
                    codes.Values.OrderBy(code =>
                    {
                        return TaxCodePriority(code);
                    }).ToList());
            }

            /// <summary>
            /// Retrieves a list of TaxCodes for the given sale line item.
            /// </summary>
            /// <param name="taxableItem">The taxable item.</param>
            /// <param name="context">The context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <returns>Tax codes applicable with the taxableItem.</returns>
            [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "By design.")]
            [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "By design.")]
            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "By design.")]
            protected virtual ReadOnlyCollection<TaxCode> GetTaxCodes(TaxableItem taxableItem, RequestContext context, SalesTransaction transaction)
            {
                ThrowIf.Null(taxableItem, "taxableItem");
                ThrowIf.Null(context, "context");

                try
                {
                    Dictionary<string, TaxCode> codes = new Dictionary<string, TaxCode>();

                    // If the line has an EndDate specified (usually because it's a Returned line),
                    // then use that value to calculate taxes, otherwise use BeginDate
                    TaxDateAndGroups key = new TaxDateAndGroups(this.GetTaxDate(taxableItem), taxableItem.SalesTaxGroupId, taxableItem.ItemTaxGroupId);

                    ReadOnlyCollection<TaxCodeInterval> taxCodeIntervals = null;
                    if (key.IsNoTax)
                    {
                        taxCodeIntervals = new List<TaxCodeInterval>().AsReadOnly();
                    }
                    else if (!this.TaxContext.TaxCodeInternalsLookup.TryGetValue(key, out taxCodeIntervals))
                    {
                        // Shouldn't get here.
                        taxCodeIntervals = this.GetTaxCodeIntervals(context, taxableItem.SalesTaxGroupId, taxableItem.ItemTaxGroupId, key.TaxDate);
                    }

                    foreach (TaxCodeInterval taxCodeInterval in taxCodeIntervals)
                    {
                        if (codes.ContainsKey(taxCodeInterval.TaxCode))
                        {
                            // Add a new 'value' entry for an existing tax code
                            var taxInterval = new TaxInterval(taxCodeInterval.TaxLimitMinimum, taxCodeInterval.TaxLimitMaximum, taxCodeInterval.TaxValue);
                            codes[taxCodeInterval.TaxCode].TaxIntervals.Add(taxInterval);
                        }
                        else
                        {
                            this.AddTaxCode(context, taxableItem, taxCodeInterval, codes, transaction);
                        }
                    }

                    // Link any taxes which rely on other taxes
                    foreach (TaxCode tax in codes.Values)
                    {
                        if (!string.IsNullOrEmpty(tax.TaxOnTax)
                            && (tax.TaxBase == TaxBase.PercentPerTax || tax.TaxBase == TaxBase.PercentPerGross)
                            && codes.Keys.Contains(tax.TaxOnTax))
                        {
                            tax.TaxOnTaxInstance = codes[tax.TaxOnTax];
                        }
                    }

                    return this.SortCodes(codes);
                }
                catch (Exception ex)
                {
                    RetailLogger.Log.CrtServicesTaxCodeProviderTaxServiceGetTaxCodesFailure(ex);
                    throw;
                }
            }

            /// <summary>
            /// Gets the tax code.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="taxableItem">The taxable item.</param>
            /// <param name="taxCodeInterval">The tax code interval.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <returns>The tax code object.</returns>
            protected virtual TaxCode GetTaxCode(RequestContext context, TaxableItem taxableItem, TaxCodeInterval taxCodeInterval, SalesTransaction transaction)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }

                return new TaxCode(context, taxableItem, taxCodeInterval, this.TaxContext, transaction);
            }

            /// <summary>
            /// Gets the tax code intervals.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTaxGroupId">The sales tax group Id.</param>
            /// <param name="itemTaxGroupId">The item sales tax group Id.</param>
            /// <param name="transDate">The transaction date.</param>
            /// <returns>The tax code object.</returns>
            protected virtual ReadOnlyCollection<TaxCodeInterval> GetTaxCodeIntervals(RequestContext context, string salesTaxGroupId, string itemTaxGroupId, DateTimeOffset transDate)
            {
                GetTaxCodeIntervalsDataRequest dataServiceRequest = new GetTaxCodeIntervalsDataRequest(salesTaxGroupId, itemTaxGroupId, transDate);
                var response = context.Execute<EntityDataServiceResponse<TaxCodeInterval>>(dataServiceRequest);

                return response.PagedEntityCollection.Results;
            }

            /// <summary>
            /// Simple version of TaxIncluded algorithm for tax code collections that are not based on:
            /// intervals, limits, collection limits and total invoice.
            /// </summary>
            /// <param name="lineItem">The taxable item.</param>
            /// <param name="codes">The collection of tax codes.</param>
            /// <returns>The base price.</returns>
            private static decimal GetBasePriceSimpleTaxIncluded(TaxableItem lineItem, ReadOnlyCollection<TaxCode> codes)
            {
                // accumulation of % based tax
                decimal fullLineTaxRate = decimal.Zero;

                // accumulation of amount based tax
                decimal fullLineUnitTax = decimal.Zero;
                decimal nonExemptLineUnitTax = decimal.Zero;

                // 1. Determine sum of all AmountByUnit taxes (ref: AX\Classes\Tax.AmountExclTax() - line 222)
                decimal codeValue = decimal.Zero;

                // Reference dev item 5747
                foreach (TaxCode code in codes.Where(c => c.TaxBase == TaxBase.AmountByUnit))
                {
                    codeValue = code.Calculate(codes, false);  // Amount by units don't depend on basePrice
                    fullLineUnitTax += codeValue;
                    nonExemptLineUnitTax += code.Exempt ? decimal.Zero : codeValue;
                }

                // 2. Determine sum of all tax rates for non-AmountByUnit taxes (ref: AX\Classes\Tax.AmountExclTax() - line 331)
                foreach (TaxCode code in codes.Where(c => c.TaxBase != TaxBase.AmountByUnit))
                {
                    if (code.TaxBase == TaxBase.PercentPerGross && string.IsNullOrEmpty(code.TaxOnTax))
                    {
                        // Sum all OTHER taxes...
                        codeValue = codes.Sum(c => (c.TaxBase == TaxBase.AmountByUnit) ? decimal.Zero : c.PercentPerTax());

                        // And then apply the Gross tax on top of that
                        codeValue *= code.PercentPerTax() / 100;

                        // Add this rate to the running total.
                        fullLineTaxRate += codeValue;
                    }
                    else
                    {
                        // Add this rate to the running total.
                        codeValue = code.PercentPerTax();
                        fullLineTaxRate += codeValue;
                    }
                }

                // 3. Back calculate the Price based on tax rates, start with the Price that includes ALL taxes
                decimal taxBase = lineItem.NetAmountWithAllInclusiveTaxPerUnit - fullLineUnitTax;
                return (taxBase * 100) / (100 + fullLineTaxRate);
            }

            /// <summary>
            ///  Advanced version of TaxIncluded algorithm for tax codes with the full range of supported tax properties.
            /// </summary>
            /// <param name="taxableItem">The taxable item.</param>
            /// <param name="codes">The codes.</param>
            /// <param name="collectLimits">A value indicating if limits are collected.</param>
            /// <param name="taxContext">The tax context.</param>
            /// <returns>The base price.</returns>
            [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode", Justification = "By design.")]
            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "By design.")]
            private static decimal GetBasePriceAdvancedTaxIncluded(
                TaxableItem taxableItem, ReadOnlyCollection<TaxCode> codes, bool collectLimits, TaxContext taxContext)
            {
                // accumulation of amount based tax
                decimal fullLineUnitTax = decimal.Zero;
                decimal nonExemptLineUnitTax = decimal.Zero;
                decimal codeValue = decimal.Zero;

                // AX variables
                decimal endAmount = taxableItem.NetAmountWithAllInclusiveTaxPerUnit; // endAmount will be the final price w/o tax
                int sign = 1;

                // 3.
                decimal taxLimitMax = decimal.Zero;
                decimal taxLimitMin = decimal.Zero;
                decimal startAmount = decimal.Zero;

                // 3a...
                decimal taxCalc = decimal.Zero;
                decimal baseCur;

                // Tax Amount deducted for a given Code
                Dictionary<string, decimal> deductedTax = new Dictionary<string, decimal>();

                // 3b ...
                decimal percentTotal;
                decimal tmpBase;

                // 3c..
                // Whether or not a Code needs to be removed from the sum of percent rates
                Dictionary<string, bool> removePercent = new Dictionary<string, bool>();

                // 3d.
                decimal totalTax = decimal.Zero;

                // Whether or not the Code needs to be calculated
                Dictionary<string, bool> calcTax = new Dictionary<string, bool>();

                string storeCurrency = taxContext.ChannelCurrency;

                // Begin Tax included calculation
                // 0. Initialize the supporting collections
                foreach (TaxCode code in codes)
                {
                    deductedTax[code.Code] = decimal.Zero;
                    removePercent[code.Code] = false;
                    calcTax[code.Code] = true;
                }

                // 1. Remove all AmountByUnit taxes
                foreach (TaxCode code in codes.Where(c => c.TaxBase == TaxBase.AmountByUnit))
                {
                    codeValue = code.Calculate(codes, false); // Reference dev item 5748.
                    fullLineUnitTax += codeValue;
                    nonExemptLineUnitTax += code.Exempt ? decimal.Zero : codeValue;
                    calcTax[code.Code] = false;
                }

                endAmount -= Math.Abs(nonExemptLineUnitTax);

                // 2. Record the sign, and then continue using the magnitude of endAmount
                sign = (endAmount < decimal.Zero) ? -1 : 1;
                endAmount = Math.Abs(endAmount);

                // 3.
                while (startAmount < endAmount)
                {
                    // 3a Consider interval limits
                    taxCalc = decimal.Zero;
                    taxLimitMax = decimal.Zero;

                    foreach (TaxCode code in codes)
                    {
                        if (code.TaxCalculationMethod == TaxCalculationMode.FullAmounts)
                        {
                            taxLimitMax = decimal.Zero;
                        }
                        else
                        {
                            if (code.IsStoreCurrency)
                            {
                                baseCur = taxContext.TaxCurrencyOperations.ConvertCurrency(storeCurrency, code.Currency, taxLimitMin);
                            }
                            else
                            {
                                baseCur = taxLimitMin;
                            }

                            baseCur += 1;

                            // if 'baseCur' falls into an interval
                            if (code.TaxIntervals.Exists(baseCur))
                            {
                                // get the Upper limit of the interval that 'baseCur'/'taxLimitMin' falls into
                                decimal amount = code.TaxIntervals.Find(taxLimitMin + 1).TaxLimitMax;
                                taxLimitMax = (amount != decimal.Zero && amount < endAmount) ? amount : endAmount;
                            }
                        }

                        taxCalc += deductedTax[code.Code];
                    }

                    // 3b. Sum up all the Tax Percentage Rates
                    percentTotal = 0;
                    tmpBase = (taxLimitMax > decimal.Zero) ? taxLimitMax : endAmount;

                    foreach (TaxCode code in codes.Where(c => calcTax[c.Code]))
                    {
                        percentTotal += GetPercentPerTax(code, tmpBase, codes);
                    }

                    decimal taxMax;
                    decimal baseInclTax;
                    decimal baseExclTax;

                    // 3c.
                    // if this is the last interval??
                    if (taxLimitMax == decimal.Zero)
                    {
                        // Forward calculate taxes to see if we exceed the CollectLimit
                        foreach (TaxCode code in codes.Where(c => calcTax[c.Code]))
                        {
                            taxMax = code.CollectLimitMax;
                            baseInclTax = endAmount - taxLimitMin - taxCalc;
                            baseExclTax = baseInclTax * 100 / (100 + percentTotal);

                            if (taxMax != decimal.Zero)
                            {
                                tmpBase = endAmount;

                                decimal percent = GetPercentPerTax(code, tmpBase, codes);

                                if ((deductedTax[code.Code] + (baseExclTax * percent / 100)) > taxMax)
                                {
                                    deductedTax[code.Code] = taxMax;
                                    removePercent[code.Code] = true;
                                }
                            }
                        }

                        // 3d.
                        // Now remove any rates that exceed their LimitMax
                        foreach (TaxCode code in codes)
                        {
                            if (removePercent[code.Code] && calcTax[code.Code])
                            {
                                tmpBase = endAmount;
                                percentTotal -= GetPercentPerTax(code, tmpBase, codes);
                                calcTax[code.Code] = false;
                            }

                            taxCalc += deductedTax[code.Code];
                        }
                    }

                    // 4. Compute tax adjusted for limits
                    totalTax = decimal.Zero;
                    foreach (TaxCode code in codes.Where(c => c.TaxBase != TaxBase.AmountByUnit))
                    {
                        if (calcTax[code.Code])
                        {
                            tmpBase = (taxLimitMax > decimal.Zero) ? taxLimitMax : endAmount;

                            decimal percent = GetPercentPerTax(code, tmpBase, codes);

                            if (taxLimitMax > decimal.Zero && taxLimitMax < endAmount)
                            {
                                deductedTax[code.Code] += (taxLimitMax - taxLimitMin) * percent / 100;
                            }
                            else
                            {
                                baseInclTax = endAmount - taxLimitMin - taxCalc;
                                baseExclTax = baseInclTax * 100 / (100 + percentTotal);
                                deductedTax[code.Code] += baseExclTax * percent / 100;
                            }

                            taxMax = code.CollectLimitMax;

                            if (taxMax > decimal.Zero && deductedTax[code.Code] > taxMax)
                            {
                                deductedTax[code.Code] = taxMax;
                            }
                        }

                        totalTax += deductedTax[code.Code];
                    }

                    if (taxLimitMax > decimal.Zero)
                    {
                        taxLimitMin = taxLimitMax;
                        startAmount = taxLimitMin + totalTax;
                    }
                    else
                    {
                        startAmount = endAmount;
                    }
                } // END if( startAmount < endAmount)

                // 5a. Total up taxes
                foreach (TaxCode code in codes)
                {
                    if (collectLimits && (deductedTax[code.Code] < code.CollectLimitMin))
                    {
                        totalTax -= deductedTax[code.Code];
                        deductedTax[code.Code] = decimal.Zero;
                    }

                    if (code.IsStoreCurrency)
                    {
                        taxCalc = Rounding.RoundToUnit(deductedTax[code.Code], code.RoundingOff, code.RoundingOffType);
                    }
                    else
                    {
                        taxCalc = deductedTax[code.Code];
                    }

                    totalTax += taxCalc - deductedTax[code.Code];
                    deductedTax[code.Code] = taxCalc;
                }

                // 5b. Determine base price
                return (endAmount - totalTax) * sign;
            }

            /// <summary>
            /// Get the PercentRate for a given Tax (takes Gross Taxes into account).
            /// </summary>
            /// <param name="code">The tax code.</param>
            /// <param name="taxBase">The tax base.</param>
            /// <param name="otherCodes">The other codes.</param>
            /// <returns>The percentage rate.</returns>
            private static decimal GetPercentPerTax(TaxCode code, decimal taxBase, ReadOnlyCollection<TaxCode> otherCodes)
            {
                decimal percent = decimal.Zero;

                if (code.TaxBase == TaxBase.PercentPerGross)
                {
                    decimal otherPercents = decimal.Zero;
                    foreach (TaxCode t in otherCodes.Where(c => (c.Code != code.Code && c.TaxBase != TaxBase.AmountByUnit)))
                    {
                        otherPercents += t.PercentPerTax(taxBase);
                    }

                    decimal grossPercent = code.PercentPerTax(taxBase);

                    // Gross Percent needs to be expressed with respect to the original item price:
                    // ActualPercent = GrossPercent * (full price + other taxes)/100
                    percent = grossPercent * (100 + otherPercents) / 100m;
                }
                else
                {
                    percent = code.PercentPerTax(taxBase);
                }

                return percent;
            }

            /// <summary>
            /// Calculate tax on the given line item.
            /// </summary>
            /// <param name="taxableItem">The taxable item.</param>
            /// <param name="context">The context.</param>
            /// <param name="transaction">Current transaction.</param>
            private void CalculateTax(TaxableItem taxableItem, RequestContext context, SalesTransaction transaction)
            {
                ReadOnlyCollection<TaxCode> codes = this.GetTaxCodes(taxableItem, context, transaction);

                LineTaxResult lineTaxResult = new LineTaxResult
                {
                    HasExempt = false,
                    TaxRatePercent = decimal.Zero,
                    TaxAmount = decimal.Zero,
                    ExemptAmount = decimal.Zero
                };

                foreach (TaxCode code in codes)
                {
                    var taxCodeAmount = code.CalculateTaxAmount(codes, this.taxCodeAmountRounder);
                    lineTaxResult.TaxAmount += taxCodeAmount;

                    // sum up the amounts that are exempt
                    if (code.Exempt)
                    {
                        lineTaxResult.HasExempt = true;
                        lineTaxResult.ExemptAmount += lineTaxResult.TaxAmount;
                    }
                }

                // Set the 'virtual tax rate', if extended price is ZERO, then just add the full amount
                decimal extendedPrice = taxableItem.Price * Math.Abs(taxableItem.Quantity);
                if (extendedPrice == decimal.Zero)
                {
                    extendedPrice = decimal.One;
                }

                lineTaxResult.TaxRatePercent = (lineTaxResult.TaxAmount * 100) / extendedPrice;
                SetLineItemTaxRate(taxableItem, lineTaxResult);
            }

            private void AddTaxCode(RequestContext context, TaxableItem taxableItem, TaxCodeInterval taxCodeInterval, Dictionary<string, TaxCode> codes, SalesTransaction transaction)
            {
                TaxCode code = this.GetTaxCode(context, taxableItem, taxCodeInterval, transaction);

                codes.Add(code.Code, code);
                this.transactionTaxCodes.Add(code);
            }

            private DateTimeOffset GetTaxDate(TaxableItem taxableItem)
            {
                DateTimeOffset taxDate = (taxableItem.EndDateTime <= NoDate) ? taxableItem.BeginDateTime : taxableItem.EndDateTime;

                if (taxDate == DateTimeOffset.MinValue)
                {
                    taxDate = this.TaxContext.NowInChannelTimeZone;
                }

                return taxDate;
            }

            private void FixTaxCodeIntervalsLookup(RequestContext context, List<TaxableItem> taxableItems)
            {
                foreach (TaxableItem taxableItem in taxableItems)
                {
                    TaxDateAndGroups key = new TaxDateAndGroups(this.GetTaxDate(taxableItem), taxableItem.SalesTaxGroupId, taxableItem.ItemTaxGroupId);

                    if (!key.IsNoTax)
                    {
                        if (!this.TaxContext.TaxCodeInternalsLookup.ContainsKey(key))
                        {
                            this.TaxContext.TaxCodeInternalsLookup.Add(key, this.GetTaxCodeIntervals(context, key.SalesTaxGroup, key.ItemTaxGroup, key.TaxDate));
                        }
                    }
                }
            }
        }
    }
}
