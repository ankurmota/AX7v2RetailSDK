/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../IAsyncResult.ts'/>
///<reference path='../ICardPayment.ts'/>
///<reference path='HardwareStationContext.ts'/>

module Commerce.Peripherals.HardwareStation {
    "use strict";

    export class CardPayment implements ICardPayment {

        /**
         * Lock token.
         */
        public lockToken: string;

        /**
         * Begins a transaction on the hardware station.
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        public beginTransaction(callerContext?: any): IVoidAsyncResult {
            var self: CardPayment = this;
            this.lockToken = null;
            var debitCashbackLimit: number;

            return new AsyncQueue()
                    .enqueue(() => {
                        return ApplicationContext.Instance.debitCashbackLimitAsync.done((limit: number) => {
                            debitCashbackLimit = limit;
                        });
                    })
                    .enqueue(() => {
                        var cardTenderType: Model.Entities.TenderType =
                            ApplicationContext.Instance.tenderTypesMap.getTenderTypeByOperationId(Operations.RetailOperation.PayCard);

                        var terminalSettings: SettingsInfo = {
                            SignatureCaptureMinimumAmount: cardTenderType.MinimumSignatureCaptureAmount,
                            MinimumAmountAllowed: cardTenderType.MinimumAmountPerLine,
                            MaximumAmountAllowed: cardTenderType.MaximumAmountPerLine,
                            DebitCashbackLimit: debitCashbackLimit,
                            Locale: ApplicationContext.Instance.deviceConfiguration.CultureName,
                            TerminalId: ApplicationContext.Instance.activeEftTerminalId
                        };

                        var cardPaymentBeginTransactionRequest: CardPaymentBeginTransactionRequest = {
                            Timeout: HardwareStationContext.HS_DEFAULT_PAYMENT_TIMEOUT,
                            PaymentConnectorName: ApplicationContext.Instance.hardwareProfile.EftPaymentConnectorName,
                            IsTestMode: ApplicationContext.Instance.hardwareProfile.EftTestMode,
                            TerminalSettings: terminalSettings
                        };

                        return HardwareStationContext.instance.peripheral("CardPayment")
                            .execute<string>("BeginTransaction", cardPaymentBeginTransactionRequest)
                            .done((result: string) => {
                                self.lockToken = result;
                            });
                    })
                    .run();
        }

        /**
         * Ends a transaction on the hardware station.
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        endTransaction(callerContext?: any): IVoidAsyncResult {
            if (!StringExtensions.isNullOrWhitespace(this.lockToken)) {
                var endTransactionRequest: LockedSessionRequest = {
                    Token: this.lockToken
                };

                this.lockToken = null;

                return HardwareStationContext.instance.peripheral("CardPayment").execute<void>("EndTransaction", endTransactionRequest);
            } else {
                return VoidAsyncResult.createResolved();
            }
        }

        /**
         * Authorizes payment given the tender information.
         * @param {number} amount The amount to get authorization for.
         * @param {Commerce.Peripherals.HardwareStation.TenderInfo} tenderInfo The tender information.
         * @param {ExtensionTransaction} [extensionTransactionProperties] The transaction properties parameters for the command.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>} The async result.
         */
        public authorizePayment(
            amount: number,
            tenderInfo: Commerce.Peripherals.HardwareStation.TenderInfo,
            extensionTransactionProperties: ExtensionTransaction,
            callerContext?: any): IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo> {

            var action: () => IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo> = () => {
                var authorizeRequest: CardPaymentAuthorizeRefundRequest = {
                    Token: this.lockToken,
                    Amount: amount,
                    Currency: ApplicationContext.Instance.deviceConfiguration.Currency,
                    TenderInfo: tenderInfo,
                    ExtensionTransactionProperties: extensionTransactionProperties
                };

                return HardwareStationContext.instance.peripheral("CardPayment")
                    .execute<PaymentInfo>("AuthorizePayment", authorizeRequest);
            };

            return HardwareStationContext.executeWithRelock(action, () => this.beginTransaction());
        }

