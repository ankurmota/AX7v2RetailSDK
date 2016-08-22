/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="../../Utilities/ThrowIf.ts" />
/// <reference path="TaskRecorderStepViewModel.ts" />

module Commerce.TaskRecorder.ViewModel {

    /**
     * Represents the Task Recorder task view model.
     */
    export class TaskRecorderTaskViewModel implements ITaskRecorderNodeViewModel {

        /**
         * The node identifier.
         */
        public id: string;

        /**
         * The node sequence number.
         */
        public sequence: Observable<number>;

        /**
         * Task name.
         */
        public name: Observable<string>;

        /**
         * Task comment.
         */
        public comment: Observable<string>;

        /**
         * The flag for editable node.
         */
        public editable: boolean;

        /**
         * The node display description.
         */
        public description: Computed<string>;

        /**
         * The node parent.
         */
        public parent: Observable<ITaskRecorderNodeViewModel>;

        /**
         * Task type.
         */
        public taskType: Model.TaskRecorderTaskType;

        /**
         * The node display number
         */
        public displayNumber: Computed<string>;

        /**
         * The node screenshot URI.
         */
        public screenshotUri: string;

        /**
         * The node OData type.
         */
        public oDataClass: string;

        /**
         * Constructor.
         * @param model {Proxy.Entities.TaskUserAction} Data model.
         * @param beginTaskViewModel {ViewModel.TaskRecorderTaskViewModel} Data model.
         */
        constructor(model: Proxy.Entities.TaskUserAction, beginTaskViewModel?: ViewModel.TaskRecorderTaskViewModel) {
            ThrowIf.argumentIsNotObject(model, "model");

            this.id = model.Id;
            this.sequence = ko.observable(model.Sequence);

            this.parent = ko.observable(null);

            this.taskType = model.UserActionTypeValue;

            if (ObjectExtensions.isNullOrUndefined(beginTaskViewModel)) {
                this.name = ko.observable<string>(model.Name);
                this.comment = ko.observable<string>(model.Comment);
            } else {
                this.name = beginTaskViewModel.name;
                this.comment = beginTaskViewModel.comment;
            }

            this.displayNumber = ko.computed(() => {
                if (ObjectExtensions.isNullOrUndefined(this.sequence())) {
                    return StringExtensions.EMPTY;
                }

                if (ObjectExtensions.isNullOrUndefined(this.parent())) {
                    return this.sequence().toString();
                }

                return this.parent().displayNumber() + "." + this.sequence().toString();
            }, this);

            this.description = ko.computed(() => {
                var taskPrefix: string = this.taskType === Model.TaskRecorderTaskType.Begin ?
                    Commerce.ViewModelAdapter.getResourceString("string_10098") : Commerce.ViewModelAdapter.getResourceString("string_10099");

                return taskPrefix + this.name();
            });

            this.editable = (this.taskType === Model.TaskRecorderTaskType.Begin);

            this.oDataClass = Proxy.Entities.TaskRecorderODataType.taskUserAction;
        }

        /**
         * Returns the underlying object.
         * @returns {Proxy.Entities.TaskUserAction} The Task Recorder user action.
         */
        public toModel(): Proxy.Entities.TaskUserAction {
            return Model.RecordingFactory.createNewTask(this.id, this.name(), this.comment(), this.description(), this.taskType);
        }
    }
}
