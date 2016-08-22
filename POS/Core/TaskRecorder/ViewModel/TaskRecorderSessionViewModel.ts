/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="../../Utilities/ThrowIf.ts" />
/// <reference path="TaskRecorderTaskViewModel.ts" />

module Commerce.TaskRecorder.ViewModel {

    /**
     * Node types.
     */
    enum NodeType {
        /**
         * Undefined node type.
         */
        none,

        /**
         * Scope node type.
         */
        scope,

        /**
         * Task User Action node type.
         */
        taskUserAction,

        /**
         * User Action node type.
         */
        userAction
    }

    /**
     * Represents the Task Recorder recording view model.
     */
    export class TaskRecorderSessionViewModel {

        /**
         * Recording name.
         */
        public name: Observable<string>;

        /**
         * Recording description.
         */
        public description: Observable<string>;

        /**
         * Recording nodes.
         */
        public nodes: ObservableArray<ViewModel.ITaskRecorderNodeViewModel>;

        /**
         * Active task.
         */
        public activeTask: Observable<ViewModel.TaskRecorderTaskViewModel>;

        private sequence: number;

        private currentNodeIndex: number;
        private currentModelSequence: number;

        /**
         * Constructor.
         * @param {Proxy.Entities.Recording} model Data model.
         */
        constructor(model: Proxy.Entities.Recording) {
            ThrowIf.argumentIsNotObject(model, "model");

            this.name = ko.observable<string>(model.Name);
            this.description = ko.observable<string>(model.Description);

            this.activeTask = ko.observable(null);
            this.nodes = ko.observableArray([]);
            this.resetSequence();

            this.parseRecording(model.RootScope);
        }

        /**
         * Starts a new task.
         * @param {TaskRecorderTaskViewModel} taskViewModel Task view model.
         */
        public startTask(taskViewModel: TaskRecorderTaskViewModel): void {
            taskViewModel.parent(this.activeTask());
            taskViewModel.sequence(this.getNextSequence());

            this.nodes.push(taskViewModel);

            this.activeTask(taskViewModel);

            this.resetSequence();
        }

        /**
         * Ends the current task.
         */
        public endTask(): void {
            if (ObjectExtensions.isNullOrUndefined(this.activeTask())) {
                throw new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_SESSION_NO_ACTIVE_TASK);
            }

            var task: Proxy.Entities.TaskUserAction = Model.RecordingFactory.createNewTask(TaskRecorderUtil.generateGuid(),
                null, null, null, Model.TaskRecorderTaskType.End);

            var taskViewModel: ViewModel.TaskRecorderTaskViewModel = new ViewModel.TaskRecorderTaskViewModel(task, this.activeTask());
            taskViewModel.parent(this.activeTask());
            taskViewModel.sequence(this.getNextSequence());

            this.nodes.push(taskViewModel);

            this.activeTask(<ViewModel.TaskRecorderTaskViewModel>(this.activeTask().parent()));

            this.restoreSequence();
        }

        /**
         * Adds a step.
         * @param {TaskRecorderStepViewModel} stepViewModel Step view model.
         */
        public addStep(stepViewModel: TaskRecorderStepViewModel): void {
            stepViewModel.parent(this.activeTask());
            stepViewModel.sequence(this.getNextSequence());

            this.nodes.push(stepViewModel);

            this.scrollDown();
        }

        /**
         * Returns the underlying object.
         * @returns {Proxy.Entities.Recording} The Task Recorder recording.
         */
        public toModel(): Proxy.Entities.Recording {
            var recording: Proxy.Entities.Recording = Model.RecordingFactory.createNew(this.name(), this.description());
            recording.RootScope.Sequence = 0;

            this.currentNodeIndex = 0;
            this.currentModelSequence = 0;
            this.buildRecording(recording.RootScope);

            return recording;
        }

