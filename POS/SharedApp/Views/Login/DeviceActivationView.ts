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

    export class DeviceActivationViewController extends ViewControllerBase {

        public viewModel: Commerce.ViewModels.LoginViewModel;
        public operatorId: Observable<string>;
        public appTitle: Observable<string>;
        public disableActivateButton: Computed<boolean>;

        // Error control properties
        public errorHeaderMessage: Observable<string>;
        public errorMessage: Observable<string>;
        public errorDetails: ObservableArray<string>;
        public clientErrorMessage: Observable<string>;
        public footerMessage: Observable<string>;
        public continueLabel: Observable<string>;

        public showErrorControl: Observable<boolean>;

        private showServerUrl: Computed<boolean>;
        private retryActivation: boolean;

        private commonHeaderData: Commerce.Controls.CommonHeaderData;

        /**
         * Create an instance of DeviceActivationViewController.
         * @constructor
         */
        constructor(options?: Model.Entities.IActivationParameters) {
            super(true);

            this.viewModel = new Commerce.ViewModels.LoginViewModel();
            this.operatorId = ko.observable(StringExtensions.EMPTY);
            this.retryActivation = false;

            // Initialize error control variables
            this.errorHeaderMessage = ko.observable(StringExtensions.EMPTY);
            this.errorMessage = ko.observable(StringExtensions.EMPTY);
            this.errorDetails = ko.observableArray([]);
            this.clientErrorMessage = ko.observable(StringExtensions.EMPTY);
            this.footerMessage = ko.observable(ViewModelAdapter.getResourceString("string_8072"));  // After you have resolved the issue, press Retry.
            this.continueLabel = ko.observable(ViewModelAdapter.getResourceString("string_8074"));  // Retry

            this.showErrorControl = ko.observable(false);

            if (!ObjectExtensions.isNullOrUndefined(options)) {
                this.retryActivation = true;
                this.viewModel.serviceUrl(options.serverUrl);
                this.viewModel.deviceId(options.deviceId);
                this.viewModel.registerId(options.registerId);
                this.operatorId(options.operatorId);
                this.viewModel.password(options.password);
            }

            this.showServerUrl = ko.computed(() => {
                var hideServerUrl: boolean = !Commerce.StringExtensions.isNullOrWhitespace(Commerce.Config.onlineDatabase)
                    || Commerce.Config.locatorServiceEnabled;

                return !hideServerUrl;
            }, this);

            this.disableActivateButton = ko.computed(() => {
                return !this.canActivate();
            }, this);

            this.appTitle = ko.observable(StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_8000"),
                Commerce.Config.appName));

            if (Commerce.Config.persistentRetailServerEnabled) {
                this.viewModel.serviceUrl(Commerce.Config.persistentRetailServerUrl);
            }

            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.commonHeaderData.toggleAllElementsVisibility(false);
            this.commonHeaderData.viewCommonHeader(true);
        }

        /**
         * Called when view is shown.
         */
        public onShown(): void {
            if (!Config.isDemoMode && !this.retryActivation) {
                this.operatorId(StringExtensions.EMPTY);
                this.viewModel.password(StringExtensions.EMPTY);
            }

            this.authenticateUser();
        }

        /**    
         * Try to activate device.
         */
        public ActivateDeviceHandler(): void {
            if (this.canActivate()) {
                this.retryActivation = false;
                var parameters: Model.Entities.IActivationParameters = {
                    serverUrl: this.viewModel.serviceUrl(),
                    deviceId: this.viewModel.deviceId(),
                    registerId: this.viewModel.registerId(),
                    operatorId: this.operatorId(),
                    password: this.viewModel.password()
                };

                this.authenticateUser().done((): void => {
                    ViewModelAdapter.navigate(Helpers.DeviceActivationHelper.DEVICE_ACTIVATION_PROCESS_VIEW_NAME, parameters);
                });
            }
        }

        /**    
         * Navigate to guided activation.
         */
        public navigateToGuidedActivation(): void {
            // Update server Url in application storage before navigating
            this.viewModel.updateServerUrl();

            ViewModelAdapter.navigate(Helpers.DeviceActivationHelper.GUIDED_ACTIVATION_VIEW_NAME);
        }

        /**
         * Retry authentication via error control.
         */
        public retryFunction(): void {
            this.showErrorControl(false);
            this.authenticateUser();
        }

        /*
         * Determines if we can try to activate the device.
         * @return {boolean} True if we can activate, false otherwise.
         */
        private canActivate(): boolean {
            return !this.viewModel.cannotActivate(this.viewModel.serviceUrl(),
                this.operatorId(), this.viewModel.password());
        }

        /**
         * Makes sure that the user is authenticated.
         * @return {IVoidAsyncResult} The async result.
         */
        private authenticateUser(): IVoidAsyncResult {
            return this.viewModel.authenticateUser().fail((errors: Model.Entities.Error[]) => {
                this.displayErrorControl(errors, Commerce.ViewModelAdapter.getResourceString("string_8044"));   // Logging in anonymously failed.
            });
        }

        /**
         * Sets the values for and displays the error control.
         * @param {Model.Entities.Error[]} errors The set of errors to be displayed.
         * @param {string} headerMessage The header message to be displayed on the error control.
         */
        private displayErrorControl(errors: Model.Entities.Error[], headerMessage: string): void {
            if (Config.aadEnabled) {
                RetailLogger.viewsCloudDeviceActivationViewActivationFailed();
            }

            var errorDetails: Model.Entities.IErrorDetails = ErrorHelper.resolveError(errors[0].ErrorCode);
            var errorMessage: string = ErrorHelper.formatErrorMessage(errors[0]);

            this.errorHeaderMessage(headerMessage);
            this.clientErrorMessage(errorDetails.clientErrorCode);
            this.errorMessage(errorMessage);

            var localizedMessageDetails: string[] = errorDetails.messageDetailsResource;
            if (ArrayExtensions.hasElements(localizedMessageDetails)) {
                for (var i: number = 0; i < localizedMessageDetails.length; i++) {
                    localizedMessageDetails[i] = ViewModelAdapter.getResourceString(localizedMessageDetails[i]);
                }
            }

            this.errorDetails(localizedMessageDetails);

            this.showErrorControl(true);
        }
    }
}