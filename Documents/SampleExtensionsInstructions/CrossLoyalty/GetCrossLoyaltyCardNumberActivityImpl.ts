///<reference path='../../Views/Controls/TextInputDialog.ts'/>
///<reference path='../../Custom.Extension.d.ts'/>
///<reference path='ModalDialogHelper.ts'/>

module Custom.Activities {
    "use strict";

    GetCrossLoyaltyCardNumberActivity.prototype.execute = function (): Commerce.IVoidAsyncResult {
        var self: GetCrossLoyaltyCardNumberActivity = <GetCrossLoyaltyCardNumberActivity>(this);

        var asyncQueue: Commerce.AsyncQueue = new Commerce.AsyncQueue();
        var textInputDialog: Commerce.Controls.TextInputDialog = new Commerce.Controls.TextInputDialog();

        asyncQueue.enqueue(() => {
            textInputDialog.title("Loyalty card number");
            textInputDialog.show({ content: Commerce.StringExtensions.EMPTY, maxLength: 60 }, false /* hideOnResult */)
                .on(Commerce.DialogResult.OK, (cardNumber: string) => {
                    self.response = { cardNumber: cardNumber };
                }).on(Commerce.DialogResult.Cancel, () => {
                    textInputDialog.hide().done(() => { asyncQueue.cancel(); });
                });

            return Commerce.Activities.ModalDialogHelper.toVoidAsyncResult(textInputDialog, false);
        }).enqueue(() => {
            var asyncResult: Commerce.AsyncResult<Commerce.ICancelableResult> = new Commerce.AsyncResult<Commerce.ICancelableResult>(null);
            Commerce.Activities.ModalDialogHelper.callResponseHandler(self, textInputDialog, Commerce.DialogResult.OK, asyncResult);

            return asyncQueue.cancelOn(asyncResult);
        });

        return asyncQueue.run().done((result: any) => {
            if (result.canceled) {
                self.response = null;
            };
        });
    };
} 
