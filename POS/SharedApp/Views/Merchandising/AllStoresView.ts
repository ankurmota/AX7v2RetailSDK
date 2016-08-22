/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../ViewControllerBase.ts'/>
///<reference path="../INavigationReturnOptions.ts" />

module Commerce.ViewControllers {
    "use strict";

    export interface IAllStoresViewControllerOptions {
        callerPage: string;
        storeSelectionCallback: (store: Model.Entities.OrgUnit) => IAsyncResult<any>; // Function called when the store selection occurs.
        locations: Model.Entities.OrgUnit[];
    }

    export class AllStoresViewController extends ViewControllerBase {

        private commonHeaderData;
        private _indeterminateWaitVisible: Observable<boolean>;
        private _selectCommandDisabled: Observable<boolean>;
        private _options: IAllStoresViewControllerOptions;
        private _locations: ObservableArray<Model.Entities.OrgUnit>;
        private _selectedStore: Model.Entities.OrgUnit;

        constructor(options: IAllStoresViewControllerOptions) {
            super(false);

            if (ObjectExtensions.isNullOrUndefined(options)) {
                throw "The options parameter was not provided and is required for AllStoresView to function correctly.";
            }

            this._options = options;
            this._indeterminateWaitVisible = ko.observable(true);
            this._selectCommandDisabled = ko.observable(true);

            // Initialize Common Header
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();

            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);

            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_6400")); // STORES
            this.commonHeaderData.enableVirtualCatalogHeader();
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_6401")); // All stores

            if (ObjectExtensions.isNullOrUndefined(options)) {
                RetailLogger.viewsMerchandisingAllStoresViewConstructorArgumentUndefined("options");
                return;
            }

            if (ObjectExtensions.isNullOrUndefined(options.locations)) {
                RetailLogger.viewsMerchandisingAllStoresViewConstructorArgumentUndefined("options.locations");
                return;
            }
            this._locations = ko.observableArray(options.locations);
        }

        public load() {

            this._indeterminateWaitVisible(false);
        }

        /**
         * Handle when user clicks a row from store list view.
         *
         * @param {Model.Entitis.OrgUnit} selecteedItem The item that user selected.
         */
        public invokeStore(selectedItem: Model.Entities.OrgUnit): void {
            this._selectedStore = selectedItem;
        }

        /**
         * Handle when user clicks 'Select' button on the app bar.
         */
        public confirmStoreSelection(): void {
            if (ObjectExtensions.isFunction(this._options.storeSelectionCallback)) {
                var asyncResult: IAsyncResult<any> = this._options.storeSelectionCallback(this._selectedStore);

                if (ObjectExtensions.isNullOrUndefined(asyncResult)) {
                    throw "storeSelectionCallback returned a null or undefined AsyncResult.";
                }

                asyncResult.done((result: any): void => {
                    if (!ObjectExtensions.isNullOrUndefined(result)) {
                        ViewModelAdapter.navigate(this._options.callerPage, result);
                    } else {
                        throw "storeSelectionCallback returned a null or undefined result.";
                    }
                })
                .fail((errors: Model.Entities.Error[]) => {
                    NotificationHandler.displayClientErrors(errors);
                });
            } else {
                throw "Invalid storeSelectionCallback provided to AllStoresView.";
            }
        }

        /**
         * Handle when store selection is changed.
         *
         * @param {Model.Entities.OrgUnit} selectedLines Stores that are selected by user.
         */
        public storeSelectionChanged(selectedLines: Model.Entities.OrgUnit[]) {
            this._selectedStore = selectedLines.length === 0 ? null : selectedLines[0];
            this._selectCommandDisabled(selectedLines.length == 0);
        }

        public loadingStateChanged(event: any) {
            var winControl: any = event.currentTarget.winControl;

            // Autoselect row if result length is 1.
            if (winControl.itemDataSource.list.length == 1 && winControl.selection.count() == 0) {
                winControl.selection.add(0);
            }
        }
    }
}