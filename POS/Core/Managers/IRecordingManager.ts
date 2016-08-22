/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../Entities/Error.ts'/>
///<reference path='../IAsyncResult.ts'/>

module Commerce.Model.Managers {
    "use strict";

    
    export var IRecordingManagerName: string = "IRecordingManager";
    

    export interface IRecordingManager {

        /**
         * Serializes the recording object and generates an XML.
         * @param {Entities.Recording} recording The recording that will be serialized.
         * @return {IAsyncResult<string>} The async result.
         */
        generateRecordingFile(recording: Entities.Recording): IAsyncResult<string>;

        /**
         * Generates the required document for the recording.
         * @param {Entities.Recording} recording The recording that will be transformed to a document.
         * @return {IAsyncResult<string>} The async result.
         */
        generateTrainingDocument(recording: Entities.Recording): IAsyncResult<string>;

        /**
         * Generates the BPM package from the recording object.
         * @param {Entities.Recording} recording The recording that will be transformed to a BPM package.
         * @return {IAsyncResult<string>} The async result.
         */
        generateBusinessProcessModelPackage(recording: Entities.Recording): IAsyncResult<string>;

        /**
         * Returns the list of BPM libraries.
         * @return {IAsyncResult<Entities.Framework[]>} The async result.
         */
        getBusinessProcessModelLibraries(): IAsyncResult<Entities.Framework[]>;

        /**
         * Returns the BPM library.
         * @param {number} businessProcessModelFrameworkLineId The BPM framework line ID.
         * @param {number} hierarchyDepth The hierarchy depth.
         * @return {IAsyncResult<Entities.Framework>} The async result.
         */
        getBusinessProcessModelLibrary(businessProcessModelFrameworkLineId: number, hierarchyDepth: number): IAsyncResult<Entities.Framework>;

        /**
         * Searches task guide by title.
         * @param {number} businessProcessModelFrameworkLineId The BPM framework line ID.
         * @param {string} taskGuideSearchText The search text.
         * @param {number} queryTypeValue The query type.
         * @return {IAsyncResult<Entities.Framework[]>} The async result.
         */
        searchTaskGuidesByTitle(
            businessProcessModelFrameworkLineId: number,
            taskGuideSearchText: string,
            queryTypeValue: number): IAsyncResult<Entities.TaskGuidesSearchResult>;

        /**
         * Downloads recording.
         * @return {IAsyncResult<Entities.Recording>} The async result.
         */
        downloadRecording(businessProcessModelLineId: number): IAsyncResult<Entities.Recording>;

        /**
         * Gets the storage access token for uploading binary files.
         * @return {IAsyncResult<Entities.StorageAccessToken>} The async result.
         */
        getStorageAccessTokenForUpload(): IAsyncResult<Entities.StorageAccessToken>;

        /**
         * Uploads the recording to LCS.
         * @param {Entities.Recording} recording The recording that will be uploaded.
         * @param {number} businessProcessModelLineId The BPM line ID.
         * @return {IVoidAsyncResult} The async result.
         */
        uploadRecording(recording: Entities.Recording, businessProcessModelLineId: number): IVoidAsyncResult;

        /**
         * Deserializes the recording XML.
         * @param {string} recordingUrl The url of XML that will be deserialized.
         * @return {IAsyncResult<Entities.Recording>} The async result.
         */
        loadRecordingFromFile(recordingUrl: string): IAsyncResult<Entities.Recording>;

        /**
         * Generates the recording bundle from the recording object.
         * @param {Entities.Recording} recording The recording that will be transformed to a recording bundle.
         * @return {IAsyncResult<string>} The async result.
         */
        generateRecordingBundle(recording: Entities.Recording): IAsyncResult<string>;
    }
}
