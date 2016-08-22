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
        using System.Collections.Generic;
        using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
        using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;

        internal class GenerateCardTokenResponse : ResponseBase
        {
            internal GenerateCardTokenResponse(string locale, string serviceAccountId, string connectorName)
                : base(locale, serviceAccountId, connectorName)
            {
            }

            internal string CardType { get; set; }

            internal string Last4Digit { get; set; }

            internal string CardToken { get; set; }

            internal string UniqueCardId { get; set; }

            internal decimal? ExpirationYear { get; set; }

            internal decimal? ExpirationMonth { get; set; }

            internal string Name { get; set; }

            internal string StreetAddress { get; set; }

            internal string StreetAddress2 { get; set; }

            internal string City { get; set; }

            internal string State { get; set; }

            internal string PostalCode { get; set; }

            internal string Country { get; set; }

            internal string Phone { get; set; }

            internal string AccountType { get; set; }

            internal IList<PaymentProperty> OtherCardProperties { get; set; }

            internal static Response ConvertTo(GenerateCardTokenResponse tokenizeResponse)
            {
                var response = new Response();
                tokenizeResponse.WriteBaseProperties(response);

                var properties = new List<PaymentProperty>();
                if (response.Properties != null)
                {
                    properties.AddRange(response.Properties);
                }

                PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.CardType, tokenizeResponse.CardType);
                PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.Last4Digits, tokenizeResponse.Last4Digit);
                PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.CardToken, tokenizeResponse.CardToken);
                PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.UniqueCardId, tokenizeResponse.UniqueCardId);
                PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.ExpirationYear, tokenizeResponse.ExpirationYear);
                PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.ExpirationMonth, tokenizeResponse.ExpirationMonth);
                PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.Name, tokenizeResponse.Name);
                PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.StreetAddress, tokenizeResponse.StreetAddress);
                PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.StreetAddress2, tokenizeResponse.StreetAddress2);
                PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.City, tokenizeResponse.City);
                PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.State, tokenizeResponse.State);
                PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.PostalCode, tokenizeResponse.PostalCode);
                PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.Country, tokenizeResponse.Country);
                PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.Phone, tokenizeResponse.Phone);
                PaymentUtilities.AddPropertyIfPresent(properties, GenericNamespace.PaymentCard, PaymentCardProperties.AccountType, tokenizeResponse.AccountType);
                properties.AddRange(tokenizeResponse.OtherCardProperties);

                response.Properties = properties.ToArray();
                return response;
            }
        }
    }
}
