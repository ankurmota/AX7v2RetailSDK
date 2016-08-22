/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='Exceptions/ProxyError.ts'/>
///<reference path='FunctionQueueHelper.ts'/>

module Commerce.Proxy {
    "use strict";

    /**
     * Provides a simple generic implementation for IAsyncResult interface.
     */
    export class AsyncResult<T> implements IAsyncResult<T> {
        private _result: T;
        private _errors: ProxyError[];

        private _succeded: boolean;
        private _failed: boolean;

        private _callerContext: any;
        private _successCallbacks: Array<(result: T) => void>;
        private _errorCallbacks: Array<(errors: ProxyError[]) => void>;

        /**
         * Creates a new instance of AsyncResult<T> class.
         *
         * @param {any} callerContext The caller context for the done and fail callbacks.
         */
        constructor(callerContext?: any) {
            this._callerContext = callerContext;
            this.clear();
        }

        /**
         * Clears the async result.
         */
        public clear() {
            this._succeded = false;
            this._failed = false;
            this._result = undefined;
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
            this._errorCallbacks = [];

            FunctionQueueHelper.callFunctions(this._successCallbacks, this._callerContext, this._result);
        }

        /**
         * Rejects the async call with the given error.
         *
         * @param {ProxyError[]} errors The error collection for the async callback.
         */
        public reject(errors: ProxyError[]) {
            this._failed = true;
            this._errors = errors;
            this._successCallbacks = [];

            FunctionQueueHelper.callFunctions(this._errorCallbacks, this._callerContext, this._errors);
        }

        /**
         * Resolves or rejects the async call with result from given async result.
         *
         * @param {IAsyncResult<T>} asyncResult The async callback.
         */
        public resolveOrRejectOn(asyncResult: IAsyncResult<T>) {
            asyncResult
                .done((result) => { this.resolve(result); })
                .fail((errors) => { this.reject(errors); });
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
         * @param {(error: ProxyError[]) => void} error The error callback function.
         */
        public fail(callback: (errors: ProxyError[]) => void): IAsyncResult<T> {
            if (this._failed && callback) {
                callback.call(this._callerContext, this._errors);
            } else {
                FunctionQueueHelper.queueFunction(this._errorCallbacks, callback);
            }

            return this;
        }

        /**
         * Calls a defined callback function on the result element, and returns a new async result with the mapped result.
         *
         * @param {(value: T) => U} mapFunction The function used to map the async result result from IAsyncResult<T> to IAsyncResult<U>.
         * @return {IAsyncResult<U>} The mapped async result.
         */
        public map<U>(mapFunction: (value: T) => U): IAsyncResult<U> {
            if (!mapFunction) {
                throw "The map function is invalid.";
            }

            var asyncResult = new AsyncResult<U>(null);
            this.done(() => { asyncResult.resolve(mapFunction(this._result)); })
                .fail((errors) => { asyncResult.reject(errors); });

            return asyncResult;
        }

        /**
         * Creates a resolved AsyncResult with an optional result.
         *
         * @param {T} [result] The optional result to resolve the AsyncResult with.
         * @return {AsyncResult<T>} The resolved async result.
         */
        public static createResolved<T>(result?: T): AsyncResult<T> {
            var asyncResult = new AsyncResult<T>(null);
            asyncResult.resolve(result);
            return asyncResult;
        }

        /**
         * Creates an rejected AsyncResult with an optional error collection.
         *
         * @param {T} [result] The optional result to resolve the AsyncResult with.
         * @return {AsyncResult<T>} The resolved async result.
         */
        public static createRejected<T>(errors?: ProxyError[]): AsyncResult<T> {
            var asyncResult = new AsyncResult<T>(null);
            asyncResult.reject(errors || []);
            return asyncResult;
        }

        /**
         * Creates a async result that resolves or rejects when all of the given async results are finished, i.e. when they either resolve or reject.
         *
         * @param {IAsyncResult<T>[]} asyncResults The list of async results to join.
         * @return {IVoidAsyncResult} The async result that rejects or resolves only when all underlying async results finish executing.
         */
        public static join<T>(asyncResults: IAsyncResult<T>[]): IVoidAsyncResult {
            if (!ArrayExtensions.hasElements(asyncResults)) {
                return AsyncResult.createResolved();
            }

            // filter out null or undefined async results
            asyncResults = asyncResults.filter(a => !ObjectExtensions.isNullOrUndefined(a));
            return new CompoundAsyncResult(asyncResults);
        }
    }

    /**
     * Represents a null result.
     */
    export interface NullResult {
    }

    /**
     * Provides a simple generic implementation for IVoidAsyncResult interface.
     */
    export class VoidAsyncResult extends AsyncResult<NullResult> implements IVoidAsyncResult {
        /**
         * Creates a new instance of AsyncResult<T> class.
         *
         * @param {any} callerContext The caller context for the done and fail callbacks.
         */
        constructor(callerContext?: any) {
            super(callerContext);
        }

        /**
         * Resolves the async call.
         */
        public resolve() {
            super.resolve(null);
        }

        /**
         * This function should be called when an async call succeeds.
         *
         * @param {(result: T) => void} callback The callback function.
         */
        public done(callback: () => void): IVoidAsyncResult {
            return super.done(callback);
        }

        /**
         * Calls a defined callback function and returns a new async result with the mapped result.
         *
         * @param {() => U} mapFunction The function used to map the async result result from IAsyncResult<T> to IAsyncResult<U>.
         * @return {IAsyncResult<U>} The mapped async result.
         */
        public map<U>(mapFunction: () => U): IAsyncResult<U> {
            return super.map(mapFunction);
        }
    }

