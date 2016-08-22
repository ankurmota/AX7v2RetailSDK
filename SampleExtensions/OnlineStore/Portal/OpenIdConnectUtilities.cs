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
        using System.IdentityModel.Tokens;
        using System.IO;
        using System.Linq;
        using System.Net;
        using System.Runtime.Serialization.Json;
        using System.Security;
        using System.Security.Claims;
        using System.Text;
        using System.Web;
        using Microsoft.Dynamics.Retail.Diagnostics;
        using Microsoft.IdentityModel.Protocols;
        using Retail.Ecommerce.Sdk.Core;

        /// <summary>
        /// Utilities specific to OpenIdConnect.
        /// </summary>
        public static class OpenIdConnectUtilities
        {
            /// <summary>
            /// Cookie name used to identify an identity provider which is being used for login purposes.
            /// </summary>
            public const string CookieCurrentProvider = "oidcp";

            /// <summary>
            /// Cookie name used to identify a state to validate Authorization Code response.
            /// </summary>
            public const string CookieState = "reqs";

            /// <summary>
            /// Cookie name used to identify a nonce to validate id_token.
            /// </summary>
            public const string CookieNonce = "reqn";

            /// <summary>
            /// Configuration section name which contains Retail Parameters.
            /// </summary>
            public const string ConfigurationSectionName = "retailConfiguration";

            /// <summary>
            /// The lifespan, in minutes, of the cookie responsible for the logged in session. 
            /// </summary>
            /// Expiration is set to 60 + 1 minute, where 60 is the expiration for id_tokens issues by at least Azure AD and Google
            /// So, setting forms auth ticket's (whos UserData is a container for id_token) expiration to be bigger than id_token 
            /// to avoid FormsAuthTicket expire earlier than id_token.
            public const int LoggedOnCookieDurationInMinutes = 61;

            /// <summary>
            /// Standard OpenID Connect Suffix which is added to an Authority(issuer) to retrieve Identity Provider Discovery Document.
            /// </summary>
            private const string DiscoveryDocumentSuffix = ".well-known/openid-configuration";

            private const string OpenIdConfigurationPathSuffix = @"/.well-known/openid-configuration";
            private const string GenericSecurityError = "The request is invalid.";

            /// <summary>
            /// Executes a request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            public static string ExecuteRequest(WebRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                try
                {
                    WebResponse response1 = request.GetResponse();
                    using (StreamReader streamReader = new StreamReader(response1.GetResponseStream()))
                    {
                        string content = streamReader.ReadToEnd();
                        if (content != null && content.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            RetailLogger.Log.OnlineStoreErrorInWebResponse(request.RequestUri.AbsoluteUri, content, "This is required for OpenIdConnect authentication");
                            throw new SecurityException(content);
                        }

                        return content;
                    }
                }
                catch (WebException ex)
                {
                    string error = HandleWebException(ex);
                    RetailLogger.Log.OnlineStoreErrorInWebResponse(request.RequestUri.AbsoluteUri, error, "This is required for OpenIdConnect authentication");
                    throw new SecurityException(error, ex);
                }
            }

            /// <summary>
            /// Sends an HttpGet request.
            /// </summary>
            /// <param name="url">URL of the request.</param>
            /// <returns>The response.</returns>
            public static string HttpGet(Uri url)
            {
                ServicePointManager.CheckCertificateRevocationList = true;
                HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;
                return ExecuteRequest(webRequest);
            }

            /// <summary>
            /// Sends a HTTP POST request and returns its response.
            /// </summary>
            /// <param name="url">URL of the request.</param>
            /// <param name="bodyParameters">Body of the request.</param>
            /// <returns>The response.</returns>
            public static string HttpPost(Uri url, string bodyParameters)
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.Method = "POST";
                Stream stream = req.GetRequestStream();
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] bytes = encoding.GetBytes(bodyParameters);
                stream.Write(bytes, 0, bytes.Length);
                req.ContentType = "application/x-www-form-urlencoded";

                string responseBody = ExecuteRequest(req);
                return responseBody;
            }

            /// <summary>
            /// Deserializes JSON.
            /// </summary>
            /// <typeparam name="T">The type to instantiate.</typeparam>
            /// <param name="json">JSON string.</param>
            /// <returns>Strongly typed version of the deserialized JSON.</returns>
            public static T DeserilizeJson<T>(string json)
            {
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(T));
                using (MemoryStream stream = new MemoryStream(ASCIIEncoding.ASCII.GetBytes(json)))
                {
                    return (T)jsonSerializer.ReadObject(stream);
                }
            }

            /// <summary>
            /// Sets a cookie used during authentication to store such parameters like current provider, state, nonce.
            /// </summary>
            /// <param name="cookieKey">The cookie's key.</param>
            /// <param name="cookieValue">The cookie's value.</param>
            /// <param name="shortLiving">True if the cookie should have short life time, False otherwise.</param>
            /// <remarks>The cookie is Http one and has an expiration 5 minutes.</remarks>
            public static void SetCustomCookie(string cookieKey, string cookieValue, bool shortLiving = true)
            {
                const int OneWeekMinutes = 10080;
                HttpCookieCollection cookies = HttpContext.Current.Response.Cookies;
                HttpCookie newCookie = new HttpCookie(cookieKey, cookieValue) { HttpOnly = true, Secure = true, Expires = DateTime.UtcNow.AddMinutes(shortLiving ? 5 : OneWeekMinutes) };

                bool providerTypeCookieSet = false;
                foreach (string cookieName in cookies.AllKeys)
                {
                    if (cookieName == cookieKey)
                    {
                        cookies.Set(newCookie);
                        providerTypeCookieSet = true;
                        break;
                    }
                }

                if (!providerTypeCookieSet)
                {
                    cookies.Add(newCookie);
                }
            }

            /// <summary>
            /// Gets Identity Provider used by a user to initiate Login process.
            /// </summary>
            /// <param name="throwExceptionIfAbsent">Throw exception if current provider settings are missing.</param>
            /// <returns>The Identity Provider.</returns>
            public static IdentityProviderClientConfigurationElement GetCurrentProviderSettings(bool throwExceptionIfAbsent = true)
            {
                HttpCookie currentProviderCookie = HttpContext.Current.Request.Cookies.Get(OpenIdConnectUtilities.CookieCurrentProvider);
                if (currentProviderCookie == null)
                {
                    if (throwExceptionIfAbsent)
                    {
                        RetailLogger.Log.OnlineStoreCookieNotFound(OpenIdConnectUtilities.CookieCurrentProvider, "Required for OpenIdConnect signIn");
                        throw new SecurityException(string.Format("{0} cookie not found.", OpenIdConnectUtilities.CookieCurrentProvider));
                    }
                    else
                    {
                        return null;
                    }
                }

                IdentityProviderClientConfigurationElement provider = Utilities.GetIdentityProviderFromConfiguration(currentProviderCookie.Value);

                if (provider == null)
                {
                    RetailLogger.Log.OnlineStoreIdentityProviderSpecifiedInRequestCookieNotSupported(currentProviderCookie.Name, currentProviderCookie.Value);
                    throw new SecurityException("Identity Provider specified on request could not be found in the app config. This could happen due to a timeout; login again.");
                }

                return provider;
            }

            /// <summary>
            /// Validates incoming request and extracts an Authorization Code.
            /// </summary>
            /// <param name="httpContextBase">The HTTP context base.</param>
            /// <returns>
            /// The Authorization Code.
            /// </returns>
            public static string ValidateRequestAndGetAuthorizationCode(HttpContextBase httpContextBase)
            {
                if (httpContextBase == null)
                {
                    throw new ArgumentNullException(nameof(httpContextBase));
                }

                string code = httpContextBase.Request.Params["code"];

                if (string.IsNullOrWhiteSpace(code))
                {
                    RetailLogger.Log.OnlineStoreRequiredParameterMissingFromRequest("code", code, "Required for OpenIdConnect authentication.");
                    throw new SecurityException("Code parameter not found in request.");
                }

                HttpCookie stateCookie = HttpContext.Current.Request.Cookies.Get(OpenIdConnectUtilities.CookieState);
                if (stateCookie == null)
                {
                    RetailLogger.Log.OnlineStoreCookieNotFound(OpenIdConnectUtilities.CookieState, "Required for OpenIdConnect sign-in");
                    throw new SecurityException(string.Format("{0} cookie not found in request.", OpenIdConnectUtilities.CookieState));
                }

                string receivedState = HttpContext.Current.Request.Params["state"];
                string stateCookieValue = HttpUtility.UrlDecode(stateCookie.Value);
                if (receivedState != stateCookieValue)
                {
                    RetailLogger.Log.OnlineStoreMismatchBetweenCookieValueAndParameterValue(OpenIdConnectUtilities.CookieState, stateCookieValue, "state", receivedState, "Required for OpenIdConnect authentication");
                    throw new SecurityException("State pararamer not found in request.");
                }

                return code;
            }

            /// <summary>
            /// Loads Discovery Document buy using well known provider's URL.
            /// </summary>
            /// <param name="issuer">Uri issuer.</param>        
            /// <returns>The Discovery Document.</returns>
            public static OpenIdConnectConfiguration GetDiscoveryDocument(Uri issuer)
            {
                if (issuer == null)
                {
                    throw new ArgumentNullException("issuer");
                }

                string openIdConfigurationBasePath = issuer.ToString().TrimEnd('/');

                string openIdConfigurationFullPath = openIdConfigurationBasePath + OpenIdConfigurationPathSuffix;
                Uri openIdConfigurationUri = new Uri(openIdConfigurationFullPath, UriKind.Absolute);

                string discoveryResponse = HttpGet(openIdConfigurationUri);

                OpenIdConnectConfiguration result = new OpenIdConnectConfiguration(discoveryResponse);
                return result;
            }

            /// <summary>
            /// Generates random ID.
            /// </summary>
            /// <returns>The generated ID.</returns>
            public static string GenerateRandomId()
            {
                return Guid.NewGuid().ToString();
            }

            /// <summary>
            /// Validates raw id_token and returns in strongly typed version.
            /// </summary>
            /// <param name="idTokenRaw">Raw id_token.</param>
            /// <returns>The validated token.</returns>
            public static JwtSecurityToken GetIdToken(string idTokenRaw)
            {
                JwtSecurityToken token = new JwtSecurityToken(idTokenRaw);

                HttpCookie nonceCookie = HttpContext.Current.Request.Cookies.Get(OpenIdConnectUtilities.CookieNonce);
                if (nonceCookie == null)
                {
                    RetailLogger.Log.OnlineStoreCookieNotFound(OpenIdConnectUtilities.CookieNonce, "Required to get OpenIdConnect token");
                    throw new SecurityException("Nonce cookie not found in request.");
                }

                Claim nonceClaim = token.Claims.SingleOrDefault(c => c.Type == "nonce");
                if (nonceClaim == null)
                {
                    RetailLogger.Log.OnlineStoreCookieNotFound("nonce", "Required to get OpenIdConnect token");
                    throw new SecurityException("Nonce claim not found in request.");
                }

                if (nonceClaim.Value != nonceCookie.Value)
                {
                    RetailLogger.Log.OnlineStoreNonceValueMisMatch(nonceCookie.Value, nonceClaim.Value);
                    throw new SecurityException("Nonce cookie and nonce claim values do not match.");
                }

                return token;
            }

            /// <summary>
            /// Removes the cookie that has the provided name.
            /// </summary>
            /// <param name="cookieName">Name of cookie to be removed.</param>
            public static void RemoveCookie(string cookieName)
            {
                HttpCookie expiredCookie = new HttpCookie(cookieName)
                {
                    HttpOnly = true,
                    Secure = true,
                    Expires = DateTime.UtcNow.AddDays(-1)
                };

                HttpContext.Current.Response.SetCookie(expiredCookie);
            }

            /// <summary>
            /// Handles an exception which could take place while sending a request.
            /// </summary>
            /// <param name="ex">The exception.</param>
            /// <returns>The error extracted from the response.</returns>
            private static string HandleWebException(WebException ex)
            {
                using (WebResponse response = ex.Response)
                {
                    using (Stream data = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(data);
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }
}