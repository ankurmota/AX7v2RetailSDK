/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/

/*
 IMPORTANT!!!
 THIS IS SAMPLE CODE ONLY.
 THE CODE SHOULD BE UPDATED TO WORK WITH THE APPROPRIATE PAYMENT PROVIDERS.
 PROPER MESASURES SHOULD BE TAKEN TO ENSURE THAT THE PA-DSS AND PCI DSS REQUIREMENTS ARE MET.
*/
namespace Contoso
{
    namespace Commerce.HardwareStation.Peripherals.PaymentTerminal.SampleL5300
    {
        using System;
        using System.Collections.Generic;
        using System.Diagnostics;
        using System.Linq;
        using System.Runtime.InteropServices;
        using System.Threading.Tasks;
        using Commerce.HardwareStation.Peripherals.PaymentTerminal.EventArgs;
        using Commerce.HardwareStation.Peripherals.PaymentTerminal.Forms;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// <c>VeriFone</c> HYDRA device implementation.
        /// </summary>
        public sealed class HydraPaymentDevice
        {
            private readonly object fpeInstanceSemaphore = new object(); // Semaphore on creating the fpe instance
            private FPEINTERFACELib.CoFPESO fpe; // FPE device interface.
    
            private volatile FPEINTERFACELib.tagTERMINALCONNECTIONSTATUS connectionStatus;
    
            #region Device varialble that must be internally cached
    
            private string track1;
            private string additionalSecInfo;
    
            #endregion
    
            /// <summary>
            /// Initializes a new instance of the <see cref="HydraPaymentDevice" /> class.
            /// </summary>
            public HydraPaymentDevice()
            {
            }
    
            /// <summary>
            ///  Card data received event.
            /// </summary>
            public event EventHandler<CardSwipeEventArgs> CardSwipeEvent = (sender, args) => { };
    
            /// <summary>
            /// Pin data received event.
            /// </summary>
            public event EventHandler<PinDataEventArgs> PinDataEvent = (sender, args) => { };
    
            /// <summary>
            /// Customer entered value event.
            /// </summary>
            public event EventHandler<InputValueEventArgs> InputValueEvent = (sender, args) => { };
    
            /// <summary>
            /// Button press received event.
            /// </summary>
            public event EventHandler<ButtonPressEventArgs> ButtonPressEvent = (sender, args) => { };
    
            /// <summary>
            /// Signature data received event.
            /// </summary>
            public event EventHandler<SignatureEventArgs> SignatureEvent = (sender, args) => { };
    
            /// <summary>
            /// Open connection between PC and the PaymentDevice.
            /// </summary>
            /// <returns>A task that can be awaited until the connection is opened.</returns>
            /// <remarks>The config key is not needed or used for the this device.</remarks>
            public Task OpenAsync()
            {
                if (this.fpe == null)
                {
                    lock (this.fpeInstanceSemaphore)
                    {
                        if (this.fpe == null)
                        {
                            this.InitializeInstance();   // Initilize fpe
                            this.SetupEvents();          // Subscribe to events (including connection event)
                            this.InitializeConnection(); // Open the device for communication (this will result in connection event in the future)
                        }
                    }
                }
    
                return Task.Run(() => true);
            }
    
            /// <summary>
            /// Close connection between PC and the PaymentDevice.
            /// </summary>
            /// <returns>A task that can be awaited until the connection is closed.</returns>
            public Task CloseAsync()
            {
                if ((this.fpe != null) && (this.connectionStatus == FPEINTERFACELib.tagTERMINALCONNECTIONSTATUS.TERMINAL_CONNECTED))
                {
                    this.ClearData();
                    this.fpe.CloseSerialPort();
                    this.connectionStatus = FPEINTERFACELib.tagTERMINALCONNECTIONSTATUS.TERMINAL_DISCONNECTED;
                }
    
                return Task.Run(() => this.RemoveEvents());
            }
    
            /// <summary>
            /// Begins the transaction asynchronous as required for this specific device.
            /// </summary>
            /// <param name="properties">The properties.</param>
            /// <returns>Boolean value to indicate success.</returns>
            public Task<bool> BeginTransactionAsync(IEnumerable<FormProperty> properties)
            {
                return this.ShowFormAsync(Form.CardSwipe, properties);
            }
    
