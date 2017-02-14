/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Controls/ExtensionPropertiesControl.ts'/>
///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../ViewControllerBase.ts'/>
///<reference path='SearchView.ts'/>

module Commerce.ViewControllers {
    "use strict";

    /**
     * ProductDetailsViewController constructor options.
     */
    export class ProductDetailOptions extends ViewControllerBase {

        public productAddModeEnum: ViewModels.ProductAddModeEnum;
        public pageCallback: string;
        public recordId: number;
        public product: Model.Entities.Product;
        public quantity: number;
        public additionalParams: any;

        constructor() {
            super(true);
            this.productAddModeEnum = ViewModels.ProductAddModeEnum.AddToCart;
            this.pageCallback = "CartView";
            this.recordId = 0;
            this.product = null;
            this.additionalParams = {};
            this.quantity = 1;
        }
    }

    export class ProductDetailsViewController extends ViewControllerBase {

        public commonHeaderData;

        public ViewModel: Commerce.ViewModels.ProductDetailsViewModel;
        public priceCheckViewModel: ViewModels.PriceCheckViewModel;
        private _productDetailOptions: Observable<ProductDetailOptions>;

        private _extensionPropertiesControl: Controls.ExtensionPropertiesControl;
        private _setDisassembleQuantityDialog: Controls.SetDisassembleQuantityDialog;
        private _isExtensionPropertyAvailable: Observable<boolean>
        private _productSubscription: any;

        private _variantDialog: Controls.VariantDialog;

        public substituteProductsDisplayData: Observable<any>;
        public selectedsubstituteProduct: Observable<any>;

        public indeterminateWaitVisible: Observable<boolean>;
        public changeComponentEnabled: Observable<boolean>;
        public toggleShowHideMenu: Observable<any>;

        public seeAllRelatedProductsLabelVisible: Computed<boolean>;

        private _productNameField: string = "ProductName";
        private _productDescriptionField: string = "Description";
        public localizedProductName: Observable<string>;
        public localizedProductDescription: Observable<string>;
        public productQuantityText: Observable<string>;
        public isDimensionSectionVisible: Computed<boolean>;

        constructor(options: ProductDetailOptions) {
            super(true);
            RetailLogger.viewsMerchandisingProductDetailsLoadStarted();
            options.quantity = options.quantity || 1;
            this._productDetailOptions = ko.observable(options);
            this.productQuantityText = ko.observable(this._productDetailOptions().quantity.toLocaleString(Commerce.Host.instance.globalization.getApplicationLanguage()));

            this.ViewModel = new Commerce.ViewModels.ProductDetailsViewModel();
            this.priceCheckViewModel = new ViewModels.PriceCheckViewModel(this);
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_803"));
            this.commonHeaderData.enableVirtualCatalogHeader();

            this.indeterminateWaitVisible = ko.observable(true);
            this._isExtensionPropertyAvailable = ko.observable(false);
            this.localizedProductName = ko.observable("");
            this.localizedProductDescription = ko.observable("");
            this.changeComponentEnabled = ko.observable(false);
            this.substituteProductsDisplayData = ko.observable([]);
            this.selectedsubstituteProduct = ko.observable(null);
            this.toggleShowHideMenu = ko.observable(() => { });
            this.seeAllRelatedProductsLabelVisible = ko.computed(() => { return this.ViewModel.relatedProducts().length > ViewModels.ProductDetailsViewModel.MaxRelatedProductsForDisplay; }, this);
            //The section is visible if the product is kit or it has dimension values or it is AddToCard mode
            this.isDimensionSectionVisible = ko.computed(() => {
                return this.ViewModel.product() && (this.ViewModel.product().IsKit || 
                    (ArrayExtensions.hasElements(this.ViewModel.dimensionValue()) || this._productDetailOptions().productAddModeEnum === ViewModels.ProductAddModeEnum.AddToCart ));
            });

            this.addControl(this._setDisassembleQuantityDialog = new Controls.SetDisassembleQuantityDialog());
            this.addControl(this._variantDialog = new Controls.VariantDialog());

            if (ObjectExtensions.isNullOrUndefined(this._productDetailOptions().product)) {
                var skipVariantExpansion: boolean = false;

                // If the product is a kit set skipVariantExpansion to true to optimize performance of load time.
                if (!ObjectExtensions.isNullOrUndefined(this._productDetailOptions().additionalParams.IsKit) && this._productDetailOptions().additionalParams.IsKit == true) {
                    skipVariantExpansion = true;
                }

                this.ViewModel.getProductDetailsByDataLevel([this._productDetailOptions().recordId], 4, skipVariantExpansion)
                    .done((products: Commerce.Model.Entities.Product[]) => {
                        this.loadProductInfo(products[0]);
                        RetailLogger.viewsMerchandisingProductDetailsLoaded();
                    }).fail((errors: Model.Entities.Error[]) => {
                        this.indeterminateWaitVisible(false);
                        Commerce.NotificationHandler.displayClientErrors(errors);
                    });
            } else {
                this.loadProductInfo(this._productDetailOptions().product);
            }

            this.ViewModel.selectedVariant.subscribe(this.updateProductPrice, this);

            this.productQuantityText.subscribe((newTextValue: string): void => {
                this._productDetailOptions().quantity = Number(newTextValue);
            });
        }

