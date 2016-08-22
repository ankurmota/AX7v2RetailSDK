/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/**
 * AsyncClient client broker definitions.
 */
declare module Microsoft.Dynamics.Commerce.ClientBroker {
    class AsyncClientRequest {
        static applyFileToOfflineDbAsync(offlineConnectionString: string, workingDirectoryPath: string, fileName: string, terminalId: string,
                  sqlCommandTimeout: number): any;
        static deleteExpiredSessionsAsync(offlineConnectionString: string): any;
        static downloadFileAsync(downloadFileUri: string, expectedCheckSum: string): any;
        static loadTransactionDataAsync(offlineDbConnectionString: string, uploadJobDefinitions: string[], terminalId: string): any;
        static purgeOfflineTransactionsAsync(offlineConnectionString: string, uploadJobDefinitions: string[]): any;
        static updateDownloadSessionStatusAsync(offlineConnectionString: string, downloadSessions: AsyncClientDownloadSession): any;
        static updateUploadFailedStatusAsync(offlineConnectionString: string): any;
        static getOfflineSyncStatsAsync(offlineConnectionString: string): any;
    }

    class AsyncClientResponseMessage {
        public requestSuccess: boolean;
        public statusText: string;
    }

    class ApplySessionFileResponseMessage {
        public requestSuccess: boolean;
        public rowsAffected: number;
        public statusText: string;
    }

    class CheckInitialDataSyncResponseMessage {
        public requireInitialSync: boolean;
        public requestSuccess: boolean;
        public statusText: string;
    }

    class DownloadFileResponseMessage {
        public fileName: string;
        public requestSuccess: boolean;
        public statusText: string;
        public workingFolder: string;
    }

    class GetOfflineSyncStatsResponseMessage {
        public offlineSyncStats: AsyncClientOfflineSyncStatsLine[];
        public requestSuccess: boolean;
        public statusText: string;
    }

    class LoadUploadTransactionResponseMessage {
        public offlineTransactionDataList: string[];
        public requestSuccess: boolean;
        public statusText: string;
    }

    class AsyncClientDownloadSession {
        public dateRequested: string;
        public dateDownloaded: string;
        public id: number;
        public jobId: string;
        public jobDescription: string;
        public fileSize: number;
        public checksum: string;
        public status: number;
        public message: string;
    }

    class AsyncClientOfflineSyncStatsLine {
        public jobDescription: string;
        public status: string;
        public lastSyncDateTime: Date;
        public fileSize: string;
        public isUploadJob: number;
    }

    class ScreenCapture {
        static takeScreenshotAsync(sessionId: string, stepId: string): any;
    }

    class TaskRecorderFileManager {
        static uploadFileToContainer(filePath: string, containerUrl: string, sasKey: string): any;
        static deleteFileFromLocalStorage(filePath: string): any;
        static deleteFolderFromLocalStorage(folderPath: string): any;
        static extractFolderPath(filePath: string): any;
    }

    class TaskRecorderUploadFileResponseMessage {
        public isRequestSuccess: boolean;
        public errorMessage: string;
        public blobUrl: string;
    }
}