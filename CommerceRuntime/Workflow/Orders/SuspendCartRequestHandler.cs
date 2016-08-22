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
        using System;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Handles workflow to suspend cart.
        /// </summary>
        public sealed class SuspendCartRequestHandler : SingleRequestHandler<SuspendCartRequest, SuspendCartResponse>
        {
            /// <summary>
            /// Executes the workflow to suspend cart.
            /// </summary>
            /// <param name="request">Instance of <see cref="SuspendCartRequest"/>.</param>
            /// <returns>Instance of <see cref="SuspendCartResponse"/>.</returns>
            protected override SuspendCartResponse Process(SuspendCartRequest request)
            {
                ThrowIf.Null(request, "request");
    
                SalesTransaction transaction = CartWorkflowHelper.LoadSalesTransaction(this.Context, request.CartId);
    
                if (transaction.IsSuspended)
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CartNotActive, request.CartId, "Cart is already suspended.");
                }
    
                if (transaction.ActiveTenderLines.Any())
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CannotSuspendCartWithActiveTenderLines, request.CartId, "Cart with tender active tender lines cannot be suspended.");
                }
    
                if (transaction.ActiveSalesLines.Any(sl => sl.IsGiftCardLine))
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CannotSuspendCartWithActiveGiftCardSalesLines, request.CartId, "Cart with tender active gift card lines cannot be suspended.");
                }
    
                if (!(transaction.TerminalId ?? string.Empty).Equals(this.Context.GetTerminal().TerminalId ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                {
                    // If the terminal id of the cart is not same as the context then it means that the cart is active on another terminal.
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_LoadingActiveCartFromAnotherTerminalNotAllowed, request.CartId);
                }
    
                // Mark the transaction suspended.
                transaction.IsSuspended = true;
                transaction.EntryStatus = TransactionStatus.OnHold;
                CartWorkflowHelper.SaveSalesTransaction(this.Context, transaction);
                Cart cart = CartWorkflowHelper.ConvertToCart(this.Context, transaction);
                CartWorkflowHelper.RemoveHistoricalTenderLines(cart);
                return new SuspendCartResponse(cart);
            }
        }
    }
}
