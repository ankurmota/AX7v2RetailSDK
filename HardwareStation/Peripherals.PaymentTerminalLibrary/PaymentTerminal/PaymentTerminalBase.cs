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
    namespace Commerce.HardwareStation.Peripherals.PaymentTerminal
    {
        using System;
        using System.Collections.Generic;
        using System.Threading;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.HardwareStation;
        using Microsoft.Dynamics.Commerce.HardwareStation.CardPayment;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.PaymentTerminal;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Retail.PaymentSDK.Portable;

        /// <summary>
        ///  Payment terminal base class which handles the payment workflow.
        /// </summary>
        public class PaymentTerminalBase : IPaymentTerminal, IDisposable
        {
            private IPaymentDevice paymentDevice;
            private SemaphoreSlim executionLock = new SemaphoreSlim(1, 1);

            /// <summary>
            /// Construct the Payment Device class and open the connection from it.
            /// </summary>
            /// <param name="peripheralName">Name of peripheral device.</param>
            /// <param name="terminalSettings">The terminal settings for the peripheral device.</param>
            /// <param name="peripheralConfig">Configuration parameters of the peripheral.</param>
            /// <returns>A task that can be awaited until the connection is opened.</returns>
            public virtual async Task OpenAsync(string peripheralName, SettingsInfo terminalSettings, PeripheralConfiguration peripheralConfig)
            {
                this.executionLock = new SemaphoreSlim(1, 1);
                this.paymentDevice = CompositionManager.Instance.GetComponent<IPaymentDevice>(peripheralName);

                if (this.paymentDevice == null)
                {
                    throw new PeripheralException(PeripheralException.PaymentTerminalError, "Payment terminal '{0}' is not available.\n\nPossible issues:\n1. Payment terminal device assembly not installed in DLLHost or IIS Hardware Station bin directory.\n2. Payment terminal device assembly not in hardwareStation->composition of the configuration file in above directory.", peripheralName);
                }

                IDictionary<string, string> peripheralConfigDictionary = new Dictionary<string, string>();
                if (peripheralConfig != null && peripheralConfig.ExtensionProperties != null)
                {
                    peripheralConfigDictionary = peripheralConfig.ExtensionProperties.ToStringDictionary();
                }
                
                await this.paymentDevice.OpenAsync(peripheralName, terminalSettings, peripheralConfigDictionary);
            }

            /// <summary>
            ///  Closes a connection to the payment terminal.
            /// </summary>
            /// <returns>A task that can be awaited until the connection is closed.</returns>
            public virtual async Task CloseAsync()
            {
                try
                {
                    await this.paymentDevice.CloseAsync();
                }
                finally
                {
                    this.paymentDevice = null;
                    this.executionLock.Dispose();
                }
            }

            /// <summary>
            ///  Begins the transaction.
            /// </summary>
            /// <param name="paymentConnectorName">The payment connector name for the peripheral device.</param>
            /// <param name="merchantPaymentPropertiesXml">The merchant provider payment properties for the peripheral device.</param>
            /// <param name="invoiceNumber">The invoice number associated with the transaction (6 characters long).</param>
            /// <param name="isTestMode">Is test mode for payments enabled for the peripheral device.</param>
            /// <returns>A task that can be awaited until the begin transaction screen is displayed.</returns>
            public virtual async Task BeginTransactionAsync(string paymentConnectorName, string merchantPaymentPropertiesXml, string invoiceNumber, bool isTestMode)
            {
                await this.Execute(async () =>
                {
                    // Get the payment connector properties
                    PaymentProperty[] merchantProperties = CardPaymentManager.ToLocalProperties(merchantPaymentPropertiesXml);
                    await this.paymentDevice.BeginTransactionAsync(merchantProperties, paymentConnectorName, invoiceNumber, isTestMode);
                });
            }

            /// <summary>
            ///  Ends the transaction.
            /// </summary>
            /// <returns>A task that can be awaited until the end transaction screen is displayed.</returns>
            public virtual async Task EndTransactionAsync()
            {
                await this.paymentDevice.EndTransactionAsync();
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
            public virtual async Task UpdateLineItemsAsync(string totalAmount, string taxAmount, string discountAmount, string subTotalAmount, IEnumerable<ItemInfo> items)
            {
                await this.Execute(async () =>
                {
                    await this.paymentDevice.UpdateLineItemsAsync(totalAmount, taxAmount, discountAmount, subTotalAmount, items);
                });
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
            public virtual async Task<PaymentInfo> AuthorizePaymentAsync(decimal amount, string currency, string voiceAuthorization, bool isManualEntry, ExtensionTransaction extensionTransactionProperties)
            {
                return await this.Execute(async () =>
                {
                    return await this.paymentDevice.AuthorizePaymentAsync(amount, currency, voiceAuthorization, isManualEntry, extensionTransactionProperties);
                });
            }

            /// <summary>
            /// Make settlement of a payment.
            /// </summary>
            /// <param name="amount">The amount.</param>
            /// <param name="currency">The currency.</param>
            /// <param name="paymentPropertiesXml">The payment properties of the authorization response.</param>
            /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
            /// <returns>A task that can await until the settlement has completed.</returns>
            public virtual async Task<PaymentInfo> CapturePaymentAsync(decimal amount, string currency, string paymentPropertiesXml, ExtensionTransaction extensionTransactionProperties)
            {
                PaymentProperty[] properties = CardPaymentManager.ToLocalProperties(paymentPropertiesXml);

                try
                {
                    return await this.paymentDevice.CapturePaymentAsync(amount, currency, properties, extensionTransactionProperties);
                }
                catch (PaymentException paymentException)
                {
                    // When payment is already captured, treat it as success.
                    if (paymentException.PaymentSdkErrors != null
                        && paymentException.PaymentSdkErrors.Count == 1
                        && paymentException.PaymentSdkErrors[0].Code == ErrorCode.MultipleCaptureNotSupported)
                    {
                        PaymentInfo paymentInfo = new PaymentInfo();
                        paymentInfo.IsApproved = true;

                        return await Task.FromResult<PaymentInfo>(paymentInfo);
                    }
                    else
                    {
                        throw paymentException;
                    }
                }
            }

            /// <summary>
            /// Make reversal/void a payment.
            /// </summary>
            /// <param name="amount">The amount.</param>
            /// <param name="currency">The currency.</param>
            /// <param name="paymentPropertiesXml">The payment properties of the authorization response.</param>
            /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
            /// <returns>A task that can await until the void has completed.</returns>
            public virtual async Task<PaymentInfo> VoidPaymentAsync(decimal amount, string currency, string paymentPropertiesXml, ExtensionTransaction extensionTransactionProperties)
            {
                PaymentProperty[] properties = CardPaymentManager.ToLocalProperties(paymentPropertiesXml);

                try
                {
                    return await this.paymentDevice.VoidPaymentAsync(amount, currency, properties, extensionTransactionProperties);
                }
                catch (PaymentException paymentException)
                {
                    // When payment is already voided, treat it as success.
                    if (paymentException.PaymentSdkErrors != null
                        && paymentException.PaymentSdkErrors.Count == 1
                        && paymentException.PaymentSdkErrors[0].Code == ErrorCode.AuthorizationIsVoided)
                    {
                        PaymentInfo paymentInfo = new PaymentInfo();
                        paymentInfo.IsApproved = true;

                        return await Task.FromResult<PaymentInfo>(paymentInfo);
                    }
                    else
                    {
                        throw paymentException;
                    }
                }
            }

            /// <summary>
            /// Make refund payment.
            /// </summary>
            /// <param name="amount">The amount.</param>
            /// <param name="currency">The currency.</param>
            /// <param name="isManualEntry">If manual credit card entry is required.</param>
            /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
            /// <returns>A task that can await until the refund has completed.</returns>
            public virtual async Task<PaymentInfo> RefundPaymentAsync(decimal amount, string currency, bool isManualEntry, ExtensionTransaction extensionTransactionProperties)
            {
                return await this.Execute(async () =>
                {
                    return await this.paymentDevice.RefundPaymentAsync(amount, currency, isManualEntry, extensionTransactionProperties);
                });
            }

            /// <summary>
            /// Fetch token for credit card.
            /// </summary>
            /// <param name="isManualEntry">The value indicating whether credit card should be entered manually.</param>
            /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
            /// <returns>A task that can await until the token generation has completed.</returns>
            public virtual async Task<PaymentInfo> FetchTokenAsync(bool isManualEntry, ExtensionTransaction extensionTransactionProperties)
            {
                return await this.Execute(async () =>
                {
                    return await this.paymentDevice.FetchTokenAsync(isManualEntry, extensionTransactionProperties);
                });
            }

            /// <summary>
            ///  Cancels an existing GetTender or RequestTenderApproval operation on the payment terminal.
            /// </summary>
            /// <returns>A task that can be awaited until the operation is cancelled.</returns>
            public virtual async Task CancelOperationAsync()
            {
                await this.paymentDevice.CancelOperationAsync();
            }

            /// <summary>
            /// Extensibility execute method.
            /// </summary>
            /// <param name="task">The task to execute.</param>
            /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
            /// <returns>The result of executing the task.</returns>
            public virtual async Task<ExtensionTransaction> ExecuteTaskAsync(string task, ExtensionTransaction extensionTransactionProperties)
            {
                IPaymentTerminalExtension extension = this.paymentDevice as IPaymentTerminalExtension;

                if (extension != null)
                {
                    return await extension.ExecuteTaskAsync(task, extensionTransactionProperties);
                }
                else
                {
                    return await Task.FromResult<ExtensionTransaction>(null);
                }
            }

            /// <summary>
            /// Opens a peripheral.
            /// </summary>
            /// <param name="peripheralName">Name of the peripheral.</param>
            /// <param name="peripheralConfig">Configuration parameters of the peripheral.</param>
            public virtual void Open(string peripheralName, PeripheralConfiguration peripheralConfig)
            {
            }

            /// <summary>
            /// Closes the peripheral.
            /// </summary>
            public virtual void Close()
            {
                Task.Run(async () =>
                {
                    await this.CancelOperationAsync();
                    await this.EndTransactionAsync();
                    await this.CloseAsync();
                }).Wait();

                if (this.executionLock != null)
                {
                    this.executionLock.Dispose();
                    this.executionLock = null;
                }
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
                if (this.executionLock != null)
                {
                    this.executionLock.Dispose();
                    this.executionLock = null;
                }
            }

            /// <summary>
            ///  Executes a task, limiting the number of tasks that can execute at the same time.
            /// </summary>
            /// <param name="task">Task to run.</param>
            /// <returns>Task that will handle concurrency.</returns>
            private async Task Execute(Func<Task> task)
            {
                await this.executionLock.WaitAsync();

                try
                {
                    await task();
                }
                finally
                {
                    this.executionLock.Release();
                }
            }

            /// <summary>
            ///  Executes a task, limiting the number of tasks that can execute at the same time.
            /// </summary>
            /// <typeparam name="T">Task return type.</typeparam>
            /// <param name="task">Task to run.</param>
            /// <returns>Task that will handle concurrency.</returns>
            private async Task<T> Execute<T>(Func<Task<T>> task)
            {
                await this.executionLock.WaitAsync();

                try
                {
                    return await task();
                }
                catch (TaskCanceledException)
                {
                    return default(T);
                }
                catch (PaymentException)
                {
                    throw;
                }
                finally
                {
                    this.executionLock.Release();
                }
            }
        }
    }
}
