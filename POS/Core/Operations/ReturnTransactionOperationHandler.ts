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

    import GetTransactionReturnLinesActivity = Activities.GetTransactionReturnLinesActivity;

    /**
     * Options passed to the ReturnTransaction operation.
     */
    export interface IReturnTransactionOperationOptions extends IOperationOptions {
        /**
         * The sales order that has to be returned.
         */
        salesOrder: Model.Entities.SalesOrder;
    }

    /**
     * Handler for the ReturnTransaction operation.
     */
    export class ReturnTransactionOperationHandler extends ReturnOperationHandlerBase {
        /**
         * Executes the ReturnTransaction operation.
         * @param {IReturnTransactionOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IReturnTransactionOperationOptions): IAsyncResult<IOperationResult> {

            options = options || { salesOrder: null };

            var asyncResult: AsyncResult<IOperationResult> = new AsyncResult<IOperationResult>();
            var activity: GetTransactionReturnLinesActivity = new GetTransactionReturnLinesActivity({ salesOrder: options.salesOrder });

            // Set the response handler to handle the data retrieved from the activity
            activity.responseHandler = (response: Activities.IGetTransactionReturnLinesActivityResponse): IVoidAsyncResult => {
                var responseHandlerAsyncResult: VoidAsyncResult = new VoidAsyncResult();

                if (response && response.salesOrder && response.salesLines) {
                    var productReturnOptions: IReturnProductOptions = {
                        customerId: response.salesOrder.CustomerId,
                        productReturnDetails: response.salesLines.map((salesLine: Model.Entities.SalesLine) => {
                            return <Model.Entities.ProductReturnDetails>{
                                cartLine: undefined,
                                manualReturn: undefined,
                                salesLineReturn: { quantity: salesLine.Quantity, returnTransactionId: response.salesOrder.Id, salesLine: salesLine }
                            };
                        }),
                        preReturnTrigger: (cartLines: Model.Entities.CartLine[]): IAsyncResult<ICancelableResult> => {
                            var preTriggerOptions: Triggers.IPreReturnTransactionTriggerOptions = {
                                cart: Session.instance.cart,
                                cartLinesForReturn: cartLines,
                                originalTransaction: response.salesOrder
                            };

                            return Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PreReturnTransaction, preTriggerOptions);
                        },
                        postReturnTrigger: (cartLines: Model.Entities.CartLine[]): IVoidAsyncResult => {
                            var postTriggerOptions: Triggers.IPostReturnTransactionTriggerOptions = {
                                cart: Session.instance.cart,
                                originalTransaction: response.salesOrder,
                                cartLinesForReturn: cartLines
                            };

                            return Triggers.TriggerManager.instance.execute(Triggers.NonCancelableTriggerType.PostReturnTransaction, postTriggerOptions);
                        }
                    };

                    this.returnProducts(productReturnOptions).done((result: IOperationResult) => {
                        if (result.canceled) {
                            responseHandlerAsyncResult.reject(null);
                        } else {
                            responseHandlerAsyncResult.resolve();
                        }
                    }).fail((errors: Model.Entities.Error[]) => {
                        responseHandlerAsyncResult.reject(errors);
                    });
                } else {
                    responseHandlerAsyncResult.reject(null);
                }

                return responseHandlerAsyncResult;
            };

            // Run the activity
            activity.execute().done(() => {
                if (activity.response) {
                    asyncResult.resolve({ canceled: false, data: null });
                } else {
                    asyncResult.resolve({ canceled: true, data: null });
                }
            }).fail((errors: Model.Entities.Error[]) => {
                asyncResult.reject(errors);
            });

            return asyncResult;
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
            var productsByRecordId: { [itemId: string]: Model.Entities.SimpleProduct } = {};

            asyncQueue.enqueue(() => {
                var productIds: number[] = [];
                returnDetails.forEach((p: Model.Entities.ProductReturnDetails) => {
                    if (p.salesLineReturn) {
                        productIds.push(p.salesLineReturn.salesLine.ProductId);
                    }
                });

                if (!ArrayExtensions.hasElements(productIds)) {
                    return VoidAsyncResult.createResolved();
                }

                if (productIds.length > 1) {
                    return this.productManager.getByIdsAsync(productIds, 0).done((products: Proxy.Entities.SimpleProduct[]) => {
                        products.forEach((p: Proxy.Entities.SimpleProduct) => { productsByRecordId[p.RecordId] = p; });
                    });
                } else {
                    return this.productManager.getByIdAsync(productIds[0], 0).done((product: Proxy.Entities.SimpleProduct): void => {
                        productsByRecordId[product.RecordId] = product;
                    });
                }
            });

            asyncQueue.enqueue(() => {
                // convert product return details to cart lines
                returnDetails.forEach((p: Model.Entities.ProductReturnDetails) => {
                    var salesLine: Model.Entities.SalesLine = p.salesLineReturn.salesLine;
                    var product: Model.Entities.SimpleProduct = productsByRecordId[salesLine.ProductId];

                    cartLines.push(<Model.Entities.CartLine>{
                        Description: ObjectExtensions.isNullOrUndefined(product) ? StringExtensions.EMPTY : product.Name,
                        ItemId: salesLine.ItemId,
                        ProductId: salesLine.ProductId,
                        Quantity: -1 * Math.abs(p.salesLineReturn.quantity),
                        ReturnLineNumber: salesLine.LineNumber,
                        ReturnTransactionId: p.salesLineReturn.returnTransactionId
                    });
                });

                return VoidAsyncResult.createResolved();
            }).enqueue((): IAsyncResult<ICancelableResult> => {
                var result: IAsyncResult<ICancelableResult> = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                    { cartLines: cartLines },
                    (c: ReasonCodesContext) => { return VoidAsyncResult.createResolved(); },
                    Proxy.Entities.ReasonCodeSourceType.ReturnTransaction).run();

                return asyncQueue.cancelOn(result);
            });

            return asyncQueue.run();
        }
    }
}