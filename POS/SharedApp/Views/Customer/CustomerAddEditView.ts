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
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    /**
     * CustomerAddEditViewController constructor parameters interface
     */
    export interface ICustomerAddEditViewOptions {
        /**
         * Customer model entity
         */
        customer: Proxy.Entities.Customer;

        /**
         * Address model entity
         */
        address: Proxy.Entities.Address;

        /**
         * Destination view.
         */
        destination: string;

        /**
         * Destination view.
         */
        destinationOptions: any;
    }

    export class CustomerAddEditViewController extends ViewControllerBase {
        private customerAddEditViewModel: ViewModels.CustomerAddEditViewModel;
        private PersonFieldsVisible: Computed<boolean>;
        private _customerTypeInputEnable: Observable<boolean>
        private customerAccountNumber: string;
        private isNewCustomer: boolean = false;
        private indeterminateWaitVisible: Observable<boolean>;
        private _cartDestination: string = "CartView";
        private _saveCustomerDestination: string = "CustomerDetailsView";
        private _destinationOptions: any = {};
        public addressFilled: Computed<boolean>;
        public commonHeaderData: Controls.CommonHeaderData;
        public streetLines: Computed<string>;
        public numberOfAddressesText: Computed<string>;

        public customer: Proxy.Entities.Customer;
        private _customerCardViewModel: ViewModels.CustomerCardViewModel;

        /**
         * constructor
         * @param {ICustomerAddEditViewOptions} options Setup data for the view
         */
        constructor(options?: ICustomerAddEditViewOptions) {
            super(false);

            if (options) {
                this.customerAccountNumber = options.customer ? options.customer.AccountNumber : undefined;

                if (!StringExtensions.isNullOrWhitespace(options.destination)) {
                    this._cartDestination = options.destination;

                    if (options.destinationOptions) {
                        this._destinationOptions = options.destinationOptions;
                    }
                }
            }

            this._customerCardViewModel =
            new ViewModels.CustomerCardViewModel(<ViewModels.ICustomerCardViewModelOptions>{
                parentView: "CustomerAddEditView",
                passThroughDestination: this._cartDestination,
                passThroughOptions: this._destinationOptions,
                isLoyaltyCardDataReadOnly: true
            });

            this.indeterminateWaitVisible = ko.observable(false);

            this.customerAddEditViewModel =
            new ViewModels.CustomerAddEditViewModel({ customer: options.customer, address: options.address });

            this.PersonFieldsVisible = ko.computed(this.arePersonFieldsVisible, this);
            this.isNewCustomer = StringExtensions.isNullOrWhitespace(this.customerAccountNumber);
            this._customerTypeInputEnable = ko.observable(this.isNewCustomer);

            this.indeterminateWaitVisible(true);
            this.customerAddEditViewModel.loadCustomer(this.customerAccountNumber, true)
                .always((): void => { this.indeterminateWaitVisible(false); })
                .done((): void => {
                    // pass the customer data onto the customerCard in the view in order for it to load
                    this._customerCardViewModel.customer(this.customerAddEditViewModel.customer());
                }).fail((errors: Proxy.Entities.Error[]) => {
                    NotificationHandler.displayClientErrors(errors);
                });

            this.commonHeaderData = new Controls.CommonHeaderData();
            //Load Common Header
            this.commonHeaderData.viewSectionInfo(true);
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.sectionTitle(ViewModelAdapter.getResourceString("string_25")); // CUSTOMERS

            if (this.isNewCustomer) {
                this.commonHeaderData.categoryName(ViewModelAdapter.getResourceString("string_1329")); // New customer
            } else {
                this.commonHeaderData.categoryName(this.customerAddEditViewModel.CustomerProxy.Name());
                this.customerAddEditViewModel.CustomerProxy.Name.subscribe((newValue) => {
                    this.commonHeaderData.categoryName(newValue);
                }, this);
            }

            this.streetLines = ko.computed(this.computeMultipleStreetLines, this);
            this.addressFilled = ko.computed(this.computeAddressFilled, this);

            this.numberOfAddressesText = ko.computed(() => {
                var addresses = this.customerAddEditViewModel.CustomerProxy.Addresses();
                if (!ObjectExtensions.isNullOrUndefined(addresses)) {
                    var numberOfAddresses = addresses.length;
                    return numberOfAddresses == 1 ? ViewModelAdapter.getResourceString("string_4843") :
                        StringExtensions.format(ViewModelAdapter.getResourceString("string_4844"), numberOfAddresses);
                }
                return "";
            }, this);
        }

        private computeMultipleStreetLines(): string {
            var street = this.customerAddEditViewModel.AddressProxy.Street();
            if (StringExtensions.isNullOrWhitespace(street)) {
                return StringExtensions.EMPTY;
            }

            var streetLinesArray = street.split("\n");
            var streetLinesHTML = StringExtensions.EMPTY;
            for (var i = 0; i < streetLinesArray.length; i++) {
                streetLinesHTML += "<h4 class='secondaryFontColor'>" + streetLinesArray[i] + "</h4>";
            }

            return streetLinesArray.length > 0 ? streetLinesHTML : street;
        }

        private arePersonFieldsVisible(): boolean {
            return this.customerAddEditViewModel.CustomerProxy.CustomerTypeValue() === Proxy.Entities.CustomerType.Person;
        }

        private computeAddressFilled(): boolean {
            return !ObjectExtensions.isNullOrUndefined(this.customerAddEditViewModel.AddressProxy)
                && !StringExtensions.isNullOrWhitespace(this.customerAddEditViewModel.AddressProxy.Street())
                && !StringExtensions.isNullOrWhitespace(this.customerAddEditViewModel.AddressProxy.City())
                && !StringExtensions.isNullOrWhitespace(this.customerAddEditViewModel.AddressProxy.ZipCode());
        }

        private onBlurContactEmail(): void {
            var customer = this.customerAddEditViewModel.CustomerProxy;

            if (!StringExtensions.isNullOrWhitespace(customer.Email())
                && StringExtensions.isNullOrWhitespace(customer.ReceiptEmail())) {
                customer.ReceiptEmail(customer.Email());
            }
        }

        public saveAndAddCustomerToSale(cartAffiliations: Model.Entities.AffiliationLoyaltyTier[]) {
            this.indeterminateWaitVisible(true);

            var asyncQueue = new AsyncQueue()
                .enqueue(() => {
                    return this.addUpdateNewCustomer(true);
                }).enqueue(() => {
                    var updatedCartAffiliations = Operations.AddCustomerOperationHelper.getUpdatedCartAffiliations(
                        Session.instance.cart, this.customerAccountNumber, cartAffiliations);

                    return this.customerAddEditViewModel.addCustomerToCart(
                        this.customerAddEditViewModel.CustomerProxy.AccountNumber(), updatedCartAffiliations);
                });

            asyncQueue.run()
                .always((): void => { this.indeterminateWaitVisible(false); })
                .done((result: ICancelableResult) => {
                    if (!result.canceled) {
                        ViewModelAdapter.navigate(this._cartDestination, this._destinationOptions);
                    }
                }).fail((errors: Proxy.Entities.Error[]) => {
                    RetailLogger.viewsCustomerAddEditViewAddCustomerFailed();
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        /**
         * Creates or updates the customer.
         */
        public saveCustomer(): void {
            this.indeterminateWaitVisible(true);

            this.addUpdateNewCustomer(false)
                .always((): void => { this.indeterminateWaitVisible(false); })
                .done(() => {
                    var customer = this.customerAddEditViewModel.CustomerProxy;
                    this._destinationOptions.AccountNumber = customer.AccountNumber();

                    var viewOptions: ICustomerDetailsViewOptions = {
                        accountNumber: customer.AccountNumber(),
                        destination: this._cartDestination,
                        destinationOptions: this._destinationOptions
                    };

                    ViewModelAdapter.navigate(this._saveCustomerDestination, viewOptions);
                }).fail((errors: Proxy.Entities.Error[]) => {
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        private addUpdateNewCustomer(addToCart: boolean): IVoidAsyncResult {
            if (this.isNewCustomer) {
                return this.customerAddEditViewModel.addNewCustomer(addToCart);
            } else {
                return this.customerAddEditViewModel.updateCustomer();
            }
        }

        private gotoAddNewAddress(): void {

            if (this.customerAddEditViewModel.Mode() === ViewModels.CustomerAddEditViewModelEnum.Add) {
                this.indeterminateWaitVisible(true);
                this.addUpdateNewCustomer(true)
                    .always((): void => { this.indeterminateWaitVisible(false); })
                    .done(() => {
                        var customer = this.customerAddEditViewModel.CustomerProxy;
                        this._destinationOptions.AccountNumber = customer.AccountNumber();
                        this.customerAddEditViewModel.Mode(ViewModels.CustomerAddEditViewModelEnum.Edit);
                        this.gotoAddNewAddressExec();
                    }).fail((errors: Proxy.Entities.Error[]) => {
                        NotificationHandler.displayClientErrors(errors);
                    });
            } else {
                if (this.customerAddEditViewModel.viewModelChanged) {
                    var options: IMessageOptions = new MessageOptions();
                    options.title = ViewModelAdapter.getResourceString("string_4846"); // Do you want to save the changes you made?
                    options.messageButtons = MessageBoxButtons.YesNo;

                    options.primaryButtonIndex = 0;
                    ViewModelAdapter.displayMessageWithOptions(StringExtensions.EMPTY, options).done((result: IMessageResult) => {
                        if (result.dialogResult === DialogResult.Yes) {
                            this.indeterminateWaitVisible(true);
                            this.addUpdateNewCustomer(true)
                                .always((): void => { this.indeterminateWaitVisible(false); })
                                .done(() => {
                                    this.gotoAddNewAddressExec();
                                }).fail((errors: Proxy.Entities.Error[]) => {
                                    NotificationHandler.displayClientErrors(errors);
                                    RetailLogger.viewsCustomerAddEditViewAddUpdateNewCustomerFailed();
                                });
                        } else {
                             this.customerAddEditViewModel.revertChanges();
                             this.gotoAddNewAddressExec();
                        }
                    });
                } else {
                    this.gotoAddNewAddressExec();
                }
            }
        }

        private gotoAddNewAddressExec(): void {
            var customer: Proxy.Entities.Customer = ObjectExtensions.unwrapObservableProxyObject(this.customerAddEditViewModel.CustomerProxy);
            this.customerAddEditViewModel.loadCustomer(customer.AccountNumber);
            var options: IAddressAddEditViewCtorOptions =
                AddressAddEditViewCtorOptions.CreateInstance(customer, null, true, "CustomerAddEditView", this._cartDestination, this._destinationOptions);

            ViewModelAdapter.navigate("AddressAddEditView", options);
        }

        private gotoAddressesView(): void {
            var options = { customer: (<Proxy.Entities.Customer>ObjectExtensions.unwrapObservableProxyObject(this.customerAddEditViewModel.CustomerProxy)) };
            ViewModelAdapter.navigate("CustomerAddressesView", options);
        }
    }
}
