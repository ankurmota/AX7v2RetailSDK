/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../Controls/VariantDialog.ts'/>
///<reference path='../ViewControllerBase.ts'/>
///<reference path='SearchView.ts'/>

module Commerce.ViewControllers {
    "use strict";

    export enum SearchTextMode {
        EnterProduct = 1,
        AddQuantity = 2,
        EnterQuantity = 3
    }

    export class StockCountDetailsViewController extends ViewControllerBase {

        public textMode: Observable<SearchTextMode>;
        public stockCountViewModel: ViewModels.StockCountDetailsViewModel;
        public searchText: Observable<string>;
        public indeterminateWaitVisible: Observable<boolean>;
        public enterProductVisible: Computed<boolean>;
        public disableRefreshProductsInJournal: Computed<boolean>;
        public enterQuantityVisible: Computed<boolean>;
        public addQuantityVisible: Computed<boolean>;
        public isProductNotSelected: Computed<boolean>;
        public isCommitDisabled: Computed<boolean>;
        public hasProducts: Computed<boolean>;
        public totalProductsString: Computed<string>;
        public totalCountedString: Computed<string>;
        public totalQuantityString: Computed<string>;
        public commonHeaderData: Controls.CommonHeaderData;

        private _product: Model.Entities.Product;
        private _currentRecordId: number;
        private _productRecordId: number;
        private _productId: string;
        private _productVariant: Model.Entities.ProductVariant;
        private _rowAutoSelected: boolean;
        private _isJournalSaved: Observable<boolean>;
        private _productDetailsViewModel: ViewModels.ProductDetailsViewModel;
        private _variantDialog: Controls.VariantDialog;
        private _options: ViewModels.StockCountDetailsViewModelParameters;

        constructor(options?: ViewModels.StockCountDetailsViewModelParameters) {
            super(true);

            this._options = options || {};
            this._product = null;
            this._isJournalSaved = ko.observable(!ObjectExtensions.isNullOrUndefined(this._options.JournalId));

            this._productVariant = new Model.Entities.ProductVariantClass();
            this._rowAutoSelected = false;

            this.stockCountViewModel = new ViewModels.StockCountDetailsViewModel(this._options);
            this._productDetailsViewModel = new ViewModels.ProductDetailsViewModel();
            this.stockCountViewModel.defaultJournalDescription = this.createJournalDescription();

            this.textMode = ko.observable(SearchTextMode.EnterProduct);
            this.searchText = ko.observable("");
            this.indeterminateWaitVisible = ko.observable(false);
            this.enterProductVisible = ko.computed(this.computeEnterProductVisible, this);
            this.enterQuantityVisible = ko.computed(this.computeEnterQuantityVisible, this);
            this.hasProducts = ko.computed(() => { return ArrayExtensions.hasElements(this.stockCountViewModel.productLines()); }, this);
            this.isCommitDisabled = ko.computed(this.computeIsCommitDisabled, this);
            this.addQuantityVisible = ko.computed(this.computeAddQuantityVisible, this);
            this.isProductNotSelected = ko.computed(this.computeProductNotSelected, this);
            this.totalProductsString = ko.computed(this.computeTotalProductsString, this);
            this.totalCountedString = ko.computed(this.computeTotalCountedString, this);
            this.totalQuantityString = ko.computed(this.computeTotalQuantityString, this);

            this.disableRefreshProductsInJournal = ko.computed(() => {
                var journalId: string = this.stockCountViewModel.journalId();
                return this.enterQuantityVisible() || StringExtensions.isNullOrWhitespace(journalId);
            });

            this.commonHeaderData = new Controls.CommonHeaderData();
            this.commonHeaderData.viewSectionInfo(true);
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_3301")); // STOCK COUNT

            this.productQuantity.select();

            this.addControl(this._variantDialog = new Controls.VariantDialog());

            if (ObjectExtensions.isNullOrUndefined(this._options.VariantId)) {
                this._options.VariantId = -1;
            }

            this.indeterminateWaitVisible(true);
        }

