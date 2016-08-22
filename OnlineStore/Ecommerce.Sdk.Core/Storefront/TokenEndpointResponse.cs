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
    namespace Retail.Ecommerce.Sdk.Core
    {
        using System.Runtime.Serialization;
    
        /// <summary>
        /// Encapsulates properties returned by a request to the Token Endpoint.
        /// </summary>
        [DataContract]
        public class TokenEndpointResponse
        {
            /// <summary>
            /// Gets or sets the access_token.
            /// </summary>
            [DataMember(Name = "access_token")]
            public string AccessToken { get; set; }
    
            /// <summary>
            /// Gets or sets the id_token.
            /// </summary>
            [DataMember(Name = "id_token")]
            public string IdToken { get; set; }
    
            /// <summary>
            /// Gets or sets the token's expiration.
            /// </summary>
            [DataMember(Name = "expires_in")]
            public string ExpiresIn { get; set; }
    
            /// <summary>
            /// Gets or sets the token's type.
            /// </summary>
            [DataMember(Name = "token_type")]
            public string TokenType { get; set; }
        }
    }
}