/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Views/Controls/AddLoyaltyCardDialog.ts'/>
///<reference path='ModalDialogHelper.ts'/>

module Commerce.Activities {
    "use strict";

    GetLoyaltyCardActivity.prototype.execute = function (): IVoidAsyncResult {
        var self: GetLoyaltyCardActivity = <GetLoyaltyCardActivity>(this);

        var asyncQueue: AsyncQueue = new AsyncQueue();
        var addLoyaltyCardDialog: Controls.AddLoyaltyCardDialog = new Controls.AddLoyaltyCardDialog();

        asyncQueue
            .enqueue(() => {
                addLoyaltyCardDialog.show({ defaultLoyaltyCardId: self.context.defaultLoyaltyCardId }, false /* hideOnResult */);

                var asyncResult: AsyncResult<ICancelableResult> = new AsyncResult<ICancelableResult>(null);
                ModalDialogHelper.callResponseHandler(self,
                    addLoyaltyCardDialog,
                    DialogResult.OK,
                    asyncResult,
                    (addLoyaltyCardDialogResult: Commerce.Controls.AddLoyaltyCardDialogResult) => {
                        return { loyaltyCardId: addLoyaltyCardDialogResult.loyaltyCardId };
                    });

                return asyncQueue.cancelOn(asyncResult);
            });

        return asyncQueue.run().done((result: ICancelableResult) => {
            if (result.canceled) {
                self.response = null;
            };
        });
    };
}