/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ModalDialog.ts'/>

module Commerce.Controls {
    "use strict";

    export interface IHardwareStationErrorDialogState {
        hardwareStation: Proxy.Entities.IHardwareStation;
        errors: Proxy.Entities.Error[];
    }

    export class HardwareStationErrorDialog extends ModalDialog<IHardwareStationErrorDialogState, boolean> {

        public _hardwareStationUrl: Observable<string>;
        public _hardwareStationDescription: Observable<string>;
        public subTitleValue: Computed<string>;
        public dontShow: Observable<boolean>;
        public messageValue: Observable<string>;
        public hardwareStationSelectionAvailable: Observable<boolean>;

        constructor() {
            super();

            this._hardwareStationUrl = ko.observable(null);
            this._hardwareStationDescription = ko.observable(null);
            this.dontShow = ko.observable(false);
            this.subTitleValue = ko.computed(() => this.getSubTitleValue());
            this.messageValue = ko.observable(null);
            this.hardwareStationSelectionAvailable = ko.observable(null);
        }

        /**
         * Shows the modal dialog.
         *
         * @param {IHardwareStationErrorDialogState} dialogState The modal dialog state.
         */
        public onShowing(dialogState: IHardwareStationErrorDialogState): void {
            this._hardwareStationUrl(dialogState.hardwareStation.Url);
            this._hardwareStationDescription(dialogState.hardwareStation.Description);
            this.messageValue(Commerce.ErrorHelper.getErrorMessages(dialogState.errors));
            this.hardwareStationSelectionAvailable(Session.instance.isLoggedOn);
            this.visible(true);
        }

       /**
        * Button click handler
        *
        * @param {string} buttonId The identifier of the button.
        */
        public buttonClickHandler(buttonId: string): void {
            switch (buttonId) {
                case Commerce.Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.OK, this.dontShow());
                    break;

                case Commerce.Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.Cancel, this.dontShow());
                    break;
            }
        }

        private getSubTitleValue(): string {
            return StringExtensions.format("{1} ({0})", this._hardwareStationUrl() || "", this._hardwareStationDescription() || "");
        }
    }
}