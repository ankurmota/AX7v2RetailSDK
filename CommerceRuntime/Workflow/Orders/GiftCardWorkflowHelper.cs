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
        /// Helper class for gift card operations.
        /// </summary>
        internal static class GiftCardWorkflowHelper
        {
            /// <summary>
            /// Voids active gift card lines on the transaction.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            internal static void VoidGiftCardSalesLines(RequestContext context, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                IEnumerable<SalesLine> activeGiftCardLines = salesTransaction.SalesLines.Where(l => l.IsGiftCardLine && !l.IsVoided);
                foreach (SalesLine line in activeGiftCardLines)
                {
                    GiftCardWorkflowHelper.VoidGiftCardOperation(context, salesTransaction, line.GiftCardId, line.GiftCardCurrencyCode, line.GiftCardOperation, line.TotalAmount);
                }
            }
    
            /// <summary>
            /// Unlocks the gift cards that are on active sales lines.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            internal static void UnlockGiftCardsOnActiveSalesLines(RequestContext context, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                IEnumerable<SalesLine> activeGiftCardLines = salesTransaction.SalesLines.Where(l => l.IsGiftCardLine && !l.IsVoided);
                foreach (SalesLine line in activeGiftCardLines)
                {
                    GiftCardWorkflowHelper.UnlockGiftCard(context, line.GiftCardId);
                }
            }
            
            /// <summary>
            /// Voids the gift card operation.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="giftCardId">The identifier of the gift card to be voided.</param>
            /// <param name="giftCardCurrencyCode">Gift card's currency code.</param>
            /// <param name="giftCardOperation">The type of gift card operation(Issue or AddTo).</param>
            /// <param name="amount">The amount on the gift card.</param>
            internal static void VoidGiftCardOperation(RequestContext context, SalesTransaction salesTransaction, string giftCardId, string giftCardCurrencyCode, GiftCardOperationType giftCardOperation, decimal amount)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                switch (giftCardOperation)
                {
                    case GiftCardOperationType.AddTo:
                    {
                        GiftCardWorkflowHelper.VoidAddToGiftCardOperation(context, salesTransaction, giftCardId, giftCardCurrencyCode, amount);
    
                        // Unlock the gift card.
                        GiftCardWorkflowHelper.UnlockGiftCard(context, giftCardId);
                        break;
                    }
    
                    case GiftCardOperationType.Issue:
                    {
                        GiftCardWorkflowHelper.VoidIssuedGiftCardOperation(context, giftCardId);
    
                        // No need to unlock gift card in this case because voiding will result gift card entry being deleted.
                        break;
                    }
    
                    default:
                        throw new InvalidOperationException(string.Format("Gift card operation {0} is not supported.", giftCardOperation));
                }
            }
    
            /// <summary>
            /// Voids the issued gift card.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <param name="giftCardId">The identifier of the gift card to void.</param>
            private static void VoidIssuedGiftCardOperation(RequestContext context, string giftCardId)
            {
                // Void the gift card.
                var serviceRequest = new VoidGiftCardServiceRequest(
                    giftCardId);
    
                context.Execute<NullResponse>(serviceRequest);
            }
    
            /// <summary>
            /// Voids the "add to gift card" operation.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="giftCardId">The identifier of the gift card to void.</param>
            /// <param name="giftCardCurrencyCode">Gift card's currency code.</param>
            /// <param name="amountAdded">The amount added to gift card.</param>
            private static void VoidAddToGiftCardOperation(RequestContext context, SalesTransaction transaction, string giftCardId, string giftCardCurrencyCode, decimal amountAdded)
            {
                // Void add to gift card operation
                var serviceRequest = new AddToGiftCardServiceRequest(
                    giftCardId,
                    decimal.Negate(amountAdded), // add to gift card line can only be voided by decreasing balance  
                    context.GetChannelConfiguration().Currency,
                    giftCardCurrencyCode,
                    transaction,
                    isReversal: true);
    
                context.Execute<GetGiftCardServiceResponse>(serviceRequest);
            }
    
            /// <summary>
            /// Unlocks the gift card.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <param name="giftCardId">Gift card identifier.</param>
            private static void UnlockGiftCard(RequestContext context, string giftCardId)
            {
                // Unlock gift cards on sales lines
                var unlockGiftCardRequest = new UnlockGiftCardServiceRequest(giftCardId);
    
                try
                {
                    context.Execute<NullResponse>(unlockGiftCardRequest);
                }
                catch (Exception ex)
                {
                    // Inform the cashier that the gift card could not be unlocked. 
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_GiftCardUnlockFailed,
                        null,
                        ex,
                        string.Format("Exception while trying to unlock gift card: {0}", giftCardId));
                }
            }
        }
    }
}
