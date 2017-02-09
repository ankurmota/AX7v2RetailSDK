/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.ViewModels.d.ts'/>
///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    export class PickUpViewController extends ViewControllerBase {

        private _indeterminateWaitVisible: Observable<boolean>;
        private commonHeaderData: Controls.CommonHeaderData;

        private _cartLines: ObservableArray<Model.Entities.CartLine>;
        private _selectedCartLines: Model.Entities.CartLine[];

        private _cartViewModel: ViewModels.CartViewModel;

        private _clearCartOnUnload: boolean;
        private _winControl: any;

        public isNoLineSelected: Observable<boolean>;

        constructor() {
            super(true);

            this._cartViewModel = new ViewModels.CartViewModel();
            this._indeterminateWaitVisible = ko.observable(true);

            this._cartLines = ko.observableArray<Model.Entities.CartLine>(null);
            this._cartViewModel.load()
                .done(() => {
                    this._indeterminateWaitVisible(false);
                    this.onCartViewModelLoaded();
                })
                .fail((errors: Model.Entities.Error[]) => {
                    this.displayError(errors);
                });
            this._clearCartOnUnload = true;
            this.isNoLineSelected = ko.observable(true);

            this.initializeCommonHeader();
        }

        //DEMO4 //TODO:AM
        //Show Next Item to be picked up with minimum amounts
        public load(): void {
            let cartLines: Proxy.Entities.CartLine[] = this._cartViewModel.cart().CartLines;
            let cartLinesNotInvoiced: Proxy.Entities.CartLine[] = [];
            cartLines.forEach((cartLine: Proxy.Entities.CartLine) => {
                if (cartLine.QuantityInvoiced === 0)
                    cartLinesNotInvoiced.push(cartLine);
            });

            if (cartLinesNotInvoiced.length > 1) {
                var xMin = Math.min.apply(null, cartLinesNotInvoiced.map(o => o.Price));
                var minXObject = cartLinesNotInvoiced.filter(o => (o.Price === xMin))[0];

                Commerce.NotificationHandler.displayErrorMessage(StringExtensions.format("Customer can pick up the next least expensive Item {0} for the price of ${1}", minXObject.ItemId,minXObject.Price * (minXObject.QuantityOrdered - minXObject.QuantityInvoiced)));
            }
        }

        private onCartViewModelLoaded(): void {
            this._cartLines(this._cartViewModel.getLinesAvailableForPickUp(this._cartViewModel.originalCartLines()));
        }

        private selectAllClick(): void {
            if (this._winControl.selection.isEverything()) {
                this._winControl.selection.clear();
            } else {
                this._winControl.selection.selectAll();
            }
        }

        private clearSelectionClick(): void {
            this._winControl.selection.clear();
        }

        private pickUpClick(): void {
            if (!ArrayExtensions.hasElements(this._selectedCartLines)) {
                return;
            }

            //demo4 // todo:am
            //add logic to override price


            //end

            this._indeterminateWaitVisible(true);
            this._cartViewModel.pickUpCartLines(this._selectedCartLines)
                .done((result: ICancelableResult) => {
                    this._indeterminateWaitVisible(false);

                    if (!result.canceled) {
                        this._clearCartOnUnload = false;
                        var navigationOptions: ICartViewControllerOptions = {
                            navigationSource: "PickUpView"
                        };
                        Commerce.ViewModelAdapter.navigate("CartView", navigationOptions);
                    }
                }).fail((errors: Model.Entities.Error[]) => { this.displayError(errors); });
        }

        private displayError(errors: Model.Entities.Error[]): void {
            this._indeterminateWaitVisible(false);
            NotificationHandler.displayClientErrors(errors);
        }

        private loadingStateChanged(event: any) {
            this._winControl = event.currentTarget.winControl;

            // Autoselect row if result length is 1.
            if (this._winControl.itemDataSource.list.length == 1 && this._winControl.selection.count() == 0) {
                this._winControl.selection.add(0);
            }
        }

        //Load Common Header 
        private initializeCommonHeader(): void {
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.commonHeaderData.sectionTitle(StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_4529"), this._cartViewModel.cart().SalesId));
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_4530"));
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.viewSearchBox(false);
        }

        private onSelectionChanged(cartLines: Model.Entities.CartLine[]): void {
            this._selectedCartLines = cartLines;
            this.isNoLineSelected(this._selectedCartLines.length <= 0);
        }

        /**
         * Unload method (called by navigator.ts)
         */
        public unload(): void {
            // cleanCartOnUpnload sets to true only in case then PickUp is successful
            if (this._clearCartOnUnload) {
                this._cartViewModel.removeCartFromSession();
            }
        }
    }
}