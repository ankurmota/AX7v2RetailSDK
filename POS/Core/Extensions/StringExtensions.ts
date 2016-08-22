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

    export class StringExtensions {

        public static EMPTY: string = "";

        /**
         * Verifies whether the string is empty, but not null.
         * @param {string} object The object.
         * @return {boolean} True if the object is empty, false otherwise.
         */
        public static isEmpty(object: string): boolean {
            if (!ObjectExtensions.isNullOrUndefined(object)) {
                if (!ObjectExtensions.isString(object)) {
                    throw new Error("StringExtensions.isEmpty() has received input parameter not of type string.");
                }

                return object === "";
            }

            return false;
        }

        /**
         * Verifies whether the string is empty or whitespace, but not null.
         * @param {string} object The object.
         * @return {boolean} True if the object is empty or whitespace, false otherwise.
         */
        public static isEmptyOrWhitespace(object: string): boolean {
            if (!ObjectExtensions.isNullOrUndefined(object)) {
                if (!ObjectExtensions.isString(object)) {
                    throw new Error("StringExtensions.isEmptyOrWhitespace() has received input parameter not of type string.");
                }

                return object === "" || object.trim() === "";
            }

            return false;
        }

        /**
         * Verifies whether the string is null or whitespace.
         * @param {string} object The object.
         * @return {boolean} True if the object is null or whitespace, false otherwise.
         */
        public static isNullOrWhitespace(object: string): boolean {
            if (!ObjectExtensions.isNullOrUndefined(object)) {
                if (!ObjectExtensions.isString(object)) {
                    throw new Error("StringExtensions.isNullOrWhitespace() has received input parameter not of type string.");
                }

                return object === "" || object.trim() === "";
            }

            return true;
        }

        /**
         * Pad left with padString until the required length is reached.
         */
        public static padLeft(str: string, padString: string, length: number): string {
            if (!ObjectExtensions.isNullOrUndefined(str)) {
                if (!ObjectExtensions.isString(str)) {
                    throw new Error("StringExtensions.padLeft() has received input parameter not of type string.");
                }

                if (!ObjectExtensions.isNullOrUndefined(padString)) {
                    while (str.length < length) {
                        str = padString + str;
                    }
                }
            }

            return str;
        }

        /**
         * Pad right with padString until the required length is reached.
         */
        public static padRight(str: string, padString: string, length: number): string {
            if (!ObjectExtensions.isNullOrUndefined(str)) {
                if (!ObjectExtensions.isString(str)) {
                    throw new Error("StringExtensions.padRight() has received input parameter not of type string.");
                }

                if (!ObjectExtensions.isNullOrUndefined(padString)) {
                    while (str.length < length) {
                        str += padString;
                    }
                }
            }

            return str;
        }

        /**
         * Basic C# like string format function.
         */
        public static format(object: string, ...params: any[]): string {
            if (!ObjectExtensions.isNullOrUndefined(object) && !ObjectExtensions.isString(object)) {
                throw new Error("StringExtensions.format() has received input parameter not of type string.");
            }

            if (StringExtensions.isNullOrWhitespace(object)) {
                return object;
            }

            if (params == null) {
                throw new Error("StringExtensions.format() Invalid parameter (params) cannot be null.");
            }

            for (var i: number = 0; i < params.length; i++) {
                if (params[i] == null) {
                    throw new Error("StringExtensions.format() Invalid parameter (at index " + i + ") cannot be null or undefined.");
                }

                // escape '$' to avoid '$0' issue, '$$$$' means '$$' escaped
                var param: string = params[i].toString().replace(/\$/gi, "$$$$");
                var regexp: RegExp = new RegExp("\\{" + i + "\\}", "gi");
                object = object.replace(regexp, param);
            }

            return object;
        }

        /**
         * Returns an ordinal to indicate the ordering of the strings
         * -1: This object is less than comparison object
         * 0: This object is equal to the comparison object
         * 1: This object is greater than the comparison object
         */
        public static compare(object: string, comparisonObject: string, ignoreCase?: boolean): number {
            if (ObjectExtensions.isNullOrUndefined(object) && ObjectExtensions.isNullOrUndefined(comparisonObject)) {
                return 0;
            } else if (ObjectExtensions.isNullOrUndefined(object)) {
                return -1;
            } else if (ObjectExtensions.isNullOrUndefined(comparisonObject)) {
                return 1;
            }

            var val1: string = ignoreCase ? object.toLowerCase() : object;
            var val2: string = ignoreCase ? comparisonObject.toLowerCase() : comparisonObject;

            return val1 < val2 ? -1 : val1 > val2 ? 1 : 0;
        }

        /**
         * Replaces new line character with <br /> for display.
         */
        public static replaceNewLineWithBr(text: string): string {
            if (text) {
                return StringExtensions.replaceAll(text, "\n", "<br />");
            }

            return text;
        }

        /**
         * Replaces all instances of @txtToReplace
         */
        public static replaceAll(txt: string, txtToReplace: string, valueToReplaceWith: string): string {
            return txt.replace(new RegExp(txtToReplace, "g"), valueToReplaceWith);
        }

        /**
         * Escapes single quote to be send as part of URLs.
         */
        public static escapeSingleQuote(text: string): string {
            return text.replace(/(')/g, "\'$1");
        }

        /**
         * Removes the trailing slashes from the URI.
         * @param {string} uri The URI to clean.
         * @return {string} The uri without trailing slashes.
         */
        public static CleanUri(uri: string): string {
            if (!ObjectExtensions.isNullOrUndefined(uri)) {
                uri = uri.trim();
            }

            if (StringExtensions.isNullOrWhitespace(uri)) {
                return "";
            }

            // the cutoff index for the string
            var cutoffIndex: number = uri.length - 1;

            while (cutoffIndex >= 0
                && (uri[cutoffIndex] === "/"
                || uri[cutoffIndex] === "\\")) {

                --cutoffIndex;
            }

            // if it ever becomes negative, cutoffIndex + 1 = 0
            return uri.substr(0, cutoffIndex + 1);
        }

        /**
         * Determines whether the beginning of a string matches a specified string.
         * @param {string} str: The string to search in.
         * @param {string} prefix: The string to compare to the substring at the beginning of str.
         * @param {boolean} caseSensitive: Determines if the comparison is case sensitive (false, by default)
         * @return {boolean} true if prefix matches the beginning of str; otherwise, false.
         */
        public static beginsWith(str: string, prefix: string, caseSensitive: boolean= false): boolean {
            if (ObjectExtensions.isNullOrUndefined(str) || ObjectExtensions.isNullOrUndefined(prefix)) {
                return false;
            }
            if (prefix.length > str.length) {
                return false;
            }

            var originalString: string = (caseSensitive) ? str : str.toLowerCase();
            var subString: string = (caseSensitive) ? prefix : prefix.toLowerCase();
            return originalString.indexOf(subString) === 0;
        }

        /**
         * Determines whether the end of string matches a specified string.
         * @param {string} str: The string to search in.
         * @param {string} suffix: The string to compare to the substring at the end of str.
         * @param {boolean} caseSensitive: Determines if the comparison case sensitive (false, by default)
         * @return {boolean} true if suffix matches the end of str; otherwise, false.
         */
        public static endsWith(str: string, suffix: string, caseSensitive: boolean= false): boolean {
            if (ObjectExtensions.isNullOrUndefined(str) || ObjectExtensions.isNullOrUndefined(suffix)) {
                return false;
            }
            if (suffix.length > str.length) {
                return false;
            }

            var originalString: string = (caseSensitive) ? str : str.toLowerCase();
            var subString: string = (caseSensitive) ? suffix : suffix.toLowerCase();
            return originalString.indexOf(subString, originalString.length - subString.length) !== -1;
        }
    }
}