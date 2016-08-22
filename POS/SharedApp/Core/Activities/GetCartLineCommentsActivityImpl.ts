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

    GetCartLineCommentsActivity.prototype.execute = function (): IVoidAsyncResult {
        var self = <GetCartLineCommentsActivity>(this);

        var asyncQueue = new AsyncQueue();
        var comments: string[] = [];
        var textInputDialog = new Controls.TextInputDialog();

        asyncQueue
            .enqueue(() => {
                var result = createProductCommentQueue(textInputDialog, self.context.cartLines, comments).run();
                return asyncQueue.cancelOn(result);
            }).enqueue(() => {
                self.response = { comments: comments };

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

    /**
     * Creates an async queue to get comments for the cart lines.
     *
     * @param {Controls.TextInputDialog} textInputDialog The modal dialog used to get comment for a cart line.
     * @param {Model.Entities.CartLine[]} cartLines The cart lines to get comments for.
     * @param {string[]} comments The comments gotten for the cart lines in the same order.
     * @return {AsyncQueue} The async queue.
     */
    function createProductCommentQueue(
        textInputDialog: Controls.TextInputDialog,
        cartLines: Model.Entities.CartLine[],
        comments: string[]): AsyncQueue {

        var asyncQueue = new AsyncQueue();
        cartLines.forEach((cartLine) => {
            asyncQueue.enqueue(() => {
                // "Product comment"
                textInputDialog.title(Commerce.ViewModelAdapter.getResourceString("string_186"));
                textInputDialog.subTitle(StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_929") /*{0}:{1} */, cartLine.ItemId, cartLine.Description));
                textInputDialog.subTitleCssClass("primaryFontColor");

                textInputDialog.show({ content: cartLine.Comment, maxLength: 60, rowsNumber: 2, hideScrollbar: true, enterKeyDisabled: true, labelResx: "string_197" /* Comment */ }, false /* hideOnResult */)
                    .on(DialogResult.OK, (comment) => {
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