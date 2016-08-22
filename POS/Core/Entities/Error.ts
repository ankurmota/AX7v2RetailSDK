/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Proxy.Entities {
    "use strict";

    /**
     * Class for representing an error.
     */
    export class Error {

        private _errorCode: string;
        private _externalLocalizedErrorMessage: string;
        private _canRetry: boolean = false;
        private _commerceException: any;
        private _formatData: any[];
        private _extraData: any;
        private _exceptionType: string;

        constructor(errorCode: string, canRetry: boolean = false, externalLocalizedErrorMessage: string = StringExtensions.EMPTY, extraData: any = null, ...formatData: any[]) {
            this._errorCode = errorCode;
            this._externalLocalizedErrorMessage = externalLocalizedErrorMessage;
            this._canRetry = canRetry;
            this._commerceException = null;
            this._formatData = formatData;
            this._extraData = extraData; 
        }
        
        /**
         * Gets the commerce exception that caused this error.
         *
         * @return {any} The commerce exception.
         */
        public get commerceException(): any {
            return this._commerceException;
        }

        /**
         * Sets the commerce exception that caused this error.
         *
         * @param {any} exception The commerce exception.
         */
        public set commerceException(exception: any) {
            this._commerceException = exception;
        }

        /**
         * Gets the commerce exception type that caused this error.
         * @return {string} The commerce exception type.
         */
        public get commerceExceptionType(): string {
            return this._exceptionType;
        }

        /**
         * Sets the commerce exception type that caused this error.
         * @param {string} exceptionType The commerce exception type.
         */
        public set commerceExceptionType(exceptionType: string) {
            this._exceptionType = exceptionType;
        }

        /**
         * Gets the error code.
         *
         * @return {string} The error code.
         */
        public get ErrorCode(): string {
            return this._errorCode;
        }

        /**
         * Gets the localized error message.
         *
         * @return {string} The localized error message.
         */
        public get ExternalLocalizedErrorMessage(): string {
            return this._externalLocalizedErrorMessage;
        }

        /**
         * Gets the format data used on formatted error messages.
         *
         * @return {any[]} The format data.
         */
        public get formatData(): any[] {
            return this._formatData;
        }

        /**
         * Gets the additional information the might be supplied by the error.
         *
         * @return {any} The extra data.
         */
        public get extraData(): any {
            return this._extraData;
        }

        /**
         * Gets the error title. Defaults to the resource string string_29004 for the error title if
         * no error title is mapped for the error.
         */
        public get ErrorTitleResourceId(): string {
            var errorTitleResourceId = "string_29004"; // Error string: There is a problem with the server
            return errorTitleResourceId;
        }

        /**
         * Gets whether the action that generated the error can be retried.
         *
         * @return {string} Whether the action that generated the error can be retried.
         */
        public get CanRetry(): boolean {
            return this._canRetry;
        }
    }
}