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
        using System.Web;
        using System.Web.Configuration;
        using Retail.Ecommerce.Sdk.Core;

        /// <summary>
        /// Manages the domain cookies for the storefront site.
        /// </summary>
        public static class CookieManager
        {
            /// <summary>
            /// Gets the specified cookie value from the request.
            /// </summary>
            /// <param name="context">An instance of the HttpContext.</param>
            /// <param name="cookieName">A string containing the name of the cookie.</param>
            /// <returns>String containing the cookie value.</returns>
            public static string GetRequestCookieValue(HttpContext context, string cookieName)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }

                try
                {
                    if (context != null && context.Request != null && !string.IsNullOrWhiteSpace(cookieName))
                    {
                        return GetCookieValue(context.Request.Cookies, cookieName);
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    throw new CookieException(ex.Message, ex);
                }
            }

            /// <summary>
            /// Sets the cookie value with the specified cookie name in the response.
            /// </summary>
            /// <param name="context">An instance of the HttpContext.</param>
            /// <param name="cookieName">A string containing the name of the cookie.</param>
            /// <param name="cookieValue">A string containing the value of the cookie.</param>
            /// <param name="expiry">The date of cookie expiration.</param>
            /// <exception cref="System.ArgumentException">The cookie name cannot be null or contain white space only.</exception>
            /// <exception cref="Microsoft.Dynamics.Retail.SharePoint.Web.Common.CookieException"></exception>
            /// <exception cref="ArgumentException">ArgumentException is thrown when invalid cookie name is specified.</exception>
            public static void SetResponseCookieValue(HttpContext context, string cookieName, string cookieValue, DateTime expiry)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }

                try
                {
                    if (string.IsNullOrWhiteSpace(cookieName))
                    {
                        throw new ArgumentNullException(nameof(cookieName), "The cookie name cannot be null or contain white space only.");
                    }

                    if (context != null && context.Response != null)
                    {
                        SetCookieValue(context.Response.Cookies, cookieName, cookieValue, expiry);
                    }
                }
                catch (Exception ex)
                {
                    throw new CookieException(ex.Message, ex);
                }
            }

            /// <summary>
            /// Expire the cookie with the specified cookie name in the response.
            /// </summary>
            /// <param name="context">An instance of the HttpContext.</param>
            /// <param name="cookieName">A string containing the name of the cookie.</param>
            public static void DeleteCookie(HttpContext context, string cookieName)
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                if (string.IsNullOrWhiteSpace(cookieName))
                {
                    throw new ArgumentNullException(nameof(cookieName), "The cookie name cannot be null or contain white space only.");
                }

                context.Response.Cookies.Set(new HttpCookie(cookieName)
                {
                    Path = "/",
                    Value = string.Empty,
                    HttpOnly = true,
                    Secure = true,
                    Expires = DateTime.Now.AddYears(-1),
                });
            }

            private static bool IsSessionOnlyCookieEnabled(HttpCookieCollection httpCookieCollection)
            {
                bool hasSessionCookie = false;
                bool hasPersistentCookie = false;

                foreach (string key in httpCookieCollection.AllKeys)
                {
                    hasSessionCookie ^= key.Equals(CookieConstants.MSAXSessionCookieCheck, StringComparison.Ordinal);
                    hasPersistentCookie ^= key.Equals(CookieConstants.MSAXPersistentCookieCheck, StringComparison.Ordinal);

                    if (hasSessionCookie && hasPersistentCookie)
                    {
                        break;
                    }
                }

                return hasSessionCookie && !hasPersistentCookie;
            }

            private static string GetCookieValue(HttpCookieCollection collection, string cookieName)
            {
                if (collection != null)
                {
                    HttpCookie cookie = collection[cookieName];
                    if (cookie != null)
                    {
                        return cookie.Value;
                    }
                }

                return null;
            }

            /// <summary>
            /// Sets the cookie value.
            /// </summary>
            /// <param name="collection">The collection.</param>
            /// <param name="cookieName">Name of the cookie.</param>
            /// <param name="cookieValue">The cookie value.</param>
            /// <param name="expiry">The expiry.</param>
            private static void SetCookieValue(HttpCookieCollection collection, string cookieName, string cookieValue, DateTime expiry)
            {
                if (collection != null)
                {
                    HttpCookie cookie = new HttpCookie(cookieName)
                    {
                        Value = cookieValue,
                        Secure = true,
                        HttpOnly = true
                    };

                    // Set date only if persistence is allowed.
                    if (!IsSessionOnlyCookieEnabled(collection))
                    {
                        cookie.Expires = expiry;
                    }

                    // Set will ensure no duplicate keys are persisted.
                    collection.Set(cookie);
                }
            }
        }
    }
}