/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ITrigger.ts' />

module Commerce.Triggers {
    "use strict";

    export class TriggerHelper {
        /**
         * Executes a async workflow consisting of pre-trigger, business logic execution, post-trigger.
         * @param {() => IAsyncResult<ICancelableResult>} The pre-trigger execution function.
         * @param {() => IAsyncResult<T>} The business logic execution function.
         * @param {(result: T) => IVoidAsyncResult} The post-trigger execution function
         * @return {IAsyncResult<ICancelableDataREsult<T>>} The result of the trigger workflow execution.
         */
        public static executeTriggerWorkflowAsync<T>(
            preTrigger: () => IAsyncResult<ICancelableResult>,
            workflowDelegate: () => IAsyncResult<T>,
            postTrigger: (result: T) => IVoidAsyncResult): IAsyncResult<ICancelableDataResult<T>> {

            if (ObjectExtensions.isNullOrUndefined(workflowDelegate)) {
                throw "businessLogic parameter cannot be null or undefined";
            }

            var asyncQueue: AsyncQueue = new AsyncQueue();
            var businessLogicReturnValue: T = null;
            if (!ObjectExtensions.isNullOrUndefined(preTrigger)) {
                asyncQueue.enqueue((): IVoidAsyncResult => {
                    var preTriggerResult: IAsyncResult<ICancelableResult> = preTrigger();

                    return asyncQueue.cancelOn(preTriggerResult);
                });
            }

            asyncQueue.enqueue((): IVoidAsyncResult => {
                var executionResult: IAsyncResult<T> = workflowDelegate().done((result: T): void => {
                    businessLogicReturnValue = result;
                });

                return asyncQueue.cancelOn(<any>executionResult);
            });

            if (!ObjectExtensions.isNullOrUndefined(postTrigger)) {
                asyncQueue.enqueue((): IVoidAsyncResult => {
                    return postTrigger(businessLogicReturnValue);
                });
            }

            return asyncQueue.run().map((result: ICancelableResult): ICancelableDataResult<T> => {
                return { data: businessLogicReturnValue, canceled: result.canceled };
            });
        }
    }
} 