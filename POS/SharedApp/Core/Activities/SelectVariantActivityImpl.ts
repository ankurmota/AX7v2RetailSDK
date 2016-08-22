/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.Core.d.ts'/>

module Commerce.Activities {
    "use strict";

    SelectVariantActivity.prototype.execute = function (): IAsyncResult<ICancelableResult> {
        var self = <SelectVariantActivity>this;

        var variantDialog = new Commerce.Controls.SelectVariantDialog();
        var dialogState: Controls.ISelectVariantDialogState = {
            product: self.context.product,
        };

        var result: AsyncResult<ICancelableResult> = new AsyncResult<ICancelableResult>();
        var updateResponse: (dialogResult: Controls.ISelectVariantDialogOuput) => SelectVariantActivityResponse =
            (dialogResult: Controls.ISelectVariantDialogOuput): SelectVariantActivityResponse => {
                return { selectedDimensions: dialogResult.selectedDimensions };
            };

        variantDialog.show(dialogState, false /* hideOnResult */)
        ModalDialogHelper.callResponseHandler(self, variantDialog, DialogResult.OK, result, updateResponse);

        return result;
    };
}