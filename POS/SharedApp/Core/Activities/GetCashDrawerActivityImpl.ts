/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.Core.d.ts'/>
///<reference path='ModalDialogHelper.ts'/>

module Commerce.Activities {
    "use strict";

    GetCashDrawerActivity.prototype.execute = function (): IVoidAsyncResult {
        var self: GetCashDrawerActivity = <GetCashDrawerActivity>this;
        var cashDrawerInputDialog: Commerce.Controls.CashDrawerInputDialog = new Commerce.Controls.CashDrawerInputDialog();

        cashDrawerInputDialog.show(this.context.availableCashDrawers, false /* hideOnResult */)
            .on(DialogResult.OK, (result: Proxy.Entities.HardwareProfileCashDrawer) => {
                self.response = <IGetCashDrawerActivityResponse> { cashDrawer: result };
            });

        return ModalDialogHelper.toVoidAsyncResult(cashDrawerInputDialog);
    };
}