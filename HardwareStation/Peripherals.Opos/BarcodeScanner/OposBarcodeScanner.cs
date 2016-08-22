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
    namespace Commerce.HardwareStation.Peripherals.BarcodeScanner
    {
        using System;
        using System.Collections;
        using System.Collections.Generic;
        using System.Composition;
        using System.Linq;
        using System.Threading.Tasks;
        using System.Threading.Tasks.Dataflow;
        using Interop.OposConstants;
        using Interop.OposScanner;
        using Microsoft.Dynamics.Commerce.HardwareStation;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Class implements OPOS based barcode scanner for hardware station.
        /// </summary>
        [Export(PeripheralType.Opos, typeof(IBarcodeScanner))]
        public sealed class OposBarcodeScanner : IBarcodeScanner
        {
            private OPOSScanner oposBarcodeScanner;
            private BufferBlock<string> scannedBarcodesBufferBlock = new BufferBlock<string>();

            /// <summary>
            /// Opens a peripheral.
            /// </summary>
            /// <param name="peripheralName">Name of the peripheral.</param>
            /// <param name="peripheralConfig">Configuration parameters of the peripheral.</param>
            public void Open(string peripheralName, PeripheralConfiguration peripheralConfig)
            {
                this.oposBarcodeScanner = new OPOSScanner();

                // Open
                this.oposBarcodeScanner.Open(peripheralName);
                OposHelper.CheckResultCode(this, this.oposBarcodeScanner.ResultCode);

                // Claim
                this.oposBarcodeScanner.ClaimDevice(OposHelper.ClaimTimeOut);
                OposHelper.CheckResultCode(this, this.oposBarcodeScanner.ResultCode);

                // Enable/Configure
                this.oposBarcodeScanner.DeviceEnabled = true;

                // Plug in handlers for data eevents
                this.oposBarcodeScanner.DataEvent += this.OnBarcodeScannerDataEvent;
                this.oposBarcodeScanner.ErrorEvent += this.OnBarcodeScannerErrorEvent;

                // Set autodisable to false
                this.oposBarcodeScanner.AutoDisable = false;

                // Enable data events
                this.oposBarcodeScanner.DataEventEnabled = true;
            }

            /// <summary>
            /// Closes a connection with Barcode Scanner.
            /// </summary>
            public void Close()
            {
                if (this.oposBarcodeScanner != null)
                {
                    this.oposBarcodeScanner.DataEvent -= this.OnBarcodeScannerDataEvent;
                    this.oposBarcodeScanner.ErrorEvent -= this.OnBarcodeScannerErrorEvent;
                    this.oposBarcodeScanner.ReleaseDevice();
                    this.oposBarcodeScanner.Close();
                    this.oposBarcodeScanner = null;
                }

                this.scannedBarcodesBufferBlock = null;
            }

            /// <summary>
            /// Returns any scanned barcodes.
            /// </summary>
            /// <param name="timeoutInSeconds">Duration of time in seconds to wait.</param>
            /// <returns>An awaitable result of scanned barcodes.</returns>
            public async Task<string[]> GetBarcodesAsync(int timeoutInSeconds)
            {
                IList<string> scannedBarcodesInBuffer = null;
                if (this.scannedBarcodesBufferBlock.TryReceiveAll(out scannedBarcodesInBuffer))
                {
                    return scannedBarcodesInBuffer.ToArray();
                }
                else
                {
                    string barcodeData = null;
                    try
                    {
                        barcodeData = await this.scannedBarcodesBufferBlock.ReceiveAsync(TimeSpan.FromSeconds(timeoutInSeconds));
                    }
                    catch (TimeoutException)
                    {
                        // Do nothing here,  we did not receive any barcode in the time allocated
                    }

                    if (barcodeData != null)
                    {
                        return new string[] { barcodeData };
                    }

                    return null;
                }
            }

            private void OnBarcodeScannerErrorEvent(int resultCode, int resultCodeExtended, int errorLocus, ref int errorResponse)
            {
                throw new PeripheralException(PeripheralException.PeripheralEventError, "Error Event from Peripheral with error code {0}", resultCode);
            }

            private void OnBarcodeScannerDataEvent(int status)
            {
                this.scannedBarcodesBufferBlock.SendAsync(this.oposBarcodeScanner.ScanData).Wait();

                // This should not be required since the auto disable is set to false
                // but apparently some barcode scanners require this explictly enabled
                // every time.
                this.oposBarcodeScanner.DataEventEnabled = true;
            }
        }
    }
}
