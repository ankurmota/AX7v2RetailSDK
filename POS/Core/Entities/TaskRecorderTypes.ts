/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Proxy.Entities {
    "use strict";

    /**
     * OData types.
     */
    export class TaskRecorderODataType {
        public static recording: string = "#Microsoft.Dynamics.Commerce.Runtime.DataModel.Recording";
        public static scope: string = "#Microsoft.Dynamics.Commerce.Runtime.DataModel.Scope";
        public static annotation: string = "#Microsoft.Dynamics.Commerce.Runtime.DataModel.FormAnnotation";
        public static taskUserAction: string = "#Microsoft.Dynamics.Commerce.Runtime.DataModel.TaskUserAction";
        public static commandUserAction: string = "#Microsoft.Dynamics.Commerce.Runtime.DataModel.CommandUserAction";
        public static infoUserAction: string = "#Microsoft.Dynamics.Commerce.Runtime.DataModel.InfoUserAction";
        public static menuItemUserAction: string = "#Microsoft.Dynamics.Commerce.Runtime.DataModel.MenuItemUserAction";
        public static propertyUserAction: string = "#Microsoft.Dynamics.Commerce.Runtime.DataModel.PropertyUserAction";
        public static validationUserAction: string = "#Microsoft.Dynamics.Commerce.Runtime.DataModel.ValidationUserAction";
    }

    /**
     * Recording entity class.
     */
    export class TaskRecorderRecordingClass implements Recording {

        
        public Scopes: Entities.Scope[];
        public FormContextEntries: Entities.FormContextDictionaryEntry[];
        public RootScope: Entities.Scope;
        public Name: string;
        public Description: string;
        

        /**
         * Construct an object from odata response.
         *
         * @param {any} odataObject The odata result object.
         */
        constructor(odataObject?: any) {
            odataObject = odataObject || {};
            this.Scopes = undefined;
            if (odataObject.Scopes) {
                this.Scopes = [];
                for (var i: number = 0; i < odataObject.Scopes.length; i++) {
                    this.Scopes[i] = odataObject.Scopes[i] ? new ScopeClass(odataObject.Scopes[i]) : null;
                }
            }
            this.FormContextEntries = undefined;
            if (odataObject.FormContextEntries) {
                this.FormContextEntries = [];
                for (var i: number = 0; i < odataObject.FormContextEntries.length; i++) {
                    this.FormContextEntries[i] = odataObject.FormContextEntries[i] ?
                        new FormContextDictionaryEntryClass(odataObject.FormContextEntries[i]) : null;
                }
            }
            this.RootScope = odataObject.RootScope ? new TaskRecorderScopeClass(odataObject.RootScope) : null;
            this.Name = odataObject.Name;
            this.Description = odataObject.Description;
        }
    }

    /**
     * Scope entity class.
     */
    export class TaskRecorderScopeClass extends NodeClass implements Scope {

        
        public Name: string;
        public ScopeTypeValue: number;
        public Children: Entities.Node[];
        public ActiveCount: number;
        public IsForm: boolean;
        

        private oDataTypePropertyName: string = "@odata.type";

        /**
         * Construct an object from odata response.
         *
         * @param {any} odataObject The odata result object.
         */
        constructor(odataObject?: any) {
            super(odataObject);
            odataObject = odataObject || {};
            this.Name = odataObject.Name;
            this.ScopeTypeValue = odataObject.ScopeTypeValue;
            this.Children = undefined;
            if (odataObject.Children) {
                this.Children = [];
                for (var i: number = 0; i < odataObject.Children.length; i++) {
                    this.Children[i] = odataObject.Children[i] ? this.createChild(odataObject.Children[i]) : null;
                }
            }
            this.ActiveCount = odataObject.ActiveCount;
            this.IsForm = odataObject.IsForm;
        }

        private createChild(odataObject?: any): Node {

            var result: Node = null;

            switch (odataObject[this.oDataTypePropertyName]) {
                case TaskRecorderODataType.scope:
                    result = new TaskRecorderScopeClass(odataObject);
                    break;

                case TaskRecorderODataType.commandUserAction:
                    result = new CommandUserActionClass(odataObject);
                    break;

                case TaskRecorderODataType.taskUserAction:
                    result = new TaskUserActionClass(odataObject);
                    break;

                case TaskRecorderODataType.infoUserAction:
                case TaskRecorderODataType.menuItemUserAction:
                case TaskRecorderODataType.propertyUserAction:
                case TaskRecorderODataType.validationUserAction:
                    result = new UserActionClass(odataObject);
                    break;

                default:
                    result = new NodeClass(odataObject);
                    break;
            }

            return result;
        }
    }
}