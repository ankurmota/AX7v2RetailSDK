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
     * Represents a Help view model.
     */
    export class HelpViewModel {
        public listTaskGuides: ObservableArray<Model.TaskGuideModel>;
        public searchText: Observable<string>;
        public searchIsInProgress: Observable<boolean>;

        private taskRecorderController: ITaskRecorderController;

        /**
         * Constructor.
         * @param {ITaskRecorderController} taskRecorderController Task Recorder Controller.
         */
        constructor(taskRecorderController: ITaskRecorderController) {
            ThrowIf.argumentIsNotObject(taskRecorderController, "taskRecorderController");
            this.taskRecorderController = taskRecorderController;

            this.searchText = ko.observable(StringExtensions.EMPTY);
            this.searchIsInProgress = ko.observable(false);

            this.listTaskGuides = ko.observableArray([]);
        }

        /**
         * Search task guides by title method.
         */
        public search(): void {
            if (this.searchText().length > 0) {
                this.listTaskGuides([]);
                this.searchIsInProgress(true);
                var manager: TaskRecorderManager = new TaskRecorderManager();
                manager.searchTaskGuidesByTitle(this.searchText())
                    .done((result: Model.TaskGuideModel[]) => {
                        this.listTaskGuides(result);
                        this.searchIsInProgress(false);
                    })
                    .fail((errors: Commerce.Model.Entities.Error[]) => {
                        Commerce.NotificationHandler.displayClientErrors(errors);
                        this.searchIsInProgress(false);
                    });
            }
        }

        /**
         * Open Start Task Guide page.
         * @param {Model.TaskGuideModel} model Task Guide model.
         */
        public openStartTaskGuide(model: Model.TaskGuideModel): void {
            this.taskRecorderController.navigate("StartTaskGuide", model.id);
        }
    }
}
