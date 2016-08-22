/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ProxyError.ts'/>
///<reference path='NotificationHandler.ts'/>
///<reference path='../Extensions/ObjectExtensions.ts'/>
///<reference path='../Extensions/StringExtensions.ts'/>
///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../Tracer.ts'/>

module Commerce.Proxy {
    "use strict";

    /**
     * Error parser class.
     */
    export class ErrorParser {

        private static CommerceExceptionType: string = "Microsoft.Dynamics.Commerce.Runtime.CommerceException";
        private static DataValidationExceptionType: string = "Microsoft.Dynamics.Commerce.Runtime.DataValidationException";
        private static CartValidationExceptionType: string = "Microsoft.Dynamics.Commerce.Runtime.CartValidationException";
        private static StorageExceptionType: string = "Microsoft.Dynamics.Commerce.Runtime.StorageException";
        private static PaymentExceptionType: string = "PaymentException";
        private static BarcodeWithLinkedItemNotSupportedExceptionType: string = "Microsoft.Dynamics.Commerce.Runtime.BarcodeWithLinkedItemNotSupportedException";
        private static MissingRequiredReasonCodeExceptionType: string = "Microsoft.Dynamics.Commerce.Runtime.MissingRequiredReasonCodeException";

        /**
         * Parses an error message.
         *
         * @param {any} error The error message.
         * @return {ProxyError[]} The collection of error messages.
         */
        public static parseErrorMessage(error: any): ProxyError[] {
            var response = error.response;
            var errors: ProxyError[] = [];

            if (!ObjectExtensions.isNullOrUndefined(response)) {

                var handled = true;
                Tracer.Information("Server response status code is '{0}'", response.statusCode);

                switch (response.statusCode) {

                    case 306: // Custom - Redirection
                        var redirectUrl = response.headers['Location'];
                        errors = [new ProxyError(ErrorTypeEnum.RETAIL_SERVER_REDIRECT_ERROR, response.statusText, StringExtensions.EMPTY, false, redirectUrl)];
                        break;

                    case 401:
                        break;

                    case 408: //Request Timeout
                        errors = [new ProxyError(ErrorTypeEnum.SERVER_TIMEOUT, response.statusText, StringExtensions.EMPTY, false)];
                        break;

                    case 0: // WWAHost reports status code as 0, If server is unreachable.
                    case 502: // Bad Gateway
                    case 503: // Service Unavailable
                    case 504: // Gateway timeout
                        errors = [new ProxyError(ErrorTypeEnum.SERVICE_UNAVAILABLE, response.statusText, StringExtensions.EMPTY, false)];
                        break;

                    default:
                        handled = false;
                        break;
                }

                if (!handled) {
                    errors = ErrorParser.parseError(error);
                }
            }

            // If we couldn't create any error object from error message, add error parsing failure
            if ((!errors) || (!errors.length)) {
                errors = [new ProxyError(ErrorTypeEnum.SERVER_ERROR, 'Could not parse error message sent by the server.')];
            }

            return errors;
        }

        /**
         * Parses an error message sent from Hardware station.
         *
         * @param {any} error The error message.
         * @retunrs {ProxyError[]} The collection of error messages.
         */
        public static parseHardwareStationErrorMessage(error: any): ProxyError[] {
            var errors: ProxyError[];

            if (!ObjectExtensions.isNullOrUndefined(error)) {
                try {
                    var response = error.responseText;

                    if (response) {
                        var jsonResponse = JSON.parse(response);

                        // Parse errors in json format
                        if (jsonResponse) {
                            errors = [new ProxyError(jsonResponse.ErrorResourceId ? jsonResponse.ErrorResourceId : ErrorTypeEnum.SERVER_ERROR, jsonResponse.Message || '')];
                        } else {
                            // If not in json, we cannot parse it
                            errors = [new ProxyError(ErrorTypeEnum.SERVER_ERROR, 'The value of Content-Type on server response is not supported.')];
                        }
                    }
                    else {
                        // In case we don't have a body, we don't know the error message but from http status
                        errors = [ErrorHelper.MapResponseStatusCodeToError(error.statusText, error.status, error)];
                    }
                }
                catch (ex) {
                    var errorMessage: string = "parseHardwareStationErrorMessage: Failed parse error message '{0}'.";
                    Tracer.Error(errorMessage, ex);
                }
            }

            // If we couldn't create any error object from error message, add error parsing failure
            if ((!errors) || (!errors.length)) {
                errors = [new ProxyError(ErrorTypeEnum.SERVER_ERROR, 'Could not parse error message sent by the server.')];
            }

            return errors;
        }

