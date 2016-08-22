/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/Peripherals.ts'/>

module Commerce.Peripherals {
    "use strict";

    /**
     * Composition of msrs for unified access.
     */
    export class CompositeMagneticStripeReader implements IMagneticStripeReader {

        private _readers: IMagneticStripeReader[];
        private _readerMsgEventHandlers: Array<(barcode: string) => void> = [];

        /**
         * Ctor.
         * @param {IMagneticStripeReader[]} readers The MSR devices.
         */
        constructor(readers: IMagneticStripeReader[]) {
            this._readers = readers;
        }

        /**
         * Enable MSR device for scan.
         * @param {cardInfo: Model.Entities.CardInfo) => void} readerMsgEventHandler The msg handler.
         * @returns {IVoidAsyncResult} The async result.
         */
        public enableAsync(readerMsgEventHandler: (cardInfo: Model.Entities.CardInfo) => void): IVoidAsyncResult {
            if (ArrayExtensions.hasElements(this._readers)) {

                // Device can be enabled by nested calls
                //  - First caller on the stack will enable/disable the device.
                //  - Last one will get the events.

                if (ObjectExtensions.isFunction(readerMsgEventHandler)) {
                    this._readerMsgEventHandlers.push(readerMsgEventHandler);
                }

                if (this._readerMsgEventHandlers.length === 1) {
                    return VoidAsyncResult.join(this._readers.map((reader: IMagneticStripeReader) => {
                        return reader.enableAsync((cardInfo: Proxy.Entities.CardInfo) => {
                            readerMsgEventHandler = this._readerMsgEventHandlers[this._readerMsgEventHandlers.length - 1];

                            if (!ObjectExtensions.isNullOrUndefined(cardInfo) && readerMsgEventHandler) {
                                readerMsgEventHandler(cardInfo);
                            }
                        });
                    }));
                }
            }

            return VoidAsyncResult.createResolved();
        }

        /**
         * Disable MSR device for scan.
         * @returns {IVoidAsyncResult} The async result.
         */
        public disableAsync(): IVoidAsyncResult {
            if (ArrayExtensions.hasElements(this._readers)) {
                this._readerMsgEventHandlers.pop();

                if (this._readerMsgEventHandlers.length === 0) {
                    return VoidAsyncResult.join(this._readers.map((reader: IMagneticStripeReader) => {
                        return reader.disableAsync();
                    }));
                }
            }

            return VoidAsyncResult.createResolved();
        }
    }
}