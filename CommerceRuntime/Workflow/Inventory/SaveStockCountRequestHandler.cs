/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

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
    namespace Commerce.Runtime.Workflow
    {
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Handler for save/ commit StockCount journal Transactions.
        /// </summary>
        public sealed class SaveStockCountRequestHandler : SingleRequestHandler<SaveStockCountRequest, SaveStockCountResponse>
        {
            /// <summary>
            /// Executes the workflow to save/ commit StockCount journal transactions.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override SaveStockCountResponse Process(SaveStockCountRequest request)
            {
                ThrowIf.Null(request, "request");
    
                Request serviceRequest;
                SaveStockCountResponse response;
    
                if (!request.IsCommitted)
                {
                    serviceRequest = new SaveStockCountJournalTransactionServiceRequest(request.JournalId, request.StockCountJournalTransactionList);
    
                    var serviceResponse = this.Context.Execute<SaveStockCountJournalTransactionServiceResponse>(serviceRequest);
                    response = new SaveStockCountResponse(serviceResponse.StockCountJournal);
                }
                else
                {
                    serviceRequest = new CommitStockCountTransactionsServiceRequest(request.JournalId, request.StockCountJournalTransactionList);
    
                    this.Context.Execute<CommitStockCountTransactionsServiceResponse>(serviceRequest);
                    response = new SaveStockCountResponse();
                }
                
                return response;
            }
        }
    }
}
