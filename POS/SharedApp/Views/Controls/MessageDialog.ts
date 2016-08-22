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
    export interface MessageDialogState {
        title?: string;
        content: string;
        additionalInfo?: string;
        buttons: any[];
    }

    export interface CheckboxMessageDialogState extends MessageDialogState {
        messageCheckboxVisible: boolean; // Indicates whether the display message checkbox should be displayed. By default it is not displayed.
        messageCheckboxChecked: boolean; // Indicates whether the display message checkbox should be checked. By default it is not checked.
        messageCheckboxLabelResourceID: string; // The resource ID of the text to display for the label. Do not set to use default label.
    }

    export class MessageDialog extends ModalDialog<MessageDialogState, Commerce.DialogResult> {

        private content: Observable<string>;
        private additionalInfo: Observable<string>;

        private messageCheckboxVisible: Observable<boolean>;
        private messageCheckboxChecked: Observable<boolean>;
        private messageCheckboxLabel: Observable<string>;

        public buttons: any[];
         
        
        /**
         * Gets whether the message checkbox is checked
         *
         * @return True if the message checkbox is checked, false otherwise.
         */
        get MessageCheckboxChecked(): boolean {
            return this.messageCheckboxChecked();
        }
        
        /**
         * Constructor.
         */  
        constructor() {
            super();
                
            // Control objects
            this.content = ko.observable(StringExtensions.EMPTY);
            this.additionalInfo = ko.observable(null);
            this.messageCheckboxVisible = ko.observable(false);
            this.messageCheckboxChecked = ko.observable(false);
            this.messageCheckboxLabel = ko.observable(StringExtensions.EMPTY);
        }

        /**
         * Sets the dialog state.
         *
         * @param {AskQuestionDialogState} dialogState The modal dialog state.
         */
        private setDialogState(dialogState: MessageDialogState): void {
            // Check the parameter
            if (ObjectExtensions.isNullOrUndefined(dialogState)) {
                return;
            }

            this.buttons = dialogState.buttons;
            this.content(dialogState.content);

            if (!StringExtensions.isNullOrWhitespace(dialogState.title)) {
                this.title(dialogState.title);
            } else {
                this.title(StringExtensions.EMPTY);
            }

            if (!StringExtensions.isNullOrWhitespace(dialogState.additionalInfo)) {
                this.additionalInfo(dialogState.additionalInfo);
            } else {
                this.additionalInfo(StringExtensions.EMPTY);
            }

            // Set the checkbox settings
            var checkedDialogState: any = dialogState;
            if (checkedDialogState.messageCheckboxVisible) {
                this.messageCheckboxVisible(true);
            }

            if (checkedDialogState.messageCheckboxChecked) {
                this.messageCheckboxChecked(true);
            }

            var labelToDisplay: string = Commerce.ViewModelAdapter.getResourceString('string_7550'); // Do not tell me again
            if (!ObjectExtensions.isNullOrUndefined(checkedDialogState.messageCheckboxLabelResourceID)) {
                var label: string = Commerce.ViewModelAdapter.getResourceString(checkedDialogState.messageCheckboxLabelResourceID);
                if (label != checkedDialogState.messageCheckboxLabelResourceID) {
                    labelToDisplay = label;
                }
            }
            this.messageCheckboxLabel(labelToDisplay);
        }

        /**
         * Shows the modal dialog.
         *
         * @param {MessageDialogState} dialogState The modal dialog state.
         * @param {boolean} [hideOnResult] Whether or not to hide the dialog after a result is provided.
         * @return {IAsyncResult<DialogResult>} Returns dialog result.
         */
        public show(dialogState: MessageDialogState, hideOnResult: boolean = true): IAsyncDialogResult<DialogResult> {
            this.setDialogState(dialogState);          
            return super.show(dialogState, hideOnResult);
        }

        /**
         * Shows the modal dialog.
         *
         * @param {AskQuestionDialogState} dialogState The modal dialog state.
         */
        public onShowing(dialogState: MessageDialogState) {
            this.setDialogState(dialogState);  
            this.visible(true);
        }

        /**
         * Button click handler
         *
         * @param {string} buttonId The identifier of the button.
         */
        private buttonClickHandler(result: string) {
            switch (result) {
                case Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.Yes, DialogResult.Yes);
                    break;

                case Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.No, DialogResult.No);
                    break;
                
                case Controls.Dialog.OperationIds.CLOSE_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.No, DialogResult.No);
                    break;
            }
        
        }
    }
}