            /// <summary>
            /// Shows a form on the screen.
            /// </summary>
            /// <param name="formName">Name of form to show.</param>
            /// <param name="properties">Device form properties.</param>
            /// <returns>Boolean value to indicate success.</returns>
            public Task<bool> ShowFormAsync(string formName, IEnumerable<FormProperty> properties)
            {
                string deviceForm = this.GetDeviceForm(formName);
    
                return Task.Run(() => this.ShowForm(deviceForm, properties));
            }
    
            /// <summary>
            /// Sets the credit card reader state on the device.
            /// </summary>
            /// <returns>Empty Task.</returns>
            public Task<bool> SetCardSwipeAsync()
            {
                return Task.Run(() => true);
            }
    
            /// <summary>
            /// Sets pin pad entry on the device.
            /// </summary>
            /// <param name="cardNumber">Input debit card number.</param>
            /// <returns>Boolean value to indicate success.</returns>
            public Task<bool> SetPinPadAsync(string cardNumber)
            {
                return Task.Run(() => this.ShowPinEntry("PinPadFrm", cardNumber));
                //// NOTE: The L5300 does not require us to use the following API:
                ////return Task.Run(() => this.PinEntryEnable("PinPadFrm", state, cardNumber));
            }
    
            /// <summary>
            /// Sets signature capture on the current form.
            /// </summary>
            /// <returns>
            /// Boolean indicating success or failure.
            /// </returns>
            public Task<bool> SetSignatureCaptureAsync()
            {
                return Task.Run(() => true);
                //// NOTE: The L5300 does not require us to use the following API:
                ////return Task.Run(() => this.SignatureEntryEnable("SigCapFrm", state));
            }
    
            /// <summary>
            ///  Cancels all pending operations.
            /// </summary>
            /// <returns>A task that can be awaited until operations are cancelled.</returns>
            public async Task CancelOperation()
            {
                await Task.Run(() => this.InternalCancelOperation());
            }
    
            /// <summary>
            ///  Gets the device form instance.
            /// </summary>
            /// <param name="strFormName">Name of form.</param>
            /// <returns>Device form.</returns>
            private string GetDeviceForm(string strFormName)
            {
                string deviceForm = string.Empty;
    
                // Map the hardware station form  name to device form name
                switch (strFormName)
                {
                    case "Processing":
                        deviceForm = "PmtProcessingFrm";
                        break;
                    case "Idle":
                        deviceForm = "TransactionEndFrm";
                        break;
                    case "CardSelection":
                        deviceForm = "CardTypeFrm";
                        break;
                    case "CardSwipe":
                        deviceForm = "TransactionMsrFrm";
                        break;
                    case "Signature":
                        deviceForm = "SigCapFrm";
                        break;
                    case "Thankyou":
                        deviceForm = "LogOffFrm";
                        break;
                    case "Total":
                        deviceForm = "TransactionMsrFrm";
                        break;
                    case "Welcome":
                        deviceForm = "StartupFrm";
                        break;
                    case "Cashback":
                        deviceForm = "CashBackFrm";
                        break;
    
                    default:
                        deviceForm = "ErrorFrm";
                        break;
                }
    
                // Not supported forms
                /*
                deviceForm = "TransactionFrm";
                deviceForm = "CLOSEDFRM";
                deviceForm = "LogOnFrm";
                deviceForm = "PINTIMEOUT";
                deviceForm = "PinPadFrm";
                deviceForm = "PinPadAmtFrm";
                deviceForm = "AmtApprovalFrm";
                 * */
    
                return deviceForm;
            }
    
            #region Device calls
    
            private void InitializeInstance()
            {
                try
                {
                    this.fpe = new FPEINTERFACELib.CoFPESO();
    
                    this.connectionStatus = FPEINTERFACELib.tagTERMINALCONNECTIONSTATUS.TERMINAL_CONNECTION_STATUS_UNKNOWN;
                }
                catch (COMException ex)
                {
                    NetTracer.Warning("L5300Terminal -Unable to create FPE COM object - make sure it is installed: {0}", ex.Message);
                    this.fpe = null;
                }
            }
    
            private void InitializeConnection()
            {
                try
                {
                    if (this.fpe != null)
                    {
                        this.fpe.UseSerialCommunication();
                    }
                }
                catch (COMException ex)
                {
                    NetTracer.Warning(ex, "L5300Terminal - FPE COM exception");
                    this.connectionStatus = FPEINTERFACELib.tagTERMINALCONNECTIONSTATUS.TERMINAL_CONNECTION_STATUS_UNKNOWN;
                }
            }
    
