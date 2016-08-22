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

    export interface AutocompleteComboboxDialogState {
        dataSource: AutocompleteDataItem[];
    }

    /**
     * Class that defines the data item of Autocomplete control.
     *
     */
    export class AutocompleteDataItem {
        public value: string;
        public description: string;
    }

    export class AutocompleteComboboxDialog extends ModalDialog<AutocompleteComboboxDialogState, string> {

        private _searchText: Observable<string>;
        private _dataSource: AutocompleteDataItem[];
        private _displaySuggestionList: ObservableArray<AutocompleteDataItem>;
        private _oKButtonDisabled: Computed<boolean>;
        private _suggestionListVisible: Computed<boolean>;

        constructor() {
            super();

            this._searchText = ko.observable<string>("");

            this._displaySuggestionList = ko.observableArray<AutocompleteDataItem>([]);

            // Computed objects
            this._oKButtonDisabled = ko.computed(() => { return !ArrayExtensions.hasElements(this._displaySuggestionList()); }, this);
            this._suggestionListVisible = ko.computed(() => { return !this._oKButtonDisabled(); }, this);
        }

        /**
         * Shows the control
         *
         * @param {AutocompleteComboboxDialogState} state The modal dialog state.
         */
        public show(state: AutocompleteComboboxDialogState) {
            if (state) {
                this._dataSource = state.dataSource;
                this._displaySuggestionList(this._dataSource);
            }

            return super.show(state);
        }

        /**
         * Button click handler
         *
         * @param {string} buttonId The identifier of the button.
         */
        private buttonClickHandler(buttonId: string) {

            var selectedValue: string = StringExtensions.EMPTY;
            if (buttonId === Controls.Dialog.OperationIds.OK_BUTTON_CLICK) {
                // Return the first suggestion item key if click the OK button.
                selectedValue = this._displaySuggestionList()[0].value;
            }

            this.closeDialog(buttonId, selectedValue);
        }

        private searchClick(): void {
            this._displaySuggestionList([]);

            if (this._searchText().length > 0) {
                for (var i: number = 0; i < this._dataSource.length; i++) {
                    // Query the matched item accroding to the value or the description of the suggestionList.
                    if (this.queryExistedSameSubString(this._dataSource[i].value, this._searchText(), true)
                        || this.queryExistedSameSubString(this._dataSource[i].description, this._searchText(), true)) {
                        this._displaySuggestionList.push(this._dataSource[i]);
                    }
                }
            }
            else {
                this._displaySuggestionList(this._dataSource);
            }
        }

        private suggestionItemInvokedHandler(item: AutocompleteDataItem): void {
            this.closeDialog(Controls.Dialog.OperationIds.OK_BUTTON_CLICK, item.value);
        }

        private closeDialog(operationId: string, selectedValue: string): void {

            this._searchText("");

            switch (operationId) {
                case Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    // Return the first suggestion item key if click the OK button.
                    this.dialogResult.resolve(DialogResult.OK, selectedValue);
                    break;
                case Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.Cancel);
                    break;
            }
        }

        private queryExistedSameSubString(sourceString: string, queryString: string, ignoreCase: boolean): boolean {

            var isExisted = this.queryExistedSameSubWord(sourceString, queryString, ignoreCase);

            if (!isExisted) {
                // Split the sourceString to an individual word array and compare the queryString with each word
                var sourceStringWordArray: string[] = sourceString.split(' ');
                if (sourceStringWordArray.length > 1) {
                    for (var wordIndex = 0; wordIndex < sourceStringWordArray.length; wordIndex++) {
                        isExisted = this.queryExistedSameSubWord(sourceStringWordArray[wordIndex], queryString, ignoreCase);
                        if (isExisted) {
                            break;
                        }
                    }
                }
            }

            return isExisted;
        }

        private queryExistedSameSubWord(sourceWordString: string, queryWordString: string, ignoreCase: boolean): boolean {
            if (StringExtensions.isNullOrWhitespace(sourceWordString) || StringExtensions.isNullOrWhitespace(queryWordString)) {
                return false;
            }

            if (ignoreCase) {
                queryWordString = queryWordString.toLowerCase();
            }

            for (var startCharIndex = 0, endCharIndex = sourceWordString.length - queryWordString.length + 1; startCharIndex < endCharIndex; startCharIndex++) {
                if (startCharIndex === 0 || sourceWordString.substr(startCharIndex - 1, 1) === " ") {
                    var subString: string = sourceWordString.substr(startCharIndex, queryWordString.length);
                    if (ignoreCase) {
                        subString = subString.toLowerCase();
                    }

                    if (subString === queryWordString) {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}