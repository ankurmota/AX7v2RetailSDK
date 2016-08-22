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
     * Represents a Control Panel view model.
     */
    export class ControlPanelViewModel {

        /**
         * The active recording session.
         */
        public session: Observable<TaskRecorderSession> = ko.observable(null);

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
        }

        /**
         * Checks if state equals to TaskRecorderState.Recording.
         * @returns {boolean} returns true if state is TaskRecorderState.Recording.
         */
        public isInRecordingState(): boolean {
            if (ObjectExtensions.isNullOrUndefined(this.session())) {
                return false;
            }

            return this.session().state() === TaskRecorderState.Recording;
        }

        /**
         * Checks if state equals to TaskRecorderState.RecordingCompleted.
         * @returns {boolean} returns true if state is TaskRecorderState.RecordingCompleted.
         */
        public isInRecordingCompletedState(): boolean {
            if (ObjectExtensions.isNullOrUndefined(this.session())) {
                return false;
            }

            return this.session().state() === TaskRecorderState.RecordingCompleted;
        }

        /**
         * Load the view model.
         * @param {TaskRecorderSession} session The recording session.
         */
        public load(session: TaskRecorderSession): void {
            ThrowIf.argumentIsNotObject(session, "session");

            this.session(session);
        }

        /**
         * Continue the recording.
         */
        public continueRecording(): void {
            this.session().continueRecording();
            Commerce.RetailLogger.taskRecorderContinueRecording(this.session().id(), this.session().name());
        }

        /**
         * Pause the recording.
         */
        public pauseRecording(): void {
            this.session().pauseRecording();
            Commerce.RetailLogger.taskRecorderPauseRecording(this.session().id(), this.session().name());
        }

        /**
         * Stop the recording.
         */
        public stopRecording(): void {
            this.taskRecorderManager.disableScreenCapture();
            this.session().stopRecording();
            Commerce.RetailLogger.taskRecorderStopRecording(this.session().id(), this.session().name());
            this.taskRecorderController.showMainPanel();
            this.taskRecorderController.navigate("CompleteRecording");
        }

        /**
         * Show the main panel.
         */
        public toggleMainPanel(): void {
            this.taskRecorderController.toggleMainPanel();
        }

        /**
         * Clean up.
         */
        public dispose(): void {
            this.session(null);
        }
    }
}
