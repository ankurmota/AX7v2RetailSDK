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

    /**
     * List of async methods that are called during PriceCheckViewModel initialization.
     */
    export enum PriceCheckContextEntitySet {
        None = 0,
        Product = 1,
        Customer = (1 << 1),
        Store = (1 << 2),
        All = Math.round((Store << 1) - 1)
    }

    export class PriceCheckViewModel extends ViewModelBase {
        private _priceCheckContextEntitySetCompleted: PriceCheckContextEntitySet;
        private _priceCheckContextEntitySetFailed: PriceCheckContextEntitySet;
        private _priceCheckContextEntityErrors: Dictionary<Model.Entities.Error[]>; // Information on failed loadPriceCheck methods.
        private _priceCheckAsyncResult: VoidAsyncResult; // The result object to report the results for the load price check method

        public variantId: Observable<number>;
        public customer: Observable<Model.Entities.Customer>;
        public customerAddress: Observable<Model.Entities.Address>;
        public product: Model.Entities.Product;
        public productPrice: Observable<number>;
        public store: Observable<Model.Entities.OrgUnit>;
        public storeAddress: Observable<Model.Entities.Address>;
        public storeNumber: string;
        public productId: Computed<string>;
        public productName: Computed<string>;
        public productImage: Computed<string>;
        public variantDimensionValues: Computed<string>;
        public unitOfMeasure: string;
        public barcodeId: string;
        public quantity: number;

        constructor(options?: any) {
            super();
            this._priceCheckContextEntitySetCompleted = null;
            this._priceCheckContextEntitySetFailed = null;
            this._priceCheckContextEntityErrors = new Dictionary<Model.Entities.Error[]>();
            
            this.unitOfMeasure = "";
            this.barcodeId = "";
            this.quantity = 0;

            if (ObjectExtensions.isNullOrUndefined(options)) {
                options = {};
            }

            if (ObjectExtensions.isNullOrUndefined(options.Customer)) {
                options.Customer = {
                    AccountNumber: "",
                    Name: "",
                    Email: ""
                };
            }

            if (ObjectExtensions.isNullOrUndefined(options.Product)) {
                options.Product = {
                    RecordId: 0,
                    BasePrice: 0,
                    ProductName: ""
                };
            }

            if (options.Product.RecordId > 0 && !options.Product.IsMasterProduct) {
                options.VariantId = options.Product.RecordId;
            }
            else if (ObjectExtensions.isNullOrUndefined(options.VariantId)) {
                options.VariantId = 0;
            }

            if (!StringExtensions.isNullOrWhitespace(options.storeId)) {
                this.storeNumber = options.storeId;
            } else {
                var storeCatalog: Model.Entities.OrgUnit = Session.instance.productCatalogStore.Store;
                this.storeNumber = storeCatalog.OrgUnitNumber;
            }

            this.customer = ko.observable(options.Customer);
            this.customerAddress = ko.observable({});
            this.product = options.Product;
            this.store = ko.observable({
                RecordId: 0,
                OrgUnitName: "",
                OrgUnitNumber: ""
            });
            this.storeAddress = ko.observable({});

            this.variantId = ko.observable(options.VariantId);
            this.productId = ko.computed(this.getProductId, this);
            this.productName = ko.computed(this.getProductName, this);
            this.productImage = ko.computed(this.getProductImageUrl, this);
            this.variantDimensionValues = ko.computed(this.getDimensionValues, this);
            this.productPrice = ko.observable(null);

        }

        private getProductName(): string {
            return ProductPropertiesHelper.getProperty(this.variantId(), this.product, ProductPropertyNameEnum.ProductName);
        }

        private getProductId(): string {
            return ProductPropertiesHelper.getProperty(this.variantId(), this.product, ProductPropertyNameEnum.ProductNumber);
        }

        private getDimensionValues(): string {
            var dimensionValuesString: string = StringExtensions.EMPTY;
            var currentVariantId: number = this.variantId();

            if (this.product.IsKit) {
                var currentVariant: Proxy.Entities.ProductVariant = ProductPropertiesHelper.getVariant(currentVariantId, this.product);

                //Kit only has configuration identifier for variant distinction
                if (!ObjectExtensions.isNullOrUndefined(currentVariant)) {
                    dimensionValuesString = currentVariant.ConfigId;
                }

                return dimensionValuesString;
            }

            var dimensions: Model.Entities.ProductDimensionResult[] = ProductPropertiesHelper.getProperty(currentVariantId, this.product, ProductPropertyNameEnum.ProductDimensionValues);

            if (!ArrayExtensions.hasElements(dimensions)) {
                return dimensionValuesString;
            }

            for (var i = 0; i < dimensions.length; i++) {
                if (dimensionValuesString.length > 0) {
                    // Add a localized delimiter for variant separator (i.e. ", " in en-US language).
                    dimensionValuesString += ViewModelAdapter.getResourceString("string_2408");
                }

                dimensionValuesString += dimensions[i].dimensionValueTranslation;
            }

            return dimensionValuesString;
        }

        private getProductImageUrl(): string {
            var variantId: number = this.variantId();
            if (variantId > 0 && !ObjectExtensions.isNullOrUndefined(this.product.Image) && ArrayExtensions.hasElements(this.product.Image.Items) &&
                !StringExtensions.isNullOrWhitespace(this.product.Image.Items[0].Url)) {
                return this.product.Image.Items[0].Url;
            }

            else return StringExtensions.EMPTY;
        }

        /**
         * Actions to take if all the methods have completed. Will not do any actions if there are methods still running.
         *
         * @param {PriceCheckContextEntitySet} entity The name of the entity that the action has failed on.
         */
        private actionOnContextEntitySetCompletion(entity: PriceCheckContextEntitySet) {
            if (entity == PriceCheckContextEntitySet.None) {
                RetailLogger.viewModelPriceCheckContextEntitySetNone();
            } else if ((this._priceCheckContextEntitySetCompleted & entity) == entity) {
                RetailLogger.viewModelPriceCheckContextEntitySetMultipleTimes(PriceCheckContextEntitySet[entity]);
            }

            this._priceCheckContextEntitySetCompleted = this._priceCheckContextEntitySetCompleted | entity;

            if (this._priceCheckContextEntitySetCompleted === PriceCheckContextEntitySet.All) {
                if (this._priceCheckContextEntitySetFailed !== PriceCheckContextEntitySet.None) {
                    this._priceCheckAsyncResult.reject(<Commerce.Model.Entities.Error[]>[new Model.Entities.Error(ErrorTypeEnum.PRICE_CHECK_INITIALIZATION_DATA_FAILED_TO_LOAD)]);
                } else {
                    this._priceCheckAsyncResult.resolve();
                }
            } else if (this._priceCheckContextEntitySetCompleted > PriceCheckContextEntitySet.All) {
                RetailLogger.viewModelPriceCheckContextEntitySetNoMethod();
            }
        }

        /**
         * Tracks the tasks that succeeds and does any callbacks if all tasks have completed
         *
         * @param {PriceCheckContextEntitySet} entity The name of the entity that the action has failed on.
         */
        private setCallSuccessful(entity: PriceCheckContextEntitySet) {
            this.actionOnContextEntitySetCompletion(entity);
        }

        /**
         * Tracks the tasks that failed and does any callbacks if all tasks have completed
         *
         * @param {PriceCheckContextEntitySet} entity The name of the entity that the action has failed on.
         * @param {Model.Entities.Error[]} [errors] The error.
         */
        private setCallFailed(entity: PriceCheckContextEntitySet, errors?: Model.Entities.Error[]) {
            this._priceCheckContextEntitySetFailed = this._priceCheckContextEntitySetFailed | entity;
            this._priceCheckContextEntityErrors.setItem(entity, errors);
            this.actionOnContextEntitySetCompletion(entity);
        } 

        /**
         * Get the current customer details on cart.
         *
         * @returns {IVoidAsyncResult} The async result.
         */
        public getCustomer(): IVoidAsyncResult {
            var customerId: string = Session.instance.cart.CustomerId;
            if (StringExtensions.isNullOrWhitespace(customerId)) {
                return VoidAsyncResult.createResolved();
            }

            return this.customerManager.getCustomerDetailsAsync(customerId)
                .done((customerDetails) => {
                    this.customer(customerDetails.customer);
                    this.customerAddress(customerDetails.primaryAddress);
                });
        }

        /**
         * Loads the view model.
         *
         * @returns {IVoidAsyncResult} The async result.
         */
        public load(): IVoidAsyncResult {
            this._priceCheckContextEntitySetCompleted = 0;
            this._priceCheckContextEntitySetFailed = 0;
            this._priceCheckContextEntityErrors.clear();
            this._priceCheckAsyncResult = new VoidAsyncResult();

            if (this.product.RecordId > 0 && this.variantId() > 0) {
                this.getProductPrice()
                    .done(() => {
                        this.setCallSuccessful(PriceCheckContextEntitySet.Product);
                    })
                    .fail((errors: Model.Entities.Error[]) => {
                        RetailLogger.viewModelPriceCheckGetProductPriceFailed(errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
                        this.setCallFailed(PriceCheckContextEntitySet.Product, errors);
                    });
            } else {
                this.setCallSuccessful(PriceCheckContextEntitySet.Product);
            }

            this.getCustomer()
                .done(() => {
                    this.setCallSuccessful(PriceCheckContextEntitySet.Customer);
                })
                .fail((errors: Model.Entities.Error[]) => {
                    RetailLogger.viewModelPriceCheckGetCustomerFailed(errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
                    this.setCallFailed(PriceCheckContextEntitySet.Customer, errors);
                });

            if (!Session.instance.isStoreCatalogVirtual) {

                this.channelManager.getStoreDetailsAsync(this.storeNumber)
                    .done((storeFound: Model.Entities.OrgUnit) => {

                        if (ObjectExtensions.isNullOrUndefined(storeFound.OrgUnitAddress)) {
                            storeFound.OrgUnitAddress = {};
                        }

                        this.store(storeFound);

                        this.storeAddress(storeFound.OrgUnitAddress);
                        this.setCallSuccessful(PriceCheckContextEntitySet.Store);
                    })
                    .fail((errors: Model.Entities.Error[]) => {
                        RetailLogger.viewModelPriceCheckGetStoreDetailsFailed(errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
                        this.setCallFailed(PriceCheckContextEntitySet.Store, errors);
                    });
            } else {
                this.store(Session.instance.productCatalogStore.Store);
                this.storeAddress({});
                this.setCallSuccessful(PriceCheckContextEntitySet.Store);
            }

            return this._priceCheckAsyncResult;
        }

        /**
         * Gets the product price.
         */
        public getProductPrice(): IVoidAsyncResult {
            var options: Operations.IPriceCheckOperationOptions = { priceCheckViewModel: this };
            return this.operationsManager.runOperation(
                Commerce.Operations.RetailOperation.PriceCheck, options);
        }

        /**
         * Get product active prices.
         * @return {IAsyncResult<Entities.ProductPrice[]>} The async result.
         */
        public getActivePrice(): IAsyncResult<Model.Entities.ProductPrice[]> {
            var productId: number = this.variantId() > 0 ? this.variantId() : this.product.RecordId;

            return ProductPropertiesHelper.getActivePricesUsingCart([productId])
                .done((activePrices: Model.Entities.ProductPrice[]) => {
                    if (ArrayExtensions.hasElements(activePrices)) {
                        this.productPrice(activePrices[0].AdjustedPrice);
                    }
                });
        }

        /**
         * Search a product by product identifier, product name, or barcode.
         * @param {string} The keyword to be searched.
         * @param {number} dataLevel The DataLevel for Product Search.
         * @return {IAsyncResult<Model.Entities.Product[]>} The async result that contains products matching the search keyword.
         */
        public searchProductsByKeyword(keyword: string, dataLevel: number, skipVariantExpansion: boolean = false): IAsyncResult<Model.Entities.Product[]> {
            return this.productManager.searchBarcodeOrProductsAsync(keyword, dataLevel, skipVariantExpansion);
        }
    }
}
