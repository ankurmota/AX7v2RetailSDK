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

    SelectHardwareStationActivity.prototype.execute = function (): IVoidAsyncResult {
        var self = <SelectHardwareStationActivity>this;
        var asyncResult = new VoidAsyncResult(null);
        var onActiveAsyncResult = new AsyncResult<Operations.ISelectHardwareStationOperationOptions>(null);
        var viewOptions: ViewControllers.HardwareStationViewControllerOptions = { onActiveInactive: onActiveAsyncResult, activeOnly: self.context.activeOnly };

        ViewModelAdapter.navigate("HardwareStationView", viewOptions);
        callResponseHandler(self, viewOptions, asyncResult);

        return asyncResult;
    };

    /**
     * Calls the response handler if any and resolves the activity result.
     */
    function callResponseHandler(
        activity: SelectHardwareStationActivity,
        viewOptions: ViewControllers.HardwareStationViewControllerOptions,
        asyncResult: VoidAsyncResult) {

        viewOptions.onActiveInactive.done((result) => {
            if (result) {
                if (result.isInactivate && activity.context.activeOnly) {
                    // we need to clear the result and attach to done again
                    viewOptions.onActiveInactive.clear();
                    viewOptions.onActiveInactive.done((result) => { callResponseHandler(activity, viewOptions, asyncResult); });
                    return;
                }

                activity.response = result;

                if (activity.responseHandler) {
                    activity.responseHandler(activity.response)
                        .done(() => {
                            // clear the async result from listening to view
                            viewOptions.onActiveInactive = null;
                            asyncResult.resolve();
                        }).fail((errors) => {
                            NotificationHandler.displayClientErrors(errors);

                            // we need to clear the onActive result and attach to done again
                            viewOptions.onActiveInactive.clear();
                            viewOptions.onActiveInactive.done((result) => { callResponseHandler(activity, viewOptions, asyncResult); });
                        });
                } else {
                    asyncResult.resolve();
                }
            } else {
                asyncResult.resolve();
            }
        });
    };
}