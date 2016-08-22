/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Views/Controls/GetSalesPersonDialog.ts'/>
///<reference path='ModalDialogHelper.ts'/>

module Commerce.Activities {
    "use strict";

    GetSalesPersonActivity.prototype.execute = function (): IVoidAsyncResult {
        var self = <GetSalesPersonActivity>(this);

        var getSalesPersonDialog = new Controls.GetSalesPersonDialog();

        getSalesPersonDialog.show(self.context, false)
            .on(DialogResult.OK, (staffId: string) => {
                self.response = { salesPersonId: staffId };
            });

        return ModalDialogHelper.toVoidAsyncResult(getSalesPersonDialog, true);
    };
}