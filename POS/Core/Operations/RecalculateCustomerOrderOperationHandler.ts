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
     * Options passed to the RecalculateCustomerOrder operation.
     */
    export interface IRecalculateCustomerOrderOperationOptions extends IOperationOptions {
        cart: Model.Entities.Cart;
    }

    /**
     * Handler for the RecalculateCustomerOrder operation.
     */
    export class RecalculateCustomerOrderOperationHandler extends OperationHandlerBase {
        /**
         * Executes the RecalculateCustomerOrder operation.
         *
         * @param {IRecalculateCustomerOrderOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IRecalculateCustomerOrderOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { cart: undefined };
            options.cart = options.cart || new Model.Entities.CartClass({ Id: StringExtensions.EMPTY });

            if (StringExtensions.isNullOrWhitespace(options.cart.Id)
                || !CustomerOrderHelper.isCustomerOrderOrQuoteCreationOrEdition(options.cart))
            {
                return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.EDIT_CUSTOMER_ORDER_OR_QUOTATION_ONLY)]);
            }

            return this.cartManager.recalculateOrderAsync(options.cart.Id).map(
                (cart) => { return <IOperationResult>{ canceled: false, data: cart }; });
        }
    }
}