    /**
     * Implements an async result that resolves or rejects when all underlying async results finish executing.
     */
    class CompoundAsyncResult extends VoidAsyncResult {

        private _allErrors: ProxyError[];

        private _asyncResultsCount: number;
        private _succeedCount: number;
        private _failedCount: number;

        constructor(asyncResults: IAsyncResult<any>[]) {
            super(null);

            this._asyncResultsCount = asyncResults.length;
            this._succeedCount = 0;
            this._failedCount = 0;
            this._allErrors = [];

            this.attachToCallbacks(asyncResults);
        }

        /**
         * Attaches to done and fail callbacks of all async results.
         *
         * @param {IAsyncResult<any>[]} asyncResults The async results collection to attach to.
         */
        private attachToCallbacks(asyncResults: IAsyncResult<any>[]) {
            asyncResults.forEach((asyncResult) => {
                asyncResult.done((result?) => {
                    this._succeedCount++;
                    this.resolveOrReject();
                });

                asyncResult.fail((errors) => {
                    this._failedCount++;
                    this._allErrors = this._allErrors.concat(errors);

                    this.resolveOrReject();
                });
            });
        }

        /**
         * Resolves or rejects the CompoundAsyncResult given that all the async results succeeded and/or failed.
         */
        private resolveOrReject() {
            // verify whether all succeeded and/or failed
            if ((this._succeedCount + this._failedCount) == this._asyncResultsCount) {
                if (this._failedCount > 0) {
                    // at least one failed
                    this.reject(this._allErrors);
                } else {
                    // all succeeded
                    this.resolve();
                }
            }
        }
    }

    /**
     * Provides a way to queue up async/sync calls.
     * The queue only executes when the run function is called.
     */
    export class AsyncQueue {

        private _asyncQueue: Array<() => IAsyncResult<any>>;
        private _asyncResult: AsyncResult<ICancelableResult>;
        private _canceled: boolean;

        /**
         * Creates an empty queue.
         */
        constructor() {
            this._asyncQueue = [];
            this._asyncResult = null;
            this._canceled = false;
        }

        /**
         * Queues the async call to be executed when accessing the result.
         *
         * @param {() => IAsyncResult<T>} asyncCall The async call to enqueue.
         * @return {AsyncQueue} A reference to the async queue for chaining.
         * @remarks The async call will not be queued if it null or undefined and/or if the async queue is already running.
         *          Async calls that do not return and IAsyncResult object are considered synchronous calls.
         */
        public enqueue<T>(asyncCall: () => IAsyncResult<T>): AsyncQueue {
            // prevents from modifying the queue while it is executing or has already executed
            if (!this._asyncResult) {
                FunctionQueueHelper.queueFunction(this._asyncQueue, asyncCall);
            }

            return this;
        }

        /**
         * Cancels the queue if the async result was canceled, i.e. the result is true.
         *
         * @param {IAsyncResult<T>} result The async result containing the value of whether it was canceled or not.
         * @return {IAsyncResult<T>} The async result passed as argument, for chaining.
         */
        public cancelOn<T extends ICancelableResult>(result: IAsyncResult<T>): IAsyncResult<T> {
            if (result) {
                result.done((cancelResult) => {
                    if (cancelResult && cancelResult.canceled) {
                        this.cancel();
                    }
                });
            }

            return result;
        }

        /**
         * Cancels queue execution and resolves the async result with a value indicating the queue was canceled.
         */
        public cancel() {
            this._canceled = true;
        }

        /**
         * Runs the queue and return the async result with whether or not the queue was canceled.
         *
         * @return {IAsyncResult<ICancelableResult>} The async result with whether or not the queue was canceled.
         * @remarks Calling this function multiple times only makes this queue run once.
         */
        public run(): IAsyncResult<ICancelableResult> {
            if (this._asyncResult) {
                return this._asyncResult;
            }

            this._asyncResult = new AsyncResult<ICancelableResult>(null);

            // executes the queue
            ObjectExtensions.forEachAsync(this._asyncQueue, (nextInQueue: () => IAsyncResult<any>, moveNext: Function) => {
                if (this._canceled) {
                    moveNext();
                    return;
                }

                var result = nextInQueue();
                if (result) {
                    result.done(() => { moveNext(); }).fail((errors) => {
                        this._asyncQueue = [];
                        this._asyncResult.reject(errors);
                    });
                } else {
                    moveNext();
                }
            }, () => {
                this._asyncQueue = [];
                this._asyncResult.resolve({ canceled: this._canceled });
            });

            return this._asyncResult;
        }
    }

    /**
     * Worker queue item.
     */
    interface WorkerQueueItem {
        asyncCall: () => IAsyncResult<any>;
        asyncResult: AsyncResult<any>;
    }

    /**
     * Encapsulates a worker queue that process async requests sequentially.
     */
    export class AsyncWorkerQueue {
        private _workerQueue: Array<WorkerQueueItem>;
        private _processing = false;

        constructor() {
            this._workerQueue = [];
        }

        /**
         * Queues the async call to be executed.
         *
         * @param {() => IAsyncResult<T>} asyncCall The async call to enqueue.
         * @return {IAsyncResult<T>} The async result.
         */
        public enqueue<T>(asyncCall: () => IAsyncResult<T>): IAsyncResult<T> {
            var asyncResult = new AsyncResult<T>(null);

            this._workerQueue.push({ asyncCall: asyncCall, asyncResult: asyncResult });

            if (!this._processing) {
                this.processQueue();
            }

            return asyncResult;
        }

        /**
         * Processes the queue.
         */
        private processQueue(): void {
            this._processing = true;
        }
    }
}