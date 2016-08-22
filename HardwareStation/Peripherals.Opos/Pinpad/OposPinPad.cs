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
        using System;
        using System.Collections.Generic;
        using System.Composition;
        using System.Threading.Tasks;
        using Interop.OposConstants;
        using Interop.OposPinpad;
        using Microsoft.Dynamics.Commerce.HardwareStation;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Class implements OPOS based PinPad for hardware station.
        /// </summary>
        [Export(PeripheralType.Opos, typeof(IPinPad))]
        public sealed class OposPinPad : IPinPad
        {
            private const string EncryptionAlgorithm = "DUKPT";
            private OPOSPINPad oposPinpad;

            /// <summary>
            /// Occurs when [entry complete event].
            /// </summary>
            public event EventHandler<PinPadResults> EntryCompleteEvent;

            /// <summary>
            /// Gets or sets the last results.
            /// </summary>
            /// <value>
            /// The last results.
            /// </value>
            public PinPadResults LastResult { get; set; }

            /// <summary>
            /// Opens a peripheral.
            /// </summary>
            /// <param name="peripheralName">Name of the peripheral.</param>
            /// <param name="peripheralConfig">Configuration parameters of the peripheral.</param>
            public void Open(string peripheralName, PeripheralConfiguration peripheralConfig)
            {
                if (this.oposPinpad == null)
                {
                    this.oposPinpad = new OPOSPINPadClass();

                    this.oposPinpad.DataEvent += this.OnPinPadDataEvent; // _IOPOSPINPadEvents_DataEventEventHandler
                    this.oposPinpad.ErrorEvent += this.OnPinPadErrorEvent; // _IOPOSPINPadEvents_ErrorEventEventHandler

                    try
                    {
                        // Open
                        Task.Run(() => this.oposPinpad.Open(peripheralName)).Wait(OposHelper.ClaimTimeOut);
                        OposHelper.CheckResultCode(this, this.oposPinpad.ResultCode);

                        // Claim
                        this.oposPinpad.ClaimDevice(OposHelper.ClaimTimeOut);
                        OposHelper.CheckResultCode(this, this.oposPinpad.ResultCode);
                    }
                    catch
                    {
                        this.Close();
                        throw;
                    }
                }
            }

            /// <summary>
            /// Closes the peripheral.
            /// </summary>
            public void Close()
            {
                if (this.oposPinpad != null)
                {
                    try
                    {
                        this.oposPinpad.DataEvent -= this.OnPinPadDataEvent;
                        this.oposPinpad.ErrorEvent -= this.OnPinPadErrorEvent;

                        this.AbortTransaction(); // Abort any pending operation.
                        this.oposPinpad.Close(); // Releases the device and its resources
                    }
                    finally
                    {
                        this.oposPinpad = null;
                    }
                }
            }

            /// <summary>
            /// Begins the electronic funds transaction.
            /// </summary>
            /// <param name="amount">The amount.</param>
            /// <param name="accountNumber">The account number.</param>
            public void BeginElectronicFundsTransaction(decimal amount, string accountNumber)
            {
                if (this.oposPinpad != null)
                {
                    this.oposPinpad.DeviceEnabled = true;
                    this.oposPinpad.DataEventEnabled = true;

                    // Clear any all device input that has been buffered (for data events)
                    this.oposPinpad.ClearInput();

                    this.oposPinpad.Amount = amount;
                    this.oposPinpad.AccountNumber = accountNumber;
                    this.oposPinpad.TerminalID = "1"; // Note - we do not provide terminal ID for device on hardware station

                    if (amount < 0)
                    {   // Money is being refunded to the debit card - this is credit
                        this.oposPinpad.TransactionType = (int)Interop.OposConstants.OPOSPINPadConstants.PPAD_TRANS_CREDIT;
                    }
                    else
                    {   // Debit trans
                        this.oposPinpad.TransactionType = (int)Interop.OposConstants.OPOSPINPadConstants.PPAD_TRANS_DEBIT;
                    }

                    this.oposPinpad.Track1Data = string.Empty;
                    this.oposPinpad.Track2Data = string.Empty;
                    this.oposPinpad.Track3Data = string.Empty;
                    this.oposPinpad.Track4Data = string.Empty;

                    // hardcoded value for Hardware Station
                    int transactionHost = 1;

                    this.oposPinpad.BeginEFTTransaction(EncryptionAlgorithm, transactionHost);
                    OposHelper.CheckResultCode(this, this.oposPinpad.ResultCode);

                    this.oposPinpad.EnablePINEntry();
                    OposHelper.CheckResultCode(this, this.oposPinpad.ResultCode);
                }
                else
                {
                    this.AbortTransaction();
                }
            }

            /// <summary>
            /// Aborts the transaction.
            /// </summary>
            public void AbortTransaction()
            {
                PinPadResults results = new PinPadResults(
                    true,
                    (int)OPOSPINPadConstants.PPAD_EFT_ABNORMAL,
                    string.Empty,
                    string.Empty);

                if (this.EntryCompleteEvent != null)
                {
                    this.EntryCompleteEvent(this, results);
                }
            }

            /// <summary>
            /// Ends the electronic funds transaction.
            /// </summary>
            /// <param name="completionCode">The completionCode code.</param>
            public void EndElectronicFundsTransaction(int completionCode)
            {
                if (this.oposPinpad != null)
                {
                    int resultCode = this.oposPinpad.EndEFTTransaction(completionCode);
                    OposHelper.CheckResultCode(this, resultCode);

                    this.oposPinpad.DeviceEnabled = false;
                    this.oposPinpad.DataEventEnabled = false;
                }
            }

            /// <summary>
            /// OPOS device error event handler.
            /// </summary>
            /// <param name="resultCode">The result code.</param>
            /// <param name="resultCodeExtended">The result code extended.</param>
            /// <param name="errorLocus">The error locus.</param>
            /// <param name="errorResponseRef">The error response reference.</param>
            /// <exception cref="System.NotImplementedException">Not yet implemented.</exception>
            private void OnPinPadErrorEvent(int resultCode, int resultCodeExtended, int errorLocus, ref int errorResponseRef)
            {
                var ex = new PeripheralException(PeripheralException.PeripheralEventError, "Error Event from Peripheral with error code {0}", resultCode);
                RetailLogger.Log.HardwareStationPeripheralError("OposPinPad", resultCode, resultCodeExtended, ex);
                throw ex;
            }

            /// <summary>
            /// OPOS device data event handler.
            /// </summary>
            /// <param name="status">The status.</param>
            private void OnPinPadDataEvent(int status)
            {
                PinPadResults results = new PinPadResults(
                    status != (int)OPOSPINPadConstants.PPAD_EFT_NORMAL,
                    status,
                    this.oposPinpad.EncryptedPIN,
                    this.oposPinpad.AdditionalSecurityInformation);

                this.LastResult = results;

                if (this.EntryCompleteEvent != null)
                {
                    this.EntryCompleteEvent(this, results);
                }
            }
        }
    }
}
