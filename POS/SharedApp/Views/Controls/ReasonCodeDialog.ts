/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='AskQuestionDialog.ts'/>
///<reference path='DateInputDialog.ts'/>
///<reference path='ListInputDialog.ts'/>
///<reference path='TextInputDialog.ts'/>
///<reference path='NumberInputDialog.ts'/>
///<reference path='SelectDropdownDialog.ts'/>

module Commerce.Controls {
    "use strict";

    /**
     * The cancelable result with a reason code line.
     */
    interface IReasonCodeLineResult extends ICancelableResult {
        reasonCodeLine?: Proxy.Entities.ReasonCodeLine;
    }

    export class ReasonCodeDialog extends ModalDialog<Proxy.Entities.ReasonCode[], Proxy.Entities.ReasonCodeLine[]> {

        private _askQuestionDialog: AskQuestionDialog;
        private _dateInputDialog: DateInputDialog;
        private _listInputDialog: ListInputDialog;
        private _numberInputDialog: NumberInputDialog;
        private _textInputDialog: TextInputDialog;
        private _selectDropDownDialog: SelectDropDownDialog;

        constructor() {
            super();

            this.addControl(this._askQuestionDialog = new AskQuestionDialog());
            this.addControl(this._dateInputDialog = new DateInputDialog());
            this.addControl(this._listInputDialog = new ListInputDialog());
            this.addControl(this._numberInputDialog = new NumberInputDialog());
            this.addControl(this._textInputDialog = new TextInputDialog());
            this.addControl(this._selectDropDownDialog = new SelectDropDownDialog());
        }

        /**
         * Shows the modal dialog.
         *
         * @param {Proxy.Entities.ReasonCode[]} reasonCodes The reason codes.
         */
        public onShowing(reasonCodes: Proxy.Entities.ReasonCode[]) {
            if (!ArrayExtensions.hasElements(reasonCodes)) {
                this.dialogResult.resolve(DialogResult.Cancel);
                return;
            }

            var reasonCodeLines: Proxy.Entities.ReasonCodeLine[] = [];

            var asyncQueue = new AsyncQueue();

            // shows the reason code control for each of the reason codes
            reasonCodes.forEach(reasonCode => {
                asyncQueue.enqueue(() => {
                    return asyncQueue.cancelOn(this.showControlForReasonCode(reasonCode))
                        .done((result) => {
                            if (result.canceled) {
                                return;
                            }

                            reasonCodeLines.push(result.reasonCodeLine);
                        });
                });
            });

            // resolves/rejects the dialog result
            asyncQueue.run().done((result) => {
                if (result.canceled) {
                    this.dialogResult.resolve(DialogResult.Cancel);
                    return;
                }

                this.dialogResult.resolve(DialogResult.OK, reasonCodeLines);
            }).fail((errors) => { this.dialogResult.reject(errors); });

            this.visible(true);
        }

        /**
         * Hides the modal dialog.
         *
         * @return {IvoidAsyncResult} The async result that is resolved when the dialog is completely hidden.
         */
        public hide(): IVoidAsyncResult {
            super.hide();
            return VoidAsyncResult.createResolved();
        }