            /// <summary>
            /// Devices the is connected.
            /// </summary>
            /// <returns>Returns true if the FPE has been initialized and the device indicates it is connected.</returns>
            private bool DeviceIsConnected()
            {
                bool isConnected =
                    (this.fpe != null) &&
                    (this.connectionStatus == FPEINTERFACELib.tagTERMINALCONNECTIONSTATUS.TERMINAL_CONNECTED);
    
                Debug.WriteIf(!isConnected, "Attempt to use Hydra device that is not connected");
    
                return isConnected;
            }
    
            private void SetupEvents()
            {
                if (this.fpe != null)
                {
                    this.fpe.ReceivedTerminalConnectionStatusEvent += new FPEINTERFACELib.IFPESOlementationEvents_ReceivedTerminalConnectionStatusEventEventHandler(this.FPE_ReceivedTerminalConnectionStatusEvent);
                    this.fpe.ReceivedVariableValue += new FPEINTERFACELib.IFPESOlementationEvents_ReceivedVariableValueEventHandler(this.FPE_ReceivedVariableValue);
                    this.fpe.ReceivedTrackDataEvent += new FPEINTERFACELib.IFPESOlementationEvents_ReceivedTrackDataEventEventHandler(this.FPE_ReceivedTrackDataEvent);
                    this.fpe.ReceivedPINBlockEvent += new FPEINTERFACELib.IFPESOlementationEvents_ReceivedPINBlockEventEventHandler(this.FPE_ReceivedPINBlockEvent);
                    this.fpe.ReceivedErrorResponseEvent += new FPEINTERFACELib.IFPESOlementationEvents_ReceivedErrorResponseEventEventHandler(this.FPE_ReceivedErrorResponseEvent);
                    this.fpe.ReceivedSigCapDataEvent += new FPEINTERFACELib.IFPESOlementationEvents_ReceivedSigCapDataEventEventHandler(this.FPE_ReceivedSigCapDataEvent);
                }
            }
    
            private void RemoveEvents()
            {
                if (this.fpe != null)
                {
                    this.fpe.ReceivedTerminalConnectionStatusEvent -= new FPEINTERFACELib.IFPESOlementationEvents_ReceivedTerminalConnectionStatusEventEventHandler(this.FPE_ReceivedTerminalConnectionStatusEvent);
                    this.fpe.ReceivedVariableValue -= new FPEINTERFACELib.IFPESOlementationEvents_ReceivedVariableValueEventHandler(this.FPE_ReceivedVariableValue);
                    this.fpe.ReceivedTrackDataEvent -= new FPEINTERFACELib.IFPESOlementationEvents_ReceivedTrackDataEventEventHandler(this.FPE_ReceivedTrackDataEvent);
                    this.fpe.ReceivedPINBlockEvent -= new FPEINTERFACELib.IFPESOlementationEvents_ReceivedPINBlockEventEventHandler(this.FPE_ReceivedPINBlockEvent);
                    this.fpe.ReceivedErrorResponseEvent -= new FPEINTERFACELib.IFPESOlementationEvents_ReceivedErrorResponseEventEventHandler(this.FPE_ReceivedErrorResponseEvent);
                    this.fpe.ReceivedSigCapDataEvent -= new FPEINTERFACELib.IFPESOlementationEvents_ReceivedSigCapDataEventEventHandler(this.FPE_ReceivedSigCapDataEvent);
                }
            }
    
            private bool ShowForm(string strFormName, IEnumerable<FormProperty> properties)
            {
                bool success = false;
    
                try
                {
                    if (this.DeviceIsConnected())
                    {
                        this.fpe.GoToScreen(strFormName);
    
                        if (strFormName == "TransactionMsrFrm" && properties != null)
                        {
                            this.DisplayLines(properties.ToArray());
                        }
    
                        success = true;
                    }
                }
                catch (COMException ex)
                {
                    NetTracer.Warning(ex, "L5300Terminal - FPE COM exception");
                }
    
                return success;
            }
    
            private bool ShowPinEntry(string strFormName, string optionalCardNumber)
            {
                bool success = false;
    
                try
                {
                    if (this.DeviceIsConnected())
                    {
                        if (!string.IsNullOrWhiteSpace(optionalCardNumber))
                        {
                            this.fpe.SetVariableValue("cnum", optionalCardNumber);
                        }
    
                        this.fpe.GoToScreen(strFormName);
    
                        success = true;
                    }
                }
                catch (COMException ex)
                {
                    NetTracer.Warning(ex, "L5300Terminal - FPE COM exception for PinPadFrm");
                }
    
                return success;
            }

