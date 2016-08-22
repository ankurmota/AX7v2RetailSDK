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
    namespace Commerce.RetailProxy.Authentication
    {
        using Microsoft.Dynamics.Commerce.Runtime;

        /// <summary>
        /// Represents an OpenId Connect user id token.
        /// </summary>
        public class UserIdToken : UserToken
        {
            /// <summary>
            /// The identifier token scheme name.
            /// </summary>
            internal const string IdTokenSchemeName = "id_token";
    
            /// <summary>
            /// Initializes a new instance of the <see cref="UserIdToken"/> class.
            /// </summary>
            /// <param name="idToken">The OpenId Connect id token.</param>
            public UserIdToken(string idToken) : base(idToken, IdTokenSchemeName)
            {
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="UserIdToken"/> class.
            /// </summary>
            protected UserIdToken() : base(IdTokenSchemeName)
            {
            }
        }
    }
}
