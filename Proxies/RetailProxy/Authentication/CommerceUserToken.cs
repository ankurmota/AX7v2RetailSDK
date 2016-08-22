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
        /// Represents the user token issued by <see cref="ICommerceAuthenticationContext"/>.
        /// </summary>
        internal class CommerceUserToken : UserToken
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CommerceUserToken"/> class.
            /// </summary>
            /// <param name="retailServerToken">The user token that can be used to authenticate Retail Server calls.</param>
            internal CommerceUserToken(UserIdToken retailServerToken) : base(UserIdToken.IdTokenSchemeName)
            {
                ThrowIf.Null(retailServerToken, "retailServerToken");
                this.RetailServerToken = retailServerToken;
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="CommerceUserToken"/> class.
            /// </summary>
            /// <param name="commerceRuntimeToken">The user token that can be used to authenticate Commerce Runtime calls.</param>
            internal CommerceUserToken(CommerceRuntimeUserToken commerceRuntimeToken) : base(CommerceRuntimeUserToken.CommerceRuntimeTokenSchemeName)
            {
                ThrowIf.Null(commerceRuntimeToken, "commerceRuntimeToken");
                this.CommerceRuntimeToken = commerceRuntimeToken;
            }
    
            /// <summary>
            /// Gets the token.
            /// </summary>
            /// <remarks>Favors the token to be used for retail server calls.</remarks>
            public override string Token
            {
                get
                {
                    return this.RetailServerToken == null ? this.CommerceRuntimeToken.Token : this.RetailServerToken.Token;
                }
            }
    
            /// <summary>
            /// Gets the token scheme name.
            /// </summary>
            /// <remarks>Favors the token to be used for retail server calls.</remarks>
            public override string SchemeName
            {
                get
                {
                    return this.RetailServerToken == null ? this.CommerceRuntimeToken.SchemeName : this.RetailServerToken.SchemeName;
                }
            }
    
            /// <summary>
            /// Gets or sets the user token to be used for Commerce Runtime calls.
            /// </summary>
            internal CommerceRuntimeUserToken CommerceRuntimeToken { get; set; }
    
            /// <summary>
            /// Gets or sets the user token to be used for Retail Server calls.
            /// </summary>
            internal UserIdToken RetailServerToken { get; set; }
        }
    }
}
