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
        /// <summary>
        /// Represents an OpenId Connect user id token.
        /// </summary>
        public class AcsToken : UserToken
        {
            /// <summary>
            /// The identifier token scheme name.
            /// </summary>
            internal const string AcsTokenSchemeName = "acs_token";

            /// <summary>
            /// Initializes a new instance of the <see cref="AcsToken"/> class.
            /// </summary>
            /// <param name="acsToken">The OpenId Connect id token.</param>
            public AcsToken(string acsToken) : base(acsToken, AcsTokenSchemeName)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AcsToken"/> class.
            /// </summary>
            protected AcsToken() : base(AcsTokenSchemeName)
            {
            }
        }
    }
}
