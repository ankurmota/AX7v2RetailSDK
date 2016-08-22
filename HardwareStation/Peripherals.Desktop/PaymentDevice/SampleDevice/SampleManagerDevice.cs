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
    namespace Commerce.HardwareStation.Peripheral.HardwareStation.Peripherals.SampleDevice
    {
        using System;
        using System.Collections.Generic;
        using System.Composition;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.HardwareStation.CardPayment;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.PaymentTerminal;
        using Microsoft.Dynamics.Retail.PaymentSDK.Portable;

        /// <summary>
        /// SampleDevice class.
        /// </summary>
        [Export("SAMPLEDEVICE", typeof(IPaymentDevice))]
        public class SampleManagerDevice : IPaymentDevice, IDisposable
        {
            /// <summary>
            /// Construct the Payment Device class and open the connection from it.
            /// </summary>
            /// <param name="peripheralName">Name of peripheral device.</param>
            /// <param name="terminalSettings">The terminal settings for the peripheral device.</param>
            /// <param name="deviceConfig">Device Configuration parameters.</param>
            /// <returns>A task that can be awaited until the connection is opened.</returns>
            public async Task OpenAsync(string peripheralName, SettingsInfo terminalSettings, IDictionary<string, string> deviceConfig)
            {
                // Open the device for payments
                await Task.Delay(500);
            }

            /// <summary>
            ///  Closes a connection to the payment terminal.
            /// </summary>
            /// <returns>A task that can be awaited until the connection is closed.</returns>
            public async Task CloseAsync()
            {
                // Close the device for payments
                await Task.Delay(500);
            }

            /// <summary>
            ///  Begins the transaction.
            /// </summary>
            /// <param name="merchantProperties">The merchant provider payment properties for the peripheral device.</param>
            /// <param name="paymentConnectorName">The payment connector name.</param>
            /// <param name="invoiceNumber">The invoice number associated with the transaction (6 characters long).</param>
            /// <param name="isTestMode">Is test mode for payments enabled for the peripheral device.</param>
            /// <returns>A task that can be awaited until the begin transaction screen is displayed.</returns>
            public async Task BeginTransactionAsync(PaymentProperty[] merchantProperties, string paymentConnectorName, string invoiceNumber, bool isTestMode)
            {
                // Begin the transaction for payments
                await Task.Delay(500);
            }

            /// <summary>
            ///  Ends the transaction.
            /// </summary>
            /// <returns>A task that can be awaited until the end transaction screen is displayed.</returns>
            public async Task EndTransactionAsync()
            {
                // End the transaction for payments
                await Task.Delay(500);
            }

            /// <summary>
            /// Update the line items on the current open session.  This method will compare against previous lines specified and make the appropriate device calls.
            /// </summary>
            /// <param name="totalAmount">The total amount of the transaction, including tax.</param>
            /// <param name="taxAmount">The total tax amount on the transaction.</param>
            /// <param name="discountAmount">The total discount amount on the transaction.</param>
            /// <param name="subTotalAmount">The sub-total amount on the transaction.</param>
            /// <param name="items">The items in the transaction.</param>
            /// <returns>A task that can be awaited until the text is displayed on the screen.</returns>
            public async Task UpdateLineItemsAsync(string totalAmount, string taxAmount, string discountAmount, string subTotalAmount, IEnumerable<ItemInfo> items)
            {
                // Display item to the payment terminal
                await Task.Delay(500);
            }

            /// <summary>
            /// Make authorization payment.
            /// </summary>
            /// <param name="amount">The amount.</param>
            /// <param name="currency">The currency.</param>
            /// <param name="voiceAuthorization">The voice approval code (optional).</param>
            /// <param name="isManualEntry">If manual credit card entry is required.</param>
            /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
            /// <returns>A task that can await until the authorization has completed.</returns>
            public async Task<PaymentInfo> AuthorizePaymentAsync(decimal amount, string currency, string voiceAuthorization, bool isManualEntry, ExtensionTransaction extensionTransactionProperties)
            {
                return await Task.FromResult<PaymentInfo>(new PaymentInfo()
                {
                    CardNumberMasked = "411111******1111",
                    CardType = Microsoft.Dynamics.Commerce.HardwareStation.CardPayment.CardType.InternationalCreditCard,
                    SignatureData = "AAgEAAQALP4hvpJrK/UfKvlX7ABkIfJFnxoZbaXC2vmnzYB8ItM1rBYwzRrw0IdLF3Qv89lwBfgGn5gBwKkFSoguAft6w8ZAJATwSYNMGJTlqmorxYYyN2BZvtGmroCuKygDAJoBkAyDAr46bUZ4kOFG7P9GmjcA",
                    PaymentSdkData = "<!--- payment sdk connector payment properties for authorization response -->",
                    CashbackAmount = 0.0m,
                    ApprovedAmount = amount,
                    IsApproved = true,
                    Errors = null
                });
            }

            /// <summary>
            /// Make settlement of a payment.
            /// </summary>
            /// <param name="amount">The amount.</param>
            /// <param name="currency">The currency.</param>
            /// <param name="paymentProperties">The payment properties of the authorization response.</param>
            /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
            /// <returns>A task that can await until the settlement has completed.</returns>
            public async Task<PaymentInfo> CapturePaymentAsync(decimal amount, string currency, PaymentProperty[] paymentProperties, ExtensionTransaction extensionTransactionProperties)
            {
                return await Task.FromResult<PaymentInfo>(new PaymentInfo()
                {
                    CardNumberMasked = string.Empty,
                    CardType = Microsoft.Dynamics.Commerce.HardwareStation.CardPayment.CardType.Unknown,
                    SignatureData = string.Empty,
                    PaymentSdkData = "<!--- payment sdk connector payment properties for capture response -->",
                    CashbackAmount = 0.0m,
                    ApprovedAmount = amount,
                    IsApproved = true,
                    Errors = null
                });
            }

            /// <summary>
            /// Make reversal/void a payment.
            /// </summary>
            /// <param name="amount">The amount.</param>
            /// <param name="currency">The currency.</param>
            /// <param name="paymentProperties">The payment properties of the authorization response.</param>
            /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
            /// <returns>A task that can await until the void has completed.</returns>
            public async Task<PaymentInfo> VoidPaymentAsync(decimal amount, string currency, PaymentProperty[] paymentProperties, ExtensionTransaction extensionTransactionProperties)
            {
                return await Task.FromResult<PaymentInfo>(new PaymentInfo()
                {
                    CardNumberMasked = string.Empty,
                    CardType = Microsoft.Dynamics.Commerce.HardwareStation.CardPayment.CardType.Unknown,
                    SignatureData = string.Empty,
                    PaymentSdkData = "<!--- payment sdk connector payment properties for void response -->",
                    CashbackAmount = 0.0m,
                    ApprovedAmount = amount,
                    IsApproved = true,
                    Errors = null
                });
            }

            /// <summary>
            /// Make refund payment.
            /// </summary>
            /// <param name="amount">The amount.</param>
            /// <param name="currency">The currency.</param>
            /// <param name="isManualEntry">If manual credit card entry is required.</param>
            /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
            /// <returns>A task that can await until the refund has completed.</returns>
            public async Task<PaymentInfo> RefundPaymentAsync(decimal amount, string currency, bool isManualEntry, ExtensionTransaction extensionTransactionProperties)
            {
                return await Task.FromResult<PaymentInfo>(new PaymentInfo()
                {
                    CardNumberMasked = "411111******1111",
                    CardType = Microsoft.Dynamics.Commerce.HardwareStation.CardPayment.CardType.InternationalCreditCard,
                    SignatureData = "AAgEAAQALP4hvpJrK/UfKvlX7ABkIfJFnxoZbaXC2vmnzYB8ItM1rBYwzRrw0IdLF3Qv89lwBfgGn5gBwKkFSoguAft6w8ZAJATwSYNMGJTlqmorxYYyN2BZvtGmroCuKygDAJoBkAyDAr46bUZ4kOFG7P9GmjcA",
                    PaymentSdkData = "<!--- payment sdk connector payment properties for refund response -->",
                    CashbackAmount = 0.0m,
                    ApprovedAmount = amount,
                    IsApproved = true,
                    Errors = null
                });
            }

            /// <summary>
            /// Fetch token for credit card.
            /// </summary>
            /// <param name="isManualEntry">The value indicating whether credit card should be entered manually.</param>
            /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
            /// <returns>A task that can await until the token generation has completed.</returns>
            public async Task<PaymentInfo> FetchTokenAsync(bool isManualEntry, ExtensionTransaction extensionTransactionProperties)
            {
                await Task.Delay(10);
                throw new System.NotImplementedException();
            }

            /// <summary>
            ///  Cancels an existing GetTender or RequestTenderApproval operation on the payment terminal.
            /// </summary>
            /// <returns>A task that can be awaited until the operation is cancelled.</returns>
            public Task CancelOperationAsync()
            {
                throw new System.NotImplementedException();
            }

            /// <summary>
            /// The dispose implementation.
            /// </summary>
            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Dispose the Native and Managed resources.
            /// </summary>
            /// <param name="disposeAll">Whether to dispose both Native and Managed resources.</param>
            protected virtual void Dispose(bool disposeAll)
            {
            }
        }
    }
}
