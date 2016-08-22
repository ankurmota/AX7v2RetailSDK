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
     * Represents Task Recorder Downloader.
     */
    export class TaskRecorderDownloader {
        /**
         * Downloads file to the local storage.
         * @param {string} url Source URL.
         * @returns {IAsyncResult<boolean>} Async result.
         */
        public static downloadFile(url: string): IAsyncResult<boolean> {
            throw new Error(Commerce.ViewModelAdapter.getResourceString(Commerce.ErrorTypeEnum.NOT_IMPLEMENTED));
        }
    }
}