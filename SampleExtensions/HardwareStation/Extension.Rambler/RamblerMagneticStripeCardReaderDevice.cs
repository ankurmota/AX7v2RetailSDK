/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

// Enable this directive when external rambler references are available.
#define EXTERNAL_RAMBLER_AVAILABLE

#undef EXTERNAL_RAMBLER_AVAILABLE
#if EXTERNAL_RAMBLER_AVAILABLE
namespace Contoso
{
    namespace Commerce.HardwareStation.RamblerSample
    {
        using System;
        using System.Collections.Generic;
        using System.Composition;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;
        using readerapi_windowsstore;
        using Retail.Diagnostics;

        /// <summary>
        /// The rambler magnetic stripe reader controller class.
        /// </summary>    
        [Export("OPOS", typeof(IRamblerStripeReader))]
        public sealed class RamblerMagneticStripeCardReaderDevice : ReaderController.ReaderStateChangedListener, IRamblerStripeReader
        {
            private static readonly object SyncLock = new object();
            private readonly ReaderController reader;

            private bool isCardReaderEnabled;

            /// <summary>
            /// Initializes a new instance of the <see cref="RamblerMagneticStripeCardReaderDevice" /> class.
            /// </summary>
            public RamblerMagneticStripeCardReaderDevice()
            {
                lock (SyncLock)
                {
                    this.reader = new ReaderController(this);
                }
            }

            /// <summary>
            /// Callback delegate used for notification of successful card read.
            /// </summary>
            public event EventHandler<RamblerMagneticStripeCardData> RamblerCardSwipeEvent;

            /// <summary>
            /// Opens a rambler device.
            /// </summary>
            /// <param name="peripheralName">Name of the peripheral.</param>
            /// <param name="peripheralConfig">Configuration parameters of the peripheral.</param>
            public void Open(string peripheralName, PeripheralConfiguration peripheralConfig)
            {
                this.StartReader();

                lock (SyncLock)
                {
                    this.isCardReaderEnabled = true;
                }
            }

            /// <summary>
            /// Closes the rambler device.
            /// </summary>
            public void Close()
            {
                this.StopReader();

                lock (SyncLock)
                {
                    this.isCardReaderEnabled = false;
                    this.RamblerCardSwipeEvent = null;
                }
            }

            /// <summary>
            /// Start the reader using the rambler controller.
            /// </summary>
            public void StartReader()
            {
                // Go back to idle state before starting the reader.
                this.StopReader();

                lock (SyncLock)
                {
                    NetTracer.Information(string.Format("The rambler device is in state: {0}", this.reader.getReaderState().ToString()));
                    
                    // Start the reader only if the device is in idle state.
                    if (this.reader.getReaderState() == ReaderControllerState.STATE_IDLE)
                    {                        
                        this.reader.startReader();
                        NetTracer.Information(string.Format("The rambler device is successfully started."));
                    }
                }
            }

            /// <summary>
            /// Stop the reader using the rambler controller.
            /// </summary>
            public void StopReader()
            {
                lock (SyncLock)
                {
                    NetTracer.Information(string.Format("Entering stop reader method..."));
                    NetTracer.Information(string.Format("The rambler device is in state: {0}", this.reader.getReaderState().ToString()));

                    if (this.reader.getReaderState() != ReaderControllerState.STATE_IDLE)
                    {
                        this.reader.stopReader();
                        NetTracer.Information(string.Format("The rambler device is stopped."));
                    }
                }
            }
            
            void ReaderController.ReaderStateChangedListener.onDecodeCompleted(Dictionary<string, string> decodeData)
            {
                NetTracer.Information(string.Format("Entering onDecodeCompleted event..."));

                if (this.RamblerCardSwipeEvent != null)
                {
                    // The card has been read and decoded.
                    // Now parse the received data and send it to the subscriber
                    lock (SyncLock)
                    {
                        if (!this.isCardReaderEnabled)
                        {
                            return;
                        }

                        try
                        {
                            RamblerMagneticStripeCardData ramblerCardData = new RamblerMagneticStripeCardData();

                            // Parse the received data and return card
                            ramblerCardData.ParseCard(decodeData);

                            // Notify the client with the rambler card data.
                            this.NotifyCompleted(ramblerCardData);
                        }
                        catch
                        {
                            NetTracer.Warning(string.Format("The tracks information are not extracted correctly. Retrying again..."));

                            // Restart the device to capture the swipe information again.
                            this.StartReader();
                        }
                    }
                }
            }

            void ReaderController.ReaderStateChangedListener.onCardSwipeDetected()
            {
            }

            void ReaderController.ReaderStateChangedListener.onDecodeError(DecodeResult decodeResult)
            {
                NetTracer.Warning(string.Format("The decode error occured : {0}", decodeResult.ToString()));

                // Restart the device to capture the swipe information again.
                this.StartReader();
            }

            void ReaderController.ReaderStateChangedListener.onError(string message)
            {
                NetTracer.Warning(string.Format("An error occured : {0}", message));

                // Restart the device to capture the swipe information again.
                this.StartReader();
            }

            void ReaderController.ReaderStateChangedListener.onGetKsnCompleted(string ksn)
            {
                NetTracer.Information(string.Format("The Ksn is completed with device info: {0}", ksn));
            }

            void ReaderController.ReaderStateChangedListener.onInterrupted()
            {
            }

            void ReaderController.ReaderStateChangedListener.onTimeout()
            {
            }

            void ReaderController.ReaderStateChangedListener.onWaitingForDevice()
            {
            }

            void ReaderController.ReaderStateChangedListener.onWaitingForCardSwipe()
            {
            }            
            
            private void NotifyCompleted(RamblerMagneticStripeCardData card)
            {
                NetTracer.Information("Notify client with the MSR card data");

                if (this.RamblerCardSwipeEvent != null)
                {
                    this.RamblerCardSwipeEvent(this, card);
                }
            }
        }
    }
}
#endif