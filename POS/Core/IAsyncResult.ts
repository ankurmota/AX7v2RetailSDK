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

module Commerce {
    "use strict";

    import Error = Model.Entities.Error;

    /**
     * Represents an async result state.
     */
    export enum AsyncResultStateEnum {
        Pending,
        Resolved,
        Rejected,
    }

    /**
     * Represents the result of asynchronous calls.
     */
    export interface IAsyncResult<T> {
        /**
         * This function should be called when an async call succeeds.
         * @param {(result: T) => void} callback The callback function.
         */
        done(callback: (result: T) => void): IAsyncResult<T>;

        /**
         * This function should be called when an error happens on an async call.
         * @param {(error: Error[]) => void} error The error callback function.
         */
        fail(callback: (error: Error[]) => void): IAsyncResult<T>;

        /**
         * This function should be called after succeed or error on any async call after done or fail is executed.
         * @param {(error: Commerce.Model.Entities.Error[]) => void} error The error callback function.
         */
        always(callback: () => void): IAsyncResult<T>;

        /**
         * Calls a defined callback function on the result element, and returns a new async result with the mapped result.
         * @param {(value: T) => U} mapFunction The function used to map the async result from IAsyncResult<T> to IAsyncResult<U>.
         * @return {IAsyncResult<U>} The mapped async result.
         */
        map<U>(mapFunction: (value: T) => U): IAsyncResult<U>;

        /**
         * Returns a current state of the result.
         * @return {AsyncResultStateEnum} The state.
         */
        state(): AsyncResultStateEnum;

        /**
         * Calls a defined callback function with the errors. The recovery function must return an async result, either resolved
         * or failed, if it was not able to recover from the errors.
         * @param {(errors: Error[]) => IAsyncResult<T>} recoveryFunction The function used to recover the failed async result.
         * @return {IAsyncResult<T>} The resolved async result.
         */
        recoverOnFailure(recoveryFunction: (errors: Error[]) => IAsyncResult<T>): IAsyncResult<T>;
    }

    // Enumeration for callback conditions.
    enum CallbackCondition { OnAlways, OnDone, OnFail }

    // alias for a dictionary of callbacks by result
    type FunctionConditionPair = { callback: Function; condition: CallbackCondition; };

    /**
     * Provides a simple generic implementation for IAsyncResult interface.
     */
    export class AsyncResult<T> implements IAsyncResult<T> {
        private _result: T;
        private _errors: Error[];

        private _state: AsyncResultStateEnum;
        private _callerContext: any;
        private _callbacks: FunctionConditionPair[];

        /**
         * Creates a new instance of AsyncResult<T> class.
         * @param {any} [callerContext] The caller context for the done and fail callbacks.
         */
        constructor(callerContext?: any) {
            this._callerContext = callerContext;
            this.clear();
        }

        /**
         * Creates a resolved AsyncResult with an optional result.
         * @param {T} [result] The optional result to resolve the AsyncResult with.
         * @return {IAsyncResult<T>} The resolved async result.
         */
        public static createResolved<T>(result?: T): IAsyncResult<T> {
            var asyncResult: AsyncResult<T> = new AsyncResult<T>();
            asyncResult.resolve(result);
            return asyncResult;
        }

        /**
         * Creates an rejected AsyncResult with an optional error collection.
         * @param {Error[]} [errors] The optional errors to reject the AsyncResult with.
         * @return {IAsyncResult<T>} The rejected async result.
         */
        public static createRejected<T>(errors?: Error[]): IAsyncResult<T> {
            var asyncResult: AsyncResult<T> = new AsyncResult<T>();
            asyncResult.reject(errors || []);
            return asyncResult;
        }

        /**
         * Returns a current state of the result.
         * @return {AsyncResultStateEnum} The state.
         */
        public state(): AsyncResultStateEnum {
            return this._state;
        }

        /**
         * Clears the async result.
         */
        public clear(): void {
            this._state = AsyncResultStateEnum.Pending;
            this._result = undefined;
            this._callbacks = [];
        }

