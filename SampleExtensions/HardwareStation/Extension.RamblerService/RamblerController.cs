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
    namespace Commerce.HardwareStation.RamblerService
    {
        using System;
        using System.Composition;
        using System.Threading.Tasks;
        using System.Web.Http;
        using System.Web.Http.Controllers;
        using Microsoft.Dynamics.Commerce.HardwareStation;
        using Microsoft.Dynamics.Commerce.HardwareStation.Models;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.Entities;
        using Microsoft.Dynamics.Retail.Diagnostics;
        using RamblerSample;

        /// <summary>
        /// MSR device web API controller class.
        /// </summary>
        [Export("MSR", typeof(IHardwareStationController))]
        [Authorize]
        public class RamblerController : ApiController, IHardwareStationController
        {
            /// <summary>
            /// Locks the specified request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The MSR lock response.</returns>
            /// <exception cref="PeripheralException">A device exception.</exception>
            [HttpPost]
            public MsrLockResponse Lock(MsrLockRequest request)
            {
                ThrowIf.Null(request, "request");

                NetTracer.Information(string.Format("The Msr lock request received: {0}", request.DeviceName));

                IRamblerStripeReader device;
                MsrLockResponse result = new MsrLockResponse();

                result.Token = LockManager.AcquireLock<IRamblerStripeReader>(Microsoft.Dynamics.Commerce.HardwareStation.HostHelpers.HostContext.GetCurrentUser(this.Request), request, out device);

                NetTracer.Information(string.Format("The Msr device is locked: {0}", result.Token));

                return result;
            }

            /// <summary>
            ///  Unlocks the payment terminal for the current client.
            /// </summary>
            /// <param name="request">Request that includes the lock token.</param>
            [HttpPost]
            public void Unlock(LockedSessionRequest request)
            {
                ThrowIf.Null(request, "request");

                LockManager.ReleaseLock<IRamblerStripeReader>(request.Token);

                NetTracer.Information(string.Format("The device is closed"));
                NetTracer.Information(string.Format("The Msr lock is released, token: {0}", request.Token));
            }

            /// <summary>
            /// Gets card swipes or waits for a specified timeout for a swipe to occur.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <exception cref="PeripheralException">Exception thrown.</exception>
            /// <returns>An awaitable task that returns an array of MagneticCardSwipeInfo scanned.</returns>
            [HttpPost]
            public async Task<MagneticCardSwipeInfo> GetMsrSwipeInfo(MsrRequest request)
            {
                var device = LockManager.GetPeripheral<IRamblerStripeReader>(request.Token);

                try
                {
                    ThrowIf.Null(request, "request");

                    device.Open(string.Empty, null);

                    var eventCompletionSource = new TaskCompletionSource<RamblerMagneticStripeCardData>();
                    EventHandler<RamblerMagneticStripeCardData> handler = null;

                    handler = (sender, args) =>
                    {
                        eventCompletionSource.SetResult(args);

                        device.RamblerCardSwipeEvent -= handler;
                    };

                    device.RamblerCardSwipeEvent += handler;

                    MagneticCardSwipeInfo swipeInfo = null;
                    if (Task.WaitAny(eventCompletionSource.Task, Task.Delay(request.TimeoutInSeconds * 1000)) == 0)
                    {
                        swipeInfo = await eventCompletionSource.Task;
                    }

                    return swipeInfo;
                }
                finally
                {
                    device.Close();
                }
            }
        }
    }
}