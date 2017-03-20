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

    import Entities = Proxy.Entities;

    /**
     * Options passed to the UpdateCustomerOrder operation.
     */
    export interface IUpdateCustomerOrderOperationOptions extends IOperationOptions {
        operationType: Entities.CustomerOrderOperations;
        parameters: Entities.CustomerOrderOperationParameters;
    }

    /**
     * Handler for the UpdateCustomerOrder operation.
     */
    export class UpdateCustomerOrderOperationHandler extends OperationHandlerBase {
        /**
         * Executes the UpdateCustomerOrder operation.
         *
         * @param {UpdateOrderOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IUpdateCustomerOrderOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { operationType: undefined, parameters: undefined };

            var asyncResult: VoidAsyncResult = new VoidAsyncResult();
            switch (options.operationType) {
                case Entities.CustomerOrderOperations.PickUpFromStore:
                    return UpdateCustomerOrderOperationHandler.pickUpInStoreCustomerOrder(options.parameters);
                case Entities.CustomerOrderOperations.Cancel:
                case Entities.CustomerOrderOperations.Edit:
                    UpdateCustomerOrderOperationHandler.updateCustomerOrder(asyncResult, options.parameters);
                    break;
                case Entities.CustomerOrderOperations.PrintPackingSlip:
                    break;
                case Entities.CustomerOrderOperations.CreatePackingSlip:
                    UpdateCustomerOrderOperationHandler.createPackingSlip(asyncResult, options.parameters);
                    break;
                case Entities.CustomerOrderOperations.CreatePickingList:
                    UpdateCustomerOrderOperationHandler.createPickingList(asyncResult, options.parameters);
                    break;
                case Entities.CustomerOrderOperations.Return:
                    UpdateCustomerOrderOperationHandler.updateReturnCart(asyncResult, options.parameters.ReturnCustomerOrderParameter);
                    break;
                default:
                    RetailLogger.genericError(StringExtensions.format("Invalid Operation Type {0}", options.operationType));
                    asyncResult.reject([new Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
                    break;
            }

            return asyncResult;
        }

        
        private static pickUpInStoreCustomerOrder(parameters: Entities.CustomerOrderOperationParameters): IVoidAsyncResult {
            
            var cartManager: Model.Managers.ICartManager = Model.Managers.Factory.GetManager(Model.Managers.ICartManagerName, null);
            var productManager: Model.Managers.IProductManager = Model.Managers.Factory.GetManager(Model.Managers.IProductManagerName, null);

            var newCart: Proxy.Entities.Cart =
                <Proxy.Entities.Cart>{ Id: Session.instance.cart.Id, CustomerOrderModeValue: Proxy.Entities.CustomerOrderMode.Pickup };

            var asyncQueue: AsyncQueue = new AsyncQueue();
            var requestedCartLines: Proxy.Entities.CartLine[];
            var productsDictionary: Dictionary<Proxy.Entities.SimpleProduct> = new Dictionary<Proxy.Entities.SimpleProduct>();
            var serialNumberAsyncQueue: AsyncQueue = new AsyncQueue();

            asyncQueue
                .enqueue(() => {
                    // Update cart type to customer order pick up.
                    return cartManager.createOrUpdateCartAsync(newCart);
                })
                .enqueue(() => {
                    var productIds: number[] = [];
                    parameters.PickUpInStoreParameter.CartLines.forEach((cartLine: Entities.CartLine) => {

                        if (ObjectExtensions.isNullOrUndefined(Session.instance.getFromProductsInCartCache(cartLine.ProductId))) {
                            productIds.push(cartLine.ProductId);
                        } else {
                            productsDictionary.setItem(cartLine.ProductId, Session.instance.getFromProductsInCartCache(cartLine.ProductId));
                        }
                    });

                    if (!ArrayExtensions.hasElements(productIds)) {
                        parameters.PickUpInStoreParameter.CartLines.forEach((cartLine: Entities.CartLine) => {
                            serialNumberAsyncQueue.enqueue(() => {
                                var product: Entities.SimpleProduct = productsDictionary.getItem(cartLine.ProductId);
                                return serialNumberAsyncQueue.cancelOn(ActivityHelper.getSerialNumberAsyncQueue(product, cartLine).run());
                            });
                        });

                        return null;
                    } else {

                        // Get product details from cart lines.
                        var channelId: number = 0;
                        return productManager.getByIdsAsync(productIds, channelId)
                            .done((productsFound: Entities.SimpleProduct[]) => {

                                parameters.PickUpInStoreParameter.CartLines.forEach((cartLine: Entities.CartLine) => {
                                    var product: Entities.SimpleProduct = ArrayExtensions.firstOrUndefined(
                                        productsFound, (product: Entities.SimpleProduct) => {
                                            return product.ItemId === cartLine.ItemId;
                                        });

                                    productsDictionary.setItem(cartLine.ProductId, product);
                                    serialNumberAsyncQueue.enqueue(() => {
                                        return serialNumberAsyncQueue.cancelOn(ActivityHelper.getSerialNumberAsyncQueue(product, cartLine).run());
                                    });
                                });
                            });
                    }
                }).enqueue(() => {
                    // run activities for getting serial number (for active in sales process cart line).
                    return asyncQueue.cancelOn(serialNumberAsyncQueue.run());
                }).enqueue(() => {

                    // get only lineId and quantity for update
                    // set also serial number for cart line that has serial number.
                    requestedCartLines = parameters.PickUpInStoreParameter.CartLines.map((cartLine: Entities.CartLine) => {

                        var product: Entities.SimpleProduct = productsDictionary.getItem(cartLine.ProductId);
                        var cartLineForPickup: Entities.CartLine = { LineId: cartLine.LineId, Quantity: cartLine.Quantity, LineManualDiscountAmount: cartLine.LineManualDiscountAmount}; //TODO:AM DEMO4

                        if (product.Behavior.HasSerialNumber) {
                            cartLineForPickup.SerialNumber = cartLine.SerialNumber;
                        }

                        return cartLineForPickup;
                    });

                    return VoidAsyncResult.createResolved();
                }).enqueue(() => {
                    // update requested cart lines to server.
                    return cartManager.updateCartLinesOnCartAsync(requestedCartLines);
                });

            return asyncQueue.run();
        }

        private static updateReturnCart(asyncResult: VoidAsyncResult, parameters: Entities.ReturnCustomerOrderParameter): void {
            var cartManager: Model.Managers.ICartManager = Model.Managers.Factory.GetManager(Model.Managers.ICartManagerName, null);
            var cart: Entities.Cart = parameters.returnCart;

            cartManager.createOrUpdateCartAsync(cart)
                .done(() => asyncResult.resolve())
                .fail((errors: Entities.Error[]) => asyncResult.reject(errors));
        }

        private static updateCustomerOrder(asyncResult: VoidAsyncResult, parameters: Entities.CustomerOrderOperationParameters): void {
            var cartManager: Model.Managers.ICartManager = Model.Managers.Factory.GetManager(Model.Managers.ICartManagerName, null);

            cartManager.setCustomerOrderModeAsync(parameters.UpdateParameter.CustomerOrderModeValue)
                .done(() => asyncResult.resolve())
                .fail((error: Entities.Error[]) => asyncResult.reject(error));
        }

        private static createPackingSlip(asyncResult: VoidAsyncResult, parameters: Entities.CustomerOrderOperationParameters): void {
            var salesOrderManager: Model.Managers.ISalesOrderManager = Model.Managers.Factory.GetManager(Model.Managers.ISalesOrderManagerName, null);

            salesOrderManager.createPackingSlip(parameters.CreatePackingSlipParameter.SalesId)
                .done(() => asyncResult.resolve())
                .fail((error: Entities.Error[]) => asyncResult.reject(error));
        }

        private static createPickingList(asyncResult: VoidAsyncResult, parameters: Entities.CustomerOrderOperationParameters): void {
            var salesOrderManager: Model.Managers.ISalesOrderManager = Model.Managers.Factory.GetManager(Model.Managers.ISalesOrderManagerName, null);

            salesOrderManager.createPickingList(parameters.CreatePickingListParameter.SalesId)
                .done(() => asyncResult.resolve())
                .fail((error: Entities.Error[]) => asyncResult.reject(error));
        }
    }
}
