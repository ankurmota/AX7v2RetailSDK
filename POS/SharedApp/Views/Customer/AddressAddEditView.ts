/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../INavigationReturnOptions.ts'/>
///<reference path='../ViewControllerBase.ts'/>
///<reference path='CustomerAddEditView.ts'/>

module Commerce.ViewControllers {
    "use strict";

    /**
     * AddressAddEditViewController constructor parameters interface
     */
    export interface IAddressAddEditViewCtorOptions {
        /**
         * Customer model entity
         */
        customer: Model.Entities.Customer;

        /**
         * Address model entity
         */
        address: Model.Entities.Address;

        /**
         * Will navigate here after save
         */
        navigateTo: string;

        /**
         * Defines should changes be saved on server or not
         */
        shouldSaveChanges: boolean;

        /**
         * Defines pass through parameters.
         */
        returnOptions: INavigationReturnOptions<any>;
    }

    /*
     * AddressAddEditViewController constructor parameters class
     */
    export class AddressAddEditViewCtorOptions implements IAddressAddEditViewCtorOptions {
        private static DefaultNavigationDestination: string = "CustomerAddEditView";

        public customer: Model.Entities.Customer;
        public address: Model.Entities.Address;
        public navigateTo: string;

        public shouldSaveChanges: boolean;
        public returnOptions: INavigationReturnOptions<any>;

        constructor(options?: IAddressAddEditViewCtorOptions) {
            if (!ObjectExtensions.isNullOrUndefined(options)) {
                this.customer = options.customer;
                this.address = options.address;
                this.navigateTo = options.navigateTo || AddressAddEditViewCtorOptions.DefaultNavigationDestination;
                this.shouldSaveChanges = options.shouldSaveChanges;
                this.returnOptions = options.returnOptions;
            } else {
                this.customer = new Model.Entities.CustomerClass();
                this.address = new Model.Entities.AddressClass();
                this.navigateTo = AddressAddEditViewCtorOptions.DefaultNavigationDestination;
                this.shouldSaveChanges = true;
                this.returnOptions = <INavigationReturnOptions<any>>{
                    destination: AddressAddEditViewCtorOptions.DefaultNavigationDestination,
                    destinationOptions: {}
                };
            }
        }

        /*
         * Creates and instance of AddressAddEditViewCtorOptions class
         * @param {Model.Entities.Customer} customer - The customer.
         * @param {Model.Entities.Address} address - The address.
         * @param {boolean} shouldSaveChanges - If true, changes will be saved on server, otherwise only in memory.
         * @param {string} destination - Name of the view for return navigation.
         * @param {any} destinationOptions - .ctor options to pass to destination view controller.
         * @return {AddressAddEditViewCtorOptions} An instance of AddressAddEditViewCtorOptions class.
         * @remarks You can create an instance of observable proxies using ObjectExtemsions.convertToObservableProxyObject
         */
        public static CreateInstance(customer: Model.Entities.Customer, address: Model.Entities.Address, shouldSaveChanges: boolean, navigateTo: string,
                                    destination: string, destinationOptions: any): AddressAddEditViewCtorOptions {
            var options: IAddressAddEditViewCtorOptions = {
                customer: customer,
                address: address,
                navigateTo: navigateTo,
                shouldSaveChanges: shouldSaveChanges,
                returnOptions: {
                    destination: destination,
                    destinationOptions: destinationOptions
                },
            };

            return new AddressAddEditViewCtorOptions(options);
        }
    }

    export class AddressAddEditViewController extends ViewControllerBase {
        private waitAddressViewModelAsync: boolean = false;
        public indeterminateWaitVisible: Observable<boolean>;
        public commonHeaderData: Controls.CommonHeaderData;
        public addressAddEditViewModel: Commerce.ViewModels.AddressAddEditViewModel;
        public isPrimary: Observable<boolean>;
        public static salesTaxGroups = ko.observableArray<Commerce.Model.Entities.SalesTaxGroup>([]);
        public showStreetName: Observable<boolean>;
        public showStreetNumber: Observable<boolean>;
        public showBuildingComplement: Observable<boolean>;
        public showCity: Observable<boolean>;
        public showCounty: Observable<boolean>;
        public showDistrict: Observable<boolean>;
        public showState: Observable<boolean>;
        public showZip: Observable<boolean>;
        public enableState: Computed<boolean>;
        public static addressTypes: Array<any>;

