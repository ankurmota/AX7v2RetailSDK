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

    /**
     * Helper class for ModalDialogs.
     */
    export class ModalDialogHelper {

        /**
         * Converts an IAsyncDialogResult to an IVoidAsyncResult.
         *
         * @param {Controls.ModalDialog<T, U>} dialog The dialog to be converted to an async result.
         * @param {boolean} [hideOnResult] Whether or not to hide the dialog after a result is provided.
         * @return {IAsyncResult<ICancelableResult>} Returns cancelable async result.
         */
        public static toVoidAsyncResult<T, U>(
            dialog: Controls.ModalDialog<T, U>,
            hideOnResult: boolean = true): IAsyncResult<ICancelableResult> {
            var asyncResult = new AsyncResult(null);
            dialog.dialogResult
                .on(DialogResult.OK, () => {
                    ModalDialogHelper.hideDialogAndResolve(dialog, hideOnResult, asyncResult);
                }).on(DialogResult.Cancel, () => {
                    ModalDialogHelper.hideDialogAndResolve(dialog, hideOnResult, asyncResult, true);
                }).on(DialogResult.Close, () => {
                    ModalDialogHelper.hideDialogAndResolve(dialog, hideOnResult, asyncResult);
                }).on(DialogResult.Yes, () => {
                    ModalDialogHelper.hideDialogAndResolve(dialog, hideOnResult, asyncResult);
                }).on(DialogResult.No, () => { asyncResult.resolve({ canceled: false }); })
                .onError((errors) => { asyncResult.reject(errors); });

            return asyncResult;
        }

        /**
         * Calls the activity response handler in the context of the given dialog. This way, dialogs can wait on the response
         * handler in order to be hidden or show an error message. If the activity does not have a response handler, the dialog
         * is hidden and the async result is resolved.
         *
         * @param {Activity<T>} activity The activity.
         * @param {Controls.ModalDialog<U, V>} dialog The modal dialog.
         * @param {DialogResult} onDialogResult The dialog result to attach to in order to call the response handler.
         * @param {AsyncResult<ICancelableResult>} asyncResult The async result to resolve or reject, given the dialog result and response handler result.
         * @param {(result: V) => T} [updateResponse] Updates the activity response, given the new dialog result.
         */
        public static callResponseHandler<T, U, V>(
            activity: Activity<T>,
            dialog: Controls.ModalDialog<U, V>,
            onDialogResult: DialogResult,
            asyncResult: AsyncResult<ICancelableResult>,
            updateResponse?: (result: V) => T) {

            dialog.dialogResult
                .on(onDialogResult, (result) => {
                    // maps dialog result to response
                    if (updateResponse) {
                        activity.response = updateResponse(result);
                    }

                    if (activity.responseHandler) {
                        // set up dialog
                        dialog.indeterminateWaitVisible(true);

                        // call response handler
                        activity.responseHandler(activity.response)
                            .done(() => {
                                dialog.hide().done(() => { asyncResult.resolve({ canceled: false }); });
                            }).fail((errors) => {
                                // setup dialog again
                                dialog.indeterminateWaitVisible(false);
                                dialog.clearResult();

                                // only retry if the session is still valid
                                if (Session.instance.isSessionStateValid) {
                                    Commerce.NotificationHandler.displayClientErrors(errors)
                                        .done(() => { dialog.focus(); });

                                    ModalDialogHelper.callResponseHandler(activity, dialog, onDialogResult, asyncResult, updateResponse);
                                } else {
                                    // hides the dialog and cancels the async result
                                    dialog.hide().done(() => { asyncResult.resolve({ canceled: true }); });
                                }
                            });
                    } else {
                        // just hides the dialog and resolves the result
                        dialog.hide().done(() => { asyncResult.resolve({ canceled: false }); });
                    }
                }).on(DialogResult.Cancel, () => {
                    dialog.hide().done(() => { asyncResult.resolve({ canceled: true }); });
                }).onError((errors) => {
                    dialog.hide().done(() => { asyncResult.reject(errors); });
                });
        }

        /**
         * Hides the dialog if required, and waits until the dialog is completely hidden in order to resolve the async resut.
         * @param {Controls.ModalDialog<T, U>} dialog The dialog to be hidden.
         * @param {boolean} hideOnResult Whether or not to hide the dialog after a result is provided.
         * @param {VoidAsyncResult} asyncResult The async result to be resolved.
         */
        private static hideDialogAndResolve<T, U>(
            dialog: Controls.ModalDialog<T, U>,
            hideOnResult: boolean,
            asyncResult: AsyncResult<ICancelableResult>,
            canceled: boolean = false) {

            if (hideOnResult) {
                dialog.hide().done(() => { asyncResult.resolve({ canceled: canceled }); });
            } else {
                asyncResult.resolve({ canceled: canceled });
            }
        }
    }
}