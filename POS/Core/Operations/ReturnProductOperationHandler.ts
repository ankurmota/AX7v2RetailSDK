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
     * Options passed to the ReturnProduct operation.
     */
    export interface IReturnProductOperationOptions {
        /**
         * The customer identifier.
         */
        customerId: string;

        /**
         * The collection of products return details.
         */
        productReturnDetails: Model.Entities.ProductReturnDetails[];
    }

    /**
     * Handler for the ReturnProduct operation.
     */
    export class ReturnProductOperationHandler extends ReturnOperationHandlerBase {
        /**
         * Executes the ReturnProduct operation.
         * @param {IReturnProductOperationOptions} options The options containing the customer identifier and a collection of product return details.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IReturnProductOperationOptions): IAsyncResult<IOperationResult> {
            var returnProductOptions: IReturnProductOptions = {
                customerId: options.customerId,
                productReturnDetails: options.productReturnDetails,
                preReturnTrigger: (cartLines: Model.Entities.CartLine[]): IAsyncResult<ICancelableResult> => {
                    var preTriggerOptions: Triggers.IPreReturnProductTriggerOptions = { cart: Session.instance.cart, cartLinesForReturn: cartLines };
                    return Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PreReturnProduct, preTriggerOptions);
                },
                postReturnTrigger: (cartLines: Model.Entities.CartLine[]): IVoidAsyncResult => {
                    var postTriggerOptions: Triggers.IPostReturnProductTriggerOptions = { cart: Session.instance.cart, cartLinesForReturn: cartLines };
                    return Triggers.TriggerManager.instance.execute(Triggers.NonCancelableTriggerType.PostReturnProduct, postTriggerOptions);
                }
            };

            return this.returnProducts(returnProductOptions);
        }

        /**
         * Creates cart lines from the product return details and adds them to the collection of cart lines.
         * @param { Model.Entities.ProductReturnDetails[] } returnDetails The return details to base the cart lines on.
         * @param { Model.Entities.CartLine[] } cartLines The collection of cart lines to update.
         * @return { IAsyncResult<ICancelableResult> } The async result.
         */
        protected createCartLinesFromReturnDetails(
            returnDetails: Model.Entities.ProductReturnDetails[],
            cartLines: Model.Entities.CartLine[]): IAsyncResult<ICancelableResult> {
            var asyncQueue: AsyncQueue = new AsyncQueue();

            asyncQueue.enqueue(() => {
                var manualReturns: Model.Entities.ProductSaleReturnDetails[] = returnDetails.filter(
                    (p: Model.Entities.ProductReturnDetails) => { return !ObjectExtensions.isNullOrUndefined(p.manualReturn); }
                    ).map((p: Model.Entities.ProductReturnDetails) => p.manualReturn);

                if (!ArrayExtensions.hasElements(manualReturns)) {
                    return null;
                }

                // make sure quantities are negative
                manualReturns.forEach((p: Model.Entities.ProductSaleReturnDetails) => p.quantity = -1 * Math.abs(p.quantity));

                var cartLinesQueue: AsyncQueue = ActivityHelper.createCartLinesAsyncQueue(manualReturns, cartLines);
                return asyncQueue.cancelOn(cartLinesQueue.run());
            }).enqueue((): IVoidAsyncResult => {
                returnDetails.forEach((p: Model.Entities.ProductReturnDetails) => {
                    if (p.cartLine) {
                        cartLines.push(<Model.Entities.CartLine>{
                            LineId: p.cartLine.LineId,
                            Description: p.cartLine.Description,
                            ItemId: p.cartLine.ItemId,
                            ProductId: p.cartLine.ProductId,
                            Quantity: -1 * Math.abs(p.cartLine.Quantity)
                        });
                    }
                });

                return VoidAsyncResult.createResolved();
            }).enqueue((): IAsyncResult<ICancelableResult> => {
                var result: IAsyncResult<ICancelableResult> = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                    { cartLines: cartLines },
                    (c: ReasonCodesContext) => { return VoidAsyncResult.createResolved(); },
                    Proxy.Entities.ReasonCodeSourceType.ReturnItem, true).run();

                return asyncQueue.cancelOn(result);
            });

            return asyncQueue.run();
        }
    }
}