        private _navigateTo: string;
        private _returnOptions: INavigationReturnOptions<any>;
        private _shouldSaveChanges: boolean;

        /*
        * constructor
        * @param {IAddressAddEditViewCtorOptions} options Setup data for the view
        */
        constructor(options: IAddressAddEditViewCtorOptions) {
            super(false);

            // ensures that options are passed, otherwise throws an Application Error.
            // we can assume that the options are a proper implementation of the IAddressAddEditViewCtorOptions.
            if (ObjectExtensions.isNullOrUndefined(options)) {
                Commerce.NotificationHandler.displayErrorMessage("string_29000");
                return;
            }

            this._navigateTo = options.navigateTo;
            this._returnOptions = options.returnOptions;
            this._shouldSaveChanges = options.shouldSaveChanges;

            this.addressAddEditViewModel = new Commerce.ViewModels.AddressAddEditViewModel(options);
            AddressAddEditViewController.initAddressTypes();

            if (ObjectExtensions.isNullOrUndefined(this.addressAddEditViewModel.addressProxy.IsPrimary())) {
                var isPrimary: boolean = !this.hasCustomerPrimaryAddress();
                this.addressAddEditViewModel.addressProxy.IsPrimary(isPrimary);
                this.isPrimary = ko.observable(isPrimary);
            } else {
                this.isPrimary = ko.observable(this.addressAddEditViewModel.addressProxy.IsPrimary());
            }

            this.indeterminateWaitVisible = ko.observable(false);
            this.initializeCommonHeader();

            this.loadSalesTaxGroups();

            this.addressAddEditViewModel.addressProxy.ThreeLetterISORegionName.subscribe(this.selectedCountryChanged, this);

            this.showStreetName = ko.observable(true);
            this.showStreetNumber = ko.observable(true);
            this.showBuildingComplement = ko.observable(true);
            this.showCity = ko.observable(true);
            this.showCounty = ko.observable(true);
            this.showDistrict = ko.observable(true);
            this.showState = ko.observable(true);
            this.showZip = ko.observable(true);
            this.selectedCountryChanged(this.addressAddEditViewModel.addressProxy.ThreeLetterISORegionName()); // initialize
            this.addressAddEditViewModel.indeterminateModelState.subscribe(this.handleModelState, this);

            this.enableState = ko.computed(() => {
                return this.addressAddEditViewModel.states().length > 0;
            }, this);
        }

        public setAddressProxyIsPrimary(eventInfo: any, isPrimary: boolean) {
            this.addressAddEditViewModel.addressProxy.IsPrimary(isPrimary);
        }

        private hasCustomerPrimaryAddress(): boolean {
            var customerProxy: any = this.addressAddEditViewModel.customerProxy;

            if (ObjectExtensions.isNullOrUndefined(customerProxy)) {
                return false;
            }

            var customer: Model.Entities.Customer = Commerce.ObjectExtensions.unwrapObservableProxyObject(customerProxy);

            var addresses: Model.Entities.Address[] = customer.Addresses;

            if (ObjectExtensions.isNullOrUndefined(addresses)) {
                return false;
            }

            return !ObjectExtensions.isUndefined(ArrayExtensions.firstOrUndefined(addresses, (address: Model.Entities.Address) => {
                return address.IsPrimary == true;
            }));
        }

        private static initAddressTypes(): void {
            if (!AddressAddEditViewController.addressTypes) {
                AddressAddEditViewController.addressTypes = [];
                for (var key in Commerce.Model.Entities.AddressType) {
                    var value: any = Commerce.Model.Entities.AddressType[key];
                    if (typeof value === "number") {
                        AddressAddEditViewController.addressTypes.push({
                            id: value,
                            description: Commerce.Model.Entities.AddressTypeHelper.getDescription(value)
                        });
                    }
                }
            }
        }

