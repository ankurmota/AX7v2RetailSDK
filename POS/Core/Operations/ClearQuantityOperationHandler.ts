/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='PrePostTriggerOperationHandlerBase.ts' />

module Commerce.Operations {
    "use strict";

    import CartLine = Model.Entities.CartLine;

    /**
     * Options passed to the ClearQuantityOperation operation.
     */
    export interface IClearQuantityOperationOptions extends IOperationOptions {
        cartLines: CartLine[];
    }

    /**
     * Handler for the ClearQuantityOperation operation.
     */
    export class ClearQuantityOperationHandler extends PrePostTriggerOperationHandlerBase {

        /**
         * Executes the pre-trigger for the ClearQuantity operation.
         * @param {IClearQuantityOperationOptions} options The operation options.
         * @return {IAsyncResult<ICancelableResult>} The result of the pre-trigger execution.
         */
        protected executePreTrigger(options: IClearQuantityOperationOptions): IAsyncResult<ICancelableResult> {
            var preTriggerOptions: Triggers.IPreClearQuantityTriggerOptions = { cart: Session.instance.cart, cartLines: options.cartLines };
            return Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PreClearQuantity, preTriggerOptions);
        }

        /**
         * Executes the post-trigger for the ClearQuantity operation.
         * @param {IClearQuantityOperationOptions} options The operation options.
         * @param {IOperationResult} result The result of the operation.
         * @return {IVoidAsyncResult} The result of the post-trigger execution.
         */
        protected executePostTrigger(options: IClearQuantityOperationOptions, result: IOperationResult): IVoidAsyncResult {
            var lineIds: string[] = options.cartLines.map((cartLine: Model.Entities.CartLine): string => {
                return cartLine.LineId;
            });

            var updatedCartLines: Model.Entities.CartLine[] = CartHelper.getCartLinesByLineIds(Session.instance.cart, lineIds);
            var postTriggerOptions: Triggers.IPostClearQuantityTriggerOptions = { cart: Session.instance.cart, cartLines: updatedCartLines };

            return Triggers.TriggerManager.instance.execute(Triggers.NonCancelableTriggerType.PostClearQuantity, postTriggerOptions);
        }

        /**
         * Sanitizes the options provided to the operation.
         * @param {IClearQuantityOperationOptions} options The provided options.
         * @return {IClearQuantityOperationOptions} The sanitized options.
         */
        protected sanitizeOptions(options: IClearQuantityOperationOptions): IClearQuantityOperationOptions {
            options = options || { cartLines: undefined };
            options.cartLines = options.cartLines || [];
            return options;
        }

        /**
         * Executes the ClearQuantity operation.
         * @param {IClearQuantityOperationOptions} options The ClearQuantity operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        protected executeInternal(options: IClearQuantityOperationOptions): IAsyncResult<IOperationResult> {
            var cartLinesToUpdate: CartLine[] = [];
            options.cartLines.forEach((c: CartLine) => {
                var quantity: number = c.Quantity > 0 ? 1 : -1;
                var clonedCartLine: CartLine = ObjectExtensions.clone(c);
                clonedCartLine.Quantity = quantity;
                cartLinesToUpdate.push(clonedCartLine);
            });

            return ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                { cartLines: cartLinesToUpdate },
                (context: ReasonCodesContext) => { return this.cartManager.updateCartLinesOnCartAsync(context.cartLines); })
                .run();
        }
    }
}