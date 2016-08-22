/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Views/Controls/TextInputDialog.ts'/>
///<reference path='ModalDialogHelper.ts'/>

module Commerce.Activities {
    "use strict";

    GetInvoiceCommentActivity.prototype.execute = function (): IVoidAsyncResult {
        var self = <GetInvoiceCommentActivity>(this);

        var asyncQueue = new AsyncQueue();
        var comments: string[] = [];
        var textInputDialog = new Controls.TextInputDialog();

        asyncQueue
            .enqueue(() => {
                // "Invoice comment"
                textInputDialog.title(Commerce.ViewModelAdapter.getResourceString("string_99"));
                textInputDialog.show({ content: self.context.cart.InvoiceComment, maxLength: 60 }, false /* hideOnResult */)
                    .on(DialogResult.OK, (comment) => {
                        self.response = comment;
                    }).on(DialogResult.Cancel, () => {
                        textInputDialog.hide().done(() => { asyncQueue.cancel(); });
                    });

                return ModalDialogHelper.toVoidAsyncResult(textInputDialog, false);
            }).enqueue(() => {
                var asyncResult = new AsyncResult<ICancelableResult>(null);
                ModalDialogHelper.callResponseHandler(self, textInputDialog, DialogResult.OK, asyncResult);

                return asyncQueue.cancelOn(asyncResult);
            });

        return asyncQueue.run().done((result) => {
            if (result.canceled) {
                self.response = null;
            };
        });
    };
}