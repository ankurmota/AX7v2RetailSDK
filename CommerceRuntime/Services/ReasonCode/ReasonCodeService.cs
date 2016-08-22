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
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// The reason code service.
        /// </summary>
        public class ReasonCodeService : IRequestHandler
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
                        typeof(GetReasonCodesServiceRequest),
                        typeof(CalculateRequiredReasonCodesServiceRequest),
                        typeof(GetReturnOrderReasonCodesServiceRequest)
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
                if (requestType == typeof(GetReasonCodesServiceRequest))
                {
                    response = GetReasonCodes((GetReasonCodesServiceRequest)request);
                }
                else if (requestType == typeof(CalculateRequiredReasonCodesServiceRequest))
                {
                    response = CalculateRequiredReasonCodes((CalculateRequiredReasonCodesServiceRequest)request);
                }
                else if (requestType == typeof(GetReturnOrderReasonCodesServiceRequest))
                {
                    response = GetReturnOrderReasonCodes((GetReturnOrderReasonCodesServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Gets the reason codes.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The info codes response.</returns>
            private static GetReasonCodesServiceResponse GetReasonCodes(GetReasonCodesServiceRequest request)
            {
                RequestContext context = request.RequestContext;
    
                GetReasonCodesDataRequest getReasonCodeRequest = new GetReasonCodesDataRequest(request.QueryResultSettings, request.ReasonCodeIds);
                var reasonCodes = context.Execute<EntityDataServiceResponse<ReasonCode>>(getReasonCodeRequest).PagedEntityCollection;
                SetProductIdsForUpsell(request.RequestContext, reasonCodes.Results);
                
                return new GetReasonCodesServiceResponse(reasonCodes);
            }
    
            /// <summary>
            /// Gets the return reason codes.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The info codes response.</returns>
            private static GetReturnOrderReasonCodesServiceResponse GetReturnOrderReasonCodes(GetReturnOrderReasonCodesServiceRequest request)
            {
                GetReturnOrderReasonCodesDataRequest getReturnOrderReasonCodesDataRequest = new GetReturnOrderReasonCodesDataRequest(request.QueryResultSettings);
                PagedResult<ReasonCode> reasonCodes = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<ReasonCode>>(getReturnOrderReasonCodesDataRequest, request.RequestContext).PagedEntityCollection;

                SetProductIdsForUpsell(request.RequestContext, reasonCodes.Results);

                return new GetReturnOrderReasonCodesServiceResponse(reasonCodes);
            }

            /// <summary>
            /// Calculates the required reason codes.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The info codes response.</returns>
            private static CalculateRequiredReasonCodesServiceResponse CalculateRequiredReasonCodes(CalculateRequiredReasonCodesServiceRequest request)
            {
                CalculateRequiredReasonCodesServiceResponse response = ReasonCodesCalculator.CalculateRequiredReasonCodes(request);
                SetProductIdsForUpsell(request.RequestContext, response.RequiredReasonCodes);
                
                return response;
            }

            /// <summary>
            /// Retrieves the products ids (record ids) for reason codes that trigger upsell.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="reasonCodes">The reason codes.</param>
            private static void SetProductIdsForUpsell(RequestContext context, IEnumerable<ReasonCode> reasonCodes)
            {
                IEnumerable<ReasonSubCode> itemSubCodes = reasonCodes.SelectMany(reasonCode => reasonCode.ReasonSubCodes)
                    .Where(subCode => subCode.TriggerFunctionType == TriggerFunctionType.Item);

                if (itemSubCodes.IsNullOrEmpty())
                {
                    return;
                }

                IEnumerable<ProductLookupClause> lookupClauses = itemSubCodes.Where(itemSubCode => !string.IsNullOrWhiteSpace(itemSubCode.TriggerCode)).Select(subCode => new ProductLookupClause(subCode.TriggerCode, inventDimId: null));
                
                var productSearchRequest = new GetProductsServiceRequest(
                        context.GetPrincipal().ChannelId,
                        lookupClauses,
                        QueryResultSettings.AllRecords);
                
                PagedResult<SimpleProduct> products = context.Execute<GetProductsServiceResponse>(productSearchRequest).Products;

                foreach (ReasonSubCode itemSubCode in itemSubCodes)
                {
                    if (string.IsNullOrWhiteSpace(itemSubCode.TriggerCode))
                    {
                        RetailLogger.Log.CrtServicesReasonCodeServiceUpsellSubCodeWithEmptyTriggerCode(itemSubCode.ReasonCodeId, itemSubCode.SubCodeId);
                        continue;
                    }

                    SimpleProduct product = products.Results.FirstOrDefault(p => string.Equals(p.ItemId, itemSubCode.TriggerCode, StringComparison.OrdinalIgnoreCase));

                    if (product != null)
                    {
                        itemSubCode.ProductId = product.RecordId;
                    }
                    else
                    {
                        RetailLogger.Log.CrtServicesReasonCodeServiceProductNotFoundForTriggeredUpsell(itemSubCode.ReasonCodeId, itemSubCode.SubCodeId, itemSubCode.TriggerCode);
                    }
                }
            }
        }
    }
}
