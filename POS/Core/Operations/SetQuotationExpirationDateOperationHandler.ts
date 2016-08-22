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
     * Options passed to the SetQuotationExpirationDate operation.
     */
    export interface ISetQuotationExpirationDateOperationOptions extends IOperationOptions {
        cart: Model.Entities.Cart;
        requestedExpirationDate: Date;
    }

    /**
     * Handler for the SetQuotationExpirationDate operation.
     */
    export class SetQuotationExpirationDateOperationHandler extends OperationHandlerBase {
        /**
         * Executes the SetQuotationExpirationDate operation.
         *
         * @param {ISetQuotationExpirationDateOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: ISetQuotationExpirationDateOperationOptions): IAsyncResult<IOperationResult> {
            if (ObjectExtensions.isNullOrUndefined(options)) {
                RetailLogger.genericError("Null or undefined reference: operationOptions on SetQuotationExpirationDateOperationHandler.execute()");
                return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
            }

            var cart: Model.Entities.Cart = options.cart;
            var expirationDate: Date = options.requestedExpirationDate;
            var asyncQueue = new AsyncQueue();

            if (!CustomerOrderHelper.isQuote(cart)) {
                return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.CREATE_OR_EDIT_QUOTATION_ONLY)]);
            }

            if (ObjectExtensions.isNullOrUndefined(expirationDate)) {
                asyncQueue.enqueue(() => {
                    var originalExpirationDate = ObjectExtensions.isNullOrUndefined(cart.QuotationExpiryDate) ?
                        CustomerOrderHelper.getDefaultQuoteExpirationDate() : cart.QuotationExpiryDate;

                    return CustomerOrderHelper.getQuotationExpirationDate(originalExpirationDate)
                        .done((requestedExpirationDate: Date) => {
                            if (ObjectExtensions.isNullOrUndefined(requestedExpirationDate)) {
                                asyncQueue.cancel();
                                return;
                            }

                            expirationDate = requestedExpirationDate;
                        });
               });
            }

            asyncQueue
                .enqueue(() => {
                    if (!DateExtensions.isTodayOrFutureDate(expirationDate)) {
                        return VoidAsyncResult.createRejected([new Model.Entities.Error(
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_QUOTEMUSTHAVEVALIDQUOTATIONEXPIRYDATE)]);
                    }
                }).enqueue(() => {
                    var requestCart: Model.Entities.Cart = new Model.Entities.CartClass();
                    requestCart.Id = cart.Id;
                    requestCart.QuotationExpiryDate = expirationDate;
                    return this.cartManager.createOrUpdateCartAsync(requestCart);
                });

            return asyncQueue.run();
        }
    }
}