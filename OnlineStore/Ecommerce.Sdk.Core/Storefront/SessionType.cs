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
        /// <summary>
        /// Used to indicate the type of the web session.
        /// </summary>
        public enum SessionType
        {
            /// <summary>
            /// When session type is not set.
            /// </summary>
            None,

            /// <summary>
            /// The shopping/browsing session of an anonymous user.
            /// </summary>
            AnonymousShopping,

            /// <summary>
            /// The checkout session of an anonymous user.
            /// </summary>
            AnonymousCheckout,

            /// <summary>
            /// The shopping/browsing session of a signed-in user.
            /// </summary>
            SignedInShopping,

            /// <summary>
            /// The checkout session of an signed-in user.
            /// </summary>
            SignedInCheckout
        }
    }
}
