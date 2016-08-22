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

    import Entities = Proxy.Entities;

    /**
     * Type interface for customer add edit view model constructor options.
     */
    export interface ICustomerCardViewModelOptions {
        address?: Model.Entities.Address;
        isLoyaltyCardDataReadOnly?: any;
        parentView: string;
        passThroughDestination?: string;
        passThroughOptions?: any;
        layoutData?: Entities.Layout;
        chooseCardClick?(operationId: string): void;
    }

    export class CustomerCardViewModel extends ViewModelBase {
        public customer: Observable<Commerce.Model.Entities.Customer>;
        public customerPrimaryAddress: Observable<Model.Entities.Address>;
        public customerInvoiceAddress: Observable<Model.Entities.Address>;
        public customerLoyaltyCards: ObservableArray<Model.Entities.LoyaltyCard>;
        public customerLoyaltyCardLabel: Observable<string>;
        public isLoyaltyCardLabelActionable: Observable<boolean>;
        public isLoyaltyCardDataReadOnly: Observable<boolean>;
        public passThroughOptions: Observable<any>;
        public passThroughDestination: Observable<string>;
        public layoutData: Entities.Layout;
        public chooseCardClick: (operationId: string) => void;

        // in order to utilize the "click-to-edit" feature of the address name,
        // include a 'parentView' property on the options object, which is the name of the view
        // currently implementing the customer card. This way, the address add/edit view will know
        // which view to return to after editing an address
        public parentView: Observable<string>;

        // options.parentView is a required option. Example: "CartView" or "CustomerAddEditView"
        constructor(options: ICustomerCardViewModelOptions) {
            super();

            this.parentView = ko.observable("");
            this.passThroughOptions = ko.observable(null);
            this.passThroughDestination = ko.observable(null);
            var primaryAddress, invoiceAddress;
            if (!ObjectExtensions.isNullOrUndefined(options)) {
                if (!ObjectExtensions.isNullOrUndefined(options.address)) {
                    if (options.address.IsPrimary) {
                        primaryAddress = options.address;
                    } else if (options.address.AddressTypeValue === Commerce.Model.Entities.AddressType.Invoice) {
                        invoiceAddress = options.address;
                    }
                }
                if (!StringExtensions.isNullOrWhitespace(options.parentView)) {
                    this.parentView(options.parentView);
                }
                if (!ObjectExtensions.isNullOrUndefined(options.passThroughOptions)) {
                    this.passThroughOptions(options.passThroughOptions);
                }
                if (!ObjectExtensions.isNullOrUndefined(options.passThroughDestination)) {
                    this.passThroughDestination(options.passThroughDestination);
                } 

                this.layoutData = options.layoutData;
                this.chooseCardClick = options.chooseCardClick;
            }

            this.customer = ko.observable(null);
            this.customerPrimaryAddress = ko.observable(primaryAddress || null);
            this.customerInvoiceAddress = ko.observable(invoiceAddress || null);
            this.customer.subscribe((newValue) => {
                var primaryAddress = null;
                var invoiceAddress = null;
                if (newValue && Commerce.ArrayExtensions.hasElements(newValue.Addresses)) {
                    primaryAddress = ArrayExtensions.firstOrUndefined(newValue.Addresses, (address) => {
                        return address.IsPrimary;
                    });

                    invoiceAddress = ArrayExtensions.firstOrUndefined(newValue.Addresses, (address) => {
                        return address.AddressTypeValue == Commerce.Model.Entities.AddressType.Invoice;
                    });
                }

                this.customerPrimaryAddress(primaryAddress || null);
                this.customerInvoiceAddress(invoiceAddress || null);
            }, this);
            this.customerLoyaltyCards = ko.observableArray([]);
            this.customerLoyaltyCardLabel = ko.observable(StringExtensions.EMPTY);
            this.isLoyaltyCardLabelActionable = ko.observable(false);
            this.isLoyaltyCardDataReadOnly = ko.observable(options ? options.isLoyaltyCardDataReadOnly : true);
        }

        public addressEditClickHandler(data: any) {
            var addressAddEditViewCtorOptions: any = {
                customer: this.customer(),
                address: data,
                shouldSaveChanges: true,
                navigateTo: this.parentView(),
                returnOptions: { destination: this.passThroughDestination() || this.parentView(), destinationOptions: this.passThroughOptions() }
            };

            Commerce.ViewModelAdapter.navigate("AddressAddEditView", addressAddEditViewCtorOptions);
        }
    }
}
