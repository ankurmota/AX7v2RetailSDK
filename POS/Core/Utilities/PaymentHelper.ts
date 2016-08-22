/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../Extensions/ObjectExtensions.ts'/>

module Commerce {
    "use strict";

    /**
     * Helper class for payments.
     */
    export class PaymentHelper {

        /**
         * Payment terminal begin transaction error.
         */
        private static paymentTerminalBeginTransactionError: Commerce.Model.Entities.Error;

        /**
         * The card number mask character.
         */
        private static maskChar: string = "*";

        /**
         * Verify whether the card number is masked.
         */
        public static isCardNumberMasked(cardNumber: string): boolean {
            if (!StringExtensions.isNullOrWhitespace(cardNumber)) {
                var result: number = cardNumber.indexOf(PaymentHelper.maskChar);

                if (result >= 0) {
                    return true;
                }
            }
            return false;
        }

        /**
         * Invokes payment terminal and card payment begin transaction.
         */
        public static callBeginTransaction(): void {
            if (Commerce.Peripherals.instance
                && Commerce.Peripherals.instance.paymentTerminal
                && StringExtensions.isNullOrWhitespace(Commerce.Peripherals.instance.paymentTerminal.lockToken)) {

                Commerce.Peripherals.instance.paymentTerminal.beginTransaction()
                    .done(() => {
                        // Clear the payment terminal begin transaction error and call card payment being transaction.
                        PaymentHelper.paymentTerminalBeginTransactionError = null;
                        PaymentHelper.callCardPaymentBeginTransaction();
                        Commerce.Peripherals.instance.paymentTerminal.displayTransaction(Commerce.Session.instance.cart);
                    }).fail((errors: Commerce.Model.Entities.Error[]) => {
                        PaymentHelper.paymentTerminalBeginTransactionError = errors[0];

                        // Display error and call card payment begin transaction.
                        if (Session.instance.getErrorDisplayState(Commerce.ErrorsDisplayedPerSession.PaymentTerminalBeginTransaction)) {
                            NotificationHandler.displayClientErrorsWithShowAgain(errors, "string_4928") // Payment device
                                .done((result: Commerce.IMessageResult) => {
                                    Session.instance.setErrorDisplayState(Commerce.ErrorsDisplayedPerSession.PaymentTerminalBeginTransaction,
                                        !result.messageCheckboxChecked);
                                    PaymentHelper.callCardPaymentBeginTransaction();
                                });
                        } else {
                            PaymentHelper.callCardPaymentBeginTransaction();
                        }
                    });
            }
        }

        /**
         * Invokes card payment begin transaction.
         */
        private static callCardPaymentBeginTransaction(): void {

            if (Commerce.Peripherals.instance
                && Commerce.Peripherals.instance.cardPayment
                && StringExtensions.isNullOrWhitespace(Commerce.Peripherals.instance.cardPayment.lockToken)) {

                Commerce.Peripherals.instance.cardPayment.beginTransaction()
                    .fail((errors: Commerce.Model.Entities.Error[]) => {
                        // If there was no payment terminal begin transaction error or if the error is different from this error then display it.
                        if (ObjectExtensions.isNullOrUndefined(PaymentHelper.paymentTerminalBeginTransactionError) ||
                            StringExtensions.compare(PaymentHelper.paymentTerminalBeginTransactionError.ErrorCode, errors[0].ErrorCode)) {
                               if (Session.instance.getErrorDisplayState(Commerce.ErrorsDisplayedPerSession.CardPaymentBeginTransaction)) {
                                    NotificationHandler.displayClientErrorsWithShowAgain(errors, "string_7201") // Payment error
                                        .done((result: Commerce.IMessageResult) => {
                                            Session.instance.setErrorDisplayState(Commerce.ErrorsDisplayedPerSession.CardPaymentBeginTransaction,
                                                !result.messageCheckboxChecked);
                                    });
                                }
                        // If the error is the same then set the error display state to be the same.
                        } else {
                            Session.instance.setErrorDisplayState(Commerce.ErrorsDisplayedPerSession.CardPaymentBeginTransaction,
                                Session.instance.getErrorDisplayState(Commerce.ErrorsDisplayedPerSession.PaymentTerminalBeginTransaction));
                        }
                });
            }
        }
    }
}