        private selectedCountryChanged(newValue: string) {
            if (!StringExtensions.isEmptyOrWhitespace(newValue)) {
                // get the format lines for the given country
                var matchingCountry = this.addressAddEditViewModel.countries.filter((country) => {
                    return country.CountryRegionId === newValue;
                });

                // update visible address elements based on newValue.AddressFormatLines
                if (ArrayExtensions.hasElements(matchingCountry)) {
                    this.updateVisibleAddressElements(matchingCountry[0].AddressFormatLines);
                }
            }
        }

        private hideAllAddressFields() {
            this.showStreetName(false);
            this.showStreetNumber(false);
            this.showBuildingComplement(false);
            this.showCity(false);
            this.showCounty(false);
            this.showDistrict(false);
            this.showState(false);
            this.showZip(false);
        }

        private updateVisibleAddressElements(formatLines: Model.Entities.AddressFormattingInfo[]) {
            this.hideAllAddressFields();

            var formatLine: Model.Entities.AddressFormattingInfo;
            for (var index = 0; index < formatLines.length; index++) {
                formatLine = formatLines[index];

                if (formatLine.Inactive) {
                    continue;
                }

                switch (formatLines[index].AddressComponentNameValue) {
                    case Model.Entities.AddressFormatLineType.StreetName:
                        this.showStreetName(true);
                        break;
                    case Model.Entities.AddressFormatLineType.StreetNumber:
                        this.showStreetNumber(true);
                        break;
                    case Model.Entities.AddressFormatLineType.BuildingCompliment:
                        this.showBuildingComplement(true);
                        break;
                    case Model.Entities.AddressFormatLineType.Postbox:
                    case Model.Entities.AddressFormatLineType.ZipCode:
                        this.showZip(true);
                        break;
                    case Model.Entities.AddressFormatLineType.County:
                        this.showCounty(true);
                        break;
                    case Model.Entities.AddressFormatLineType.District:
                        this.showDistrict(true);
                        break;
                    case Model.Entities.AddressFormatLineType.State:
                        this.showState(true);
                        break;
                    case Model.Entities.AddressFormatLineType.City:
                        this.showCity(true);
                        break;
                }
            }
        }

