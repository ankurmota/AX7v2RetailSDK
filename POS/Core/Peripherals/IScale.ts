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

    export interface IScale {

        /**
         * Reads the weight value from scale.
         *
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<number>} The async result.
         */
        read(callerContext?: any): IAsyncResult<number>;
    }
}