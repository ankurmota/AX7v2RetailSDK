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
        using System;
        using System.Net;
        using Commerce.RetailProxy.Adapters;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Helper class for <see cref="CommunicationException"/> class.
        /// </summary>
        public static class CommunicationExceptionHelper
        {
            /// <summary>
            /// The redirect location header name in HTTP response header.
            /// </summary>
            private const string RedirectLocationHeaderName = "Location";
            
            /// <summary>
            /// Traverses and throws the commerce exception from the input exception hierarchy.
            /// </summary>
            /// <param name="exception">The exception to traverse.</param>
            public static void ThrowAsCommerceException(Exception exception)
            {
                Exception tempException = exception == null ? null : exception.InnerException;
    
                while (tempException != null && !string.IsNullOrWhiteSpace(tempException.Message))
                {
                    // Deserializes to exception
                    RetailProxyException crtException = null;
                    if (DefaultExceptionHandlingBehavior.TryDeserializeFromJsonString(tempException.Message, out crtException)
                        && crtException != null)
                    {
                        throw crtException;
                    }
    
                    tempException = tempException.InnerException;
                }
            }
    
            /// <summary>
            /// Throws as retail proxy exception based on HTTP status code.
            /// </summary>
            /// <param name="exception">The exception from OData client.</param>
            /// <param name="httpStatusCode">The HTTP status code.</param>
            public static void ThrowAsRetailProxyExceptionOnHttpStatuCode(Exception exception, int httpStatusCode)
            {
                if (exception == null)
                {
                    throw new ArgumentNullException("exception", "The exception object should not be null.");
                }
    
                string message = exception.Message;
                switch (httpStatusCode)
                {
                    case (int)HttpStatusCode.PreconditionFailed:
                        throw new AuthenticationException(
                            AuthenticationErrors.DeviceAuthenticationFailure,
                            message,
                            exception);
    
                    case (int)HttpStatusCode.Unauthorized:
                        throw new AuthenticationException(
                            AuthenticationErrors.UserAuthenticationFailure,
                            message,
                            exception);
    
                    case (int)HttpStatusCode.Forbidden:
                        throw new CommunicationException(
                            ClientCommunicationErrors.UserAuthorizationError,
                            message,
                            exception);
    
                    case (int)HttpStatusCode.InternalServerError:
                        throw new CommunicationException(
                            ClientCommunicationErrors.ServerInternalError,
                            message,
                            exception);
    
                    case (int)HttpStatusCode.NotFound:
                    case (int)HttpStatusCode.NotImplemented:
                    case (int)HttpStatusCode.BadGateway:
                    case (int)HttpStatusCode.ServiceUnavailable:
                        throw new CommunicationException(
                            ClientCommunicationErrors.NotFound,
                            message,
                            exception);
    
                    case (int)HttpStatusCode.RequestTimeout:
                    case (int)HttpStatusCode.GatewayTimeout:
                        throw new CommunicationException(
                            ClientCommunicationErrors.RequestTimeout,
                            message,
                            exception);
    
                    case (int)HttpStatusCode.BadRequest:
                        throw new CommunicationException(
                            ClientCommunicationErrors.BadRequest,
                            message,
                            exception);
    
                    case (int)HttpStatusCode.Ambiguous:
                        throw new CommunicationException(
                            ClientCommunicationErrors.Ambiguous,
                            message,
                            exception);
    
                    case (int)HttpStatusCode.Unused:
                        // Only unhandled retail server wrong endpoint exception with 306 response header will be thrown as CommunicationException.
                        throw new CommunicationException(
                            ClientCommunicationErrors.Redirection,
                            message,
                            exception);
    
                    default:
                        // Do nothing
                        break;
                }
            }
        }
    }
}
