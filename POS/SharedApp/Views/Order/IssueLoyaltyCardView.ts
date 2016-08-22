/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.ViewModels.d.ts'/>
///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../Merchandising/SearchView.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    export interface IIssueLoyaltyCardViewControllerOptions {
        customer?: Commerce.Model.Entities.Customer;
        loyaltyCard?: Commerce.Model.Entities.LoyaltyCard;
    }

    /**
     * Represents the Issue Loyalty Card view controller.
     */
    export class IssueLoyaltyCardViewController extends ViewControllerBase {
        // private state properties
        private _activateOrIssueLoyaltyCardInAction: boolean;

        // view models
        public issueLoyaltyCardViewModel: Commerce.ViewModels.IssueLoyaltyCardViewModel;

        // state properties
        public enableLoyaltyCardAction: Computed<boolean>;
        public enableIssueLoyaltyCardAction: Computed<boolean>;
        public indeterminateWaitVisible: Observable<boolean>;
        public showAccountDetails: Computed<boolean>;
        public customer: Observable<Model.Entities.Customer>;
        public customerName: Computed<string>;
        public customerAccountNumber: Computed<string>;
        private _layoutData = { ShowName: true, ShowAccountNumber: true };

        // issue loyalty card type properties
        public selectCardNumberInput: Observable<any>;

        // common header properties
        public commonHeaderData: Commerce.Controls.CommonHeaderData;

        /**
         * constructor
         *
         * @param {IIssueLoyaltyCardViewControllerOptions} options Setup data for the view
         * 
         * Supported option properties:
         * {Commerce.Model.Entities.Customers} customer The customer for the loyalty card
         * {Commerce.Model.Entities.LoyaltyCard} loyaltyCard The loyalty card
         *
         * Comment:
         * For some tasks, options is sent as a parameter to the next page in navigation and may contain additional properties
         * not used by this view.
         */
        constructor(options?: IIssueLoyaltyCardViewControllerOptions) {
            super(true);

            // private state properties
            this._activateOrIssueLoyaltyCardInAction = false;

            // Set the values from the options
            this.customer = ko.observable(null);
            var loyaltyCard: Commerce.Model.Entities.LoyaltyCard = null;
            if (!ObjectExtensions.isNullOrUndefined(options)) {
                this.customer(options.customer);
                loyaltyCard = options.loyaltyCard;
            }

            // Set the customer display fields
            this.customerName = ko.computed(() => {
                return ObjectExtensions.isNullOrUndefined(this.customer()) || ObjectExtensions.isNullOrUndefined(this.customer().Name) ? StringExtensions.EMPTY : this.customer().Name;
            }, this);
                
            this.customerAccountNumber = ko.computed(() => {
                return ObjectExtensions.isNullOrUndefined(this.customer()) || ObjectExtensions.isNullOrUndefined(this.customer().AccountNumber) ? StringExtensions.EMPTY : this.customer().AccountNumber;
            }, this);

            // view models
            this.issueLoyaltyCardViewModel = new Commerce.ViewModels.IssueLoyaltyCardViewModel(this.customer(), loyaltyCard);

            // Show only fields that are available.
            // Different ways of getting to this view may bring different details set.
            if (!ObjectExtensions.isNullOrUndefined(this.customer())) {
                this._layoutData = <any>{
                    ShowImage: true,
                    ShowName: true,
                    ShowAccountNumber: true,
                    ShowLoyaltyCard: !ObjectExtensions.isNullOrUndefined(loyaltyCard),
                    ShowBalance: !ObjectExtensions.isNullOrUndefined(this.customer().Balance),
                    ShowCreditLimit: !ObjectExtensions.isNullOrUndefined(this.customer().CreditLimit),
                    AddressType: ArrayExtensions.hasElements(this.customer().Addresses) ? 1 : 0
                };
            }

            // state properties
            var self = this;
            this.enableLoyaltyCardAction = ko.computed(() => {
                return self.issueLoyaltyCardViewModel.isLoyaltyCardInformationComplete() && !self._activateOrIssueLoyaltyCardInAction
            });
            this.enableIssueLoyaltyCardAction = ko.computed(() => {
                return self.enableLoyaltyCardAction() && self.issueLoyaltyCardViewModel.canIssueLoyaltyCard();
            });
            this.indeterminateWaitVisible = ko.observable<boolean>(false);
            this.showAccountDetails = ko.computed(() => {
                var customer: Model.Entities.Customer = this.customer();
                return !ObjectExtensions.isNullOrUndefined(customer) && !StringExtensions.isNullOrWhitespace(customer.AccountNumber);
            });

            // issue loyalty card type properties
            this.selectCardNumberInput = ko.observable(() => { });

            // load common header 
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.commonHeaderData.viewSectionInfo(true);
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_4700"));  // Issue Loyalty Card
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_4704")); // Enter loyalty card
        }

        /**
         * Called when view is shown.
         */
        public onShown() {
            this.enablePageEvents();
        }

        /**
         * Called when view is hidden.
         */
        public onHidden() {
            this.disablePageEvents();
        }

        /**
         * Disable the page events
         */
        public disablePageEvents() {
            Commerce.Peripherals.instance.barcodeScanner.disableAsync();
        }

        /**
         * Enable the page events
         */
        public enablePageEvents() {
            Commerce.Peripherals.instance.barcodeScanner.enableAsync(
                (barcode) => {
                    if (!ObjectExtensions.isNullOrUndefined(barcode)) {
                        this.issueLoyaltyCardViewModel.updateLoyaltyCard(barcode);
                    }
                });
        }

        /**
         * Activates the loyalty card - activates the loyalty card but does not add it to the transaction
         */
        private activateLoyaltyCard() {
            this.indeterminateWaitVisible(true);
            this._activateOrIssueLoyaltyCardInAction = true;

            this.issueLoyaltyCardViewModel.activateLoyaltyCardAsync()
                .done((loyaltyCard: Model.Entities.LoyaltyCard) => { this.activateLoyaltyCardSuccess(loyaltyCard); })
                .fail((errors) => { this.activateLoyaltyCardFailed(errors); });

        }

        /**
         * Issues the loyalty card - activates the loyalty card and adds it to the transaction
         */
        private issueLoyaltyCard(numPadResult: Controls.NumPad.INumPadResult): void {
            this.indeterminateWaitVisible(true);
            this._activateOrIssueLoyaltyCardInAction = true;
            var self = this;

            var issuedLoyaltyCard: Model.Entities.LoyaltyCard = null;
            var navigateToCartView: boolean = false;
            var asyncQueue = new AsyncQueue()
                .enqueue(() => {
                    return this.issueLoyaltyCardViewModel.activateLoyaltyCardAsync()
                        .done((loyaltyCard: Model.Entities.LoyaltyCard) => {
                            issuedLoyaltyCard = loyaltyCard;
                        });
                }).enqueue(() => {
                    return this.issueLoyaltyCardViewModel.addLoyaltyCardToCart()
                        .fail((errors) => {
                            navigateToCartView = true;
                        });
                });

            asyncQueue.run()
                .done(() => {
                    self.activateLoyaltyCardSuccess(issuedLoyaltyCard);
                })
                .fail((errors) => {
                    self.activateLoyaltyCardFailed(errors, navigateToCartView);
                });
        }

        /**
         * Action to take for issue loyalty card and activate loyalty card if the loyalty card activation was successful
         */
        private activateLoyaltyCardSuccess(loyaltyCard: Model.Entities.LoyaltyCard) {
            Commerce.ViewModelAdapter.navigate("CartView");
            this._activateOrIssueLoyaltyCardInAction = false;
            this.indeterminateWaitVisible(false);
        }

        /**
         * Action to take for issue loyalty card and activate loyalty card if the loyalty card activation failed.
         *
         * @param {(errors: Model.Entities.Error[]) => void} errorCallback Errors callback.
         * @param {boolean} [navigateToCartView] Whether or not to navigate to the cart view.
         */
        private activateLoyaltyCardFailed(errors: Commerce.Model.Entities.Error[], navigateToCartView: boolean = false) {
            this.indeterminateWaitVisible(false);
            Commerce.NotificationHandler.displayClientErrors(errors);
            this._activateOrIssueLoyaltyCardInAction = false;

            if (navigateToCartView) {
                ViewModelAdapter.navigate("CartView");
            }
        }

        //
        // Customer/Account methods
        //

        /**
         * Change customer account.
         */
        private changeCustomerAccount(): void {
            this.searchCustomers();
        }

        /**
         * Search customers.
         */
        private searchCustomers(): void {
            // Create the options object to send
            var options: any = {
                searchEntity: "Customers",
                addModeEnum: ViewModels.ProductAddModeEnum.None,
                customerAddModeEnum: ViewModels.CustomerAddModeEnum.IssueLoyaltyCard,
                customer: this.issueLoyaltyCardViewModel.customer,
                loyaltyCard: ObjectExtensions.unwrapObservableProxyObject(this.issueLoyaltyCardViewModel.loyaltyCard)
            }

            Commerce.ViewModelAdapter.navigate("SearchView", options);
        }

        /**
         * Create new customer.
         */
        private createNewCustomer(): void {
            var navigateBackOptions: IIssueLoyaltyCardViewControllerOptions = {
                loyaltyCard: this.issueLoyaltyCardViewModel.loyaltyCard,
                customer: this.customer()
            };

            var customerAddOptions: Operations.ICustomerAddOperationOptions = { destination: "IssueLoyaltyCardView", destinationOptions: navigateBackOptions };

            Operations.OperationsManager.instance.runOperation(
                Commerce.Operations.RetailOperation.CustomerAdd,
                customerAddOptions);
        }

        /**
         * Navigates to customer details.
         */
        public navigateToCustomerDetails(): void {
            var parameters: ICustomerDetailsViewOptions = {
                accountNumber: this.customerAccountNumber(),
                destination: "CartView",
                destinationOptions: null
            };

            Commerce.ViewModelAdapter.navigate("CustomerDetailsView", parameters);
        }

        /**
         * Remove the customer for the loyalty card.
         */
        public removeCustomer(): void {
            // Update model to remove a customer
            this.issueLoyaltyCardViewModel.customer = null;
            this.customer(null);
            this.indeterminateWaitVisible(false);
        }


        //
        // Swipe methods
        //

        /**
         * Handles the card swipe.
         *
         * @param {string} cardNumber The card number.
         * @param {string} month The month.
         * @param {string} year The year.
         * @param {string} t1 The track 1 data.
         * @param {string} t2 The track 2 data.
         * @param {string} t3 The track 3 data.
         */
        private handleCardSwipe(cardNumber?: string, month?: number, year?: number, t1?: string, t2?: string, t3?: string): void {
            if (!ObjectExtensions.isNullOrUndefined(cardNumber)) {
                this.issueLoyaltyCardViewModel.updateLoyaltyCard(cardNumber);
            }
        }
    }
}