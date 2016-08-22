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
        using System;
        using System.Composition;
        using System.Web.Http;
        using Microsoft.Dynamics.Commerce.HardwareStation;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Cash dispenser web API controller class.
        /// </summary>
        [Export("CASHDISPENSER", typeof(IHardwareStationController))]
        public class CashDispenserController : ApiController, IHardwareStationController
        {
            private const string CashDispenserTestName = "CashDispenserTest";

            /// <summary>
            /// Collect the change in the cash dispenser.
            /// </summary>
            /// <param name="request">The cash dispenser request value.</param>
            /// <returns>Returns success if the change is collected.</returns>
            public bool CollectChange(CashDispenserRequest request)
            {
                ThrowIf.Null(request, "request");

                ICashDispenser cashDispenser = CompositionManager.Instance.GetComponent<ICashDispenser>("WINDOWS");
                string deviceName = request.DeviceName;

                if (string.IsNullOrWhiteSpace(deviceName))
                {
                    deviceName = CashDispenserController.CashDispenserTestName;
                }

                if (cashDispenser != null)
                {
                    try
                    {
                        cashDispenser.Open(deviceName, null);
                        cashDispenser.CollectChange(request.Change, request.Currency);

                        return true;
                    }
                    catch (Exception ex)
                    {
                        RetailLogger.Log.HardwareStationActionFailure("Hardware station an exception occurred when operating on cash dispenser.", ex);
                        throw new PeripheralException("Microsoft_Dynamics_Commerce_HardwareStation_CashDispenser_Error", ex.Message, ex);
                    }
                    finally
                    {
                        cashDispenser.Close();
                    }
                }
                else
                {
                    throw new PeripheralException("Microsoft_Dynamics_Commerce_HardwareStation_CashDispenser_Error");
                }
            }
        }
    }
}