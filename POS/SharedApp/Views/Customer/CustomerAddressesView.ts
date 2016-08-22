/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.ViewModels.d.ts'/>
/// <reference path='../Controls/CommonHeader.ts'/>
/// <reference path='../ViewControllerBase.ts'/>


module Commerce.ViewControllers {
    "use strict";

    /*
    * CustomerAddressesViewController constructor parameters interface
    */
    export interface ICustomerAddressesViewControllerOptions {
        /**
         * Customer model entity
         */
        customer: Model.Entities.Customer;

        /**
         * The cart lines to be shipped. Must be passed back to ShippingMethodsView after selecting an address.
         */
        cartLines: Model.Entities.CartLine[];

        /**
         * Option indicating whether the addresses are selectable or not.
         */
        selectionMode: boolean;
    }

    export class AddressWrapper {

        public address: Model.Entities.Address;

        public addressTitleClicked: () => void; // Handles address title click
        public addressSelected: () => void; // Handles address selection (click address card area under address title)

        constructor(address: Model.Entities.Address) {
            this.address = address;
        }
    }

    export class CustomerAddressesViewController extends ViewControllerBase {
        public commonHeaderData;
        public customer: Model.Entities.Customer;

        public addresses: Model.Entities.Address[];
        public primaryAddressWrappers: AddressWrapper[];
        public otherAddressWrappers: AddressWrapper[];

        public cartLines: Model.Entities.CartLine[];

        private disposables: IDisposable[];
        private selectionMode: boolean;

        /*
         * constructor
         */
        constructor(options: ICustomerAddressesViewControllerOptions) {
            super(true);

            options = options || { customer: null, cartLines: null, selectionMode: null };

            this.disposables = [];

            this.customer = ObjectExtensions.unwrapObservableProxyObject(options.customer) || null;
            this.cartLines = options.cartLines || [];
            this.selectionMode = !ObjectExtensions.isNullOrUndefined(options.selectionMode) ? options.selectionMode : true;

            //get all customer addresses and set wrappers for primary and ordinary addresses
            this.addresses = this.unwrapObservableAddresses(this.customer.Addresses);
            this.setCustomerAddressesWrappers();

            //Load Common Header
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.commonHeaderData.viewSectionInfo(true);
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);

            this.commonHeaderData.sectionTitle(this.customer.Name);
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_4834")); //Addresses
        }

        /*
         * Sets customer addresses wrappers.
         *
         */
        private setCustomerAddressesWrappers() {
            var ordinaryAddresses = this.addresses.filter((address: Model.Entities.Address) => address.IsPrimary === false);
            this.otherAddressWrappers = this.getCustomerAddressesWrappers(ordinaryAddresses);

            var primaryAddresses = this.addresses.filter((address: Model.Entities.Address) => address.IsPrimary === true);
            this.primaryAddressWrappers = this.getCustomerAddressesWrappers(primaryAddresses);
        }

        /*
         * Gets wrappers for customer addresses.
         * @param {Model.Entities.Address[]} addresses Customer addresses for which method creates wrappers.
         * @return {AddressWrapper[]} The array of wrappers for customer addresses.
         */
        private getCustomerAddressesWrappers(addresses: Model.Entities.Address[]): AddressWrapper[]{
            var self = this;

            var addressWrappers: AddressWrapper[] = [];
            addresses.forEach((address: Model.Entities.Address) => {
                var wrapper: AddressWrapper = new AddressWrapper(address);

                wrapper.addressTitleClicked = () => {
                    self.addressTitleClicked(wrapper);
                };
                WinJS.Utilities.markSupportedForProcessing(wrapper.addressTitleClicked);

                if (this.selectionMode) {
                    wrapper.addressSelected = () => {
                        self.applyAddress(wrapper);
                    };

                    WinJS.Utilities.markSupportedForProcessing(wrapper.addressSelected);
                }

                addressWrappers.push(wrapper);
            });

            return addressWrappers;
        }

        /*
         * Handles address title clicking.
         * @param {AddressWrapper} addressWrapper Customer address wrapper which has been selected.
         */
        private addressTitleClicked(addressWrapper: AddressWrapper): void {
            var address = addressWrapper.address;

            var addressProxy = ObjectExtensions.cloneToObservableProxyObject(address);
            var customerProxy: any = ObjectExtensions.cloneToObservableProxyObject(this.customer);

            var options: Commerce.ViewControllers.IAddressAddEditViewCtorOptions = AddressAddEditViewCtorOptions.CreateInstance(
                    this.customer,
                    address,
                    true,
                    "CustomerAddressesView",
                    "CustomerAddressesView",
                    <ICustomerAddressesViewControllerOptions> { customer: this.customer, cartLines: this.cartLines, selectionMode: this.selectionMode });

            Commerce.ViewModelAdapter.navigate('AddressAddEditView', options);
        }

        /*
         * Handles customer address selection (clicking area under address title) and navigates to Shipping methods view. 
         * @param {AddressWrapper} addressWrapper Customer address wrapper which has been selected.
         */
        private applyAddress(addressWrapper: AddressWrapper): void {
                var options: IShippingMethodsViewCtorOptions = {
                    shippingAddress: ObjectExtensions.unwrapObservableProxyObject(addressWrapper.address),
                    cartLines: this.cartLines
                };

                Commerce.ViewModelAdapter.navigate("ShippingMethodsView", options);
        }

        /*
         * Add new address button handler
         */
        private addNewAddressButtonClick(): void {
            var customerProxy: any = ObjectExtensions.cloneToObservableProxyObject(this.customer);
            var options: Commerce.ViewControllers.IAddressAddEditViewCtorOptions = AddressAddEditViewCtorOptions.CreateInstance(
                this.customer,
                null,
                true,
                "ShippingMethodsView",
                "ShippingMethodsView",
                <IShippingMethodsViewCtorOptions> { customer: this.customer, cartLines: this.cartLines, shippingAddress: null });

            Commerce.ViewModelAdapter.navigate('AddressAddEditView', options);
        }

        /*
         * Called when the page is supposed to be unloaded.
         * This will let us to remove custom subscriptions to ko.observables, when we are unloading this page.
         */
        public unload(): void {
            if (ArrayExtensions.hasElements(this.disposables)) {
                this.disposables.forEach((disposable: IDisposable)=> disposable.dispose());
            }

            super.unload();
        }

        /*
         * Unwraps observable addresses
         *
         * @param {Model.Entities.Address[]} addresses - The array of observable proxies ofcustomer addresses.
         * @return {Model.Entities.Address[]} The array of unwrapped customer addresses
         */
        private unwrapObservableAddresses(addresses: Model.Entities.Address[]): Model.Entities.Address[] {
            if (ArrayExtensions.hasElements(addresses)) {
                for (var i = 0; i < addresses.length; i++) {
                    addresses[i] = ObjectExtensions.unwrapObservableProxyObject(addresses[i]);
                }
            }

            return addresses;
        }
    }
}