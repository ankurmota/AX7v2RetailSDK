/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Core/DefaultButtonGridHandler.ts'/>
///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../IKeepAliveView.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    /**
     * Options passed to the HomeViewController.
     */
    export interface IHomeViewOptions {
    }

    export class HomeViewController extends ViewControllerBase implements IKeepAliveView<any> {
        public viewModel: ViewModels.HomeViewModel;
        public catalogViewModel: ViewModels.CatalogViewModel;
        public indeterminateWaitVisible: Observable<boolean>;
        public backgroundImageEncodingURL: Computed<string>;
        public backgroundImageEncodingSrc: Computed<string>;
        public commonHeaderData: Controls.CommonHeaderData;

        constructor(options: IHomeViewOptions) {
            super(true /* saveInHistory */);

            this.indeterminateWaitVisible = ko.observable(false);
            this.viewModel = new ViewModels.HomeViewModel();
            this.catalogViewModel = new ViewModels.CatalogViewModel();

            this.commonHeaderData = new Controls.CommonHeaderData();
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(false);
            this.commonHeaderData.viewSearchBox(false);
            this.commonHeaderData.backButtonVisible(false);
            this.commonHeaderData.viewHeader(false);

            /* must use single quotes here, do not change to double quotes */
            this.backgroundImageEncodingURL = ko.computed(() => {
                return 'url(data:image/png;base64,' + this.viewModel.base64ImageData() + ')';
            }, this);
            this.backgroundImageEncodingSrc = ko.computed(() => {
                return 'data:image/png;base64,' + this.viewModel.base64ImageData();
            }, this);
        }

        /**
         * Set options during navigaton for IKeepAliveView.
         * @param {ICartViewControllerOptions} options The view options.
         */
        public keepAliveViewActivated(options: any): void {
            if (Session.instance.productCatalogStore.StoreType != Proxy.Entities.StoreButtonControlType.CurrentStore) {
                var asyncResult: IVoidAsyncResult = this.catalogViewModel.setVirtualCatalog(Proxy.Entities.StoreButtonControlType.CurrentStore, null, null);
                this.handleAsyncResult(asyncResult);
            }
        }

        /**
         * click handle delegation for button grid buttons.
         * @param {number} action The view options.
         * @param {string} actionProperty The extra parameters for operation.
         */
        public buttonGridClick(operationId: number, actionProperty: string): boolean {
            RetailLogger.viewsHomeTileClick(operationId.toString());

            // Overridden button grid clicks
            switch (operationId) {
                case Operations.RetailOperation.AddAffiliation:
                    if (!StringExtensions.isNullOrWhitespace(actionProperty)) {
                        var affiliationNames: string[] = actionProperty.split(";");
                        var options: Operations.IAddAffiliationOperationOptions = { affiliationNames: affiliationNames, affiliations: [] };
                        var operationResult: IAsyncResult<ICancelableResult> = Operations.OperationsManager.instance.runOperation(operationId, options);
                    } else {
                        this.indeterminateWaitVisible(false);
                        ViewModelAdapter.navigate("CartView");
                    }
                    return true;

                default:
                    return Operations.DefaultButtonGridHandler.handleOperation(operationId, actionProperty, this.indeterminateWaitVisible);
            }
        }

        private handleAsyncResult<T>(asyncResult: IAsyncResult<T>): IAsyncResult<T> {
            this.indeterminateWaitVisible(true);
            return asyncResult
                .always((): void => { this.indeterminateWaitVisible(false); })
                .fail((errors: Proxy.Entities.Error[]) => { NotificationHandler.displayClientErrors(errors); });
        }
    }
};