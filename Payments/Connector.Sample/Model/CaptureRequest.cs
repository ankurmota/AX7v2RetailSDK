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

/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/
namespace Contoso
{
    namespace Retail.SampleConnector.Portable
    {
        using System;
        using System.Collections.Generic;
        using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
        using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;

        internal class CaptureRequest : RequestBase
        {
            internal CaptureRequest()
                : base()
            {
            }

            internal string CardType { get; set; }

            internal bool? IsSwipe { get; set; }

            internal string CardNumber { get; set; }

            internal string Track1 { get; set; }

            internal string Track2 { get; set; }

            internal string CardToken { get; set; }

            internal string Last4Digit { get; set; }

            internal string AccountType { get; set; }

            internal string UniqueCardId { get; set; }

            internal string VoiceAuthorizationCode { get; set; }

            internal decimal? Amount { get; set; }

            internal string CurrencyCode { get; set; }

            internal string PurchaseLevel { get; set; }

            internal Level2Data Level2Data { get; set; }

            internal IEnumerable<Level3Data> Level3Data { get; set; }

            internal string AuthorizationProviderTransactionId { get; set; }

            internal string AuthorizationApprovalCode { get; set; }

            internal string AuthorizationResponseCode { get; set; }

            internal decimal? AuthorizationApprovedAmount { get; set; }

            internal decimal? AuthorizationCashbackAmount { get; set; }

            internal string AuthorizationResult { get; set; }

            internal string AuthorizationProviderMessage { get; set; }

            internal DateTime? AuthorizationTransactionDateTime { get; set; }

            internal string AuthorizationTransactionType { get; set; }

            internal static CaptureRequest ConvertFrom(Request request)
            {
                var captureRequest = new CaptureRequest();
                var errors = new List<PaymentError>();
                captureRequest.ReadBaseProperties(request, errors);

                // Check authorization response
                Hashtable hashtable = PaymentProperty.ConvertToHashtable(request.Properties);
                PaymentProperty authorizationResponsePropertyList = PaymentProperty.GetPropertyFromHashtable(hashtable, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.Properties);
                Hashtable authorizationHashtable = null;
                if (authorizationResponsePropertyList == null)
                {
                    errors.Add(new PaymentError(ErrorCode.InvalidRequest, "Authorization response is missing."));
                    throw new SampleException(errors);
                }
                else
                {
                    authorizationHashtable = PaymentProperty.ConvertToHashtable(authorizationResponsePropertyList.PropertyList);
                }

                // Read card data
                captureRequest.CardType = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.CardType,
                    errors,
                    ErrorCode.InvalidRequest);

                captureRequest.IsSwipe = PaymentUtilities.GetPropertyBooleanValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.IsSwiped);
                if (captureRequest.IsSwipe ?? false)
                {
                    captureRequest.Track1 = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.Track1);
                    captureRequest.Track2 = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.Track2);
                    captureRequest.CardNumber = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.CardNumber);

                    if (string.IsNullOrEmpty(captureRequest.CardNumber))
                    {
                        captureRequest.CardNumber = PaymentUtilities.ParseTrack1ForCardNumber(captureRequest.Track1);
                        if (captureRequest.CardNumber == null)
                        {
                            captureRequest.CardNumber = PaymentUtilities.ParseTrack2ForCardNumber(captureRequest.Track2);
                        }
                    }

                    if (string.IsNullOrEmpty(captureRequest.CardNumber))
                    {
                        errors.Add(new PaymentError(ErrorCode.InvalidCardTrackData, "Invalid card track data."));
                    }
                }
                else
                {
                    captureRequest.CardToken = PaymentUtilities.GetPropertyStringValue(
                        authorizationHashtable,
                        GenericNamespace.AuthorizationResponse,
                        AuthorizationResponseProperties.CardToken);
                    if (captureRequest.CardToken == null)
                    {
                        captureRequest.CardToken = PaymentUtilities.GetPropertyStringValue(
                            hashtable,
                            GenericNamespace.PaymentCard,
                            PaymentCardProperties.CardToken);
                        if (captureRequest.CardToken == null)
                        {
                            captureRequest.CardNumber = PaymentUtilities.GetPropertyStringValue(
                                hashtable,
                                GenericNamespace.PaymentCard,
                                PaymentCardProperties.CardNumber,
                                errors,
                                ErrorCode.InvalidCardNumber);
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(captureRequest.CardNumber)
                    && string.IsNullOrWhiteSpace(captureRequest.CardToken))
                {
                    errors.Add(new PaymentError(ErrorCode.InvalidRequest, string.Format("Neither card number nor card token is provided.")));
                }

                captureRequest.Last4Digit = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.Last4Digits);
                captureRequest.AccountType = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.AccountType);
                captureRequest.UniqueCardId = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.UniqueCardId);
                captureRequest.VoiceAuthorizationCode = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.VoiceAuthorizationCode);

                // Read transaction data
                captureRequest.Amount = PaymentUtilities.GetPropertyDecimalValue(
                    hashtable,
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.Amount,
                    errors,
                    ErrorCode.InvalidAmount);
                captureRequest.CurrencyCode = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.CurrencyCode,
                    errors,
                    ErrorCode.InvalidRequest);
                captureRequest.PurchaseLevel = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.PurchaseLevel);

                captureRequest.Level2Data = PaymentUtilities.GetLevel2Data(hashtable);
                captureRequest.Level3Data = PaymentUtilities.GetLevel3Data(hashtable);

                // Read authorization data
                captureRequest.AuthorizationTransactionType = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.TransactionType,
                    errors,
                    ErrorCode.InvalidRequest);
                if (captureRequest.AuthorizationTransactionType != null
                    && !TransactionType.Authorize.ToString().Equals(captureRequest.AuthorizationTransactionType, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(new PaymentError(ErrorCode.InvalidTransaction, "Capture does not support this type of transaction"));
                }

                captureRequest.AuthorizationApprovalCode = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.ApprovalCode);
                captureRequest.AuthorizationApprovedAmount = PaymentUtilities.GetPropertyDecimalValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.ApprovedAmount,
                    errors,
                    ErrorCode.InvalidRequest);
                captureRequest.AuthorizationCashbackAmount = PaymentUtilities.GetPropertyDecimalValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.CashBackAmount);
                captureRequest.AuthorizationProviderMessage = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.ProviderMessage);
                captureRequest.AuthorizationProviderTransactionId = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.ProviderTransactionId,
                    errors,
                    ErrorCode.InvalidRequest);
                captureRequest.AuthorizationResponseCode = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.ResponseCode);
                captureRequest.AuthorizationResult = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.AuthorizationResult,
                    errors,
                    ErrorCode.InvalidRequest);
                captureRequest.AuthorizationTransactionDateTime = PaymentUtilities.GetPropertyDateTimeValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.TransactionDateTime);

                if (errors.Count > 0)
                {
                    throw new SampleException(errors);
                }

                return captureRequest;
            }
        }
    }
}
