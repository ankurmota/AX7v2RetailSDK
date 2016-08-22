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

    GetCustomerAccountDepositLineCommentsActivity.prototype.execute = function (): IVoidAsyncResult {
        var self: any = <GetCustomerAccountDepositLineCommentsActivity>(this);

        var asyncQueue: AsyncQueue = new AsyncQueue();
        var comments: string[] = [];
        var textInputDialog: Controls.TextInputDialog = new Controls.TextInputDialog();

        asyncQueue
            .enqueue(() => {
                var result: IAsyncResult<ICancelableResult> = createLineCommentQueue(textInputDialog, self.context.customerAccountDepositLines, comments).run();
                return asyncQueue.cancelOn(result);
            }).enqueue(() => {
                self.response = { comments: comments };

                var asyncResult: AsyncResult<ICancelableResult> = new AsyncResult<ICancelableResult>(null);
                ModalDialogHelper.callResponseHandler(self, textInputDialog, DialogResult.OK, asyncResult);

                return asyncQueue.cancelOn(asyncResult);
            });

        return asyncQueue.run().done((result: ICancelableResult) => {
            if (result.canceled) {
                self.response = null;
            };
        });
    };

    /**
     * Creates an async queue to get comments for the cart lines.
     *
     * @param {Controls.TextInputDialog} textInputDialog The modal dialog used to get comment for a cart line.
     * @param {Model.Entities.CustomerAccountDepositLine[]} customerAccountDepositLines The customer account deposit lines to get comments for.
     * @param {string[]} comments The comments gotten for the cart lines in the same order.
     * @return {AsyncQueue} The async queue.
     */
    function createLineCommentQueue(
        textInputDialog: Controls.TextInputDialog,
        customerAccountDepositLines: Model.Entities.CustomerAccountDepositLine[],
        comments: string[]): AsyncQueue {

        var asyncQueue: AsyncQueue = new AsyncQueue();
        customerAccountDepositLines.forEach((line: Model.Entities.CustomerAccountDepositLine) => {
            asyncQueue.enqueue(() => {
                // Customer account deposit comment
                textInputDialog.title(Commerce.ViewModelAdapter.getResourceString("string_8100"));

                textInputDialog.show({ content: line.Comment, maxLength: 60 }, false /* hideOnResult */)
                    .on(DialogResult.OK, (comment: string) => {
                        comments.push(comment);
                    }).on(DialogResult.Cancel, () => {
                        textInputDialog.hide();
                    });

                return asyncQueue.cancelOn(ModalDialogHelper.toVoidAsyncResult(textInputDialog, false));
            });
        });

        return asyncQueue;
    }
}