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
    namespace Commerce.HardwareStation.Peripherals.MagneticSwipeReader
    {
        using System;
        using System.Collections.Generic;
        using System.Composition;
        using Interop.OposMSR;
        using Microsoft.Dynamics.Commerce.HardwareStation;
        using Microsoft.Dynamics.Commerce.HardwareStation.CardPayment;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.Entities;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Class implements OPOS based Magnetic Swipe Reader for hardware station.
        /// </summary>
        [Export(PeripheralType.Opos, typeof(IMagneticSwipeReader))]
        public sealed class OposMagneticSwipeReader : IMagneticSwipeReader
        {
            private OPOSMSR oposMsr;

            /// <summary>
            /// Occurs when card is swiped.
            /// </summary>
            public event EventHandler<MagneticCardSwipeInfo> MsrSwipeEvent;

            /// <summary>
            /// Opens a peripheral.
            /// </summary>
            /// <param name="peripheralName">Name of the peripheral.</param>
            /// <param name="peripheralConfig">Configuration parameters of the peripheral.</param>
            public void Open(string peripheralName, PeripheralConfiguration peripheralConfig)
            {
                this.oposMsr = new OPOSMSR();

                // Open
                this.oposMsr.Open(peripheralName);
                OposHelper.CheckResultCode(this, this.oposMsr.ResultCode);

                // Claim
                this.oposMsr.ClaimDevice(OposHelper.ClaimTimeOut);
                OposHelper.CheckResultCode(this, this.oposMsr.ResultCode);

                // Enable/Configure
                this.oposMsr.DeviceEnabled = true;

                // Set the decode data - so that the device decodes the scanned data
                this.oposMsr.DecodeData = true;

                // Note: there are two properties that look similar
                // ParseDecodedData and ParseDecodeData
                // Both do the same as per the OPOS spec.
                // Setting this property makes the device return data
                // in individual fields.
                this.oposMsr.ParseDecodedData = true;

                // Set Transmit Sentinels to true
                // so that when the data is sent, we can get the sentenels
                // and can parse the data of the tracks.
                this.oposMsr.TransmitSentinels = true;

                // Plug in handlers for data eevents
                this.oposMsr.DataEvent += this.OnMsrDataEvent;
                this.oposMsr.ErrorEvent += this.OnMsrErrorEvent;

                // Set autodisable to false
                this.oposMsr.AutoDisable = false;

                // Enable data events
                this.oposMsr.DataEventEnabled = true;
            }

            /// <summary>
            /// Closes a connection with MSR.
            /// </summary>
            public void Close()
            {
                if (this.oposMsr != null)
                {
                    this.oposMsr.DataEvent -= this.OnMsrDataEvent;
                    this.oposMsr.ErrorEvent -= this.OnMsrErrorEvent;
                    this.oposMsr.ReleaseDevice();
                    this.oposMsr.Close();
                    this.oposMsr = null;
                }
            }

            private void OnMsrErrorEvent(int resultCode, int resultCodeExtended, int errorLocus, ref int errorResponse)
            {
                var ex = new PeripheralException(PeripheralException.PeripheralEventError, "Error Event from Peripheral with error code {0}", resultCode);
                RetailLogger.Log.HardwareStationPerpheralError("Magnetic Swipe Reader (MSR)", resultCode, resultCodeExtended, ex);

                throw ex;
            }

            private void OnMsrDataEvent(int status)
            {
                if (this.MsrSwipeEvent != null)
                {
                    var swipeInfo = new MagneticCardSwipeInfo();
                    swipeInfo.ParseTracks(this.oposMsr.Track1Data, this.oposMsr.Track2Data);

                    // Store the account number in the temporary storage and mask the accout number only if it's the payment card.
                    if (swipeInfo.IsPaymentCard)
                    {
                        // Store the swipe info into the temporary memory storage.
                        var temporaryMemoryManager = new TemporaryMemoryManager<string, string>();
                        temporaryMemoryManager.AddCardInfoToMemory(MagneticCardSwipeInfo.MsrCardKey, swipeInfo.AccountNumber, DateTime.UtcNow, swipeInfo.AccountNumber);

                        // Mask the account number using mask chars.
                        string maskedAccountNumber = Utilities.GetMaskedCardNumber(swipeInfo.AccountNumber);
                        swipeInfo.AccountNumber = maskedAccountNumber;
                    }

                    this.MsrSwipeEvent(this, swipeInfo);
                }

                // This should not be required since the auto disable is set to false
                // but apparently some Msrs require this explictly enabled
                // every time.
                this.oposMsr.DataEventEnabled = true;
            }
        }
    }
}
