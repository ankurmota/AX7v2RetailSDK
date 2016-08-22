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
        /// Centralized location for managing all of the cookies.
        /// </summary>
        public static class CookieConstants
        {
            /// <summary>
            /// Cookie name for checkout cart token for the anonymous user.
            /// </summary>
            public const string AnonymousCheckoutCartToken = "cct";

            /// <summary>
            /// Cookie name for shopping cart token for the anonymous user.
            /// </summary>
            public const string AnonymousShoppingCartToken = "sct";

            /// <summary>
            /// Name for application cookie type of authentication.
            /// </summary>
            /// <remarks>This is the same value as Microsoft.AspNet.Identity.DefaultAuthenticationTypes.ApplicationCookie.</remarks>
            public const string ApplicationCookieAuthenticationType = "ApplicationCookie";

            /// <summary>
            /// The authentication type.
            /// </summary>
            public const string AuthenticationType = "AuthenticationType";

            /// <summary>
            /// The caption.
            /// </summary>
            public const string Caption = "Caption";

            /// <summary>
            /// Cookie name for the culture Id.
            /// </summary>
            public const string CultureIdToken = "cuid";

            /// <summary>
            /// The customer account number.
            /// </summary>
            public const string CustomerAccountNumber = "CustomerAccountNumber";

            /// <summary>
            /// The email.
            /// </summary>
            public const string Email = "Email";

            /// <summary>
            /// Name for the container that contains first half of the external authentication token.
            /// </summary>
            public const string ExternalTokenPart1 = "t1";

            /// <summary>
            /// Name for the container that contains second half of the external authentication token.
            /// </summary>
            public const string ExternalTokenPart2 = "t2";

            /// <summary>
            /// The first name.
            /// </summary>
            public const string FirstName = "FN";

            /// <summary>
            /// The identity provider type.
            /// </summary>
            public const string IdentityProviderType = "IdentityProviderType";

            /// <summary>
            /// The last name.
            /// </summary>
            public const string LastName = "LN";

            /// <summary>
            /// Token to test regular cookie.
            /// </summary>
            public const string MSAXPersistentCookieCheck = "MSAXPCC";

            /// <summary>
            /// Token to test session cookie.
            /// </summary>
            public const string MSAXSessionCookieCheck = "MSAXSCC";

            /// <summary>
            /// Cookie name for the Retail request digest.
            /// </summary>
            public const string RetailRequestDigestCookieName = "rrd";

            /// <summary>
            /// Cookie name for checkout cart token for the signed-in user.
            /// </summary>
            public const string SignedInCheckoutCartToken = "sicct";

            /// <summary>
            /// Cookie name for shopping cart token for the signed-in user.
            /// </summary>
            public const string SignedInShoppingCartToken = "sisct";
        }
    }
}