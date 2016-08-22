/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ModalDialogHelper.ts'/>

module Commerce.Activities {
    "use strict";

    GetPriceOverrideActivity.prototype.execute = function (): IVoidAsyncResult {
        var self = <GetPriceOverrideActivity>this;
        var priceInputDialog = new Commerce.Controls.PriceInputDialog();

        var asyncQueue = new AsyncQueue();
        asyncQueue
            .enqueue(() => {
                priceInputDialog.title(Commerce.ViewModelAdapter.getResourceString("string_5700"));
                priceInputDialog.subTitle(self.context.cartLine.Description);
                priceInputDialog.show({
                    defaultPrice: self.context.cartLine.Price,
                    minPrice: Number.NaN,
                    maxPrice: Number.NaN,
                    minPriceInclusive: true,
                    maxPriceInclusive: true
                }, false)
                    .on(DialogResult.OK, (inputValue) => {
                        self.response = { newPrice: inputValue };
                    })
                    .on(DialogResult.Cancel, () => {
                        priceInputDialog.hide();
                    });

                return asyncQueue.cancelOn(ModalDialogHelper.toVoidAsyncResult(priceInputDialog, false));
            }).enqueue(() => {
                var asyncResult = new AsyncResult<ICancelableResult>(null);
                ModalDialogHelper.callResponseHandler(
                    self, priceInputDialog, DialogResult.OK, asyncResult,
                    (result) => { return { newPrice: result }; });

                return asyncQueue.cancelOn(asyncResult);
            });

        return asyncQueue.run().done((result) => {
            if (result.canceled) {
                self.response = null;
            };
        });
    };
}