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

    /**
     * Options passed to the VoidProducts operation.
     */
    export interface IVoidProductsOperationOptions extends IOperationOptions {
        cartLines: Proxy.Entities.CartLine[];
    }

    /**
     * Handler for the VoidProducts operation.
     */
    export class VoidProductsOperationHandler extends PrePostTriggerOperationHandlerBase {
        /**
         * Executes the pre-trigger for the VoidProducts operation.
         * @param {IVoidProductsOperationOptions} options The operation options.
         * @return {IAsyncResult<ICancelableResult>} The result of the pre-trigger execution.
         */
        protected executePreTrigger(options: IVoidProductsOperationOptions): IAsyncResult<ICancelableResult> {
            var preTriggerOptions: Triggers.IPreVoidProductsTriggerOptions = { cart: Session.instance.cart, cartLines: options.cartLines };
            return Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PreVoidProducts, preTriggerOptions);
        }

        /**
         * Executes the post-trigger for the VoidProducts operation.
         * @param {IVoidProductsOperationOptions} options The operation options.
         * @param {IOperationResult} result The result of the operation.
         * @return {IVoidAsyncResult} The result of the post-trigger execution.
         */
        protected executePostTrigger(options: IVoidProductsOperationOptions, result: IOperationResult): IVoidAsyncResult {
            var lineIds: string[] = options.cartLines.map((cartLine: Model.Entities.CartLine): string => {
                return cartLine.LineId;
            });

            var updatedCartLines: Model.Entities.CartLine[] = CartHelper.getCartLinesByLineIds(Session.instance.cart, lineIds);
            var postTriggerOptions: Triggers.IPostVoidProductsTriggerOptions = { cart: Session.instance.cart, cartLines: updatedCartLines };

            return Triggers.TriggerManager.instance.execute(Triggers.NonCancelableTriggerType.PostVoidProducts, postTriggerOptions);
        }

        /**
         * Sanitizes the options provided to the operation.
         * @param {IVoidProductsOperationOptions} options The provided options.
         * @return {IVoidProductsOperationOptions} The sanitized options.
         */
        protected sanitizeOptions(options: IVoidProductsOperationOptions): IVoidProductsOperationOptions {
            options = options || { cartLines: undefined };
            return options;
        }

        /**
         * Executes the VoidProducts operation.
         * @param {IVoidProductsOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        protected executeInternal(options: IVoidProductsOperationOptions): IAsyncResult<IOperationResult> {
            if (ArrayExtensions.hasElements(options.cartLines)) {
                // cartLines below are going to be modified for a server request though we don't want to mess it with a local state thus they are cloned.
                var cartLines: Proxy.Entities.CartLine[] = ObjectExtensions.clone(options.cartLines);
                var cartLinesToVoid: Proxy.Entities.CartLine[] = cartLines.filter((c: Proxy.Entities.CartLine) => !c.IsVoided);
                var cartLinesToUnvoid: Proxy.Entities.CartLine[] = cartLines.filter((c: Proxy.Entities.CartLine) => c.IsVoided);

                // Negate the void status
                cartLines.forEach((c: Proxy.Entities.CartLine) => c.IsVoided = !c.IsVoided);

                return ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                    { cartLines: cartLinesToVoid },
                    (c: ReasonCodesContext) => {
                        return this.cartManager.voidCartLinesOnCartAsync(c.cartLines.concat(cartLinesToUnvoid));
                    },
                    Proxy.Entities.ReasonCodeSourceType.VoidItem).run();
            }

            return VoidAsyncResult.createResolved();
        }
    }
}