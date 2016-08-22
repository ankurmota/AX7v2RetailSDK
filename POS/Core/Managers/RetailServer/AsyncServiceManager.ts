/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../IAsyncServiceManager.ts'/>

module Commerce.Model.Managers.RetailServer {
    "use strict";

    import Common = Proxy.Common;

    export class AsyncServiceManager implements Commerce.Model.Managers.IAsyncServiceManager {
        private _commerceContext: Proxy.CommerceContext = null;

        constructor(commerceContext: Proxy.CommerceContext) {
            this._commerceContext = commerceContext;
        }

        /**
         * Get download interval.
         * @param {string} dataStoreName The data store name.
         * @return {IAsyncResult<string>} The async result with the download interval.
         */
        public getDownloadIntervalAsync(dataStoreName: string): IAsyncResult<string> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getDownloadInterval(dataStoreName);
            return request.execute<string>();
        }

        /**
         * Get download link.
         * @param {string} dataStoreName The data store name.
         * @param {number} downloadSessionId The download session identifier.
         * @return {IAsyncResult<string>} The async result with the download link.
         */
        public getDownloadLinkAsync(dataStoreName: string, downloadSessionId: number): IAsyncResult<string> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getDownloadLink(dataStoreName, downloadSessionId);
            return request.execute<string>();
        }

        /**
         * Get download sessions.
         * @param {string} dataStoreName The data store name.
         * @return {IAsyncResult<Entities.DownloadSession[]>} The async result with the download session array.
         */
        public getDownloadSessionsAsync(dataStoreName: string): IAsyncResult<Entities.DownloadSession[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations()
                .getDownloadSessions(dataStoreName);
            return request.execute<Entities.DownloadSession[]>();
        }

        /**
         * Get data store name by terminal identifier.
         * @param {string} terminalId The terminal identifier.
         * @return {IAsyncResult<string>} The async result with the data store name.
         */
        public getTerminalDataStoreNameAsync(dataStoreName: string): IAsyncResult<string> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getTerminalDataStoreName(dataStoreName);
            return request.execute<string>();
        }

        /**
         * Get upload job definitions.
         * @param {string} dataStoreName The data store name.
         * @return {IAsyncResult<string[]>} The async result with the upload job definition array.
         */
        public getUploadJobDefinitionsAsync(dataStoreName: string): IAsyncResult<string[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getUploadJobDefinitions(dataStoreName);
            return request.execute<string[]>();
        }

        /**
         * Get upload interval.
         * @param {string} dataStoreName The data store name.
         * @return {IAsyncResult<string>} The async result with the upload interval.
         */
        public getUploadIntervalAsync(dataStoreName: string): IAsyncResult<string> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getUploadInterval(dataStoreName);
            return request.execute<string>();
        }

        /**
         * Sync offline transaction.
         * @param {string[]} offlineTransactionData The offline transaction data.
         * @return {IAsyncResult<boolean>} The async result with a boolean that is true if data is applied, false otherwise.
         */
        public syncOfflineTransactionAsync(offlineTransactionData: string[]): IAsyncResult<boolean> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations()
                .postOfflineTransactions(offlineTransactionData);
            return request.execute<boolean>();
        }

        /**
         * Update download session status.
         * @param {Entities.DownloadSession} The download session.
         * @return {IAsyncResult<boolean>} The async result with a boolean that is true if status is updated, false otherwise.
         */
        public updateDownloadSessionAsync(downloadSession: Entities.DownloadSession): IAsyncResult<boolean> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().updateDownloadSession(downloadSession);
            return request.execute<boolean>();
        }
    }
}