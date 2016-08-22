/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ViewModelBase.ts'/>

module Commerce.ViewModels {
    "use strict";

    export class ReceiptViewModel extends ViewModelBase {

        public printableReceipts: ObservableArray<Model.Entities.PrintableReceipt>;

        constructor() {
            super();
            this.printableReceipts = ko.observableArray<Proxy.Entities.PrintableReceipt>([]);
        }

        /*
         * Converts an array of receipts to printable receipts
         * @param {Proxy.Entities.Receipt[]} receipts The list of receipts to convert
         * @return {Proxy.Entities.PrintableReceipt[]} The array of printable receipts
         */
        public getPrintableReceipts(receipts: Proxy.Entities.Receipt[]): Proxy.Entities.PrintableReceipt[] {
            var printableReceiptArray: Proxy.Entities.PrintableReceipt[] = [];

            if (ArrayExtensions.hasElements(receipts)) {
                receipts.forEach((receipt: Proxy.Entities.Receipt) => {
                    var printerList: Proxy.Entities.Printer[] = [];

                    if (ArrayExtensions.hasElements(receipt.Printers)) {
                        printerList = receipt.Printers;
                    }

                    printerList.forEach((printer: Proxy.Entities.Printer) => {
                        var printableReceipt: Proxy.Entities.PrintableReceipt = new Proxy.Entities.PrintableReceipt(receipt, printer);
                        if (printableReceipt.ShouldPrint) {
                            printableReceiptArray.push(printableReceipt);
                        }
                    });
                });
            }

            return printableReceiptArray;
        }

        /*
         * Prints the list of printable receipts that are set to print
         * @param {Model.Entities.PrintableReceipt[]} printableReceipts The list of possible receipts to print
         * @param {Proxy.Entities.SalesOrder} associatedOrder The sales order associated with the receipt.
         * @param {boolean} isCopyOfReceipt Whether or not we are printing another copy of the receipt.
         * @return {IAsyncResult<ICancelableResult>} The async result.
         */
        public printPrintableReceipts(
            printableReceipts: Proxy.Entities.PrintableReceipt[],
            associatedOrder?: Proxy.Entities.SalesOrder,
            isCopyOfReceipt?: boolean): IAsyncResult<ICancelableResult> {

            if (!ArrayExtensions.hasElements(printableReceipts)) {
                return AsyncResult.createResolved<ICancelableResult>({ canceled: false });
            }

            // Filter the receipts to get the set of receipts set to print
            var receiptsToPrint: Model.Entities.PrintableReceipt[] = printableReceipts.filter((printableReceipt: Model.Entities.PrintableReceipt) => {
                return printableReceipt.ShouldPrint;
            });

            var printingQueue: AsyncQueue = new AsyncQueue();

            if (isCopyOfReceipt) {
                printingQueue.enqueue((): IAsyncResult<ICancelableResult> => {
                    var preTriggerOptions: Triggers.IPrePrintReceiptCopyTriggerOptions = { salesOrder: associatedOrder, receipts: receiptsToPrint };
                    var preTriggerResult: IAsyncResult<ICancelableResult> =
                        Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PrePrintReceiptCopy, preTriggerOptions);

                    return printingQueue.cancelOn(preTriggerResult);
                });
            }

            printingQueue.enqueue((): IVoidAsyncResult => {
                // Print the receipts
                return Commerce.Peripherals.instance.printer.printAsync(receiptsToPrint);
            });

            return printingQueue.run();
        }

        /**
         * Determines if a receipt can be printed.
         * @param {Model.Entities.Receipt} receipt The receipt.
         * @return {boolean} True: if the receipt can be printed. False: otherwise.
         */
        public canReceiptBePrinted(receipt: Model.Entities.Receipt): boolean {
            var canBePrinted: boolean = false;

            if (receipt != null && ArrayExtensions.hasElements(receipt.Printers)) {
                canBePrinted = !receipt.Printers.every((printer: Proxy.Entities.Printer) =>
                    printer.PrintBehaviorValue === Model.Entities.PrintBehavior.Never);
            }

            return canBePrinted;
        }
    }
}
