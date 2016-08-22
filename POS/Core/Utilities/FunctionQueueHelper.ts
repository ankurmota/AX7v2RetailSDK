/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Extensions/ObjectExtensions.ts'/>

module Commerce {
    "use strict";

    /**
     * Helper class for function queue management.
     */
    export class FunctionQueueHelper {
        /**
         * Call all the functions on the queue with optional data and clears the queue.
         * @param {Function[]} functionQueue The queue of functions to be called.
         * @param {any} callerContext The caller context for the callback.
         * @param {any} ...data Parameter to be passed as the function arguments.
         */
        public static callFunctions(functionQueue: Function[], callerContext: any, ...data: any[]): void {
            if (!ArrayExtensions.hasElements(functionQueue)) {
                return;
            }

            Commerce.Host.instance.timers.setImmediate(FunctionQueueHelper.callFunctionsInternal, functionQueue, callerContext, data);
        }

        /**
         * Internal implementation that calls all the functions on the queue with optional data and clears the queue.
         * @param {Function[]} functions The queue of functions to be called.
         * @param {any} callerContext The caller context for the callback.
         * @param {any[]} data Parameter set to be passed as the function arguments.
         */
        private static callFunctionsInternal(functions: Function[], callerContext: any, data: any[]): void {
            while (functions.length > 0) {
                var callback: Function = functions.shift();
                callback.apply(callerContext, data);
            }
        }
    }
}