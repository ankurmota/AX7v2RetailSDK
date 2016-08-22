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

    GetCustomerAccountDepositAmountActivity.prototype.execute = function(): IVoidAsyncResult {
        var self = <GetCustomerAccountDepositAmountActivity>(this);

        var amountInputDialog = new Commerce.Controls.NumberInputDialog();
        var dialogState: Controls.NumberInputDialogState = { content: 0, max: Number.MAX_VALUE, min: 0 };
        amountInputDialog.title(Commerce.ViewModelAdapter.getResourceString("string_1927")); // Customer Account Deposit pop-up header title
        amountInputDialog.label(Commerce.ViewModelAdapter.getResourceString("string_1928")); // Customer Account Deposit pop-up label

        amountInputDialog.show(
                dialogState, false /* hideOnResult */)
            .on(DialogResult.OK, (result?: number)=> {
            if (result < 0) {
                NotificationHandler.displayErrorMessage('string_29343'); // The customer account deposit amount cannot be negative.
                return;
            }
            self.response = result;
        });

        return ModalDialogHelper.toVoidAsyncResult(amountInputDialog);

    }

}