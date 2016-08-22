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
        public sealed class RecallCustomerOrderRequestHandler : SingleRequestHandler<RecallCustomerOrderRequest, RecallCustomerOrderResponse>
        {
            /// <summary>
            /// Executes the workflow to fetch the sales order and convert into a cart.
            /// </summary>
            /// <param name="request">Instance of <see cref="RecallCustomerOrderRequest"/>.</param>
            /// <returns>Instance of <see cref="RecallCustomerOrderResponse"/>.</returns>
            protected override RecallCustomerOrderResponse Process(RecallCustomerOrderRequest request)
            {
                ThrowIf.Null(request, "request");
                
                // Recall the customer order
                var realtimeRequest = new RecallCustomerOrderRealtimeRequest(
                    request.Id,
                    request.IsQuote);
    
                var serviceResponse = this.Context.Execute<RecallCustomerOrderRealtimeResponse>(realtimeRequest);
    
                if (serviceResponse.SalesOrder == null)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound, string.Format("The order/ quote: {0} is not found.", request.Id));
                }
    
                // update the transaction id in sales order if the request contains transaction Id.
                if (!string.IsNullOrWhiteSpace(request.TransactionId))
                {
                    serviceResponse.SalesOrder.Id = request.TransactionId;
                }
    
                Cart cart = CustomerOrderWorkflowHelper.SaveTransactionAndConvertToCart(this.Context, serviceResponse.SalesOrder);
                
                // Return cart
                return new RecallCustomerOrderResponse(cart);
            }
        }
    }
}