        public static parseJSONError(errorString: string): ProxyError[] {
            var commerceErrors: ProxyError[];

            try {
                var odataError: any = JSON.parse(errorString);

                // due to odata json format limitation, a collection of errors is provided in the message field
                var serializedException: string = odataError.error.message;
                var exceptionType: string = odataError.error.code;

                if (exceptionType === ErrorHelper.MICROSOFT_DYNAMICS_SERVER_INTERNAL_ERROR) {
                    commerceErrors = [new ProxyError(ErrorHelper.MICROSOFT_DYNAMICS_SERVER_INTERNAL_ERROR,
                        'Server failed with uncaught exception. Please report this failure.')];
                } else if (serializedException != null && exceptionType != null) {
                    var commerceException: Entities.CommerceException = JSON.parse(serializedException) || {};
                    commerceErrors = ErrorParser.convertExceptionToErrors(commerceException, exceptionType);
                } else {
                    throw 'Server exception is not in expected format.'
                }
            }
            catch (ex) {
                var errorMessage: string = "DataServiceRequest::parseJSONError: Invalid json format from server. It was not possible to parse error message. {0}";
                Tracer.Error(errorMessage, ex);
            }

            if (!ArrayExtensions.hasElements(commerceErrors)) {
                commerceErrors = [new ProxyError(ErrorTypeEnum.GENERICERRORMESSAGE, 'Could not parse error message from server. Possible invalid OData request, please check your request.')];
            }

            return commerceErrors;
        }

        private static convertExceptionToErrors(serverException: Entities.CommerceException, exceptionType: string): ProxyError[] {

            var errors: ProxyError[] = [];

            switch (exceptionType) {
                // CartValidationExceptionType extends DataValidationExceptionType
                case ErrorParser.DataValidationExceptionType:
                case ErrorParser.CartValidationExceptionType:
                    var dataValidation: Entities.DataValidationException = <Entities.DataValidationException>serverException;
                    // json is in odata format, array has elements inside result member
                    errors = ErrorParser.getErrorsFromDataValidationFailures((<any>dataValidation.ValidationResults));
                    errors.push(new ProxyError(dataValidation.ErrorResourceId, StringExtensions.EMPTY, dataValidation.LocalizedMessage));
                    break;

                case ErrorParser.BarcodeWithLinkedItemNotSupportedExceptionType:
                case ErrorParser.MissingRequiredReasonCodeExceptionType:
                    var exception: Entities.DataValidationException
                        = <Entities.DataValidationException>serverException;

                    var error: ProxyError = new ProxyError(
                        exception.ErrorResourceId,
                        StringExtensions.EMPTY,
                        exception.LocalizedMessage);
                    error.commerceException = exception;

                    errors.push(error);
                    break;

                case ErrorParser.StorageExceptionType:
                    errors.push(new ProxyError(ErrorTypeEnum.SERVICE_UNAVAILABLE, StringExtensions.EMPTY, serverException.LocalizedMessage, false));
                    break;

                case ErrorParser.PaymentExceptionType:
                    var paymentException: Entities.PaymentException = <Entities.PaymentException>serverException;
                    var paymentError: ProxyError = new ProxyError(
                        paymentException.ErrorResourceId,
                        StringExtensions.EMPTY,
                        paymentException.LocalizedMessage);
                    paymentError.commerceException = paymentException;

                    errors.push(paymentError);
                    break;

                // Everything extends CommerceRuntimeExceptionType
                case ErrorParser.CommerceExceptionType:
                default:
                    var commerceException: Entities.CommerceException = <Entities.CommerceException>serverException;
                    errors.push(new ProxyError(commerceException.ErrorResourceId, StringExtensions.EMPTY, serverException.LocalizedMessage));
                    break;
            }

            return errors;
        }

        private static getErrorsFromDataValidationFailures(failures: Entities.DataValidationFailure[]): ProxyError[] {
            var errors: ProxyError[] = [];
            failures = failures || [];

            for (var i = 0; i < failures.length; i++) {
                var failure: Entities.DataValidationFailure = failures[i];
                errors.push(new ProxyError(failure.ErrorResourceId, failure.ErrorContext || '', failure.LocalizedMessage));
            }

            return errors;
        }

        /**
         * Parses an error to get the error code and error message.
         *
         * @param {any} error The error message.
         * @retunrs {ProxyError[]} The collection of error messages.
         */
        private static parseError(error: any): ProxyError[] {
            var response = error.response;
            var errors: ProxyError[] = [];

            if (!ObjectExtensions.isNullOrUndefined(response.body)) {
                // If we have a response body
                var contentType: string = (response.headers["Content-Type"] || response.headers["content-type"]) || "";
                var bodyString: string = response.body;

                // Parse errors in json format
                if (contentType.toLowerCase().indexOf("application/json") != -1) {
                    errors = ErrorParser.parseJSONError(bodyString);
                } else {
                    var message: string = StringExtensions.format(
                        "Server error has been received with unsupported content type: '{0}'.",
                        contentType);
                    RetailLogger.genericWarning(message);
                }
            }

            // if we couldn't parse the errors from the body, we can only rely on status text and status code
            if (!ArrayExtensions.hasElements(errors)) {
                // In case we don't have a body, we don't know the error message but from http status code
                errors = [ErrorHelper.MapResponseStatusCodeToError(response.statusText, response.statusCode, error)];
            }

            return errors;
        }
    }
}