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
     * Options passed to the GetPaymentsHistory operation.
     */
    export interface IGetPaymentsHistoryOperationOptions extends IOperationOptions {
        cart: Model.Entities.Cart;
    }

    /**
     * Handler for the GetPaymentsHistory operation.
     */
    export class GetPaymentsHistoryOperationHandler extends OperationHandlerBase {
        /**
         * Executes the GetPaymentsHistory operation.
         * @param {IGetPaymentsHistoryOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IGetPaymentsHistoryOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { cart: undefined };

            return this.cartManager.getPaymentsHistoryAsync(options.cart.Id)
                .map((tenderLines: Model.Entities.TenderLine[]) => { return <IOperationResult>{ canceled: false, data: tenderLines }; });
        }
    }
}