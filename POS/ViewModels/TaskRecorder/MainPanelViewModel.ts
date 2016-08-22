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

    const ELEMENT_ID_TASKRECORDER_MAINPANEL_PAGEHOST: string = "taskRecorderMainPanelPageHost";

    /**
     * Represents a Main Panel view model.
     */
    export class MainPanelViewModel {

        private taskRecorderController: ITaskRecorderController;
        private taskRecorderManager: TaskRecorderManager;
        private viewManager: TaskRecorderViewManager;

        private pageHostElement: HTMLElement = null;
        private currentPage: IViewViewModel;

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

        private static errorInvalidDom(): Proxy.Entities.Error[] {
            return [new Proxy.Entities.Error(ErrorTypeEnum.TASK_RECORDER_INVALID_DOM)];
        }

        /**
         * Load the view model.
         * @param {TaskRecorderViewManager} viewManager Task Recorder View Manager.
         */
        public load(viewManager: TaskRecorderViewManager): void {
            ThrowIf.argumentIsNotObject(viewManager, "viewManager");

            this.viewManager = viewManager;
            this.pageHostElement = this.getHostElement();
        }

        /**
         * Navigates to the given page in main panel.
         * @param {pageName} pageName The name of a page to navigate.
         * @param {any} options The optional state to be passed to the navigated page.
         */
        public navigate(pageName: string, options: any): void {
            ThrowIf.argumentIsNotString(pageName, "pageName");

            if (this.currentPage) {
                this.viewManager.unloadView(this.pageHostElement, this.currentPage, false);
            }

            // Loading the new page.
            this.viewManager.loadView(pageName, options).done((vvm: IViewViewModel) => {

                // Detaching (but not disposing) the current page if present.
                this.currentPage = vvm;

                // Attaching the page.
                this.pageHostElement.appendChild(vvm.element);

                // Applying bindings.
                ko.applyBindings(vvm.viewModel, vvm.element);
            });
        }

        /**
         * Hides the main panel.
         */
        public hide(): void {
            this.taskRecorderController.hideMainPanel();
        }

        private getHostElement(): HTMLElement {
            var hostElement: HTMLElement = document.getElementById(ELEMENT_ID_TASKRECORDER_MAINPANEL_PAGEHOST);

            if (!hostElement) {
                throw MainPanelViewModel.errorInvalidDom();
            }

            return hostElement;
        }
    }
}
