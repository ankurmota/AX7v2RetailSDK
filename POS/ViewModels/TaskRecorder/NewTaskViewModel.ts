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
      * Represents a New Task view model.
      */
    export class NewTaskViewModel {

        /**
         * An active recording session.
         */
        public session: Observable<TaskRecorderSession> = ko.observable(null);

        /**
         * Name of the new task.
         */
        public taskName: Observable<string> = ko.observable(null);

        /**
         * Comment for the new task.
         */
        public taskComment: Observable<string> = ko.observable(null);

        /**
         * True, if start task button is to be disabled.
         */
        public isStartTaskDisabled: Computed<boolean>;

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
            this.isStartTaskDisabled = ko.computed(() => { return StringExtensions.isNullOrWhitespace(this.taskName()); }, this);
        }

        /**
         * Load the view model.
         * @param {TaskRecorderSession} session An active recording session.
         */
        public load(session: TaskRecorderSession): void {
            ThrowIf.argumentIsNotObject(session, "session");
            this.session(session);
        }

        /**
         * Start the task.
         */
        public startTask(): void {
            this.session().startTask(this.taskName(), this.taskComment());
            this.taskRecorderController.navigate("Recording");
        }

        /**
         * Cancel creation of the task.
         */
        public cancel(): void {
            this.taskRecorderController.navigate("Recording");
        }
    }
}
