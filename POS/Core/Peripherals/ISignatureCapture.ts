/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/Peripherals.ts'/>
///<reference path='../IAsyncResult.ts'/>

module Commerce.Peripherals {
    "use strict";

    export interface ISignatureCapture {

        /**
         * Check if device is available (has already been opened).
         */
        isActive: boolean;

        /**
         * Opens the signature capture device for use.
         *
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        openDevice(callerContext?: any): IVoidAsyncResult;

        /**
         * Closes the signature capture device.
         *
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        closeDevice(callerContext?: any): IVoidAsyncResult;

        /**
         * Gets the signature.
         *
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<string>} The async result with the signature points as a byte array base 64 string.
         */
        getSignature(callerContext?: any): IAsyncResult<string>;

        /**
         * Cancels getting the signature.
         *
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        cancelSignature(callerContext?: any): IVoidAsyncResult;
    }
}