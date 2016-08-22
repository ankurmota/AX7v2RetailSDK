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

    /**
     * Represents the product search view model.
     */
    export class ProductSearchViewModel extends ViewModelBase {
        public products: ObservableArray<Entities.Product>;
        public searchTextParameter: string;
        public productRefinerValuesParameter: Entities.ProductRefinerValue[];
        public offlineProductImages: Commerce.Dictionary<Observable<Entities.MediaBlob>>;
        public hasProductSearchResults: Observable<boolean>;

        constructor() {
            super();

            this.products = ko.observableArray<Entities.Product>([]);
            this.offlineProductImages = new Commerce.Dictionary<Observable<Entities.MediaBlob>>();
            this.hasProductSearchResults = ko.observable(false);
        }

        /**
         * Clears all search parameters (including refiners).
         */
        public clearParameters(): void {
            this.searchTextParameter = null;
            this.productRefinerValuesParameter = null;
        }

        /**
         * Searches and returns product search results matching search text and refiners (if set).
         * @param {number} pageSize Number of records per page.
         * @param {number} skip Number of records to be skipped.
         * @return {IAsyncResult<Entities.ProductSearchResult[]>} The async result.
         */
        public searchProducts(pageSize: number, skip: number): IAsyncResult<Entities.ProductSearchResult[]> {
            if (StringExtensions.isNullOrWhitespace(this.searchTextParameter)) {
                return AsyncResult.createResolved<Entities.ProductSearchResult[]>([]);
            }

            var channelId: number = Commerce.Session.instance.productCatalogStore.Context.ChannelId;
            var catalogId: number = Commerce.Session.instance.productCatalogStore.Context.CatalogId;
            var searchResults: Entities.ProductSearchResult[];
            var asyncQueue: AsyncQueue = new AsyncQueue();

            asyncQueue.enqueue((): IAsyncResult<Entities.ProductSearchResult[]> => {
                var asyncSearchResults: IAsyncResult<Entities.ProductSearchResult[]>;

                if (!ArrayExtensions.hasElements(this.productRefinerValuesParameter)) {
                    asyncSearchResults = this.productManager.searchByTextAsync(
                        this.searchTextParameter,
                        channelId,
                        catalogId,
                        pageSize,
                        skip);
                } else {
                    asyncSearchResults = this.productManager.refineSearchByTextAsync(
                        this.searchTextParameter,
                        this.productRefinerValuesParameter,
                        channelId,
                        catalogId,
                        pageSize,
                        skip);
                }

                return asyncSearchResults.done((results: Entities.ProductSearchResult[]): void => {
                    // The price for ProductSearchResult is being deprecated and it will be later populated by GetActivePrice.
                    // It's set to null so it won't show at the product search result grid.
                    results.forEach((result: Entities.ProductSearchResult) => { result.Price = null; });
                    searchResults = results;
                });
            });

            return asyncQueue.run()
                .map(() => {
                    // If no skip is specified, then it is the first page requested. 
                    // So, in the first page, if there are no results, then there won't be any results at all.
                    if (NumberExtensions.isNullOrZero(skip)) {
                        this.hasProductSearchResults(
                            ArrayExtensions.hasElements(searchResults) || ArrayExtensions.hasElements(this.productRefinerValuesParameter));
                    }
                    return searchResults;
                })
                .fail((errors: Entities.Error[]): void => {
                    RetailLogger.viewModelProductSearchViewModelSearchProductsByTextFailed(
                        this.searchTextParameter,
                        JSON.stringify(this.productRefinerValuesParameter),
                        JSON.stringify(errors));
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        /**
         * Get product refiners using current product search details.
         * @return {IAsyncResult<Entities.ProductRefiner[]>} The async result.
         */
        public getRefiners(): IAsyncResult<Entities.ProductRefiner[]> {
            //  Refiners are available only for current store.
            if ((Commerce.ApplicationContext.Instance.storeInformation.RecordId === Commerce.Session.instance.productCatalogStore.Store.RecordId) &&
                !StringExtensions.isNullOrWhitespace(this.searchTextParameter)) {

                return this.productManager.getRefinersByTextAsync(
                    this.searchTextParameter,
                    Commerce.Session.instance.productCatalogStore.Context.CatalogId)
                    .fail((errors: Proxy.Entities.Error[]): void => {
                        RetailLogger.viewModelProductSearchViewModelGetRefinersByTextFailed(this.searchTextParameter, JSON.stringify(errors));
                        NotificationHandler.displayClientErrors(errors);
                    });
            }

            return AsyncResult.createResolved([]);
        }

        /**
         * Get product refiner values for the specified refiner using the current product search details.
         * @param {Entities.ProductRefiner[]} refiner The refiner.
         * @return {IAsyncResult<Entities.ProductRefinerValue[]>} The async result.
         */
        public getRefinerValues(productRefiner: Entities.ProductRefiner): IAsyncResult<Entities.ProductRefinerValue[]> {
            if (ObjectExtensions.isNullOrUndefined(productRefiner)) {
                return AsyncResult.createResolved([]);
            }

            return this.productManager.getRefinerValuesByTextAsync(
                this.searchTextParameter,
                productRefiner.RecordId,
                productRefiner.SourceValue,
                Commerce.Session.instance.productCatalogStore.Context.CatalogId)
                .fail((errors: Proxy.Entities.Error[]): void => {
                    RetailLogger.viewModelProductSearchViewModelGetRefinerValuesByTextFailed(
                        this.searchTextParameter, productRefiner.RecordId,
                        productRefiner.SourceValue,
                        JSON.stringify(errors));
                    NotificationHandler.displayClientErrors(errors);
                });
        }
    }
}