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

    import Diagnostics = Microsoft.Dynamics.Diagnostics;
    import Entities = Proxy.Entities;

    /**
     * Options passed to the ItemSale operation.
     */
    export interface IItemSaleOperationOptions extends IOperationOptions {
        productSaleDetails: Entities.ProductSaleReturnDetails[];
    }

    /**
     * Handler for the ItemSale operation.
     */
    export class ItemSaleOperationHandler extends PrePostTriggerOperationHandlerBase {

        /**
         * Executes the pre-trigger for the operation.
         * @param {IItemSaleOperationOptions} options The operation options.
         * @return {IAsyncResult<ICancelableResult>} The result of the pre-trigger execution.
         */
        protected executePreTrigger(options: IItemSaleOperationOptions): IAsyncResult<ICancelableResult> {
            var preTriggerOptions: Triggers.IPreProductSaleTriggerOptions = { cart: Session.instance.cart, productSaleDetails: options.productSaleDetails };
            return Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PreProductSale, preTriggerOptions);
        }

        /**
         * Executes the post-trigger for the operation.
         * @param {IItemSaleOperationOptions} options The operation options.
         * @param {IOperationResult} result The result of the operation.
         * @return {IVoidAsyncResult} The result of the post-trigger execution.
         */
        protected executePostTrigger(options: IItemSaleOperationOptions, result: IOperationResult): IVoidAsyncResult {
            var postTriggerOptions: Triggers.IPostProductSaleTriggerOptions = { cart: Session.instance.cart, productSaleDetails: options.productSaleDetails };
            return Triggers.TriggerManager.instance.execute(Triggers.NonCancelableTriggerType.PostProductSale, postTriggerOptions);
        }

        /**
         * Sanitizes the options provided to the operation.
         * @param {IItemSaleOperationOptions} options The provided options.
         * @return {IItemSaleOperationOptions} The sanitized options.
         */
        protected sanitizeOptions(options: IItemSaleOperationOptions): IItemSaleOperationOptions {
            options = options || { productSaleDetails: undefined };
            return options;
        }

        /**
         * Executes the ItemSale operation.
         *
         * @param {IItemSaleOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        protected executeInternal(options: IItemSaleOperationOptions): IAsyncResult<IOperationResult> {
            var productSaleDetails = options.productSaleDetails || [];
            var cartLines: Entities.CartLine[] = [];
            var productSaleDetailsToSell: Entities.ProductSaleReturnDetails[] = [];
            var productSaleDetailsToReturn: Entities.ProductSaleReturnDetails[] = [];

            productSaleDetails.forEach((detail: Entities.ProductSaleReturnDetails) => {
                var product: Entities.SimpleProduct = detail.product;
                var negativeQuantity: boolean = !ObjectExtensions.isNullOrUndefined(detail.quantity) && detail.quantity < 0;
                var qtyBecomesNegative: boolean = !ObjectExtensions.isNullOrUndefined(product) && product.Behavior.IsNegativeQuantityAllowed;
                if (negativeQuantity || qtyBecomesNegative) {
                    productSaleDetailsToReturn.push(detail);
                } else {
                    productSaleDetailsToSell.push(detail);
                }
            });

            //POSHackF
            var SC_IsKit: boolean = false;
            if (productSaleDetailsToSell.length > 0 && productSaleDetailsToSell[0].productId === 68719485372) {//DEMO 4 TODO : Change this per ENV
                productSaleDetailsToSell = [
                    {

                        productId: 22565421965,
                        quantity: 1
                    }
                    ,
                    {

                        productId: 22565421966,
                        quantity: 1
                    }
                    ,
                    {

                        productId: 22565421967,
                        quantity: 1
                    },
                    {

                        productId: 22565421968,
                        quantity: 1
                    },
                    {

                        productId: 22565421971,
                        quantity: 1
                    },
                    {

                        productId: 22565421983,
                        quantity: 1
                    }
                ];
                SC_IsKit = true;

            }
            //POSHackF end

            var asyncQueue: AsyncQueue = new AsyncQueue();

            if (ArrayExtensions.hasElements(productSaleDetailsToReturn)) {
                asyncQueue.enqueue(() => {
                    var returnOptions: IReturnProductOperationOptions = {
                        customerId: Session.instance.cart.CustomerId,
                        productReturnDetails: productSaleDetailsToReturn.map((detail: Entities.ProductSaleReturnDetails) => {
                            return <Entities.ProductReturnDetails>{ manualReturn: detail }
                        })
                    };

                    return asyncQueue.cancelOn(OperationsManager.instance.runOperation(Operations.RetailOperation.ReturnItem, returnOptions));
                });
            }

            if (ArrayExtensions.hasElements(productSaleDetailsToSell)) {
                asyncQueue
                    .enqueue(() => {
                        var reasonCodeQueue = ActivityHelper.getStartOfTransactionReasonCodesAsyncQueue(Session.instance.cart);
                        return asyncQueue.cancelOn(reasonCodeQueue.run());
                    }).enqueue(() => {
                        var getProductDetailsCorrelationId: string = Diagnostics.TypeScriptCore.Utils.generateGuid();
                        RetailLogger.operationItemSaleGetProductSaleDetailsStarted(getProductDetailsCorrelationId);
                        var productDetailsQueue = ActivityHelper.getProductSaleDetailsAsyncQueue(productSaleDetailsToSell);
                        return asyncQueue.cancelOn(productDetailsQueue.run()).done((getProductSaleDetailsResult: ICancelableResult): void => {
                            RetailLogger.operationItemSaleGetProductSaleDetailsFinished(getProductDetailsCorrelationId, true);
                        }).fail((errors: Entities.Error[]): void => {
                            RetailLogger.operationItemSaleGetProductSaleDetailsFinished(getProductDetailsCorrelationId, false);
                        });
                    }).enqueue(() => {
                        var createCartLinesCorrelationId: string = Diagnostics.TypeScriptCore.Utils.generateGuid();
                        RetailLogger.operationItemSaleCreateCartLinesStarted(createCartLinesCorrelationId);
                        //POSHackF - Add parameter SC_IsKit 
                        var cartLinesQueue = ActivityHelper.createCartLinesAsyncQueue(productSaleDetailsToSell, cartLines, SC_IsKit);
                        return asyncQueue.cancelOn(cartLinesQueue.run()).done((createCartLinesResult: ICancelableResult): void => {
                            RetailLogger.operationItemSaleCreateCartLinesFinished(createCartLinesCorrelationId, true);
                        }).fail((errors: Entities.Error[]): void => {
                            RetailLogger.operationItemSaleCreateCartLinesFinished(createCartLinesCorrelationId, false);
                        });
                    }).enqueue(() => {
                        var addCartLinesQueue = ActivityHelper.addCartLinesAsyncQueue(cartLines);
                        return asyncQueue.cancelOn(addCartLinesQueue.run());
                    });
            }

            return asyncQueue.run();
        }
    }
}