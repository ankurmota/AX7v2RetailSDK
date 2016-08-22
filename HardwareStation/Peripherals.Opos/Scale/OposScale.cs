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
        using Interop.OposConstants;
        using Interop.OposScale;
        using Microsoft.Dynamics.Commerce.HardwareStation;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;

        /// <summary>
        /// Class implements OPOS based scale driver for hardware station.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "By design")]
        [Export(PeripheralType.Opos, typeof(IScale))]
        public sealed class OposScale : IScale
        {
            private IOPOSScale oposScale;

            /// <summary>
            /// Establishes a connection to the specified scale.
            /// </summary>
            /// <param name="peripheralName">Name of scale device to open.</param>
            /// <param name="peripheralConfig">Configuration parameters of the peripheral.</param>
            public void Open(string peripheralName, PeripheralConfiguration peripheralConfig)
            {
                this.oposScale = new OPOSScale();

                // Open
                this.oposScale.Open(peripheralName);
                OposHelper.CheckResultCode(this, this.oposScale.ResultCode);

                // Claim
                this.oposScale.ClaimDevice(OposHelper.ClaimTimeOut);
                OposHelper.CheckResultCode(this, this.oposScale.ResultCode);

                // Enable/Configure
                this.oposScale.DeviceEnabled = true;
                this.oposScale.PowerNotify = (int)OPOS_Constants.OPOS_PN_ENABLED;
            }

            /// <summary>
            /// Terminates a connection to the scale.
            /// </summary>
            public void Close()
            {
                if (this.oposScale != null)
                {
                    this.oposScale.ReleaseDevice();
                    this.oposScale.Close();
                    this.oposScale = null;
                }
            }

            /// <summary>
            /// Reads the weight from scale.
            /// </summary>
            /// <param name="timeout">The timeout.</param>
            /// <returns>
            /// The weight value.
            /// </returns>
            /// <exception cref="Microsoft.Dynamics.Commerce.HardwareStation.PeripheralException">Exception thrown when the call fails.</exception>
            public decimal Read(int timeout)
            {
                int weight = 0;

                if (this.oposScale != null && this.oposScale.DeviceEnabled)
                {
                    int result = this.oposScale.ReadWeight(out weight, timeout);

                    if (result != (int)OPOS_Constants.OPOS_SUCCESS)
                    {
                        throw new PeripheralException(PeripheralException.ScaleError);
                    }
                }

                return weight;
            }
        }
    }
}
