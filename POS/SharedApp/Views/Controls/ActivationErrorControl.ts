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
     * Options passed to the activation error control.
     */
    export interface IActivationErrorControlOptions {
        errorHeaderMessage: Observable<string>;
        clientErrorMessage: Observable<string>;
        errorMessage: Observable<string>;
        errorDetails: ObservableArray<string>;
        footerMessage: Observable<string>;
        previousLabel?: Observable<string>;
        continueLabel: Observable<string>;
        headerDeviceId?: Observable<string>;
        headerRegisterNumber?: Observable<string>;
        previousFunction?: () => void;
        retryFunction: () => void;
    }

    /**
     * User control for rendering the activation error control.
     */
    export class ActivationErrorControl extends UserControl {

        public errorHeaderMessage: Observable<string>;
        public headerDeviceId: Observable<string>;
        public headerRegisterNumber: Observable<string>;
        public clientErrorMessage: Observable<string>;
        public errorMessage: Observable<string>;
        public errorDetails: ObservableArray<string>;
        public footerMessage: Observable<string>;
        public previousLabel: Observable<string>;
        public continueLabel: Observable<string>;

        public appSessionId: string;
        public previousFunction: () => void;
        public retryFunction: () => void;

        private headerMessageVisible: Computed<boolean>;
        private previousButtonVisible: Observable<boolean>;

        /**
         * User control for activation error control
         */
        constructor(options: IActivationErrorControlOptions) {
            super();

            this.errorHeaderMessage = options.errorHeaderMessage;
            this.clientErrorMessage = options.clientErrorMessage;
            this.errorMessage = options.errorMessage;
            this.errorDetails = options.errorDetails;
            this.footerMessage = options.footerMessage;
            this.continueLabel = options.continueLabel;
            this.retryFunction = options.retryFunction;

            this.headerDeviceId = ObjectExtensions.isNullOrUndefined(options.headerDeviceId)
                ? ko.observable(StringExtensions.EMPTY) : options.headerDeviceId;
            this.headerRegisterNumber = ObjectExtensions.isNullOrUndefined(options.headerRegisterNumber)
                ? ko.observable(StringExtensions.EMPTY) : options.headerRegisterNumber;
            this.previousLabel = ObjectExtensions.isNullOrUndefined(options.previousLabel)
                ? ko.observable(Commerce.ViewModelAdapter.getResourceString("string_8073")) : options.previousLabel;

            if (!ObjectExtensions.isNullOrUndefined(options.previousFunction)) {
                this.previousButtonVisible = ko.observable(true);
                this.previousFunction = options.previousFunction;
            } else {
                this.previousButtonVisible = ko.observable(false);
                this.previousFunction = null;
            }

            this.headerMessageVisible = ko.computed(() => {
                return !Commerce.StringExtensions.isNullOrWhitespace(this.headerDeviceId()) &&
                    !Commerce.StringExtensions.isNullOrWhitespace(this.headerRegisterNumber());
            }, this);

            this.appSessionId = TsLogging.LoggerBase.getAppSessionId();
        }
    }
}