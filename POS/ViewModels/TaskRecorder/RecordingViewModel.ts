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
     * Represents a Recording view model.
     */
    export class RecordingViewModel {

        /**
         * Task recorder session.
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

            this.session = this.taskRecorderManager.activeSession;
        }

        /**
         * Checks if state equals to TaskRecorderState.Recording.
         * @returns {boolean} returns true if state is TaskRecorderState.Recording.
         */
        public isInRecordingState(): boolean {
            return this.session().state() === TaskRecorderState.Recording;
        }

        /**
         * Checks if task was created.
         * @returns {boolean} returns true if task created.
         */
        public isTaskCreated(): boolean {
            return this.session().isTaskCreated();
        }

        /**
         * Continue recording in current session.
         */
        public continueRecording(): void {
            this.session().continueRecording();
            Commerce.RetailLogger.taskRecorderContinueRecording(this.session().id(), this.session().name());
        }

        /**
         * Pause recording in current session.
         */
        public pauseRecording(): void {
            this.session().pauseRecording();
            Commerce.RetailLogger.taskRecorderPauseRecording(this.session().id(), this.session().name());
        }

        /**
         * Stop recording in current session.
         */
        public stopRecording(): void {
            this.taskRecorderManager.disableScreenCapture();
            this.session().stopRecording();
            Commerce.RetailLogger.taskRecorderStopRecording(this.session().id(), this.session().name());
            this.taskRecorderController.navigate("CompleteRecording");
        }

        /**
         * Toggle event for screen capturing
         * @param {any} eventInfo The event info.
         * @param {boolean} captureScreen The status of the screen capture.
         */
        public toggleScreenCapture(eventInfo: any, captureScreen: boolean): void {
            if (captureScreen) {
                this.taskRecorderManager.enableScreenCapture();
            } else {
                this.taskRecorderManager.disableScreenCapture();
            }
        }

        /**
         * Start task in recording for current session.
         */
        public startTask(): void {
            this.taskRecorderController.navigate("NewTask", this.session());
        }

        /**
         * End task in recording for current session.
         */
        public endTask(): void {
            this.session().endTask();
            Commerce.RetailLogger.taskRecorderEndTask(this.session().id(), this.session().name());
        }

        /**
         * Edit node in recording for current session.
         */
        public editNode(nodeViewModel: ITaskRecorderNodeViewModel): void {
            var viewName: string;
            switch (nodeViewModel.oDataClass) {
                case Proxy.Entities.TaskRecorderODataType.commandUserAction:
                    viewName = "EditStep";
                    break;
                case Proxy.Entities.TaskRecorderODataType.taskUserAction:
                    viewName = "EditTask";
                    break;
                default:
                    throw new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_ODATA_TYPE_NOT_FOUND);
            }

            this.taskRecorderController.navigate(viewName, nodeViewModel.id);
        }

        /**
         * Check screenshot feature.
         */
        public screenshotsEnabled(): boolean {
            try {
                if (ObjectExtensions.isFunction(Microsoft.Dynamics.Commerce.ClientBroker.ScreenCapture.takeScreenshotAsync)) {
                    return true;
                }
            } catch (error) {
                return false;
            }

            return false;
        }
    }
}