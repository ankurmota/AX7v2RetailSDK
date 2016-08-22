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
     * Interface for file type choice.
     */
    export interface ITaskRecorderFileTypeChoice {

        /**
         * Display name.
         */
        name: string;

        /**
         * File extensions.
         */
        fileExtensions: string[];
    }

    /**
     * Represents Task Recorder dialog helper.
     */
    export class TaskRecorderDialogHelper {

        /**
         * Possible file type choices.
         * @remark Keep all extensions in lowercase.
         */
        private static fileTypeChoices: ITaskRecorderFileTypeChoice[] = [
            {
                name: "string_10102",
                fileExtensions: [".xml"]
            },
            {
                name: "string_10103",
                fileExtensions: [".doc", ".docx"]
            },
            {
                name: "string_10105",
                fileExtensions: [".ax7bpm"]
            },
            {
                name: "string_10121",
                fileExtensions: [".axtr"]
            }
        ];

        /**
         * Shows "Save as..." dialog.
         * @param {string} suggestedFileName Default file name.
         * @returns {IAsyncResult<Windows.Storage.StorageFile>} Async result.
         */
        public static showSaveDialog(suggestedFileName: string): IAsyncResult<Windows.Storage.StorageFile> {
            var result: AsyncResult<Windows.Storage.StorageFile> = new AsyncResult<Windows.Storage.StorageFile>();

            var fileExtension: string = TaskRecorderDialogHelper.extractFileExtension(suggestedFileName);
            var fileTypeChoice: ITaskRecorderFileTypeChoice = TaskRecorderDialogHelper.getFileTypeChoice(fileExtension);

            if (ObjectExtensions.isNullOrUndefined(fileTypeChoice)) {
                return AsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_UNEXPECTED_FILE_EXTENSION,
                    false, StringExtensions.EMPTY, null, fileExtension)]);
            }

            RetailLogger.taskRecorderShowSaveDialog(suggestedFileName, fileTypeChoice.name);

            var savePicker: Windows.Storage.Pickers.FileSavePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.suggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.documentsLibrary;

            savePicker.fileTypeChoices.insert(Commerce.ViewModelAdapter.getResourceString(fileTypeChoice.name), <any>fileTypeChoice.fileExtensions);

            savePicker.suggestedFileName = suggestedFileName;
            savePicker.pickSaveFileAsync().then((file: Windows.Storage.StorageFile) => {
                result.resolve(file);
            }, (error: any) => {
                result.reject([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_ERROR_OCCURRED_DURING_DISPLAYING_SAVE_DIALOG,
                    false, StringExtensions.EMPTY, null, JSON.stringify(error))]);
            });

            return result;
        }

        /**
         * Searches file type choice by file extension.
         * @param {string} fileExtension File extension.
         * @returns {ITaskRecorderFileTypeChoice} Found file type choice or null.
         */
        private static getFileTypeChoice(fileExtension: string): ITaskRecorderFileTypeChoice {
            fileExtension = fileExtension.toLowerCase();

            for (var i: number = 0; i < this.fileTypeChoices.length; i++) {
                if (this.fileTypeChoices[i].fileExtensions.indexOf(fileExtension) !== -1) {
                    return this.fileTypeChoices[i];
                }
            }

            return null;
        }

        /**
         * Extracts file extension from file name.
         * @param {string} fileName Processing file name.
         * @returns {string} Extracted file extension.
         */
        private static extractFileExtension(fileName: string): string {
            var lastDotPos: number = fileName.lastIndexOf(".");
            if (lastDotPos === -1) {
                return StringExtensions.EMPTY;
            }

            return fileName.slice(lastDotPos);
        }
    }
}