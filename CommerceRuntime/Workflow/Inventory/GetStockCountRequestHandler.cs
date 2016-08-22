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
        /// Handler for retrieving StockCount journals and associated Transactions from RetailServer database.
        /// </summary>
        public sealed class GetStockCountRequestHandler : SingleRequestHandler<GetStockCountRequest, GetStockCountResponse>
        {
            /// <summary>
            /// Executes the workflow to retrieve StockCount journal / transactions RetailServer database.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override GetStockCountResponse Process(GetStockCountRequest request)
            {
                ThrowIf.Null(request, "request");
    
                Request serviceRequest;
                GetStockCountResponse response;
    
                if (!request.IncludeOnlyTransactions)
                {
                    serviceRequest = new GetStockCountJournalServiceRequest()
                    {
                        JournalId = request.JournalId,
                        QueryResultSettings = request.QueryResultSettings
                    };
    
                    var serviceResponse = this.Context.Execute<GetStockCountJournalServiceResponse>(serviceRequest);
                    response = new GetStockCountResponse(serviceResponse.StockCountJournals);
                }
                else
                {
                    serviceRequest = new GetStockCountJournalTransactionServiceRequest()
                    {
                        JournalId = request.JournalId
                    };
    
                    var serviceResponse = this.Context.Execute<GetStockCountJournalTransactionServiceResponse>(serviceRequest);
                    response = new GetStockCountResponse(serviceResponse.StockCountJournalTransactions);
                }
    
                return response;
            }
        }
    }
}
