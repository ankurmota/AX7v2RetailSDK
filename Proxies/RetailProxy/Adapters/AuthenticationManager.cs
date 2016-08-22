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
    namespace Commerce.RetailProxy.Adapters
    {
        using System;
        using System.Collections.Generic;
        using System.Threading.Tasks;
        using RetailProxy.Authentication;

        /// <summary>
        /// Class to adapt IPC originated calls into a format that can be consumed by <see cref="CommerceAuthenticationManager"/>.
        /// </summary>
        internal class AuthenticationManager
        {
            private static readonly Lazy<CommerceRuntimeContext> CommerceRuntimeContext = new Lazy<CommerceRuntimeContext>(() =>
            {
                CommerceAuthenticationRuntimeProvider authenticationProvider = new CommerceAuthenticationRuntimeProvider();
                return new CommerceRuntimeContext(AdaptorCaller.GetCrtConfigurationByHostFunc, authenticationProvider);
            });

            private static readonly Lazy<CommerceAuthenticationManager> CommerceAuthenticationManager = new Lazy<CommerceAuthenticationManager>(() => new CommerceAuthenticationManager(CommerceRuntimeContext.Value));

            /// <summary>
            /// Acquires a user token.
            /// </summary>
            /// <param name="grant_type">The grant type.</param>
            /// <param name="client_id">The client identifier.</param>
            /// <param name="username">Name of the user.</param>
            /// <param name="password">The password of the user.</param>
            /// <param name="operation_id">The operation identifier.</param>
            /// <param name="credential">The user's credential.</param>
            /// <returns>The user token.</returns>
            /// <remarks>The parameter names must match the server definition, otherwise <see cref="AdaptorCaller"/> will not bind the method.</remarks>
            public Task<UserToken> Token(string grant_type, string client_id, string username, string password, RetailOperation? operation_id, string credential)
            {
                CommerceAuthenticationParameters parameters = new CommerceAuthenticationParameters(grant_type, client_id, operation_id, credential);

                return CommerceAuthenticationManager.Value.AcquireToken(username, password, parameters);
            }

            /// <summary>
            /// Acquires a user token.
            /// </summary>
            /// <param name="grant_type">The grant type.</param>
            /// <param name="client_id">The client identifier.</param>
            /// <param name="username">Name of the user.</param>
            /// <param name="password">The password of the user.</param>
            /// <param name="credential">The user's credential.</param>
            /// <returns>The user token.</returns>
            /// <remarks>The parameter names must match the server definition, otherwise <see cref="AdaptorCaller"/> will not bind the method.
            /// The overload is necessary because the <see cref="AdaptorCaller"/> binds on parameter number.</remarks>
            public Task<UserToken> Token(string grant_type, string client_id, string username, string password, string credential)
            {
                return this.Token(grant_type, client_id, username, password, operation_id: null, credential: credential);
            }

            /// <summary>
            /// Changes the password.
            /// </summary>
            /// <param name="userId">The user identifier.</param>
            /// <param name="oldPassword">The current password.</param>
            /// <param name="newPassword">The new password.</param>
            /// <returns>A task.</returns>
            public Task ChangePassword(string userId, string oldPassword, string newPassword)
            {
                return CommerceAuthenticationManager.Value.ChangePassword(userId, oldPassword, newPassword);
            }

            /// <summary>
            /// Resets the password of the user <param name="userId"/>.
            /// </summary>
            /// <param name="userId">The id of the user having the password changed.</param>
            /// <param name="newPassword">The newPassword.</param>
            /// <param name="mustChangePasswordAtNextLogOn">Whether the password needs to be changed at the next logon.</param>
            /// <returns>A Task.</returns>
            public Task ResetPassword(string userId, string newPassword, bool mustChangePasswordAtNextLogOn)
            {
                return CommerceAuthenticationManager.Value.ResetPassword(userId, newPassword, mustChangePasswordAtNextLogOn);
            }

            /// <summary>
            /// Sets the authentication tokens.
            /// </summary>
            /// <param name="userToken">The user token.</param>
            /// <param name="deviceToken">The device token.</param>
            /// <returns>A task representing completion.</returns>
            public Task SetAuthenticationTokens(string userToken, string deviceToken)
            {
                CommerceRuntimeUserToken userIdToken = string.IsNullOrWhiteSpace(userToken)
                    ? null
                    : new CommerceRuntimeUserToken(userToken);

                CommerceRuntimeContext.Value.SetUserToken(userIdToken);
                CommerceRuntimeContext.Value.SetDeviceToken(deviceToken);

                return Task.FromResult<object>(null);
            }

            /// <summary>
            /// Enrolls user credentials.
            /// </summary>
            /// <param name="userId">The user identifier.</param>
            /// <param name="grantType">The grant type.</param>
            /// <param name="credential">The user credential.</param>
            /// <param name="extraParameters">The extra parameters.</param>
            /// <returns>A task.</returns>
            public Task EnrollUserCredentials(string userId, string grantType, string credential, IDictionary<string, object> extraParameters)
            {
                return CommerceAuthenticationManager.Value.EnrollUserCredentials(userId, grantType, credential, extraParameters);
            }

            /// <summary>
            /// Enrolls user credentials.
            /// </summary>
            /// <param name="userId">The user identifier.</param>
            /// <param name="grantType">The grant type.</param>
            /// <param name="credential">The user credential.</param>
            /// <returns>A task.</returns>
            public Task EnrollUserCredentials(string userId, string grantType, string credential)
            {
                return this.EnrollUserCredentials(userId, grantType, credential, new Dictionary<string, object>());
            }

            /// <summary>
            /// Removes user credentials.
            /// </summary>
            /// <param name="userId">The user identifier.</param>
            /// <param name="grantType">The grant type.</param>
            /// <returns>A task.</returns>
            public Task UnenrollUserCredentials(string userId, string grantType)
            {
                return CommerceAuthenticationManager.Value.UnenrollUserCredentials(userId, grantType);
            }
        }
    }
}
