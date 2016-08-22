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
    namespace Commerce.HardwareStation.Peripherals
    {
        using System.Collections.Generic;
        using System.Composition;
        using System.Diagnostics.CodeAnalysis;
        using Interop.OposCashDrawer;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;

        /// <summary>
        /// Class implements OPOS based cash drawer driver for hardware station.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "By design")]
        [Export(PeripheralType.Opos, typeof(ICashDrawer))]
        public class OposCashDrawer : ICashDrawer
        {
            private IOPOSCashDrawer oposCashDrawer;

            /// <summary>
            /// Gets a value indicating whether the cash drawer is open or not.
            /// </summary>
            public bool IsOpen
            {
                get
                {
                    if (this.oposCashDrawer != null && this.oposCashDrawer.DeviceEnabled)
                    {
                        return this.oposCashDrawer.DrawerOpened;
                    }

                    return false;
                }
            }

            /// <summary>
            /// Establishes a connection to the specified cash drawer.
            /// </summary>
            /// <param name="peripheralName">Name of cash drawer device to open.</param>
            /// <param name="peripheralConfig">Configuration parameters of the peripheral.</param>
            public void Open(string peripheralName, PeripheralConfiguration peripheralConfig)
            {
                this.oposCashDrawer = new OPOSCashDrawer();

                // Open
                this.oposCashDrawer.Open(peripheralName);
                OposHelper.CheckResultCode(this, this.oposCashDrawer.ResultCode);

                // Claim
                this.oposCashDrawer.ClaimDevice(OposHelper.ClaimTimeOut);
                OposHelper.CheckResultCode(this, this.oposCashDrawer.ResultCode);

                // Enable
                this.oposCashDrawer.DeviceEnabled = true;
                OposHelper.CheckResultCode(this, this.oposCashDrawer.ResultCode);
            }

            /// <summary>
            /// Terminates a connection to the cash drawer.
            /// </summary>
            public void Close()
            {
                if (this.oposCashDrawer != null)
                {
                    this.oposCashDrawer.ReleaseDevice();
                    this.oposCashDrawer.Close();
                    this.oposCashDrawer = null;
                }
            }

            /// <summary>
            /// Causes the cash drawer to be physically opened.
            /// </summary>
            public void OpenDrawer()
            {
                if (this.oposCashDrawer != null && this.oposCashDrawer.DeviceEnabled)
                {
                    this.oposCashDrawer.OpenDrawer();
                }
            }
        }
    }
}
