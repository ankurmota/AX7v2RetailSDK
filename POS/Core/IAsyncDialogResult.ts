/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='Entities/Error.ts'/>
///<reference path='Utilities/FunctionQueueHelper.ts'/>
///<reference path='NotificationHandler.ts'/>

module Commerce {
    "use strict";

    /**
     * Represents the result of asynchronous calls on dialogs.
     */
    export interface IAsyncDialogResult<T> {
        /**
         * This function should be called when an async call succeeds.
         *
         * @param {DialogResult} dialogResult The dialog result.
         * @param {(result: T) => void} callback The callback function.
         */
        on(dialogResult: DialogResult, callback: (result: T) => void): IAsyncDialogResult<T>;

        /**
         * This function should be called when executing behavior on any result.
         *
         * @param {(result: T, dialogResult: DialogResult) => void} callback The callback function.
         */
        onAny(callback: (result: T, dialogResult: DialogResult) => void): IAsyncDialogResult<T>;

        /**
         * This function should be called when an error happens on an async call.
         *
         * @param {(error: Commerce.Model.Entities.Error[]) => void} error The error callback function.
         */
        onError(callback: (error: Commerce.Model.Entities.Error[]) => void): IAsyncDialogResult<T>;
    }

    /**
     * Provides a simple generic implementation for IAsyncDialogResult interface.
     */
    export class AsyncDialogResult<T> implements IAsyncDialogResult<T> {
        private _result: T;
        private _errors: Commerce.Model.Entities.Error[];

        private _succeded: { [key: string]: boolean };
        private _failed: boolean;

        private _callerContext: any;
        private _onDialogResultCallbacks: { [key: string]: Array<(result: T) => void>; };
        private _onErrorCallbacks: Array<(errors: Commerce.Model.Entities.Error[]) => void>;

        /**
         * Creates a new instance of AsyncResult<T> class.
         *
         * @param {any} callerContext The caller context for the done and fail callbacks.
         */
        constructor(callerContext: any) {
            this._callerContext = callerContext;
            this.clear();
        }

        /**
         * Clears the dialog result.
         */
        public clear() {
            this._failed = false;
            this._succeded = {};
            this._onDialogResultCallbacks = {};
            this._onErrorCallbacks = [];
            this._result = undefined;
        }

        /**
         * Resolves the async call with the given result.
         *
         * @param {DialogResult} dialogResult The dialog result.
         * @param {T} result The result of the async callback.
         */
        public resolve(dialogResult: DialogResult, result: T = null) {
            var key = DialogResult[dialogResult];
            this._succeded[key] = true;
            this._result = result;
            this._onErrorCallbacks = [];

            FunctionQueueHelper.callFunctions(this.getDialogResultCallbackQueue(key), this._callerContext, this._result, dialogResult);
        }

        /**
         * Rejects the async call with the given error.
         *
         * @param {Commerce.Model.Entities.Error[]} errors The error collection for the async callback.
         */
        public reject(errors: Commerce.Model.Entities.Error[]) {
            this._failed = true;
            this._errors = errors;
            this._onDialogResultCallbacks = {};

            FunctionQueueHelper.callFunctions(this._onErrorCallbacks, this._callerContext, this._errors);
        }

        /**
         * This function should be called when an async call succeeds.
         *
         * @param {DialogResult} dialogResult The dialog result.
         * @param {(result: T) => void} callback The callback function.
         */
        public on(dialogResult: DialogResult, callback: (result: T) => void): IAsyncDialogResult<T> {
            var key = DialogResult[dialogResult];

            if (this._succeded[key] && callback) {
                Commerce.Host.instance.timers.setImmediate(() => {
                    callback.call(this._callerContext, this._result, dialogResult);
                });
            } else {
                this.getDialogResultCallbackQueue(key).push(callback);
            }

            return this;
        }

        /**
         * This function should be called when executing behavior on any success result.
         *
         * @param {(result: T, dialogResult: DialogResult) => void} callback The callback function.
         */
        public onAny(callback: (result: T, dialogResult: DialogResult) => void): IAsyncDialogResult<T> {
            var key: string;
            var dialogResultValue: DialogResult;

            for (var dialogResult in DialogResult) {
                var resultKey: string = DialogResult[dialogResult];
                if (this._succeded[resultKey]) {
                    key = resultKey;
                    dialogResultValue = dialogResult;
                    break;
                }
            }

            if (!ObjectExtensions.isNullOrUndefined(key) && callback) {
                window.setImmediate(() => {
                    callback.call(this._callerContext, this._result, dialogResultValue);
                });
            } else {
                // Queue the callback to be called back from any result
                for (var dialogResult in DialogResult) {
                    key = DialogResult[dialogResult];
                    this.getDialogResultCallbackQueue(key).push(callback);
                }
            }

            return this;
        }

        /**
         * This function should be called when an error happens on an async call.
         *
         * @param {(error: Commerce.Model.Entities.Error[]) => void} error The error callback function.
         */
        public onError(callback: (errors: Commerce.Model.Entities.Error[]) => void): IAsyncDialogResult<T> {
            if (this._failed && callback) {
                Commerce.Host.instance.timers.setImmediate(() => {
                    callback.call(this._callerContext, this._errors);
                });
            } else {
                if (!this._onErrorCallbacks) {
                    this._onErrorCallbacks = [];
                }

                this._onErrorCallbacks.push(callback);
            }

            return this;
        }

        /**
         * Gets the dialog result callback queue for a key.
         *
         * @param {string} key The key.
         * @return {Function[]} The callback queue.
         */
        private getDialogResultCallbackQueue(key: string): Function[] {
            if (!this._onDialogResultCallbacks[key]) {
                this._onDialogResultCallbacks[key] = [];
            }

            return this._onDialogResultCallbacks[key];
        }
    }
}