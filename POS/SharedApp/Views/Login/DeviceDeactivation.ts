/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.ViewModels.d.ts'/>

module Commerce.Controls {
    "use strict";

    export class DeviceDeactivation extends UserControl {

        public viewModel: ViewModels.LoginViewModel;

        /**
         * Create an instance of LoginViewModel
         * @constructor
         */
        constructor() {
            super();

            this.viewModel = new ViewModels.LoginViewModel();
        }

        /**
         * Deactivate a device.
         */
        public deactivateDeviceHandler(): void {

            var dialogDisplayString: string = StringExtensions.format("{0} \n\n {1} \n {2}",
                ViewModelAdapter.getResourceString("string_1415"),
                ViewModelAdapter.getResourceString("string_1414"),
                ViewModelAdapter.getResourceString("string_1416"));

            ViewModelAdapter.displayMessage(dialogDisplayString,
                MessageType.Info,
                MessageBoxButtons.YesNo)
                .done((result: DialogResult) => {
                    if (result === DialogResult.Yes) {
                        this.viewModel.deactivateDevice().done(() => {
                            UI.Tutorial.init();

                            if (Utilities.OfflineHelper.isOfflineEnabled()) {
                                // Clear offline sync timer.
                                clearTimeout(Session.instance.offlineParameters.syncDownloadOfflineData);
                                Session.instance.offlineParameters.syncDownloadOfflineData = 0;
                                clearTimeout(Session.instance.offlineParameters.syncUploadOfflineData);
                                Session.instance.offlineParameters.syncUploadOfflineData = 0;
                            }
                        }).fail((errors: Proxy.Entities.Error[]) => {
                            NotificationHandler.displayClientErrors(errors, "string_1417");
                        });
                    }
                }).fail((errors: Proxy.Entities.Error[]) => {
                    NotificationHandler.displayClientErrors(errors, "string_1417");
                });
        }
    }
}