        /**
         * Builds nested structure from plain nodes.
         * @param {Proxy.Entities.Scope} currentScope Current scope.
         */
        private buildRecording(currentScope: Proxy.Entities.Scope): void {
            while (this.currentNodeIndex < this.nodes().length) {
                var node: ITaskRecorderNodeViewModel = this.nodes()[this.currentNodeIndex];
                this.currentNodeIndex++;

                var dataModel: Proxy.Entities.Node = node.toModel();
                dataModel.Sequence = ++this.currentModelSequence;
                dataModel.ParentScopeId = currentScope.Id;
                dataModel.ParentSequence = currentScope.Sequence;
                currentScope.Children.push(dataModel);

                if (node.oDataClass !== Proxy.Entities.TaskRecorderODataType.commandUserAction) {
                    if ((<TaskRecorderTaskViewModel>node).taskType === Model.TaskRecorderTaskType.Begin) {
                        var scope: Proxy.Entities.Scope = Model.RecordingFactory.createNewScope(TaskRecorderUtil.generateGuid(),
                            null, null, Model.ScopeType.Task);

                        scope.Sequence = ++this.currentModelSequence;
                        scope.ParentScopeId = currentScope.Id;
                        scope.ParentSequence = currentScope.Sequence;
                        currentScope.Children.push(scope);

                        this.buildRecording(scope);
                    } else {
                        return;
                    }
                }
            }
        }

        private parseRecording(currentScope: Proxy.Entities.Scope): void {
            if (!ObjectExtensions.isNullOrUndefined(currentScope)) {
                currentScope.Children.forEach((node: Proxy.Entities.Node) => {
                    if (!ObjectExtensions.isNullOrUndefined(node)) {
                        // ObjectExtensions.isOfType() can't determine class here
                        switch (this.getNodeType(node)) {
                            case NodeType.scope:
                                this.parseRecording(<Proxy.Entities.Scope>node);

                                break;

                            case NodeType.taskUserAction:
                                var taskUserAction: Proxy.Entities.TaskUserAction = <Proxy.Entities.TaskUserAction>node;
                                if (taskUserAction.UserActionTypeValue === Model.TaskRecorderTaskType.Begin) {
                                    this.startTask(new TaskRecorderTaskViewModel(taskUserAction));
                                } else {
                                    this.endTask();
                                }

                                break;

                            case NodeType.userAction:
                                this.addStep(new TaskRecorderStepViewModel(node));

                                break;
                        }
                    }
                });
            }
        }

        private getNodeType(node: Proxy.Entities.Node): NodeType {
            var nodeObj: any = <any>node;

            if (ObjectExtensions.isNumber(nodeObj.ScopeTypeValue)) {
                return NodeType.scope;
            }

            if (ObjectExtensions.isString(nodeObj.TaskId)) {
                return NodeType.taskUserAction;
            }

            if (ObjectExtensions.isString(nodeObj.GlobalId)) {
                return NodeType.userAction;
            }

            return NodeType.none;
        }

        private resetSequence(): void {
            this.sequence = 0;
        }

        private restoreSequence(): void {
            // find max sequence of elements in the current level
            var maxSequence: number = 0;
            var nodes: ViewModel.ITaskRecorderNodeViewModel[] = this.nodes();
            for (var i: number = (nodes.length - 1); i >= 0; i--) {
                var element: ViewModel.ITaskRecorderNodeViewModel = nodes[i];
                if ((element.parent() === this.activeTask()) && (element.sequence() > maxSequence)) {
                    maxSequence = element.sequence();
                }
            }

            this.sequence = maxSequence;
        }

        private getNextSequence(): number {
            return ++this.sequence;
        }

        /**
         * Scrolls the step list to the latest element.
         */
        private scrollDown(): void {
            var listSteps: NodeListOf<Element> = document.getElementsByClassName("listSteps");

            if (listSteps.length === 0) {
                return;
            }

            listSteps[0].scrollTop = listSteps[0].scrollHeight;
        }
    }
}
