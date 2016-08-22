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
        /// Copies a shopping cart.
        /// </summary>
        public sealed class CopyCartRequestHandler : SingleRequestHandler<CopyCartRequest, CopyCartResponse>
        {
            /// <summary>
            /// Copies the cart from the request to a different cart.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns><see cref="CopyCartResponse"/> object containing the new cart.</returns>
            protected override CopyCartResponse Process(CopyCartRequest request)
            {
                ThrowIf.Null(request, "request");
             
                // Loading sales transaction.
                SalesTransaction salesTransaction = CartWorkflowHelper.LoadSalesTransaction(this.Context, request.CartId);
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                // Assigning new transaction id (cart id).
                salesTransaction.Id = CartWorkflowHelper.GenerateRandomTransactionId(this.Context);
    
                // Setting the cart type from the request.
                salesTransaction.CartType = request.TargetCartType;
    
                // Saving sales transaction.
                CartWorkflowHelper.SaveSalesTransaction(this.Context, salesTransaction);
    
                // Reloading sales transaction.
                salesTransaction = CartWorkflowHelper.LoadSalesTransaction(this.Context, salesTransaction.Id);
    
                Cart cart = CartWorkflowHelper.ConvertToCart(this.Context, salesTransaction);
                CartWorkflowHelper.RemoveHistoricalTenderLines(cart);
    
                return new CopyCartResponse(cart);
            }
        }
    }
}
