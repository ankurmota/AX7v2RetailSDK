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
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// Handles workflow to resume suspended cart.
        /// </summary>
        public sealed class ResumeCartRequestHandler : SingleRequestHandler<ResumeCartRequest, ResumeCartResponse>
        {
            /// <summary>
            /// Executes the workflow to resume suspended cart.
            /// </summary>
            /// <param name="request">Instance of <see cref="ResumeCartRequest"/>.</param>
            /// <returns>Instance of <see cref="ResumeCartResponse"/>.</returns>
            protected override ResumeCartResponse Process(ResumeCartRequest request)
            {
                ThrowIf.Null(request, "request");

                var getSalesTransactionServiceRequest = new GetSalesTransactionsServiceRequest(
                    new CartSearchCriteria { CartId = request.CartId },
                    QueryResultSettings.SingleRecord,
                    mustRemoveUnavailableProductLines: true);
                var getSalesTransactionServiceResponse = this.Context.Execute<GetSalesTransactionsServiceResponse>(getSalesTransactionServiceRequest);
                SalesTransaction transaction = null;
                if (getSalesTransactionServiceResponse.SalesTransactions != null)
                {
                    transaction = getSalesTransactionServiceResponse.SalesTransactions.FirstOrDefault();
                }

                if (!transaction.IsSuspended)
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidStatus, request.CartId, "Cart is not suspended.");
                }

                // Resume the suspended transaction to normal state.
                transaction.EntryStatus = TransactionStatus.Normal;
                transaction.IsSuspended = false;
                transaction.TerminalId = this.Context.GetTerminal().TerminalId;
                transaction.BeginDateTime = this.Context.GetNowInChannelTimeZone();
                CartWorkflowHelper.Calculate(this.Context, transaction, null);
                CartWorkflowHelper.SaveSalesTransaction(this.Context, transaction);

                if (getSalesTransactionServiceResponse.LinesWithUnavailableProducts.Any())
                {
                    // Send notification to decide if caller should be notified about discontinued products.
                    var notification = new ProductDiscontinuedFromChannelNotification(getSalesTransactionServiceResponse.LinesWithUnavailableProducts);
                    this.Context.Notify(notification);
                }

                Cart cart = CartWorkflowHelper.ConvertToCart(this.Context, transaction);
                CartWorkflowHelper.RemoveHistoricalTenderLines(cart);

                return new ResumeCartResponse(cart);
            }
        }
    }
}