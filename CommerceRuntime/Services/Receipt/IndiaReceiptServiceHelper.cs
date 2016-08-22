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
    namespace Commerce.Runtime.Services.ReceiptIndia
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// The India receipt helper to get the formatted receipts.
        /// </summary>
        public static class IndiaReceiptServiceHelper
        {
            private const string Line = "line";
            private const string LineID = "line_id";
            private const string CharPosTable = "charpos";
            private const string SortAsc = "nr asc";
            private const char TaxCodeBeginChar = 'A';
            private const char TaxCodeEndChar = 'Z';
            private const string SingleSpace = "|1C";
            private const string DoubleSpace = "|2C";
            private const string Esc = "&#x1B;";

            /// <summary>
            /// Populate tax summary for India.
            /// </summary>
            /// <param name="salesTransaction">The transaction.</param>
            /// <param name="taxSummarySettingIndia">Tax summary setting.</param>
            internal static void PopulateTaxSummaryForIndia(SalesTransaction salesTransaction, TaxSummarySettingIndia taxSummarySettingIndia)
            {
                if (salesTransaction == null)
                {
                    throw new ArgumentNullException("salesTransaction");
                }

                if (taxSummarySettingIndia == null)
                {
                    throw new ArgumentNullException("taxSummarySettingIndia");
                }

                if (taxSummarySettingIndia.TaxDetailsType == ReceiptTaxDetailsTypeIndia.PerTaxComponent)
                {
                    IList<TaxLineIndia> indiaTaxItems = new List<TaxLineIndia>();

                    foreach (SalesLine saleLine in salesTransaction.ActiveSalesLines)
                    {
                        foreach (TaxLine taxLine in saleLine.TaxLines)
                        {
                            TaxLineIndia taxLineIndia = taxLine as TaxLineIndia;
                            if (taxLineIndia != null)
                            {
                                indiaTaxItems.Add(taxLineIndia);
                            }
                        }
                    }

                    if (taxSummarySettingIndia.ShowTaxonTax)
                    {
                        salesTransaction.TaxLines.Clear();
                        salesTransaction.TaxLines.AddRange(BuildIndiaTaxSummaryPerComponentShowTaxonTax(indiaTaxItems));
                    }
                    else
                    {
                        salesTransaction.TaxLines.Clear();
                        salesTransaction.TaxLines.AddRange(BuildIndiaTaxSummaryPerComponentNotShowTaxonTax(indiaTaxItems));
                    }
                }
                else if (taxSummarySettingIndia.TaxDetailsType == ReceiptTaxDetailsTypeIndia.PerLine)
                {
                    salesTransaction.TaxLines.Clear();
                    salesTransaction.TaxLines.AddRange(BuildIndiaTaxSummaryPerLine(salesTransaction));
                }
            }

            /// <summary>
            /// Build tax summary lines of the India receipt, with tax amounts be aggregated by sale line items.
            /// </summary>
            /// <param name="theTransaction">The retail transaction.</param>
            /// <returns>The tax summary lines of the India receipt.</returns>
            /// <remarks>
            /// In this case, the settings of <c>"RetailStoreTable > Misc > Receipts"</c> is as follows,
            /// 1) The "Tax details" option is set as "Per line"
            /// 2) The "Show tax on tax" option is set as "N/A", as it is disabled in this case
            /// For example, the retail transaction has four sale line items, as follows,
            /// <c>
            /// Item ID | Price | Tax code | Formula       | Tax basis | Tax rate | Tax amount
            /// 0001    | 100   | SERV5    | Line amount   | 100.00    |  5%      |  5.00
            ///         |       | E-CSS5   | Excl.+[SERV5] |   5.00    |  5%      |  0.25
            /// 0002    | 100   | VAT10    | Line amount   | 100.00    | 10%      | 10.00
            ///         |       | Surchg2  | Excl.+[VAT10] |  10.00    |  2%      |  0.20
            /// 0003    | 100   | SERV4    | Line amount   | 100.00    |  4%      |  4.00
            ///         |       | E-CSS5   | Excl.+[SERV4] |   4.00    |  5%      |  0.20
            /// 0004    | 100   | VAT12    | Line amount   | 100.00    | 12%      | 12.00
            ///         |       | Surchg2  | Excl.+[VAT12] |  12.00    |  2%      |  0.24
            /// And the tax summary lines will be as follows,
            /// Tax code | Tax basis | Tax rate | Tax amount
            /// AA       | 100.00    |  5.25%   |  5.25
            /// AB       | 100.00    | 10.20%   | 10.20
            /// AC       | 100.00    |  4.20%   |  4.20
            /// AD       | 100.00    | 12.24%   | 12.24
            /// Tax codes are automatically named from "AA" to "AZ", ...
            /// </c>
            /// </remarks>
            private static IList<TaxLine> BuildIndiaTaxSummaryPerLine(SalesTransaction theTransaction)
            {
                if (theTransaction == null)
                {
                    throw new ArgumentNullException("theTransaction");
                }

                List<TaxLine> lines = new List<TaxLine>();

                char code1 = TaxCodeBeginChar, code2 = TaxCodeBeginChar;
                foreach (SalesLine saleItem in theTransaction.ActiveSalesLines)
                {
                    TaxLineIndia t = new TaxLineIndia();
                    t.TaxCode = string.Empty + code1 + code2;
                    t.TaxBasis = saleItem.TaxLines.First(x => !(x as TaxLineIndia).IsTaxOnTax).TaxBasis;
                    t.Amount = saleItem.TaxLines.Sum(x => x.Amount);
                    t.Percentage = t.TaxBasis != decimal.Zero ? 100 * t.Amount / t.TaxBasis : decimal.Zero;
                    lines.Add(t);

                    // Generate tax code of the next line
                    code2++;
                    if (code2 > TaxCodeEndChar)
                    {
                        code2 = TaxCodeBeginChar;
                        code1++;

                        if (code1 > TaxCodeEndChar)
                        {
                            code1 = TaxCodeBeginChar;
                        }
                    }
                }

                return lines;
            }

            /// <summary>
            /// Build tax summary line of the India receipt, with tax amounts be aggregated by "main" tax codes (which
            /// are not India tax on tax codes).
            /// </summary>
            /// <param name="indiaTaxItems">All tax items of the India retail transaction.</param>
            /// <returns>The tax summary lines of the India receipt.</returns>
            /// <remarks>
            /// In this case, the settings of <c>>"RetailStoreTable > Misc. > Receipts"</c> is as follows,
            /// 1) The "Tax details" option is set as "Per tax component"
            /// 2) The "Show tax on tax" option is turned OFF
            /// For example, the retail transaction has four sale line items, as follows,
            /// <c>
            /// Item ID | Price | Tax code | Formula       | Tax basis | Tax rate | Tax amount
            /// 0001    | 100   | SERV5    | Line amount   | 100.00    |  5%      |  5.00
            ///         |       | E-CSS5   | Excl.+[SERV5] |   5.00    |  5%      |  0.25
            /// 0002    | 100   | VAT10    | Line amount   | 100.00    | 10%      | 10.00
            ///         |       | Surchg2  | Excl.+[VAT10] |  10.00    |  2%      |  0.20
            /// 0003    | 100   | SERV4    | Line amount   | 100.00    |  4%      |  4.00
            ///         |       | E-CSS5   | Excl.+[SERV4] |   4.00    |  5%      |  0.20
            /// 0004    | 100   | VAT12    | Line amount   | 100.00    | 12%      | 12.00
            ///         |       | Surchg2  | Excl.+[VAT12] |  12.00    |  2%      |  0.24
            /// And the tax summary lines will be as follows,
            /// Tax code | Tax basis | Tax rate | Tax amount
            /// SERV5    | 100.00    |  5.25%   |  5.25
            /// SERV4    | 100.00    |  4.20%   |  4.20
            /// VAT10    | 100.00    | 10.20%   | 10.20
            /// VAT12    | 100.00    | 12.24%   | 12.24.
            /// </c>
            /// </remarks>
            private static IList<TaxLine> BuildIndiaTaxSummaryPerComponentNotShowTaxonTax(IList<TaxLineIndia> indiaTaxItems)
            {
                if (indiaTaxItems == null)
                {
                    throw new ArgumentNullException("indiaTaxItems");
                }

                if (indiaTaxItems.Count == 0)
                {
                    throw new ArgumentException("The specified collection cannot be empty.", "indiaTaxItems");
                }

                List<TaxLine> lines = new List<TaxLine>();
                var groups = indiaTaxItems.GroupBy(x =>
                {
                    string taxCode = x.IsTaxOnTax ?
                        x.TaxFormula.Split(',').First() :
                        x.TaxCode;
                    return new { x.TaxGroup, taxCode };
                });

                foreach (var group in groups)
                {
                    TaxLineIndia t = new TaxLineIndia();
                    t.TaxGroup = group.Key.TaxGroup;
                    t.TaxCode = group.Key.taxCode;
                    t.TaxBasis = group.First(x => !x.IsTaxOnTax).TaxBasis;
                    t.Amount = group.Sum(x => x.Amount);
                    t.Percentage = t.TaxBasis != decimal.Zero ? (100 * t.Amount / t.TaxBasis) : decimal.Zero;
                    t.TaxComponent = group.First(x => !x.IsTaxOnTax).TaxComponent;
                    lines.Add(t);
                }

                // Order by tax component
                lines = new List<TaxLine>(lines.OrderBy(x => (x as TaxLineIndia).TaxComponent));

                return lines;
            }

            /// <summary>
            /// Build tax summary line of the India receipt, with tax amounts be aggregated by tax codes.
            /// </summary>
            /// <param name="indiaTaxItems">All tax items of the India retail transaction.</param>
            /// <returns>The tax summary lines of the India receipt.</returns>
            /// <remarks>
            /// In this case, the settings of <c>"RetailStoreTable > Misc. > Receipts"</c> is as follows,
            /// 1) The "Tax details" option is set as "Per tax component"
            /// 2) The "Show tax on tax" option is turned ON
            /// For example, the retail transaction has four sale line items, as follows,
            /// <c>
            /// Item ID | Price | Tax code | Formula       | Tax basis | Tax rate | Tax amount
            /// 0001    | 100   | SERV5    | Line amount   | 100.00    |  5%      |  5.00
            ///         |       | E-CSS5   | Excl.+[SERV5] |   5.00    |  5%      |  0.25
            /// 0002    | 100   | VAT10    | Line amount   | 100.00    | 10%      | 10.00
            ///         |       | Surchg2  | Excl.+[VAT10] |  10.00    |  2%      |  0.20
            /// 0003    | 100   | SERV4    | Line amount   | 100.00    |  4%      |  4.00
            ///         |       | E-CSS5   | Excl.+[SERV4] |   4.00    |  5%      |  0.20
            /// 0004    | 100   | VAT12    | Line amount   | 100.00    | 12%      | 12.00
            ///         |       | Surchg2  | Excl.+[VAT12] |  12.00    |  2%      |  0.24
            /// And the tax summary lines will be as follows,
            /// Tax component | Tax code | Tax basis | Tax rate | Tax amount
            /// Service       | SERV5    | 100.00    |  5%      |  5.00
            /// Service       | SERV4    | 100.00    |  4%      |  4.00
            /// E-CSS         | E-CSS5   |   9.00    |  5%      |  0.45
            /// VAT           | VAT10    | 100.00    | 10%      | 10.00
            /// VAT           | VAT12    | 100.00    | 12%      | 12.00
            /// Surcharge     | Surchg2  |  22.00    |  2%      |  0.44.
            /// </c>
            /// </remarks>
            private static IList<TaxLine> BuildIndiaTaxSummaryPerComponentShowTaxonTax(IList<TaxLineIndia> indiaTaxItems)
            {
                List<TaxLine> lines = new List<TaxLine>();

                var groups = indiaTaxItems.GroupBy(x => new { x.TaxComponent, x.TaxCode });
                foreach (var group in groups)
                {
                    TaxLineIndia t = new TaxLineIndia();
                    t.TaxComponent = group.Key.TaxComponent;
                    t.TaxCode = group.Key.TaxCode;
                    t.Amount = group.Sum(x => x.Amount);
                    t.Percentage = group.First().Percentage;
                    t.TaxBasis = group.Sum(x => x.TaxBasis);
                    t.TaxGroup = group.First().TaxGroup;

                    lines.Add(t);
                }

                // Order by tax component
                lines = new List<TaxLine>(lines.OrderBy(x => (x as TaxLineIndia).TaxComponent));

                return lines;
            }
        }
    }
}