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
    namespace Commerce.HardwareStation.Peripherals.Mx925Device
    {
        using System;
        using System.Collections.Generic;
        using System.Composition;
        using System.Linq;
        using System.Threading.Tasks;
        using Commerce.HardwareStation.Peripherals.PaymentTerminal.EventArgs;
        using Commerce.HardwareStation.Peripherals.PaymentTerminal.Forms;
        using Commerce.HardwareStation.Peripherals.PaymentTerminal.MX925Device;
        using Microsoft.Dynamics.Commerce.HardwareStation;
        using Microsoft.Dynamics.Commerce.HardwareStation.CardPayment;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.Entities;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals.PaymentTerminal;
        using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
        using Microsoft.Dynamics.Retail.PaymentSDK.Portable.Constants;
        using Microsoft.Dynamics.Retail.SDKManager.Portable;
        using LocalPaymentTerminal = Commerce.HardwareStation.Peripherals.PaymentTerminal;

        /// <summary>
        /// <c>VeriFone</c> manager payment device class.
        /// </summary>
        [Export("MX925", typeof(IPaymentDevice))]
        public class VerifoneManagerPaymentDevice : IPaymentDevice, IDisposable
        {
            // 10-minute delay in milliseconds.
            private const int CacheCleanUpDelay = 10 * 60 * 1000;
            private const int EncryptedPinLength = 16;

            // Variable to lock multiple updates.
            private static readonly object Padlock = new object();

            // Cache to store credit card number, the key will be returned to client in Authorization payment sdk blob.
            private static Dictionary<Guid, TemporaryCardMemoryStorage<string>> cardCache = new Dictionary<Guid, TemporaryCardMemoryStorage<string>>();

            private VerifonePaymentDevice paymentDevice;
            private SettingsInfo terminalSettings;
            private PaymentProperty[] merchantProperties;
            private string paymentConnectorName;
            private IPaymentProcessor processor;
            private LocalPaymentTerminal.TrackData cardTrackData;
            private TenderInfo tenderInfo;
            private bool isTestMode;

            /// <summary>
            /// Construct the Payment Device class and open the connection from it.
            /// </summary>
            /// <param name="peripheralName">Name of peripheral device.</param>
            /// <param name="terminalSettings">The terminal settings for the peripheral device.</param>
            /// <param name="deviceConfig">Device Configuration parameters.</param>
            /// <returns>A task that can be awaited until the connection is opened.</returns>
            public Task OpenAsync(string peripheralName, SettingsInfo terminalSettings, IDictionary<string, string> deviceConfig)
            {
                this.paymentDevice = new VerifonePaymentDevice();
                this.terminalSettings = terminalSettings;

                return this.paymentDevice.OpenAsync(deviceConfig);
            }

            /// <summary>
            ///  Closes a connection to the payment terminal.
            /// </summary>
            /// <returns>A task that can be awaited until the connection is closed.</returns>
            public Task CloseAsync()
            {
                return this.paymentDevice.CloseAsync();
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
                this.merchantProperties = merchantProperties;
                this.paymentConnectorName = paymentConnectorName;
                this.isTestMode = isTestMode;

                if (!(await this.paymentDevice.BeginTransactionAsync(null)))
                {
                    throw new PeripheralException(PeripheralException.PaymentTerminalError, "Failed to begin a transaction.", inner: null);
                }

                EventHandler<CardSwipeEventArgs> cardSwipeHandler = null;
                cardSwipeHandler = (sender, args) =>
                {
                    this.cardTrackData = new LocalPaymentTerminal.TrackData { Track1 = args.Track1, Track2 = args.Track2 };
                    this.paymentDevice.CardSwipeEvent -= cardSwipeHandler;
                };

                this.paymentDevice.CardSwipeEvent += cardSwipeHandler;

                if (!(await this.paymentDevice.SetCardSwipeAsync(LocalPaymentTerminal.DeviceState.Enabled)))
                {
                    this.paymentDevice.CardSwipeEvent -= cardSwipeHandler;
                    throw new PeripheralException(PeripheralException.PaymentTerminalError, "Failed to enable card swipe.", inner: null);
                }
            }

            /// <summary>
            ///  Ends the transaction.
            /// </summary>
            /// <returns>A task that can be awaited until the end transaction screen is displayed.</returns>
            public async Task EndTransactionAsync()
            {
                if (!(await this.paymentDevice.ShowFormAsync(Form.ThankYou, null)))
                {
                    throw new PeripheralException(PeripheralException.PaymentTerminalError, string.Format("Failed to open form: {0}.", Form.ThankYou), inner: null);
                }

                await Task.Run(() => this.InternalClearCache());

                if (!(await this.paymentDevice.SetCardSwipeAsync(LocalPaymentTerminal.DeviceState.Disabled)))
                {
                    throw new PeripheralException(PeripheralException.PaymentTerminalError, "Failed to disable card swipe.", inner: null);
                }
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
                var formProperties = items.Select(x => new FormProperty { Name = Form.ItemListProperty, Value = string.Format("{0}    {1} - {2}", x.Description, x.Quantity, x.UnitPrice) }).ToList();

                formProperties.Add(new FormProperty { Name = Form.SubtotalProperty, Value = subTotalAmount });
                formProperties.Add(new FormProperty { Name = Form.DiscountProperty, Value = discountAmount });
                formProperties.Add(new FormProperty { Name = Form.TaxProperty, Value = taxAmount });
                formProperties.Add(new FormProperty { Name = Form.TotalProperty, Value = totalAmount });

                if (!(await this.paymentDevice.ShowFormAsync(Form.Total, formProperties)))
                {
                    throw new PeripheralException(PeripheralException.PaymentTerminalError, string.Format("Failed to open form: {0}.", Form.Total), inner: null);
                }
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
                if (amount < this.terminalSettings.MinimumAmountAllowed)
                {
                    throw new CardPaymentException(CardPaymentException.AmountLessThanMinimumLimit, "Amount does not meet minimum amount allowed.");
                }

                if (this.terminalSettings.MaximumAmountAllowed > 0 && amount > this.terminalSettings.MaximumAmountAllowed)
                {
                    throw new CardPaymentException(CardPaymentException.AmountExceedsMaximumLimit, "Amount exceeds the maximum amount allowed.");
                }

                if (this.processor == null)
                {
                    this.processor = CardPaymentManager.GetPaymentProcessor(this.merchantProperties, this.paymentConnectorName);
                }

                PaymentInfo paymentInfo = new PaymentInfo();

                // Get tender
                TenderInfo maskedTenderInfo = await this.GetTenderAsync(true);
                if (maskedTenderInfo == null)
                {
                    return paymentInfo;
                }

                paymentInfo.CardNumberMasked = maskedTenderInfo.CardNumber;
                paymentInfo.CashbackAmount = maskedTenderInfo.CashBackAmount;
                paymentInfo.CardType = (Microsoft.Dynamics.Commerce.HardwareStation.CardPayment.CardType)maskedTenderInfo.CardTypeId;

                if (paymentInfo.CashbackAmount > this.terminalSettings.DebitCashbackLimit)
                {
                    throw new CardPaymentException(CardPaymentException.CashbackAmountExceedsLimit, "Cashback amount exceeds the maximum amount allowed.");
                }

                // Authorize
                Response response = CardPaymentManager.ChainedAuthorizationCall(this.processor, this.merchantProperties, this.tenderInfo, amount, currency, this.terminalSettings.Locale, this.isTestMode, this.terminalSettings.TerminalId, extensionTransactionProperties);

                Guid cardStorageKey = Guid.NewGuid();
                CardPaymentManager.MapAuthorizeResponse(response, paymentInfo, cardStorageKey, this.terminalSettings.TerminalId);

                if (paymentInfo.IsApproved)
                {
                    // Backup credit card number
                    TemporaryCardMemoryStorage<string> cardStorage = new TemporaryCardMemoryStorage<string>(DateTime.UtcNow, this.tenderInfo.CardNumber);
                    cardStorage.StorageInfo = paymentInfo.PaymentSdkData;
                    cardCache.Add(cardStorageKey, cardStorage);

                    // need signature?
                    if (this.terminalSettings.SignatureCaptureMinimumAmount < paymentInfo.ApprovedAmount)
                    {
                        paymentInfo.SignatureData = await this.RequestTenderApprovalAsync(paymentInfo.ApprovedAmount);
                    }
                }

                return paymentInfo;
            }

            /// <summary>
            /// Make settlement of a payment.
            /// </summary>
            /// <param name="amount">The amount.</param>
            /// <param name="currency">The currency.</param>
            /// <param name="paymentProperties">The payment properties of the authorization response.</param>
            /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
            /// <returns>A task that can await until the settlement has completed.</returns>
            public Task<PaymentInfo> CapturePaymentAsync(decimal amount, string currency, PaymentProperty[] paymentProperties, ExtensionTransaction extensionTransactionProperties)
            {
                if (amount < this.terminalSettings.MinimumAmountAllowed)
                {
                    throw new CardPaymentException(CardPaymentException.AmountLessThanMinimumLimit, "Amount does not meet minimum amount allowed.");
                }

                if (this.terminalSettings.MaximumAmountAllowed > 0 && amount > this.terminalSettings.MaximumAmountAllowed)
                {
                    throw new CardPaymentException(CardPaymentException.AmountExceedsMaximumLimit, "Amount exceeds the maximum amount allowed.");
                }

                if (this.processor == null)
                {
                    this.processor = CardPaymentManager.GetPaymentProcessor(this.merchantProperties, this.paymentConnectorName);
                }

                PaymentInfo paymentInfo = new PaymentInfo();

                // Handle multiple chain connectors by returning single instance used in capture.
                IPaymentProcessor currentProcessor = null;
                PaymentProperty[] currentMerchantProperties = null;
                CardPaymentManager.GetRequiredConnector(this.merchantProperties, paymentProperties, this.processor, out currentProcessor, out currentMerchantProperties);

                Request request = CardPaymentManager.GetCaptureRequest(currentMerchantProperties, paymentProperties, amount, currency, this.terminalSettings.Locale, this.isTestMode, this.terminalSettings.TerminalId, cardCache, extensionTransactionProperties);
                Response response = currentProcessor.Capture(request);
                CardPaymentManager.MapCaptureResponse(response, paymentInfo);

                return Task.FromResult(paymentInfo);
            }

            /// <summary>
            /// Make reversal/void a payment.
            /// </summary>
            /// <param name="amount">The amount.</param>
            /// <param name="currency">The currency.</param>
            /// <param name="paymentProperties">The payment properties of the authorization response.</param>
            /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
            /// <returns>A task that can await until the void has completed.</returns>
            public Task<PaymentInfo> VoidPaymentAsync(decimal amount, string currency, PaymentProperty[] paymentProperties, ExtensionTransaction extensionTransactionProperties)
            {
                if (amount < this.terminalSettings.MinimumAmountAllowed)
                {
                    throw new CardPaymentException(CardPaymentException.AmountLessThanMinimumLimit, "Amount does not meet minimum amount allowed.");
                }

                if (this.terminalSettings.MaximumAmountAllowed > 0 && amount > this.terminalSettings.MaximumAmountAllowed)
                {
                    throw new CardPaymentException(CardPaymentException.AmountExceedsMaximumLimit, "Amount exceeds the maximum amount allowed.");
                }

                if (this.processor == null)
                {
                    this.processor = CardPaymentManager.GetPaymentProcessor(this.merchantProperties, this.paymentConnectorName);
                }

                PaymentInfo paymentInfo = new PaymentInfo();

                // Handle multiple chain connectors by returning single instance used in capture.
                IPaymentProcessor currentProcessor = null;
                PaymentProperty[] currentMerchantProperties = null;
                CardPaymentManager.GetRequiredConnector(this.merchantProperties, paymentProperties, this.processor, out currentProcessor, out currentMerchantProperties);

                Request request = CardPaymentManager.GetCaptureRequest(currentMerchantProperties, paymentProperties, amount, currency, this.terminalSettings.Locale, this.isTestMode, this.terminalSettings.TerminalId, cardCache, extensionTransactionProperties);
                Response response = currentProcessor.Void(request);
                CardPaymentManager.MapVoidResponse(response, paymentInfo);

                return Task.FromResult(paymentInfo);
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
                if (amount < this.terminalSettings.MinimumAmountAllowed)
                {
                    throw new CardPaymentException(CardPaymentException.AmountLessThanMinimumLimit, "Amount does not meet minimum amount allowed.");
                }

                if (this.terminalSettings.MaximumAmountAllowed > 0 && amount > this.terminalSettings.MaximumAmountAllowed)
                {
                    throw new CardPaymentException(CardPaymentException.AmountExceedsMaximumLimit, "Amount exceeds the maximum amount allowed.");
                }

                if (this.processor == null)
                {
                    this.processor = CardPaymentManager.GetPaymentProcessor(this.merchantProperties, this.paymentConnectorName);
                }

                PaymentInfo paymentInfo = new PaymentInfo();

                // Get tender
                TenderInfo maskedTenderInfo = await this.GetTenderAsync(false);
                if (maskedTenderInfo == null)
                {
                    return paymentInfo;
                }

                paymentInfo.CardNumberMasked = maskedTenderInfo.CardNumber;
                paymentInfo.CashbackAmount = maskedTenderInfo.CashBackAmount;
                paymentInfo.CardType = (Microsoft.Dynamics.Commerce.HardwareStation.CardPayment.CardType)maskedTenderInfo.CardTypeId;

                if (paymentInfo.CashbackAmount > this.terminalSettings.DebitCashbackLimit)
                {
                    throw new CardPaymentException(CardPaymentException.CashbackAmountExceedsLimit, "Cashback amount exceeds the maximum amount allowed.");
                }

                // Refund
                Response response = CardPaymentManager.ChainedRefundCall(this.processor, this.merchantProperties, this.tenderInfo, amount, currency, this.terminalSettings.Locale, this.isTestMode, this.terminalSettings.TerminalId, extensionTransactionProperties);

                CardPaymentManager.MapRefundResponse(response, paymentInfo);

                if (paymentInfo.IsApproved)
                {
                    // need signature?
                    if (this.terminalSettings.SignatureCaptureMinimumAmount < paymentInfo.ApprovedAmount)
                    {
                        paymentInfo.SignatureData = await this.RequestTenderApprovalAsync(paymentInfo.ApprovedAmount);
                    }
                }

                return paymentInfo;
            }

            /// <summary>
            /// Fetch token for credit card.
            /// </summary>
            /// <param name="isManualEntry">The value indicating whether credit card should be entered manually.</param>
            /// <param name="extensionTransactionProperties">Optional extension transaction properties.</param>
            /// <returns>A task that can await until the token generation has completed.</returns>
            public async Task<PaymentInfo> FetchTokenAsync(bool isManualEntry, ExtensionTransaction extensionTransactionProperties)
            {
                PaymentInfo paymentInfo = new PaymentInfo();

                // Get tender
                TenderInfo maskedTenderInfo = await this.GetTenderAsync(false);
                if (maskedTenderInfo == null)
                {
                    return paymentInfo;
                }

                if (this.processor == null)
                {
                    this.processor = CardPaymentManager.GetPaymentProcessor(this.merchantProperties, this.paymentConnectorName);
                }

                paymentInfo.CardNumberMasked = maskedTenderInfo.CardNumber;
                paymentInfo.CashbackAmount = maskedTenderInfo.CashBackAmount;
                paymentInfo.CardType = (Microsoft.Dynamics.Commerce.HardwareStation.CardPayment.CardType)maskedTenderInfo.CardTypeId;

                PaymentProperty[] defaultMerchantProperties = this.merchantProperties;

                if (this.merchantProperties[0].Namespace.Equals(GenericNamespace.Connector) && this.merchantProperties[0].Name.Equals(ConnectorProperties.Properties))
                {
                    defaultMerchantProperties = this.merchantProperties[0].PropertyList;
                }

                // Generate card token
                Request request = CardPaymentManager.GetTokenRequest(defaultMerchantProperties, this.tenderInfo, this.terminalSettings.Locale, extensionTransactionProperties);
                Response response = this.processor.GenerateCardToken(request, null);
                CardPaymentManager.MapTokenResponse(response, paymentInfo);

                return paymentInfo;
            }

            /// <summary>
            ///  Cancels an existing GetTender or RequestTenderApproval operation on the payment terminal.
            /// </summary>
            /// <returns>A task that can be awaited until the operation is cancelled.</returns>
            public Task CancelOperationAsync()
            {
                return this.paymentDevice.CancelOperation();
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
            /// We don't have the payment provider authorization piece for this function, here we just
            /// assume we get the bank authorization.
            /// </summary>
            /// <param name="amount">Required payment amount.</param>
            /// <returns>TenderInfo object.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "amount", Justification = "Other devices support the amount for signature approval.")]
            public async Task<string> RequestTenderApprovalAsync(decimal amount)
            {
                string signature = await this.GetSignatureData();

                var formProperties = new List<FormProperty> { new FormProperty { Name = Form.ProcessingTextProperty, Value = "Authorizing..." } };

                if (!(await this.paymentDevice.ShowFormAsync(Form.Processing, formProperties)))
                {
                    throw new PeripheralException(PeripheralException.PaymentTerminalError, string.Format("Failed to open form: {0}.", Form.Processing), inner: null);
                }

                return signature;
            }

            /// <summary>
            /// Dispose the Native and Managed resources.
            /// </summary>
            /// <param name="disposeAll">Whether to dispose both Native and Managed resources.</param>
            protected virtual void Dispose(bool disposeAll)
            {
                if (this.paymentDevice != null)
                {
                    this.paymentDevice.Dispose();
                    this.paymentDevice = null;
                }
            }

            /// <summary>
            /// Get the signature data from the device.
            /// </summary>
            /// <returns>Empty Task.</returns>
            private async Task<string> GetSignatureData()
            {
                var getSignature = new TaskCompletionSource<string>();
                EventHandler<SignatureEventArgs> signatureHandler = null;

                signatureHandler = (sender, args) =>
                {
                    if (args.IsCanceled)
                    {
                        getSignature.TrySetCanceled();
                    }
                    else
                    {
                        getSignature.SetResult(args.SignatureData);
                    }

                    this.paymentDevice.SignatureEvent -= signatureHandler;
                };

                this.paymentDevice.SignatureEvent += signatureHandler;

                if (!(await this.paymentDevice.ShowFormAsync(Form.Signature, null)))
                {
                    this.paymentDevice.SignatureEvent -= signatureHandler;
                    throw new PeripheralException(PeripheralException.PaymentTerminalError, string.Format("Failed to open form: {0}.", Form.Signature), inner: null);
                }

                if (!(await this.paymentDevice.SetSignatureCaptureAsync(LocalPaymentTerminal.DeviceState.Enabled)))
                {
                    this.paymentDevice.SignatureEvent -= signatureHandler;
                    throw new PeripheralException(PeripheralException.PaymentTerminalError, "Failed to enable signature capture.", inner: null);
                }

                string signature = await getSignature.Task;

                return signature;
            }

            /// <summary>
            /// This function would aggregate several interface functions in paymentDevice and put those into TenderInfo.
            /// </summary>
            /// <param name="allowCashback">True if allow cash back.</param>
            /// <returns>TenderInfo object.</returns>
            private async Task<TenderInfo> GetTenderAsync(bool allowCashback)
            {
                return await this.FillTenderInfo(true, allowCashback);
            }

            /// <summary>
            /// Fills this.TenderInfo.
            /// </summary>
            /// <param name="maskCardNumber">True if a card number should be masked.</param>
            /// <param name="allowCashback">True if allow cash back.</param>
            /// <returns>The tender info.</returns>
            private async Task<TenderInfo> FillTenderInfo(bool maskCardNumber, bool allowCashback)
            {
                const string EndTrack1Sentinel = "^";
                const string EndTrack2Sentinel = "=";
                int startTrack1CreditCardNumber = 1;
                int endTrack1CreditCardNumber = 0;
                int startTrack2CreditCardNumber = 0;
                int endTrack2CreditCardNumber = 0;
                int expirationLength = 4;

                this.tenderInfo = new TenderInfo();
                this.tenderInfo.CardTypeId = (int)Microsoft.Dynamics.Commerce.HardwareStation.CardPayment.CardType.InternationalCreditCard;
                this.tenderInfo.TenderId = await this.GetButtonPress(Form.CardSelection);
                if (string.IsNullOrEmpty(this.tenderInfo.TenderId))
                {
                    // User aborted
                    return null;
                }

                var cardTrack = await this.GetCardSwipeData();
                if (string.IsNullOrEmpty(cardTrack.Track1) && string.IsNullOrEmpty(cardTrack.Track2))
                {
                    // User aborted
                    return null;
                }

                this.tenderInfo.Track1 = cardTrack.Track1;
                this.tenderInfo.Track2 = cardTrack.Track2;
                this.tenderInfo.Track3 = cardTrack.Track3;

                // we need to find the sentinel to find the card number and expiration month and year.
                if (!string.IsNullOrEmpty(this.tenderInfo.Track1))
                {
                    this.tenderInfo.IsSwipe = true;
                    endTrack1CreditCardNumber = cardTrack.Track1.IndexOf(EndTrack1Sentinel, startTrack1CreditCardNumber);
                    if (endTrack1CreditCardNumber > 0)
                    {
                        this.tenderInfo.CardNumber = cardTrack.Track1.Substring(startTrack1CreditCardNumber, endTrack1CreditCardNumber - startTrack1CreditCardNumber);
                    }

                    int startExpiration = cardTrack.Track1.IndexOf(EndTrack1Sentinel, endTrack1CreditCardNumber + 1) + 1;
                    if (startExpiration > 0 && startExpiration + expirationLength <= cardTrack.Track1.Length)
                    {
                        string expiration = cardTrack.Track1.Substring(startExpiration, expirationLength);
                        this.SetExpirationMonthYear(expiration, this.tenderInfo);
                    }
                }

                if (!string.IsNullOrEmpty(this.tenderInfo.Track2))
                {
                    this.tenderInfo.IsSwipe = true;
                    endTrack2CreditCardNumber = cardTrack.Track2.IndexOf(EndTrack2Sentinel, startTrack2CreditCardNumber);
                    if (endTrack2CreditCardNumber > 0)
                    {
                        this.tenderInfo.CardNumber = cardTrack.Track2.Substring(startTrack2CreditCardNumber, endTrack2CreditCardNumber - startTrack2CreditCardNumber);
                    }

                    int startExpiration = endTrack2CreditCardNumber + 1;
                    if (startExpiration > 0 && startExpiration + expirationLength <= cardTrack.Track2.Length)
                    {
                        string expiration = cardTrack.Track2.Substring(startExpiration, expirationLength);
                        this.SetExpirationMonthYear(expiration, this.tenderInfo);
                    }
                }

                if (string.IsNullOrEmpty(this.tenderInfo.Track1) && string.IsNullOrEmpty(this.tenderInfo.Track2))
                {
                    this.tenderInfo.CardNumber = string.Empty;
                }

                // If it is a debit card, get the encrypted Pin from it
                if (this.tenderInfo.TenderId == Form.DebitButton)
                {
                    this.tenderInfo.CardTypeId = (int)Microsoft.Dynamics.Commerce.HardwareStation.CardPayment.CardType.InternationalDebitCard;

                    if (allowCashback)
                    {
                        // Get cashback
                        string cashbackAmount = await this.GetButtonPress(Form.Cashback);
                        if (string.IsNullOrEmpty(cashbackAmount))
                        {
                            // User aborted
                            return null;
                        }

                        decimal amountCashBack;
                        if (decimal.TryParse(cashbackAmount, out amountCashBack))
                        {
                            this.tenderInfo.CashBackAmount = amountCashBack;
                        }
                    }

                    this.tenderInfo.EncryptedPin = await this.GetDebitCardEncryptedPin(this.tenderInfo.CardNumber);
                    if (string.IsNullOrEmpty(this.tenderInfo.EncryptedPin))
                    {
                        // User aborted
                        return null;
                    }

                    if (!string.IsNullOrEmpty(this.tenderInfo.EncryptedPin) && this.tenderInfo.EncryptedPin.Length > EncryptedPinLength)
                    {
                        if (string.IsNullOrEmpty(this.tenderInfo.AdditionalSecurityData))
                        {
                            this.tenderInfo.AdditionalSecurityData = this.tenderInfo.EncryptedPin.Substring(EncryptedPinLength);
                        }

                        this.tenderInfo.EncryptedPin = this.tenderInfo.EncryptedPin.Substring(0, EncryptedPinLength);
                    }
                }

                // After all above, display the payware connection page
                var formProperties = new List<FormProperty> { new FormProperty { Name = Form.ProcessingTextProperty, Value = "Authorizing..." } };

                if (!(await this.paymentDevice.ShowFormAsync(Form.Processing, formProperties)))
                {
                    throw new PeripheralException(PeripheralException.PaymentTerminalError, string.Format("Failed to open form: {0}.", Form.Processing), inner: null);
                }

                // Only return tender masked
                TenderInfo returnTenderInfo = new TenderInfo();
                returnTenderInfo.CardTypeId = this.tenderInfo.CardTypeId;
                returnTenderInfo.TenderId = this.tenderInfo.TenderId;
                returnTenderInfo.CashBackAmount = this.tenderInfo.CashBackAmount;
                returnTenderInfo.EncryptedPin = this.tenderInfo.EncryptedPin;
                returnTenderInfo.AdditionalSecurityData = this.tenderInfo.AdditionalSecurityData;
                returnTenderInfo.IsSwipe = this.tenderInfo.IsSwipe;

                // Keep first 6 and last 4 characters in the card number, mask the rest.
                if (maskCardNumber)
                {
                    returnTenderInfo.CardNumber = Utilities.GetMaskedCardNumber(this.tenderInfo.CardNumber);

                    if (!string.IsNullOrEmpty(this.tenderInfo.Track1))
                    {
                        returnTenderInfo.Track1 = this.tenderInfo.Track1.Substring(0, startTrack1CreditCardNumber) + returnTenderInfo.CardNumber + this.tenderInfo.Track1.Substring(endTrack1CreditCardNumber);
                    }

                    if (!string.IsNullOrEmpty(this.tenderInfo.Track2))
                    {
                        returnTenderInfo.Track2 = this.tenderInfo.Track2.Substring(0, startTrack2CreditCardNumber) + returnTenderInfo.CardNumber + this.tenderInfo.Track2.Substring(endTrack2CreditCardNumber);
                    }
                }
                else
                {
                    returnTenderInfo = this.tenderInfo;
                }

                return returnTenderInfo;
            }

            /// <summary>
            /// Enable the card selection page and get the button press result.
            /// </summary>
            /// <param name="formName">Name of form to show.</param>
            /// <returns>A task that can be awaited until a button is pressed.</returns>
            private async Task<string> GetButtonPress(string formName)
            {
                var getButtonPress = new TaskCompletionSource<string>();
                EventHandler<ButtonPressEventArgs> tenderHandler = null;

                tenderHandler = (sender, args) =>
                {
                    if (args.IsCanceled)
                    {
                        getButtonPress.TrySetCanceled();
                    }
                    else
                    {
                        getButtonPress.SetResult(args.ButtonName);
                    }

                    this.paymentDevice.ButtonPressEvent -= tenderHandler;
                };

                this.paymentDevice.ButtonPressEvent += tenderHandler;

                // Some device don't support the required form, if false is returned skip the waiting for button to be pressed.
                if (!(await this.paymentDevice.ShowFormAsync(formName, null)))
                {
                    this.paymentDevice.ButtonPressEvent -= tenderHandler;
                    if (formName.Equals(Form.Cashback))
                    {
                        return "0";
                    }
                    else
                    {
                        throw new PeripheralException(PeripheralException.PaymentTerminalError, string.Format("Failed to open form: {0}.", formName), inner: null);
                    }
                }

                return await getButtonPress.Task;
            }

            /// <summary>
            /// Get the debit card encrypted pin.
            /// </summary>
            /// <param name="cardNumber">The card Number.</param>
            /// <returns>A task that can be awaited until the pin is received.</returns>
            private async Task<string> GetDebitCardEncryptedPin(string cardNumber)
            {
                var getPinData = new TaskCompletionSource<string>();
                EventHandler<PinDataEventArgs> tenderHandler = null;

                tenderHandler = (sender, args) =>
                {
                    if (args.IsCanceled)
                    {
                        getPinData.TrySetCanceled();
                    }
                    else
                    {
                        getPinData.SetResult(args.PinData);
                    }

                    this.paymentDevice.PinDataEvent -= tenderHandler;
                };

                this.paymentDevice.PinDataEvent += tenderHandler;

                if (!(await this.paymentDevice.SetPinPadAsync(LocalPaymentTerminal.DeviceState.Enabled, cardNumber)))
                {
                    this.paymentDevice.PinDataEvent -= tenderHandler;
                    throw new PeripheralException(PeripheralException.PaymentTerminalError, "Failed to enable PIN pad.", inner: null);
                }
                
                string pin = await getPinData.Task;
                this.paymentDevice.PinDataEvent -= tenderHandler;

                return pin;
            }

            /// <summary>
            /// Enable the card swipe page and get the card tracker data into TenderInfo.
            /// </summary>
            /// <returns>A task that can be awaited until the card data is received.</returns>
            private async Task<LocalPaymentTerminal.TrackData> GetCardSwipeData()
            {
                if (!string.IsNullOrEmpty(this.cardTrackData.Track1))
                {
                    var creditCard = this.cardTrackData;
                    this.cardTrackData.Track1 = null;
                    this.cardTrackData.Track2 = null;
                    this.cardTrackData.Track3 = null;
                    return creditCard;
                }

                var getCardTrackData = new TaskCompletionSource<LocalPaymentTerminal.TrackData>();
                EventHandler<CardSwipeEventArgs> tenderHandler = null;

                tenderHandler = (sender, args) =>
                {
                    if (args.IsCanceled)
                    {
                        getCardTrackData.TrySetCanceled();
                    }
                    else
                    {
                        getCardTrackData.SetResult(new LocalPaymentTerminal.TrackData { Track1 = args.Track1, Track2 = args.Track2, Track3 = args.Track3 });
                    }

                    this.paymentDevice.CardSwipeEvent -= tenderHandler;
                };

                this.paymentDevice.CardSwipeEvent += tenderHandler;

                if (!(await this.paymentDevice.ShowFormAsync(Form.CardSwipe, null)))
                {
                    this.paymentDevice.CardSwipeEvent -= tenderHandler;
                    throw new PeripheralException(PeripheralException.PaymentTerminalError, string.Format("Failed to open form: {0}.", Form.CardSwipe), inner: null);
                }

                // NOTE: This is not required for the L5300 HydraPaymentDevice as the Form controls swipe enable
                if (!(await this.paymentDevice.SetCardSwipeAsync(LocalPaymentTerminal.DeviceState.Enabled)))
                {
                    this.paymentDevice.CardSwipeEvent -= tenderHandler;
                    throw new PeripheralException(PeripheralException.PaymentTerminalError, "Failed to enable card swipe.", inner: null);
                }

                var cardSwipe = await getCardTrackData.Task;

                this.cardTrackData.Track1 = null;
                this.cardTrackData.Track2 = null;
                this.cardTrackData.Track3 = null;
                return cardSwipe;
            }

            /// <summary>
            /// Clear the cache.
            /// </summary>
            private void InternalClearCache()
            {
                lock (Padlock)
                {
                    if (cardCache.Count > 0)
                    {
                        foreach (var item in cardCache.Values)
                        {
                            item.ClearCardNumber();
                        }
                    }

                    cardCache.Clear();
                }
            }

            private void SetExpirationMonthYear(string expiration, TenderInfo tenderInfo)
            {
                if (tenderInfo != null && !string.IsNullOrEmpty(expiration) && expiration.Length == 4)
                {
                    string year = expiration.Substring(0, 2);
                    int yearValue;
                    if (int.TryParse(year, out yearValue))
                    {
                        tenderInfo.ExpirationYear = yearValue + 2000;
                    }

                    string month = expiration.Substring(2, 2);
                    int monthValue;
                    if (int.TryParse(month, out monthValue))
                    {
                        tenderInfo.ExpirationMonth = monthValue;
                    }
                }
            }
        }
    }
}
