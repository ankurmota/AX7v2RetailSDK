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
        /// Handles workflow for order recalculation.
        /// </summary>
        public sealed class RecalculateOrderRequestHandler : SingleRequestHandler<RecalculateOrderRequest, RecalculateOrderResponse>
        {
            /// <summary>
            /// Executes the workflow to recalculate a sales transaction and return a cart representing the transaction.
            /// </summary>
            /// <param name="request">Instance of <see cref="RecalculateOrderRequest"/>.</param>
            /// <returns>Instance of <see cref="RecalculateOrderResponse"/>.</returns>
            protected override RecalculateOrderResponse Process(RecalculateOrderRequest request)
            {
                ThrowIf.Null(request, "request");
    
                // Recovers transaction from database
                SalesTransaction salesTransaction = CartWorkflowHelper.LoadSalesTransaction(this.Context, request.CartId);
    
                if (salesTransaction == null)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CartNotFound, "Cart does not exist.");
                }

                // Check permissions.
                RetailOperation operation = salesTransaction.CartType == CartType.CustomerOrder ? RetailOperation.RecalculateCustomerOrder : RetailOperation.CalculateFullDiscounts;
                request.RequestContext.Execute<NullResponse>(new CheckAccessServiceRequest(operation));
    
                // When recalcalculating order, unlock prices so new prices and discounts are applied to the entire order.
                foreach (SalesLine salesLine in salesTransaction.SalesLines)
                {
                    salesLine.IsPriceLocked = false;
                }
    
                // Recalculate transaction
                CartWorkflowHelper.Calculate(this.Context, salesTransaction, requestedMode: null, discountCalculationMode: DiscountCalculationMode.CalculateAll);
    
                // Update order on database
                CartWorkflowHelper.SaveSalesTransaction(this.Context, salesTransaction);
    
                // Convert the SalesOrder into a cart object for the client
                Cart cart = CartWorkflowHelper.ConvertToCart(this.Context, salesTransaction);
                CartWorkflowHelper.RemoveHistoricalTenderLines(cart);
    
                // Return cart
                return new RecalculateOrderResponse(cart);
            }
        }
    }
}
