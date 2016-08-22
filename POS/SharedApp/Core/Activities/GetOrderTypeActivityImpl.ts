/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Views/Controls/GetOrderTypeDialog.ts'/>
///<reference path='ModalDialogHelper.ts'/>

module Commerce.Activities {
    "use strict";

    GetOrderTypeActivity.prototype.execute = function (): IVoidAsyncResult {
        var self = <GetOrderTypeActivity>(this);

        var getOrderTypeDialog = new Controls.GetOrderTypeDialog();

        getOrderTypeDialog.show(self.context, false)
            .on(DialogResult.OK, (orderMode: Model.Entities.CustomerOrderMode) => {
                self.response = { customerOrderMode: orderMode };
            });

        return ModalDialogHelper.toVoidAsyncResult(getOrderTypeDialog, true);
    };
}