        /**
         * Shows the control for a reason code.
         *
         * @param {Proxy.Entities.ReasonCode} reasonCode The reason code.
         * @return IAsyncResult<IReasonCodeLineResult> The cancelable async result.
         */
        private showControlForReasonCode(reasonCode: Proxy.Entities.ReasonCode): IAsyncResult<IReasonCodeLineResult> {
            var asyncResult = new AsyncResult<IReasonCodeLineResult>(null);
            var dialog: ModalDialog<any, any> = null;
            var reasonCodeLine: Proxy.Entities.ReasonCodeLine = {
                ReasonCodeId: reasonCode.ReasonCodeId,
                InputTypeValue: reasonCode.InputTypeValue,
                SubReasonCodeId: "",
                Information: ""
            };

            // a max length of 0 means an infinite number of characters, however the max length supported is 100
            var maxLength: number = reasonCode.MaximumLength > 0 ? reasonCode.MaximumLength : 100;

            switch (reasonCode.InputTypeValue) {
                case Proxy.Entities.ReasonCodeInputTypeEnum.Text:
                case Proxy.Entities.ReasonCodeInputTypeEnum.None:
                    this._textInputDialog.title(reasonCode.Prompt);
                    this._textInputDialog.subTitle(this.subTitle());

                    // Get a valid input value for the reasoncode.
                    this.getValidInputValue(
                        this._textInputDialog,
                        { content: "", maxLength: maxLength },
                        reasonCode,
                        reasonCodeLine,
                        asyncResult);

                    dialog = this._textInputDialog;
                    break;

                case Proxy.Entities.ReasonCodeInputTypeEnum.SubCode:
                case Proxy.Entities.ReasonCodeInputTypeEnum.SubCodeButtons:
                    dialog = this._selectDropDownDialog;

                    if (reasonCode.InputTypeValue == Proxy.Entities.ReasonCodeInputTypeEnum.SubCodeButtons) {
                        dialog = this._listInputDialog;
                    }

                    dialog.title(reasonCode.Prompt);
                    dialog.subTitle(this.subTitle());

                    var listItems = reasonCode.ReasonSubCodes.map((reasonSubCode: Proxy.Entities.ReasonSubCode) => {
                        return reasonSubCode.Description;
                    });

                    dialog.show(listItems, false)
                        .on(DialogResult.OK, (selectedItem) => {
                            dialog.hide().done(() => {
                                var selectedReasonSubCode: Proxy.Entities.ReasonSubCode;
                                for (var itemIndex: number = 0; itemIndex < reasonCode.ReasonSubCodes.length; itemIndex++) {
                                    if (reasonCode.ReasonSubCodes[itemIndex].Description === selectedItem) {
                                        selectedReasonSubCode = reasonCode.ReasonSubCodes[itemIndex];
                                        break;
                                    }
                                }

                                reasonCodeLine.SubReasonCodeId = selectedReasonSubCode.SubCodeId;
                                reasonCodeLine.Information = selectedReasonSubCode.Description; // Save only input value description
                                asyncResult.resolve({ canceled: false, reasonCodeLine: reasonCodeLine });
                            });
                        });

                    break;

                case Proxy.Entities.ReasonCodeInputTypeEnum.Date:
                    var dateNow: Date = new Date();

                    this._dateInputDialog.title(reasonCode.Prompt);
                    this._dateInputDialog.subTitle(this.subTitle());
                    this._dateInputDialog.show({ minYear: (dateNow.getFullYear() - 200), maxYear: (dateNow.getFullYear() + 200) })
                        .on(DialogResult.OK, (date) => {
                            this._dateInputDialog.hide().done(() => {
                                reasonCodeLine.Information = date.toDateString(); // Save only input date value
                                asyncResult.resolve({ canceled: false, reasonCodeLine: reasonCodeLine });
                            });
                        });

                    dialog = this._dateInputDialog;
                    break;

                case Proxy.Entities.ReasonCodeInputTypeEnum.AgeLimit:
                    var dateNow: Date = new Date();

                    // Calculate the year for age limit.
                    var year: number = dateNow.getFullYear();
                    year = year - reasonCode.MinimumValue;

                    dateNow.setFullYear(year);

                    this._askQuestionDialog.title(reasonCode.Prompt);
                    this._askQuestionDialog.subTitle(this.subTitle());

                    this._askQuestionDialog.show({ content: StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_5922"), reasonCode.MinimumValue, dateNow.toLocaleDateString()) })
                        .on(DialogResult.Yes, (inputValue) => {
                            this._askQuestionDialog.hide().done(() => {
                                reasonCodeLine.Information = inputValue; // Save input value only
                                asyncResult.resolve({ canceled: false, reasonCodeLine: reasonCodeLine });
                            });
                        });

                    dialog = this._askQuestionDialog;
                    break;

                case Proxy.Entities.ReasonCodeInputTypeEnum.Numeric:
                    this._numberInputDialog.title(reasonCode.Prompt);
                    this._numberInputDialog.subTitle(this.subTitle());

                    // Get a valid input value for the reason code.
                    this.getValidInputValue(
                        this._numberInputDialog,
                        { content: 0, min: reasonCode.MinimumValue, max: reasonCode.MaximumValue },
                        reasonCode,
                        reasonCodeLine,
                        asyncResult);

                    dialog = this._numberInputDialog;
                    break;

                case Proxy.Entities.ReasonCodeInputTypeEnum.Item:
                case Proxy.Entities.ReasonCodeInputTypeEnum.Customer:
                    // Display a text input control to get the search term to use for product / customer search
                    this._textInputDialog.title(reasonCode.Prompt);
                    this._textInputDialog.subTitle(this.subTitle());

                    this._textInputDialog.show({ content: "", maxLength: maxLength }, false)
                        .on(DialogResult.OK, (inputValue) => {
                            this._textInputDialog.hide().done(() => {
                                var reasonCodeAsyncResult: AsyncResult<ICancelableDataResult<string>> = new AsyncResult<ICancelableDataResult<string>>(StringExtensions.EMPTY);

                                // navigate to the searchview with the right parameters
                                var options = {
                                    searchText: inputValue,
                                    reasonCodeAsyncResult: reasonCodeAsyncResult,
                                    addModeEnum: null,
                                    customerAddModeEnum: null,
                                    searchEntity: StringExtensions.EMPTY
                                };

                                if (reasonCode.InputTypeValue == Proxy.Entities.ReasonCodeInputTypeEnum.Item) {
                                    this._textInputDialog.title(Commerce.ViewModelAdapter.getResourceString("string_5923"));
                                    options.addModeEnum = ViewModels.ProductAddModeEnum.AddToReasonCode;
                                    options.searchEntity = "Products";
                                } else {
                                    this._textInputDialog.title(Commerce.ViewModelAdapter.getResourceString("string_5924"));
                                    options.customerAddModeEnum = ViewModels.CustomerAddModeEnum.AddToReasonCode;
                                    options.searchEntity = "Customers";
                                }

                                reasonCodeAsyncResult.done((result: ICancelableDataResult<string>) => {
                                    if (!result.canceled) {
                                        reasonCodeLine.Information = result.data; // Save input value only
                                    asyncResult.resolve({ canceled: false, reasonCodeLine: reasonCodeLine });
                                    } else {
                                        if (reasonCode.InputRequired) {
                                            asyncResult.resolve({ canceled: true });
                                        } else {
                                            asyncResult.resolve({ canceled: false, reasonCodeLine: reasonCodeLine });
                                        }
                                    }
                                })
                                    .fail((errors: Proxy.Entities.Error[]) => { asyncResult.reject(errors); });

                                Commerce.ViewModelAdapter.navigate("SearchView", options);
                            });
                        });

                    dialog = this._textInputDialog;

                    break;

                case Proxy.Entities.ReasonCodeInputTypeEnum.Staff:
                    var operators: Proxy.Entities.Employee[] = JSON.parse(ApplicationStorage.getItem(ApplicationStorageIDs.EMPLOYEE_LIST_KEY));

                    var listItems = operators.map((employee: Proxy.Entities.Employee) => {
                        return employee.Name;
                    });

                    this._selectDropDownDialog.title(reasonCode.Prompt);
                    this._selectDropDownDialog.subTitle(this.subTitle());

                    this._selectDropDownDialog.show(listItems, false)
                        .on(DialogResult.OK, (selectedValue) => {
                            this._selectDropDownDialog.hide().done(() => {
                                var selectedStaff: Proxy.Entities.Employee;
                                for (var itemIndex: number = 0; itemIndex < operators.length; itemIndex++) {
                                    if (operators[itemIndex].Name === selectedValue) {
                                        selectedStaff = operators[itemIndex];
                                        break;
                                    }
                                }

                                reasonCodeLine.Information = selectedStaff.StaffId; // Save input value only
                                asyncResult.resolve({ canceled: false, reasonCodeLine: reasonCodeLine});
                            });
                        });

                    dialog = this._selectDropDownDialog;
                    break;
                case Proxy.Entities.ReasonCodeInputTypeEnum.CompositeSubCodes:
                    var listItems = reasonCode.ReasonSubCodes.map((subCode: Proxy.Entities.ReasonSubCode) => {
                        return subCode.Description;
                    });

                    this._selectDropDownDialog.title(reasonCode.Prompt);
                    this._selectDropDownDialog.subTitle(this.subTitle());

                    this._selectDropDownDialog.show(listItems, false)
                        .on(DialogResult.OK, (selectedValue) => {
                            this._selectDropDownDialog.hide().done(() => {
                                var selectedSubCode: Proxy.Entities.ReasonSubCode;
                                for (var itemIndex: number = 0; itemIndex < reasonCode.ReasonSubCodes.length; itemIndex++) {
                                    if (reasonCode.ReasonSubCodes[itemIndex].Description === selectedValue) {
                                        selectedSubCode = reasonCode.ReasonSubCodes[itemIndex];
                                        break;
                                    }
                                }

                                // Composite subcodes is several reason codes clubbed together as subcodes under one reason code for display uniformity.
                                // They need special processing to be applied correctly.
                                reasonCodeLine.ReasonCodeId = selectedSubCode.SubCodeId;
                                reasonCodeLine.Information = selectedSubCode.Description; // Save input value only
                                asyncResult.resolve({ canceled: false, reasonCodeLine: reasonCodeLine });
                            });
                        });

                    dialog = this._selectDropDownDialog;
                    break;
                default:
                    asyncResult.resolve({ canceled: false, reasonCodeLine: reasonCodeLine });
                    break;
            }

            // handles cancel
            if (dialog) {
                dialog.dialogResult.on(DialogResult.Cancel, (result) => {
                    dialog.hide().done(() => {
                        asyncResult.resolve({ canceled: true });
                    });
                });
            }

            return asyncResult;
        }


        /**
         * Gets a valid input for a given reason code. Automatically retries until a valid value is entered or the reasoncode is cancelled.
         *
         * @param {dialog: ModalDialog<any, any>} dialog The dialog to use to get the reason code.
         * @param {dialogArgs: any} dialogArgs The arguments to pass into the dialog.
         * @param {reasonCode: Proxy.Entities.ReasonCode} reasonCode The reason code to use for validation.
         * @param {reasonCodeLine: Proxy.Entities.ReasonCodeLine} reasonCodeLine The reason code line to use for the result.
         * @param {asyncResult: AsyncResult<IReasonCodeLineResult>} asyncResult The asyncresult to resolve on valid input.
         */
        private getValidInputValue(
            dialog: ModalDialog<any, any>,
            dialogArgs: any,
            reasonCode: Proxy.Entities.ReasonCode,
            reasonCodeLine: Proxy.Entities.ReasonCodeLine,
            asyncResult: AsyncResult<IReasonCodeLineResult>): void {

            dialog.show(dialogArgs, true)
                .on(DialogResult.OK, (inputValue) => {
                    var error: Proxy.Entities.Error;

                    switch (reasonCode.InputTypeValue) {
                        case Proxy.Entities.ReasonCodeInputTypeEnum.Text:
                        case Proxy.Entities.ReasonCodeInputTypeEnum.None:
                            if (inputValue.length < reasonCode.MinimumLength) {
                                error = new Proxy.Entities.Error(ErrorTypeEnum.REASONCODE_LENGTH_SHORT);
                            } else if (reasonCode.MaximumLength > 0 && inputValue.length > reasonCode.MaximumLength) {
                                error = new Proxy.Entities.Error(ErrorTypeEnum.REASONCODE_LENGTH_EXCEEDED);
                            }
                            break;
                    };

                    if (!ObjectExtensions.isNullOrUndefined(error)) {
                        Commerce.NotificationHandler.displayClientErrors([error]).done(() => {
                            this.getValidInputValue(dialog, dialogArgs, reasonCode, reasonCodeLine, asyncResult);

                            dialog.dialogResult.on(DialogResult.Cancel, (result) => {
                                dialog.hide().done(() => {
                                    asyncResult.resolve({ canceled: true });
                                });
                            });
                        });
                    } else {
                        reasonCodeLine.Information = inputValue.toString(); // Save input value only
                        asyncResult.resolve({ canceled: false, reasonCodeLine: reasonCodeLine });
                    }
                });
        }
    }
}
