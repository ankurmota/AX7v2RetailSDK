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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
    
        /// <summary>
        /// Implementation for sales order demo mode transaction service.
        /// </summary>
        public class SalesOrderTransactionServiceDemoMode : IRequestHandler
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
                        typeof(PostLoyaltyCardRewardPointRealtimeRequest),
                        typeof(GetOrdersRealtimeRequest),
                        typeof(MarkReturnedItemsRealtimeRequest),
                        typeof(GetReturnLocationRealtimeRequest),
                        typeof(SearchJournalTransactionsRealtimeRequest)
                    };
                }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
    
                if (requestType == typeof(PostLoyaltyCardRewardPointRealtimeRequest))
                {
                    response = PostLoyaltyCardRewardPoint((PostLoyaltyCardRewardPointRealtimeRequest)request);
                }
                else if (requestType == typeof(GetOrdersRealtimeRequest))
                {
                    response = SearchOrders((GetOrdersRealtimeRequest)request);
                }
                else if (requestType == typeof(MarkReturnedItemsRealtimeRequest))
                {
                    response = MarkItemsReturned((MarkReturnedItemsRealtimeRequest)request);
                }
                else if (requestType == typeof(GetReturnLocationRealtimeRequest))
                {
                    response = GetReturnLocaion();
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
    
            private static NullResponse PostLoyaltyCardRewardPoint(PostLoyaltyCardRewardPointRealtimeRequest postLoyaltyCardRewardPointService)
            {
                ThrowIf.Null(postLoyaltyCardRewardPointService, "postLoyaltyCardRewardPointService");
    
                return new NullResponse();
            }
    
            private static GetOrdersRealtimeResponse SearchOrders(GetOrdersRealtimeRequest request)
            {
                ThrowIf.Null(request, "request");
    
                var remoteOrders = new List<SalesOrder>();
    
                return new GetOrdersRealtimeResponse(remoteOrders.AsPagedResult());
            }
    
            private static NullResponse MarkItemsReturned(MarkReturnedItemsRealtimeRequest request)
            {
                ThrowIf.Null(request, "serviceRequest");
    
                return new NullResponse();
            }
    
            /// <summary>
            /// Get return location parameters.
            /// </summary>
            /// <returns>The transaction service response.</returns>
            private static GetReturnLocationRealtimeResponse GetReturnLocaion()
            {
                bool printReturnLabel = false;
                string returnWarehouseText = string.Empty;
                string returnLocationText = string.Empty;
                string returnPalleteText = string.Empty;
    
                return new GetReturnLocationRealtimeResponse(printReturnLabel, returnWarehouseText, returnLocationText, returnPalleteText);
            }
    
            private SearchJournalTransactionsRealtimeResponse SearchJournalTransactions(SearchJournalTransactionsRealtimeRequest request)
            {
                ThrowIf.Null(request, "request");
    
                var transactions = new List<Transaction>();
                return new SearchJournalTransactionsRealtimeResponse(transactions.AsPagedResult());
            }
        }
    }
}
