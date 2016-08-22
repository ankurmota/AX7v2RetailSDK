/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

// Following reference is added to fix the build dependency between CommerceContxt and DataServiceQuery.
// This will eventually be moved to CommerceContext.ts when MPOS proxy and Commerce Proxy are merged.
///<reference path='..\DataServiceQuery.ts'/>

module Commerce.Proxy {
    "use strict";

    /**
     * Class for representing an error.
     */
    export class ProxyError {

        private _errorCode: string;
        private _errorMessage: string;
        private _localizedErrorMessage: string;
        private _canRetry: boolean = false;
        private _commerceException: any;
        private _formatData: any[];
        private _extraData: any;

        constructor(errorCode: string, errorMessage: string, localizedErrorMessage: string = StringExtensions.EMPTY, canRetry: boolean = false, extraData: any = null, ...formatData: any[]) {
            this._errorCode = errorCode;
            this._errorMessage = errorMessage;
            this._localizedErrorMessage = localizedErrorMessage;
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
         * Gets the error code.
         *
         * @return {string} The error code.
         */
        public get ErrorCode(): string {
            return this._errorCode;
        }

        /**
         * Gets the error message.
         *
         * @return {string} The error message.
         */
        public get ErrorMessage(): string {
            return this._errorMessage;
        }

        /**
         * Gets the localized error message.
         *
         * @return {string} The localized error message.
         */
        public get LocalizedErrorMessage(): string {
            return this._localizedErrorMessage;
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