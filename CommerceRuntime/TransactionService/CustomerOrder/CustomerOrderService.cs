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
        using System.Globalization;
        using System.Linq;
        using System.Xml;
        using Commerce.Runtime.Services.CustomerOrder;
        using Commerce.Runtime.TransactionService;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Customer order service.
        /// </summary>
        public class CustomerOrderService : IRequestHandler
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
                SaveCustomerOrderRealtimeRequest saveCustomerOrderRealtimeRequest;
                RecallCustomerOrderRealtimeRequest recallCustomerOrderRealtimeRequest;
                GetInvoiceRealtimeRequest getInvoiceRealtimeRequest;
                PickAndPackOrderRealtimeRequest pickAndPackOrderRealtimeRequest;                
                SettleInvoiceRealtimeRequest settleInvoiceRequest;
    
                if ((saveCustomerOrderRealtimeRequest = request as SaveCustomerOrderRealtimeRequest) != null)
                {
                    NetTracer.Information("CustomerOrderService.ExecuteRequest(): SaveCustomerOrderRealtimeRequest");
                    response = CustomerOrderService.SaveCustomerOrder(saveCustomerOrderRealtimeRequest);
                }
                else if ((recallCustomerOrderRealtimeRequest = request as RecallCustomerOrderRealtimeRequest) != null)
                {
                    NetTracer.Information("CustomerOrderService.ExecuteRequest(): RecallCustomerOrderRealtimeRequest");
                    response = CustomerOrderService.RecallCustomerOrder(recallCustomerOrderRealtimeRequest);
                }
                else if ((getInvoiceRealtimeRequest = request as GetInvoiceRealtimeRequest) != null)
                {
                    NetTracer.Information("CustomerOrderService.ExecuteRequest(): GetInvoiceRealtimeRequest");
                    response = CustomerOrderService.GetInvoices(getInvoiceRealtimeRequest);
                }
                else if ((pickAndPackOrderRealtimeRequest = request as PickAndPackOrderRealtimeRequest) != null)
                {
                    NetTracer.Information("CustomerOrderService.ExecuteRequest(): PickAndPackOrderRealtimeRequest");
                    response = CustomerOrderService.PickAndPackOrder(pickAndPackOrderRealtimeRequest);
                }               
                else if ((settleInvoiceRequest = request as SettleInvoiceRealtimeRequest) != null)
                {
                    NetTracer.Information("CustomerOrderService.ExecuteRequest(): SettleInvoiceRealtimeRequest");
                    response = CustomerOrderService.SettleInvoice(settleInvoiceRequest);
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
            /// <param name="request">The request for picking/packing.</param>
            /// <returns>The operation result.</returns>
            private static Response PickAndPackOrder(PickAndPackOrderRealtimeRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (string.IsNullOrWhiteSpace(request.SalesId))
                {
                    throw new ArgumentException("SalesId must be set in request", "request");
                }
    
                if (string.IsNullOrWhiteSpace(request.InventoryLocationId))
                {
                    throw new ArgumentException("InventoyLocationId must be set in request", "request");
                }
    
                var client = new TransactionServiceClient(request.RequestContext);
                if (request.CreatePickingList)
                {
                    client.CreatePickingList(request.SalesId, request.InventoryLocationId);
                }
    
                if (request.CreatePackingSlip)
                {
                    client.CreatePackingSlip(request.SalesId, request.InventoryLocationId);
                }
    
                return new NullResponse();
            }
    
            /// <summary>
            /// Get invoices filtering by the request.
            /// </summary>
            /// <param name="request">The request containing the sales or invoice id.</param>
            /// <returns>The response containing the invoices.</returns>
            private static GetInvoiceRealtimeResponse GetInvoices(GetInvoiceRealtimeRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (string.IsNullOrWhiteSpace(request.SalesId) && string.IsNullOrWhiteSpace(request.InvoiceId) && string.IsNullOrWhiteSpace(request.CustomerAccountNumber))
                {
                    throw new ArgumentException("CustomerAccount, SalesId and InvoiceId must be set in request", "request");
                }
    
                // Calls TS API
                ReadOnlyCollection<object> transactionResponse;
    
                if (!string.IsNullOrWhiteSpace(request.CustomerAccountNumber))
                {
                    // if a Customer Account was specified, then get all invoices for the given customer.
                    transactionResponse = new TransactionServiceClient(request.RequestContext).GetSalesInvoices(request.CustomerAccountNumber);
                }
                else
                {
                    // Else, get the specific invoice(s) for the given sales/invoice id.
                    transactionResponse = new TransactionServiceClient(request.RequestContext).GetSalesInvoices(request.SalesId, request.InvoiceId);
                }
    
                SalesInvoice[] invoices = null;
                SalesOrder order = null;
    
                if (!string.IsNullOrWhiteSpace(request.CustomerAccountNumber))
                {
                    // If CustomerAccount was specified, parse the list of invoices for that customer.
                    try
                    {
                        invoices = InvoiceHelper.GetInvoicesFromArray(transactionResponse);
                    }
                    catch (XmlException ex)
                    {
                        RetailLogger.Log.CrtTransactionServiceInvoiceXmlDocumentCreationFailure(request.CustomerAccountNumber, request.SalesId, request.InvoiceId, ex);
                    }
                }
                else if (string.IsNullOrWhiteSpace(request.InvoiceId))
                {
                    // If a sales order id was specified, parse the list of invoices from that sales order.
                    try
                    {
                        invoices = InvoiceHelper.GetInvoicesFromXml(transactionResponse[0].ToString());
                    }
                    catch (XmlException ex)
                    {
                        RetailLogger.Log.CrtTransactionServiceInvoiceXmlDocumentCreationFailure(request.CustomerAccountNumber, request.SalesId, request.InvoiceId, ex);
                    }
                }
                else
                {
                    // Otherwise, parse the invoice details from the single given invoice id.
                    try
                    {
                        order = InvoiceHelper.GetSalesOrderFromXml(transactionResponse[0].ToString(), request.RequestContext);
                    }
                    catch (XmlException ex)
                    {
                        RetailLogger.Log.CrtTransactionServiceInvoiceXmlDocumentCreationFailure(request.CustomerAccountNumber, request.SalesId, request.InvoiceId, ex);
                    }
    
                    // Check that the channel currency code is the same as the recalled order
                    if (order != null && !string.IsNullOrWhiteSpace(order.CurrencyCode) && !request.RequestContext.GetChannelConfiguration().Currency.Equals(order.CurrencyCode, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CurrencyChannelOrderMismatch,
                            string.Format("Channel currency = {0} doesn't match sales order currency = {1}", request.RequestContext.GetChannelConfiguration().Currency, order.CurrencyCode));
                    }
                }
    
                return new GetInvoiceRealtimeResponse(invoices.AsPagedResult(), order);
            }
    
            /// <summary>
            /// Settle payment against an invoice.
            /// </summary>
            /// <param name="request">Request containing the settlement information.</param>
            /// <returns>Response containing the result of the settlement.</returns>
            private static SettleInvoiceRealtimeResponse SettleInvoice(SettleInvoiceRealtimeRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (request.InvoiceId == null)
                {
                    throw new ArgumentException("request.InvoiceId cannot be null.");
                }
    
                // Calls TS API
                // if a Customer Account was specified, then get all invoices for the given customer.
                new TransactionServiceClient(request.RequestContext).PaySalesInvoice(
                    request.InvoiceId,
                    request.InvoiceAmount,
                    request.RequestContext.GetTerminal().TerminalId,
                    request.RequestContext.GetOrgUnit().OrgUnitNumber,
                    request.TransactionId);
    
                return new SettleInvoiceRealtimeResponse(request.InvoiceId);
            }
    
            /// <summary>
            /// Recall a customer order by sales id.
            /// </summary>
            /// <param name="request">The request containing the sales id.</param>
            /// <returns>The response containing the sales order.</returns>
            private static RecallCustomerOrderRealtimeResponse RecallCustomerOrder(RecallCustomerOrderRealtimeRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                var client = new TransactionServiceClient(request.RequestContext);
                ReadOnlyCollection<object> transactionResponse;
                if (request.IsQuote)
                {
                    transactionResponse = client.GetCustomerQuote(request.Id);
                }
                else
                {
                    transactionResponse = client.GetCustomerOrder(request.Id, includeOnlineOrders: true);
                }
    
                var orderInfo = CustomerOrderInfo.FromXml(transactionResponse[0].ToString());
                var order = SalesOrderHelper.GetSalesOrderFromInfo(orderInfo, request.RequestContext.GetChannelConfiguration(), request.RequestContext);
    
                // Check that the channel currency code is the same as the recalled order
                if (order != null && !string.IsNullOrWhiteSpace(order.CurrencyCode) && !request.RequestContext.GetChannelConfiguration().Currency.Equals(order.CurrencyCode, StringComparison.OrdinalIgnoreCase))
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CurrencyChannelOrderMismatch,
                        string.Format("Channel currency = {0} doesn't match sales order currency = {1}", request.RequestContext.GetChannelConfiguration().Currency, order.CurrencyCode));
                }
    
                var response = new RecallCustomerOrderRealtimeResponse(order);
    
                return response;
            }
    
            /// <summary>
            /// Processes a save customer order request.
            /// </summary>
            /// <param name="request">The save customer order request.</param>
            /// <returns>The customer order service default response.</returns>
            private static Response SaveCustomerOrder(SaveCustomerOrderRealtimeRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (request.SalesTransaction == null)
                {
                    throw new ArgumentException("SalesTransaction is not set in request", "request");
                }
    
                if (request.ChannelConfiguration == null)
                {
                    throw new ArgumentException("ChannelConfiguration is not set in request", "request");
                }
    
                SalesTransaction salesTransaction = request.SalesTransaction;
                if (salesTransaction.CustomerOrderMode == CustomerOrderMode.Cancellation)
                {
                    return CustomerOrderService.SaveCancellationOrder(request);
                }
    
                return SaveCustomerOrderInHeadquarter(request);
            }
    
            /// <summary>
            /// Cancels a customer order on the headquarters.
            /// </summary>
            /// <param name="request">The save customer order request.</param>
            /// <returns>The customer order service default response.</returns>
            private static Response SaveCancellationOrder(SaveCustomerOrderRealtimeRequest request)
            {
                Response response;
                SalesTransaction salesTransaction = request.SalesTransaction;
                ChannelConfiguration channelConfiguration = request.ChannelConfiguration;
    
                // Keep all charge line references
                List<ChargeLine> chargeLines = new List<ChargeLine>(salesTransaction.ChargeLines);
    
                if (!string.IsNullOrWhiteSpace(channelConfiguration.CancellationChargeCode))
                {
                    // Get all cancellation charges
                    var cancellationChargeLines = salesTransaction.ChargeLines
                        .Where(chargeLine => channelConfiguration.CancellationChargeCode.Equals(chargeLine.ChargeCode, StringComparison.OrdinalIgnoreCase))
                        .ToArray();
    
                    // Remove all non-cancellation charges from header - since AX will blindly use the charges to header as cancellation charges
                    salesTransaction.ChargeLines.Clear();
                    salesTransaction.ChargeLines.AddRange(cancellationChargeLines);
                }
    
                // Cancels order in headquarters
                response = CustomerOrderService.SaveCustomerOrderInHeadquarter(request);
    
                // Restore charge lines
                salesTransaction.ChargeLines.Clear();
                salesTransaction.ChargeLines.AddRange(chargeLines);
    
                return response;
            }
    
            /// <summary>
            /// Processes a save customer order request in headquarter.
            /// </summary>
            /// <param name="request">The save customer order request.</param>
            /// <returns>The customer order service default response.</returns>
            private static Response SaveCustomerOrderInHeadquarter(SaveCustomerOrderRealtimeRequest request)
            {
                string xmlRequest;
                SalesTransaction salesTransaction;
                ReadOnlyCollection<object> resultCollection;
    
                salesTransaction = request.SalesTransaction;
    
                // Converts transaction into xml request blob
                xmlRequest = SalesTransactionSerializationHelper.ConvertSalesTransactionToXml(
                    salesTransaction,
                    request.CardTokenInfo,
                    request.CardAuthorizationTokenResponseXml ?? string.Empty,
                    request.RequestContext);
    
                // Calls TS API
                resultCollection = new TransactionServiceClient(request.RequestContext).SaveCustomerOrder(
                    salesTransaction.SalesId,
                    salesTransaction.CustomerOrderType,
                    salesTransaction.CustomerOrderMode,
                    xmlRequest);
    
                switch (salesTransaction.CustomerOrderMode)
                {
                    case CustomerOrderMode.QuoteCreateOrEdit:
                    case CustomerOrderMode.CustomerOrderCreateOrEdit:
                    case CustomerOrderMode.Return:
                        // Sets Sales Id for the order on the transaction
                        salesTransaction.SalesId = (string)resultCollection[0];
                        salesTransaction.CustomerOrderType = salesTransaction.CustomerOrderMode == CustomerOrderMode.QuoteCreateOrEdit
                            ? CustomerOrderType.Quote
                            : CustomerOrderType.SalesOrder;
                        break;
    
                    // no return value for pick up and cancellation
                    case CustomerOrderMode.Cancellation:
                    case CustomerOrderMode.Pickup:
                        break;
    
                    default:
                        throw new NotSupportedException(salesTransaction.CustomerOrderMode + " type for customer order is not supported.");
                }
    
                return new NullResponse();
            }
        }
    }
}
