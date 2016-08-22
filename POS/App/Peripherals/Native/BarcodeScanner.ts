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

    type BarcodeScannerSuccess = (scanner: Windows.Devices.PointOfService.BarcodeScanner) => void;
    type BarcodeScannerFailure = (error: Error) => void;

    export class BarcodeScanner implements IBarcodeScanner, IInitializable {

        private _barcodeScanner: Windows.Devices.PointOfService.BarcodeScanner;
        private _claimedBarcodeScanner: Windows.Devices.PointOfService.ClaimedBarcodeScanner;
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
            var asyncResult: VoidAsyncResult = new VoidAsyncResult();
            var self = this;

            var success: BarcodeScannerSuccess = (scanner: Windows.Devices.PointOfService.BarcodeScanner): void => {
                if (scanner != null) {
                    self._barcodeScanner = scanner;
                }
                asyncResult.resolve();
            };

            var failure: BarcodeScannerFailure = (error: Error) => {
                RetailLogger.peripheralsBarcodeScannerEnableFailed(error.message);
                asyncResult.reject(null);
            };

            try {
                if (StringExtensions.isNullOrWhitespace(this._deviceId)) {
                    Windows.Devices.PointOfService.BarcodeScanner.getDefaultAsync().then(success, failure);
                } else {
                    Windows.Devices.PointOfService.BarcodeScanner.fromIdAsync(this._deviceId).then(success, failure);
                }
            } catch (error) {
                failure(error);
            }

            return asyncResult;
        }

        /**
         * Enable barcode scanner device.
         *
         * @param {(barcode: string) => void} scannerMsgEventHandler The msg handler.
         * @returns {IVoidAsyncResult} The async result.
         */
        public enableAsync(scannerMsgEventHandler: (barcode: string) => void): IVoidAsyncResult {
            return VoidAsyncResult.retryOnFailure(
                () => this.enableAsyncInternal(scannerMsgEventHandler),
                () => this.initializeAsync());
        }

        private enableAsyncInternal(scannerMsgEventHandler: (barcode: string) => void): IVoidAsyncResult {
            var asyncResult = new VoidAsyncResult();

            if (this._barcodeScanner != null) {
                var self = this;

                this.disableAsync();
                this._barcodeScanner.claimScannerAsync()
                    .done((claimedScanner: Windows.Devices.PointOfService.ClaimedBarcodeScanner) => {
                        if (claimedScanner) {
                            this._claimedBarcodeScanner = claimedScanner;
                            claimedScanner.isDecodeDataEnabled = true;
                            claimedScanner.ondatareceived = ((args) => {
                                if (scannerMsgEventHandler) {
                                    scannerMsgEventHandler(
                                        Windows.Storage.Streams.DataReader.fromBuffer(args.report.scanDataLabel).readString(args.report.scanDataLabel.length).replace(/(\r\n|\n|\r)/gm, ""));
                                }
                            });

                            claimedScanner.enableAsync().done(() => {
                                asyncResult.resolve();
                            }, (e) => {
                                RetailLogger.peripheralsBarcodeScannerEnableFailed(e.message);
                                asyncResult.reject([new Proxy.Entities.Error(ErrorTypeEnum.PERIPHERALS_BARCODE_SCANNER_ENABLE_FAILED)]);
                            });
                        } else {
                            asyncResult.reject([new Proxy.Entities.Error(ErrorTypeEnum.PERIPHERALS_BARCODE_SCANNER_ENABLE_FAILED)]);
                        }
                    }, (e) => {
                        asyncResult.reject([new Proxy.Entities.Error(ErrorTypeEnum.PERIPHERALS_BARCODE_SCANNER_ENABLE_FAILED)]);
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
            if (this._barcodeScanner && this._claimedBarcodeScanner) {
                this._claimedBarcodeScanner.close();
                this._claimedBarcodeScanner = null;
            }

            return VoidAsyncResult.createResolved();
        }
    }
}