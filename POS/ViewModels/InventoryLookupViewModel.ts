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

    export class InventoryLookupViewModel extends ViewModelBase {
        public resultCount: Computed<number>;
        public stores: ObservableArray<Model.Entities.OrgUnitAvailability>;
        public currentStore: Observable<Model.Entities.OrgUnitAvailability>;
        public product: Observable<Model.Entities.Product>;
        public variantId: Observable<number>;
        public variantDimensionValues: Computed<string>;
        public productSearchResults: Model.Entities.Product[];
        public productImageRelativeUrl: Computed<string>;
        public productImageAltText: Observable<string>;

        constructor(options?: any) {
            super();
            options = options || {};
            this.product = ko.observable(options.Product || new Commerce.Model.Entities.ProductClass());
            this.productImageRelativeUrl = ko.computed(this.getProductImageUrl, this);
            this.productImageAltText = ko.observable("");

            this.stores = ko.observableArray(options.Stores || []);
            this.currentStore = ko.observable({ ItemAvailabilities: [] });
            this.resultCount = ko.computed(this.CountStores, this);
            this.variantId = ko.observable(options.VariantId || 0);
            this.variantDimensionValues = ko.computed(this.CountDimensionValues, this);
        }

        private CountStores(): number {
            return this.stores().length;
        }

        private CountDimensionValues(): string {
            var dimensionValuesString: string = StringExtensions.EMPTY;
            var currentProduct: Proxy.Entities.Product = this.product();
            var currentVariantId: number = this.variantId();

            if (currentProduct.IsKit) {
                var currentVariant: Proxy.Entities.ProductVariant = ProductPropertiesHelper.getVariant(currentVariantId, currentProduct);

                //Kit only has configuration identifier for variant distinction
                if (!ObjectExtensions.isNullOrUndefined(currentVariant)) {
                    dimensionValuesString = currentVariant.ConfigId;
                }

                return dimensionValuesString;
            }

            var dimensionValues: Model.Entities.ProductDimensionResult[] =
                <Model.Entities.ProductDimensionResult[]>(ProductPropertiesHelper.getProperty(currentVariantId, currentProduct, ProductPropertyNameEnum.ProductDimensionValues));

            if (!ArrayExtensions.hasElements(dimensionValues)) {
                return dimensionValuesString;
            }

            for (var i = 0; i < dimensionValues.length; i++) {
                if (dimensionValuesString.length > 0) {
                    // Add a localized delimiter for variant separator (i.e. ", " in en-US language).
                    dimensionValuesString += ViewModelAdapter.getResourceString("string_2408");
                }

                dimensionValuesString += dimensionValues[i].dimensionValueTranslation;
            }

            return dimensionValuesString;
        }

        private getProductImageUrl(): string {
            var product: Model.Entities.Product = this.product();
            if (!ObjectExtensions.isNullOrUndefined(product.Image) && ArrayExtensions.hasElements(product.Image.Items) &&
                !StringExtensions.isNullOrWhitespace(product.Image.Items[0].Url)) {
                return product.Image.Items[0].Url;
            }

            else return StringExtensions.EMPTY;
        }

        /**
         * Get the list of inventories available by product.
         *
         * @param {number?} variantId The variant id of the product.
         * @returns {IAsyncResult<Model.Entities.Inventory[]>} The async callback.
         */
        public getInventoriesByProduct(variantId?: number): IVoidAsyncResult {
            if (variantId != null) {
                this.variantId(variantId);
            }

            var options: Operations.IInventoryLookupOperationOptions = {
                inventoryLookupViewModel: this, variantId: variantId
            };

            return this.operationsManager.runOperation(Commerce.Operations.RetailOperation.InventoryLookup, options)
                .done(() => {
                    this.stores().forEach((store: Model.Entities.OrgUnitAvailability) => {
                        if (store.OrgUnitLocation.OrgUnitNumber === ApplicationContext.Instance.storeNumber) {
                            this.currentStore(store);
                        }
                    });
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

        /**
         * Pick up the product in the selected store.
         * @param {Proxy.Entities.OrgUnitLocation} storeLocation The store to be picked up.
         * @returns {AsyncQueue} The async queue.
         */
        public pickUpInStore(storeLocation: Model.Entities.OrgUnitLocation): IAsyncResult<ICancelableResult> {
            var asyncQueue: AsyncQueue = new AsyncQueue();
            var cart: Model.Entities.Cart = Session.instance.cart;
            var requestedPickupDate: Date;
            var quotationExpirationDate: Date;
            var purposeOperationId: Proxy.Entities.CustomerOrderMode;

            // Add more async queue operations when cart type is not customer order or quotation.
            // When the cart being picked is not a customer order, automatically
            // ask the user to make it a customer order / quotation.
            if (!CustomerOrderHelper.isCustomerOrderOrQuoteCreationOrEdition(cart)) {

                asyncQueue.enqueue(() => {
                    // A dialog that asks user to convert cart to customer order or quote.
                    var activity: Activities.GetOrderTypeActivity = new Activities.GetOrderTypeActivity({ operationId: Operations.RetailOperation.PickupSelectedProducts });
                    return activity.execute().done(() => {
                        if (!activity.response) {
                            asyncQueue.cancel();
                            return;
                        }

                        purposeOperationId = activity.response.customerOrderMode;
                    });
                }).enqueue(() => {
                    // When user choose to convert cart to quotation,
                    // run async queue for Quotation expiration dialog.
                    if (purposeOperationId === Proxy.Entities.CustomerOrderMode.QuoteCreateOrEdit) {

                        return CustomerOrderHelper.getQuotationExpirationDate()
                            .done((expirationDate: Date) => {
                                if (ObjectExtensions.isNullOrUndefined(expirationDate)) {
                                    asyncQueue.cancel();
                                    return;
                                }

                                quotationExpirationDate = expirationDate;
                            });
                    } else {
                        return VoidAsyncResult.createResolved();
                    }
                });
            }

            asyncQueue.enqueue(() => {

                // Get the pick up date
                var shippingDateActivityParameters: Activities.GetShippingDateActivityContext = {
                    originalShippingDate: DeliveryHelper.getDeliveryDate(Session.instance.cart, []),
                    deliveryMethodDescription: StringExtensions.EMPTY
                };
                var getShippingDateActivity: Activities.GetShippingDateActivity = new Activities.GetShippingDateActivity(
                    shippingDateActivityParameters);

                return getShippingDateActivity.execute().done(() => {
                    if (!getShippingDateActivity.response) {
                        // user clicks Cancel button on Get shipping date dialog.
                        // Cancel the queue.
                        asyncQueue.cancel();
                        return;
                    }

                    var activityResponse: Activities.GetShippingDateActivityResponse = getShippingDateActivity.response;
                    requestedPickupDate = activityResponse.requestedShippingDate;
                });
            });

            if (!CustomerOrderHelper.isCustomerOrderOrQuoteCreationOrEdition(cart)) {

                asyncQueue.enqueue(() => {

                    // Run operation to convert cart to become customer order / quotation.
                    switch (purposeOperationId) {
                        case Proxy.Entities.CustomerOrderMode.CustomerOrderCreateOrEdit:
                            var options: Operations.ICreateCustomerOrderOperationOptions = {
                                cart: Session.instance.cart
                            };

                            return this.operationsManager.runOperation(Operations.RetailOperation.CreateCustomerOrder, options);
                        case Proxy.Entities.CustomerOrderMode.QuoteCreateOrEdit:
                            var quoteCreationOperationParameters: Operations.ICreateCustomerQuoteOperationOptions = {
                                cart: Session.instance.cart,
                                quotationExpirationDate: quotationExpirationDate
                            };

                            return this.operationsManager.runOperation(
                                Operations.RetailOperation.CreateQuotation, quoteCreationOperationParameters);
                        default:
                            asyncQueue.cancel();
                            return;
                    }
                });
            }

            // Add product to cart would be the last operation
            // since user may click cancel on any of the previous dialogs
            // and we don't want to add the product to cart for any canceled dialogs.
            asyncQueue.enqueue(() => {
                return this.addProductPickUpToCart(storeLocation, requestedPickupDate);
            });

            return asyncQueue.run();
        }

        private addProductPickUpToCart(storeLocation: Model.Entities.OrgUnitLocation, pickupDate: Date): IVoidAsyncResult {
            var variantId: number = this.variantId();
            var productId: number = NumberExtensions.isNullOrZero(variantId) ? this.product().RecordId : variantId;
            var lineIdsByLineId: { [lineId: string]: string } = {};
            Session.instance.cart.CartLines.forEach(c => lineIdsByLineId[c.LineId] = c.LineId);

            var asyncQueue: AsyncQueue = new AsyncQueue()
                .enqueue((): IAsyncResult<any> => {
                    var itemSaleOptions: Operations.IItemSaleOperationOptions = {
                        productSaleDetails: [{
                            productId: productId, quantity: 0
                        }]
                    };

                    return this.operationsManager.runOperation(Operations.RetailOperation.ItemSale, itemSaleOptions);
                }).enqueue((): IAsyncResult<any> => {
                    var cart: Model.Entities.Cart = Session.instance.cart;
                    var isAggregateLine: boolean = ApplicationContext.Instance.deviceConfiguration.AllowItemsAggregation;
                    // if the line is not aggregate, make sure it was not present before
                    var newCartLineAsArray: Model.Entities.CartLine[] = cart.CartLines.filter(c =>
                        c.ProductId === productId && (isAggregateLine || (!isAggregateLine && !lineIdsByLineId[c.LineId])));
                    var pickupOptions: Operations.IPickupSelectedOperationOptions = {
                        cart: cart,
                        cartLines: newCartLineAsArray,
                        pickupDate: pickupDate,
                        storeAddress: storeLocation.Address,
                        storeNumber: storeLocation.OrgUnitNumber,
                    };

                    // we need to use pickup all if there's only one cart line added
                    if (cart.CartLines.length === 1) {
                        return this.operationsManager.runOperation(Operations.RetailOperation.PickupAllProducts, pickupOptions);
                    }

                    return this.operationsManager.runOperation(Operations.RetailOperation.PickupSelectedProducts, pickupOptions);
                });

            return asyncQueue.run();
        }
    }
}
