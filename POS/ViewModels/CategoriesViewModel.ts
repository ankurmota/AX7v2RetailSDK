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

    export class CategoriesViewModel extends ViewModelBase {
        Categories: ObservableArray<Commerce.Model.Entities.Category>;
        ParentCategoryName: Observable<string>;
        RootCategoryName: Observable<string>;
        RootCategoryRecordId: Observable<number>;

        constructor() {
            super();
            this.RootCategoryName = ko.observable(Commerce.ViewModelAdapter.getResourceString("string_606"));
            this.RootCategoryRecordId = ko.observable(0);
            this.Categories = ko.observableArray<Commerce.Model.Entities.Category>([]);
        }

        public load() {
            this.GetCategoriesAsync();
        }

        public GetCategoriesAsync(skip?: number, take?: number): IAsyncResult<Model.Entities.Category[]> {
            return this.productManager.getCategoriesAsync(Commerce.Session.instance.productCatalogStore.Context.ChannelId, skip, take)
                .done((result) => { this.GetCategoriesSuccessCallBack(result); });
        }

        public GetCategoriesSuccessCallBack(categories: Model.Entities.Category[]) {
            var items = categories.filter((c, i, a) => {
                return c.ParentCategory == 0;
            })

            if (ArrayExtensions.hasElements(items) && !ObjectExtensions.isNullOrUndefined(items[0].RecordId)) {
                this.RootCategoryRecordId(items[0].RecordId);
            }

            this.Categories(categories);
        }

        public GetProductsByCategoryAsync(
            categoryId: number,
            includeDescendantCategories: boolean,
            skip?: number,
            take?: number): IAsyncResult<Model.Entities.Product[]> {
            return this.productManager.getProductsByCategoryAsync(
                Commerce.Session.instance.productCatalogStore.Context,
                categoryId,
                includeDescendantCategories,
                skip,
                take);
        }

        public GetChildCategoriesAsync(categoryId: number, skip?: number, take?: number): IAsyncResult<Model.Entities.Category[]> {
            return this.productManager.getChildCategoriesAsync(Commerce.Session.instance.productCatalogStore.Context.ChannelId, categoryId, skip, take);
        }
    }
}