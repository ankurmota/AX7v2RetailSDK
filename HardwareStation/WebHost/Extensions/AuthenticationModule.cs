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
    namespace Commerce.HardwareStation.WebHost.Extensions
    {
        using System;
        using System.Net;
        using System.Net.Http.Headers;
        using System.Security.Principal;
        using System.Threading;
        using System.Web;
        using Microsoft.Dynamics.Commerce.HardwareStation;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Authentication module that verifies the request's <see cref="T:System.Security.Principal.IPrincipal" />.
        /// </summary>
        /// <remarks>
        /// Thread.CurrentPrincipal has been assigned when this filter gets called. Based on that, a principal will be assigned to Thread.CurrentPrincipal.
        /// </remarks>
        public class AuthenticationModule : IHttpModule
        {
            private const string AuthorizationScheme = "MessageCredential";
            private const string AuthorizationHeader = "Authorization";
            private const string AuthenticateHeader = "WWW-Authenticate";
            private const char AuthorizationParametersDelimeter = ' ';
            private const int AuthorizationParametersParts = 2;

            /// <summary>
            /// Initializes a module and prepares it to handle requests.
            /// </summary>
            /// <param name="context">An <see cref="T:System.Web.HttpApplication" /> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application.</param>
            public void Init(HttpApplication context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }

                context.AuthenticateRequest += this.OnApplicationAuthenticateRequest;
                context.EndRequest += this.OnApplicationEndRequest;
            }

            /// <summary>
            /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule" />.
            /// </summary>
            public void Dispose()
            {
            }

            /// <summary>
            /// Sets the principal.
            /// </summary>
            /// <param name="principal">The principal.</param>
            private static void SetPrincipal(IPrincipal principal)
            {
                Thread.CurrentPrincipal = principal;
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.User = principal;
                }
            }

            /// <summary>
            /// Called on authentication request.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
            private void OnApplicationAuthenticateRequest(object sender, EventArgs e)
            {
                HttpRequest request = HttpContext.Current.Request;
                var authorizationHeader = request.Headers[AuthorizationHeader];

                if (!string.IsNullOrWhiteSpace(authorizationHeader))
                {
                    var authorizationRequest = AuthenticationHeaderValue.Parse(authorizationHeader);

                    if (authorizationRequest.Scheme.Equals(AuthorizationScheme, StringComparison.OrdinalIgnoreCase)
                        && authorizationRequest.Parameter != null)
                    {
                        string[] parameters = authorizationRequest.Parameter.Split(AuthorizationParametersDelimeter);
                        string pairingKeyAsString = null;

                        LocalStorage localStorage = LocalStorage.GetInstance(LocalStorageContentType.PairingKey);

                        if (parameters != null
                            && parameters.Length == AuthorizationParametersParts
                            && (pairingKeyAsString = localStorage.SecureStorage.GetValue(parameters[0])) != null)
                        {
                            PairingKeyValidationResult validationResult = PairingKeyValidationResult.Invalid;
                            PairingKey pairingKey = PairingKey.FromString(pairingKeyAsString);

                            if (pairingKey != null)
                            {
                                validationResult = pairingKey.Validate(parameters[1]);
                            }

                            switch (validationResult)
                            {
                                case PairingKeyValidationResult.Valid:
                                    SetPrincipal(new GenericPrincipal(new GenericIdentity(parameters[0]), null));
                                    break;

                                case PairingKeyValidationResult.Expired:
                                    NetTracer.Information("Paring key received from host '{0}' for device '{1}' has expired.", HttpContext.Current.Request.UserHostAddress, parameters[0]);
                                    break;

                                case PairingKeyValidationResult.Invalid:
                                default:
                                    NetTracer.Information("Invalid pairing key received from host '{0}' for device '{1}'.", HttpContext.Current.Request.UserHostAddress, parameters[0]);
                                    break;
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Called when [application end request].
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
            private void OnApplicationEndRequest(object sender, EventArgs e)
            {
                var response = HttpContext.Current.Response;

                if (response.StatusCode == (int)HttpStatusCode.Unauthorized)
                {
                    NetTracer.Warning("Authentication failed for request '{0}' from the host '{1}'", HttpContext.Current.Request.Url, HttpContext.Current.Request.UserHostAddress);
                    response.Headers.Add(AuthenticateHeader, AuthorizationScheme);
                }
            }
        }
    }
}
