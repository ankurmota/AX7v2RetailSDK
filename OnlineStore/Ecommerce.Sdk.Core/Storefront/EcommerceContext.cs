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
        /// Encapsulates the eCommerce context.
        /// </summary>
        public class EcommerceContext
        {
            /// <summary>
            /// Gets or sets the authentication token for the current session. 
            /// </summary>
            public string AuthenticationToken { get; set; }

            /// <summary>
            /// Gets or sets the identity provider type for the current session.
            /// </summary>
            public IdentityProviderType IdentityProviderType { get; set; }

            /// <summary>
            /// Gets or sets the user language locale.
            /// </summary>
            public string Locale { get; set; }

            /// <summary>
            /// Gets or sets the operating unit identifier.
            /// </summary>
            public string OperatingUnitId { get; set; }

            /// <summary>
            /// Gets an anonymous version of context.
            /// </summary>
            /// <returns>Anonymous context.</returns>
            public EcommerceContext GetAnonymousContext()
            {
                return new EcommerceContext()
                {
                    AuthenticationToken = null,
                    IdentityProviderType = IdentityProviderType.None,
                    Locale = this.Locale,
                    OperatingUnitId = this.OperatingUnitId
                };
            }
        }
    }
}