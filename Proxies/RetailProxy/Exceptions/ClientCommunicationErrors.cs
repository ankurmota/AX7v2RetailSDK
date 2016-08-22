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
        /// Class container for all errors resulting from communications in the client application.
        /// </summary>
        /// <remarks>
        /// When adding a new constant, modifying the name of a constant, or deleting a constant, a corresponding change needs to
        /// be made in <c>Channels\Apps\WindowsPhone\C1\Pos\Application\Resources\AppResources.resx</c> so a proper default UI message is displayed.
        /// </remarks>
        public static class ClientCommunicationErrors
        {
            /// <summary>
            /// Indicates that there was an error in establishing communication with the remote server (HTTP 502).
            /// </summary>
            public const string NotFound = "Microsoft_Dynamics_Commerce_Client_Communication_NotFound";
    
            /// <summary>
            /// Indicates that there was an error in communicating with the remote server due to network timeout (HTTP 408).
            /// </summary>
            public const string RequestTimeout = "Microsoft_Dynamics_Commerce_Client_Communication_RequestTimeOut";
    
            /// <summary>
            /// Indicates that there was an error in communicating with the remote server due to server internal error (HTTP 500).
            /// </summary>
            public const string ServerInternalError = "Microsoft_Dynamics_Internal_Server_Error";
    
            /// <summary>
            /// Indicates that there was an error in the request (HTTP 400).
            /// </summary>
            public const string BadRequest = "Microsoft_Dynamics_Commerce_Client_Communication_BadRequest";
    
            /// <summary>
            /// Indicates that there was an error in deserializations.
            /// </summary>
            public const string DeserializationError = "Microsoft_Dynamics_Commerce_Client_Communication_DeserializationError";
    
            /// <summary>
            /// Indicates that there was an error in communication due to ambiguous requests (HTTP 300).
            /// </summary>
            public const string Ambiguous = "Microsoft_Dynamics_Commerce_Client_Communication_Ambiguous";
    
            /// <summary>
            /// Indicates that there was an error in communication due to redirection (HTTP 306).
            /// </summary>
            public const string Redirection = "Microsoft_Dynamics_Commerce_Client_Communication_Redirection";
    
            /// <summary>
            /// Indicates that there was an error in the user authorization request (HTTP 403).
            /// </summary>
            public const string UserAuthorizationError = "Microsoft_Dynamics_Commerce_Client_Communication_UserAuthorizationError";
    
            /// <summary>
            /// Indicates that there was an error in the user authentication request (HTTP 401).
            /// </summary>
            public const string UserAuthenticationFailure = "Microsoft_Dynamics_Commerce_Client_Communication_UserAuthenticationFailure";
    
            /// <summary>
            /// Indicates that there was an error in establishing communication with the remote server (catch all).
            /// </summary>
            public const string OtherErrors = "Microsoft_Dynamics_Commerce_Client_Communication_OtherErrors";
                    
            /// <summary>
            /// Indicates that one or more required reason codes are missing.
            /// </summary>
            public static readonly string RequiredReasonCodesMissing = "Microsoft_Dynamics_Commerce_Runtime_RequiredReasonCodesMissing";
        }
    }
}
