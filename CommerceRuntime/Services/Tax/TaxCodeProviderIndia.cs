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
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// Provides India specific functionality.
        /// </summary>
        internal sealed class TaxCodeProviderIndia : TaxCodeProvider
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TaxCodeProviderIndia"/> class.
            /// </summary>
            /// <param name="taxContext">Tax context.</param>
            internal TaxCodeProviderIndia(TaxContext taxContext)
                : base(taxContext)
            {
            }
    
            /// <summary>
            /// Sorts the specified tax codes by priority.
            /// </summary>
            /// <param name="codes">The tax codes.</param>
            /// <returns>An ordered collection of tax codes.</returns>
            protected override ReadOnlyCollection<TaxCode> SortCodes(Dictionary<string, TaxCode> codes)
            {
                if (codes == null)
                {
                    throw new ArgumentNullException("codes");
                }
    
                // Return codes to be processed in the following order:
                // Non-India codes
                // India codes ordered by Id
                return new ReadOnlyCollection<TaxCode>(
                    codes.Values.OrderBy(code =>
                    {
                        TaxCodeIndia codeIndia = code as TaxCodeIndia;
                        if (codeIndia != null)
                        {
                            return codeIndia.Formula.Id + MaxPriorityTaxCode + 1;
                        }
                        else
                        {
                            return TaxCodeProvider.TaxCodePriority(code);
                        }
                    }).ToList());
            }
    
            /// <summary>
            /// Gets the tax code.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="taxableItem">The taxable item.</param>
            /// <param name="taxCodeInterval">The tax code interval.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <returns>The tax code object.</returns>
            protected override TaxCode GetTaxCode(RequestContext context, TaxableItem taxableItem, TaxCodeInterval taxCodeInterval, SalesTransaction transaction)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }
    
                TaxCodeIntervalIndia taxCodeIntervalIndia = taxCodeInterval as TaxCodeIntervalIndia;
    
                if (taxCodeIntervalIndia.TaxType == TaxTypeIndia.None)
                {
                    return base.GetTaxCode(context, taxableItem, taxCodeInterval, transaction);
                }
                else
                {
                    return new TaxCodeIndia(context, taxableItem, taxCodeIntervalIndia, this.TaxContext, transaction);
                }
            }
    
            /// <summary>
            /// Gets the tax code intervals.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTaxGroupId">The sales tax group Id.</param>
            /// <param name="itemTaxGroupId">The item sales tax group Id.</param>
            /// <param name="transDate">The transaction date.</param>
            /// <returns>The tax code interval object.</returns>
            protected override ReadOnlyCollection<TaxCodeInterval> GetTaxCodeIntervals(RequestContext context, string salesTaxGroupId, string itemTaxGroupId, DateTimeOffset transDate)
            {
                GetTaxCodeIntervalsIndiaDataRequest dataRequest = new GetTaxCodeIntervalsIndiaDataRequest(salesTaxGroupId, itemTaxGroupId, transDate);
                ReadOnlyCollection<TaxCodeInterval> taxCodeIntervals = context.Execute<EntityDataServiceResponse<TaxCodeInterval>>(dataRequest).PagedEntityCollection.Results;
    
                return taxCodeIntervals;
            }
    
            /// <summary>
            /// Retrieves a list of TaxCodes for the given sale line item.
            /// </summary>
            /// <param name="taxableItem">The taxable item.</param>
            /// <param name="context">The context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <returns>Tax codes applicable with the taxableItem.</returns>
            /// <remarks>
            /// No user input or variable in the SQL query. No SQL injection threat.
            /// </remarks>
            protected override ReadOnlyCollection<TaxCode> GetTaxCodes(TaxableItem taxableItem, RequestContext context, SalesTransaction transaction)
            {
                // Only use codes that are not processed as India tax codes, or those that are and are
                // supported.
                return new ReadOnlyCollection<TaxCode>(base.GetTaxCodes(taxableItem, context, transaction).Where(c =>
                {
                    var codeIndia = c as TaxCodeIndia;
    
                    bool isChargeLine = taxableItem is ChargeLine;
                    return codeIndia == null || SupportedTax(isChargeLine, codeIndia, context);
                }).ToList<TaxCode>());
            }
    
            /// <summary>
            /// Only support:
            /// Codes that are of the supported tax basis.
            /// And codes that are defined in the formula (Id > 0).
            /// And codes that are of either ServiceTax or VAT or SalesTax.
            /// </summary>
            /// <param name="isChargeLine">Indicate whether taxable item is chargeLine.</param>
            /// <param name="codeIndia">The tax code.</param>
            /// <param name="context">The request context.</param>
            /// <returns>The flag indicating whether the tax type is supported or not.</returns>
            private static bool SupportedTax(bool isChargeLine, TaxCodeIndia codeIndia, RequestContext context)
            {
                GetTaxParameterDataRequest dataRequest = new GetTaxParameterDataRequest(QueryResultSettings.AllRecords);
                TaxParameters taxParameter = context.Execute<SingleEntityDataServiceResponse<TaxParameters>>(dataRequest).Entity;
    
                // Only support service tax for misc charges
                if (isChargeLine)
                {
                    return (codeIndia.Formula.SupportedTaxBasisForMiscCharge && codeIndia.Formula.Id > 0) &&
                        (codeIndia.TaxType == TaxTypeIndia.ServiceTax && taxParameter.ServiceTaxIndia);
                }
    
                if (codeIndia.Formula.SupportedTaxBasis && codeIndia.Formula.Id > 0)
                {
                    if (codeIndia.TaxType == TaxTypeIndia.SalesTax && taxParameter.SalesTaxIndia)
                    {
                        return true;
                    }
    
                    if (codeIndia.TaxType == TaxTypeIndia.ServiceTax && taxParameter.ServiceTaxIndia)
                    {
                        return true;
                    }
    
                    if (codeIndia.TaxType == TaxTypeIndia.VAT && taxParameter.VATIndia)
                    {
                        return true;
                    }
                }
    
                return false;
            }
        }
    }
}
