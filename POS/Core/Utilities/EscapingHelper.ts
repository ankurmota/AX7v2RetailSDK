/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce {
    "use strict";

    export class EscapingHelper {

        /**
         *  Escapes a string that is to be included as part of the HTML body (e.g. innerHTML of an element).
         *  @param {string} input string to be escaped.
         *  @returns {string} HTML escaped string.
         */
        public static escapeHtml(input: string): string {
            var div: HTMLDivElement = document.createElement("div");
            div.appendChild(document.createTextNode(String(input)));
            return div.innerHTML;
        }

        /**
         *  Escapes a string that is to be included as a HTML attribute (e.g. <div someAttr="...escape content with this function ...">).
         *  @param {string} input string to be escaped.
         *  @returns {string} HTML escaped string.
         */
        public static escapeHtmlAttribute(input: string): string {
            return String(input)
                .replace(/&/g, "&amp;")
                .replace(/"/g, "&quot;")
                .replace(/'/g, "&#39;")
                .replace(/</g, "&lt;")
                .replace(/>/g, "&gt;");
        }
    }
}