        /**
         * Resolves the async call with the given result.
         * @param {T} result The result of the async callback.
         */
        public resolve(result: T): void {
            this._state = AsyncResultStateEnum.Resolved;
            this._result = result;

            var _notOnFailCallbacks: Function[] = this._callbacks.filter((c: FunctionConditionPair) => c.condition !== CallbackCondition.OnFail)
                .map((c: FunctionConditionPair) => c.callback);
            this._callbacks = [];

            FunctionQueueHelper.callFunctions(_notOnFailCallbacks, this._callerContext, this._result);
        }

        /**
         * Rejects the async call with the given error.
         * @param {Error[]} errors The error collection for the async callback.
         */
        public reject(errors: Error[]): void {
            this._state = AsyncResultStateEnum.Rejected;
            this._errors = errors;

            var _notOnDoneCallbacks: Function[] = this._callbacks.filter((c: FunctionConditionPair) => c.condition !== CallbackCondition.OnDone)
                .map((c: FunctionConditionPair) => c.callback);
            this._callbacks = [];

            FunctionQueueHelper.callFunctions(_notOnDoneCallbacks, this._callerContext, this._errors);
        }

        /**
         * Resolves or rejects the async call with result from given async result.
         * @param {IAsyncResult<T>} asyncResult The async callback.
         */
        public resolveOrRejectOn(asyncResult: IAsyncResult<T>): void {
            asyncResult
                .done((result: T): void => { this.resolve(result); })
                .fail((errors: Error[]): void => { this.reject(errors); });
        }

        /**
         * This function should be called when an async call succeeds.
         * @param {(result: T) => void} callback The callback function.
         */
        public done(callback: (result: T) => void): IAsyncResult<T> {
            if (this._state === AsyncResultStateEnum.Resolved && callback) {
                Commerce.Host.instance.timers.setImmediate(() => {
                    callback.call(this._callerContext, this._result);
                });
            } else if (!ObjectExtensions.isNullOrUndefined(callback)) {
                this._callbacks.push({ callback: callback, condition: CallbackCondition.OnDone });
            }

            return this;
        }

        /**
         * This function should be called when an error happens on an async call.
         * @param {(error: Error[]) => void} error The error callback function.
         */
        public fail(callback: (errors: Error[]) => void): IAsyncResult<T> {
            if (this._state === AsyncResultStateEnum.Rejected && callback) {
                Commerce.Host.instance.timers.setImmediate(() => {
                    callback.call(this._callerContext, this._errors);
                });
            } else if (!ObjectExtensions.isNullOrUndefined(callback)) {
                this._callbacks.push({ callback: callback, condition: CallbackCondition.OnFail });
            }

            return this;
        }

        /**
         * This function should be called after succeed or error on any async call after done or fail is executed.
         * @param {(error: Commerce.Model.Entities.Error[]) => void} error The error callback function.
         */
        public always(callback: () => void): IAsyncResult<T> {
            if (this._state !== AsyncResultStateEnum.Pending && callback) {
                Commerce.Host.instance.timers.setImmediate(() => {
                    callback.call(this._callerContext);
                });
            } else {
                this._callbacks.push({ callback: callback, condition: CallbackCondition.OnAlways });
            }

            return this;
        }

        /**
         * Calls a defined callback function on the result element, and returns a new async result with the mapped result.
         * @param {(value: T) => U} mapFunction The function used to map the async result from IAsyncResult<T> to IAsyncResult<U>.
         * @return {IAsyncResult<U>} The mapped async result.
         */
        public map<U>(mapFunction: (value: T) => U): IAsyncResult<U> {
            if (!mapFunction) {
                throw "The map function is invalid.";
            }

            var asyncResult: AsyncResult<U> = new AsyncResult<U>();
            this.done((): void => { asyncResult.resolve(mapFunction(this._result)); })
                .fail((errors: Error[]): void => { asyncResult.reject(errors); });

            return asyncResult;
        }

