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
     * Options passed to the InvoiceComment operation.
     */
    export interface IInvoiceCommentOperationOptions {
        cart: Model.Entities.Cart;
        comment: string;
    }

    /**
     * Handler for the InvoiceComment operation.
     */
    export class InvoiceCommentOperationHandler extends OperationHandlerBase {
        /**
         * Executes the InvoiceComment operation.
         *
         * @param {IInvoiceCommentOperationOptions} options The invoice comment operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IInvoiceCommentOperationOptions): IAsyncResult<IOperationResult> {
            options = options || { cart: undefined, comment: undefined };
            options.cart = options.cart || Session.instance.cart;
            options.comment = options.comment || undefined;

            var asyncQueue = new AsyncQueue();

            if (options.cart.CartTypeValue === Model.Entities.CartType.CustomerOrder) {
                // Invoice comment is not for customer order, so we exit with an error message:
                var errors: Model.Entities.Error[] = [];
                errors.push(new Commerce.Model.Entities.Error(ErrorTypeEnum.INVOICE_COMMENTS_NOT_AVAILABLE));
                return VoidAsyncResult.createRejected(errors);
            }

            if (StringExtensions.isNullOrWhitespace(options.comment)) {
                asyncQueue.enqueue(() => {
                    var activity = new Activities.GetInvoiceCommentActivity({ cart: options.cart });
                    activity.responseHandler = (comment) => {
                        return this.cartManager.addInvoiceCommentAsync(comment);
                    };

                    return activity.execute().done(() => {
                        if (!activity.response) {
                            asyncQueue.cancel();
                            return;
                        }
                    });
                });
            } else {
                asyncQueue.enqueue(() => {
                    return this.cartManager.addInvoiceCommentAsync(options.comment);
                });
            }

            return asyncQueue.run();
        }
    }
}