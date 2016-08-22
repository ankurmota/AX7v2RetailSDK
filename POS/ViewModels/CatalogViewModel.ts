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

    export class CatalogViewModel extends ViewModelBase {
        public catalogs: ObservableArray<Commerce.Model.Entities.ProductCatalog>;
        private _channelId: number;

        constructor() {
            super();
            this.catalogs = ko.observableArray<Commerce.Model.Entities.ProductCatalog>([]);
        }

        /**
         * Gets all of the catalogs for the current channel context.
         */
        public getCatalogs(): IAsyncResult<Model.Entities.ProductCatalog[]> {
            this._channelId = Commerce.Session.instance.productCatalogStore.Context.ChannelId;
            return this.productManager.getCatalogsAsync(this._channelId)
                .done((result) => {
                    var allStoreProductImage = Commerce.Session.instance.defaultCatalogImageFormat
                        .replace("{LanguageId}", ApplicationContext.Instance.deviceConfiguration.CultureName)
                        .replace("{CatalogName}", Commerce.ViewModelAdapter.getResourceString("string_33"))
                        .replace("{ChannelName}", Commerce.Session.instance.productCatalogStore.Store.OrgUnitName);

                    // Add 'All products' catalog if we are browsing the current store.
                    result.splice(0, 0, new Model.Entities.ProductCatalogClass({
                        RecordId: 0,
                        Name: Commerce.ViewModelAdapter.getResourceString("string_33"),
                        IsSnapshotEnabled: false,
                        ValidFrom: new Date(),
                        ValidTo: new Date(),
                        CreatedOn: new Date(),
                        ModifiedOn: new Date(),
                        PublishedOn: new Date(),
                        Image: { Items: [{ Url: allStoreProductImage }] }
                    }));
                    this.catalogs(result);
                });
        }

        public getCategories(): IVoidAsyncResult {
            return this.productManager.getCategoriesAsync(Commerce.Session.instance.productCatalogStore.Context.ChannelId)
                .done((result) => { Commerce.Session.instance.CurrentCategoryList = result; });
        }

        /**
         * Set virtual catalog to be able to browse or search products / catalogs from other stores or warehouse.
         *
         * @param {Model.Entities.StoreButtonControlType} catalogType The catalog store type.
         * @param {string} [storeNumber] The optional parameter of store number.
         * @param {Model.Entities.OrgUnit} [storeEntity] The optional parameter of store entity.
         * @return {IVoidAsyncResult} The async result.
         */
        public setVirtualCatalog(catalogType: Model.Entities.StoreButtonControlType, storeNumber?: string, storeEntity?: Model.Entities.OrgUnit): IVoidAsyncResult {
            var asyncResult = new VoidAsyncResult();
            Session.instance.productCatalogStore.StoreType = catalogType;

            switch (catalogType) {
                case Model.Entities.StoreButtonControlType.AllStores:
                    Session.instance.productCatalogStore.Context = { ChannelId: 0, CatalogId: 0 };

                    var warehouseDetails: Model.Entities.OrgUnit = new Model.Entities.OrgUnitClass();
                    warehouseDetails.OrgUnitNumber = ViewModelAdapter.getResourceString("string_5503");
                    warehouseDetails.OrgUnitName = warehouseDetails.OrgUnitNumber;
                    warehouseDetails.OrgUnitAddress = { Name: warehouseDetails.OrgUnitNumber };
                    Session.instance.productCatalogStore.Store = warehouseDetails;

                    asyncResult.resolve();
                    break;
                case Model.Entities.StoreButtonControlType.Warehouse:
                    Session.instance.productCatalogStore.Context = { ChannelId: 0, CatalogId: 0 };

                    var warehouseDetails: Model.Entities.OrgUnit = new Model.Entities.OrgUnitClass();
                    warehouseDetails.OrgUnitNumber = ViewModelAdapter.getResourceString("string_5504");
                    warehouseDetails.OrgUnitName = warehouseDetails.OrgUnitNumber;
                    warehouseDetails.OrgUnitAddress = { Name: warehouseDetails.OrgUnitNumber };
                    Session.instance.productCatalogStore.Store = warehouseDetails;
                    Commerce.ApplicationContextLoader.loadCategories(true);
                    asyncResult.resolve();
                    break;
                case Model.Entities.StoreButtonControlType.CurrentStore:
                    Session.instance.productCatalogStore.Store = <Model.Entities.OrgUnit>ObjectExtensions.clone(ApplicationContext.Instance.storeInformation);
                    Session.instance.productCatalogStore.Context = { ChannelId: ApplicationContext.Instance.storeInformation.RecordId, CatalogId: 0 };
                    Commerce.Session.instance.catalogName = Commerce.ViewModelAdapter.getResourceString("string_33");
                    Commerce.ApplicationContextLoader.loadCategories(true);
                    asyncResult.resolve();
                    break;
                case Model.Entities.StoreButtonControlType.FindStore:
                    if (!ObjectExtensions.isNullOrUndefined(storeEntity)) {
                        Session.instance.productCatalogStore.Context = { ChannelId: storeEntity.RecordId, CatalogId: 0 };
                        Session.instance.productCatalogStore.Store = storeEntity;
                        Commerce.Session.instance.catalogName = Commerce.ViewModelAdapter.getResourceString("string_33");
                        Commerce.ApplicationContextLoader.loadCategories(true);
                        asyncResult.resolve();
                    }
                    else {
                        this.channelManager.getStoreDetailsAsync(storeNumber)
                            .done((storeDetails: Model.Entities.OrgUnit) => {
                                Session.instance.productCatalogStore.Context = { ChannelId: storeDetails.RecordId, CatalogId: 0 };
                                Session.instance.productCatalogStore.Store = storeDetails;
                                asyncResult.resolve();
                            })
                            .fail((errors: Model.Entities.Error[]) => {
                                asyncResult.reject(errors);
                            });
                    }
                    break;
                default:
                    throw "Unrecognized StoreButtonControlType enum: " + catalogType;
            }

            return asyncResult;
        }
    }
}