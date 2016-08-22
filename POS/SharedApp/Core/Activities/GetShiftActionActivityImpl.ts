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

    GetShiftActionActivity.prototype.execute = function (): IVoidAsyncResult {
        var self: GetShiftActionActivity = <GetShiftActionActivity>this;
        var shiftActionDialog: Commerce.Controls.ShiftActionDialog = new Commerce.Controls.ShiftActionDialog();

        shiftActionDialog.show(this.context.shiftActions, false /* hideOnResult */)
            .on(DialogResult.OK, (result: ShiftActionType) => {
            self.response = <IGetShiftActionActivityResponse>{ shiftActionType: result };
            });

        return ModalDialogHelper.toVoidAsyncResult(shiftActionDialog);
    };
}