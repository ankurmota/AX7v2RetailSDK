/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Entities/Error.ts'/>
///<reference path='../../Extensions/ObjectExtensions.ts'/>
///<reference path='../../Extensions/StringExtensions.ts'/>
///<reference path='../../ErrorHandler.ts'/>
///<reference path='../../NotificationHandler.ts'/>

module Commerce.Proxy.Context {
    "use strict";

    /**
     * Error parser class.
     */
    export class ErrorParser {
        private static EXCEPTION_CLASS_BY_TYPE_NAME: { [typeName: string]: any } = {
            "MissingRequiredReasonCodeException": Proxy.Entities.MissingRequiredReasonCodeExceptionClass
        };

        /**
         * Parses an error message.
         * @param {Common.IXmlHttpResponse} response The response message.
         * @return {Entities.Error[]} The collection of error messages.
         */
        public static parseErrorMessage(response: Common.IXmlHttpResponse): Entities.Error[] {
            var errors: Entities.Error[] = [];

            if (!ObjectExtensions.isNullOrUndefined(response)) {

                var handled: boolean = true;
                RetailLogger.modelManagersServerResponseStatusCode(response.statusCode);

                switch (response.statusCode) {

                    case 306: // Custom - Redirection
                        var redirectUrl: string = ErrorParser.getHeaderValue(response.headers, "Location");
                        errors.push(new Entities.Error(ErrorTypeEnum.RETAIL_SERVER_REDIRECT_ERROR, false, response.statusText, redirectUrl));
                        break;

                    case 408: // Request Timeout
                        errors.push(new Entities.Error(ErrorTypeEnum.SERVER_TIMEOUT, false, response.statusText));
                        break;

                    // WWAHost reports status code as 0, If server is unreachable.
                    case 0:

                    // This is also an indication that server is unreachable. 404 can be returned by IIS when the RS endpoint is not available,
                    // or by Retail Server WebAPI layer when requesting url is not found.
                    case 404:
                        var extraData: any = null;

                        if (ErrorParser.isPossibleLoopbackBlocked(response)) {
                            extraData = {
                                additionalInfo: ErrorTypeEnum.POSSIBLE_LOOPBACK_BLOCKED
                            };
                        }

                        errors.push(new Entities.Error(ErrorTypeEnum.SERVICE_UNAVAILABLE, false, response.statusText, extraData));
                        break;
                    case 500:
                        errors = ErrorParser.parseError(response);
                        if (!ArrayExtensions.hasElements(errors) || errors[0].ErrorCode === ErrorTypeEnum.SERVER_INTERNAL_ERROR) {
                            errors.push(new Proxy.Entities.Error(ErrorTypeEnum.SERVER_INTERNAL_ERROR));
                        }

                        break;
                    case 502:
                        var responseText: string = response.body;

                        if (!StringExtensions.isNullOrWhitespace(responseText)) {
                            responseText = responseText.toUpperCase();
                        }

                        if (responseText.indexOf(ErrorHelper.HTTPRESPONSE_DNS) >= 0 &&
                            responseText.indexOf(ErrorHelper.HTTPRESPONSE_FAILED) >= 0) {
                            errors.push(new Proxy.Entities.Error(
                                ErrorTypeEnum.MICROSOFT_DYNAMICS_POS_SERVERCONNECTIVITYCHECK_DNS_LOOKUP_FAILED.serverErrorCode));
                        } else if (responseText.indexOf(ErrorHelper.HTTPRESPONSE_TIMED) >= 0
                            && responseText.indexOf(ErrorHelper.HTTPRESPONSE_OUT) >= 0) {
                            errors.push(new Proxy.Entities.Error(
                                ErrorTypeEnum.MICROSOFT_DYNAMICS_POS_SERVERCONNECTIVITYCHECK_SERVER_TIMED_OUT.serverErrorCode));
                        } else if (responseText.indexOf(ErrorHelper.HTTPRESPONSE_FIREWALL) >= 0) {
                            errors.push(new Proxy.Entities.Error(
                                ErrorTypeEnum.MICROSOFT_DYNAMICS_POS_SERVERCONNECTIVITYCHECK_FIREWALL_BLOCKED.serverErrorCode));
                        } else {
                            errors.push(new Proxy.Entities.Error(
                                ErrorTypeEnum.MICROSOFT_DYNAMICS_POS_SERVERCONNECTIVITYCHECK_ERROR.serverErrorCode));
                        }
                        break;
                    case 503: // Service Unavailable
                    case 504: // Gateway timeout
                        if (Commerce.Session.instance.connectionStatus === ConnectionStatusType.Online) {
                            RetailLogger.genericError(response.statusText);
                        }

                        errors.push(new Entities.Error(ErrorTypeEnum.SERVICE_UNAVAILABLE));
                        break;

                    default:
                        handled = false;
                        break;
                }

                if (!handled) {
                    errors = ErrorParser.parseError(response);
                }
            }

            // If we couldn't create any error object from error message, add error parsing failure
            if ((!errors) || (!errors.length)) {
                RetailLogger.genericError("Could not parse error message sent by the server.");
                errors = [new Entities.Error(ErrorTypeEnum.SERVER_INTERNAL_ERROR)];
            }

            return errors;
        }

