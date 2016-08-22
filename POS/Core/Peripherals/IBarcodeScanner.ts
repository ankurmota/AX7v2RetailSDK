/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../IAsyncResult.ts'/>

module Commerce.Peripherals {
    "use strict";

    export interface IBarcodeScanner {

        /**
         * Enable barcode scanner device.
         * @param {(barcode: string) => void} scannerMsgEventHandler The msg handler.
         * @returns {IVoidAsyncResult} The async result.
         */
        enableAsync(scannerMsgEventHandler: (barcode: string) => void): IVoidAsyncResult;

        /**
         * Disable barcode scanner device for scan.
         * @returns {IVoidAsyncResult} The async result.
         */
        disableAsync(): IVoidAsyncResult;
    }
}