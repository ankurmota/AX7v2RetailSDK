/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='UserControl.ts'/>

module Commerce.Controls {
    "use strict";

    import Entities = Proxy.Entities;
    import Managers = Model.Managers;

    /**
     * Options passed to the extension properties control.
     */
    export interface IRefinerControlOptions {
        resetProductRefinersHandler: Observable<(() => void)>;
        applyRefinerHandler: (productRefinerValues: Entities.ProductRefinerValue[]) => void;
        getRefinersHandler: (() => IAsyncResult<Entities.ProductRefiner[]>);
        getRefinerValuesHandler: ((productRefiner: Entities.ProductRefiner) => IAsyncResult<Entities.ProductRefinerValue[]>);
    }

    /**
     * User control for rendering extension properties.
     */
    export class RefinerControl extends UserControl {

        private _refinerControlOptions: IRefinerControlOptions;
        private refinerListElement: HTMLElement;

        // Refiner Flyout Variables
        public toggleFilterListFlyout: Observable<any>;
        public forceLayoutFilterListListView: Observable<any>;
        public indeterminateWaitVisible: Observable<boolean>;
        
        private originalProductRefiners: Entities.ProductRefiner[];
        private selectedProductRefiners: ObservableArray<Entities.ProductRefiner>;
        private availableProductRefiners: ObservableArray<Entities.ProductRefiner>;

        //selected refiner value collection
        private selectedRefinerValues: Observable<Entities.ProductRefinerValue[]>[];

        /**
         * Initializes a new instance of the ExtensionPropertiesControl class.
         */
        constructor(options: IRefinerControlOptions) {
            super();

            this._refinerControlOptions = options ||
            {
                resetProductRefinersHandler: undefined,
                applyRefinerHandler: undefined,
                getRefinersHandler: undefined,
                getRefinerValuesHandler: undefined
            };

            this.availableProductRefiners = ko.observableArray<Entities.ProductRefiner>([]);
            this.originalProductRefiners = [];
            this.selectedProductRefiners = ko.observableArray<Entities.ProductRefiner>([]);
            this.toggleFilterListFlyout = ko.observable(() => { });
            this.forceLayoutFilterListListView = ko.observable(() => { });
            this.selectedRefinerValues = [];
            this.indeterminateWaitVisible = ko.observable(false);

            if (ObjectExtensions.isFunction(options.resetProductRefinersHandler)) {
                options.resetProductRefinersHandler((() => {
                    this.selectedProductRefiners([]);
                    this.selectedRefinerValues = [];
                    this.refinerListElement.innerHTML = '';
                    this.originalProductRefiners = [];
                    this.availableProductRefiners([]);
                    this.getRefiners();
                }));
            }
        }

        /**
         * This method is called when the control has been loaded into the DOM.
         */
        public onLoaded() {
            this.refinerListElement = $(this.element).find('.refinerList')[0];
        }

        /**
         * Get refiners.
         */
        private getRefiners(): void {
            if (ObjectExtensions.isFunction(this._refinerControlOptions.getRefinersHandler)) {
                this.indeterminateWaitVisible(true);

                this._refinerControlOptions.getRefinersHandler().done((productRefiners: Entities.ProductRefiner[]) => {
                    this.indeterminateWaitVisible(false);
                    this.originalProductRefiners = productRefiners;
                    this.availableProductRefiners(productRefiners);
                }).fail((error: Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);
                });
            }
        }

        /**
         * Clears all refiners and associated state and calls the appropriate handler.
         */
        private clearFilter(): void {
            this.availableProductRefiners(this.originalProductRefiners);
            this.selectedProductRefiners([]);
            this.selectedRefinerValues = [];
            this.refinerListElement.innerHTML = '';
            if (ObjectExtensions.isFunction(this._refinerControlOptions.applyRefinerHandler)) {
                this._refinerControlOptions.applyRefinerHandler([]);
            }
        }

        /**
         * Applies the selected refiner values.
         */
        private applyFilter(): void {
            RetailLogger.viewsControlsRefinersApplyFilters();
            if (ObjectExtensions.isFunction(this._refinerControlOptions.applyRefinerHandler)) {
                this._refinerControlOptions.applyRefinerHandler(this.getSelectedRefinerValues());
            }
        }

