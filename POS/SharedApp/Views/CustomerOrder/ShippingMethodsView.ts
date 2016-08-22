/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Controls/dialog/knockout.dialog.ts'/>
///<reference path='../../Controls/tileList/knockout.tileList.ts'/>
///<reference path='../Cart/CartView.ts'/>
///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    export interface ExtendedDeliveryOption extends Model.Entities.DeliveryOption {
        Price?: number;
    }

    /*
     * ShippingMethodsViewController constructor parameters interface
     */
    export interface IShippingMethodsViewCtorOptions {
        cartLines: Model.Entities.CartLine[];
        shippingAddress: Model.Entities.Address;
        destinationOptions?: any;
    }

    export class ShippingMethodsViewController extends ViewControllerBase {

        private shippingMethods: ObservableArray<Model.Entities.DeliveryOption>;
        private cartLines: Model.Entities.CartLine[];

        private shippingAddress: Observable<Model.Entities.Address>;
        private addressTypeText: Computed<string>;
        private customerName: Observable<string>;
        private customer: Model.Entities.Customer;

        private _deliveryViewModel: ViewModels.ShippingViewModel;
        private _cartViewModel: ViewModels.CartViewModel;
        private _customerViewModel: ViewModels.CustomerDetailsViewModel;
        private _selectedDeliveryMethod: Observable<ExtendedDeliveryOption>;
        private _isCustomerLoaded: Observable<boolean>;
        private _originalShippingCharge: number;
        private _originalDeliveryDate: Date;
        public totalCustomerAddressesString: Observable<string>;
        public totalAddressesVisibility: Observable<boolean>;

        public commonHeaderData: Commerce.Controls.CommonHeaderData;
        public _indeterminateWaitVisible: Observable<boolean>;

        constructor(options?: IShippingMethodsViewCtorOptions) {
            super(true);

            // initialize view models
            this._cartViewModel = new ViewModels.CartViewModel();
            this._customerViewModel = new ViewModels.CustomerDetailsViewModel();
            this._deliveryViewModel = new ViewModels.ShippingViewModel();

            this._indeterminateWaitVisible = ko.observable(false);

            // initialize members
            this.cartLines = [];
            this.shippingMethods = ko.observableArray<Model.Entities.DeliveryOption>([]);
            this.shippingAddress = ko.observable(null);
            this.shippingAddress.subscribe(this.getShippingMethods, this);

            if (!ObjectExtensions.isNullOrUndefined(options)) {
                if (!ObjectExtensions.isNullOrUndefined(options.destinationOptions) && ArrayExtensions.hasElements(options.destinationOptions.cartLines)) {
                    this.cartLines = options.destinationOptions.cartLines;
                } else if (ArrayExtensions.hasElements(options.cartLines)) {
                    this.cartLines = options.cartLines;
                }

                if (!ObjectExtensions.isNullOrUndefined(options.shippingAddress) && !NumberExtensions.isNullOrZero(options.shippingAddress.RecordId)) {
                    this.shippingAddress(ObjectExtensions.unwrapObservableProxyObject(options.shippingAddress));
                } else if (ArrayExtensions.hasElements(this.cartLines)) {

                    var cartLine: Model.Entities.CartLine;
                    for (var i: number = 0; i < this.cartLines.length; i++) {
                        cartLine = this.cartLines[i];

                        // If options parameter does not have shipping address,
                        // then try to get the shipping address from the first cartline available.
                        if (!ObjectExtensions.isNullOrUndefined(cartLine.ShippingAddress)) {
                            this.shippingAddress(cartLine.ShippingAddress);
                            break;
                        }
                    }
                }
            }

            this.customerName = ko.observable(StringExtensions.EMPTY);
            
            this._selectedDeliveryMethod = ko.observable<ExtendedDeliveryOption>({ RecordId: 0 });
            this._isCustomerLoaded = ko.observable(false);
                        
            // set up methods
            this.getCustomer();

            this.totalCustomerAddressesString = ko.observable("");
            this.totalAddressesVisibility = ko.observable(false);

            this.addressTypeText = ko.computed(() => {
                var addressTypeTextValue: string;
                addressTypeTextValue = !ObjectExtensions.isNullOrUndefined(this.shippingAddress()) ? Commerce.Formatters.AddressTypeTextFormatter(this.shippingAddress().AddressTypeValue) : '';
                return addressTypeTextValue;
            }, this);

            var cart: Model.Entities.Cart = this._cartViewModel.cart();
            this._originalShippingCharge = DeliveryHelper.calculateDeliveryCharges(cart, this.cartLines);
            this._originalDeliveryDate = DeliveryHelper.getDeliveryDate(cart, this.cartLines);

            // Load Common Header
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.commonHeaderData.viewSectionInfo(true);
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_4330")); // CUSTOMER ORDER
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_2501")); // Shipping method
        }

        private updateAddressButtonClick() {
            var selectedAddress: Model.Entities.Address = this.shippingAddress();
            if (ObjectExtensions.isNullOrUndefined(selectedAddress)) {
                return;
            }

            var options: Commerce.ViewControllers.IAddressAddEditViewCtorOptions = 
                AddressAddEditViewCtorOptions.CreateInstance(this.customer, selectedAddress, true, "ShippingMethodsView", "ShippingMethodsView",
                    <IShippingMethodsViewCtorOptions> { cartLines: this.cartLines, shippingAddress: null });

            ViewModelAdapter.navigate('AddressAddEditView', options);
        }
        
        private createNewAddressButtonClick() {
            var options: Commerce.ViewControllers.IAddressAddEditViewCtorOptions =
                AddressAddEditViewCtorOptions.CreateInstance(this.customer, null, true, "ShippingMethodsView", "ShippingMethodsView",
                    <IShippingMethodsViewCtorOptions> { cartLines: this.cartLines, shippingAddress: null });

            ViewModelAdapter.navigate('AddressAddEditView', options);
        }

        private viewAllCustomerAddresses() {
            ViewModelAdapter.navigate('CustomerAddressesView', <ICustomerAddressesViewControllerOptions>{ customer: this.customer, cartLines: this.cartLines, selectionMode: null });
        }

        private shippingMethodButtonClick(eventArgs: Commerce.TileList.IItemInvokedArgs) {
            var shippingMethod: Model.Entities.DeliveryOption = eventArgs.data;
            var requestedShippingDate: Date;
            var requestedShippingCharge: number;
            
            if (shippingMethod.Code === ApplicationContext.Instance.channelConfiguration.PickupDeliveryModeCode) {
                Commerce.ViewModelAdapter.navigate("PickUpInStoreView", {
                    isForPickUp: true,
                    cartLines: this.cartLines
                });
                return;
            }

            if (!this.isShippingAddressProvided) {
                this.createNewAddressButtonClick();
                return;
            }

            this._indeterminateWaitVisible(true);
            var asyncQueue = new AsyncQueue();

            asyncQueue
                .enqueue(() => {
                    // Get the shipping charges
                    var shippingChargeActivityParameters: Activities.GetShippingChargeActivityContext = {
                        originalShippingCharge: this._originalShippingCharge,
                        deliveryMethodDescription: shippingMethod.Description
                    };
                    var getShippingChargeActivity = new Activities.GetShippingChargeActivity(
                        shippingChargeActivityParameters);

                    return getShippingChargeActivity.execute().done(() => {
                        if (!getShippingChargeActivity.response) {
                            // user clicks Cancel button on Get shipping charge dialog.
                            // Cancel the queue.
                            asyncQueue.cancel();
                            return;
                        }

                        var activityResponse: Activities.GetShippingChargeActivityResponse = getShippingChargeActivity.response;
                        requestedShippingCharge = activityResponse.shippingChargeAmount;
                    });
                }).enqueue(() => {
                    // Get the shipping date
                    var shippingDateActivityParameters: Activities.GetShippingDateActivityContext = {
                        originalShippingDate: this._originalDeliveryDate,
                        deliveryMethodDescription: shippingMethod.Description
                    };
                    var getShippingDateActivity = new Activities.GetShippingDateActivity(
                        shippingDateActivityParameters);

                    return getShippingDateActivity.execute().done(() => {
                        if (!getShippingDateActivity.response) {
                            // user clicks Cancel button on Get shipping date dialog.
                            // Cancel the queue.
                            asyncQueue.cancel();
                            return;
                        }

                        var activityResponse: Activities.GetShippingDateActivityResponse = getShippingDateActivity.response;
                        requestedShippingDate = activityResponse.requestedShippingDate;
                    });
                }).enqueue(() => {
                    // Set delivery address with charges and dates to server.
                    return this._deliveryViewModel.setShipDeliveryAddressAsync(
                        this._cartViewModel.cart(),
                        this.cartLines,
                        shippingMethod.Code,
                        requestedShippingDate,
                        requestedShippingCharge,
                        this.shippingAddress());
                }).enqueue(() => {
                    // Navigate to CartView
                    var parameters: ICartViewControllerOptions = {
                        navigationSource: "ShippingMethodsView",
                        transactionDetailViewMode: ViewControllers.CartViewTransactionDetailViewMode.Delivery
                    };
                    ViewModelAdapter.navigate("CartView", parameters);

                    return VoidAsyncResult.createResolved();
                });

            asyncQueue.run()
                .done(() => {
                    this._indeterminateWaitVisible(false);
                })
                .fail((errors: Model.Entities.Error[]) => {
                    this._indeterminateWaitVisible(false);
                    NotificationHandler.displayClientErrors(errors);
            });
        }

        private get isShippingAddressProvided(): boolean {
            var shippingAddress: Commerce.Model.Entities.Address = this.shippingAddress();
            return (!ObjectExtensions.isNullOrUndefined(shippingAddress)
                && !NumberExtensions.isNullOrZero(shippingAddress.RecordId));
        }

        private createCustomer(): void {
            var options: Operations.ICustomerAddOperationOptions = {
                destination: "ShippingMethodsView",
                destinationOptions: {
                    cartLines: this.cartLines
                }
            };

            Operations.OperationsManager.instance.runOperation(RetailOperation.CustomerAdd, options);
        }

        private getCustomer() {
            // if this is a customer order, customer id will be set
            var customerId: string = Session.instance.cart.CustomerId;
            if (!StringExtensions.isNullOrWhitespace(customerId)) {
                this._customerViewModel.loadCustomer(customerId)
                    .done(() => { this.getCustomerSuccess(this._customerViewModel.Customer()); })
                    .fail((errors) => { this.getCustomerError(errors); });
            }
        }

        private getCustomerSuccess(customer: Model.Entities.Customer) {
            // primary address will be used as default shipping address
            var primaryAddress: Model.Entities.Address = this._customerViewModel.customerAddress();
            this.customer = customer;

            this.setShippingAddress(primaryAddress, customer);
            this._isCustomerLoaded(true);

            var nonPrimaryAddresses: Model.Entities.Address[] = customer.Addresses.filter((address: Model.Entities.Address) => { return !address.IsPrimary; });

            if (ArrayExtensions.hasElements(nonPrimaryAddresses)) {
                this.totalCustomerAddressesString(StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_4836"), this.customer.Addresses.length));
                this.totalAddressesVisibility(true);
            }
        }

        private getCustomerError(errors: Model.Entities.Error[]) {
            NotificationHandler.displayClientErrors(errors);
        }

        private setShippingAddress(shippingAddress: Model.Entities.Address, customer: Model.Entities.Customer) {
            this.customerName(customer.Name);
            var originalShippingAddress: Model.Entities.Address = this.shippingAddress();
            var shouldSetShippingAddress: boolean =
                ObjectExtensions.isNullOrUndefined(originalShippingAddress)
                || ObjectExtensions.isNullOrUndefined(originalShippingAddress.RecordId)
                || originalShippingAddress.RecordId === 0;

            if (shouldSetShippingAddress) {
                this.shippingAddress(shippingAddress);
            } else {
                this.getShippingMethods(originalShippingAddress);
            }
        }

        private getShippingMethods(shippingAddress: Model.Entities.Address): void {
            if (ObjectExtensions.isNullOrUndefined(shippingAddress) || NumberExtensions.isNullOrZero(shippingAddress.RecordId)) {
                this.shippingMethods([]);
            } else {
                this._indeterminateWaitVisible(true);
                var address: Model.Entities.Address = this.getDeliveryAddress(shippingAddress);
                this._deliveryViewModel.getDeliveryModes(address, this.cartLines)
                    .done((deliveryOptions: Model.Entities.SalesLineDeliveryOption[]) => this.successDeliveryModes(deliveryOptions))
                    .fail((errors: Model.Entities.Error[]) => this.errorDeliveryModes(errors));
             }
        }

        private searchCustomers(): void {
            var cart: Commerce.Model.Entities.Cart = this._cartViewModel.cart();

            //We are not allowed to change customer in recalled order
            if (cart.CartTypeValue === Commerce.Model.Entities.CartType.CustomerOrder
                && !StringExtensions.isNullOrWhitespace(cart.SalesId)) {
                NotificationHandler.displayErrorMessage('string_4420'); //You can not change customer on recalled order
                return;
            }

            var returnNavigationOptions: IShippingMethodsViewCtorOptions = {
                cartLines: this.cartLines,
                shippingAddress: null
            };

            ViewModelAdapter.navigate(
                "SearchView", {
                    searchEntity: "Customers",
                    searchText: "",
                    destination: "ShippingMethodsView",
                    destinationOptions: returnNavigationOptions
                });
        }

        private successDeliveryModes(methods: Model.Entities.SalesLineDeliveryOption[]): void {
            this._indeterminateWaitVisible(false);
            var options = new Dictionary <Model.Entities.DeliveryOption[]> ();
            methods.forEach((salesLineOptions) => {
                salesLineOptions.DeliveryOptions.forEach((option) => {
                    var value: Model.Entities.DeliveryOption[] = options.hasItem(option.Code) ? options.getItem(option.Code) : [];
                    value.push(option);
                    options.setItem(option.Code.toString(), value);
                });
            });

            // filter options available for all lines
            options = options
                .filter((key: string, value: Model.Entities.DeliveryOption[]) => {
                    return value.length === methods.length;
                });
            var validOptions: Model.Entities.DeliveryOption[] = options.getItems().map((value: Model.Entities.DeliveryOption[]) => {
                    return value[0];
            });

            if (!ArrayExtensions.hasElements(validOptions)) {

                if (this.cartLines.length > 1) {
                    NotificationHandler.displayErrorMessage("string_2544"); // no delivery methods available - multiple lines
                } else {
                    NotificationHandler.displayErrorMessage("string_2540"); // no delivery methods available - single line
                }

                return null;
            }

            this.shippingMethods(validOptions);
        }

        private errorDeliveryModes(errors: Model.Entities.Error[]): void {
            this._indeterminateWaitVisible(false);
            NotificationHandler.displayClientErrors(errors);
        }

        /**
         * Converts Address to Address
         *
         * @param {Model.Entities.Address} customerAddress: Customer address.
         * @return {Model.Entities.Address} Address.
         */
        private getDeliveryAddress(customerAddress: Model.Entities.Address): Model.Entities.Address {
            return <Model.Entities.Address> {
                Name: customerAddress.Name,
                RecordId: customerAddress.RecordId,
                ThreeLetterISORegionName: customerAddress.ThreeLetterISORegionName,
                TwoLetterISORegionName: customerAddress.TwoLetterISORegionName,
                State: customerAddress.State,
                County: customerAddress.County,
                City: customerAddress.City,
                DistrictName: customerAddress.DistrictName,
                Street: customerAddress.Street,
                StreetNumber: customerAddress.StreetNumber,
                ZipCode: customerAddress.ZipCode,
                Email: customerAddress.Email,
                Phone: customerAddress.Phone,
                Url: customerAddress.Url,
            };
        }
    }
}