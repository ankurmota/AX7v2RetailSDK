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
        using System.Collections.ObjectModel;
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Handlers;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Represents a gatekeeper for tender line operations.
        /// </summary>
        public class PaymentManagerService : INamedRequestHandler
        {
            /// <summary>
            /// Gets the unique name for this request handler.
            /// </summary>
            public string HandlerName
            {
                get { return "PaymentManager"; }
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
                        typeof(AuthorizePaymentServiceRequest),
                        typeof(VoidPaymentServiceRequest),
                        typeof(CapturePaymentServiceRequest),
                        typeof(GetChangePaymentServiceRequest),
                        typeof(ValidateTenderLineForAddServiceRequest)
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
    
                if (requestType == typeof(AuthorizePaymentServiceRequest))
                {
                    response = AuthorizePayment((AuthorizePaymentServiceRequest)request);
                }
                else if (requestType == typeof(VoidPaymentServiceRequest))
                {
                    response = CancelPayment((VoidPaymentServiceRequest)request);
                }
                else if (requestType == typeof(CapturePaymentServiceRequest))
                {
                    response = CapturePayment((CapturePaymentServiceRequest)request);
                }
                else if (requestType == typeof(GetChangePaymentServiceRequest))
                {
                    response = GetChange((GetChangePaymentServiceRequest)request);
                }
                else if (requestType == typeof(ValidateTenderLineForAddServiceRequest))
                {
                    response = ValidateTenderLine((ValidateTenderLineForAddServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
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
    
                TenderType tenderType = GetTenderType(request.RequestContext, request.TenderLine.TenderTypeId);
    
                // Resolve payment service.
                IRequestHandler paymentService = ResolvePaymentService(request.RequestContext, request.GetType(), request.TenderLine.TenderTypeId);
    
                // Calculate amount to be authorized (some tender type like currency and credit memo do not have amount set on tender line).
                CalculatePaymentAmountServiceRequest calculateAmountRequest = new CalculatePaymentAmountServiceRequest(request.TenderLine);
                CalculatePaymentAmountServiceResponse calculateAmountResponse = request.RequestContext.Execute<CalculatePaymentAmountServiceResponse>(calculateAmountRequest, paymentService);
                request.TenderLine = calculateAmountResponse.TenderLine;
    
                if (!request.TenderLine.IsPreProcessed)
                {
                    request.TenderLine.Amount = RoundAmountByTenderType(request.RequestContext, request.TenderLine.TenderTypeId, request.TenderLine.Amount, isChange: false);
                }
    
                // Update tender lines with amounts and exchange rates for channel and company currencies.
                CalculateTenderLineCurrencyAmounts(request.TenderLine, request.RequestContext);
    
                if (request.TenderLine.Amount == 0)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidAmount, "An amount of zero is not allowed on the tenderline.");
                }
    
                // Do amount validation.
                if (!request.SkipLimitValidation)
                {
                    var validateRequest = new ValidateTenderLineForAddServiceRequest(request.Transaction, request.TenderLine, tenderType);
                    request.RequestContext.Execute<NullResponse>(validateRequest);
                }
    
                // Check tender line status
                AuthorizePaymentServiceResponse response;
                if (request.TenderLine.Status == TenderLineStatus.PendingCommit
                    || request.TenderLine.Status == TenderLineStatus.Committed
                    || request.TenderLine.Status == TenderLineStatus.Voided)
                {
                    // Return the tender line directly if already authorized
                    response = new AuthorizePaymentServiceResponse(request.TenderLine);
                }
                else
                {
                    // Process authorization.
                    response = request.RequestContext.Execute<AuthorizePaymentServiceResponse>(request, paymentService);
                }
    
                // If we have cashback amount set on the tender line, we add it to the amount on the tender line after authorization
                // and set the cashback amount on the tender line to 0. This is because we do not cashback field in the
                // RetailTransactionPaymentTrans table. We are doing this at this point to mimic EPOS.
                if (response.TenderLine.CashBackAmount != 0)
                {
                    CalculateAmountsWithCashBack(response.TenderLine, request.RequestContext);
                }
    
                return response;
            }
    
            /// <summary>
            /// Validates the tender line.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>An empty service response when validation succeeds.</returns>
            /// <exception cref="DataValidationException">Validation failed.</exception>
            private static NullResponse ValidateTenderLine(ValidateTenderLineForAddServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (request.TenderType == null)
                {
                    request.TenderType = GetTenderType(request.RequestContext, request.TenderLine.TenderTypeId);
    
                    // Update tender lines with amounts and exchange rates for channel and company currencies.
                    CalculateTenderLineCurrencyAmounts(request.TenderLine, request.RequestContext);
                }
    
                ValidateTenderLineLimits(request.RequestContext, request.Transaction, request.TenderLine, request.TenderType);
                ValidateTransactionLimits(request.Transaction, request.TenderLine, request.TenderType);
    
                return new NullResponse();
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
    
                if (!request.TenderLine.IsVoidable)
                {
                    throw new PaymentException(
                        PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_TenderLineCannotBeVoided,
                        string.Format("Tender line {0} cannot be voided.", request.TenderLine.TenderLineId));
                }
    
                IRequestHandler paymentService = ResolvePaymentService(request.RequestContext, request.GetType(), request.TenderLine.TenderTypeId);
                return request.RequestContext.Runtime.Execute<VoidPaymentServiceResponse>(request, request.RequestContext, paymentService);
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
    
                switch (request.TenderLine.Status)
                {
                    case TenderLineStatus.PendingCommit:
                        IRequestHandler paymentService = ResolvePaymentService(request.RequestContext, request.GetType(), request.TenderLine.TenderTypeId);
                        return request.RequestContext.Runtime.Execute<CapturePaymentServiceResponse>(request, request.RequestContext, paymentService);
                    case TenderLineStatus.Committed:
                        return new CapturePaymentServiceResponse(request.TenderLine);
                    case TenderLineStatus.Historical:
                    case TenderLineStatus.Voided:
                        // for tender lines that are voided or historical do nothing and simple return same tender line.
                        return new CapturePaymentServiceResponse(request.TenderLine);
                    case TenderLineStatus.NotProcessed:
                    case TenderLineStatus.None:
                        throw new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest,
                            string.Format("Tender line in status {0} cannot be captured.", request.TenderLine.Status));
                    default:
                        throw new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest, 
                            string.Format("Tender line in status {0} is not supported.", request.TenderLine.Status));
                }
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
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                if (string.IsNullOrWhiteSpace(request.PaymentTenderTypeId))
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPaymentRequest, "request.TenderTypeId is null or empty.");
                }

                TenderType overtenderTenderType = GetTenderType(request.RequestContext, request.PaymentTenderTypeId);
                if (overtenderTenderType == null)
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound,
                        string.Format("Tender type with id '{0}' not found", request.PaymentTenderTypeId));
                }

                string changeTenderTypeId;
                if (overtenderTenderType.AboveMinimumChangeAmount != decimal.Zero 
                    && Math.Abs(request.ChangeAmount) > Math.Abs(overtenderTenderType.AboveMinimumChangeAmount)
                    && !string.IsNullOrWhiteSpace(overtenderTenderType.AboveMinimumChangeTenderTypeId))
                {
                    // Use "above minimum change tender type" if amount exceeds configured value.
                    changeTenderTypeId = overtenderTenderType.AboveMinimumChangeTenderTypeId;
                }
                else
                {
                    changeTenderTypeId = overtenderTenderType.ChangeTenderTypeId;
                }

                if (string.IsNullOrWhiteSpace(changeTenderTypeId))
                {
                    // If change tender type is not configured using tender type of last payment (ePOS behavior)
                    changeTenderTypeId = overtenderTenderType.TenderTypeId;
                }
    
                TenderType changeTenderType = GetTenderType(request.RequestContext, changeTenderTypeId);
                if (changeTenderType == null)
                {
                    // If change tender type is not configured using tender type of last payment (ePOS behavior)
                    changeTenderType = overtenderTenderType;
                }
    
                request.ChangeTenderTypeId = changeTenderType.TenderTypeId;
    
                // Round change amount.
                request.ChangeAmount = RoundAmountByTenderType(request.RequestContext, request.ChangeTenderTypeId, request.ChangeAmount, isChange: true);
    
                IRequestHandler paymentService = ResolvePaymentService(request.RequestContext, request.GetType(), request.ChangeTenderTypeId);
                GetChangePaymentServiceResponse response = request.RequestContext.Runtime.Execute<GetChangePaymentServiceResponse>(request, request.RequestContext, paymentService);
    
                // Update tender lines with amounts and exchange rates for channel and company currencies.
                CalculateTenderLineCurrencyAmounts(response.TenderLine, request.RequestContext);
    
                return response;
            }
    
            /// <summary>
            /// Gets the payment service based on the tender type.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="requestType">Request type.</param>
            /// <param name="tenderTypeId">The id of tender type.</param>
            /// <returns>Tender service.</returns>
            private static IRequestHandler ResolvePaymentService(RequestContext context, Type requestType, string tenderTypeId)
            {
                if (string.IsNullOrWhiteSpace(tenderTypeId))
                {
                    throw new ArgumentException("tenderTypeId is null or empty", "tenderTypeId");
                }
    
                TenderType tenderType = GetTenderType(context, tenderTypeId);
    
                if (tenderType == null)
                {
                    var message = string.Format(
                        CultureInfo.InvariantCulture,
                        "Failed to load tender type with id: {0}",
                        tenderTypeId);
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound, message);
                }
    
                IRequestHandler paymentHandler = context.Runtime.GetRequestHandler(requestType, tenderType.OperationId);
    
                if (paymentHandler == null)
                {
                    throw new ConfigurationException(
                        ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidProviderConfiguration,
                        string.Format("Unable to retrieve tender service for operation: {0}.", tenderType.OperationId));
                }
    
                return paymentHandler;
            }
    
            /// <summary>
            /// Get the tender type configuration by identifier.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="tenderTypeId">The id of tender type.</param>
            /// <returns>The matching tender type.</returns>
            private static TenderType GetTenderType(RequestContext context, string tenderTypeId)
            {
                var dataServiceRequest = new GetChannelTenderTypesDataRequest(context.GetPrincipal().ChannelId, QueryResultSettings.AllRecords);
                var response = context.Execute<EntityDataServiceResponse<TenderType>>(dataServiceRequest);
    
                return response.PagedEntityCollection.Results.SingleOrDefault(channelTenderType => string.Equals(channelTenderType.TenderTypeId, tenderTypeId, StringComparison.OrdinalIgnoreCase));
            }
    
            /// <summary>
            /// Validate that amount on tender line do not exceed limits defined for tender type.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">Sales transaction.</param>
            /// <param name="tenderLine">Tender line to validate.</param>
            /// <param name="tenderType">Tender type information.</param>
            private static void ValidateTenderLineLimits(RequestContext context, SalesTransaction salesTransaction, TenderLine tenderLine, TenderType tenderType)
            {
                if (salesTransaction == null)
                {
                    throw new ArgumentNullException("salesTransaction");
                }
    
                if (tenderLine == null)
                {
                    throw new ArgumentNullException("tenderLine");
                }
    
                if (tenderType == null)
                {
                    throw new ArgumentNullException("tenderType");
                }
    
                decimal amountDue = RoundAmountByTenderType(context, tenderLine.TenderTypeId, salesTransaction.AmountDue, isChange: false);
    
                var validationFailures = new Collection<DataValidationFailure>();
    
                if (tenderLine.Amount != 0 && (Math.Sign(amountDue) != Math.Sign(tenderLine.Amount)))
                {
                    validationFailures.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_IncorrectPaymentAmountSign, "tenderLine.Amount"));
                }
    
                if (tenderType.MaximumAmountPerLine > 0 && Math.Abs(tenderLine.Amount) > tenderType.MaximumAmountPerLine)
                {
                    // 1396 = The maximum amount allowed is:
                    validationFailures.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_PaymentExceedsMaximumAmountPerLine, "tenderLine.Amount"));
                }
    
                if (tenderType.MinimumAmountPerLine > 0 && Math.Abs(tenderLine.Amount) < tenderType.MinimumAmountPerLine)
                {
                    // 1397 = The minimum amount allowed is:
                    validationFailures.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_PaymentExceedsMinimumAmountPerLine, "tenderLine.Amount"));
                }

                // Card refund does not honor overtender/undertender limits.
                // Only check those limits when the tender is not card refund.
                if (!IsCardRefund(tenderType, tenderLine))
                {
                    if (Math.Abs(amountDue) < Math.Abs(tenderLine.Amount))
                    {
                        if (!tenderType.IsOvertenderAllowed)
                        {
                            // 1391 = No change allowed:
                            validationFailures.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ChangebackIsNotAllowed, "tenderLine.Amount"));
                        }

                        if (tenderType.MaximumOvertenderAmount > 0
                            && tenderType.MaximumOvertenderAmount < (Math.Abs(tenderLine.Amount) - Math.Abs(amountDue)))
                        {
                            validationFailures.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_OvertenderAmountExceedsMaximumAllowedValue, "tenderLine.Amount"));
                        }
                    }
                    else if (Math.Abs(amountDue) > Math.Abs(tenderLine.Amount))
                    {
                        if (!tenderType.IsUndertenderAllowed)
                        {
                            // 1394 = This payment must be used to finalize the transaction:
                            validationFailures.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_PaymentMustBeUsedToFinalizeTransaction, "tenderLine.Amount"));
                        }

                        if (tenderType.MaximumUndertenderAmount > 0 && (Math.Abs(amountDue) - Math.Abs(tenderLine.Amount)) > tenderType.MaximumUndertenderAmount)
                        {
                            validationFailures.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_BalanceAmountExceedsMaximumAllowedValue, "tenderLine.Amount"));
                        }

                        if (tenderLine.CashBackAmount > 0)
                        {
                            validationFailures.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_PaymentMustBeUsedToFinalizeTransaction, "tenderLine.Amount"));
                        }
                    }
                }
    
                if (tenderType.OperationType == RetailOperation.PayCard)
                {
                    validationFailures.AddRange(ValidateCardTypeLimits(context, tenderLine));
                }
    
                if (validationFailures.Any())
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_AggregateValidationError, validationFailures, "Payment amount exceeds limits defined per transaction.");
                }
            }
    
            /// <summary>
            /// Validates that tender line fulfills limits configured for card type.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="tenderLine">Payment information.</param>
            /// <returns>Collection of <see cref="DataValidationFailure"/>.</returns>
            private static IEnumerable<DataValidationFailure> ValidateCardTypeLimits(RequestContext context, TenderLine tenderLine)
            {
                List<DataValidationFailure> failures = new List<DataValidationFailure>();
    
                CardTypeInfo cardTypeInfo = CardTypeHelper.GetCardTypeConfiguration(tenderLine.CardTypeId, context);
    
                if (cardTypeInfo == null)
                {
                    failures.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound, "Card type with id '{0}' not found", tenderLine.CardTypeId));
                }
    
                if (tenderLine.CashBackAmount < 0)
                {
                    failures.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidCashBackAmount, "Cash back amount cannot be negative."));
                }
    
                if (tenderLine.CashBackAmount != 0 && tenderLine.Amount < 0)
                {
                    failures.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_AmountDueMustBePaidBeforeCheckout, "Cash back not allowed for refunds."));
                }
    
                if (cardTypeInfo != null && tenderLine.CashBackAmount > cardTypeInfo.CashBackLimit)
                {
                    string message = string.Format("Cash back amount for card type '{0}' exceed maximum allowed value {1}.", cardTypeInfo.TypeId, cardTypeInfo.CashBackLimit);
                    failures.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidCashBackAmount, message));
                }
    
                return failures;
            }
    
            /// <summary>
            /// Validate that all payments made with same tender type do not exceed limits defined for tender type.
            /// </summary>
            /// <param name="salesTransaction">Sales transaction.</param>
            /// <param name="tenderLine">Tender line to validate.</param>
            /// <param name="tenderType">Tender type information.</param>
            private static void ValidateTransactionLimits(SalesTransaction salesTransaction, TenderLine tenderLine, TenderType tenderType)
            {
                if (salesTransaction == null)
                {
                    throw new ArgumentNullException("salesTransaction");
                }
    
                if (tenderLine == null)
                {
                    throw new ArgumentNullException("tenderLine");
                }
    
                if (tenderType == null)
                {
                    throw new ArgumentNullException("tenderType");
                }
    
                decimal amountAlreadyPaidWithSameTenderType = salesTransaction.TenderLines.Where(t => t.Status != TenderLineStatus.Voided && t.TenderTypeId == tenderLine.TenderTypeId).Sum(t => t.Amount);
                decimal totalAmountPaidWithTenderType = tenderLine.Amount + amountAlreadyPaidWithSameTenderType;
    
                var validationFailures = new Collection<DataValidationFailure>();

                // Card refund does not honor MaximumAmountPerTransaction.
                // Only check the limit when the tender is not card refund.
                if (!IsCardRefund(tenderType, tenderLine))
                {
                    if (tenderType.MaximumAmountPerTransaction > 0 && Math.Abs(totalAmountPaidWithTenderType) > tenderType.MaximumAmountPerTransaction)
                    {
                        validationFailures.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_PaymentExceedsMaximumAmountPerTransaction, "tenderLine.Amount"));
                    }
                }
    
                if (validationFailures.Any())
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_AggregateValidationError, validationFailures, "Payment amount exceeds limits defined per transaction.");
                }
            }
    
            /// <summary>
            /// Update tender line amounts with cash back for company and channel currencies.
            /// </summary>
            /// <param name="tenderLine">The tender line to update.</param>
            /// <param name="context">The request context.</param>
            private static void CalculateAmountsWithCashBack(TenderLine tenderLine, RequestContext context)
            {
                tenderLine.Amount += tenderLine.CashBackAmount;
    
                // In case of cashback, tendered currency is always equal to the channel currency.
                tenderLine.AmountInTenderedCurrency += tenderLine.CashBackAmount;
    
                string companyCurrencyCode = context.GetChannelConfiguration().CompanyCurrency;
                if (!tenderLine.Currency.Equals(companyCurrencyCode, StringComparison.OrdinalIgnoreCase))
                {
                    // Convert cashback from tendered to company currency.
                    GetCurrencyValueServiceResponse cashBackConversionInfo = ConvertCurrencyAmount(
                        tenderLine.CashBackAmount,
                        tenderLine.Currency,
                        companyCurrencyCode,
                        context);
                    tenderLine.AmountInCompanyCurrency += cashBackConversionInfo.ConvertedAmount;
                }
                else
                {
                    tenderLine.AmountInCompanyCurrency += tenderLine.CashBackAmount;
                }
    
                tenderLine.CashBackAmount = 0M;
            }
    
            /// <summary>
            /// Update tender line with amounts and exchange rates for company and channel currencies.
            /// </summary>
            /// <param name="tenderLine">Tender line to update.</param>
            /// <param name="context">Request context.</param>
            private static void CalculateTenderLineCurrencyAmounts(TenderLine tenderLine, RequestContext context)
            {
                string channelCurrencyCode = context.GetChannelConfiguration().Currency;
                if (!tenderLine.Currency.Equals(channelCurrencyCode, StringComparison.OrdinalIgnoreCase))
                {
                    if (tenderLine.AmountInTenderedCurrency == 0)
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest, "Currency on tender line is different from store currency but AmountInTenderedCurrency is not specified.");
                    }
    
                    IEnumerable<string> supportedCurrencies = GetCurrencyCodeSupportedByChannel(context);
                    if (supportedCurrencies.FirstOrDefault(c => string.Equals(c, tenderLine.Currency, StringComparison.OrdinalIgnoreCase)) == null)
                    {
                        throw new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CurrencyConversionFailed,
                            string.Format("Currency code '{0}' is not supported by current channel.", tenderLine.Currency));
                    }
    
                    // Convert amount and get exchange rate from tendered currency to channel currency.
                    GetCurrencyValueServiceResponse channelConversionInfo = ConvertCurrencyAmount(
                        tenderLine.AmountInTenderedCurrency,
                        tenderLine.Currency,
                        channelCurrencyCode,
                        context);
    
                    tenderLine.ExchangeRate = channelConversionInfo.ExchangeRate;
                    tenderLine.Amount = channelConversionInfo.RoundedConvertedAmount;
    
                    // Round the amount in tendered currency.
                    tenderLine.AmountInTenderedCurrency = RoundAmountByCurrency(context, tenderLine.AmountInTenderedCurrency, tenderLine.Currency);
                }
                else
                {
                    tenderLine.ExchangeRate = 1m;
                    tenderLine.AmountInTenderedCurrency = tenderLine.Amount;
                }
    
                string companyCurrencyCode = context.GetChannelConfiguration().CompanyCurrency;
                if (!tenderLine.Currency.Equals(companyCurrencyCode, StringComparison.OrdinalIgnoreCase))
                {
                    // Convert tendered amount to company amount, and get exchange rate from tendered to company currencies.
                    GetCurrencyValueServiceResponse tenderConversionInfo = ConvertCurrencyAmount(
                        tenderLine.AmountInTenderedCurrency,
                        tenderLine.Currency,
                        companyCurrencyCode,
                        context);
                    tenderLine.CompanyCurrencyExchangeRate = tenderConversionInfo.ExchangeRate;
                    tenderLine.AmountInCompanyCurrency = tenderConversionInfo.RoundedConvertedAmount;
                }
                else
                {
                    tenderLine.CompanyCurrencyExchangeRate = 1m;
                    tenderLine.AmountInCompanyCurrency = PaymentManagerService.RoundAmountByCurrency(context, tenderLine.AmountInTenderedCurrency, tenderLine.Currency);
                }
            }
    
            /// <summary>
            /// Convert amount from one currency to another.
            /// </summary>
            /// <param name="amountInCurrency">Amount to convert.</param>
            /// <param name="fromCurrencyCode">Currency to convert from.</param>
            /// <param name="toCurrencyCode">Currency to convert to.</param>
            /// <param name="context">Request context.</param>
            /// <returns>Response that contains converted amount along with exchange rate.</returns>
            private static GetCurrencyValueServiceResponse ConvertCurrencyAmount(decimal amountInCurrency, string fromCurrencyCode, string toCurrencyCode, RequestContext context)
            {
                var request = new GetCurrencyValueServiceRequest(
                    fromCurrencyCode,
                    toCurrencyCode,
                    amountInCurrency);
    
                GetCurrencyValueServiceResponse response = context.Execute<GetCurrencyValueServiceResponse>(request);
    
                return response;
            }
    
            /// <summary>
            /// Get list of currency codes supported by current channel.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <returns>Collection of currency codes.</returns>
            private static IEnumerable<string> GetCurrencyCodeSupportedByChannel(RequestContext context)
            {
                GetChannelCurrenciesDataRequest dataRequest = new GetChannelCurrenciesDataRequest(QueryResultSettings.AllRecords);
                IEnumerable<CurrencyAmount> currencies = context.Execute<EntityDataServiceResponse<CurrencyAmount>>(dataRequest).PagedEntityCollection.Results;
                return currencies.Select(c => c.CurrencyCode);
            }
    
            /// <summary>
            /// Get the rounded amount based on the currency code.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="amount">The amount to be rounded.</param>
            /// <param name="currencyCode">The currency code.</param>
            /// <returns>The rounded value.</returns>
            private static decimal RoundAmountByCurrency(RequestContext context, decimal amount, string currencyCode)
            {
                var roundingRequest = new GetRoundedValueServiceRequest(amount, currencyCode);
                GetRoundedValueServiceResponse roundingResponse = context.Execute<GetRoundedValueServiceResponse>(roundingRequest);
                return roundingResponse.RoundedValue;
            }
    
            /// <summary>
            /// Round amount by tender type id.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="tenderTypeId">Tender type id.</param>
            /// <param name="amount">The amount.</param>
            /// <param name="isChange">Value indicating whether this request is for change.</param>
            /// <returns>Rounded amount.</returns>
            private static decimal RoundAmountByTenderType(RequestContext context, string tenderTypeId, decimal amount, bool isChange)
            {
                ThrowIf.Null(context, "context");
    
                GetPaymentRoundedValueServiceRequest request = new GetPaymentRoundedValueServiceRequest(amount, tenderTypeId, isChange);
                GetRoundedValueServiceResponse response = context.Execute<GetRoundedValueServiceResponse>(request);
    
                return response.RoundedValue;
            }

            /// <summary>
            /// Gets a value indicating whether a tender is card refund.
            /// </summary>
            /// <param name="tenderType">The tender type.</param>
            /// <param name="tenderLine">The tender line.</param>
            /// <returns>The result value.</returns>
            private static bool IsCardRefund(TenderType tenderType, TenderLine tenderLine)
            {
                return tenderType.OperationType == RetailOperation.PayCard && tenderLine.Amount < 0;
            }
        }
    }
}
