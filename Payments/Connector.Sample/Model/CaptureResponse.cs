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

        internal class CaptureResponse : ResponseBase
        {
            internal CaptureResponse(string locale, string serviceAccountId, string connectorName)
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

            internal string CurrencyCode { get; set; }

            internal string CaptureResult { get; set; }

            internal string ProviderMessage { get; set; }

            internal string TransactionType { get; set; }

            internal DateTime? TransactionDateTime { get; set; }

            internal static Response ConvertTo(CaptureResponse captureResponse)
            {
                var response = new Response();
                captureResponse.WriteBaseProperties(response);

                var properties = new List<PaymentProperty>();
                if (response.Properties != null)
                {
                    properties.AddRange(response.Properties);
                }

                var captureRespnseProperties = new List<PaymentProperty>();
                PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CardType, captureResponse.CardType);
                PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.Last4Digits, captureResponse.Last4Digit);
                PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CardToken, captureResponse.CardToken);
                PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.UniqueCardId, captureResponse.UniqueCardId);
                PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.ProviderTransactionId, captureResponse.ProviderTransactionId);
                PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.ApprovalCode, captureResponse.ApprovalCode);
                PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.ResponseCode, captureResponse.ResponseCode);
                PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CurrencyCode, captureResponse.CurrencyCode);
                PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.CaptureResult, captureResponse.CaptureResult);
                PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.ProviderMessage, captureResponse.ProviderMessage);
                PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.TransactionType, captureResponse.TransactionType);
                PaymentUtilities.AddPropertyIfPresent(captureRespnseProperties, GenericNamespace.CaptureResponse, CaptureResponseProperties.TransactionDateTime, captureResponse.TransactionDateTime);
                properties.Add(new PaymentProperty(GenericNamespace.CaptureResponse, CaptureResponseProperties.Properties, captureRespnseProperties.ToArray()));

                response.Properties = properties.ToArray();
                return response;
            }
        }
    }
}