        /**
         * Gets all selected refiner values.
         * @return {Entities.ProductRefinerValue[]} Selected refiner values.
         */
        private getSelectedRefinerValues(): Entities.ProductRefinerValue[]{
            var selectedValues: Entities.ProductRefinerValue[] = [];

            this.selectedRefinerValues.forEach((value: Observable<Entities.ProductRefinerValue[]>): void => {
                selectedValues = selectedValues.concat(value());
            });
            
            return selectedValues;
        }
        
        /**
         * Get the refiner values and add refiner UI for the specified product refiner.
         * @param {Entities.ProductRefiner} selectedRefiner The product refiner.
         */
        private AddRefinerUI(selectedRefiner: Entities.ProductRefiner): void {
            this.toggleFilterListFlyout()();

            if (ObjectExtensions.isNullOrUndefined(selectedRefiner)) {
                return;
            }

            var asyncQueue: Commerce.AsyncQueue = new Commerce.AsyncQueue();
            if (!ArrayExtensions.hasElements(selectedRefiner.Values) && ObjectExtensions.isFunction(this._refinerControlOptions.getRefinerValuesHandler)) {
                asyncQueue.enqueue(() => {
                    this.indeterminateWaitVisible(true);
                    return this._refinerControlOptions.getRefinerValuesHandler(selectedRefiner)
                        .done((productRefinerValues: Entities.ProductRefinerValue[]) => {
                            selectedRefiner.Values = productRefinerValues;
                        })
                })
            };

            asyncQueue.enqueue(() => {
                this.createRefinerUI(selectedRefiner);
                this.selectedProductRefiners.push(selectedRefiner);
                this.availableProductRefiners(this.availableProductRefiners().filter((value: Entities.ProductRefiner) => {
                    for (var i = 0; i < this.selectedProductRefiners().length; i++) {
                        if (this.selectedProductRefiners()[i].RecordId == value.RecordId && this.selectedProductRefiners()[i].SourceValue == value.SourceValue) {
                            return false;
                        }
                    }
                    return true;
                }));
                return AsyncResult.createResolved();
            });

            asyncQueue.run()
                .always(() => {
                    this.indeterminateWaitVisible(false);
                });
        }

        /**
         * Creates the actual refiner control for the specified product refiner.
         * @param {Entities.ProductRefiner} selectedRefiner The product refiner.
         * @return {boolean} True if successfully created the refiner UI, else false.
         */
        private createRefinerUI(selectedRefiner: Entities.ProductRefiner): boolean {
            if (!ObjectExtensions.isNullOrUndefined(selectedRefiner)) {
                var selectedRefinerValuesForControl: Observable<Entities.ProductRefinerValue[]> = ko.observable([]);
                this.selectedRefinerValues.push(selectedRefinerValuesForControl);

                switch (selectedRefiner.DisplayTemplateValue) {
                    case 0:
                        if (selectedRefiner.RefinerTypeValue == 1) {
                            var select = document.createElement("div");
                            $(this.refinerListElement).append(select);
                            ko.applyBindingsToNode(select, { MultiSelectRefinerControl: { Refiner: selectedRefiner, SelectedRefinerValues: selectedRefinerValuesForControl } });
                        } else if (selectedRefiner.RefinerTypeValue == 0) {
                            var select = document.createElement("div");
                            $(this.refinerListElement).append(select);
                            ko.applyBindingsToNode(select, { SingleSelectRefinerControl: { Refiner: selectedRefiner, SelectedRefinerValues: selectedRefinerValuesForControl } });
                        } else {
                            RetailLogger.viewsControlsRefinersTypeNotSupported(JSON.stringify(selectedRefiner));
                        }
                        break;
                    case 1:
                        var select = document.createElement("div");
                        $(this.refinerListElement).append(select);
                        ko.applyBindingsToNode(select, { sliderRefinerControl: { Refiner: selectedRefiner, SelectedRefinerValues: selectedRefinerValuesForControl } });
                        break;
                    case 3:
                        var select = document.createElement("div");
                        $(this.refinerListElement).append(select);
                        ko.applyBindingsToNode(select, { SingleSelectRefinerControl: { Refiner: selectedRefiner, SelectedRefinerValues: selectedRefinerValuesForControl } });
                        break;
                    default:
                        RetailLogger.viewsControlsRefinersDisplayTemplateNotSupported(JSON.stringify(selectedRefiner));
                        return false;
                        break;
                }
            } else {
                RetailLogger.viewsControlsRefinersWrongInputParameters(JSON.stringify(selectedRefiner));
                return false;
            }
        }
    }
}