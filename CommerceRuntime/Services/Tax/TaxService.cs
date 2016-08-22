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
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Tax Service implementation.
        /// </summary>
        public class TaxService : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(CalculateTaxServiceRequest),
                    };
                }
            }
    
            /// <summary>
            /// Executes the request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
                if (requestType == typeof(CalculateTaxServiceRequest))
                {
                    response = CalculateTax((CalculateTaxServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Gets the tax regime.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="shippingAddress">Shipping address.</param>
            /// <returns>
            /// The sales tax regime information.
            /// </returns>
            internal static string GetTaxRegime(RequestContext context, Address shippingAddress)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }
    
                if (context.Runtime == null)
                {
                    throw new ArgumentNullException("context", "context.Runtime");
                }
    
                if (shippingAddress == null)
                {
                    throw new ArgumentNullException("shippingAddress");
                }
    
                if (string.IsNullOrWhiteSpace(shippingAddress.ThreeLetterISORegionName) &&
                    string.IsNullOrWhiteSpace(shippingAddress.TwoLetterISORegionName))
                {
                    throw new ArgumentNullException("shippingAddress", "shippingAddress ISORegionName");
                }
    
                string taxRegime = string.Empty;
    
                // If address has a tax group, then return this tax group.
                if (!string.IsNullOrWhiteSpace(shippingAddress.TaxGroup))
                {
                    taxRegime = shippingAddress.TaxGroup;
                    return taxRegime;
                }
    
                Dictionary<string, string> predicates = new Dictionary<string, string>();
    
                var addressComponentsFilterHandlers = BuildDbtFilterList();
                for (int i = 0; i < addressComponentsFilterHandlers.Count; i++)
                {
                    predicates.Clear();
    
                    for (int j = i; j < addressComponentsFilterHandlers.Count; j++)
                    {
                        if (addressComponentsFilterHandlers[j].CanHandle(shippingAddress))
                        {
                            addressComponentsFilterHandlers[j].Handle(shippingAddress, predicates);
                        }
                    }
    
                    GetSalesTaxGroupDataRequest dataRequest = new GetSalesTaxGroupDataRequest(predicates);
                    taxRegime = context.Execute<SingleEntityDataServiceResponse<string>>(dataRequest).Entity;
    
                    if (!string.IsNullOrWhiteSpace(taxRegime))
                    {
                        break;
                    }
                }
    
                if (string.IsNullOrWhiteSpace(taxRegime))
                {
                    InvalidTaxGroupNotification notification = new InvalidTaxGroupNotification(shippingAddress);
                    context.Notify(notification);
                }
    
                return taxRegime;
            }
    
            /// <summary>
            /// Gets the tax group for India inter-state transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="inventLocationId">Inventory location id.</param>
            /// <param name="shippingAddress">Shipping address.</param>
            /// <param name="isInterState">The flag indicates whether it's inter state.</param>
            /// <returns>
            /// The sales tax group information.
            /// </returns>
            internal static string GetInterStateTaxRegimeIndia(RequestContext context, string inventLocationId, Address shippingAddress, out bool isInterState)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }
    
                if (context.Runtime == null)
                {
                    throw new ArgumentException("context.Runtime cannot be null.");
                }
    
                if (shippingAddress == null)
                {
                    throw new ArgumentNullException("shippingAddress");
                }
    
                if (string.IsNullOrWhiteSpace(shippingAddress.ThreeLetterISORegionName) &&
                    string.IsNullOrWhiteSpace(shippingAddress.TwoLetterISORegionName))
                {
                    throw new ArgumentNullException("shippingAddress", "shippingAddress ISORegionName");
                }
    
                string taxRegime = string.Empty;
                isInterState = false;
                bool isApplyInterStateTax = false;
    
                if (!string.IsNullOrWhiteSpace(inventLocationId))
                {
                    GetApplyInterstateTaxIndiaDataRequest getApplyInterstateTaxIndiaDataRequest = new GetApplyInterstateTaxIndiaDataRequest(QueryResultSettings.SingleRecord);
                    ApplyInterStateTaxIndia interStateTaxSetting = context.Runtime.Execute<SingleEntityDataServiceResponse<ApplyInterStateTaxIndia>>(getApplyInterstateTaxIndiaDataRequest, context).Entity;
    
                    if (interStateTaxSetting != null)
                    {
                        isApplyInterStateTax = interStateTaxSetting.IsApplyInterStateTaxIndia;
                    }
    
                    if (isApplyInterStateTax)
                    {
                        GetWarehouseAddressIndiaDataRequest getWarehouseAddressIndiaDataRequest = new GetWarehouseAddressIndiaDataRequest(inventLocationId, QueryResultSettings.SingleRecord);
                        Address shippingFromAddress = context.Runtime.Execute<SingleEntityDataServiceResponse<Address>>(getWarehouseAddressIndiaDataRequest, context).Entity;
    
                        if (shippingFromAddress != null &&
                            (shippingFromAddress.ThreeLetterISORegionName == shippingAddress.ThreeLetterISORegionName) &&
                            !string.IsNullOrWhiteSpace(shippingFromAddress.State) &&
                            !string.IsNullOrWhiteSpace(shippingAddress.State) &&
                            (shippingFromAddress.State != shippingAddress.State))
                        {
                            isInterState = true;
                        }
    
                        if (isInterState)
                        {
                            GetTaxRegimeIndiaDataRequest getTaxRegimeIndiaDataRequest = new GetTaxRegimeIndiaDataRequest(QueryResultSettings.SingleRecord);
                            taxRegime = context.Runtime.Execute<SingleEntityDataServiceResponse<string>>(getTaxRegimeIndiaDataRequest, context).Entity;
    
                            if (string.IsNullOrWhiteSpace(taxRegime))
                            {
                                InvalidTaxGroupNotification notification = new InvalidTaxGroupNotification(shippingFromAddress);
                                context.Notify(notification);
                            }
                        }
                    }
                }
    
                return taxRegime;
            }
    
            /// <summary>
            /// Calculates the tax for the last item.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private static CalculateTaxServiceResponse CalculateTax(CalculateTaxServiceRequest request)
            {
                ThrowIf.Null(request, "request");
    
                TaxHelpers.SetSalesTaxGroup(request.RequestContext, request.Transaction);
                SalesTaxOverrideHelper.CalculateTaxOverrides(request.RequestContext, request.Transaction);
    
                // Consider active (non-void) lines for tax.
                // Need to recalculate tax on return-by-receipt lines because we cannot reconstruct tax lines from return transaction lines alone.
                // A few key information like IsExempt, IsTaxInclusive, TaxCode are not available on return transaction line.
                foreach (var saleItem in request.Transaction.ActiveSalesLines)
                {
                    saleItem.TaxRatePercent = 0;
                    saleItem.TaxLines.Clear();
                }
    
                var totaler = new SalesTransactionTotaler(request.Transaction);
                totaler.CalculateTotals(request.RequestContext);
    
                ClearChargeTaxLines(request.Transaction);
    
                TaxContext taxContext = new TaxContext(request.RequestContext);
    
                TaxCodeProvider defaultProvider = GetTaxProvider(request.RequestContext, taxContext);
                defaultProvider.CalculateTax(request.RequestContext, request.Transaction);
    
                return new CalculateTaxServiceResponse(request.Transaction);
            }
    
            /// <summary>
            /// Get the tax code provider.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="taxContext">Tax context.</param>
            /// <returns>The tax code provider.</returns>
            private static TaxCodeProvider GetTaxProvider(RequestContext context, TaxContext taxContext)
            {
                TaxCodeProvider taxCodeProvider;
                switch (context.GetChannelConfiguration().CountryRegionISOCode)
                {
                    case CountryRegionISOCode.IN:
                        taxCodeProvider = new TaxCodeProviderIndia(taxContext);
                        break;
                    default:
                        taxCodeProvider = new TaxCodeProvider(taxContext);
                        break;
                }
    
                return taxCodeProvider;
            }
    
            /// <summary>
            /// Clears the tax lines in miscellaneous charge.
            /// </summary>
            /// <param name="transaction">The sales transaction.</param>
            private static void ClearChargeTaxLines(SalesTransaction transaction)
            {
                foreach (var charge in transaction.ChargeLines)
                {
                    charge.TaxLines.Clear();
                }
    
                // Consider active (non-void) lines for tax.
                // Need to recalculate tax on return-by-receipt lines because we cannot reconstruct tax lines from return transaction lines alone.
                // A few key information like IsExempt, IsTaxInclusive, TaxCode are not available on return transaction line.
                foreach (var line in transaction.ActiveSalesLines)
                {
                    foreach (var charge in line.ChargeLines)
                    {
                        charge.TaxLines.Clear();
                    }
                }
            }
    
            /// <summary>
            /// Builds the chain of destination-based tax filters.
            /// </summary>
            /// <returns>A collection of tax filters.</returns>
            private static List<DestinationFilterHandler> BuildDbtFilterList()
            {
                var filterList = new List<DestinationFilterHandler>();
    
                // This is order dependent.
                filterList.Add(DestinationFilterHandler.BuildZipPostalCodeFilter());
                filterList.Add(DestinationFilterHandler.BuildDistrictFilter());
                filterList.Add(DestinationFilterHandler.BuildCityFilter());
                filterList.Add(DestinationFilterHandler.BuildCountyFilter());
                filterList.Add(DestinationFilterHandler.BuildStateFilter());
                filterList.Add(DestinationFilterHandler.BuildCountryFilter());
    
                return filterList;
            }
        }
    }
}
