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
        using Microsoft.Dynamics.Commerce.Runtime;

        /// <summary>
        /// Class container for all errors resulting from authentications in the client application.
        /// </summary>
        /// <remarks>
        /// When adding a new constant, modifying the name of a constant, or deleting a constant, a corresponding change needs to
        /// be made in <c>Channels\Apps\WindowsPhone\C1\Pos\Application\Resources\AppResources.resx</c> so a proper default UI message is displayed.
        /// </remarks>
        public static class AuthenticationErrors
        {
            /// <summary>
            /// Indicates that there was a generic error in authenticating the user identity (HTTP 401).
            /// </summary>
            public const string UserAuthenticationFailure = "Microsoft_Dynamics_Retail_Pos_Authentication_UserAuthenticationFailure";
    
            /// <summary>
            /// Indicates that there was an error in authenticating the device identity (HTTP 412).
            /// </summary>
            public const string DeviceAuthenticationFailure = "Microsoft_Dynamics_Retail_Pos_Authentication_DeviceAuthenticationFailure";
    
            /// <summary>
            /// Indicates that that a commerce identity could not be found.
            /// </summary>
            public const string CommerceIdentityNotFound = "Microsoft_Dynamics_Commerce_Runtime_CommerceIdentityNotFound";

            /// <summary>
            /// Indicates that that a user is awaiting activation.
            /// </summary>
            public const string UserNotActivated = "Microsoft_Dynamics_Commerce_Runtime_UserNotActivated";
        }
    }
}
