/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='FieldValidator.ts'/>
///<reference path='IEntityValidator.ts'/>

module Commerce.Proxy.Entities {
    "use strict";

    /**
     * Base class for entity validators.
     * Fields of derived classes are used for validation.
     */
    export class EntityValidatorBase implements IEntityValidator {

        constructor() {
        }

        /**
         * Set validation attributes of specified field to an element.
         *
         * @param {Element} element to set validation attributes for.
         * @param {string} fieldName - field of validation object to be used for attributes set.
         */
        public setValidationAttributes(element: Element, fieldName: string): void {
            var fieldValidator: FieldValidator = this[fieldName];
            if (fieldValidator) {
                fieldValidator.setValidationAttributes(element);
            }
        }
    }
}