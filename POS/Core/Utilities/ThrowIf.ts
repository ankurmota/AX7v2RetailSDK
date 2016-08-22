/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="../Extensions/ObjectExtensions.ts" />

module Commerce {
    "use strict";

    export module ThrowIf {

        /**
         * Throws TypeError in case if the provided argument is not an object.
         * @param val {any} Argument value.
         * @param parameterName {string} Parameter name.
         */
        export function argumentIsNotObject(val: any, parameterName: string): void {
            if (ObjectExtensions.isNull(val) || !ObjectExtensions.isObject(val)) {
                throw new TypeError("The argument '" + parameterName + "' should be an object.");
            }
        }

        /**
         * Throws TypeError in case if the provided argument is not a function.
         * @param val {any} Argument value.
         * @param parameterName {string} Parameter name.
         */
        export function argumentIsNotFunction(val: any, parameterName: string): void {
            if (!ObjectExtensions.isFunction(val)) {
                throw new TypeError("The argument '" + parameterName + "' should be a function.");
            }
        }

        /**
         * Throws TypeError in case if the provided argument is not a string.
         * @param val {any} Argument value.
         * @param parameterName {string} Parameter name.
         */
        export function argumentIsNotString(val: any, parameterName: string): void {
            if (!ObjectExtensions.isString(val)) {
                throw new TypeError("The argument '" + parameterName + "' should be a string.");
            }
        }

        /**
         * Throws TypeError in case if the provided argument is not a string or null.
         * @param val {any} Argument value.
         * @param parameterName {string} Parameter name.
         */
        export function argumentIsNotStringOrNull(val: any, parameterName: string): void {
            if (!ObjectExtensions.isNull(val) && !ObjectExtensions.isString(val)) {
                throw new TypeError("The argument '" + parameterName + "' should be a string or null.");
            }
        }
    }
}