        private loadSalesTaxGroups() {
            // no need to download sales tax groups on every view load - if we already have them, just download again on next app reload
            if (AddressAddEditViewController.salesTaxGroups.length == 0) {
                this.addressAddEditViewModel.getSalesTaxGroups()
                    .done((salesTaxGroups) => {
                        AddressAddEditViewController.salesTaxGroups(salesTaxGroups);
                    }).fail((errors: Model.Entities.Error[]) => {
                        RetailLogger.viewsCustomerAddressAddEditViewDownloadTaxGroupsFailed(errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
                    });
            }
        }

        private cloneProxy(proxy: any): any {
            if (ObjectExtensions.isNullOrUndefined(proxy)) {
                return null;
            }

            var unwrapped: any = Commerce.ObjectExtensions.unwrapObservableProxyObject(proxy);
            var observable: any = Commerce.ObjectExtensions.convertToObservableProxyObject(unwrapped);
            return observable;
        }

        private saveClicked(): void {
            var options: ICustomerAddEditViewOptions = {
                customer: ObjectExtensions.unwrapObservableProxyObject(this.addressAddEditViewModel.customerProxy),
                address: ObjectExtensions.unwrapObservableProxyObject(this.addressAddEditViewModel.addressProxy),
                destination: this._returnOptions.destination,
                destinationOptions: this._returnOptions.destinationOptions
            };

            if (this._shouldSaveChanges) {
                if (this.addressAddEditViewModel.indeterminateModelState()) {
                    // View is waiting for address view model async result.
                    this.waitAddressViewModelAsync = true;
                } else {
                    this.saveCustomerAddress(options);
                }
            } else {
                Commerce.ViewModelAdapter.navigate(this._navigateTo, options.destinationOptions);
            }
        }

        private saveCustomerAddress(dataOptions: ICustomerAddEditViewOptions): void {
            var viewModelOptions: ViewModels.ICustomerAddEditViewModelOptions = {
                customer: dataOptions.customer,
                address: dataOptions.address,
            };

            var customerEditViewModel = new ViewModels.CustomerAddEditViewModel(viewModelOptions);

            var asyncQueue: AsyncQueue = new AsyncQueue();

            if (customerEditViewModel.canSave()) {
                var asyncResult: IVoidAsyncResult;
                this.indeterminateWaitVisible(true);

                if (this.isCustomerNew(customerEditViewModel)) {
                    asyncResult = customerEditViewModel.addNewCustomer(false);
                } else {
                    asyncResult = customerEditViewModel.updateCustomer();
                }

                asyncResult.done(() => { this.saveCustomerAddressSuccessCallback(customerEditViewModel, dataOptions); })
                    .fail((errors) => { this.saveCustomerAddressErrorCallback(errors); });

            } else {
                NotificationHandler.displayErrorMessage("string_1367"); // Save the customer before adding an address
            }
        }

        private isCustomerNew(customerEditViewModel: ViewModels.CustomerAddEditViewModel): boolean {
            return StringExtensions.isNullOrWhitespace(customerEditViewModel.CustomerProxy.AccountNumber());
        }

        private saveCustomerAddressSuccessCallback(customerEditViewModel: ViewModels.CustomerAddEditViewModel, dataOptions: ICustomerAddEditViewOptions): void {
            this.indeterminateWaitVisible(false);
            var options;
            switch(this._navigateTo) {
                case "CustomerAddEditView":
                    // when navigating back to customer add edit view, explicitly pass required parameters
                    options = <ICustomerAddEditViewOptions> {
                        customer: ObjectExtensions.unwrapObservableProxyObject(customerEditViewModel.CustomerProxy),
                        address: ObjectExtensions.unwrapObservableProxyObject(customerEditViewModel.AddressProxy),
                        destination: dataOptions.destination,
                        destinationOptions: dataOptions.destinationOptions,
                    };
                    break;
                case "CustomerDetailsView":
                    options = <ICustomerDetailsViewOptions> {
                        accountNumber: customerEditViewModel.CustomerProxy.AccountNumber(),
                        destination: dataOptions.destination,
                        destinationOptions: dataOptions.destinationOptions
                    };
                    break;
                case "PaymentView":
                    // payment view options are required.
                    options = dataOptions.destinationOptions;
                    break;
                case "ShippingMethodsView":
                    var shippingMethodOptions : ViewControllers.IShippingMethodsViewCtorOptions = {
                        cartLines: this._returnOptions.destinationOptions.cartLines,
                        shippingAddress: ObjectExtensions.unwrapObservableProxyObject(customerEditViewModel.AddressProxy)
                    };
                    options = shippingMethodOptions;
                    break;
                case "CustomerAddressesView":
                    options = <ICustomerAddressesViewControllerOptions>{
                        customer: ObjectExtensions.unwrapObservableProxyObject(customerEditViewModel.CustomerProxy),
                        cartLines: this._returnOptions.destinationOptions.cartLines,
                        selectionMode: this._returnOptions.destinationOptions.selectionMode
                    };
                    break;
                default:
                    options = { customer: ObjectExtensions.unwrapObservableProxyObject(customerEditViewModel.CustomerProxy) };
                    break;
            }

            Commerce.ViewModelAdapter.navigate(this._navigateTo, options);
        }

        private saveCustomerAddressErrorCallback(errors: Model.Entities.Error[]): void {
            this.indeterminateWaitVisible(false);
            NotificationHandler.displayClientErrors(errors);
        }

        private initializeCommonHeader(): void {
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.viewSearchBox(false);
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_4832"));

            this.commonHeaderData.sectionTitle(this.addressAddEditViewModel.addressProxy.Name());
        }

        private handleModelState(): void {
            var modelState: boolean = this.addressAddEditViewModel.indeterminateModelState();

            if (modelState && this.waitAddressViewModelAsync) {
                // Address view model async result is done.
                // Save the current address.
                this.waitAddressViewModelAsync = false;
                this.saveClicked();
            }

            this.indeterminateWaitVisible(modelState);
        }
    }
}
