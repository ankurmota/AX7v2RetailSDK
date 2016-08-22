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

    GetSerialNumberActivity.prototype.execute = function (): IVoidAsyncResult {
        var self = <GetSerialNumberActivity>(this);

        var serialNumberInputDialog = new Commerce.Controls.SerialNumberInputDialog();
        
        serialNumberInputDialog.show({
            product: self.context.product
        }, false /* hideOnResult */)
            .on(DialogResult.OK, (serialNumber) => {
                self.response = { serialNumber: serialNumber };
            });

        return ModalDialogHelper.toVoidAsyncResult(serialNumberInputDialog);
    };
}