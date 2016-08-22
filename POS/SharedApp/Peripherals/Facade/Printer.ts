/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.Core.d.ts'/>

module Commerce.Peripherals.Facade {
    "use strict";

    import PrintableReceipt = Model.Entities.PrintableReceipt;

    export class Printer implements IPrinter {

        private _printers: Dictionary<IPrinter>;

        constructor() {
            this._printers = new Dictionary<IPrinter>();
        }

        /**
         * Registers the printer within the facade.
         * @param {Model.Entities.PeripheralType} [type] The printers type.
         * @param {IPrinter} [printer] The printer.
         */
        public registerPrinter(type: Model.Entities.PeripheralType, printer: IPrinter): void {

            if (ObjectExtensions.isNullOrUndefined(printer)) {
                throw new Error("Printer instance is null or undefined.");
            }

            this._printers.setItem(type, printer);
        }

        /**
         * Prints the receipt.
         * @param {PrintableReceipt[]} printableReceipt[] The receipt objects.
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        public printAsync(printableReceipts: PrintableReceipt[], callerContext?: any): IVoidAsyncResult {


            var asyncResult: VoidAsyncResult = new VoidAsyncResult(callerContext);
            var errors: Model.Entities.Error[] = new Array();

            // Localize the receipt text.
            printableReceipts.forEach((printableReceipt: PrintableReceipt) => {
                printableReceipt.ReceiptHeader = ReceiptHelper.translateReceiptContent(printableReceipt.ReceiptHeader);
                printableReceipt.ReceiptBody = ReceiptHelper.translateReceiptContent(printableReceipt.ReceiptBody);
                printableReceipt.ReceiptFooter = ReceiptHelper.translateReceiptContent(printableReceipt.ReceiptFooter);
            });

            var receiptsGroupedByPrinterType: PrintableReceipt[][]
                = ObjectExtensions.groupBy<PrintableReceipt>(printableReceipts, (element: PrintableReceipt) => element.PrinterType);

            ObjectExtensions.forEachAsync(receiptsGroupedByPrinterType,
                (groupedPrintableReceipts: PrintableReceipt[], next: () => void, printerType: any) => {

                    if (!groupedPrintableReceipts) {
                        next();
                    } else {
                        if (this._printers.hasItem(printerType)) {

                            this._printers.getItem(printerType).printAsync(groupedPrintableReceipts)
                                .done(() => next())
                                .fail((error: Model.Entities.Error[]) => {
                                    errors = errors.concat(error);
                                    next();
                                });

                        } else {
                            var printerTypeNotFoundError: Model.Entities.Error =
                                new Model.Entities.Error(ErrorTypeEnum.PERIPHERAL_UNSUPPORTED_PRINTERTYPE_ERROR);
                            errors = errors.concat(printerTypeNotFoundError);
                            RetailLogger.peripheralsUnsupportedPrinterType(printerType);
                            next();
                        }
                    }
                },
                () => {
                    if (errors.length === 0) {
                        asyncResult.resolve();
                    } else {
                        asyncResult.reject(errors);
                    }
                }
                );

            return asyncResult;
        }
    }
}