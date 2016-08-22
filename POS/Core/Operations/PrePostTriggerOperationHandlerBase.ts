/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='OperationHandlerBase.ts' />

module Commerce.Operations {
    "use strict";

    /**
     * Operation Handler base for operations that have a pre and/or post trigger.
     */
    export class PrePostTriggerOperationHandlerBase extends OperationHandlerBase {
        /**
         * Executes the pre-trigger for the operation.
         * @param {IOperationOptions} options The operation options.
         * @return {IAsyncResult<ICancelableResult>} The result of the pre-trigger execution.
         */
        protected executePreTrigger(options: IOperationOptions): IAsyncResult<ICancelableResult> {
            return AsyncResult.createResolved<ICancelableResult>({ canceled: false });
        }

        /**
         * Executes the post-trigger for the operation.
         * @param {IOperationOptions} options The operation options.
         * @param {IOperationResult} result The result of the operation.
         * @return {IVoidAsyncResult} The result of the post-trigger execution.
         */
        protected executePostTrigger(options: IOperationOptions, result: IOperationResult): IVoidAsyncResult {
            return VoidAsyncResult.createResolved();
        }

        /**
         * Sanitizes the options provided to the operation.
         * @param {IOperationOptions} options The provided options.
         * @return {IOperationOptions} The sanitized options.
         */
        protected sanitizeOptions(options: IOperationOptions): IOperationOptions {
            return options;
        }

        /**
         * Executes the internal operation logic. 
         * @param {IOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The result of the operation execution.
         */
        protected executeInternal(options: IOperationOptions): IAsyncResult<IOperationResult> {
            throw "executeInternal not implemented.";
        }

        /**
         * Executes the operation workflow, including pre and post triggers.
         * @param {IOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The result of the operation workflow.
         */
        public execute(options: IOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = this.sanitizeOptions(options);

            return Triggers.TriggerHelper.executeTriggerWorkflowAsync<IOperationResult>(
                (): IAsyncResult<ICancelableResult> => {
                    return this.executePreTrigger(options);
                },
                (): IAsyncResult<IOperationResult> => {
                    return this.executeInternal(options);
                },
                (result: IOperationResult): IVoidAsyncResult => {
                    return this.executePostTrigger(options, result);
                }).map((value: ICancelableDataResult<IOperationResult>): IOperationResult => {
                    return value.data || { canceled: value.canceled };
                });
        }
    }
} 