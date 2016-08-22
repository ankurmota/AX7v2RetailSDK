/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path='../ApplicationContext.ts'/>
/// <reference path='../globalize.d.ts'/>
/// <reference path='../Lazy.ts'/>

module Commerce {
    "use strict";

    export class NumberExtensions {
        /**
         * The default decimal precision.
         */
        public static DEFAULT_DECIMAL_PRECISION: number = 2;

        private static _decimalSeparator: Lazy<string> = new Lazy<string>(() => {
            return Globalize.culture(ApplicationContext.Instance.deviceConfiguration.CultureName).numberFormat["."];
        });

        private static _groupSeparator: Lazy<string> = new Lazy<string>(() => {
            return Globalize.culture(ApplicationContext.Instance.deviceConfiguration.CultureName).numberFormat[","];
        });

        /**
         * Gets the decimal separator based on the channel language.
         */
        public static get decimalSeparator(): string {
            return NumberExtensions._decimalSeparator.value;
        }

        /**
         * Gets the group separator based on the channel language.
         */
        public static get groupSeparator(): string {
            return NumberExtensions._groupSeparator.value;
        }

        /**
         * Gets the number of digits after the decimal.
         * @param {number} value The value to get the number of digits after the decimal.
         * @return {number} The number of digits after the decimal.
         */
        public static getNumberOfDecimals(value: number): number {
            // Check the parameter
            if (ObjectExtensions.isNullOrUndefined(value) || isNaN(value)) {
                return 0;
            }

            // Split the value into the number and the exponent
            var valueParts: string[] = value.toString().split("e");
            var valueWithoutExponent: string = valueParts[0];
            var exponent: string = valueParts.length > 1 ? valueParts[1] : "0";

            // Compute the number of digits in the number after the decimal
            var numberOfDigitsInFractionWithSeperator: number = valueWithoutExponent.length - Math.floor(Number(valueWithoutExponent)).toString().length;
            var numberOfDecimals: number = numberOfDigitsInFractionWithSeperator === 0 ? 0 : numberOfDigitsInFractionWithSeperator - 1;

            // Compute the number of digits in the number after the exponent is applied
            numberOfDecimals -= Number(exponent);
            numberOfDecimals = numberOfDecimals < 0 ? 0 : numberOfDecimals;

            return numberOfDecimals;
        }

        /**
         * For the specified currency, get the specified decimal precision.
         * If the currency is not specified or supported the decimal precision for store currency is returned.
         * If store currency is not properly set up, the default decimal precision is returned.
         * @param {string} currencyCode The currency code of the currency to get the format information. If not provided, the store currency code is used.
         * @return {number} The number of digits after the decimal.
         */
        public static getDecimalPrecision(currencyCode: string = ApplicationContext.Instance.deviceConfiguration.Currency): number {
            var currency: Proxy.Entities.Currency = ApplicationContext.Instance.currenciesMap.getItem(currencyCode);
            if (ObjectExtensions.isNullOrUndefined(currency)) {
                return NumberExtensions.DEFAULT_DECIMAL_PRECISION;
            }

            return NumberExtensions.toDecimalPrecision(currency.NumberOfDecimals);
        }

        /**
         * Formats a value based on the decimal precision, format and culture given.
         * @param {number} value The value to format.
         * @param {number} decimalPrecision The decimal precision. If less than 0 or not set will treat as the default decimal precision.
         * If a fraction, it will be rounded.
         * @param {string} format The format used to format the number.
         * @param {string} cultureName The culture name used for formatting.
         * @return {string} The formatted value.
         */
        public static formatNumber(
            value: number,
            decimalPrecision: number,
            format: string = "N",
            cultureName: string = ApplicationContext.Instance.deviceConfiguration.CultureName): string {

            var numberFormat: GlobalizeNumberFormat = Globalize.culture(cultureName).numberFormat;
            numberFormat.decimals = <any>NumberExtensions.toDecimalPrecision(decimalPrecision);

            return Globalize.format(value, format, cultureName);
        }