        public loadProductInfo(product: Model.Entities.Product) {
            this.ViewModel.product(product);
            this.updateImages(this.ViewModel.product().Image, true);
            this.indeterminateWaitVisible(true);

            if (product.IsKit) {
                // Wait for all Kit components to be loaded...
                this.ViewModel.LoadKitComponents(this, this._productDetailOptions().recordId, this.LoadKitComponentsSuccessCallback, (errors: Model.Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);
                    Commerce.NotificationHandler.displayClientErrors(errors);
                });
            }
            else {
                this.ViewModel.dimensionValue(this.ViewModel.getProductDimensions(this, this.ViewModel.product(), this._productDetailOptions().recordId, this.DimensionsChangeHandler));
                this.indeterminateWaitVisible(false);
            }
            this.populateProductProperties();
            this.LoadRelatedProducts();

            this.updateProductPrice();
        }

        private LoadKitComponentsSuccessCallback() {
            for (var i = 0; i < this.ViewModel.kitComponents().length; i++) {
                var dimensionvalue: any = this.ViewModel.ParseProductDimensions(this, this.ViewModel.kitComponents()[i], this.kitComponentDimensionsChangeHandler, i, this._productDetailOptions().recordId);
                var imageUrl: string = "";
                var altText: string = "";
                if (!ObjectExtensions.isNullOrUndefined(this.ViewModel.kitComponents()[i].Image) && ArrayExtensions.hasElements(this.ViewModel.kitComponents()[i].Image.Items)) {
                    imageUrl = this.ViewModel.kitComponents()[i].Image.Items[0].Url;
                    altText = this.ViewModel.kitComponents()[i].Image.Items[0].AltText;
                }

                this.ViewModel.kitDimensions().push(dimensionvalue);

                var kitLineIdentifier: number = this.ViewModel.product().CompositionInformation.KitDefinition.KitLineDefinitions[i].KitLineIdentifier;
                var defaultComponentId: number;
                if (this.ViewModel.product().RecordId == this._productDetailOptions().recordId) {
                    defaultComponentId = this.ViewModel.product().CompositionInformation.KitDefinition.KitLineDefinitions[i].ComponentProductId;
                } else {

                    var kitVariantContent: Model.Entities.KitVariantContent[] = this.ViewModel.product().CompositionInformation.KitDefinition.KitVariantToComponentMap.filter((value) => { return value.KitVariantId == this._productDetailOptions().recordId });
                    if (ArrayExtensions.hasElements(kitVariantContent)) {
                        kitVariantContent[0].KitComponentKeyList.forEach((kitComponentKey: Model.Entities.KitComponentKey) => {
                            if (this.ViewModel.product().CompositionInformation.KitDefinition.KitLineDefinitions[i].KitLineIdentifier == kitComponentKey.KitLineIdentifier) {
                                defaultComponentId = kitComponentKey.DistinctProductId;
                            }
                        });
                    } else {
                        RetailLogger.viewsMerchandisingProductDetailsKitVariantNotFound(this._productDetailOptions().recordId, this.ViewModel.product().RecordId);
                        return [];
                    }
                }
                //var defaultComponentId: number = this.ViewModel.product().CompositionInformation.KitDefinition.KitLineDefinitions[i].ComponentProductId;
                var componentProperty = this.ViewModel.getComponentProperty(kitLineIdentifier, defaultComponentId, this.ViewModel.kitComponents()[i]);
                this.ViewModel.kitComponentsForDisplay.push(
                    {
                        ProductName: Commerce.ProductPropertiesHelper.ProductPropertyFormatter(this.ViewModel.kitComponents()[i], this._productNameField),
                        RecordId: this.ViewModel.kitComponents()[i].RecordId,
                        KitLineIdentifier: kitLineIdentifier,
                        ItemId: this.ViewModel.kitComponents()[i].ItemId,
                        ImageUrl: imageUrl,
                        AltText: altText,
                        ProductSubtitutionApplicable: this.ViewModel.IsSubstitutionApplicable(i),
                        Dimensionvalue: dimensionvalue,
                        ViewComponentDetails: this.viewProductInformation,
                        Quantity: componentProperty.Quantity,
                        Charge: componentProperty.Charge,
                        index: i,
                    });
            }

            this.ViewModel.kitDimensions(this.ViewModel.kitDimensions());
            this.ViewModel.kitComponentsForDisplay(this.ViewModel.kitComponentsForDisplay());
            this.ViewModel.setActiveKitVariant().fail((errors) => { Commerce.NotificationHandler.displayClientErrors(errors); });
            this.indeterminateWaitVisible(false);
        }

