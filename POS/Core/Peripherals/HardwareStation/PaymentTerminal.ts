/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../IAsyncResult.ts'/>
///<reference path='../IPaymentTerminal.ts'/>
///<reference path='HardwareStationContext.ts'/>

module Commerce.Peripherals.HardwareStation {
    "use strict";

    export class PaymentTerminal implements IPaymentTerminalFull {

        /**
         * Lock token.
         */
        public lockToken: string;

        /**
         * Check if the device is available.
         */
        public isActive: boolean;

        /**
         * Begins a transaction on the payment terminal device.
         *
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        public beginTransaction(callerContext?: any): IVoidAsyncResult {
            var self = this;
            this.lockToken = null;
            var debitCashbackLimit: number;

            if (ApplicationContext.Instance.hardwareProfile.PinPadDeviceTypeValue === Model.Entities.PeripheralDeviceType.Windows
                || ApplicationContext.Instance.hardwareProfile.PinPadDeviceTypeValue === Model.Entities.PeripheralDeviceType.Network) {
                // Device is marked active if configured.
                self.isActive = true;

                return new AsyncQueue()
                    .enqueue(() => {
                        return ApplicationContext.Instance.debitCashbackLimitAsync.done((limit: number) => {
                            debitCashbackLimit = limit;
                        });
                    })
                    .enqueue(() => {
                        var cardTenderType: Model.Entities.TenderType = ApplicationContext.Instance.tenderTypesMap.getTenderTypeByOperationId(Commerce.Operations.RetailOperation.PayCard);
                        var cartId: string = Commerce.Session.instance.cart.Id;
                        var positionOfCartId: number = cartId.lastIndexOf("-") + 1;
                        cartId = cartId.substr(positionOfCartId);

                        var terminalSettings: SettingsInfo = {
                            SignatureCaptureMinimumAmount: cardTenderType.MinimumSignatureCaptureAmount,
                            MinimumAmountAllowed: cardTenderType.MinimumAmountPerLine,
                            MaximumAmountAllowed: cardTenderType.MaximumAmountPerLine,
                            DebitCashbackLimit: debitCashbackLimit,
                            Locale: ApplicationContext.Instance.deviceConfiguration.CultureName,
                            TerminalId: ApplicationContext.Instance.activeEftTerminalId
                        };

                        var paymentTerminalLockRequest: PaymentTerminalLockRequest = {
                            DeviceName: ApplicationContext.Instance.hardwareProfile.PinPadDeviceName,
                            DeviceType: Model.Entities.PeripheralDeviceType[ApplicationContext.Instance.hardwareProfile.PinPadDeviceTypeValue],
                            Timeout: HardwareStationContext.HS_DEFAULT_PAYMENT_TIMEOUT,
                            PaymentConnectorName: ApplicationContext.Instance.hardwareProfile.EftPaymentConnectorName,
                            InvoiceNumber: (cartId.length > 6) ? cartId.substr(cartId.length - 6, 6) : StringExtensions.padLeft(cartId, "0", 6), // Point device requires invoice number on initialization and since we do not generate invoice number until after order is invoiced in AX, we are using truncated transaction id.
                            IsTestMode: ApplicationContext.Instance.hardwareProfile.EftTestMode,
                            TerminalSettings: terminalSettings
                        };

                        // Set device IP and port for network payment terminal
                        if (ApplicationContext.Instance.hardwareProfile.PinPadDeviceTypeValue === Model.Entities.PeripheralDeviceType.Network) {
                            var pinpadConfiguration: Model.Entities.HardwareConfiguration;
                            var hardwareStation: Model.Entities.IHardwareStation = HardwareStationEndpointStorage.getActiveHardwareStation();
                            if (!ObjectExtensions.isNullOrUndefined(hardwareStation) && hardwareStation.ProfileId) {
                                pinpadConfiguration = hardwareStation.HardwareConfigurations.PinPadConfiguration;
                            } else {
                                pinpadConfiguration = ApplicationContext.Instance.deviceConfiguration.HardwareConfigurations.PinPadConfiguration;
                            }

                            if (pinpadConfiguration) {
                                var transportConfig = <Model.Entities.CommerceProperty>{
                                    Key: PeripheralConfigKey.TransportType,
                                    Value: <Model.Entities.CommercePropertyValue>{
                                        StringValue: TransportType.TcpTransport
                                    }
                                };
                                var ipConfig = <Model.Entities.CommerceProperty>{
                                    Key: PeripheralConfigKey.IpAddress,
                                    Value: <Model.Entities.CommercePropertyValue>{
                                        StringValue: pinpadConfiguration.IPAddress
                                    }
                                };
                                var portConfig = <Model.Entities.CommerceProperty>{
                                    Key: PeripheralConfigKey.Port,
                                    Value: <Model.Entities.CommercePropertyValue>{
                                        IntegerValue: pinpadConfiguration.Port
                                    }
                                };
                                paymentTerminalLockRequest.DeviceConfig = <PeripheralConfiguration>{
                                    ExtensionProperties: [transportConfig, ipConfig, portConfig]
                                };
                            }
                        }

                        return HardwareStationContext.instance.peripheral('PaymentTerminal').execute<string>('Lock', paymentTerminalLockRequest, HardwareStationContext.HS_DEFAULT_PAYMENT_TIMEOUT)
                            .done((result: string) => {
                                self.lockToken = result;
                            });
                    })
                    .run();
            }
            else {
                return VoidAsyncResult.createResolved();
            }
        }

        /**
         * Ends a transaction on payment terminal device.
         *
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        endTransaction(callerContext?: any): IVoidAsyncResult {
            if (!StringExtensions.isNullOrWhitespace(this.lockToken)) {
                var unlockRequest: UnlockRequest = {
                    Token: this.lockToken
                };

                this.lockToken = null;

                return HardwareStationContext.instance.peripheral('PaymentTerminal').execute<void>('Unlock', unlockRequest);
            }
            else {
                return VoidAsyncResult.createResolved();
            }
        }

        /**
         * Display transaction on the payment terminal device.
         *
         * @param {Model.Entities.Cart} cart The cart to display.
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        displayTransaction(cart: Model.Entities.Cart, callerContext?: any): IVoidAsyncResult {
            if (this.isActive && !StringExtensions.isNullOrWhitespace(this.lockToken) && cart.CartLines.length > 0) {
                var items: ItemInfo[] = [];

                for (var i = 0; i < cart.CartLines.length; i++) {
                    var cartLine = cart.CartLines[i];

                    var item: ItemInfo = {
                        LineItemId: i + 1, // Point device requires an integer line item id.
                        Sku: '', // Setting this to empty string as we do not have sku information for the product on the cart.
                        Upc: cartLine.Barcode,
                        Description: (cartLine.Description.length > 31) ? cartLine.Description.substr(0, 31) : cartLine.Description,
                        Quantity: cartLine.Quantity,
                        UnitPrice: cartLine.Price.toFixed(2).toString(),
                        ExtendedPriceWithTax: cartLine.TotalAmount.toFixed(2).toString(),
                        IsVoided: cartLine.IsVoided,
                        Discount: cartLine.DiscountAmount.toFixed(2).toString()
                    };

                    items.push(item);
                }

                var action = () => {
                    var paymentTerminalDisplayRequest: PaymentTerminalDisplayRequest = {
                        Token: this.lockToken,
                        TotalAmount: cart.TotalAmount.toFixed(2).toString(),
                        TaxAmount: cart.TaxAmount.toFixed(2).toString(),
                        DiscountAmount: cart.DiscountAmount.toFixed(2).toString(),
                        SubTotalAmount: cart.SubtotalAmount.toFixed(2).toString(),
                        Items: items
                    };

                    return HardwareStationContext.instance.peripheral('PaymentTerminal').execute<void>('UpdateLineItems', paymentTerminalDisplayRequest);
                };

                return HardwareStationContext.executeWithRelock(action, () => this.beginTransaction());
            }
            else {
                return VoidAsyncResult.createResolved();
            }
        }

        /**
         * Gets the tender information from the payment terminal device.
         *
         * @param {number} amount The amount to request.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<TenderInfo>} The async result.
         */
        public getTender(amount: number, callerContext?: any): IAsyncResult<Model.Entities.CardInfo> {
            return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.NOT_IMPLEMENTED)]);
        }

        /**
         * Get the tender approval from the payment terminal device.
         *
         * @param {number} amount The amount to request.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Model.Entities.CardInfo>} The async result.
         */
        public getTenderApproval(amount: number, callerContext?: any): IAsyncResult<Model.Entities.CardInfo> {
            return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.NOT_IMPLEMENTED)]);
        }

        /**
        * Gets the tender information from the payment terminal device.
        *
        * @param {Model.Entities.TenderLine} tenderLine The tender line to request.
        * @param {string} merchantProperties The merchantProperties to make the payment request.
        * @param {string} paymentConnectorName The connector name to make the payment request.
        * @param {Commerce.Peripherals.HardwareStation.PaymentType} paymentType The paymentType to make the payment request.
        * @param {Commerce.Model.Entities.CardType} cardTypeValue The card type.
        * @param {any} [callerContext] The callback context.
        * @return {IAsyncResult<TenderInfo>} The async result.
        */
        public makePayment(tenderLine: Model.Entities.TenderLine, merchantProperties: string, paymentConnectorName: string, paymentType: Commerce.Peripherals.HardwareStation.PaymentType, cardTypeValue: Commerce.Model.Entities.CardType, callerContext?: any): IAsyncResult<Model.Entities.TenderLine> {
            return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.NOT_IMPLEMENTED)]);
        }

        /**
         * Authorizes payment and gets the authorized payment information from the payment terminal device.
         *
         * @param {number} amount The amount to get authorization for.
         * @param {string} voiceAuthorization The voice authorization code.
         * @param {boolean} isManualEntry Set to true if card number is entered manually in the device (point device). It is not related to manual entry of card number in Payment view.
         * @param {ExtensionTransaction} [extensionTransactionProperties] The transaction properties parameters for the command.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>} The async result.
         */
        public authorizePayment(amount: number, voiceAuthorization: string, isManualEntry: boolean, extensionTransactionProperties: ExtensionTransaction, callerContext?: any): IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo> {
            if (this.isActive) {

                var action = () => {
                    var authorizeRequest: PaymentTerminalAuthorizeRequest = {
                        Token: this.lockToken,
                        Amount: amount,
                        Currency: ApplicationContext.Instance.deviceConfiguration.Currency,
                        ExtensionTransactionProperties: extensionTransactionProperties,
                        IsManualEntry: isManualEntry,
                        VoiceAuthorization: voiceAuthorization
                    };

                    return HardwareStationContext.instance.peripheral('PaymentTerminal').execute<PaymentInfo>('AuthorizePayment', authorizeRequest, HardwareStationContext.HS_DEFAULT_PAYMENT_TIMEOUT, /* suppressGlobalErrorEvent: */true);
                };

                return HardwareStationContext.executeWithRelock(action, () => this.beginTransaction());
            }
            else {
                return AsyncResult.createResolved<PaymentInfo>(null);
            }
        }   
        
        /**
         * Voids the payment using the payment terminal device.
         *
         * @param {number} amount The amount to void payment for.
         * @param {string} paymentPropertiesXml The payment properties blob.
         * @param {ExtensionTransaction} [extensionTransactionProperties] The transaction properties parameters for the command.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>} The async result.
         */
        public voidPayment(amount: number, paymentPropertiesXml: string, extensionTransactionProperties: ExtensionTransaction, callerContext?: any): IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo> {
            if (this.isActive) {

                var action = () => {
                    var voidRequest: VoidPaymentRequest = {
                        Token: this.lockToken,
                        Amount: amount,
                        Currency: ApplicationContext.Instance.deviceConfiguration.Currency,
                        PaymentPropertiesXml: paymentPropertiesXml,
                        ExtensionTransactionProperties: extensionTransactionProperties
                    };

                    return HardwareStationContext.instance.peripheral('PaymentTerminal').execute<PaymentInfo>('VoidPayment', voidRequest);
                };

                return HardwareStationContext.executeWithRelock(action, () => this.beginTransaction());
            }
            else {
                return AsyncResult.createResolved<PaymentInfo>(null);
            }
        }    

        /**
         * Captures the payment using the payment terminal device.
         *
         * @param {number} amount The amount to capture payment for.
         * @param {string} paymentPropertiesXml The payment properties blob.
         * @param {ExtensionTransaction} [extensionTransactionProperties] The transaction properties parameters for the command.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>} The async result.
         */
        public capturePayment(amount: number, paymentPropertiesXml: string, extensionTransactionProperties: ExtensionTransaction, callerContext?: any): IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo> {
            if (this.isActive) {

                var action = () => {
                    var captureRequest: CapturePaymentRequest = {
                        Token: this.lockToken,
                        Amount: amount,
                        Currency: ApplicationContext.Instance.deviceConfiguration.Currency,
                        PaymentPropertiesXml: paymentPropertiesXml,
                        ExtensionTransactionProperties: extensionTransactionProperties
                    };

                    return HardwareStationContext.instance.peripheral('PaymentTerminal').execute<PaymentInfo>('CapturePayment', captureRequest);
                };

                return HardwareStationContext.executeWithRelock(action, () => this.beginTransaction());
            }
            else {
                return AsyncResult.createResolved<PaymentInfo>(null);
            }
        }    

        /**
         * Refunds the payment using the payment terminal device.
         *
         * @param {number} amount The amount to get refund for.
         * @param {boolean} isManualEntry Set to true if card number is entered manually in the device (point device). It is not related to manual entry of card number in Payment view.
         * @param {ExtensionTransaction} [extensionTransactionProperties] The transaction properties parameters for the command.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>} The async result.
         */
        public refundPayment(amount: number, isManualEntry: boolean, extensionTransactionProperties: ExtensionTransaction, callerContext?: any): IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo> {
            if (this.isActive) {

                var action = () => {
                    var refundRequest: PaymentTerminalRefundRequest = {
                        Token: this.lockToken,
                        Amount: amount,
                        Currency: ApplicationContext.Instance.deviceConfiguration.Currency,
                        IsManualEntry: isManualEntry,
                        ExtensionTransactionProperties: extensionTransactionProperties
                    };

                    return HardwareStationContext.instance.peripheral('PaymentTerminal').execute<PaymentInfo>('RefundPayment', refundRequest, HardwareStationContext.HS_DEFAULT_PAYMENT_TIMEOUT, /* suppressGlobalErrorEvent: */true);
                };

                return HardwareStationContext.executeWithRelock(action, () => this.beginTransaction());
            }
            else {
                return AsyncResult.createResolved<PaymentInfo>(null);
            }
        }   
        
        /**
         * Fetches card token using the payment terminal device.
         *
         * @param {boolean} isManualEntry The value indicating whether the card number is entered manually on the device. 
         * @param {ExtensionTransaction} [extensionTransactionProperties] The transaction properties parameters for the command.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>} The async result.
         */
        public fetchToken(isManualEntry: boolean, extensionTransactionProperties: ExtensionTransaction, callerContext?: any): IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo> {
            if (this.isActive) {

                var action = () => {
                    var tokenizeRequest: PaymentTerminalFetchTokenRequest = {
                        Token: this.lockToken,
                        IsManualEntry: isManualEntry,
                        ExtensionTransactionProperties: extensionTransactionProperties
                    };

                    return HardwareStationContext.instance.peripheral('PaymentTerminal').execute<PaymentInfo>('FetchToken', tokenizeRequest, HardwareStationContext.HS_DEFAULT_PAYMENT_TIMEOUT, /* suppressGlobalErrorEvent: */true);
                };

                return HardwareStationContext.executeWithRelock(action, () => this.beginTransaction());
            }
            else {
                return AsyncResult.createResolved<PaymentInfo>(null);
            }
        }  

        /**
         * Cancel the current operation.
         *
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        public cancelOperation(callerContext?: any): IVoidAsyncResult {
            if (!StringExtensions.isNullOrWhitespace(this.lockToken)) {
                var paymentTerminalCancelOperationRequest: LockedSessionRequest = {
                    Token: this.lockToken
                };

                return HardwareStationContext.instance.peripheral('PaymentTerminal').execute<void>('CancelOperation', paymentTerminalCancelOperationRequest);
            }
            else {
                return VoidAsyncResult.createResolved();
            }
        }

        /**
         * Extensibility execute method.
         * @param {string} task The task to execute.
         * @param {ExtensionTransaction} [extensionTransactionProperties] The transaction properties parameters for the command.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<PeripheralConfiguration[]>} The async result.
         */
        public executeTask(
            task: string,
            extensionTransactionProperties: ExtensionTransaction,
            callerContext?: any): IAsyncResult<PeripheralConfiguration[]> {

            if (this.isActive) {

                var action: () => IAsyncResult<PeripheralConfiguration[]> = () => {
                    var executeTaskRequest: PaymentTerminalExecuteTaskRequest = {
                        Token: this.lockToken,
                        Task: task,
                        ExtensionTransactionProperties: extensionTransactionProperties
                    };

                    return HardwareStationContext.instance.peripheral('PaymentTerminal').execute<PeripheralConfiguration[]>('ExecuteTask', executeTaskRequest);
                };

                return HardwareStationContext.executeWithRelock(action, () => this.beginTransaction());
            }
            else {
                return AsyncResult.createResolved<PeripheralConfiguration[]>(null);
            }
        }

    }
}
