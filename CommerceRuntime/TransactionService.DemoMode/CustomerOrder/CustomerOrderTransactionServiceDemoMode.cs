/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1403:FileMayOnlyContainASingleNamespace", Justification = "This file requires multiple namespaces to support the Retail Sdk code generation.")]

namespace Contoso
{
    namespace Commerce.Runtime.TransactionService
    {
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using Commerce.Runtime.Services.CustomerOrder;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// Customer order demo mode transaction service.
        /// </summary>
        public class CustomerOrderTransactionServiceDemoMode : IRequestHandler
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
                        typeof(SaveCustomerOrderRealtimeRequest),
                        typeof(RecallCustomerOrderRealtimeRequest),
                        typeof(GetInvoiceRealtimeRequest),
                        typeof(PickAndPackOrderRealtimeRequest),
                        typeof(GetCustomerOrderCalculationModesServiceRequest),
                        typeof(SettleInvoiceRealtimeRequest)
                    };
                }
            }
    
            /// <summary>
            /// Processes the requests.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            public Response Execute(Request request)
            {
                Response response;
                GetCustomerOrderCalculationModesServiceRequest getCustomerOrderCalculationModesServiceRequest;
    
                if ((request as SaveCustomerOrderRealtimeRequest) != null)
                {
                    NetTracer.Information("CustomerOrderService.ExecuteRequest(): SaveCustomerOrderServiceRequest");
                    response = SaveCustomerOrder();
                }
                else if ((request as RecallCustomerOrderRealtimeRequest) != null)
                {
                    NetTracer.Information("CustomerOrderService.ExecuteRequest(): RecallCustomerOrderServiceRequest");
                    response = RecallCustomerOrder();
                }
                else if ((request as GetInvoiceRealtimeRequest) != null)
                {
                    NetTracer.Information("CustomerOrderService.ExecuteRequest(): GetInvoiceServiceRequest");
                    response = GetInvoices();
                }
                else if ((request as PickAndPackOrderRealtimeRequest) != null)
                {
                    NetTracer.Information("CustomerOrderService.ExecuteRequest(): PickAndPackOrderServiceRequest");
                    response = PickAndPackOrder();
                }
                else if ((getCustomerOrderCalculationModesServiceRequest = request as GetCustomerOrderCalculationModesServiceRequest) != null)
                {
                    NetTracer.Information("CustomerOrderService.ExecuteRequest(): GetCustomerOrderCalculationModesServiceRequest");
                    response = CalculationModesHelper.GetCalculationModes(getCustomerOrderCalculationModesServiceRequest);
                }
                else if ((request as SettleInvoiceRealtimeRequest) != null)
                {
                    NetTracer.Information("CustomerOrderService.ExecuteRequest(): SettleInvoiceServiceRequest");
                    response = SettleInvoice();
                }
                else
                {
                    NetTracer.Information("CustomerOrderService.ExecuteRequest(): Unknown Request");
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request));
                }
    
                return response;
            }
    
            /// <summary>
            /// Picks and packs customer order.
            /// </summary>
            /// <returns>The operation result.</returns>
            private static Response PickAndPackOrder()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "PickAndPackOrder is not supported in demo mode.");
            }
    
            /// <summary>
            /// Get invoices filtering by the request.
            /// </summary>
            /// <returns>The response containing the invoices.</returns>
            private static GetInvoiceRealtimeResponse GetInvoices()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "GetInvoices is not supported in demo mode.");
            }
    
            /// <summary>
            /// Settle payment against an invoice.
            /// </summary>
            /// <returns>Response containing the result of the settlement.</returns>
            private static SettleInvoiceRealtimeResponse SettleInvoice()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "SettleInvoice is not supported in demo mode.");
            }
    
            /// <summary>
            /// Recall a customer order by sales id.
            /// </summary>
            /// <returns>The response containing the sales order.</returns>
            private static RecallCustomerOrderRealtimeResponse RecallCustomerOrder()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "RecallCustomerOrder is not supported in demo mode.");
            }
    
            /// <summary>
            /// Processes a save customer order request.
            /// </summary>
            /// <returns>The customer order service default response.</returns>
            private static Response SaveCustomerOrder()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "SaveCustomerOrder is not supported in demo mode.");
            }
        }
    }
}
