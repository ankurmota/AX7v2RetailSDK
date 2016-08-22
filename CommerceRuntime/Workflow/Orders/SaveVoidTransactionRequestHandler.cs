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
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        
        /// <summary>
        /// Handler for the submit sales transaction request.
        /// </summary>
        public sealed class SaveVoidTransactionRequestHandler : SingleRequestHandler<SaveVoidTransactionRequest, SaveVoidTransactionResponse>
        {
            /// <summary>
            /// Creates a sales transaction given the cart.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override SaveVoidTransactionResponse Process(SaveVoidTransactionRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.CartId, "request.CartId");
    
                // Get the Sales Transaction
                SalesTransaction salesTransaction = CartWorkflowHelper.LoadSalesTransaction(this.Context, request.CartId, ignoreProductDiscontinuedNotification: true);
                if (salesTransaction == null)
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CartNotFound, request.CartId);
                }
    
                if (salesTransaction.IsSuspended)
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CartNotActive, request.CartId);
                }
    
                // If there are unvoided non-historical Tender lines throw exception that Transaction cannot be voided.
                if (salesTransaction.TenderLines != null && salesTransaction.TenderLines.Any(l => l.TransactionStatus != TransactionStatus.Voided && l.Status != TenderLineStatus.Historical))
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_VoidTransactionContainsTenderedLines, request.CartId);
                }
    
                // Add or update any incoming reason codes on the transaction.
                Cart cartToBeVoided = new Cart
                {
                    Id = request.CartId
                };

                if (request.ReasonCodeLines != null)
                {
                    cartToBeVoided.ReasonCodeLines = request.ReasonCodeLines.ToList();
                }
    
                ReasonCodesWorkflowHelper.AddOrUpdateReasonCodeLinesOnTransaction(salesTransaction, cartToBeVoided);
    
                // Calculate the required reason codes on the tender line for voiding transaction.
                ReasonCodesWorkflowHelper.CalculateRequiredReasonCodesOnTransaction(this.Context, salesTransaction, ReasonCodeSourceType.VoidTransaction);
    
                GiftCardWorkflowHelper.VoidGiftCardSalesLines(this.Context, salesTransaction);
    
                // Set the Sales Transaction Status to Voided.
                salesTransaction.EntryStatus = TransactionStatus.Voided;
    
                // Set the transaction type to represent Cash & Carry sales.
                if (salesTransaction.TransactionType != SalesTransactionType.IncomeExpense)
                {
                    salesTransaction.TransactionType = SalesTransactionType.Sales;
                }
    
                // Fill in Store and Terminal information.
                OrderWorkflowHelper.FillTransactionWithContextData(this.Context, salesTransaction);
    
                // Create order
                var salesOrder = OrderWorkflowHelper.CreateSalesOrder(this.Context, salesTransaction);
    
                // We also need to delete the shopping cart.
                CartWorkflowHelper.TryDeleteCart(
                    this.Context,
                    new SalesTransaction
                    {
                        Id = request.CartId,
                        TerminalId = salesTransaction.TerminalId,
                        CustomerId = salesTransaction.CustomerId,
                    });
    
                CartWorkflowHelper.LogAuditEntry(
                    this.Context,
                    "SaveVoidTransactionRequestHandler.Process",
                    string.Format("Transaction '{0}' voided.", request.CartId));
    
                return new SaveVoidTransactionResponse(salesOrder);
            }
        }
    }
}
