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
        using System.Text;
        using System.Xml.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Handlers;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
        using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
        using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;
        using Microsoft.Dynamics.Retail.SDKManager.Portable;
        using PaymentSDK = Microsoft.Dynamics.Retail.PaymentSDK.Portable;

        /// <summary>
        /// The payment service implementation.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Will be fixed in CTP2")]
        public class CardPaymentService : IOperationRequestHandler
        {
            /// <summary>
            /// The card number mask character.
            /// </summary>
            private const char CardNumberMaskCharacter = '*';

            /// <summary>
            /// The default length of the masked cared number. Here we don't distinguish different lengths of bank card numbers.
            /// </summary>
            private const int MaskedCardNumberLength = 16;

            /// <summary>
            /// The number of digits of bank identification number (a.k.a. BIN).
            /// </summary>
            private const int BankIdentificationNumberLength = 6;

            /// <summary>
            /// The number of digits of card suffix.
            /// </summary>
            private const int CardSuffixLength = 4;

            /// <summary>
            /// The locale.
            /// </summary>
            private static string locale;

            /// <summary>
            /// The country region mapper.
            /// </summary>
            private static CountryRegionMapper countryRegionMapper;

            /// <summary>
            /// Gets a collection of operation identifiers supported by this request handler.
            /// </summary>
            public IEnumerable<int> SupportedOperationIds
            {
                get
                {
                    return new[]
                    {
                    (int)RetailOperation.PayCard
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
                    typeof(AuthorizeTokenizedCardPaymentServiceRequest),
                    typeof(VoidPaymentServiceRequest),
                    typeof(CapturePaymentServiceRequest),
                    typeof(SupportedCardTypesPaymentServiceRequest),
                    typeof(GetCardPaymentPropertiesServiceRequest),
                    typeof(GetChangePaymentServiceRequest),
                    typeof(GetCardPaymentAcceptPointServiceRequest),
                    typeof(RetrieveCardPaymentAcceptResultServiceRequest)
                };
                }
            }

            /// <summary>
            /// Entry point to Payment service. Takes a Payment service request and returns the result
            /// of the request execution.
            /// </summary>
            /// <param name="request">The Currency service request to execute.</param>
            /// <returns>Result of executing request, or null object for void operations.</returns>
            public Microsoft.Dynamics.Commerce.Runtime.Messages.Response Execute(Microsoft.Dynamics.Commerce.Runtime.Messages.Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                if (countryRegionMapper == null)
                {
                    countryRegionMapper = new CountryRegionMapper(request.RequestContext);
                }

                Type requestType = request.GetType();
                Microsoft.Dynamics.Commerce.Runtime.Messages.Response response;

                if (requestType == typeof(CalculatePaymentAmountServiceRequest))
                {
                    response = CalculatePaymentAmount((CalculatePaymentAmountServiceRequest)request);
                }
                else if (requestType == typeof(GetChangePaymentServiceRequest))
                {
                    response = GetChange((GetChangePaymentServiceRequest)request);
                }
                else if (requestType == typeof(AuthorizePaymentServiceRequest))
                {
                    response = AuthorizePayment((AuthorizePaymentServiceRequest)request);
                }
                else if (requestType == typeof(AuthorizeTokenizedCardPaymentServiceRequest))
                {
                    response = AuthorizeTokenizedCardPayment((AuthorizeTokenizedCardPaymentServiceRequest)request);
                }
                else if (requestType == typeof(VoidPaymentServiceRequest))
                {
                    response = CancelPayment((VoidPaymentServiceRequest)request);
                }
                else if (requestType == typeof(CapturePaymentServiceRequest))
                {
                    response = CapturePayment((CapturePaymentServiceRequest)request);
                }
                else if (requestType == typeof(SupportedCardTypesPaymentServiceRequest))
                {
                    response = SupportedCardTypes((SupportedCardTypesPaymentServiceRequest)request);
                }
                else if (requestType == typeof(GetCardPaymentPropertiesServiceRequest))
                {
                    response = GetAuthorizationPropertiesForReceipt((GetCardPaymentPropertiesServiceRequest)request);
                }
                else if (requestType == typeof(GetCardPaymentAcceptPointServiceRequest))
                {
                    response = GetCardPaymentAcceptPoint((GetCardPaymentAcceptPointServiceRequest)request);
                }
                else if (requestType == typeof(RetrieveCardPaymentAcceptResultServiceRequest))
                {
                    response = RetrieveCardPaymentAcceptResult((RetrieveCardPaymentAcceptResultServiceRequest)request);
                }
                else
                {
                    RetailLogger.Log.CrtServicesUnsupportedRequestType(request.GetType(), "CardPaymentService");
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }

                NetTracer.Information("Completed Payment.Execute");
                return response;
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
            /// Calculate amount to do be paid.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>A response containing the updated tender line.</returns>
            private static GetCardPaymentPropertiesServiceResponse GetAuthorizationPropertiesForReceipt(GetCardPaymentPropertiesServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                string portablePaymentPropertyXmlBlob = string.Empty;
                string accountType = null;
                string approvalCode = null;
                string connectorName = null;
                string providerTransactionId = null;
                string infoMessage = null;
                string externalReceipt = string.Empty;
                string eftTerminalId = null;
                decimal availableBalance = 0;

                string authorizationXml = request.AuthorizationXml;
                if (!request.AuthorizationXml.StartsWith("<![CDATA[", StringComparison.OrdinalIgnoreCase))
                {
                    authorizationXml = request.AuthorizationXml.Substring(36);
                }
                else
                {
                    authorizationXml = ExtractAuthorizationXml(request.AuthorizationXml);
                }

                PaymentProperty[] properties = PaymentProperty.ConvertXMLToPropertyArray(authorizationXml);

                Hashtable propertyTable = PaymentProperty.ConvertToHashtable(properties);

                PaymentProperty property = PaymentProperty.GetPropertyFromHashtable(propertyTable, GenericNamespace.PaymentCard, PaymentCardProperties.AccountType);

                if (property != null)
                {
                    accountType = property.StringValue ?? string.Empty;
                }

                property = PaymentProperty.GetPropertyFromHashtable(propertyTable, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AvailableBalance);

                if (property != null)
                {
                    availableBalance = property.DecimalValue;
                }

                property = PaymentProperty.GetPropertyFromHashtable(propertyTable, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ApprovalCode);

                // if failed to retrieve property from namespace AuthorizationResponse (maps to purchasing), then try to retrieve from namespace RefundResponse (maps to refund).
                if (property == null)
                {
                    property = PaymentProperty.GetPropertyFromHashtable(propertyTable, GenericNamespace.RefundResponse, AuthorizationResponseProperties.ApprovalCode);
                }

                if (property != null)
                {
                    approvalCode = property.StringValue ?? string.Empty;
                }

                property = PaymentProperty.GetPropertyFromHashtable(propertyTable, GenericNamespace.Connector, ConnectorProperties.ConnectorName);

                if (property != null)
                {
                    connectorName = property.StringValue ?? string.Empty;
                }

                property = PaymentProperty.GetPropertyFromHashtable(propertyTable, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ProviderTransactionId);

                if (property != null)
                {
                    providerTransactionId = property.StringValue ?? string.Empty;
                }

                property = PaymentProperty.GetPropertyFromHashtable(propertyTable, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AuthorizationResult);

                if (property != null)
                {
                    if (property.StringValue.Equals(AuthorizationResult.Success.ToString(), StringComparison.OrdinalIgnoreCase)
                            || property.StringValue.Equals(AuthorizationResult.PartialAuthorization.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        // send APPROVED text if the authorization is approved.
                        infoMessage = "<T:string_6156>";
                    }
                    else
                    {
                        // send DECLINED text if the authorization is declined.
                        infoMessage = "<T:string_6157>";
                    }
                }

                property = PaymentProperty.GetPropertyFromHashtable(propertyTable, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ExternalReceipt);

                if (property != null)
                {
                    externalReceipt = property.StringValue ?? string.Empty;
                }

                property = PaymentProperty.GetPropertyFromHashtable(propertyTable, GenericNamespace.AuthorizationResponse, TransactionDataProperties.TerminalId);

                if (property != null)
                {
                    eftTerminalId = property.StringValue ?? string.Empty;
                }

                GetCardPaymentPropertiesServiceResponse response = new GetCardPaymentPropertiesServiceResponse(accountType, approvalCode, connectorName, providerTransactionId, availableBalance, infoMessage, eftTerminalId, portablePaymentPropertyXmlBlob: portablePaymentPropertyXmlBlob, externalReceipt: externalReceipt);
                return response;
            }

            /// <summary>
            /// Gets the accepting point of card payment.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response containing the accepting point.</returns>
            private static GetCardPaymentAcceptPointServiceResponse GetCardPaymentAcceptPoint(GetCardPaymentAcceptPointServiceRequest request)
            {
                NetTracer.Information("Calling Payment.GetCardPaymentAcceptPoint");

                if (request.CardPaymentAcceptSettings == null)
                {
                    throw new ArgumentException("request.CardPaymentAcceptSettings cannot be null.");
                }

                if (!request.CardPaymentAcceptSettings.CardTokenizationEnabled && !request.CardPaymentAcceptSettings.CardPaymentEnabled)
                {
                    throw new ArgumentException("The payment must have tokenization and/or payment enabled.");
                }

                var properties = new List<PaymentProperty>();

                // Find merchant properties
                RequestContext context = request.RequestContext;
                ChannelConfiguration channelConfiguration = context.GetChannelConfiguration();
                string channelCurrency = channelConfiguration.Currency;
                PaymentConnectorConfiguration paymentConnectorConfig;

                paymentConnectorConfig = PickPaymentConnectorConfiguration(context, channelCurrency, requestCardType: null);

                PaymentProperty[] merchantProperties = PaymentProperty.ConvertXMLToPropertyArray(paymentConnectorConfig.ConnectorProperties);
                properties.AddRange(merchantProperties);

                // Find card data properties
                PaymentProperty property;

                if (request.DefaultAddress != null)
                {
                    property = new PaymentProperty(
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.ShowSameAsShippingAddress,
                        request.ShowSameAsShippingAddress.ToString());
                    properties.Add(property);

                    property = new PaymentProperty(
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.StreetAddress,
                        request.DefaultAddress.Street);
                    properties.Add(property);

                    property = new PaymentProperty(
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.City,
                        request.DefaultAddress.City);
                    properties.Add(property);

                    property = new PaymentProperty(
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.State,
                        request.DefaultAddress.State);
                    properties.Add(property);

                    property = new PaymentProperty(
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.PostalCode,
                        request.DefaultAddress.ZipCode);
                    properties.Add(property);

                    // If two letter ISO region name is available use that, else map three letter code to two letter code.
                    property = CardPaymentService.GetCountryProperty(string.IsNullOrWhiteSpace(request.DefaultAddress.TwoLetterISORegionName) ? request.DefaultAddress.ThreeLetterISORegionName : request.DefaultAddress.TwoLetterISORegionName);
                    properties.Add(property);
                }

                // Find transaction data properties
                PaymentProperty testModeProperty = new PaymentProperty(
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.IsTestMode,
                    paymentConnectorConfig.IsTestMode.ToString());
                properties.Add(testModeProperty);

                Microsoft.Dynamics.Retail.PaymentSDK.Portable.TransactionType transactionType = Microsoft.Dynamics.Retail.PaymentSDK.Portable.TransactionType.None;
                bool supportCardTokenization = false;
                if (request.CardPaymentAcceptSettings.CardPaymentEnabled)
                {
                    if (channelConfiguration.ChannelType == RetailChannelType.RetailStore)
                    {
                        // Tokenize + Authorize + Capture:
                        // Used for cash-and-carry and order deposit.
                        // The payment will be authorize and captured.
                        // The card token will be returned in case to revert capture (refund).
                        transactionType = PaymentSDK.TransactionType.Capture;
                        supportCardTokenization = true;
                    }
                    else if (channelConfiguration.ChannelType == RetailChannelType.SharePointOnlineStore)
                    {
                        // Tokenize + Authorize:
                        // Can be used for ecommerce.
                        transactionType = PaymentSDK.TransactionType.Authorize;
                        supportCardTokenization = true;
                    }
                }
                else if (request.CardPaymentAcceptSettings.CardTokenizationEnabled)
                {
                    // Tokenize only, used for order fulfillment or ecommerce.
                    supportCardTokenization = true;
                }

                property = new PaymentProperty(
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.TransactionType,
                    transactionType.ToString());
                properties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.SupportCardTokenization,
                    supportCardTokenization.ToString());
                properties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.HostPageOrigin,
                    request.CardPaymentAcceptSettings.HostPageOrigin);
                properties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.PaymentAcceptAdaptorPath,
                    request.CardPaymentAcceptSettings.AdaptorPath);
                properties.Add(property);

                property = new PaymentProperty(
                        GenericNamespace.TransactionData,
                        TransactionDataProperties.IndustryType,
                        GetIndustryType(channelConfiguration.ChannelType).ToString());
                properties.Add(property);

                property = new PaymentProperty(
                        GenericNamespace.TransactionData,
                        TransactionDataProperties.SupportCardSwipe,
                        (channelConfiguration.ChannelType == RetailChannelType.RetailStore).ToString());
                properties.Add(property);

                if (request.CardPaymentAcceptSettings.CardPaymentEnabled)
                {
                    property = new PaymentProperty(
                        GenericNamespace.TransactionData,
                        TransactionDataProperties.AllowPartialAuthorization,
                        (channelConfiguration.ChannelType == RetailChannelType.RetailStore).ToString());
                    properties.Add(property);

                    property = new PaymentProperty(
                        GenericNamespace.TransactionData,
                        TransactionDataProperties.AllowVoiceAuthorization,
                        (channelConfiguration.ChannelType == RetailChannelType.RetailStore).ToString());
                    properties.Add(property);

                    property = new PaymentProperty(
                        GenericNamespace.TransactionData,
                        TransactionDataProperties.Amount,
                        request.CardPaymentAcceptSettings.PaymentAmount);
                    properties.Add(property);

                    property = new PaymentProperty(
                        GenericNamespace.TransactionData,
                        TransactionDataProperties.CurrencyCode,
                        channelCurrency);
                    properties.Add(property);
                }

                // Call payment processor to get accepting point of card payment
                IPaymentProcessor processor = CardPaymentService.GetPaymentProcessor(paymentConnectorConfig.Name);
                Request paymentRequest = new Request
                {
                    Locale = locale ?? GetLocale(request.RequestContext),
                    Properties = properties.ToArray()
                };
                Response paymentResponse = processor.GetPaymentAcceptPoint(paymentRequest);
                NetTracer.Information("Completed Payment.GetCardPaymentAcceptPoint");

                // Return service response
                string operationName = "GetCardPaymentAcceptPoint";
                var error = PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToGetCardPaymentAcceptPoint;
                if (paymentResponse == null)
                {
                    // throw generic exception if operation failed but no errors returned.
                    throw new PaymentException(error, string.Format("{0} failed.", operationName));
                }

                if (paymentResponse.Errors != null && paymentResponse.Errors.Any())
                {
                    VerifyResponseErrors(operationName, paymentResponse, error);
                }

                Hashtable responseProperties = PaymentProperty.ConvertToHashtable(paymentResponse.Properties);

                string paymentAcceptUrl;
                PaymentProperty.GetPropertyValue(
                    responseProperties,
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.PaymentAcceptUrl,
                    out paymentAcceptUrl);

                if (string.IsNullOrEmpty(paymentAcceptUrl))
                {
                    // throw generic exception if operation failed but no errors returned.
                    throw new PaymentException(error, string.Format("{0} failed.", operationName));
                }

                string paymentAcceptSubmitUrl;
                PaymentProperty.GetPropertyValue(
                    responseProperties,
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.PaymentAcceptSubmitUrl,
                    out paymentAcceptSubmitUrl);

                string paymentAcceptMessageOrigin;
                PaymentProperty.GetPropertyValue(
                    responseProperties,
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.PaymentAcceptMessageOrigin,
                    out paymentAcceptMessageOrigin);

                if (string.IsNullOrEmpty(paymentAcceptMessageOrigin))
                {
                    paymentAcceptMessageOrigin = new System.Uri(paymentAcceptUrl).GetComponents(UriComponents.SchemeAndServer, UriFormat.UriEscaped);
                }

                var cardPaymentAcceptPoint = new CardPaymentAcceptPoint();
                cardPaymentAcceptPoint.AcceptPageUrl = paymentAcceptUrl;
                cardPaymentAcceptPoint.AcceptPageSubmitUrl = paymentAcceptSubmitUrl;
                cardPaymentAcceptPoint.MessageOrigin = paymentAcceptMessageOrigin;
                return new GetCardPaymentAcceptPointServiceResponse(cardPaymentAcceptPoint);
            }

            /// <summary>
            /// Gets the payment industry type from the channel type.
            /// </summary>
            /// <param name="retailChannelType">The channel type.</param>
            /// <returns>The industry type.</returns>
            private static IndustryType GetIndustryType(RetailChannelType retailChannelType)
            {
                IndustryType industryType = IndustryType.Retail;
                switch (retailChannelType)
                {
                    case RetailChannelType.RetailStore:
                        industryType = IndustryType.Retail;
                        break;
                    case RetailChannelType.OnlineMarketplace:
                    case RetailChannelType.OnlineStore:
                    case RetailChannelType.SharePointOnlineStore:
                        industryType = IndustryType.Ecommerce;
                        break;
                    default:
                        break;
                }

                return industryType;
            }

            /// <summary>
            /// Captures the payment.
            /// </summary>
            /// <param name="request">The payment capture request.</param>
            /// <returns>Response to capture payment request. </returns>
            private static CapturePaymentServiceResponse CapturePayment(CapturePaymentServiceRequest request)
            {
                NetTracer.Information("Calling Payment.CapturePayment");

                if (request.TenderLine.IsPreProcessed)
                {
                    request.TenderLine.Status = TenderLineStatus.Committed;
                    return new CapturePaymentServiceResponse(request.TenderLine);
                }

                Request paymentRequest = new Request
                {
                    Locale = locale ?? GetLocale(request.RequestContext),
                    Properties = GetPaymentPropertiesForCapture(request.TenderLine).ToArray()
                };

                if (paymentRequest.Properties == null
                    || !paymentRequest.Properties.Any())
                {
                    RetailLogger.Log.CrtServicesCardPaymentServiceCapturePaymentRequestInvalid(request.ToString());
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "Invalid request.");
                }

                PaymentProperty[] updatedPaymentRequestProperties = paymentRequest.Properties;

                string connectorName;
                updatedPaymentRequestProperties = AddConnectorPropertiesByServiceAccountId(request.RequestContext, updatedPaymentRequestProperties, out connectorName);

                paymentRequest.Properties = updatedPaymentRequestProperties;

                // Get payment processor
                IPaymentProcessor processor = CardPaymentService.GetPaymentProcessor(connectorName);

                // Run Capture
                Response response = processor.Capture(paymentRequest);
                VerifyResponseResult("Capture", response, GenericNamespace.CaptureResponse, CaptureResponseProperties.CaptureResult, new List<string>() { CaptureResult.Success.ToString() }, PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToCapturePayment);

                NetTracer.Information("Completed Payment.Capture");
                return new CapturePaymentServiceResponse(GetTenderLineFromCaptureResponse(request));
            }

            /// <summary>
            /// Authorizes the tokenized card payment.
            /// </summary>
            /// <param name="authorizationRequest">The authorization request.</param>
            /// <returns>
            /// A payment authorization response.
            /// </returns>
            /// <exception cref="DataValidationException">Throw if card token has an invalid format.</exception>
            private static AuthorizePaymentServiceResponse AuthorizeTokenizedCardPayment(AuthorizeTokenizedCardPaymentServiceRequest authorizationRequest)
            {
                NetTracer.Information("Calling Payment.TokenenizedCardPaymentAuthorize");
                string connectorName;
                PaymentProperty[] paymentRequestProperties = null;

                string cardTypeId = authorizationRequest.TenderLine.CardTypeId ?? authorizationRequest.TokenizedPaymentCard.CardTypeId;
                CardTypeInfo cardTypeConfiguration = CardTypeHelper.GetCardTypeConfiguration(cardTypeId, authorizationRequest.RequestContext);
                if (!authorizationRequest.SkipLimitValidation)
                {
                    ValidateCardEntry(authorizationRequest.TokenizedPaymentCard, cardTypeConfiguration);
                }

                ValidateTokenInfo(authorizationRequest.TokenizedPaymentCard.CardTokenInfo);

                CalculateTenderLineCurrencyAmounts(authorizationRequest.TenderLine, authorizationRequest.RequestContext);

                paymentRequestProperties = GetPaymentPropertiesForAuthorizationWithToken(authorizationRequest.TenderLine, authorizationRequest.TokenizedPaymentCard, authorizationRequest.AllowPartialAuthorization);

                paymentRequestProperties = AddConnectorPropertiesByServiceAccountId(authorizationRequest.RequestContext, paymentRequestProperties, out connectorName);

                AuthorizePaymentServiceResponse authorizeResponse = AuthorizePayment(
                    authorizationRequest.RequestContext,
                    authorizationRequest.TenderLine,
                    connectorName,
                    paymentRequestProperties);

                return authorizeResponse;
            }

            /// <summary>
            /// Authorizes the payment.
            /// </summary>
            /// <param name="authorizationRequest">The authorization request.</param>
            /// <returns>A payment authorization response.</returns>
            private static AuthorizePaymentServiceResponse AuthorizePayment(AuthorizePaymentServiceRequest authorizationRequest)
            {
                NetTracer.Information("Calling Payment.Authorize");

                if (authorizationRequest.TenderLine.IsPreProcessed)
                {
                    return new AuthorizePaymentServiceResponse(authorizationRequest.TenderLine);
                }
                else
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "Invalid request.");
                }
            }

            /// <summary>
            /// Authorizes payment.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="tenderLine">The tender line.</param>
            /// <param name="connectorName">Name of the connector.</param>
            /// <param name="paymentRequestProperties">The payment request properties.</param>
            /// <returns>
            /// The <see cref="AuthorizePaymentServiceResponse"/>.
            /// </returns>
            private static AuthorizePaymentServiceResponse AuthorizePayment(RequestContext context, TenderLine tenderLine, string connectorName, PaymentProperty[] paymentRequestProperties)
            {
                Request paymentRequest = new Request { Locale = locale ?? GetLocale(context) };
                paymentRequest.Properties = paymentRequestProperties;

                // Get payment processor
                IPaymentProcessor processor = CardPaymentService.GetPaymentProcessor(connectorName);
                Response response;

                if (tenderLine.Amount >= 0)
                {
                    // Run authorize
                    response = processor.Authorize(paymentRequest, null);
                    VerifyResponseResult("Authorization", response, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AuthorizationResult, new List<string>() { AuthorizationResult.Success.ToString(), AuthorizationResult.PartialAuthorization.ToString() }, PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToAuthorizePayment);

                    NetTracer.Information("Completed Payment.Authorize");
                    return new AuthorizePaymentServiceResponse(GetTenderLineFromAuthorizationResponse(tenderLine, response.Properties));
                }
                else
                {
                    response = processor.Refund(paymentRequest, null);
                    VerifyResponseResult("Refund", response, GenericNamespace.RefundResponse, RefundResponseProperties.RefundResult, new List<string>() { RefundResult.Success.ToString() }, PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToRefundPayment);
                    NetTracer.Information("Completed Payment.Refund");
                    return new AuthorizePaymentServiceResponse(GetTenderLineFromRefundResponse(tenderLine, response.Properties));
                }
            }

            /// <summary>
            /// Validates the token information.
            /// </summary>
            /// <param name="cardTokenInfo">The card token information.</param>
            /// <exception cref="DataValidationException">Throw if any of the crucial properties on the card token info object are not set.</exception>
            private static void ValidateTokenInfo(CardTokenInfo cardTokenInfo)
            {
                string validationErrorMessage = string.Empty;
                bool didValidationFail = false;

                if (string.IsNullOrEmpty(cardTokenInfo.ServiceAccountId))
                {
                    validationErrorMessage += "A non-empty payment service account identifier, that was used to create the credit card token, must be specified for payment processing to proceed.";
                    didValidationFail = true;
                }

                if (string.IsNullOrEmpty(cardTokenInfo.CardToken))
                {
                    validationErrorMessage += "The CardToken field must not be empty.";
                    didValidationFail = true;
                }

                if (string.IsNullOrEmpty(cardTokenInfo.UniqueCardId))
                {
                    validationErrorMessage += "The UniqueCardId field must not be empty.";
                    didValidationFail = true;
                }

                if (string.IsNullOrEmpty(cardTokenInfo.MaskedCardNumber))
                {
                    validationErrorMessage += "The MaskedCardNumber field must not be empty.";
                    didValidationFail = true;
                }

                if (didValidationFail)
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToAuthorizePayment, validationErrorMessage);
                }
            }

            /// <summary>
            /// Verifies response result.
            /// </summary>
            /// <param name="operationName">
            /// The operation name.
            /// </param>
            /// <param name="response">
            /// The response.
            /// </param>
            /// <param name="responseNamespace">
            /// The response namespace.
            /// </param>
            /// <param name="resultProperty">
            /// The result property.
            /// </param>
            /// <param name="expectedValues">
            /// A list of expected values.
            /// </param>
            /// <param name="paymentError">
            /// The <see cref="PaymentErrors"/> enumeration.
            /// </param>
            private static void VerifyResponseResult(string operationName, Response response, string responseNamespace, string resultProperty, List<string> expectedValues, PaymentErrors paymentError)
            {
                PaymentProperty property = null;

                // Payment connector might of returned no properties
                if (response.Properties != null)
                {
                    PaymentSDK.Hashtable hashTable = PaymentProperty.ConvertToHashtable(response.Properties);
                    property = PaymentProperty.GetPropertyFromHashtable(
                        hashTable,
                        responseNamespace,
                        resultProperty);
                }

                if (property == null || !expectedValues.Where(s => s.Equals(property.StringValue, StringComparison.OrdinalIgnoreCase)).Any())
                {
                    VerifyResponseErrors(operationName, response, paymentError);

                    // throw generic exception if operation failed but no errors returned.
                    throw new PaymentException(paymentError, string.Format("{0} failed.", operationName));
                }
            }

            /// <summary>
            /// Converts to Payment SDK errors.
            /// </summary>
            /// <param name="errors">The payment errors.</param>
            /// <returns>The list of Payment SDK errors.</returns>
            private static IList<Microsoft.Dynamics.Commerce.Runtime.DataModel.PaymentError> ConvertToPaymentSdkErrors(PaymentSDK.PaymentError[] errors)
            {
                IList<Microsoft.Dynamics.Commerce.Runtime.DataModel.PaymentError> paymentSdkErrors = errors != null && errors.Any()
                    ? errors.Select(error => new Microsoft.Dynamics.Commerce.Runtime.DataModel.PaymentError() { Code = error.Code.ToString(), Message = error.Message }).ToList()
                    : new List<Microsoft.Dynamics.Commerce.Runtime.DataModel.PaymentError>();

                return paymentSdkErrors;
            }

            /// <summary>
            /// Verifies response errors.
            /// </summary>
            /// <param name="operationName">
            /// The operation name.
            /// </param>
            /// <param name="response">
            /// The response.
            /// </param>
            /// <param name="paymentError">
            /// The <see cref="PaymentErrors"/> enumeration.
            /// </param>
            private static void VerifyResponseErrors(string operationName, Response response, PaymentErrors paymentError)
            {
                StringBuilder errorsInfo = new StringBuilder("Error returned from the payment connector. See error details below: ");

                if (response.Errors == null)
                {
                    return;
                }

                IList<Microsoft.Dynamics.Commerce.Runtime.DataModel.PaymentError> paymentSdkErrors = ConvertToPaymentSdkErrors(response.Errors);
                errorsInfo.Append(string.Join(";", paymentSdkErrors.Select(error => string.Format("{0}, {1}", error.Code, error.Message)).ToArray()));

                string exceptionMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} failed. Error(s): {1}",
                    operationName,
                    errorsInfo);
                var exception = new PaymentException(paymentError, paymentSdkErrors, exceptionMessage);

                throw exception;
            }

            /// <summary>
            /// Cancels the payment.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>Payment cancellation response.</returns>
            /// <exception cref="DataValidationException">Invalid request.</exception>
            private static VoidPaymentServiceResponse CancelPayment(VoidPaymentServiceRequest request)
            {
                NetTracer.Information("Calling Payment.Cancel");

                if (request.TenderLine.IsPreProcessed)
                {
                    // For preprocessed payments, there is no extra step needed for cancelling the payment.
                    request.TenderLine.Status = TenderLineStatus.Voided;
                    request.TenderLine.IsVoidable = false;
                    return new VoidPaymentServiceResponse(request.TenderLine);
                }

                if (request.TenderLine.Status != TenderLineStatus.Committed)
                {
                    VoidPendingPayment(request);
                }
                else
                {
                    RefundCommittedPayment(request);
                }

                NetTracer.Information("Completed Payment.Cancel");
                return new VoidPaymentServiceResponse(GetTenderLineForVoid(request));
            }

            /// <summary>
            /// Voids a pending authorization.
            /// </summary>
            /// <param name="request">The request.</param>
            private static void VoidPendingPayment(VoidPaymentServiceRequest request)
            {
                Request paymentRequest = new Request
                {
                    Locale = locale ?? GetLocale(request.RequestContext),
                    Properties = GetPaymentPropertiesForCancellation(request).ToArray()
                };

                if (paymentRequest.Properties == null || !paymentRequest.Properties.Any())
                {
                    RetailLogger.Log.CrtServicesCardPaymentServiceCancelPaymentRequestInvalid(request.ToString());
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "Invalid request.");
                }

                PaymentProperty[] updatedPaymentRequestProperties = paymentRequest.Properties;

                string connectorName;
                updatedPaymentRequestProperties = AddConnectorPropertiesByServiceAccountId(request.RequestContext, updatedPaymentRequestProperties, out connectorName);

                paymentRequest.Properties = updatedPaymentRequestProperties;

                // Get payment processor
                IPaymentProcessor processor = CardPaymentService.GetPaymentProcessor(connectorName);

                // Run void
                Response response = processor.Void(paymentRequest);
                VerifyResponseResult("Void", response, GenericNamespace.VoidResponse, VoidResponseProperties.VoidResult, new List<string>() { VoidResult.Success.ToString() }, PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToCancelPayment);
            }

            /// <summary>
            /// Refunds a captured authorization.
            /// </summary>
            /// <param name="request">The request.</param>
            private static void RefundCommittedPayment(VoidPaymentServiceRequest request)
            {
                // Get payment properties from the previous response.
                string authorizationXml = ExtractAuthorizationXml(request.TenderLine.Authorization);
                PaymentProperty[] previousResponseProperties = PaymentProperty.ConvertXMLToPropertyArray(authorizationXml);
                List<PaymentProperty> paymentRequestPropertyList = new List<PaymentProperty>();
                paymentRequestPropertyList.AddRange(previousResponseProperties);

                // Add refund transaction data
                TenderLine tenderLine = request.TenderLine;
                ChannelConfiguration channelConfiguration = request.RequestContext.GetChannelConfiguration();
                PaymentProperty paymentProperty = new PaymentProperty(
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.IndustryType,
                    GetIndustryType(channelConfiguration.ChannelType).ToString());
                paymentRequestPropertyList.Add(paymentProperty);

                paymentProperty = new PaymentProperty(
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.Amount,
                    Math.Abs(tenderLine.Amount)); // for refunds request amount must be positive
                paymentRequestPropertyList.Add(paymentProperty);

                paymentProperty = new PaymentProperty(
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.CurrencyCode,
                    tenderLine.Currency);
                paymentRequestPropertyList.Add(paymentProperty);

                paymentProperty = new PaymentProperty(
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.SupportCardTokenization,
                    false.ToString());
                paymentRequestPropertyList.Add(paymentProperty);

                paymentProperty = new PaymentProperty(
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.AllowPartialAuthorization,
                    false.ToString());
                paymentRequestPropertyList.Add(paymentProperty);

                // Add connector properties by service account ID
                PaymentProperty[] paymentRequestProperties = paymentRequestPropertyList.ToArray();
                string connectorName;
                paymentRequestProperties = AddConnectorPropertiesByServiceAccountId(request.RequestContext, paymentRequestProperties, out connectorName);

                // Prepare refund request
                Request paymentRequest = new Request
                {
                    Locale = locale ?? GetLocale(request.RequestContext),
                    Properties = paymentRequestProperties
                };

                // Get payment processor
                IPaymentProcessor processor = CardPaymentService.GetPaymentProcessor(connectorName);

                // Run refund and validate result
                Response paymentResponse = processor.Refund(paymentRequest, null);
                VerifyResponseResult("Refund", paymentResponse, GenericNamespace.RefundResponse, RefundResponseProperties.RefundResult, new List<string>() { RefundResult.Success.ToString() }, PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToCancelPayment);
            }

            private static PaymentConnectorConfiguration PickPaymentConnectorConfiguration(RequestContext context, string requestCurrency, string requestCardType)
            {
                var paymentConnectorConfigs = GetMerchantConnectorInformation(context);
                if (paymentConnectorConfigs == null || paymentConnectorConfigs.Count == 0)
                {
                    RetailLogger.Log.CrtServicesCardPaymentServiceNoConnectorsLoadedFailure();
                    throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_PaymentConnectorNotFound, "No payment connectors loaded.");
                }

                PaymentConnectorConfiguration matchPaymentConnectorConfiguration = null;
                foreach (PaymentConnectorConfiguration paymentConnectorConfig in paymentConnectorConfigs)
                {
                    PaymentProperty[] properties = PaymentProperty.ConvertXMLToPropertyArray(paymentConnectorConfig.ConnectorProperties);
                    PaymentSDK.Hashtable merchantConnectorInformation = PaymentProperty.ConvertToHashtable(properties);

                    string currencies;
                    if (PaymentProperty.GetPropertyValue(merchantConnectorInformation, GenericNamespace.MerchantAccount, MerchantAccountProperties.SupportedCurrencies, out currencies)
                        && !string.IsNullOrWhiteSpace(currencies))
                    {
                        if (currencies.ToUpper().Contains(requestCurrency))
                        {
                            if (!string.IsNullOrEmpty(requestCardType))
                            {
                                string cardTypes;
                                if (PaymentProperty.GetPropertyValue(merchantConnectorInformation, GenericNamespace.MerchantAccount, MerchantAccountProperties.SupportedTenderTypes, out cardTypes)
                                    && !string.IsNullOrWhiteSpace(cardTypes))
                                {
                                    if (cardTypes.ToUpper().Contains(requestCardType))
                                    {
                                        string connectorName = paymentConnectorConfig.Name;

                                        if (string.IsNullOrEmpty(connectorName))
                                        {
                                            RetailLogger.Log.CrtServicesCardPaymentServiceConnectorToServiceFailue();
                                            throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "Connector to service request not found!");
                                        }

                                        matchPaymentConnectorConfiguration = paymentConnectorConfig;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                matchPaymentConnectorConfiguration = paymentConnectorConfig;
                                break;
                            }
                        }
                    }
                    else
                    {
                        NetTracer.Warning("Connector {0} missing supported currencies property", paymentConnectorConfig.Name);
                    }
                }

                // If there is no match for payment connector configuration based on card types and currency, we should throw an exception instead of returning null.
                if (matchPaymentConnectorConfiguration == null)
                {
                    throw new ConfigurationException(
                        ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_PaymentConnectorNotFound,
                        string.Format("No payment connector could be found for channel id '{0}' that supports the requested currency type '{1}' and card type '{2}'.", context.GetChannelConfiguration().RecordId, requestCurrency, requestCardType));
                }

                return matchPaymentConnectorConfiguration;
            }

            /// <summary>
            /// Adds the merchant specific payment connector properties by service account identifier, and returns updated payment properties.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="paymentRequestProperties">The payment properties of the request.</param>
            /// <param name="connectorName">Name of the connector.</param>
            /// <returns>Updated payment properties.</returns>
            /// <exception cref="DataValidationException">Thrown if the ServiceAccountId was not specified in the request.</exception>
            /// <exception cref="ConfigurationException">Thrown if no payment connectors with the requested ServiceAccountId could be found.</exception>
            private static PaymentProperty[] AddConnectorPropertiesByServiceAccountId(RequestContext context, PaymentProperty[] paymentRequestProperties, out string connectorName)
            {
                connectorName = string.Empty;

                PaymentSDK.Hashtable requestHashtable = PaymentProperty.ConvertToHashtable(paymentRequestProperties);

                string requestedServiceAccountId;
                if (!PaymentProperty.GetPropertyValue(requestHashtable, GenericNamespace.MerchantAccount, MerchantAccountProperties.ServiceAccountId, out requestedServiceAccountId)
                    || string.IsNullOrWhiteSpace(requestedServiceAccountId))
                {
                    RetailLogger.Log.CrtServicesCardPaymentServiceMissingPropertyFailue("requestedServiceAccountId");
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "ServiceAccountId not found.");
                }

                // Need to load correct merchant account details
                List<PaymentProperty> updatedPaymentRequestProperties = paymentRequestProperties.ToList();

                var paymentConnectorConfigs = GetMerchantConnectorInformation(context);
                bool wasRequestedServiceAccountIdFound = false;
                foreach (PaymentConnectorConfiguration connectorConfig in paymentConnectorConfigs)
                {
                    PaymentProperty[] connectorProperties = PaymentProperty.ConvertXMLToPropertyArray(connectorConfig.ConnectorProperties);
                    PaymentSDK.Hashtable connectorPropertiesHashTable = PaymentProperty.ConvertToHashtable(connectorProperties);
                    string foundServiceId;

                    if (PaymentProperty.GetPropertyValue(connectorPropertiesHashTable, GenericNamespace.MerchantAccount, MerchantAccountProperties.ServiceAccountId, out foundServiceId)
                        && !string.IsNullOrWhiteSpace(foundServiceId))
                    {
                        if (foundServiceId == requestedServiceAccountId)
                        {
                            connectorName = connectorConfig.Name;
                            wasRequestedServiceAccountIdFound = true;
                            foreach (PaymentProperty connectorProperty in connectorProperties)
                            {
                                if (!updatedPaymentRequestProperties.Contains(connectorProperty))
                                {
                                    updatedPaymentRequestProperties.Add(connectorProperty);
                                }
                            }

                            break;
                        }
                    }
                }

                if (!wasRequestedServiceAccountIdFound)
                {
                    string message = string.Format("No payment connector configurations were found that have the requested ServiceAccountId : '{0}'.", requestedServiceAccountId);
                    throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_PaymentConnectorNotFound, message);
                }

                PaymentProperty connectorNamePaymentProperty = new PaymentProperty(GenericNamespace.Connector, ConnectorProperties.ConnectorName, connectorName);

                var previousConnectorNameProperty = updatedPaymentRequestProperties.SingleOrDefault(i => (i.Namespace.Equals(connectorNamePaymentProperty.Namespace) && i.Name.Equals(connectorNamePaymentProperty.Name)));
                if (previousConnectorNameProperty == null)
                {
                    updatedPaymentRequestProperties.Add(connectorNamePaymentProperty);
                }
                else
                {
                    previousConnectorNameProperty.StringValue = connectorName;
                }

                return updatedPaymentRequestProperties.ToArray();
            }

            /// <summary>
            /// Supported the card types.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>Response containing supported card types.</returns>
            private static SupportedCardTypesPaymentServiceResponse SupportedCardTypes(SupportedCardTypesPaymentServiceRequest request)
            {
                NetTracer.Information("Calling Payment.SupportedCardTypes");
                if (request == null || string.IsNullOrWhiteSpace(request.Currency))
                {
                    RetailLogger.Log.CrtServicesCardPaymentServiceSupportedCardTypeRequestInvalid(request.ToString());
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "Invalid request.");
                }

                var merchantConnectorInformationList = GetMerchantConnectorInformation(request.RequestContext);
                if (merchantConnectorInformationList == null || !merchantConnectorInformationList.Any())
                {
                    RetailLogger.Log.CrtServicesCardPaymentServiceNoConnectorsLoadedFailure();
                    throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_PaymentConnectorNotFound, "No payment connectors loaded.");
                }

                Collection<string> responseCardTypes = new Collection<string>();

                foreach (PaymentConnectorConfiguration item in merchantConnectorInformationList)
                {
                    PaymentProperty[] properties = PaymentProperty.ConvertXMLToPropertyArray(item.ConnectorProperties);
                    PaymentSDK.Hashtable merchantConnectorInformation = PaymentProperty.ConvertToHashtable(properties);
                    string currencies;
                    if (PaymentProperty.GetPropertyValue(
                        merchantConnectorInformation,
                        GenericNamespace.MerchantAccount,
                        MerchantAccountProperties.SupportedCurrencies,
                        out currencies) && !string.IsNullOrWhiteSpace(currencies))
                    {
                        string requestCurrency = request.Currency.ToUpper();
                        if (currencies.ToUpper().Contains(requestCurrency))
                        {
                            string cardTypes;
                            if (PaymentProperty.GetPropertyValue(
                                merchantConnectorInformation,
                                GenericNamespace.MerchantAccount,
                                MerchantAccountProperties.SupportedTenderTypes,
                                out cardTypes) && !string.IsNullOrWhiteSpace(cardTypes))
                            {
                                string[] splitCardTypes = cardTypes.Split(';');
                                foreach (string cardTypeItem in splitCardTypes)
                                {
                                    if (!responseCardTypes.Contains(cardTypeItem))
                                    {
                                        responseCardTypes.Add(cardTypeItem);
                                    }
                                }
                            }
                            else
                            {
                                NetTracer.Warning("Connector {0} missing supported card types", item.Name);
                            }
                        }
                    }
                    else
                    {
                        NetTracer.Warning("Connector {0} missing supported currencies property", item.Name);
                    }
                }

                NetTracer.Information("Completed Payment.SupportedCardTypes");
                return new SupportedCardTypesPaymentServiceResponse(responseCardTypes.AsPagedResult());
            }

            /// <summary>
            /// Retrieves the result of card payment accepting.
            /// </summary>
            /// <param name="request">The request containing the result access code.</param>
            /// <returns>The response containing the result tender line and/or the result card token.</returns>
            private static RetrieveCardPaymentAcceptResultServiceResponse RetrieveCardPaymentAcceptResult(RetrieveCardPaymentAcceptResultServiceRequest request)
            {
                NetTracer.Information("Calling Payment.RetrieveCardPaymentAcceptResult");

                // Retrieve payment response by access code
                Response paymentResponse = RetrieveCardPaymentAcceptResult(request.RequestContext, request.ResultAccessCode);

                // Read payment response properties.
                TenderLine tenderLine = null;
                TokenizedPaymentCard tokenizedPaymentCard = null;
                IList<Microsoft.Dynamics.Commerce.Runtime.DataModel.PaymentError> resultErrors = new List<Microsoft.Dynamics.Commerce.Runtime.DataModel.PaymentError>();
                Hashtable responseProperties = PaymentProperty.ConvertToHashtable(paymentResponse.Properties);

                // Fill in the card token if any
                PaymentProperty cardTokenProperty = PaymentProperty.GetPropertyFromHashtable(responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.CardToken);
                if (cardTokenProperty != null && !string.IsNullOrWhiteSpace(cardTokenProperty.StringValue))
                {
                    tokenizedPaymentCard = new TokenizedPaymentCard();
                    PaymentProperty property = PaymentProperty.GetPropertyFromHashtable(responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.Name);
                    tokenizedPaymentCard.NameOnCard = (property != null) ? property.StringValue : string.Empty;

                    property = PaymentProperty.GetPropertyFromHashtable(responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.StreetAddress);
                    tokenizedPaymentCard.Address1 = (property != null) ? property.StringValue : string.Empty;

                    property = PaymentProperty.GetPropertyFromHashtable(responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.StreetAddress2);
                    tokenizedPaymentCard.Address2 = (property != null) ? property.StringValue : string.Empty;

                    property = PaymentProperty.GetPropertyFromHashtable(responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.City);
                    tokenizedPaymentCard.City = (property != null) ? property.StringValue : string.Empty;

                    property = PaymentProperty.GetPropertyFromHashtable(responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.State);
                    tokenizedPaymentCard.State = (property != null) ? property.StringValue : string.Empty;

                    property = PaymentProperty.GetPropertyFromHashtable(responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.Country);
                    tokenizedPaymentCard.Country = (property != null) ? property.StringValue : string.Empty;

                    property = PaymentProperty.GetPropertyFromHashtable(responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.PostalCode);
                    tokenizedPaymentCard.Zip = (property != null) ? property.StringValue : string.Empty;

                    property = PaymentProperty.GetPropertyFromHashtable(responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.ExpirationMonth);
                    tokenizedPaymentCard.ExpirationMonth = (property != null) ? (int)property.DecimalValue : 0;

                    property = PaymentProperty.GetPropertyFromHashtable(responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.ExpirationYear);
                    tokenizedPaymentCard.ExpirationYear = (property != null) ? (int)property.DecimalValue : 0;

                    tokenizedPaymentCard.IsSwipe = false; // Always false for tokenized payment card.

                    tokenizedPaymentCard.CardTokenInfo = new CardTokenInfo();
                    var binStartProperty = PaymentProperty.GetPropertyFromHashtable(responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.BankIdentificationNumberStart);
                    var last4Digitsproperty = PaymentProperty.GetPropertyFromHashtable(responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.Last4Digits);
                    tokenizedPaymentCard.CardTokenInfo.MaskedCardNumber = GetMaskedCardNumber(binStartProperty, last4Digitsproperty);

                    property = PaymentProperty.GetPropertyFromHashtable(responseProperties, GenericNamespace.PaymentCard, PaymentCardProperties.UniqueCardId);
                    tokenizedPaymentCard.CardTokenInfo.UniqueCardId = (property != null) ? property.StringValue : string.Empty;

                    property = PaymentProperty.GetPropertyFromHashtable(responseProperties, GenericNamespace.MerchantAccount, MerchantAccountProperties.ServiceAccountId);
                    tokenizedPaymentCard.CardTokenInfo.ServiceAccountId = (property != null) ? property.StringValue : string.Empty;

                    tokenizedPaymentCard.CardTokenInfo.CardToken = PaymentProperty.ConvertPropertyArrayToXML(paymentResponse.Properties);
                }

                // Fill in the result tender line if any
                PaymentProperty authorizationResponsePropertyList = PaymentProperty.GetPropertyFromHashtable(responseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.Properties);
                if (authorizationResponsePropertyList != null)
                {
                    tenderLine = new TenderLine();
                    Hashtable authorizationResponseProperties = PaymentProperty.ConvertToHashtable(authorizationResponsePropertyList.PropertyList);

                    var binStartProperty = PaymentProperty.GetPropertyFromHashtable(authorizationResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.BankIdentificationNumberStart);
                    var last4DigitsProperty = PaymentProperty.GetPropertyFromHashtable(authorizationResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.Last4Digits);
                    tenderLine.MaskedCardNumber = GetMaskedCardNumber(binStartProperty, last4DigitsProperty);

                    PaymentProperty authorizationResponseProperty = PaymentProperty.GetPropertyFromHashtable(authorizationResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ApprovedAmount);
                    tenderLine.Amount = (authorizationResponseProperty != null) ? authorizationResponseProperty.DecimalValue : 0M;

                    authorizationResponseProperty = PaymentProperty.GetPropertyFromHashtable(authorizationResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.CurrencyCode);
                    tenderLine.Currency = (authorizationResponseProperty != null) ? authorizationResponseProperty.StringValue : string.Empty;

                    if (string.IsNullOrWhiteSpace(tenderLine.Currency))
                    {
                        ChannelConfiguration channelConfiguration = request.RequestContext.GetChannelConfiguration();
                        tenderLine.Currency = channelConfiguration.Currency;
                    }

                    authorizationResponseProperty = PaymentProperty.GetPropertyFromHashtable(authorizationResponseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AuthorizationResult);
                    if (authorizationResponseProperty != null
                        && (authorizationResponseProperty.StringValue.Equals(AuthorizationResult.Success.ToString(), StringComparison.OrdinalIgnoreCase)
                            || authorizationResponseProperty.StringValue.Equals(AuthorizationResult.PartialAuthorization.ToString(), StringComparison.OrdinalIgnoreCase)))
                    {
                        tenderLine.Status = TenderLineStatus.PendingCommit;
                        tenderLine.IsVoidable = true;
                    }
                    else
                    {
                        tenderLine.Status = TenderLineStatus.NotProcessed;
                        tenderLine.IsVoidable = false;
                    }

                    tenderLine.Authorization = PaymentProperty.ConvertPropertyArrayToXML(paymentResponse.Properties);

                    // When the payment is authorized, check whether the payment has been captured too.
                    if (tenderLine.Status == TenderLineStatus.PendingCommit)
                    {
                        PaymentProperty captureResponsePropertyList = PaymentProperty.GetPropertyFromHashtable(responseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.Properties);
                        if (captureResponsePropertyList != null)
                        {
                            Hashtable captureResponseProperties = PaymentProperty.ConvertToHashtable(captureResponsePropertyList.PropertyList);
                            PaymentProperty captureResponseProperty = PaymentProperty.GetPropertyFromHashtable(captureResponseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CaptureResult);
                            if (captureResponseProperty != null
                                && captureResponseProperty.StringValue.Equals(CaptureResult.Success.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                tenderLine.Status = TenderLineStatus.Committed;
                            }
                        }

                        // A payment authorization could be voided for two reasons:
                        // 1. Partial authorization was rejected by user;
                        // 2. Capture failed.
                        // So check if the payment had been voided successfully.
                        PaymentProperty voidResponsePropertyList = PaymentProperty.GetPropertyFromHashtable(responseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.Properties);
                        if (voidResponsePropertyList != null)
                        {
                            Hashtable voidResponseProperties = PaymentProperty.ConvertToHashtable(voidResponsePropertyList.PropertyList);
                            PaymentProperty voidResponseProperty = PaymentProperty.GetPropertyFromHashtable(voidResponseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.VoidResult);
                            if (voidResponseProperty != null
                                && voidResponseProperty.StringValue.Equals(VoidResult.Success.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                tenderLine.Status = TenderLineStatus.Voided;
                                resultErrors = CardPaymentService.ConvertToPaymentSdkErrors(paymentResponse.Errors);
                            }
                        }
                    }
                }

                NetTracer.Information("Completed Payment.RetrieveCardPaymentAcceptResult");
                var cardPaymentAcceptResult = new CardPaymentAcceptResult();
                cardPaymentAcceptResult.TenderLine = tenderLine;
                cardPaymentAcceptResult.TokenizedPaymentCard = tokenizedPaymentCard;
                cardPaymentAcceptResult.PaymentSdkErrors = resultErrors;
                return new RetrieveCardPaymentAcceptResultServiceResponse(cardPaymentAcceptResult);
            }

            private static Response RetrieveCardPaymentAcceptResult(RequestContext context, string resultAccessCode)
            {
                var properties = new List<PaymentProperty>();

                // Prepare merchant properties
                ChannelConfiguration channelConfiguration = context.GetChannelConfiguration();
                string channelCurrency = channelConfiguration.Currency;

                PaymentConnectorConfiguration paymentConnectorConfig = PickPaymentConnectorConfiguration(context, channelCurrency, requestCardType: null);
                PaymentProperty[] merchantProperties = PaymentProperty.ConvertXMLToPropertyArray(paymentConnectorConfig.ConnectorProperties);
                properties.AddRange(merchantProperties);

                // Prepare transaction data properties
                PaymentProperty property = new PaymentProperty(
                GenericNamespace.TransactionData,
                TransactionDataProperties.PaymentAcceptResultAccessCode,
                resultAccessCode);
                properties.Add(property);

                // Call payment processor to retrieve payment result
                IPaymentProcessor processor = CardPaymentService.GetPaymentProcessor(paymentConnectorConfig.Name);
                Request paymentRequest = new Request
                {
                    Locale = locale ?? GetLocale(context),
                    Properties = properties.ToArray()
                };
                Response paymentResponse = processor.RetrievePaymentAcceptResult(paymentRequest);

                // Validate response
                string operationName = "RetrieveCardPaymentAcceptResult";
                var error = PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToRetrieveCardPaymentAcceptResult;
                if (paymentResponse == null)
                {
                    // throw generic exception if operation failed but no errors returned.
                    throw new PaymentException(error, string.Format("{0} failed.", operationName));
                }

                if (paymentResponse.Errors != null && paymentResponse.Errors.Any())
                {
                    CardPaymentService.VerifyResponseErrors(operationName, paymentResponse, error);
                }

                return paymentResponse;
            }

            private static Collection<PaymentConnectorConfiguration> GetMerchantConnectorInformation(RequestContext context)
            {
                var merchantConnectorInformationList = new Collection<PaymentConnectorConfiguration>();

                var connectorConfigs = new Collection<PaymentConnectorConfiguration>();
                if (IsRequestFromTerminal(context))
                {
                    if (context.GetTerminal() != null)
                    {
                        GetPaymentConnectorDataRequest dataRequest = new GetPaymentConnectorDataRequest(context.GetTerminal().TerminalId);
                        var connector =
                            context.Execute<SingleEntityDataServiceResponse<PaymentConnectorConfiguration>>(dataRequest)
                                .Entity;

                        RefreshTerminalMerchantProperties(connector, context);

                        connectorConfigs.Add(connector);
                    }
                }
                else
                {
                    long currentChannelId = context.GetPrincipal().ChannelId;

                    // Get all setup merchant payment connector settings for a channel
                    GetPaymentConnectorConfigurationDataRequest configurationRequest = new GetPaymentConnectorConfigurationDataRequest(currentChannelId);
                    IEnumerable<PaymentConnectorConfiguration> connectors = context.Execute<EntityDataServiceResponse<PaymentConnectorConfiguration>>(configurationRequest).PagedEntityCollection.Results;

                    if (currentChannelId != 0)
                    {
                        RefreshOnlineChannelMerchantProperties(connectors, currentChannelId, context);
                    }

                    connectorConfigs.AddRange(connectors);
                }

                if (!connectorConfigs.Any())
                {
                    throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_PaymentConnectorNotFound, "No payment connectors found.");
                }

                foreach (PaymentConnectorConfiguration item in connectorConfigs)
                {
                    if (item == null || string.IsNullOrEmpty(item.Name) || string.IsNullOrEmpty(item.ConnectorProperties))
                    {
                        continue;
                    }

                    NetTracer.Information("Adding merchant information {0}", item.Name);

                    // Save merchant data
                    merchantConnectorInformationList.Add(item);
                }

                LoadAllSetupConnectors(merchantConnectorInformationList.ToArray());

                return merchantConnectorInformationList;
            }

            private static void RefreshOnlineChannelMerchantProperties(IEnumerable<PaymentConnectorConfiguration> connectors, long channelId, RequestContext context)
            {
                // Refresh merchant properties from AX HQ, Call TS
                var realtimeRequest =
                    new GetOnlineChannelMerchantPaymentProviderDataRealtimeRequest(channelId);
                var realtimeResponse =
                    context.Runtime.Execute<GetOnlineChannelMerchantPaymentProviderDataRealtimeResponse>(realtimeRequest, context);

                Dictionary<long, string> responseData = new Dictionary<long, string>();
                XElement responseDataXml = XElement.Parse(realtimeResponse.MerchantData);
                foreach (var item in responseDataXml.Elements())
                {
                    long key = long.Parse(item.Attribute("Key").Value);
                    string value = item.Attribute("Value").Value;
                    responseData.Add(key, value);
                }

                foreach (var item in connectors)
                {
                    if (responseData.ContainsKey(item.RecordId))
                    {
                        item.ConnectorProperties = responseData[item.RecordId];
                    }
                }
            }

            private static void RefreshTerminalMerchantProperties(PaymentConnectorConfiguration config, RequestContext context)
            {
                // Refresh merchant properties from AX HQ, Call TS
                var realtimeRequest =
                    new GetTerminalMerchantPaymentProviderDataRealtimeRequest(config.ProfileId);
                var realtimeResponse =
                    context.Runtime.Execute<GetTerminalMerchantPaymentProviderDataRealtimeResponse>(realtimeRequest, context);

                config.ConnectorProperties = realtimeResponse.PaymentMerchantInformation.PaymentConnectorPropertiesXml;

                if (string.IsNullOrEmpty(config.ConnectorProperties))
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_PaymentRequiresMerchantProperties, "Merchant properties are null.");
                }
            }

            /// <summary>
            /// Loads all payment connector assemblies.
            /// </summary>
            /// <param name="paymentConfigs">The payment configurations.</param>
            private static void LoadAllSetupConnectors(params PaymentConnectorConfiguration[] paymentConfigs)
            {
                List<string> connectors = new List<string>();

                foreach (PaymentConnectorConfiguration item in paymentConfigs)
                {
                    PaymentProperty[] properties = PaymentProperty.ConvertXMLToPropertyArray(item.ConnectorProperties);
                    PaymentSDK.Hashtable merchantConnectorInformation = PaymentProperty.ConvertToHashtable(properties);

                    // CRT now only loads the PCL version of the payment connectors
                    // Preference is to load the new portable assembly name, else use the old property
                    string connectorAssemblyName;
                    if (!PaymentProperty.GetPropertyValue(
                        merchantConnectorInformation,
                        GenericNamespace.MerchantAccount,
                        MerchantAccountProperties.PortableAssemblyName,
                        out connectorAssemblyName))
                    {
                        // Get old assembly names for connectors
                        PaymentProperty.GetPropertyValue(
                            merchantConnectorInformation,
                            GenericNamespace.MerchantAccount,
                            MerchantAccountProperties.AssemblyName,
                            out connectorAssemblyName);
                    }

                    if (!string.IsNullOrEmpty(connectorAssemblyName))
                    {
                        if (!connectors.Contains(connectorAssemblyName))
                        {
                            NetTracer.Information("Adding assemblies to load {0}", connectorAssemblyName);
                            connectors.Add(connectorAssemblyName);
                        }
                    }
                }

                // Load all setup connectors
                if (connectors.Any())
                {
                    try
                    {
                        NetTracer.Information("Loading connectors");
                        PaymentProcessorManager.Create(connectors.ToArray());
                    }
                    catch (Exception ex)
                    {
                        // Swallow exception so CRT doesn't crash
                        RetailLogger.Log.CrtServicesCardPaymentServiceLoadingAllConnectorsFailure(ex);
                    }
                }
            }

            /// <summary>
            /// Determines whether the current request is coming from a terminal.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>
            /// If <c>true</c> the request originated from a terminal.
            /// </returns>
            private static bool IsRequestFromTerminal(RequestContext context)
            {
                return context != null && context.GetPrincipal() != null && context.GetPrincipal().TerminalId > 0;
            }

            /// <summary>
            /// Gets the locale.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>The current locale.</returns>
            private static string GetLocale(RequestContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }

                GetDefaultLanguageIdDataRequest languageIdRequest = new GetDefaultLanguageIdDataRequest();
                return locale = context.Execute<SingleEntityDataServiceResponse<string>>(languageIdRequest).Entity;
            }

            /// <summary>
            /// Gets the payment properties for cancel (void).
            /// </summary>
            /// <param name="request">The payment request.</param>
            /// <returns>A list of payment properties that would be required for a card payment capture request.</returns>
            private static IList<PaymentProperty> GetPaymentPropertiesForCancellation(VoidPaymentServiceRequest request)
            {
                List<PaymentProperty> properties = new List<PaymentProperty>();
                PaymentProperty property;

                // Currency
                property = new PaymentProperty(
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.CurrencyCode,
                    request.TenderLine.Currency);
                properties.Add(property);

                // Extract payment properties from TenderLine.Authorization.
                PaymentProperty[] storedAuthorizationResponseProperties = PaymentProperty.ConvertXMLToPropertyArray(request.TenderLine.Authorization);
                var paymentProperties = CombinePaymentProperties(storedAuthorizationResponseProperties, properties);

                return paymentProperties;
            }

            /// <summary>
            /// Gets the payment properties for payment authorization.
            /// </summary>
            /// <param name="tenderLine">The tender line.</param>
            /// <param name="tokenizedPaymentCard">The tokenized payment card.</param>
            /// <param name="allowPartialAuthorization">If set to <c>true</c> [then allow partial authorization].</param>
            /// <returns>
            /// An array of payment properties required for a valid authorization request.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">Thrown when tenderLine or tokenizedPaymentCard is null.</exception>
            /// <exception cref="DataValidationException">Throw if card token has an invalid format.</exception>
            private static PaymentProperty[] GetPaymentPropertiesForAuthorizationWithToken(TenderLine tenderLine, TokenizedPaymentCard tokenizedPaymentCard, bool allowPartialAuthorization)
            {
                if (tenderLine == null)
                {
                    throw new ArgumentNullException("tenderLine");
                }

                if (tokenizedPaymentCard == null)
                {
                    throw new ArgumentNullException("tokenizedPaymentCard");
                }

                List<PaymentProperty> paymentProperties = new List<PaymentProperty>();

                PaymentProperty property = new PaymentProperty(GenericNamespace.TransactionData, TransactionDataProperties.Amount, Math.Abs(tenderLine.Amount)); // For refunds request, amount must be positive.
                paymentProperties.Add(property);

                property = new PaymentProperty(GenericNamespace.TransactionData, TransactionDataProperties.CurrencyCode, tenderLine.Currency);
                paymentProperties.Add(property);

                property = new PaymentProperty(GenericNamespace.TransactionData, TransactionDataProperties.SupportCardTokenization, true.ToString());
                paymentProperties.Add(property);

                property = new PaymentProperty(GenericNamespace.TransactionData, TransactionDataProperties.AllowPartialAuthorization, allowPartialAuthorization.ToString());
                paymentProperties.Add(property);

                property = new PaymentProperty(GenericNamespace.PaymentCard, PaymentCardProperties.Name, tokenizedPaymentCard.NameOnCard);
                paymentProperties.Add(property);

                property = new PaymentProperty(GenericNamespace.PaymentCard, PaymentCardProperties.ExpirationYear, tokenizedPaymentCard.ExpirationYear);
                paymentProperties.Add(property);

                property = new PaymentProperty(GenericNamespace.PaymentCard, PaymentCardProperties.ExpirationMonth, tokenizedPaymentCard.ExpirationMonth);
                paymentProperties.Add(property);

                property = new PaymentProperty(GenericNamespace.PaymentCard, PaymentCardProperties.StreetAddress, tokenizedPaymentCard.Address1);
                paymentProperties.Add(property);

                property = new PaymentProperty(GenericNamespace.PaymentCard, PaymentCardProperties.PostalCode, tokenizedPaymentCard.Zip);
                paymentProperties.Add(property);

                property = GetCountryProperty(tokenizedPaymentCard.Country);
                paymentProperties.Add(property);

                property = new PaymentProperty(GenericNamespace.PaymentCard, PaymentCardProperties.UniqueCardId, tokenizedPaymentCard.CardTokenInfo.UniqueCardId);
                paymentProperties.Add(property);

                // If card token is the payment properties xml string then extract card token property from it.
                if (tokenizedPaymentCard.CardTokenInfo.CardToken.Contains("<![CDATA["))
                {
                    string authorizationXml = ExtractAuthorizationXml(tokenizedPaymentCard.CardTokenInfo.CardToken);

                    PaymentProperty[] properties = PaymentProperty.ConvertXMLToPropertyArray(authorizationXml);
                    Hashtable propertyTable = PaymentProperty.ConvertToHashtable(properties);

                    property = PaymentProperty.GetPropertyFromHashtable(propertyTable, GenericNamespace.PaymentCard, PaymentCardProperties.CardType);
                    paymentProperties.Add(property);

                    property = PaymentProperty.GetPropertyFromHashtable(propertyTable, GenericNamespace.PaymentCard, PaymentCardProperties.CardToken);
                    paymentProperties.Add(property);
                }
                else
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidFormat, "Invalid card token format.");
                }

                string last4Digits = GetLast4Digits(tokenizedPaymentCard.CardTokenInfo.MaskedCardNumber);
                property = new PaymentProperty(GenericNamespace.PaymentCard, PaymentCardProperties.Last4Digits, last4Digits);
                paymentProperties.Add(property);

                property = new PaymentProperty(GenericNamespace.PaymentCard, PaymentCardProperties.IsSwipe, tokenizedPaymentCard.IsSwipe.ToString());
                paymentProperties.Add(property);

                property = new PaymentProperty(
                    GenericNamespace.MerchantAccount,
                    MerchantAccountProperties.ServiceAccountId,
                    tokenizedPaymentCard.CardTokenInfo.ServiceAccountId);
                paymentProperties.Add(property);

                return paymentProperties.ToArray();
            }

            /// <summary>
            /// Gets the payment properties for capture.
            /// </summary>
            /// <param name="tenderLine">The tender line.</param>
            /// <returns>A list of payment properties that would be required for a card payment capture request.</returns>
            private static List<PaymentProperty> GetPaymentPropertiesForCapture(TenderLine tenderLine)
            {
                List<PaymentProperty> properties = new List<PaymentProperty>();
                PaymentProperty property;

                // Amount.
                property = new PaymentProperty(
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.Amount,
                    Math.Abs(tenderLine.Amount)); // for refunds request amount must be positive
                properties.Add(property);

                // Currency
                property = new PaymentProperty(
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.CurrencyCode,
                    tenderLine.Currency);
                properties.Add(property);

                // Extract payment properties from TenderLine.Authorization.
                PaymentProperty[] storedAuthorizationResponseProperties = PaymentProperty.ConvertXMLToPropertyArray(tenderLine.Authorization);
                var paymentProperties = CombinePaymentProperties(storedAuthorizationResponseProperties, properties);

                return new List<PaymentProperty>(paymentProperties);
            }

            /// <summary>
            /// Combines the payment properties.
            /// </summary>
            /// <param name="paymentProperties1">First set of payment properties.</param>
            /// <param name="paymentProperties2">Second set of payment properties.</param>
            /// <returns>An array which is a combination of the unique elements in two sets of input payment properties.</returns>
            private static PaymentProperty[] CombinePaymentProperties(IEnumerable<PaymentProperty> paymentProperties1, IEnumerable<PaymentProperty> paymentProperties2)
            {
                // This logic can be replaced by a hashset to increase performance.
                // Using HashSet will require changes to PaymentProperty.
                var combinedPaymentProperties = new List<PaymentProperty>(paymentProperties1);

                foreach (var paymentProperty in paymentProperties2)
                {
                    if (!combinedPaymentProperties.Contains(paymentProperty))
                    {
                        combinedPaymentProperties.Add(paymentProperty);
                    }
                }

                return combinedPaymentProperties.ToArray();
            }

            /// <summary>
            /// Gets the tender line from the specified payment properties.
            /// </summary>
            /// <param name="request">Capture request.</param>
            /// <returns>
            /// The tender line.
            /// </returns>
            private static TenderLine GetTenderLineFromCaptureResponse(CapturePaymentServiceRequest request)
            {
                TenderLine tenderLine = request.TenderLine;
                tenderLine.Status = TenderLineStatus.Committed;
                tenderLine.IsVoidable = false; // payment cannot be voided once captured.
                return tenderLine;
            }

            /// <summary>
            /// Gets the tender line from the specified payment properties.
            /// </summary>
            /// <param name="request">Void request.</param>
            /// <returns>
            /// The tender line.
            /// </returns>
            private static TenderLine GetTenderLineForVoid(VoidPaymentServiceRequest request)
            {
                TenderLine tenderLine = request.TenderLine;
                tenderLine.Status = TenderLineStatus.Voided;
                tenderLine.IsVoidable = false; // payment can not be voided twice.
                return tenderLine;
            }

            /// <summary>
            /// Gets the tender line from the specified authorization payment properties.
            /// </summary>
            /// <param name="tenderLine">The tender line.</param>
            /// <param name="authorizationProperties">The properties.</param>
            /// <returns>
            /// The updated tender line.
            /// </returns>
            private static TenderLine GetTenderLineFromAuthorizationResponse(TenderLine tenderLine, PaymentProperty[] authorizationProperties)
            {
                tenderLine.Status = TenderLineStatus.PendingCommit;
                tenderLine.IsVoidable = true;

                PaymentSDK.Hashtable hashTable = PaymentProperty.ConvertToHashtable(authorizationProperties);

                PaymentProperty property = PaymentProperty.GetPropertyFromHashtable(hashTable, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ApprovedAmount);
                tenderLine.Amount = (property != null) ? property.DecimalValue : 0.0m;

                property = PaymentProperty.GetPropertyFromHashtable(hashTable, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.CashBackAmount);
                tenderLine.CashBackAmount = (property != null) ? property.DecimalValue : 0.0m;

                property = PaymentProperty.GetPropertyFromHashtable(hashTable, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.Last4Digits);
                tenderLine.MaskedCardNumber = GetMaskedCardNumber(null, property);

                // Some payment connectors may capture in one payment operation.
                property = PaymentProperty.GetPropertyFromHashtable(hashTable, GenericNamespace.CaptureResponse, CaptureResponseProperties.CaptureResult);
                if (property != null && property.StringValue.Equals(CaptureResult.Success.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    // Yes it has been captured, set the tender line status to committed.
                    tenderLine.Status = TenderLineStatus.Committed;
                }

                tenderLine.Authorization = PaymentProperty.ConvertPropertyArrayToXML(authorizationProperties);

                return tenderLine;
            }

            /// <summary>
            /// Gets the tender line from the specified refund payment properties.
            /// </summary>
            /// <param name="tenderLine">The tender line.</param>
            /// <param name="refundProperties">The properties.</param>
            /// <returns>
            /// The updated tender line.
            /// </returns>
            private static TenderLine GetTenderLineFromRefundResponse(TenderLine tenderLine, PaymentProperty[] refundProperties)
            {
                tenderLine.Status = TenderLineStatus.Committed; // refund doesn't require capture
                tenderLine.IsVoidable = false; // refund cannot be voided.

                PaymentSDK.Hashtable hashTable = PaymentProperty.ConvertToHashtable(refundProperties);

                PaymentProperty property = PaymentProperty.GetPropertyFromHashtable(hashTable, GenericNamespace.RefundResponse, RefundResponseProperties.Last4Digits);
                tenderLine.MaskedCardNumber = GetMaskedCardNumber(null, property);
                tenderLine.Authorization = PaymentProperty.ConvertPropertyArrayToXML(refundProperties);

                return tenderLine;
            }

            /// <summary>
            /// Gets the country property.
            /// </summary>
            /// <param name="countryRegionCode">The country region code.</param>
            /// <returns>Payment property that encapsulates the country code.</returns>
            /// <exception cref="System.ArgumentException">Thrown if countryRegionCode is empty or invalid.</exception>
            /// <exception cref="DataValidationException">Invalid country/region code format.</exception>
            private static PaymentProperty GetCountryProperty(string countryRegionCode)
            {
                string twoLetterCountryCode;

                if (string.IsNullOrWhiteSpace(countryRegionCode))
                {
                    twoLetterCountryCode = string.Empty;
                }
                else if (countryRegionCode.Length == 2)
                {
                    twoLetterCountryCode = countryRegionCode;
                }
                else if (countryRegionCode.Length == 3)
                {
                    twoLetterCountryCode = countryRegionMapper.ConvertToTwoLetterCountryCode(countryRegionCode);
                }
                else
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidFormat, "Invalid country/region code format.");
                }

                PaymentProperty property = new PaymentProperty(
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.Country,
                        twoLetterCountryCode);
                return property;
            }

            /// <summary>
            /// Fail if card number was manually entered and manual entry is blocked for selected card type.
            /// </summary>
            /// <param name="paymentCardBase">Card information.</param>
            /// <param name="cardTypeInfo">Card type configuration.</param>
            private static void ValidateCardEntry(PaymentCardBase paymentCardBase, CardTypeInfo cardTypeInfo)
            {
                if (!cardTypeInfo.AllowManualInput && !paymentCardBase.IsSwipe)
                {
                    throw new PaymentException(
                        PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_ManualCardNumberNotAllowed,
                        string.Format("The card number for card type '{0}' cannot be manually entered.", cardTypeInfo.TypeId));
                }
            }

            /// <summary>
            /// Gets the payment processor by the specified name. Throws an exception if the specified payment processor is not found.
            /// </summary>
            /// <param name="paymentConnectorName">Name of the payment connector.</param>
            /// <returns>The payment processor that was requested.</returns>
            private static IPaymentProcessor GetPaymentProcessor(string paymentConnectorName)
            {
                IPaymentProcessor processor = null;
                try
                {
                    processor = PaymentProcessorManager.GetPaymentProcessor(paymentConnectorName);
                    if (processor == null)
                    {
                        var message = string.Format("The specified payment connector {0} could not be loaded.", paymentConnectorName);
                        ConfigurationException configurationException = new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_PaymentConnectorNotFound, message);
                        throw configurationException;
                    }
                }
                catch (InvalidOperationException exception)
                {
                    var message = string.Format("The specified payment connector {0} could not be loaded.", paymentConnectorName);
                    throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_PaymentConnectorNotFound, exception, message);
                }

                return processor;
            }

            private static string ExtractAuthorizationXml(string authorizationBlob)
            {
                // The authorization blob may start with a GUID
                if (authorizationBlob.StartsWith("<![CDATA[", StringComparison.OrdinalIgnoreCase))
                {
                    return authorizationBlob;
                }
                else
                {
                    return authorizationBlob.Substring(36);
                }
            }

            /// <summary>
            /// Gets the masked card number from BIN start number and last 4 digits.
            /// </summary>
            /// <param name="bankIdentificationNumberStartProperty">The payment property contains the start number of card BIN.</param>
            /// <param name="last4DigitProperty">The payment property containing the last 4 digits of card number.</param>
            /// <returns>The masked card number.</returns>
            private static string GetMaskedCardNumber(PaymentProperty bankIdentificationNumberStartProperty, PaymentProperty last4DigitProperty)
            {
                string binStart = bankIdentificationNumberStartProperty != null ? bankIdentificationNumberStartProperty.StringValue : null;
                string last4Digit = last4DigitProperty != null ? last4DigitProperty.StringValue : null;

                if (binStart != null)
                {
                    binStart = binStart.Trim();
                    if (binStart.Length > BankIdentificationNumberLength)
                    {
                        // BIN cannot be longer than 6 digits.
                        binStart = binStart.Substring(0, BankIdentificationNumberLength);
                    }
                }

                if (last4Digit != null)
                {
                    last4Digit = last4Digit.Trim();
                    if (last4Digit.Length > CardSuffixLength)
                    {
                        last4Digit = last4Digit.Substring(last4Digit.Length - CardSuffixLength);
                    }
                }

                string maskedCardNumber = null;
                if (string.IsNullOrWhiteSpace(binStart) && string.IsNullOrWhiteSpace(last4Digit))
                {
                    maskedCardNumber = string.Empty;
                }
                else if (string.IsNullOrWhiteSpace(binStart))
                {
                    maskedCardNumber = new string(CardNumberMaskCharacter, MaskedCardNumberLength - last4Digit.Length) + last4Digit;
                }
                else if (string.IsNullOrWhiteSpace(last4Digit))
                {
                    maskedCardNumber = binStart + new string(CardNumberMaskCharacter, MaskedCardNumberLength - binStart.Length);
                }
                else
                {
                    maskedCardNumber = binStart + new string(CardNumberMaskCharacter, MaskedCardNumberLength - binStart.Length - last4Digit.Length) + last4Digit;
                }

                return maskedCardNumber;
            }

            /// <summary>
            /// Gets the last 4 digits of the card number from the masked card number.
            /// </summary>
            /// <param name="maskedCardNumber">The masked card number.</param>
            /// <returns>The last 4 digits.</returns>
            private static string GetLast4Digits(string maskedCardNumber)
            {
                string last4Digits = string.Empty;
                if (!string.IsNullOrWhiteSpace(maskedCardNumber))
                {
                    maskedCardNumber = maskedCardNumber.Trim();
                    if (maskedCardNumber.Length > CardSuffixLength)
                    {
                        last4Digits = maskedCardNumber.Substring(maskedCardNumber.Length - CardSuffixLength);
                    }
                    else
                    {
                        last4Digits = maskedCardNumber;
                    }
                }

                return last4Digits;
            }

            /// <summary>
            /// Updates tender line with amounts and exchange rates for company and channel currencies.
            /// </summary>
            /// <param name="tenderLine">Tender line to update.</param>
            /// <param name="context">Request context.</param>
            private static void CalculateTenderLineCurrencyAmounts(TenderLine tenderLine, RequestContext context)
            {
                // In card payment, the tender currency is always the same as the channel currency.
                // Set AmountInTenderedCurrency to Amount (in channel currency) if not set already. 
                if (tenderLine.AmountInTenderedCurrency == 0M)
                {
                    tenderLine.ExchangeRate = 1m;
                    tenderLine.AmountInTenderedCurrency = tenderLine.Amount;
                }

                string companyCurrencyCode = context.GetChannelConfiguration().CompanyCurrency;
                if (!tenderLine.Currency.Equals(companyCurrencyCode, StringComparison.OrdinalIgnoreCase))
                {
                    // Convert tendered amount to company currency
                    var request = new GetCurrencyValueServiceRequest(
                        tenderLine.Currency,
                        companyCurrencyCode,
                        tenderLine.AmountInTenderedCurrency);

                    GetCurrencyValueServiceResponse tenderAmountInCompanyCurrency = context.Execute<GetCurrencyValueServiceResponse>(request);

                    tenderLine.CompanyCurrencyExchangeRate = tenderAmountInCompanyCurrency.ExchangeRate;
                    tenderLine.AmountInCompanyCurrency = tenderAmountInCompanyCurrency.RoundedConvertedAmount;
                }
                else
                {
                    tenderLine.CompanyCurrencyExchangeRate = 1m;
                    tenderLine.AmountInCompanyCurrency = tenderLine.Amount;
                }
            }
        }
    }
}