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
      * Represents a New Recording view model.
      */
    export class NewRecordingViewModel {

        /**
         * Name of the recording.
         */
        public recordingName: Observable<string> = ko.observable(null);

        /**
         * Description of the recording.
         */
        public recordingDescription: Observable<string> = ko.observable(null);

        /**
         * True, if start recording button is to be disabled.
         */
        public isStartRecordingDisabled: Computed<boolean>;

        private taskRecorderController: ITaskRecorderController;
        private taskRecorderManager: TaskRecorderManager;

        /**
         * Constructor.
         * @param {ITaskRecorderController} taskRecorderController Task Recorder Controller.
         * @param {TaskRecorderManager} taskRecorderManager Task Recorder Manager.
         */
        constructor(taskRecorderController: ITaskRecorderController, taskRecorderManager: TaskRecorderManager) {
            ThrowIf.argumentIsNotObject(taskRecorderController, "taskRecorderController");
            ThrowIf.argumentIsNotObject(taskRecorderManager, "taskRecorderManager");

            this.taskRecorderController = taskRecorderController;
            this.taskRecorderManager = taskRecorderManager;
            this.isStartRecordingDisabled = ko.computed(() => { return StringExtensions.isNullOrWhitespace(this.recordingName()); }, this);
        }

        /**
         * Create the new recording.
         */
        public createRecording(): void {
            this.taskRecorderManager.startNewSession(this.recordingName(), this.recordingDescription()).done(() => {
                this.taskRecorderManager.activeSession().startRecording();
                this.taskRecorderController.navigate("Recording");
            }).fail((errors: Proxy.Entities.Error[]) => {
                NotificationHandler.displayClientErrors(errors);
            });
        }

        /**
         * Cancel creation of the recording.
         */
        public cancel(): void {
            this.taskRecorderController.navigate("Welcome");
        }
    }
}
