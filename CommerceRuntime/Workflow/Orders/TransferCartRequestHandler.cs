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
    
        /// <summary>
        /// Saves a shopping cart.
        /// </summary>
        public sealed class TransferCartRequestHandler : SingleRequestHandler<TransferCartRequest, NullResponse>
        {
            /// <summary>
            /// Transfer the shopping cart on the request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns><see cref="NullResponse"/> object containing nothing.</returns>
            protected override NullResponse Process(TransferCartRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.Cart, "request.Cart");
    
                // After transfering the cart, it would remain as offline till the whole transaction is done.
                request.Cart.IsCreatedOffline = true;
    
                // For offline cart, persist the object directly into database.
                var transaction = CartWorkflowHelper.ConvertToSalesTransaction(request.Cart);
                CartWorkflowHelper.TransferSalesTransaction(this.Context, transaction);
    
                return new NullResponse();
            }
        }
    }
}
