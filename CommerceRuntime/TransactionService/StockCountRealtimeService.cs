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
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
    
        /// <summary>
        /// Stock count real time service.
        /// </summary>
        public class StockCountRealtimeService : IRequestHandler
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
                        typeof(GetStockCountJournalsRealtimeRequest),
                        typeof(GetStockCountJournalTransactionsRealtimeRequest),
                        typeof(CommitStockCountJournalRealtimeRequest),
                        typeof(CreateStockCountJournalRealtimeRequest),
                        typeof(DeleteStockCountJournalRealtimeRequest)
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
                if (requestType == typeof(CreateStockCountJournalRealtimeRequest))
                {
                    response = CreateStockCountJournal((CreateStockCountJournalRealtimeRequest)request);
                }
                else if (requestType == typeof(DeleteStockCountJournalRealtimeRequest))
                {
                    response = DeleteStockCountJournal((DeleteStockCountJournalRealtimeRequest)request);
                }
                else if (requestType == typeof(GetStockCountJournalsRealtimeRequest))
                {
                    response = GetStockCountJournals((GetStockCountJournalsRealtimeRequest)request);
                }
                else if (requestType == typeof(GetStockCountJournalTransactionsRealtimeRequest))
                {
                    response = GetStockCountJournalTransactions((GetStockCountJournalTransactionsRealtimeRequest)request);
                }
                else if (requestType == typeof(CommitStockCountJournalRealtimeRequest))
                {
                    response = CommitStockCountJournal((CommitStockCountJournalRealtimeRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Gets stock count journals in AX by location id.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The collection of <see cref="StockCountJournal"/> items.</returns>
            private static EntityDataServiceResponse<StockCountJournal> GetStockCountJournals(GetStockCountJournalsRealtimeRequest request)
            {
                var transactionClient = new TransactionService.TransactionServiceClient(request.RequestContext);
                PagedResult<StockCountJournal> stockCountJournals = transactionClient.GetStockCountJournals(request.LocationId);
    
                return new EntityDataServiceResponse<StockCountJournal>(stockCountJournals);
            }
    
            /// <summary>
            /// Commits stock count journals in AX.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The <see cref="StockCountJournal"/> journal.</returns>
            private static SingleEntityDataServiceResponse<StockCountJournal> CommitStockCountJournal(CommitStockCountJournalRealtimeRequest request)
            {
                var transactionClient = new TransactionService.TransactionServiceClient(request.RequestContext);
                StockCountJournal stockCountJournal = transactionClient.CommitStockCounts(request.Journal);
    
                return new SingleEntityDataServiceResponse<StockCountJournal>(stockCountJournal);
            }
    
            /// <summary>
            /// Deletes stock count journal in AX.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The <see cref="NullResponse"/> response.</returns>
            private static NullResponse DeleteStockCountJournal(DeleteStockCountJournalRealtimeRequest request)
            {
                var transactionClient = new TransactionService.TransactionServiceClient(request.RequestContext);
                transactionClient.DeleteStockJournal(request.JournalId);
    
                return new NullResponse();
            }
    
            /// <summary>
            /// Gets stock count journals with transactions from AX by location and journal ids.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The collection of <see cref="StockCountJournalTransaction"/> items.</returns>
            private static EntityDataServiceResponse<StockCountJournalTransaction> GetStockCountJournalTransactions(GetStockCountJournalTransactionsRealtimeRequest request)
            {
                var transactionClient = new TransactionService.TransactionServiceClient(request.RequestContext);
                PagedResult<StockCountJournalTransaction> stockCountJournals = transactionClient.GetStockCountJournalsTransaction(request.JournalId, request.LocationId);
    
                return new EntityDataServiceResponse<StockCountJournalTransaction>(stockCountJournals);
            }
    
            /// <summary>
            /// Creates stock count journal in AX by location id and description.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The collection of <see cref="StockCountJournal"/> items.</returns>
            private static EntityDataServiceResponse<StockCountJournal> CreateStockCountJournal(CreateStockCountJournalRealtimeRequest request)
            {
                var transactionClient = new TransactionService.TransactionServiceClient(request.RequestContext);
                PagedResult<StockCountJournal> stockCountJournals = transactionClient.CreateStockCountJournal(request.LocationId, request.Description);
    
                return new EntityDataServiceResponse<StockCountJournal>(stockCountJournals);
            }
        }
    }
}
