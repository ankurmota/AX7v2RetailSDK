/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Views/Controls/WeighItemDialog.ts'/>
///<reference path='ModalDialogHelper.ts'/>

module Commerce.Activities {
    "use strict";

    GetCartLineWeightActivity.prototype.execute = function (): IVoidAsyncResult {
        var self: GetCartLineWeightActivity = <GetCartLineWeightActivity>(this);
        var weighItemDialog: Controls.WeighItemDialog = new Controls.WeighItemDialog();

        var asyncQueue: AsyncQueue = new AsyncQueue()
            .enqueue(() => {
                weighItemDialog.show({ cartLines: [self.context.cartLine] }, false)
                    .on(DialogResult.OK, (result: Controls.IWeighItemDialogOutput) => {
                        self.response = { weight: result.weights[0] };
                    });

                return ModalDialogHelper.toVoidAsyncResult(weighItemDialog, false);
            }).enqueue(() => {
                var asyncResult: AsyncResult<ICancelableResult> = new AsyncResult<ICancelableResult>(null);
                ModalDialogHelper.callResponseHandler(self, weighItemDialog, DialogResult.OK, asyncResult,
                    (result: Controls.IWeighItemDialogOutput): GetCartLineWeightActivityResponse => {
                        return { weight: result.weights[0] };
                    });

                return asyncQueue.cancelOn(asyncResult);
            });

        return asyncQueue.run().done((result: ICancelableResult) => {
            if (result.canceled) {
                self.response = null;
            };
        });
    };
}