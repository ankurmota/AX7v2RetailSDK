/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="../Managers/IManagerFactory.ts" />
/// <reference path="../Managers/ICartManager.ts" />
/// <reference path="../RetailLogger.d.ts" />
/// <reference path="GetProductsActivity.ts" />
/// <reference path="GetProductKeyInPriceActivity.ts" />
/// <reference path="GetProductKeyQuantityActivity.ts" />
/// <reference path="GetCartLineWeightActivity.ts" />
/// <reference path="GetReasonCodeLinesActivity.ts" />
/// <reference path="GetSerialNumberActivity.ts" />
/// <reference path="RegisterTimeActivity.ts" />
/// <reference path="SelectVariantActivity.ts" />

module Commerce {
    "use strict";

    import Entities = Proxy.Entities;
    import CartLine = Entities.CartLine;
    import CartLineClass = Entities.CartLineClass;
    import ICartManager = Model.Managers.ICartManager;
    import IProductManager = Model.Managers.IProductManager;
    import SimpleProduct = Entities.SimpleProduct;
    import SimpleLinkedProduct = Entities.SimpleLinkedProduct;
    import ProductSaleReturnDetails = Entities.ProductSaleReturnDetails;
    import ProductReturnDetails = Entities.ProductReturnDetails;

    /**
     * Exposes functions to create async queues for various activities.
     */
    export class ActivityHelper {

        /**
         * Creates an async queue with the business logic of how to ask for reason codes at the start of transaction, if any required.
         * @param {Proxy.Entities.Cart} cart The cart to add the reason codes to.
         * @return {AsyncQueue} The async queue.
         */
        public static getStartOfTransactionReasonCodesAsyncQueue(cart: Proxy.Entities.Cart): AsyncQueue {
            var asyncQueue: AsyncQueue = new AsyncQueue();
            var reasonCodeId: string = ApplicationContext.Instance.deviceConfiguration.StartOfTransaction;

            // if it is not a new cart and or a reason code is not required at the start of transaction
            if (ObjectExtensions.isNullOrUndefined(cart)
                || !StringExtensions.isNullOrWhitespace(cart.Id)
                || StringExtensions.isNullOrWhitespace(reasonCodeId)) {
                return asyncQueue;
            }

            var cartManager: ICartManager = Model.Managers.Factory.getManager<ICartManager>(Model.Managers.ICartManagerName);
            var affiliationLines: Proxy.Entities.AffiliationLoyaltyTier[] = cart.AffiliationLines;

            return asyncQueue.enqueue((): IAsyncResult<any> => {
                var result: IAsyncResult<ICancelableResult> = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                    { cart: cart, affiliationLines: affiliationLines },
                    (c: ReasonCodesContext) => { return cartManager.createOrUpdateCartAsync(c.cart); },
                    Proxy.Entities.ReasonCodeSourceType.StartOfTransaction).run();

                return asyncQueue.cancelOn(result);
            });
        }

