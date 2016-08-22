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
        /// Handles workflow to issue or add balance to gift card.
        /// </summary>
        public sealed class IssueOrAddToGiftCardRequestHandler : SingleRequestHandler<IssueOrAddToGiftCardRequest, IssueOrAddToGiftCardResponse>
        {
            /// <summary>
            /// Executes the workflow to issue or add balance to gift card.
            /// </summary>
            /// <param name="request">Instance of <see cref="IssueOrAddToGiftCardRequest"/>.</param>
            /// <returns>Instance of <see cref="IssueOrAddToGiftCardResponse"/>.</returns>
            protected override IssueOrAddToGiftCardResponse Process(IssueOrAddToGiftCardRequest request)
            {
                ThrowIf.Null(request, "request");
    
                SalesTransaction transaction = CartWorkflowHelper.LoadSalesTransaction(this.Context, request.CartId);
    
                GetGiftCardServiceResponse serviceResponse;
                GiftCardOperationType operation;
    
                if (request.ExistingGiftCard)
                {
                    // add to gift card
                    var serviceRequest = new AddToGiftCardServiceRequest(
                        request.GiftCardId,
                        request.Amount,
                        request.CurrencyCode,
                        transaction);
    
                    serviceResponse = this.Context.Execute<GetGiftCardServiceResponse>(serviceRequest);
    
                    operation = GiftCardOperationType.AddTo;
                }
                else
                {
                    // Issue new gift card
                    var serviceRequest = new IssueGiftCardServiceRequest(
                        request.GiftCardId,
                        request.Amount,
                        request.CurrencyCode,
                        transaction);
    
                    serviceResponse = this.Context.Execute<GetGiftCardServiceResponse>(serviceRequest);
    
                    operation = GiftCardOperationType.Issue;
                }
    
                CartLine giftCardLine = new CartLine
                {
                    Price = request.Amount,
                    IsGiftCardLine = true,
                    Description = request.LineDescription,
                    Comment = serviceResponse.GiftCard.Id,
                    GiftCardId = serviceResponse.GiftCard.Id,
                    GiftCardCurrencyCode = serviceResponse.GiftCard.CardCurrencyCode,
                    GiftCardOperation = operation,
                    Quantity = 1m
                };
    
                return new IssueOrAddToGiftCardResponse(giftCardLine);
            }
        }
    }
}
