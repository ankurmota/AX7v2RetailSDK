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

        internal class RefundResponse : ResponseBase
        {
            internal RefundResponse(string locale, string serviceAccountId, string connectorName)
                : base(locale, serviceAccountId, connectorName)
            {
            }

            internal string CardType { get; set; }

            internal string Last4Digit { get; set; }

            internal string CardToken { get; set; }

            internal string UniqueCardId { get; set; }

            internal string ProviderTransactionId { get; set; }

            internal string ApprovalCode { get; set; }

            internal string ResponseCode { get; set; }

            internal decimal? ApprovedAmount { get; set; }

            internal string CurrencyCode { get; set; }

            internal string RefundResult { get; set; }

            internal string ProviderMessage { get; set; }

            internal string TransactionType { get; set; }

            internal DateTime? TransactionDateTime { get; set; }

            internal static Response ConvertTo(RefundResponse refundResponse)
            {
                var response = new Response();
                refundResponse.WriteBaseProperties(response);

                var properties = new List<PaymentProperty>();
                if (response.Properties != null)
                {
                    properties.AddRange(response.Properties);
                }

                var refundRespnseProperties = new List<PaymentProperty>();
                PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.CardType, refundResponse.CardType);
                PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.Last4Digits, refundResponse.Last4Digit);
                PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.CardToken, refundResponse.CardToken);
                PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.UniqueCardId, refundResponse.UniqueCardId);
                PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.ProviderTransactionId, refundResponse.ProviderTransactionId);
                PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.ApprovalCode, refundResponse.ApprovalCode);
                PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.ResponseCode, refundResponse.ResponseCode);
                PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.ApprovedAmount, refundResponse.ApprovedAmount);
                PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.CurrencyCode, refundResponse.CurrencyCode);
                PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.RefundResult, refundResponse.RefundResult);
                PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.ProviderMessage, refundResponse.ProviderMessage);
                PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.TransactionType, refundResponse.TransactionType);
                PaymentUtilities.AddPropertyIfPresent(refundRespnseProperties, GenericNamespace.RefundResponse, RefundResponseProperties.TransactionDateTime, refundResponse.TransactionDateTime);
                properties.Add(new PaymentProperty(GenericNamespace.RefundResponse, RefundResponseProperties.Properties, refundRespnseProperties.ToArray()));

                response.Properties = properties.ToArray();
                return response;
            }
        }
    }
}
