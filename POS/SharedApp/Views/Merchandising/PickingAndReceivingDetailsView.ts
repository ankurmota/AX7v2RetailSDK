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
///<reference path='SearchView.ts'/>
///<reference path='../ViewControllerBase.ts'/>
///<reference path='../../Core/Converters.ts'/>

module Commerce.ViewControllers {
    "use strict";

    export enum PickingAndReceivingTextMode {
        EnterProduct = 1,
        EnterReceivedNow = 2,
        AddToReceivedNow = 3
    }

    export class PickingAndReceivingDetailsViewController extends ViewControllerBase {
        private static _statusCreated: string = "Created";

        public indeterminateWaitVisible: Observable<boolean>;
        public detailsViewModel: Commerce.ViewModels.PickingAndReceivingDetailsViewModel;
        public orderType: Model.Entities.PurchaseTransferOrderType;
        public searchKeyword: Observable<string>;
        public enterReceivedNowVisible: Computed<boolean>;
        public enterProductVisible: Computed<boolean>;
        public addToReceivedNowVisible: Computed<boolean>;
        public textMode: Observable<PickingAndReceivingTextMode>;
        public orderedOrTransferredLabel: Observable<string>;
        public receivedOrShippedLabel: Observable<string>;
        public receivedNowOrShippedNowLabel: Observable<string>;
        public addToReceivedNowOrShippedNowLabel: Observable<string>;
        public receiveAllOrShipAllLabel: Observable<string>;
        public commonHeaderData: Controls.CommonHeaderData;
        public isTransferOut: Observable<boolean>;

        private _isJournalSaved: Observable<boolean>;
        private _currentProduct: Model.Entities.Product;
        private _currentVariantId: number;
        private _currentColorId: string;
        private _currentConfigurationId: string;
        private _currentSizeId: string;
        private _currentStyleId: string;
        private _currentProductId: string;
        private _currentRecordId: number;
        private _rowAutoSelected: boolean;
        private _productDetailsViewModel: ViewModels.ProductDetailsViewModel;

        private _variantDialog: Controls.VariantDialog;
        private _options: ViewModels.PickingAndReceivingDetailsViewModelParameters;

        constructor(options?: ViewModels.PickingAndReceivingDetailsViewModelParameters) {
            super(true);

            this._options = options || {};
            this._isJournalSaved = ko.observable(true);
            this.detailsViewModel = new Commerce.ViewModels.PickingAndReceivingDetailsViewModel(this._options);
            this._productDetailsViewModel = new ViewModels.ProductDetailsViewModel();
            this.searchKeyword = ko.observable(StringExtensions.EMPTY);

            this._currentColorId = StringExtensions.EMPTY;
            this._currentConfigurationId = StringExtensions.EMPTY;
            this._currentSizeId = StringExtensions.EMPTY;
            this._currentStyleId = StringExtensions.EMPTY;
            this._rowAutoSelected = false;

            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();

            // Load Common Header
            this.commonHeaderData.viewSectionInfo(true);
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);
            this.orderType = this._options.JournalType;
            this.commonHeaderData.sectionTitle(Commerce.Formatters.PurchaseTransferOrderEnumFormatter(this.orderType));
            this.setTitle(this._options.JournalId);

            this.textMode = ko.observable(PickingAndReceivingTextMode.EnterProduct);
            this.enterProductVisible = ko.computed(this.computeEnterProductVisible, this);
            this.enterReceivedNowVisible = ko.computed(this.computeEnterReceivedNowVisible, this);
            this.addToReceivedNowVisible = ko.computed(this.computeAddToReceivedNowVisible, this);
            this.isTransferOut = ko.observable(false);

            this.orderedOrTransferredLabel = ko.observable(StringExtensions.EMPTY);
            this.receivedOrShippedLabel = ko.observable(StringExtensions.EMPTY);
            this.receivedNowOrShippedNowLabel = ko.observable(StringExtensions.EMPTY);
            this.addToReceivedNowOrShippedNowLabel = ko.observable(StringExtensions.EMPTY);
            this.receiveAllOrShipAllLabel = ko.observable(StringExtensions.EMPTY);

