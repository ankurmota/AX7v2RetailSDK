/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='HardwareStationContext.ts'/>

module Commerce.Peripherals.HardwareStation {
    "use strict";

    /*
     * Hardware Station response with an acquired token.
     */
    interface ILockResponse {
        // @NOTE: Hardware Station contract violates tslint rules.
        
        Token: string;
        
    }

    /**
     * A base class with a common logic for long polling implementations of OPOS barcode scanner and msr.
     * @typeparam THSData The data type sent by the hardware station to the client.
     * @typeparam THandlerData The data type used by the peripheral data handler.
     */
    export class LongPollingLockPeripheralBase<THSData, THandlerData> {

        private static HS_LOCK_ACTION: string = "Lock";
        private static HS_UNLOCK_ACTION: string = "Unlock";
        private static ERROR_TIMEOUT_MULTIPLIER: number = 2;
        private static ERROR_TIMEOUT_SEED_MS: number = 500;
        private static MAX_ERROR_TIMEOUT_MS: number = LongPollingLockPeripheralBase.ERROR_TIMEOUT_SEED_MS
                                                            * Math.pow(LongPollingLockPeripheralBase.ERROR_TIMEOUT_MULTIPLIER, 4);

        private _lockToken: string;
        private _pollingTimeoutInSeconds: number;
        private _eventHandlers: Array<(data: THandlerData) => void>;
        private _asyncWorkerQueue: AsyncWorkerQueue;
        private _currentErrorTimeoutMs: number;

        /**
         * Ctor.
         * @param {number} [pollingTimeoutInSeconds] The timeout for long polling of THSData.
         */
        constructor(pollingTimeoutInSeconds?: number) {
            this._pollingTimeoutInSeconds = pollingTimeoutInSeconds || HardwareStationContext.HS_DEFAULT_POLLING_TIMEOUT_IN_SECONDS;
            this._eventHandlers = [];
            this._asyncWorkerQueue = new AsyncWorkerQueue();
            this.resetCurrentErrorTimeout();
        }

        /**
         * Lock the peripheral.
         * @param {data: THandlerData) => void} handler The data handler.
         * @returns {IVoidAsyncResult} The async result.
         */
        protected lockAsync(handler: (data: THandlerData) => void): IVoidAsyncResult {
            return this._asyncWorkerQueue.enqueue(() => {
                return this.lockInternalAsync(handler);
            });
        }

        /**
         * Unlock the peripheral.
         * @returns {IVoidAsyncResult} The async result.
         */
        protected unlockAsync(): IVoidAsyncResult {
            return this._asyncWorkerQueue.enqueue(() => {
                return this.unlockInternalAsync();
            });
        }

        /**
         * Create the request object for lock.
         * @returns {any} The lock request.
         */
        protected createLockRequest(): any {
            throw "Abstract method. Not implemented";
        }

        /**
         * Gets the device type.
         * @returns {Commerce.Model.Entities.PeripheralType} The device type.
         */
        protected get deviceType(): Commerce.Model.Entities.PeripheralType {
            throw "Abstract method. Not implemented";
        }

        /**
         * Gets the peripheral name (DeviceName).
         * @returns {string} The name.
         */
        protected get peripheralName(): string {
            throw "Abstract method. Not implemented";
        }

        /**
         * Gets the ODATA action name to get THSData.
         * @returns {string} The action name.
         */
        protected get getDataActionName(): string {
            throw "Abstract method. Not implemented";
        }

        /**
         * Handles the THSData using the peripheral handler.
         * @returns {string} The actiona name.
         */
        protected handleData(handler: (data: THandlerData) => void, data: THSData): void {
            throw "Abstract method. Not implemented";
        }

        private lockInternalAsync(handler: (data: THandlerData) => void): IVoidAsyncResult {

            if (!this.isConfigured()) {
                return VoidAsyncResult.createRejected();
            }

            var result: IVoidAsyncResult;

            if (this.isEnabled()) {
                result = VoidAsyncResult.createResolved();
            } else {
                result = this.sendLockRequestAsync();
            }

            result.done(() => {
                this._eventHandlers.push(handler);
            });

            return result;
        }

        private unlockInternalAsync(): IVoidAsyncResult {
            if (!this.isConfigured() || !this.isEnabled()) {
                return VoidAsyncResult.createRejected();
            }

            var result: IVoidAsyncResult;
            if (this._eventHandlers.length > 1) {
                result = VoidAsyncResult.createResolved();
            } else {
                result = this.sendUnlockRequestAsync();
            }

            result.always(() => {
                this._eventHandlers.pop();
            });

            return result;
        }