        /**
         * Calls a defined callback function with the errors. The recovery function must return an async result, either resolved
         * or failed, if it was not able to recover from the errors.
         * @param {(errors: Error[]) => IAsyncResult<T>} recoveryFunction The function used to recover the failed async result.
         * @return {IAsyncResult<T>} The resolved async result.
         */
        public recoverOnFailure(recoveryFunction: (errors: Error[]) => IAsyncResult<T>): IAsyncResult<T> {
            if (!recoveryFunction) {
                throw "The map function is invalid.";
            }

            var asyncResult: AsyncResult<T> = new AsyncResult<T>();
            this.done((result: T): void => { asyncResult.resolve(result); })
                .fail((errors: Error[]): void => {
                    var recoverAsyncResult: IAsyncResult<T> = recoveryFunction(errors);
                    if (recoverAsyncResult) {
                        asyncResult.resolveOrRejectOn(recoverAsyncResult);
                    } else {
                        asyncResult.reject(errors);
                    }
                });

            return asyncResult;
        }
    }

    /**
     * Represents a null result.
     */
    export interface INullResult {
    }

    /**
     * Represents the result of result-less asynchronous calls.
     */
    export interface IVoidAsyncResult extends IAsyncResult<INullResult> {
        /**
         * This function should be called when an async call succeeds.
         * @param {() => void} callback The callback function.
         */
        done(callback: () => void): IVoidAsyncResult;

        /**
         * This function should be called when an error happens on an async call.
         * @param {(errors: Error[]) => void} error The error callback function.
         */
        fail(callback: (errors: Error[]) => void): IVoidAsyncResult;

        /**
         * This function should be called after succeed or error on any async call after done or fail is executed.
         * @param {(error: Commerce.Model.Entities.Error[]) => void} error The error callback function.
         */
        always(callback: () => void): IVoidAsyncResult;

        /**
         * Calls a defined callback function and returns a new async result with the mapped result.
         * @param {() => U} mapFunction The function used to map the async result from IAsyncResult<T> to IAsyncResult<U>.
         * @return {IAsyncResult<U>} The mapped async result.
         */
        map<U>(mapFunction: () => U): IAsyncResult<U>;

        /**
         * Calls a defined callback function with the errors. The recovery function must return an async result, either resolved
         * or failed, if it was not able to recover from the errors.
         * @param {(errors: Error[]) => IVoidAsyncResult} recoveryFunction The function used to recover the failed async result.
         * @return {IVoidAsyncResult} The resolved async result.
         */
        recoverOnFailure(recoveryFunction: (errors: Error[]) => IVoidAsyncResult): IVoidAsyncResult;
    }

    /**
     * Provides a simple generic implementation for IVoidAsyncResult interface which defers logic to AsyncResult.
     */
    export class VoidAsyncResult extends AsyncResult<INullResult> implements IVoidAsyncResult {
        private static _resolvedAsyncResult: IVoidAsyncResult;

        /**
         * Creates a new instance of AsyncResult<T> class.
         * @param {any} [callerContext] The caller context for the done and fail callbacks.
         */
        constructor(callerContext?: any) {
            super();
        }

        /**
         * Creates a resolved IVoidAsyncResult.
         * @return {IVoidAsyncResult} The resolved async result.
         */
        public static createResolved(): IVoidAsyncResult {
            if (VoidAsyncResult._resolvedAsyncResult) {
                return VoidAsyncResult._resolvedAsyncResult;
            }

            VoidAsyncResult._resolvedAsyncResult = AsyncResult.createResolved<INullResult>();
            return VoidAsyncResult._resolvedAsyncResult;
        }

        /**
         * Creates an rejected IVoidAsyncResult with an optional error collection.
         * @param {Error[]} [errors] The optional errors to reject the AsyncResult with.
         * @return {IVoidAsyncResult} The rejected async result.
         */
        public static createRejected(errors?: Error[]): IVoidAsyncResult {
            return AsyncResult.createRejected<INullResult>(errors);
        }

        /**
         * Creates a async result that resolves or rejects when all of the given async results are finished, i.e. when they either resolve or reject.
         * @param {IAsyncResult<any>[]} asyncResults The list of async results to join.
         * @return {IVoidAsyncResult} The async result that rejects or resolves only when all underlying async results finish executing.
         */
        public static join(asyncResults: IAsyncResult<any>[]): IVoidAsyncResult {
            if (!ArrayExtensions.hasElements(asyncResults)) {
                return AsyncResult.createResolved();
            }

            // filter out null or undefined async results
            asyncResults = asyncResults.filter((a: IAsyncResult<any>) => !ObjectExtensions.isNullOrUndefined(a));
            return new CompoundAsyncResult(asyncResults);
        }

