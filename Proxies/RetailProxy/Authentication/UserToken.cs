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
        /// The user token.
        /// </summary>
        public abstract class UserToken
        {
            private readonly string token;
            private readonly string schemeName;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="UserToken"/> class.
            /// </summary>
            /// <param name="token">The token.</param>
            /// <param name="schemeName">Name of the authentication scheme for the token.</param>
            protected UserToken(string token, string schemeName)
            {
                ThrowIf.NullOrWhiteSpace(token, "token");
                ThrowIf.NullOrWhiteSpace(schemeName, "schemeName");
                this.token = token;
                this.schemeName = schemeName;
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="UserToken"/> class.
            /// </summary>
            /// <param name="schemeName">Name of the authentication scheme for the token.</param>
            protected UserToken(string schemeName)
            {
                ThrowIf.NullOrWhiteSpace(schemeName, "schemeName");
                this.schemeName = schemeName;
            }
    
            /// <summary>
            /// Gets the token.
            /// </summary>
            public virtual string Token
            {
                get
                {
                    return this.token;
                }
            }
    
            /// <summary>
            /// Gets the name of the scheme.
            /// </summary>
            public virtual string SchemeName
            {
                get
                {
                    return this.schemeName;
                }
            }
        }
    }
}
