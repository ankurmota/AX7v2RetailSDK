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
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Handlers;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// Encapsulates service that implements gift card operations.
        /// </summary>
        public class GiftCardService : IOperationRequestHandler
        {
                    /// <summary>
            /// Gets a collection of operation identifiers supported by this request handler.
            /// </summary>
            public IEnumerable<int> SupportedOperationIds
            {
                get
                {
                    return new[]
                    {
                        (int)RetailOperation.PayGiftCertificate,
                        (int)RetailOperation.IssueGiftCertificate,
                        (int)RetailOperation.AddToGiftCard,
                        (int)RetailOperation.GiftCardBalance
                    };
                }
            }
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(CalculatePaymentAmountServiceRequest),
                        typeof(AuthorizePaymentServiceRequest),
                        typeof(CapturePaymentServiceRequest),
                        typeof(VoidPaymentServiceRequest),
                        typeof(GetChangePaymentServiceRequest),
                        typeof(GetGiftCardServiceRequest),
                        typeof(AddToGiftCardServiceRequest),
                        typeof(IssueGiftCardServiceRequest),
                        typeof(UnlockGiftCardServiceRequest),
                        typeof(VoidGiftCardServiceRequest)
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
    
                Response response;
                Type requestType = request.GetType();
    
                if (requestType == typeof(CalculatePaymentAmountServiceRequest))
                {
                    response = CalculatePaymentAmount((CalculatePaymentAmountServiceRequest)request);
                }
                else if (requestType == typeof(AuthorizePaymentServiceRequest))
                {
                    response = AuthorizePayment((AuthorizePaymentServiceRequest)request);
                }
                else if (requestType == typeof(VoidPaymentServiceRequest))
                {
                    response = CancelPayment((VoidPaymentServiceRequest)request);
                }
                else if (requestType == typeof(GetChangePaymentServiceRequest))
                {
                    response = GetChange((GetChangePaymentServiceRequest)request);
                }
                else if (requestType == typeof(CapturePaymentServiceRequest))
                {
                    response = CapturePayment((CapturePaymentServiceRequest)request);
                }
                else if (requestType == typeof(GetGiftCardServiceRequest))
                {
                    response = GetGiftCard((GetGiftCardServiceRequest)request);
                }
                else if (requestType == typeof(IssueGiftCardServiceRequest))
                {
                    response = IssueGiftCard((IssueGiftCardServiceRequest)request);
                }
                else if (requestType == typeof(UnlockGiftCardServiceRequest))
                {
                    response = UnlockGiftCard((UnlockGiftCardServiceRequest)request);
                }
                else if (requestType == typeof(AddToGiftCardServiceRequest))
                {
                    response = AddToGiftCard((AddToGiftCardServiceRequest)request);
                }
                else if (requestType == typeof(VoidGiftCardServiceRequest))
                {
                    response = VoidGiftCard((VoidGiftCardServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Calculate amount to do be paid.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>A response containing the updated tender line.</returns>
            private static CalculatePaymentAmountServiceResponse CalculatePaymentAmount(CalculatePaymentAmountServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                // no calculation required.
                return new CalculatePaymentAmountServiceResponse(request.TenderLine);
            }
    
            /// <summary>
            /// Authorizes the payment.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>A response containing the authorized tender line.</returns>
            private static AuthorizePaymentServiceResponse AuthorizePayment(AuthorizePaymentServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                ValidateNotAlreadyAdded(request.Transaction, request.TenderLine.GiftCardId);
    
                var lockGiftCardRealtimeRequest = new LockGiftCardRealtimeRequest(
                    request.TenderLine.GiftCardId,
                    request.RequestContext.GetPrincipal().ChannelId,
                    request.RequestContext.GetTerminal() == null ? string.Empty : request.RequestContext.GetTerminal().TerminalId);
                GiftCard giftCard = request.RequestContext.Execute<SingleEntityDataServiceResponse<GiftCard>>(lockGiftCardRealtimeRequest).Entity;
    
                GetCurrencyValueServiceResponse convertAmountToGiftCardCurrencyResponse;
    
                try
                {
                    if (!request.TenderLine.Currency.Equals(request.RequestContext.GetChannelConfiguration().Currency, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new PaymentException(
                            PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_GiftCardCurrencyMismatch, 
                            string.Format("Gift card payments are only supported in channel currency. Currency requested: {0}", request.TenderLine.Currency));
                    }
    
                    convertAmountToGiftCardCurrencyResponse = ToGiftCardCurrency(request.RequestContext, request.TenderLine.Amount, giftCard.CardCurrencyCode);
    
                    // Check if gift card has enough balance.
                    if (giftCard.BalanceInCardCurrency < convertAmountToGiftCardCurrencyResponse.RoundedConvertedAmount)
                    {
                        throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_PaymentAmountExceedsGiftBalance);
                    }
                }
                catch
                {
                    try
                    {
                        // At this point payment failed but gift card is already locked.
                        // Need to unlock gift card (better as void payment) so it can be used in future.
                        var voidGiftCardPaymentRequest = new VoidGiftCardPaymentRealtimeRequest(
                                request.TenderLine.GiftCardId,
                                request.RequestContext.GetPrincipal().ChannelId,
                                request.RequestContext.GetTerminal() == null ? string.Empty : request.RequestContext.GetTerminal().TerminalId);
    
                        request.RequestContext.Execute<NullResponse>(voidGiftCardPaymentRequest);
                    }
                    catch (Exception ex)
                    {
                        RetailLogger.Log.CrtServicesGiftCardServiceUnlockGiftCardFailure(ex);
                    }
    
                    throw;
                }
    
                request.TenderLine.Currency = giftCard.CardCurrencyCode;
                request.TenderLine.AmountInTenderedCurrency = convertAmountToGiftCardCurrencyResponse.RoundedConvertedAmount;
                request.TenderLine.ExchangeRate = convertAmountToGiftCardCurrencyResponse.ExchangeRate;
                request.TenderLine.Status = TenderLineStatus.PendingCommit;
                request.TenderLine.IsVoidable = true;
    
                return new AuthorizePaymentServiceResponse(request.TenderLine);
            }
    
            /// <summary>
            /// Captures the payment.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>A response containing the captured tender line.</returns>
            private static CapturePaymentServiceResponse CapturePayment(CapturePaymentServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                var payUsingGiftCardRequest = new PayGiftCardRealtimeRequest(
                    request.TenderLine.GiftCardId,
                    request.TenderLine.AmountInTenderedCurrency, // amount in gift card currency
                    request.TenderLine.Currency, // gift card currency code
                    request.RequestContext.GetPrincipal().ChannelId,
                    request.RequestContext.GetTerminal() == null ? string.Empty : request.RequestContext.GetTerminal().TerminalId,
                    request.RequestContext.GetPrincipal().UserId ?? string.Empty,
                    request.Transaction.Id,
                    request.Transaction.ReceiptId ?? string.Empty);
    
                request.RequestContext.Execute<NullResponse>(payUsingGiftCardRequest);
    
                // To match ePOS behavior (and not break statement calculation and posting) updating tender
                // line to respresent payment done in channel currency.
                request.TenderLine.Currency = request.RequestContext.GetChannelConfiguration().Currency;
                request.TenderLine.AmountInTenderedCurrency = request.TenderLine.Amount;
                request.TenderLine.ExchangeRate = 1.0m;
                request.TenderLine.Status = TenderLineStatus.Committed;
                request.TenderLine.IsVoidable = true;
    
                return new CapturePaymentServiceResponse(request.TenderLine);
            }
    
            /// <summary>
            /// Cancels the payment.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>A response containing the canceled tender line.</returns>
            private static VoidPaymentServiceResponse CancelPayment(VoidPaymentServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (request.TenderLine == null)
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPaymentRequest, "request.TenderLine is null.");
                }
    
                var voidGiftCardPayment = new VoidGiftCardPaymentRealtimeRequest(
                    request.TenderLine.GiftCardId,
                    request.RequestContext.GetPrincipal().ChannelId,
                    request.RequestContext.GetTerminal() == null ? string.Empty : request.RequestContext.GetTerminal().TerminalId);
                request.RequestContext.Execute<NullResponse>(voidGiftCardPayment);
    
                request.TenderLine.Status = TenderLineStatus.Voided;
                request.TenderLine.IsVoidable = false;
    
                return new VoidPaymentServiceResponse(request.TenderLine);
            }
    
            /// <summary>
            /// Get the gift card.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>A response containing the gift card information.</returns>
            private static GetGiftCardServiceResponse GetGiftCard(GetGiftCardServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (string.IsNullOrWhiteSpace(request.GiftCardId))
                {
                    throw new ArgumentException("request.Id is null or empty.", "request");
                }
    
                var requestGetGiftCardBalance = new GetGiftCardRealtimeRequest(request.GiftCardId);
                GiftCard card = request.RequestContext.Execute<SingleEntityDataServiceResponse<GiftCard>>(requestGetGiftCardBalance).Entity;
    
                GetCurrencyValueServiceResponse convertBalanceToChannelCurrencyResponse = FromGiftCardCurrency(request.RequestContext, card.BalanceInCardCurrency, card.CardCurrencyCode);
    
                GiftCard giftCard = new GiftCard
                {
                    Id = request.GiftCardId,
                    BalanceCurrencyCode = convertBalanceToChannelCurrencyResponse.ToCurrencyCode,
                    Balance = convertBalanceToChannelCurrencyResponse.RoundedConvertedAmount,
                    CardCurrencyCode = card.CardCurrencyCode,
                    BalanceInCardCurrency = card.BalanceInCardCurrency
                };
    
                return new GetGiftCardServiceResponse(giftCard);
            }
    
            /// <summary>
            /// Issue new gift card.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>A response containing the gift card information.</returns>
            private static GetGiftCardServiceResponse IssueGiftCard(IssueGiftCardServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (string.IsNullOrWhiteSpace(request.Id))
                {
                    throw new ArgumentException("request.Id is null or empty.", "request");
                }
    
                if (request.Amount <= 0)
                {
                    throw new ArgumentException("request.Amount has to be positive value.", "request");
                }
    
                if (string.IsNullOrWhiteSpace(request.CurrencyCode))
                {
                    throw new ArgumentException("request.Id is null or empty.", "request");
                }
    
                if (!request.CurrencyCode.Equals(request.RequestContext.GetChannelConfiguration().Currency, StringComparison.OrdinalIgnoreCase))
                {
                    throw new PaymentException(
                        PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_GiftCardCurrencyMismatch,
                        string.Format("Gift card cannot be issued in currency different from channel currency. Currency requested: {0}", request.CurrencyCode));
                }
    
                ValidateNotAlreadyAdded(request.Transaction, request.Id);
    
                var issueGiftCardRequest = new IssueGiftCardRealtimeRequest(
                    request.Id,
                    request.Amount,
                    request.CurrencyCode,
                    request.RequestContext.GetPrincipal().ChannelId,
                    request.RequestContext.GetTerminal() == null ? string.Empty : request.RequestContext.GetTerminal().TerminalId,
                    request.RequestContext.GetPrincipal().UserId,
                    request.Transaction.Id,
                    string.Empty); // ReceiptId is passed as an empty string in EPOS too at this point.
    
                GiftCard issuedGiftCard = request.RequestContext.Execute<SingleEntityDataServiceResponse<GiftCard>>(issueGiftCardRequest).Entity;
    
                return new GetGiftCardServiceResponse(issuedGiftCard);
            }
    
            /// <summary>
            /// Unlock the gift card.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>
            /// A response containing the gift card.
            /// </returns>
            private static NullResponse UnlockGiftCard(UnlockGiftCardServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                var unlockRequest = new UnlockGiftCardRealtimeRequest(request.GiftCardId);
                request.RequestContext.Execute<NullResponse>(unlockRequest);
    
                return new NullResponse();
            }
    
            /// <summary>
            /// Add balance to existing gift card.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>A response containing the gift card information.</returns>
            private static GetGiftCardServiceResponse AddToGiftCard(AddToGiftCardServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (string.IsNullOrWhiteSpace(request.GiftCardId))
                {
                    throw new ArgumentException("request.Id is null or empty.", "request");
                }
    
                if (string.IsNullOrWhiteSpace(request.CurrencyCode))
                {
                    throw new ArgumentException("request.Id is null or empty.", "request");
                }
    
                if (!request.CurrencyCode.Equals(request.RequestContext.GetChannelConfiguration().Currency, StringComparison.OrdinalIgnoreCase))
                {
                    throw new PaymentException(
                        PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_GiftCardCurrencyMismatch,
                        string.Format("Amount cannot be added to gift in currency different from channel currency. Currency requested: {0}", request.CurrencyCode));
                }
    
                // Skip validation if we are reversing operation.
                if (request.Amount > 0)
                {
                    ValidateNotAlreadyAdded(request.Transaction, request.GiftCardId);
                }
    
                string giftCardCurrencyCode;
                GiftCard currentGiftCard;
    
                if (request.IsReversal)
                {
                    // In case of reversal (voiding gift card sales line) gift card we already know gift card currency
                    // so call to GetGiftCardBalance() can be skipped.
                    giftCardCurrencyCode = request.CardCurrencyCode;
                }
                else
                {
                    var getGiftCardBalanceRequest = new GetGiftCardRealtimeRequest(request.GiftCardId);
                    currentGiftCard = request.RequestContext.Execute<SingleEntityDataServiceResponse<GiftCard>>(getGiftCardBalanceRequest).Entity;
                    giftCardCurrencyCode = currentGiftCard.CardCurrencyCode;
                }
    
                GetCurrencyValueServiceResponse convertAmountToGiftCardCurrencyResponse = ToGiftCardCurrency(request.RequestContext, request.Amount, giftCardCurrencyCode);
    
                var addToGiftCardRequest = new AddToGiftCardRealtimeRequest(
                    request.GiftCardId,
                    convertAmountToGiftCardCurrencyResponse.RoundedConvertedAmount,
                    convertAmountToGiftCardCurrencyResponse.ToCurrencyCode,
                    request.RequestContext.GetPrincipal().ChannelId,
                    request.RequestContext.GetTerminal() == null ? string.Empty : request.RequestContext.GetTerminal().TerminalId,
                    request.RequestContext.GetPrincipal().UserId,
                    request.Transaction.Id,
                    string.Empty); // ReceiptId is passed as an empty string in EPOS too at this point.
                GiftCard card = request.RequestContext.Execute<SingleEntityDataServiceResponse<GiftCard>>(addToGiftCardRequest).Entity;
    
                GetCurrencyValueServiceResponse convertBalanceToChannelCurrencyResponse = FromGiftCardCurrency(request.RequestContext, card.BalanceInCardCurrency, card.BalanceCurrencyCode);
    
                GiftCard giftCard = new GiftCard
                {
                    Id = request.GiftCardId,
                    BalanceCurrencyCode = convertBalanceToChannelCurrencyResponse.ToCurrencyCode,
                    Balance = convertBalanceToChannelCurrencyResponse.RoundedConvertedAmount,
                    CardCurrencyCode = giftCardCurrencyCode,
                    BalanceInCardCurrency = card.BalanceInCardCurrency
                };
    
                return new GetGiftCardServiceResponse(giftCard);
            }
    
            /// <summary>
            /// Voids the gift card.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>A response.</returns>
            private static NullResponse VoidGiftCard(VoidGiftCardServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (string.IsNullOrWhiteSpace(request.GiftCardId))
                {
                    throw new ArgumentException("request.Id is null or empty.", "request");
                }
    
                var voidGiftCardRequest = new VoidGiftCardRealtimeRequest(request.GiftCardId);
                request.RequestContext.Execute<NullResponse>(voidGiftCardRequest);
    
                return new NullResponse();
            }
    
            /// <summary>
            /// Gets the change.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>
            /// A response containing the change tender line.
            /// </returns>
            private static GetChangePaymentServiceResponse GetChange(GetChangePaymentServiceRequest request)
            {
                // Change cannot be given in gift cards because it requires manual card number input.
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_ChangeTenderTypeNotSupported, string.Format("Request '{0}' is not supported. Verify change tender type settings.", request.GetType()));
            }
    
            /// <summary>
            /// Check if Gift card is already added to transaction either as sale or payment.
            /// </summary>
            /// <param name="transaction">The sales transaction.</param>
            /// <param name="giftCardId">The gift card identifier.</param>
            /// <exception cref="DataValidationException">The exception is thrown if a gift card is used for two operations in the same transaction.</exception>
            private static void ValidateNotAlreadyAdded(SalesTransaction transaction, string giftCardId)
            {
                // If the SalesTransaction is null, Cart is not create yet and so no validation is needed.
                if (transaction != null)
                {
                    // Once a Gift card is added to the transaction for any operation (Issue, AddTo, Payment), then
                    // the same card cannot be added to the transaction again for any other operation.
                    IList<SalesLine> salesLines = transaction.ActiveSalesLines;
                    IList<TenderLine> tenderLines = transaction.TenderLines;
    
                    // Check for all active sales lines where they are not voided and the gift card identifier in the sales line matches the current card identifier.
                    if (salesLines != null
                        && salesLines.Any(l => l.IsGiftCardLine && (!l.IsVoided) && (l.GiftCardId == giftCardId)))
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_NoMoreThanOneOperationWithAGiftCard, "You cannot perform two operations with the same gift card in a single transaction.");
                    }
    
                    // Check for all tender lines where the gift card identifier in the sales line matches the current card identifier.
                    if (tenderLines != null
                        && tenderLines.Any(l => ((l.Status != TenderLineStatus.Voided) && (l.GiftCardId == giftCardId))))
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_NoMoreThanOneOperationWithAGiftCard, "You cannot perform two operations with the same gift card in a single transaction.");
                    }
                }
            }
    
            /// <summary>
            /// Convert amount from gift card currency to channel currency.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <param name="amountInGiftCardCurrency">Amount to convert.</param>
            /// <param name="giftCardCurrencyCode">Gift card's currency code.</param>
            /// <returns>Combination of currency code, amount and exchange rate.</returns>
            private static GetCurrencyValueServiceResponse FromGiftCardCurrency(RequestContext context, decimal amountInGiftCardCurrency, string giftCardCurrencyCode)
            {
                if (string.IsNullOrWhiteSpace(giftCardCurrencyCode))
                {
                    throw new ArgumentException("Currency is null or empty.", giftCardCurrencyCode);
                }
    
                string channelCurrencyCode = context.GetChannelConfiguration().Currency;
    
                if (giftCardCurrencyCode.Equals(channelCurrencyCode, StringComparison.OrdinalIgnoreCase))
                {
                    // Skip currency conversion if gift card currency is same as channel currency.
                    return new GetCurrencyValueServiceResponse(giftCardCurrencyCode, channelCurrencyCode, amountInGiftCardCurrency, amountInGiftCardCurrency, amountInGiftCardCurrency, 1.0m);
                }
    
                return ConvertCurrencyAmount(context, amountInGiftCardCurrency, giftCardCurrencyCode, channelCurrencyCode);
            }
    
            /// <summary>
            /// Convert amount from channel currency to gift card currency.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <param name="amountInChannelCurrency">Amount to convert.</param>
            /// <param name="giftCardCurrencyCode">Gift card's currency code.</param>
            /// <returns>Combination of currency code, amount and exchange rate.</returns>
            private static GetCurrencyValueServiceResponse ToGiftCardCurrency(RequestContext context, decimal amountInChannelCurrency, string giftCardCurrencyCode)
            {
                if (string.IsNullOrWhiteSpace(giftCardCurrencyCode))
                {
                    throw new ArgumentException("Currency is null or empty.", giftCardCurrencyCode);
                }
    
                string channelCurrencyCode = context.GetChannelConfiguration().Currency;
    
                if (giftCardCurrencyCode.Equals(channelCurrencyCode, StringComparison.OrdinalIgnoreCase))
                {
                    // Skip currency conversion if gift card currency is same as channel currency.
                    return new GetCurrencyValueServiceResponse(channelCurrencyCode, giftCardCurrencyCode, amountInChannelCurrency, amountInChannelCurrency, amountInChannelCurrency, 1.0m);
                }
    
                return ConvertCurrencyAmount(context, amountInChannelCurrency, channelCurrencyCode, giftCardCurrencyCode);
            }
    
            /// <summary>
            /// Convert amount from one currency to another.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <param name="amountToConvert">Amount to convert.</param>
            /// <param name="fromCurrencyCode">Currency to convert from.</param>
            /// <param name="toCurrencyCode">Currency to convert to.</param>
            /// <returns>Response that contains converted amount along with exchange rate.</returns>
            private static GetCurrencyValueServiceResponse ConvertCurrencyAmount(RequestContext context, decimal amountToConvert, string fromCurrencyCode, string toCurrencyCode)
            {
                var request = new GetCurrencyValueServiceRequest(
                    fromCurrencyCode,
                    toCurrencyCode,
                    amountToConvert);
    
                GetCurrencyValueServiceResponse response = context.Execute<GetCurrencyValueServiceResponse>(request);
    
                return response;
            }
        }
    }
}
