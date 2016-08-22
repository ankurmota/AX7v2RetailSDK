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

    GetProductKeyInPriceActivity.prototype.execute = function (): IVoidAsyncResult {
        var self: GetProductKeyInPriceActivity = <GetProductKeyInPriceActivity>(this);

        var priceInputDialog: Controls.PriceInputDialog = new Controls.PriceInputDialog();

        // "Specify price"
        priceInputDialog.title(Commerce.ViewModelAdapter.getResourceString("string_826"));

        // "Enter a price"
        var subTitle: string;
        if (StringExtensions.isNullOrWhitespace(self.context.product.ItemId) || StringExtensions.isNullOrWhitespace(self.context.product.Name)) {
            subTitle = Commerce.ViewModelAdapter.getResourceString("string_825");
        } else {
            subTitle = StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_929"), self.context.product.ItemId, self.context.product.Name);
        }
        priceInputDialog.subTitle(subTitle);

        priceInputDialog.show(
            {
                defaultPrice: self.context.product.Price,
                maxPrice: self.context.maxPrice,
                minPrice: self.context.minPrice,
                maxPriceInclusive: self.context.maxPriceInclusive,
                minPriceInclusive: self.context.minPriceInclusive

            },
            true /* hideOnResult */);

        var asyncResult: AsyncResult<ICancelableResult> = new AsyncResult<ICancelableResult>(null);
        ModalDialogHelper.callResponseHandler(self, priceInputDialog, DialogResult.OK, asyncResult,
            (result: number): { keyInPrice: number } => { return { keyInPrice: result }; });

        return asyncResult.done((result: ICancelableResult) => {
            if (result.canceled) {
                self.response = null;
            };
        });
    };
}