        /**
         * Creates an async queue with the business logic of how to get product details, i.e. a collection of product/quantity pairs.
         * @param {ProductSaleReturnDetails[]} productSaleDetails The collection where the product sale details are added to.
         * @return {AsyncQueue} The async queue.
         */
        public static getProductSaleDetailsAsyncQueue(productSaleDetails: ProductSaleReturnDetails[]): AsyncQueue {
            var asyncQueue: AsyncQueue = new AsyncQueue();
            if (ArrayExtensions.hasElements(productSaleDetails)) {
                var productManager: IProductManager = Model.Managers.Factory.getManager<IProductManager>(Model.Managers.IProductManagerName);
                var productIdsToBeFetched: number[] = [];
                var productSaleDetailsByProductId: { [id: number]: Entities.ProductSaleReturnDetails } = Object.create(null);
                productSaleDetails.forEach((detail: ProductSaleReturnDetails) => {
                    if (ObjectExtensions.isNullOrUndefined(detail.product) && !(ObjectExtensions.isNullOrUndefined(detail.productId))) {
                        productSaleDetailsByProductId[detail.productId] = detail;
                        productIdsToBeFetched.push(detail.productId);
                    }
                });

                if (!ArrayExtensions.hasElements(productIdsToBeFetched)) {
                    return asyncQueue;
                }

                var channelId: number = Commerce.Session.instance.productCatalogStore.Context.ChannelId;

                // If there is more than one product id to fetch use getByIds if only one id use getById
                if (productIdsToBeFetched.length > 1) {
                    asyncQueue.enqueue(() => {
                        return productManager.getByIdsAsync(productIdsToBeFetched, channelId).done((products: SimpleProduct[]) => {
                            products.forEach((product: SimpleProduct) => {
                                var detail: ProductSaleReturnDetails = productSaleDetailsByProductId[product.RecordId];
                                if (!ObjectExtensions.isNullOrUndefined(detail)) {
                                    detail.product = product;
                                }
                            });
                        });
                    });
                } else {
                    asyncQueue.enqueue((): IVoidAsyncResult => {
                        return productManager.getByIdAsync(productIdsToBeFetched[0], channelId).done((fetchedProduct: SimpleProduct): void => {
                            var productSaleDetail: ProductSaleReturnDetails = productSaleDetailsByProductId[fetchedProduct.RecordId];
                            if (!ObjectExtensions.isNullOrUndefined(productSaleDetail)) {
                                productSaleDetail.product = fetchedProduct;
                            }
                        });
                    });
                }

                return asyncQueue;
            }

            return asyncQueue.enqueue((): IAsyncResult<any> => {
                // get all product details
                var activity: Activities.GetProductsActivity = new Activities.GetProductsActivity();
                return activity.execute().done(() => {
                    if (!activity.response) {
                        asyncQueue.cancel();
                        return null;
                    }

                    activity.response.forEach((productDetail: ProductSaleReturnDetails) => {
                        productSaleDetails.push(productDetail);
                    });
                });
            });
        }

        /**
         * Creates an async queue with the business logic of how to get product details to be returned.
         * @param {ProductReturnDetails[]} productReturnDetails The collection where the product return details are added to.
         * @return {AsyncQueue} The async queue.
         */
        public static getProductReturnDetailsAsyncQueue(productReturnDetails: ProductReturnDetails[]): AsyncQueue {
            var asyncQueue: AsyncQueue = new AsyncQueue();
            if (ArrayExtensions.hasElements(productReturnDetails)) {
                return asyncQueue;
            }

            return asyncQueue.enqueue((): IAsyncResult<any> => {
                // get all product details
                var activity: Activities.GetProductsToReturnActivity = new Activities.GetProductsToReturnActivity();
                return activity.execute().done(() => {
                    if (!activity.response) {
                        asyncQueue.cancel();
                        return null;
                    }

                    activity.response.forEach((productDetail: ProductReturnDetails) => {
                        productReturnDetails.push(productDetail);
                    });
                });
            });
        }

        /**
         * Creates an async queue with the business logic of how to create cart lines out of a collection product details.
         * @param {ProductSaleReturnDetails[]} productSaleDetails The collection of product sale details.
         * @param {CartLine[]} cartLines The collection where the cart lines are pushed into.
         * @return {AsyncQueue} The async queue.
         */
        public static createCartLinesAsyncQueue(productSaleDetails: ProductSaleReturnDetails[], cartLines: CartLine[]): AsyncQueue {
            var productQueue: AsyncQueue = new AsyncQueue();

            productQueue.enqueue((): IAsyncResult<any> => {
                // One or more items are serialized and can not have multiple quantities applied.
                // Please select a different item or reset the quantity amount on the transaction page and try again.
                var hasSerializedProductWithMultipleQuantities: boolean = productSaleDetails.some((productDetail: ProductSaleReturnDetails) => {
                    return productDetail.quantity > 1 && productDetail.product.Behavior.HasSerialNumber;
                });

                if (hasSerializedProductWithMultipleQuantities) {
                    return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.CANNOT_CHANGE_QUANTITY_WHEN_SERIALIZED)]);
                }

