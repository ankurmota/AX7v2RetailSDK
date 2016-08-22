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
    namespace RetailServer.TestClient
    {
        using System;
        using System.Configuration;
        using System.Diagnostics;
        using Microsoft.IdentityModel.Clients.ActiveDirectory;

        /// <summary>
        /// Helper class for Azure Active Directory.
        /// </summary>
        public static class AzureActiveDirectoryHelper
        {
            private const string AuthorizationHeaderName = "Authorization";
            private const string RetailServerResourceId = @"https://commerce.dynamics.com";
            private static string clientId = ConfigurationManager.AppSettings["ModernPosAzureClientId"];
            private static string commonAuthority = ConfigurationManager.AppSettings["AADLoginUrl"];

            /// <summary>
            /// Gets the Azure Active Directory token given the user credentials.
            /// </summary>
            /// <param name="userName">The AAD user name.</param>
            /// <param name="password">The AAD user password.</param>
            /// <returns>The AAD token.</returns>
            /// <remarks>This function is meant for test only. Do not store any username and passwords, instead use GetAADHeaderWithPrompt().</remarks>
            public static string GetAADToken(string userName, string password)
            {
                return GetAADToken(userName, password, RetailServerResourceId).Result.AccessToken;
            }

            /// <summary>
            /// Gets the Azure Active Directory token given the user credentials and the targeted audience/resource id.
            /// </summary>
            /// <param name="userName">The AAD user name.</param>
            /// <param name="password">The AAD user password.</param>
            /// <param name="resourceId">The resource identifier.</param>
            /// <returns>The AAD token.</returns>
            /// <remarks>This function is meant for test only. Do not store any username and passwords, instead use GetAADHeaderWithPrompt().</remarks>
            public static async System.Threading.Tasks.Task<AuthenticationResult> GetAADToken(string userName, string password, string resourceId)
            {
                AuthenticationContext context = new AuthenticationContext(AzureActiveDirectoryHelper.commonAuthority, false);
                AuthenticationResult authResult = null;
                try
                {
                    authResult = await context.AcquireTokenAsync(resourceId, AzureActiveDirectoryHelper.clientId, new UserCredential(userName, password));
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("Failure retrieving the AAD token from {0} with the paramaters: username {1}, clientId {2} and resource {3}. Exception: {4}", AzureActiveDirectoryHelper.commonAuthority, userName, AzureActiveDirectoryHelper.clientId, resourceId, exception);
                    throw;
                }

                return authResult;
            }

            /// <summary>
            /// Gets the AAD AuthenticationResult from user dialog.
            /// </summary>
            /// <returns>The AAD AuthenticationResult.</returns>
            public static AuthenticationResult GetAADAuthenticationResultWithPrompt()
            {
                AuthenticationResult result = null;

                AuthenticationContext context = new AuthenticationContext(AzureActiveDirectoryHelper.commonAuthority);

                result = context.AcquireToken(
                    clientId: AzureActiveDirectoryHelper.clientId,
                    redirectUri: new Uri("urn:ietf:wg:-Oauth:2.0:-Oob"),
                    resource: RetailServerResourceId,
                    promptBehavior: PromptBehavior.Auto,
                    userId: UserIdentifier.AnyUser,
                    extraQueryParameters: "nux=1");

                return result;
            }

            /// <summary>
            /// Gets the AAD token from user dialog.
            /// </summary>
            /// <returns>The AAD token.</returns>
            public static string GetAADHeaderWithPrompt()
            {
                return GetAADAuthenticationResultWithPrompt().CreateAuthorizationHeader().Substring("Bearer ".Length);
            }
        }
    }
}