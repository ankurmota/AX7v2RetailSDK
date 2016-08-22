/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Entities/Error.ts'/>
///<reference path='../../Extensions/StringExtensions.ts'/>
///<reference path='../Context/CommerceContext.g.ts'/>
///<reference path='../IRecordingManager.ts'/>

module Commerce.Model.Managers.RetailServer {
    "use strict";

    import Common = Proxy.Common;

    export class RecordingManager implements Commerce.Model.Managers.IRecordingManager {

        private _commerceContext: Proxy.CommerceContext;

        /**
         * Constructor.
         * @param {Proxy.CommerceContext} commerceContext The commerce context.
         */
        constructor(commerceContext: Proxy.CommerceContext) {
            this._commerceContext = commerceContext;
        }

        /**
         * Serializes the recording object and generates an XML.
         * @param {Entities.Recording} recording The recording that will be serialized.
         * @return {IAsyncResult<string>} The async result.
         */
        public generateRecordingFile(recording: Entities.Recording): IAsyncResult<string> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().generateRecordingFile(recording);
            return request.execute<string>();
        }

        /**
         * Generates the required document for the recording.
         * @param {Entities.Recording} recording The recording that will be transformed to a document.
         * @return {IAsyncResult<string>} The async result.
         */
        public generateTrainingDocument(recording: Entities.Recording): IAsyncResult<string> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().generateTrainingDocument(recording);
            return request.execute<string>();
        }

        /**
         * Generates the BPM package from the recording object.
         * @param {Entities.Recording} recording The recording that will be transformed to a BPM package.
         * @return {IAsyncResult<string>} The async result.
         */
        public generateBusinessProcessModelPackage(recording: Entities.Recording): IAsyncResult<string> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().generateBusinessProcessModelPackage(recording);
            return request.execute<string>();
        }

        /**
         * Generates the recording bundle from the recording object.
         * @param {Entities.Recording} recording The recording that will be transformed to a recording bundle.
         * @return {IAsyncResult<string>} The async result.
         */
        public generateRecordingBundle(recording: Entities.Recording): IAsyncResult<string> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().generateRecordingBundle(recording);
            return request.execute<string>();
        }

        /**
         * Returns the list of BPM libraries.
         * @return {IAsyncResult<Entities.Framework[]>} The async result.
         */
        public getBusinessProcessModelLibraries(): IAsyncResult<Entities.Framework[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getBusinessProcessModelLibraries();
            return request.execute<Entities.Framework[]>();
        }

        /**
         * Returns the BPM library.
         * @param {number} businessProcessModelFrameworkLineId The BPM framework line ID.
         * @param {number} hierarchyDepth The hierarchy depth.
         * @return {IAsyncResult<Entities.Framework>} The async result.
         */
        public getBusinessProcessModelLibrary(businessProcessModelFrameworkLineId: number, hierarchyDepth: number): IAsyncResult<Entities.Framework> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getBusinessProcessModelLibrary(
                businessProcessModelFrameworkLineId,
                hierarchyDepth);

            return request.execute<Entities.Framework>();
        }

        /**
         * Searches task guide by title.
         * @param {number} businessProcessModelFrameworkLineId The BPM framework line ID.
         * @param {string} taskGuideSearchText The search text.
         * @param {Proxy.Entities.QueryType} queryType The query type.
         * @return {IAsyncResult<Entities.Framework[]>} The async result.
         */
        public searchTaskGuidesByTitle(
            businessProcessModelFrameworkLineId: number,
            taskGuideSearchText: string,
            queryTypeValue: Proxy.Entities.QueryType): IAsyncResult<Entities.TaskGuidesSearchResult> {

            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().searchTaskGuidesByTitle(
                businessProcessModelFrameworkLineId,
                taskGuideSearchText,
                queryTypeValue);

            return request.execute<Entities.Framework[]>();
        }

        /**
         * Downloads recording.
         * @return {IAsyncResult<Entities.Recording>} The async result.
         */
        public downloadRecording(businessProcessModelLineId: number): IAsyncResult<Entities.Recording> {
            var request: Common.IDataServiceRequest = this.getDownloadRecordingRequest(businessProcessModelLineId);
            return request.execute<Entities.Recording>();
        }

        /**
         * Gets the storage access token for uploading binary files.
         * @return {IAsyncResult<Entities.StorageAccessToken>} The async result.
         */
        public getStorageAccessTokenForUpload(): IAsyncResult<Entities.StorageAccessToken> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getStorageAccessTokenForUpload();
            return request.execute<Entities.StorageAccessToken>();
        }

        /**
         * Uploads the recording to LCS.
         * @param {Entities.Recording} recording The recording that will be uploaded.
         * @param {number} businessProcessModelLineId The BPM line ID.
         * @return {IVoidAsyncResult} The async result.
         */
        public uploadRecording(recording: Entities.Recording, businessProcessModelLineId: number): IVoidAsyncResult {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().uploadRecording(recording, businessProcessModelLineId);
            return request.execute();
        }

        /**
         * Deserializes the recording XML.
         * @param {string} recordingUrl The url of XML that will be deserialized.
         * @return {IAsyncResult<Entities.Recording>} The async result.
         */
        public loadRecordingFromFile(recordingUrl: string): IAsyncResult<Entities.Recording> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().loadRecordingFromFile(recordingUrl);
            return request.execute<Entities.Recording>();
        }

        /**
         * Gets downloadRecording request. The similar auto-generated request doesn't parse recording object properly.
         * The request needs to TaskRecorderRecordingClass as return type instead of auto-generated RecordingClass type.
         * @param {number} businessProcessModelLineId The BPM line ID.
         * @return {Common.IDataServiceRequest} The request.
         */
        private getDownloadRecordingRequest(businessProcessModelLineId: number): Common.IDataServiceRequest {
            var oDataOperationParameters: Common.ODataOperationParameters = new Common.ODataOperationParameters();
            oDataOperationParameters.parameters = { businessProcessModelLineId: businessProcessModelLineId };

            return this._commerceContext.storeOperations().createDataServiceRequestForOperation(
                "DownloadRecording",
                true,
                Entities.TaskRecorderRecordingClass,
                "false",
                oDataOperationParameters);
        }
    }
}