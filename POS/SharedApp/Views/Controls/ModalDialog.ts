/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='UserControl.ts'/>

module Commerce.Controls {
    "use strict";

    /**
     * Options representing Modal dialog state.
     */
    export interface IModalDialogOptions {
        visible: boolean;
        title: string;
        subTitle: string;
        message: string;
    }

    /**
     * Base class for modal dialogs.
     */
    export class ModalDialog<DialogState, Output> extends UserControl {

        private _visible: Observable<boolean>;
        private _title: Observable<string>;
        private _subTitle: Observable<string>;
        private _message: Observable<string>;
        private _dialogResult: AsyncDialogResult<Output>;
        private _indeterminateWaitVisible: Observable<boolean>;
        private _subTitleCssClass: Observable<string>;
        private _onHidden: VoidAsyncResult;

        /**
         * Constructs a new instance of the ModalDialog class.
         */
        constructor() {
            super();

            this._visible = ko.observable(false);
            this._title = ko.observable(StringExtensions.EMPTY);
            this._subTitle = ko.observable(StringExtensions.EMPTY);
            this._message = ko.observable(StringExtensions.EMPTY);
            this._indeterminateWaitVisible = ko.observable(false);
            this._subTitleCssClass = ko.observable(null);
            this._onHidden = new VoidAsyncResult(null);
        }

        /**
         * Gets the modal dialog visibility observable.
         *
         * @return {Observable<boolean>} The modal dialog visibility observable.
         */
        public get visible(): Observable<boolean> {
            return this._visible;
        }

        /**
         * Gets the modal dialog title observable.
         *
         * @return {Observable<string>} The modal dialog title observable.
         */
        public get title(): Observable<string> {
            return this._title;
        }

        /**
         * Gets the modal dialog sub-title observable.
         *
         * @return {Observable<string>} The modal dialog sub-title observable.
         */
        public get subTitle(): Observable<string> {
            return this._subTitle;
        }

        /**
         * Gets the modal dialog sub-title css class observable.
         *
         * @return {Observable<string>} The modal dialog sub-title css class observable.
         */
        public get subTitleCssClass(): Observable<string> {
            return this._subTitleCssClass;
        }

        /**
         * Gets the modal dialog message.
         *
         * @return {Observable<string>} The modal dialog sub-title observable.
         */
        public get message(): Observable<string> {
            return this._message;
        }

        /**
         * Gets the modal dialog indeterminate wait visibility observable.
         *
         * @return {Observable<boolean>} The modal dialog indeterminate wait visibility observable.
         */
        public get indeterminateWaitVisible(): Observable<boolean> {
            return this._indeterminateWaitVisible;
        }

        /**
         * Gets the modal dialog async result.
         *
         * @return {AsyncDialogResult<Output>} The modal dialog async result.
         */
        public get dialogResult(): AsyncDialogResult<Output> {
            return this._dialogResult;
        }

        /**
         * Clears the modal dialog async result.
         */
        public clearResult() {
            this._dialogResult.clear();
        }

        /**
         * Shows the modal dialog.
         *
         * @param {DialogState} dialogState The dialog state.
         * @param {boolean} [hideOnResult] Whether or not to hide the dialog after a result is provided.
         * @return {IAsyncDialogResult<Output>} The async dialog result.
         * @remarks Modal dialogs are hidden by default after a result is provided.
         */
        public show(dialogState: DialogState, hideOnResult: boolean = true): IAsyncDialogResult<Output> {
            this._dialogResult = new AsyncDialogResult<Output>(null);

            if (hideOnResult) {
                this._dialogResult.on(DialogResult.OK, (result) => { this.hide(); });
                this._dialogResult.on(DialogResult.Cancel, (result) => { this.hide(); });
                this._dialogResult.on(DialogResult.Close, (result) => { this.hide(); });
                this._dialogResult.on(DialogResult.Yes, (result) => { this.hide(); });
                this._dialogResult.on(DialogResult.No, (result) => { this.hide(); });
            }

            if (!this.element.parentNode) {
                // Adding the IRemoveable attribute to make sure this is removed if the application is suspended.
                $(this.element).attr("IRemoveable", "true");
                document.body.appendChild(this.element);
            }

            this.render().done(() => {
                this.onShowing(dialogState);
                RetailLogger.viewsControlsModalDialogRendered();
            });

            return this._dialogResult;
        }

        /**
         * This method is supposed to be implemented by the derived classes and is called on
         * showing the control, i.e. when the control is supposed to be shown.
         * Derived dialogs will decide wether the control should be shown or not by
         * setting the visibility flag to true or false.
         *
         * @param {DialogState} dialogState The dialog state.
         */
        public onShowing(dialogState: DialogState) {
            this.visible(true);
        }

        /**
         * Hides the modal dialog.
         *
         * @return {IvoidAsyncResult} The async result that is resolved when the dialog is completely hidden.
         */
        public hide(): IVoidAsyncResult {
            // if it is already hidden, resolve
            if (!this.visible()) {
                this._onHidden.resolve();
            }

            this.visible(false);
            this.indeterminateWaitVisible(false);

            return this._onHidden;
        }

        /**
         * This is called when the dialog is completely hidden.
         */
        public onHidden() {
            if (document.body === this.element.parentNode) {
                document.body.removeChild(this.element);
            }

            this._onHidden.resolve();
        }
    }
}