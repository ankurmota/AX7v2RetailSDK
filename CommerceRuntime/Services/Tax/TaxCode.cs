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

        /// <summary>
        /// Represents a single tax code.
        /// </summary>
        [DebuggerDisplay("{Code}, {TaxBase}, {TaxLimitBase}")]
        internal class TaxCode
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TaxCode"/> class.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="lineItem">The taxable line item.</param>
            /// <param name="interval">The tax code interval.</param>
            /// <param name="taxContext">Tax context.</param>
            /// <param name="transaction">Current transaction.</param>
            public TaxCode(RequestContext context, TaxableItem lineItem, TaxCodeInterval interval, TaxContext taxContext, SalesTransaction transaction)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }

                if (interval == null)
                {
                    throw new ArgumentNullException("interval");
                }

                this.Code = interval.TaxCode;
                this.TaxableEntity = lineItem;
                this.TaxGroup = interval.TaxItemGroup;
                this.Currency = interval.TaxCurrencyCode;
                this.Exempt = interval.IsTaxExempt;
                this.TaxBase = (TaxBase)interval.TaxBase;
                this.TaxLimitBase = (TaxLimitBase)interval.TaxLimitBase;
                this.TaxCalculationMethod = (TaxCalculationMode)interval.TaxCalculationMethod;
                this.TaxOnTax = interval.TaxOnTax;
                this.Unit = interval.TaxUnit;
                this.RoundingOff = interval.TaxRoundOff;
                this.RoundingOffType = Rounding.ConvertRoundOffTypeToRoundingMethod(interval.TaxRoundOffType);
                this.CollectLimitMax = interval.TaxMaximum;
                this.CollectLimitMin = interval.TaxMinimum;
                this.TaxGroupRounding = interval.IsGroupRounding;
                this.IsTaxIncludedInTax = interval.IsTaxIncludedInTax;

                this.TaxIntervals = new Collection<TaxInterval>(new List<TaxInterval>(1));

                // should this be removed in favor of intervals?
                this.Value = interval.TaxValue;
                this.TaxLimitMin = interval.TaxLimitMinimum;
                this.TaxLimitMax = interval.TaxLimitMaximum;
                this.RequestContext = context;
                this.Transaction = transaction;
                this.TaxIntervals.Add(new TaxInterval(interval.TaxLimitMinimum, interval.TaxLimitMaximum, interval.TaxValue));

                this.TaxContext = taxContext;
            }

            internal TaxCode(string taxCode)
            {
                this.Code = taxCode;
            }

            /// <summary>
            /// Gets or sets the tax code.
            /// </summary>
            public string Code { get; protected set; }

            /// <summary>
            /// Gets or sets the tax group that this tax code belongs to.
            /// </summary>
            public string TaxGroup { get; protected set; }

            /// <summary>
            /// Gets or sets the currency that this tax is calculated in.
            /// </summary>
            public string Currency { get; protected set; }

            /// <summary>
            /// Gets the Value/Rate of the tax.
            /// </summary>
            public decimal Value { get; private set; }

            /// <summary>
            /// Gets the minimum amount required to calculate this tax.
            /// </summary>
            public decimal TaxLimitMin { get; private set; }

            /// <summary>
            /// Gets the maximum amount required to calculate this tax.
            /// </summary>
            public decimal TaxLimitMax { get; private set; }

            /// <summary>
            /// Gets or sets the collection limits, the minimum tax that can be collected.
            /// </summary>
            /// <remarks>
            /// If the calculated tax is less than this, then ZERO tax will be collected.
            /// </remarks>
            public decimal CollectLimitMin { get; protected set; }

            /// <summary>
            /// Gets or sets the collection limits, the maximum tax that can be collected.
            /// </summary>
            /// <remarks>
            /// If the calculated tax is more than this, then THIS will be the tax amount collected.
            /// </remarks>
            public decimal CollectLimitMax { get; protected set; }

            /// <summary>
            /// Gets or sets a value indicating whether or not this tax is exempt.
            /// </summary>
            public bool Exempt { get; protected set; }

            /// <summary>
            /// Gets a value indicating whether tax is calculated before sales tax or not. Mainly used for duty charges.
            /// </summary>
            public bool IsTaxIncludedInTax { get; private set; }

            /// <summary>
            /// Gets or sets the origin from which sales tax is calculated.
            /// </summary>
            public TaxBase TaxBase { get; protected set; }

            /// <summary>
            /// Gets or sets the basis of sales tax limits.
            /// </summary>
            public TaxLimitBase TaxLimitBase { get; internal set; }

            /// <summary>
            /// Gets or sets whether tax is calculated for entire amounts or for intervals.
            /// </summary>
            public TaxCalculationMode TaxCalculationMethod { get; protected set; }

            /// <summary>
            /// Gets or sets the TaxCode of the other sales tax that this tax is based on.
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TaxOn", Justification = "Domain-specific terminology.")]
            public string TaxOnTax { get; protected set; }

            /// <summary>
            /// Gets or sets the unit for calculating per-unit amounts.
            /// </summary>
            public string Unit { get; protected set; }

            /// <summary>
            /// Gets or sets the tax rounding off.
            /// </summary>
            public decimal RoundingOff { get; protected set; }

            /// <summary>
            /// Gets or sets the tax rounding off type.
            /// </summary>
            public RoundingMethod RoundingOffType { get; protected set; }

            /// <summary>
            /// Gets or sets the TaxCode instance referred by TaxOnTax property.
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TaxOn", Justification = "Domain-specific terminology.")]
            public TaxCode TaxOnTaxInstance { get; set; }

            /// <summary>
            /// Gets or sets the taxable entity.
            /// </summary>
            /// <value>
            /// The taxable entity.
            /// </value>
            public TaxableItem TaxableEntity { get; protected set; }

            /// <summary>
            /// Gets or sets the tax value.
            /// </summary>
            /// <value>
            /// The tax value.
            /// </value>
            public decimal TaxValue { get; set; }

            /// <summary>
            /// Gets or sets the amount per unit value.
            /// </summary>
            /// <value>
            /// The amount per unit value.
            /// </value>
            public decimal AmountPerUnitValue { get; set; }

            /// <summary>
            /// Gets or sets the tax basis. A value used as starting point for tax formula.
            /// </summary>
            public decimal TaxBasis { get; protected set; }

            /// <summary>
            /// Gets or sets a value indicating whether or not this code should be rounded at the Tax Group scope.
            /// </summary>
            public bool TaxGroupRounding { get; set; }

            /// <summary>
            /// Gets a value indicating whether the price includes the tax amount.
            /// </summary>
            /// <value>
            /// If <c>true</c> then tax is included in the price; otherwise, <c>false</c>.
            /// </value>
            public virtual bool TaxIncludedInPrice
            {
                get { return this.Transaction.IsTaxIncludedInPrice; }
            }

            /// <summary>
            /// Gets the calculation base for AmountPerUnit tax calculations.
            /// </summary>
            public decimal AmountPerUnitCalculationBase
            {
                get
                {
                    return (TaxLimitBase == TaxLimitBase.UnitWithoutVat || TaxLimitBase == TaxLimitBase.UnitWithVat) ? decimal.One : Math.Abs(this.TaxableEntity.Quantity);
                }
            }

            /// <summary>
            /// Gets a value indicating whether the current tax code is in the store's currency.
            /// </summary>
            /// <value>
            /// If <c>true</c>, this tax code is in the store's currency; otherwise, <c>false</c>.
            /// </value>
            public bool IsStoreCurrency
            {
                get
                {
                    return string.IsNullOrEmpty(this.Currency)
                        || this.Currency.Equals(this.TaxContext.ChannelCurrency, StringComparison.OrdinalIgnoreCase);
                }
            }

            /// <summary>
            /// Gets the collection of tax rate intervals defined for this tax code.
            /// </summary>
            internal Collection<TaxInterval> TaxIntervals { get; private set; }

            internal TaxContext TaxContext { get; private set; }

            /// <summary>
            /// Gets the context.
            /// </summary>
            protected RequestContext RequestContext { get; private set; }

            /// <summary>
            /// Gets the current transaction.
            /// </summary>
            protected SalesTransaction Transaction { get; private set; }

            /// <summary>
            /// Calculates tax for this code for the line item.
            /// Updates the line item by adding a new Tax Item.
            /// </summary>
            /// <param name="codes">The collection of tax codes.</param>
            /// <param name="taxCodeAmountRounder">The current, accrued totals for this tax code.</param>
            /// <returns>
            /// The calculated amount of tax.
            /// </returns>
            public virtual decimal CalculateTaxAmount(ReadOnlyCollection<TaxCode> codes, TaxCodeAmountRounder taxCodeAmountRounder)
            {
                decimal taxAmount = decimal.Zero;

                this.TaxableEntity.ItemTaxGroupId = this.TaxGroup;
                var taxAmountNonRounded = this.TaxIncludedInPrice ? this.CalculateTaxIncluded(codes) : this.CalculateTaxExcluded(codes);

                string groupRoundingKey = string.Empty;
                if (this.TaxGroupRounding)
                {
                    // tax codes already sorted for this taxable item.
                    foreach (var code in codes)
                    {
                        groupRoundingKey += code.Code + "@:$";
                    }
                }
                else if (this.TaxLimitBase == TaxLimitBase.InvoiceWithoutVat)
                {
                    // total tax required for whole invoice by this tax code only.
                    groupRoundingKey = this.Code;
                }

                // rounding required when tax is not zero (not exempted).
                if (taxAmountNonRounded != decimal.Zero)
                {
                    // Round per tax code or adjust tax code amount.
                    taxAmount = taxCodeAmountRounder.Round(this.TaxContext, this, groupRoundingKey, taxAmountNonRounded);
                }

                // record amounts on line item
                TaxLine taxLine = new TaxLine
                {
                    Amount = taxAmount,
                    Percentage = this.Value,
                    TaxCode = this.Code,
                    TaxGroup = this.TaxGroup,
                    IsExempt = this.Exempt,
                    IsIncludedInPrice = this.TaxIncludedInPrice,
                    TaxBasis = this.TaxBasis
                };

                this.TaxableEntity.TaxLines.Add(taxLine);

                return taxAmount;
            }

            /// <summary>
            /// Calculates tax for this tax code.
            /// </summary>
            /// <param name="codes">The collection of tax codes.</param>
            /// <param name="calculateBasePrice">Base price needs to be calculated.</param>
            /// <returns>
            /// The calculated unrounded tax.
            /// </returns>
            public decimal Calculate(ReadOnlyCollection<TaxCode> codes, bool calculateBasePrice)
            {
                decimal taxAmount = decimal.Zero;

                // 1a. Determine the base for the tax calculation
                var taxBases = this.GetBases(codes, this.IsStoreCurrency, calculateBasePrice);
                decimal calculationBase = taxBases.Item1;
                decimal limitBase = taxBases.Item2;
                this.TaxBasis = this.TaxContext.TaxCurrencyOperations.Round(calculationBase, this.RoundingOff, this.RoundingOffType);

                // 1b. Determine how many intervals are needed for calculation
                IEnumerable<TaxInterval> calculationIntervals = this.GetIntervals(limitBase);

                // 2. Calculate the tax amount
                foreach (TaxInterval interval in calculationIntervals)
                {
                    decimal intervalBase;
                    if (this.TaxCalculationMethod == TaxCalculationMode.FullAmounts || this.TaxBase == TaxBase.AmountByUnit)
                    {
                        // use the whole amount for each interval (which should only be ONE in practice)
                        intervalBase = calculationBase;
                    }
                    else
                    {
                        // use the segment of the base that falls into this interval
                        if (interval.TaxLimitMax == decimal.Zero)
                        {
                            // Max of ZERO signals infinite upper bound, so just use the full amount of the base
                            intervalBase = calculationBase - interval.TaxLimitMin;
                        }
                        else
                        {
                            // Otherwise, use the segment of the base that is bounded by the min and max.
                            intervalBase = Math.Min(calculationBase, interval.TaxLimitMax) - interval.TaxLimitMin;
                        }
                    }

                    switch (this.TaxBase)
                    {
                        case TaxBase.AmountByUnit:
                            // Quantity * TaxValueAmount
                            taxAmount += intervalBase * interval.Value;
                            break;

                        case TaxBase.PercentPerNet:
                        case TaxBase.PercentPerGross:
                        case TaxBase.PercentPerTax:
                            // Price * TaxValue
                            // (Price+Taxes) * TaxValue
                            // (OtherTaxAmount) * TaxValue
                            taxAmount += (intervalBase * interval.Value) / 100;
                            break;

                        case TaxBase.PercentGrossOnNet:
                            // Price * PGON(TaxValue)
                            taxAmount += (intervalBase * PercentGrossOnNet(interval.Value)) / 100;
                            break;
                    }
                }

                // 2c. Margin base: Per unit, line, invoice
                switch (this.TaxLimitBase)
                {
                    case TaxLimitBase.UnitWithoutVat:
                    case TaxLimitBase.UnitWithVat:
                        // taxes were calculated per single unit, so multiply by the line quantity
                        // Note:  Abs(Qty) required as bases are all positive during Calculate().
                        taxAmount *= Math.Abs(this.TaxableEntity.Quantity);
                        break;

                    case TaxLimitBase.LineWithoutVat:
                    case TaxLimitBase.LineWithVat:
                        // Do nothing else, taxes were already calculated for the whole line
                        break;

                    default:
                        // Do nothing else.
                        break;
                }

                // 3. If the tax was in a different currency, then convert the computed amount back into the store currency
                if (!this.IsStoreCurrency)
                {
                    taxAmount = this.TaxContext.TaxCurrencyOperations.ConvertCurrency(this.Currency, this.TaxContext.ChannelCurrency, taxAmount);
                }

                // 4. Adjust tax sign per sign of the Line Item quantity.
                taxAmount *= Math.Sign(this.TaxableEntity.Quantity);

                // Return non-rounded value.
                return taxAmount;
            }

            /// <summary>
            /// Return the basic rate for this tax.
            /// </summary>
            /// <returns>The basic rate for this tax.</returns>
            public decimal PercentPerTax()
            {
                switch (this.TaxBase)
                {
                    case TaxBase.PercentPerTax:
                        if (!string.IsNullOrEmpty(this.TaxOnTax) && this.TaxOnTaxInstance != null)
                        {
                            return (this.Value * this.TaxOnTaxInstance.Value) / 100;
                        }

                        return decimal.Zero;

                    case TaxBase.PercentPerGross:
                        if (!string.IsNullOrEmpty(this.TaxOnTax) && this.TaxOnTaxInstance != null)
                        {
                            return this.Value + ((this.Value * this.TaxOnTaxInstance.Value) / 100);
                        }

                        return this.Value;

                    case TaxBase.PercentGrossOnNet:
                        return PercentGrossOnNet(this.Value);

                    case TaxBase.AmountByUnit:
                    case TaxBase.PercentPerNet:
                    default:
                        return this.Value;
                }
            }

            /// <summary>
            /// Gets the basic rate for this tax.
            /// </summary>
            /// <param name="limitBase">The limit base.</param>
            /// <returns>The basic rate.</returns>
            public decimal PercentPerTax(decimal limitBase)
            {
                // Find the interval for this limitBase
                TaxInterval interval = this.TaxIntervals.Find(limitBase);
                decimal intervalRate = interval.Value;

                switch (this.TaxBase)
                {
                    case TaxBase.PercentPerTax:
                        if (!string.IsNullOrEmpty(this.TaxOnTax) && this.TaxOnTaxInstance != null)
                        {
                            return (intervalRate * this.TaxOnTaxInstance.Value) / 100;
                        }

                        return decimal.Zero;

                    case TaxBase.PercentPerGross:
                        if (!string.IsNullOrEmpty(this.TaxOnTax) && this.TaxOnTaxInstance != null)
                        {
                            return intervalRate + ((intervalRate * this.TaxOnTaxInstance.Value) / 100);
                        }

                        return intervalRate;

                    case TaxBase.PercentGrossOnNet:
                        return PercentGrossOnNet(intervalRate);

                    case TaxBase.AmountByUnit:
                    case TaxBase.PercentPerNet:
                    default:
                        return intervalRate;
                }
            }

            /// <summary>
            /// Calculate TaxInclusive amounts for a single tax code.
            /// </summary>
            /// <param name="codes">The collection of tax codes.</param>
            /// <returns>The tax inclusive amount.</returns>
            public decimal CalculateTaxIncluded(ReadOnlyCollection<TaxCode> codes)
            {
                decimal codeAmount = this.Calculate(codes, true);

                return codeAmount;
            }

            /// <summary>
            /// Calculate TaxExclusive amounts for a single tax code.
            /// </summary>
            /// <param name="codes">The tax codes.</param>
            /// <returns>The tax exclusive amount.</returns>
            public decimal CalculateTaxExcluded(ReadOnlyCollection<TaxCode> codes)
            {
                decimal codeAmount = decimal.Zero;

                if (!this.Exempt)
                {
                    // Calculate the tax amount
                    codeAmount = this.Calculate(codes, true);

                    // Apply collection limits
                    return this.ApplyCollectLimit(codeAmount);
                }

                return codeAmount;
            }

            /// <summary>
            /// Gets the bases for tax calculations.
            /// </summary>
            /// <param name="codes">The collection of tax codes.</param>
            /// <param name="taxInStoreCurrency">If set to <c>true</c>, tax amount is in the store's currency.</param>
            /// <param name="calculateBasePrice">If set to <c>true</c>, calculate the base price.</param>
            /// <returns>The calculation base and The limit base in <see cref="Tuple">Tuple</see>.</returns>
            protected virtual Tuple<decimal, decimal> GetBases(
                ReadOnlyCollection<TaxCode> codes,
                bool taxInStoreCurrency,
                bool calculateBasePrice)
            {
                decimal calculationBase;
                decimal limitBase;
                decimal basePrice = decimal.Zero;
                if (calculateBasePrice)
                {
                    basePrice = this.TaxIncludedInPrice ?
                        TaxCodeProvider.GetBasePriceForTaxIncluded(this.TaxableEntity, codes, this.TaxContext) :
                        this.TaxableEntity.NetAmountPerUnit + GetApplicableDutyTaxes(this.TaxableEntity, codes);
                }

                // 1. Get initial value for the Calculation Base
                switch (this.TaxBase)
                {
                    case TaxBase.PercentPerTax:
                        // Base is the amount of the other tax
                        switch (this.TaxLimitBase)
                        {
                            case TaxLimitBase.InvoiceWithoutVat:
                            case TaxLimitBase.InvoiceWithVat:
                                calculationBase = Math.Abs(this.CalculateTaxOnTax(this.Transaction));
                                break;

                            case TaxLimitBase.UnitWithoutVat:
                            case TaxLimitBase.UnitWithVat:
                                // if this tax's Limit is per-unit, then we need to convert the existing tax amounts from per-line to per-unit
                                decimal quantity = (this.TaxableEntity.Quantity == decimal.Zero) ? decimal.One : this.TaxableEntity.Quantity;
                                calculationBase = Math.Abs(this.CalculateTaxOnTax()) / Math.Abs(quantity);
                                break;

                            default:
                                calculationBase = Math.Abs(this.CalculateTaxOnTax());
                                break;
                        }

                        break;

                    case TaxBase.PercentPerGross:
                        // Base is the price + other taxes
                        calculationBase = basePrice;

                        // If the Limit base is NOT per-unit, then we need to factor in the line quanity
                        if (TaxLimitBase != TaxLimitBase.UnitWithoutVat && TaxLimitBase != TaxLimitBase.UnitWithVat)
                        {
                            calculationBase *= Math.Abs(this.TaxableEntity.Quantity);
                        }

                        if (!string.IsNullOrEmpty(this.TaxOnTax))
                        {
                            // Base is the Price + the amount of a single other tax
                            calculationBase += Math.Abs(this.CalculateTaxOnTax());
                        }
                        else
                        {
                            // Base is the Price + all other taxes
                            calculationBase += Math.Abs(TaxCode.SumAllTaxAmounts(this.TaxableEntity));
                        }

                        break;

                    case TaxBase.AmountByUnit:
                        calculationBase = this.AmountPerUnitCalculationBase;
                        break;

                    case TaxBase.PercentPerNet:
                    case TaxBase.PercentGrossOnNet:
                    default:
                        // Base is the Price
                        calculationBase = basePrice;

                        // If the Limit base is NOT per-unit, then we need to factor in the line quanity
                        if (TaxLimitBase != TaxLimitBase.UnitWithoutVat && TaxLimitBase != TaxLimitBase.UnitWithVat)
                        {
                            calculationBase *= Math.Abs(this.TaxableEntity.Quantity);
                        }

                        break;
                }

                // 3. Set Limit Base
                if (this.TaxBase == TaxBase.AmountByUnit)
                {
                    // Base for limits/intervals is base-quantity * price
                    limitBase = calculationBase * basePrice;

                    // Convert limit base to Tax currency, if different
                    if (!taxInStoreCurrency)
                    {
                        limitBase = this.TaxContext.TaxCurrencyOperations.ConvertCurrency(this.TaxContext.ChannelCurrency, this.Currency, limitBase);
                    }

                    // If the tax is calculated in a different UOM, then convert if possible
                    // this is only applicable for lineItem taxes.
                    SalesLine salesLine = this.TaxableEntity as SalesLine;

                    if (salesLine != null && !string.Equals(this.Unit, this.TaxableEntity.SalesOrderUnitOfMeasure, StringComparison.OrdinalIgnoreCase))
                    {
                        ItemUnitConversion conversion = new ItemUnitConversion
                        {
                            FromUnitOfMeasure = this.TaxableEntity.SalesOrderUnitOfMeasure,
                            ToUnitOfMeasure = this.Unit,
                            ItemId = this.TaxableEntity.ItemId
                        };

                        var conversions = new List<ItemUnitConversion>();
                        conversions.Add(conversion);

                        var getUomConvertionDataRequest = new GetUnitOfMeasureConversionDataRequest(conversions, QueryResultSettings.SingleRecord);
                        UnitOfMeasureConversion converter = this.RequestContext.Runtime
                            .Execute<GetUnitOfMeasureConversionDataResponse>(getUomConvertionDataRequest, this.RequestContext).UnitConversions.SingleOrDefault();

                        calculationBase *= converter.GetFactorForQuantity(this.TaxableEntity.Quantity);
                    }
                }
                else
                {
                    // Convert base to Tax currency, if different
                    if (!taxInStoreCurrency)
                    {
                        calculationBase = this.TaxContext.TaxCurrencyOperations.ConvertCurrency(this.TaxContext.ChannelCurrency, this.Currency, calculationBase);
                    }

                    // Base for limits/intervals is same for Calculations
                    limitBase = calculationBase;
                }

                return new Tuple<decimal, decimal>(calculationBase, limitBase);
            }

            /// <summary>
            /// Calculate the Percent Gross On Net amount for a given percent.
            /// </summary>
            /// <param name="percent">The percentage.</param>
            /// <returns>The calculated percent gross on net amount.</returns>
            private static decimal PercentGrossOnNet(decimal percent)
            {
                // Lifted from AX \ Classes \ Tax.calcPctGrosOnNet(...)
                if (percent == 100m)
                {
                    return 100m;
                }

                return (percent / (100m - percent)) * 100m;
            }

            /// <summary>
            /// Return a sum of all the currently applied tax amounts.
            /// </summary>
            /// <param name="lineItem">The line item.</param>
            /// <returns>The summation of all tax amount.</returns>
            private static decimal SumAllTaxAmounts(TaxableItem lineItem)
            {
                decimal allTaxAmounts = lineItem.TaxLines.Sum(t => t.IsExempt ? decimal.Zero : t.Amount);
                return allTaxAmounts;
            }

            /// <summary>
            /// Gets the duty taxes.
            /// </summary>
            /// <param name="lineItem">The line item.</param>
            /// <param name="codes">The codes.</param>
            /// <returns>Sum of duty taxes applicable on the line.</returns>
            private static decimal GetApplicableDutyTaxes(TaxableItem lineItem, IEnumerable<TaxCode> codes)
            {
                decimal dutyTaxSum = 0;

                if (lineItem != null && !codes.IsNullOrEmpty())
                {
                    var dutyTaxes = codes.Where(c => c.IsTaxIncludedInTax);
                    dutyTaxSum = dutyTaxes.Sum(dt => dt.Value * Math.Abs(lineItem.Quantity));
                }

                return dutyTaxSum;
            }

            /// <summary>
            /// Get the intervals used for determining the tax rate/values.
            /// </summary>
            /// <param name="limitBase">The limit base.</param>
            /// <returns>The collection of tax intervals.</returns>
            private IEnumerable<TaxInterval> GetIntervals(decimal limitBase)
            {
                if (this.TaxCalculationMethod == TaxCalculationMode.FullAmounts)
                {
                    // ONLY return the FIRST interval where price is in the closed interval (including Min/Max). - This is AX behaviour
                    // Order by Tax Min limit, so that it is ensured, we picked the tax interval matching with the least TaxMin value
                    return new Collection<TaxInterval>() { this.TaxIntervals.OrderBy(t => t.TaxLimitMin).FirstOrDefault(t => t.WholeAmountInInterval(limitBase)) };
                }
                else if (this.TaxCalculationMethod == TaxCalculationMode.Interval)
                {
                    // return ANY interval where the Price is greater than the Min
                    return this.TaxIntervals.Where(t => t.AmountInInterval(limitBase));
                }

                return new TaxInterval[0];
            }

            /// <summary>
            /// Calculate the previous tax amount that the current tax is based on.
            /// </summary>
            /// <returns>The tax amount.</returns>
            private decimal CalculateTaxOnTax()
            {
                decimal taxAmount = decimal.Zero;
                if (!string.IsNullOrEmpty(this.TaxOnTax) && this.TaxOnTaxInstance != null)
                {
                    // For each TaxLine that matches, sum the Amount
                    taxAmount = this.TaxableEntity.TaxLines.Where(taxLine => taxLine.TaxCode == this.TaxOnTax).Sum(taxLine => taxLine.Amount);
                }

                return taxAmount;
            }

            /// <summary>
            /// Sum up all amounts from the Tax-On-Tax for the whole transaction.
            /// </summary>
            /// <param name="transaction">The sales transaction.</param>
            /// <returns>The total tax amount.</returns>
            private decimal CalculateTaxOnTax(SalesTransaction transaction)
            {
                decimal taxAmount = decimal.Zero;
                if (!string.IsNullOrEmpty(this.TaxOnTax))
                {
                    // For each SaleItem, for each TaxLine that matches, sum the TaxLine.Amounts

                    // Consider active (non-void) lines for tax.
                    // Need to recalculate tax on return-by-receipt lines, because we cannot reconstruct tax lines from return transaction lines alone.
                    // A few key information like IsExempt, IsTaxInclusive, TaxCode are not available on return transaction line.
                    taxAmount = transaction.ActiveSalesLines.Sum(
                        saleItem => saleItem.TaxLines.Where(taxLine => taxLine.TaxCode == this.TaxOnTax).Sum(taxLine => taxLine.Amount));
                }

                return taxAmount;
            }

            /// <summary>
            /// Apply Tax Collect limits for this line.
            /// </summary>
            /// <param name="taxAmount">The tax amount.</param>
            /// <returns>The limited amount.</returns>
            private decimal ApplyCollectLimit(decimal taxAmount)
            {
                decimal limitedAmount = taxAmount;

                if (!this.IsStoreCurrency)
                {
                    // convert from channel currency to tax currency to evaluate limits
                    limitedAmount = this.TaxContext.TaxCurrencyOperations.ConvertCurrency(this.TaxContext.ChannelCurrency, this.Currency, limitedAmount);
                }

                // Evaluate the min and max tax collect limits
                if (this.CollectLimitMin != decimal.Zero && Math.Abs(limitedAmount) < this.CollectLimitMin)
                {
                    limitedAmount = decimal.Zero;
                }
                else if (this.CollectLimitMax != decimal.Zero && Math.Abs(limitedAmount) > this.CollectLimitMax)
                {
                    limitedAmount = this.CollectLimitMax * Math.Sign(limitedAmount);

                    if (!this.IsStoreCurrency)
                    {
                        // convert upper limit amount to channel currency
                        limitedAmount = this.TaxContext.TaxCurrencyOperations.ConvertCurrency(this.Currency, this.TaxContext.ChannelCurrency, limitedAmount);
                    }
                }
                else
                {
                    // tax limits are not applicable, simply return original tax amount.
                    limitedAmount = taxAmount;
                }

                return limitedAmount;
            }
        }
    }
}
