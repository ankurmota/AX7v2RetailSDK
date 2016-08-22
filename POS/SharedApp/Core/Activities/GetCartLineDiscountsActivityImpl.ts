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

    GetCartLineDiscountsActivity.prototype.execute = function (): IVoidAsyncResult {
        var self = <GetCartLineDiscountsActivity>(this);

        var asyncQueue = new AsyncQueue();
        var discounts: number[] = [];
        var addDiscountDialog = new Controls.AddDiscountDialog();

        asyncQueue
            .enqueue(() => {
                var result = createProductDiscountQueue(addDiscountDialog, self.context, discounts).run();
                return asyncQueue.cancelOn(result);
            }).enqueue(() => {
                self.response = { discounts: discounts };

                var asyncResult = new AsyncResult<ICancelableResult>(null);
                ModalDialogHelper.callResponseHandler(
                    self, addDiscountDialog, DialogResult.OK, asyncResult,
                    (result) => { return { discounts: [result.discountValue] }; });

                return asyncQueue.cancelOn(asyncResult);
            });

        return asyncQueue.run().done((result) => {
            if (result.canceled) {
                self.response = null;
            };
        });
    };

    /**
     * Creates an async queue to get discounts for the cart lines.
     *
     * @param {Controls.AddDiscountDialog} addDiscountDialog The modal dialog used to get a discount for a cart line.
     * @param {GetCartLineDiscountsActivityContext} context The activity context.
     * @param {number[]} discounts The discounts gotten for the cart lines in the same order.
     * @return {AsyncQueue} The async queue.
     */
    function createProductDiscountQueue(
        addDiscountDialog: Controls.AddDiscountDialog,
        context: GetCartLineDiscountsActivityContext,
        discounts: number[]): AsyncQueue {

        var asyncQueue = new AsyncQueue();
        context.cartLines.forEach((cartLine) => {
            asyncQueue.enqueue(() => {
                var dialogState: Controls.AddDiscountDialogState = {
                    cartLine: cartLine,
                    discountType: context.isPercent ? Model.Entities.ManualDiscountType.LineDiscountPercent : Model.Entities.ManualDiscountType.LineDiscountAmount,
                };

                addDiscountDialog.show(dialogState, false /* hideOnResult */)
                    .on(DialogResult.OK, (result) => {
                        discounts.push(result.discountValue);
                    }).on(DialogResult.Cancel, () => {
                        addDiscountDialog.hide().done(() => { asyncQueue.cancel(); });
                    });

                return ModalDialogHelper.toVoidAsyncResult(addDiscountDialog, false);
            });
        });

        return asyncQueue;
    }
}