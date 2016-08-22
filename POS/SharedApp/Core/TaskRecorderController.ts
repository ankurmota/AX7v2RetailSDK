/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="..\Pos.config.ts" />

module Commerce.TaskRecorder {

    const ELEMENT_ID_TASKRECORDER_HOST: string = "taskRecorderHost";

    export class TaskRecorderController implements ITaskRecorderController {

        /**
         * The Task Recorder Manager
         */
        public manager: TaskRecorderManager = null;

        /**
         * The Task Recorder View Manager
         */
        public viewManager: TaskRecorderViewManager = null;

        private hostElement: HTMLElement = null;

        private mainPanel: IViewViewModel = null;
        private controlPanel: IViewViewModel = null;

        /**
         * Constructor.
         */
        constructor() {
            this.manager = new TaskRecorderManager();
            this.viewManager = new TaskRecorderViewManager(this, this.manager, Config.viewRoot);

            ko.computed(() => {
                var session: TaskRecorderSession = this.manager.activeSession();
                return session != null && session.state() !== TaskRecorderState.None ? session : null;
            }).subscribe(this.toggleControlPanel.bind(this));

            this.initializeHtmlHost();
            this.initializePanelDefinitions();
            this.initializePageDefinitions();
        }

        private static errorInvalidDom(): Proxy.Entities.Error[] {
            return [new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_INVALID_DOM)];
        }

        /**
         * Showes the Task Recorder main panel with page related to actual state.
         */
        public showMainPanel(): void {
            if (this.mainPanel) {
                $(this.mainPanel.element).show();
            }
        }

        /**
         * Hides the Task Recorder main panel with page related to actual state.
         */
        public hideMainPanel(): void {
            $(this.mainPanel.element).hide();

            var session: TaskRecorderSession = this.manager.activeSession();

            if (!session || session.state() === TaskRecorderState.RecordingCompleted) {
                this.deactivateMainPanel();
            }
        }

        /**
         * Toggles the Task Recorder main panel with page related to actual state.
         */
        public toggleMainPanel(): void {
            $(this.mainPanel.element).toggle();
        }

        /**
         * Displays the Task Recorder main panel with page related to actual state.
         * @param {string} [viewName] The specific view.
         */
        public activateMainPanel(viewName?: string): void {
            var session: TaskRecorderSession = this.manager.activeSession();

            if (session) {
                var state: TaskRecorderState = session.state();

                switch (state) {
                    case TaskRecorderState.Recording:
                    case TaskRecorderState.RecordingPaused:
                        this.navigate("Recording");
                        break;

                    case TaskRecorderState.RecordingCompleted:
                        this.navigate("CompleteRecording");
                        break;

                    default:
                        // The current state of Task Recorder session is not expected.
                        throw new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_CONTROLLER_NOT_SUPPORTED_STATE);
                }
            } else {
                // No active session, opening welcome screen.
                if (!StringExtensions.isNullOrWhitespace(viewName)) {
                    this.navigate(viewName);
                } else {
                    this.navigate("Welcome");
                }
            }
        }

        /**
         * Deactivates the Task Recorder main panel.
         */
        public deactivateMainPanel(): void {
            if (this.mainPanel) {
                this.viewManager.unloadView(this.hostElement, this.mainPanel);
                this.mainPanel = null;
            }
        }

        /**
         * Navigates to the given page in main panel with preservation of existing page to navigate back.
         * @param {pageName} pageName The name of a page to navigate.
         * @param {any} options The optional state to be passed to the navigated page.
         */
        public navigate(pageName: string, options?: any): void {
            ThrowIf.argumentIsNotString(pageName, "pageName");

            this.openMainPanel().done((mainPanel: IViewViewModel) => {
                (<ViewModel.MainPanelViewModel>mainPanel.viewModel).navigate(pageName, options);
            }).fail((errors: Proxy.Entities.Error[]) => {
                NotificationHandler.displayClientErrors(errors);
            });
        }

        private initializeHtmlHost(): void {
            var hostElement: HTMLElement = document.getElementById(ELEMENT_ID_TASKRECORDER_HOST);

            if (!hostElement) {
                throw TaskRecorderController.errorInvalidDom();
            }

            this.hostElement = hostElement;
        }

        private openMainPanel(): IAsyncResult<IViewViewModel> {
            if (this.mainPanel) {
                return AsyncResult.createResolved<IViewViewModel>(this.mainPanel);
            }

            return this.viewManager.loadView("MainPanel", this.viewManager, this.hostElement).done((vvm: IViewViewModel) => {
                // Applying bindings.
                ko.applyBindings(vvm.viewModel, vvm.element);

                this.mainPanel = vvm;
                return vvm;
            });
        }

        private toggleControlPanel(session: TaskRecorderSession): void {
            if (session) {
                this.openControlPanel(session);
            } else {
                this.closeControlPanel();
            }
        }

        private openControlPanel(session: TaskRecorderSession): IAsyncResult<IViewViewModel> {
            if (this.controlPanel) {
                return AsyncResult.createResolved<IViewViewModel>(this.controlPanel);
            }

            return this.viewManager.loadView("ControlPanel", session, this.hostElement).done((vvm: IViewViewModel) => {
                // Applying bindings.
                ko.applyBindings(vvm.viewModel, vvm.element);

                this.controlPanel = vvm;
                return vvm;
            });
        }

        private closeControlPanel(): void {
            if (this.controlPanel) {
                this.viewManager.unloadView(this.hostElement, this.controlPanel);
                this.controlPanel = null;
            }
        }

        private initializePanelDefinitions(): void {
            // Registering all Task Recorder panels from configuration.
            Object.getOwnPropertyNames(Config.taskRecorderPanels).forEach((viewName: string) => {
                var viewDefinition: IViewDefinition = Config.taskRecorderPanels[viewName];
                this.viewManager.registerView(viewName, viewDefinition.viewModelType);
            });
        }

        private initializePageDefinitions(): void {
            // Registering all Task Recorder page views from configuration.
            Object.getOwnPropertyNames(Config.taskRecorderPages).forEach((viewName: string) => {
                var viewDefinition: IViewDefinition = Config.taskRecorderPages[viewName];
                this.viewManager.registerView(viewName, viewDefinition.viewModelType);
            });
        }
    }
}
