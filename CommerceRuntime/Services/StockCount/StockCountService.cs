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
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Represents an implementation of the stock count service.
        /// </summary>
        public class StockCountService : IRequestHandler
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
                        typeof(CreateStockCountJournalServiceRequest),
                        typeof(SaveStockCountJournalTransactionServiceRequest),
                        typeof(GetStockCountJournalServiceRequest),
                        typeof(GetStockCountJournalTransactionServiceRequest),
                        typeof(SyncStockCountJournalsFromAxServiceRequest),
                        typeof(SyncStockCountTransactionsFromAxServiceRequest),
                        typeof(CommitStockCountTransactionsServiceRequest),
                        typeof(DeleteStockCountJournalServiceRequest),
                        typeof(DeleteStockCountTransactionServiceRequest)
                    };
                }
            }
    
            /// <summary>
            /// Executes the specified service request.
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
    
                if (requestType == typeof(CreateStockCountJournalServiceRequest))
                {
                    response = CreateStockCountJournal((CreateStockCountJournalServiceRequest)request);
                }
                else if (requestType == typeof(DeleteStockCountJournalServiceRequest))
                {
                    response = DeleteStockCountJournal((DeleteStockCountJournalServiceRequest)request);
                }
                else if (requestType == typeof(DeleteStockCountTransactionServiceRequest))
                {
                    response = DeleteStockCountJournalTransaction((DeleteStockCountTransactionServiceRequest)request);
                }
                else if (requestType == typeof(SaveStockCountJournalTransactionServiceRequest))
                {
                    response = SaveStockCountTransactions((SaveStockCountJournalTransactionServiceRequest)request);
                }
                else if (requestType == typeof(GetStockCountJournalServiceRequest))
                {
                    response = GetStockCountJournal((GetStockCountJournalServiceRequest)request);
                }
                else if (requestType == typeof(SyncStockCountJournalsFromAxServiceRequest))
                {
                    response = SyncStockCountJournalsFromAx((SyncStockCountJournalsFromAxServiceRequest)request);
                }
                else if (requestType == typeof(SyncStockCountTransactionsFromAxServiceRequest))
                {
                    response = SyncStockCountTransactionsFromAx((SyncStockCountTransactionsFromAxServiceRequest)request);
                }
                else if (requestType == typeof(CommitStockCountTransactionsServiceRequest))
                {
                    response = CommitStockCountTransactions((CommitStockCountTransactionsServiceRequest)request);
                }
                else if (requestType == typeof(GetStockCountJournalTransactionServiceRequest))
                {
                    response = GetStockCountJournalTransactions((GetStockCountJournalTransactionServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Invokes the method in StockCount that executes SaveStockCountTransactions.
            /// </summary>
            /// <param name="request">SaveStockCountJournalTransactionRequest request.</param>
            /// <returns>Returns the SaveStockCountJournalTransactionResponse that contains the result.</returns>
            private static SaveStockCountJournalTransactionServiceResponse SaveStockCountTransactions(SaveStockCountJournalTransactionServiceRequest request)
            {
                return StockCountServiceHelper.SaveStockCountJournalTransactions(request.JournalId, request.RequestContext, request.StockCountJournalTransactions);
            }
    
            /// <summary>
            /// Invokes the method in StockCount that executes CommitStockCountTransactions.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private static CommitStockCountTransactionsServiceResponse CommitStockCountTransactions(CommitStockCountTransactionsServiceRequest request)
            {
                return StockCountServiceHelper.CommitStockCountTransactions(request.RequestContext, request.JournalId, request.StockCountJournalTransactions);
            }
    
            /// <summary>
            /// Invokes the method in StockCount that executes SyncStockCountJournalsFromAx.
            /// </summary>
            /// <param name="request">SyncStockCountJournalsFromAxServiceRequest request.</param>
            /// <returns>Returns SyncStockCountJournalsFromAxServiceResponse.</returns>
            private static SyncStockCountJournalsFromAxServiceResponse SyncStockCountJournalsFromAx(SyncStockCountJournalsFromAxServiceRequest request)
            {
                return StockCountServiceHelper.SyncStockCountJournalFromAx(request.RequestContext);
            }
    
            /// <summary>
            /// Invokes the method in StockCount that executes SyncStockCountTransactionsFromAx.
            /// </summary>
            /// <param name="request">SyncStockCountTransactionsFromAxServiceRequest request.</param>
            /// <returns>Returns SyncStockCountTransactionsFromAxServiceResponse.</returns>
            private static SyncStockCountTransactionsFromAxServiceResponse SyncStockCountTransactionsFromAx(SyncStockCountTransactionsFromAxServiceRequest request)
            {
                return StockCountServiceHelper.SyncStockCountJournalTransactions(request.RequestContext, request.JournalId);
            }
    
            /// <summary>
            /// Invokes the method in StockCount that executes CreateStockCountJournal.
            /// </summary>
            /// <param name="request">CreateStockCountJournalServiceRequest request.</param>
            /// <returns>Returns CreateStockCountJournalServiceResponse.</returns>
            private static CreateStockCountJournalServiceResponse CreateStockCountJournal(CreateStockCountJournalServiceRequest request)
            {
                return StockCountServiceHelper.CreateStockCountJournal(request.RequestContext, request.Description);
            }
    
            /// <summary>
            /// Invokes the method in StockCount service that executes DeleteStockCountJournal.
            /// </summary>
            /// <param name="request">DeleteStockCountJournalServiceRequest request.</param>
            /// <returns>Returns DeleteStockCountServiceResponse.</returns>
            private static DeleteStockCountServiceResponse DeleteStockCountJournal(DeleteStockCountJournalServiceRequest request)
            {
                return StockCountServiceHelper.DeleteStockCountJournal(request.RequestContext, request.JournalId);
            }
    
            /// <summary>
            /// Invokes the method in StockCount service that executes DeleteStockCountJournalTransaction.
            /// </summary>
            /// <param name="request">DeleteStockCountTransactionServiceRequest request.</param>
            /// <returns>Returns DeleteStockCountServiceResponse.</returns>
            private static DeleteStockCountServiceResponse DeleteStockCountJournalTransaction(DeleteStockCountTransactionServiceRequest request)
            {
                return StockCountServiceHelper.DeleteStockCountJournalTransaction(request.RequestContext, request.JournalId, request.ItemId, request.InventSizeId, request.InventColorId, request.InventStyleId, request.ConfigId);
            }
    
            /// <summary>
            /// Invokes the method in StockCount that executes GetStockCountJournal.
            /// </summary>
            /// <param name="request">GetStockCountJournalServiceRequest request.</param>
            /// <returns>Returns GetStockCountJournalServiceResponse.</returns>
            private static GetStockCountJournalServiceResponse GetStockCountJournal(GetStockCountJournalServiceRequest request)
            {
                return StockCountServiceHelper.GetStockCountJournal(request);
            }
    
            /// <summary>
            /// Invokes the method in StockCount that executes GetStockCountJournalTransactions.
            /// </summary>
            /// <param name="request">GetStockCountJournalTransactionServiceRequest request.</param>
            /// <returns>Returns GetStockCountJournalTransactionServiceResponse.</returns>
            private static GetStockCountJournalTransactionServiceResponse GetStockCountJournalTransactions(GetStockCountJournalTransactionServiceRequest request)
            {
                return StockCountServiceHelper.GetStockCountJournalTransactions(request.RequestContext, request.JournalId);
            }
        }
    }
}
