/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ProxyError.ts'/>
///<reference path='ErrorHelper.ts'/>
///<reference path='../Extensions/StringExtensions.ts'/>

module Commerce.Proxy {
    "use strict";

    export enum MessageType {
        Info,
        Error
    }

    export enum MessageBoxButtons {
        Default,
        OKCancel,
        YesNo,
        RetryNo
    }

    export enum DialogResult {
        Close,
        OK,
        Cancel,
        Yes,
        No
    }

    export class NotificationHandler {

        /**
         * Display mulitple client filtered error message.
         *
         * @param {ProxyError[]} errors The array of errors.
         * @param {string} titleResourceId The resource id of the error title. (optional)
         * @return {IAsyncResult<DialogResult>} The async dialog result.
         */
        public static displayClientErrors(errors: ProxyError[], titleResourceId?: string): IAsyncResult<DialogResult> {
            var errorMessage = ""
            var limitOfErrorLines = 5;

            var previousMessageCodes: string[] = [];

            if (ArrayExtensions.hasElements(errors)) {
                for (var i = 0; i < errors.length && i < limitOfErrorLines; i++) {
                    var error: ProxyError = errors[i];

                    Commerce.Proxy.Tracer.Error("NotificationHandler.displayMultipleErrorMessages() ErrorCode {0} ErrorMessage {1}", error.ErrorCode, error.ErrorMessage);

                    // skip aggregated messages if we have more than one error
                    if (errors.length > 1 && ErrorHelper.isAggregatedErrorResourceId(error.ErrorCode)) {
                        continue;
                    }

                    // server might send same error code more than once, make sure we don't display same message twice
                    if (previousMessageCodes.indexOf(error.ErrorCode) != -1) {
                        continue;
                    }

                    previousMessageCodes.push(error.ErrorCode);
                    errorMessage += NotificationHandler.clientError(error);
                    if (i != errors.length - 1) {
                        errorMessage += "\n";
                    }
                }

                //TODO anandjo to anandjo:
                //return Commerce.ViewModelAdapter.displayMessage(errorMessage, MessageType.Error, MessageBoxButtons.Default, !ObjectExtensions.isNullOrUndefined(titleResourceId) ? Commerce.ViewModelAdapter.getResourceString(titleResourceId) : null, 0);
                return null;
            }

            // Return a default result when there are no errors
            var asyncResult = new AsyncResult<DialogResult>(null);
            var dialogResult: DialogResult = DialogResult.Close;
            asyncResult.resolve(dialogResult);
            return asyncResult;
        }

        /**
        * Displays the specified error message.
        *
        * @param {string} resourceId The resource id of the error message.
        * @param {string} params The parameters. (optional)
        * @return {IAsyncResult<DialogResult>} The async dialog result.
        */
        public static displayErrorMessage(resourceId: string, ...params: any[]): IAsyncResult<DialogResult> {
            // Format the error message
            var errorMessage = StringExtensions.format(
                //Commerce.ViewModelAdapter.getResourceString(resourceId),
                "NotImplemented",
                params);

            // Log the error message
            Commerce.Proxy.Tracer.Error("NotificationHandler.displayErrorMessage() ErrorMessage {0}", errorMessage);

            //TODO anandjo to anandjo
            // Display the error message
            //return Commerce.ViewModelAdapter.displayMessage(errorMessage, MessageType.Error);
            return null;
        }

        /**
         * Returns the error message.
         *
         * @param {ProxyError[]} errors The error.
         */
        public static getErrorMessage(errors: ProxyError): string {
            return NotificationHandler.clientError(errors);
        }

        private static clientError(clientError: ProxyError): string {
            var localeStringReference: string;
            var message: string;
            var errorCode = clientError.ErrorCode.toUpperCase();

            // Transaction service errors comes already localized from server.
            if (errorCode == ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_HEADQUARTERTRANSACTIONSERVICEMETHODCALLFAILURE) {
                message = clientError.ErrorMessage;
            } else {
                var getEnum = ErrorTypeEnum[errorCode];

                if (!StringExtensions.isNullOrWhitespace(getEnum)) {
                    localeStringReference = getEnum;
                } else {
                    localeStringReference = clientError.ErrorCode;
                }

                //anandjo
                //message = Commerce.ViewModelAdapter.getResourceString(localeStringReference);
                message = "NotImplemented";

                // Special handling for payment errors 
                if ((errorCode === ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOCAPTUREPAYMENT.toUpperCase())
                    || (errorCode === ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOAUTHORIZEPAYMENT.toUpperCase())
                    || (errorCode === ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_MANUALCARDNUMBERNOTALLOWED.toUpperCase())
                    || (errorCode === ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_TENDERLINECANNOTBEVOIDED.toUpperCase())
                    || (errorCode === ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDPAYMENTREQUEST.toUpperCase())
                    || (errorCode === ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTUSINGUNAUTHORIZEDACCOUNT.toUpperCase())
                    || (errorCode === ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTKEYNOTFOUND.toUpperCase())
                    ) {
                    // This pattern should match what is used in VerifyResponseErrors() in Components\Platform\Libraries\Runtime\Services\Payment\CardPaymentService.cs
                    var errors: string[] = clientError.ErrorMessage.match(/ErrorCode:(.*)\r\n /g);
                    for (var i in errors) {
                        message += "\n" + errors[i];
                    }
                    Commerce.Proxy.Tracer.Error("NotificationHandler.DisplayClientError() payment errors: {0}", clientError.ErrorMessage);
                }                   
            }

            if (StringExtensions.isNullOrWhitespace(message) || message == localeStringReference) {
                Commerce.Proxy.Tracer.Warning("NotificationHandler.DisplayClientError() ErrorCode not found {0}", clientError.ErrorCode);
                //anandjo
                //message = Commerce.ViewModelAdapter.getResourceString(ErrorTypeEnum.APPLICATION_ERROR);
                message = "NotImplemented";
            }

            if (!ObjectExtensions.isNullOrUndefined(clientError.formatData))
            {
                message = StringExtensions.format(message, clientError.formatData);
            }

            return message;
        }
    }
}
