/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="../../Resources/Resources.ts" />

module Contoso.Retail.Ecommerce {
    export class Utils {

        private static isLocalizationDataLoaded: boolean;
        private static currentUiCulture = "";
        private static defaultUiCulture = "en-US";
        private static uiCultureCookieName = "cuid";
       
        // Test to see if the object is null or undefined.
        public static isNullOrUndefined(o): boolean {
            return (o === undefined || o === null);
        }

        // Tests to see if an object is null or empty.
        public static isNullOrEmpty(o): boolean {
            return (Utils.isNullOrUndefined(o) || o === '');
        }

        // Tests to see if an object is null or contains just white space.
        public static isNullOrWhiteSpace(o: string): boolean {
            return (Utils.isNullOrEmpty(o) || (typeof o === 'string' && o.replace(/\s/g, '').length < 1));
        }

        // Test to see if the array has elements.
        public static hasElements(o: any[]): boolean {
            return !Utils.isNullOrUndefined(o) && o.length > 0;
        }

        // Return specified default value if object is null or undefined or whitespace.
        public static getValueOrDefault(o: string, defaultValue): string {
            return Utils.isNullOrWhiteSpace(o) ? defaultValue : o;
        }

        // Tests to see if an object has errors.
        public static hasErrors(o: any): boolean {
            return (!Utils.isNullOrUndefined(o) && !this.hasElements(o.Errors));
        }


        /**
        * Basic C# like string format function.
        */
        public static format(object: string, ...params: any[]): string {
            if (Utils.isNullOrWhiteSpace(object)) {
                return object;
            }

            if (params == null) {
                throw Contoso.Retail.Ecommerce.Sdk.Controls.Resources.String_70;
            }

            for (var index = 0; index < params.length; index++) {
                if (params[index] == null) {
                    throw Contoso.Retail.Ecommerce.Sdk.Controls.Resources.String_70;
                }

                var regexp = new RegExp('\\{' + index + '\\}', 'gi');
                object = object.replace(regexp, params[index]);
            }

            return object;
        }

        // Append two strings with the given append character
        public static appendString(originalStr: string, appendStr: string, appendChar: string): string {
            return appendStr ? ( originalStr ? originalStr + appendChar + appendStr : appendStr) : originalStr;
        }

        public static parseNumberFromLocaleString(localizedNumberString: string) {
            var currDecimalOperator = this.getDecimalOperatorForUiCulture();

            var numberTokens = localizedNumberString.split(currDecimalOperator);
            if (numberTokens.length > 2) {
                throw Contoso.Retail.Ecommerce.Sdk.Controls.Resources.String_204;
            }

            var regexp = new RegExp("[^0-9]", "gi");
            var integerDigits = numberTokens[0].replace(regexp, "");
            var fractionalDigits = "";
            if (numberTokens.length == 2) {
                fractionalDigits = numberTokens[1].replace(regexp, "");
            }

            var numberString = integerDigits + '.' + fractionalDigits;

            var parsedNumber: number = parsedNumber = Number(numberString);
            if (isNaN(parsedNumber)) {
                parsedNumber = 0;
            }
            return parsedNumber; 
        }

        public static getDecimalOperatorForUiCulture() {
            // Intl is currently not supported in all browsers. Hence this method is currently being hardcoded to work against "en-us" only.
            //var uiCulture = this.getCurrentUiCulture();
            //var nf: any;
            //nf = new Intl.NumberFormat(uiCulture);
            //var localizedNumString: string;
            //localizedNumString = nf.format(1.1); //Eg: 1.1 will become 1,1 in fr-ca. 1.1 wil become 1.1 in en-us.

            //var decimalOperator = localizedNumString.charAt(1);
            //return decimalOperator;

            return '.';
        }

        public static getQueryStringValue(key: string): string {
            var url = window.location.href;
            var keysValues = url.split(/[\?&]+/);
            for (var i = 0; i < keysValues.length; i++) {
                var keyValue = keysValues[i].split("=");
                if (keyValue[0] == key) {
                    return keyValue[1];
                }
            }
        }

        public static formatNumber(numberValue: number) {
            // Intl is currently not supported in all browsers. Hence, this method is currently being hardcoded to work against "en-us" only.
            //var uiCulture = this.getCurrentUiCulture();

            //var nf: any;
            //nf = new Intl.NumberFormat([uiCulture], { minimumFractionDigits:2, maximumFractionDigits: 2});
            //var formattedNumber = nf.format(numberValue);
            var formattedNumber = numberValue.toFixed(2);
            return formattedNumber;
        }