        private LoadSubstituteComponentsSuccessCallback(componentNumber: number) {
            var dimensionvalue: any = this.ViewModel.ParseProductDimensions(this, this.ViewModel.kitComponents()[componentNumber], this.kitComponentDimensionsChangeHandler, componentNumber, this._productDetailOptions().recordId, true);
            this.ViewModel.kitDimensions()[componentNumber] = dimensionvalue;
            var imageUrl: string = "";
            var altText: string = "";
            if (!ObjectExtensions.isNullOrUndefined(this.ViewModel.kitComponents()[componentNumber].Image) && ArrayExtensions.hasElements(this.ViewModel.kitComponents()[componentNumber].Image.Items)) {
                imageUrl = this.ViewModel.kitComponents()[componentNumber].Image.Items[0].Url;
                altText = this.ViewModel.kitComponents()[componentNumber].Image.Items[0].AltText;
            }

            var kitLineIdentifier: number = this.ViewModel.product().CompositionInformation.KitDefinition.KitLineDefinitions[componentNumber].KitLineIdentifier;

            var productId = this.ViewModel.kitComponents()[componentNumber].RecordId;
            var componentProperty = this.ViewModel.getComponentProperty(kitLineIdentifier, productId, this.ViewModel.kitComponents()[componentNumber]);

            this.ViewModel.kitComponentsForDisplay()[componentNumber] =
            {
                ProductName: Commerce.ProductPropertiesHelper.ProductPropertyFormatter(this.ViewModel.kitComponents()[componentNumber], this._productNameField),
                RecordId: this.ViewModel.kitComponents()[componentNumber].RecordId,
                KitLineIdentifier: kitLineIdentifier,
                ItemId: this.ViewModel.kitComponents()[componentNumber].ItemId,
                ImageUrl: imageUrl,
                AltText: altText,
                ProductSubtitutionApplicable: this.ViewModel.IsSubstitutionApplicable(componentNumber),
                Dimensionvalue: dimensionvalue,
                ViewComponentDetails: this.viewProductInformation,
                Quantity: componentProperty.Quantity,
                Charge: componentProperty.Charge,
                index: componentNumber,
            };
            this.ViewModel.kitDimensions(this.ViewModel.kitDimensions());
            this.ViewModel.kitComponentsForDisplay(this.ViewModel.kitComponentsForDisplay());
            this.ViewModel.setActiveKitVariant().fail((errors) => { Commerce.NotificationHandler.displayClientErrors(errors); });
            this.indeterminateWaitVisible(false);
        }

