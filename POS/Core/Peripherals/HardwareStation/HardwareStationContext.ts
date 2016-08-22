/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Extensions/StringExtensions.ts'/>
///<reference path='../../ApplicationContext.ts'/>
///<reference path='../../ApplicationStorage.ts'/>
///<reference path='../../IAsyncResult.ts'/>

module Commerce.Peripherals.HardwareStation {

    import ErrorParser = Proxy.Context.ErrorParser;

    export interface PairingRequest {
        DeviceNumber: string;
        HardwareStationToken: string;
    }

    export interface SavePaymentMerchantInformationRequest {
        HardwareProfileId: string;
        PaymentMerchantInformation: string;
    }

    export interface PeripheralConfiguration {
        ExtensionProperties?: Proxy.Entities.CommerceProperty[];
    }

    export interface ExtensionTransaction {
        ExtensionProperties?: Proxy.Entities.CommerceProperty[];
    }

    export interface PeripheralRequest {
        DeviceName: string;
        DeviceType: string;
    }

    export interface PeripheralOpenRequest extends PeripheralRequest {
        DeviceConfig?: PeripheralConfiguration;
    }

    export interface LockRequest extends PeripheralOpenRequest {
        Culture?: string;
        Timeout?: number;
        Override?: boolean;
    }

    export interface UnlockRequest {
        Token: string;
    }

    export interface LockedSessionRequest {
        Token: string;
    }

    export interface LineDisplayLockRequest extends LockRequest {
        CharacterSet: number;
        BinaryConversion: boolean;
    }

    export interface LineDisplayLockResponse {
        Token: string;
        Columns: number;
    }

    export interface LineDisplayRequest extends LockedSessionRequest {
        Lines?: string[];
        Clear?: boolean;
    }

    export interface PrintRequest extends PeripheralOpenRequest {
        CharacterSet: number;
        BinaryConversion: boolean;
        Header: string;
        Lines: string;
        Footer: string;
    }

    export interface ScaleRequest extends PeripheralOpenRequest {
        Timeout: number;
    }

    export interface CardPaymentBeginTransactionRequest {
        PaymentConnectorName: string;
        IsTestMode: boolean;
        TerminalSettings: SettingsInfo;
        Timeout?: number;
    }

    export interface CardPaymentAuthorizeRefundRequest extends LockedSessionRequest {
        Amount: number;
        Currency: string;
        TenderInfo: TenderInfo;
        ExtensionTransactionProperties: ExtensionTransaction;
    }

    export interface CardPaymentFetchTokenRequest extends LockedSessionRequest {
        TenderInfo: TenderInfo;
    }

    export interface PaymentTerminalExecuteTaskRequest extends LockedSessionRequest {
        Task: string;
        ExtensionTransactionProperties: ExtensionTransaction;
    }

    export interface PaymentTerminalLockRequest extends LockRequest {
        PaymentConnectorName: string;
        InvoiceNumber: string;
        IsTestMode: boolean;
        TerminalSettings: SettingsInfo;
    }

    export interface PaymentTerminalDisplayRequest extends LockedSessionRequest {
        TotalAmount: string;
        TaxAmount: string;
        DiscountAmount: string;
        SubTotalAmount: string;
        Items: ItemInfo[];
    }

    export interface PaymentTerminalAuthorizeRequest extends LockedSessionRequest {
        Amount: number;
        Currency: string;
        VoiceAuthorization: string;
        IsManualEntry: boolean;
        ExtensionTransactionProperties: ExtensionTransaction;
    }

    export interface CapturePaymentRequest extends LockedSessionRequest {
        Amount: number;
        Currency: string;
        PaymentPropertiesXml: string;
        ExtensionTransactionProperties: ExtensionTransaction;
    }

    export interface PaymentTerminalFetchTokenRequest extends LockedSessionRequest {
        IsManualEntry: boolean;
        ExtensionTransactionProperties: ExtensionTransaction;
    }

    export interface PaymentTerminalRefundRequest extends LockedSessionRequest {
        Amount: number;
        Currency: string;
        IsManualEntry: boolean;
        ExtensionTransactionProperties: ExtensionTransaction;
    }

    export interface VoidPaymentRequest extends LockedSessionRequest {
        Amount: number;
        Currency: string;
        PaymentPropertiesXml: string;
        ExtensionTransactionProperties: ExtensionTransaction;
    }

    export enum PaymentType {
        // Default setting not supported by server.
        Unknown = 0,

        // Authorize or refund payment type.
        AuthorizeOrRefund = 1,

        // Void payment type.
        Void = 2,

        // Generate card token payment type.
        CardToken = 3,

        // Capture payment type.
        Capture = 4
    }

