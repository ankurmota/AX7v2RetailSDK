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
     * Options passed to the CreateCustomerQuote operation.
     */
    export interface ICreateCustomerQuoteOperationOptions extends IOperationOptions {
        cart: Model.Entities.Cart;
        quotationExpirationDate: Date;
    }

    /**
     * Handler for the CreateCustomerQuote operation.
     */
    export class CreateCustomerQuoteOperationHandler extends OperationHandlerBase {
        /**
         * Executes the CreateCustomerQuote operation.
         *
         * @param {ICreateCustomerQuoteOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: ICreateCustomerQuoteOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { cart: undefined, quotationExpirationDate: undefined };

            // validation
            var errors: Commerce.Model.Entities.Error[] = CreateCustomerQuoteOperationHandler.preOperationValidation(options);
            if (ArrayExtensions.hasElements(errors)) {
                return AsyncResult.createRejected(errors);
            }

            // if mode is already customer quote, avoid going to the server
            if (CustomerOrderHelper.isQuote(options.cart)) {
                return VoidAsyncResult.createResolved();
            }

            // update cart mode to become quotation
            var newCart: Model.Entities.Cart = new Model.Entities.CartClass({
                Id: options.cart.Id,
                CustomerOrderModeValue: Model.Entities.CustomerOrderMode.QuoteCreateOrEdit,
                CartTypeValue: Model.Entities.CartType.CustomerOrder
            });

            if (!ObjectExtensions.isNullOrUndefined(options.quotationExpirationDate)) {
                newCart.QuotationExpiryDate = options.quotationExpirationDate;
            }

            return this.cartManager.createOrUpdateCartAsync(newCart);
        }

        /**
         * Checks that the cart is valid to be converted to a quotation.
         *
         * Checks executed:
         * 1. The cart is not an existing customer order
         * 2. Cart lines are valid
         * 
         * @param {ICreateCustomerQuoteOperationOptions} options The create quote operation options.
         * @return {Commerce.Model.Entities.Error[]} The list of errors found, null if there was no errors.
         */
        private static preOperationValidation(options: ICreateCustomerQuoteOperationOptions): Commerce.Model.Entities.Error[] {
            var errors: Commerce.Model.Entities.Error[] = [];

            if (CustomerOrderHelper.isCustomerOrder(options.cart) && !CustomerOrderHelper.isQuote(options.cart)) {
                errors.push(new Commerce.Model.Entities.Error(ErrorTypeEnum.FINISH_TRANSACTION_BEFORE_STARTING_ANOTHER));
                return errors;
            }

            // Check each cart line
            errors = errors.concat(CustomerOrderHelper.validateCartLinesForCustomerOrder(options.cart));
            return errors;
        }
    }
}
