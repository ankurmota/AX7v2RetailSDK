/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../IAsyncResult.ts'/>

module Commerce.Model.Managers {
    "use strict";

    
    export var IAsyncServiceManagerName: string = "IAsyncServiceManager";
    

    export interface IAsyncServiceManager {
        /**
         * Get download interval.
         * @param {string} dataStoreName The data store name.
         * @return {IAsyncResult<string>} The async result with the download interval.
         */
        getDownloadIntervalAsync(dataStoreName: string): IAsyncResult<string>;

        /**
         * Get download link.
         * @param {string} dataStoreName The data store name.
         * @param {number} downloadSessionId The download session identifier.
         * @return {IAsyncResult<string>} The async result with the download link.
         */
        getDownloadLinkAsync(dataStoreName: string, downloadSessionId: number): IAsyncResult<string>;

        /**
         * Get download sessions.
         * @param {string} dataStoreName The data store name.
         * @return {IAsyncResult<Entities.DownloadSession[]>} The async result with the download session array.
         */
        getDownloadSessionsAsync(dataStoreName: string): IAsyncResult<Entities.DownloadSession[]>;

        /**
         * Get data store name by terminal identifier.
         * @param {string} terminalId The terminal identifier.
         * @return {IAsyncResult<string>} The async result with the data store name.
         */
        getTerminalDataStoreNameAsync(terminalId: string): IAsyncResult<string>;

        /**
         * Get upload job definitions.
         * @param {string} dataStoreName The data store name.
         * @return {IAsyncResult<string[]>} The async result with the upload job definition array.
         */
        getUploadJobDefinitionsAsync(dataStoreName: string): IAsyncResult<string[]>;

        /**
         * Get upload interval.
         * @param {string} dataStoreName The data store name.
         * @return {IAsyncResult<string>} The async result with the upload interval.
         */
        getUploadIntervalAsync(dataStoreName: string): IAsyncResult<string>;

        /**
         * Sync offline transaction.
         * @param {string[]} offlineTransactionData The offline transaction data.
         * @return {IAsyncResult<boolean>} The async result with a boolean that is true if data is applied, false otherwise.
         */
        syncOfflineTransactionAsync(offlineTransactionData: string[]): IAsyncResult<boolean>;

        /**
         * Update download session status.
         * @param {Entities.DownloadSession} The download session.
         * @return {IAsyncResult<boolean>} The async result with a boolean that is true if status is updated, false otherwise.
         */
        updateDownloadSessionAsync(downloadSession: Entities.DownloadSession): IAsyncResult<boolean>;
    }
}
