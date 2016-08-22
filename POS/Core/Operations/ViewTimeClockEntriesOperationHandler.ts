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
     * Options passed to the ViewTimeClockEntries operation.
     */
    export interface IViewTimeClockEntriesOperationOptions extends IOperationOptions {
    }

    /**
     * Handler for the ViewTimeClockEntries operation.
     */
    export class ViewTimeClockEntriesOperationHandler extends OperationHandlerBase {
        /**
         * Executes the ViewTimeClockEntries operation.
         *
         * @param {IViewTimeClockEntriesOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IViewTimeClockEntriesOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || {};

            var asyncResult: VoidAsyncResult = new VoidAsyncResult();

            if (ApplicationContext.Instance.deviceConfiguration.EnableTimeRegistration) {
                Commerce.ViewModelAdapter.navigate("TimeClockManagerView");
                asyncResult.resolve();
            } else {
                RetailLogger.operationTimeClockNotEnabled();
                var errors: Proxy.Entities.Error[] = [new Proxy.Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_TIME_CLOCK_DISABLED)];
                asyncResult.reject(errors);
            }

            return asyncResult;
        }
    }
}