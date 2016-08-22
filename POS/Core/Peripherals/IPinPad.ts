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

    export interface IPinPad {

        /**
         * Check if device is available (has already been opened).
         */
        isActive: boolean;

        /**
         * Opens the pin pad device for use.
         *
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        openDevice(callerContext?: any): IVoidAsyncResult;

        /**
         * Closes the pin pad device.
         *
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        closeDevice(callerContext?: any): IVoidAsyncResult;

        /**
         * Gets the pin pad entry.
         *
         * @param {string} [cardNumber] The card number.
         * @param {number} [paymentAmount] The amount being paid.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Model.Entities.PinPadInfo>} The async result with information from the pin pad.
         */
        getPinEntry(cardNumber: string, paymentAmount: number, callerContext?: any): IAsyncResult<Model.Entities.PinPadInfo>;

        /**
         * Cancels the pin pad entry.
         *
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        cancelPinEntry(callerContext?: any): IVoidAsyncResult;
    }
}