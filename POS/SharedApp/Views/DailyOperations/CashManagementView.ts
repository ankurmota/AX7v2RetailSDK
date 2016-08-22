/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.ViewModels.d.ts'/>
///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    export class CashManagementViewController extends ViewControllerBase {
        public commonHeaderData;
        public storeOperationsViewModel: Commerce.ViewModels.StoreOperationsViewModel;
        public cashManagementType: Proxy.Entities.TransactionType;
        public indeterminateWaitVisible: Observable<boolean>;
        public amountToChange: Observable<string>;
        public storeCurrency: Observable<string>;
        public shiftToUse: Model.Entities.Shift;
        public totalAmountToDeclare: Observable<number>;
        public operationComment: Observable<string>;
        public isSaveDisabled: Observable<boolean>;

        private _declaredAmount: number;
        private _amountEntered: number;
        private _navigationDestination: string;
        private _printReceiptDialog: Controls.PrintReceiptDialog;
        private _processingOperation: boolean;

        constructor(options: any) {
            super(true);

            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.storeOperationsViewModel = new Commerce.ViewModels.StoreOperationsViewModel();
            this.cashManagementType = options.nonSalesTenderType;
            this.indeterminateWaitVisible = ko.observable(false);
            this.amountToChange = ko.observable("");
            this.operationComment = ko.observable("");
            this.totalAmountToDeclare = ko.observable(0);
            this.storeCurrency = ko.observable(Commerce.ApplicationContext.Instance.deviceConfiguration.Currency);
            this.addControl(this._printReceiptDialog = new Controls.PrintReceiptDialog());
            this.isSaveDisabled = ko.observable(true);

            if (!ObjectExtensions.isNullOrUndefined(options.shift)) {
                this.shiftToUse = options.shift;
                this._navigationDestination = "BlindCloseView";
            }
            else {
                this.shiftToUse = Session.instance.Shift;
                this._navigationDestination = "HomeView";
            }

            //Load Common Header
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_4007"));

            this._declaredAmount = 0;
            this._amountEntered = 0;
            this._processingOperation = false;

            if (this.cashManagementType == Proxy.Entities.TransactionType.StartingAmount) {
                this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_4154"));
                this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_4008"));
            }
            else if (this.cashManagementType == Proxy.Entities.TransactionType.FloatEntry) {
                this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_4160"));
                this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_4009"));
            }
            else if (this.cashManagementType == Proxy.Entities.TransactionType.RemoveTender) {
                this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_4010"));
            }
        }

        public load(): void {
            if (this.cashManagementType == Proxy.Entities.TransactionType.StartingAmount) {
                this.indeterminateWaitVisible(true);
                this.storeOperationsViewModel.getDeclaredStartingAmount(this.shiftToUse)
                    .done((declaredAmount) => {
                        this.indeterminateWaitVisible(false);
                        this._declaredAmount = declaredAmount;
                        this.totalAmountToDeclare(declaredAmount);
                    }).fail((errors) => {
                        this.indeterminateWaitVisible(false);
                        Commerce.NotificationHandler.displayClientErrors(errors, "string_4179");
                    });
            }
        }

        public enterAmount(numPadResult: Controls.NumPad.INumPadResult): void {
            var amount: number = NumberExtensions.parseNumber(this.amountToChange());

            if (isNaN(amount) || amount < 0) {
                Commerce.NotificationHandler.displayClientErrors([new Commerce.Model.Entities.Error(Commerce.ErrorTypeEnum.AMOUNT_IS_NOT_VALID)]);
            } else {
                // The dialog if total amount is not 0.
                if (this.totalAmountToDeclare() != 0 && this.cashManagementType == Proxy.Entities.TransactionType.StartingAmount) {
                    ViewModelAdapter.displayMessage(Commerce.ViewModelAdapter.getResourceString("string_4155"),
                        MessageType.Info, MessageBoxButtons.YesNo)
                        .done((result: DialogResult) => {
                            if (result === DialogResult.Yes) {
                                this.overrideTotalAmount(amount);
                            }
                        });
                } else {
                    this.overrideTotalAmount(amount);
                }
            }
        }

        public endOperation(): void {
            Commerce.ViewModelAdapter.navigate(this._navigationDestination);
        }

        /**
         * Changes the amount to update.
         */
        private overrideTotalAmount(amount: number): void {
            this.totalAmountToDeclare(this.totalAmountToDeclare() + amount);
            // Used for updating starting amount.
            this._amountEntered += amount;
            // Clear current amount.
            this.amountToChange(null);
            // Enables save command
            this.isSaveDisabled(false);
        }

        private operationSuccessCallback(receipts: Model.Entities.Receipt[]): void {
            this.indeterminateWaitVisible(false);
            var dialogState: Controls.IPrintReceiptDialogState = { receipts: receipts, rejectOnHardwareStationErrors: true };
            this.showDialog(this._printReceiptDialog, dialogState)
                .on(DialogResult.OK, (result: Controls.IPrintReceiptDialogOutput) => { this.endOperation(); })
                .on(DialogResult.Cancel,(result: Controls.IPrintReceiptDialogOutput) => { this.endOperation(); })
                .onError((errors: Model.Entities.Error[]) => {
                    RetailLogger.viewsDailyOperationsCashManagementViewOperationFailed(errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
                    this.endOperation();
                });
        }

        private operationErrorCallback(errors: Model.Entities.Error[]): void {
            Commerce.NotificationHandler.displayClientErrors(errors)
                .done(() => {
                    this.indeterminateWaitVisible(false);
                    this._processingOperation = false;
                }).fail(() => {
                    this.indeterminateWaitVisible(false);
                    this._processingOperation = false;
                });
        }

        /**
         * Shows a modal dialog and handle default results.
         */
        private showDialog<T, U>(dialog: Controls.ModalDialog<T, U>, input: T): IAsyncDialogResult<U> {
            return dialog.show(input)
                .onError((errors) => { Commerce.NotificationHandler.displayClientErrors(errors); });
        }

        public processOperation(): void {
            // Don't process the operation again, if an operation is already being processed.
            if (this._processingOperation) {
                return;
            }

            this._processingOperation = true;
            this.indeterminateWaitVisible(true);

            var result: IAsyncResult<Model.Entities.Receipt[]>;

            if (this.cashManagementType == Proxy.Entities.TransactionType.StartingAmount) {
                result = this.storeOperationsViewModel.declareStartAmount(this._amountEntered, this.operationComment(), this.shiftToUse);
            } else if (this.cashManagementType == Proxy.Entities.TransactionType.FloatEntry) {
                result = this.storeOperationsViewModel.floatEntry(this._amountEntered, this.operationComment(), this.shiftToUse);
            } else if (this.cashManagementType == Proxy.Entities.TransactionType.RemoveTender) {
                result = this.storeOperationsViewModel.tenderRemove(this._amountEntered, this.operationComment(), this.shiftToUse);
            }

            if (result) {
                result.done((receipts) => { this.operationSuccessCallback(receipts); })
                    .fail((errors) => { this.operationErrorCallback(errors); });
            } else {
                this._processingOperation = false;
                this.indeterminateWaitVisible(false);
            }
        }
    }
}
