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
    namespace Commerce.Runtime.Services
    {
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
    
        /// <summary>
        /// Gift card real time service.
        /// </summary>
        public class GiftCardRealtimeService : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetGiftCardRealtimeRequest),
                        typeof(IssueGiftCardRealtimeRequest),
                        typeof(AddToGiftCardRealtimeRequest),
                        typeof(PayGiftCardRealtimeRequest),
                        typeof(VoidGiftCardRealtimeRequest),
                        typeof(VoidGiftCardPaymentRealtimeRequest),
                        typeof(LockGiftCardRealtimeRequest),
                        typeof(UnlockGiftCardRealtimeRequest)
                    };
                }
            }
    
            /// <summary>
            /// Executes the request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
                if (requestType == typeof(GetGiftCardRealtimeRequest))
                {
                    response = GetGiftCard((GetGiftCardRealtimeRequest)request);
                }
                else if (requestType == typeof(IssueGiftCardRealtimeRequest))
                {
                    response = IssueGiftCard((IssueGiftCardRealtimeRequest)request);
                }
                else if (requestType == typeof(AddToGiftCardRealtimeRequest))
                {
                    response = AddToGiftCard((AddToGiftCardRealtimeRequest)request);
                }
                else if (requestType == typeof(PayGiftCardRealtimeRequest))
                {
                    response = PayGiftCard((PayGiftCardRealtimeRequest)request);
                }
                else if (requestType == typeof(VoidGiftCardRealtimeRequest))
                {
                    response = VoidGiftCard((VoidGiftCardRealtimeRequest)request);
                }
                else if (requestType == typeof(VoidGiftCardPaymentRealtimeRequest))
                {
                    response = VoidGiftCardPayment((VoidGiftCardPaymentRealtimeRequest)request);
                }
                else if (requestType == typeof(LockGiftCardRealtimeRequest))
                {
                    response = LockGiftCard((LockGiftCardRealtimeRequest)request);
                }
                else if (requestType == typeof(UnlockGiftCardRealtimeRequest))
                {
                    response = UnlockGiftCard((UnlockGiftCardRealtimeRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Pay and unlock gift card.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The <see cref="SingleEntityDataServiceResponse{GiftCard}"/> response.</returns>
            private static NullResponse PayGiftCard(PayGiftCardRealtimeRequest request)
            {
                var transactionClient = new TransactionService.TransactionServiceClient(request.RequestContext);
                decimal balance;
                string currencyCode;
    
                transactionClient.PayGiftCard(
                    request.GiftCardId,
                    request.Amount,
                    request.CurrencyCode,
                    request.ChannelId,
                    request.TerminalId,
                    request.StaffId,
                    request.TransactionId,
                    request.ReceiptId,
                    out currencyCode,
                    out balance);
    
                return new NullResponse();
            }
    
            /// <summary>
            /// Reserves the gift card for a given terminal so it cannot be used on other terminals.
            /// </summary>
            /// <param name="request">The <see cref="LockGiftCardRealtimeRequest"/> request.</param>
            /// <returns>The <see cref="SingleEntityDataServiceResponse{GiftCard}"/> response.</returns>
            private static SingleEntityDataServiceResponse<GiftCard> LockGiftCard(LockGiftCardRealtimeRequest request)
            {
                var transactionClient = new TransactionService.TransactionServiceClient(request.RequestContext);
    
                string cardCurrencyCode;
                decimal amount;
    
                transactionClient.LockGiftCard(request.GiftCardId, request.ChannelId, request.TerminalId, out cardCurrencyCode, out amount);
                var giftCard = new GiftCard
                {
                    Id = request.GiftCardId,
                    CardCurrencyCode = cardCurrencyCode,
                    BalanceInCardCurrency = amount
                };
    
                return new SingleEntityDataServiceResponse<GiftCard>(giftCard);
            }
    
            /// <summary>
            /// Removes reservation of the gift card from a given terminal so it can be used on other terminals.
            /// </summary>
            /// <param name="request">The <see cref="UnlockGiftCardRealtimeRequest"/> request.</param>
            /// <returns>The <see cref="NullResponse"/> response.</returns>
            private static NullResponse UnlockGiftCard(UnlockGiftCardRealtimeRequest request)
            {
                var transactionClient = new TransactionService.TransactionServiceClient(request.RequestContext);
    
                transactionClient.UnlockGiftCard(request.GiftCardId);
    
                return new NullResponse();
            }
    
            /// <summary>
            /// Voids gift card.
            /// </summary>
            /// <param name="request">The <see cref="VoidGiftCardRealtimeRequest"/> request.</param>
            /// <returns>The <see cref="NullResponse"/> response.</returns>
            private static NullResponse VoidGiftCard(VoidGiftCardRealtimeRequest request)
            {
                var transactionClient = new TransactionService.TransactionServiceClient(request.RequestContext);
    
                transactionClient.VoidGiftCard(request.GiftCardId);
    
                return new NullResponse();
            }
    
            /// <summary>
            /// Voids gift card payment.
            /// </summary>
            /// <param name="request">The <see cref="VoidGiftCardRealtimeRequest"/> request.</param>
            /// <returns>The <see cref="NullResponse"/> response.</returns>
            private static NullResponse VoidGiftCardPayment(VoidGiftCardPaymentRealtimeRequest request)
            {
                var transactionClient = new TransactionService.TransactionServiceClient(request.RequestContext);
    
                transactionClient.VoidGiftCardPayment(request.GiftCardId, request.ChannelId, request.TerminalId);
    
                return new NullResponse();
            }
    
            /// <summary>
            /// Issue gift card.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The <see cref="SingleEntityDataServiceResponse{GiftCard}"/> response that contains gift card.</returns>
            private static SingleEntityDataServiceResponse<GiftCard> IssueGiftCard(IssueGiftCardRealtimeRequest request)
            {
                var transactionClient = new TransactionService.TransactionServiceClient(request.RequestContext);
    
                string cardId;
    
                transactionClient.IssueGiftCard(request.GiftCardId, request.Amount, request.CurrencyCode, request.ChannelId, request.TerminalId, request.StaffId, request.TransactionId, request.ReceiptId, out cardId);
    
                GiftCard giftCard = new GiftCard
                {
                    Id = cardId,
                    BalanceCurrencyCode = request.CurrencyCode,
                    Balance = request.Amount,
                    CardCurrencyCode = request.CurrencyCode,
                    BalanceInCardCurrency = request.Amount
                };
    
                return new SingleEntityDataServiceResponse<GiftCard>(giftCard);
            }
    
            /// <summary>
            /// Add to gift card.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The <see cref="SingleEntityDataServiceResponse{GiftCard}"/> response that contains gift card.</returns>
            private static SingleEntityDataServiceResponse<GiftCard> AddToGiftCard(AddToGiftCardRealtimeRequest request)
            {
                var transactionClient = new TransactionService.TransactionServiceClient(request.RequestContext);
    
                string cardCurrencyCode;
                decimal balanceInCardCurrency;
    
                transactionClient.AddToGiftCard(request.GiftCardId, request.Amount, request.CurrencyCode, request.ChannelId, request.TerminalId, request.StaffId, request.TransactionId, request.ReceiptId, out cardCurrencyCode, out balanceInCardCurrency);
    
                GiftCard giftCard = new GiftCard
                {
                    Id = request.GiftCardId,
                    BalanceCurrencyCode = cardCurrencyCode,
                    BalanceInCardCurrency = balanceInCardCurrency
                };
    
                return new SingleEntityDataServiceResponse<GiftCard>(giftCard);
            }
    
            /// <summary>
            /// Gets gift card by id.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The <see cref="SingleEntityDataServiceResponse{GiftCard}"/> response.</returns>
            private static SingleEntityDataServiceResponse<GiftCard> GetGiftCard(GetGiftCardRealtimeRequest request)
            {
                var transactionClient = new TransactionService.TransactionServiceClient(request.RequestContext);
    
                string currencyCode;
                decimal balance;
    
                transactionClient.GetGiftCardBalance(request.GiftCardId, out currencyCode, out balance);
    
                var giftCard = new GiftCard
                {
                    Id = request.GiftCardId,
                    CardCurrencyCode = currencyCode,
                    BalanceInCardCurrency = balance
                };
    
                return new SingleEntityDataServiceResponse<GiftCard>(giftCard);
            }
        }
    }
}