        /**
         * Called when the page is loaded on the DOM.
         */
        public load(): void {

            this.stockCountViewModel.loadJournal()
                .done(() => {
                    var pageTitle: string = this.stockCountViewModel.journalId();
                    if (StringExtensions.isNullOrWhitespace(pageTitle)) {
                        pageTitle = Commerce.ViewModelAdapter.getResourceString("string_3302");
                    }

                    this.commonHeaderData.categoryName(pageTitle);

                    if (this._options.Product
                        && this._options.Product.RecordId > 0
                        && ((this._options.Product.IsMasterProduct && this._options.VariantId <= 0) || !this._options.Product.IsMasterProduct)) {
                        this.processSingleProduct(this._options.Product);
                    } else if (this._options.Product
                        && this._options.Product.RecordId > 0
                        && this._options.Product.IsMasterProduct
                        && this._options.VariantId > 0) {
                        this.addProductLineHighlight(this._options.VariantId, this._options.Product);
                    }

                    this.indeterminateWaitVisible(false);
                })
                .fail((errors: Model.Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);
                    Commerce.NotificationHandler.displayClientErrors(errors);
                });
        }

        public onShown(): void {
            Peripherals.instance.barcodeScanner.enableAsync((barcode: string) => {
                this.searchText(barcode);
                this.enterProductOrQuantity();
            });
        }

        public onHidden(): void {
            Peripherals.instance.barcodeScanner.disableAsync();
        }

        public deleteProductLine(): void {
            this.indeterminateWaitVisible(true);
            this.stockCountViewModel.deleteProductLine(this._currentRecordId, this._productId, this._productVariant)
                .done(() => {
                    this.deleteProductLineCleanup();
                })
                .fail((errors: Model.Entities.Error[]) => {
                    Commerce.NotificationHandler.displayClientErrors(errors);
                    this.deleteProductLineCleanup();
                });
        }

        public get productQuantity(): JQuery {
            if (this.textMode() === SearchTextMode.EnterProduct) {
                return $("#productInput");
            } else {
                return $("#quantityInput");
            }
        }

        public enterProductOrQuantityNumpad(numpadResult: Controls.NumPad.INumPadResult): void {
            this.enterProductOrQuantity();
        }

        public enterProductOrQuantity(): void {
            if (this.textMode() === SearchTextMode.EnterProduct) {
                this._currentRecordId = 0;
                this.indeterminateWaitVisible(true);
                this.stockCountViewModel.searchProduct(this.searchText())
                    .done((products: Model.Entities.Product[]) => {
                        this.enterProductSuccess(products);
                    }).fail((errors: Model.Entities.Error[]) => {
                        this.indeterminateWaitVisible(false);
                        Commerce.NotificationHandler.displayClientErrors(errors);
                    });
            } else {
                var quantity: number = NumberExtensions.parseNumber(this.searchText());

                if (quantity >= 0) {

                    if (this.textMode() === SearchTextMode.AddQuantity) {
                        this.addProductLine(quantity);
                    }

                    if (this.textMode() === SearchTextMode.EnterQuantity) {
                        this.enterProductLine(quantity);
                    }

                    this.clearSelection();
                    this.changeToProductMode();
                } else {
                    this.displayInvalidQuantityErrorMessage();
                }
            }
        }

        public productSelectionChanged(items: Model.Entities.StockCountLine[]): void {

            if (items.length !== 1) {
                this.changeToProductMode();
                return;
            }

            if (this._rowAutoSelected) {
                this._rowAutoSelected = false;
                return;
            }

            var firstItem: Model.Entities.StockCountLine = items[0];
            this._currentRecordId = firstItem.recordId;
            this._productId = firstItem.itemId;
            this._productVariant.ColorId = firstItem.inventColorId;
            this._productVariant.ConfigId = firstItem.configId;
            this._productVariant.SizeId = firstItem.inventSizeId;
            this._productVariant.StyleId = firstItem.inventStyleId;

            this.changeToEnterQuantityMode();
        }

        /**
         * Commits the journal.
         */
        public commitJournal(): void {

            if (this._isJournalSaved()) {
                this.commitJournalImpl();
            } else {
                this.saveJournal().done(() => {
                    this.commitJournalImpl();
                });
            }
        }

        /**
         * Saves the journal changes.
         * @return {IVoidAsyncResult} The async result.
         */
        public saveJournal(): IVoidAsyncResult {
            var asyncResult: VoidAsyncResult = new VoidAsyncResult(this);
            this.indeterminateWaitVisible(true);
            this.stockCountViewModel.saveJournal()
                .done(() => {
                    this._isJournalSaved(true);
                    this.commonHeaderData.categoryName(this.stockCountViewModel.journalId());
                    this.indeterminateWaitVisible(false);
                    asyncResult.resolve();
                }).fail((errors: Model.Entities.Error[]) => {
                    Commerce.NotificationHandler.displayClientErrors(errors);
                    asyncResult.reject(errors);
                    this.indeterminateWaitVisible(false);
                });

            return asyncResult;
        }