                return VoidAsyncResult.createResolved();
            });

            productSaleDetails.forEach((productDetail: ProductSaleReturnDetails) => {
                var cartLine: CartLineClass = new CartLineClass();
                cartLine.Quantity = productDetail.quantity;
                cartLine.ItemId = productDetail.product.ItemId;

                // If unit of measure is not explicitly provided use the product's default unit of measure; otherwise the provided unit of measure.
                if (StringExtensions.isNullOrWhitespace(productDetail.unitOfMeasureSymbol)) {
                    cartLine.UnitOfMeasureSymbol = productDetail.product.DefaultUnitOfMeasure;
                } else {
                    cartLine.UnitOfMeasureSymbol = productDetail.unitOfMeasureSymbol;
                }

                if (!ObjectExtensions.isNullOrUndefined(productDetail.barcode)) {
                    cartLine.Barcode = productDetail.barcode.BarcodeId;
                }

                // check whether it needs variant number or just product identifier
                productQueue.enqueue((): IAsyncResult<any> => {
                    var productIdQueue: AsyncQueue = ActivityHelper.getProductIdAsyncQueue(productDetail, cartLine);
                    return productQueue.cancelOn(productIdQueue.run());
                });

                // check whether we need to key in quantity
                productQueue.enqueue((): IAsyncResult<any> => {
                    var keyInQuantityQueue: AsyncQueue = ActivityHelper.getProductKeyInQuantityAsyncQueue(productDetail, cartLine);
                    return productQueue.cancelOn(keyInQuantityQueue.run());
                });

                // gets cart line description from product and cache product on session
                productQueue.enqueue((): IAsyncResult<any> => {
                    cartLine.Description = productDetail.product.Name;
                    Session.instance.addToProductsInCartCache(productDetail.product);
                    return VoidAsyncResult.createResolved();
                });

                // check whether it needs serial number
                productQueue.enqueue((): IAsyncResult<any> => {
                    var serialNumberQueue: AsyncQueue = ActivityHelper.getSerialNumberAsyncQueue(productDetail.product, cartLine);
                    return productQueue.cancelOn(serialNumberQueue.run());
                });

                // check whether we need to key in price
                productQueue.enqueue((): IAsyncResult<any> => {
                    var getProductKeyInPriceQueue: AsyncQueue = ActivityHelper.getProductKeyInPriceAsyncQueue(productDetail, cartLine);
                    return productQueue.cancelOn(getProductKeyInPriceQueue.run());
                });

                // check whether product needs to be weighed
                productQueue.enqueue((): IAsyncResult<any> => {
                    var getProductWeightQueue: AsyncQueue = ActivityHelper.getProductWeightAsyncQueue(productDetail, cartLine);
                    return productQueue.cancelOn(getProductWeightQueue.run());
                });

                // check whether we need to key in comment
                productQueue.enqueue((): IAsyncResult<any> => {
                    var getProductKeyInCommentQueue: AsyncQueue = ActivityHelper.getProductKeyInCommentAsyncQueue(productDetail.product, cartLine);
                    return productQueue.cancelOn(getProductKeyInCommentQueue.run());
                });

                // add the cart line and check whether it has linked products
                // this way linked products are added immediately after the product
                productQueue.enqueue((): IAsyncResult<any> => {
                    cartLines.push(cartLine);

                    var linkedProductsQueue: AsyncQueue = ActivityHelper.getLinkedProductsAsyncQueue(productDetail, cartLines);
                    return productQueue.cancelOn(linkedProductsQueue.run());
                });
            });

            return productQueue;
        }

        /**
         * Creates an async queue for adding cart lines to a cart. It also takes care of required reason codes.
         * @param {CartLine[]} cartLines The collection of cart lines to add to the cart.
         * @return {AsyncQueue} The async queue.
         */
        public static addCartLinesAsyncQueue(cartLines: CartLine[]): AsyncQueue {
            var correlationId: string = Microsoft.Dynamics.Diagnostics.TypeScriptCore.Utils.generateGuid();
            RetailLogger.helpersActivityHelperAddCartLinesStarted(correlationId);

            var asyncQueue: AsyncQueue = new AsyncQueue();
            return asyncQueue.enqueue((): IAsyncResult<any> => {
                // we add the cart lines to the cart and check for reason codes on failure
                var cartManager: Model.Managers.ICartManager = Model.Managers.Factory.GetManager(Model.Managers.ICartManagerName, null);
                var result: IAsyncResult<ICancelableResult> = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                    { cart: Session.instance.cart, cartLines: cartLines },
                    (c: ReasonCodesContext) => { return cartManager.addCartLinesToCartAsync(c.cartLines); }).run();

                return asyncQueue.cancelOn(result)
                    .done((addCartLinesResult: ICancelableResult) => {
                        if (addCartLinesResult && !addCartLinesResult.canceled) {
                            RetailLogger.helpersActivityHelperAddCartLinesFinished(correlationId);
                        }
                    });
            });
        }

        /**
         * Creates an async queue with the business logic of how to get a product serial number, if required.
         * @param {SimpleProduct} product The product to get the serial number for.
         * @param {CartLine} cartLine The cart line where the serial number is added to.
         * @return {AsyncQueue} The async queue.
         */
        public static getSerialNumberAsyncQueue(product: SimpleProduct, cartLine: CartLine): AsyncQueue {
            var asyncQueue: AsyncQueue = new AsyncQueue();
            if (!CustomerOrderHelper.isSerializedNumberRequired(product, cartLine)) {
                return asyncQueue;
            }

            return asyncQueue.enqueue((): IAsyncResult<any> => {
                var activity: Activities.GetSerialNumberActivity = new Activities.GetSerialNumberActivity({ product: product });
                return activity.execute().done(() => {
                    if (!activity.response) {
                        asyncQueue.cancel();
                        return;
                    }

                    cartLine.SerialNumber = activity.response.serialNumber;
                });
            });
        }

        /**
         * Creates an async queue with the business logic of how to get the default kit configuration product identifier.
         * @param {ProductSaleReturnDetails} productSaleDetail The product sale detail to get identifier from.
         * @param {CartLine} cartLine The cart line where the product identifier is added to.
         * @return {AsyncQueue} The async queue.
         */
        private static getDefaultKitConfigurationAsyncQueue(productSaleDetail: ProductSaleReturnDetails, cartLine: CartLine): AsyncQueue {
            var kitConfigurationQueue: AsyncQueue = new AsyncQueue();
            var product: Entities.SimpleProduct = productSaleDetail.product;

            if (product.ProductTypeValue !== Entities.ProductType.KitMaster) {
                cartLine.ProductId = product.RecordId;
                return kitConfigurationQueue;
            }

            var channelId: number = Session.instance.productCatalogStore.Context.ChannelId;
            var defaultComponentsInSlots: Entities.ComponentInSlotRelation[];
            var productManager: IProductManager = Model.Managers.Factory.getManager<IProductManager>(Model.Managers.IProductManagerName);

            kitConfigurationQueue.enqueue((): IVoidAsyncResult => {
                // Get the default kit components and create the component in slot relations.
                return productManager.getDefaultComponentsAsync(product.RecordId, channelId).done((components: Entities.ProductComponent[]): void => {
                    defaultComponentsInSlots = components.map((component: Proxy.Entities.ProductComponent): Entities.ComponentInSlotRelation => {
                        return { ComponentId: component.ProductId, SlotId: component.SlotId };
                    });
                });
            }).enqueue((): IVoidAsyncResult => {
                // Get the kit variant based on the default component selection value.
                return productManager.getVariantsByComponentsInSlotsAsync(product.RecordId, channelId, defaultComponentsInSlots, 1, 0)
                    .done((kitVariants: Entities.SimpleProduct[]): void => {
                        if (ArrayExtensions.hasElements(kitVariants)) {
                            cartLine.ProductId = kitVariants[0].RecordId;
                        }
                    });
            });

            return kitConfigurationQueue;
        }

        /**
         * Creates an async queue with the business logic of how to get a product identifier.
         * If the product is a master product, asks for a product variant instead.
         * @param {ProductSaleReturnDetails} productSaleDetail The product sale detail to get identifier from.
         * @param {CartLine} cartLine The cart line where the product identifier is added to.
         * @return {AsyncQueue} The async queue.
         */
        private static getProductIdAsyncQueue(productSaleDetail: ProductSaleReturnDetails, cartLine: CartLine): AsyncQueue {
            var asyncQueue: AsyncQueue = new AsyncQueue();
            var product: Proxy.Entities.SimpleProduct = productSaleDetail.product;

            if (product.ProductTypeValue === Entities.ProductType.KitMaster) {
                return ActivityHelper.getDefaultKitConfigurationAsyncQueue(productSaleDetail, cartLine);
            } else if (product.ProductTypeValue !== Proxy.Entities.ProductType.Master) {
                cartLine.ProductId = product.RecordId;
                return asyncQueue;
            }

            var productManager: IProductManager = Model.Managers.Factory.getManager<IProductManager>(Model.Managers.IProductManagerName);

            return asyncQueue.enqueue((): IAsyncResult<any> => {

                var activity: Activities.SelectVariantActivity
                    = new Activities.SelectVariantActivity({ product: product });

                activity.responseHandler = (response: Activities.SelectVariantActivityResponse): IVoidAsyncResult => {
                    var variantRetrievalResult: VoidAsyncResult = new VoidAsyncResult();

                    // Validate that a value was selected for each product dimension.
                    if (!ArrayExtensions.hasElements(response.selectedDimensions) || response.selectedDimensions.length !== product.Dimensions.length) {
                        var requiredDimensionValuesMissingError: Entities.Error = new Entities.Error(ErrorTypeEnum.REQUIRED_DIMENSION_VALUES_MISSING);
                        variantRetrievalResult.reject([requiredDimensionValuesMissingError]);
                    }

                    var channelId: number = Session.instance.productCatalogStore.Context.ChannelId;
                    productManager.getVariantsByDimensionValuesAsync(product.RecordId, channelId, response.selectedDimensions)
                        .done((variants: Entities.SimpleProduct[]): void => {
                            var variantProduct: Entities.SimpleProduct = ArrayExtensions.firstOrUndefined(variants);
                            if (!ObjectExtensions.isNullOrUndefined(variantProduct)) {
                                cartLine.ProductId = variantProduct.RecordId;
                                productSaleDetail.product = variantProduct;
                                variantRetrievalResult.resolve();
                            } else {
                                var variantNotFoundError: Entities.Error = new Entities.Error(ErrorTypeEnum.MATCHING_VARIANT_NOT_FOUND);
                                variantRetrievalResult.reject([variantNotFoundError]);
                            }
                        }).fail((variantRetrievalErrors: Entities.Error[]): void => {
                            variantRetrievalResult.reject(variantRetrievalErrors);
                        });

                    return variantRetrievalResult;
                };

                return asyncQueue.cancelOn(activity.execute());
            });
        }

        /**
         * Creates an async queue with the business logic of how to get a product price, if required.
         * @param {Product} product The product to get the price for.
         * @param {CartLine} cartLine The cart line where the serial number is added to.
         * @return {AsyncQueue} The async queue.
         */
        private static getProductKeyInPriceAsyncQueue(productDetail: Proxy.Entities.ProductSaleReturnDetails, cartLine: CartLine): AsyncQueue {
            var asyncQueue: AsyncQueue = new AsyncQueue();

            // If the product detail has a barcode and the barcode contains price information we do not prompt for key in price.
            if (!ObjectExtensions.isNullOrUndefined(productDetail.barcode) && !NumberExtensions.isNullOrZero(productDetail.barcode.BarcodePrice)) {
                return asyncQueue;
            }

            var min: number;
            var max: number;
            var minPriceInclusive: boolean;
            var maxPriceInclusive: boolean;
            var validationError: Entities.Error = null;
            var product: SimpleProduct = productDetail.product;

            // Determine the allowed price range if the product price is not 0
            if (product.Price !== 0) {
                switch (product.Behavior.KeyInPriceValue) {
                    case (Entities.KeyInPriceRestriction.None):
                    case (Entities.KeyInPriceRestriction.NotAllowed):
                        return asyncQueue;
                    case (Entities.KeyInPriceRestriction.NewPrice):
                        min = 0;
                        max = Number.MAX_VALUE;
                        break;
                    case (Entities.KeyInPriceRestriction.HigherOrEqualPrice):
                        min = product.Price;
                        max = Number.MAX_VALUE;
                        break;
                    case (Entities.KeyInPriceRestriction.LowerOrEqualPrice):
                        min = 0;
                        max = product.Price;
                        break;
                }

                minPriceInclusive = true;
                maxPriceInclusive = true;
            } else {
                // The behavior for setting a price when a product price is 0 is based on the product and the functionality profile based on the table below:
                //               PRODUCT SETTINGS                                         FUNCTIONALITY PROFILE - MUST KEY IN PRICE IF ZERO
                // KEY IN PRICE                       |  ZERO PRICE VALID  |  Expected behavior when TRUE        Expected behavior when FALSE
                // ************                       |  ****************  |  ***************************     |  ***************************
                // Not mandatory                      |  TRUE              |  Key in any price, zero price OK |  Product is added, key in price not requested
                // Must key in new price              |  TRUE              |  Key in any price, zero price OK |  Key in any price, zero price OK
                // Must key in higher or equal price  |  TRUE              |  Key in >= price, zero price OK  |  Key in >= price, zero price OK
                // Must key in lower or equal price   |  TRUE              |  Key in <= price, zero price OK  |  Key in <= price, zero price OK
                // Must NOT key in price              |  TRUE              |  Product is added, key in price not requested  (both for TRUE and FALSE)
                // Not mandatory                      |  FALSE             |  Key in any non-zero price       |  Key in any non-zero price 
                // Must key in new price              |  FALSE             |  Key in any non-zero price       |  Key in any non-zero price 
                // Must key in higher or equal price  |  FALSE             |  Key in > price. No zero price.  |  Key in > price. No zero price.
                // Must key in lower or equal price   |  FALSE             |  Error message, product is not added | Error message, product is not added.
                // Must NOT key in price              |  FALSE             |  Error message, product is not added (both for TRUE and FALSE).
                var mustKeyInPriceIfZero: boolean = ApplicationContext.Instance.deviceConfiguration.MustKeyInPriceIfZero;
                var zeroPriceValid: boolean = product.Behavior.IsZeroSalePriceAllowed;
                switch (product.Behavior.KeyInPriceValue) {
                    case (Proxy.Entities.KeyInPriceRestriction.None):
                        if (zeroPriceValid && !mustKeyInPriceIfZero) {
                            return asyncQueue;
                        } else {
                            min = 0;
                            max = Number.MAX_VALUE;
                            minPriceInclusive = zeroPriceValid;
                            maxPriceInclusive = true;
                        }
                        break;
                    case (Proxy.Entities.KeyInPriceRestriction.NotAllowed):
                        if (zeroPriceValid) {
                            return asyncQueue;
                        } else {
                            validationError = new Entities.Error(ErrorTypeEnum.ITEM_ADD_INVALID_NON_UPDATABLE_PRICE, false, null, null, product.Name);
                        }
                    case (Proxy.Entities.KeyInPriceRestriction.NewPrice):
                        min = 0;
                        max = Number.MAX_VALUE;
                        minPriceInclusive = zeroPriceValid;
                        maxPriceInclusive = true;
                        break;
                    case (Proxy.Entities.KeyInPriceRestriction.HigherOrEqualPrice):
                        min = product.Price;
                        max = Number.MAX_VALUE;
                        minPriceInclusive = zeroPriceValid;
                        maxPriceInclusive = true;
                        break;
                    case (Proxy.Entities.KeyInPriceRestriction.LowerOrEqualPrice):
                        if (zeroPriceValid) {
                            min = 0;
                            max = product.Price;
                            minPriceInclusive = true;
                            maxPriceInclusive = true;
                        } else {
                            validationError = new Entities.Error(ErrorTypeEnum.ITEM_ADD_INVALID_NON_UPDATABLE_PRICE, false, null, null, product.Name);
                        }
                        break;
                }
            }

            // Fail the execution if a validation error exists.
            if (!ObjectExtensions.isNullOrUndefined(validationError)) {
                return asyncQueue.enqueue(() => {
                    return AsyncResult.createRejected([validationError]);
                });
            }

            // Call the activity to get the price
            return asyncQueue.enqueue((): IAsyncResult<any> => {
                var activity: Activities.GetProductKeyInPriceActivity = new Activities.GetProductKeyInPriceActivity(
                    { product: product, minPrice: min, maxPrice: max, minPriceInclusive: minPriceInclusive, maxPriceInclusive: maxPriceInclusive });

                return activity.execute().done(() => {
                    if (!activity.response) {
                        asyncQueue.cancel();
                        return;
                    }

                    cartLine.IsPriceKeyedIn = true;
                    cartLine.Price = activity.response.keyInPrice;
                });
            });
        }

        /**
         * Creates an async queue with the business logic of how to get a product quantity, if required.
         * @param {ProductSaleReturnDetails} productDetails The product to get the quantity for.
         * @param {CartLine} cartLine The cart line where the quantity number is added to.
         * @return {AsyncQueue} The async queue.
         */
        private static getProductKeyInQuantityAsyncQueue(productDetails: ProductSaleReturnDetails, cartLine: CartLine): AsyncQueue {
            var asyncQueue: AsyncQueue = new AsyncQueue();
            var product: SimpleProduct = productDetails.product;

            if (product.Behavior.MustWeighProductAtSale || !NumberExtensions.isNullOrZero(productDetails.quantity)) {
                return asyncQueue;
            }

            if (product.Behavior.KeyInQuantityValue === Entities.KeyInQuantityRestriction.Required) {
                return asyncQueue.enqueue((): IAsyncResult<any> => {
                    var activity: Activities.GetProductKeyInQuantityActivity = new Activities.GetProductKeyInQuantityActivity({ product: product });
                    return activity.execute().done(() => {
                        if (!activity.response) {
                            asyncQueue.cancel();
                            return;
                        }

                        cartLine.Quantity = activity.response.keyInQuantity;
                    });
                });
            } else {
                productDetails.quantity = 1;
                cartLine.Quantity = productDetails.quantity;
                return asyncQueue;
            }
        }

        /**
         * Creates an async queue with the business logic of how to get a product comment, if required.
         * @param {SimpleProduct} product The product to get the comment for.
         * @param {CartLine} cartLine The cart line where the comment is to be added to.
         * @return {AsyncQueue} The async queue.
         */
        private static getProductKeyInCommentAsyncQueue(product: SimpleProduct, cartLine: CartLine): AsyncQueue {
            var asyncQueue: AsyncQueue = new AsyncQueue();

            if (!product.Behavior.MustKeyInComment) {
                return asyncQueue;
            }

            return asyncQueue.enqueue((): IAsyncResult<any> => {
                var activity: Activities.GetCartLineCommentsActivity = new Activities.GetCartLineCommentsActivity({ cartLines: [cartLine] });
                return activity.execute().done(() => {
                    if (!activity.response) {
                        asyncQueue.cancel();
                        return;
                    }

                    cartLine.Comment = activity.response.comments[0];
                });
            });
        }

        /**
         * Creates an async queue with the business logic of how to get a product weight, if required.
         * @param {ProductSaleReturnDetails} productSaleDetail The product return/sale detail to get weight for.
         * @param {CartLine} cartLine The cart line where the weight is added to.
         * @return {AsyncQueue} The async queue.
         */
        private static getProductWeightAsyncQueue(productSaleDetail: ProductSaleReturnDetails, cartLine: CartLine): AsyncQueue {
            var asyncQueue: AsyncQueue = new AsyncQueue();
            var product: SimpleProduct = productSaleDetail.product;

            if (!product.Behavior.MustWeighProductAtSale) {
                return asyncQueue;
            }

            return asyncQueue.enqueue((): IAsyncResult<any> => {
                var activity: Activities.GetCartLineWeightActivity = new Activities.GetCartLineWeightActivity({ cartLine: cartLine });
                return activity.execute().done(() => {
                    if (!activity.response) {
                        asyncQueue.cancel();
                        return;
                    }

                    // if quantity is negative due to manual return
                    var sign: number = 1;
                    if (productSaleDetail.quantity !== 0 && !isNaN(productSaleDetail.quantity)) {
                        sign = productSaleDetail.quantity / Math.abs(productSaleDetail.quantity);
                    }

                    cartLine.Quantity = sign * activity.response.weight;
                });
            });
        }

        /**
         * Creates an async queue with the business logic of how to get a linked products, if required.
         * @param {ProductSaleReturnDetails} productSaleDetail The product return/sale detail to get linked products from.
         * @param {CartLine[]} cartLines The cart line collection where the linked products are added to.
         * @return {AsyncQueue} The async queue.
         */
        private static getLinkedProductsAsyncQueue(productSaleDetail: ProductSaleReturnDetails, cartLines: CartLine[]): AsyncQueue {
            var asyncQueue: AsyncQueue = new AsyncQueue();
            var product: SimpleProduct = productSaleDetail.product;

            if (!ArrayExtensions.hasElements(product.LinkedProducts)) {
                return asyncQueue;
            }

            var linkedProductSaleDetails: ProductSaleReturnDetails[] = null;

            return asyncQueue.enqueue((): IAsyncResult<any> => {

                linkedProductSaleDetails = product.LinkedProducts.map((linkedProduct: SimpleLinkedProduct) => {
                    var product: SimpleProduct = linkedProduct;
                    var sign: number = 1;

                    if (productSaleDetail.quantity !== 0 && !isNaN(productSaleDetail.quantity)) {
                        sign = productSaleDetail.quantity / Math.abs(productSaleDetail.quantity);
                    }

                    return <ProductSaleReturnDetails>{
                        product: product,
                        // uses the same sign as the original product to honor returns
                        quantity: sign * Math.abs(linkedProduct.Quantity) * Math.abs(productSaleDetail.quantity)
                    };
                });

                // add these products right after the product that links to them
                var cartLinesQueue: AsyncQueue = ActivityHelper.createCartLinesAsyncQueue(linkedProductSaleDetails, cartLines);
                return cartLinesQueue.run();
            });
        }
    }
}