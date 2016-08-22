/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../../SharedApp/Commerce.Core.d.ts'/>

declare module Microsoft.Dynamics.Commerce.NetworkPeripherals.Peripherals.MultifunctionDevice.VeriFoneE255Compatible {
    class VfE255Device {
        static getVfE255Device(id: string): Microsoft.Dynamics.Commerce.NetworkPeripherals.Peripherals.MultifunctionDevice.VeriFoneE255Compatible.VfE255Device
        enableBarcodeScannerAsync(): Windows.Foundation.IAsyncOperation<any>
        disableBarcodeScannerAsync(): Windows.Foundation.IAsyncOperation<any>
        enableMsrAsync(): Windows.Foundation.IAsyncOperation<any>
        disableMsrAsync(): Windows.Foundation.IAsyncOperation<any>
        onbarcodereceived: any;
        onmsrdatareceived: any;
    }
}

module Commerce.Peripherals.Native {
    "use strict";

    export class VfE255BarcodeScanner implements IBarcodeScanner, IInitializable {

        private _barcodeScanner: Microsoft.Dynamics.Commerce.NetworkPeripherals.Peripherals.MultifunctionDevice.VeriFoneE255Compatible.VfE255Device;
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
         * Initialize barcode scanner.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public initializeAsync(): IVoidAsyncResult {
            var asyncResult = new VoidAsyncResult();
            this._barcodeScanner = Microsoft.Dynamics.Commerce.NetworkPeripherals.Peripherals.MultifunctionDevice.VeriFoneE255Compatible.VfE255Device.getVfE255Device(this._deviceId);
            asyncResult.resolve();
            return VoidAsyncResult.createResolved();
        }

        /**
         * Enable barcode scanner device.
         *
         * @param {(barcode: string) => void} scannerMsgEventHandler The msg handler.
         * @returns {IVoidAsyncResult} The async result.
         */
        enableAsync(scannerMsgEventHandler: (barcode: string) => void): IVoidAsyncResult {
            var asyncResult = new VoidAsyncResult();
            if (this._barcodeScanner != null) {
                var self = this;
                self._barcodeScanner.onbarcodereceived = (args) => {
                    if (scannerMsgEventHandler) {
                        scannerMsgEventHandler(
                            args.barcodeData.replace(/(\r\n|\n|\r)/gm, ""));
                    }
                }
                this._barcodeScanner.enableBarcodeScannerAsync()
                    .done((result) => {
                        if (result) {
                            asyncResult.resolve();
                        }
                        else {
                            self._barcodeScanner.onbarcodereceived = null;
                            asyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.PERIPHERALS_BARCODE_SCANNER_ENABLE_FAILED)]);
                        }
                    }, (e) => {
                        asyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.PERIPHERALS_BARCODE_SCANNER_ENABLE_FAILED)]);
                    });
            }
            else {
                asyncResult.resolve();
            }

            return asyncResult;
        }

        /**
         * Disable barcode scanner device for scan.
         * @returns {IVoidAsyncResult} The async result.
         */
        public disableAsync(): IVoidAsyncResult {
            if (this._barcodeScanner) {
                this._barcodeScanner.disableBarcodeScannerAsync();
                this._barcodeScanner.onbarcodereceived = null;
            }

            return VoidAsyncResult.createResolved();
        }
    }
}