        /**
         * Close the journal and navigate to search journal page.
         */
        public closeJournal(): void {
            if (!this._isJournalSaved()) {
                // show UI that we cannot close journal if journal has not being saved yet.
                ViewModelAdapter.displayMessage(
                    Commerce.ViewModelAdapter.getResourceString("string_3386"),
                    MessageType.Error,
                    MessageBoxButtons.Default,
                    Commerce.ViewModelAdapter.getResourceString("string_3385"));
            } else {
                // navigate to journal search view
                Commerce.ViewModelAdapter.navigate("SearchStockCountView");
            }
        }

        /**
         * Refresh the committed product lines in the current journal back to uncommitted so they can be edited.
         * @return {IVoidAsyncResult} The async result.
         */
        public refreshProductLines(): IVoidAsyncResult {
            var asyncResult: VoidAsyncResult = new VoidAsyncResult(this);
            this.indeterminateWaitVisible(true);

            this.stockCountViewModel.refreshProductLines()
                .done(() => {
                    this.indeterminateWaitVisible(false);
                    asyncResult.resolve();
                }).fail((errors: Model.Entities.Error[]) => {
                    asyncResult.reject(errors);
                    Commerce.NotificationHandler.displayClientErrors(errors);
                    this.indeterminateWaitVisible(false);
                });

            return asyncResult;
        }

        private computeTotalProductsString(): string {
            var totalProducts: number = this.stockCountViewModel.totalProducts();
            return NumberExtensions.formatNumber(totalProducts, 0);
        }

        private computeTotalCountedString(): string {
            return NumberExtensions.formatNumber(this.stockCountViewModel.totalCounted(), NumberExtensions.getDecimalPrecision());
        }

        private computeTotalQuantityString(): string {
            return NumberExtensions.formatNumber(this.stockCountViewModel.totalQuantity(), NumberExtensions.getDecimalPrecision());
        }

        private computeProductNotSelected(): boolean {
            return this.textMode() !== SearchTextMode.AddQuantity && this.textMode() !== SearchTextMode.EnterQuantity;
        }

        private computeEnterProductVisible(): boolean {
            return this.textMode() === SearchTextMode.EnterProduct;
        }

        private computeAddQuantityVisible(): boolean {
            return this.textMode() === SearchTextMode.AddQuantity;
        }

        private computeEnterQuantityVisible(): boolean {
            return this.textMode() === SearchTextMode.EnterQuantity;
        }

        private computeIsCommitDisabled(): boolean {
            return this.enterQuantityVisible() || !this.hasProducts();
        }

        private triggerVariantSelection(): void {
            this._variantDialog.show({ products: [this._product] })
                .on(DialogResult.OK, (result: number[]) => { this.variantSuccessCallback(result); })
                .on(DialogResult.Cancel, (result: number[]) => { this.variantCancelCallback(); })
                .onError((errors: Model.Entities.Error[]) => { this.variantErrorCallback(errors); });
        }

        private variantSuccessCallback(variantIds: number[]): void {
            // product is non-variant, add product to lines with 1 quantity.
            this.addProductLineHighlight(variantIds[0], this._product);
        }

        private variantErrorCallback(errors: Model.Entities.Error[]): void {
            this._product = null;
            Commerce.NotificationHandler.displayClientErrors(errors);
        }

        private variantCancelCallback(): void {
            this.clearSelection();
            this.changeToProductMode();
        }

        private getProductIndex(): number {
            return this.stockCountViewModel.getProductIndex(this._currentRecordId, this._productId,
                this._productVariant.ColorId,
                this._productVariant.ConfigId,
                this._productVariant.SizeId,
                this._productVariant.StyleId);
        }

        /**
         * Changes the page input to receive a quantity and add it to the selected line.
         */
        private changeToAddQuantityMode(): void {
            this.textMode(SearchTextMode.AddQuantity);
            var quantity: number = 1; // Since quantity is added from previous quantity, set qty to the lowest, valid ones (1).

            this.populateProductOrQuantityInput(quantity.toString());
        }

        /**
         * Changes the page input to receive a quantity and overwrite the selected line.
         */
        private changeToEnterQuantityMode(): void {
            this.textMode(SearchTextMode.EnterQuantity);
            var rowIndex: number = this.getProductIndex();
            var quantity: number = this.stockCountViewModel.productLines()[rowIndex].quantity;

            if (quantity === 0) {
                this.populateProductOrQuantityInput("1");
            } else {
                this.populateProductOrQuantityInput(quantity.toString());
            }
        }

