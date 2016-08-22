/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path="../Commerce.Core.d.ts" />

module Custom.Triggers {
    "use strict";

    /**
     * Implementation of a pre-confirm return transaction trigger that validates that the transaction being returned is within the return period.
     */
    export class ValidateReturnPreConfirmReturnTransactionTrigger implements Commerce.Triggers.IPreConfirmReturnTransactionTrigger {
        private static MILLISECONDS_PER_DAY: number = 86400000;
        private static RETURN_PERIOD_IN_DAYS: number = 0;
        private static RETURN_PERIOD_ENDED_ERROR_CODE: string = "Cannot return, you are past return date";

        /**
         * Executes the trigger.
         */
        public execute(options: Commerce.Triggers.IPreConfirmReturnTransactionTriggerOptions): Commerce.IAsyncResult<Commerce.ICancelableResult> {
            var timeDiff: number = Math.abs(new Date().getTime() - options.originalTransaction.BusinessDate.getTime());
            var diffDays: number = Math.ceil(timeDiff / ValidateReturnPreConfirmReturnTransactionTrigger.MILLISECONDS_PER_DAY);
            if (diffDays > ValidateReturnPreConfirmReturnTransactionTrigger.RETURN_PERIOD_IN_DAYS) {
                var error: Commerce.Proxy.Entities.Error = new Commerce.Proxy.Entities.Error(ValidateReturnPreConfirmReturnTransactionTrigger.RETURN_PERIOD_ENDED_ERROR_CODE);
                return Commerce.AsyncResult.createRejected([error]);
            }

            return Commerce.AsyncResult.createResolved({ canceled: false });
        }
    }

    /**
     * Implementation of a pre product sale trigger that is used to ensure there are no return lines in the cart.
     */
    export class ValidateProductSalePreProductSaleTrigger implements Commerce.Triggers.IPreProductSaleTrigger {
        private static SALE_NOT_ALLOWED_IN_SAME_TRANSACTION_AS_RETURN_ERROR_CODE: string = "Return and sale not allowed in same transaction";

        /**
         * Executes the trigger.
         */
        public execute(options: Commerce.Operations.IItemSaleOperationOptions): Commerce.IAsyncResult<Commerce.ICancelableResult> {
            var hasReturnLine: boolean = Commerce.Session.instance.cart.CartLines.some((cartLine: Commerce.Proxy.Entities.CartLine): boolean => {
                return cartLine.Quantity < 0 && !cartLine.IsVoided;
            });

            var result: Commerce.AsyncResult<Commerce.ICancelableResult> = new Commerce.AsyncResult<Commerce.ICancelableResult>(null);
            if (hasReturnLine) {
                var error: Commerce.Proxy.Entities.Error =
                    new Commerce.Proxy.Entities.Error(ValidateProductSalePreProductSaleTrigger.SALE_NOT_ALLOWED_IN_SAME_TRANSACTION_AS_RETURN_ERROR_CODE);

                result.reject([error]);
            } else {
                result.resolve({ canceled: false });
            }

            return result;
        }
    }

    /**
     * Implementation of a post log on trigger that is used to perform conidtional registration of other triggers.
     */
    export class ConditionalRegistrationPostLogOnTrigger implements Commerce.Triggers.IPostLogOnTrigger {
        private static alreadyRegistered: boolean = false;

        /**
         * Executes the trigger.
         */
        public execute(options: Commerce.Triggers.IPostLogOnTriggerOptions): Commerce.IVoidAsyncResult {
            // Check to ensure the triggers have not already been registered.
            if (!ConditionalRegistrationPostLogOnTrigger.alreadyRegistered) {
                this.performRegistration();

                // Set already registered field to true to prevent duplicate trigger registration.
                ConditionalRegistrationPostLogOnTrigger.alreadyRegistered = true;
            }

            return Commerce.VoidAsyncResult.createResolved();
        }

        /**
         * Perform the conditional registration of triggers.
         */
        private performRegistration(): void {
            var conditionIsMet: boolean = true;
            if (conditionIsMet) {
                Commerce.Triggers.TriggerManager.instance.register(
                    Commerce.Triggers.CancelableTriggerType.PreConfirmReturnTransaction,
                    new ValidateReturnPreConfirmReturnTransactionTrigger());

                Commerce.Triggers.TriggerManager.instance.register(
                    Commerce.Triggers.CancelableTriggerType.PreProductSale,
                    new ValidateProductSalePreProductSaleTrigger());
            }
        }
    }
}