    export interface PinPadEntryRequest extends LockedSessionRequest {
        AccountNumber: string;
        Amount: number;
    }

    export interface PinPadResults {
        EncryptedPin: string;
        AdditionalSecurityInfo: string;
        Aborted: boolean;
        Status: number;
    }

    export interface SignatureCaptureResults {
        UserTerminatedSignature: boolean;
        Signature: string;
    }

    export interface ItemInfo {
        LineItemId: number;
        Sku: string;
        Upc: string;
        Description: string;
        Quantity: number;
        UnitPrice: string;
        ExtendedPriceWithTax: string;
        IsVoided: boolean;
        Discount: string; 
    }

    export interface SettingsInfo {
        SignatureCaptureMinimumAmount: number;
        MinimumAmountAllowed: number;
        MaximumAmountAllowed: number;
        DebitCashbackLimit: number;
        Locale: string;
        TerminalId: string;
    }

    export interface PaymentInfo {
        CardNumberMasked: string;
        CardType: Proxy.Entities.CardType;
        SignatureData: string;
        PaymentSdkData: string;
        CashbackAmount: number;
        ApprovedAmount: number;
        IsApproved: boolean;
        Errors: Proxy.Entities.Error[];
    }

    export interface TenderInfo {
        TenderId: string;
        CardTypeId?: string;
        CardNumber?: string;
        Track1?: string;
        Track2?: string;
        Track3?: string;
        EncryptedPin?: string;
        AdditionalSecurityData?: string;
        CashbackAmount?: number;
        CCID?: string;
        VoiceAuthorizationCode?: string;
        IsSwipe?: boolean;
        Name?: string;
        Country?: string;
        Address?: string;
        Zip?: string;
        ExpirationMonth?: number;
        ExpirationYear?: number;
    }

    export interface BarcodeScannerLockRequest extends LockRequest {
    }

    export interface BarcodeScannerLockResponse {
        Token: string;
    }

    export interface BarcodeScannerRequest extends LockedSessionRequest {
        TimeoutInSeconds: number;
    }

    export interface MsrLockRequest extends LockRequest {
    }

    export interface MsrLockResponse {
        Token: string;
    }

    export interface MsrRequest extends LockedSessionRequest {
        TimeoutInSeconds: number;
    }

    export interface MagneticCardSwipeInfo {
        AccountNumber: string;
        FirstName: string;
        LastName: string;
        ExpirationMonth: number;
        ExpirationYear: number;
        Track1Data: string;
        Track2Data: string;
    }

    /**
     * Represents a hardware station request object.
     */
    export class HardwareStationRequest {

        private _requestUri: string;
        private _requestAuthorization: string;
        private _onError: (errors: Proxy.Entities.Error[]) => void;

        /**
         * Ctor
         *
         * @param {string} endpointUri The service end point uri.
         * @param {string} entity The peripheral name.
         * @param {(errors: Proxy.Entities.Error[]) => void} [onError] The error callback.
         */
        constructor(hardwareStation: Proxy.Entities.IHardwareStation, peripheral: string, onError?: (errors: Proxy.Entities.Error[]) => void) {
            if (!ObjectExtensions.isNullOrUndefined(hardwareStation)) {
                // e.g. http://HardwareStation/Printer
                this._requestUri = StringExtensions.format("{0}/{1}", hardwareStation.Url, peripheral);
                this._requestAuthorization = StringExtensions.format("MessageCredential {0} {1}",
                    ApplicationStorage.getItem(ApplicationStorageIDs.DEVICE_ID_KEY),
                    HardwareStationEndpointStorage.getHardwareStationToken(hardwareStation.RecordId, hardwareStation.Url) || "");
            }
            this._onError = onError;
        }

