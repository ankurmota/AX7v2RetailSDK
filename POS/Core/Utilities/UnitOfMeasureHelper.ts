/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../Extensions/NumberExtensions.ts'/>
///<reference path='../Extensions/ObjectExtensions.ts'/>
///<reference path='../Core.d.ts'/>

module Commerce {
    "use strict";

    export class UnitOfMeasureHelper {
        /**
         * Checks whether quantity is valid for the unit of measure.
         * @param {number} quantity The quantity.
         * @param {Proxy.Entities.UnitOfMeasure} unitOfMeasure The unit of measure.
         * @return {boolean} True if the quantity is valid for the unit of measure or unit of measure not specified, false otherwise.
         */
        public static isQuantityValid(quantity: number, unitOfMeasure: Proxy.Entities.UnitOfMeasure): boolean {
            var isValid: boolean = true;
            if (!ObjectExtensions.isNullOrUndefined(unitOfMeasure)) {
                isValid = NumberExtensions.getNumberOfDecimals(quantity) <= unitOfMeasure.DecimalPrecision;
            }

            return isValid;
        }

        /**
         * Rounds the number to the valid number of decimal places for the unit of measure. If the unit of measure symbol
         * could not be found 0 decimal places are used.
         * @param {number} quantity The quantity.
         * @param {string} unitOfMeasureSymbol The unit of measure.
         * @return {string} The rounded value for display.
         */
        public static roundToDisplay(quantity: number, unitOfMeasureSymbol: string): string {
            var decimalPrecision: number = UnitOfMeasureHelper.getDecimalPrecision(unitOfMeasureSymbol);
            var roundedValue: number = NumberExtensions.roundToNDigits(quantity, decimalPrecision);
            return NumberExtensions.formatNumber(roundedValue, decimalPrecision);
        }

        public static getDecimalPrecision(unitOfMeasureSymbol: string): number {
            var decimalPrecision: number = 0;
            var unitOfMeasure: Proxy.Entities.UnitOfMeasure;

            if (!StringExtensions.isNullOrWhitespace(unitOfMeasureSymbol)) {
                unitOfMeasure = ApplicationContext.Instance.unitsOfMeasureMap.getItem(unitOfMeasureSymbol.toLowerCase());
            }

            if (!ObjectExtensions.isNullOrUndefined(unitOfMeasure)) {
                decimalPrecision = Math.max(0, unitOfMeasure.DecimalPrecision);
            }

            return decimalPrecision;
        }

        /**
         * Returns the description for the unit of measure symbol.
         * @param {string} unitOfMeasureSymbol The unit of measure.
         * @return {string} The description.
         */
        public static getDescriptionForSymbol(unitOfMeasureSymbol: string): string {
            var description: string = null;

            if (!ObjectExtensions.isNullOrUndefined(unitOfMeasureSymbol)
                && ApplicationContext.Instance.unitsOfMeasureMap.hasItem(unitOfMeasureSymbol.toLowerCase())) {
                description = ApplicationContext.Instance.unitsOfMeasureMap.getItem(unitOfMeasureSymbol.toLowerCase()).Description;
            }

            return description;
        }
    }
}