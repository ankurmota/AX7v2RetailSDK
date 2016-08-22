/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.ViewModels.d.ts'/>
///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {

    "use strict";
    export class CategoriesViewController extends ViewControllerBase {
        private ViewModel: Commerce.ViewModels.CategoriesViewModel;
        public IndeterminateWaitVisible: Observable<boolean>;
        public commonHeaderData;

        constructor(options) {
            super(true);

            this.IndeterminateWaitVisible = ko.observable(true);
            this.ViewModel = new Commerce.ViewModels.CategoriesViewModel();
            this.ViewModel.GetCategoriesSuccessCallBack(Commerce.Session.instance.CurrentCategoryList);
            this.IndeterminateWaitVisible(false);
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();

            //Load Common Header
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);

            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_609"));//PRODUCTS
            this.commonHeaderData.enableVirtualCatalogHeader();
            this.commonHeaderData.categoryName(this.ViewModel.RootCategoryName());

            RetailLogger.viewsMerchandisingCategoriesViewLoaded();
        }

        private itemInvokedHandler(item) {
            Commerce.ViewModelAdapter.navigate("ProductsView", <IProductsViewOptions>{ category: item, activeMode: Commerce.ViewModels.ProductsViewModelActiveMode.Products });
        }

        private categoriesCommand(options: any) {
            Commerce.ViewModelAdapter.navigate("ProductsView", <IProductsViewOptions>{ category: options.SelectedGroupHeader, activeMode: Commerce.ViewModels.ProductsViewModelActiveMode.Products });
        }
    }
}