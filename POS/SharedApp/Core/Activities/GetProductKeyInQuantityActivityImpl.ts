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

    GetProductKeyInQuantityActivity.prototype.execute = function (): IVoidAsyncResult {
        var self = <GetProductKeyInQuantityActivity>(this);

        var numberInputDialog = new Commerce.Controls.NumberInputDialog();

        // "Specify quantity"
        numberInputDialog.title(Commerce.ViewModelAdapter.getResourceString("string_827"));

        // "Enter quantity"
        numberInputDialog.subTitle(StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_929"), self.context.product.ItemId, self.context.product.Name));
        numberInputDialog.subTitleCssClass("primaryFontColor");
        numberInputDialog.label(Commerce.ViewModelAdapter.getResourceString("string_5306"));

        numberInputDialog.show({ content: 0, min: 0, max: Number.MAX_VALUE }, false /* hideOnResult */)
            .on(DialogResult.OK, (keyInQuantity) => {
                self.response = { keyInQuantity: keyInQuantity };
            });

        return ModalDialogHelper.toVoidAsyncResult(numberInputDialog);
    };
}