        private sendLockRequestAsync(): IVoidAsyncResult {
            var lockRequest: any = this.createLockRequest();
            return this.execute<ILockResponse>(LongPollingLockPeripheralBase.HS_LOCK_ACTION, lockRequest, this._pollingTimeoutInSeconds)
                .done((response: ILockResponse) => {
                    this._lockToken = response.Token;
                    this.doGetData(response.Token);
                });
        }

        private sendUnlockRequestAsync(): IVoidAsyncResult {
            var unlockRequest: LockedSessionRequest = {
                Token: this._lockToken
            };
            return this.execute(LongPollingLockPeripheralBase.HS_UNLOCK_ACTION, unlockRequest, this._pollingTimeoutInSeconds)
                        .always(() => {
                            this._lockToken = null;
                        });
        }

        private sendGetDataRequestAsync(token: string): IAsyncResult<THSData> {
            var getDataRequest: any = {
                Token: token,
                TimeoutInSeconds: this._pollingTimeoutInSeconds
            };

            return this.execute<THSData>(this.getDataActionName, getDataRequest, /*timeout: */null, /* suppressGlobalErrorEvent: */true);
        }

        private isConfigured(): boolean {
            return this.deviceType === Model.Entities.PeripheralType.OPOS;
        }

        private isEnabled(): boolean {
            return !ObjectExtensions.isNullOrUndefined(this._lockToken);
        }

        private execute<T>(action: string, data?: any, timeout?: number, suppressGlobalErrorEvent?: boolean): IAsyncResult<T> {
            return HardwareStationContext
                        .instance
                        .peripheral(this.peripheralName)
                        .execute<T>(action, data, timeout, suppressGlobalErrorEvent);
        }

        private doGetData(currentToken: string): void {
            if (this.shouldStopGetData(currentToken)) {
                return;
            }

            this.sendGetDataRequestAsync(currentToken)
                .done((data: THSData) => {
                    this.onDoGetDataSuccess(currentToken, data);
                })
                .fail((errors: Model.Entities.Error[]) => {
                    RetailLogger.peripheralsLongPollingLockGetDataError(ErrorHelper.serializeErrorsForRetailLogger(errors));
                    this.onDoGetDataError(errors, currentToken);
                });
        }

        private onDoGetDataSuccess(currentToken: string, data: THSData): void {
            this.resetCurrentErrorTimeout();

            if (this.shouldStopGetData(currentToken)) {
                return;
            }

            if (!ObjectExtensions.isNullOrUndefined(data)) {
                var handler: (data: THandlerData) => void = this._eventHandlers[this._eventHandlers.length - 1];
                if (ObjectExtensions.isFunction(handler)) {
                    try {
                        this.handleData(handler, data);
                    } catch (e) {
                        RetailLogger.peripheralsLongPollingLockGetDataUnhandledError(this.getTraceMessage("Unhandled data handle exception. Details: {0}", e));
                    }
                }
            }

            this.doGetData(currentToken);
        }

        private onDoGetDataError(errors: Model.Entities.Error[], currentToken: string): void {

            if (this.shouldStopGetData(currentToken)) {
                return;
            }

            if (HardwareStationContext.isLockNotAcquiredError(errors)) {
                this._asyncWorkerQueue.enqueue(() => {
                    return this.sendLockRequestAsync();
                });
                return;
            }

            setTimeout(() => {
                this.doGetData(currentToken);
            }, this._currentErrorTimeoutMs);

            this.increaseCurrentErrorTimeout();
        }

        private shouldStopGetData(currentToken: string): boolean {
            var scannerReEnabled: boolean = (currentToken !== this._lockToken);
            return scannerReEnabled || !this.isEnabled();
        }

        private resetCurrentErrorTimeout(): void {
            this._currentErrorTimeoutMs = LongPollingLockPeripheralBase.ERROR_TIMEOUT_SEED_MS;
        }

        private increaseCurrentErrorTimeout(): void {
            var newTimeout: number = this._currentErrorTimeoutMs * LongPollingLockPeripheralBase.ERROR_TIMEOUT_MULTIPLIER;
            this._currentErrorTimeoutMs = Math.min(newTimeout, LongPollingLockPeripheralBase.MAX_ERROR_TIMEOUT_MS);
        }

        private getTraceMessage(message: string, ...params: any[]): string {
            var formattedMessage: string = StringExtensions.format(message, params);
            return StringExtensions.format("Peripheral {0}. {1}", this.peripheralName, formattedMessage);
        }
    }
}
