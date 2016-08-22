/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Extensions/NumberExtensions.ts'/>
///<reference path='../ApplicationContext.ts'/>

module Commerce.Helpers {
    "use strict";

    export class CurrencyHelper {
        /**
         * Checks that the amount is a valid amount for the currency
         * @param {number} amount The amount to format.
         * @param {string} [currencyCode] The currency code of the currency to get the format information. If not provided, the store currency code is used.
         * @return {boolean} True if the amount is a valid amount for the currency, false otherwise
         */
        public static isValidAmount(amount: number, currencyCode: string = ApplicationContext.Instance.deviceConfiguration.Currency): boolean {
            // Get the currency code for formatting the number of digits after the decimal
            if (ObjectExtensions.isNullOrUndefined(amount) || isNaN(amount)) {
                return false;
            }

            // Get the number of digits after the decimal for the currency
            var numberOfDigitsAfterDecimalForCurrency: number = NumberExtensions.getDecimalPrecision(currencyCode);

            // Get the number of digits after the decimal for the amount
            var numberOfDigitsAfterDecimalForAmount: number = NumberExtensions.getNumberOfDecimals(amount);
            return numberOfDigitsAfterDecimalForCurrency >= numberOfDigitsAfterDecimalForAmount;
        }
    }
}