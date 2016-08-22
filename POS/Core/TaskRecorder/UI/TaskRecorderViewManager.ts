/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="../../Extensions/StringExtensions.ts" />
/// <reference path="../../Utilities/ThrowIf.ts" />
/// <reference path="../../Entities/CommerceTypes.g.ts" />
/// <reference path="../../Utilities/ErrorHelper.ts" />
/// <reference path="../TaskRecorderManager.ts" />
/// <reference path="../ITaskRecorderController.ts" />
/// <reference path="ViewDefinition.ts" />

module Commerce.TaskRecorder {

    /**
     * Manager for views of Task Recorder.
     */
    export class TaskRecorderViewManager {

        private taskRecorderController: ITaskRecorderController;
        private taskRecorderManager: TaskRecorderManager;

        private baseViewUri: string;
        private registeredViews: IViewDefinitionMap = {};

        /**
         * Constructor.
         * @param {ITaskRecorderController} taskRecorderController Task Recorder Controller.
         * @param {TaskRecorderManager} taskRecorderManager Task Recorder Manager.
         * @param {string} baseViewUri URI of base view.
         */
        constructor(taskRecorderController: ITaskRecorderController, taskRecorderManager: TaskRecorderManager, baseViewUri: string) {
            ThrowIf.argumentIsNotObject(taskRecorderController, "taskRecorderController");
            ThrowIf.argumentIsNotObject(taskRecorderManager, "taskRecorderManager");
            ThrowIf.argumentIsNotString(baseViewUri, "baseViewUri");

            this.taskRecorderController = taskRecorderController;
            this.taskRecorderManager = taskRecorderManager;
            this.baseViewUri = baseViewUri;
        }

        private static error(errorCode: string, ...args: any[]): Proxy.Entities.Error[] {
            return [new Proxy.Entities.Error(errorCode, false, StringExtensions.EMPTY, null, args)];
        }

        /**
         * Register view.
         * @param {string} viewName View name.
         * @param {IViewModelConstructor} viewModelType Type of registering view model.
         */
        public registerView(viewName: string, viewModelType: IViewModelConstructor): void {
            ThrowIf.argumentIsNotString(viewName, "viewName");
            ThrowIf.argumentIsNotFunction(viewModelType, "viewModelType");

            var viewDefinition: IViewDefinition = {
                viewUri: this.getViewUri(viewName),
                viewModelType: viewModelType
            };

            this.registeredViews[viewName] = viewDefinition;
        }

        /**
         * Load view to page.
         * @param {string} viewName Name of view.
         * @param {any} [options] Options.
         * @param {HTMLElement} parent Parent HTML element.
         * @returns {IAsyncResult<IViewViewModel>} The view model async result.
         */
        public loadView(viewName: string, options?: any, parent?: HTMLElement): IAsyncResult<IViewViewModel> {
            ThrowIf.argumentIsNotString(viewName, "viewName");

            // Retrieve view definition.
            var viewDefinition: IViewDefinition = this.registeredViews[viewName];
            if (!viewDefinition) {
                return AsyncResult.createRejected(TaskRecorderViewManager.error(ErrorTypeEnum.TASK_RECORDER_VIEWMANAGER_VIEW_NOT_FOUND, viewName));
            }

            // Loading view.
            var that: TaskRecorderViewManager = this;
            var asyncResult: AsyncResult<IViewViewModel> = new AsyncResult<IViewViewModel>();
            var element: HTMLDivElement = document.createElement("div");

            WinJS.UI.Pages.render(viewDefinition.viewUri, element)
                .then((control: any) => {
                    try {

                        // Initializing view model.
                        var viewModelType: IViewModelConstructor = viewDefinition.viewModelType;
                        var viewModel: IViewModel = new viewModelType(that.taskRecorderController, that.taskRecorderManager, options);

                        // Attaching the element to parent if provided.
                        if (parent) {
                           parent.appendChild(element);
                        }

                        // Invoking the model's load() if defined.
                        if (viewModel.load) {
                            viewModel.load(options);
                        }

                        // Return element and view model.
                        asyncResult.resolve({
                            element: element,
                            viewModel: viewModel
                        });

                    } catch (e) {
                        asyncResult.reject(TaskRecorderViewManager.error(ErrorTypeEnum.TASK_RECORDER_VIEWMANAGER_LOAD_FAILED, viewName));
                    }
                });

            return asyncResult;
        }

        /**
         * Unload view from page.
         * @param {HTMLElement} parent Parent HTML element.
         * @param {IViewViewModel} vvm The unloading ViewViewModel
         * @param {boolean} [dispose] Whether or not to clean ViewViewModel.
         */
        public unloadView(parent: HTMLElement, vvm: IViewViewModel, dispose: boolean = true): void {
            parent.removeChild(vvm.element);
            if (dispose) {
                this.disposeView(vvm);
            }
        }

        /**
         * Clean up.
         * @param {IViewViewModel} vvm The disposing ViewViewModel .
         */
        public disposeView(vvm: IViewViewModel): void {
            if (vvm.viewModel && vvm.viewModel.dispose) {
                vvm.viewModel.dispose();
            }
        }

        private getViewUri(viewName: string): string {
            return this.baseViewUri + "/TaskRecorder/" + viewName + "View.html";
        }
    }
}