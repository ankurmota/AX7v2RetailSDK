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

    export enum CustomerAddEditViewModelEnum {
        Add = 1,
        Edit = 2
    }

    /**
     * Type interface for customer add edit view model constructor options.
     */
    export interface ICustomerAddEditViewModelOptions {
        /**
         * The customer.
         */
        customer: Model.Entities.Customer;

        /**
         * The address.
         */
        address: Model.Entities.Address;
    }

    /* BEGIN SDKSAMPLE_CUSTOMERPREFERENCES (do not remove this)
    export interface IExtendedCustomer extends Model.Entities.Customer {
        emailPrefOptIn: number;
    }
    END SDKSAMPLE_CUSTOMERPREFERENCES (do not remove this) */

    export class CustomerAddEditViewModel extends ViewModelBase {
        public CustomerProxy: any;
        public AddressProxy: any;

        public Mode: Observable<number>;
        public canSave: Computed<boolean>;

        /* BEGIN SDKSAMPLE_CUSTOMERPREFERENCES (do not remove this)
        public emailOptIn: Observable<boolean>;
        private _emailOptIn_Key: string = "EMAILOPTIN";
        END SDKSAMPLE_CUSTOMERPREFERENCES (do not remove this) */

        //dropdown data
        public States: ObservableArray<Commerce.Model.Entities.StateProvinceInfo>;

        //customer properties but not yet defined on retail server data object
        public Image: Observable<string>;

        public Countries: Model.Entities.CountryRegionInfo[] = Commerce.ApplicationContext.Instance.Countries;
        public Currencies: Model.Entities.Currency[] = Commerce.ApplicationContext.Instance.Currencies;
        public CustomerGroups: ObservableArray<Model.Entities.CustomerGroup>;
        public CustomerTypes: ObservableArray<Model.Entities.ICustomerType>;
        public ReceiptOptions: ObservableArray<Model.Entities.ReceiptOption>;
        public Languages: ObservableArray<Model.Entities.LanguagesInfo>;
        public customerAffiliations: Commerce.Model.Entities.CustomerAffiliation[];
        public customer: Observable<Commerce.Model.Entities.Customer>;
        public viewModelChanged: boolean;

        /*
         * constructor
         * @param {ICustomerAddEditViewModelOptions} options Setup data for the view model.
         */
        constructor(options: ICustomerAddEditViewModelOptions) {
            super();

            // sanitize options.
            options = options || { customer: null, address: null };
            this.States = ko.observableArray([]);
            this.Mode = ko.observable(-1);

            this.CustomerProxy = ObjectExtensions.convertToObservableProxyObject(options.customer ? options.customer : new Model.Entities.CustomerClass(), (): void => {
                this.viewModelChanged = true;
            });

            this.AddressProxy = options.address ? ObjectExtensions.convertToObservableProxyObject(options.address) :
                                                  ObjectExtensions.convertToObservableProxyObject(new Model.Entities.AddressClass());

            var isPrimary: boolean = !ObjectExtensions.isNullOrUndefined(this.AddressProxy.IsPrimary()) ? this.AddressProxy.IsPrimary() : true;
            this.AddressProxy.IsPrimary(isPrimary);

            if (StringExtensions.isNullOrWhitespace(this.AddressProxy.ThreeLetterISORegionName())) {
                this.AddressProxy.ThreeLetterISORegionName(ApplicationContext.Instance.storeInformation.OrgUnitAddress.ThreeLetterISORegionName);
            }

            this.canSave = ko.computed(() => {
                return this.canSaveCustomer();
            });

            this.AddressProxy.ThreeLetterISORegionName.subscribe(this.countryChanged, this);

            this.customer = ko.observable(null);

            this.CustomerGroups = ko.observableArray([]);
            this.CustomerTypes = ko.observableArray([]);
            this.Languages = ko.observableArray([]);
            this.ReceiptOptions = ko.observableArray([]);

            /* BEGIN SDKSAMPLE_CUSTOMERPREFERENCES (do not remove this)
            this.emailOptIn = ko.observable(false);
            this.emailOptIn.subscribe((newEmailOptIn) => {
                this.addUpdateExtensionProperty(this._emailOptIn_Key, (newEmailOptIn ? 1 : 0));
            });
            END SDKSAMPLE_CUSTOMERPREFERENCES (do not remove this) */
        }

        /* BEGIN SDKSAMPLE_CUSTOMERPREFERENCES (do not remove this)
        private addUpdateExtensionProperty(key: string, intValue: number) {

            if (this.CustomerProxy) {
                if (!this.CustomerProxy.ExtensionProperties()) {
                    this.CustomerProxy.ExtensionProperties = ko.observableArray([]);
                }

                var properties = this.CustomerProxy.ExtensionProperties().filter((property) => {
                    return property.Key === key;
                });

                var emailOptInP: Model.Entities.CommercePropertyClass;
                if (ArrayExtensions.hasElements(properties)) {
                    emailOptInP = properties[0];
                }
                else {
                    emailOptInP = new Model.Entities.CommercePropertyClass();
                    emailOptInP.Key = key;
                    this.CustomerProxy.ExtensionProperties().push(emailOptInP);
                }

                if (!emailOptInP.Value) {
                    emailOptInP.Value = new Model.Entities.CommercePropertyValueClass();
                }

                emailOptInP.Value.IntegerValue = intValue;
            }
        }
        END SDKSAMPLE_CUSTOMERPREFERENCES (do not remove this) */

        private countryChanged() {
            this.AddressProxy.State("");
            this.refreshStates(this.AddressProxy.ThreeLetterISORegionName());
        }

        private refreshStates(countryId: string): IVoidAsyncResult {
            if (StringExtensions.isNullOrWhitespace(countryId)) {
                return AsyncResult.createResolved([]);
            }

            return this.channelManager.getStateProvincesAsync(countryId)
                .done((newStates) => { this.States(newStates); });
        }

        /**
         * Validate if required fields are being put so we can save customer data
         */
        private canSaveCustomer(): boolean {
            var customerType: Model.Entities.CustomerType = CustomerHelper.getCustomerType(this.CustomerProxy.CustomerTypeValue());

            var isNameValid: boolean = false;

            switch (customerType) {
                case Model.Entities.CustomerType.Organization:
                    isNameValid = !StringExtensions.isNullOrWhitespace(this.CustomerProxy.Name());
                    break;
                case Model.Entities.CustomerType.Person:
                    isNameValid = !StringExtensions.isNullOrWhitespace(this.CustomerProxy.FirstName()) &&
                        !StringExtensions.isNullOrWhitespace(this.CustomerProxy.LastName());
                    break;
                default:
                    RetailLogger.viewModelCustomerAddEditUnknownCustomerType(this.CustomerProxy.CustomerTypeValue());
            }

            var result: boolean = isNameValid &&
                customerType !== Model.Entities.CustomerType.None && 
                !StringExtensions.isNullOrWhitespace(this.CustomerProxy.CurrencyCode()) && 
                !StringExtensions.isNullOrWhitespace(this.CustomerProxy.CustomerGroup()) &&
                !StringExtensions.isNullOrWhitespace(this.CustomerProxy.Language()); 
                
            return result;
        }

        /**
         * Loads the customer with given account number after setting available customer options. 
         * @param {string} accountNumber The customer account number.
         * @param {boolean} isInitialLoad Equals to 'true' if this method is called for initial customer loading on the view, otherwise 'false'. 
         * @return {IVoidAsyncResult} The async result.
         */
        public loadCustomer(accountNumber: string, isInitialLoad: boolean = false): IVoidAsyncResult {
            var asyncResult: VoidAsyncResult = new VoidAsyncResult();

            //firstly calls setCustomerOptions() to guarantee the right order of customer options loading and their further settings.
            this.setCustomerOptions().done(() => {
                this.loadCustomerImpl(accountNumber, isInitialLoad).done(() => {
                    asyncResult.resolve();
                }).fail((errors: Model.Entities.Error[]) => {
                        asyncResult.reject(errors);
                    });
            }).fail((errors: Model.Entities.Error[]) => {
                    asyncResult.reject(errors);
                });

            return asyncResult;
        }

        /**
         * Sets available options for customer: customer groups, customer types, languages, receipt options.
         * @return {IVoidAsyncResult} The async result.
         */
        private setCustomerOptions(): IVoidAsyncResult {

            var customerGroupsResult: IAsyncResult<Model.Entities.CustomerGroup[]> =
                ApplicationContext.Instance.customerGroupsAsync
                    .done((groups: Model.Entities.CustomerGroup[]) => {
                        this.CustomerGroups(groups);
                    });

            var customerTypesResult: IAsyncResult<Model.Entities.ICustomerType[]> =
                ApplicationContext.Instance.customerTypesAsync
                    .done((types: Model.Entities.ICustomerType[]) => {
                        this.CustomerTypes(types);
                    });

            var languagesResult: IAsyncResult<Model.Entities.LanguagesInfo[]> =
                ApplicationContext.Instance.languagesAsync
                    .done((languages: Model.Entities.LanguagesInfo[]) => {
                        this.Languages(languages);
                    });

            var receiptOptionsResult: IAsyncResult<Model.Entities.ReceiptOption[]> =
                ApplicationContext.Instance.receiptOptionsAsync
                    .done((options: Model.Entities.ReceiptOption[]) => {
                        this.ReceiptOptions(options);
                    });

            var asyncResults: IAsyncResult<any>[] = [customerGroupsResult, customerTypesResult, languagesResult, receiptOptionsResult];

            return VoidAsyncResult.join(asyncResults);
        }

        /**
         * Loads the customer with given account number.
         * @param {string} accountNumber The customer account number.
         * @param {boolean} isInitialLoad Equals to 'true' if this method is called for initial customer loading on the view, otherwise 'false'. 
         * @return {IVoidAsyncResult} The async result.
         */ 
        private loadCustomerImpl(accountNumber: string, isInitialLoad: boolean = false): IVoidAsyncResult {
            var customerAddress: Commerce.Model.Entities.Address;
            var asyncResult: IVoidAsyncResult;

            //no customer Id on Cart Session, Add mode.
            if (StringExtensions.isNullOrWhitespace(accountNumber) || accountNumber === "0") {
                var defaultCustomerId = ApplicationContext.Instance.storeInformation.DefaultCustomerAccount;
                return this.customerManager.getCustomerDetailsAsync(defaultCustomerId).done((customerDetails) => {
                    /* BEGIN SDKSAMPLE_CUSTOMERPREFERENCES (do not remove this)
                    var extendedCustomer = <IExtendedCustomer>customerDetails.customer;
                    this.emailOptIn(extendedCustomer.emailPrefOptIn == 1);
                    END SDKSAMPLE_CUSTOMERPREFERENCES (do not remove this) */
                    this.CustomerProxy.CustomerTypeValue(customerDetails.customer.CustomerTypeValue);
                    this.CustomerProxy.CustomerGroup(customerDetails.customer.CustomerGroup);
                    this.CustomerProxy.Language(customerDetails.customer.Language);
                    this.CustomerProxy.ReceiptSettings(customerDetails.customer.ReceiptSettings);
                    this.CustomerProxy.CurrencyCode(customerDetails.customer.CurrencyCode);
                    this.AddressProxy.IsPrimary(this.AddressProxy.IsPrimary() || true);
                    this.Mode(CustomerAddEditViewModelEnum.Add);
                });
               
            } else {
                //customer Id found on Cart Session, Edit mode.
                this.Mode(CustomerAddEditViewModelEnum.Edit);

                //try to get object from CustomerProxy
                var tempCustomerProxy = <Model.Entities.Customer>ObjectExtensions.unwrapObservableProxyObject(this.CustomerProxy);

                if (ObjectExtensions.isNullOrUndefined(tempCustomerProxy) ||
                    tempCustomerProxy.AccountNumber !== accountNumber ||
                    tempCustomerProxy.AccountNumber === "0" || isInitialLoad) {

                    //if customer from proxy object is null, or account number from customer proxy is different with the one from session
                    //customer is not updated. Call retail server to get customer details and update the bindings
                    var asyncQueue = new AsyncQueue()
                        .enqueue(() => {
                            return this.customerManager.getCustomerDetailsAsync(accountNumber)
                                .done((customerDetails) => {
                                    this.customer(customerDetails.customer);
                                    customerAddress = customerDetails.primaryAddress;

                                    /* BEGIN SDKSAMPLE_CUSTOMERPREFERENCES (do not remove this)
                                    var properties = customerDetails.customer.ExtensionProperties.filter((property) => {
                                        return property.Key === this._emailOptIn_Key;
                                    });

                                    if (ArrayExtensions.hasElements(properties)) {
                                        var emailOptInProperty = properties[0];

                                        if (emailOptInProperty.Value) {
                                            this.emailOptIn(emailOptInProperty.Value.IntegerValue === 1);
                                        }
                                    }
                                    END SDKSAMPLE_CUSTOMERPREFERENCES (do not remove this) */

                                    if (customerAddress == null) {
                                        customerAddress = new Model.Entities.AddressClass();
                                        customerAddress.ThreeLetterISORegionName = Commerce.ApplicationContext.Instance.storeInformation.OrgUnitAddress.ThreeLetterISORegionName;
                                    }

                                    this.populateCustomerAffiliations(customerDetails.customer.CustomerAffiliations);
                                });
                        }).enqueue(() => {
                            return this.refreshStates(customerAddress.ThreeLetterISORegionName).done(() => {
                                this.updateCustomerDetails(this.customer(), customerAddress);
                            });
                        });

                    asyncResult = asyncQueue.run();
                } else {
                    this.customer(tempCustomerProxy);
                    asyncResult = VoidAsyncResult.createResolved();
                }
            }

            if (this.Mode() === CustomerAddEditViewModelEnum.Add) {
                this.CustomerProxy.CurrencyCode(this.CustomerProxy.CurrencyCode() || Commerce.ApplicationContext.Instance.deviceConfiguration.Currency);
                this.CustomerProxy.Language(this.CustomerProxy.Language() || Commerce.ApplicationContext.Instance.deviceConfiguration.CultureName);
            }

            return asyncResult;
        }

        private updateCustomerDetails(
            customerFound: Model.Entities.Customer,
            address: Model.Entities.Address): void {

            if (ObjectExtensions.isNullOrUndefined(customerFound)) {
                customerFound = new Model.Entities.CustomerClass();
            }

            if (ObjectExtensions.isNullOrUndefined(address)) {
                address = new Model.Entities.AddressClass();
            }

            this.CustomerProxy.update(customerFound);
            this.AddressProxy.update(address);

            // refreshes the state, since view model will clear it when re-setting the country
            this.AddressProxy.State(address.State);
            this.viewModelChanged = false;
        }

        /**
         * Clear the customer view model.
         * @return {IVoidAsyncResult} The async result.
         */
        public clearViewModel(): IVoidAsyncResult {
            this.updateCustomerDetails(null, null);
            return this.loadCustomer(null);
        }

        /**
         * Add a new customer to retail server.
         * @param {boolean} defines if add customer to sale or not on success.
         * @return {IVoidAsyncResult} The async result.
         */
        public addNewCustomer(addToSale: boolean): IVoidAsyncResult {
            var customer: Model.Entities.Customer = <Model.Entities.Customer>ObjectExtensions.unwrapObservableProxyObject(this.CustomerProxy);

            /* BEGIN SDKSAMPLE_CUSTOMERPREFERENCES (do not remove this)
            var extendedCustomer = <IExtendedCustomer>(<any>new Model.Entities.CustomerClass(customer));
            extendedCustomer.emailPrefOptIn = (this.emailOptIn() ? 1 : 0);
            END SDKSAMPLE_CUSTOMERPREFERENCES (do not remove this) */

            // check to make sure a valid address has been passed to prevent passing a blank address when creating the customer
            // pre-condition: an address has already been validated for this customer if an address was added on the address page -OR-
            // no address was added, therefore we do not have a valid address, and the customer is sent to the server without an address.
            var unwrappedAddressProxy = <Commerce.Model.Entities.Address>ObjectExtensions.unwrapObservableProxyObject(this.AddressProxy);
            
            // since we validate the Street (among other fields) to be populated before allowing the user to try to save address
            // if street field is not defined, then the address was not provided at all
            var isAddressProvided: boolean = typeof (unwrappedAddressProxy.Street) != 'undefined';

            // validate address when provided
            var addressValidationErrors: Commerce.Model.Entities.Error[] = this.customerManager.validateCustomerAddress(unwrappedAddressProxy);            
            if (isAddressProvided && ArrayExtensions.hasElements(addressValidationErrors)) {
                return VoidAsyncResult.createRejected(addressValidationErrors);
            }

            if (isAddressProvided) {
                customer.Addresses = <Model.Entities.Address[]>[<Model.Entities.Address>unwrappedAddressProxy];
            } else {
                customer.Addresses = [];
            }

            var asyncQueue = new AsyncQueue().enqueue(() => {
                return this.customerManager.addNewCustomerAsync(customer)
                    .done((createdCustomer) => {
                    customer = createdCustomer;

                    //update customer and address observable
                    this.CustomerProxy.update(createdCustomer);

                    if (ArrayExtensions.hasElements(createdCustomer.Addresses)) {
                        var primaryAddress = createdCustomer.Addresses.filter(address => address.IsPrimary);
                        if (primaryAddress.length > 0) {
                            this.AddressProxy.update(primaryAddress[0]);
                            // refreshes the state, since view model will clear it when re-setting the country
                            this.AddressProxy.State(primaryAddress[0].State);
                        }
                    }
                });
            }).enqueue(() => {
                if (addToSale) {
                    return this.addNewCustomerAndAddToCartSucess(customer);
                }

                return this.customerManager.getCustomerDetailsAsync(customer.AccountNumber)
                    .done((customerDetails) => {
                    this.updateCustomerDetails(customerDetails.customer, customerDetails.primaryAddress);
                });
            }).enqueue((): IVoidAsyncResult => {
                    var postTriggerOptions: Triggers.IPostCustomerAddTriggerOptions = { cart: Session.instance.cart, customer: customer };
                    return Triggers.TriggerManager.instance.execute(Triggers.NonCancelableTriggerType.PostCustomerAdd, postTriggerOptions);
            });

            return asyncQueue.run();
        }

        /**
         * Update a customer to retail server.
         * @return {IVoidAsyncResult} The async result.
         */
        public updateCustomer(): IVoidAsyncResult {
            var customer: Model.Entities.Customer = <Model.Entities.Customer>ObjectExtensions.unwrapObservableProxyObject(this.CustomerProxy);
            var updatedAddress: Model.Entities.Address = <Model.Entities.Address>ObjectExtensions.unwrapObservableProxyObject(this.AddressProxy);
            var matchingAddressFound: boolean = false;

            /* BEGIN SDKSAMPLE_CUSTOMERPREFERENCES (do not remove this)
            var extendedCustomer = <IExtendedCustomer>this.customer();
            extendedCustomer.emailPrefOptIn = (this.emailOptIn() ? 1 : 0);
            END SDKSAMPLE_CUSTOMERPREFERENCES (do not remove this) */

            if (ObjectExtensions.isNullOrUndefined(customer.Addresses)) {
                customer.Addresses = this.customerManager.validateCustomerAddress(updatedAddress).length == 0 ? [<Model.Entities.Address>updatedAddress] : [];
            } else {
                for (var i = 0; i < customer.Addresses.length; i++) {
                    if (customer.Addresses[i].RecordId == updatedAddress.RecordId) {
                        customer.Addresses[i] = updatedAddress;
                        matchingAddressFound = true;
                        break;
                    }
                }
                if (!matchingAddressFound) {
                    customer.Addresses.push(updatedAddress);
                }
            }

            var accountNumber = customer.AccountNumber;
            var asyncQueue = new AsyncQueue();

            asyncQueue
                .enqueue(() => {
                    return this.customerManager.updateCustomerAsync(customer);
                }).enqueue(() => {
                    return this.customerManager.getCustomerDetailsAsync(accountNumber)
                        .done((customerDetails) => {
                            // If address was updated, then select updated address, else select newly added (last from customer addresses collection).  
                            var address: Model.Entities.Address = matchingAddressFound ?
                                customerDetails.customer.Addresses.filter((address: Model.Entities.Address) => address.DirectoryPartyLocationRecordId == updatedAddress.DirectoryPartyLocationRecordId)[0] :
                                customerDetails.customer.Addresses[customerDetails.customer.Addresses.length - 1];
                            this.updateCustomerDetails(customerDetails.customer, address);
                        });
                });

            return asyncQueue.run();
        }

        public addCustomerToCart(
            customerId: string,
            cartAffiliationLines: Model.Entities.AffiliationLoyaltyTier[]): IVoidAsyncResult {
            var options: Operations.IAddCustomerToSalesOrderOperationOptions = {
                customerId: customerId,
                cartAffiliations: cartAffiliationLines
            };

            return this.operationsManager.runOperation(
                Operations.RetailOperation.SetCustomer, options);
        }

        /**
        * Reverts the changes done by the user. 
        */
        public revertChanges(): void {
            this.CustomerProxy.update(this.customer());
            this.viewModelChanged = false;
        }

        private addNewCustomerAndAddToCartSucess(createdCustomer: Model.Entities.Customer): IVoidAsyncResult {
            //add new customer on cart
            return this.addCustomerToCart(createdCustomer.AccountNumber, Session.instance.cart.AffiliationLines);
        }

        private populateCustomerAffiliations(customerAffiliations: Model.Entities.CustomerAffiliation[]): void {
            this.customerAffiliations = customerAffiliations;
        }
    }
}