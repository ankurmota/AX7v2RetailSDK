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

        internal class AuthorizeResponse : ResponseBase
        {
            internal AuthorizeResponse(string locale, string serviceAccountId, string connectorName)
                : base(locale, serviceAccountId, connectorName)
            {
            }

            internal string CardType { get; set; }

            internal bool? IsSwipe { get; set; }

            internal string Last4Digit { get; set; }

            internal string CardToken { get; set; }

            internal string UniqueCardId { get; set; }

            internal string VoiceAuthorizationCode { get; set; }

            internal decimal? CashBackAmount { get; set; }

            internal string AccountType { get; set; }

            internal string ProviderTransactionId { get; set; }

            internal string ApprovalCode { get; set; }

            internal string ResponseCode { get; set; }

            internal decimal? ApprovedAmount { get; set; }

            internal string CurrencyCode { get; set; }

            internal string AuthorizationResult { get; set; }

            internal string ProviderMessage { get; set; }

            internal string AVSResult { get; set; }

            internal string AVSDetail { get; set; }

            internal string CVV2Result { get; set; }

            internal decimal? AvailableBalance { get; set; }

            internal string TransactionType { get; set; }

            internal DateTime? TransactionDateTime { get; set; }

            internal static Response ConvertTo(AuthorizeResponse authorizeResponse)
            {
                var response = new Response();
                authorizeResponse.WriteBaseProperties(response);

                var properties = new List<PaymentProperty>();
                if (response.Properties != null)
                {
                    properties.AddRange(response.Properties);
                }

                var authorizationRespnseProperties = new List<PaymentProperty>();
                PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.CardType, authorizeResponse.CardType);
                PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.IsSwiped, authorizeResponse.IsSwipe);
                PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.Last4Digits, authorizeResponse.Last4Digit);
                PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.CardToken, authorizeResponse.CardToken);
                PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.UniqueCardId, authorizeResponse.UniqueCardId);
                PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.VoiceAuthorizationCode, authorizeResponse.VoiceAuthorizationCode);
                PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.CashBackAmount, authorizeResponse.CashBackAmount);
                PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AccountType, authorizeResponse.AccountType);
                PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ProviderTransactionId, authorizeResponse.ProviderTransactionId);
                PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ApprovalCode, authorizeResponse.ApprovalCode);
                PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ResponseCode, authorizeResponse.ResponseCode);
                PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ApprovedAmount, authorizeResponse.ApprovedAmount);
                PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.CurrencyCode, authorizeResponse.CurrencyCode);
                PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AuthorizationResult, authorizeResponse.AuthorizationResult);
                PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.ProviderMessage, authorizeResponse.ProviderMessage);
                PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AVSResult, authorizeResponse.AVSResult);
                PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AVSDetail, authorizeResponse.AVSDetail);
                PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.CVV2Result, authorizeResponse.CVV2Result);
                PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.AvailableBalance, authorizeResponse.AvailableBalance);
                PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.TransactionType, authorizeResponse.TransactionType);
                PaymentUtilities.AddPropertyIfPresent(authorizationRespnseProperties, GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.TransactionDateTime, authorizeResponse.TransactionDateTime);
                properties.Add(new PaymentProperty(GenericNamespace.AuthorizationResponse, AuthorizationResponseProperties.Properties, authorizationRespnseProperties.ToArray()));

                response.Properties = properties.ToArray();
                return response;
            }
        }
    }
}
