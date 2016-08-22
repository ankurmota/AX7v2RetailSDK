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
     * Options passed to the Pay operation.
     */
    export interface IPaymentOperationOptions extends IOperationOptions {
        /**
         * Kind of tender to be executed (e.g. cash, card, etc)
         */
        tenderType: Model.Entities.TenderType;

        /**
         * Payment card information, if available.
         */
        paymentCardInfo: Model.Entities.CardInfo;

        /**
         * Source of the payment card information, if available.
         */
        paymentCardSource: Model.Entities.CardSource;

        /**
         * How much is to be charged
         */
        paymentAmount: () => number;

        /**
         * State of the current amount
         */
        currentPaymentAmountText: string;

        /**
         * Do not process payment. Tokenize payment card only.
         * Controller will asume false, if not provided.
         */
        tokenizeCard: boolean;

        /**
         * Loyalty card Id, if available
         */
        loyaltyCardId: string;

        /**
         * The card payment accept page url.
         */
        cardPaymentAcceptPageUrl: string;

        /**
         * The card payment accept page submit url.
         */
        cardPaymentAcceptPageSubmitUrl: string;

        /**
         * The origin of the card payment accept messaging page.
         */
        cardPaymentAcceptMessageOrigin: string;
    }

    /**
     * Handler for the Pay operation.
     */
    export class PaymentOperationHandler extends OperationHandlerBase {
        /**
         * Executes the Pay operation.
         *
         * @param {IPaymentOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IPaymentOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || {
                currentPaymentAmountText: undefined,
                loyaltyCardId: undefined,
                paymentAmount: undefined,
                paymentCardInfo: undefined,
                paymentCardSource: undefined,
                tokenizeCard: undefined,
                tenderType: undefined,
                cardPaymentAcceptPageUrl: undefined,
                cardPaymentAcceptPageSubmitUrl: undefined,
                cardPaymentAcceptMessageOrigin: undefined
            };

            var asyncQueue = new AsyncQueue();
            var cart = Session.instance.cart;
            var navigateBack: boolean = false;

            // Check whether the totals need to be calculated before payment
            if (!CustomerOrderHelper.isCustomerOrderReturnOrPickup(cart)) {
                if (!ObjectExtensions.isNullOrUndefined(cart.IsDiscountFullyCalculated)
                    && (!cart.IsDiscountFullyCalculated)
                    && (!CartHelper.isCartType(cart, Proxy.Entities.CartType.AccountDeposit))) {
                    asyncQueue.enqueue(() => {
                        return OperationsManager.instance.runOperation(Operations.RetailOperation.CalculateFullDiscounts, null)
                            .done((result) => {
                                if (result.canceled) {
                                    asyncQueue.cancel();
                                } else {
                                    cart = Session.instance.cart;
                                    options.paymentAmount = CartHelper.cartAmountDue;
                                }
                            }).fail((errors) => {
                                NotificationHandler.displayClientErrors(errors, "string_4374"); // Calculate total
                            });
                    });
                }
            }

            // check to see if a customer account payment has already been made on the transaction if paying by customer account
            if (options.tenderType.OperationId === RetailOperation.PayCustomerAccount) {
                if (ArrayExtensions.hasElements(cart.TenderLines)
                    && cart.TenderLines.filter(t => !t.IsHistorical && t.StatusValue !== Model.Entities.TenderLineStatus.Voided && t.TenderTypeId === options.tenderType.TenderTypeId).length > 0) {
                    asyncQueue.enqueue(() => {
                        return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.PAYMENT_ONLY_ONE_CUSTOMER_ACCOUNT_PAYMENT_ALLOWED)]);
                    });
                }
            }

            asyncQueue.enqueue((): IAsyncResult<ICancelableResult> => {
                var preTriggerOptions: Triggers.IPrePaymentTriggerOptions = { cart: cart, tenderType: options.tenderType };
                var preTriggerResult: IAsyncResult<ICancelableResult> =
                    Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PrePayment, preTriggerOptions);

                return asyncQueue.cancelOn(preTriggerResult);
            });

            // check for hardware station selection
            // if there is no payment made, select hardware station first
            if (ApplicationContext.Instance.deviceConfiguration.SelectHardwareStationOnTendering
                && !ArrayExtensions.hasElements(Session.instance.cart.TenderLines)) {

                asyncQueue.enqueue(() => {
                    navigateBack = true;
                    var activity = new Activities.SelectHardwareStationActivity({ activeOnly: true });
                    return activity.execute()
                        .done(() => {
                            if (!activity.response) {
                                asyncQueue.cancel();
                                return;
                            }

                            // lock the payment terminal
                            PaymentHelper.callBeginTransaction();
                        });
                });
            } else {
                // lock the payment terminal if not already locked
                PaymentHelper.callBeginTransaction();
            }

            asyncQueue.enqueue(() => {
                // Update line display
                Commerce.Peripherals.HardwareStation.LineDisplayHelper.displayBalance(cart.TotalAmount, cart.AmountDue);

                return VoidAsyncResult.createResolved();
            });

            asyncQueue.enqueue(() => {
                if (navigateBack) {
                    ViewModelAdapter.navigateBack();
                }

                ViewModelAdapter.navigate("PaymentView", options);
                return VoidAsyncResult.createResolved();
            });

            asyncQueue.run();

            // we cannot resolve result, it would mean that payment was processed, but this is just navigating.
            return new VoidAsyncResult();
        }
    }
}