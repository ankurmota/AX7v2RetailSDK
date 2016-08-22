/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="Utils.ts" />

module Contoso.Retail.Ecommerce.Sdk.Controls {
    /**
     * Represents the result of asynchronous calls.
     */
    export interface IAsyncResult<T> {
        /**
         * This function should be called when an async call succeeds.
         *
         * @param {(result: T) => void} callback The callback function.
         */
        done(callback: (result: T) => void): IAsyncResult<T>;

        /**
         * This function should be called when an error happens on an async call.
         *
         * @param {(error: any[]) => void} error The error callback function.
         */
        fail(callback: (error: any[]) => void): IAsyncResult<T>;
    }

    /**
     * Provides a simple generic implementation for IAsyncResult interface.
     */
    export class AsyncResult<T> implements IAsyncResult<T> {
        private _result: T;
        private _errors: any[];

        private _succeded: boolean;
        private _failed: boolean;

        private _callerContext: any;
        private _successCallbacks: Array<(result: T) => void>;
        private _errorCallbacks: Array<(errors: any[]) => void>;

        /**
         * Creates a new instance of AsyncResult<T> class.
         *
         * @param {any} callerContext The caller context for the done and fail callbacks.
         */
        constructor() {
            this._callerContext = this;
            this._succeded = false;
            this._failed = false;
            this._successCallbacks = [];
            this._errorCallbacks = [];
        }

        /**
         * Resolves the async call with the given result.
         *
         * @param {T} result The result of the async callback.
         */
        public resolve(result: T) {
            this._succeded = true;
            this._result = result;

            FunctionQueueHelper.callFunctions(this._successCallbacks, this._callerContext, this._result);
        }

        /**
         * Rejects the async call with the given error.
         *
         * @param {any[]} errors The error collection for the async callback.
         */
        public reject(errors: any[]) {
            this._failed = true;
            this._errors = errors;

            FunctionQueueHelper.callFunctions(this._errorCallbacks, this._callerContext, this._errors);
        }

        /**
         * This function should be called when an async call succeeds.
         *
         * @param {(result: T) => void} callback The callback function.
         */
        public done(callback: (result: T) => void): IAsyncResult<T> {
            if (this._succeded && callback) {
                callback.call(this._callerContext, this._result);
            } else {
                FunctionQueueHelper.queueFunction(this._successCallbacks, callback);
            }

            return this;
        }

        /**
         * This function should be called when an error happens on an async call.
         *
         * @param {(error: any[]) => void} error The error callback function.
         */
        public fail(callback: (errors: any[]) => void): IAsyncResult<T> {
            if (this._failed && callback) {
                callback.call(this._callerContext, this._errors);
            } else {
                FunctionQueueHelper.queueFunction(this._errorCallbacks, callback);
            }

            return this;
        }
    }

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
            if (!Contoso.Retail.Ecommerce.Utils.hasElements(functionQueue)) {
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
            if (!Contoso.Retail.Ecommerce.Utils.isNullOrUndefined(callback)) {
                functionQueue.push(callback);
            }
        }
    }

} 