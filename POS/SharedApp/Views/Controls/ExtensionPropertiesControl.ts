/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='UserControl.ts'/>

module Commerce.Controls {
    "use strict";

    /**
     * Options passed to the extension properties control.
     */
    export interface ExtensionPropertiesControlOptions {
        data: IPropertyBag;
        extensionVisible: Observable<boolean>;
    }

    /**
     * User control for rendenring extension properties.
     */
    export class ExtensionPropertiesControl extends UserControl {

        private _data: Observable<IPropertyBag>;
        private _flattenedData: Computed<PropertyKeyValueType[]>;

        /**
         * Initializes a new instance of the ExtensionPropertiesControl class.
         */
        constructor(options: ExtensionPropertiesControlOptions) {
            super();

            options = options || { data: undefined, extensionVisible: null };
            this._data = ko.observable(options.data);
            this._flattenedData = ko.computed(() => {
                var propertyKeyValueType : PropertyKeyValueType[] = ExtensibilityHelper.flatten(this._data());
                if (propertyKeyValueType.length > 0 && !ObjectExtensions.isNullOrUndefined(options.extensionVisible)) {
                    options.extensionVisible(true);
                }
                return propertyKeyValueType;
            });
        }

        /**
         * Gets the observable data associated with the control.
         *
         * @return {Observable<IPropertyBag>} The observable data rendered by the control.
         */
        public get data(): Observable<IPropertyBag> {
            return this._data;
        }
    }
}