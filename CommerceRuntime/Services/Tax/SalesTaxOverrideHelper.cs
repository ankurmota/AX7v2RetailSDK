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
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Adds or updates a tax override.
        /// </summary>
        public static class SalesTaxOverrideHelper
        {
            /// <summary>
            /// Updates the tax overrides for cart.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="transaction">Transaction to calculate taxes for.</param>
            internal static void CalculateTaxOverrides(RequestContext context, SalesTransaction transaction)
            {
                // apply cart level
                if (!string.IsNullOrWhiteSpace(transaction.TaxOverrideCode))
                {
                    context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.OverrideTaxTransactionList));

                    GetTaxOverrideDetailsDataRequest dataServiceRequest = new GetTaxOverrideDetailsDataRequest(transaction.TaxOverrideCode);
                    dataServiceRequest.QueryResultSettings = QueryResultSettings.AllRecords;
                    SingleEntityDataServiceResponse<TaxOverride> response = context.Runtime.Execute<SingleEntityDataServiceResponse<TaxOverride>>(dataServiceRequest, context);
    
                    ApplyTransactionLevelOverride(transaction, response.Entity);
                }
    
                // apply linelevel
                // note: it is not a meaningful business case to have both cart and line level overrides, but technically it is still possible
                // on the off chance, this happens, line's will override cart-inherited overrides
                if (transaction.SalesLines != null &&
                    transaction.SalesLines.Any())
                {
                    foreach (var line in transaction.SalesLines)
                    {
                        if (!string.IsNullOrWhiteSpace(line.TaxOverrideCode) &&
                            string.CompareOrdinal(line.TaxOverrideCode, transaction.TaxOverrideCode) != 0)
                        {
                            context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.OverrideTaxLineList));

                            GetTaxOverrideDetailsDataRequest dataServiceRequest = new GetTaxOverrideDetailsDataRequest(line.TaxOverrideCode);
                            dataServiceRequest.QueryResultSettings = QueryResultSettings.AllRecords;
                            SingleEntityDataServiceResponse<TaxOverride> response = context.Runtime.Execute<SingleEntityDataServiceResponse<TaxOverride>>(dataServiceRequest, context);
    
                            ApplyLineLevelOverride(transaction, response.Entity, line.LineId);
                        }
                    }
                }
            }
    
            /// <summary>
            /// Applies the line level override.
            /// </summary>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="taxOverride">The tax override.</param>
            /// <param name="lineId">The line id.</param>
            /// <returns>True if tax override successfully applied, false otherwise.</returns>
            private static bool ApplyLineLevelOverride(SalesTransaction salesTransaction, TaxOverride taxOverride, string lineId)
            {
                // get the applicable line
                var line = salesTransaction.SalesLines.Where(l => l.LineId == lineId).FirstOrDefault();
                bool overridden = false;
    
                if (line != null)
                {
                    switch (taxOverride.OverrideType)
                    {
                        case TaxOverrideType.ItemSalesTaxGroup:
    
                            if (string.IsNullOrWhiteSpace(taxOverride.SourceTaxGroup) ||
                                 taxOverride.SourceTaxGroup.Equals(line.ItemTaxGroupId, StringComparison.OrdinalIgnoreCase))
                            {
                                line.OriginalItemTaxGroupId = line.ItemTaxGroupId;
                                line.ItemTaxGroupId = taxOverride.DestinationItemTaxGroup;
                                overridden = true;
                            }
    
                            break;
                        case TaxOverrideType.SalesTaxGroup:
                            if (string.IsNullOrWhiteSpace(taxOverride.SourceTaxGroup) ||
                                 taxOverride.SourceTaxGroup.Equals(line.SalesTaxGroupId, StringComparison.OrdinalIgnoreCase))
                            {
                                line.OriginalSalesTaxGroupId = line.SalesTaxGroupId;
                                line.SalesTaxGroupId = taxOverride.DestinationTaxGroup;
                                overridden = true;
                            }
    
                            break;
                    }
    
                    line.TaxOverrideCode = taxOverride.Code;
                }
    
                return overridden;
            }
    
            /// <summary>
            /// Applies the transaction level override.
            /// </summary>
            /// <param name="transaction">The transaction.</param>
            /// <param name="taxOverride">The tax override.</param>
            /// <returns>True if tax override successfully applied, false otherwise.</returns>
            private static bool ApplyTransactionLevelOverride(SalesTransaction transaction, TaxOverride taxOverride)
            {
                bool overridden = false;
                transaction.TaxOverrideCode = taxOverride.Code;
    
                if (transaction.SalesLines != null && transaction.SalesLines.Any())
                {
                    foreach (var line in transaction.SalesLines)
                    {
                        overridden |= ApplyLineLevelOverride(transaction, taxOverride, line.LineId);
                    }
                }
    
                return overridden;
            }
        }
    }
}
