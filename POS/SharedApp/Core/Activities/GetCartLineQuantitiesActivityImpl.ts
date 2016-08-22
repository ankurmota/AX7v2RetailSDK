/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Views/Controls/SetQuantityDialog.ts'/>
///<reference path='ModalDialogHelper.ts'/>

module Commerce.Activities {
    "use strict";

    GetCartLineQuantitiesActivity.prototype.execute = function (): IVoidAsyncResult {
        var self: GetCartLineQuantitiesActivity = <GetCartLineQuantitiesActivity>(this);
        var setQuantityDialog: Controls.SetQuantityDialog = new Controls.SetQuantityDialog();

        var asyncQueue: AsyncQueue = new AsyncQueue()
            .enqueue(() => {
                setQuantityDialog.show({ cartLines: self.context.cartLines }, false)
                    .on(DialogResult.OK, (result: Controls.ISetQuantityDialogOutput) => {
                        self.response = { quantities: [].concat(result.quantities) };
                    });

                return ModalDialogHelper.toVoidAsyncResult(setQuantityDialog, false);
            }).enqueue(() => {
                var asyncResult: AsyncResult<ICancelableResult> = new AsyncResult<ICancelableResult>(null);
                ModalDialogHelper.callResponseHandler(self, setQuantityDialog, DialogResult.OK, asyncResult,
                    (result: Controls.ISetQuantityDialogOutput): GetCartLineQuantitiesActivityResponse => {
                        return { quantities: [].concat(result.quantities) };
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