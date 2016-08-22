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

    PrintReceiptActivity.prototype.execute = function (): IVoidAsyncResult {
        var self: PrintReceiptActivity = <PrintReceiptActivity>this;

        var printReceiptDialog: Commerce.Controls.PrintReceiptDialog = new Commerce.Controls.PrintReceiptDialog();
        var context: PrintReceiptActivityContext = self.context;
        printReceiptDialog.show({
            receipts: context.receipts,
            rejectOnHardwareStationErrors: true,
            notifyOnNoPrintableReceipts: context.notifyOnNoPrintableReceipts
        }, false /* hideOnResult */);

        return ModalDialogHelper.toVoidAsyncResult(printReceiptDialog);
    };
}