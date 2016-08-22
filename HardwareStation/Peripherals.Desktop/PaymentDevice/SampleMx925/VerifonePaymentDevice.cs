/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/*
 IMPORTANT!!!
 THIS IS SAMPLE CODE ONLY.
 THE CODE SHOULD BE UPDATED TO WORK WITH THE APPROPRIATE PAYMENT PROVIDERS.
 PROPER MESASURES SHOULD BE TAKEN TO ENSURE THAT THE PA-DSS AND PCI DSS REQUIREMENTS ARE MET.
*/
namespace Contoso
{
    namespace Commerce.HardwareStation.Peripherals.PaymentTerminal.MX925Device
    {
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using System.Threading.Tasks;
        using Commerce.HardwareStation.Peripherals.PaymentTerminal.EventArgs;
        using Commerce.HardwareStation.Peripherals.PaymentTerminal.Forms;
        using Commerce.HardwareStation.Peripherals.PaymentTerminal.SampleMX925.DeviceForms;
        using Commerce.HardwareStation.Peripherals.PaymentTerminal.SampleMX925.Protocols;
        using Commerce.HardwareStation.Peripherals.PaymentTerminal.SampleMX925.Protocols.EventArgs;
        using Microsoft.Dynamics.Commerce.HardwareStation;
        using Microsoft.Dynamics.Commerce.HardwareStation.Configuration;

        /// <summary>
        /// <c>VeriFone</c> MX925 device implementation.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Verifone", Justification = "Device manufacturer.")]
        public class VerifonePaymentDevice : IDisposable
        {
            private const string LanguageKey = "language";

            private IDeviceProtocol protocol;
            private string languageCode;

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
            /// <param name="config">Case insensitive configuration parameters.</param>
            /// <returns>A task that can be awaited until the connection is opened.</returns>
            public Task OpenAsync(IDictionary<string, string> config)
            {
                this.languageCode = config.GetValueOrDefault(LanguageKey, string.Empty);
                this.protocol = new VerifoneDeviceProtocol(config);

                this.protocol.CardSwipeEvent += (sender, args) => this.CardSwipeEvent(sender, args);
                this.protocol.PinDataEvent += (sender, args) => this.PinDataEvent(sender, args);
                this.protocol.SignatureEvent += (sender, args) => this.SignatureEvent(sender, args);
                this.protocol.EnterKeypadEvent += this.HandleEnterKeyPadEvent;
                this.protocol.ButtonPressEvent += this.HandleButtonPressEvent;

                return this.protocol.OpenAsync();
            }

            /// <summary>
            /// Close connection between PC and the PaymentDevice.
            /// </summary>
            /// <returns>A task that can be awaited until the connection is closed.</returns>
            public Task CloseAsync()
            {
                return this.protocol.CloseAsync();
            }

            /// <summary>
            /// Begins the transaction asynchronous as required for this specific device.
            /// </summary>
            /// <param name="properties">The properties.</param>
            /// <returns>Boolean value to indicate success.</returns>
            public Task<bool> BeginTransactionAsync(IEnumerable<FormProperty> properties)
            {
                return this.ShowFormAsync(Form.Welcome, properties);
            }

            /// <summary>
            /// Shows a form on the screen.
            /// </summary>
            /// <param name="formName">Name of form to show.</param>
            /// <param name="properties">Device form properties.</param>
            /// <returns>Boolean value to indicate success.</returns>
            public Task<bool> ShowFormAsync(string formName, IEnumerable<FormProperty> properties)
            {
                if (formName == null)
                {
                    throw new ArgumentNullException("formName");
                }

                if (formName.Equals(Form.Cashback))
                {
                    // Currently the cash back form isn't supported so return
                    return Task.FromResult(false);
                }

                IForm deviceForm = this.GetDeviceForm(formName);
                IEnumerable<DeviceFormProperty> deviceProperties = null;
                if (properties != null)
                {
                    deviceProperties = deviceForm.CreateProperties(properties);
                }

                return this.protocol.ShowFormAsync(deviceForm.FormName, deviceProperties);
            }

            /// <summary>
            /// Sets the credit card reader state on the device.
            /// </summary>
            /// <param name="state">Enabled or disabled.</param>
            /// <returns>Empty Task.</returns>
            public Task<bool> SetCardSwipeAsync(DeviceState state)
            {
                return this.protocol.SetCardSwipeAsync(state);
            }

            /// <summary>
            /// Sets pin pad entry on the device.
            /// </summary>
            /// <param name="state">Enabled or disabled.</param>
            /// <param name="cardNumber">Input debit card number.</param>
            /// <returns>Boolean value to indicate success.</returns>
            public Task<bool> SetPinPadAsync(DeviceState state, string cardNumber)
            {
                return this.protocol.SetPinPadAsync(state, cardNumber);
            }

            /// <summary>
            /// Sets signature capture on the current form.
            /// </summary>
            /// <param name="state">Enabled or disabled.</param>
            /// <returns>
            /// Boolean indicating success or failure.
            /// </returns>
            public Task<bool> SetSignatureCaptureAsync(DeviceState state)
            {
                return this.protocol.SetSignatureCaptureAsync(Signature.StartX, Signature.StartY, Signature.EndX, Signature.EndY, state);
            }

            /// <summary>
            ///  Cancels all pending operations.
            /// </summary>
            /// <returns>A task that can be awaited until operations are cancelled.</returns>
            public async Task CancelOperation()
            {
                await this.protocol.ResetTerminalStateAsync();

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

            /// <summary>
            /// Disposes of the connection to the device.
            /// </summary>
            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Disposes of the wrapped transport.
            /// </summary>
            /// <param name="disposing">Whether to dispose managed resources.</param>
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (this.protocol != null)
                    {
                        this.protocol.Dispose();
                        this.protocol = null;
                    }
                }
            }

            /// <summary>
            ///  Gets the device form instance.
            /// </summary>
            /// <param name="formName">Name of form.</param>
            /// <returns>Device form.</returns>
            private IForm GetDeviceForm(string formName)
            {
                string localizedFormName = string.Concat(formName, this.languageCode).ToUpperInvariant();
                var deviceForm = CompositionManager.Instance.GetComponent<IForm>(localizedFormName) ?? CompositionManager.Instance.GetComponent<IForm>(formName.ToUpperInvariant());
                if (deviceForm == null)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Cannot find form {0}", formName));
                }

                return deviceForm;
            }

            /// <summary>
            ///  Translates the protocol event args to value event args.
            /// </summary>
            /// <param name="sender">Event sender.</param>
            /// <param name="args">Event arguments.</param>
            private void HandleEnterKeyPadEvent(object sender, DeviceKeypadEventArgs args)
            {
                var inputValueArgs = new InputValueEventArgs { IsCanceled = args.IsCanceled, Value = args.Value };
                this.InputValueEvent(sender, inputValueArgs);
            }

            /// <summary>
            ///  Translates the protocol event args to value event args.
            /// </summary>
            /// <param name="sender">Event sender.</param>
            /// <param name="args">Event arguments.</param>
            private void HandleButtonPressEvent(object sender, DeviceButtonPressEventArgs args)
            {
                var deviceForm = this.GetDeviceForm(args.FormName);
                string buttonName = deviceForm.GetControlName(args.ControlId);
                var buttonPressArgs = new ButtonPressEventArgs { ButtonName = buttonName };
                this.ButtonPressEvent(sender, buttonPressArgs);
            }
        }
    }
}
