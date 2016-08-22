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
    namespace Commerce.HardwareStation.WebHost
    {
        using System.Web;
    
        /// <summary>
        /// Cross-origin resource sharing support class.
        /// </summary>
        internal static class CorsSupport
        {
            /// <summary>
            /// Handles a CORS OPTIONS request.
            /// </summary>
            public static void HandlePreflightRequest()
            {
                HttpRequest request = HttpContext.Current.Request;
                HttpResponse response = HttpContext.Current.Response;
    
                string origin = request.Headers["Origin"];
    
                if (!string.IsNullOrWhiteSpace(origin))
                {
                    // must have allow origin with specific origin value in every response if allow-credentials is true
                    response.Headers["Access-Control-Allow-Origin"] = origin;
                }
    
                // implementing simple preflight handling - using web.config for access control headers
                if (request.HttpMethod == "OPTIONS")
                {
                    // HTTP 200 OK
                    // Response.End() should not be used because it will throw a thread abort exception.
                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                }
            }
        }
    }
}
