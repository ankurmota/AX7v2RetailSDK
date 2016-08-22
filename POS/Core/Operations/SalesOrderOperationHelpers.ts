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

    export class AddCustomerOperationHelper {

        /**
         * Merges the new affiliations passed in with the existing affiliations in the cart.
         *
         * @param {Commerce.Model.Entities.Cart} cart The cart
         * @param {string} customerId The customer Id
         * @param {Commerce.Model.Entities.AffiliationLoyaltyTier[]} newCartAffiliations The affiliations that will try to add to the cart
         * @return {Commerce.Model.Entities.AffiliationLoyaltyTier[]} The affiliation collection that need add to the cart.
         */
        public static getUpdatedCartAffiliations(cart: Model.Entities.Cart, customerId: string, newCartAffiliations: Commerce.Model.Entities.AffiliationLoyaltyTier[]): Commerce.Model.Entities.AffiliationLoyaltyTier[] {
            if (ObjectExtensions.isNullOrUndefined(cart)) {
                return [];
            }

            var updatedCartAffiliations: Model.Entities.AffiliationLoyaltyTier[] = [];

            if (ArrayExtensions.hasElements(newCartAffiliations)) {
                var isAddCustomer: boolean = StringExtensions.isNullOrWhitespace(cart.CustomerId) && !StringExtensions.isNullOrWhitespace(customerId);
                var isClearCustomer: boolean = !isAddCustomer && customerId === '';
                var isUpdateCustomer: boolean = !isAddCustomer && !isClearCustomer && customerId !== cart.CustomerId;
                var isChangedCustomer: boolean = isAddCustomer || isClearCustomer || isUpdateCustomer;

                if (isChangedCustomer) {
                    if (ArrayExtensions.hasElements(cart.AffiliationLines)) {
                        // Handling the cases of updating customer in cart and adding customer to cart.
                        if (isUpdateCustomer || isAddCustomer) {

                            // If add new customer to Cart, keep the affiliations added manually.
                            if (isAddCustomer) {
                                cart.AffiliationLines.forEach((cartAffiliationLine: Model.Entities.AffiliationLoyaltyTier) => {
                                    if (StringExtensions.isNullOrWhitespace(cartAffiliationLine.CustomerId)) {
                                        updatedCartAffiliations.push(cartAffiliationLine);
                                    }
                                });
                            }

                            // Remove affiliations added automatically with previous customer from cart.
                            // Only keep the affiliations added manually.
                            if (isUpdateCustomer) {
                                cart.AffiliationLines.forEach((cartAffiliationLine: Model.Entities.AffiliationLoyaltyTier) => {
                                    if (cartAffiliationLine.CustomerId != cart.CustomerId) {
                                        updatedCartAffiliations.push(cartAffiliationLine);
                                    }
                                });
                            }

                            // Add new affiliations to cart.
                            newCartAffiliations.forEach((cartAffiliationLine: Model.Entities.AffiliationLoyaltyTier) => {
                                var isDuplicate: boolean = updatedCartAffiliations.some((value: Model.Entities.AffiliationLoyaltyTier) => value.AffiliationId === cartAffiliationLine.AffiliationId);
                                // Add affiliation which is not in cart.
                                if (!isDuplicate) {
                                    updatedCartAffiliations.push(cartAffiliationLine);
                                }
                            });

                        } else if (isClearCustomer) {
                            // Handling the case of removing customer from cart.
                            // If affiliations were added automatically with customer, then remove these affiliations from cart,
                            // and only keep the affiliations added manually.
                            cart.AffiliationLines.forEach((cartAffiliationLine: Model.Entities.AffiliationLoyaltyTier) => {
                                if (cartAffiliationLine.CustomerId != cart.CustomerId) {
                                    updatedCartAffiliations.push(cartAffiliationLine);
                                }
                            });
                        }
                    } else {
                        updatedCartAffiliations = newCartAffiliations;
                    }
                } else {
                    updatedCartAffiliations = cart.AffiliationLines;
                }
            } else {
                updatedCartAffiliations = cart.AffiliationLines;
            }

            return updatedCartAffiliations;
        }

        /**
         * Gets the customer affiliations that are not in current Cart.
         *
         * @param {string} customerId The customer id.
         * @param {Model.Entities.CustomerAffiliation[]} customerAffiliations The customer's affiliations.
         * @return {Model.Entities.CustomerAffiliation[]} return affiliations of the customer that are not in current Cart.
         */
        public static getNotInCartCustomerAffiliations(customerId: string, customerAffiliations: Model.Entities.CustomerAffiliation[]): Model.Entities.CustomerAffiliation[] {
            if (!ArrayExtensions.hasElements(customerAffiliations) || StringExtensions.isNullOrWhitespace(customerId)) {
                return [];
            }

            var cartAffiliations: Model.Entities.AffiliationLoyaltyTier[] = Session.instance.cart.AffiliationLines;
            var notInCartCustomerAffiliations: Model.Entities.CustomerAffiliation[] = [];

            if (ArrayExtensions.hasElements(cartAffiliations)) {
                customerAffiliations.forEach((customerAffiliation: Model.Entities.CustomerAffiliation) => {
                    var isInCart: boolean = cartAffiliations.some((cartAffiliation: Model.Entities.AffiliationLoyaltyTier) => {
                        // Only check the customer affiliation already in the affiliations added manually to Cart.
                        return (StringExtensions.isNullOrWhitespace(cartAffiliation.CustomerId) && cartAffiliation.AffiliationId === customerAffiliation.RetailAffiliationId);
                    });

                    if (!isInCart) {
                        notInCartCustomerAffiliations.push(customerAffiliation);
                    }
                });
            } else {
                notInCartCustomerAffiliations = customerAffiliations;
            }

            return notInCartCustomerAffiliations;
        }
    }

    export class IssueCreditMemoOperationHelper {
        /**
         * Checks that the operation can be performed on a cart
         *
         * Checks executed:
         * 1. The amount due is less than 0
         * 
         * @param {Commerce.Model.Entities.Cart} cart The cart
         * @return {Commerce.Model.Entities.Error[]} The list of errors found, null if there was no errors.
         */
        public static preOperationValidation(cart: Commerce.Model.Entities.Cart): Commerce.Model.Entities.Error[] {
            var errors: Commerce.Model.Entities.Error[] = [];

            // For return customer order discount calculation check is not required
            // Check whether the totals need to be calculated before payment
            if (!CustomerOrderHelper.isCustomerOrderReturnOrPickup(cart)) {
                if (!ObjectExtensions.isNullOrUndefined(cart.IsDiscountFullyCalculated) && (!cart.IsDiscountFullyCalculated)) {
                    errors.push(new Commerce.Model.Entities.Error(ErrorTypeEnum.CALCULATE_TOTAL_BEFORE_PAYMENT));
                }
            }

            // Check that the amount due is less than 0
            if (isNaN(cart.AmountDue) || (cart.AmountDue >= 0)) {
                errors.push(new Commerce.Model.Entities.Error(ErrorTypeEnum.CREDIT_MEMO_INVALID_AMOUNT));
            }   

            return errors.length == 0 ? null : errors;
        }
    }

    export class PaymentOperationHelper {
        /**
         * Checks that the operation can be performed on a cart for a payment.
         *
         * @param {Operations.RetailOperation} paymentOperationId The payment operation to check.
         * @param {Commerce.Model.Entities.Cart} cart The cart.
         * @return {Commerce.Model.Entities.Error[]} The list of errors found, null if there was no errors.
         */
        public static preOperationValidation(paymentOperationId: Operations.RetailOperation, cart: Commerce.Model.Entities.Cart): Commerce.Model.Entities.Error[] {
            var errors: Commerce.Model.Entities.Error[] = [];

            if (paymentOperationId) {
                // Call the preoperation validation for the specified payment operation
                switch (paymentOperationId) {
                    case Operations.RetailOperation.PayCreditMemo:
                        errors = PayCreditMemoOperationHelper.preOperationValidation(cart);
                        break;
                }
            }

            if (errors) {
                errors = errors.length == 0 ? null : errors
            }

            return errors;
        }
    }

    export class PayCreditMemoOperationHelper {
        /**
         * Checks that the operation can be performed on a cart.
         *
         * Checks executed:
         * 1. The amount due is greater than 0.
         * 
         * @param {Commerce.Model.Entities.Cart} cart The cart.
         * @return {Commerce.Model.Entities.Error[]} The list of errors found, null if there was no errors.
         */
        public static preOperationValidation(cart: Commerce.Model.Entities.Cart): Commerce.Model.Entities.Error[] {
            var errors: Commerce.Model.Entities.Error[] = [];

            // Check that the amount due is greater than 0
            if (isNaN(cart.AmountDue) || (cart.AmountDue <= 0)) {
                errors.push(new Commerce.Model.Entities.Error(ErrorTypeEnum.PAYMENT_CREDIT_MEMO_NEGATIVE_BALANCE));
            }

            return errors.length == 0 ? null : errors;
        }
    }
}