/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Contoso.Retail.Ecommerce.Sdk.Controls {
    "use strict";

    export class ErrorHelper {

        // Aggregated resource ids.
        private static AGGREGATED_ERROR_RESOUCEIDS: string[] = [
            "Microsoft_Dynamics_Commerce_Runtime_AggregateValidationError",
            "Microsoft_Dynamics_Commerce_Runtime_AggregateCommunicationError",
            "Microsoft_Dynamics_Commerce_Runtime_InvalidCartLinesAggregateError"];

        /**
         * Gets the error message from the list of errors.
         *
         * @param {CommerceProxy.ProxyError[]} errors The array of errors.
         * @return {string[]} The error messages.
         */
        public static getErrorMessages(errors: CommerceProxy.ProxyError[]): string[] {
            var messages: string[] = [];
            var limitOfErrorLines = 5;
            var previousMessageCodes: string[] = [];

            if (Utils.hasElements(errors)) {
                for (var i = 0; i < errors.length && i < limitOfErrorLines; i++) {
                    var error: CommerceProxy.ProxyError = errors[i];

                    // skip aggregated messages if we have more than one error
                    if (errors.length > 1 && ErrorHelper.isAggregatedErrorResourceId(error.ErrorCode)) {
                        continue;
                    }

                    // server might send same error code more than once, make sure we don't display same message twice
                    if (previousMessageCodes.indexOf(error.ErrorCode) != -1) {
                        continue;
                    }

                    previousMessageCodes.push(error.ErrorCode);

                    // Add error message to the array.
                    messages.push(ErrorHelper.clientError(error));
                }
            }

            return messages;
        }

        /**
         * Gets the error message from the error.
         *
         * @param {CommerceProxy.ProxyError} proxyError The error.
         * @return {string} The localized error message.
         */
        public static clientError(proxyError: CommerceProxy.ProxyError): string {

            // If the error code has been mapped to a localized client error use that localized error message 
            // otherwise if there is a localized error message from server use that
            // otherwise use default localized error message
            var localizedErrorMessage: string = Resources[ErrorTypeEnum.GENERICERRORMESSAGE];
            var errorCode: string;

            if (proxyError.ErrorCode) {
                errorCode = proxyError.ErrorCode.toUpperCase();

                var clientResourceId: string = ErrorTypeEnum[errorCode];

                if (!Utils.isNullOrWhiteSpace(clientResourceId)) {
                    localizedErrorMessage = Resources[clientResourceId];
                } else if (!Utils.isNullOrWhiteSpace(proxyError.LocalizedErrorMessage)) {
                    localizedErrorMessage = proxyError.LocalizedErrorMessage;
                }
            }

            return localizedErrorMessage;
        }

        /**
         * Returns true if resource id is aggregated resource id, false otherwise.
         *
         * @return {boolean} True if resource id is aggregated resource id, false otherwise.
         */
        private static isAggregatedErrorResourceId(errorResourceId: string): boolean {
            return ErrorHelper.AGGREGATED_ERROR_RESOUCEIDS.indexOf(errorResourceId) != -1;
        }
    }
}