            this.receivedNowInput.select();

            this.addControl(this._variantDialog = new Controls.VariantDialog());

            if (ObjectExtensions.isNullOrUndefined(this._options.VariantId)) {
                this._options.VariantId = -1;
            }

            this.indeterminateWaitVisible = ko.observable(true);
        }

        /**
         * Called when the page is loaded on the DOM.
         */
        public load(): void {
            this.detailsViewModel.loadJournal()
                .done(() => {
                    this.indeterminateWaitVisible(false);

                    if (this._options.Product
                        && this._options.Product.RecordId > 0
                        && ((this._options.Product.IsMasterProduct && this._options.VariantId <= 0) || !this._options.Product.IsMasterProduct)) {
                        this.processSingleProduct(this._options.Product);
                    } else if (this._options.Product
                        && this._options.Product.RecordId > 0
                        && this._options.Product.IsMasterProduct
                        && this._options.VariantId > 0) {
                        this.addProductLineHighlight(this._options.VariantId, this._options.Product);
                        this.receivedNowInput.select();
                    }

                    var journalHeader: Model.Entities.PickingAndReceivingOrder = this.detailsViewModel.journalHeader();

                    switch (journalHeader.orderType) {
                        case Model.Entities.PurchaseTransferOrderType.PurchaseOrder:
                            this.orderedOrTransferredLabel(Commerce.ViewModelAdapter.getResourceString("string_3722")); // ORDERED
                            this.receivedOrShippedLabel(Commerce.ViewModelAdapter.getResourceString("string_3723")); // RECEIVED
                            this.receivedNowOrShippedNowLabel(Commerce.ViewModelAdapter.getResourceString("string_3742")); // Received now
                            this.receiveAllOrShipAllLabel(Commerce.ViewModelAdapter.getResourceString("string_3820")); // Receive all
                            this.addToReceivedNowOrShippedNowLabel(Commerce.ViewModelAdapter.getResourceString("string_3744")); // Add to Received now
                            break;
                        case Model.Entities.PurchaseTransferOrderType.TransferIn:
                            if (journalHeader.status === PickingAndReceivingDetailsViewController._statusCreated) {
                                this.orderedOrTransferredLabel(Commerce.ViewModelAdapter.getResourceString("string_3728")); // TRANSFER QUANTITY
                            } else {
                                this.orderedOrTransferredLabel(Commerce.ViewModelAdapter.getResourceString("string_3726")); // TRANSFERRED
                            }
                            this.receivedOrShippedLabel(Commerce.ViewModelAdapter.getResourceString("string_3723")); // RECEIVED
                            this.receivedNowOrShippedNowLabel(Commerce.ViewModelAdapter.getResourceString("string_3742")); // Received now
                            this.receiveAllOrShipAllLabel(Commerce.ViewModelAdapter.getResourceString("string_3820")); // Receive all
                            this.addToReceivedNowOrShippedNowLabel(Commerce.ViewModelAdapter.getResourceString("string_3744")); // Add to Received now
                            break;
                        case Model.Entities.PurchaseTransferOrderType.TransferOut:
                            if (journalHeader.status === PickingAndReceivingDetailsViewController._statusCreated) {
                                this.orderedOrTransferredLabel(Commerce.ViewModelAdapter.getResourceString("string_3728")); // TRANSFER QUANTITY
                            } else {
                                this.orderedOrTransferredLabel(Commerce.ViewModelAdapter.getResourceString("string_3726")); // TRANSFERRED
                            }
                            this.receivedOrShippedLabel(Commerce.ViewModelAdapter.getResourceString("string_3727")); // SHIPPED
                            this.receivedNowOrShippedNowLabel(Commerce.ViewModelAdapter.getResourceString("string_3745")); // Ship now
                            this.receiveAllOrShipAllLabel(Commerce.ViewModelAdapter.getResourceString("string_4303")); // Ship all
                            this.addToReceivedNowOrShippedNowLabel(Commerce.ViewModelAdapter.getResourceString("string_3746")); // Add to Shipped now
                            this.isTransferOut(true);
                            break;
                        case Model.Entities.PurchaseTransferOrderType.PickingList:
                            this.orderedOrTransferredLabel(Commerce.ViewModelAdapter.getResourceString("string_3722")); // ORDERED
                            this.receivedOrShippedLabel(Commerce.ViewModelAdapter.getResourceString("string_3723")); // RECEIVED
                            this.receivedNowOrShippedNowLabel(Commerce.ViewModelAdapter.getResourceString("string_3742")); // Received now
                            this.receiveAllOrShipAllLabel(Commerce.ViewModelAdapter.getResourceString("string_3820")); // Receive all
                            this.addToReceivedNowOrShippedNowLabel(Commerce.ViewModelAdapter.getResourceString("string_3744")); // Add to Received now
                            break;
                    }
                })
                .fail((errors: Model.Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);
                    RetailLogger.viewsMerchandisingPickingAndReceivingDetailsViewLoadJournalFailed(this._options.JournalId);
                    Commerce.NotificationHandler.displayClientErrors(errors);
                });
        }

        public onShown(): void {
            Peripherals.instance.barcodeScanner.enableAsync((barcode: string) => {
                this.searchKeyword(barcode);
                this.enterProductOrReceivedNow();
            });
        }

        public onHidden(): void {
            Peripherals.instance.barcodeScanner.disableAsync();
        }


        public get receivedNowInput(): JQuery {
            if (this.textMode() === PickingAndReceivingTextMode.EnterProduct) {
                return $("#productInput");
            }

            return $("#quantityInput");
        }

        public saveJournal(): IVoidAsyncResult {
            this.indeterminateWaitVisible(true);
            return this.detailsViewModel.saveJournal()
                .done(() => {
                this.indeterminateWaitVisible(false);
                this._isJournalSaved(true);
            }).fail((errors: Model.Entities.Error[]) => {
                this.indeterminateWaitVisible(false);
                Commerce.NotificationHandler.displayClientErrors(errors);
            });
        }

        public commitJournal(): void {
            if (this._isJournalSaved()) {
                this.commitJournalImpl();
            } else {
                this.saveJournal()
                    .done(() => {
                    this.commitJournalImpl();
                });
            }
        }

        public commitJournalImpl(): void {
            this.indeterminateWaitVisible(true);

            this.detailsViewModel.commitJournal()
                .done(() => {
                this.indeterminateWaitVisible(false);
                if (this.orderType === Model.Entities.PurchaseTransferOrderType.PickingList) {
                    Commerce.ViewModelAdapter.displayMessage(
                        Commerce.ViewModelAdapter.getResourceString("string_3868"),
                        Commerce.MessageType.Info,
                        Commerce.MessageBoxButtons.Default).done(function (): void {
                        // navigate to journal search view
                        Commerce.ViewModelAdapter.navigate("SearchPickingAndReceivingView");
                    });
                } else {
                    // navigate to journal search view
                    Commerce.ViewModelAdapter.navigate("SearchPickingAndReceivingView");
                }

            }).fail((errors: Model.Entities.Error[]) => {
                this.indeterminateWaitVisible(false);
                Commerce.NotificationHandler.displayClientErrors(errors);
            });
        }

        public closeJournal(): void {
            if (!this._isJournalSaved()) {
                // show UI that we cannot close journal if journal has not being saved yet.
                ViewModelAdapterWinJS.displayMessage(
                    Commerce.ViewModelAdapterWinJS.getResourceString("string_3861"),
                    MessageType.Error,
                    MessageBoxButtons.Default,
                    Commerce.ViewModelAdapterWinJS.getResourceString("string_3860")
                    );
            } else {
                // navigate to journal search view
                Commerce.ViewModelAdapter.navigate("SearchPickingAndReceivingView");
            }
        }

        public receiveOrShipAllLines(): void {
            this.detailsViewModel.receiveAllLines();
            this._isJournalSaved(false);
        }

        public enterProductOrReceivedNowNumpad(numpadResult: Controls.NumPad.INumPadResult): void {
            this.enterProductOrReceivedNow();
        }

        public enterProductOrReceivedNow(): void {
            if (this.textMode() === PickingAndReceivingTextMode.EnterProduct) {
                this._currentRecordId = 0;
                this.indeterminateWaitVisible(true);
                this.detailsViewModel.searchProduct(this.searchKeyword())
                    .done((products: Model.Entities.Product[]) => {
                    this.enterProductSuccess(products);
                }).fail((errors: Model.Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);
                    Commerce.NotificationHandler.displayClientErrors(errors);
                });

            } else if (this.textMode() === PickingAndReceivingTextMode.EnterReceivedNow) {
                var quantity: number = NumberExtensions.parseNumber(this.searchKeyword());
                if (!this.hasQuantityErrors(quantity)) {
                    this.overwriteProductLine(quantity);
                    this.changeToProductMode();
                }

            } else if (this.textMode() === PickingAndReceivingTextMode.AddToReceivedNow) {
                var newQuantity: number = NumberExtensions.parseNumber(this.searchKeyword());
                var previousQuantity: number = this.detailsViewModel.lineDetails()[this.getProductIndex()].quantityReceivedNow;
                if (!this.hasQuantityErrors(newQuantity + previousQuantity)) {
                    this.addProductLine(newQuantity);
                    this.changeToProductMode();
                }
            }
        }

        public productSelectionChanged(items: Model.Entities.PickingAndReceivingOrderLine[]): void {
            if (items.length !== 1) {

                if (!this.computeEnterProductVisible() || items.length > 1) {
                    this.changeToProductMode();
                }

                return;
            }

            if (this._rowAutoSelected) {
                this._rowAutoSelected = false;
                return;
            }

            this._currentRecordId = items[0].recordId;
            this._currentProductId = items[0].productNumber;
            this._currentColorId = items[0].colorId;
            this._currentConfigurationId = items[0].configurationId;
            this._currentSizeId = items[0].sizeId;
            this._currentStyleId = items[0].styleId;

            this.changeToOverwriteQuantityMode();
        }

        private processSingleProduct(product: Model.Entities.Product): void {

            if (product.IsMasterProduct) {

                var productVariant: Model.Entities.ProductVariant = ProductPropertiesHelper.getActiveVariant(product);

                        if (!ObjectExtensions.isNullOrUndefined(productVariant)) {
                            // Active variant is available, add product to product lines.
                    this.addProductLineHighlight(productVariant.DistinctProductVariantId, product);
                } else if (product.IsKit) {
                    // If product is kit and an active variant is not selected, navigate to product details view
                    // to choose kit variants as there is no need to re- read the full product object which includes all variants
                    this.navigateToProductDetails(product);
                        } else {
                            // This is a regular master product. Trigger variant selection
                            // to get variant that user wants.
                    this._productDetailsViewModel.getProductDetails([product.RecordId])
                        .done((productDetails: Model.Entities.Product[]) => {
                            this.triggerVariantSelection(productDetails[0]);
                })
                .fail((errors: Model.Entities.Error[]) => {
                    Commerce.NotificationHandler.displayClientErrors(errors);
                });
        }
            } else {
                // product is nonvariant, add product to product lines.
                this.addProductLineHighlight(product.RecordId, product);
            }
        }

        private navigateToProductDetails(product: Model.Entities.Product): void {
            // save journals first before navigating to Kit Details View
            this.saveJournal()
                .done(() => {
                    this.indeterminateWaitVisible(false);
                    var parameters: any = {
                        JournalId: this.detailsViewModel.journalId,
                        JournalType: this.detailsViewModel.journalHeader().orderType
                    };

                    var productDetailOptions: ProductDetailOptions = new ProductDetailOptions();
                    productDetailOptions.recordId = product.RecordId;
                    productDetailOptions.pageCallback = "PickingAndReceivingDetailsView";
                    productDetailOptions.additionalParams = parameters;
                    productDetailOptions.productAddModeEnum = ViewModels.ProductAddModeEnum.PickingAndReceiving;
                    productDetailOptions.product = product;
                    Commerce.ViewModelAdapter.navigate("ProductDetailsView", productDetailOptions);
                });
        }


        private computeEnterProductVisible(): boolean {
            return this.textMode() === PickingAndReceivingTextMode.EnterProduct;
        }

        private computeEnterReceivedNowVisible(): boolean {
            return this.textMode() === PickingAndReceivingTextMode.EnterReceivedNow;
        }

        /**
         * Determines page input mode
         * @return {boolean} True if page is in Add to Received now, false otherwise.
         */
        private computeAddToReceivedNowVisible(): boolean {
            return this.textMode() === PickingAndReceivingTextMode.AddToReceivedNow;
        }

        private setTitle(pageTitle: string): void {
            if (StringExtensions.isNullOrWhitespace(pageTitle)) {
                pageTitle = "";
            }

            this.commonHeaderData.categoryName(pageTitle);
        }

        private searchForProduct(products?: Model.Entities.Product[]): void {

            // save the journal first before navigating to Product Search page.
            this.saveJournal()
                .done(() => {
                    // navigate to product search view page
                    var parameters: any = {
                        searchText: this.searchKeyword(),
                        searchEntity: "Products",
                        relatedItems: products,
                        addModeEnum: ViewModels.ProductAddModeEnum.PickingAndReceiving,
                        JournalId: this.detailsViewModel.journalId,
                        JournalType: this.detailsViewModel.journalHeader().orderType,
                        ProductDetailsDictionary: this.detailsViewModel.productDetailsDictionary,
                        pageCallback: "PickingAndReceivingDetailsView"
                    };

                    Commerce.ViewModelAdapter.navigate("SearchView", parameters);
                });
        }

        private triggerVariantSelection(product: Model.Entities.Product): void {
            this._variantDialog.show({ products: [product] })
                .on(DialogResult.OK, (variantIds: number[]) => {
                    this.addProductLineHighlight(variantIds[0], product);
                    this.receivedNowInput.focus();
                }).on(DialogResult.Cancel, (result: number[]) => {
                    this.changeToProductMode();
                }).onError((errors: Model.Entities.Error[]) => {
                    Commerce.NotificationHandler.displayClientErrors(errors);
                });
        }

        private clearSelection(): void {
            // clear all selections
            Commerce.Host.instance.timers.setImmediate(() => {
                var listView: any = document.getElementById("pickAndReceiveDetailsView").winControl;
                listView.selection.clear();
            });
        }

        /**
         * Sets input to replace the selected line's quantity with the given value.
         */
        private changeToOverwriteQuantityMode(): void {
            this.textMode(PickingAndReceivingTextMode.EnterReceivedNow);
            var rowIndex: number = this.getProductIndex();
            var quantity: number = this.detailsViewModel.lineDetails()[rowIndex].quantityReceivedNow;

            this.searchKeyword(quantity.toString());
            this.selectAndFocusTextbox();
        }

        private changeToProductMode(): void {
            this.clearSelection();
            this._currentProduct = null;
            this._currentVariantId = -1;
            this.textMode(PickingAndReceivingTextMode.EnterProduct);
            this.clearAndFocusTextbox();
        }

        /**
         * Sets input to add given value to selected line's quantity.
         */
        private changeToAddToQuantityMode(): void {
            this.textMode(PickingAndReceivingTextMode.AddToReceivedNow);
            var quantity: number = 1;

            this.searchKeyword(quantity.toString());
            this.selectAndFocusTextbox();
        }

        /**
         * Select and focus the numpad input
         */
        private selectAndFocusTextbox(): void {
            // Need to wait for UI processing to finish.
            setTimeout(() => {
                this.receivedNowInput.focus();
                this.receivedNowInput.select();
            }, 100);
        }

        private clearAndFocusTextbox(): void {
            this.searchKeyword("");
            this.receivedNowInput.focus();
        }

        private highlightIndex(rowIndex: number): void {
            // make the UI grid to select the respective index
            this._rowAutoSelected = true;
            Commerce.Host.instance.timers.setImmediate(() => {
                var listView: any = document.getElementById("pickAndReceiveDetailsView").winControl;
                listView.selection.set(rowIndex);
                listView.ensureVisible(rowIndex);
            });
        }

        private getProductIndex(): number {
            return this.detailsViewModel.getProductIndex(this._currentRecordId, this._currentProductId,
                this._currentColorId,
                this._currentConfigurationId,
                this._currentSizeId,
                this._currentStyleId);
        }

		/**
		 * Increases the quantity of the selected line by some value
		 * @param {number} quantity The amount to increase the line's quantity by.
		 */
        private addProductLine(quantity: number): void {
            var indexResult: number = this.getProductIndex();
            var errors: Model.Entities.Error[] = [];

            if (indexResult >= 0) {
                errors = this.detailsViewModel.addToExistingProductLine(indexResult, quantity);

                if (ArrayExtensions.hasElements(errors)) {
                    NotificationHandler.displayClientErrors(errors);
                }

            } else {
                indexResult = this.detailsViewModel.addNewProductLine(this._currentVariantId, this._currentProduct, quantity);
            }

            this._isJournalSaved(false);
            this.highlightIndex(indexResult);
        }

		/**
		 * Overwrites the selected line's quantity with a new value
		 * @param {number} quantity The new quantity of the product line
		 */
        private overwriteProductLine(quantity: number): void {
            var indexResult: number = this.getProductIndex();
            var errors: Model.Entities.Error[] = [];

            if (indexResult >= 0) {
                errors = this.detailsViewModel.overwriteExistingProductLine(indexResult, quantity);

                if (ArrayExtensions.hasElements(errors)) {
                    NotificationHandler.displayClientErrors(errors);
                }

            } else {
                indexResult = this.detailsViewModel.addNewProductLine(this._currentVariantId, this._currentProduct, quantity);
            }

            this._isJournalSaved(false);
            this.highlightIndex(indexResult);
        }

        private showAddProductError(orderType: Model.Entities.PurchaseTransferOrderType): void {
            var orderTypeString: string = "";

            switch (orderType) {
                case Model.Entities.PurchaseTransferOrderType.PickingList:
                    orderTypeString = Commerce.ViewModelAdapterWinJS.getResourceString("string_3867");
                    break;
                case Model.Entities.PurchaseTransferOrderType.TransferOut:
                    orderTypeString = Commerce.ViewModelAdapterWinJS.getResourceString("string_3864");
                    break;
            }

            var errorMessage: string = Commerce.ViewModelAdapterWinJS.getResourceString("string_3866");

            ViewModelAdapterWinJS.displayMessage(
                StringExtensions.format(errorMessage, orderTypeString),
                MessageType.Error,
                MessageBoxButtons.Default,
                Commerce.ViewModelAdapterWinJS.getResourceString("string_3865")
                );
        }

        private addProductLineHighlight(variantId: number, product: Model.Entities.Product): void {
            // This method is being executed after user enters the existing product number / barcode.
            // 1 . User enters a product number / barcode
            // 2a. If product already exists on products grid, highlight the row and change input mode to add to quantity
            // 2b. If product grid does not have the product, add product to line grid with qty = 1, highlight the row and change input mode to enter quantity
            //    Also highlight the textbox to become '1' so user just can type
            //    input easily to override number and/ or hit enter to submit the desired quantity.

            this._currentProduct = product;
            this._currentVariantId = variantId;
            this._currentProductId = ProductPropertiesHelper.getProperty(this._currentVariantId, this._currentProduct, ProductPropertyNameEnum.ProductNumber);

            if (product.IsMasterProduct) {
                var productVariant: Model.Entities.ProductVariant = ProductPropertiesHelper.getVariant(this._currentVariantId, this._currentProduct);
                this._currentColorId = productVariant.ColorId;
                this._currentConfigurationId = productVariant.ConfigId;
                this._currentSizeId = productVariant.SizeId;
                this._currentStyleId = productVariant.StyleId;
            } else {
                this._currentColorId = StringExtensions.EMPTY;
                this._currentConfigurationId = StringExtensions.EMPTY;
                this._currentSizeId = StringExtensions.EMPTY;
                this._currentStyleId = StringExtensions.EMPTY;
            }

            var rowIndex: number = this.getProductIndex();
            var defaultQuantity: number = 1;

            if (rowIndex === -1) {

                var orderType: Model.Entities.PurchaseTransferOrderType = this.detailsViewModel.journalHeader().orderType;
                if (orderType === Model.Entities.PurchaseTransferOrderType.TransferOut ||
                    orderType === Model.Entities.PurchaseTransferOrderType.PickingList) {

                    this.showAddProductError(orderType);
                    this.receivedNowInput.select();
                    return;
                }

                this.addProductLine(defaultQuantity);
                this.changeToOverwriteQuantityMode();
            } else {
                this.highlightIndex(rowIndex);
                defaultQuantity = this.detailsViewModel.lineDetails()[rowIndex].quantityReceivedNow;
                this.changeToAddToQuantityMode();
            }
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
                ViewModelAdapterWinJS.displayMessage(
                    Commerce.ViewModelAdapterWinJS.getResourceString("string_3382"),
                    MessageType.Error,
                    MessageBoxButtons.Default,
                    Commerce.ViewModelAdapterWinJS.getResourceString("string_3381")
                    );
                this.receivedNowInput.select();
            }
        }

        private displayInvalidQuantityErrorMessage(): void {
            // show UI for not valid quantity
            ViewModelAdapterWinJS.displayMessage(
                Commerce.ViewModelAdapterWinJS.getResourceString("string_3384"),
                MessageType.Error,
                MessageBoxButtons.Default,
                Commerce.ViewModelAdapterWinJS.getResourceString("string_3383")
                );
            this.receivedNowInput.select();
        }

        private displayExceedQuantityErrorMessage(): void {
            // show UI for exceeded quantity
            ViewModelAdapterWinJS.displayMessage(
                Commerce.ViewModelAdapterWinJS.getResourceString("string_3870"),
                MessageType.Error,
                MessageBoxButtons.Default,
                Commerce.ViewModelAdapterWinJS.getResourceString("string_3869")
                );
            this.receivedNowInput.select();
        }

        private isQuantityAllowed(quantity: number): boolean {
            var orderType: Model.Entities.PurchaseTransferOrderType = this.detailsViewModel.journalHeader().orderType;

            // if order type is transfer out or picking list, check if quantity exceed total of received / transferred now + received / transferred
            if (orderType === Model.Entities.PurchaseTransferOrderType.PickingList || orderType === Model.Entities.PurchaseTransferOrderType.TransferOut) {
                var productIndex: number = this.getProductIndex();
                var productLine: Model.Entities.PickingAndReceivingOrderLine = this.detailsViewModel.lineDetails()[productIndex];
                var quantityLeft: number = productLine.quantityOrdered - (productLine.quantityReceived + quantity);

                if (quantityLeft < 0) {
                    return false;
                }
            }

            return true;
        }

        /**
         * Determines if the quantity is allowed for the currently selected line and displays resulting errors.
         * @param {number} quantity The new quantity for selected line.
         * @return {boolean} True if there was an error, false if no errors.
         */
        private hasQuantityErrors(quantity: number): boolean {
            if (quantity < 0) {
                // show errors NaN
                this.displayInvalidQuantityErrorMessage();
                return true;
            }

            if (!this.isQuantityAllowed(quantity)) {
                // show message that quantity exceeds allowable quantity to be add.
                this.displayExceedQuantityErrorMessage();
                return true;
            }

            return false;
        }
    }
}