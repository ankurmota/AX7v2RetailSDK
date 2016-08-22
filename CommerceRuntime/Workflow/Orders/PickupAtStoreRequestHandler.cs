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
    
        /// <summary>
        /// Pick up at store request handler.
        /// </summary>
        public sealed class PickupAtStoreRequestHandler : SingleRequestHandler<PickupAtStoreRequest, PickupAtStoreResponse>
        {
            /// <summary>
            /// Process the request.
            /// </summary>
            /// <param name="request">The request object.</param>
            /// <returns>The response object.</returns>
            protected override PickupAtStoreResponse Process(PickupAtStoreRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.CartId, "request.CartId");
    
                // Get the Sales Transaction
                SalesTransaction transaction = CustomerOrderWorkflowHelper.GetSalesTransaction(this.Context, request.CartId, request.ReceiptEmailAddress);
    
                // Validate lines
                CustomerOrderWorkflowHelper.ValidateOrderForPickup(this.Context, transaction);
    
                // Fill in receipt id. Receipt id will be needed if paying with credit memo.
                transaction = CustomerOrderWorkflowHelper.FillInReceiptId(this.Context, transaction, request.ReceiptNumberSequence);
    
                // Handle payments
                transaction = CustomerOrderWorkflowHelper.HandlePayments(this.Context, transaction);
    
                // Saves the order (this will invoice the items picked up in AX)
                SalesOrder order = CustomerOrderWorkflowHelper.SaveCustomerOrder(this.Context, transaction);
    
                CartWorkflowHelper.TryDeleteCart(this.Context, transaction);
    
                return new PickupAtStoreResponse(order);
            }
        }
    }
}
