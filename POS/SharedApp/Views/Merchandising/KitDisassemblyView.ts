/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Controls/SetDisassembleQuantityDialog.ts'/>
///<reference path='../ViewControllerBase.ts'/>
///<reference path='SearchView.ts'/>

module Commerce.ViewControllers {
    "use strict";

    export class KitDisassemblyViewController extends ViewControllerBase {
        public textMode: Observable<SearchTextMode>;
        public isKitEnabled: Observable<boolean>;
        public kitDisassemblyViewModel: ViewModels.KitDisassemblyViewModel;
        public searchText: Observable<string>;
        public indeterminateWaitVisible: Observable<boolean>;
        public toggleShowHideMenu: Observable<any>;
        public enterProductVisible: Computed<boolean>;
        public enterQuantityVisible: Computed<boolean>;
        public commonHeaderData: Controls.CommonHeaderData;

        private _totalProducts: Computed<number>;
        private _setDisassembleQuantityDialog: Controls.SetDisassembleQuantityDialog;

        constructor(options?: any) {
            super(true);

            this.textMode = ko.observable(SearchTextMode.EnterProduct);
            this.searchText = ko.observable("");
            this.indeterminateWaitVisible = ko.observable(false);
            this.toggleShowHideMenu = ko.observable(() => { return; });
            this.enterProductVisible = ko.computed(this.computeEnterProductVisible, this);
            this.enterQuantityVisible = ko.computed(this.computeEnterQuantityVisible, this);
            this.commonHeaderData = new Controls.CommonHeaderData();
            this.commonHeaderData.viewSectionInfo(true);
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.sectionTitle(ViewModelAdapter.getResourceString("string_419"));

            this.addControl(this._setDisassembleQuantityDialog = new Controls.SetDisassembleQuantityDialog());

            this.kitDisassemblyViewModel = new ViewModels.KitDisassemblyViewModel(options);

            this._totalProducts = ko.computed(this.computeTotalProducts, this);

            this.changeToProductMode();
            this.isKitEnabled = ko.observable(false);

            this.commonHeaderData.categoryName(ViewModelAdapter.getResourceString("string_419"));
        }

        /**
         * Called on load of view controller.
         */
        public load(): void {
            this.indeterminateWaitVisible(true);
            this.kitDisassemblyViewModel.load()
                .always((): void => { this.indeterminateWaitVisible(false); })
                .done(() => {
                    if (StringExtensions.isNullOrWhitespace(this.kitDisassemblyViewModel.kitId())) {
                        this.commonHeaderData.categoryName(ViewModelAdapter.getResourceString("string_419"));
                    } else {
                        this.isKitEnabled(true);
                        this.commonHeaderData.categoryName(this.kitDisassemblyViewModel.kitId() + ", " + this.kitDisassemblyViewModel.kitName());
                    }
                }).fail((error: Proxy.Entities.Error[]) => {
                    NotificationHandler.displayClientErrors(error);
                });

            // Parse keyboard input
            this.handleKeys = this.handleKeys.bind(this);
            $(document).on("keypress", this.handleKeys);
        }

        /**
         * Called when view is hidden.
         */
        public onHidden(): void {
            // Removes eventhandler. 
            $(document).off("keypress", this.handleKeys);
        }

        /**
         * Handles when the enter key or button is pressed on the product input number pad.
         * @param {Controls.NumPad.INumPadResult} numPadResult The number pad result.
         */
        public enterProductNumpad(numPadResult: Controls.NumPad.INumPadResult): void {
            this.indeterminateWaitVisible(true);
            this.kitDisassemblyViewModel.searchProduct(this.searchText())
                .always((): void => { this.indeterminateWaitVisible(false); })
                .done((products: Proxy.Entities.Product[]) => {
                    this.enterProductSuccess(products.filter((p: Proxy.Entities.Product) => { return p.IsKit; }));
                }).fail((error: Proxy.Entities.Error[]) => {
                    NotificationHandler.displayClientErrors(error);
                });
        }

        /**
         * Handles when the enter key or button is pressed on the quantity input number pad.
         * @param {Controls.NumPad.INumPadResult} numPadResult The number pad result.
         */
        public enterQuantityNumpad(numPadResult: Controls.NumPad.INumPadResult): void {
            var quantity: number = NumberExtensions.parseNumber(this.searchText());

            // find selected component and modify its add to cart quantity
            if (this.kitDisassemblyViewModel.selectedComponentIndex !== -1) {
                var selectedComponent: ViewModels.ComponentLine;
                selectedComponent = this.kitDisassemblyViewModel.componentLines()[this.kitDisassemblyViewModel.selectedComponentIndex];

                if (selectedComponent.validateAddToCartQty(quantity)) {
                    var updatedComponentLine: ViewModels.ComponentLine = selectedComponent.getUpdatedComponentLine(
                        quantity, this.kitDisassemblyViewModel.kitQuantity());
                    this.kitDisassemblyViewModel.componentLines.splice(selectedComponent.index, 1, updatedComponentLine);
                } else {
                    this.displayInvalidQuantityErrorMessage();
                }
            } else {
                // show errors
            }
        }

        /**
         * Saves the kit (disassembly) transaction to the server.
         */
        public commitKitTransaction(): void {
            this.indeterminateWaitVisible(true);
            this.kitDisassemblyViewModel.commit()
                .always((): void => { this.indeterminateWaitVisible(false); })
                .done(() => {
                    ViewModelAdapter.navigate("CartView");
                }).fail((error: Proxy.Entities.Error[]) => {
                    NotificationHandler.displayClientErrors(error);
                });
        }

