/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Proxy.Requests {
    "use strict";

    /**
     * Class represents retail server request factory.
     */
    export class RetailServerRequestFactory implements Common.IDataServiceRequestFactory {

        private _serverUri: string;
        private _locale: string;

        constructor(serverUri: string, locale: string) {
            this._serverUri = serverUri;
            this._locale = locale;
        }

        /**
         * Gets Locale for the current data service factory instance.
         */
        public get locale(): string {
            return this._locale;
        }

        /**
         * Create a request.
         * @param {Common.IDataServiceQueryInternal} dataServiceQuery The data service query.
         * @return {IDataServiceRequest} The data service request.
         */
        public create(dataServiceQuery: Common.IDataServiceQueryInternal): Common.IDataServiceRequest {
            return new RetailServerRequest(this._serverUri, dataServiceQuery, this._locale);
        }
    }
}