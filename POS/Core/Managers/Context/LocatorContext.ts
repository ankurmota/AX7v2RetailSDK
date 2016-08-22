/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Proxy.Context {

    export class LocatorContext {

        private _serverUri: string;
        private _dataServiceRequestFactory: Common.IDataServiceRequestFactory;

        constructor(serverUri: string) {
            this.serverUri = serverUri;
        }

        public get serverUri(): string {
            return this._serverUri;
        }

        public set serverUri(serverUri: string) {
            this._serverUri = serverUri;
            this._dataServiceRequestFactory = new Requests.LocatorServiceRequestFactory(serverUri);
        }

        public getRetailServerUrl(tenantId: string): Common.IDataServiceRequest {
            var query: Common.DataServiceQuery<any> = new Common.DataServiceQuery<any>(
                this._dataServiceRequestFactory,
                "ServiceEndpoints",
                "ServiceEndpoint",
                Locator.Model.Entities.ServiceEndpointClass,
                null);

            query.filter("TenantId eq guid" + "'" + tenantId + "'");
            var request: Common.IDataServiceRequest = query.read();

            return request;
        }
    }
}