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

    import Entities = Proxy.Entities;

    export interface MappedSalesLine extends Proxy.Entities.SalesLine {
        ProductName?: string;
        RelatedProducts?: Entities.RelatedProduct[];
        Image?: Entities.RichMediaLocations;
        ReceiptId?: string;
        DatePurchased?: string;
    }

    export enum OrderHistorySortField {
        Date,
        Number,
        Total
    }

    export interface ProductVariantDimensionValues {
        variantId: number;
        productId: number;
        dimensions: string;
    }

    export class CustomerDetailsViewModel extends ViewModelBase {
        public customerAccountNumber: string;
        public customerAddress: Observable<Proxy.Entities.Address>;
        public salesLines: ObservableArray<Proxy.Entities.SalesLine>;
        public salesOrders: ObservableArray<Proxy.Entities.SalesOrder>;
        public recentPurchases: ObservableArray<Proxy.Entities.PurchaseHistory>;
        public suggestedProducts: ObservableArray<Proxy.Entities.Product>;
        public suggestedProductsForDisplay: ObservableArray<Proxy.Entities.Product>;
        public loyaltyCards: ObservableArray<Proxy.Entities.LoyaltyCard>;
        public Customer: Observable<Proxy.Entities.Customer>;

        /* Customer visit and creation details */
        public customerImage: Observable<string>;
        public customerAccount: Computed<string>;
        public invoiceAccount: Computed<string>;
        public dateOfLastVisit: Observable<Date>;
        public dateOfLastVisitString: Computed<string>;
        public placeOfLastVisitString: Observable<string>;
        public placeOfLastVisitId: Observable<string>;
        public dateCreatedString: Observable<string>;

        /* Recent Purchases Fields */
        public static RECENT_PURCHASES_TO_INITIAL_DISPLAY: number = 4; // UX requirements
        public static RECENT_PURCHASES_PAGE_SIZE: number = 20;
        public displayZeroProductsText: Computed<boolean>;

        /* Wish List Fields */
        public wishLists: ObservableArray<Proxy.Entities.CommerceList>;
        public totalWishListsString: Computed<string>;
        public wishListProducts: ObservableArray<Proxy.Entities.Product>;

        /* Suggested Products Fields */
        public static RECENT_PURCHASES_FOR_SUGGESTED_PRODUCTS: number = 5; // UX requirements
        public totalSuggestedProductsString: Computed<string>;
        private _totalSuggestedProducts: Observable<number>;

        /* Loyalty Card Fields */
        public totalLoyaltyCardsString: Computed<string>;

        /* Order History Fields */
        public static SALES_ORDERS_FOR_INITIAL_DISPLAY: number = 10; // UX requirements
        public salesOrdersToDisplay: ObservableArray<Proxy.Entities.SalesOrder>;
        public totalSalesOrdersString: Computed<string>;
        private _totalSalesOrders: Observable<number>;
        private _orderHistorySortColumnName: Observable<string>;
        private _orderHistorySortColumn: OrderHistorySortField;
        private _isOrderHistorySortingDescending: boolean;
        public displayZeroSalesOrdersText: Observable<boolean>;
        public displaySalesOrdersGrid: Observable<boolean>;
        public displaySalesOrdersSortByMenu: Observable<boolean>;
        private _countOfSalesOrdersToLoad: number;

        /* Customer Affiliation Fields */
        private static CUSTOMER_AFFILIATIONS_FOR_INITIAL_DISPLAY: number = 5;
        public customerAffiliationsToDisplay: ObservableArray<Proxy.Entities.CustomerAffiliation>;
        public totalCustomerAffiliationsString: Computed<string>;
        public isCustomerAffiliationsTextVisible: Observable<boolean>;
        public isCustomerAffiliationsGridVisible: Observable<boolean>;
        public customerAffiliations: ObservableArray<Proxy.Entities.CustomerAffiliation>;
        private _totalCustomerAffiliations: Observable<number>;

        private shortDateFormatter: Host.IDateTimeFormatter;

        constructor() {
            super();

            this.shortDateFormatter = Host.instance.globalization.getDateTimeFormatter(Host.Globalization.DateTimeFormat.DATE_TIME);

            this.Customer = ko.observable(<Proxy.Entities.Customer>null);
            this.customerImage = ko.observable("");

            this.customerAddress = ko.observable(<Proxy.Entities.Address>null);

            this.salesLines = ko.observableArray<Proxy.Entities.SalesLine>([]);
            this.recentPurchases = ko.observableArray<Proxy.Entities.PurchaseHistory>([]);
            this.suggestedProducts = ko.observableArray<Proxy.Entities.Product>([]);
            this.suggestedProductsForDisplay = ko.observableArray<Proxy.Entities.Product>([]);
            this.salesOrders = ko.observableArray<Proxy.Entities.SalesOrder>([]);
            this.loyaltyCards = ko.observableArray<Proxy.Entities.LoyaltyCard>([]);
            this.customerAffiliations = ko.observableArray([]);
            this.customerAccount = ko.computed<string>(() => {
                return this.Customer() ? this.Customer().AccountNumber : "";
            }, this);
            this.invoiceAccount = ko.computed<string>(() => {
                return this.Customer() ? this.Customer().InvoiceAccount : "";
            }, this);
            this.dateOfLastVisit = ko.observable(null);
            this.dateOfLastVisitString = ko.computed<string>(() => {
                return this.dateOfLastVisit() ? this.shortDateFormatter.format(this.dateOfLastVisit()) : "";
            }, this);
            this.dateCreatedString = ko.observable("");
            this.placeOfLastVisitString = ko.observable("");
            this.placeOfLastVisitId = ko.observable("");

            /* Initialization of Wish List Fields */
            this.wishLists = ko.observableArray<Proxy.Entities.CommerceList>([]);
            this.totalWishListsString = ko.computed<string>(() => {
                return StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_256"), this.wishLists().length);
            }, this);
            this.wishListProducts = ko.observableArray<Proxy.Entities.Product>([]);

            /* Initialization of Suggested Products Fields */
            this._totalSuggestedProducts = ko.observable(0);
            this.totalSuggestedProductsString = ko.computed<string>(() => {
                return StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_252"), this._totalSuggestedProducts());
            }, this);
            this.displayZeroProductsText = ko.computed<boolean>(() => { return !ArrayExtensions.hasElements(this.recentPurchases()); });

            /* Initialization of Loyalty Card Fields */
            this.totalLoyaltyCardsString = ko.computed<string>(() => {

                var numberOfCards: number = this.loyaltyCards().length;
                var formatString: string = numberOfCards === 1 ? "string_275" : "string_271";

                return StringExtensions.format(Commerce.ViewModelAdapter.getResourceString(formatString), numberOfCards);
            }, this);

            /* Initialization of Order History Fields */
            this.salesOrdersToDisplay = ko.observableArray<Proxy.Entities.SalesOrder>([]);
            this._totalSalesOrders = ko.observable(0);
            this._orderHistorySortColumn = OrderHistorySortField.Date;
            this.totalSalesOrdersString = ko.computed<string>(() => {
                return StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_243"), this._totalSalesOrders());
            }, this);
            this._orderHistorySortColumnName = ko.observable(Commerce.ViewModelAdapter.getResourceString("string_244"));
            this._isOrderHistorySortingDescending = true;
            this.displayZeroSalesOrdersText = ko.observable(false);
            this.displaySalesOrdersGrid = ko.observable(false);
            this.displaySalesOrdersSortByMenu = ko.observable(false);
            this._countOfSalesOrdersToLoad = CustomerDetailsViewModel.SALES_ORDERS_FOR_INITIAL_DISPLAY;

            /* Initialization of Customer Affiliations Fields */
            this.totalCustomerAffiliationsString = ko.computed(() => {
                return StringExtensions.format(
                    Commerce.ViewModelAdapter.getResourceString(this.customerAffiliations().length != 1
                        ? "string_6302" /* {0} affiliations */
                        : "string_6308" /* {0} affiliation */), this.customerAffiliations().length);
            }, this);
            this.isCustomerAffiliationsTextVisible = ko.observable(false);
            this.isCustomerAffiliationsGridVisible = ko.observable(false);
            this.customerAffiliationsToDisplay = ko.observableArray([]);
        }

        /**
         * Loads the customer given the account number.
         *
         * @param {string} accountNumber The customer account number.
         * @return {IVoidAsyncResult} The async result.
         */
        public loadCustomer(accountNumber): IVoidAsyncResult {
            this.customerAccountNumber = accountNumber;

            if (StringExtensions.isNullOrWhitespace(this.customerAccountNumber)) {
                return VoidAsyncResult.createResolved();
            }

            return this.customerManager.getCustomerDetailsAsync(this.customerAccountNumber)
                .done((customerDetails) => {
                    this.Customer(customerDetails.customer);

                    // Sets the value for the date created string
                    if (!ObjectExtensions.isNullOrUndefined(customerDetails.customer)) {
                        this.dateCreatedString(this.shortDateFormatter.format(customerDetails.customer.CreatedDateTime));
                    }

                    this.customerImage(Commerce.ImageDisplayHelper.GetCustomerImageContent(customerDetails.customer));

                    if (customerDetails.primaryAddress) {
                        this.customerAddress(customerDetails.primaryAddress);
                    }

                    this.populateCustomerAffiliations(customerDetails.customer.CustomerAffiliations);
                });
        }

        /**
         * Adds the selected customer to the session cart.
         * @return {IVoidAsyncResult} The async result.
         */
        public addCustomerToCart(cartAffiliationLines: Proxy.Entities.AffiliationLoyaltyTier[]): IAsyncResult<ICancelableResult> {
            if (StringExtensions.isNullOrWhitespace(this.customerAccountNumber)) {
                return VoidAsyncResult.createResolved();
            }

            var options: Operations.IAddCustomerToSalesOrderOperationOptions = {
                customerId: this.customerAccountNumber,
                cartAffiliations: cartAffiliationLines
            };

            return this.operationsManager.runOperation(Operations.RetailOperation.SetCustomer, options);
        }

        /**
        * Gets customer balance.
        *
        * @param {string} customerId The customer identifier.
        * @param {string} invoiceId The invoice idintifier.
        * @returns {Entities.CustomerBalances} The async result.
        */
        public getBalanceAsync(customerId: string, invoiceId: string): IAsyncResult<Proxy.Entities.CustomerBalances> {
            return this.customerManager.getCustomerBalanceAsync(customerId, invoiceId);
        }

        /**
         * Populates an array of products suggested to the customer.
         *
         * @param {Proxy.Entities.SalesOrder[]} salesOrders - Customer sales orders.
         * @param {number} countOfRecentPurchasesToLoad - Sets how many items are included in the final collection.
         * @return {IVoidAsyncResult} The async result.
         */
        private populateSuggestedProducts(salesOrders: Proxy.Entities.SalesOrder[], countOfRecentPurchasesToLoad?: number): IVoidAsyncResult {
            countOfRecentPurchasesToLoad = countOfRecentPurchasesToLoad || CustomerDetailsViewModel.RECENT_PURCHASES_TO_INITIAL_DISPLAY;

            var listingIds: number[] = [];
            var salesLineByListingId: { [index: number]: MappedSalesLine } = {};

            // filters out voided transactions and if order placed date is null or undefined,
            // we must ignore it since there is no context for "Recency"
            salesOrders = salesOrders.filter(s => !ObjectExtensions.isNullOrUndefined(s.CreatedDateTime)
                && !(s.StatusValue === Proxy.Entities.SalesStatus.Canceled));

            salesOrders.forEach(salesOrder => {
                var receiptId = salesOrder.ReceiptId;
                // Find alternative id.
                if (StringExtensions.isNullOrWhitespace(salesOrder.ReceiptId)) {
                    receiptId = salesOrder.ChannelReferenceId;

                    if (StringExtensions.isNullOrWhitespace(salesOrder.ChannelReferenceId)) {
                        receiptId = salesOrder.SalesId;
                    }
                }

                var dateTimePurchased = salesOrder.CreatedDateTime;
                var dateTimePurchasedFormatted = this.shortDateFormatter.format(dateTimePurchased);

                salesOrder.SalesLines.forEach(salesLine => {
                    var mappedSalesLine: MappedSalesLine = salesLine;
                    // Ignore returned line.
                    if (StringExtensions.isNullOrWhitespace(mappedSalesLine.ReturnTransactionId) && mappedSalesLine.Quantity >= 0) {
                        mappedSalesLine.ReceiptId = receiptId;
                        mappedSalesLine.DatePurchased = dateTimePurchasedFormatted;

                        if (ObjectExtensions.isNullOrUndefined(this.dateOfLastVisit()) || dateTimePurchased >= this.dateOfLastVisit()) {
                            this.dateOfLastVisit(dateTimePurchased);
                        }

                        listingIds.push(mappedSalesLine.ListingId);
                        salesLineByListingId[mappedSalesLine.ListingId] = mappedSalesLine;
                    }
                });
            });

            var products: Proxy.Entities.Product[];
            var asyncQueue = new AsyncQueue()
                .enqueue(() => {
                    return this.productManager.getProductDetailsByDataLevelAsync(listingIds, 3, true)
                        .done((productDetails) => {
                            products = ProductPropertiesHelper.ArrangeProducts(listingIds, productDetails);
                        });
                }).enqueue(() => {
                    var recentlyPurchasedProductIds = this.getRecentlyPurchasedProductIds(
                        products, salesLineByListingId, countOfRecentPurchasesToLoad);

                    return this.productManager.getProductDetailsByDataLevelAsync(recentlyPurchasedProductIds, 1, true)
                        .done((recentlyPurchasedProducts: Proxy.Entities.Product[]) => {
                            this.suggestedProducts(recentlyPurchasedProducts);
                            this.suggestedProductsForDisplay(recentlyPurchasedProducts.slice(0, CustomerDetailsViewModel.RECENT_PURCHASES_TO_INITIAL_DISPLAY));
                            this._totalSuggestedProducts(recentlyPurchasedProductIds.length);
                        })
                });

            return asyncQueue.run();
        }

        private getRecentlyPurchasedProductIds(
            products: Proxy.Entities.Product[],
            salesLineByListingId: { [index: number]: MappedSalesLine },
            countOfRecentPurchasesToLoad: number): number[] {

            // keep track of the number of products processed, limited to "countOfRecentPurchasesToLoad" parameter
            var matchingProduct: Proxy.Entities.ProductVariant = null;
            var salesLines: MappedSalesLine[] = [];

            for (var i = 0; i < products.length; i++) {
                // CompositionInformation > VariantInformation > Variants
                var productRecordId: number = products[i].RecordId;
                var mappedSalesLine = salesLineByListingId[productRecordId];

                if (ObjectExtensions.isNullOrUndefined(mappedSalesLine)) {
                    // productRecordId refers to the Master Product Id, but we need to get the variant id
                    // of the actual product purchased:
                    var variants = products[i].CompositionInformation.VariantInformation.Variants;
                    var matchingProducts = variants.filter((productVariantItem) => {
                        var productVariantId = productVariantItem.DistinctProductVariantId;
                        return !ObjectExtensions.isNullOrUndefined(salesLineByListingId[productVariantId]);
                    });

                    if (ArrayExtensions.hasElements(matchingProducts)) {
                        matchingProduct = matchingProducts[0];
                        mappedSalesLine = salesLineByListingId[matchingProduct.DistinctProductVariantId];
                    }
                }

                // Pull the ProductName from within the list of Translated Properties, ignoring case
                var productName: string = "";
                if (!ObjectExtensions.isNullOrUndefined(matchingProduct)
                    && ArrayExtensions.hasElements(matchingProduct.PropertiesAsList)) {

                    var translatedProperties = matchingProduct.PropertiesAsList[0].TranslatedProperties;
                    for (var j = 0; j < translatedProperties.length; j++) {
                        if (translatedProperties[j].KeyName.match(/ProductName/i)) { // match the string ignoring case
                            productName = translatedProperties[j].ValueString;
                            break;
                        }
                    }
                } else {
                    productName = products[i].ProductName;
                }

                mappedSalesLine.ProductName = productName;
                mappedSalesLine.RelatedProducts = products[i].RelatedProducts;
                mappedSalesLine.Image = products[i].Image;

                if (salesLines.length < countOfRecentPurchasesToLoad) {
                    this.salesLines.push(mappedSalesLine);
                } else {
                    break;
                }

                matchingProduct = null;
            }

            for (var listingId in salesLineByListingId) {
                if (salesLines.length < countOfRecentPurchasesToLoad) {
                    salesLines.push(salesLineByListingId[listingId]);
                } else {
                    break;
                }
            }
            
            // 1. sort products array alphabetically and by date
            var sortedProductsArray = this.sortSalesLines(salesLines);

            // now that we have the most recent purchase, we can grab the place the purchase was made (place of last visit)
            if (sortedProductsArray.length > 0 && !StringExtensions.isEmptyOrWhitespace(sortedProductsArray[0].FulfillmentStoreId)) {
                this.channelManager.getStoreDetailsAsync(sortedProductsArray[0].FulfillmentStoreId)
                    .done((storeDetails: Proxy.Entities.OrgUnit) => {
                        this.placeOfLastVisitString(storeDetails.OrgUnitFullAddress);
                        this.placeOfLastVisitId(sortedProductsArray[0].FulfillmentStoreId);
                    });
            }

            var recentlyPurchasedProductIds: number[] = [];

            // Get the related products of Min(Recent purchased products length, 5)
            for (var i = 0; i < Math.min(sortedProductsArray.length, CustomerDetailsViewModel.RECENT_PURCHASES_FOR_SUGGESTED_PRODUCTS); i++) {
                var relatedProducts: Proxy.Entities.RelatedProduct[] = sortedProductsArray[i].RelatedProducts;

                if (ArrayExtensions.hasElements(relatedProducts)) {
                    relatedProducts.forEach((relatedProduct) => {
                        recentlyPurchasedProductIds.push(relatedProduct.RelatedProductRecordId);
                    });
                }
            }

            return recentlyPurchasedProductIds;
        }

        /**
        *  Collects sales into the same dates and then returns an array alphabetically sorted within each date group
        *
        *  @param {Proxy.Entities.SalesLine[]} unsortedSalesLines - the unsorted array of sales lines to be sorted
        */
        private sortSalesLines(unsortedSalesLines: MappedSalesLine[]): MappedSalesLine[]{
            // group the products by date into separate arrays
            var salesLinesGroupedByDate = {};
            for (var i = 0; i < unsortedSalesLines.length; i++) {
                var salesLine = unsortedSalesLines[i];
                var datePurchased = salesLine.DatePurchased;
                if (Array.isArray(salesLinesGroupedByDate[datePurchased])) {
                    salesLinesGroupedByDate[datePurchased].push(salesLine);
                } else {
                    salesLinesGroupedByDate[datePurchased] = [salesLine];
                }
            }

            // iterate over each group and sort alphabetically, but retain date grouping
            var sortedAlphabeticallyArray = [];
            var unsortedDateArray = [];
            for (var dateGroup in salesLinesGroupedByDate) {
                var sortedArray = <MappedSalesLine[]>salesLinesGroupedByDate[dateGroup].sort(
                    (salesLineA: MappedSalesLine, salesLineB: MappedSalesLine) => {
                        var productNameA: string = salesLineA.ProductName;
                        var productNameB: string = salesLineB.ProductName;
                        if (!StringExtensions.isNullOrWhitespace(productNameB) && productNameB.localeCompare(productNameA) <= 0) {
                            return -1;
                        } else {
                            return 1;
                        }
                    });
                unsortedDateArray.push([dateGroup, sortedArray]);
            }

            // sort by date (olest - > newest) then reverse (newest -> oldest)
            var sortedDateArray = unsortedDateArray.sort().reverse();

            // [[date, [sales lines sorted alphabetically], ...]
            for (var i = 0; i < sortedDateArray.length; i++) {
                var sortedArray: MappedSalesLine[] = sortedDateArray[i][1];
                if (sortedAlphabeticallyArray.length == 0) {
                    sortedAlphabeticallyArray = sortedArray;
                } else {
                    sortedAlphabeticallyArray = sortedAlphabeticallyArray.concat(sortedDateArray[i][1]);
                }
            }

            return <MappedSalesLine[]>sortedAlphabeticallyArray;
        }

        /**
        * Loads the wish list items.
        *
        * @param {number} customerId The account number of the customer.
         * @return {IVoidAsyncResult} The async result.
        */
        public getWishLists(customerId: string): IVoidAsyncResult {
            return this.customerManager.getWishListsByCustomerIdAsync(customerId)
                .done((wishLists: Proxy.Entities.CommerceList[]) => {
                        this.wishLists(wishLists);
                });
        }

        /**
         * Gets the products for a wishlist.
         *
         * @param {Proxy.Entities.CommerceList} wishlist The wishlist to get the products for.
         * @return {IVoidAsyncResult} The async result.
         */
        public getWishListProducts(wishlist: Proxy.Entities.CommerceList): IVoidAsyncResult {
            var productIds = new Array();
            wishlist.CommerceListLines.forEach((value, index) => {
                productIds.push(value.ProductId);
            });

            return this.productManager.getProductDetailsAsync(productIds)
                .done((products: Proxy.Entities.Product[]) => {
                    products = Commerce.ProductPropertiesHelper.ArrangeProducts(productIds, products);
                    this.wishListProducts(products);
                });
        }

        /**
         * Gets all the loyalty cards for the current customer
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public getLoyaltyCards(): IVoidAsyncResult {
            return this.customerManager.getCustomerLoyaltyCardsAsync(this.customerAccountNumber)
                .done((loyaltyCards) => {
                    this.loyaltyCards(loyaltyCards);
                });
        }

        /**
         * Adds the loyalty card to the cart.
         * @param {string} loyatyCardId The loyalty card identifier to be added to the cart.
         * @return {IAsyncResult<ICancelableResult>} The async result.
         */
        public addLoyaltyCardToCart(loyaltyCardId: string): IAsyncResult<ICancelableResult> {
            var options: Operations.IAddLoyaltyCardOperationOptions = { loyaltyCardId: loyaltyCardId };
            return this.operationsManager.runOperation(Operations.RetailOperation.LoyaltyRequest, options);
        }

        /**
         * Gets the customer sales orders.
         *
         * @param {string} accountNumber Customer account number.
         * @param {number} countOfSalesOrdersToLoad Count of Sales Orders for loading in order history.
         * @return {IVoidAsyncResult} The async result.
         */
        public getSalesOrdersByCustomer(
            customerAccountNumber: string,
            countOfSalesOrdersToLoad?: number,
            countOfRecentPurchasesToLoad?: number): IVoidAsyncResult {
            var salesOrders: Proxy.Entities.SalesOrder[];
            var asyncQueue = new AsyncQueue()
                .enqueue(() => {
                    return this.salesOrderManager.getSalesOrdersByCustomerIdAsync(customerAccountNumber, countOfSalesOrdersToLoad)
                        .done((salesOrderByCustomerId) => {
                            salesOrders = salesOrderByCustomerId;
                            this.populateSalesOrders(salesOrders, countOfSalesOrdersToLoad);
                        });
                }).enqueue(() => {
                    return this.populateSuggestedProducts(salesOrders, countOfRecentPurchasesToLoad);
                });

            return asyncQueue.run();
        }

        /**
         * Gets the purchase history for a customer.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of customers to skip.
         * @return {IVoidAsyncResult} The async result.
         */
         public getRecentPurchases(
             pageSize?: number,
             skip?: number): IVoidAsyncResult {
         
             return this.customerManager.getPurchaseHistoryAsync(this.customerAccountNumber, pageSize, skip)
                 .done((purchaseHistoryResults) => {
                     this.recentPurchases(purchaseHistoryResults);
                 }).fail((errors: Model.Entities.Error[]) => {
                     if (errors[0].ErrorCode.toUpperCase() !== ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_REALTIMESERVICENOTSUPPORTED.serverErrorCode) {
                         Commerce.NotificationHandler.displayClientErrors(errors);
                     }
                 });
         }

        /**
         *  Populates an array of all sales orders.
         *
         *  @param {Proxy.Entities.SalesOrder[]} salesOrders - Customer sales orders.
         *  @param {number} countOfSalesOrdersToLoad - Sets how many items are included in the final collection.
         */
        private populateSalesOrders(salesOrders: Proxy.Entities.SalesOrder[], countOfSalesOrdersToLoad?: number) {
            this._totalSalesOrders(salesOrders.length);
            this.displayZeroSalesOrdersText(!ArrayExtensions.hasElements(salesOrders));
            this.displaySalesOrdersGrid(ArrayExtensions.hasElements(salesOrders));
            this.displaySalesOrdersSortByMenu(ArrayExtensions.hasElements(salesOrders));
            this.salesOrders(salesOrders);
            if (!ObjectExtensions.isNullOrUndefined(countOfSalesOrdersToLoad) && countOfSalesOrdersToLoad > 0) {
                this._countOfSalesOrdersToLoad = countOfSalesOrdersToLoad;
            }
            this.sortSalesOrders();
        }

        public sortOrderHistory(sortColumn) {
            if (sortColumn != this._orderHistorySortColumn) {
                this._isOrderHistorySortingDescending = true;
            } else {
                this._isOrderHistorySortingDescending = !this._isOrderHistorySortingDescending;
            }

            this._orderHistorySortColumn = sortColumn;
            this.sortSalesOrders();
        }

        public sortSalesOrders() {
            var sortList = this.salesOrders();
            var directionSign: number = this._isOrderHistorySortingDescending ? -1 : 1;
            switch (this._orderHistorySortColumn) {
                case OrderHistorySortField.Date:
                    sortList = sortList.sort((item1: Proxy.Entities.SalesOrder, item2: Proxy.Entities.SalesOrder) => {
                        return directionSign * NumberExtensions.compare(item1.CreatedDateTime.getTime(), item2.CreatedDateTime.getTime());
                    });
                    this._orderHistorySortColumnName(Commerce.ViewModelAdapter.getResourceString("string_244"));
                    break;
                case OrderHistorySortField.Number:
                    sortList = sortList.sort((item1: Proxy.Entities.SalesOrder, item2: Proxy.Entities.SalesOrder) => {
                        return directionSign * StringExtensions.compare(item1.ReceiptId, item2.ReceiptId);
                    });
                    this._orderHistorySortColumnName(Commerce.ViewModelAdapter.getResourceString("string_245"));
                    break;
                case OrderHistorySortField.Total:
                    sortList = sortList.sort((item1: Proxy.Entities.SalesOrder, item2: Proxy.Entities.SalesOrder) => {
                        return directionSign * NumberExtensions.compare(item1.TotalAmount, item2.TotalAmount);
                    });
                    this._orderHistorySortColumnName(Commerce.ViewModelAdapter.getResourceString("string_246"));
                    break;
            }

            this.salesOrders(sortList);

            if (this.salesOrders().length == this.salesOrdersToDisplay().length) {
                this.salesOrdersToDisplay(this.salesOrders());
            } else {
                this.salesOrdersToDisplay(this.salesOrders().slice(0, this._countOfSalesOrdersToLoad));
            }
        }

        /**
         *  Check whether the affiliation is already associated Customer or not.
         *
         *  @param {number} affiliationId The primary key of the affiliation.
         *  @return {boolean} return true if the affiliation is associated Customer, otherwise false.
         */
        public isAlreadyInCustomer(affiliationId: number): boolean {
            if (ArrayExtensions.hasElements(this.customerAffiliations())) {
                return this.customerAffiliations().some(
                    (value, index, array) => { return value.RetailAffiliationId === affiliationId; });
            }

            return false;
        }

        /**
         * Add affiliation to customer.
         *
         * @param {Proxy.Entities.CustomerAffiliation} selectedCustomerAffiliation The customer's affiliation.
         */
        public addAffiliationToCustomer(selectedCustomerAffiliation: Proxy.Entities.CustomerAffiliation): IVoidAsyncResult {
            var updatedCustomerAffiliations: Proxy.Entities.CustomerAffiliation[] = [];

            // Add the seleted affiliations to the updatedCustomerAffiliations collection.
            updatedCustomerAffiliations.push(new Proxy.Entities.CustomerAffiliationClass(selectedCustomerAffiliation));

            // Copy the customer's affiliations collection to the updatedCustomerAffiliations collection.
            this.customerAffiliations().forEach((affiliation: Proxy.Entities.CustomerAffiliation) => {
                updatedCustomerAffiliations.push(affiliation);
            });

            return this.updateCustomerAffiliations(updatedCustomerAffiliations);
        }

        /**
         * Remove affiliations from customer.
         *
         * @param {number[]} deletedCustomerAffiliationKeys The removed customer affiliation keys.
         * @return {IVoidAsyncResult} The async result.
         */
        public deleteAffiliationsFromCustomer(deletedCustomerAffiliationKeys: number[]): IVoidAsyncResult {
            if (!ArrayExtensions.hasElements(deletedCustomerAffiliationKeys)) {
                return VoidAsyncResult.createResolved();
            }

            var updatedCustomerAffiliations: Proxy.Entities.CustomerAffiliation[] = [];

            // Add no deleted customer affiliations to the updatedCustomerAffiliations collection.
            this.customerAffiliations().forEach((affiliation: Proxy.Entities.CustomerAffiliation) => {
                if (!deletedCustomerAffiliationKeys.some((value, index, array) => { return value === affiliation.RecordId; })) {
                    updatedCustomerAffiliations.push(affiliation);
                }
            });

            return this.updateCustomerAffiliations(updatedCustomerAffiliations);
        }

        /**
         * Updates customer affiliations.
         *
         * @param {Proxy.Entities.CustomerAffiliation[]} customerAffiliations The customer's affiliations.
         * @return {IvoidAsyncResult} The async result.
         */
        private updateCustomerAffiliations(customerAffiliations: Proxy.Entities.CustomerAffiliation[]): IVoidAsyncResult {
            var customer: Proxy.Entities.Customer = <Proxy.Entities.Customer>ObjectExtensions.unwrapObservableProxyObject(this.Customer());
            var customerAddress: Proxy.Entities.Address = new Proxy.Entities.AddressClass();

            if (!ObjectExtensions.isNullOrUndefined(this.customerAddress())) {
                customerAddress = <Proxy.Entities.Address>ObjectExtensions.unwrapObservableProxyObject(this.customerAddress());
            }

            if (ObjectExtensions.isNullOrUndefined(customer.Addresses)) {
                customer.Addresses = this.customerManager.validateCustomerAddress(customerAddress).length == 0 ? [<Proxy.Entities.Address>customerAddress] : [];
            } else {
                var matchingAddress = customer.Addresses.filter(a => a.RecordId == customerAddress.RecordId);
                if (matchingAddress.length > 0) {
                    matchingAddress[0] = customerAddress;
                } else {
                    customer.Addresses.push(customerAddress);
                }
            }

            customer.CustomerAffiliations = customerAffiliations;

            var accountNumber = customer.AccountNumber;
            var asyncQueue = new AsyncQueue()
                .enqueue(() => {
                    return this.customerManager.updateCustomerAsync(customer);
                }).enqueue(() => {
                    return this.loadCustomer(accountNumber);
                });

            if (accountNumber === Session.instance.cart.CustomerId) {
               var cartManager: Model.Managers.ICartManager = Model.Managers.Factory.GetManager(Model.Managers.ICartManagerName, null);

               asyncQueue.enqueue(() => {

                   var cart: Proxy.Entities.Cart = Session.instance.cart;
                   // gets affiliations without customer affiliations
                   var updatedCartAffiliations = cart.AffiliationLines.filter((value, index, array) => { return value.CustomerId !== accountNumber; });

                   customerAffiliations.forEach((customerAffiliation: Proxy.Entities.CustomerAffiliation) => {
                       var affiliationLoyaltyTier: Proxy.Entities.AffiliationLoyaltyTier = new Proxy.Entities.AffiliationLoyaltyTierClass();
                       affiliationLoyaltyTier.AffiliationId = customerAffiliation.RetailAffiliationId;
                       affiliationLoyaltyTier.CustomerId = accountNumber;
                       affiliationLoyaltyTier.LoyaltyTierId = 0;
                       updatedCartAffiliations.push(affiliationLoyaltyTier);
                   })

                    var newCart: Proxy.Entities.Cart = {
                        Id: cart.Id,
                        AffiliationLines: updatedCartAffiliations
                    }
                    return cartManager.createOrUpdateCartAsync(newCart);
                });
            }

            return asyncQueue.run();
        }

        private populateCustomerAffiliations(customerAffiliations: Proxy.Entities.CustomerAffiliation[]): void {
            this.customerAffiliations(customerAffiliations);
            this.customerAffiliationsToDisplay(
                !ObjectExtensions.isNullOrUndefined(this.customerAffiliations())
                ? this.customerAffiliations().slice(0, CustomerDetailsViewModel.CUSTOMER_AFFILIATIONS_FOR_INITIAL_DISPLAY)
                : new Array<Proxy.Entities.CustomerAffiliation>()
                );
            this.isCustomerAffiliationsTextVisible(!ArrayExtensions.hasElements(this.customerAffiliations()));
            this.isCustomerAffiliationsGridVisible(ArrayExtensions.hasElements(this.customerAffiliations()));
        }
    }
}
