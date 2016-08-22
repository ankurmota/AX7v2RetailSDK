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
    namespace Commerce.Runtime.DataServices.SqlServer
    {
        using System;
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        ///  Stock count SQL server data service class.
        /// </summary>
        public class StockCountSqlServerDataService : IRequestHandler
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
                        typeof(DeleteStockCountJournalsDataRequest),
                        typeof(DeleteStockCountTransactionDataRequest),
                        typeof(CreateUpdateStockCountJournalDataRequest),
                        typeof(CreateUpdateStockCountJournalTransactionDataRequest),
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
    
                if (requestType == typeof(DeleteStockCountJournalsDataRequest))
                {
                    response = this.DeleteStockCountJournals((DeleteStockCountJournalsDataRequest)request);
                }
                else if (requestType == typeof(DeleteStockCountTransactionDataRequest))
                {
                    response = this.DeleteStockCountTransaction((DeleteStockCountTransactionDataRequest)request);
                }
                else if (requestType == typeof(CreateUpdateStockCountJournalDataRequest))
                {
                    response = this.CreateUpdateStockCountJournals((CreateUpdateStockCountJournalDataRequest)request);
                }
                else if (requestType == typeof(CreateUpdateStockCountJournalTransactionDataRequest))
                {
                    response = this.CreateUpdateStockCountJournalTransaction((CreateUpdateStockCountJournalTransactionDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Get item availabilities by requested item quantities.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns><see cref="NullResponse"/> object.</returns>
            private NullResponse DeleteStockCountJournals(DeleteStockCountJournalsDataRequest request)
            {
                var dataManager = new StockCountDataManager(request.RequestContext);
                dataManager.DeleteStockCountJournals(request.JournalIds);
    
                return new NullResponse();
            }
    
            /// <summary>
            /// Deletes the Stock Count Transactions for the given JournalId and item identifier.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The <see cref="NullResponse"/> object.</returns>
            private NullResponse DeleteStockCountTransaction(DeleteStockCountTransactionDataRequest request)
            {
                var dataManager = new StockCountDataManager(request.RequestContext);
                dataManager.DeleteStockJournalTransactionByItemId(
                    request.JournalId,
                    request.ItemId,
                    request.InventSizeId,
                    request.InventColorId,
                    request.InventStyleId,
                    request.ConfigId);
    
                return new NullResponse();
            }
    
            /// <summary>
            ///  Insert/Update one (or) more of stock count journals.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The <see cref="NullResponse"/> object.</returns>
            private NullResponse CreateUpdateStockCountJournals(CreateUpdateStockCountJournalDataRequest request)
            {
                var dataManager = new StockCountDataManager(request.RequestContext);
                dataManager.CreateUpdateStockCountJournals(request.Journals);
    
                return new NullResponse();
            }
    
            /// <summary>
            ///  Insert/Update one or more stock count journal transactions.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The <see cref="NullResponse"/> object.</returns>
            private NullResponse CreateUpdateStockCountJournalTransaction(CreateUpdateStockCountJournalTransactionDataRequest request)
            {
                var dataManager = new StockCountDataManager(request.RequestContext);
                dataManager.CreateUpdateStockCountTransactions(request.JournalTransactions);
    
                return new NullResponse();
            }
        }
    }
}
