/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Views/Controls/UnitOfMeasureDialog.ts'/>
///<reference path='ModalDialogHelper.ts'/>

module Commerce.Activities {
    "use strict";

    GetCartLineUnitOfMeasuresActivity.prototype.execute = function (): IVoidAsyncResult {
        var self: GetCartLineUnitOfMeasuresActivity = <GetCartLineUnitOfMeasuresActivity>(this);
        var unitOfMeasureDialog: Controls.UnitOfMeasureDialog = new Controls.UnitOfMeasureDialog();

        var asyncQueue: AsyncQueue = new AsyncQueue()
            .enqueue(() => {
                var dialogOptions: Controls.IUnitOfMeasureDialogState = {
                    cartLinesWithUnitOfMeasureOptions: self.context.cartLinesWithUnitOfMeasureOptions
                };
                unitOfMeasureDialog.show(dialogOptions, false)
                    .on(DialogResult.OK, (result: Controls.IUnitOfMeasureDialogOutput) => {
                        self.response = { selectedUnitsOfMeasure: [].concat(result.selectedUnitsOfMeasure) };
                    });

                return ModalDialogHelper.toVoidAsyncResult(unitOfMeasureDialog, true);
            });

        return asyncQueue.run().done((result: ICancelableResult) => {
            if (result.canceled) {
                self.response = null;
            };
        });
    };
}