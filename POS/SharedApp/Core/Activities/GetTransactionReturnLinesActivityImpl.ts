/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.Core.d.ts'/>

module Commerce.Activities {
    "use strict";

    import ISearchReceiptsReturnLineData = ViewControllers.ISearchReceiptsReturnLineData;

    GetTransactionReturnLinesActivity.prototype.execute = function (): IVoidAsyncResult {
        var self: GetTransactionReturnLinesActivity = <GetTransactionReturnLinesActivity>(this);

        var asyncResult: VoidAsyncResult = new VoidAsyncResult(null);
        var onReturnSalesOrderSalesLines: AsyncResult<ISearchReceiptsReturnLineData> = new AsyncResult<ISearchReceiptsReturnLineData>(null);
        var getTransactionReturnLinesActivityContext: IGetTransactionReturnLinesActivityContext = <IGetTransactionReturnLinesActivityContext> this.context;
        var processing: Observable<boolean> = ko.observable(false);
        var viewOptions: ViewControllers.ISearchReceiptsViewControllerOptions = {
            salesOrderToReturn: getTransactionReturnLinesActivityContext.salesOrder,
            onReturnSalesOrderSalesLines: onReturnSalesOrderSalesLines,
            processing: processing
        };
        callResponseHandler(self, viewOptions, asyncResult);
        ViewModelAdapter.navigate("SearchReceiptsView", viewOptions);

        return asyncResult;
    };

     /**
      * Sets the response handler for the activity on the view.
      * @param {GetTransactionReturnLinesActivity} activity The activity that will handle the response.
      * @param {ViewControllers.SearchReceiptsViewControllerOptions} viewOptions The view to set the response handle to call.
      * @param {VoidAsyncResult} asyncResult The result to call to notify the calling code of the activity that the activity has completed.
      */
    function callResponseHandler(
        activity: GetTransactionReturnLinesActivity,
        viewOptions: ViewControllers.ISearchReceiptsViewControllerOptions,
        asyncResult: VoidAsyncResult): void {

        viewOptions.onReturnSalesOrderSalesLines.done((result: ISearchReceiptsReturnLineData) => {
            if (result) {
                activity.response = { salesOrder: result.salesOrder, salesLines: result.salesLines };

                if (activity.responseHandler) {
                    viewOptions.processing(true);
                    activity.responseHandler(activity.response)
                        .done(() => {
                        // clear the async result from listening to view
                        viewOptions.onReturnSalesOrderSalesLines = null;
                        asyncResult.resolve();
                    }).fail((errors: Model.Entities.Error[]) => {
                        NotificationHandler.displayClientErrors(errors);

                        // we need to clear the onReturnSalesOrderSalesLines result and attach to the asyncResult done again as it is cleared out when called
                        viewOptions.onReturnSalesOrderSalesLines.clear();
                        viewOptions.onReturnSalesOrderSalesLines.done((result: ISearchReceiptsReturnLineData) => {
                            callResponseHandler(activity, viewOptions, asyncResult);
                        });
                    }).always(() => {
                        viewOptions.processing(false);
                    });
                } else {
                    asyncResult.resolve();
                }
            } else {
                activity.response = null;
                asyncResult.resolve();
            }
        });
    };
}