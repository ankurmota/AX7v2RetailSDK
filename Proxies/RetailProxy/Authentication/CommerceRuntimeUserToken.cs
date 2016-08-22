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
        using Commerce.RetailProxy.Adapters;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;

        /// <summary>
        /// Represents a user token issued by <see cref="CommerceAuthenticationRuntimeContext"/>.
        /// </summary>
        /// <remarks>This type of user token is meant to be used on Commerce Runtime calls.</remarks>
        internal class CommerceRuntimeUserToken : UserToken
        {
            /// <summary>
            /// The Commerce Runtime token authentication scheme name.
            /// </summary>
            internal const string CommerceRuntimeTokenSchemeName = "commerceruntime_token";

            private string commerceRuntimeToken;

            /// <summary>
            /// Initializes a new instance of the <see cref="CommerceRuntimeUserToken"/> class.
            /// </summary>
            /// <param name="commerceIdentity">The commerce identity.</param>
            public CommerceRuntimeUserToken(CommerceIdentity commerceIdentity) : this(null, commerceIdentity)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CommerceRuntimeUserToken"/> class.
            /// </summary>
            /// <param name="initialIdentity">The initial commerce identity.</param>
            /// <param name="newIdentity">The new commerce identity.</param>
            public CommerceRuntimeUserToken(CommerceIdentity initialIdentity, CommerceIdentity newIdentity) : base(CommerceRuntimeTokenSchemeName)
            {
                ThrowIf.Null(newIdentity, "newIdentity");

                if (initialIdentity == null)
                {
                    initialIdentity = new CommerceIdentity();
                }

                CommerceRuntimeManager.UpdateCommerceIdentity(initialIdentity, newIdentity);

                this.commerceRuntimeToken = Newtonsoft.Json.JsonConvert.SerializeObject(initialIdentity);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CommerceRuntimeUserToken"/> class.
            /// </summary>
            /// <param name="token">The token raw value.</param>
            public CommerceRuntimeUserToken(string token)
                : this(Newtonsoft.Json.JsonConvert.DeserializeObject<CommerceIdentity>(token))
            {
            }

            /// <summary>
            /// Gets the token.
            /// </summary>
            public override string Token
            {
                get { return this.commerceRuntimeToken; }
            }
        }
    }
}
