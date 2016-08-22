/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Operations {
    "use strict";

    /**
     * Options passed to the CalculateTotal operation.
     */
    export interface ICalculateTotalOperationOptions extends IOperationOptions {
    }

    /**
     * Handler for the CalculateTotal operation.
     */
    export class CalculateTotalOperationHandler extends OperationHandlerBase {
        /**
         * Executes the CalculateTotal operation.
         * @param {ICalculateTotalOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: ICalculateTotalOperationOptions): IAsyncResult<IOperationResult> {
            // Calculate the total order and update the cart in the session
            return this.cartManager.recalculateOrderAsync(Session.instance.cart.Id)
                .map((result: Model.Entities.Cart): IOperationResult => { return { canceled: false, data: result }; });
        }
    }
}