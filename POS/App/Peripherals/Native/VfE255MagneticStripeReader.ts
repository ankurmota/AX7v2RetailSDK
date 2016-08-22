/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../../SharedApp/Commerce.Core.d.ts'/>

module Commerce.Peripherals.Native {
    "use strict";

    export class VfE255MagneticStripeReader implements IMagneticStripeReader, IInitializable {

        private _msr: Microsoft.Dynamics.Commerce.NetworkPeripherals.Peripherals.MultifunctionDevice.VeriFoneE255Compatible.VfE255Device;
        private _deviceId: string;

        /**
         * Ctor.
         *
         * @param {string} deviceId The deviceId.
         */
        constructor(deviceId: string) {
            this._deviceId = deviceId;
        }

        /**
         * Initialize MSR.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public initializeAsync(): IVoidAsyncResult {
            var asyncResult = new VoidAsyncResult();
            this._msr = Microsoft.Dynamics.Commerce.NetworkPeripherals.Peripherals.MultifunctionDevice.VeriFoneE255Compatible.VfE255Device.getVfE255Device(this._deviceId);
            asyncResult.resolve();
            return asyncResult;
        }

        /**
         * Enable magnetic stripe reader.
         *
         * @param {(cardInfo: Model.Entities.CardInfo) => void} readerMsgEventHandler The msg handler.
         * @returns {IVoidAsyncResult} The async result.
         */
        enableAsync(readerMsgEventHandler: (cardInfo: Model.Entities.CardInfo) => void): IVoidAsyncResult {
            var asyncResult = new VoidAsyncResult();
            if (this._msr != null) {
                var self = this;
                self._msr.onmsrdatareceived = (args) => {
                    if (readerMsgEventHandler) {
                        readerMsgEventHandler(
                            <Model.Entities.CardInfo>{
                                CardNumber: args.cardInfo.accountNumber,
                                FirstName: args.cardInfo.firstName,
                                LastName: args.cardInfo.lastName,
                                ExpirationMonth: args.cardInfo.expirationMonth,
                                ExpirationYear: args.cardInfo.expirationYear,
                                Track1: args.cardInfo.track1Data,
                                Track2: args.cardInfo.track2Data,
                                Track3: ""
                            });
                    }
                }
                this._msr.enableMsrAsync()
                    .done((result) => {
                        if (result) {
                            asyncResult.resolve();
                        }
                        else {
                            self._msr.onmsrdatareceived = null;
                            asyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.PERIPHERALS_MSR_ENABLE_FAILED)]);
                        }
                    }, (e) => {
                        asyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.PERIPHERALS_MSR_ENABLE_FAILED)]);
                    });
            }
            else {
                asyncResult.resolve();
            }

            return asyncResult;
        }

        /**
         * Disable MSR device for scan.
         * @returns {IVoidAsyncResult} The async result.
         */
        public disableAsync(): IVoidAsyncResult {
            if (this._msr) {
                this._msr.disableMsrAsync();
                this._msr.onmsrdatareceived = null;
            }

            return VoidAsyncResult.createResolved();
        }
    }
}