        /**
         * Parses an error message sent from Hardware station.
         * @param {any} error The error message.
         * @retunrs {Entities.Error[]} The collection of error messages.
         */
        public static parseHardwareStationErrorMessage(error: any): Entities.Error[] {
            var errors: Entities.Error[];

            if (!ObjectExtensions.isNullOrUndefined(error)) {
                try {
                    var response: string = error.responseText;

                    if (response) {
                        var jsonResponse: any = JSON.parse(response);

                        // Parse errors in json format
                        if (jsonResponse) {
                            errors = new Array<Entities.Error>();

                            // Retrieves the exception class name, like "Microsoft.Dynamics.Commerce.HardwareStation.PaymentException".
                            var fullExceptionName: string = jsonResponse.ClassName;

                            // Parses exception first, if it is a PaymentException with list of PaymentSdkErrors; 
                            // fallbacks to outer exception, otherwise.
                            if (!StringExtensions.isNullOrWhitespace(fullExceptionName)
                                && fullExceptionName.indexOf(ErrorHandling.CommerceExceptionTypes.PAYMENT_EXCEPTION_TYPE) > -1
                                && jsonResponse.PaymentSdkErrors) {
                                for (var i: number = 0; i < jsonResponse.PaymentSdkErrors.length; i++) {
                                    var code: string = ObjectExtensions.isNullOrUndefined(jsonResponse.PaymentSdkErrors[i].Code)
                                        ? PaymentErrorTypeEnum[PaymentErrorHelper.GENERAL_EXCEPTION_ERROR_CODE]
                                        : PaymentErrorTypeEnum[jsonResponse.PaymentSdkErrors[i].Code.toString()];

                                    // This handles both numeric error code with the string error code case.
                                    var errorResId: string = PaymentErrorHelper.PAYMENT_EXCEPTION_NAMESPACE
                                        + (ObjectExtensions.isNullOrUndefined(code) ? jsonResponse.PaymentSdkErrors[i].Code : code);
                                    errors.push(new Entities.Error(errorResId.toUpperCase()));
                                }
                            } else {
                                var message: string = StringExtensions.format(
                                    "Exception occurred while communicating with Hardware Station. Exception: {0}",
                                    error.responseText);
                                RetailLogger.genericError(message);
                                errors = [new Entities.Error(
                                    jsonResponse.ErrorResourceId
                                        ? jsonResponse.ErrorResourceId
                                        : ErrorTypeEnum.PERIPHERALS_HARDWARESTATION_COMMUNICATION_FAILED,
                                    false,
                                    jsonResponse.Message || "")];
                            }
                        } else {
                            // If not in json, we cannot parse it
                            RetailLogger.genericError("The value of Content-Type from hardware station response is not supported.");
                            errors = [new Entities.Error(ErrorTypeEnum.PERIPHERALS_HARDWARESTATION_COMMUNICATION_FAILED)];
                        }
                    } else {
                        // In case we don't have a body, we don't know the error message but from http status
                        var message: string = StringExtensions.format(
                            "Exception occurred while communicating with Hardware Station. Exception: {0}",
                            error.responseText);
                        RetailLogger.genericError(message);

                        // This probably is a connection failure for upper layers.
                        errors = [new Entities.Error(ErrorTypeEnum.PERIPHERALS_HARDWARESTATION_COMMUNICATION_FAILED)];
                    }
                } catch (ex) {
                    RetailLogger.applicationFailedToParseError(ex.toString());
                }
            }

            // If we couldn't create any error object from error message, add error parsing failure
            if ((!errors) || (!errors.length)) {
                RetailLogger.genericError("Could not parse error message sent by the hardware station.");
                errors = [new Entities.Error(ErrorTypeEnum.PERIPHERALS_HARDWARESTATION_COMMUNICATION_FAILED)];
            }

            return errors;
        }

