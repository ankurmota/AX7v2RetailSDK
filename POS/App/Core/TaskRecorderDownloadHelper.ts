/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.TaskRecorder {

    /**
     * Represents Task Recorder download helper.
     */
    export class TaskRecorderDownloadHelper {

        /**
         * Downloads file to local storage.
         * @param {string} url Source URL.
         * @param {string} file Destination file.
         * @returns {IVoidAsyncResult} Async result.
         */
        public static downloadFile(url: string, file: Windows.Storage.StorageFile): IVoidAsyncResult {
            var result: VoidAsyncResult = new VoidAsyncResult();

            RetailLogger.taskRecorderDownloadFile(url, file.path);

            try {

                // lock the file while the download is not finished
                // for more information, please visit https://msdn.microsoft.com/en-us/library/windows/apps/windows.storage.cachedfilemanager.deferupdates
                Windows.Storage.CachedFileManager.deferUpdates(file);

                var uri: Windows.Foundation.Uri = new Windows.Foundation.Uri(url);
                var downloader: Windows.Networking.BackgroundTransfer.BackgroundDownloader = new Windows.Networking.BackgroundTransfer.BackgroundDownloader();
                var download: Windows.Networking.BackgroundTransfer.DownloadOperation = downloader.createDownload(uri, file);

                download.startAsync().then(() => {
                    Windows.Storage.CachedFileManager.completeUpdatesAsync(file).then(
                        (updateStatus: Windows.Storage.Provider.FileUpdateStatus) => {
                            if (updateStatus === Windows.Storage.Provider.FileUpdateStatus.complete) {
                                RetailLogger.taskRecorderFileWasSaved(file.name);

                                result.resolve();
                            } else {
                                result.reject([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_COULDNT_SAVE_FILE,
                                    false, StringExtensions.EMPTY, null, file.name, updateStatus)]);
                            }
                        },
                        (errors: any) => {
                            result.reject([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_COULDNT_COMPLETE_UPDATES_FOR_FILE,
                                false, StringExtensions.EMPTY, null, file.name, JSON.stringify(errors))]);
                        });
                    },
                    (errors: any) => {
                        result.reject([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_COULDNT_DOWNLOAD_FILE,
                            false, StringExtensions.EMPTY, null, url, JSON.stringify(errors))]);
                    });
            } catch (exception) {
                result.reject([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_COULDNT_DOWNLOAD_FILE,
                    false, StringExtensions.EMPTY, null, url, JSON.stringify(exception))]);
            }

            return result;
        }
    }
}