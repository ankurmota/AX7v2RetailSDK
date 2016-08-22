/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ModalDialogHelper.ts'/>

module Commerce.Activities {
    "use strict";

    GetReasonCodeLinesActivity.prototype.execute = function (): IVoidAsyncResult {
        var self = <GetReasonCodeLinesActivity>this;

        var description = null;
        if (self.context.cartLine) {
            description = self.context.cartLine.Description;
        }

        var reasonCodeDialog = new Commerce.Controls.ReasonCodeDialog();
        reasonCodeDialog.title(Commerce.ViewModelAdapter.getResourceString("string_186")); // Product comment
        reasonCodeDialog.subTitle(description);
        reasonCodeDialog.show(self.context.reasonCodes, false)
            .on(DialogResult.OK, (reasonCodeLines) => {
                self.response = { reasonCodeLines: reasonCodeLines };
            });

        return ModalDialogHelper.toVoidAsyncResult(reasonCodeDialog)
            .done((): void => { reasonCodeDialog.onHidden(); });
    };
}