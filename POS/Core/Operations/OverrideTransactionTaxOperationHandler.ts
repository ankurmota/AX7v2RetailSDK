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
     * Options passed to the OverrideTransactionTaxOperationHandler operation.
     * It contains the cart where the given tax override will be applied to.
     */
    export interface IOverrideTransactionTaxOperationOptions extends IOperationOptions {
        taxOverride: Model.Entities.TaxOverride;
        cart: Model.Entities.Cart;
    }

    /**
     * Handler for the OverrideTransactionTax operation.
     */
    export class OverrideTransactionTaxOperationHandler extends OperationHandlerBase {
        /**
         * Executes the OverrideTransactionTax operation.
         * @param {IOverrideTransactionTaxOperationOptions} options The options containing the cart to apply the given tax override to.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IOverrideTransactionTaxOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { taxOverride: undefined, cart: undefined };
            options.taxOverride = options.taxOverride || new Model.Entities.TaxOverrideClass({ Code: StringExtensions.EMPTY });

            // create new cart with applied tax override code
            var newCart: Model.Entities.Cart = new Model.Entities.CartClass({ Id: options.cart.Id, TaxOverrideCode: options.taxOverride.Code });

            var asyncQueue: AsyncQueue = new AsyncQueue().enqueue((): IAsyncResult<any> => {
                var result: IAsyncResult<ICancelableResult> = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                    { cart: newCart },
                    (c: ReasonCodesContext) => { return this.cartManager.createOrUpdateCartAsync(c.cart); },
                    Proxy.Entities.ReasonCodeSourceType.TransactionTaxChange).run();

                return asyncQueue.cancelOn(result);
            });

            return asyncQueue.run();
        }
    }
}