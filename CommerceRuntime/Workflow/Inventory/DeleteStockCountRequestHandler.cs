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
        using Microsoft.Dynamics.Commerce.Runtime.Services;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Handler for deleting StockCount journals and associated Transactions from RetailServer database.
        /// </summary>
        public sealed class DeleteStockCountRequestHandler : SingleRequestHandler<DeleteStockCountRequest, NullResponse>
        {
            /// <summary>
            /// Executes the workflow to delete StockCount journal / transactions from RetailServer database.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override NullResponse Process(DeleteStockCountRequest request)
            {
                ThrowIf.Null(request, "request");
    
                Request serviceRequest;
                NullResponse response;
    
                if (request.IsCascadeJournalDelete)
                {
                    // This call deletes the journal and transactions for the given journal identifier.
                    serviceRequest = new DeleteStockCountJournalServiceRequest()
                    {
                        JournalId = request.JournalId
                    };
    
                    this.Context.Execute<DeleteStockCountServiceResponse>(serviceRequest);
                    response = new NullResponse();
                }
                else
                {
                    // This call deletes the particular transaction of the given journal identifier and item identifier.
                    serviceRequest = new DeleteStockCountTransactionServiceRequest()
                    {
                        JournalId = request.JournalId,
                        ItemId = request.ItemId,
                        InventSizeId = request.InventSizeId,
                        InventColorId = request.InventColorId,
                        InventStyleId = request.InventStyleId,
                        ConfigId = request.ConfigId
                    };
    
                    this.Context.Execute<DeleteStockCountServiceResponse>(serviceRequest);
                    response = new NullResponse();
                }
    
                return response;
            }
        }
    }
}
