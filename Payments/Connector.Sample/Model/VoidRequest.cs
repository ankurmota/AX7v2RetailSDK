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

        internal class VoidRequest : RequestBase
        {
            internal VoidRequest()
                : base()
            {
            }

            internal string CardType { get; set; }

            internal bool? IsSwipe { get; set; }

            internal string CardToken { get; set; }

            internal string Last4Digit { get; set; }

            internal string AccountType { get; set; }

            internal string UniqueCardId { get; set; }

            internal string VoiceAuthorizationCode { get; set; }

            internal string CurrencyCode { get; set; }

            internal string AuthorizationProviderTransactionId { get; set; }

            internal string AuthorizationApprovalCode { get; set; }

            internal string AuthorizationResponseCode { get; set; }

            internal decimal? AuthorizationApprovedAmount { get; set; }

            internal decimal? AuthorizationCashbackAmount { get; set; }

            internal string AuthorizationResult { get; set; }

            internal string AuthorizationProviderMessage { get; set; }

            internal DateTime? AuthorizationTransactionDateTime { get; set; }

            internal string AuthorizationTransactionType { get; set; }

            internal static VoidRequest ConvertFrom(Request request)
            {
                var voidRequest = new VoidRequest();
                var errors = new List<PaymentError>();
                voidRequest.ReadBaseProperties(request, errors);

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
                voidRequest.CardType = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.CardType,
                    errors,
                    ErrorCode.InvalidRequest);
                voidRequest.IsSwipe = PaymentUtilities.GetPropertyBooleanValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.IsSwiped);
                voidRequest.CardToken = PaymentUtilities.GetPropertyStringValue(
                        authorizationHashtable,
                        GenericNamespace.AuthorizationResponse,
                        AuthorizationResponseProperties.CardToken);
                voidRequest.Last4Digit = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.Last4Digits);
                voidRequest.AccountType = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.AccountType);
                voidRequest.UniqueCardId = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.UniqueCardId);
                voidRequest.VoiceAuthorizationCode = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.VoiceAuthorizationCode);

                // Read authorization data
                voidRequest.AuthorizationTransactionType = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.TransactionType,
                    errors,
                    ErrorCode.InvalidRequest);
                if (voidRequest.AuthorizationTransactionType != null
                    && !TransactionType.Authorize.ToString().Equals(voidRequest.AuthorizationTransactionType, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(new PaymentError(ErrorCode.InvalidTransaction, "Void does not support this type of transaction"));
                }

                voidRequest.AuthorizationApprovalCode = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.ApprovalCode);
                voidRequest.AuthorizationApprovedAmount = PaymentUtilities.GetPropertyDecimalValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.ApprovedAmount,
                    errors,
                    ErrorCode.InvalidRequest);
                voidRequest.CurrencyCode = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.CurrencyCode,
                    errors,
                    ErrorCode.InvalidRequest);
                voidRequest.AuthorizationCashbackAmount = PaymentUtilities.GetPropertyDecimalValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.CashBackAmount);
                voidRequest.AuthorizationProviderMessage = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.ProviderMessage);
                voidRequest.AuthorizationProviderTransactionId = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.ProviderTransactionId,
                    errors,
                    ErrorCode.InvalidRequest);
                voidRequest.AuthorizationResponseCode = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.ResponseCode);
                voidRequest.AuthorizationResult = PaymentUtilities.GetPropertyStringValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.AuthorizationResult,
                    errors,
                    ErrorCode.InvalidRequest);
                voidRequest.AuthorizationTransactionDateTime = PaymentUtilities.GetPropertyDateTimeValue(
                    authorizationHashtable,
                    GenericNamespace.AuthorizationResponse,
                    AuthorizationResponseProperties.TransactionDateTime);

                if (errors.Count > 0)
                {
                    throw new SampleException(errors);
                }

                return voidRequest;
            }
        }
    }
}
