/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ViewModelBase.ts'/>

module Commerce.ViewModels {
    "use strict";

    export class HomeViewModel extends ViewModelBase {

        public buttonGridData: ObservableArray<Commerce.Model.Entities.ButtonGrid>;
        public base64ImageData: Observable<string>;

        constructor() {
            super();

            this.buttonGridData = ko.observableArray<Commerce.Model.Entities.ButtonGrid>([]);
            this.base64ImageData = ko.observable("");

            this.initWelcomeScreenLayout();
        }

        /**
        *  retrieves the tillLayout containing buttonGrid identifiers, then retrieves each button grid
        */
        private initWelcomeScreenLayout() {
            // add applicable welcome screens to enable them for viewing on the welcome screen
            var welcomeScreenZones = ["WelcomeScreen1", "WelcomeScreen2", "WelcomeScreen3", "WelcomeScreen4", "WelcomeScreen5"];
            var tillLayoutProxy = Commerce.ApplicationContext.Instance.tillLayoutProxy;
            var buttonGrids = tillLayoutProxy.getButtonGridByZoneIds(welcomeScreenZones);
            var imageZones = tillLayoutProxy.getImageZones(["WelcomeScreen1"]);

            // gets the background image encoding
            if (!ObjectExtensions.isNullOrUndefined(imageZones) && imageZones.length > 0 && !StringExtensions.isNullOrWhitespace(imageZones[0].PictureAsBase64)) {
                this.base64ImageData(imageZones[0].PictureAsBase64);
            }

            this.buttonGridData(buttonGrids.getItems());
        }
    }
}