        public ChangeProduct(data) {

            this.indeterminateWaitVisible(true);
            var KitLineDefinitions: any = this.ViewModel.product().CompositionInformation.KitDefinition.KitLineDefinitions.filter((value) => {
                return value.KitLineIdentifier == data.KitLineIdentifier;
            });

            var substituteProductIds: number[] = [].concat(KitLineDefinitions[0].SubstituteProductIds);
            substituteProductIds.concat(KitLineDefinitions[0].ComponentProductId);

            this.ViewModel.productManager.getProductDetailsByDataLevelAsync(substituteProductIds, 1, true)
                .done((products: Commerce.Model.Entities.Product[]) => {

                    var existingKitComponent = this.ViewModel.kitComponentsForDisplay().filter((kitComponent) => {
                        return kitComponent.KitLineIdentifier == data.KitLineIdentifier;
                    });
                    var substitutes = products.filter((product) => { return product.RecordId != existingKitComponent[0].RecordId; });
                    this.substituteProductsDisplayData(substitutes.map((line) => { return { Component: line, RecordId: line.RecordId, KitLineIdentifier: data.KitLineIdentifier, ComponentProperty: this.ViewModel.getComponentProperty(data.KitLineIdentifier, line.RecordId, line) } }));
                    this.changeComponentEnabled(true);
                    this.indeterminateWaitVisible(false);
                    this.commonHeaderData.backButtonVisible(false);

                }).fail((errors: Model.Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);
                    Commerce.NotificationHandler.displayClientErrors(errors);
                });
        }

        private addProductToCallbackPage() {
            this._productDetailOptions().additionalParams.Product = this.ViewModel.product();

            if (this.ViewModel.product().IsKit) {
                this.processSelectedKitVariant((kitVariantInfo) => {
                    this._productDetailOptions().additionalParams.VariantId = kitVariantInfo.kitProductVariantId;
                    this._productDetailOptions().additionalParams.Product = kitVariantInfo.kitProduct;
                    this._productDetailOptions().additionalParams.kitComponentProducts = this.ViewModel.kitComponents(),
                    Commerce.ViewModelAdapter.navigate(this._productDetailOptions().pageCallback, this._productDetailOptions().additionalParams);
                }, false);
            } else {

                var variantRecordId: number = this.ViewModel.getVariantId(this.ViewModel.product(), this.ViewModel.dimensionValue());
                this._productDetailOptions().additionalParams.VariantId = variantRecordId;
                Commerce.ViewModelAdapter.navigate(this._productDetailOptions().pageCallback, this._productDetailOptions().additionalParams);
            }

        }

        private processVariantInfo(isQuickSale: boolean = true) {

            if (this.ViewModel.product().IsKit
                //POSHack // David, Stephen
                && this.ViewModel.getVariantId(this.ViewModel.product(), this.ViewModel.dimensionValue()) != 22565421965
                && this.ViewModel.getVariantId(this.ViewModel.product(), this.ViewModel.dimensionValue()) != 22565421966
                && this.ViewModel.getVariantId(this.ViewModel.product(), this.ViewModel.dimensionValue()) != 22565421967
                && this.ViewModel.getVariantId(this.ViewModel.product(), this.ViewModel.dimensionValue()) != 22565421968
                && this.ViewModel.getVariantId(this.ViewModel.product(), this.ViewModel.dimensionValue()) != 22565421971
                && this.ViewModel.getVariantId(this.ViewModel.product(), this.ViewModel.dimensionValue()) != 22565421983
                )
            {
                this.processSelectedKitVariant((kitVariantInfo) => { this.AddToCart(kitVariantInfo.kitProductVariantId, isQuickSale); }, false);
            } else {
                var variantId: number = this.ViewModel.getVariantId(this.ViewModel.product(), this.ViewModel.dimensionValue());

                // If the user did not select any or finished to select the product dimensions, the variant selection triggered by ItemSaleOperation will take care of that.
                if (variantId === -1) {
                    variantId = this.ViewModel.product().RecordId;
                }

                this.AddToCart(variantId, isQuickSale);
            }
        }

        /**
         * Add to cart button handler
         *
         */
        private AddProductToCart() {
            this.processVariantInfo(false);
            RetailLogger.viewsMerchandisingProductDetailsAddItem();
        }

        private QuickSale() {
            this.processVariantInfo(true);
            RetailLogger.viewsMerchandisingProductDetailsQuickSale();
        }


        /**
         * Add to cart button handler
         *
         */
        private AddToCart(variantId: number, isQuickSale: boolean = true) {

            this.indeterminateWaitVisible(true);
            var options: Operations.IItemSaleOperationOptions = {
                productSaleDetails: [{
                    productId: variantId,
                    quantity: this._productDetailOptions().quantity || 0
                }]
            };

            // POShack //David/Stephen
            //if (variantId == 68719482372 && isQuickSale == false) { // Server
            if (variantId === 68719485372 && isQuickSale === false) { //Local
                var options: Operations.IItemSaleOperationOptions = {
                    productSaleDetails: [
                        {

                            productId: 22565421965,
                            quantity: 1
                        }
                        ,
                        {

                            productId: 22565421966,
                            quantity: 1
                        }
                        ,
                        {

                            productId: 22565421967,
                            quantity: 1
                        },
                        {

                            productId: 22565421968,
                            quantity: 1
                        },
                        {

                            productId: 22565421971,
                            quantity: 1
                        },
                        {

                            productId: 22565421983,
                            quantity: 1
                        }
                    ]
                };
            }
            else {
                //
                var options: Operations.IItemSaleOperationOptions = {
                    productSaleDetails: [{
                        productId: variantId,
                        quantity: this._productDetailOptions().quantity || 0
                    }]
                };
            }
            //POShack END

            var operationResult = Operations.OperationsManager.instance.runOperation(
                Operations.RetailOperation.ItemSale, options);

            operationResult
                .done((result) => {
                    this.indeterminateWaitVisible(false);

                    if (!result.canceled && isQuickSale) {
                        ViewModelAdapter.navigate("CartView");
                    }
                }).fail((errors) => {
                    this.indeterminateWaitVisible(false);
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        /**
         *  Called when dimension value for the component has changed 
         * @param selectedDimension selected dimesion
         * @param componentId kit component(product)
         * @param index selected dimension index
         */
        private kitComponentDimensionsChangeHandler(selectedDimension: any, componentId: number, index: number): void {
            var dimensions: Model.Entities.ProductDimensionSet[] = this.ViewModel.kitComponents()[componentId].CompositionInformation.VariantInformation.Dimensions;
            if (!ObjectExtensions.isNullOrUndefined(selectedDimension)) {
                var dimensionValues = [];
                var currentDimension: Model.Entities.ProductDimensionSet = dimensions[index];
                var currentVariantSelection: number[];
                var areAllDimensionsSet: boolean = true; 
                if (dimensions.length !== 1) {
                    // Get the set of applicable variant Ids
                    for (var dimensionValueIndex = 0; dimensionValueIndex < currentDimension.DimensionValues.length; dimensionValueIndex++) {
                        if (currentDimension.DimensionValues[dimensionValueIndex].DimensionValue === selectedDimension.value) {
                            currentVariantSelection = currentDimension.DimensionValues[dimensionValueIndex].VariantSet;
                            break;
                        }
                    }

                    if (index < dimensions.length - 1) {
                        areAllDimensionsSet = index + 1 >= dimensions.length;
                        index++;
                        let nextDimensionBinding: any = this.ViewModel.kitDimensions()[componentId][index];
                        let nextDimension: Model.Entities.ProductDimensionSet = dimensions[index];

                        for (let i = 0; i < nextDimension.DimensionValues.length; i++) {
                            for (let j = 0; j < nextDimension.DimensionValues[i].VariantSet.length; j++) {
                                if (currentVariantSelection.indexOf(nextDimension.DimensionValues[i].VariantSet[j]) !== -1) {

                                    dimensionValues.push({
                                        value: {
                                            dimension: nextDimension.DimensionKey,
                                            value: nextDimension.DimensionValues[i].DimensionValue.toString()
                                        },
                                        text: nextDimension.DimensionValues[i].DimensionValue.toString()
                                    });
                                    break;
                                }
                            }
                        }

                        nextDimensionBinding.IsSelectionEnabled(true);
                        nextDimensionBinding.AvailableOptions(dimensionValues);
                    }
                }

                if (areAllDimensionsSet) {
                    var variantId: number = this.ViewModel.getVariantId(this.ViewModel.kitComponents()[componentId], this.ViewModel.kitComponentsForDisplay()[componentId].Dimensionvalue);
                    if (variantId > -1) {
                        var updatedKitComponentDisplayData = this.refreshKitComponentDisplayData(componentId, variantId);
                        this.ViewModel.kitComponentsForDisplay().splice(updatedKitComponentDisplayData.index, 1, updatedKitComponentDisplayData);
                        this.ViewModel.kitComponentsForDisplay(this.ViewModel.kitComponentsForDisplay());
                        var selectedComponentvariantIds: any = [];
                        selectedComponentvariantIds.push(variantId);
                    }
                    this.ViewModel.setActiveKitVariant().fail((errors) => { NotificationHandler.displayClientErrors(errors); });
                }
            } else if(index + 1 < dimensions.length) {
                // Clear the dependent variants.
                this.ViewModel.kitDimensions()[componentId][index + 1].SelectedOption(null);
                this.ViewModel.kitDimensions()[componentId][index + 1].IsSelectionEnabled(false);
            }
        }

        private refreshKitComponentDisplayData(componentIndex: number, ProductId: number): any {
            var selectedKitLineIdentifier = this.ViewModel.kitComponentsForDisplay()[componentIndex].KitLineIdentifier;
            var previousKitComponentDisplayData = this.ViewModel.kitComponentsForDisplay().filter((data) => data.KitLineIdentifier == selectedKitLineIdentifier)[0];
            var componentProperty = this.ViewModel.getComponentProperty(previousKitComponentDisplayData.KitLineIdentifier, ProductId, this.ViewModel.kitComponents()[componentIndex]);
            return {
                ProductName: previousKitComponentDisplayData.ProductName,
                RecordId: previousKitComponentDisplayData.RecordId,
                KitLineIdentifier: previousKitComponentDisplayData.KitLineIdentifier,
                ItemId: previousKitComponentDisplayData.ItemId,
                ImageUrl: previousKitComponentDisplayData.ImageUrl,
                AltText: previousKitComponentDisplayData.AltText,
                ProductSubtitutionApplicable: previousKitComponentDisplayData.ProductSubtitutionApplicable,
                Dimensionvalue: previousKitComponentDisplayData.Dimensionvalue,
                OnProductChange: previousKitComponentDisplayData.OnProductChange,
                ViewComponentDetails: previousKitComponentDisplayData.ViewComponentDetails,
                Quantity: componentProperty.Quantity,
                Charge: componentProperty.Charge,
                index: previousKitComponentDisplayData.index
            }
        }

        private processSelectedKitVariant(processCallBack: (kitVariantInfo: any) => void, forceComponentVariantSelection: boolean = true) {
            // Check if all dimensions are selected for each of the components
            var selectedComponentvariantIds: number[] = [];
            var kitVariantInfo: any;
            for (var i = 0; i < this.ViewModel.kitComponents().length; i++) {
                var variantId: number = this.ViewModel.getVariantId(this.ViewModel.kitComponents()[i], this.ViewModel.kitComponentsForDisplay()[i].Dimensionvalue);
                if (variantId > -1) {
                    selectedComponentvariantIds.push(variantId);
                } else {
                    break;
                }
            }

            if (selectedComponentvariantIds.length == this.ViewModel.product().CompositionInformation.KitDefinition.KitLineDefinitions.length) {
                var kitVariantIds: number[] = ProductPropertiesHelper.getKitVariant(this.ViewModel.product(), selectedComponentvariantIds);
                if (ArrayExtensions.hasElements(kitVariantIds)) {
                    kitVariantInfo = {
                        kitProductVariantId: kitVariantIds[0],
                        kitProduct: this.ViewModel.product(),
                        kitComponentProducts: this.ViewModel.kitComponents(),
                    };
                    processCallBack(kitVariantInfo);
                }
            } else {
                if (!Commerce.ObjectExtensions.isNullOrUndefined(forceComponentVariantSelection) && forceComponentVariantSelection) {
                    this._variantDialog.show({ products: this.ViewModel.kitComponents() })
                        .on(DialogResult.OK, (variantIds) => {
                            var kitVariantIds = ProductPropertiesHelper.getKitVariant(this.ViewModel.product(), variantIds);
                            if (ArrayExtensions.hasElements(kitVariantIds)) {
                                var variantId: number = kitVariantIds[0];
                                var product: Model.Entities.Product = this.ViewModel.product();
                                var inventDimId: string = ProductPropertiesHelper.getProperty(
                                    variantId, product, ProductPropertyNameEnum.InventoryDimensionId);

                                if (StringExtensions.isNullOrWhitespace(inventDimId)) {

                                    // Product Variant not found from the master product.
                                    // Get product details with variant id so we can have
                                    // product instance with the designated variant id.
                                    this.ViewModel.getProductDetailsByDataLevel(kitVariantIds, 4, true)
                                        .done((products: Model.Entities.Product[]) => {
                                            kitVariantInfo = {
                                                kitProductVariantId: kitVariantIds[0],
                                                kitProduct: products[0],
                                                kitComponentProducts: this.ViewModel.kitComponents()
                                            };
                                            processCallBack(kitVariantInfo);
                                        })
                                        .fail((errors: Model.Entities.Error[]) => {
                                            NotificationHandler.displayClientErrors(errors);
                                        });
                                } else {
                                    kitVariantInfo = {
                                        kitProductVariantId: kitVariantIds[0],
                                        kitProduct: this.ViewModel.product(),
                                        kitComponentProducts: this.ViewModel.kitComponents()
                                    };
                                    processCallBack(kitVariantInfo);
                                }
                            }
                        }).on(DialogResult.Cancel, (result) => {
                            // Nothing
                        }).onError((errors) => {
                            Commerce.NotificationHandler.displayClientErrors(errors);
                        });
                } else {
                    NotificationHandler.displayClientErrors([new Model.Entities.Error("string_829")], "string_828"); // Not all dimension variants selected.
                }
            }
        }

        private populateProductProperties() {
            var product: Model.Entities.Product = this.ViewModel.product();

            if (!ObjectExtensions.isNullOrUndefined(product.ProductProperties) && ArrayExtensions.hasElements(product.ProductProperties)) {
                var localizedProductPropertyList: Model.Entities.ProductPropertyTranslation[] = [];
                localizedProductPropertyList = product.ProductProperties.filter((value: Model.Entities.ProductPropertyTranslation) => {
                    return value.TranslationLanguage == Commerce.ApplicationContext.Instance.deviceConfiguration.CultureName;
                });

                if (!ArrayExtensions.hasElements(localizedProductPropertyList)) {
                    localizedProductPropertyList.push(product.ProductProperties[0]);
                }

                this.ViewModel.productProperty(localizedProductPropertyList[0].TranslatedProperties.filter((productProperty: Model.Entities.ProductProperty) => {
                    if (!StringExtensions.compare(productProperty.KeyName, this._productNameField, true)) {
                        this.localizedProductName(productProperty.ValueString);
                        this.commonHeaderData.categoryName(productProperty.ValueString);
                    } else if (!StringExtensions.compare(productProperty.KeyName, this._productDescriptionField, true)) {
                        this.localizedProductDescription(productProperty.ValueString);
                        return false;
                    }
                    return productProperty.PropertyTypeValue < Commerce.ViewModels.ProductDetailsViewModel.ProductVideoPropertyTypeValue;
                }));
            }
        }

        private substituteSelectionChangedEventHandler(items: any[]): void {
            this.selectedsubstituteProduct(null);
            if (ArrayExtensions.hasElements(items)) {
                this.selectedsubstituteProduct(items[0]);
            }
        }

        private substituteSelectionHandler() {

            this.ViewModel.SubstituteKitComponent(this.selectedsubstituteProduct().RecordId, this.selectedsubstituteProduct().KitLineIdentifier, this, this.LoadSubstituteComponentsSuccessCallback, (errors: Model.Entities.Error[]) => {
                this.indeterminateWaitVisible(false);
                this.commonHeaderData.backButtonVisible(true);
                Commerce.NotificationHandler.displayClientErrors(errors);
            });
            this.changeComponentEnabled(false);
            this.commonHeaderData.backButtonVisible(true);
        }

        private substituteCancelHandler() {
            this.changeComponentEnabled(false);
            this.commonHeaderData.backButtonVisible(true);
            this.substituteProductsDisplayData([]);
        }

        private viewProductInformation(data: any) {
            var productDetailOptions: ProductDetailOptions = new ProductDetailOptions();
            productDetailOptions.recordId = data.RecordId;
            Commerce.ViewModelAdapter.navigate("ProductDetailsView", productDetailOptions);
        }

        private displayAllRelatedProducts() {
            var relatedProducts = this.ViewModel.relatedProducts();
            Commerce.ViewModelAdapter.navigate("SearchView", { relatedItems: relatedProducts, searchEntity: "Products" });
        }

        private navigateToRichMediaView() {
            Commerce.ViewModelAdapter.navigate("ProductRichMediaView", this.ViewModel.images());
        }

        /**
         * Update Images in UI
         *
         * @param {any} Rich Media Locations which contains a list of Images
         * @param {bool} clearAndUpdate - Boolean when set to true clear the image collection and then update the collection
         */
        private updateImages(images: any, clearAndUpdate: boolean) {
            if (clearAndUpdate) {
                if (!ObjectExtensions.isNullOrUndefined(images) && ArrayExtensions.hasElements(images.Items)) {

                    if (Commerce.Session.instance.connectionStatus == Commerce.ConnectionStatusType.Online) {
                        this.ViewModel.images(Commerce.ObjectExtensions.clone(images.Items));
                    }
                    else {
                        this.ViewModel.images([{ Url: this.ViewModel.product().OfflineImage }]);
                    }
                } else if (ArrayExtensions.hasElements(images) &&
                    ArrayExtensions.hasElements(images[0].Items)) {
                    this.ViewModel.images(images[0].Items);
                } else {
                    this.ViewModel.images([]);
                }
            } else {
                if (ArrayExtensions.hasElements(images) &&
                    ArrayExtensions.hasElements(images[0].Items)) {
                    images[0].Items.forEach((value) => {
                        this.ViewModel.images.push(value);
                    });
                }
            }

        }

        private LoadRelatedProducts() {
            this.ViewModel.LoadRelatedProducts().fail((errors: Model.Entities.Error[]) => {
                Commerce.NotificationHandler.displayClientErrors(errors);
            });
        }

        /**
         * Handler for tracking dimension value changes.
         *
         * @param {any} selectedDimension - Selected dimension value
         */
        public DimensionsChangeHandler(selectedDimension: any) {
            if (!ObjectExtensions.isNullOrUndefined(selectedDimension)) {
                var Dimensions: Commerce.Model.Entities.ProductDimensionSet[] = this.ViewModel.product().CompositionInformation.VariantInformation.Dimensions;
                var dimensionValues = [];
                var CurrentVariantSelection: number[];

                // Get the set of applicable variant Ids
                for (var dimensionCount = 0; dimensionCount < Dimensions.length; dimensionCount++) {
                    if (Dimensions[dimensionCount].DimensionKey == selectedDimension.dimension) {
                        for (var dimensionValueIndex = 0; dimensionValueIndex < Dimensions[dimensionCount].DimensionValues.length; dimensionValueIndex++) {
                            if (Dimensions[dimensionCount].DimensionValues[dimensionValueIndex].DimensionValue == selectedDimension.value) {
                                CurrentVariantSelection = Dimensions[dimensionCount].DimensionValues[dimensionValueIndex].VariantSet;
                                break;
                            }
                        }
                        break;
                    }
                }

                var images: any;
                if (dimensionCount >= Dimensions.length - 1) {

                    /// change the images here
                    var variantId = this.ViewModel.getVariantId(this.ViewModel.product(), this.ViewModel.dimensionValue());
                    if (variantId != -1) {
                        var selectedProductVariant: Model.Entities.ProductVariant[] = this.ViewModel.product().CompositionInformation.VariantInformation.Variants.filter((value: Model.Entities.ProductVariant) => {
                            return value.DistinctProductVariantId == variantId;
                        });

                        if (!ObjectExtensions.isNullOrUndefined(selectedProductVariant[0].PropertiesAsList) && ArrayExtensions.hasElements(selectedProductVariant[0].PropertiesAsList)
                            && ArrayExtensions.hasElements(selectedProductVariant[0].PropertiesAsList[0].TranslatedProperties)) {

                            var productProperties: Model.Entities.ProductProperty[] = [];

                            if (!ObjectExtensions.isNullOrUndefined(this.ViewModel.product().ProductProperties) && ArrayExtensions.hasElements(this.ViewModel.product().ProductProperties)
                                && ArrayExtensions.hasElements(this.ViewModel.product().ProductProperties[0].TranslatedProperties)) {
                                productProperties = ObjectExtensions.clone(this.ViewModel.product().ProductProperties[0].TranslatedProperties.filter((value) => {
                                    return value.PropertyTypeValue < Commerce.ViewModels.ProductDetailsViewModel.ProductVideoPropertyTypeValue;
                                }));
                            }

                            this.ViewModel.selectedVariant(selectedProductVariant[0]);

                            selectedProductVariant[0].PropertiesAsList[0].TranslatedProperties.forEach((productProperty: Model.Entities.ProductProperty) => {
                                if (!StringExtensions.isNullOrWhitespace(productProperty.ValueString) && productProperty.PropertyTypeValue < Commerce.ViewModels.ProductDetailsViewModel.ProductVideoPropertyTypeValue) {
                                    //productProperties.
                                    var result: Model.Entities.ProductProperty[] = $.grep(productProperties, (value: Model.Entities.ProductProperty) => {
                                        return value.RecordId == productProperty.RecordId && value.PropertyTypeValue < Commerce.ViewModels.ProductDetailsViewModel.ProductVideoPropertyTypeValue;
                                    }, false);
                                    if (ArrayExtensions.hasElements(result)) {
                                        result[0].ValueString = productProperty.ValueString;
                                    } else {
                                        productProperties.push(productProperty);
                                    }
                                }
                            });

                            this.ViewModel.productProperty(productProperties);
                        }

                        images = selectedProductVariant[0].Images;
                    }
                    return;
                } else {
                    images = this.ViewModel.product().Image;
                    dimensionCount++;
                }

                for (var i = 0; i < Dimensions[dimensionCount].DimensionValues.length; i++) {
                    for (var j = 0; j < Dimensions[dimensionCount].DimensionValues[i].VariantSet.length; j++) {
                        if (CurrentVariantSelection.indexOf(Dimensions[dimensionCount].DimensionValues[i].VariantSet[j]) != -1) {

                            dimensionValues.push({
                                value: {
                                    dimension: Dimensions[dimensionCount].DimensionKey,
                                    value: Dimensions[dimensionCount].DimensionValues[i].DimensionValue.toString(),
                                },
                                text: Dimensions[dimensionCount].DimensionValues[i].DimensionValue.toString(),
                            });
                            break;
                        }
                    }
                }

                var requiredUpdateImage: boolean = true;
                this.ViewModel.dimensionValue()[dimensionCount].IsSelectionEnabled(true);
                this.ViewModel.dimensionValue()[dimensionCount].AvailableOptions(dimensionValues);

                for (dimensionCount++; dimensionCount < Dimensions.length; dimensionCount++) {
                    // another dimenstion will update image
                    if (this.ViewModel.dimensionValue()[dimensionCount].SelectedOption()) {
                        requiredUpdateImage = false;
                    }
                    this.ViewModel.dimensionValue()[dimensionCount].SelectedOption(undefined);
                    this.ViewModel.dimensionValue()[dimensionCount].IsSelectionEnabled(false);
                }
                if (requiredUpdateImage) {
                    this.updateImages(images, true);
                }
            } else {
                this.updateImages(this.ViewModel.product().Image, true);

                if (!ObjectExtensions.isNullOrUndefined(this.ViewModel.product().ProductProperties) && ArrayExtensions.hasElements(this.ViewModel.product().ProductProperties)
                    && ArrayExtensions.hasElements(this.ViewModel.product().ProductProperties[0].TranslatedProperties)) {
                    this.ViewModel.productProperty(this.ViewModel.product().ProductProperties[0].TranslatedProperties.filter((value) => {
                        return value.PropertyTypeValue < Commerce.ViewModels.ProductDetailsViewModel.ProductVideoPropertyTypeValue;
                    }));
                }
            }
        }


        /**
         * Called when the page is loaded on the DOM.
         */
        public load() {
            this._productSubscription = this.ViewModel.product.subscribe((newValue) => {
                this._extensionPropertiesControl.data(newValue);
            }, this);
        }

        /**
         * Called when the page is unloaded from the DOM.
         */
        public unload() {
            this._productSubscription.dispose();
        }

        /**
         * Update the product price.
         * This should be called when variant of a product is changed (for master product),
         * product substitution is selected (for kit products),
         * or when page is loaded for the first time.
         */
        private updateProductPrice(): void {

            var product: Model.Entities.Product = this.ViewModel.product();
            var variantId: number;

            if (ObjectExtensions.isNullOrUndefined(product)) {
                // ProductDetailsViewModel does not yet have the product,
                // no need to proceed further.
                return;
            }

            var selectedVariant: Model.Entities.ProductVariant = this.ViewModel.selectedVariant();

            if (product.IsMasterProduct) {
                if (ObjectExtensions.isNullOrUndefined(selectedVariant)) {
                    // Variant has not been selected for master product / kits.
                    // Just set the initial price of the product.
                    this.priceCheckViewModel.product = product;
                    this.indeterminateWaitVisible(true);
                    this.priceCheckViewModel.getActivePrice().always(() => {
                        this.indeterminateWaitVisible(false);
                    });
                    return;
                }

                variantId = selectedVariant.DistinctProductVariantId;
            }

            if (!product.IsMasterProduct || variantId > 0) {
                // All the required data has been captured
                // i.e. variant information is available for Kit / Master products
                // or product is NonVariant.
                // Get the latest product price from server.

                this.priceCheckViewModel.product = product;
                this.priceCheckViewModel.variantId(variantId);
                this.priceCheckViewModel.store(Session.instance.productCatalogStore.Store);

                this.indeterminateWaitVisible(true);
                this.priceCheckViewModel.getActivePrice().always(() => {
                    this.indeterminateWaitVisible(false);
                });
            } else {
                // Otherwise, set the initial price of the product.
                this.indeterminateWaitVisible(true);
                this.priceCheckViewModel.getActivePrice().always(() => {
                    this.indeterminateWaitVisible(false);
                });
            }
        }

        /*
        *   Switches view between templates
        *
        *   @param {any} eventInfo The event information.
        */
        private showSubstitutesViewMenu(eventInfo: any) {
            this.toggleShowHideMenu()();
        }
    }
}
