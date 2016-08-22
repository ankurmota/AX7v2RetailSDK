/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='Extensions/ObjectExtensions.ts'/>

module Commerce.Proxy {
    "use strict";

    /**
     * Helper class for function queue management.
     */
    export class FunctionQueueHelper {
        /**
         * Call all the functions on the queue with optional data and clears the queue.
         *
         * @param {Function[]} functionQueue The queue of functions to be called.
         * @param {any} callerContext The caller context for the callback.
         * @param {any} [data] Optional parameter to be passed as the function argument.
         */
        public static callFunctions(functionQueue: Function[], callerContext: any, data?: any): void {
            if (functionQueue === undefined || functionQueue === null || functionQueue.length == 0 ) {
                return;
            }

            for (var i = 0; i < functionQueue.length; i++) {
                functionQueue[i].call(callerContext, data);
            }

            // clears the queue after calling all callbacks
            functionQueue = [];
        }

        /**
         * Queues a function to be called later.
         *
         * @param {Function[]} functionQueue The queue of functions to be called.
         * @param {Function} callback The function to be queued.
         */
        public static queueFunction(functionQueue: Function[], callback: Function): void {
            if (!(callback === undefined || callback === null)) {
                functionQueue.push(callback);
            }
        }
    }
}