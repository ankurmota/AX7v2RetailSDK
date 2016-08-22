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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// Handler for the submit sales transaction request.
        /// </summary>
        public sealed class SubmitSalesTransactionRequestHandler : SingleRequestHandler<SubmitSalesTransactionRequest, SubmitSalesTransactionResponse>
        {
            /// <summary>
            /// Creates a sales transaction given the cart.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override SubmitSalesTransactionResponse Process(SubmitSalesTransactionRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.CartId, "request.CartId");
    
                // Get the Sales Transaction
                SalesTransaction salesTransaction = CartWorkflowHelper.LoadSalesTransaction(this.Context, request.CartId);
                if (salesTransaction == null)
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CartNotFound, request.CartId);
                }
    
                if (salesTransaction.IsSuspended)
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CartNotActive, request.CartId);
                }
    
                if (!(salesTransaction.TerminalId ?? string.Empty).Equals(this.Context.GetTerminal().TerminalId ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                {
                    // If the terminal id of the cart is not same as the context then it means that the cart is active on another terminal.
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_LoadingActiveCartFromAnotherTerminalNotAllowed, request.CartId);
                }
    
                salesTransaction.ReceiptEmail = request.ReceiptEmail;
    
                // Set the transaction type to represent Cash & Carry sales by default, if it's not an IncomeExpense nor AccountDeposit transaction.
                if (salesTransaction.TransactionType != SalesTransactionType.IncomeExpense && salesTransaction.TransactionType != SalesTransactionType.CustomerAccountDeposit)
                {
                    salesTransaction.TransactionType = SalesTransactionType.Sales;
                }
    
                // Fill in Store and Terminal information.
                OrderWorkflowHelper.FillTransactionWithContextData(this.Context, salesTransaction);
    
                // Calculate required reason code for end of transaction.
                ReasonCodesWorkflowHelper.CalculateRequiredReasonCodesOnTransaction(this.Context, salesTransaction, ReasonCodeSourceType.EndOfTransaction);
    
                // Validate required reason code lines has been filled.
                ReasonCodesWorkflowHelper.ValidateRequiredReasonCodeLinesFilled(this.Context, salesTransaction);
    
                // Validate return permission.
                CartWorkflowHelper.ValidateReturnPermission(this.Context, salesTransaction, salesTransaction.CartType);
    
                // Fill in variant information.
                OrderWorkflowHelper.FillVariantInformation(this.Context, salesTransaction);
    
                // Fill in Receipt Id.
                OrderWorkflowHelper.FillInReceiptId(this.Context, salesTransaction, request.ReceiptNumberSequence);
    
                // Validate sales order for creation.
                OrderWorkflowHelper.ValidateContextForCreateOrder(this.Context, salesTransaction);
    
                // Validate payments.
                OrderWorkflowHelper.CalculateAndValidateAmountPaidForCheckout(this.Context, salesTransaction);
    
                salesTransaction = OrderWorkflowHelper.ProcessCheckoutPayments(this.Context, salesTransaction);
    
                // release/unlock gift cards on sales lines
                GiftCardWorkflowHelper.UnlockGiftCardsOnActiveSalesLines(this.Context, salesTransaction);
    
                // Pay Sales Invoices...
                OrderWorkflowHelper.SettleInvoiceSalesLines(this.Context, salesTransaction);
    
                // Create order
                var salesOrder = OrderWorkflowHelper.CreateSalesOrder(this.Context, salesTransaction);
    
                // We also need to delete the shopping cart.
                CartWorkflowHelper.TryDeleteCart(
                    this.Context,
                    new SalesTransaction
                    {
                        Id = salesTransaction.Id,
                        TerminalId = salesTransaction.TerminalId,
                        CustomerId = salesTransaction.CustomerId,
                    });
    
                return new SubmitSalesTransactionResponse(salesOrder);
            }
        }
    }
}
