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
        using System.Collections.Generic;
        using System.Diagnostics;

        /// <summary>
        /// Tax Code Amount Rounder Class.
        /// Used for 2 cases:
        ///     - Sales Tax Group: Tax Code Combination Rounding Support
        ///     - Tax Code Marginal Base Support: Net amount of invoice balance.
        /// The class to keep track of the current rounded and non-rounded tax code combination totals
        /// in order to use these values later to adjust tax code amount on the sales line
        /// if there is any discrepancy appears between these values on a given sales line tax iteration.
        /// </summary>
        internal class TaxCodeAmountRounder
        {
            private Dictionary<string, Totals> currentTotals;

            internal TaxCodeAmountRounder()
            {
                this.currentTotals = new Dictionary<string, Totals>();
            }

            internal void DebugTotals()
            {
                foreach (var currentTotal in this.currentTotals)
                {
                    Debug.WriteLine("Group key: [{0}], Rounded total: {1}, non-rounded: {2}", currentTotal.Key, currentTotal.Value.TotalRoundedValue, currentTotal.Value.TotalNonRoundedValue);
                }
            }

            /// <summary>
            /// Keeps track of totals aggregated by tax codes and rounds current value accordingly.
            /// </summary>
            /// <param name="taxContext">The tax context.</param>
            /// <param name="taxCode">The tax code. Used to get rounding configuration.</param>
            /// <param name="groupKey">The tax code grouping key, such as tax code (net invoice) or tax code combination.</param>
            /// <param name="taxCodeAmountNonRounded">The non-rounded tax code amount.</param>
            /// <returns>The rounded value of tax code amount.</returns>
            internal decimal Round(TaxContext taxContext, TaxCode taxCode, string groupKey, decimal taxCodeAmountNonRounded)
            {
                // Adjustments after roundings on accrued totals are needed for following cases.
                if (taxCode.TaxGroupRounding || taxCode.TaxLimitBase == TaxLimitBase.InvoiceWithoutVat)
                {
                    // Calculate current TaxCode.TotalAmount by summing up all non-rounded, [all previous and current] taxLines.taxCode.Amounts. 
                    // That’s what we supposed to collect as a Tax for a given tax code (or combination).
                    Totals groupTotals;

                    if (this.currentTotals.TryGetValue(groupKey, out groupTotals))
                    {
                        decimal previousTotalRoundedValue = groupTotals.TotalRoundedValue;

                        groupTotals.TotalNonRoundedValue += taxCodeAmountNonRounded;
                        decimal newRoundedTotal = taxContext.TaxCurrencyOperations.Round(groupTotals.TotalNonRoundedValue, taxCode.RoundingOff, taxCode.RoundingOffType);
                        decimal newTaxCodeAmountRounded = newRoundedTotal - previousTotalRoundedValue;

                        groupTotals.TotalRoundedValue = newRoundedTotal;

                        return newTaxCodeAmountRounded;
                    }
                    else
                    {
                        groupTotals = new Totals
                        {
                            TotalNonRoundedValue = taxCodeAmountNonRounded,
                            TotalRoundedValue = taxContext.TaxCurrencyOperations.Round(taxCodeAmountNonRounded, taxCode.RoundingOff, taxCode.RoundingOffType)
                        };

                        this.currentTotals.Add(groupKey, groupTotals);

                        // no further adjustments needed since there are no previous lines for the tax code.
                        return groupTotals.TotalRoundedValue;
                    }
                }
                else
                {
                    // line (non-invoice) level, no tax code combination rounding is straight forward for given tax codes.
                    return taxContext.TaxCurrencyOperations.Round(taxCodeAmountNonRounded, taxCode.RoundingOff, taxCode.RoundingOffType);
                }
            }

            private class Totals
            {
                /// <summary>
                ///  Gets or sets rounded value of the current total of the given tax code.
                /// </summary>
                public decimal TotalRoundedValue { get; set; }

                /// <summary>
                ///  Gets or sets raw, non-rounded value of the current total of the given tax code.
                /// </summary>
                public decimal TotalNonRoundedValue { get; set; }
            }
        }
    }
}