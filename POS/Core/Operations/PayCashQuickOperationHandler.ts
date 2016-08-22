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
     * Options passed to the PayCashQuick operation.
     */
    export interface IPayCashQuickOperationOptions extends IOperationOptions {
    }

    /**
     * Handler for the PayCashQuick operation.
     */
    export class PayCashQuickOperationHandler extends OperationHandlerBase {
        /**
         * Executes the PayCashQuick operation.
         *
         * @param {IPayCashQuickOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IPayCashQuickOperationOptions): IAsyncResult<IOperationResult> {
            //sanitize options
            options = options || {};

            //create tender line
            var amount = Session.instance.cart.AmountDue;
            var cashTenderType: Model.Entities.TenderType;
            var cart = Session.instance.cart;
            var hasPayableCartLine: boolean = ArrayExtensions.hasElements(cart.CartLines)
                && cart.CartLines.some((c: Model.Entities.CartLine) => !c.IsVoided);

            // Check whether the totals need to be calculated before payment
            if (!ObjectExtensions.isNullOrUndefined(cart.IsDiscountFullyCalculated)
                && (!cart.IsDiscountFullyCalculated)
                && hasPayableCartLine
                && (!CartHelper.isCartType(cart, Proxy.Entities.CartType.AccountDeposit))) {
                return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.PAYMENT_INVALID_CALCULATE_TRANSACTION_REQUIRED)]);
            }

            cashTenderType = ApplicationContext.Instance.tenderTypesMap.getTenderTypeByOperationId(
                Operations.RetailOperation.PayCash);

            if (ObjectExtensions.isNullOrUndefined(cashTenderType)) {
                var error = new Model.Entities.Error(ErrorTypeEnum.PAYMENT_CASH_PAYMENT_NOT_AVAILABLE);
                return VoidAsyncResult.createRejected([error]);
            }

            var asyncQueue: AsyncQueue = new AsyncQueue();

            // Execute Pre-Payment Trigger
            asyncQueue.enqueue((): IAsyncResult<ICancelableResult> => {
                var preTriggerOptions: Triggers.IPrePaymentTriggerOptions = { cart: cart, tenderType: cashTenderType };
                var preTriggerResult: IAsyncResult<ICancelableResult> =
                    Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PrePayment, preTriggerOptions);

                return asyncQueue.cancelOn(preTriggerResult);
            });

            // check for hardware station selection
            // if there is no payment made, select hardware station first
            if (ApplicationContext.Instance.deviceConfiguration.SelectHardwareStationOnTendering
                && !ArrayExtensions.hasElements(Session.instance.cart.TenderLines)) {

                asyncQueue.enqueue(() => {
                    var activity = new Activities.SelectHardwareStationActivity({ activeOnly: true });
                    return activity.execute().done(() => {
                        if (!activity.response) {
                            asyncQueue.cancel();
                            return;
                        }

                        // lock the hardware station
                        PaymentHelper.callBeginTransaction();
                    });
                });

                asyncQueue.enqueue(() => {
                    ViewModelAdapter.navigate("CartView");
                    return null;
                });
            }

            var tenderLine: Proxy.Entities.TenderLine;


            asyncQueue.enqueue((): IAsyncResult<ICancelableResult> => {
                tenderLine = new Model.Entities.TenderLineClass({
                    TenderLineId: "",
                    Amount: amount,
                    Currency: ApplicationContext.Instance.deviceConfiguration.Currency,
                    TenderTypeId: cashTenderType.TenderTypeId,
                });

                var preAddTenderLineTriggerOptions: Triggers.IPreAddTenderLineTriggerOptions = { cart: cart, tenderLine: tenderLine };
                var preAddTenderLineTriggerResult: IAsyncResult<ICancelableResult> =
                    Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PreAddTenderLine, preAddTenderLineTriggerOptions);

                return asyncQueue.cancelOn(preAddTenderLineTriggerResult);
            }).enqueue((): IAsyncResult<ICancelableResult> => {
                var result = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                    { tenderLines: [tenderLine] },
                    c => { return this.cartManager.addTenderLineToCartAsync(c.tenderLines[0]); }).run();

                // call RS to add tender line to cart
                return asyncQueue.cancelOn(result);
            }).enqueue((): IVoidAsyncResult => {
                tenderLine = CartHelper.getLastTenderLine(Session.instance.cart);
                var postTriggerOptions: Triggers.IPostPaymentTriggerOptions = { cart: Session.instance.cart, tenderLine: tenderLine };
                return Triggers.TriggerManager.instance.execute(Triggers.NonCancelableTriggerType.PostPayment, postTriggerOptions);
            });

            return asyncQueue.run();
        }
    }
}