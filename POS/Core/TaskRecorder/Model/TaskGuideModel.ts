/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.TaskRecorder.Model {

    /**
     * Represents the Task Guide view model.
     */
    export class TaskGuideModel {

        /**
         * The Task Guide identifier.
         */
        public id: number;

        /**
         * The Task Guide title.
         */
        public title: string;

        /**
         * The Task Guide publisher.
         */
        public publisher: string;

        /**
         * Constructor.
         * @param {Proxy.Entities.Line} model Data model.
         * @param {string} publisher Task Guide publisher.
         */
        constructor(model: Proxy.Entities.Line, publisher: string) {
            this.id = model.Id;
            this.title = model.Name;
            this.publisher = publisher;
        }
    }
}