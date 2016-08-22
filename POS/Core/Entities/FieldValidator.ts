/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../RegularExpressionValidations.ts'/>

module Commerce.Proxy.Entities {
    "use strict";

    /**
     * Interface for FieldValidator constructor parameters.
     * To be able to initialize any subset of FieldValidator fields.
     * @param {number} [maxLength] - maxLength HTML5 input attribute.
     * @param {number} [max] - max HTML5 input attribute.
     * @param {number} [min] - min HTML5 input attribute.
     * @param {string} [pattern] - pattern HTML5 input attribute.
     * @param {boolean} [required] - required HTML5 input attribute.
     */
    export interface IFieldValidatorParams {
        maxLength?: number;
        max?: number;
        min?: number;
        pattern?: string;
        required?: boolean;
        title?: string;
    };

    /**
    * Validator class for entity fields. Sets up HTML5 attributes.
    */
    export class FieldValidator {
        private _validationAttributes: IFieldValidatorParams;

        /**
        * Constructs instance of FieldValidator class. Sets up HTML5 validation attributes.
        *
        * @param {IFieldValidatorParams} params - HTML5 validation attributes to set.
        */
        constructor(params: IFieldValidatorParams) {
            this._validationAttributes = params;
        }

        /**
         * Sets validation attributes to an element.
         *
         * @param {Element} element to set validation attributes for.
         */
        public setValidationAttributes(element: Element): void {
            for (var attrName in this._validationAttributes) {
                var value: any = this._validationAttributes[attrName];
                if (value) {
                    element.setAttribute(attrName, value);
                }

                // Remove "required" attribute from element if it is not set to true.
                if (this._validationAttributes.required !== true) {
                    element.removeAttribute("required");
                }
            }
        }
    }

    /**
    * Validator class for email field.
    */
    export class EmailFieldValidator extends FieldValidator {
        /**
        * Constructs instance of EmailFieldValidator class. Sets up HTML5 validation attributes.
        *
        * @param {title} title HTML5 attribute to set.
        * @param {required} required HTML5 attribute to set.
        */
        constructor(title?: string, required?: boolean) {
            super({ title: title, required: required, pattern: Core.RegularExpressionValidations.EMAIL_REGEX, maxLength: 80 });
        }
    }
}