        public static parseJSONError(errorString: string): Entities.Error[] {
            var commerceErrors: Entities.Error[];

            try {
                var deserializedError: any = JSON.parse(errorString);

                // tries to parse commerce error in the Exception field, if any
                if (deserializedError != null) {
                    var exceptionType: string = deserializedError.TypeName;
                    var serializedException: string = deserializedError.Exception;

                    if (exceptionType != null && serializedException != null) {
                        var commerceException: Entities.CommerceException = JSON.parse(serializedException) || {};
                        commerceErrors = ErrorParser.convertExceptionToErrors(commerceException, exceptionType);
                    } else {
                        throw new Error("Server exception is not in expected format.");
                    }
                } else {
                    throw new Error("Server exception is not in expected format.");
                }
            } catch (ex) {
                RetailLogger.applicationFailedToParseErrorInvalidJson(ex.toString());
            }

            if (!ArrayExtensions.hasElements(commerceErrors)) {
                RetailLogger.genericError("Could not parse error message from server. Possible invalid OData request, please check your request.");
                commerceErrors = [new Entities.Error(ErrorTypeEnum.GENERICERRORMESSAGE)];
            }

            return commerceErrors;
        }

        private static convertExceptionToErrors(serverException: Entities.CommerceException, exceptionTypeName: string): Entities.Error[] {

            var errors: Entities.Error[] = [];

            switch (exceptionTypeName) {
                case ErrorHandling.CommerceExceptionTypes.ITEM_DISCONTINUED_EXCEPTION_TYPE:
                    var itemDiscontinuedException: Entities.ItemDiscontinuedException = <Entities.ItemDiscontinuedException>serverException;
                    var itemDiscontinuedError: Entities.Error = new Entities.Error(
                        itemDiscontinuedException.ErrorResourceId,
                        false,
                        itemDiscontinuedException.LocalizedMessage,
                        null,
                        itemDiscontinuedException.RemovedUnavailableItemIds
                    );
                    itemDiscontinuedError.commerceException = itemDiscontinuedException;
                    errors.push(itemDiscontinuedError);
                    break;
                // CART_VALIDATION_EXCEPTION_TYPE extends DATA_VALIDATION_EXCEPTION_TYPE
                case ErrorHandling.CommerceExceptionTypes.DATA_VALIDATION_EXCEPTION_TYPE:
                case ErrorHandling.CommerceExceptionTypes.CART_VALIDATION_EXCEPTION_TYPE:
                    var dataValidation: Entities.DataValidationException = <Entities.DataValidationException>serverException;
                    // json is in odata format, array has elements inside result member
                    errors = ErrorParser.getErrorsFromDataValidationFailures((<any>dataValidation.ValidationResults));
                    errors.push(new Entities.Error(dataValidation.ErrorResourceId, false, dataValidation.LocalizedMessage));
                    break;

                case ErrorHandling.CommerceExceptionTypes.MISSING_REQUIRED_REASON_CODE_EXCEPTION:
                    var exception: Entities.DataValidationException
                        = <Entities.DataValidationException>serverException;

                    var error: Entities.Error = new Entities.Error(
                        exception.ErrorResourceId, false,
                        exception.LocalizedMessage);

                    var exceptionType: (object: any) => void = ErrorParser.EXCEPTION_CLASS_BY_TYPE_NAME[exceptionTypeName];
                    error.commerceException = new exceptionType(exception);

                    errors.push(error);
                    break;

                case ErrorHandling.CommerceExceptionTypes.PAYMENT_EXCEPTION_TYPE:
                    var paymentException: Entities.PaymentException = <Entities.PaymentException>serverException;
                    var paymentError: Entities.Error = new Entities.Error(
                        paymentException.ErrorResourceId,
                        false,
                        paymentException.LocalizedMessage);
                    paymentError.commerceException = paymentException;

                    errors.push(paymentError);
                    break;

                case ErrorHandling.CommerceExceptionTypes.TENDER_VALIDATION_EXCEPTION:
                    var tenderValidationException: Entities.TenderValidationException
                        = <Entities.TenderValidationException>serverException;
                    var exceedsMaximumDifferenceAmountError: Entities.Error = new Entities.Error(
                        tenderValidationException.ErrorResourceId,
                        false,
                        tenderValidationException.LocalizedMessage,
                        null,
                        tenderValidationException.TenderTypeId);
                    exceedsMaximumDifferenceAmountError.commerceException = tenderValidationException;

                    errors.push(exceedsMaximumDifferenceAmountError);
                    break;

                // Everything extends COMMERCE_RUNTIME_EXCEPTION_TYPE
                case ErrorHandling.CommerceExceptionTypes.COMMERCE_EXCEPTION_TYPE:
                default:
                    var commerceException: Entities.CommerceException = <Entities.CommerceException>serverException;
                    errors.push(new Entities.Error(commerceException.ErrorResourceId, false, commerceException.LocalizedMessage));
                    break;
            }

            for (var i: number = 0; i < errors.length; i++) {
                errors[i].commerceExceptionType = exceptionTypeName;
            }

            return errors;
        }