            private void InternalCancelOperation()
            {
                try
                {
                    this.ClearData();
    
                    var signatureArgs = new SignatureEventArgs { IsCanceled = true };
                    this.SignatureEvent(null, signatureArgs);
    
                    var inputValueArgs = new InputValueEventArgs { IsCanceled = true };
                    this.InputValueEvent(null, inputValueArgs);
    
                    var creditCardArgs = new CardSwipeEventArgs { IsCanceled = true };
                    this.CardSwipeEvent(null, creditCardArgs);
    
                    var pinDataArgs = new PinDataEventArgs { IsCanceled = true };
                    this.PinDataEvent(null, pinDataArgs);
    
                    var buttonPressArgs = new ButtonPressEventArgs { IsCanceled = true };
                    this.ButtonPressEvent(null, buttonPressArgs);
                }
                catch (COMException ex)
                {
                    NetTracer.Warning(ex, "L5300Terminal - FPE COM exception");
                }
            }
    
            /// <summary>
            /// Clears the data to ensure that any Personal Identifiable data is cleared.
            /// </summary>
            private void ClearData()
            {
                try
                {
                    // Clear device variables that must be internally cachsed
                    this.additionalSecInfo = string.Empty;
                    this.track1 = string.Empty;
    
                    if (this.DeviceIsConnected())
                    {
                        this.fpe.SendData("M\\IR01001001*", 1, 1, 1); // Update display "M"  IR=Remove, 01=Control Id type (01 List), 001=ControlId, 0001=Item, * = all
                        this.fpe.SetVariableValue("cnum", string.Empty);
                        this.fpe.SetVariableValue("Track1Data", string.Empty);
                        this.fpe.SetVariableValue("Track2Data", string.Empty);
                        this.fpe.SetVariableValue("EncryptedPIN", string.Empty);
                        this.fpe.SetVariableValue("AdditionalSecurityInformation", string.Empty);
                    }
                }
                catch (COMException ex)
                {
                    NetTracer.Warning(ex, "L5300Terminal - FPE COM exception");
                }
            }
    
            private void DisplayLines(FormProperty[] properties)
            {
                try
                {
                    if (this.DeviceIsConnected())
                    {
                        this.fpe.SendData("M\\IR01001001*", 1, 1, 1); // Update display "M"  IR=Remove, 01=Control Id type (01 List), 001=ControlId, 0001=Item, * = all
    
                        if (properties != null && properties.Length > 4)
                        {
                            for (int index = 0; index < properties.Length - 4; index++)
                            {
                                this.fpe.SetVariableValue("controlId", "1"); // Select control #1 (RetailListBox)
                                this.fpe.SetVariableValue("li:desc", properties[index].Value); // FPE interferace, appendix A, "Li"stbox
                            }
                        }
                    }
                }
                catch (COMException ex)
                {
                    NetTracer.Warning(ex, "L5300Terminal - FPE COM exception");
                }
            }
    
            #endregion
    
            #region Device Events
    
            private void FPE_ReceivedSigCapDataEvent(object signatureData)
            {
                string signature = string.Empty;
                HydraSignatureEnhanced sig = new HydraSignatureEnhanced((byte[])signatureData);
                signature = Convert.ToBase64String(sig.ToByteArray());
    
                SignatureEventArgs args = new SignatureEventArgs() { SignatureData = signature };
                this.SignatureEvent(this, args);
            }
    
            private void FPE_ReceivedErrorResponseEvent(FPEINTERFACELib.tagERRORRESPONSECODES strErrorResponseCode, string strErrorResponseText)
            {
                RetailLogger.Log.HardwareStationHydraPaymentDeviceError(strErrorResponseCode.ToString(), strErrorResponseText);
    
                if (strErrorResponseCode == FPEINTERFACELib.tagERRORRESPONSECODES.PIN_ENTRY_TIMEDOUT)
                {
                    PinDataEventArgs pinArg = new PinDataEventArgs();
                    pinArg.IsCanceled = true;
                    pinArg.EncryptedPin = string.Empty;
                    pinArg.AdditionalSecurity = string.Empty;
                    this.PinDataEvent(this, pinArg);
                }
            }
    
