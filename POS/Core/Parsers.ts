/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='Entities/CommerceTypes.g.ts'/>
///<reference path='Extensions/ObjectExtensions.ts'/>
///<reference path='IParser.ts'/>

module Commerce {
    "use strict";

    export class DecimalNotRequiredParser implements IParser {
        /**
         * Parses a string and returns a string representation of the parsed input.
         * @param {string} input The input to parse.
         * @return {string} The parsed string.
         */
        public parse(input: string): string {
            var deviceConfiguration: Proxy.Entities.DeviceConfiguration = ApplicationContext.Instance.deviceConfiguration;
            if (ObjectExtensions.isNullOrUndefined(deviceConfiguration)) {
                return StringExtensions.EMPTY + input;
            }

            var value: number = NumberExtensions.parseNumber(input);
            if (isNaN(value)) {
                return StringExtensions.EMPTY;
            }

            var decimalPrecision: number = NumberExtensions.getDecimalPrecision();

            if (input.indexOf(NumberExtensions.decimalSeparator) !== -1) {
                value = NumberExtensions.roundToNDigits(value, decimalPrecision);
            } else if (deviceConfiguration.DecimalNotRequiredForMinorCurrencyUnit) {
                value = NumberExtensions.roundToNDigits(value / Math.pow(10, decimalPrecision), decimalPrecision);
            }

            return NumberExtensions.formatNumber(value, decimalPrecision);
        }
    }
}