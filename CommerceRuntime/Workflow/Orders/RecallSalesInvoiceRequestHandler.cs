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
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
    
        /// <summary>
        /// Handles workflow for recall customer order into a cart.
        /// </summary>
        public sealed class RecallSalesInvoiceRequestHandler : SingleRequestHandler<RecallSalesInvoiceRequest, RecallSalesInvoiceResponse>
        {
            /// <summary>
            /// Executes the workflow to fetch the invoice and convert to a cart.
            /// </summary>
            /// <param name="request">Instance of <see cref="RecallSalesInvoiceRequest"/>.</param>
            /// <returns>Instance of <see cref="RecallSalesInvoiceResponse"/>.</returns>
            protected override RecallSalesInvoiceResponse Process(RecallSalesInvoiceRequest request)
            {
                ThrowIf.Null(request, "request");

                // Get the invoices
                var realtimeRequest = new GetInvoiceRealtimeRequest(
                    string.Empty,
                    request.InvoiceId);
    
                GetInvoiceRealtimeResponse realtimeResponse = this.Context.Execute<GetInvoiceRealtimeResponse>(realtimeRequest);
    
                if (realtimeResponse.Order == null)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound, string.Format("The sales invoice: {0} is not found.", request.InvoiceId));
                }
    
                SalesTransaction transaction = realtimeResponse.Order;
    
                // Update warehouse since the order is being returned in this store
                transaction.InventoryLocationId = this.Context.GetOrgUnit().InventoryLocationId;
                transaction.StoreId = this.Context.GetOrgUnit().OrgUnitNumber;
                
                foreach (SalesLine salesLine in transaction.ActiveSalesLines)
                {
                    salesLine.InventoryLocationId = transaction.InventoryLocationId;
                    salesLine.FulfillmentStoreId = transaction.StoreId;
                }
    
                // update the transaction id in sales order if the request contains transaction Id.
                if (!string.IsNullOrWhiteSpace(request.TransactionId))
                {
                    realtimeResponse.Order.Id = request.TransactionId;
                }
    
                Cart cart = CustomerOrderWorkflowHelper.SaveTransactionAndConvertToCart(this.Context, realtimeResponse.Order);
                
                return new RecallSalesInvoiceResponse(cart);
            }
        }
    }
}
