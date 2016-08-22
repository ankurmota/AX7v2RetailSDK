/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../../SharedApp/Commerce.Core.d.ts'/>

module Commerce.Peripherals.Native {
    "use strict";

    /**
     * With TypeScript compiler version 1.5, the MSApp had some methods removed.
     * This interface is not public on purpose.
     */
    interface IExtendedMSApp extends MSApp {
        getHtmlPrintDocumentSource(htmlDoc: any): any;
    }

    export class Printer implements IPrinter {

        private static PRINT_REQUEST_EVENT = "printtaskrequested";
        private _printManager: Windows.Graphics.Printing.PrintManager;
        private _barcodeMap: Lazy<any[]>;

        constructor() {
            this._printManager = Windows.Graphics.Printing.PrintManager.getForCurrentView();
            this._barcodeMap = new Lazy<any[]>(() => this.initializeBarcodeMap());
        }

        /**
         * Prints the receipt.
         *
         * @param {PrintableReceipt[]} printableReceipt[] The receipt objects.
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        public printAsync(printableReceipts: Model.Entities.PrintableReceipt[], callerContext?: any): IVoidAsyncResult {

            var self = this;
            var asyncResult = new VoidAsyncResult(callerContext);

            var onPrintTaskRequested = (printEvent) => {
                var printTask = printEvent.request.createPrintTask(Commerce.ViewModelAdapter.getResourceString("string_3"), (args) => {
                    // Do not use prepareContentUnsafe to render to a page without HTML escaping.
                    args.setSource((<IExtendedMSApp>MSApp).getHtmlPrintDocumentSource(self.prepareContentUnsafe(printableReceipts)));

                    printTask.oncompleted = (printTaskCompletionEvent) => {
                        self._printManager.onprinttaskrequested = null;
                    }
                });
            }

            self._printManager.onprinttaskrequested = onPrintTaskRequested;

            Windows.Graphics.Printing.PrintManager.showPrintUIAsync()
                .done((result) => {
                    if (result) {
                        asyncResult.resolve();
                    } else {
                        asyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.PERIPHERALS_PRINTER_FAILED)]);
                    }
                });

            return asyncResult;
        }

        /**
         *  Returns an HTML document fragment representing the receipt.
         *  Do not use this to render to a page without HTML escaping.
        */
        private prepareContentUnsafe(printableReceipts: Model.Entities.PrintableReceipt[]): DocumentFragment {
            var textEscapeMarker = '&#x1B;';
            var textBoldMarker = textEscapeMarker + '|2C';
            var textNewLineMarker = '\r\n';
            var htmlBoldTagStart = '<b>';
            var htmlBoldTagEnd = '</b>';
            var htmlSpaceTag = '&nbsp;';
            var htmlNewLineTag = '<br/>';
            var htmlNewLineTagName = 'br';
            var htmlDivTag = 'div';
            var htmlCanvasTag = 'canvas';
            var htmlFontTagStart = '<font face="Courier New" size=1>';
            var htmlFontTagEnd = '</font>';
            var htmlPageBreakStyle = 'always';
            var barCodeRegEx = "<B: (.+?)>";
            var localizationStringRegEx = "<T:(.+?)>";
            var documentFragment = document.createDocumentFragment();

            printableReceipts.forEach((receipt) => {

                var printBuffer = ((receipt.ReceiptHeader || '') + (receipt.ReceiptBody || '') + (receipt.ReceiptFooter || '')).trim();

                // Convert bold text markers.
                while (true) {
                    var boldMarkerIndex = printBuffer.indexOf(textBoldMarker);

                    if (boldMarkerIndex < 0)
                        break;

                    var endOfLineMarkerIndex = printBuffer.indexOf(textNewLineMarker, boldMarkerIndex);
                    var nextStyleMarkerIndex = printBuffer.indexOf(textEscapeMarker, boldMarkerIndex + 1);
                    var endOfBoldMarkerIndex = Math.min(endOfLineMarkerIndex, nextStyleMarkerIndex);

                    // The entire bold item, e.g: "012532    ". 
                    var entireBoldItem = printBuffer.substring(boldMarkerIndex + textBoldMarker.length, endOfBoldMarkerIndex);
                    var emptySpaces = Array(entireBoldItem.length + 1).join(" ");

                    printBuffer = (printBuffer.substring(0, endOfBoldMarkerIndex) + htmlBoldTagEnd + emptySpaces + printBuffer.substring(endOfBoldMarkerIndex))
                        .replace(textBoldMarker, htmlBoldTagStart);
                }

                // Check if barcode mask is present.
                var regExp = new RegExp(barCodeRegEx);
                var match = regExp.exec(printBuffer)
                var barcodes: string[] = [];
                var i = 0;
                while (match) {
                    barcodes[i++] = match[1];

                    // Remove barcode marker from buffer
                    printBuffer = printBuffer.replace(match[0], StringExtensions.EMPTY);

                    match = regExp.exec(printBuffer)
                }

                // Replacing localization strings inside the buffer
                match = (new RegExp(localizationStringRegEx)).exec(printBuffer);
                while (match) {
                    printBuffer = printBuffer.replace(match[0], Commerce.ViewModelAdapter.getResourceString(match[1]));
                    match = (new RegExp(localizationStringRegEx)).exec(printBuffer);
                }

                printBuffer = htmlFontTagStart
                    + printBuffer.replace(/&#x1B;\|1C/g, StringExtensions.EMPTY).replace(/\r\n/g, htmlNewLineTag).replace(/\s/g, htmlSpaceTag)
                    + htmlFontTagEnd

                var divElement = document.createElement(htmlDivTag);

                divElement.innerHTML = printBuffer;
                documentFragment.appendChild(divElement);

                // Create barcode element if required.
                if (barcodes) {
                    barcodes.forEach((barcode) => {
                        var canvasElement = document.createElement(htmlCanvasTag);

                        documentFragment.appendChild(canvasElement);
                        documentFragment.appendChild(document.createElement(htmlNewLineTagName))
                        this.renderBarcode(canvasElement, barcode);
                    });
                }

                // Create page break element.
                var divElement = document.createElement(htmlDivTag);

                documentFragment.appendChild(divElement);
                divElement.style.pageBreakAfter = 'always';
            });

            return documentFragment;
        }

        private renderBarcode(canvasElement: any, text: string): void {
            var barcodeText = this.formatBarcodeText(text);
            var encodedBarcode = this.encodeBarcode(barcodeText);
            var startX = 0;
            var strokeWidth = 1.5
            var barcodeHeight = 40;
            var strokeCount = 0;
            var fontSize = 12;
            var canvasContext = canvasElement.getContext("2d");
            var currentStroke = encodedBarcode[0];

            canvasElement.style.backgroundColor = '#000000';
            canvasContext.canvas.width = encodedBarcode.length * strokeWidth;
            canvasContext.canvas.height = barcodeHeight + fontSize;

            for (var x = 0; x < encodedBarcode.length; x++) {
                // Group similar stokes for better rendering.
                if (currentStroke == encodedBarcode[x]) {
                    strokeCount++;
                    continue;
                }

                if (currentStroke == '1') {
                    canvasContext.fillRect(startX + (x - strokeCount) * strokeWidth, 0, strokeWidth * strokeCount, barcodeHeight);
                }
                currentStroke = encodedBarcode[x];
                strokeCount = 1;
            }

            // Render text on the bottom
            canvasContext.font = fontSize + 'pt Courier New';
            canvasContext.fillText(barcodeText,
                startX + Math.floor((encodedBarcode.length * strokeWidth - canvasContext.measureText(barcodeText).width) / 2),
                barcodeHeight + fontSize);
        }

        private initializeBarcodeMap(): any[] {
            var barcodeMap = [];

            // Code39 based barcode mask array
            barcodeMap['0'] = "101001101101";
            barcodeMap['1'] = "110100101011";
            barcodeMap['2'] = "101100101011";
            barcodeMap['3'] = "110110010101";
            barcodeMap['4'] = "101001101011";
            barcodeMap['5'] = "110100110101";
            barcodeMap['6'] = "101100110101";
            barcodeMap['7'] = "101001011011";
            barcodeMap['8'] = "110100101101";
            barcodeMap['9'] = "101100101101";
            barcodeMap['A'] = "110101001011";
            barcodeMap['B'] = "101101001011";
            barcodeMap['C'] = "110110100101";
            barcodeMap['D'] = "101011001011";
            barcodeMap['E'] = "110101100101";
            barcodeMap['F'] = "101101100101";
            barcodeMap['G'] = "101010011011";
            barcodeMap['H'] = "110101001101";
            barcodeMap['I'] = "101101001101";
            barcodeMap['J'] = "101011001101";
            barcodeMap['K'] = "110101010011";
            barcodeMap['L'] = "101101010011";
            barcodeMap['M'] = "110110101001";
            barcodeMap['N'] = "101011010011";
            barcodeMap['O'] = "110101101001";
            barcodeMap['P'] = "101101101001";
            barcodeMap['Q'] = "101010110011";
            barcodeMap['R'] = "110101011001";
            barcodeMap['S'] = "101101011001";
            barcodeMap['T'] = "101011011001";
            barcodeMap['U'] = "110010101011";
            barcodeMap['V'] = "100110101011";
            barcodeMap['W'] = "110011010101";
            barcodeMap['X'] = "100101101011";
            barcodeMap['Y'] = "110010110101";
            barcodeMap['Z'] = "100110110101";
            barcodeMap['-'] = "100101011011";
            barcodeMap['.'] = "110010101101";
            barcodeMap[' '] = "100110101101";
            barcodeMap['$'] = "100100100101";
            barcodeMap['/'] = "100100101001";
            barcodeMap['+'] = "100101001001";
            barcodeMap['%'] = "101001001001";
            barcodeMap['*'] = "100101101101";

            return barcodeMap;
        }

        private encodeBarcode(barcodeText: string): string {
            var result = StringExtensions.EMPTY;

            for (var x = 0; x < barcodeText.length; x++) {
                result += this._barcodeMap.value[barcodeText.substr(x, 1).toUpperCase()];
                result += '0'; //WhiteSpace
            }

            return result;
        }

        private formatBarcodeText(text: string): string {
            var result = StringExtensions.EMPTY;

            for (var x = 0; x < text.length; x++) {
                var character = text.substr(x, 1).toUpperCase();
                var value = this._barcodeMap.value[character];
                if (value) {
                    result += character;
                }
            }

            return "*" + result + "*";
        }
    }
}