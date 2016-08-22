/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../../SharedApp/Commerce.Core.d.ts'/>
///<reference path='WindowTimersExtensionBase.ts'/>

module Commerce.Host.CrossBrowser {
    "use strict";

    /**
     * window.setTimeout based WindowTimersExtension implementation.
     * @remarks Should be used as a fallback strategy, if other implementations are not supported.
     * This is the slowest available implementation.
     */
    export class SetTimeoutWindowTimersExtension extends WindowTimersExtensionBase {

        /**
         * Requests that a function be called when current or 
         * pending tasks are complete, such as events or screen updates.
         * @param {Function} [handler] The function to be called.
         * @param {any[]} [...args] Arguments to be passed to the function.
         * @return {number} A handle to the request.
         * @remarks This method is used to break up long running operations and run a callback function immediately.
         */
        public setImmediate(handler: Function, ...args: any[]): number {
            var handleId: number = this.addHandler(arguments);

            var appliedHandler: () => void = this.partialApply(this.executeHandler, handleId);
            window.setTimeout(appliedHandler, 0);

            return handleId;
        }
    }
}