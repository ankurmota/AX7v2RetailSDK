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
      * Represents a Welcome view model.
      */
    export class WelcomeViewModel {

        private taskRecorderController: ITaskRecorderController;

        /**
         * Constructor.
         * @param {ITaskRecorderController} taskRecorderController Task Recorder Controller.
         */
        constructor(taskRecorderController: ITaskRecorderController) {
            ThrowIf.argumentIsNotObject(taskRecorderController, "taskRecorderController");
            this.taskRecorderController = taskRecorderController;
        }

        /**
         * Load the view model.
         */
        public load(): void {
            // Left empty on purpose
        }

        /**
         * Create the recording.
         */
        public createNewRecording(): void {
            this.taskRecorderController.navigate("NewRecording");
        }
    }
}
