/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Operations {
    "use strict";

    import Entities = Model.Entities;

    /**
     * Operation validators
     */
    export class Validators {
        /**
         * Function to ensure there is only one cart line present in an incoming collection.
         * @param {Model.Entities.CartLine[]} collection The collection of cart lines to validate.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static singleCartLineOperationValidator(collection: Model.Entities.CartLine[]): Entities.Error[] {
            var validationErrors: Entities.Error[] = [];

            if (ObjectExtensions.isNullOrUndefined(collection)) {
                validationErrors.push(new Entities.Error(ErrorTypeEnum.OPERATION_VALIDATION_INVALID_ARGUMENTS));
            } else if (collection.length > 1) {
                validationErrors.push(new Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_MULTIPLE_CART_LINES));
            } else if (collection.length === 0) {
                validationErrors.push(new Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_NO_CART_LINE_SELECTED));
            }

            return validationErrors;
        }

        /**
         * Function to ensure that the item allows price override.
         * @param {Model.Entities.CartLine[]} collection The collection of cart lines to validate.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static itemAllowsPriceOverrideOperationValidator(collection: Model.Entities.CartLine[]): Entities.Error[] {
            var validationErrors: Entities.Error[] = Validators.singleCartLineOperationValidator(collection);

            if (validationErrors.length === 0) {
                var cartLine: Model.Entities.CartLine = collection[0];
                var product: Entities.SimpleProduct = Session.instance.getFromProductsInCartCache(cartLine.ProductId);
                if (!ObjectExtensions.isNullOrUndefined(product)) {
                    if (product.Behavior.KeyInPriceValue === Proxy.Entities.KeyInPriceRestriction.NotAllowed) {
                        validationErrors.push(new Entities.Error(ErrorTypeEnum.PRICE_OVERRIDE_NOT_ALLOWED_FOR_PRODUCT));
                    }
                }
            }

            return validationErrors;
        }

        /**
         * Function to ensure that the cart line is not voided.
         * @param {Model.Entities.CartLine[]} collection The collection of cart lines to validate.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static nonVoidedOperationValidator(collection: Model.Entities.CartLine[]): Entities.Error[] {
            var validationErrors: Entities.Error[] = Validators.singleCartLineOperationValidator(collection);

            if (validationErrors.length === 0 && collection[0].IsVoided === true) {
                validationErrors.push(new Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_PRODUCT_IS_VOIDED));
            }

            return validationErrors;
        }

        /**
         * Function to ensure that the cart line is not from a gift certificate.
         * @param {Model.Entities.CartLine[]} collection The collection of cart lines to validate.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static notFromAGiftCertificateOperationValidator(collection: Model.Entities.CartLine[]): Entities.Error[] {
            var validationErrors: Entities.Error[] = Validators.singleCartLineOperationValidator(collection);

            if (validationErrors.length === 0 && collection[0].IsGiftCardLine) {
                validationErrors.push(new Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_FOR_A_GIFT_CARD));
            }

            return validationErrors;
        }

        /**
         * Function to ensure that the cart line is not from a receipt.
         * @param {Model.Entities.CartLine[]} collection The collection of cart lines to validate.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static notFromAReceiptOperationValidator(collection: Model.Entities.CartLine[]): Entities.Error[] {
            var validationErrors: Entities.Error[] = Validators.singleCartLineOperationValidator(collection);

            if (validationErrors.length === 0 && CartLineHelper.isFromAReceipt(collection[0])) {
                validationErrors.push(new Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_PRODUCT_IS_FOR_A_RECEIPT));
            }

            return validationErrors;
        }

        /**
         * Function to ensure that the cart line does not have an overriden price.
         * @param {Model.Entities.CartLine[]} collection The collection of cart lines to validate.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static notHaveOverridenPrice(collection: Model.Entities.CartLine[]): Entities.Error[] {
            var validationErrors: Entities.Error[] = Validators.singleCartLineOperationValidator(collection);

            if (validationErrors.length === 0 && collection[0].IsPriceOverridden) {
                validationErrors.push(new Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_PRICE_IS_OVERRIDDEN));
            }

            return validationErrors;
        }

        /**
         * Function to ensure that the cart line does not have a product that does not allow quantity update.
         * @param {Model.Entities.CartLine[]} collection The collection of cart lines to validate.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static notAllowQuantityUpdate(collection: Model.Entities.CartLine[]): Entities.Error[] {
            var validationErrors: Entities.Error[] = Validators.singleCartLineOperationValidator(collection);

            if (validationErrors.length === 0) {
                var cartLine: Model.Entities.CartLine = collection[0];
                var product: Entities.SimpleProduct = Session.instance.getFromProductsInCartCache(cartLine.ProductId);
                if (!ObjectExtensions.isNullOrUndefined(product)) {
                    if (product.Behavior.KeyInQuantityValue === Proxy.Entities.KeyInQuantityRestriction.NotAllowed) {
                        validationErrors.push(new Entities.Error(ErrorTypeEnum.UNIT_OF_MEASURE_NOT_VALID_ITEM_NOT_ALLOW_QUANTITY_UPDATE));
                    }
                }
            }

            return validationErrors;
        }

        /**
         * Function to ensure there is only one payment line is present in an incoming collection.
         * @param {Model.Entities.TenderLine[]} collection The collection of tender lines to validate.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static singlePaymentLineOperationValidator(collection: Model.Entities.TenderLine[]): Entities.Error[] {
            var validationErrors: Entities.Error[] = [];

            if (ObjectExtensions.isNullOrUndefined(collection)) {
                validationErrors.push(new Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_NO_PAYMENT_LINE_SELECTED));
            } else if (collection.length > 1) {
                validationErrors.push(new Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_MULTIPLE_PAYMENT_LINES));
            } else if (collection.length === 0) {
                validationErrors.push(new Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_NO_PAYMENT_LINE_SELECTED));
            }

            return validationErrors;
        }

        /**
         * Function to ensure that there is no current transaction.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static noExistingCart(): Entities.Error[] {
            var validationErrors: Entities.Error[] = [];
            if (Session.instance.isCartInProgress) {
                validationErrors.push(new Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_FINISH_CURRENT_TRANSACTION));
            }

            return validationErrors;
        }

        /**
         * Function to ensure that there is a current transaction.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static existingCart(): Entities.Error[] {
            var validationErrors: Entities.Error[] = [];
            if (!Session.instance.isCartInProgress) {
                validationErrors.push(new Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_NO_CURRENT_TRANSACTION));
            }

            return validationErrors;
        }

        /**
         * Function to ensure that if the transaction is a customer order or quote is in the create or edit state.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static isCustomerOrderOrQuoteInCreateOrEditState(): Entities.Error[] {
            var validationErrors: Entities.Error[] = [];
            var cart: Model.Entities.Cart = Session.instance.cart;

            if (CustomerOrderHelper.isCustomerOrder(cart)
                && !CustomerOrderHelper.isCustomerOrderOrQuoteCreationOrEdition(cart)) {
                validationErrors.push(new Entities.Error(
                    ErrorTypeEnum.CUSTOMER_ORDER_OPERATION_PICKUP_CANCEL_RETURN_NOT_SUPPORTED));
            }

            return validationErrors;
        }

        /**
         * Function to ensure that if the transaction is a customer order or quote it is in the return state.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static notAllowedOnNonReturnCustomerOrderOperationValidator(): Entities.Error[] {
            var cart: Model.Entities.Cart = Session.instance.cart;
            var errors: Model.Entities.Error[] = [];

            if (Session.instance.isCartInProgress
                && cart.CartTypeValue === Model.Entities.CartType.CustomerOrder
                && cart.CustomerOrderModeValue !== Model.Entities.CustomerOrderMode.Return) {
                errors.push(new Model.Entities.Error(ErrorTypeEnum.ORDERS_CANNOT_INCLUDE_RETURNS));
            }

            return errors;
        }

        /**
         * Function to ensure that operation is not allowed in offline state.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static notAllowedInOffline(): Entities.Error[] {
            var validationErrors: Entities.Error[] = [];
            if (Session.instance.connectionStatus !== ConnectionStatusType.Online) {
                validationErrors.push(new Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_IN_OFFLINE_STATE));
            }

            return validationErrors;
        }

        /**
         * Function to ensure that the cart is not for an income/expense transaction.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static notIncomeExpenseTransaction(): Entities.Error[] {
            var validationErrors: Entities.Error[] = [];
            var cart: Model.Entities.Cart = Session.instance.cart;
            if (CartHelper.isCartType(cart, Commerce.Model.Entities.CartType.IncomeExpense)) {
                validationErrors.push(new Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_INCOME_EXPENSE_TRANSACTION));
            }

            return validationErrors;
        }

        /**
         * Function to ensure that no voided cart lines are present.
         * @param {Model.Entities.CartLine[]} cartLines The cart lines to validate.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static notAllowedOnVoidedCartLinesOperationValidator(cartLines: Model.Entities.CartLine[]): Entities.Error[] {
            var errors: Model.Entities.Error[] = [];

            if (cartLines.some((cartLine: Entities.CartLine) => cartLine.IsVoided)) {
                errors.push(new Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_PRODUCT_IS_VOIDED));
            }

            return errors;
        }

        /**
         * Function to ensure that no gift card cart lines are present.
         * @param {Model.Entities.CartLine[]} cartLines The cart lines to validate.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static notAllowedOnGiftCardCartLinesOperationValidator(cartLines: Model.Entities.CartLine[]): Entities.Error[] {
            var errors: Model.Entities.Error[] = [];

            if (cartLines.some((cartLine: Entities.CartLine) => cartLine.IsGiftCardLine)) {
                errors.push(new Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_FOR_A_GIFT_CARD));
            }

            return errors;
        }

        /**
         * Function to ensure that the cart line does not have a serialized product.
         * @param {Model.Entities.CartLine[]} cartLines The cart lines to validate.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static notAllowedOnSerializedProductCartLinesOperationValidator(cartLines: Model.Entities.CartLine[]): Entities.Error[] {
            var errors: Model.Entities.Error[] = [];

            cartLines.forEach((cartLine: Entities.CartLine) => {
                var product: Model.Entities.SimpleProduct = Session.instance.getFromProductsInCartCache(cartLine.ProductId);

                var isProduct: boolean = !ObjectExtensions.isNullOrUndefined(product);

                // Check that the product is not a serialized item and
                // active in the sales process for the product tracking group (MustPromptForSerialNumberOnlyAtSale)
                if ((!ObjectExtensions.isNullOrUndefined(cartLine.SerialNumber)
                    && !StringExtensions.isEmptyOrWhitespace(cartLine.SerialNumber))
                    || (isProduct && product.Behavior.MustPromptForSerialNumberOnlyAtSale)) {
                    errors.push(new Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_FOR_A_SERIALIZED_ITEM));
                }

                return;
            });

            return errors;
        }

        /**
         * Function to ensure that the product on the cart line allows quantity to be keyed in.
         * @param {Model.Entities.CartLine[]} cartLines The cart lines to validate.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static notAllowedToKeyInQuantityOnProductCartLinesOperationValidator(cartLines: Model.Entities.CartLine[]): Entities.Error[] {
            var errors: Model.Entities.Error[] = [];

            cartLines.forEach((cartLine: Entities.CartLine) => {
                var product: Model.Entities.SimpleProduct = Session.instance.getFromProductsInCartCache(cartLine.ProductId);
                var isProduct: boolean = !ObjectExtensions.isNullOrUndefined(product);

                // Check that the item does not allow quantity override
                if (isProduct
                    && product.Behavior.KeyInQuantityValue === Proxy.Entities.KeyInQuantityRestriction.NotAllowed) {
                    errors.push(new Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_KEY_IN_QUANTITY_NOT_ALLOWED_FOR_ITEM));
                }
                return;
            });

            return errors;
        }

        /**
         * Function to ensure that no customer account tender lines exist on the current cart.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static notAllowedOnCartWithCustomerAccountTenderLineOperationValidator(): Entities.Error[] {
            var cart: Commerce.Model.Entities.Cart = Session.instance.cart;

            // Check that a payment by account is not on the transaction
            var errors: Commerce.Model.Entities.Error[] = [];
            cart.TenderLines.forEach((tenderLine: Entities.TenderLine) => {
                if (parseInt(tenderLine.TenderTypeId, 10) === Model.Entities.TenderTypeId.CustomerAccount
                    && tenderLine.StatusValue !== Model.Entities.TenderLineStatus.Voided) {
                    errors.push(new Entities.Error(ErrorTypeEnum.CANNOT_REMOVE_CUSTOMER_PARTIAL_ORDER));
                    return;
                }
            });

            return errors;
        }

        /**
         * Function to ensure that the current cart has a customer account set.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static notAllowedOnCartWithoutCustomerAccountOperationValidator(): Entities.Error[] {
            var errors: Proxy.Entities.Error[] = [];

            if (StringExtensions.isNullOrWhitespace(Session.instance.cart.CustomerId)) {
                errors.push(new Entities.Error("string_29341"));
            }

            return errors;
        }

        /**
         * Function to ensure that the current cart does not contain any non customer account deposit line items in any state.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static notAllowedOnCartWithNonCustomerAccountDepositLines(): Entities.Error[] {
            var cart: Proxy.Entities.Cart = Session.instance.cart;
            var errors: Proxy.Entities.Error[] = [];

            if (ArrayExtensions.hasElements(cart.CartLines) || ArrayExtensions.hasElements(cart.IncomeExpenseLines)) {
                errors.push(new Entities.Error(ErrorTypeEnum.CUSTOMERACCOUNTDEPOSIT_MULTIPLECARTLINESNOTALLOWED));
            }

            return errors;
        }

        /**
         * Function to ensure that the current cart is not a customer account deposit.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static notAllowedOnCustomerAccountDeposit(): Entities.Error[] {
            var cart: Entities.Cart = Session.instance.cart;
            var errors: Entities.Error[] = [];

            // Check state
            if (ObjectExtensions.isNullOrUndefined(cart)) {
                RetailLogger.coreOperationValidatorsNoCartOnCartValidator("notAllowedOnCustomerAccountDeposit");
            }

            // Check that the cart is not a customer account deposit
            if (CartHelper.isCartType(cart, Entities.CartType.AccountDeposit)) {
                errors.push(
                    new Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_CUSTOMER_ACCOUNT_DEPOSIT));
            }

            return errors;
        }

        /**
         * Function to ensure that a new customer is not being added to a customer account deposit.
         * @param {string} customerId The id of the customer.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static notAllowedOnCustomerAccountDepositWithNewCustomer(customerId: string): Entities.Error[] {
            var cart: Entities.Cart = Session.instance.cart;
            var errors: Entities.Error[] = [];

            // Check state
            if (ObjectExtensions.isNullOrUndefined(cart)) {
                RetailLogger.coreOperationValidatorsNoCartOnCartValidator("notAllowedOnCustomerAccountDepositWithNewCustomer");
            }

            // Check that the cart is not a customer account deposit
            if (CartHelper.isCartType(cart, Entities.CartType.AccountDeposit) && (!CartHelper.isCustomerOnCart(cart, customerId))) {
                errors.push(
                    new Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_CUSTOMER_ACCOUNT_DEPOSIT));
            }

            return errors;
        }

        /**
         * Function to ensure that the current cart is not a recalled order.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static notAllowedOnRecalledOrder(): Entities.Error[] {
            var cart: Entities.Cart = Session.instance.cart;
            var errors: Entities.Error[] = [];
            // Check that the transaction is not a recalled order
            if (cart.CartTypeValue === Commerce.Model.Entities.CartType.CustomerOrder
                && !StringExtensions.isNullOrWhitespace(cart.SalesId)) {
                errors.push(
                    new Entities.Error(ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CANNOTCHANGECUSTOMERIDWHENEDITINGCUSTOMERORDER));
            }

            return errors;
        }

        /**
         * Function to ensure that no invalid cart lines are being returned.
         * @param {Model.Entities.CartLine[]} cartLines The cart lines to validate.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static returnCartLinesOperationValidator(cartLines: Model.Entities.CartLine[]): Entities.Error[] {
            var errors: Model.Entities.Error[] = [];

            if (!ArrayExtensions.hasElements(cartLines)) {
                return errors;
            }

            errors = errors.concat(
                Validators.notAllowedOnVoidedCartLinesOperationValidator(cartLines),
                Validators.notAllowedOnGiftCardCartLinesOperationValidator(cartLines));

            return errors;
        }

        /**
         * Function to ensure return limits, if existing, are adhered to.
         * @param {Operations.IReturnProductOperationOptions} options The options passed to the ReturnProduct operation handler.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static returnLimitsValidator(options: Operations.IReturnProductOperationOptions): Entities.Error[] {
            var validationErrors: Entities.Error[] = [];
            var totalReturnAmount: number = 0;

            if (Session.instance.CurrentEmployee.Permissions.MaximumLineReturnAmount > 0) {
                options.productReturnDetails.forEach((prd: Model.Entities.ProductReturnDetails): void => {
                    var lineReturnAmount: number = 0;

                    if (!ObjectExtensions.isNullOrUndefined(prd.cartLine)) {
                        lineReturnAmount = prd.cartLine.TotalAmount;
                    } else if (!ObjectExtensions.isNullOrUndefined(prd.manualReturn)) {
                        lineReturnAmount = prd.manualReturn.product.Price;
                    } else if (!ObjectExtensions.isNullOrUndefined(prd.salesLineReturn)) {
                        lineReturnAmount = prd.salesLineReturn.salesLine.TotalAmount;
                    }

                    totalReturnAmount += lineReturnAmount;

                    if (lineReturnAmount > Session.instance.CurrentEmployee.Permissions.MaximumLineReturnAmount) {
                        validationErrors.push(new Entities.Error(ErrorTypeEnum.RETURN_MAX_RETURN_LINE_AMOUNT_EXCEEDED));
                        return;
                    }
                });
            }

            if ((Commerce.Session.instance.CurrentEmployee.Permissions.MaxTotalReturnAmount > 0)
                && (totalReturnAmount > Commerce.Session.instance.CurrentEmployee.Permissions.MaxTotalReturnAmount)) {
                validationErrors.push(new Entities.Error(ErrorTypeEnum.RETURN_MAX_RETURN_TOTAL_AMOUNT_EXCEEDED));
            }

            return validationErrors;
        }

        /**
         * Function to ensure that a shift is currently assigned.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static notAllowedInNonDrawerModeOperationValidator(): Entities.Error[] {
            var validationErrors: Entities.Error[] = [];
            if (Session.instance.Shift.ShiftId === 0) {
                validationErrors.push(new Entities.Error(ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_NONDRAWEROPERATIONSONLY));
            }

            return validationErrors;
        }

        /**
         * Function to ensure that a shift is currently assigned, or that one was provided as an option to the operation.
         * @param {Entities.Shift} shift The shift provided to the operation.
         * @return {Entities.Error[]} The validation errors, if any.
         */
        public static notAllowedInNonDrawerModeUnlessShiftIsProvidedOperationValidator(shift: Entities.Shift): Entities.Error[] {
            if (ObjectExtensions.isNullOrUndefined(shift) || NumberExtensions.isNullOrZero(shift.ShiftId)) {
                return Validators.notAllowedInNonDrawerModeOperationValidator();
            } else {
                return [];
            }
        }

        /**
         * Validates whether cart is allowed for payments history operation.
         * @param {Proxy.Entities.Cart} The cart object.
         * @returns {Proxy.Entities.Error[]} The error message returns from validating the cart.
         */
        public static paymentsHistoryOperationValidator(cart: Proxy.Entities.Cart): Proxy.Entities.Error[] {
            var errors: Proxy.Entities.Error[] = [];

            // Payments history operation is allowed only on Customer Order or Quotation.
            if (!CustomerOrderHelper.isCustomerOrderEdition(cart) && !CustomerOrderHelper.isCustomerOrderPickup(cart)) {
                errors.push(new Proxy.Entities.Error(ErrorTypeEnum.EDIT_OR_PICKUP_CUSTOMER_ORDER_ONLY));
            }

            return errors;
        }
    }
}