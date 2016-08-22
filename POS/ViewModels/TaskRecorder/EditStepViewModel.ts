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
     * Represents an Edit Step view model.
     */
    export class EditStepViewModel {

        /**
         * Text of the step.
         */
        public text: Observable<string>;

        /**
         * Notes of the step.
         */
        public notes: Observable<string>;

        private storedTaskRecorderStepViewModel: TaskRecorderStepViewModel;

        private taskRecorderController: ITaskRecorderController;
        private session: Observable<TaskRecorderSession> = ko.observable(null);

        /**
         * Constructor.
         * @param {ITaskRecorderController} taskRecorderController Task Recorder Controller.
         * @param {TaskRecorderManager} taskRecorderManager Task Recorder Manager.
         * @param {string} stepId ID of editable step.
         */
        constructor(taskRecorderController: ITaskRecorderController, taskRecorderManager: TaskRecorderManager, stepId: string) {
            ThrowIf.argumentIsNotObject(taskRecorderController, "taskRecorderController");
            ThrowIf.argumentIsNotObject(taskRecorderManager, "taskRecorderManager");
            ThrowIf.argumentIsNotString(stepId, "stepId");

            this.taskRecorderController = taskRecorderController;
            this.session = taskRecorderManager.activeSession;

            var node: ITaskRecorderNodeViewModel = ArrayExtensions.firstOrUndefined(this.session().nodes(),
                (node: ITaskRecorderNodeViewModel) => {
                    return node.id === stepId;
                });

            this.storedTaskRecorderStepViewModel = <TaskRecorderStepViewModel>node;

            if (ObjectExtensions.isNullOrUndefined(this.storedTaskRecorderStepViewModel)) {
                throw new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_STEP_VIEW_MODEL_NOT_FOUND);
            }

            this.text = ko.observable(this.storedTaskRecorderStepViewModel.text());
            this.notes = ko.observable(this.storedTaskRecorderStepViewModel.notes());
        }

        /**
         * Save changes.
         */
        public saveStep(): void {
            this.storedTaskRecorderStepViewModel.text(this.text());
            this.storedTaskRecorderStepViewModel.notes(this.notes());
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
