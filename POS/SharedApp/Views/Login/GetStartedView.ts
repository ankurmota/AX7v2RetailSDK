/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    export class GetStartedViewController extends ViewControllerBase {

        public appTitle: Observable<string>;

        /**
         * Create an instance of GetStartedViewController
         * @constructor
         */
        constructor() {
            super(true);

            this.appTitle = ko.observable(StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_8000"),
                Commerce.Config.appName));
        }

        /**
         * Navigates to activation page.
         */
        public navigateToActivationPage(): void {
            Helpers.DeviceActivationHelper.navigateToActivationPage();
        }
    }
}