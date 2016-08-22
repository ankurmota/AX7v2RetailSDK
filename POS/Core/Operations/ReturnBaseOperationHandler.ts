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

    import CartLine = Model.Entities.CartLine;

    /**
     * Options passed to the ReturnProduct operation.
     */
    export interface IReturnProductOptions {
        /**
         * The customer identifier.
         */
        customerId: string;

        /**
         * The collection of products return details.
         */
        productReturnDetails: Model.Entities.ProductReturnDetails[];

        /**
         * The trigger to execute prior to performing the return.
         */
        preReturnTrigger: (cartLines: CartLine[]) => IAsyncResult<ICancelableResult>;

        /**
         * The trigger to execute after performing the return.
         */
        postReturnTrigger: (cartLines: CartLine[]) => IVoidAsyncResult;
    }

    /**
     * Handler for the Return operations.
     */
    export class ReturnOperationHandlerBase extends OperationHandlerBase {

        /**
         * Executes the Return operation.
         * @param {IOperationOptions} options The options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IOperationOptions): IAsyncResult<IOperationResult> {
            throw "Abstract method 'execute' not implemented.";
        }

        /**
         * Creates cart lines from the product return details and adds them to the collection of cart lines.
         * @param { Model.Entities.ProductReturnDetails[] } returnDetails The return details to base the cart lines on.
         * @param { CartLine[] } cartLines The collection of cart lines to update.
         * @return { IAsyncResult<ICancelableResult> } The async result.
         */
        protected createCartLinesFromReturnDetails(
            returnDetails: Model.Entities.ProductReturnDetails[],
            cartLines: CartLine[]): IAsyncResult<ICancelableResult> {
            throw "Abstract method 'createCartLinesFromReturnDetails' not implemented.";
        }

        /**
         * Return one or more products.
         * @param {IReturnProductOptions} options The options containing the customer identifier and a collection of product return details.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        protected returnProducts(options: IReturnProductOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { customerId: undefined, productReturnDetails: [], preReturnTrigger: null, postReturnTrigger: null };
            options.productReturnDetails = options.productReturnDetails || [];

            var cartLines: CartLine[] = [];

            var asyncQueue: AsyncQueue = new AsyncQueue();

            asyncQueue.enqueue(() => {
                var reasonCodeQueue: AsyncQueue = ActivityHelper.getStartOfTransactionReasonCodesAsyncQueue(Session.instance.cart);
                return asyncQueue.cancelOn(reasonCodeQueue.run());
            });

            asyncQueue.enqueue(() => {
                var productDetailsQueue: AsyncQueue = ActivityHelper.getProductReturnDetailsAsyncQueue(options.productReturnDetails);
                return asyncQueue.cancelOn(productDetailsQueue.run());
            });

            // get product details for sales lines
            asyncQueue.enqueue(() => {
                return asyncQueue.cancelOn(this.createCartLinesFromReturnDetails(options.productReturnDetails, cartLines));
            });

            if (!ObjectExtensions.isNullOrUndefined(options.preReturnTrigger)) {
                asyncQueue.enqueue((): IAsyncResult<ICancelableResult> => {
                    return asyncQueue.cancelOn(options.preReturnTrigger(cartLines));
                });
            }

            // adds return lines
            asyncQueue.enqueue(() => {
                var linesToAdd: CartLine[] = cartLines.filter((c: CartLine) => {
                    return StringExtensions.isNullOrWhitespace(c.LineId);
                });

                if (linesToAdd.length === 0) {
                    return null;
                }

                var addCartLinesQueue: AsyncQueue = ActivityHelper.addCartLinesAsyncQueue(linesToAdd);
                return asyncQueue.cancelOn(addCartLinesQueue.run());
            });

            // updates cart lines
            asyncQueue.enqueue(() => {
                var linesToUpdate: CartLine[] = cartLines.filter((c: CartLine) => {
                    return !StringExtensions.isNullOrWhitespace(c.LineId);
                });

                if (linesToUpdate.length === 0) {
                    return null;
                }

                return this.cartManager.updateCartLinesOnCartAsync(linesToUpdate);
            });

            // adds customer, if any
            if (!StringExtensions.isNullOrWhitespace(options.customerId)
                && options.customerId !== Session.instance.cart.CustomerId) {

                asyncQueue.enqueue(() => {
                    return this.cartManager.addCustomerToCartAsync(options.customerId, []);
                });
            }

            if (!ObjectExtensions.isNullOrUndefined(options.postReturnTrigger)) {
                asyncQueue.enqueue((): IVoidAsyncResult => {
                    return options.postReturnTrigger(cartLines);
                });
            }

            return asyncQueue.run();
        }
    }
}