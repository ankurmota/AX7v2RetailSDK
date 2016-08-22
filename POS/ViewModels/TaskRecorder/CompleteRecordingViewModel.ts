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
     * Represents a Complete Recording view model.
     */
    export class CompleteRecordingViewModel {

        /**
         * The completed recording session.
         */
        public session: Observable<TaskRecorderSession> = ko.observable(null);
        public _indeterminateWaitVisible: Observable<boolean> = ko.observable(false);
        public recordingName: Observable<string> = ko.observable(null);

        public isLcsAvailable: Observable<boolean> = ko.observable(false);

        private taskRecorderController: ITaskRecorderController;
        private taskRecorderManager: TaskRecorderManager;

        /**
         * CompleteRecordingViewModel constructor.
         * @param {ITaskRecorderController} taskRecorderController Task Recorder Controller.
         * @param {TaskRecorderManager} taskRecorderManager Task Recorder Manager.
         */
        constructor(taskRecorderController: ITaskRecorderController, taskRecorderManager: TaskRecorderManager) {
            ThrowIf.argumentIsNotObject(taskRecorderController, "taskRecorderController");
            ThrowIf.argumentIsNotObject(taskRecorderManager, "taskRecorderManager");

            this.taskRecorderController = taskRecorderController;
            this.taskRecorderManager = taskRecorderManager;

            this.session = this.taskRecorderManager.activeSession;
            this.recordingName(this.session().name());

            this.isLcsAvailable = ko.observable(false);
        }

        /**
         * Save recording session as recording bundle.
         */
        public saveToThisPC(): void {
            this.processSessionSave(this.taskRecorderManager.saveSessionAsRecordingBundle,
                RetailLogger.taskRecorderSaveSessionAsRecordingBundleFailed,
                ErrorTypeEnum.TASK_RECORDER_SAVE_SESSION_AS_RECORDING_BUNDLE_ERROR);
        }

        /**
         * Save recording session.
         */
        public saveRecording(): void {
            this.processSessionSave(this.taskRecorderManager.saveSessionAsXml,
                RetailLogger.taskRecorderSaveBpmPackageFailed,
                ErrorTypeEnum.TASK_RECORDER_XML_EXPORT_ERROR);
        }

        /**
         * Save recording to the Word document.
         */
        public saveWordDocument(): void {
            this.processSessionSave(this.taskRecorderManager.saveSessionAsWordDocument,
                RetailLogger.taskRecorderSaveBpmPackageFailed,
                ErrorTypeEnum.TASK_RECORDER_WORD_EXPORT_ERROR);
        }

        /**
         * Save recording to the BPM-package.
         */
        public saveBpmPackage(): void {
            this.processSessionSave(this.taskRecorderManager.saveBusinessProcessModelPackage,
                RetailLogger.taskRecorderSaveBpmPackageFailed,
                ErrorTypeEnum.TASK_RECORDER_BPM_PACKAGE_EXPORT_ERROR);
        }

        /**
         * Save recording to the lifecycle services.
         */
        public saveToLifecycleServices(): void {
            this.taskRecorderManager.saveToLifecycleServices();
            this.taskRecorderController.deactivateMainPanel();
        }

        /**
         * Close main panel and clear recording session.
         */
        public closeRecording(): void {
            this.session(null);
            this.taskRecorderController.deactivateMainPanel();
        }

        private hasSaveFileErrors(errors: Proxy.Entities.Error[]): boolean {
            if (ErrorHelper.hasError(errors, ErrorTypeEnum.TASK_RECORDER_COULDNT_SAVE_FILE) ||
                ErrorHelper.hasError(errors, ErrorTypeEnum.TASK_RECORDER_COULDNT_COMPLETE_UPDATES_FOR_FILE)) {
                Commerce.NotificationHandler.displayClientErrors([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_SAVE_FILE_ERROR)]);

                return true;
            }

            return false;
        }

        /**
         * Common processor for saving the document
         * @param action taskRecorderManager action to run.
         * @param logger logger for the error.
         * @param errorType type of the error to raise.
         */
        private processSessionSave(action: () => IVoidAsyncResult, logger: (id: string, message: string) => void, errorType: string): void {
            this._indeterminateWaitVisible(true);
            (action.bind(this.taskRecorderManager))()
                .always(() => {
                    this._indeterminateWaitVisible(false);
                })
                .fail((errors: Proxy.Entities.Error[]) => {
                    logger(this.session().id(), ErrorHelper.getErrorMessages(errors));

                    if (!this.hasSaveFileErrors(errors)) {
                        NotificationHandler.displayClientErrors([new Proxy.Entities.Error(errorType,
                            false, StringExtensions.EMPTY, null, this.session().id())], "string_10104");
                    }
                });
        }
    }
}
