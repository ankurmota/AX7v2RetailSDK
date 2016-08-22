/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="../../Utilities/ThrowIf.ts" />

module Commerce.TaskRecorder.ViewModel {

    /**
     * Represents the Task Recorder step view model.
     */
    export class TaskRecorderStepViewModel implements ITaskRecorderNodeViewModel {

        /**
         * The node identifier.
         */
        public id: string;

        /**
         * The node sequence number.
         */
        public sequence: Observable<number>;

        /**
         * The flag for editable node.
         */
        public editable: boolean;

        /**
         * The text of step.
         */
        public text: Observable<string>;

        /**
         * The notes of step.
         */
        public notes: Observable<string>;

        /**
         * The node screenshot URI.
         */
        public screenshotUri: string;

        /**
         * The node display description.
         */
        public description: Computed<string>;

        /**
         * The node parent.
         */
        public parent: Observable<ITaskRecorderNodeViewModel>;

        /**
         * The node OData type.
         */
        public oDataClass: string;

        /**
         * The node display number.
         */
        public displayNumber: Computed<string>;

        /**
         * Constructor.
         * @param model {Proxy.Entities.UserAction} Data model.
         */
        constructor(model: Proxy.Entities.UserAction) {
            ThrowIf.argumentIsNotObject(model, "model");

            this.id = model.Id;
            this.sequence = ko.observable(model.Sequence);
            this.text = ko.observable(model.Description);

            if (!ObjectExtensions.isNullOrUndefined(model.Annotations) && model.Annotations.length > 0) {
                this.notes = ko.observable(model.Annotations[0].Description);
            } else {
                this.notes = ko.observable(StringExtensions.EMPTY);
            }

            this.parent = ko.observable(null);

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
                return this.text();
            });

            this.editable = true;

            this.oDataClass = Proxy.Entities.TaskRecorderODataType.commandUserAction;
        }

        /**
         * Returns the underlying object.
         * @returns {Proxy.Entities.CommandUserAction} The Task Recorder user action.
         */
        public toModel(): Proxy.Entities.CommandUserAction {
            return Model.RecordingFactory.createNewCommand(this.id, this.text(), this.text(), this.notes(), this.screenshotUri);
        }
    }
}