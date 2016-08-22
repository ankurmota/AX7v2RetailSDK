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
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Handles workflow for AddDiscountCodeToCart and RemoveDiscountCodeFromCart.
        /// </summary>
        public sealed class AddOrRemoveDiscountCodesRequestHandler : SingleRequestHandler<AddOrRemoveDiscountCodesRequest, SaveCartResponse>
        {
            /// <summary>
            /// Executes the workflow to add or delete discount codes in cart. 
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override SaveCartResponse Process(AddOrRemoveDiscountCodesRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.CartId, "request.CartId");
                ThrowIf.Null(request.DiscountCodes, "request.DiscountCodes");
   
                // Load sales transaction.
                SalesTransaction transaction = CartWorkflowHelper.LoadSalesTransaction(this.Context, request.CartId);
    
                if (transaction == null)
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CartNotFound, request.CartId);
                }
    
                IEnumerable<SalesTransaction> salesTransactions = new[] { transaction };
                transaction = salesTransactions.SingleOrDefault();
    
                if (transaction == null)
                {
                    return new SaveCartResponse(new Cart());
                }
    
                bool update = false;
    
                switch (request.DiscountCodesOperation)
                {
                    case DiscountCodesOperation.Add: 
                        foreach (string discountCode in request.DiscountCodes)
                        {
                            if (!transaction.DiscountCodes.Contains(discountCode))
                            {
                                transaction.DiscountCodes.Add(discountCode);
                                update = true;
                            }
                        }
    
                        break;
    
                    case DiscountCodesOperation.Remove:
                        foreach (string discountCode in request.DiscountCodes)
                        {
                            transaction.DiscountCodes.Remove(discountCode);
                            update = true;
                        }
    
                        break;
                    default:
                        throw new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest,
                            string.Format("Invalid discount code operation value: {0}", request.DiscountCodesOperation));
                }
    
                if (update)
                {
                    // Calculate totals
                    CartWorkflowHelper.Calculate(this.Context, transaction, null);
    
                    // Save the sales transaction
                    CartWorkflowHelper.SaveSalesTransaction(this.Context, transaction);
                }
    
                Cart cart = CartWorkflowHelper.ConvertToCart(this.Context, transaction);
                CartWorkflowHelper.RemoveHistoricalTenderLines(cart);
    
                return new SaveCartResponse(cart);
            }        
        }
    }
}
