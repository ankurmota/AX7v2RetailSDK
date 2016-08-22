/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='Entities/Error.ts'/>
///<reference path='Extensions/StringExtensions.ts'/>
///<reference path='Utilities/ErrorHelper.ts'/>
///<reference path='Core.d.ts'/>

module Commerce {
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

    interface IFormattedErrorMessage {
        errorMessage: string;
        additionalInfo: string;
    }

    class FormattedErrorMessage implements IFormattedErrorMessage {
        errorMessage: string = null;
        additionalInfo: string = null;

        /**
         * Append text of the current message object with a text from the given message object.
         *
         * @param {IFormattedErrorMessage} The message object to append to the current object.
         */
        public append(appendMessage: IFormattedErrorMessage): void {
            this.errorMessage = FormattedErrorMessage.appendString(this.errorMessage, appendMessage.errorMessage);
            this.additionalInfo = FormattedErrorMessage.appendString(this.additionalInfo, appendMessage.additionalInfo);
        }

        private static appendString(originalStr: string, appendStr: string): string {
            return appendStr ? originalStr ? originalStr + "\n" + appendStr : appendStr : originalStr;
        }
    }

    export class NotificationHandler {

        /**
         * Gets the error message from the list of errors.
         *
         * @param {Model.Entities.Error[]} errors The array of errors.
         * @return {IFormattedErrorMessage} The structure containing error message and additional information.
         */
        private static getFormattedErrorMessage(errors: Model.Entities.Error[]): IFormattedErrorMessage {
            var message = new FormattedErrorMessage();
            var limitOfErrorLines = 5;
            var previousMessageCodes: string[] = [];

            if (ArrayExtensions.hasElements(errors)) {
                for (var i = 0; i < errors.length && i < limitOfErrorLines; i++) {
                    var error: Model.Entities.Error = errors[i];
                    var currentMessage: IFormattedErrorMessage;

                    currentMessage = NotificationHandler.clientError(error);
                    RetailLogger.errorMessageDisplay(error.ErrorCode, currentMessage.errorMessage);

                    // skip aggregated messages if we have more than one error
                    if (errors.length > 1 && ErrorHelper.isAggregatedErrorResourceId(error.ErrorCode)) {
                        continue;
                    }

                    // server might send same error code more than once, make sure we don't display same message twice
                    if (previousMessageCodes.indexOf(error.ErrorCode) != -1) {
                        continue;
                    }

                    previousMessageCodes.push(error.ErrorCode);

                    // Append error to the aggregated message.
                    message.append(currentMessage);
                }
            }

            return message;
        }

        /**
         * Display mulitple client filtered error message and ask whether to show the error again.
         *
         * @param {Model.Entities.Error[]} errors The array of errors.
         * @param {string} titleResourceId The resource id of the error title. (optional)
         * @return {IAsyncResult<DialogResult>} The async dialog result.
         */
        public static displayClientErrorsWithShowAgain(errors: Model.Entities.Error[], titleResourceId?: string): IAsyncResult<IMessageResult> {
            if (ArrayExtensions.hasElements(errors)) {
                var message: IFormattedErrorMessage = NotificationHandler.getFormattedErrorMessage(errors);

                var messageOptions: Commerce.IMessageOptions = {
                    title: titleResourceId ? Commerce.ViewModelAdapter.getResourceString(titleResourceId) : null,
                    additionalInfo: message.additionalInfo,
                    messageType: MessageType.Error,
                    displayMessageCheckbox: true,
                    messageCheckboxChecked: false,
                    messageCheckboxLabelResourceID: null
                };

                return Commerce.ViewModelAdapter.displayMessageWithOptions(message.errorMessage, messageOptions);
            }

            // Return a default result when there are no errors
            var asyncResult = new AsyncResult<IMessageResult>(null);
            var dialogResult: DialogResult = DialogResult.Close;
            asyncResult.resolve({ dialogResult: dialogResult, messageCheckboxChecked: false });
            return asyncResult;
        }

        /**
         * Display mulitple client filtered error message.
         *
         * @param {Model.Entities.Error[]} errors The array of errors.
         * @param {string} titleResourceId The resource id of the error title. (optional)
         * @return {IAsyncResult<DialogResult>} The async dialog result.
         */
        public static displayClientErrors(errors: Model.Entities.Error[], titleResourceId?: string): IAsyncResult<IMessageResult> {
            if (ArrayExtensions.hasElements(errors)) {
                var message = NotificationHandler.getFormattedErrorMessage(errors);

                return ViewModelAdapter.displayMessageWithOptions(message.errorMessage, {
                    title: titleResourceId ? Commerce.ViewModelAdapter.getResourceString(titleResourceId) : null,
                    additionalInfo: message.additionalInfo,
                    messageType: MessageType.Error
                });
            }

            // Return a default result when there are no errors
            var asyncResult = new AsyncResult<IMessageResult>();
            asyncResult.resolve({ dialogResult: DialogResult.Close });
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
                Commerce.ViewModelAdapter.getResourceString(resourceId),
                ...params);

            // Log the error message
            RetailLogger.errorMessageDisplay("none", errorMessage);

            // Display the error message
            return Commerce.ViewModelAdapter.displayMessage(errorMessage, MessageType.Error);
        }

        /**
         * Returns the error message.
         *
         * @param {Model.Entities.Error[]} errors The error.
         */
        public static getErrorMessage(errors: Model.Entities.Error): string {
            return NotificationHandler.clientError(errors).errorMessage;
        }

        private static clientError(clientError: Model.Entities.Error): IFormattedErrorMessage {
            var message = new FormattedErrorMessage();

            if (clientError) {
                message.errorMessage = ErrorHelper.formatErrorMessage(clientError);
                if (clientError.extraData && clientError.extraData.additionalInfo) {
                    message.additionalInfo = ViewModelAdapter.getResourceString(clientError.extraData.additionalInfo);
                }
            }

            return message;
        }
    }
}
