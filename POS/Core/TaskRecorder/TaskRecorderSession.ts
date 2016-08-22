/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="../Extensions/StringExtensions.ts" />
/// <reference path="../Utilities/ThrowIf.ts" />
/// <reference path="../Entities/CommerceTypes.g.ts" />
/// <reference path="../Utilities/ErrorHelper.ts" />
/// <reference path="Configuration/ITaskRecorderConfig.ts" />
/// <reference path="ViewModel/TaskRecorderSessionViewModel.ts" />
/// <reference path="TaskRecorderState.ts" />
/// <reference path="ITaskSubscriber.ts" />
/// <reference path="TaskRecorderEventListener.ts" />

module Commerce.TaskRecorder {

    /**
     * Represents an active Task Recorder session with opened recording.
     */
    export class TaskRecorderSession implements TaskRecorder.ITaskSubscriber {

        /**
         * The session id.
         */
        public id: Observable<string>;

        /**
         * The recording name.
         */
        public name: Observable<string>;

        /**
         * The recording description.
         */
        public description: Observable<string>;

        /**
         * The current session state.
         */
        public state: Observable<TaskRecorderState>;

        /**
         * Recording nodes.
         */
        public nodes: ObservableArray<ViewModel.ITaskRecorderNodeViewModel>;

        /**
         * Event listener.
         */
        public eventListener: TaskRecorderEventListener;

        private configuration: Configuration.ITaskRecorderConfig;

        private recordingViewModel: ViewModel.TaskRecorderSessionViewModel;

        /**
         * Constructor.
         * @param {Proxy.Entities.Recording} recording Recording model.
         * @param {Configuration.ITaskRecorderConfig} configuration Task Recorder configuration.
         */
        constructor(recording: Proxy.Entities.Recording, configuration: Configuration.ITaskRecorderConfig) {
            ThrowIf.argumentIsNotObject(recording, "recording");
            ThrowIf.argumentIsNotObject(configuration, "configuration");

            // Initialize observable view model.
            this.recordingViewModel = new ViewModel.TaskRecorderSessionViewModel(recording);

            // generate guid for session id.
            this.id = ko.observable<string>(TaskRecorderUtil.generateGuid());

            // Initialize initial state.
            this.state = ko.observable<TaskRecorderState>(TaskRecorderState.None);

            // Initialize view model accessors.
            this.name = this.recordingViewModel.name;
            this.description = this.recordingViewModel.description;

            this.nodes = this.recordingViewModel.nodes;

            this.configuration = configuration;
        }

        private static error(errorCode: string, ...args: any[]): Proxy.Entities.Error[] {
            return [new Proxy.Entities.Error(errorCode, false, StringExtensions.EMPTY, null, args)];
        }

        /**
         * Returns the underlying recording for the current session.
         * @returns {Proxy.Entities.Recording} The recording.
         */
        public getRecordingAsync(): IAsyncResult<Proxy.Entities.Recording> {
            var asyncResult: AsyncResult<Proxy.Entities.Recording> = new AsyncResult<Proxy.Entities.Recording>();

            TaskRecorderUploadScreenshotsHandler.execute(this.recordingViewModel.nodes()).done(() => {
                asyncResult.resolve(this.recordingViewModel.toModel());
            })
            .fail(() => {
                asyncResult.reject([new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_COULDNT_UPLOAD_SCREENSHOT)]);
            });

            return asyncResult;
        }

        /**
         * Starts a new recording task with given name and description and makes it active for collecting steps.
         * @param {string} name The name of the task.
         * @param {string} comment The comment of the task.
         */
        public startTask(name: string, comment: string): void {
            ThrowIf.argumentIsNotString(name, "name");
            ThrowIf.argumentIsNotStringOrNull(comment, "comment");

            // Creating a new task (and its view model).
            var task: Proxy.Entities.TaskUserAction = Model.RecordingFactory.createNewTask(TaskRecorderUtil.generateGuid(),
                name, comment, null, Model.TaskRecorderTaskType.Begin);

            this.recordingViewModel.startTask(new ViewModel.TaskRecorderTaskViewModel(task));
        }

