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
        using System.Linq;
        using Commerce.Runtime.Services.PricingEngine;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Represents a single tax code for India.
        /// </summary>
        [DebuggerDisplay("{TaxType}, {Code}, {TaxBase}, {TaxLimitBase}")]
        internal sealed class TaxCodeIndia : TaxCode
        {
            /// <summary>
            /// Gets or sets the formula of the tax code.
            /// </summary>
            private FormulaIndia formula;

            private IndiaPriceHelper indiaPriceHelper;

            /// <summary>
            /// Tax component.
            /// </summary>
            private string taxComponent;

            private IList<string> taxCodesInFormula;

            /// <summary>
            /// Initializes a new instance of the <see cref="TaxCodeIndia"/> class.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="taxableItem">The taxable line item.</param>
            /// <param name="taxCodeInterval">The tax code interval.</param>
            /// <param name="taxContext">Tax context.</param>
            /// <param name="transaction">Current transaction.</param>
            public TaxCodeIndia(
                RequestContext context, TaxableItem taxableItem, TaxCodeIntervalIndia taxCodeInterval, TaxContext taxContext, SalesTransaction transaction)
                : base(context, taxableItem, taxCodeInterval, taxContext, transaction)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }

                if (taxCodeInterval == null)
                {
                    throw new ArgumentNullException("taxCodeInterval");
                }

                this.TaxType = taxCodeInterval.TaxType;
                this.AbatementPercent = taxCodeInterval.AbatementPercent;
                this.taxCodesInFormula = new List<string>();
            }

            /// <summary>
            /// Gets a value indicating whether the price includes the tax amount.
            /// </summary>
            /// <value>
            /// If <c>true</c> then tax is included in the price; otherwise, <c>false</c>.
            /// </value>
            public override bool TaxIncludedInPrice
            {
                get
                {
                    return this.Formula.PriceIncludesTax;
                }
            }

            /// <summary>
            /// Gets or sets the percentage off item tax basis.
            /// </summary>
            public decimal AbatementPercent { get; set; }

            /// <summary>
            /// Gets or sets the tax type of the code.
            /// </summary>
            public TaxTypeIndia TaxType { get; set; }

            /// <summary>
            /// Gets the formula.
            /// </summary>
            public FormulaIndia Formula
            {
                get
                {
                    if (this.formula == null)
                    {
                        GetTaxCodeFormulaIndiaDataRequest dataRequest = new GetTaxCodeFormulaIndiaDataRequest(this.TaxGroup, this.Code);
                        this.formula = this.RequestContext.Execute<SingleEntityDataServiceResponse<FormulaIndia>>(dataRequest).Entity;
                    }

                    return this.formula;
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the tax code is a tax-on-tax or not.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TaxOn", Justification = "Domain-specific terminology.")]
            public bool IsTaxOnTax { get; set; }

            /// <summary>
            /// Gets all tax codes in the formula.
            /// </summary>
            /// <remarks>
            /// For example, if the formula of a tax item is +[ST-DL]+[ST-KA], the property will be valued as ["ST-DL", "ST-KA"].
            /// </remarks>
            public IList<string> TaxCodesInFormula
            {
                get { return this.taxCodesInFormula; }
            }

            /// <summary>
            /// Gets the tax component.
            /// </summary>
            /// <returns>The tax component.</returns>
            public string GetTaxComponent()
            {
                if (this.taxComponent == null)
                {
                    GetTaxComponentIndiaDataRequest getTaxComponentIndiaDataRequest = new GetTaxComponentIndiaDataRequest(this.Code, QueryResultSettings.SingleRecord);
                    TaxComponentIndia taxComponentIndia = this.RequestContext.Runtime.Execute<SingleEntityDataServiceResponse<TaxComponentIndia>>(getTaxComponentIndiaDataRequest, this.RequestContext).Entity;
                    if (taxComponentIndia == null)
                    {
                        throw new ArgumentException("{0} has no tax component defined", this.Code);
                    }

                    this.taxComponent = taxComponentIndia.TaxComponent;
                }

                return this.taxComponent;
            }

            /// <summary>
            /// Calculates tax for this code for the line item.
            /// Updates the line item by adding a new Tax Item.
            /// </summary>
            /// <param name="codes">The tax codes collection.</param>
            /// <param name="taxCodeAmountRounder">The current, accrued totals for this tax code.</param>
            /// <returns>The calculated amount of tax.</returns>
            public override decimal CalculateTaxAmount(ReadOnlyCollection<TaxCode> codes, TaxCodeAmountRounder taxCodeAmountRounder)
            {
                if (codes == null)
                {
                    return decimal.Zero;
                }

                decimal taxAmount = decimal.Zero;

                this.TaxableEntity.ItemTaxGroupId = this.TaxGroup;
                taxAmount = this.TaxIncludedInPrice ? this.CalculateTaxIncluded(codes) : this.CalculateTaxExcluded(codes);

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

                // rounding required for above cases and when tax is not zero (not exempted).
                if (taxAmount != decimal.Zero)
                {
                    // Adjust tax code amount.
                    taxAmount = taxCodeAmountRounder.Round(this.TaxContext, this, groupRoundingKey, taxAmount);
                }

                // Record amounts on line item
                TaxLineIndia taxLine = new TaxLineIndia();

                taxLine.Amount = taxAmount;
                taxLine.Percentage = this.Value;
                taxLine.TaxCode = this.Code;
                taxLine.TaxGroup = this.TaxGroup;
                taxLine.IsExempt = this.Exempt;
                taxLine.IsIncludedInPrice = this.TaxIncludedInPrice;
                taxLine.TaxComponent = this.GetTaxComponent();
                taxLine.IsTaxOnTax = this.IsTaxOnTax;

                foreach (string codeInFormula in this.TaxCodesInFormula)
                {
                    if (!taxLine.TaxCodesInFormula.Contains(codeInFormula))
                    {
                        taxLine.TaxCodesInFormula.Add(codeInFormula);
                    }
                }

                switch (this.Formula.TaxableBasis)
                {
                    case TaxableBasisIndia.MaxRetailPrice:
                        taxLine.IsIncludedInPrice = false;
                        break;
                    case TaxableBasisIndia.ExclusiveLineAmount:
                        string[] tokens = this.Formula.ParseExpression();

                        // Iterate through the formula
                        if (tokens.Length > 1)
                        {
                            GetTaxCodeFormulaIndiaDataRequest dataRequest = new GetTaxCodeFormulaIndiaDataRequest(this.TaxGroup, tokens[1]);
                            FormulaIndia basisFormula = this.RequestContext.Execute<SingleEntityDataServiceResponse<FormulaIndia>>(dataRequest).Entity;

                            if (basisFormula != null && basisFormula.TaxableBasis == TaxableBasisIndia.MaxRetailPrice)
                            {
                                taxLine.IsIncludedInPrice = false;
                            }
                        }

                        break;
                    default:
                        break;
                }

                taxLine.TaxBasis = this.TaxBasis;

                this.TaxableEntity.TaxLines.Add(taxLine);

                return taxAmount;
            }

            /// <summary>
            /// Calculate the tax bases calculationBase and limitBase (which is zero for India).
            /// </summary>
            /// <param name="codes">The tax codes.</param>
            /// <param name="taxInStoreCurrency">If set to <c>true</c> [tax in store currency].</param>
            /// <param name="calculateBasePrice">If set to <c>true</c> [Calculate the base price].</param>
            /// <returns>The calculation base as Item1 and The limit base as Item2 in Tuple.</returns>
            protected override Tuple<decimal, decimal> GetBases(ReadOnlyCollection<TaxCode> codes, bool taxInStoreCurrency, bool calculateBasePrice)
            {
                const decimal LimitBase = decimal.Zero;
                decimal calculationBase;

                // For amount by unit calculation base is just the quantity.
                if (this.TaxBase == TaxBase.AmountByUnit)
                {
                    calculationBase = this.TaxableEntity.Quantity;

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

                    return new Tuple<decimal, decimal>(calculationBase, LimitBase);
                }

                // Determine the starting calculation base (includes the line price or not)
                switch (this.Formula.TaxableBasis)
                {
                    case TaxableBasisIndia.LineAmount:
                        calculationBase = this.TaxableEntity.NetAmountWithAllInclusiveTaxPerUnit;
                        break;
                    case TaxableBasisIndia.MaxRetailPrice:
                        calculationBase = this.GetItemMaxRetailPrice();
                        break;
                    default:
                        calculationBase = decimal.Zero;
                        break;
                }

                if (this.TaxIncludedInPrice)
                {
                    calculationBase = GetBasePriceForTaxIncluded(calculationBase, codes, this.Formula, this.RequestContext);
                }

                calculationBase *= Math.Abs(this.TaxableEntity.Quantity);

                // Calculation expression is of the form: +[BCD]+[CVD]+[E-CESS_CVD]+[PE-C_CVD]+[SHE-C_CVD]
                // where the brackets are replaced with the delimiter char(164)
                // and BCD, CVD ... are tax codes.
                // The operator may be + - / *.
                string[] tokens = this.Formula.ParseExpression();

                for (int index = 1; index < tokens.Length; index += 2)
                {
                    TaxLine taxLine = (from line in this.TaxableEntity.TaxLines
                                       where line.TaxCode == tokens[index]
                                       select line).FirstOrDefault();

                    if (taxLine != null)
                    {
                        this.IsTaxOnTax = true;
                        if (!this.taxCodesInFormula.Contains(taxLine.TaxCode))
                        {
                            this.taxCodesInFormula.Add(taxLine.TaxCode);
                        }
                    }

                    decimal amount = taxLine == null ? decimal.Zero : taxLine.Amount * Math.Sign(this.TaxableEntity.Quantity);
                    int tokenNumber = index - 1;

                    switch (tokens[tokenNumber])
                    {
                        case "+":
                            calculationBase += amount;
                            break;
                        case "-":
                            calculationBase -= amount;
                            break;
                        case "*":
                            calculationBase *= amount;
                            break;
                        case "/":
                            calculationBase = amount == decimal.Zero ? calculationBase : calculationBase /= amount;
                            break;
                        default:
                            RetailLogger.Log.CrtServicesTaxCodeIndiaTaxServiceInvalidOperatorFoundInBaseCalculation(tokens[tokenNumber]);
                            break;
                    }
                }

                // Knock any abatement off of the taxable basis
                calculationBase *= (100 - this.AbatementPercent) / 100;
                return new Tuple<decimal, decimal>(calculationBase, LimitBase);
            }

            /// <summary>
            /// Back calculates the base price for items with included taxes.  Based off of the class in AX
            /// <c>Tax.calcBaseAmtExclTax_IN</c>.
            /// </summary>
            /// <param name="baseLine">The base line amount.</param>
            /// <param name="codes">The tax codes.</param>
            /// <param name="formula">The formula value.</param>
            /// <param name="context">The request context.</param>
            /// <returns>The base price.</returns>
            private static decimal GetBasePriceForTaxIncluded(decimal baseLine, ReadOnlyCollection<TaxCode> codes, FormulaIndia formula, RequestContext context)
            {
                decimal taxVal;         // A summarized tax rate for this code computed from the formula
                decimal taxValLine;     // The summed tax rates for all codes for this line
                decimal amtPerUnitVal;  // A summarized amount per unit contribution for this code computed from the formula
                decimal amtPerUnitLine; // The summed amount per unit contributions for all codes for this line.

                taxValLine = decimal.Zero;
                amtPerUnitLine = decimal.Zero;

                foreach (var code in codes)
                {
                    taxVal = decimal.Zero;
                    amtPerUnitVal = decimal.Zero;

                    if (code.TaxIncludedInPrice)
                    {
                        // Handle codes differently based on whether they are India or not.
                        TaxCodeIndia codeIndia = code as TaxCodeIndia;
                        if (codeIndia != null)
                        {
                            CalculateTaxAmountsIndia(codes, codeIndia, ref amtPerUnitVal, ref taxVal, formula, context);
                        }
                        else
                        {
                            CalculateTaxAmounts(code, ref amtPerUnitVal, ref taxVal);
                        }

                        code.TaxValue = taxVal;
                        code.AmountPerUnitValue = amtPerUnitVal;
                    }

                    taxValLine += taxVal;
                    amtPerUnitLine += amtPerUnitVal;
                }

                // Back compute and set the price from price with tax (baseLine).
                return (Math.Abs(baseLine) - amtPerUnitLine) / (1 + taxValLine);
            }

            /// <summary>
            /// Calculates the tax amounts for non India tax codes (simple tax codes only supported).
            /// </summary>
            /// <param name="code">The tax code.</param>
            /// <param name="amtPerUnitVal">The value of amount per unit.</param>
            /// <param name="taxVal">The tax value.</param>
            private static void CalculateTaxAmounts(TaxCode code, ref decimal amtPerUnitVal, ref decimal taxVal)
            {
                if (code.TaxBase == TaxBase.AmountByUnit)
                {
                    amtPerUnitVal = code.Value;
                    taxVal = decimal.Zero;
                }
                else
                {
                    amtPerUnitVal = decimal.Zero;
                    taxVal = code.Value;
                }
            }

            /// <summary>
            /// Calculates the tax amounts india.
            /// </summary>
            /// <param name="codes">The codes.</param>
            /// <param name="codeIndia">The code india.</param>
            /// <param name="amtPerUnitVal">The value of the amount per unit.</param>
            /// <param name="taxVal">The tax value.</param>
            /// <param name="formula">The formula value.</param>
            /// <param name="context">The request context.</param>
            private static void CalculateTaxAmountsIndia(ReadOnlyCollection<TaxCode> codes, TaxCodeIndia codeIndia, ref decimal amtPerUnitVal, ref decimal taxVal, FormulaIndia formula, RequestContext context)
            {
                decimal amtPerUnit = decimal.Zero;
                decimal taxCodeValue = decimal.Zero;
                decimal taxValueLoc;

                taxVal = decimal.Zero;
                amtPerUnitVal = decimal.Zero;

                if (codeIndia.Formula.TaxableBasis != TaxableBasisIndia.ExclusiveLineAmount && codeIndia.Formula.TaxableBasis != formula.TaxableBasis)
                {
                    return;
                }

                string[] tokens = codeIndia.Formula.ParseExpression();

                if (tokens.Length > 1)
                {
                    GetTaxCodeFormulaIndiaDataRequest dataRequest = new GetTaxCodeFormulaIndiaDataRequest(codeIndia.TaxGroup, tokens[1]);
                    FormulaIndia basisFormula = context.Execute<SingleEntityDataServiceResponse<FormulaIndia>>(dataRequest).Entity;

                    if (basisFormula != null && basisFormula.TaxableBasis == formula.TaxableBasis)
                    {
                        // Iterate through the formula
                        for (int index = 1; index < tokens.Length; index += 2)
                        {
                            TaxCode basisCode = (from c in codes
                                                 where c.Code == tokens[index]
                                                 select c).FirstOrDefault();

                            if ((basisCode != null) && !basisCode.Exempt)
                            {
                                codeIndia.IsTaxOnTax = true;
                                if (!codeIndia.taxCodesInFormula.Contains(basisCode.Code))
                                {
                                    codeIndia.taxCodesInFormula.Add(basisCode.Code);
                                }
                                ////Either add or subtract the values based on the operator
                                switch (tokens[index - 1])
                                {
                                    case "-":
                                        if (basisCode.TaxBase == TaxBase.AmountByUnit)
                                        {
                                            amtPerUnit -= basisCode.AmountPerUnitValue;
                                        }
                                        else
                                        {
                                            taxCodeValue -= basisCode.TaxValue;
                                        }

                                        break;
                                    case "+":
                                        if (basisCode.TaxBase == TaxBase.AmountByUnit)
                                        {
                                            amtPerUnit += basisCode.AmountPerUnitValue;
                                        }
                                        else
                                        {
                                            taxCodeValue += basisCode.TaxValue;
                                        }

                                        break;
                                    default:
                                        RetailLogger.Log.CrtServicesTaxCodeIndiaTaxServiceNotSupportedOperatorFoundInTaxAmountCalculation(tokens[index - 1]);
                                        break;
                                }
                            }
                        }
                    }
                }

                taxValueLoc = codeIndia.Value;

                if (codeIndia.TaxBase == TaxBase.AmountByUnit)
                {
                    taxVal = decimal.Zero;
                    amtPerUnitVal = taxValueLoc;
                }
                else
                {
                    if (codeIndia.Formula.TaxableBasis != TaxableBasisIndia.ExclusiveLineAmount)
                    {
                        taxVal = ((1 + taxCodeValue) * taxValueLoc) / 100;
                    }
                    else
                    {
                        taxVal = (taxCodeValue * taxValueLoc) / 100;
                    }

                    taxVal *= (100 - codeIndia.AbatementPercent) / 100;
                    amtPerUnitVal = amtPerUnit * taxValueLoc / 100;
                }
            }

            /// <summary>
            /// Gets the maximum retail price of the item.
            /// </summary>
            /// <returns>The maximum retail price of the item.</returns>
            private decimal GetItemMaxRetailPrice()
            {
                decimal maxRetailPrice = decimal.Zero;
                SalesLine salesLine = this.TaxableEntity as SalesLine;
                if (salesLine != null)
                {
                    // get maximum retail price from trade agreement
                    try
                    {
                        Customer customer = null;
                        if (!string.IsNullOrWhiteSpace(this.Transaction.CustomerId))
                        {
                            var getCustomerDataRequest = new GetCustomerDataRequest(this.Transaction.CustomerId);
                            var getCustomerDataResponse = this.RequestContext.Execute<SingleEntityDataServiceResponse<Customer>>(getCustomerDataRequest);
                            customer = getCustomerDataResponse.Entity;
                        }

                        string priceGroup = customer != null ? customer.PriceGroup : string.Empty;
                        this.indiaPriceHelper = new IndiaPriceHelper(this.RequestContext.GetChannelConfiguration(), new PricingDataServiceManager(this.RequestContext), this.Transaction, priceGroup);
                    }
                    catch (ArgumentException)
                    {
                        maxRetailPrice = decimal.Zero;
                    }

                    if (this.indiaPriceHelper != null)
                    {
                        maxRetailPrice = this.indiaPriceHelper.GetMaximumRetailPriceFromTradeAgreement(salesLine, this.RequestContext);
                    }
                }

                if (maxRetailPrice == decimal.Zero)
                {
                    // No maximum retail price was defined in trade agreement, then use maximum retail price defined in item master.
                    GetItemMaxRetailPriceDataRequest dataRequest = new GetItemMaxRetailPriceDataRequest(this.TaxableEntity.ItemId);
                    dataRequest.QueryResultSettings = new QueryResultSettings(new ColumnSet("MAXIMUMRETAILPRICE_IN"), PagingInfo.AllRecords);
                    maxRetailPrice = this.RequestContext.Runtime.Execute<SingleEntityDataServiceResponse<ItemMaxRetailPriceIndia>>(dataRequest, this.RequestContext).Entity.MaximumRetailPrice;
                }

                return maxRetailPrice;
            }
        }
    }
}