        public static getCurrentUiCulture() {
            if (!Utils.isNullOrWhiteSpace(this.currentUiCulture)) {
                return this.currentUiCulture;
            }

            var uiCulture = Utils.getCookieValue(this.uiCultureCookieName);

            if (Utils.isNullOrWhiteSpace(uiCulture)) {
                uiCulture = this.defaultUiCulture;
            }

            return uiCulture;
        } 

        public static getCookieValue(cookieName: string): string {
            var nameWithEqSign = cookieName + "=";
            var allCookies = document.cookie.split(';');
            for (var i = 0; i < allCookies.length; i++) {
                var singleCookie = allCookies[i];
                while (singleCookie.charAt(0) == ' ') {
                    singleCookie = singleCookie.substring(1, singleCookie.length);
                }
                if (singleCookie.indexOf(nameWithEqSign) == 0) {
                    return singleCookie.substring(nameWithEqSign.length, singleCookie.length);
                }
            }

            return null;
        }

        public static setCookieValue(cookieName: string, cookieValue: string) {
            if (!Utils.isNullOrWhiteSpace(cookieName)) {
                document.cookie = cookieName + "=" + cookieValue;
            }
        }

        /**
        * Makes a copy of an object
        * jquery example call that this method replaces: <any >$.extend(true, { }, <your object to clone >);
        *
        * @param {any} origObject The original object to clone
        * @return {any} The cloned object
        */
        public static clone(origObject: any): any {
            return Utils.safeClone(origObject, []);
        }

        /**
        * Makes a copy of an object
        *
        * @param {any} origObject The original object to clone
        * @param {any} cloneMap A map of the objects that are being cloned to identify circular references
        * @return {any} The cloned object
        */
        private static safeClone(origObject: any, cloneMap: any[]): any {
            if (Utils.isNullOrUndefined(origObject)) {
                return origObject;
            }

            var newObj: any;
            if (origObject instanceof Array) {
                // Use the array reference in the object map if it exists
                if (!cloneMap.some((val: any): boolean => {
                    if (val.id === origObject) {
                        newObj = val.value;
                        return true;
                    }

                    return false;
                })) {
                    // Add the cloned array reference to the object map and clone the array
                    newObj = [];
                    cloneMap.push({ id: origObject, value: newObj });
                    for (var i = 0; i < origObject.length; i++) {
                        if (typeof origObject[i] == "object") {
                            newObj.push(Utils.safeClone(origObject[i], cloneMap));
                        } else {
                            newObj.push(origObject[i]);
                        }
                    }
                }
            } else if (origObject instanceof Date) {
                newObj = new Date((<Date> origObject).valueOf());
            } else if (origObject instanceof Object) {
                // Use the object reference in the object map if it exists
                if (!cloneMap.some((val: any): boolean => {
                    if (val.id === origObject) {
                        newObj = val.value;
                        return true;
                    }

                    return false;
                })) {
                    // Add the cloned object reference to the object map and clone the object
                    newObj = <any >$.extend(false, {}, origObject);
                    cloneMap.push({ id: origObject, value: newObj });
                    for (var property in newObj) {
                        if (newObj.hasOwnProperty(property)) {
                            if (typeof newObj[property] == "object") {
                                if (property === "__metadata") {
                                    newObj[property] = <any >$.extend(false, {}, origObject[property]);
                                } else {
                                    newObj[property] = Utils.safeClone(origObject[property], cloneMap);
                                }
                            }
                        }
                    }
                }
            } else {
                newObj = origObject;
            }

            return newObj;
        }

        /**
         * Rounds to the specified number of digits after the decimal point
         *
         * @param {number} value The value to be rounded
         * @param {numOfDigits} numOfDigits The number of digits. If less than 0 or not set will treat as 0.
         *                                  If a fraction, will round.
         * @return {number} The number of digits after the decimal
         */
        public static roundToNDigits(value: number, numOfDigits: number): number {
            if (this.isNullOrUndefined(numOfDigits) || (numOfDigits <= 0)) {
                numOfDigits = 0;
            } else {
                numOfDigits = Math.round(numOfDigits);
            }

            if (numOfDigits === 0) {
                return Math.round(value);
            }

            // Use this instead of toFixed otherwise you will not round anything, simply lose digits
            return Math.round(value * Math.pow(10, numOfDigits)) / Math.pow(10, numOfDigits);
        }
    }
}