        /**
         * Execute a hardware station request.
         *
         * @param {string} action The action to execute.
         * @param {any} [data] The request object.
         * @param {number} [timeout] The connection timeout in seconds.
         * @param {boolean} [suppressGlobalErrorEvent] Indicates that the global error event should be suppressed.
         * @return {IAsyncResult<T>} The async result.
         */
        public execute<T>(action: string, data?: any, timeout?: number, suppressGlobalErrorEvent?: boolean): IAsyncResult<T> {
            var asyncResult = new AsyncResult<T>();
            var actionRequest = this._requestUri + '/' + action;
            var self = this;

            if (!StringExtensions.isNullOrWhitespace(this._requestUri)) {
                if (this._requestUri.toUpperCase().indexOf(HardwareStationContext.localStation) === 0) {
                    try {
                        var hardwareStationRequestMessage = new Microsoft.Dynamics.Commerce.ClientBroker.HardwareStationRequestMessage();

                        hardwareStationRequestMessage.requestUri = actionRequest;
                        hardwareStationRequestMessage.method = "POST";
                        hardwareStationRequestMessage.body = data ? JSON.stringify(data, null, 2) : null;

                        Microsoft.Dynamics.Commerce.ClientBroker.HardwareStationRequest.executeAsync(hardwareStationRequestMessage)
                            .done((result: Microsoft.Dynamics.Commerce.ClientBroker.HardwareStationResponeMessage) => {
                                if (Proxy.Common.HttpStatusCodes.isSuccessful(result.status)) {
                                    RetailLogger.peripheralsHardwareStationContextActionRequestSucceeded(actionRequest);
                                    asyncResult.resolve(result.responseText ? JSON.parse(result.responseText) : null);
                                } else {
                                    this.onHardwareStationError(asyncResult, result, actionRequest, suppressGlobalErrorEvent);
                                }
                            },
                            (error) => {
                                this.onHardwareStationError(asyncResult, error, actionRequest, suppressGlobalErrorEvent);
                            });
                    }
                    catch (error) {
                        this.onHardwareStationError(asyncResult, error, actionRequest, suppressGlobalErrorEvent);
                    }
                } else {
                    $.ajax(actionRequest, {
                        type: "POST",
                        data: data ? JSON.stringify(data, null, 2) : null,
                        contentType: "application/json",
                        timeout: (timeout || Commerce.Config.connectionTimeout) * 1000,
                        accepts: "application/json",
                        beforeSend: function (request) {
                            request.setRequestHeader("Authorization", self._requestAuthorization);
                        },
                        cache: false,
                        crossDomain: true,
                        success: (result) => {
                            RetailLogger.peripheralsHardwareStationContextActionRequestSucceeded(actionRequest);
                            asyncResult.resolve(result);
                        },
                        error: (xhr, error, response) => {
                            this.onHardwareStationError(asyncResult, xhr, actionRequest, suppressGlobalErrorEvent);
                        }
                    });
                }
            } else {
                asyncResult.reject([new Proxy.Entities.Error(ErrorTypeEnum.PERIPHERALS_HARDWARESTATION_NOTCONFIGURED)]);
            }

            return asyncResult;
        }

        private onHardwareStationError<T>(asyncResult: AsyncResult<T>, error: any, actionRequest: string, suppressGlobalErrorEvent: boolean): void {
            var errors: Proxy.Entities.Error[] = ErrorParser.parseHardwareStationErrorMessage(error);
            RetailLogger.peripheralsHardwareStationContextActionRequestFailed(actionRequest, ErrorHelper.serializeErrorsForRetailLogger(errors));
            if (!suppressGlobalErrorEvent && ObjectExtensions.isFunction(this._onError)) {
                this._onError(errors);
            }
            asyncResult.reject(errors);
        }
    }

    /**
     * Represents a hardware station context.
     */
    export class HardwareStationContext {

        private static _instance: HardwareStationContext = null;
        public static localStation = "IPC://LOCALHOST"; // Keep uppercase
        public static HS_DEFAULT_LOCK_TIMEOUT: number = 28800; // 8 hours
        public static HS_DEFAULT_PAYMENT_TIMEOUT: number = 600; // 10 minutes
        public static HS_DEFAULT_POLLING_TIMEOUT_IN_SECONDS: number = 30; // 30 seconds
        public onError: (hardwareStation: Proxy.Entities.IHardwareStation, errors: Proxy.Entities.Error[]) => void; 

        /**
         * Gets the instance of hardware station context.
         */
        public static get instance(): HardwareStationContext {
            if (ObjectExtensions.isNullOrUndefined(HardwareStationContext._instance)) {
                HardwareStationContext._instance = new HardwareStationContext();
            }

            return HardwareStationContext._instance;
        }

        /**
         * Execute an action against hardware with relock if lock expired.
         *
         * @param {() => IAsyncResult<T>} action The action to execute.
         * @param {() => IVoidAsyncResult} [relockAction] The relock action to execute.
         * @return {IAsyncResult<T>} The async result.
         */
        public static executeWithRelock<T>(action?: () => IAsyncResult<T>, relockAction?: () => IVoidAsyncResult): IAsyncResult<T> {
            var asyncResult = new AsyncResult<T>(null);

            action()
                .done((result) => {
                    asyncResult.resolve(result);
                })
                .fail((errors) => {

                    if (HardwareStationContext.isLockNotAcquiredError(errors)) {
                        var asyncQueue = new AsyncQueue();

                        asyncQueue.enqueue(() => {
                            return relockAction();
                        }).enqueue(() => {
                            return action()
                                .done((result) => {
                                    asyncResult.resolve(result);
                                });
                        });

                        asyncQueue.run()
                            .fail((errors) => {
                                asyncResult.reject(errors);
                            });
                    } else {
                        asyncResult.reject(errors);
                    }
                });

            return asyncResult;
        }

