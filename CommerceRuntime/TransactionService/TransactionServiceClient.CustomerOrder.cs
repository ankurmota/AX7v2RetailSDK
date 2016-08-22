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
    namespace Commerce.Runtime.TransactionService
    {
        using System;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Commerce.Runtime.TransactionService.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Transaction Service Commerce Runtime Client API.
        /// </summary>
        public sealed partial class TransactionServiceClient
        {
            // Transaction service method names.
            private const string GetCustomerQuoteMethodName = "GetCustomerQuote";
            private const string GetCustomerOrderMethodName = "GetCustomerOrder";
            private const string CreateCustomerQuoteMethodName = "CreateCustomerQuote";
            private const string UpdateCustomerQuoteMethodName = "UpdateCustomerQuote";
            private const string CreateCustomerReturnOrderMethodName = "CreateCustomerReturnOrder";
            private const string CreateCustomerOrderMethodName = "CreateCustomerOrder";
            private const string UpdateCustomerOrderMethodName = "UpdateCustomerOrder";
            private const string ConvertCustomerQuoteToOrderMethodName = "ConvertCustomerQuoteToOrder";
            private const string SettleCustomerOrderMethodName = "SettleCustomerOrder";
            private const string CancelCustomerOrderMethodName = "CancelCustomerOrder";
            private const string CreatePickingListMethodName = "CreatePickingList";
            private const string CreatePackingSlipMethodName = "CreatePackingSlip";
            private const string GetSalesInvoicesBySalesIdMethodName = "GetSalesInvoicesBySalesId";
            private const string GetSalesInvoiceDetailMethodName = "GetSalesInvoiceDetail";
            private const string GetSalesInvoiceListMethodName = "GetSalesInvoiceList";
            private const string PaySalesInvoiceMethodName = "PaySalesInvoice";
            private const string GetReturnLocationByInfocode = "GetReturnLocationByInfocode";
            private const string GetReturnLocationByReasoncode = "GetReturnLocationByReasonCode";
    
            /// <summary>
            /// Creates picking list.
            /// </summary>
            /// <param name="salesId">The sales order identifier.</param>
            /// <param name="inventoryLocationId">The inventory location identifier.</param>
            public void CreatePickingList(string salesId, string inventoryLocationId)
            {
                this.InvokeMethodNoDataReturn(CreatePickingListMethodName, salesId, inventoryLocationId);
            }
    
            /// <summary>
            /// Creates packing slip.
            /// </summary>
            /// <param name="salesId">The sales order identifier.</param>
            /// <param name="inventoryLocationId">The warehouse id (inventory location id) where the packing slip is being created.</param>
            public void CreatePackingSlip(string salesId, string inventoryLocationId)
            {
                this.InvokeMethodNoDataReturn(CreatePackingSlipMethodName, salesId, inventoryLocationId);
            }
    
            /// <summary>
            /// Gets the sales invoice collections.
            /// </summary>
            /// <param name="salesId">The sales order identifier.</param>
            /// <param name="invoiceId">The invoice identifier.</param>
            /// <returns>The collection results from the transaction service call.</returns>
            public ReadOnlyCollection<object> GetSalesInvoices(string salesId, string invoiceId)
            {
                if (!string.IsNullOrWhiteSpace(salesId))
                {
                    // Find invoices by sales id
                    return this.InvokeMethod(GetSalesInvoicesBySalesIdMethodName, salesId);
                }
                else
                {
                    // Find invoices by invoice id
                    return this.InvokeMethod(GetSalesInvoiceDetailMethodName, invoiceId);
                }
            }
    
            /// <summary>
            /// Gets the sales invoice collection for a given customer.
            /// </summary>
            /// <param name="customerAccountNumber">The customer account number.</param>
            /// <returns>The collection results from the transaction service call.</returns>
            public ReadOnlyCollection<object> GetSalesInvoices(string customerAccountNumber)
            {
                return this.InvokeMethod(GetSalesInvoiceListMethodName, customerAccountNumber);
            }
    
            /// <summary>
            /// Gets the return location by info code.
            /// </summary>
            /// <param name="transaction">The sales transaction.</param>
            /// <param name="line">The sales line.</param>
            /// <returns>The collection results from the transaction service call.</returns>
            public ReadOnlyCollection<object> GetReturnLocationByInfoCode(SalesTransaction transaction, SalesLine line)
            {
                ThrowIf.Null<SalesTransaction>(transaction, "transaction");
                ThrowIf.Null<SalesLine>(line, "line");
    
                var returnLocationPrintParameter = new ReturnLocationPrintParameter
                {
                    StoreId = transaction.StoreId,
                    ItemId = line.ItemId
                };
    
                if (!line.ReasonCodeLines.IsNullOrEmpty())
                {
                    // We use last record to print labels.
                    var reasonCodeNode = line.ReasonCodeLines.Last();
                    var infoCodeLine = reasonCodeNode;
    
                    returnLocationPrintParameter.Codes.InfocodeId = infoCodeLine.ReasonCodeId;
                    returnLocationPrintParameter.Codes.SubcodeId = infoCodeLine.SubReasonCodeId;
                }
    
                return this.InvokeMethod(GetReturnLocationByInfocode, returnLocationPrintParameter.ToXml());
            }
    
            /// <summary>
            /// Gets the return location by reason code.
            /// </summary>
            /// <param name="transaction">The transaction.</param>
            /// <param name="line">The sales line.</param>
            /// <param name="reasonCodeId">The reason code identifier.</param>
            /// <returns>The collection results from the transaction service call.</returns>
            public ReadOnlyCollection<object> GetReturnLocationByReasonCode(SalesTransaction transaction, SalesLine line, string reasonCodeId)
            {
                ThrowIf.Null<SalesTransaction>(transaction, "transaction");
                ThrowIf.Null<SalesLine>(line, "line");
    
                return this.InvokeMethod(GetReturnLocationByReasoncode, transaction.StoreId, line.ItemId, reasonCodeId);
            }
    
            /// <summary>
            /// Settle payment against an existing open sales invoice.
            /// </summary>
            /// <param name="invoiceId">Id of the invoice to settle payment against.</param>
            /// <param name="paymentAmount">Amount of payment to settle, in the company currency.</param>
            /// <param name="terminalId">Id of the terminal where the payment occurred.</param>
            /// <param name="storeId">Id of the store where the payment occurred.</param>
            /// <param name="transactionId">Id of the transaction the payment was collected in.</param>
            /// <returns>The collection results from the transaction service call.</returns>
            public ReadOnlyCollection<object> PaySalesInvoice(string invoiceId, decimal paymentAmount, string terminalId, string storeId, string transactionId)
            {
                return this.InvokeMethod(PaySalesInvoiceMethodName, invoiceId, paymentAmount, terminalId, storeId, transactionId);
            }
    
            /// <summary>
            /// Gets the customer quote by identifier.
            /// </summary>
            /// <param name="quoteId">The quote identifier.</param>
            /// <returns>The collection results from the transaction service call.</returns>
            public ReadOnlyCollection<object> GetCustomerQuote(string quoteId)
            {
                return this.InvokeMethod(GetCustomerQuoteMethodName, quoteId);
            }
    
            /// <summary>
            /// Gets the customer order by identifier.
            /// </summary>
            /// <param name="salesId">The sales order identifier.</param>
            /// <param name="includeOnlineOrders">Whether to include online orders.</param>
            /// <returns>The collection results from the transaction service call.</returns>
            public ReadOnlyCollection<object> GetCustomerOrder(string salesId, bool includeOnlineOrders)
            {
                return this.InvokeMethod(GetCustomerOrderMethodName, salesId, includeOnlineOrders);
            }
    
            /// <summary>
            /// Saves customer order into AX.
            /// </summary>
            /// <param name="salesId">The sales order identifier.</param>
            /// <param name="type">The order type.</param>
            /// <param name="mode">The order mode.</param>
            /// <param name="requestXml">The converted request xml.</param>
            /// <returns>The collection results from the transaction service call.</returns>
            public ReadOnlyCollection<object> SaveCustomerOrder(string salesId, CustomerOrderType type, CustomerOrderMode mode, string requestXml)
            {
                bool isUpdate = !string.IsNullOrWhiteSpace(salesId);
    
                string methodName;
    
                // Decides which TS API to call based on customer order mode
                switch (mode)
                {
                    case CustomerOrderMode.QuoteCreateOrEdit:
                        methodName = isUpdate ? UpdateCustomerQuoteMethodName : CreateCustomerQuoteMethodName;
                        return this.InvokeMethod(methodName, requestXml);
    
                    case CustomerOrderMode.Return:
                        methodName = CreateCustomerReturnOrderMethodName;
                        return this.InvokeMethod(methodName, requestXml);
    
                    case CustomerOrderMode.CustomerOrderCreateOrEdit:
                        methodName = type == CustomerOrderType.Quote
                                         ? ConvertCustomerQuoteToOrderMethodName // If order type is quote, we are converting quote to customer order
                                         : (isUpdate ? UpdateCustomerOrderMethodName : CreateCustomerOrderMethodName);
                        return this.InvokeMethod(methodName, requestXml);
    
                    case CustomerOrderMode.Pickup:
                        this.InvokeMethodNoDataReturn(SettleCustomerOrderMethodName, requestXml);
                        return null;
    
                    case CustomerOrderMode.Cancellation:
                        this.InvokeMethodNoDataReturn(CancelCustomerOrderMethodName, requestXml);
                        return null;
    
                    default:
                        string invalidOperationMessage = string.Format(
                            "TransactionServiceClient::SaveCustomerOrder: unsupported salesTransaction.CustomerOrderMode: {0}", mode);
                        throw new InvalidOperationException(invalidOperationMessage);
                }
            }
        }
    }
}
