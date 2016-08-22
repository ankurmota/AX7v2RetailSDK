/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Core/Navigator.ts'/>
///<reference path='../Controls/AutocompleteComboboxDialog.ts'/>
///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    export class CustomerAffiliationsViewController extends ViewControllerBase {
        public commonHeaderData;
        public indeterminateWaitVisible: Observable<boolean>;

        public affiliationsViewModel: Commerce.ViewModels.AffiliationsViewModel;
        public customerDetailsViewModel: Commerce.ViewModels.CustomerDetailsViewModel;

        public customerAffiliations: ObservableArray<Commerce.Model.Entities.CustomerAffiliation>;

        private _deleteCustomerAffiliationKeys: number[];

        private _autocompleteControl: Controls.AutocompleteComboboxDialog;

        constructor(options?: any) {
            super(true);

            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();

            this.indeterminateWaitVisible = ko.observable(false);

            this.affiliationsViewModel = new Commerce.ViewModels.AffiliationsViewModel();
            this.customerDetailsViewModel = new Commerce.ViewModels.CustomerDetailsViewModel();

            // Initial values of the customerDetailsViewModel.
            this.customerDetailsViewModel.customerAffiliations(options.customerAffiliations);
            this.customerDetailsViewModel.Customer(options.customerProxy);
            this.customerDetailsViewModel.customerAddress(options.addressProxy);

            //Load Common Header
            this.commonHeaderData.viewSectionInfo(false);
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_5200"));//"Affiliations"
            this.commonHeaderData.sectionTitle(this.customerDetailsViewModel.Customer().Name);

            this.addControl(this._autocompleteControl = new Controls.AutocompleteComboboxDialog());
        }

        /**
         * Loads the view controller.
         */
        public load(): void {
            this.affiliationsViewModel.load()
                .fail((errors: Proxy.Entities.Error[]) => { NotificationHandler.displayClientErrors(errors); });
        }

        /**
         * Customer Affiliation List Selection Changed EventHandler.
         *
         * @param {Commerce.Model.Entities.CustomerAffiliation[]} customer affiliations to remove.
         */
        private currentTargetSelectionChanged(selectedItems: Model.Entities.CustomerAffiliation[]) {
            // Add the selected affiliations to the delete CustomerAffiliation collection.
            this._deleteCustomerAffiliationKeys = [];
            selectedItems.forEach((selectedAffiliation: Model.Entities.CustomerAffiliation) => {
                this._deleteCustomerAffiliationKeys.push(selectedAffiliation.RecordId);
            });
        }

        /**
         * Add affiliation to customer.
         */
        public addAffiliationToCustomer() {
            var allAffiliations = this.affiliationsViewModel.affiliations();
            if (ArrayExtensions.hasElements(allAffiliations)) {

                var affiliationNamesDictonary: Dictionary<Commerce.Model.Entities.Affiliation> = new Dictionary<Commerce.Model.Entities.Affiliation>();
                allAffiliations.forEach((affiliationItem: Commerce.Model.Entities.Affiliation) => {
                    if (!this.customerDetailsViewModel.isAlreadyInCustomer(affiliationItem.RecordId)) {
                        affiliationNamesDictonary.setItem(affiliationItem.Name, affiliationItem);
                    }
                });

                this._autocompleteControl.title(Commerce.ViewModelAdapter.getResourceString("string_6306"));
                this._autocompleteControl.subTitle(Commerce.ViewModelAdapter.getResourceString("string_6305"));

                var affiliationDataSource: Commerce.Controls.AutocompleteDataItem[] = [];
                affiliationNamesDictonary.getItems().forEach((affiliation: Commerce.Model.Entities.Affiliation) => {
                    var dataItem: Commerce.Controls.AutocompleteDataItem = new Commerce.Controls.AutocompleteDataItem();
                    dataItem.value = affiliation.Name;
                    dataItem.description = affiliation.Description;
                    affiliationDataSource.push(dataItem);
                });

                this._autocompleteControl.show({ dataSource: affiliationDataSource })
                    .on(DialogResult.OK, (selectedText: string) => {

                        var selectedAffiliation: Commerce.Model.Entities.Affiliation = affiliationNamesDictonary.getItem(selectedText);

                        if (!ObjectExtensions.isNullOrUndefined(selectedAffiliation)) {
                            var selectedCustomerAffiliation: Model.Entities.CustomerAffiliationClass = new Model.Entities.CustomerAffiliationClass();
                            selectedCustomerAffiliation.RetailAffiliationId = selectedAffiliation.RecordId;

                            // Add the selected affiliation to customer.
                            this.indeterminateWaitVisible(true);
                            this.customerDetailsViewModel.addAffiliationToCustomer(selectedCustomerAffiliation)
                                .done(() => {
                                    this.indeterminateWaitVisible(false);
                                }).fail((errors: Model.Entities.Error[]) => {
                                    this.indeterminateWaitVisible(false);
                                    NotificationHandler.displayClientErrors(errors);
                                });
                        }
                    });
            }
        }

        /**
         * Delete the selected affiliations from customer.
         */
        public deleteAffiliationsFromCustomer() {
            if (ArrayExtensions.hasElements(this._deleteCustomerAffiliationKeys)) {

                // Remove customer affiliations.
                this.indeterminateWaitVisible(true);
                this.customerDetailsViewModel.deleteAffiliationsFromCustomer(this._deleteCustomerAffiliationKeys)
                    .done(() => {
                        this.indeterminateWaitVisible(false);
                    }).fail((errors: Model.Entities.Error[]) => {
                        this.indeterminateWaitVisible(false);
                        NotificationHandler.displayClientErrors(errors);
                    });
            }
        }

        /**
         * Navigate to customer detail page.
         */
        public navigateToCustomerDetailPage() {
            var viewOptions: ICustomerDetailsViewOptions = {
                accountNumber: this.customerDetailsViewModel.Customer().AccountNumber,
                destination: "CartView",
                destinationOptions: null
            };

            Commerce.ViewModelAdapter.navigate("CustomerDetailsView", viewOptions);
        }
    }
}