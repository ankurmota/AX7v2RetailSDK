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

        internal class AuthorizeRequest : RequestBase
        {
            internal AuthorizeRequest()
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

            internal string VoiceAuthorizationCode { get; set; }

            internal decimal? CashBackAmount { get; set; }

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

            internal bool? AllowPartialAuthorization { get; set; }

            internal string AuthorizationProviderTransactionId { get; set; }

            internal string PurchaseLevel { get; set; }

            internal static AuthorizeRequest ConvertFrom(Request request)
            {
                var authorizeRequest = new AuthorizeRequest();
                var errors = new List<PaymentError>();
                authorizeRequest.ReadBaseProperties(request, errors);

                // Read card data
                Hashtable hashtable = PaymentProperty.ConvertToHashtable(request.Properties);
                authorizeRequest.CardType = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.CardType,
                    errors,
                    ErrorCode.InvalidRequest);
                if (authorizeRequest.CardType != null
                    && !PaymentUtilities.ValidateCardType(authorizeRequest.SupportedTenderTypes, authorizeRequest.CardType))
                {
                    errors.Add(new PaymentError(ErrorCode.CardTypeNotSupported, string.Format("Card type is not supported: {0}.", authorizeRequest.CardType)));
                }

                authorizeRequest.IsSwipe = PaymentUtilities.GetPropertyBooleanValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.IsSwipe);
                if (authorizeRequest.IsSwipe ?? false)
                {
                    authorizeRequest.Track1 = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.Track1);
                    authorizeRequest.Track2 = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.Track2);

                    authorizeRequest.CardNumber = PaymentUtilities.ParseTrack1ForCardNumber(authorizeRequest.Track1);
                    if (authorizeRequest.CardNumber == null)
                    {
                        authorizeRequest.CardNumber = PaymentUtilities.ParseTrack2ForCardNumber(authorizeRequest.Track2);
                    }

                    if (authorizeRequest.CardNumber == null)
                    {
                        errors.Add(new PaymentError(ErrorCode.InvalidCardTrackData, "Invalid card track data."));
                    }

                    decimal expirationYear, expirationMonth;
                    HelperUtilities.ParseTrackDataForExpirationDate(authorizeRequest.Track1 ?? string.Empty, authorizeRequest.Track2 ?? string.Empty, out expirationYear, out expirationMonth);
                    authorizeRequest.ExpirationYear = expirationYear;
                    authorizeRequest.ExpirationMonth = expirationMonth;
                }
                else
                {
                    authorizeRequest.CardToken = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.CardToken);

                    if (authorizeRequest.CardToken == null)
                    {
                        authorizeRequest.CardNumber = PaymentUtilities.GetPropertyStringValue(
                            hashtable,
                            GenericNamespace.PaymentCard,
                            PaymentCardProperties.CardNumber,
                            errors,
                            ErrorCode.InvalidCardNumber);
                    }
                    else
                    {
                        authorizeRequest.Last4Digit = PaymentUtilities.GetPropertyStringValue(
                            hashtable,
                            GenericNamespace.PaymentCard,
                            PaymentCardProperties.Last4Digits);
                    }

                    authorizeRequest.ExpirationYear = PaymentUtilities.GetPropertyDecimalValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.ExpirationYear,
                        errors,
                        ErrorCode.InvalidExpirationDate);
                    authorizeRequest.ExpirationMonth = PaymentUtilities.GetPropertyDecimalValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.ExpirationMonth,
                        errors,
                        ErrorCode.InvalidExpirationDate);
                }

                if (authorizeRequest.CardNumber != null
                    && !HelperUtilities.ValidateBankCardNumber(authorizeRequest.CardNumber))
                {
                    errors.Add(new PaymentError(ErrorCode.InvalidCardNumber, "Invalid card number."));
                }

                if (authorizeRequest.ExpirationYear.HasValue
                    && authorizeRequest.ExpirationMonth.HasValue
                    && authorizeRequest.ExpirationYear >= 0M
                    && authorizeRequest.ExpirationMonth >= 0M
                    && !PaymentUtilities.ValidateExpirationDate(authorizeRequest.ExpirationYear.Value, authorizeRequest.ExpirationMonth.Value))
                {
                    errors.Add(new PaymentError(ErrorCode.InvalidExpirationDate, "Invalid expiration date."));
                }

                if (Microsoft.Dynamics.Retail.PaymentSDK.Portable.CardType.Debit.ToString().Equals(authorizeRequest.CardType, StringComparison.OrdinalIgnoreCase))
                {
                    authorizeRequest.EncryptedPin = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.EncryptedPin,
                        errors,
                        ErrorCode.CannotVerifyPin);
                    authorizeRequest.AdditionalSecurityData = PaymentUtilities.GetPropertyStringValue(
                        hashtable,
                        GenericNamespace.PaymentCard,
                        PaymentCardProperties.AdditionalSecurityData);
                }

                authorizeRequest.CashBackAmount = PaymentUtilities.GetPropertyDecimalValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.CashBackAmount);
                if (authorizeRequest.CashBackAmount.HasValue
                    && authorizeRequest.CashBackAmount > 0M
                    && !Microsoft.Dynamics.Retail.PaymentSDK.Portable.CardType.Debit.ToString().Equals(authorizeRequest.CardType, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(new PaymentError(ErrorCode.CashBackNotAvailable, "Cashback is not available."));
                }

                authorizeRequest.CardVerificationValue = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.CardVerificationValue);
                authorizeRequest.VoiceAuthorizationCode = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.VoiceAuthorizationCode);
                authorizeRequest.Name = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.Name);
                authorizeRequest.StreetAddress = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.StreetAddress);
                authorizeRequest.StreetAddress2 = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.StreetAddress2);
                authorizeRequest.City = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.City);
                authorizeRequest.State = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.State);
                authorizeRequest.PostalCode = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.PostalCode);
                authorizeRequest.Country = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.Country);
                authorizeRequest.Phone = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.Phone);
                authorizeRequest.AccountType = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.AccountType);
                authorizeRequest.UniqueCardId = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.PaymentCard,
                    PaymentCardProperties.UniqueCardId);

                // Read transaction data
                authorizeRequest.Amount = PaymentUtilities.GetPropertyDecimalValue(
                    hashtable,
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.Amount,
                    errors,
                    ErrorCode.InvalidAmount);
                authorizeRequest.CurrencyCode = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.CurrencyCode,
                    errors,
                    ErrorCode.InvalidRequest);
                if (authorizeRequest.CurrencyCode != null
                    && !PaymentUtilities.ValidateCurrencyCode(authorizeRequest.SupportedCurrencies, authorizeRequest.CurrencyCode))
                {
                    errors.Add(new PaymentError(ErrorCode.UnsupportedCurrency, string.Format("Currency code is not supported: {0}.", authorizeRequest.CurrencyCode)));
                }

                authorizeRequest.SupportCardTokenization = PaymentUtilities.GetPropertyBooleanValue(
                    hashtable,
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.SupportCardTokenization);
                authorizeRequest.AllowPartialAuthorization = PaymentUtilities.GetPropertyBooleanValue(
                    hashtable,
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.AllowPartialAuthorization);

                authorizeRequest.AuthorizationProviderTransactionId = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.AuthorizationProviderTransactionId);

                authorizeRequest.PurchaseLevel = PaymentUtilities.GetPropertyStringValue(
                    hashtable,
                    GenericNamespace.TransactionData,
                    TransactionDataProperties.PurchaseLevel);

                if (errors.Count > 0)
                {
                    throw new SampleException(errors);
                }

                return authorizeRequest;
            }
        }
    }
}