        /**
         * Ends the active recording task.
         */
        public endTask(): void {
            var state: TaskRecorderState = this.state();

            if (state !== TaskRecorderState.Recording) {
                var stateName: string = TaskRecorderState[state];
                throw TaskRecorderSession.error(ErrorTypeEnum.TASK_RECORDER_SESSION_INVALID_STATE, stateName);
            }

            this.recordingViewModel.endTask();
        }

        /**
         * Starts recording of steps for the sessions.
         */
        public startRecording(): void {
            this.state(TaskRecorderState.Recording);

            this.eventListener = new TaskRecorderEventListener(this, this.configuration);
        }

        /**
         * Pause recording of steps for the session.
         */
        public pauseRecording(): void {
            var state: TaskRecorderState = this.state();

            if (state !== TaskRecorderState.Recording) {
                var stateName: string = TaskRecorderState[state];
                throw TaskRecorderSession.error(ErrorTypeEnum.TASK_RECORDER_SESSION_INVALID_STATE, stateName);
            }

            this.eventListener.dispose();
            this.eventListener = null;

            this.state(TaskRecorderState.RecordingPaused);
        }

        /**
         * Continue recording of steps for the session.
         */
        public continueRecording(): void {
            var state: TaskRecorderState = this.state();

            if (state !== TaskRecorderState.RecordingPaused) {
                var stateName: string = TaskRecorderState[state];
                throw TaskRecorderSession.error(ErrorTypeEnum.TASK_RECORDER_SESSION_INVALID_STATE, stateName);
            }

            this.eventListener = new TaskRecorderEventListener(this, this.configuration);

            this.state(TaskRecorderState.Recording);
        }

        /**
         * Stops recording of steps for the session.
         */
        public stopRecording(): void {
            var state: TaskRecorderState = this.state();

            if (state !== TaskRecorderState.Recording) {
                var stateName: string = TaskRecorderState[state];
                throw TaskRecorderSession.error(ErrorTypeEnum.TASK_RECORDER_SESSION_INVALID_STATE, stateName);
            }

            this.eventListener.dispose();
            this.eventListener = null;

            this.state(TaskRecorderState.RecordingCompleted);
        }

        /**
         * Adds a new step to the active task.
         * @param {Proxy.Entities.UserAction} step The step to add.
         */
        public addStep(step: Proxy.Entities.UserAction): void {
            ThrowIf.argumentIsNotObject(step, "step");

            // Wrapping up the step around view model.
            var stepViewModel: ViewModel.TaskRecorderStepViewModel = new ViewModel.TaskRecorderStepViewModel(step);
            stepViewModel.id = TaskRecorderUtil.generateGuid();

            this.recordingViewModel.addStep(stepViewModel);

            var event: any = document.createEvent("Event");
            event.stepId = stepViewModel.id;
            event.initEvent("screenshot", true, false);
            document.dispatchEvent(event);
        }

        /**
         * Checks if task was created.
         * @returns {boolean} returns true if task created.
         */
        public isTaskCreated(): boolean {
            return !ObjectExtensions.isNullOrUndefined(this.recordingViewModel.activeTask());
        }

        /**
         * Checks if task was created.
         * @param {string} stepId The step id.
         * @param {string} screenshotUri The screenshot URI.
         */
        public updateScreenshotUri(stepId: string, screenshotUri: string): void {
            var step: ViewModel.ITaskRecorderNodeViewModel = ArrayExtensions.firstOrUndefined(this.nodes(), (node: ViewModel.ITaskRecorderNodeViewModel) => {
                return node.id === stepId;
            });

            if (ObjectExtensions.isNullOrUndefined(step)) {
                throw new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_STEP_VIEW_MODEL_NOT_FOUND);
            }

            step.screenshotUri = screenshotUri;
        }
    }
}
