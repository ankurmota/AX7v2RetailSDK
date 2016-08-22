/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.TaskRecorder {
    "use strict";

    import Managers = Commerce.Model.Managers;

    /**
     * Handler for screenshot uploading.
     */
    export class TaskRecorderUploadScreenshotsHandler {

        /**
         * Executes uploading screenshots.
         *
         * @param {ViewModel.ITaskRecorderNodeViewModel[]} nodes Nodes which contain screenshot URIs.
         * @return {IVoidAsyncResult} The async result.
         */
        public static execute(nodes: ViewModel.ITaskRecorderNodeViewModel[]): IVoidAsyncResult {
            var recordingManager: Managers.IRecordingManager = Managers.Factory.getManager<Managers.IRecordingManager>(Managers.IRecordingManagerName);

            var storageAccessToken: Proxy.Entities.StorageAccessToken;

            var asyncQueue: AsyncQueue = new AsyncQueue();

            var screenshotsFolders: Array<string> = [];

            asyncQueue.enqueue(() => {
                return recordingManager.getStorageAccessTokenForUpload().done
                    ((result: Proxy.Entities.StorageAccessToken) => {
                        storageAccessToken = result;
                    })
                    .fail(() => {
                        asyncQueue.cancel();
                    });
            });

            for (var i: number = 0; i < nodes.length; i++) {
                if (!StringExtensions.isNullOrWhitespace(nodes[i].screenshotUri) && !this.isUrl(nodes[i].screenshotUri)) {
                    // add folder to delete
                    var folderPath: string = Microsoft.Dynamics.Commerce.ClientBroker.TaskRecorderFileManager.extractFolderPath(nodes[i].screenshotUri);
                    if (screenshotsFolders.indexOf(folderPath) === -1) {
                        screenshotsFolders.push(folderPath);
                    }

                    ((userAction: ViewModel.ITaskRecorderNodeViewModel) => asyncQueue.enqueue(() => {
                        var result: VoidAsyncResult = new VoidAsyncResult();
                        try {
                            Microsoft.Dynamics.Commerce.ClientBroker.TaskRecorderFileManager.uploadFileToContainer(
                                userAction.screenshotUri,
                                storageAccessToken.Url,
                                storageAccessToken.SasKey)
                                .done((responseMessage: Microsoft.Dynamics.Commerce.ClientBroker.TaskRecorderUploadFileResponseMessage) => {
                                    if (responseMessage.isRequestSuccess) {
                                        Microsoft.Dynamics.Commerce.ClientBroker.TaskRecorderFileManager.deleteFileFromLocalStorage(userAction.screenshotUri);
                                        userAction.screenshotUri = responseMessage.blobUrl;
                                        result.resolve();
                                    } else {
                                        result.reject([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_ERROR_OCCURED_DURING_UPLOADING_FILE,
                                            false, StringExtensions.EMPTY, null, responseMessage.errorMessage)]);
                                        asyncQueue.cancel();
                                    }
                                });
                        } catch (error) {
                            result.reject([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_ERROR_OCCURED_DURING_UPLOADING_FILE,
                                false, StringExtensions.EMPTY, null, JSON.stringify(error))]);
                            asyncQueue.cancel();
                        }

                        return result;
                    }))(nodes[i]);
                }
            }

            return asyncQueue.run().done(() => {
                this.clearLocalStorage(screenshotsFolders);
            }).fail((errors: Proxy.Entities.Error[]) => {
                    Commerce.RetailLogger.taskRecorderScreenshotsUploadingFailed(ErrorHelper.getErrorMessages(errors));

                this.clearLocalStorage(screenshotsFolders);
            });
        }

        private static isUrl(url: string): boolean {
            return (url.lastIndexOf("http://") === 0) || (url.lastIndexOf("https://") === 0);
        }

        private static clearLocalStorage(folders: string[]): void {
            folders.forEach((folder: string) => {
                try {
                    Microsoft.Dynamics.Commerce.ClientBroker.TaskRecorderFileManager.deleteFolderFromLocalStorage(folder);
                } catch (error) {
                    RetailLogger.taskRecorderDeleteFolderFromLocalStorageFailed(folder, JSON.stringify(error));
                }
            });
        }
    }
}