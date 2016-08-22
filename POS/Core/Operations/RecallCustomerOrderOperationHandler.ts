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
     * Options passed to the RecallCustomerOrder operation.
     */
    export interface IRecallCustomerOrderOperationOptions extends IOperationOptions {
        searchCriteria?: Model.Entities.SalesOrderSearchCriteria;
    }

    /**
     * Handler for the RecallCustomerOrder operation.
     */
    export class RecallCustomerOrderOperationHandler extends OperationHandlerBase {
        /**
         * Executes the RecallCustomerOrder operation.
         * @param {IRecallCustomerOrderOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IRecallCustomerOrderOperationOptions): IAsyncResult<IOperationResult> {
            options = options || {};

            ViewModelAdapter.navigate("SearchOrdersView", options);
            return AsyncResult.createResolved<IOperationResult>({ canceled: false });
        }
    }
}