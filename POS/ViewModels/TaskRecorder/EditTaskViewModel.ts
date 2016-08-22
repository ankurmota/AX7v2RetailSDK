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
     * Represents a Edit Task view model.
     */
    export class EditTaskViewModel {

        /**
         * Name of the new task.
         */
        public name: Observable<string> = ko.observable(null);

        /**
         * Comment of the new task.
         */
        public comment: Observable<string> = ko.observable(null);

        private taskRecorderController: ITaskRecorderController;

        private session: Observable<TaskRecorderSession> = ko.observable(null);

        private storedTaskRecorderTaskViewModel: TaskRecorderTaskViewModel;

        /**
         * Constructor.
         * @param {ITaskRecorderController} taskRecorderController Task Recorder Controller.
         * @param {TaskRecorderManager} taskRecorderManager Task Recorder Manager.
         * @param {string} taskId ID of editable task.
         */
        constructor(taskRecorderController: ITaskRecorderController, taskRecorderManager: TaskRecorderManager, taskId: string) {
            ThrowIf.argumentIsNotObject(taskRecorderController, "taskRecorderController");
            ThrowIf.argumentIsNotObject(taskRecorderManager, "taskRecorderManager");
            ThrowIf.argumentIsNotString(taskId, "taskId");

            this.taskRecorderController = taskRecorderController;
            this.session = taskRecorderManager.activeSession;

            var node: ITaskRecorderNodeViewModel = ArrayExtensions.firstOrUndefined(this.session().nodes(),
                (node: ITaskRecorderNodeViewModel) => {
                    return node.id === taskId;
                });

            this.storedTaskRecorderTaskViewModel = <TaskRecorderTaskViewModel>node;

            if (ObjectExtensions.isNullOrUndefined(this.storedTaskRecorderTaskViewModel)) {
                throw new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_TASK_VIEW_MODEL_NOT_FOUND);
            }

            this.name = ko.observable(this.storedTaskRecorderTaskViewModel.name());
            this.comment = ko.observable(this.storedTaskRecorderTaskViewModel.comment());
        }

        /**
         * Save changes.
         */
        public saveTask(): void {
            this.storedTaskRecorderTaskViewModel.name(this.name());
            this.storedTaskRecorderTaskViewModel.comment(this.comment());
            this.taskRecorderController.navigate("Recording");
        }

        /**
         * Cancel changes.
         */
        public cancel(): void {
            this.taskRecorderController.navigate("Recording");
        }
    }
}
