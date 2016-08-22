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
        using System;
        using System.Collections.Generic;
        using System.IO;
        using System.Linq;
        using System.Net;
        using System.Net.Http;
        using System.Text;
        using System.Threading.Tasks;
        using Commerce.RetailProxy.Adapters;
        using Microsoft.Dynamics.Commerce.Runtime;

        /// <summary>
        /// The authentication context used to retrieve user tokens from the Commerce Authentication service.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Dynamics.Retail.StyleCop.Rules.FileNameAnalyzer", "SR1704:FileNameDoesNotMatchElementInside", Justification = "Will be removed once file is renamed.")]
        public class CommerceAuthenticationRetailServerProvider : CommerceAuthenticationProvider
        {
            private const string DeviceTokenHeaderName = "DeviceToken";
            private const string JsonContentType = "application/json";
            private const string FormUrlEncondedContentType = "application/x-www-form-urlencoded";
            
            private Uri serviceRoot;

            /// <summary>
            /// Initializes a new instance of the <see cref="CommerceAuthenticationRetailServerProvider"/> class.
            /// </summary>
            /// <param name="serviceRoot">The service root.</param>
            public CommerceAuthenticationRetailServerProvider(Uri serviceRoot)
            {
                this.serviceRoot = serviceRoot;
            }
            
            /// <summary>
            /// The event that is triggered when a request is issued by this provider.
            /// </summary>
            public event EventHandler<ComerceAuthenticationSendingRequestEventArgs> SendingRequest;

            /// <summary>
            /// Acquires the token.
            /// </summary>
            /// <param name="userName">Name of the user.</param>
            /// <param name="password">The password of the user.</param>
            /// <param name="commerceAuthenticationParameters">The additional commerce authentication parameters.</param>
            /// <returns>The user token.</returns>
            internal override async Task<UserToken> AcquireToken(string userName, string password, CommerceAuthenticationParameters commerceAuthenticationParameters)
            {
                ThrowIf.Null(commerceAuthenticationParameters, "commerceAuthenticationParameters");

                StringBuilder data = this.CreateAcquireTokenRequest(userName, password, commerceAuthenticationParameters);
                const string TokenEntityName = "token";

                using (var response = await this.SendServerRequest(data, HttpMethod.Post, TokenEntityName, FormUrlEncondedContentType, commerceAuthenticationParameters.RetailOperation.HasValue))
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var result = Newtonsoft.Json.JsonConvert.DeserializeObject<CommerceAuthenticationResult>(reader.ReadToEnd());

                    return new UserIdToken(result.IdToken);
                }
            }

            /// <summary>
            /// Changes the password.
            /// </summary>
            /// <param name="userId">The user identifier.</param>
            /// <param name="oldPassword">The current password.</param>
            /// <param name="newPassword">The new password.</param>
            /// <returns>A task.</returns>
            internal override async Task ChangePassword(string userId, string oldPassword, string newPassword)
            {
                ThrowIf.NullOrWhiteSpace(userId, "userId");
                ThrowIf.NullOrWhiteSpace(oldPassword, "oldPassword");
                ThrowIf.NullOrWhiteSpace(newPassword, "newPassword");

                StringBuilder data = this.CreateChangePasswordRequest(userId, oldPassword, newPassword);
                const string ChangePassworOperation = "ChangePassword";

                using (var response = await this.SendServerRequest(data, HttpMethod.Post, ChangePassworOperation, JsonContentType, true))
                {
                }
            }

            /// <summary>
            /// Resets the password of the user <param name="userId" />.
            /// </summary>
            /// <param name="userId">The id of the user having the password changed.</param>
            /// <param name="newPassword">The newPassword.</param>
            /// <param name="mustChangePasswordAtNextLogOn">Whether the password needs to be changed at the next logon.</param>
            /// <returns>A Task.</returns>
            internal override async Task ResetPassword(string userId, string newPassword, bool mustChangePasswordAtNextLogOn)
            {
                ThrowIf.NullOrWhiteSpace(userId, "userId");
                ThrowIf.NullOrWhiteSpace(newPassword, "newPassword");

                StringBuilder data = this.CreateResetPasswordRequest(userId, newPassword, mustChangePasswordAtNextLogOn);
                const string ResetPasswordOperation = "ResetPassword";

                using (var response = await this.SendServerRequest(data, HttpMethod.Post, ResetPasswordOperation, JsonContentType, true))
                {
                }
            }

            /// <summary>
            /// Enrolls user credentials.
            /// </summary>
            /// <param name="userId">The user identifier.</param>
            /// <param name="grantType">The grant type.</param>
            /// <param name="credential">The user credential.</param>
            /// <param name="extraParameters">The extra parameters.</param>
            /// <returns>A task.</returns>
            internal override async Task EnrollUserCredentials(string userId, string grantType, string credential, IDictionary<string, object> extraParameters)
            {
                const string UserIdParameterName = "userId";
                const string GrantTypeParameterName = "grantType";
                const string CredentialParameterName = "credential";
                const string EnrollUserCredentialsOperationName = "EnrollUserCredentials";

                Dictionary<string, object> parameters;
                
                if (extraParameters == null)
                {
                    parameters = new Dictionary<string, object>();
                }
                else
                {
                    parameters = new Dictionary<string, object>(extraParameters);
                }
                
                parameters[UserIdParameterName] = userId;
                parameters[GrantTypeParameterName] = grantType;
                parameters[CredentialParameterName] = credential;
                
                string payload = Newtonsoft.Json.JsonConvert.SerializeObject(parameters);
                StringBuilder data = new StringBuilder(payload);

                using (var response = await this.SendServerRequest(data, HttpMethod.Post, EnrollUserCredentialsOperationName, JsonContentType, true))
                {
                }
            }

            /// <summary>
            /// Removes user credentials.
            /// </summary>
            /// <param name="userId">The user identifier.</param>
            /// <param name="grantType">The grant type.</param>
            /// <returns>A task.</returns>
            internal override async Task UnenrollUserCredentials(string userId, string grantType)
            {
                const string UserIdParameterName = "userId";
                const string GrantTypeParameterName = "grantType";
                const string EnrollUserCredentialsOperationName = "UnenrollUserCredentials";

                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters[UserIdParameterName] = userId;
                parameters[GrantTypeParameterName] = grantType;

                string payload = Newtonsoft.Json.JsonConvert.SerializeObject(parameters);
                StringBuilder data = new StringBuilder(payload);

                using (var response = await this.SendServerRequest(data, HttpMethod.Post, EnrollUserCredentialsOperationName, JsonContentType, true))
                {
                }
            }

            /// <summary>
            /// Executes the <param name="method"></param> with exception handling.
            /// </summary>
            /// <typeparam name="TResult">The type of the result.</typeparam>
            /// <param name="method">The method.</param>
            /// <returns>The object of type TResult.</returns>
            private static async Task<TResult> ExecuteWithExceptionHandlingAsync<TResult>(Func<Task<TResult>> method)
            {
                try
                {
                    return await method();
                }
                catch (WebException ex)
                {
                    using (var streamReader = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        var response = streamReader.ReadToEnd();

                        // Deserializes to exception
                        RetailProxyException crtException = null;
                        if (DefaultExceptionHandlingBehavior.TryDeserializeFromJsonString(response, out crtException) && crtException != null)
                        {
                            throw crtException;
                        }
                        else
                        {
                            // If exception is not mapped to a commerce runtime exception, throws the local exception based on HTTP status code.
                            CommunicationExceptionHelper.ThrowAsRetailProxyExceptionOnHttpStatuCode(ex, (int)((HttpWebResponse)ex.Response).StatusCode);
                            throw ex;
                        }
                    }
                }
            }

            /// <summary>
            /// Creates the acquire token request.
            /// </summary>
            /// <param name="userName">Name of the user.</param>
            /// <param name="password">The password.</param>
            /// <param name="authenticationParameters">The authentication parameters.</param>
            /// <returns>The request body.</returns>
            private StringBuilder CreateAcquireTokenRequest(string userName, string password, CommerceAuthenticationParameters authenticationParameters)
            {
                // Create the data to send to token endpoint
                StringBuilder data = new StringBuilder();
                data.Append("username=" + Uri.EscapeDataString(userName));
                data.Append("&password=" + Uri.EscapeDataString(password));
                foreach (var authenticationParameter in authenticationParameters)
                {
                    if (authenticationParameter.Value != null)
                    {
                        data.Append(string.Format("&{0}={1}", authenticationParameter.Key, Uri.EscapeDataString(authenticationParameter.Value.ToString())));
                    }
                }

                return data;
            }

            /// <summary>
            /// Creates the change password request.
            /// </summary>
            /// <param name="userId">The user identifier.</param>
            /// <param name="oldPassword">The current password.</param>
            /// <param name="newPassword">The new password.</param>
            /// <returns>The payload.</returns>
            private StringBuilder CreateChangePasswordRequest(string userId, string oldPassword, string newPassword)
            {
                ChangePasswordRequest changePasswordRequest = new ChangePasswordRequest();
                changePasswordRequest.OldPassword = oldPassword;
                changePasswordRequest.NewPassword = newPassword;
                changePasswordRequest.UserId = userId;
                string payload = Newtonsoft.Json.JsonConvert.SerializeObject(changePasswordRequest);
                return new StringBuilder(payload);
            }

            /// <summary>
            /// Creates the reset password request.
            /// </summary>
            /// <param name="userId">The id of the user having the password changed.</param>
            /// <param name="newPassword">The newPassword.</param>
            /// <param name="changePasswordOnLogOn">Whether the password needs to be changed on the next logon.</param>
            /// <returns>The payload.</returns>
            private StringBuilder CreateResetPasswordRequest(string userId, string newPassword, bool changePasswordOnLogOn)
            {
                ResetPasswordRequest resetPasswordRequest = new ResetPasswordRequest();
                resetPasswordRequest.UserId = userId;
                resetPasswordRequest.NewPassword = newPassword;
                resetPasswordRequest.MustChangePasswordAtNextLogOn = changePasswordOnLogOn;
                string payload = Newtonsoft.Json.JsonConvert.SerializeObject(resetPasswordRequest);
                return new StringBuilder(payload);
            }

            /// <summary>
            /// Sends the request to the authentication service.
            /// </summary>
            /// <param name="data">The data.</param>
            /// <param name="method">The method.</param>
            /// <param name="entityName">The name of the entity targeted by the request.</param>
            /// <param name="contentType">The content type of the payload.</param>
            /// <param name="includeUserToken">Whether the user token should not or not be included in the request.</param>
            /// <returns>The web response.</returns>
            private async Task<HttpWebResponse> SendServerRequest(StringBuilder data, HttpMethod method, string entityName, string contentType, bool includeUserToken)
            {
                // Create a byte array of the data to be sent
                byte[] byteArray = Encoding.UTF8.GetBytes(data.ToString());

                // Setup the Request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(Path.Combine(this.serviceRoot.ToString(), entityName)));
                request.Method = method.Method;
                request.Accept = "application/json";
                request.ContentType = contentType;

                if (!string.IsNullOrWhiteSpace(this.Locale))
                {
                    request.Headers[HttpRequestHeader.AcceptLanguage] = this.Locale;
                }

                if (!string.IsNullOrWhiteSpace(this.DeviceToken))
                {
                    request.Headers[DeviceTokenHeaderName] = this.DeviceToken;
                }

                if (includeUserToken && this.UserToken != null)
                {
                    request.Headers[HttpRequestHeader.Authorization] = string.Format("{0} {1}", this.UserToken.SchemeName, this.UserToken.Token);
                }

                if (this.SendingRequest != null)
                {
                    ComerceAuthenticationSendingRequestEventArgs eventArguments = new ComerceAuthenticationSendingRequestEventArgs();
                    eventArguments.Headers.AddRange(request.Headers.AllKeys.Select(key => new KeyValuePair<string, string>(key, request.Headers[key])));
                    this.SendingRequest(this, eventArguments);

                    // update headers
                    foreach (KeyValuePair<string, string> header in eventArguments.Headers)
                    {
                        if (!string.Equals(header.Value, request.Headers[header.Key], StringComparison.Ordinal))
                        {
                            request.Headers[header.Key] = header.Value;
                        }
                    }
                }

                // Write data
                using (Stream postStream = await request.GetRequestStreamAsync())
                {
                    postStream.Write(byteArray, 0, byteArray.Length);
                }

                // Send Request & Get Response
                WebResponse webResponse = await ExecuteWithExceptionHandlingAsync(request.GetResponseAsync);
                return webResponse as HttpWebResponse;
            }
        }
    }
}
