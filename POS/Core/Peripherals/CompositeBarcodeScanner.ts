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
     * Composition of barcode scanners for unified access.
     */
    export class CompositeBarcodeScanner implements IBarcodeScanner {

        private _scanners: IBarcodeScanner[];
        private _scannerEventHandlers: Array<(barcode: string) => void> = [];

        /**
         * Ctor.
         * @param {IBarcodeScanner[]} scanners The barcode scanners.
         */
        constructor(scanners: IBarcodeScanner[]) {
            this._scanners = scanners;
        }

        /**
         * Enable barcode scanner device.
         * @param {(barcode: string) => void} scannerMsgEventHandler The msg handler.
         * @returns {IVoidAsyncResult} The async result.
         */
        public enableAsync(scannerMsgEventHandler: (barcode: string) => void): IVoidAsyncResult {
            if (ArrayExtensions.hasElements(this._scanners)) {

                // Device can be enabled by nested calls
                //  - First caller on the stack will enable/disable the device.
                //  - Last one will get the events.

                if (ObjectExtensions.isFunction(scannerMsgEventHandler)) {
                    this._scannerEventHandlers.push(scannerMsgEventHandler);
                }

                if (this._scannerEventHandlers.length === 1) {
                    return VoidAsyncResult.join(this._scanners.map((scanner: IBarcodeScanner) => {
                        return scanner.enableAsync((barcode: string) => {
                            scannerMsgEventHandler = this._scannerEventHandlers[this._scannerEventHandlers.length - 1];

                            if (!StringExtensions.isNullOrWhitespace(barcode) && scannerMsgEventHandler) {
                                scannerMsgEventHandler(barcode);
                            }
                        });
                    }));
                }
            }

            return VoidAsyncResult.createResolved();
        }

        /**
         * Disable barcode scanner device for scan.
         * @returns {IVoidAsyncResult} The async result.
         */
        public disableAsync(): IVoidAsyncResult {
            if (ArrayExtensions.hasElements(this._scanners)) {
                this._scannerEventHandlers.pop();

                if (this._scannerEventHandlers.length === 0) {
                    return VoidAsyncResult.join(this._scanners.map((scanner: IBarcodeScanner) => {
                        return scanner.disableAsync();
                    }));
                }
            }

            return VoidAsyncResult.createResolved();
        }
    }
}