        /**
         * Voids the payment.
         * @param {number} amount The amount to void payment for.
         * @param {string} paymentPropertiesXml The payment properties blob.
         * @param {ExtensionTransaction} [extensionTransactionProperties] The transaction properties parameters for the command.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>} The async result.
         */
        public voidPayment(
            amount: number,
            paymentPropertiesXml: string,
            extensionTransactionProperties: ExtensionTransaction,
            callerContext?: any): IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo> {

            var action: () => IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo> = () => {
                var voidRequest: VoidPaymentRequest = {
                    Token: this.lockToken,
                    Amount: amount,
                    Currency: ApplicationContext.Instance.deviceConfiguration.Currency,
                    PaymentPropertiesXml: paymentPropertiesXml,
                    ExtensionTransactionProperties: extensionTransactionProperties
                };

                return HardwareStationContext.instance.peripheral("CardPayment").execute<PaymentInfo>("VoidPayment", voidRequest);
            };

            return HardwareStationContext.executeWithRelock(action, () => this.beginTransaction());
        }

        /**
         * Captures the payment.
         * @param {number} amount The amount to capture payment for.
         * @param {string} paymentPropertiesXml The payment properties blob.
         * @param {ExtensionTransaction} [extensionTransactionProperties] The transaction properties parameters for the command.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>} The async result.
         */
        public capturePayment(
            amount: number,
            paymentPropertiesXml: string,
            extensionTransactionProperties: ExtensionTransaction,
            callerContext?: any): IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo> {

            var action: () => IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo> = () => {
                var captureRequest: CapturePaymentRequest = {
                    Token: this.lockToken,
                    Amount: amount,
                    Currency: ApplicationContext.Instance.deviceConfiguration.Currency,
                    PaymentPropertiesXml: paymentPropertiesXml,
                    ExtensionTransactionProperties: extensionTransactionProperties
                };

                return HardwareStationContext.instance.peripheral("CardPayment")
                    .execute<PaymentInfo>("CapturePayment", captureRequest);
            };

            return HardwareStationContext.executeWithRelock(action, () => this.beginTransaction());
        }

        /**
         * Refunds the payment given the tender information.
         * @param {number} amount The amount to get refund for.
         * @param {Commerce.Peripherals.HardwareStation.TenderInfo} tenderInfo The tender information.
         * @param {ExtensionTransaction} [extensionTransactionProperties] The transaction properties parameters for the command.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>} The async result.
         */
        public refundPayment(
            amount: number,
            tenderInfo: Commerce.Peripherals.HardwareStation.TenderInfo,
            extensionTransactionProperties: ExtensionTransaction,
            callerContext?: any): IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo> {

            var action: () => IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo> = () => {
                var refundRequest: CardPaymentAuthorizeRefundRequest = {
                    Token: this.lockToken,
                    Amount: amount,
                    Currency: ApplicationContext.Instance.deviceConfiguration.Currency,
                    TenderInfo: tenderInfo,
                    ExtensionTransactionProperties: extensionTransactionProperties
                };

                return HardwareStationContext.instance.peripheral("CardPayment").execute<PaymentInfo>("RefundPayment", refundRequest);
            };

            return HardwareStationContext.executeWithRelock(action, () => this.beginTransaction());
        }

        /**
         * Gets the card token given the tender information.
         * @param {Commerce.Peripherals.HardwareStation.TenderInfo} tenderInfo The tender information.
         * @param {ExtensionTransaction} [extensionTransactionProperties] The transaction properties parameters for the command.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>} The async result.
         */
        public fetchToken(
            tenderInfo: Commerce.Peripherals.HardwareStation.TenderInfo,
            extensionTransactionProperties: ExtensionTransaction,
            callerContext?: any): IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo> {

            var action: () => IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo> = () => {
                var fetchTokenRequest: CardPaymentFetchTokenRequest = {
                    Token: this.lockToken,
                    TenderInfo: tenderInfo
                };

                return HardwareStationContext.instance.peripheral("CardPayment").execute<PaymentInfo>("FetchToken", fetchTokenRequest);
            };

            return HardwareStationContext.executeWithRelock(action, () => this.beginTransaction());
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

            var action: () => IAsyncResult<PeripheralConfiguration[]> = () => {
                var executeTaskRequest: PaymentTerminalExecuteTaskRequest = {
                    Token: this.lockToken,
                    Task: task,
                    ExtensionTransactionProperties: extensionTransactionProperties
                };

                return HardwareStationContext.instance.peripheral("CardPayment").execute<PeripheralConfiguration[]>("ExecuteTask", executeTaskRequest);
            };

            return HardwareStationContext.executeWithRelock(action, () => this.beginTransaction());
        }
    }
}
