/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Extensions/StringExtensions.ts'/>

module Commerce {
    "use strict";

    export class ReceiptHelper {

        private static TEXT_ESCAPE_MARKER: string = "&#x1B;";
        private static TEXT_BOLD_MARKER: string = ReceiptHelper.TEXT_ESCAPE_MARKER + "|2C";
        private static TEXT_NEW_LINE_MARKER: string = "\r\n";
        private static HTML_NEW_LINE_TAG: string = "<br/>";

        /**
         * Convert the raw receipt text to HTML and remove the logo text.
         * @param {string} receiptText The receipt text.
         * @return {string} The converted receipt text in HTML format.
         */
        public static convertToHtml(receiptText: string): string {
            if (StringExtensions.isNullOrWhitespace(receiptText)) {
                return StringExtensions.EMPTY;
            }

            receiptText = ReceiptHelper.translateReceiptContent(receiptText);

            var endOfBoldMarkerIndex: number = -1;
            // Convert bold text markers.
            while (true) {
                var boldMarkerIndex: number = receiptText.indexOf(ReceiptHelper.TEXT_BOLD_MARKER, endOfBoldMarkerIndex + 1);

                if (boldMarkerIndex < 0) {
                    break;
                }

                var endOfLineMarkerIndex: number = receiptText.indexOf(ReceiptHelper.TEXT_NEW_LINE_MARKER, boldMarkerIndex);
                var nextStyleMarkerIndex: number = receiptText.indexOf(ReceiptHelper.TEXT_ESCAPE_MARKER, boldMarkerIndex + 1);
                endOfBoldMarkerIndex = Math.min(endOfLineMarkerIndex, nextStyleMarkerIndex);

                // The entire bold item, e.g: "012532    ". 
                var entireBoldItem: string = receiptText.substring(boldMarkerIndex + ReceiptHelper.TEXT_BOLD_MARKER.length, endOfBoldMarkerIndex);
                var emptySpaces: string = Array(entireBoldItem.length + 1).join(" ");

                receiptText = (receiptText.substring(0, endOfBoldMarkerIndex) + emptySpaces + receiptText.substring(endOfBoldMarkerIndex));
            }

            var rawReceiptText: string = receiptText.replace(/<L:(.+?)>/g, "").replace(/<L>/g, "");
            var formatedReceiptText: string = EscapingHelper
                .escapeHtml(rawReceiptText.replace(/&#x1B;\|1C/g, "")
                .replace(/&#x1B;\|2C/g, ""))
                .replace(/\r\n/g, ReceiptHelper.HTML_NEW_LINE_TAG);
            return formatedReceiptText;
        }

        /**
         * Translates receipt content which is a subject for localization.
         * @param {string} receiptText The receipt text.
         * @return {string} The converted receipt text with translations.
         */
        public static translateReceiptContent(receiptText: string): string {

            var localizationStringRegEx: RegExp = new RegExp("<F:(.+?)>");
            var transactionStringRegEx: RegExp = new RegExp("<T:(.+?)>");

            // Replacing localization strings inside the buffer
            var match: RegExpExecArray = localizationStringRegEx.exec(receiptText);
            while (match) {

                var translatedString: string = Commerce.ViewModelAdapter.getResourceString(match[1]);
                var emptySpaces: string = Array(Math.abs(match[0].length - translatedString.length) + 1).join(" ");

                if (match[0].length >= translatedString.length) {
                    receiptText = receiptText.replace(match[0], translatedString + emptySpaces);
                } else {
                    receiptText = receiptText.replace(match[0] + emptySpaces, match[0]);
                    receiptText = receiptText.replace(match[0], translatedString);
                }

                match = localizationStringRegEx.exec(receiptText);
            }

            var translationMatch: RegExpExecArray = transactionStringRegEx.exec(receiptText);
            while (translationMatch) {
                receiptText = receiptText.replace(translationMatch[0], Commerce.ViewModelAdapter.getResourceString(translationMatch[1]));
                translationMatch = transactionStringRegEx.exec(receiptText);
            }

            return receiptText;
        }

        /**
         * Determines if a sales order can contain a gift receipt.
         * @param {Model.Entities.SalesOrder} salesOrder The sales order.
         * @return {boolean} True: if the sales order can contain a gift receipt. False: otherwise.
         */
        public static canSalesOrderContainGiftReceipt(salesOrder: Model.Entities.SalesOrder): boolean {
            return ArrayExtensions.hasElements(salesOrder.SalesLines) &&
                salesOrder.SalesLines.some((salesLine: Model.Entities.SalesLine) =>
                    !(salesLine.IsGiftCardLine || salesLine.IsVoided || salesLine.IsCustomerAccountDeposit || salesLine.IsReturnByReceipt)
                    && (salesLine.Quantity > 0 && salesLine.ItemId !== StringExtensions.EMPTY));
        }
    }
}