        /**
         * Formats a value as currency based on the currency code and culture given.
         * @param {number} value The value to format.
         * @param {string} currencyCode The currency code.
         * @param {string} cultureName The culture name used for formatting.
         * @return {string} The formatted value.
         */
        public static formatCurrency(
            value: number,
            currencyCode: string = ApplicationContext.Instance.deviceConfiguration.Currency,
            cultureName: string = ApplicationContext.Instance.deviceConfiguration.CultureName): string {

            var currency: Proxy.Entities.Currency = ApplicationContext.Instance.currenciesMap.getItem(currencyCode);
            var currencyFormat: GlobalizeCurrency = Globalize.culture(cultureName).numberFormat.currency;
            currencyFormat.symbol = currency.CurrencySymbol;
            currencyFormat.decimals = NumberExtensions.toDecimalPrecision(currency.NumberOfDecimals);

            return Globalize.format(value, "C", cultureName);
        }

        /**
         * Parses a string to a number.
         * @param {string} value The value to parse.
         * @param {string} cultureName The culture name used for parsing.
         * @return {number} Either the parsed value as a number or NaN if value is not defined or empty.
         */
        public static parseNumber(
            value: string,
            cultureName: string = ApplicationContext.Instance.deviceConfiguration.CultureName): number {

            if (StringExtensions.isNullOrWhitespace(value)) {
                return NaN;
            }

            return Globalize.parseFloat(value, 10, cultureName);
        }

        /**
         * Rounds to the specified number of digits after the decimal point.
         * @param {number} value The value to be rounded.
         * @param {number} decimalPrecision The decimal precision. If less than 0 or not set will treat as the default decimal precision.
         * If a fraction, it will be rounded.
         * @return {number} The rounded value.
         */
        public static roundToNDigits(value: number, decimalPrecision: number): number {
            decimalPrecision = NumberExtensions.toDecimalPrecision(decimalPrecision);
            if (decimalPrecision === 0) {
                return Math.round(value);
            }

            // Use this instead of toFixed otherwise it will not round anything, simply lose digits
            return Math.round(value * Math.pow(10, decimalPrecision)) / Math.pow(10, decimalPrecision);
        }

        /**
         * Returns an ordinal to indicate the ordering of the numbers
         * -1: This object is less than comparison object
         * 0: This object is equal to the comparison object
         * 1: This object is greater than the comparison object
         */
        public static compare(object: number, comparisonObject: number): number {
            if (ObjectExtensions.isNullOrUndefined(object) && ObjectExtensions.isNullOrUndefined(comparisonObject)) {
                return 0;
            } else if (ObjectExtensions.isNullOrUndefined(object)) {
                return -1;
            } else if (ObjectExtensions.isNullOrUndefined(comparisonObject)) {
                return 1;
            }

            return object < comparisonObject ? -1 : object > comparisonObject ? 1 : 0;
        }

        /**
         * Verifies whether the number is null or equal to 0.
         * @param {numeric} object The number.
         * @return {boolean} True if the number is undefined, null or equal to zero, false otherwise.
         */
        public static isNullOrZero(object: number): boolean {
            return (ObjectExtensions.isNullOrUndefined(object) || object === 0);
        }

        /**
         * Determines whether two numbers are roughly equal based on a margin of error
         * @param {numeric} object The first number to compare
         * @param {numeric} comparisonObject The second number to compare
         * @param {numeric?} sigma The optional margin of error to use
         * @return {boolean} True if the numbers are equal within the margin of error, false otherwise
         */
        public static areEquivalent(object: number, comparisonObject: number, sigma?: number): boolean {
            if (ObjectExtensions.isNullOrUndefined(object) && ObjectExtensions.isNullOrUndefined(comparisonObject)) {
                return true;
            } else if (ObjectExtensions.isNullOrUndefined(object) || ObjectExtensions.isNullOrUndefined(comparisonObject)) {
                return false;
            }

            if (ObjectExtensions.isNullOrUndefined(sigma)) {
                sigma = .00001;
            }

            if ((object - sigma > comparisonObject) || (object + sigma < comparisonObject)) {
                return false;
            } else {
                return true;
            }
        }

        /**
         * Converts the given number to a valid decimal precision. If an invalid value is passed, returns the default decimal precision.
         */
        private static toDecimalPrecision(decimalPrecision: number, defaultIfInvalid: number = NumberExtensions.DEFAULT_DECIMAL_PRECISION): number {
            if (ObjectExtensions.isNumber(decimalPrecision) && decimalPrecision >= 0) {
                return Math.round(decimalPrecision);
            }

            return defaultIfInvalid;
        }
    }
}