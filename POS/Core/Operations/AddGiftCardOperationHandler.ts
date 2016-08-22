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
     * Options passed to the AddGiftCard operation.
     */
    export interface IAddGiftCardOperationOptions extends IOperationOptions {
        giftCardId: string;
        amount: number;
        currency: string;
        lineDescription: string;
    }

    /**
     * Handler for the AddGiftCard operation.
     */
    export class AddGiftCardOperationHandler extends OperationHandlerBase {
        /**
         * Executes the AddGiftCard operation.
         *
         * @param {IAddGiftCardOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IAddGiftCardOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { amount: undefined, currency: undefined, giftCardId: undefined, lineDescription: undefined };

            RetailLogger.operationAddGiftCard(options.giftCardId, options.amount, options.currency, options.lineDescription);
            var cart: Model.Entities.Cart = Session.instance.cart;

            var asyncQueue = new AsyncQueue();
            if (StringExtensions.isNullOrWhitespace(cart.Id)) {
                //Cart has no Id yet. Create empty cart request so server can give us back a cart with session Id.
                asyncQueue.enqueue(() => { return this.cartManager.createEmptyCartAsync(); });
            }

            asyncQueue.enqueue(() => {
                return this.cartManager.addGiftCardToCartAsync(options.giftCardId, options.amount, options.currency, options.lineDescription);
            });

            return asyncQueue.run();
        }
    }
}