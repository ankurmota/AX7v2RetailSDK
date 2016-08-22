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
    namespace Retail.Ecommerce.Web.Storefront
    {
        using System;
        using System.Collections.Generic;
        using System.Configuration;
        using System.Linq;
        using System.Security.Claims;
        using System.Text;
        using System.Web;
        using System.Web.Security;
        using Commerce.RetailProxy;
        using Microsoft.Dynamics.Retail.Diagnostics;
        using Microsoft.Owin;
        using Retail.Ecommerce.Sdk.Core;

        /// <summary>
        /// ServiceUtilities class.
        /// </summary>
        public static class ServiceUtilities
        {
            private static readonly double SessionTokenCookieMinutesUntilExpiration = TimeSpan.FromDays(30).TotalMinutes;

            /// <summary>
            /// Gets the cart identifier from request cookie.
            /// </summary>
            /// <param name="httpContextBase">The HTTP context base.</param>
            /// <param name="sessionType">Type of the session.</param>
            /// <returns>The cart identifier.</returns>
            public static string GetCartIdFromRequestCookie(HttpContextBase httpContextBase, SessionType sessionType)
            {
                if (httpContextBase == null)
                {
                    throw new ArgumentNullException(nameof(httpContextBase));
                }

                string cookieName = GetCartTokenCookieName(sessionType);
                string cartId = CookieManager.GetRequestCookieValue(httpContextBase.ApplicationInstance.Context, cookieName);
                return cartId;
            }

            /// <summary>
            /// Sets the cart identifier on response cookie.
            /// </summary>
            /// <param name="httpContextBase">The HTTP context base.</param>
            /// <param name="sessionType">Type of the session.</param>
            /// <param name="cartId">The cart identifier.</param>
            public static void SetCartIdInResponseCookie(HttpContextBase httpContextBase, SessionType sessionType, string cartId)
            {
                if (httpContextBase == null)
                {
                    throw new ArgumentNullException(nameof(httpContextBase));
                }

                string cookieName = GetCartTokenCookieName(sessionType);
                CookieManager.SetResponseCookieValue(httpContextBase.ApplicationInstance.Context, cookieName, cartId, DateTime.Now.AddMinutes(SessionTokenCookieMinutesUntilExpiration));
            }

            /// <summary>
            /// Gets the type of the session.
            /// </summary>
            /// <param name="httpContext">The HTTP context.</param>
            /// <param name="isCheckoutSession">If set to <c>true</c> [is checkout session].</param>
            /// <returns>The session type.</returns>
            public static SessionType GetSessionType(HttpContextBase httpContext, bool isCheckoutSession)
            {
                if (httpContext == null)
                {
                    throw new ArgumentNullException(nameof(httpContext));
                }

                SessionType sessionType = SessionType.None;

                if (httpContext.Request.IsAuthenticated)
                {
                    if (isCheckoutSession)
                    {
                        sessionType = SessionType.SignedInCheckout;
                    }
                    else
                    {
                        sessionType = SessionType.SignedInShopping;
                    }
                }
                else
                {
                    if (isCheckoutSession)
                    {
                        sessionType = SessionType.AnonymousCheckout;
                    }
                    else
                    {
                        sessionType = SessionType.AnonymousShopping;
                    }
                }

                return sessionType;
            }

            /// <summary>
            /// Gets the name of the cart token cookie.
            /// </summary>
            /// <param name="sessionType">Type of the session.</param>
            /// <returns>The cart token cookie name.</returns>
            public static string GetCartTokenCookieName(SessionType sessionType)
            {
                string cookieName = string.Empty;
                string message = string.Empty;

                switch (sessionType)
                {
                    case SessionType.AnonymousShopping:
                        cookieName = CookieConstants.AnonymousShoppingCartToken;
                        break;
                    case SessionType.AnonymousCheckout:
                        cookieName = CookieConstants.AnonymousCheckoutCartToken;
                        break;
                    case SessionType.SignedInShopping:
                        cookieName = CookieConstants.SignedInShoppingCartToken;
                        break;
                    case SessionType.SignedInCheckout:
                        cookieName = CookieConstants.SignedInCheckoutCartToken;
                        break;
                    default:

                        message = string.Format("SessionType '{0}' not recognized.", sessionType);
                        throw new NotSupportedException(message);
                }

                return cookieName;
            }

            /// <summary>
            /// Performs clean up operations after any authentication/authorization failure.
            /// </summary>
            /// <param name="httpContextBase">The HTTP context.</param>
            public static void CleanUpOnSignOutOrAuthFailure(HttpContextBase httpContextBase)
            {
                if (httpContextBase == null)
                {
                    throw new ArgumentNullException(nameof(httpContextBase));
                }

                var ctx = httpContextBase.Request.GetOwinContext();
                ctx.Authentication.SignOut(CookieConstants.ApplicationCookieAuthenticationType);

                OpenIdConnectUtilities.RemoveCookie(CookieConstants.ExternalTokenPart2);

                string cookieName = GetCartTokenCookieName(SessionType.SignedInShopping);
                CookieManager.DeleteCookie(httpContextBase.ApplicationInstance.Context, cookieName);

                cookieName = GetCartTokenCookieName(SessionType.SignedInCheckout);
                CookieManager.DeleteCookie(httpContextBase.ApplicationInstance.Context, cookieName);
            }

            /// <summary>
            /// Clears the cart cookies.
            /// </summary>
            /// <param name="httpContextBase">The HTTP context base.</param>
            public static void ClearCartCookies(HttpContextBase httpContextBase)
            {
                if (httpContextBase == null)
                {
                    throw new ArgumentNullException(nameof(httpContextBase));
                }

                string cookieName = GetCartTokenCookieName(SessionType.SignedInShopping);
                CookieManager.DeleteCookie(httpContextBase.ApplicationInstance.Context, cookieName);

                cookieName = GetCartTokenCookieName(SessionType.SignedInCheckout);
                CookieManager.DeleteCookie(httpContextBase.ApplicationInstance.Context, cookieName);

                cookieName = GetCartTokenCookieName(SessionType.AnonymousShopping);
                CookieManager.DeleteCookie(httpContextBase.ApplicationInstance.Context, cookieName);

                cookieName = GetCartTokenCookieName(SessionType.AnonymousCheckout);
                CookieManager.DeleteCookie(httpContextBase.ApplicationInstance.Context, cookieName);
            }

            /// <summary>
            /// Gets the eCommerce context.
            /// </summary>
            /// <param name="httpContextBase">The HTTP context.</param>
            /// <param name="authToken">The authentication token.</param>
            /// <param name="identityProviderType">The type of identity provider associated with the authentication token.</param>
            /// <returns>The eCommerce context.</returns>
            public static EcommerceContext GetEcommerceContext(HttpContextBase httpContextBase, string authToken, IdentityProviderType identityProviderType)
            {
                if (httpContextBase == null)
                {
                    throw new ArgumentNullException(nameof(httpContextBase));
                }

                string locale = null;
                if (httpContextBase.Request.UserLanguages != null)
                {
                    locale = httpContextBase.Request.UserLanguages.FirstOrDefault();
                }

                EcommerceContext ecommerceContext = new EcommerceContext()
                {
                    AuthenticationToken = authToken,
                    IdentityProviderType = identityProviderType,
                    Locale = locale,
                    OperatingUnitId = ConfigurationManager.AppSettings["OperatingUnitNumber"]
                };

                return ecommerceContext;
            }

            /// <summary>
            /// Gets the eCommerce context.
            /// </summary>
            /// <param name="httpContextBase">The HTTP context.</param>
            /// <returns>The eCommerce context.</returns>
            public static EcommerceContext GetEcommerceContext(HttpContextBase httpContextBase)
            {
                if (httpContextBase == null)
                {
                    throw new ArgumentNullException(nameof(httpContextBase));
                }

                IOwinContext ctx = httpContextBase.GetOwinContext();
                ClaimsPrincipal user = ctx.Authentication.User;
                IEnumerable<Claim> claims = user.Claims;

                string token = ServiceUtilities.GetExternalAuthToken(httpContextBase, claims);

                Claim identityProviderTypeClaim = claims.Where(cl => string.Equals(cl.Type, CookieConstants.IdentityProviderType, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
                IdentityProviderType identityProviderType = (identityProviderTypeClaim != null) ? (IdentityProviderType)Enum.Parse(typeof(IdentityProviderType), identityProviderTypeClaim.Value, true) : IdentityProviderType.None;

                string locale = null;

                // UserLanguages are not necessarily sent so need to be ready to process null property.
                if (httpContextBase.Request.UserLanguages != null)
                {
                    locale = httpContextBase.Request.UserLanguages.FirstOrDefault();
                }

                EcommerceContext ecommerceContext = new EcommerceContext()
                {
                    AuthenticationToken = token,
                    IdentityProviderType = identityProviderType,
                    Locale = locale,
                    OperatingUnitId = ConfigurationManager.AppSettings["OperatingUnitNumber"]
            };

                return ecommerceContext;
            }

            /// <summary>
            /// Gets the external authentication token.
            /// </summary>
            /// <param name="httpContextBase">The HTTP context base.</param>
            /// <param name="claims">The claims.</param>
            /// <returns>The external authentication token.</returns>
            private static string GetExternalAuthToken(HttpContextBase httpContextBase, IEnumerable<Claim> claims)
            {
                string externalAuthToken = string.Empty;

                Claim authTokenClaim = claims.Where(cl => string.Equals(cl.Type, CookieConstants.ExternalTokenPart1, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
                string externalAuthTokenPart1 = (authTokenClaim != null) ? authTokenClaim.Value : null;
                if (externalAuthTokenPart1 != null)
                {
                    string externalAuthTokenPart2 = GetAuthTokenSecondHalfFromCookie(httpContextBase);
                    externalAuthToken = externalAuthTokenPart1 + externalAuthTokenPart2;
                }

                return externalAuthToken;
            }

            /// <summary>
            /// Gets the authentication token second half from cookie.
            /// </summary>
            /// <param name="httpContextBase">The HTTP context base.</param>
            /// <returns>The second half of the authentication token which is stored in a cookie.</returns>
            /// <exception cref="SecurityException">The second half of the external authentication token could not be found.</exception>
            private static string GetAuthTokenSecondHalfFromCookie(HttpContextBase httpContextBase)
            {
                HttpCookie idTokenPart2Cookie = httpContextBase.Request.Cookies.Get(CookieConstants.ExternalTokenPart2);
                if (idTokenPart2Cookie == null || idTokenPart2Cookie.Value == null)
                {
                    RetailLogger.Log.OnlineStoreSecondHalfOfAuthenticationTokenNotFoundInCookie(CookieConstants.ExternalTokenPart2, (idTokenPart2Cookie != null) ? idTokenPart2Cookie.Value : string.Empty);
                    throw new SecurityException("The second half of the external authentication token could not be found.");
                }

                byte[] idTokenPart2Encrypted = Convert.FromBase64String(idTokenPart2Cookie.Value);
                byte[] idTokenPart2Decrypted = MachineKey.Unprotect(idTokenPart2Encrypted);
                string idTokenPart2 = Encoding.ASCII.GetString(idTokenPart2Decrypted);

                return idTokenPart2;
            }
        }
    }
}