        /**
         * Calls the provided action and if the call fails then retries the action after executing the recovery action.
         *
         * @param {() => IVoidAsyncResult} asyncCall The function to call.
         * @param {(errors: Error[]) => IVoidAsyncResult} asyncRecoveryCall The function used to recover the failed async result.
         * @return {IVoidAsyncResult} The async result.
         */
        public static retryOnFailure<T>(asyncCall: () => IVoidAsyncResult, asyncRecoveryCall: (errors: Error[]) => IVoidAsyncResult): IVoidAsyncResult {
            return asyncCall()
                .recoverOnFailure((errors: Proxy.Entities.Error[]) => {
                    var asyncQueue: AsyncQueue = new AsyncQueue();

                    asyncQueue.enqueue((): IVoidAsyncResult => {
                        if (asyncRecoveryCall) {
                            return asyncRecoveryCall(errors);
                        }

                        return VoidAsyncResult.createRejected(errors);
                    }).enqueue((): IVoidAsyncResult => {
                        return asyncCall();
                    });

                    return asyncQueue.run();
                });
        }

        /**
         * Resolves the async call.
         */
        public resolve(): void {
            super.resolve(null);
        }

        /**
         * This function should be called when an async call succeeds.
         * @param {(result: T) => void} callback The callback function.
         */
        public done(callback: () => void): IVoidAsyncResult {
            return super.done(callback);
        }

        /**
         * This function should be called when an error happens on an async call.
         * @param {(errors: Error[]) => void} error The error callback function.
         */
        public fail(callback: (errors: Error[]) => void): IVoidAsyncResult {
            return super.fail(callback);
        }

        /**
         * This function should be called after succeed or error on any async call after done or fail is executed.
         * @param {(error: Commerce.Model.Entities.Error[]) => void} error The error callback function.
         */
        public always(callback: () => void): IVoidAsyncResult {
            return super.always(callback);
        }

        /**
         * Calls a defined callback function and returns a new async result with the mapped result.
         * @param {() => U} mapFunction The function used to map the async result from IAsyncResult<T> to IAsyncResult<U>.
         * @return {IAsyncResult<U>} The mapped async result.
         */
        public map<U>(mapFunction: () => U): IAsyncResult<U> {
            return super.map(mapFunction);
        }

        /**
         * Calls a defined callback function with the errors. The recovery function must return an async result, either resolved
         * or failed, if it was not able to recover from the errors.
         * @param {(errors: Error[]) => IVoidAsyncResult} recoveryFunction The function used to recover the failed async result.
         * @return {IVoidAsyncResult} The resolved async result.
         */
        public recoverOnFailure(recoveryFunction: (errors: Error[]) => IVoidAsyncResult): IVoidAsyncResult {
            return super.recoverOnFailure(recoveryFunction);
        }
    }

    /**
     * Implements an async result that resolves or rejects when all underlying async results finish executing.
     */
    class CompoundAsyncResult extends VoidAsyncResult {

        private _allErrors: Error[];

        private _asyncResultsCount: number;
        private _succeedCount: number;
        private _failedCount: number;

        constructor(asyncResults: IAsyncResult<any>[]) {
            super();

            this._asyncResultsCount = asyncResults.length;
            this._succeedCount = 0;
            this._failedCount = 0;
            this._allErrors = [];

            this.attachToCallbacks(asyncResults);
        }

        /**
         * Attaches to done and fail callbacks of all async results.
         * @param {IAsyncResult<any>[]} asyncResults The async results collection to attach to.
         */
        private attachToCallbacks(asyncResults: IAsyncResult<any>[]): void {
            asyncResults.forEach((asyncResult: IAsyncResult<any>): void => {
                asyncResult.done((result?: any): void => {
                    this._succeedCount++;
                    this.resolveOrReject();
                });

                asyncResult.fail((errors: Error[]): void => {
                    this._failedCount++;
                    this._allErrors = this._allErrors.concat(errors);

                    this.resolveOrReject();
                });
            });
        }

