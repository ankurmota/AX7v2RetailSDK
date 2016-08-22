/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../IAsyncResult.ts'/>
///<reference path='../IPinPad.ts'/>
///<reference path='HardwareStationContext.ts'/>

module Commerce.Peripherals.HardwareStation {
    "use strict";

    export class PinPad implements IPinPad {

        public isActive: boolean = false;

        private _lockToken: string;

        /**
         * Opens the pin pad device for use.
         *
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        public openDevice(callerContext?: any): IVoidAsyncResult {
            var asyncResult = new VoidAsyncResult(callerContext);
            var self = this;
            self.isActive = false;
            self._lockToken = null;

            if (Commerce.ApplicationContext.Instance.hardwareProfile.PinPadDeviceTypeValue === Model.Entities.PeripheralDeviceType.OPOS) {
                var pinPadLockRequest: LockRequest = {
                    DeviceName: Commerce.ApplicationContext.Instance.hardwareProfile.PinPadDeviceName,
                    DeviceType: Commerce.Model.Entities.PeripheralDeviceType[Commerce.ApplicationContext.Instance.hardwareProfile.PinPadDeviceTypeValue],
                    Culture: Commerce.ApplicationContext.Instance.deviceConfiguration.CultureName,
                    Timeout: HardwareStationContext.HS_DEFAULT_PAYMENT_TIMEOUT
                };

                HardwareStationContext.instance.peripheral('PinPad').execute<string>('Lock', pinPadLockRequest)
                    .done((result) => {
                        self._lockToken = result;
                        self.isActive = true;
                        asyncResult.resolve();
                    })
                    .fail((error) => asyncResult.reject(error));
            }
            else {
                asyncResult.resolve();
            }

            return asyncResult;
        }

        /**
         * Closes the pin pad device.
         *
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        public closeDevice(callerContext?: any): IVoidAsyncResult {
            var asyncResult = new VoidAsyncResult(callerContext);
            var self = this;

            if (self.isActive) {
                var unlockRequest: UnlockRequest = {
                    Token: self._lockToken
                };

                HardwareStationContext.instance.peripheral('PinPad').execute<void>('Unlock', unlockRequest)
                    .done((result) => {
                        self._lockToken = null;
                        self.isActive = false;
                        asyncResult.resolve();
                    })
                    .fail((error) => asyncResult.reject(error));
            }
            else {
                asyncResult.resolve();
            }

            return asyncResult;
        }

        /**
         * Gets the pin pad entry.
         *
         * @param {string} [cardNumber] The card number.
         * @param {number} [paymentAmount] The amount being paid.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Model.Entities.PinPadInfo>} The async result with information from the pin pad.
         */
        public getPinEntry(cardNumber: string, paymentAmount: number, callerContext?: any): IAsyncResult<Model.Entities.PinPadInfo> {
            var asyncResult = new AsyncResult<Model.Entities.PinPadInfo>(callerContext);
            var self = this;

            var pinEntryRequest: PinPadEntryRequest = {
                Token: self._lockToken,
                AccountNumber: cardNumber,
                Amount: paymentAmount
            };

            if (self.isActive) {
                HardwareStationContext.instance.peripheral('PinPad').execute<PinPadResults>('GetPinEntry', pinEntryRequest, HardwareStationContext.HS_DEFAULT_PAYMENT_TIMEOUT, /* suppressGlobalErrorEvent: */true)
                    .done((results) => {
                        if (!results.Aborted) {
                            // Return the card information
                            var pinPadInfo = new Model.Entities.PinPadInfo(results.EncryptedPin, results.AdditionalSecurityInfo);

                            asyncResult.resolve(pinPadInfo);
                        }
                        else {
                            // Pin pad entry was cancelled from device
                            asyncResult.resolve(null);
                        }
                    })
                    .fail((error) => asyncResult.reject(error));
            }
            else {
                asyncResult.resolve(null);
            }

            return asyncResult;
        }

        /**
         * Cancels the pin pad entry.
         *
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        public cancelPinEntry(callerContext?: any): IVoidAsyncResult {
            var asyncResult = new VoidAsyncResult(callerContext);
            var self = this;

            var lockedRequest: LockedSessionRequest = {
                Token: self._lockToken,
            };

            if (self.isActive) {
                HardwareStationContext.instance.peripheral('PinPad').execute<void>('CancelOperation', lockedRequest)
                    .done((result) => {
                        asyncResult.resolve();
                    })
                    .fail((error) => asyncResult.reject(error));
            }
            else {
                asyncResult.resolve();
            }

            return asyncResult;
        }
    }
}