        /**
         * Changes the page input to receive a product search term.
         */
        private changeToProductMode(): void {
            this.textMode(SearchTextMode.EnterProduct);
            this._product = null;
            this.populateProductOrQuantityInput("");
        }

        /**
         * Adds a string to the page input with select and focus.
         * @param {number} quantityOrProduct The string to use in the input.
         */
        private populateProductOrQuantityInput(quantityOrProduct: string): void {
            this.searchText(quantityOrProduct);

            // This is a short-term work around to ensure focus and being able to edit happen.
            // Need to wait for methods underneath to finish, ideally call pattern would be direct.
            setTimeout(() => {
                this.productQuantity.select();
                this.productQuantity.focus();
            }, 100);
        }

        private displayInvalidQuantityErrorMessage(): void {
            // show UI for not valid quantity
            ViewModelAdapter.displayMessage(
                Commerce.ViewModelAdapter.getResourceString("string_3388"), // The quantity is not valid. Select a line to directly edit the quantity.
                MessageType.Error,
                MessageBoxButtons.Default,
                Commerce.ViewModelAdapter.getResourceString("string_3383")); // The quantity is not valid

            this.productQuantity.select();
        }

        private createJournalDescription(): string {
            var journalDescription: string = Commerce.ViewModelAdapter.getResourceString("string_3380");
            var currentDeviceConfiguration: Model.Entities.DeviceConfiguration = ApplicationContext.Instance.deviceConfiguration;
            var currentDate: Date = new Date();
            var dateFormat: string = (currentDate.getMonth() + 1) + "/" + currentDate.getDate() + "/" + currentDate.getFullYear();
            journalDescription = StringExtensions.format(journalDescription, currentDeviceConfiguration.InventLocationId, dateFormat);

            return journalDescription;
        }

        private processSingleProduct(product: Model.Entities.Product): void {

            this._product = product;
            if (product.IsMasterProduct) {

                var productVariant: Model.Entities.ProductVariant = ProductPropertiesHelper.getActiveVariant(product);

                if (!ObjectExtensions.isNullOrUndefined(productVariant)) {
                    // Active variant is available, add product to product lines.
                    this.addProductLineHighlight(productVariant.DistinctProductVariantId, this._product);
                } else if (product.IsKit) {
                    // If product is kit and there is no active variant selected, then navigate to product details view 
                    // to choose kit variants as there is no need to re- read the full product object which includes all variants
                    this.navigateToProductDetails(product);
                } else {
                    this._productDetailsViewModel.getProductDetails([product.RecordId])
                        .done((productDetails: Model.Entities.Product[]) => {
                            this._product = productDetails[0];

                            // This is a regular master product. Trigger variant selection
                            // to get variant that user wants.
                            this.triggerVariantSelection();
                        }).fail((errors: Model.Entities.Error[]) => {
                            Commerce.NotificationHandler.displayClientErrors(errors);
                        });
                }
            } else {
                // product is not a variant, add product to product lines.
                this.addProductLineHighlight(product.RecordId, this._product);
            }
        }

        private navigateToProductDetails(product: Model.Entities.Product): void {
            // Save journal first before navigating to Kit Details View
            this.saveJournal()
                .done(() => {
                    var parameters: any = {
                        JournalId: this.stockCountViewModel.stockCountJournal().JournalId
                    };

                    var productDetailOptions: ProductDetailOptions = new ProductDetailOptions();
                    productDetailOptions.recordId = product.RecordId;
                    productDetailOptions.product = product;
                    productDetailOptions.pageCallback = "StockCountDetailsView";
                    productDetailOptions.additionalParams = parameters;
                    productDetailOptions.productAddModeEnum = ViewModels.ProductAddModeEnum.StockCount;

                    Commerce.ViewModelAdapter.navigate("ProductDetailsView", productDetailOptions);
                });
        }

        private enterProductSuccess(products: Model.Entities.Product[]): void {
            this.indeterminateWaitVisible(false);
            if (products.length > 1) {
                // navigate to product search page
                this.searchForProduct(products);
            } else if (products.length === 1) {
                this.processSingleProduct(products[0]);
            } else {
                // show UI no products in this stock count
                ViewModelAdapter.displayMessage(
                    Commerce.ViewModelAdapter.getResourceString("string_3382"),
                    MessageType.Error,
                    MessageBoxButtons.Default,
                    Commerce.ViewModelAdapter.getResourceString("string_3381"));
                this.productQuantity.select();
            }
        }

