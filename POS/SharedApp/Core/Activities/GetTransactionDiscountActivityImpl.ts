/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Views/Controls/AddDiscountDialog.ts'/>
///<reference path='ModalDialogHelper.ts'/>

module Commerce.Activities {
    "use strict";

    GetTransactionDiscountActivity.prototype.execute = function (): IVoidAsyncResult {
        var self = <GetTransactionDiscountActivity>(this);

        var asyncQueue = new AsyncQueue();
        var addDiscountDialog = new Controls.AddDiscountDialog();

        asyncQueue
            .enqueue(() => {
                var dialogState: Controls.AddDiscountDialogState = {
                    cartLine: null,
                    discountType: self.context.isPercent ? Model.Entities.ManualDiscountType.TotalDiscountPercent : Model.Entities.ManualDiscountType.TotalDiscountAmount,
                };

                addDiscountDialog.show(dialogState, false /* hideOnResult */)
                    .on(DialogResult.OK, (result) => {
                        self.response = { discount: result.discountValue };
                    }).on(DialogResult.Cancel, () => {
                        addDiscountDialog.hide().done(() => { asyncQueue.cancel(); });
                    });

                return ModalDialogHelper.toVoidAsyncResult(addDiscountDialog, false);
            }).enqueue(() => {
                var asyncResult = new AsyncResult<ICancelableResult>(null);
                ModalDialogHelper.callResponseHandler(
                    self, addDiscountDialog, DialogResult.OK, asyncResult,
                    (result) => { return { discount: result.discountValue } });

                return asyncQueue.cancelOn(asyncResult);
            });

        return asyncQueue.run().done((result) => {
            if (result.canceled) {
                self.response = null;
            };
        });
    };
}