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
        using System.Threading.Tasks;
        using Commerce.RetailProxy.Adapters;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Client;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Entities;

        /// <summary>
        /// The authentication provider used to retrieve user tokens from the Commerce Runtime.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Dynamics.Retail.StyleCop.Rules.FileNameAnalyzer", "SR1704:FileNameDoesNotMatchElementInside", Justification = "Will be removed once file is renamed.")]
        public class CommerceAuthenticationRuntimeProvider : CommerceAuthenticationProvider
        {
            /// <summary>
            /// Acquires the user token.
            /// </summary>
            /// <param name="userName">Name of the user.</param>
            /// <param name="password">The password of the user.</param>
            /// <param name="commerceAuthenticationParameters">The commerce authentication parameters.</param>
            /// <returns>The user commerce runtime token.</returns>
            internal override Task<UserToken> AcquireToken(string userName, string password, CommerceAuthenticationParameters commerceAuthenticationParameters)
            {
                ThrowIf.Null(commerceAuthenticationParameters, "commerceAuthenticationParameters");

                return Execute<UserToken>(() =>
                {
                    CommerceRuntimeUserToken commerceUserToken;
                    CommerceIdentity originalIdentity = CommerceRuntimeManager.Identity;
                    ConnectionRequest connectionRequest = this.CreateAcquireTokenRequest(userName, password);
                    connectionRequest.Credential = commerceAuthenticationParameters.Credential;
                    connectionRequest.GrantType = commerceAuthenticationParameters.GrantType;
                    connectionRequest.AdditionalAuthenticationData = commerceAuthenticationParameters;

                    LogonCredentials credentials = null;

                    if (!commerceAuthenticationParameters.RetailOperation.HasValue)
                    {
                        try
                        {
                            CommerceIdentity commerceIdentity = new CommerceIdentity(string.Empty, 0, 0, new string[] { });
                            commerceIdentity.Roles.Add(CommerceRoles.Anonymous);

                            // Set anonymous identity from request.
                            CommerceRuntimeManager.Identity = commerceIdentity;

                            credentials = SecurityManager.Create(CommerceRuntimeManager.Runtime).LogOn(connectionRequest);

                            // Clear the commerce identity.
                            CommerceRuntimeManager.Identity = null;
                        }
                        catch (Exception)
                        {
                            CommerceRuntimeManager.Identity = originalIdentity;
                            throw;
                        }

                        commerceUserToken = new CommerceRuntimeUserToken(credentials.Identity);
                    }
                    else
                    {
                        credentials = SecurityManager.Create(CommerceRuntimeManager.Runtime).ElevateUser(connectionRequest, (RetailOperation)commerceAuthenticationParameters.RetailOperation);
                        commerceUserToken = new CommerceRuntimeUserToken(originalIdentity, credentials.Identity);
                    }

                    return commerceUserToken;
                });                
            }
    
            /// <summary>
            /// Changes the password.
            /// </summary>
            /// <param name="userId">The user identifier.</param>
            /// <param name="oldPassword">The current password.</param>
            /// <param name="newPassword">The new password.</param>
            /// <returns>A task.</returns>
            internal override Task ChangePassword(string userId, string oldPassword, string newPassword)
            {
                ThrowIf.NullOrWhiteSpace(userId, "userId");
                ThrowIf.NullOrWhiteSpace(oldPassword, "oldPassword");
                ThrowIf.NullOrWhiteSpace(newPassword, "newPassword");

                return Execute(() => SecurityManager.Create(CommerceRuntimeManager.Runtime).ChangePassword(userId, oldPassword, newPassword));
            }
    
            /// <summary>
            /// Resets the password of the user <param name="userId" />.
            /// </summary>
            /// <param name="userId">The id of the user having the password changed.</param>
            /// <param name="newPassword">The newPassword.</param>
            /// <param name="mustChangePasswordAtNextLogOn">Whether the password needs to be changed at the next logon.</param>
            /// <returns>A Task.</returns>
            internal override Task ResetPassword(string userId, string newPassword, bool mustChangePasswordAtNextLogOn)
            {
                ThrowIf.NullOrWhiteSpace(userId, "userId");
                ThrowIf.NullOrWhiteSpace(newPassword, "newPassword");
    
                return Execute(() => SecurityManager.Create(CommerceRuntimeManager.Runtime).ResetPassword(userId, newPassword, mustChangePasswordAtNextLogOn));
            }

            /// <summary>
            /// Enrolls user credentials.
            /// </summary>
            /// <param name="userId">The user identifier.</param>
            /// <param name="grantType">The grant type.</param>
            /// <param name="credential">The user credential.</param>
            /// <param name="extraParameters">The extra parameters.</param>
            /// <returns>A task.</returns>
            internal override Task EnrollUserCredentials(string userId, string grantType, string credential, IDictionary<string, object> extraParameters)
            {
                return Execute(() => SecurityManager.Create(CommerceRuntimeManager.Runtime).EnrollUserCredentials(userId, grantType, credential, extraParameters));
            }

            /// <summary>
            /// Removes user credentials.
            /// </summary>
            /// <param name="userId">The user identifier.</param>
            /// <param name="grantType">The grant type.</param>
            /// <returns>A task.</returns>
            internal override Task UnenrollUserCredentials(string userId, string grantType)
            {
                return Execute(() => SecurityManager.Create(CommerceRuntimeManager.Runtime).UnenrollUserCredentials(userId, grantType));
            }

            /// <summary>
            /// Executes an action with the proper error handling logic.
            /// </summary>
            /// <param name="action">The action to be performed.</param>
            /// <returns>A task.</returns>
            private static Task Execute(Action action)
            {
                return Execute<object>(() =>
                {
                    action();
                    return null;
                });
            }

            /// <summary>
            /// Executes an action with the proper error handling logic.
            /// </summary>
            /// <typeparam name="T">The return type.</typeparam>
            /// <param name="action">The action to be performed.</param>
            /// <returns>The returned value from the action.</returns>
            private static Task<T> Execute<T>(Func<T> action)
            {
                return Task.Run(() =>
                {
                    try
                    {
                        return action();
                    }
                    catch (Exception ex)
                    {
                        string exceptionResponse = ex.SerializeToCommerceException();

                        RetailProxyException crtException = null;
                        if (DefaultExceptionHandlingBehavior.TryDeserializeFromJsonString(exceptionResponse, out crtException) && crtException != null)
                        {
                            throw crtException;
                        }

                        throw new RetailProxyException(string.Format("Unexpected exception payload {0}.", exceptionResponse));
                    }
                });
            }

            /// <summary>
            /// Creates the acquire token request.
            /// </summary>
            /// <param name="userName">Name of the user.</param>
            /// <param name="password">The password.</param>
            /// <returns>The connection request.</returns>
            private ConnectionRequest CreateAcquireTokenRequest(string userName, string password)
            {
                // Create the data to send to token endpoint
                ConnectionRequest connectionRequest = new ConnectionRequest()
                {
                    UserId = userName,
                    Password = password
                };
    
                if (!string.IsNullOrWhiteSpace(this.DeviceToken))
                {
                    connectionRequest.DeviceToken = this.DeviceToken;
                }
    
                return connectionRequest;
            }
        }
    }
}
