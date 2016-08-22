/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='IBarcodeScanner.ts'/>

module Commerce.Peripherals {
    "use strict";

    export class KeyboardBarcodeScanParser implements Peripherals.IBarcodeScanner {
        private _scanBuffer: string = "";
        private _keystrokeLogTimerID: number;
        private _readerStreamTimerID: number;
        private _isScanEnabled: boolean = false;
        private _scannerMsgEventHandler: any;
        private _keyboardEventHandlerPointer: (e: KeyboardEvent) => boolean = this.keyboardEventHandler.bind(this);

        /**
         * Enable barcode scanner device.
         * @param {(barcode: string) => void} scannerMsgEventHandler The msg handler.
         * @returns {IVoidAsyncResult} The async result.
         */
        public enableAsync(scannerMsgEventHandler: (barcode: string) => void): IVoidAsyncResult {
            var result: VoidAsyncResult = new VoidAsyncResult();

            if (ObjectExtensions.isFunction(scannerMsgEventHandler)) {

                this._scannerMsgEventHandler = scannerMsgEventHandler;

                if (!this._isScanEnabled) {
                    KeyboardPressEventAggregator.addListener(this._keyboardEventHandlerPointer);
                    this._isScanEnabled = true;
                }
            }

            result.resolve();
            return result;
        }

        /**
         * Disable barcode scanner device for scan.
         * @returns {IVoidAsyncResult} The async result.
         */
        disableAsync(): IVoidAsyncResult {
            if (this._isScanEnabled) {
                KeyboardPressEventAggregator.removeListner(this._keyboardEventHandlerPointer);
                this._isScanEnabled = false;
            }

            return VoidAsyncResult.createResolved();
        }

        private keyboardEventHandler(e: KeyboardEvent): boolean {
            var pressedChar: string = String.fromCharCode(e.keyCode);
            // We will only try to handle keyboard barcode scans if either we are not any input field
            // or the input field allows this explicitly.
            if (!KeyboardPressEventAggregator.isInputField(e.srcElement) || this.isBarcodeScannerEnabled(<HTMLElement>e.srcElement)) {
                if (this._scanBuffer === "") {
                    // Validate scan in 1/8 second.
                    this._readerStreamTimerID = setTimeout(() => this.validateBarcodeReaderStream(), 125);
                    // Scan should complete in 2 seconds else we invalidate buffer.
                    this._keystrokeLogTimerID = setTimeout(() => this._scanBuffer = "", 2000);
                }

                if (this._scanBuffer.length > 0 && e.keyCode === 13) { // If enter (key = 13) has been pressed.
                    this.fireEvent();
                    return false;
                } else {
                    this._scanBuffer += pressedChar;
                }
            }

            return true;
        }

        private validateBarcodeReaderStream(): void {
            // Must get at least 4 chars in 1/8 second to be considered as non-human meaningful interaction.
            if (this._scanBuffer.length < 4) {
                this._scanBuffer = "";
            }
        }

        private fireEvent(): void {
            if (this._scanBuffer.length > 0) {
                clearInterval(this._keystrokeLogTimerID);
                clearInterval(this._readerStreamTimerID);
                var barcode: string = this._scanBuffer;
                this._scanBuffer = "";

                this._scannerMsgEventHandler.call(this, barcode);
            }
        }

        private isBarcodeScannerEnabled(element: HTMLElement): boolean {
            var dataset: any = element.dataset || {};
            return StringExtensions.compare(dataset.axBarcodeScannerEnabled, "true", true) === 0;
        }
    }
}
