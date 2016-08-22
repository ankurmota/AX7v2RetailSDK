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
    namespace RetailServer.TestClient
    {
        using Commerce.RetailProxy.Authentication;

        /// <summary>
        /// The AAD-based user token.
        /// </summary>
        internal sealed class AADToken : UserToken
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AADToken"/> class.
            /// </summary>
            /// <param name="token">The token.</param>
            public AADToken(string token) : base(token, "Bearer")
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AADToken"/> class.
            /// </summary>
            public AADToken() : base("Bearer")
            {
            }
        }
    }
}