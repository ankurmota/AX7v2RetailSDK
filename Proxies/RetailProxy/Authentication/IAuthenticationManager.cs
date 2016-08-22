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

        /// <summary>
        /// Defines the contract of authentication managers that can be registered with the Retail Proxy.
        /// </summary>
        public interface IAuthenticationManager : IEntityManager
        {
            /// <summary>
            /// Acquires a user token.
            /// </summary>
            /// <param name="userName">Name of the user.</param>
            /// <param name="password">The password of the user.</param>
            /// <param name="commerceAuthenticationParameters">Additional authentication parameters.</param>
            /// <returns>The user token.</returns>
            Task<UserToken> AcquireToken(string userName, string password, Dictionary<string, object> commerceAuthenticationParameters);
    
            /// <summary>
            /// Changes the password.
            /// </summary>
            /// <param name="userId">The user identifier.</param>
            /// <param name="oldPassword">The current password.</param>
            /// <param name="newPassword">The new password.</param>
            /// <returns>A task.</returns>
            Task ChangePassword(string userId, string oldPassword, string newPassword);
    
            /// <summary>
            /// Resets the password of the user <param name="userId"/>.
            /// </summary>
            /// <param name="userId">The id of the user having the password changed.</param>
            /// <param name="newPassword">The newPassword.</param>
            /// <param name="mustChangePasswordAtNextLogOn">Whether the password needs to be changed at the next logon.</param>
            /// <returns>A Task.</returns>
            Task ResetPassword(string userId, string newPassword, bool mustChangePasswordAtNextLogOn);

            /// <summary>
            /// Enrolls user credentials.
            /// </summary>
            /// <param name="userId">The user identifier.</param>
            /// <param name="grantType">The grant type.</param>
            /// <param name="credential">The user credential.</param>
            /// <param name="extraParameters">The extra parameters.</param>
            /// <returns>A task.</returns>
            Task EnrollUserCredentials(string userId, string grantType, string credential, IDictionary<string, object> extraParameters);

            /// <summary>
            /// Removes user credentials.
            /// </summary>
            /// <param name="userId">The user identifier.</param>
            /// <param name="grantType">The grant type.</param>
            /// <returns>A task.</returns>
            Task UnenrollUserCredentials(string userId, string grantType);
        }
    }
}
