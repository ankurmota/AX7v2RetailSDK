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
     * Options passed to the IssueGiftCard operation.
     */
    export interface IIssueGiftCardOperationOptions extends IOperationOptions {
        giftCardId: string;
        amount: number;
        currency: string;
        lineDescription: string;
    }

    /**
     * Handler for the IssueGiftCard operation.
     */
    export class IssueGiftCardOperationHandler extends OperationHandlerBase {
        /**
         * Executes the IssueGiftCard operation.
         *
         * @param {IIssueGiftCardOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IIssueGiftCardOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { amount: undefined, currency: undefined, giftCardId: undefined, lineDescription: undefined };

            if (ObjectExtensions.isNullOrUndefined(options.lineDescription)) {
                options.lineDescription = "#" + options.giftCardId;
            }

            RetailLogger.operationIssueGiftCard(options.giftCardId, options.amount, options.currency, options.lineDescription);

            var asyncQueue = new AsyncQueue();

            if (StringExtensions.isNullOrWhitespace(Session.instance.cart.Id)) {
                //Cart has no Id yet. Create empty cart request so server can give us back a cart with session Id.
                asyncQueue.enqueue(() => {
                    return this.cartManager.createEmptyCartAsync();
                });
            }

            asyncQueue.enqueue(() => {
                return this.cartManager.issueGiftCardToCartAsync(options.giftCardId, options.amount, options.currency, options.lineDescription);
            });

            return asyncQueue.run();
        }
    }
}