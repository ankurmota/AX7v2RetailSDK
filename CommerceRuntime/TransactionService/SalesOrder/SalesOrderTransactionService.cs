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
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Globalization;
        using System.Linq;
        using Commerce.Runtime.TransactionService.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Implementation for sales order realtime service.
        /// </summary>
        public class SalesOrderTransactionService : IRequestHandler
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
                        typeof(GetOrdersRealtimeRequest),
                        typeof(MarkReturnedItemsRealtimeRequest),
                        typeof(GetReturnLocationRealtimeRequest),
                        typeof(SearchJournalTransactionsRealtimeRequest)
                    };
                }
            }
    
            /// <summary>
            /// Execute sales order transaction service requests.
            /// </summary>
            /// <param name="request">The serviceRequest.</param>
            /// <returns>The response.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
    
                if (requestType == typeof(GetOrdersRealtimeRequest))
                {
                    response = this.SearchOrders((GetOrdersRealtimeRequest)request);
                }
                else if (requestType == typeof(MarkReturnedItemsRealtimeRequest))
                {
                    response = this.MarkItemsReturned((MarkReturnedItemsRealtimeRequest)request);
                }
                else if (requestType == typeof(GetReturnLocationRealtimeRequest))
                {
                    response = this.GetReturnLocaion((GetReturnLocationRealtimeRequest)request);
                }
                else if (requestType == typeof(SearchJournalTransactionsRealtimeRequest))
                {
                    response = this.SearchJournalTransactions((SearchJournalTransactionsRealtimeRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request type '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Get return location parameters.
            /// </summary>
            /// <param name="request">The transaction service request.</param>
            /// <returns>The transaction service response.</returns>
            private GetReturnLocationRealtimeResponse GetReturnLocaion(GetReturnLocationRealtimeRequest request)
            {
                ReadOnlyCollection<object> transactionServiceResponse = null;
                var transactionService = new TransactionServiceClient(request.RequestContext);
    
                if (request.IsInfoCode)
                {
                    transactionServiceResponse = transactionService.GetReturnLocationByInfoCode(request.SalesTransaction, request.Line);
                }
                else
                {
                    transactionServiceResponse = transactionService.GetReturnLocationByReasonCode(request.SalesTransaction, request.Line, request.ReasonCodeId);
                }
    
                return new GetReturnLocationRealtimeResponse((bool)transactionServiceResponse[0], (string)transactionServiceResponse[1], (string)transactionServiceResponse[2], (string)transactionServiceResponse[3]);
            }
    
            private GetOrdersRealtimeResponse SearchOrders(GetOrdersRealtimeRequest request)
            {
                ThrowIf.Null(request, "serviceRequest");
    
                var transactionServiceClient = new TransactionServiceClient(request.RequestContext);
                int maxTransactionSearchResults = request.QueryResultSettings.Paging.Top != 0
                                                      ? (int)request.QueryResultSettings.Paging.NumberOfRecordsToFetch
                                                      : 250;
    
                var remoteOrders = transactionServiceClient.SearchOrders(request.Criteria, maxTransactionSearchResults);
    
                return new GetOrdersRealtimeResponse(remoteOrders);
            }
    
            private SearchJournalTransactionsRealtimeResponse SearchJournalTransactions(SearchJournalTransactionsRealtimeRequest request)
            {
                ThrowIf.Null(request, "serviceRequest");
    
                var transactionServiceClient = new TransactionServiceClient(request.RequestContext);
                var transactions = transactionServiceClient.SearchJournalTransactions(request.RequestContext.GetPrincipal().ChannelId, request.Criteria, request.QueryResultSettings);
    
                return new SearchJournalTransactionsRealtimeResponse(transactions.AsPagedResult());
            }
    
            private NullResponse MarkItemsReturned(MarkReturnedItemsRealtimeRequest request)
            {
                ThrowIf.Null(request, "request");
    
                // Try convert sales line to returned items
                // Consider active lines only. Ignore voided lines.
                List<ItemReturn> itemReturns = new List<ItemReturn>();
                foreach (var salesLine in request.SalesTransaction.ActiveSalesLines)
                {
                    if (salesLine.IsReturnByReceipt)
                    {
                        // Might be empty after customer order return
                        if (string.IsNullOrWhiteSpace(salesLine.ReturnStore))
                        {
                            salesLine.ReturnStore = request.SalesTransaction.StoreId;
                        }
    
                        // Might be empty after customer order return
                        if (string.IsNullOrWhiteSpace(salesLine.ReturnTerminalId))
                        {
                            salesLine.ReturnTerminalId = request.SalesTransaction.TerminalId;
                        }
    
                        // we don't store the return channel identifier for the lines, just for the transaction
                        if (salesLine.ReturnChannelId == 0)
                        {
                            salesLine.ReturnChannelId = request.SalesTransaction.ChannelId;
                        }
    
                        var itemReturn = new ItemReturn();
                        itemReturn.ChannelId = salesLine.ReturnChannelId;
                        itemReturn.StoreId = salesLine.ReturnStore;
                        itemReturn.TerminalId = salesLine.ReturnTerminalId;
                        itemReturn.TransactionId = salesLine.ReturnTransactionId;
                        itemReturn.LineNumber = salesLine.ReturnLineNumber;
                        itemReturn.Quantity = salesLine.Quantity;
    
                        itemReturns.Add(itemReturn);
                    }
                }
    
                // Mark return quantities if needed
                if (itemReturns.Any())
                {
                    // Call the transaction service to update return quantities
                    var transactionService = new TransactionServiceClient(request.RequestContext);
                    transactionService.MarkItemsReturned(itemReturns.ToArray());
                }
    
                return new NullResponse();
            }
        }
    }
}
