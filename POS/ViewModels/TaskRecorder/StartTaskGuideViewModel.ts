/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.TaskRecorder.ViewModel {
    "use strict";

    /**
     * Represents a Start Task Guide view model.
     */
    export class StartTaskGuideViewModel {

        public recording: Observable<TaskRecorderSessionViewModel>;
        public isRecordingReady: Observable<boolean>;

        // observable controlling the spinner
        public indeterminateWaitVisible: Observable<boolean>;

        private taskRecorderController: ITaskRecorderController;
        private taskRecorderManager: TaskRecorderManager;

        /**
         * Constructor
         * @param {ITaskRecorderController} taskRecorderController Task Recorder Controller.
         */
        constructor(taskRecorderController: ITaskRecorderController, taskRecorderManager: TaskRecorderManager, recorderId: number) {
            ThrowIf.argumentIsNotObject(taskRecorderController, "taskRecorderController");
            this.taskRecorderController = taskRecorderController;
            this.taskRecorderManager = taskRecorderManager;

            this.recording = ko.observable(null);
            this.isRecordingReady = ko.observable(false);
            this.indeterminateWaitVisible = ko.observable(false);

            this.load(recorderId);
        }

        /**
         * Save recording to the Word document.
         */
        public saveWordDocument(): void {
            this.taskRecorderManager.saveRecordingAsWordDocument(this.recording().toModel())
                .fail((errors: Commerce.Model.Entities.Error[]) => {
                    Commerce.NotificationHandler.displayClientErrors(errors);
                });
        }

        private load(recorderId: number): void {
            this.indeterminateWaitVisible(true);

            this.taskRecorderManager.downloadRecording(recorderId)
                .done((result: TaskRecorderSessionViewModel) => {
                    this.recording(result);
                    this.isRecordingReady(true);
                    this.indeterminateWaitVisible(false);
                })
                .fail((errors: Commerce.Model.Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);
                    Commerce.NotificationHandler.displayClientErrors(errors);
                });
        }
    }
}
