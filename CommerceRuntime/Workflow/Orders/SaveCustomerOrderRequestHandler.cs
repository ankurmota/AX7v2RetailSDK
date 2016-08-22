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
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Handler for the submit order request.
        /// </summary>
        public sealed class SaveCustomerOrderRequestHandler : SingleRequestHandler<SaveCustomerOrderRequest, SaveCustomerOrderResponse>
        {
            /// <summary>
            /// Creates a sales order given the cart and payment information.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>        
            protected override SaveCustomerOrderResponse Process(SaveCustomerOrderRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.CartId, "request.CartId");
    
                // Get the Sales Transaction
                SalesTransaction transaction = CustomerOrderWorkflowHelper.GetSalesTransaction(this.Context, request.CartId, request.ReceiptEmailAddress);
    
                // Update customer order fields before validation
                CustomerOrderWorkflowHelper.UpdateCustomerOrderFieldsOnCheckout(transaction);
    
                // Return validations
                switch (transaction.CustomerOrderMode)
                {
                    case CustomerOrderMode.Return:
                        CustomerOrderWorkflowHelper.ValidateOrderForReturn(this.Context, transaction);
                        break;
    
                    case CustomerOrderMode.CustomerOrderCreateOrEdit:
                    case CustomerOrderMode.QuoteCreateOrEdit:
                        CustomerOrderWorkflowHelper.ValidateOrderAndQuoteCreationAndUpdate(this.Context, transaction);
                        break;
    
                    default:
                        throw new NotSupportedException(
                            string.Format(CultureInfo.InvariantCulture, "Customer order mode {0} not supported.", transaction.CustomerOrderMode));
                }
    
                // Add customer order specific missing dependencies to the sales transaction
                CustomerOrderWorkflowHelper.FillMissingRequirementsForOrder(this.Context, transaction);
    
                // Resolve addresses
                ShippingHelper.ValidateAndResolveAddresses(this.Context, transaction);
    
                // Validate shipping addresses
                ShippingHelper.ValidateShippingInformation(this.Context, transaction);
    
                // Validate required reason code lines has been filled
                ReasonCodesWorkflowHelper.ValidateRequiredReasonCodeLinesFilled(this.Context, transaction);
    
                // Fill in receipt id. Receipt id will be needed if paying with credit memo.
                transaction = CustomerOrderWorkflowHelper.FillInReceiptId(this.Context, transaction, request.ReceiptNumberSequence);
    
                // Handle payments
                if (request.TokenizedPaymentCard != null)
                {
                    transaction = CustomerOrderWorkflowHelper.HandlePayments(this.Context, transaction, request.TokenizedPaymentCard);
                }
                else
                {
                    transaction = CustomerOrderWorkflowHelper.HandlePayments(this.Context, transaction);
                }
    
                // Create order through transaction service
                SalesOrder salesOrder = CustomerOrderWorkflowHelper.SaveCustomerOrder(this.Context, transaction);
    
                CartWorkflowHelper.TryDeleteCart(this.Context, transaction);
    
                return new SaveCustomerOrderResponse(salesOrder);
            }
        }
    }
}
