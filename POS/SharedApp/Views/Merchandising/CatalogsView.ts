/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.ViewModels.d.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    /**
     * CatalogsViewController constructor options.
     */
    export interface ICatalogsViewControllerOptions {
        destination: string;
        destinationOptions: any;
    }

    export class CatalogsViewController extends ViewControllerBase {

        private _channelManager: Commerce.Model.Managers.IChannelManager;
        private _options: ICatalogsViewControllerOptions;

        public catalogViewModel: Commerce.ViewModels.CatalogViewModel;
        public currentStore: Model.Entities.ProductCatalogStore;
        public currentStoreLocation: Observable<string>;

        public IndeterminateWaitVisible: Observable<boolean>;
        public commonHeaderData;

        constructor(options: ICatalogsViewControllerOptions) {
            super(true);

            this._options = options;
            this.catalogViewModel = new Commerce.ViewModels.CatalogViewModel();
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.backButtonVisible(false);
            this.commonHeaderData.enableVirtualCatalogHeader();
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_600"));

            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_32")); //"Catalogs"
            this.currentStore = Commerce.Session.instance.productCatalogStore.Store;
            this.IndeterminateWaitVisible = ko.observable(false);
            this.currentStoreLocation = ko.observable("");

            if (Commerce.Session.instance.productCatalogStore.StoreType != Model.Entities.StoreButtonControlType.Warehouse) {
                if (!ObjectExtensions.isNullOrUndefined(Commerce.Session.instance.productCatalogStore.Store) &&
                    !ObjectExtensions.isNullOrUndefined(Commerce.Session.instance.productCatalogStore.Store.OrgUnitFullAddress)) {
                    this.currentStoreLocation(Commerce.Session.instance.productCatalogStore.Store.OrgUnitAddress.City + ", " + Commerce.Session.instance.productCatalogStore.Store.OrgUnitAddress.State);
                }

                this.IndeterminateWaitVisible(true);
                this.catalogViewModel.getCatalogs()
                    .done((productCatalogs: Commerce.Model.Entities.ProductCatalog[]) => {
                        this.IndeterminateWaitVisible(false);
                    }).fail((errors: Model.Entities.Error[]) => {
                        this.displayErrors(errors);
                    });
            } else {
                var allStoreProductImage = Commerce.Session.instance.defaultCatalogImageFormat
                    .replace("{LanguageId}", ApplicationContext.Instance.deviceConfiguration.CultureName)
                    .replace("{CatalogName}", Commerce.ViewModelAdapter.getResourceString("string_33"))
                    .replace("{ChannelName}", Commerce.Session.instance.productCatalogStore.Store.OrgUnitName);
                this.catalogViewModel.catalogs([new Model.Entities.ProductCatalogClass({
                    RecordId: 0,
                    Name: Commerce.ViewModelAdapter.getResourceString("string_33"),
                    IsSnapshotEnabled: false,
                    ValidFrom: new Date(),
                    ValidTo: new Date(),
                    CreatedOn: new Date(),
                    ModifiedOn: new Date(),
                    PublishedOn: new Date(),
                    Image: { Items: [{ Url: allStoreProductImage }] }
                })]);
            }
        }

        private ItemInvokedHandler(item) {
            RetailLogger.viewsMerchandisingCatalogsCatalogClicked(item.RecordId.toString(), item.Name);
            Commerce.Session.instance.productCatalogStore.Context.CatalogId = item.RecordId;
            Commerce.Session.instance.catalogName = item.Name;

            if (!Commerce.ObjectExtensions.isNullOrUndefined(this._options) && !Commerce.StringExtensions.isNullOrWhitespace(this._options.destination)) {
                Commerce.ViewModelAdapter.navigate(this._options.destination, this._options.destinationOptions);
            }
            else {
                Commerce.ViewModelAdapter.navigate("CategoriesView");
            }
        }

        private switchToOtherStores() {
            var self: CatalogsViewController = this;
            var parameters: IPickUpInStoreViewControllerOptions = {
                isForPickUp: false,
                callerPage: "CatalogsView",
                storeSelectionCallback: (store: Model.Entities.OrgUnit): IAsyncResult<ICatalogsViewControllerOptions> => {
                    return self.catalogViewModel.setVirtualCatalog(Model.Entities.StoreButtonControlType.FindStore, store.OrgUnitNumber, store)
                        .map((): ICatalogsViewControllerOptions => {
                            return this._options || { destination: null, destinationOptions: null };
                        });
                }
            };

            Commerce.ViewModelAdapter.navigate(
                "PickUpInStoreView", parameters);
        }

        private switchToCurrentStore() {
            this.IndeterminateWaitVisible(true);
            this.catalogViewModel.setVirtualCatalog(Model.Entities.StoreButtonControlType.CurrentStore, null, null)
                .done(() => {
                    this.IndeterminateWaitVisible(false);
                    Commerce.ViewModelAdapter.navigate("CatalogsView", this._options);
                })
                .fail((errors: Model.Entities.Error[]) => {
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        private switchToAllStoreProducts() {
            this.IndeterminateWaitVisible(true);
            this.catalogViewModel.setVirtualCatalog(Model.Entities.StoreButtonControlType.Warehouse, null, null)
                .done(() => {
                    this.IndeterminateWaitVisible(false);
                    Commerce.ViewModelAdapter.navigate("CatalogsView", this._options);
                })
                .fail((errors: Model.Entities.Error[]) => {
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        private navigateToStoreDetails() {
            Commerce.ViewModelAdapter.navigate("StoreDetailsView", { StoreId: Commerce.Session.instance.productCatalogStore.Store.OrgUnitNumber });
        }

        private displayErrors(errors: Model.Entities.Error[]): void {
            this.IndeterminateWaitVisible(false);
            NotificationHandler.displayClientErrors(errors);
        }
    }
}