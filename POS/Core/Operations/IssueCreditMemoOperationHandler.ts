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
     * Options passed to the IssueCreditMemo operation.
     */
    export interface IIssueCreditMemoOperationOptions extends IOperationOptions {
        /**
         * The recipient email address to send receipt to after checkout.
         */
        recipientEmailAddress: string;
    }

    /**
     * Handler for the IssueCreditMemo operation.
     */
    export class IssueCreditMemoOperationHandler extends OperationHandlerBase {
        /**
         * Executes the IssueCreditMemo operation.
         *
         * @param {IIssueCreditMemoOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IIssueCreditMemoOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { recipientEmailAddress: undefined };

            // Get the tender type for issue credit memo
            // Uses Pay credit memo for the tender type for Issue credit memo
            var issueCreditMemoTenderType: Model.Entities.TenderType = Commerce.ApplicationContext.Instance.tenderTypesMap.getTenderTypeByOperationId(Operations.RetailOperation.PayCreditMemo);
            if (ObjectExtensions.isNullOrUndefined(issueCreditMemoTenderType)) {
                return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.OPERATION_ISSUE_CREDIT_MEMO_NOT_AVAILABLE)]);
            }

            // For return customer order discount calculation check is not required
            // Check whether the totals need to be calculated before payment
            if (!CustomerOrderHelper.isCustomerOrderReturnOrPickup(Commerce.Session.instance.cart)) {
                if (!ObjectExtensions.isNullOrUndefined(Commerce.Session.instance.cart.IsDiscountFullyCalculated) && (!Commerce.Session.instance.cart.IsDiscountFullyCalculated)) {
                    return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.CALCULATE_TOTAL_BEFORE_PAYMENT)]);
                }
            }

            var cartManager: Commerce.Model.Managers.ICartManager = Commerce.Model.Managers.Factory.GetManager(Commerce.Model.Managers.ICartManagerName, null);
            var tenderLine: Commerce.Model.Entities.TenderLine;
            var salesOrder: Model.Entities.SalesOrder;
            var asyncQueue = new AsyncQueue()
                .enqueue(() => {
                    // Check that the amount is valid for issue credit memo
                    var amount = Commerce.Session.instance.cart.AmountDue;
                    if (amount >= 0) {
                        return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.CREDIT_MEMO_INVALID_AMOUNT)]);
                    }

                    // Create the tender line
                    tenderLine = <Commerce.Model.Entities.TenderLine> {
                        TenderLineId: "",
                        Amount: amount,
                        Currency: Commerce.ApplicationContext.Instance.deviceConfiguration.Currency,
                        TenderTypeId: issueCreditMemoTenderType.TenderTypeId,
                    };

                    return VoidAsyncResult.createResolved();
                }).enqueue(() => {
                    var retryQueue = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                        { tenderLines: [tenderLine] },
                        c => { return this.cartManager.addTenderLineToCartAsync(c.tenderLines[0]); });

                    return asyncQueue.cancelOn(retryQueue.run());
                }).enqueue(() => {
                    // Checkout the cart
                    var updateCartBeforeCheckout: boolean = false;
                    var retryQueue = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                        { cart: Session.instance.cart },
                        (context) => {
                            var updateAsyncQueue = new AsyncQueue()
                            if (updateCartBeforeCheckout) {
                                updateAsyncQueue.enqueue(() => {
                                    // updates the cart with the added reason codes
                                    return this.cartManager.createOrUpdateCartAsync(context.cart);
                                });
                            }

                            updateCartBeforeCheckout = true;

                            updateAsyncQueue.enqueue(() => {
                                return this.cartManager.checkoutCartAsync(options.recipientEmailAddress)
                                    .done((salesOrderResult: Model.Entities.SalesOrder) => {
                                        salesOrder = salesOrderResult;
                                    });
                            });

                            return updateAsyncQueue.run();
                        });

                    return asyncQueue.cancelOn(retryQueue.run());
                });

            return asyncQueue.run().map((result) => { return { canceled: result.canceled, data: salesOrder }; });
        }
    }
}