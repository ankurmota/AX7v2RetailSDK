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
     * Scope types.
     */
    export enum ScopeType {
        Public,
        Private,
        PrivateInline,
        Task
    }

    /**
     * Task types.
     */
    export enum TaskRecorderTaskType {
        /**
         * Begin task.
         */
        Begin = 0x0001,

        /**
         * End task.
         */
        End = 0x0010
    }

    /**
     * Provides utility methods for constructing Task Recorder model objects.
     */
    export class RecordingFactory {

        private static oDataPropertyName: string = "@odata.type";

        /**
         * Creates a new Task Recorder recording with given name and description.
         * @param {string} name The name of the recording.
         * @param {string} description The description of the recording.
         * @returns {Proxy.Entities.Recording} The Task Recorder recording.
         */
        public static createNew(name?: string, description?: string): Proxy.Entities.Recording {
            var rootScope: Proxy.Entities.Scope = RecordingFactory.createNewScope(TaskRecorderUtil.generateGuid(),
                name, null, ScopeType.Public);

            var recording: Proxy.Entities.Recording = {
                Name: name,
                Description: description,
                RootScope: rootScope
            };

            recording[this.oDataPropertyName] = Proxy.Entities.TaskRecorderODataType.recording;

            return recording;
        }

        /**
         * Creates a new Task Recorder scope.
         * @param {string} id The Id of the scope.
         * @param {string} name The name of the scope.
         * @param {string} description The description of the scope.
         * @param {ScopeType} scopeType Scope type.
         * @returns {Proxy.Entities.Scope} The Task Recorder scope.
         */
        public static createNewScope(id?: string, name?: string, description?: string, scopeType?: ScopeType): Proxy.Entities.Scope {

            var scope: Proxy.Entities.Scope = {
                Id: id,
                Name: name,
                Description: description,
                ScopeTypeValue: scopeType,
                ActiveCount: 0,
                Children: []
            };

            scope[this.oDataPropertyName] = Proxy.Entities.TaskRecorderODataType.scope;

            return scope;
        }

        /**
         * Creates a new Task Recorder task.
         * @param {string} id The task Id.
         * @param {string} name The name of the task.
         * @param {string} comment The comment of the task.
         * @param {string} description The description of the task.
         * @param {TaskRecorderTaskType} taskType The type of the task.
         * @returns {Proxy.Entities.Scope} The Task Recorder task.
         */
        public static createNewTask(id?: string, name?: string, comment?: string, description?: string,
            taskType?: TaskRecorderTaskType): Proxy.Entities.TaskUserAction {

            var task: Proxy.Entities.TaskUserAction = {
                Id: id,
                TaskId: id,
                Name: name,
                Description: description,
                Comment: comment,
                UserActionTypeValue: taskType
            };

            task[this.oDataPropertyName] = Proxy.Entities.TaskRecorderODataType.taskUserAction;

            return task;
        }

        /**
         * Creates a new Task Recorder command.
         * @param {string} id The command Id.
         * @param {string} name The name of the command.
         * @param {string} description The description of the command.
         * @param {string} notes The notes of the command.
         * @param {string} screenshotUri The screenshotUri of the command.
         * @returns {Proxy.Entities.CommandUserAction} The Task Recorder command.
         */
        public static createNewCommand(id?: string, name?: string, description?: string,
            notes?: string, screenshotUri?: string): Proxy.Entities.CommandUserAction {

            var command: Proxy.Entities.CommandUserAction = {
                Id: id,
                CommandName: name,
                Description: description,
                ReturnTypeValue: 0, // Void
                Annotations: [],
                ScreenshotUri: screenshotUri
            };

            if (!StringExtensions.isNullOrWhitespace(notes)) {
                command.Annotations.push(this.createNewAnnotation(notes));
            }

            command[this.oDataPropertyName] = Proxy.Entities.TaskRecorderODataType.commandUserAction;

            return command;
        }

        /**
         * Creates a new Task Recorder annotation.
         * @param {string} description The description of the annotation.
         * @returns {Proxy.Entities.FormAnnotation} The Task Recorder annotation.
         */
        public static createNewAnnotation(description?: string): Proxy.Entities.FormAnnotation {
            var annotation: Proxy.Entities.FormAnnotation = {
                Description: description
            };

            annotation[this.oDataPropertyName] = Proxy.Entities.TaskRecorderODataType.annotation;

            return annotation;
        }
    }
}