        private searchForProduct(products?: Model.Entities.Product[]): void {

            // save the journal first before navigating to Product Search page.

            this.saveJournal()
                .done(() => {
                    // navigate to product search view page
                    var parameters: any = {
                        searchText: this.searchText(),
                        searchEntity: "Products",
                        relatedItems: products,
                        addModeEnum: ViewModels.ProductAddModeEnum.StockCount,
                        JournalId: this.stockCountViewModel.stockCountJournal().JournalId,
                        ProductDetailsDictionary: this.stockCountViewModel.productDetailsDictionary,
                        pageCallback: "StockCountDetailsView"
                    };

                    Commerce.ViewModelAdapter.navigate("SearchView", parameters);
                });
        }

        private addProductLineHighlight(variantId: number, product: Model.Entities.Product): void {
            // This method is being executed after user enters the existing product number / barcode.
            // 1 . User enters a product number / barcode
            // 2a. If product already exists on products grid, highlight the row and change input mode to enter quantity
            // 2b. If product grid does not have the product, add product to line grid with qty = 1, highlight the row and change input mode to enter quantity
            //    Also highlight the textbox to become '1' so user just can 
            //    type input easily to override number and/ or hit enter to submit the desired quantity.

            this._productRecordId = variantId;
            this._product = product;
            this._productId = ProductPropertiesHelper.getProperty(this._productRecordId, this._product, ProductPropertyNameEnum.ProductNumber);

            if (product.IsMasterProduct) {
                this._productVariant = ProductPropertiesHelper.getVariant(this._productRecordId, product);
            } else {
                this._productVariant.ColorId = null;
                this._productVariant.ConfigId = null;
                this._productVariant.SizeId = null;
                this._productVariant.StyleId = null;
            }

            var rowIndex: number = this.getProductIndex();
            if (rowIndex === -1) {
                this.addProductLine(0);
                this.changeToEnterQuantityMode();
            } else {
                this.highlightIndex(rowIndex);
                this.changeToAddQuantityMode();
            }
        }

        private clearSelection(): void {
            // clear all selections
            Commerce.Host.instance.timers.setImmediate(() => {
                var listView: any = document.getElementById("stockCountDetailsView").winControl;
                listView.selection.clear();
            });
        }

        private highlightIndex(rowIndex: number): void {
            // make the UI grid to select the respective index
            this._rowAutoSelected = true;
            Commerce.Host.instance.timers.setImmediate(() => {
                var listView: any = document.getElementById("stockCountDetailsView").winControl;
                listView.selection.set(rowIndex);
                listView.ensureVisible(rowIndex);
            });
        }

        private addProductLine(quantity: number): void {
            // add the new quantity to the old quantity
            var indexResult: number = this.getProductIndex();

            if (indexResult >= 0) {
                this.stockCountViewModel.addExistingProductLine(indexResult, quantity);
            } else {
                indexResult = this.stockCountViewModel.addNewProductLine(this._productRecordId, this._product, quantity);
            }

            this._isJournalSaved(false);
            this.highlightIndex(indexResult);
        }

        /**
         * Overwrites the quantity of selected line
         * 
         * @param {number} quantity The new quantity to be used
         */
        private enterProductLine(quantity: number): void {
            // overwrite the old quantity with the new quantity
            var indexResult: number = this.getProductIndex();
            var errors: Model.Entities.Error[] = this.stockCountViewModel.overwriteExistingProductLine(indexResult, quantity);

            if (ArrayExtensions.hasElements(errors)) {
                NotificationHandler.displayClientErrors(errors);
            } else {
                this._isJournalSaved(false);
                this.highlightIndex(indexResult);
            }
        }

        private commitJournalImpl(): void {
            this.indeterminateWaitVisible(true);
            this.stockCountViewModel.commitJournal()
                .done(() => {
                    // refresh product lines back to local database to show accurate number of products when we sync journal header to local database.
                    this.refreshProductLines()
                        .done(() => {
                            this.indeterminateWaitVisible(false);
                            Commerce.ViewModelAdapter.navigate("SearchStockCountView");
                        }).fail((errors: Model.Entities.Error[]) => {
                            // no need to display error message since refreshProductLines method already does that.
                            this.indeterminateWaitVisible(false);
                        });

                }).fail((errors: Model.Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);
                    Commerce.NotificationHandler.displayClientErrors(errors);
                });
        }

        private deleteProductLineCleanup(): void {
            this.textMode(SearchTextMode.EnterProduct);
            this.searchText(StringExtensions.EMPTY);
            this.indeterminateWaitVisible(false);
        }
    }
}