        private static getErrorsFromDataValidationFailures(failures: Entities.DataValidationFailure[]): Entities.Error[] {
            var errors: Entities.Error[] = [];
            failures = failures || [];

            for (var i: number = 0; i < failures.length; i++) {
                var failure: Entities.DataValidationFailure = failures[i];
                RetailLogger.genericError(failure.ErrorContext);
                errors.push(new Entities.Error(failure.ErrorResourceId, false, failure.LocalizedMessage));
            }

            return errors;
        }

        /**
         * Parses an error to get the error code and error message.
         * @param {Common.IXmlHttpResponse} response The error response.
         * @retunrs {Entities.Error[]} The collection of error messages.
         */
        private static parseError(response: Common.IXmlHttpResponse): Entities.Error[] {
            var errors: Entities.Error[] = [];

            if (!ObjectExtensions.isNullOrUndefined(response.body)) {
                // If we have a response body
                var contentType: string = ErrorParser.getHeaderValue(response.headers, Common.HttpHeaders.CONTENT_TYPE);
                var bodyString: string = response.body;

                // Parse errors in json format
                if (contentType.toLowerCase().indexOf(Common.MimeTypes.APPLICATION_JSON) !== -1) {
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
                errors = [ErrorHelper.MapResponseStatusCodeToError(response.statusText, response.statusCode)];
            }

            return errors;
        }

        private static isPossibleLoopbackBlocked(response: Common.IXmlHttpResponse): boolean {
            var requestUri: string = response ? response.requestUri : null;
            return UrlHelper.isLocalAddress(requestUri);
        }

        /**
         * Gets the specified response header value.
         * @param {any} headers The object containing the headers.
         * @param {string} headerName The name of the header to get.
         * @returns {string} The value of the header if it is found. An empty string if it is not.
         */
        private static getHeaderValue(headers: any, headerName: string): string {
            return (headers[headerName] || headers[headerName.toLowerCase()]) || StringExtensions.EMPTY;
        }
    }
}