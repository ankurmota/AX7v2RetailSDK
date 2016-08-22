/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Controls {
    "use strict";

    export class PrintableReceiptToDisplay extends Model.Entities.PrintableReceipt {
        public shouldPrintObservable: Observable<boolean>;
    }

    export interface IPrintReceiptDialogState {
        receipts: Model.Entities.Receipt[];
        associatedOrder?: Proxy.Entities.SalesOrder;
        rejectOnHardwareStationErrors: boolean;
        notifyOnNoPrintableReceipts?: boolean;
        ignoreShouldPrompt?: boolean;
        isCopyOfReceipt?: boolean;
    }

    export interface IPrintReceiptDialogOutput {
    }

    export class PrintReceiptDialog extends ModalDialog<IPrintReceiptDialogState, IPrintReceiptDialogOutput> {
        // Print Receipt Control objects
        private _OKButtonDisabled: Computed<boolean>;

        // Print Receipt Data objects
        private _receiptViewModel: Commerce.ViewModels.ReceiptViewModel;
        private _printableReceipts: Model.Entities.PrintableReceipt[];
        private _printableReceiptsToDisplay: ObservableArray<PrintableReceiptToDisplay>;

        private _dialogState: IPrintReceiptDialogState;

        constructor() {
            super();

            // Print Receipt objects
            this._receiptViewModel = new Commerce.ViewModels.ReceiptViewModel();
            this._printableReceipts = [];
            this._printableReceiptsToDisplay = ko.observableArray(<PrintableReceiptToDisplay[]>[]);

            // Computed objects
            this._OKButtonDisabled = ko.computed(() => { return !this.anyReceiptsToPrint(); }, this);
        }

        /**
         * Shows the dialog.
         * @param {Model.Entities.Receipt[]} receipts The list of receipts to print.
         */
        public onShowing(dialogState: IPrintReceiptDialogState): void {
            this._dialogState = dialogState;
            this._printableReceiptsToDisplay([]);
            this._printableReceipts = this._receiptViewModel.getPrintableReceipts(dialogState.receipts);

            var isHardwareStationActive: boolean = Peripherals.HardwareStation.HardwareStationContext.instance.isActive();
            var hasDevicePrinter: boolean = this._printableReceipts.some((receipt: Model.Entities.PrintableReceipt) =>
                receipt.PrinterType === Model.Entities.PeripheralType.Device);

            // Check if the prompt behavior was not overriden and if there is any receipt to be prompted
            var promptAnyReceipt: boolean = !dialogState.ignoreShouldPrompt
                && this._printableReceipts.some((receipt: Model.Entities.PrintableReceipt) => receipt.ShouldPrompt);

            // Try to print only if the one hardware station is active or if there is at least one device printer 
            if (isHardwareStationActive || hasDevicePrinter) {
                if (promptAnyReceipt) {
                    this._printableReceipts.forEach((printableReceipt: Model.Entities.PrintableReceipt) => {
                        // _printableReceipts and _printableReceiptsToDisplay should have the same number of receipts
                        var printReceiptToDisplay: Commerce.Controls.PrintableReceiptToDisplay =
                            <Commerce.Controls.PrintableReceiptToDisplay>$.extend(<Commerce.Controls.PrintableReceiptToDisplay>{
                                shouldPrintObservable: ko.observable(printableReceipt.ShouldPrint)
                            }, printableReceipt);

                        this._printableReceiptsToDisplay.push(printReceiptToDisplay);
                    });

                    RetailLogger.viewsControlsPrintReceiptShown();
                    this.visible(true);
                } else {
                    RetailLogger.viewsControlsPrintReceiptSkipped();
                    this.printReceipts(this._printableReceipts);
                }
            } else if (dialogState.rejectOnHardwareStationErrors) {
                // If there is no hardware station active nor device printer, then show an error (if not overriden)
                var notifyResult: IVoidAsyncResult = this.showNoPrintableReceiptsError();
                notifyResult.always(() => {
                    this.cancelDialog();
                });
            } else {
                this.cancelDialog();
            }
        }

        /**
         * Method called when a print receipt control button is clicked
         * @param {string} operationId The id of the button clicked
         */
        public printButtonClick(operationId: string): void {
            switch (operationId) {
                case Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.dontPrintReceiptsClicked();
                    break;
                case Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    this.printReceiptsClicked();
                    break;
            }
        }

        /**
         * Cancels the print dialog
         */
        private cancelDialog(): void {
            this.dialogResult.resolve(DialogResult.Cancel);
        }

        /**
         * Checks if there is any receipt to be printed.
         * @return {boolean} True if there is. Otherwise, false.
         */
        private anyReceiptsToPrint(): boolean {
            var printAnyReceipt: boolean = false;

            if (this._printableReceiptsToDisplay) {
                printAnyReceipt = this._printableReceiptsToDisplay()
                    .some((receipt: Commerce.Controls.PrintableReceiptToDisplay) => receipt.ShouldPrompt && receipt.shouldPrintObservable());
            }

            return printAnyReceipt;
        }

        /**
         * Action when "Print" is clicked
         */
        private printReceiptsClicked(): void {
            // Update the receipts in the view model
            for (var i: number = 0; i < this._printableReceiptsToDisplay().length; i++) {
                this._printableReceipts[i].ShouldPrint = this._printableReceiptsToDisplay()[i].shouldPrintObservable();
            }

            this.printReceipts(this._printableReceipts);
        }

        /**
         * Action when "Don't print" is clicked
         */
        private dontPrintReceiptsClicked(): void {
            // Disable the printing of all receipts prompted to the user
            for (var i: number = 0; i < this._printableReceiptsToDisplay().length; i++) {
                if (this._printableReceiptsToDisplay()[i].ShouldPrompt) {
                    this._printableReceipts[i].ShouldPrint = false;
                }
            }

            this.printReceipts(this._printableReceipts);
        }

        /**
         * Prints the receipts with ShouldPrint from the given list of receipts
         * @param {Model.Entities.PrintableReceipt[]} printableReceipts The list of receipts
         */
        private printReceipts(printableReceipts: Model.Entities.PrintableReceipt[]): void {
            var printAnyReceipt: boolean =
                printableReceipts.some((receipt: Proxy.Entities.PrintableReceipt) => receipt.ShouldPrint);

            // If no receipts to print, don't print
            if (!printAnyReceipt) {
                var notifyResult: IVoidAsyncResult = this.showNoPrintableReceiptsError();
                notifyResult.always(() => {
                    this.printReceiptsSuccess();
                });
            } else {
                this.indeterminateWaitVisible(true);
                this._receiptViewModel.printPrintableReceipts(printableReceipts, this._dialogState.associatedOrder, this._dialogState.isCopyOfReceipt)
                    .done((printingResult: ICancelableResult) => {
                        if (printingResult && !printingResult.canceled) {
                            this.printReceiptsSuccess();
                            RetailLogger.viewsControlsPrintReceiptPrinted();
                        } else {
                            this.dialogResult.resolve(DialogResult.Cancel);
                        }
                    }).fail((errors: Model.Entities.Error[]) => {
                        this.printReceiptsError(errors);
                    });
            }
        }

        /**
         * Shows an error that no receipts can be printed.
         * @return {IVoidAsyncResult} Async result.
         */
        private showNoPrintableReceiptsError(): IVoidAsyncResult {
            var notifyResult: IVoidAsyncResult;

            if (this._dialogState.notifyOnNoPrintableReceipts) {
                notifyResult = NotificationHandler.displayClientErrors([
                    new Proxy.Entities.Error(ErrorTypeEnum.MICROSOFT_DYNAMICS_POS_NO_PRINTABLE_RECEIPTS)
                ]);
            } else {
                notifyResult = VoidAsyncResult.createResolved();
            }

            return notifyResult;
        }

        /**
         * Shows the list of errors when printing
         * @param {Model.Entities.Error[]} errors The list of errors
         */
        private printReceiptsError(errors: Model.Entities.Error[]): void {
            this.indeterminateWaitVisible(false);
            NotificationHandler.displayClientErrors(errors, "string_1826")
                .done(() => this.dialogResult.resolve(DialogResult.Cancel));
        }

        /**
         * Callback when the printing is succeeded.
         */
        private printReceiptsSuccess(): void {
            this.dialogResult.resolve(DialogResult.OK);
        }

        
        /**
         * Retrieves the receipt title
         * @param {Model.Entities.PrintableReceipt} receipt The receipt being shown
         * @return {string} The receipt title
         */
        private getReceiptTitle(receipt: Model.Entities.PrintableReceipt): string {
            
            var receiptTitle: string = receipt.ReceiptName;
            var transactionStringRegEx: RegExp = new RegExp("<T:(.+?)>");
            var translationMatch: RegExpExecArray = transactionStringRegEx.exec(receiptTitle);

            while (translationMatch) {
                receiptTitle = receiptTitle.replace(translationMatch[0], Commerce.ViewModelAdapter.getResourceString(translationMatch[1]));
                translationMatch = transactionStringRegEx.exec(receiptTitle);
            }

            return receiptTitle;
        }
    }
}