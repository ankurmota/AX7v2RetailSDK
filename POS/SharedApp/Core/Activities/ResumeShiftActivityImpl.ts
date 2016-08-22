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

    ResumeShiftActivity.prototype.execute = function (): IVoidAsyncResult {
        var asyncResult: VoidAsyncResult = new VoidAsyncResult(null);
        var viewOptions: ViewControllers.IResumeShiftViewControllerOptions;

        var resolveResult: () => void = (): void => {
            viewOptions.onShiftSelected = null;
            asyncResult.resolve();
            // Navigate back if not high-jacked.
            if (Commerce.ViewModelAdapter.isInView("ResumeShiftView")) {
                ViewModelAdapter.navigateBack();
            }
        };

        viewOptions = {
            availableShiftActions: this.context.availableShiftActions,
            onShiftSelected: (shift: Proxy.Entities.Shift): IVoidAsyncResult => {
                this.response = shift;

                if (this.responseHandler) {
                    return this.responseHandler(this.response)
                        .done(() => {
                            resolveResult();
                        }).fail((errors: Proxy.Entities.Error[]) => {
                            NotificationHandler.displayClientErrors(errors);
                        });
                } else {
                    resolveResult();
                    return VoidAsyncResult.createResolved();
                }
            }
        };

        ViewModelAdapter.navigate("ResumeShiftView", viewOptions);

        return asyncResult;
    };
}