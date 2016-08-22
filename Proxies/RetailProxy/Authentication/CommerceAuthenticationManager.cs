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
    namespace Commerce.RetailProxy
    {
        using System.Collections.Generic;
        using System.Threading.Tasks;
        using Commerce.RetailProxy.Authentication;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// The commerce authentication manager used to acquire commerce user tokens and manager user credentials.
        /// </summary>
        public class CommerceAuthenticationManager : IAuthenticationManager
        {
            private readonly IContext context;

            /// <summary>
            /// Initializes a new instance of the <see cref="CommerceAuthenticationManager"/> class.
            /// </summary>
            /// <param name="context">The context.</param>
            public CommerceAuthenticationManager(IContext context)
            {
                ThrowIf.Null(context, "context");
    
                this.context = context;
            }

            /// <summary>
            /// Acquires a user token.
            /// </summary>
            /// <param name="userName">Name of the user.</param>
            /// <param name="password">The password of the user.</param>
            /// <param name="commerceAuthenticationParameters">Additional authentication parameters.</param>
            /// <returns>The user token.</returns>
            public async Task<UserToken> AcquireToken(string userName, string password, Dictionary<string, object> commerceAuthenticationParameters)
            {
                ThrowIf.Null(commerceAuthenticationParameters, "commerceAuthenticationParameters");

                OperationParameter[] operationParameters = new OperationParameter[]
                {
                    new OperationParameter() { Name = "userName", Value = userName },
                    new OperationParameter() { Name = "password", Value = password },
                    new OperationParameter() { Name = "commerceAuthenticationParameters", Value = commerceAuthenticationParameters },
                };

                UserToken commerceUserToken = await this.context.ExecuteAuthenticationOperationSingleResultAsync<UserToken>("AcquireToken", operationParameters);
  
                return commerceUserToken;
            }
    
            /// <summary>
            /// Changes the password of the user <param name="userId" />.
            /// </summary>
            /// <param name="userId">The user identifier.</param>
            /// <param name="oldPassword">The current password.</param>
            /// <param name="newPassword">The new password.</param>
            /// <returns>A task.</returns>
            public async Task ChangePassword(string userId, string oldPassword, string newPassword)
            {
                ThrowIf.NullOrWhiteSpace(userId, "userId");
                ThrowIf.NullOrWhiteSpace(oldPassword, "oldPassword");
                ThrowIf.NullOrWhiteSpace(newPassword, "newPassword");
    
                OperationParameter[] operationParameters = new OperationParameter[]
                {
                    new OperationParameter() { Name = "userId", Value = userId },
                    new OperationParameter() { Name = "oldPassword", Value = oldPassword },
                    new OperationParameter() { Name = "newPassword", Value = newPassword },
                };

                await this.context.ExecuteAuthenticationOperationAsync("ChangePassword", operationParameters);
            }
    
            /// <summary>
            /// Resets the password of the user <param name="userId" />.
            /// </summary>
            /// <param name="userId">The id of the user having the password changed.</param>
            /// <param name="newPassword">The newPassword.</param>
            /// <param name="mustChangePasswordAtNextLogOn">Whether the password needs to be changed at the next logon.</param>
            /// <returns>A Task.</returns>
            public async Task ResetPassword(string userId, string newPassword, bool mustChangePasswordAtNextLogOn)
            {
                ThrowIf.NullOrWhiteSpace(userId, "userId");
                ThrowIf.NullOrWhiteSpace(newPassword, "newPassword");

                OperationParameter[] operationParameters = new OperationParameter[]
                {
                    new OperationParameter() { Name = "userId", Value = userId },
                    new OperationParameter() { Name = "newPassword", Value = newPassword },
                    new OperationParameter() { Name = "mustChangePasswordAtNextLogOn", Value = mustChangePasswordAtNextLogOn },
                };

                await this.context.ExecuteAuthenticationOperationAsync("ResetPassword", operationParameters);
            }

            /// <summary>
            /// Enrolls user credentials.
            /// </summary>
            /// <param name="userId">The user identifier.</param>
            /// <param name="grantType">The grant type.</param>
            /// <param name="credential">The user credential.</param>
            /// <param name="extraParameters">The extra parameters.</param>
            /// <returns>A task.</returns>
            public async Task EnrollUserCredentials(string userId, string grantType, string credential, IDictionary<string, object> extraParameters)
            {
                OperationParameter[] operationParameters = new OperationParameter[]
                {
                    new OperationParameter() { Name = "userId", Value = userId },
                    new OperationParameter() { Name = "grantType", Value = grantType },
                    new OperationParameter() { Name = "credential", Value = credential },
                    new OperationParameter() { Name = "extraParameters", Value = extraParameters },
                };

                await this.context.ExecuteAuthenticationOperationAsync("EnrollUserCredentials", operationParameters);
            }

            /// <summary>
            /// Removes user credentials.
            /// </summary>
            /// <param name="userId">The user identifier.</param>
            /// <param name="grantType">The grant type.</param>
            /// <returns>A task.</returns>
            public async Task UnenrollUserCredentials(string userId, string grantType)
            {
                ThrowIf.Null(userId, "userId");
                ThrowIf.Null(grantType, "grantType");

                OperationParameter[] operationParameters = new OperationParameter[]
                {
                    new OperationParameter() { Name = "userId", Value = userId },
                    new OperationParameter() { Name = "grantType", Value = grantType },
                };

                await this.context.ExecuteAuthenticationOperationAsync("UnenrollUserCredentials", operationParameters);
            }
        }
    }
}
