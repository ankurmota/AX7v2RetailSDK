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

    /*
     * AddressAddEditViewModel constructor parameters interface
     */
    export interface IAddressAddEditViewModelOptions {
        /**
         * The customer.
         */
        customer: Model.Entities.Customer;

        /**
         * The address.
         */
        address: Model.Entities.Address
    }

    export class AddressAddEditViewModel extends ViewModelBase {
        public addressProxy: any;
        public customerProxy: any;
        public states: ObservableArray<Commerce.Model.Entities.StateProvinceInfo>;
        public countries: Model.Entities.CountryRegionInfo[] = Commerce.ApplicationContext.Instance.Countries;
        
        public indeterminateModelState: Observable<boolean>;

        /**
         * Initializes a new instance of the AddressAddEditViewModel class.
         * @param {IAddressAddEditViewModelOptions} options Setup data for the view model.
         */
        constructor(options: IAddressAddEditViewModelOptions) {
            super();

            options = options || { customer: null, address: null };

            this.indeterminateModelState = ko.observable(false);
            this.states = ko.observableArray([]);                        
            this.addressProxy = options.address ? ObjectExtensions.convertToObservableProxyObject(options.address)
                                                : ObjectExtensions.convertToObservableProxyObject(new Model.Entities.AddressClass());

            this.customerProxy = options.customer ? ObjectExtensions.convertToObservableProxyObject(options.customer)
                                                  : ObjectExtensions.convertToObservableProxyObject(new Model.Entities.CustomerClass());

            if (ObjectExtensions.isNullOrUndefined(this.addressProxy.ThreeLetterISORegionName())) {
                this.addressProxy.ThreeLetterISORegionName(Commerce.ApplicationContext.Instance.storeInformation.OrgUnitAddress.ThreeLetterISORegionName);
            }

            this.initStates();
            this.addressProxy.ThreeLetterISORegionName.subscribe(this.countryChanged, this);  
            this.addressProxy.ZipCode.subscribe(this.zipCodeChanged, this);                            

            if (StringExtensions.isNullOrWhitespace(this.addressProxy.Name())) {
                this.addressProxy.Name = ko.observable(this.getAddressName());
            }
        }                                               

        /**
        *   Get sales tax groups.
        *   
        *   @return {IAsyncResult<Model.Entities.SalesTaxGroup[]>} async result containing tax groups.
        */
        public getSalesTaxGroups(): IAsyncResult<Model.Entities.SalesTaxGroup[]> {
            return this.channelManager.getSalesTaxGroups();
        }

        private getAddressName(): string {
            if (!ObjectExtensions.isNullOrUndefined(this.customerProxy)) {

                if (StringExtensions.isNullOrWhitespace(this.customerProxy.FirstName())) {
                    //to avoid unnecessary spaces in address name field
                    return this.customerProxy.LastName() || StringExtensions.EMPTY;
                }

                var nameFormat: string = ViewModelAdapter.getResourceString("string_1366"); // {0} {1} - (FirstName LastName)
                return StringExtensions.format(nameFormat, (this.customerProxy.FirstName() || ""), (this.customerProxy.LastName() || ""));
            } else {
                return StringExtensions.EMPTY;
            }
        }

        private initStates(): void {
            //NOTE: ko reset the value during the update so we need to set it again
            var state: string = this.addressProxy.State();
            this.refreshStates(this.addressProxy.ThreeLetterISORegionName())
                .done((states) => {
                    this.states(states);
                    this.addressProxy.State(state || null);
                });
        }

        private countryChanged() {
            this.indeterminateModelState(true);
            
            this.states([]);            
            this.addressProxy.State(null);                        
            
            this.refreshStates(this.addressProxy.ThreeLetterISORegionName())
                .done((states) => {
                    this.states(states);
                    this.indeterminateModelState(false);
            });
        }

        private refreshStates(countryId: string): IAsyncResult<Model.Entities.StateProvinceInfo[]> {
            if (ObjectExtensions.isNullOrUndefined(countryId)) {
                return AsyncResult.createResolved([]);
            }

            return this.channelManager.getStateProvincesAsync(countryId);
        }        
                

        private zipCodeInfoToAddress(zipCodeItem: Commerce.Model.Entities.ZipCodeInfo) {            
            this.addressProxy.State(zipCodeItem.StateId);
            this.addressProxy.County(zipCodeItem.CountyId);
            this.addressProxy.City(zipCodeItem.CityName);
            this.addressProxy.DistrictName(zipCodeItem.District);                     
        }        

        /**
        *   Event handler of ZIP code value update event.
        *   Fills all possible elements of address according to entered zip code.
        */
        private zipCodeChanged() {
            if (!StringExtensions.isEmptyOrWhitespace(this.addressProxy.ZipCode())) {
                this.indeterminateModelState(true);
                this.channelManager.getAddressFromZipCodeAsync(this.addressProxy.ThreeLetterISORegionName(), this.addressProxy.ZipCode())
                    .done((newZipCodeInfoCollection: Model.Entities.ZipCodeInfo[]) => {
                        if (newZipCodeInfoCollection.length > 0) {
                            var zipCodeInfoItem: Model.Entities.ZipCodeInfo = newZipCodeInfoCollection[0];                            
                            this.zipCodeInfoToAddress(zipCodeInfoItem);
                        } else {
                            this.zipCodeInfoToAddress(new Commerce.Model.Entities.ZipCodeInfoClass());                            
                        }
                    })
                    .fail((errors: Model.Entities.Error[]) => {
                        RetailLogger.viewModelAddressAddEditGetAddressFromZipCodeFailed();
                        Commerce.NotificationHandler.displayClientErrors(errors);
                    });
                this.indeterminateModelState(false);
            }
        }   
    }
}