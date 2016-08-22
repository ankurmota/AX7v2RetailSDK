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
        using System.Collections.Generic;
        using System.Diagnostics.CodeAnalysis;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// Handler for the submit order request.
        /// </summary>
        public sealed class SubmitOrderRequestHandler : SingleRequestHandler<CreateOrderFromCartRequest, CreateOrderFromCartResponse>
        {
            /// <summary>
            /// Operation types inside submit order workflow. Used for logging.
            /// </summary>
            private enum SubmitOrderOperationType
            {
                GetSalesTransaction = 0,
                ValidateContextForCreateOrder,
                ValidateAndResolveAddresses,
                ValidateShippingAddresses,
                ValidateReasonCodes,
                AuthorizePayments,
                FillMissingRequirementsForOrder,
                CreateSaleOrderInCrt
            }
    
            /// <summary>
            /// Creates a sales order given the cart and payment information.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Logging exception as warning only.")]
            protected override CreateOrderFromCartResponse Process(CreateOrderFromCartRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.CartId, "request.CartId");
    
                List<TenderLine> tenderLines = new List<TenderLine>();
    
                // Get the Sales Transaction.
                SalesTransaction salesTransaction = null;
                ExecutionHandler(
                    delegate
                    {
                        PopulateSalesTransaction(this.Context, request.CartId, request.ReceiptEmailAddress, out salesTransaction);
                    },
                    SubmitOrderOperationType.GetSalesTransaction.ToString());
    
                // Validate sales order for creation.
                ExecutionHandler(
                    delegate
                    {
                        OrderWorkflowHelper.ValidateContextForCreateOrder(this.Context, salesTransaction);
                    },
                    SubmitOrderOperationType.ValidateContextForCreateOrder.ToString());
    
                // Validate addresses.
                ExecutionHandler(
                    delegate
                    {
                        ShippingHelper.ValidateAndResolveAddresses(this.Context, salesTransaction);
                    },
                    SubmitOrderOperationType.ValidateAndResolveAddresses.ToString());
    
                // Add missing dependencies to the sales transaction.
                ExecutionHandler(
                    delegate
                    {
                        OrderWorkflowHelper.FillMissingRequirementsForOrder(this.Context, salesTransaction);
                    },
                    SubmitOrderOperationType.FillMissingRequirementsForOrder.ToString());
    
                // Validate addresses for shipping.
                ExecutionHandler(
                    delegate
                    {
                        ShippingHelper.ValidateShippingInformation(this.Context, salesTransaction);
                    },
                    SubmitOrderOperationType.ValidateShippingAddresses.ToString());
    
                // Validate required reason code lines has been filled.
                ExecutionHandler(
                    () => ReasonCodesWorkflowHelper.ValidateRequiredReasonCodeLinesFilled(this.Context, salesTransaction),
                    SubmitOrderOperationType.ValidateReasonCodes.ToString());
    
                // Authorize payments.
                ExecutionHandler(
                    delegate
                    {
                        tenderLines = OrderWorkflowHelper.ProcessPendingOrderPayments(this.Context, salesTransaction, request.CartTenderLines);
                    },
                    SubmitOrderOperationType.AuthorizePayments.ToString());
    
                SalesOrder salesOrder = null;
                try
                {
                    // Create order and add all authorization payment blobs.
                    ExecutionHandler(
                        delegate
                        {
                            salesOrder = OrderWorkflowHelper.CreateSalesOrder(this.Context, salesTransaction);
                        },
                        SubmitOrderOperationType.CreateSaleOrderInCrt.ToString());
                }
                catch (Exception ex)
                {
                    try
                    {
                        // Cancel the payment authorizations
                        OrderWorkflowHelper.CancelPayments(this.Context, salesTransaction, tenderLines, request.CartTenderLines);
                    }
                    catch (PaymentException cancelPaymentsEx)
                    {
                        // Inside of CancelPayments() we always wrap Exception as PaymentException.
                        RetailLogger.Log.CrtWorkflowCancelingPaymentFailure(ex, cancelPaymentsEx);
                    }
    
                    throw;
                }
    
                // We also need to delete the shopping cart.
                CartWorkflowHelper.TryDeleteCart(
                    this.Context,
                    new SalesTransaction
                    {
                        Id = request.CartId,
                        TerminalId = salesTransaction.TerminalId,
                        CustomerId = salesTransaction.CustomerId,
                    });
    
                return new CreateOrderFromCartResponse(salesOrder);
            }
    
            /// <summary>
            /// Populates the sales transaction.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="cartId">The cart id.</param>
            /// <param name="email">The email.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            private static void PopulateSalesTransaction(RequestContext context, string cartId, string email, out SalesTransaction salesTransaction)
            {
                salesTransaction = CartWorkflowHelper.LoadSalesTransaction(context, cartId);
    
                if (salesTransaction == null)
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CartNotFound, cartId);
                }
    
                salesTransaction.ReceiptEmail = email;
                salesTransaction.TransactionType = SalesTransactionType.PendingSalesOrder;
    
                if (string.IsNullOrEmpty(salesTransaction.ReceiptEmail))
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "Receipt email must be provided.");
                }
    
                OrderWorkflowHelper.FillTransactionWithContextData(context, salesTransaction);
            }
    
            /// <summary>
            /// Executions the handler.
            /// </summary>
            /// <param name="action">The action.</param>
            /// <param name="operationType">Type of the operation.</param>
            private static void ExecutionHandler(Action action, string operationType)
            {
                try
                {
                    action();
                    NetTracer.Information("Operation {0} succeeded", operationType);
                }
                catch (Exception e)
                {
                    RetailLogger.Log.CrtWorkflowSubmitOrderRequestHandlerExecutionFailure(operationType, e);
                    throw;
                }
            }
        }
    }
}
