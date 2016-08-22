/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ViewModelBase.ts'/>

module Commerce.ViewModels {
    "use strict";

    /**
     * Represents the issue loyalty card view model.
     */
    export class IssueLoyaltyCardViewModel extends ViewModelBase {
        // Public properties
        public loyaltyCard: any;
        public isLoyaltyCardInformationComplete: Computed<boolean>;
        public canIssueLoyaltyCard: Computed<boolean>;

        // Private members
        private _customer: Commerce.Model.Entities.Customer;
        private _customerAddress: Commerce.Model.Entities.Address;

        /**
         * Gets the customer.
        */
        public get customer(): Commerce.Model.Entities.Customer {
            return this._customer;
        }

        /**
         * Gets the customer address.
        */
        public get customerAddress(): Commerce.Model.Entities.Address {
            return this._customerAddress;
        }

        /**
         * Sets the customer values
        */
        public set customer(value: Commerce.Model.Entities.Customer) {
            this.loyaltyCard.CustomerAccount(value ? value.AccountNumber : undefined);
            this._customerAddress = CustomerHelper.getPrimaryCustomerAddressFromCustomerObject(value);
            this._customer = value;
        }

        /**
         * Instantiates the loyalty card view model.
         * If customer and loyalty card is provided, will set the customer of the loyalty card to the provided customer
         *
         * @param {Commerce.Model.Entities.Customer} [customer] The default customer.
         * @param {Commerce.Model.Entities.LoyaltyCard} [loyaltyCard] The default loyalty card.
         */
        constructor(customer?: Commerce.Model.Entities.Customer, loyaltyCard?: Commerce.Model.Entities.LoyaltyCard) {
            super();
            // Set loyalty card to use the provided customer
            if (customer && loyaltyCard) {
                loyaltyCard.CustomerAccount = customer.AccountNumber;
            }

            // Initialize loyalty card data to default value;
            this.initializeLoyaltyCard(loyaltyCard);

            // Initialize common data
            this.customer = customer;
            this.isLoyaltyCardInformationComplete = ko.computed(this.computeIsLoyaltyCardInformationComplete, this);
            this.canIssueLoyaltyCard = ko.computed(this.computeCanIssueLoyaltyCard, this);
        }

        /**
         * Validates whether the loyalty card information is complete for processing.
         *
         * @return {boolean} True: loyalty card information is complete; false: otherwise.
         */
        public computeIsLoyaltyCardInformationComplete(): boolean {
            return !StringExtensions.isNullOrWhitespace(this.loyaltyCard.CardNumber());
        }

        /**
         * Checks whether the loyalty card can be issued
         *
         * @return {boolean} True: Loyalty card can be issued; false: otherwise.
         */
        public computeCanIssueLoyaltyCard(): boolean {
            // A loyalty card can be issued if the information is complete and will not conflict with the customer on the cart
            var hasNoCustomerConflict: boolean = true;
            var cart: Commerce.Model.Entities.Cart = Commerce.Session.instance.cart;
            if (!ObjectExtensions.isNullOrUndefined(this.customer) && !StringExtensions.isNullOrWhitespace(this.customer.AccountNumber)) {
                if (!ObjectExtensions.isNullOrUndefined(cart) && !StringExtensions.isNullOrWhitespace(cart.CustomerId)) {
                    hasNoCustomerConflict = this.customer.AccountNumber == cart.CustomerId;
                }
            }

            return hasNoCustomerConflict;
        }

        /**
         * Updates the loyalty card for the card swipe. 
         */
        public updateLoyaltyCard(cardNumber?: string): void {
            this.loyaltyCard.CardNumber(cardNumber);
        }

        /**
         * Initializes loyalty card.
         *
         * @param {Commerce.Model.Entities.LoyaltyCard} [loyaltyCard] The loyalty card.
         */
        private initializeLoyaltyCard(loyaltyCard?: Commerce.Model.Entities.LoyaltyCard): void {
            if (ObjectExtensions.isNullOrUndefined(loyaltyCard)) {
                this.loyaltyCard = <Commerce.Model.Entities.LoyaltyCard>{
                    CardNumber: "",
                    CustomerAccount: ""
                };
            } else {
                this.loyaltyCard = loyaltyCard;
            }

            this.loyaltyCard = ObjectExtensions.cloneToObservableProxyObject(this.loyaltyCard);
            if (StringExtensions.isNullOrWhitespace(this.loyaltyCard.CustomerAccount())) {
                this.loyaltyCard.CustomerAccount(null);
            }
        }

        /**
         * Activates loyalty card.
         *
         * @return {IAsyncResult} The async result.
         */
        public activateLoyaltyCardAsync(): IAsyncResult<Model.Entities.LoyaltyCard> {
            var loyaltyCard = ObjectExtensions.unwrapObservableProxyObject(this.loyaltyCard);

            // Create the loyalty card to issue
            var loyaltyCardToIssue: Commerce.Model.Entities.LoyaltyCard = <Commerce.Model.Entities.LoyaltyCard> {
                CardNumber: loyaltyCard.CardNumber,
                CustomerAccount: ObjectExtensions.isNullOrUndefined(loyaltyCard.CustomerAccount) ? undefined : loyaltyCard.CustomerAccount
            };

            // Issue the loyalty card and set the account information on the cart if account information
            // is provided
            return this.customerManager.issueLoyaltyCardAsync(loyaltyCard)
                .done((result) => {
                    this.loyaltyCard = result;
                });
        }

        /**
         * Clear customer from the cart
         *
         * @return {IVoidAsyncResult } async result.
         */
        public removeCustomerFromCartAsync(): IVoidAsyncResult {
            return Commerce.Operations.OperationsManager.instance.runOperation(Commerce.Operations.RetailOperation.CustomerClear, this)
                .done(() => { this.customer = null; });
        }

        /**
         * Adds the previously issued loyalty card to the cart.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public addLoyaltyCardToCart(): IVoidAsyncResult {
            var loyaltyCard = ObjectExtensions.unwrapObservableProxyObject(this.loyaltyCard);
            var options: Operations.IAddLoyaltyCardOperationOptions = { loyaltyCard: loyaltyCard, customer: this.customer };
            return Operations.OperationsManager.instance.runOperation(Operations.RetailOperation.LoyaltyRequest, options);
        }
    }
}