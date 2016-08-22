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
    namespace Commerce.HardwareStation.CashDispenserSample
    {
        using System.Composition;
        using System.Web.Http;
        using Microsoft.Dynamics.Commerce.HardwareStation;

        /// <summary>
        /// The new ping controller is the new controller class to receive ping request.
        /// </summary>
        [Export("CUSTOMPING", typeof(IHardwareStationController))]
        public class CustomPingController : ApiController, IHardwareStationController
        {
            /// <summary>
            /// The test method that returns the successful ping.
            /// </summary>
            /// <param name="pingRequest">The ping request.</param>
            /// <returns>Returns the successful ping message.</returns>
            [HttpPost]
            public string CustomPing(PingRequest pingRequest)
            {
                ThrowIf.Null(pingRequest, "pingRequest");

                return string.Format("Your message is successfully received: {0}", pingRequest.Message);
            }
        }
    }
}