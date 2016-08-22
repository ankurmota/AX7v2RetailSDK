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

    /**
     * Options passed to the VoidPayment operation.
     */
    export interface IVoidPaymentOperationOptions extends IOperationOptions {
        /**
         * The tenders line to void.
         */
        tenderLines: Model.Entities.CartTenderLine[];
    }

    export class VoidPaymentOperationHandler extends PrePostTriggerOperationHandlerBase {
        /**
         * Executes the pre-trigger for the VoidPayment operation.
         * @param {IVoidPaymentOperationOptions} options The operation options.
         * @return {IAsyncResult<ICancelableResult>} The result of the pre-trigger execution.
         */
        protected executePreTrigger(options: IVoidPaymentOperationOptions): IAsyncResult<ICancelableResult> {
            var preTriggerOptions: Triggers.IPreVoidPaymentTriggerOptions = { cart: Session.instance.cart, tenderLines: options.tenderLines };
            return Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PreVoidPayment, preTriggerOptions);
        }

        /**
         * Executes the post-trigger for the VoidPayment operation.
         * @param {IVoidPaymentOperationOptions} options The operation options.
         * @param {IOperationResult} result The result of the operation.
         * @return {IVoidAsyncResult} The result of the post-trigger execution.
         */
        protected executePostTrigger(options: IVoidPaymentOperationOptions, result: IOperationResult): IVoidAsyncResult {
            var tenderLineIds: string[] = options.tenderLines.map((tenderLine: Proxy.Entities.TenderLine): string => {
                return tenderLine.TenderLineId;
            });

            var updatedTenderLines: Proxy.Entities.TenderLine[] = CartHelper.getTenderLineByTenderLineIds(Session.instance.cart, tenderLineIds);
            var postTriggerOptions: Triggers.IPostVoidPaymentTriggerOptions = { cart: Session.instance.cart, tenderLines: updatedTenderLines };
            return Triggers.TriggerManager.instance.execute(Triggers.NonCancelableTriggerType.PostVoidPayment, postTriggerOptions);
        }

        /**
         * Sanitizes the options provided to the operation.
         * @param {IVoidPaymentOperationOptions} options The provided options.
         * @return {IVoidPaymentOperationOptions} The sanitized options.
         */
        protected sanitizeOptions(options: IVoidPaymentOperationOptions): IVoidPaymentOperationOptions {
            options = options || { tenderLines: [] };
            return options;
        }

        /**
         * Executes the VoidPayment operation.
         *
         * @param {IVoidPaymentOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        protected executeInternal(options: IVoidPaymentOperationOptions): IAsyncResult<IOperationResult> {
            // if there is no tender line or tender line is already voided
            if (!ArrayExtensions.hasElements(options.tenderLines)
                || options.tenderLines[0].StatusValue == Model.Entities.TenderLineStatus.Voided
                || !options.tenderLines[0].IsVoidable) {
                var error = new Model.Entities.Error(ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_TENDERLINECANNOTBEVOIDED);
                return VoidAsyncResult.createRejected([error]);
            }

            // Get the tender line
            var cartTenderLine: Model.Entities.CartTenderLine = options.tenderLines[0];
            var isCreditOrDebitCard: boolean = ((cartTenderLine.TenderTypeId === Commerce.ApplicationContext.Instance.tenderTypesMap.getTenderTypeByOperationId(Operations.RetailOperation.PayCard).TenderTypeId)
                && (StringExtensions.isNullOrWhitespace(cartTenderLine.GiftCardId))
                && (StringExtensions.isNullOrWhitespace(cartTenderLine.LoyaltyCardId)));

            // Non-card payments have all needed information as part of the CartTenderLine.
            // Non-card payments were made through retail server and can be voided through the same path.
            // Process the void through retail server using the CartTenderLine data for non-card payments (including loyalty card and gift card as these are AX cards)
            if (!isCreditOrDebitCard) {
                return this.voidTenderLineInCart(cartTenderLine, null, false);
            }

            // Get the data for card payments
            var asyncResult = new VoidAsyncResult();
            var message: string;

            var voidPaymentCall = (payment: any) => {

                if (ObjectExtensions.isNullOrUndefined(tenderLine)) {
                    message = Commerce.ViewModelAdapter.getResourceString("string_29826"); // Payment cannot be voided.
                    this.voidPaymentFailureHandler(cartTenderLine, asyncResult, message);
                }

                payment.voidPayment(tenderLine.Amount, tenderLine.Authorization, null, this)
                    .done((paymentInfo: Commerce.Peripherals.HardwareStation.PaymentInfo) => {
                        if (paymentInfo && paymentInfo.IsApproved) {
                            // Only update the authorization blob when PaymentSdkData is not empty.
                            if (!StringExtensions.isNullOrWhitespace(paymentInfo.PaymentSdkData)) {
                                tenderLine.Authorization = paymentInfo.PaymentSdkData;
                            }

                            // Void the payment status in the transaction
                            message = Commerce.ViewModelAdapter.getResourceString("string_1191"); // The payment was voided but could not be updated in the transaction. Would you like to try to update the transaction again?
                            var voidPaymentResult = this.voidTenderLineInCart(tenderLine, message, true);
                            asyncResult.resolveOrRejectOn(voidPaymentResult);
                        }
                        else {
                            message = Commerce.ViewModelAdapter.getResourceString("string_1194"); // Sorry, something went wrong. We are unable to reverse the payment with the payment provider. Click OK to void the payment locally in the cart?
                            this.voidPaymentFailureHandler(cartTenderLine, asyncResult, message);
                        }
                    }).fail((errors: Model.Entities.Error[]) => {
                        message = Commerce.ViewModelAdapter.getResourceString("string_1194"); // Sorry, something went wrong. We are unable to reverse the payment with the payment provider. Click OK to void the payment locally in the cart?
                        this.voidPaymentFailureHandler(cartTenderLine, asyncResult, message);
                    });
            };

            // Handle void for the different payment peripherals
            var tenderLine: Model.Entities.TenderLine = this.getTenderLineFromCart(cartTenderLine, asyncResult);
            var paymentType: Proxy.Entities.PeripheralPaymentType = Commerce.CartHelper.getPeripheralPaymentType(tenderLine);
            switch (paymentType) {
                // Handle void for credit and debit card payments not using a payment terminal
                case Commerce.Model.Entities.PeripheralPaymentType.CardPaymentController:
                    voidPaymentCall(Peripherals.instance.cardPayment);
                    break;
                // Handle void for payment terminal peripherals
                case Commerce.Model.Entities.PeripheralPaymentType.PaymentTerminal:
                    voidPaymentCall(Peripherals.instance.paymentTerminal);
                    break;
                // Handle void for retail server / none / default
                default:
                    this.voidTenderLineInCart(cartTenderLine, null, false)
                        .done(() => {
                            asyncResult.resolve();
                        }).fail((errors: Model.Entities.Error[]) => {
                            message = Commerce.ViewModelAdapter.getResourceString("string_29826"); // Payment cannot be voided.
                            this.voidPaymentFailureHandler(cartTenderLine, asyncResult, message);
                        });
                    break;
            }

            return asyncResult;
        }

        /**
         * Asks the user whether to voids the preprocessed tender line in the cart/retail server and the voids the line on user OK.
         *
         * @param {Model.Entities.CartTenderLine} cartTenderLine The cart tender line.
         * @param {string} message The message.
         * @return {IVoidAsyncResult} The async result.
         */
        private voidTenderLineInCartOnAsk(cartTenderLine: Model.Entities.CartTenderLine, message: string): IVoidAsyncResult {
            var asyncResult = new VoidAsyncResult();

            var title: string = Commerce.ViewModelAdapter.getResourceString("string_1194"); // Sorry, something went wrong. We are unable to reverse the payment with the payment provider. Click OK to void the payment locally in the cart?
            Commerce.ViewModelAdapter.displayMessage(message, MessageType.Info, MessageBoxButtons.YesNo, title)
                .done((result: Commerce.DialogResult) => {
                    if (result === Commerce.DialogResult.Yes) {
                        asyncResult.resolveOrRejectOn(this.voidTenderLineInCart(cartTenderLine, message, true));
                    } else {
                        asyncResult.reject(null);
                    }
                }).fail(() => {
                    asyncResult.reject(null);
                });

            return asyncResult;
        }

        /**
         * Voids the preprocessed tender line in the cart/retail server. Does not void the tender line with the payment provider.
         * 
         * @param {Model.Entities.CartTenderLine} cartTenderLine The cart tender line.
         * @param {string} message The message.
         * @return {IVoidAsyncResult} The async result.
         */
        private voidTenderLineInCart(cartTenderLine: Model.Entities.CartTenderLine, message: string, isPreprocessed: boolean): IVoidAsyncResult {
            var asyncResult = new VoidAsyncResult();

            // Create the method to void the tender line in retail server
            var retryVoid = () => {
                ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                    { cart: Session.instance.cart, tenderLines: [cartTenderLine] },
                    c => { return this.cartManager.voidTenderLineAsync(c.tenderLines[0].TenderLineId, c.cart.ReasonCodeLines, isPreprocessed); })
                    .run()
                    .done(() => {
                    asyncResult.resolve();
                }).fail((errors: Commerce.Model.Entities.Error[]) => {

                    // if tender line has already been voided
                    if (ErrorHelper.hasError(errors, ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTALREADYVOIDED)) {
                        // retrieve updated cart from server and just treat operation as successful
                        this.cartManager.getCartByCartIdAsync(Session.instance.cart.Id)
                            .done(() => {
                                asyncResult.resolve();
                            }).fail((getCartError: Model.Entities.Error[]) => {
                                // if we could not get the cart, then nothing else can be done, fail initial operation
                                asyncResult.reject(errors);
                            });
                    } else if (isPreprocessed) {
                        var title: string = Commerce.ViewModelAdapter.getResourceString("string_1193"); // Transaction error
                        Commerce.ViewModelAdapter.displayMessage(message, MessageType.Info, MessageBoxButtons.OKCancel, title)
                            .done((result: Commerce.DialogResult) => {
                                if ((result === Commerce.DialogResult.Yes) || (result === Commerce.DialogResult.OK)) {
                                    retryVoid();
                                } else {
                                    asyncResult.reject(null);
                                }
                            }).fail(() => {
                                asyncResult.reject(null);
                            });
                    } else {
                        asyncResult.reject(errors);
                    }
                });
            }

            // Void the tender line in retail server
            retryVoid();

            return asyncResult;
        }

        /**
         * Gets the tender line from cart given the CartTenderLine entity.
         *
         * @param {Model.Entities.CartTenderLine} cartTenderLine The cart tender line entity.
         * @param {VoidAsyncResult} asyncResult The async result.
         * @return {Model.Entities.TenderLine} The tender line entity.
         */
        private getTenderLineFromCart(cartTenderLine: Model.Entities.CartTenderLine, asyncResult: VoidAsyncResult): Model.Entities.TenderLine {
            // Get the tender line from cart.
            var cart: Model.Entities.Cart = Session.instance.cart;
            var tenderLine: Model.Entities.TenderLine = null;

            if (ArrayExtensions.hasElements(cart.TenderLines)) {
                cart.TenderLines.forEach((currentTenderLine: Commerce.Model.Entities.TenderLine) => {
                    if (currentTenderLine.TenderLineId === cartTenderLine.TenderLineId) {
                        tenderLine = currentTenderLine;
                    }
                });
            }

            return tenderLine;
        }

        /**
         * Void payment failure handler.
         *
         * @param {Model.Entities.CartTenderLine} cartTenderLine The cart tender line entity.
         * @param {VoidAsyncResult} asyncResult The async result.
         * @param {string} message The void payment failure message.
         */
        private voidPaymentFailureHandler(cartTenderLine: Model.Entities.CartTenderLine, asyncResult: VoidAsyncResult, message: string) {
            var voidPaymentResult = this.voidTenderLineInCartOnAsk(cartTenderLine, message);
            asyncResult.resolveOrRejectOn(voidPaymentResult);
        }
    }
}