        /**
         * Checks whether the errors are related to not acquired lock issue.
         * @param {Proxy.Entities.Error[]} errors The errors.
         * @return {boolean} True if the errors are related to a lock issue otherwise false.
         */
        public static isLockNotAcquiredError(errors: Proxy.Entities.Error[]): boolean {
            var error: Proxy.Entities.Error = <Proxy.Entities.Error>ArrayExtensions.firstOrUndefined(errors);

            if (ObjectExtensions.isNullOrUndefined(error)) {
                return false;
            }

            var code: string = error.ErrorCode;

            return (code == "Microsoft_Dynamics_Commerce_HardwareStation_PeripheralLockNotAcquired")
                || (code == "Microsoft_Dynamics_Commerce_HardwareStation_CardPayment_LockNotAcquired");
        }

        /**
         * Check if the given hardware station is a local hardware station.
         * @param {Proxy.Entities.IHardwareStation} hardwareStation The hardware station object.
         * @return {boolean} True or false.
         */
        public static isLocalStation(hardwareStation: Proxy.Entities.IHardwareStation): boolean {
            return (hardwareStation && (hardwareStation.Url.toUpperCase().indexOf(Peripherals.HardwareStation.HardwareStationContext.localStation) === 0));
        }

        /**
         * Gets the hardware station URL from a hardware station profile.
         *
         * @param {Proxy.Entities.HardwareStationProfile} The hardware station profile.
         * @return {string} The hardware station URL.
         */
        public static getHardwareStationUrlFromProfile(profile: Proxy.Entities.HardwareStationProfile): string {
            var hardwareStationUrl: string = StringExtensions.CleanUri(profile.HardwareStationUrl);
            var applicationType: Proxy.Entities.ApplicationTypeEnum = Commerce.Host.instance.application.getApplicationType();

            if (applicationType !== Proxy.Entities.ApplicationTypeEnum.CloudPos) {
                if (Commerce.UrlHelper.isLocalAddress(hardwareStationUrl)) {
                    hardwareStationUrl = Peripherals.HardwareStation.HardwareStationContext.localStation;
                }
            }

            return hardwareStationUrl;
        }

        /**
         * Gets the security request.
         *
         * @param {Proxy.Entities.IHardwareStation} hardwareStation The hardware station to initiate pairing with.
         */
        public security(hardwareStation: Proxy.Entities.IHardwareStation): HardwareStationRequest {
            return new HardwareStationRequest(hardwareStation, "Security");
        }

        /**
         * Gets whether there is an active hardware station.
         *
         * @return {boolean} True if there is an active hardware station. False if there isn't.
         */
        public isActive(): boolean {
            return !ObjectExtensions.isNullOrUndefined(this.getActiveHardwareStation());
        }

        /**
         * Gets the peripheral request.
         *
         * @param {string} peripheralKind The kind of peripheral (e.g. 'Printer').
         */
        public peripheral(peripheralKind: string): HardwareStationRequest {
            var activeHardwareStation: Proxy.Entities.IHardwareStation = this.getActiveHardwareStation();

            return new HardwareStationRequest(activeHardwareStation, peripheralKind, (errors: Proxy.Entities.Error[]) => {
                if(ObjectExtensions.isFunction(this.onError)) {
                    this.onError(activeHardwareStation, errors);
                }
            });
        }
        
        /**
         * Gets the hardware station url
         *
         * @return {Proxy.Entities.IHardwareStation} The hardware station.
         */
        private getActiveHardwareStation(): Proxy.Entities.IHardwareStation {
            return HardwareStationEndpointStorage.getActiveHardwareStation();
        }
    }

    /**
     * Contains the key constants of the peripheral configuration parameters.
     */
    export class PeripheralConfigKey {

        public static IpAddress = "IpAddress";
        public static Port = "Port";
        public static TransportType = "Transport";
    }

    /**
     * Contains the value constants of the transport types.
     */
    export class TransportType {

        public static TcpTransport = "tcp";
        public static TcpTlsTransport = "tcptls";
        public static SerialTransport = "serial";
    }
}

declare module Microsoft.Dynamics.Commerce.ClientBroker {
    class HardwareStationRequestMessage {
        public requestUri: string;
        public method: string;
        public body: string;
    }

    class HardwareStationResponeMessage {
        public status: number;
        public statusText: string;
        public responseText: string;
    }

    class HardwareStationRequest {
        static executeAsync(hardwareStationRequestMessage: HardwareStationRequestMessage): any;
    }
}
