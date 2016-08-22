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

        internal class VoidResponse : ResponseBase
        {
            internal VoidResponse(string locale, string serviceAccountId, string connectorName)
                : base(locale, serviceAccountId, connectorName)
            {
            }

            internal string CardType { get; set; }

            internal string Last4Digit { get; set; }

            internal string UniqueCardId { get; set; }

            internal string ProviderTransactionId { get; set; }

            internal string ResponseCode { get; set; }

            internal string CurrencyCode { get; set; }

            internal string VoidResult { get; set; }

            internal string ProviderMessage { get; set; }

            internal string TransactionType { get; set; }

            internal DateTime? TransactionDateTime { get; set; }

            internal static Response ConvertTo(VoidResponse voidResponse)
            {
                var response = new Response();
                voidResponse.WriteBaseProperties(response);

                var properties = new List<PaymentProperty>();
                if (response.Properties != null)
                {
                    properties.AddRange(response.Properties);
                }

                var voidRespnseProperties = new List<PaymentProperty>();
                PaymentUtilities.AddPropertyIfPresent(voidRespnseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.CardType, voidResponse.CardType);
                PaymentUtilities.AddPropertyIfPresent(voidRespnseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.Last4Digits, voidResponse.Last4Digit);
                PaymentUtilities.AddPropertyIfPresent(voidRespnseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.UniqueCardId, voidResponse.UniqueCardId);
                PaymentUtilities.AddPropertyIfPresent(voidRespnseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.ProviderTransactionId, voidResponse.ProviderTransactionId);
                PaymentUtilities.AddPropertyIfPresent(voidRespnseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.ResponseCode, voidResponse.ResponseCode);
                PaymentUtilities.AddPropertyIfPresent(voidRespnseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.CurrencyCode, voidResponse.CurrencyCode);
                PaymentUtilities.AddPropertyIfPresent(voidRespnseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.VoidResult, voidResponse.VoidResult);
                PaymentUtilities.AddPropertyIfPresent(voidRespnseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.ProviderMessage, voidResponse.ProviderMessage);
                PaymentUtilities.AddPropertyIfPresent(voidRespnseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.TransactionType, voidResponse.TransactionType);
                PaymentUtilities.AddPropertyIfPresent(voidRespnseProperties, GenericNamespace.VoidResponse, VoidResponseProperties.TransactionDateTime, voidResponse.TransactionDateTime);
                properties.Add(new PaymentProperty(GenericNamespace.VoidResponse, VoidResponseProperties.Properties, voidRespnseProperties.ToArray()));

                response.Properties = properties.ToArray();
                return response;
            }
        }
    }
}
