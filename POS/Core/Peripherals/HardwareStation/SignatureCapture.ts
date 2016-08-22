/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../IAsyncResult.ts'/>
///<reference path='../ISignatureCapture.ts'/>
///<reference path='HardwareStationContext.ts'/>

module Commerce.Peripherals.HardwareStation {
    "use strict";

    export class SignatureCapture implements ISignatureCapture {

        public isActive: boolean = false;
        private _lockToken: string;

        /**
         * Opens the signature capture device for use.
         *
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        public openDevice(callerContext?: any): IVoidAsyncResult {
            var asyncResult = new VoidAsyncResult(callerContext);
            var self = this;
            self.isActive = false;
            self._lockToken = null;

            if (Commerce.ApplicationContext.Instance.hardwareProfile.SignatureCaptureDeviceTypeValue == Model.Entities.PeripheralDeviceType.OPOS) {
                var sigCapLockRequest: LockRequest = {
                    DeviceName: Commerce.ApplicationContext.Instance.hardwareProfile.SignatureCaptureDeviceName,
                    DeviceType: Commerce.Model.Entities.PeripheralDeviceType[Commerce.ApplicationContext.Instance.hardwareProfile.SignatureCaptureDeviceTypeValue],
                    Culture: Commerce.ApplicationContext.Instance.deviceConfiguration.CultureName,
                    Timeout: HardwareStationContext.HS_DEFAULT_PAYMENT_TIMEOUT
                };

                HardwareStationContext.instance.peripheral('SignatureCapture').execute<string>('Lock', sigCapLockRequest)
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
         * Closes the signature capture device.
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

                HardwareStationContext.instance.peripheral('SignatureCapture').execute<void>('Unlock', unlockRequest)
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
         * Gets the signature.
         *
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<string>} The async result with the signature points as a byte array base 64 string.
         */
        public getSignature(callerContext?: any): IAsyncResult<string> {
            var asyncResult = new AsyncResult<string>(callerContext);
            var self = this;

            var signatureRequest: LockedSessionRequest = {
                Token: self._lockToken
            };

            if (self.isActive) {
                HardwareStationContext.instance.peripheral('SignatureCapture').execute<SignatureCaptureResults>('GetSignature', signatureRequest, HardwareStationContext.HS_DEFAULT_PAYMENT_TIMEOUT, /* suppressGlobalErrorEvent: */true)
                    .done((results) => {
                        // If signature entry cannot be terminated from device, termination occurs from MPOS but a signature is still provided
                        if (!results.UserTerminatedSignature || !StringExtensions.isEmptyOrWhitespace(results.Signature)) {
                            asyncResult.resolve(results.Signature);
                        }
                        else {
                            // Signature entry was cancelled from device
                            asyncResult.resolve(StringExtensions.EMPTY);
                        }
                    })
                    .fail((error) => asyncResult.reject(error));
            }
            else {
                asyncResult.resolve(StringExtensions.EMPTY);
            }

            return asyncResult;
        }

        /**
         * Cancels getting the signature.
         *
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        public cancelSignature(callerContext?: any): IVoidAsyncResult {
            var asyncResult = new VoidAsyncResult(callerContext);
            var self = this;

            var lockedRequest: LockedSessionRequest = {
                Token: self._lockToken,
            };

            if (self.isActive) {
                HardwareStationContext.instance.peripheral('SignatureCapture').execute<void>('CancelOperation', lockedRequest)
                    .done((result) => {
                        asyncResult.resolve();
                    })
                    .fail((error) => asyncResult.reject(error));;
            }
            else {
                asyncResult.resolve();
            }

            return asyncResult;
        }
    }
}