        /**
         * Handles when the selection is changed on the component lines.
         * @param {ViewModels.ComponentLine[]} items The selected component lines.
         */
        public componentLineSelectionChanged(items: ViewModels.ComponentLine[]): void {
            if (ArrayExtensions.hasElements(items)) {
                this.kitDisassemblyViewModel.selectedComponentIndex = items[0].index;
                this.changeToQuantityMode();
            } else {
                this.kitDisassemblyViewModel.selectedComponentIndex = -1;
                this.changeToProductMode();
            }
        }

        private setKitQuantityToDisassemble(): void {
            var state: Controls.SetDisassembleQuantityDialogState = {
                product: this.kitDisassemblyViewModel.kiproduct,
                originalQuantity: this.kitDisassemblyViewModel.kitQuantity()
            };

            this._setDisassembleQuantityDialog.show(state)
                .on(DialogResult.OK, (result: number) => {
                    this.kitDisassemblyViewModel.setQuantityToDisassemble(result);
                }).on(DialogResult.Cancel, (result: number) => {
                    this.changeToProductMode();
                }).onError((errors: Proxy.Entities.Error[]) => {
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        private computeEnterProductVisible(): boolean {
            return this.textMode() === SearchTextMode.EnterProduct;
        }

        private computeEnterQuantityVisible(): boolean {
            return this.textMode() === SearchTextMode.EnterQuantity;
        }

        
        private resetAddToSaleQuantities(): void {
            this.kitDisassemblyViewModel.resetAddToSaleQuantities();
        }
        

        private changeToQuantityMode(): void {
            this.textMode(SearchTextMode.EnterQuantity);
            this.searchText("");
        }

        private changeToProductMode(): void {
            this.textMode(SearchTextMode.EnterProduct);
            this.searchText("");
        }

        private displayInvalidQuantityErrorMessage(): void {
            // show UI for invalid AddToSale quantity
            ViewModelAdapter.displayMessage(
                StringExtensions.format(ViewModelAdapter.getResourceString("string_5371"),
                    this.kitDisassemblyViewModel.componentLines()[this.kitDisassemblyViewModel.selectedComponentIndex].totalQuantity),
                MessageType.Error,
                MessageBoxButtons.Default,
                ViewModelAdapter.getResourceString("string_3383"));
        }

        private enterProductSuccess(products: Proxy.Entities.Product[]): void {
            if (products.length > 1) {
                // navigate to product search form
                this.searchForProduct(products);
            } else if (products.length === 1) {
                // product is a kit product, goto kit details form to choose kit variants
                if (products[0].IsMasterProduct && products[0].IsKit) {
                    var selectedVariantId: number = products[0].CompositionInformation.VariantInformation.ActiveVariantProductId;
                    if (selectedVariantId === 0) {
                        var productDetailOptions: ProductDetailOptions = new ProductDetailOptions();
                        productDetailOptions.recordId = products[0].RecordId;
                        productDetailOptions.product = products[0];
                        productDetailOptions.pageCallback = "KitDisassemblyView";
                        productDetailOptions.productAddModeEnum = ViewModels.ProductAddModeEnum.KitDisassembly;
                        ViewModelAdapter.navigate("ProductDetailsView", productDetailOptions);
                    } else {
                        this.clearKitDetail();
                        this.kitDisassemblyViewModel.populateViewModel(products[0], selectedVariantId, null);
                        this.commonHeaderData.categoryName(this.kitDisassemblyViewModel.kitId() + ", " + this.kitDisassemblyViewModel.kitName());
                        this.setKitQuantityToDisassemble();
                    }

                } else {
                    // display error if user selects non kit product
                    ViewModelAdapter.displayMessage(
                        ViewModelAdapter.getResourceString("string_3382"),
                        MessageType.Error,
                        MessageBoxButtons.Default,
                        ViewModelAdapter.getResourceString("string_3381"));
                }
            } else {
                // display error if user enters non existing product
                ViewModelAdapter.displayMessage(
                    ViewModelAdapter.getResourceString("string_3382"),
                    MessageType.Error,
                    MessageBoxButtons.Default,
                    ViewModelAdapter.getResourceString("string_3381"));
            }
        }

        private searchForProduct(products?: Proxy.Entities.Product[]): void {
            // navigate to product search view
            var parameters: any = {
                searchText: "",
                searchEntity: "Products",
                relatedItems: products,
                addModeEnum: ViewModels.ProductAddModeEnum.KitDisassembly,
                pageCallback: "KitDisassemblyView"
            };

            ViewModelAdapter.navigate("SearchView", parameters);
        }

        private clearKitDetail(): void {
            // clear the view model
            this.kitDisassemblyViewModel.clearViewModel();
            this.commonHeaderData.categoryName(ViewModelAdapter.getResourceString("string_419"));
            this.changeToProductMode();
        }

        /**
         * Switches view between templates.
         * @param {any} eventInfo The event information.
         */
        
        private showViewMenu(eventInfo: any): void {
            this.toggleShowHideMenu()();
        }
        

        /**
         * Gets the total number of components in the kit.
         * @return {number} The total number of kit components.
         */
        private computeTotalProducts(): number {
            return this.kitDisassemblyViewModel.componentLines().length;
        }

        /**
         * Handles Key stroke events
         * @param {JQueryEventObject} event The JQuery event object.
         */
        private handleKeys(event: JQueryEventObject): void {
            if (event.keyCode !== 13) {
                if (this.enterProductVisible()) {
                    $("#kitAssemblyProductInput").focus();
                } else {
                    $("#kitAssemblyQuantityInput").focus();
                }
            }
        }
    }
}