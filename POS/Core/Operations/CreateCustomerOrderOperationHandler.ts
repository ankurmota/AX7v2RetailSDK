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
     * Options passed to the CreateCustomerOrder operation.
     */
    export interface ICreateCustomerOrderOperationOptions extends IOperationOptions {
        cart: Model.Entities.Cart;
    }

    /**
     * Handler for the CreateCustomerOrder operation.
     */
    export class CreateCustomerOrderOperationHandler extends OperationHandlerBase {
        /**
         * Checks that the cart is okay to convert to a customer order.
         *
         * Checks executed:
         * 1. The cart is not an existing customer order
         * 2. The cart lines do not contain gift card transactions
         * 3. The cart lines do not contain return transactions
         * 
         * @param {Commerce.Model.Entities.Cart} cart The cart
         * @return {Commerce.Model.Entities.Error[]} The list of errors found, null if there was no errors.
         */
        public static preOperationValidation(cart: Commerce.Model.Entities.Cart): Commerce.Model.Entities.Error[] {
            var errors: Commerce.Model.Entities.Error[] = [];

            if ((cart.CartTypeValue === Commerce.Model.Entities.CartType.CustomerOrder)
                && (cart.CustomerOrderModeValue === Model.Entities.CustomerOrderMode.Cancellation)
                || (cart.CustomerOrderModeValue === Model.Entities.CustomerOrderMode.Pickup)
                || (cart.CustomerOrderModeValue === Model.Entities.CustomerOrderMode.Return)
                || (cart.CustomerOrderModeValue === Model.Entities.CustomerOrderMode.OrderRecalled)) {
                errors.push(new Commerce.Model.Entities.Error(ErrorTypeEnum.FINISH_TRANSACTION_BEFORE_STARTING_ANOTHER));
                return errors;
            }

            // Check each cart line
            errors = CustomerOrderHelper.validateCartLinesForCustomerOrder(cart);
            return errors;
        }

        /**
         * Executes the CreateCustomerOrder operation.
         *
         * @param {ICreateCustomerOrderOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: ICreateCustomerOrderOperationOptions): IAsyncResult<IOperationResult> {
            if (!options || !options.cart) {
                throw "Operation options are required.";
            }

            var cart: Proxy.Entities.Cart = options.cart;

            // validation
            var errors: Commerce.Model.Entities.Error[] = CreateCustomerOrderOperationHandler.preOperationValidation(cart);
            if (ArrayExtensions.hasElements(errors)) {
                return AsyncResult.createRejected(errors);
            }

            var result: IAsyncResult<IOperationResult>;

            // if mode is already customer order, avoid going to the server
            if (cart.CustomerOrderModeValue === Model.Entities.CustomerOrderMode.CustomerOrderCreateOrEdit) {
                result = AsyncResult.createResolved(<IOperationResult>{ data: Session.instance.cart, canceled: false });
            } else {
                // update cart customer order mode
                result = this.cartManager.setCustomerOrderModeAsync(Model.Entities.CustomerOrderMode.CustomerOrderCreateOrEdit).map(() => {
                    return <IOperationResult>{ data: Session.instance.cart, canceled: false };
                });
            }

            return result;
        }
    }
}