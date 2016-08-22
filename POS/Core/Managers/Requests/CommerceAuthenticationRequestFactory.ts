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
     * Class represents a request factory for Commerce Authentication requests.
     */
    export class CommerceAuthenticationRequestFactory implements Common.IDataServiceRequestFactory {
        private static AUTH_ENDPOINT_NAME: string = "Auth";

        private endpointUri: string;
        private _locale: string;

        constructor(endpointUri: string, locale: string) {
            this.setEndpointUrl(endpointUri);
            this._locale = locale;
        }

        /**
         * Gets Locale for the current data service factory instance.
         */
        public get locale(): string {
            return this._locale;
        }

        /**
         * Sets Locale for the current data service factory instance.
         */
        public set locale(locale: string) {
            this._locale = locale;
        }

        /**
         * Create a request.
         * @param {Common.IDataServiceQueryInternal} dataServiceQuery The data service query.
         * @return {IDataServiceRequest} The data service request.
         */
        public create(dataServiceQuery: Common.IDataServiceQueryInternal): Common.IDataServiceRequest {
            return new CommerceAuthenticationRequest(
                this.endpointUri,
                dataServiceQuery);
        }



        /**
         * Sets the endpoint url.
         * @param {string} retailServerUrl the endpoint url of retail server.
         */
        private setEndpointUrl(serverUrl: string): void {
            // removes trailing Commerce and training "/" from the url
            var baseAuthEndpointUrl: string = (serverUrl || "").replace(/(\/)?(Commerce)?(\/)?$/i, "");
            this.endpointUri = StringExtensions.format("{0}/{1}", baseAuthEndpointUrl, CommerceAuthenticationRequestFactory.AUTH_ENDPOINT_NAME);
        }
    }
}