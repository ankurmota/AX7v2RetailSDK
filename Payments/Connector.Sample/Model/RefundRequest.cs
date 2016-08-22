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

        internal class RefundRequest : RequestBase
        {
            internal RefundRequest()
                : base()
            {
            }

            internal string CardType { get; set; }

            internal bool? IsSwipe { get; set; }

            internal string CardNumber { get; set; }

            internal string Track1 { get; set; }

            internal string Track2 { get; set; }

            internal decimal? ExpirationYear { get; set; }

            internal decimal? ExpirationMonth { get; set; }

            internal string CardToken { get; set; }

            internal string Last4Digit { get; set; }

            internal string EncryptedPin { get; set; }

            internal string AdditionalSecurityData { get; set; }

            internal string CardVerificationValue { get; set; }

            internal string Name { get; set; }

            internal string StreetAddress { get; set; }

            internal string StreetAddress2 { get; set; }

            internal string City { get; set; }

            internal string State { get; set; }

            internal string PostalCode { get; set; }

            internal string Country { get; set; }

            internal string Phone { get; set; }

            internal string AccountType { get; set; }

            internal string UniqueCardId { get; set; }

            internal decimal? Amount { get; set; }

            internal string CurrencyCode { get; set; }

            internal bool? SupportCardTokenization { get; set; }

            internal string PurchaseLevel { get; set; }

            internal Level2Data Level2Data { get; set; }

            internal IEnumerable<Level3Data> Level3Data { get; set; }

            internal bool IsLinkedRefund { get; set; }

            internal string CaptureResult { get; set; }

            internal string CaptureApprovalCode { get; set; }

            internal string CaptureResponseCode { get; set; }

            internal string CaptureProviderMessage { get; set; }

            internal string CaptureProviderTransactionId { get; set; }

            internal string CaptureTransactionType { get; set; }

            internal DateTime? CaptureTransactionDateTime { get; set; }

            internal static RefundRequest ConvertFrom(Request request)
            {
                var refundRequest = new RefundRequest();
                var errors = new List<PaymentError>();
                refundRequest.ReadBaseProperties(request, errors);

                // Check capture response
                Hashtable hashtable = PaymentProperty.ConvertToHashtable(request.Properties);
                PaymentProperty captureResponsePropertyList = PaymentProperty.GetPropertyFromHashtable(hashtable, GenericNamespace.CaptureResponse, CaptureResponseProperties.Properties);

                // Read card data
                if (captureResponsePropertyList != null)
                {
                    refundRequest.IsLinkedRefund = true;

                    // Linked refund, get card data from CaptureResponse
                    Hashtable captureHashtable = PaymentProperty.ConvertToHashtable(captureResponsePropertyList.PropertyList);
                    refundRequest.CardType = PaymentUtilities.GetPropertyStringValue(
                        captureHashtable,
                        GenericNamespace.CaptureResponse,
                        CaptureResponseProperties.CardType,
                        errors,
                        ErrorCode.InvalidRequest);
                    refundRequest.CardToken = PaymentUtilities.GetPropertyStringValue(
                        captureHashtable,
                        GenericNamespace.CaptureResponse,
                        CaptureResponseProperties.CardToken);
                    refundRequest.Last4Digit = PaymentUtilities.GetPropertyStringValue(
                        captureHashtable,
                        GenericNamespace.CaptureResponse,
                        CaptureResponseProperties.Last4Digits);
                    refundRequest.UniqueCardId = PaymentUtilities.GetPropertyStringValue(
                        captureHashtable,
                        GenericNamespace.CaptureResponse,
                        CaptureResponseProperties.UniqueCardId);

                    // Get other capture transaction data
                    refundRequest.CaptureTransactionType = PaymentUtilities.GetPropertyStringValue(
                        captureHashtable,
                        GenericNamespace.CaptureResponse,
                        CaptureResponseProperties.TransactionType,
                        errors,
                        ErrorCode.InvalidRequest);
                    if (refundRequest.CaptureTransactionType != null
                        && !TransactionType.Capture.ToString().Equals(refundRequest.CaptureTransactionType, StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add(new PaymentError(ErrorCode.InvalidTransaction, "Refund does not support this type of transaction"));
                    }

                    refundRequest.CaptureApprovalCode = PaymentUtilities.GetPropertyStringValue(
                        captureHashtable,
                        GenericNamespace.CaptureResponse,
                        CaptureResponseProperties.ApprovalCode);
                    refundRequest.CaptureProviderMessage = PaymentUtilities.GetPropertyStringValue(
                        captureHashtable,
                        GenericNamespace.CaptureResponse,
                        CaptureResponseProperties.ProviderMessage);
                    refundRequest.CaptureProviderTransactionId = PaymentUtilities.GetPropertyStringValue(
                        captureHashtable,
                        GenericNamespace.CaptureResponse,
                        CaptureResponseProperties.ProviderTransactionId,
                        errors,
                        ErrorCode.InvalidRequest);
                    refundRequest.CaptureResponseCode = PaymentUtilities.GetPropertyStringValue(
                        captureHashtable,
                        GenericNamespace.CaptureResponse,
                        CaptureResponseProperties.ResponseCode);
                    refundRequest.CaptureResult = PaymentUtilities.GetPropertyStringValue(
                        captureHashtable,
                        GenericNamespace.CaptureResponse,
                        CaptureResponseProperties.CaptureResult,
                        errors,
                        ErrorCode.InvalidRequest);
                    refundRequest.CaptureTransactionDateTime = PaymentUtilities.GetPropertyDateTimeValue(
                        captureHashtable,
                        GenericNamespace.CaptureResponse,
                        CaptureResponseProperties.TransactionDateTime);
                }
                else
                {
                    // Not a linked refund, get card data from PaymentCard
                    refundRequest.CardType = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.CardType,
                    errors,
                    ErrorCode.InvalidRequest);
                    if (refundRequest.CardType != null
                        && !PaymentUtilities.ValidateCardType(refundRequest.SupportedTenderTypes, refundRequest.CardType))
                    {
                        errors.Add(new PaymentError(ErrorCode.CardTypeNotSupported, string.Format("Card type is not supported: {0}.", refundRequest.CardType)));
                    }

                    refundRequest.IsSwipe = PaymentUtilities.GetPropertyBooleanValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.IsSwipe);
                    if (refundRequest.IsSwipe ?? false)
                    {
                        refundRequest.Track1 = PaymentUtilities.GetPropertyStringValue(
                            hashtable,
                            GenericNamespace.PaymentCard,
                            PaymentCardProperties.Track1);
                        refundRequest.Track2 = PaymentUtilities.GetPropertyStringValue(
                            hashtable,
                            GenericNamespace.PaymentCard,
                            PaymentCardProperties.Track2);

                        refundRequest.CardNumber = PaymentUtilities.ParseTrack1ForCardNumber(refundRequest.Track1);
                        if (refundRequest.CardNumber == null)
                        {
                            refundRequest.CardNumber = PaymentUtilities.ParseTrack2ForCardNumber(refundRequest.Track2);
                        }

                        if (refundRequest.CardNumber == null)
                        {
                            errors.Add(new PaymentError(ErrorCode.InvalidCardTrackData, "Invalid card track data."));
                        }

                        decimal expirationYear, expirationMonth;
                        HelperUtilities.ParseTrackDataForExpirationDate(refundRequest.Track1 ?? string.Empty, refundRequest.Track2 ?? string.Empty, out expirationYear, out expirationMonth);
                        refundRequest.ExpirationYear = expirationYear;
                        refundRequest.ExpirationMonth = expirationMonth;
                    }
                    else
                    {
                        refundRequest.CardToken = PaymentUtilities.GetPropertyStringValue(
                            hashtable,
                            GenericNamespace.PaymentCard,
                            PaymentCardProperties.CardToken);

                        if (refundRequest.CardToken == null)
                        {
                            refundRequest.CardNumber = PaymentUtilities.GetPropertyStringValue(
                                hashtable,
                                GenericNamespace.PaymentCard,
                                PaymentCardProperties.CardNumber,
                                errors,
                                ErrorCode.InvalidCardNumber);
                        }
                        else
                        {
                            refundRequest.Last4Digit = PaymentUtilities.GetPropertyStringValue(
                                hashtable,
                                GenericNamespace.PaymentCard,
                                PaymentCardProperties.Last4Digits);
                        }

                        refundRequest.ExpirationYear = PaymentUtilities.GetPropertyDecimalValue(
                            hashtable,
                            GenericNamespace.PaymentCard,
                            PaymentCardProperties.ExpirationYear,
                            errors,
                            ErrorCode.InvalidExpirationDate);
                        refundRequest.ExpirationMonth = PaymentUtilities.GetPropertyDecimalValue(
                            hashtable,
                            GenericNamespace.PaymentCard,
                            PaymentCardProperties.ExpirationMonth,
                            errors,
                            ErrorCode.InvalidExpirationDate);
                    }

                    if (refundRequest.CardNumber != null
                        && !HelperUtilities.ValidateBankCardNumber(refundRequest.CardNumber))
                    {
                        errors.Add(new PaymentError(ErrorCode.InvalidCardNumber, "Invalid card number."));
                    }

                    if (refundRequest.ExpirationYear.HasValue
                        && refundRequest.ExpirationMonth.HasValue
                        && refundRequest.ExpirationYear >= 0M
                        && refundRequest.ExpirationMonth >= 0M
                        && !PaymentUtilities.ValidateExpirationDate(refundRequest.ExpirationYear.Value, refundRequest.ExpirationMonth.Value))
                    {
                        errors.Add(new PaymentError(ErrorCode.InvalidExpirationDate, "Invalid expiration date."));
                    }

                    if (Microsoft.Dynamics.Retail.PaymentSDK.Portable.CardType.Debit.ToString().Equals(refundRequest.CardType, StringComparison.OrdinalIgnoreCase))
                    {
                        refundRequest.EncryptedPin = PaymentUtilities.GetPropertyStringValue(
                            hashtable,
                            GenericNamespace.PaymentCard,
                            PaymentCardProperties.EncryptedPin,
                            errors,
                            ErrorCode.CannotVerifyPin);
                        refundRequest.AdditionalSecurityData = PaymentUtilities.GetPropertyStringValue(
                            hashtable,
                            GenericNamespace.PaymentCard,
                            PaymentCardProperties.AdditionalSecurityData);
                    }

                    refundRequest.CardVerificationValue = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.CardVerificationValue);
                    refundRequest.Name = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.Name);
                    refundRequest.StreetAddress = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.StreetAddress);
                    refundRequest.StreetAddress2 = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.StreetAddress2);
                    refundRequest.City = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.City);
                    refundRequest.State = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.State);
                    refundRequest.PostalCode = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.PostalCode);
                    refundRequest.Country = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.Country);
                    refundRequest.Phone = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.Phone);
                    refundRequest.AccountType = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.AccountType);
                    refundRequest.UniqueCardId = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.UniqueCardId);
                }

                // Read transaction data
                refundRequest.Amount = PaymentUtilities.GetPropertyDecimalValue(
                    hashtable,
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.Amount,
                    errors,
                    ErrorCode.InvalidAmount);
                refundRequest.CurrencyCode = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.CurrencyCode,
                    errors,
                    ErrorCode.InvalidRequest);
                if (refundRequest.CurrencyCode != null
                    && !PaymentUtilities.ValidateCurrencyCode(refundRequest.SupportedCurrencies, refundRequest.CurrencyCode))
                {
                    errors.Add(new PaymentError(ErrorCode.UnsupportedCurrency, string.Format("Currency code is not supported: {0}.", refundRequest.CurrencyCode)));
                }

                refundRequest.SupportCardTokenization = PaymentUtilities.GetPropertyBooleanValue(
                    hashtable,
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.SupportCardTokenization);

                refundRequest.PurchaseLevel = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.PurchaseLevel);

                refundRequest.Level2Data = PaymentUtilities.GetLevel2Data(hashtable);
                refundRequest.Level3Data = PaymentUtilities.GetLevel3Data(hashtable);

                if (errors.Count > 0)
                {
                    throw new SampleException(errors);
                }

                return refundRequest;
            }
        }
    }
}
