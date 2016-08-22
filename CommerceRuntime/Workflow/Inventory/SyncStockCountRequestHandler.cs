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
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Handler for syncing StockCount journals and associated Transactions from AX to RetailServer database.
        /// </summary>
        public sealed class SyncStockCountRequestHandler : SingleRequestHandler<SyncStockCountRequest, SyncStockCountResponse>
        {
            /// <summary>
            /// Executes the workflow to sync StockCount journal / transactions from AX to RetailServer database.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override SyncStockCountResponse Process(SyncStockCountRequest request)
            {
                ThrowIf.Null(request, "request");
    
                Request serviceRequest;
                SyncStockCountResponse response;
    
                if (string.IsNullOrEmpty(request.JournalId))
                {
                    serviceRequest = new SyncStockCountJournalsFromAxServiceRequest();
    
                    var serviceResponse = this.Context.Execute<SyncStockCountJournalsFromAxServiceResponse>(serviceRequest);
                    response = new SyncStockCountResponse(serviceResponse.StockCountJournals);
                }
                else
                {
                    serviceRequest = new SyncStockCountTransactionsFromAxServiceRequest()
                    {
                        JournalId = request.JournalId
                    };
    
                    var serviceResponse = this.Context.Execute<SyncStockCountTransactionsFromAxServiceResponse>(serviceRequest);
                    response = new SyncStockCountResponse(serviceResponse.StockCountJournalTransactions);
                }
    
                return response;
            }
        }
    }
}