            private void FPE_ReceivedVariableValue(string strVariableName, string strVariableValue)
            {
                // NOTE: Can not read or set variables in this event handler.  Device API does not support
                switch (strVariableName)
                {
                    case "cnum":
                        // This is for Credit Card Number - as we don't need value disregard result.
                        break;
    
                    case "Track1Data":
                        this.track1 = strVariableValue;
                        break;
    
                    case "Track2Data":
                        // Get previous event data sent and then clear them out
                        var swipeArgs = new CardSwipeEventArgs { Track1 = this.track1, Track2 = strVariableValue, Track3 = string.Empty };
                        this.CardSwipeEvent(this, swipeArgs);
                        break;
    
                    case "RetailCardType":
                        var typeArgs = new ButtonPressEventArgs();
                        typeArgs.ButtonName = strVariableValue;
                        typeArgs.IsCanceled = false;
                        this.ButtonPressEvent(this, typeArgs);
                        break;
    
                    case "CashBackAmt":
                    case "AmountOk":
                        var cashBackArgs = new ButtonPressEventArgs();
                        cashBackArgs.ButtonName = strVariableValue;
                        cashBackArgs.IsCanceled = false;
                        this.ButtonPressEvent(this, cashBackArgs);
                        break;
    
                    case "AdditionalSecurityInformation":
                        this.additionalSecInfo = strVariableValue;
                        break;
    
                    case "EncryptedPIN":
                        PinDataEventArgs pinArg = new PinDataEventArgs();
                        pinArg.IsCanceled = false;
                        pinArg.EncryptedPin = strVariableValue;
                        pinArg.AdditionalSecurity = this.additionalSecInfo;
                        this.PinDataEvent(this, pinArg);
                        this.additionalSecInfo = string.Empty;
                        break;
    
                    case "PinPadEntry":
                        // PIN entry "Cancel" or "TMO"
                        // PIN entry "Request" is set upon form entry (but no fire event)
                        PinDataEventArgs pinPadArg = new PinDataEventArgs();
                        pinPadArg.IsCanceled = true;
                        pinPadArg.EncryptedPin = string.Empty;
                        pinPadArg.AdditionalSecurity = string.Empty;
                        this.PinDataEvent(this, pinPadArg);
                        break;
    
                    case "sig":
                        string signature = string.Empty;
                        HydraSignatureEnhanced sig = new HydraSignatureEnhanced(Convert.FromBase64String(strVariableValue));
                        signature = Convert.ToBase64String(sig.ToByteArray());
    
                        SignatureEventArgs sigArgs = new SignatureEventArgs() { SignatureData = signature };
                        this.SignatureEvent(this, sigArgs);
                        break;
    
                    default:
                        NetTracer.Information("L5300Terminal variable {0} not set to {1}", strVariableName, strVariableValue);
                        break;
                }
            }
    
            private void FPE_ReceivedPINBlockEvent(string strPINBlock, string strKSN)
            {
                PinDataEventArgs pinPadArg = new PinDataEventArgs();
    
                pinPadArg.IsCanceled = false;
                pinPadArg.EncryptedPin = strPINBlock;
                pinPadArg.AdditionalSecurity = strKSN;
                this.PinDataEvent(this, pinPadArg);
            }
    
            private void FPE_ReceivedTrackDataEvent(object varTrack1Data, object varTrack2Data, object varTrack3Data)
            {
                var swipeArgs = new CardSwipeEventArgs { Track1 = varTrack1Data.ToString(), Track2 = varTrack2Data.ToString(), Track3 = varTrack3Data.ToString() };
                this.CardSwipeEvent(this, swipeArgs);
            }
    
            private void FPE_ReceivedTerminalConnectionStatusEvent(FPEINTERFACELib.tagTERMINALCONNECTIONSTATUS terminalConnectionStatus)
            {
                this.connectionStatus = terminalConnectionStatus;
    
                switch (terminalConnectionStatus)
                {
                    case FPEINTERFACELib.tagTERMINALCONNECTIONSTATUS.TERMINAL_CONNECTED:
                        NetTracer.Information("L5300Terminal Connected");
                        Debug.WriteLine("L5300Terminal Connected");
                        break;
                    case FPEINTERFACELib.tagTERMINALCONNECTIONSTATUS.TERMINAL_CONNECTION_STATUS_UNKNOWN:
                        NetTracer.Warning("L5300Terminal connection status unknown");
                        Debug.WriteLine("L5300Terminal connection status unknown");
                        break;
                    case FPEINTERFACELib.tagTERMINALCONNECTIONSTATUS.TERMINAL_DISCONNECTED:
                        NetTracer.Information("L5300Terminal connection status disconnected");
                        Debug.WriteLine("L5300Terminal connection status disconnected");
                        break;
                }
            }
    
            #endregion
        }
    }
}