        /**
         * Resolves or rejects the CompoundAsyncResult given that all the async results succeeded and/or failed.
         */
        private resolveOrReject(): void {
            // verify whether all succeeded and/or failed
            if ((this._succeedCount + this._failedCount) === this._asyncResultsCount) {
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
     * Provides an interface for a cancelable result.
     */
    export interface ICancelableResult {
        canceled: boolean;
    }

    /**
     * Provides an interface for a cancelable result with data.
     */
    export interface ICancelableDataResult<T> extends ICancelableResult {
        data: T;
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
         * @param {() => IAsyncResult<T>} asyncCall The async call to enqueue.
         * @return {AsyncQueue} A reference to the async queue for chaining.
         * @remarks The async call will not be queued if it null or undefined and/or if the async queue is already running.
         *          Async calls that do not return and IAsyncResult object are considered synchronous calls.
         */
        public enqueue<T>(asyncCall: () => IAsyncResult<T>): AsyncQueue {
            // prevents from modifying the queue while it is executing or has already executed
            if (!this._asyncResult && !ObjectExtensions.isNullOrUndefined(asyncCall)) {
                this._asyncQueue.push(asyncCall);
            }

            return this;
        }

        /**
         * Cancels the queue if the async result was canceled, i.e. the result is true.
         * @param {IAsyncResult<T>} result The async result containing the value of whether it was canceled or not.
         * @return {IAsyncResult<T>} The async result passed as argument, for chaining.
         */
        public cancelOn<T extends ICancelableResult>(result: IAsyncResult<T>): IAsyncResult<T> {
            if (result) {
                result.done((cancelResult: T) => {
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
        public cancel(): void {
            this._canceled = true;
        }

        /**
         * Runs the queue and return the async result with whether or not the queue was canceled.
         * @return {IAsyncResult<ICancelableResult>} The async result with whether or not the queue was canceled.
         * @remarks Calling this function multiple times only makes this queue run once.
         */
        public run(): IAsyncResult<ICancelableResult> {
            if (this._asyncResult) {
                return this._asyncResult;
            }

            this._asyncResult = new AsyncResult<ICancelableResult>();

            // executes the queue
            ObjectExtensions.forEachAsync(this._asyncQueue, (nextInQueue: () => IAsyncResult<any>, moveNext: Function): void => {
                if (this._canceled) {
                    moveNext();
                    return;
                }

                var result: IAsyncResult<any> = nextInQueue();
                if (result) {
                    result.done((): void => { moveNext(); })
                        .fail((errors: Error[]): void => {
                            this._asyncQueue = [];
                            this._asyncResult.reject(errors);
                        });
                } else {
                    moveNext();
                }
            }, (): void => {
                this._asyncQueue = [];
                this._asyncResult.resolve({ canceled: this._canceled });
            });

            return this._asyncResult;
        }
    }

    /**
     * Worker queue item.
     */
    interface IWorkerQueueItem {
        asyncCall: () => IAsyncResult<any>;
        asyncResult: AsyncResult<any>;
    }

    /**
     * Encapsulates a worker queue that process async requests sequentially.
     */
    export class AsyncWorkerQueue {
        private _workerQueue: Array<IWorkerQueueItem>;
        private _processing: boolean = false;

        constructor() {
            this._workerQueue = [];
        }

        /**
         * Queues the async call to be executed.
         * @param {() => IAsyncResult<T>} asyncCall The async call to enqueue.
         * @return {IAsyncResult<T>} The async result.
         */
        public enqueue<T>(asyncCall: () => IAsyncResult<T>): IAsyncResult<T> {
            var asyncResult: AsyncResult<T> = new AsyncResult<T>();

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

            Commerce.Host.instance.timers.setImmediate(() => {
                var queueItem: IWorkerQueueItem = this._workerQueue.shift();

                if (queueItem) {
                    var asyncResult: IAsyncResult<any> = queueItem.asyncCall();

                    asyncResult
                        .done((result: any): void => {
                            this.processQueue();
                            queueItem.asyncResult.resolve(result);
                        }).fail((errors: Error[]) => {
                            this.processQueue();
                            queueItem.asyncResult.reject(errors);
                        });
                } else {
                    this._processing = false;
                }
            });
        }
    }
}