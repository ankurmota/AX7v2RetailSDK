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
     * Options passed to the OverrideLineProductTaxOperationHandler operation.
     * It contains the cart line where the given tax override will be applied to.
     */
    export interface IOverrideLineProductTaxOperationOptions extends IOperationOptions {
        taxOverride: Model.Entities.TaxOverride;
        cartLine: Model.Entities.CartLine;
    }

    /**
     * Handler for the OverrideLineProductTax operation.
     */
    export class OverrideLineProductTaxOperationHandler extends OperationHandlerBase {
        /**
         * Executes the OverrideLineProductTax operation.
         * @param {IOverrideLineProductTaxOperationOptions} options The options containing the cart line to apply the given tax override to.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IOverrideLineProductTaxOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { taxOverride: undefined, cartLine: undefined };
            options.taxOverride = options.taxOverride || new Model.Entities.TaxOverrideClass({ Code: StringExtensions.EMPTY });

            // copy cart line and modify tax override code
            options.cartLine = new Model.Entities.CartLineClass(options.cartLine);
            options.cartLine.TaxOverrideCode = options.taxOverride.Code;

            var asyncQueue: AsyncQueue = new AsyncQueue().enqueue((): IAsyncResult<any> => {
                var result: IAsyncResult<ICancelableResult> = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                    { cartLines: [options.cartLine] },
                    (c: ReasonCodesContext) => { return this.cartManager.updateCartLinesOnCartAsync(c.cartLines); },
                    Proxy.Entities.ReasonCodeSourceType.LineItemTaxChange).run();

                return asyncQueue.cancelOn(result);
            });

            return asyncQueue.run();
        }
    }
}