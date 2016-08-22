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
    namespace Commerce.HardwareStation.Peripherals.PaymentTerminal.SampleMX925.Protocols
    {
        using System;
        using System.Collections.Generic;
        using System.Diagnostics;
        using System.Drawing;
        using System.Globalization;
        using System.Linq;
        using System.Threading;
        using System.Threading.Tasks;
        using Commerce.HardwareStation.Peripherals.PaymentTerminal.EventArgs;
        using Commerce.HardwareStation.Peripherals.PaymentTerminal.SampleMX925.DeviceForms;
        using Commerce.HardwareStation.Peripherals.PaymentTerminal.SampleMX925.Protocols.EventArgs;
        using Commerce.HardwareStation.Peripherals.PaymentTerminal.SampleMX925.Transport;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        ///  Class implementing MX925 device protocol.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Verifone", Justification = "Device manufacturer.")]
        public class VerifoneDeviceProtocol : IDeviceProtocol
        {
            /// <summary>
            ///  Default buffer size.
            /// </summary>
            public const int DefaultBufferSize = 1024;
            private const int MaxRetries = 3;
    
            private const int AcknowledgementTimeoutSeconds = 3;
            private const int ResponseTimeoutSeconds = 5;
    
            // Max time in seconds to wait for the user to begin signing (0 - 300).
            private const int SignatureStartTime = 45;
    
            // Max time to wait after a user completes signing (0 - 300).
            private const int SignatureEndTime = 8;
    
            // Signature resolution.
            private const string SignatureResolution = "000";

            private const string Name = "VerifoneDeviceProtocol";

            private readonly List<byte> signatureData = new List<byte>();
            private ITransport transport;
            private CancellationTokenSource cancelTokenSource;
            private string currentForm;
            private Task receiveDataTask;
    
            private TaskCompletionSource<bool> acknowledgement;
            private TaskCompletionSource<ResponseMessage> response;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="VerifoneDeviceProtocol"/> class.
            /// </summary>
            /// <param name="config">Configuration parameters.</param>
            public VerifoneDeviceProtocol(IDictionary<string, string> config)
            {
                this.transport = TransportFactory.GetTransport(config);
            }
    
            /// <summary>
            ///  Card Data Event.
            /// </summary>
            public event EventHandler<CardSwipeEventArgs> CardSwipeEvent;
    
            /// <summary>
            /// Pin Data Event.
            /// </summary>
            public event EventHandler<PinDataEventArgs> PinDataEvent;
    
            /// <summary>
            /// Enter Keypad Event.
            /// </summary>
            public event EventHandler<DeviceKeypadEventArgs> EnterKeypadEvent;
    
            /// <summary>
            /// Button Press Event.
            /// </summary>
            public event EventHandler<DeviceButtonPressEventArgs> ButtonPressEvent;
    
            /// <summary>
            /// Signature Event.
            /// </summary>
            public event EventHandler<SignatureEventArgs> SignatureEvent;
    
            /// <summary>
            /// Open connection between PC and the PaymentDevice.
            /// </summary>
            /// <returns>A task that can be awaited until the connection is opened.</returns>
            public async Task OpenAsync()
            {
                if (this.cancelTokenSource != null)
                {
                    this.cancelTokenSource.Dispose();
                }
    
                this.cancelTokenSource = new CancellationTokenSource();
                await this.transport.ConnectAsync();
                this.receiveDataTask = Task.Run(() => this.ReceiveDataFromDevice(), this.cancelTokenSource.Token);
                await this.ResetTerminalStateAsync();
            }
    
            /// <summary>
            /// CloseAsync connection between PC and the PaymentDevice.
            /// </summary>
            /// <returns>A task that can be awaited until the connection is opened.</returns>
            public async Task CloseAsync()
            {
                await this.ResetTerminalStateAsync();
                await this.ShowIdleForm();
                this.cancelTokenSource.Cancel();
                await this.transport.CloseAsync();
                this.cancelTokenSource.Dispose();
                this.cancelTokenSource = null;
                await this.receiveDataTask;
            }
    
            /// <summary>
            /// Shows a form on the screen.
            /// </summary>
            /// <param name="formName">Name of form to show.</param>
            /// <param name="properties">Device form properties. </param>
            /// <returns> Boolean value to indicate success.</returns>
            public async Task<bool> ShowFormAsync(string formName, IEnumerable<DeviceFormProperty> properties)
            {
                var initFormResponse = await this.SendCommandWaitForAckAndResult(ProtocolCommands.InitForm, formName);
                if (initFormResponse == null || !string.Equals(initFormResponse[0], ProtocolConstants.Success))
                {
                    return false;
                }
    
                if (properties != null)
                {
                    bool success = await this.AddFormProperties(properties);
                    if (!success)
                    {
                        return false;
                    }
                }
    
                var showFormResponse = await this.SendCommandWaitForAckAndResult(ProtocolCommands.ShowForm);
                if (showFormResponse == null || !string.Equals(showFormResponse[0], ProtocolConstants.Success))
                {
                    return false;
                }
    
                this.currentForm = formName;
    
                return true;
            }
    
            /// <summary>
            /// Turn on the PaymentDevice lights for card swipe.
            /// </summary>
            /// <param name="state">Enabled or disabled.</param>
            /// <returns>Empty Task.</returns>
            public async Task<bool> SetCardSwipeAsync(DeviceState state)
            {
                await this.ResetTerminalStateAsync();
    
                if (state == DeviceState.Disabled)
                {
                    return true;
                }
    
                // Don't wait for response since it is handled as an event.
                return await this.SendCommandWaitForAck(ProtocolCommands.EnableCreditCardTrack123);
            }
    
            /// <summary>
            /// Gets the encrypted PIN block from the card.
            /// </summary>
            /// <param name="state">Enabled or disabled.</param>
            /// <param name="cardNumber">Input debit card number.</param>
            /// <returns>Boolean value to indicate success.</returns>
            public async Task<bool> SetPinPadAsync(DeviceState state, string cardNumber)
            {
                await this.ResetTerminalStateAsync();
    
                if (state == DeviceState.Disabled)
                {
                    return true;
                }
    
                var enablePinCommand = string.Format(ProtocolCommands.EnablePinPadCommandFormat, cardNumber);
    
                // Don't wait for response since it is handled as an event.
                return await this.SendCommandWaitForAck(enablePinCommand);
            }
    
            /// <summary>
            /// Send signature commands and get signature data.
            /// </summary>
            /// <param name="startX">The start X coordinate on form to capture signature.</param>
            /// <param name="startY">The start Y coordinate on form to capture signature.</param>
            /// <param name="endX">The end X coordinate on form to capture signature.</param>
            /// <param name="endY">The end Y coordinate on form to capture signature.</param>
            /// <param name="state">Enabled or disabled.</param>
            /// <returns>
            /// Boolean indicating success or failure.
            /// </returns>
            public async Task<bool> SetSignatureCaptureAsync(int startX, int startY, int endX, int endY, DeviceState state)
            {
                await this.ResetTerminalStateAsync();
    
                if (state == DeviceState.Disabled)
                {
                    return true;
                }
    
                var signatureParametersCommand = string.Format(ProtocolCommands.SetSignatureCaptureParametersFormat, SignatureStartTime, SignatureEndTime, SignatureResolution);
    
                // Don't wait for response since it is handled as an event.
                bool success = await this.SendCommandWaitForAck(signatureParametersCommand);
    
                if (success)
                {
                    var signatureAreaCommand = string.Format(ProtocolCommands.SetSignatureCaptureAreaFormat, startX, startY, endX, endY);
                    success = await this.SendCommandWaitForAck(signatureAreaCommand);
                }
    
                if (success)
                {
                    success = await this.SendCommandWaitForAck(ProtocolCommands.EnableSignatureCapture);
                }
    
                return success;
            }
    
            /// <summary>
            /// Resets the terminal application state.
            /// </summary>
            /// <returns>Empty Task.</returns>
            public Task<bool> ResetTerminalStateAsync()
            {
                return this.SendCommandWaitForAck(ProtocolCommands.ResetTerminalState);
            }
    
            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
    
            /// <summary>
            /// Disposes of the wrapped transport.
            /// </summary>
            /// <param name="disposing">Dispose of unmanaged resources.</param>
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (this.transport != null)
                    {
                        this.CloseAsync().Wait();
                        this.transport.Dispose();
                        this.transport = null;
                    }
    
                    if (this.cancelTokenSource != null)
                    {
                        this.cancelTokenSource.Dispose();
                        this.cancelTokenSource = null;
                    }
                }
            }
    
            /// <summary>
            ///  Prints the response message to the output window.
            /// </summary>
            /// <param name="message">Message received.</param>
            private static void WriteDebugMessage(ResponseMessage message)
            {
                string logMessage;

                if (message.IsAck)
                {
                    logMessage = "ACK";
                }
                else if (message.IsNak)
                {
                    logMessage = "NAK";
                }
                else if (message.ResponseValues != null)
                {
                    logMessage = message.Command + " " + string.Join(", ", message.ResponseValues);
                }
                else
                {
                    logMessage = message.Command;
                }

                RetailLogger.Log.HardwareStationPeripheralInteraction(VerifoneDeviceProtocol.Name, "<--" + logMessage);
            }

            /// <summary>
            /// Displays the idle form.
            /// </summary>
            /// <returns>A task that can be awaited until the command is finished.</returns>
            private Task ShowIdleForm()
            {
                var idleForm = new Idle();
                return this.SendCommandWaitForAckAndResult(ProtocolCommands.ShowIdleForm, idleForm.FormName, idleForm.IdleTimeout.ToString(CultureInfo.InvariantCulture));
            }
    
            /// <summary>
            ///  Sets properties on the current form.
            /// </summary>
            /// <param name="properties">Properties to set.</param>
            /// <returns> Boolean value to indicate success.</returns>
            private async Task<bool> AddFormProperties(IEnumerable<DeviceFormProperty> properties)
            {
                foreach (DeviceFormProperty property in properties)
                {
                    string controlId = property.ControlId.ToString(CultureInfo.InvariantCulture);
                    if (property.ControlType == ControlType.ListBox)
                    {
                        var addItemResponse = await this.SendCommandWaitForAckAndResult(ProtocolCommands.AddListBoxItem, controlId, ProtocolConstants.EndOfList, property.Value, ProtocolConstants.DoNotCache);
                        if (addItemResponse == null || !string.Equals(addItemResponse[0], ProtocolConstants.Success))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        var setPropertyResponse = await this.SendCommandWaitForAckAndResult(ProtocolCommands.SetPropValue, controlId, property.Name, property.PropertyType, property.Value);
                        if (setPropertyResponse == null || !string.Equals(setPropertyResponse[0], ProtocolConstants.Success))
                        {
                            return false;
                        }
                    }
                }
    
                return true;
            }
    
            /// <summary>
            ///  Sends a command to the device, wait for an <c>ack</c> and a response result.
            /// </summary>
            /// <param name="parameters">Command name, optional parameters.</param>
            /// <returns>An array of response values.</returns>
            private async Task<IList<string>> SendCommandWaitForAckAndResult(params string[] parameters)
            {
                this.response = new TaskCompletionSource<ResponseMessage>();
                var ackResponse = await this.SendCommandWaitForAck(parameters);
    
                if (ackResponse)
                {
                    return await this.ReceiveResponse(parameters[0]);
                }
    
                return null;
            }
    
            /// <summary>
            ///  Sends a command to the device and wait for an <c>ack</c>.
            /// </summary>
            /// <param name="parameters">Command name, optional parameters.</param>
            /// <returns>Whether the command was sent successfully.</returns>
            private async Task<bool> SendCommandWaitForAck(params string[] parameters)
            {
                if (parameters == null || parameters.Length == 0)
                {
                    throw new ArgumentException("Invalid command");
                }
    
                string commandString = string.Join(ProtocolConstants.FieldSeparator, parameters);
                RetailLogger.Log.HardwareStationPeripheralInteraction(VerifoneDeviceProtocol.Name, "--> " + commandString);
                var envelope = ProtocolUtilities.CreateEnvelope(commandString);
    
                var ackResponse = await this.SendMessage(envelope);
    
                return ackResponse;
            }
    
            /// <summary>
            ///  Sends a message to the transport and waits for an acknowledgement.
            /// </summary>
            /// <param name="envelope">Envelope to send.</param>
            /// <returns>True if an <c>ack</c> was received, false if a <c>nak</c> was received or no response.</returns>
            private async Task<bool> SendMessage(byte[] envelope)
            {
                int retryCount = 0;
                bool ackResponse;
    
                do
                {
                    using (var timeout = new CancellationTokenSource(AcknowledgementTimeoutSeconds * 1000))
                    {
                        this.acknowledgement = new TaskCompletionSource<bool>();
                        timeout.Token.Register(() => this.acknowledgement.TrySetCanceled());
                        await this.transport.SendDataAsync(envelope, 0, envelope.Length, this.cancelTokenSource.Token);
    
                        try
                        {
                            ackResponse = await this.acknowledgement.Task;
                        }
                        catch (TaskCanceledException)
                        {
                            return false;
                        }
    
                        retryCount++;
                    }
                }
                while (!ackResponse && retryCount < MaxRetries);
                return ackResponse;
            }
    
            /// <summary>
            ///  Waits for a response message to be received.
            /// </summary>
            /// <param name="command">Command for which the response is for.</param>
            /// <returns>Result parameters.</returns>
            private async Task<IList<string>> ReceiveResponse(string command)
            {
                using (var timeout = new CancellationTokenSource(ResponseTimeoutSeconds * 1000))
                {
                    timeout.Token.Register(() => this.response.TrySetCanceled());
                    try
                    {
                        ResponseMessage responseMessage = await this.response.Task;
    
                        if (string.Equals(responseMessage.Command, command))
                        {
                            return responseMessage.ResponseValues;
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        return null;
                    }
                }
    
                return null;
            }
    
            /// <summary>
            ///  Sends an acknowledgement packet.
            /// </summary>
            /// <param name="success">Send <c>ack</c> if true, <c>nak</c> if false.</param>
            /// <returns>A task that can be awaited until the acknowledgment packet was sent.</returns>
            private async Task SendAcknowledgement(bool success)
            {
                var envelope = new byte[1];
                envelope[0] = success ? ProtocolConstants.Ack : ProtocolConstants.Nak;
    
                await this.transport.SendDataAsync(envelope, 0, envelope.Length, this.cancelTokenSource.Token);
                RetailLogger.Log.HardwareStationPeripheralInteraction(VerifoneDeviceProtocol.Name, "--> " + (success ? "ACK" : "NAK"));
            }
    
            /// <summary>
            ///  Receive loop to receive data from the device.
            /// </summary>
            /// <returns>A task that can be awaited until the receive loop is finished.</returns>
            private async Task ReceiveDataFromDevice()
            {
                var buffer = new byte[DefaultBufferSize];
    
                while (true)
                {
                    if (this.cancelTokenSource == null || this.cancelTokenSource.IsCancellationRequested)
                    {
                        break;
                    }
    
                    try
                    {
                        int messageLength = await this.transport.ReceiveDataAsync(buffer, 0, buffer.Length, this.cancelTokenSource.Token);
    
                        var message = ResponseMessage.Parse(buffer, messageLength);
                        WriteDebugMessage(message);
    
                        if (!message.IsAck && !message.IsNak)
                        {
                            await this.SendAcknowledgement(message.IsValid);
                        }
    
                        if (message.IsValid)
                        {
                            this.RouteResponse(message);
                        }
    
                        Array.Clear(buffer, 0, DefaultBufferSize);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(ex.ToString());
                    }
                }
            }
    
            /// <summary>
            ///  Handles checking if the message is an acknowledgement message.
            /// </summary>
            /// <param name="responseMessage">Message to check.</param>
            /// <returns>True if message is an acknowledgement, false otherwise.</returns>
            private bool HandleAcknowledgementMessage(ResponseMessage responseMessage)
            {
                if (this.acknowledgement != null)
                {
                    if (responseMessage.IsAck)
                    {
                        this.acknowledgement.TrySetResult(true);
                        return true;
                    }
    
                    if (responseMessage.IsNak)
                    {
                        this.acknowledgement.TrySetResult(false);
                        return true;
                    }
                }
    
                return false;
            }
    
            /// <summary>
            ///  Route the message to the correct task completion or event handler.
            /// </summary>
            /// <param name="responseMessage">Message to route.</param>
            private void RouteResponse(ResponseMessage responseMessage)
            {
                if (this.HandleAcknowledgementMessage(responseMessage))
                {
                    return;
                }
    
                switch (responseMessage.Command)
                {
                    case ProtocolCommands.FormEvent:
                        this.HandleFormEvent(responseMessage);
                        break;
                    case ProtocolCommands.CreditCardDataReceived:
                        this.HandleCreditCardEvent(responseMessage);
                        break;
                    case ProtocolCommands.PinDataReceived:
                        this.HandleEnterPinEvent(responseMessage);
                        break;
                    case ProtocolCommands.SignatureStart:
                        this.HandleSignatureStartEvent(responseMessage);
                        break;
                    case ProtocolCommands.SignatureDataReceived:
                        this.HandleSignatureEvent(responseMessage);
                        break;
                    default:
                        // Otherwise treat as a message response.
                        if (this.response != null && !this.response.Task.IsCompleted)
                        {
                            this.response.TrySetResult(responseMessage);
                        }
    
                        break;
                }
            }
    
            /// <summary>
            ///  Handles signature data started.
            /// </summary>
            /// <param name="responseMessage">Event message.</param>
            private void HandleSignatureStartEvent(ResponseMessage responseMessage)
            {
                this.signatureData.Clear();
    
                // If no signature data.
                if (string.Equals(responseMessage.ResponseValues[0], "00000"))
                {
                    if (this.SignatureEvent != null)
                    {
                        var args = new SignatureEventArgs() { IsCanceled = true };
                        this.SignatureEvent(this, args);
                    }
                }
            }
    
            /// <summary>
            ///  Handles signature data received.
            /// </summary>
            /// <param name="responseMessage">Event message.</param>
            private void HandleSignatureEvent(ResponseMessage responseMessage)
            {
                const int SignatureDataHeaderLength = 9;
    
                // Check for invalid signature event message.
                if (responseMessage.ResponseValues == null || responseMessage.ResponseValues.Count < 2)
                {
                    return;
                }
    
                int length = int.Parse(responseMessage.ResponseValues[1], CultureInfo.InvariantCulture.NumberFormat);
                this.signatureData.AddRange(responseMessage.RawData.Skip(SignatureDataHeaderLength).Take(length));
    
                // No more data to receive.
                if (string.Equals(responseMessage.ResponseValues[0], "N"))
                {
                    if (this.SignatureEvent != null)
                    {
                        var args = new SignatureEventArgs() { SignatureData = this.ConvertSignatureToUpos(this.signatureData) };
                        this.SignatureEvent(this, args);
                    }
                }
            }
    
            /// <summary>
            ///  Handles the credit card data received event.
            /// </summary>
            /// <param name="responseMessage">Event message.</param>
            private void HandleCreditCardEvent(ResponseMessage responseMessage)
            {
                // Check for invalid credit card event message.
                if (responseMessage.ResponseValues == null || responseMessage.ResponseValues.Count < 4)
                {
                    return;
                }
    
                if (this.CardSwipeEvent != null)
                {
                    var args = new CardSwipeEventArgs { Track1 = responseMessage.ResponseValues[0], Track2 = responseMessage.ResponseValues[1], Track3 = responseMessage.ResponseValues[2] };
                    this.CardSwipeEvent(this, args);
                }
            }
    
            /// <summary>
            ///  Handles the credit card data received event.
            /// </summary>
            /// <param name="responseMessage">Event message.</param>
            private void HandleEnterPinEvent(ResponseMessage responseMessage)
            {
                if (this.PinDataEvent != null)
                {
                    var args = new PinDataEventArgs();
    
                    // If we didn't receive the expected number of arguments the data was canceled.
                    if (responseMessage.ResponseValues.Count == 3)
                    {
                        args.AdditionalSecurity = responseMessage.ResponseValues[1];
                        args.EncryptedPin = responseMessage.ResponseValues[2];
                    }
                    else
                    {
                        args.IsCanceled = true;
                    }
    
                    this.PinDataEvent(this, args);
                }
            }
    
            /// <summary>
            ///  Handles the button press and keypad event types.
            /// </summary>
            /// <param name="responseMessage">Event message.</param>
            private void HandleFormEvent(ResponseMessage responseMessage)
            {
                // Check for invalid form event message.
                if (responseMessage.ResponseValues == null || responseMessage.ResponseValues.Count < 2)
                {
                    return;
                }
    
                var controlType = (ControlType)Enum.Parse(typeof(ControlType), responseMessage.ResponseValues[0]);
                var controlId = int.Parse(responseMessage.ResponseValues[1], CultureInfo.InvariantCulture.NumberFormat);
    
                switch (controlType)
                {
                    case ControlType.Button:
                        if (this.ButtonPressEvent != null)
                        {
                            var args = new DeviceButtonPressEventArgs() { ControlId = controlId, FormName = this.currentForm };
                            this.ButtonPressEvent(this, args);
                        }
    
                        break;
                    case ControlType.Keypad:
                        if (this.EnterKeypadEvent != null)
                        {
                            // Check for invalid key press event message.
                            if (responseMessage.ResponseValues.Count < 3)
                            {
                                return;
                            }
    
                            var keyPress = (KeyPressEvent)Enum.Parse(typeof(KeyPressEvent), responseMessage.ResponseValues[2]);
    
                            if (keyPress == KeyPressEvent.EnterPressed || keyPress == KeyPressEvent.Timeout || keyPress == KeyPressEvent.Cancel)
                            {
                                bool isCancel = keyPress == KeyPressEvent.Cancel;
                                string value = isCancel ? string.Empty : responseMessage.ResponseValues[3];
                                var args = new DeviceKeypadEventArgs() { ControlId = controlId, FormName = this.currentForm, Value = value, IsCanceled = isCancel };
                                this.EnterKeypadEvent(this, args);
                            }
                        }
    
                        break;
    
                    default:
                        // Unhandled event type.
                        break;
                }
            }
    
            /// <summary>
            ///  Converts the <c>VeriFone</c> Raw Point signature format to UPOS format.
            /// </summary>
            /// <param name="signatureData"><c>VeriFone</c> raw format signature data.</param>
            /// <returns>UPOS signature format data as base 64.</returns>
            private string ConvertSignatureToUpos(IList<byte> signatureData)
            {
                const string VefifoneRawPointsHeader = "VFISIG00";
                const int VefifoneRawPointsHeaderLength = 8;
                const int NumberOfPointsDataLength = 2;
    
                string result = string.Empty;
    
                // Validate the header
                if (signatureData != null
                    && (signatureData.Count > VefifoneRawPointsHeaderLength + NumberOfPointsDataLength)
                    && (new System.Text.ASCIIEncoding()).GetString(signatureData.Take(VefifoneRawPointsHeaderLength).ToArray()).Equals(VefifoneRawPointsHeader, StringComparison.Ordinal))
                {
                    // Verifone signature format uses 32 bit per point (16 bit X and 16 bit Y) [Big-Endian]
                    // Upos format is 64 bit per point (32 bit X and 32 bit Y) [Little-Endian]
                    int numberOfPoints = BitConverter.ToInt16(signatureData.Skip(VefifoneRawPointsHeaderLength).Take(NumberOfPointsDataLength).Reverse().ToArray(), 0);
                    byte[] verifoneSignatue = signatureData.Skip(VefifoneRawPointsHeaderLength + NumberOfPointsDataLength).ToArray();
                    List<byte> uposSignature = new List<byte>(numberOfPoints * sizeof(int) * 2); // 64bits per point * number of source points.
    
                    for (int i = 0; i < numberOfPoints; i++)
                    {
                        int currentPoint = i * sizeof(int); // 32bits per point
    
                        int x = (verifoneSignatue[currentPoint]  << 8) + verifoneSignatue[currentPoint + 1];
                        int y = (verifoneSignatue[currentPoint + 2] << 8) + verifoneSignatue[currentPoint + 3];
    
                        // Pen up condition.
                        if (x == 0xFFFF)
                        {
                            x = -1;
                        }
    
                        if (y == 0xFFFF)
                        {
                            y = -1;
                        }
    
                        uposSignature.AddRange(BitConverter.GetBytes(x));
                        uposSignature.AddRange(BitConverter.GetBytes(y));
                    }
    
                    result = Convert.ToBase64String(uposSignature.ToArray());
                }
                else
                {
                    RetailLogger.Log.HardwareStationInvalidSignatureFormatError("Verifone MX9x5");
                }
    
                return result;
            }
        }
    }
}
