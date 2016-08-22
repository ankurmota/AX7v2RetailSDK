/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../Controls/ReasonCodeDialog.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    /**
     * Options passed to the AffiliationsViewController class.
     */
    export interface IAffiliationsViewOptions {
    }

    export class AffiliationsViewController extends ViewControllerBase {
        public viewModel: ViewModels.AffiliationsViewModel;
        public commonHeaderData: Controls.CommonHeaderData;
        public indeterminateWaitVisible: Observable<boolean>;

        private _selectedAffiliationDictionary: Dictionary<Proxy.Entities.Affiliation>;
        private _isAutoTriggerSelectionEvent: boolean;
        private _isApplyButtonDisabled: Observable<boolean>;
        private _isSelectionChanged: boolean;

        /**
         * Creates a new instance of the AffiliationsViewController class.
         */
        constructor(options?: IAffiliationsViewOptions) {
            super(true /* saveInHistory */);

            this.viewModel = new ViewModels.AffiliationsViewModel();
            this.commonHeaderData = new Controls.CommonHeaderData();
            this.indeterminateWaitVisible = ko.observable(false);
            this._selectedAffiliationDictionary = this.viewModel.getAffiliationsInCart();
            this._isAutoTriggerSelectionEvent = false;

            //Load Common Header
            this.commonHeaderData.viewSectionInfo(false);
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.categoryName(ViewModelAdapter.getResourceString("string_5201")); // "Apply affiliations"
            this.commonHeaderData.sectionTitle(ViewModelAdapter.getResourceString("string_5206")); // "Add affiliations from list"

            this._isApplyButtonDisabled = ko.observable(true);
            this._isSelectionChanged = false;
        }

        /**
         * Loads the view controller.
         */
        public load(): void {
            this.viewModel.load()
                .fail((errors: Proxy.Entities.Error[]) => { NotificationHandler.displayClientErrors(errors); });
        }

        /**
         * Adds affiliations to transaction
         */
        public addAffiliationsToCart() {
            if (this._isSelectionChanged) {
                this.indeterminateWaitVisible(true);

                var options: Operations.IAddAffiliationOperationOptions = {
                    affiliationNames: [], affiliations: this.getSelectedAffiliations()
                };

                var operationResult: IAsyncResult<ICancelableResult> = Operations.OperationsManager.instance.runOperation(
                    Operations.RetailOperation.AddAffiliation, options);

                operationResult
                    .always((): void => { this.indeterminateWaitVisible(false); })
                    .done((result: ICancelableResult): void => {
                        if (!result.canceled) {
                            ViewModelAdapter.navigate("CartView")
                        }
                    }).fail((errors: Proxy.Entities.Error[]) => {
                        NotificationHandler.displayClientErrors(errors);
                    });
            } else {
                // directly return to Cart screen when there is no change.
                ViewModelAdapter.navigate("CartView");
            }
        }

        private getSelectedAffiliations(): Proxy.Entities.Affiliation[] {
            var selectedAffiliations: Proxy.Entities.Affiliation[] = [];
            this._selectedAffiliationDictionary.forEach((key: string, value: Proxy.Entities.Affiliation) => {
                this.viewModel.affiliations().forEach((affiliation: Proxy.Entities.Affiliation) => {
                    if (affiliation.RecordId.toString() === key) {
                        selectedAffiliations.push(affiliation);
                    }
                });
            });

            return selectedAffiliations;
        }

        /**
         * Affiliation List Selection Changed EventHandler.
         * @param {Proxy.Entities.Affiliation[]} affiliations to add.
         */
        private currentTargetSelectionChanged(selectedItems: Proxy.Entities.Affiliation[]): void {
            if (!this._isAutoTriggerSelectionEvent) {
                // Add the selected affiliations to the affiliation dictionary.
                this._selectedAffiliationDictionary.clear();
                selectedItems.forEach((selectedAffiliation: Proxy.Entities.Affiliation) => {
                    this._selectedAffiliationDictionary.setItem(selectedAffiliation.RecordId, selectedAffiliation);
                });
            }

            this._isAutoTriggerSelectionEvent = false;

            var commandAppBar = document.getElementById("commandAppBar").winControl;
            if (commandAppBar.hidden) {
                commandAppBar.show();
            }

            this.updateIsSelectionDiff();
        }

        /**
         * Affiliation List loading Changed EventHandler.
         * @param {any} event parameter.
         */
        private currentTargetLoadingStateChanged(event: any): void {
            if (event.currentTarget.winControl.loadingState == "complete") {
                // Force the affiliation items in the cart to be selected on data load
                if (event.currentTarget.winControl.itemDataSource.list.length > 0) {
                    this.viewModel.affiliations().forEach((value, index, array) => {
                        if (this._selectedAffiliationDictionary.hasItem(value.RecordId)) {
                            this._isAutoTriggerSelectionEvent = true;
                            event.currentTarget.winControl.selection.add(index);
                        }
                    });
                }
            }
        }

        /**
         * Check if selected affiliations are different from affiliations in cart.
         */
        private updateIsSelectionDiff(): void {
            var selectedAffiliations: Proxy.Entities.Affiliation[] = this.getSelectedAffiliations();
            var cartAffiliations: Proxy.Entities.AffiliationLoyaltyTier[] = Session.instance.cart.AffiliationLines;

            this._isSelectionChanged = true;
            if (!ObjectExtensions.isNullOrUndefined(cartAffiliations)
                && selectedAffiliations.length === cartAffiliations.length) {
                if (selectedAffiliations.length === 0) {
                    this._isSelectionChanged = false;
                } else {
                    for (var i = 0; i < selectedAffiliations.length; i++) {
                        this._isSelectionChanged = !cartAffiliations.some((value, index, array) => value.AffiliationId === selectedAffiliations[i].RecordId);
                        if (this._isSelectionChanged) {
                            break;
                        }
                    }
                }
            } else if (ObjectExtensions.isNullOrUndefined(cartAffiliations)) {
                this._isSelectionChanged = (selectedAffiliations.length > 0);
            }

            //enable the ok button from the initial state(disable) if there is change.
            if (this._isSelectionChanged && this._isApplyButtonDisabled()) {
                this._isApplyButtonDisabled(false);
            }
        }
    }
}