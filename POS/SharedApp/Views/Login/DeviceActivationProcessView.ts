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

    export class DeviceActivationProcessViewController extends ViewControllerBase {

        private static appTitleResourceStrFormat: string = "string_8000";
        private static getStartedResourceStr: string = "string_8026";
        private static deviceActivatedResourceStr: string = "string_8025";

        public controllerState: Observable<Model.Entities.DeviceActivationControllerState>;
        public loginViewModel: ViewModels.LoginViewModel;

        // Progress view properties
        public progressBarValue: Observable<number>;
        public progressMessage: Observable<string>;

        // Error control properties
        public errorHeaderMessage: Observable<string>;
        public errorMessage: Observable<string>;
        public errorDetails: ObservableArray<string>;
        public clientErrorMessage: Observable<string>;
        public footerMessage: Observable<string>;
        public headerDeviceId: Observable<string>;
        public headerRegisterNumber: Observable<string>;
        public continueLabel: Observable<string>;

        // Success control properties
        public getStartedMessage: Observable<string>;
        public deviceActivatedMessage: Observable<string>;

        public appTitle: Observable<string>;

        private operatorId: string;
        private activationParameters: Model.Entities.IActivationParameters;
        private indeterminateWaitVisible: Observable<boolean>;
        private commonHeaderData: Commerce.Controls.CommonHeaderData;
        private applicationLanguage: string;

        /**
         * Create an instance of DeviceActivationProcessViewController
         * @constructor
         */
        constructor(options: Model.Entities.IActivationParameters) {
            super(true);

            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.commonHeaderData.toggleAllElementsVisibility(false);
            this.commonHeaderData.viewCommonHeader(true);

            this.activationParameters = options;
            this.operatorId = StringExtensions.EMPTY;
            this.loginViewModel = new ViewModels.LoginViewModel();
            this.applicationLanguage = ViewModelAdapter.getCurrentAppLanguage();

            this.indeterminateWaitVisible = ko.observable(false);

            if (!ObjectExtensions.isNullOrUndefined(this.activationParameters)) {
                if (ObjectExtensions.isString(options)) {
                    this.activationParameters = this.deserializeActivationParameters(<string>options);
                }

                this.loginViewModel.serviceUrl(this.activationParameters.serverUrl);
                this.loginViewModel.deviceId(this.activationParameters.deviceId);
                this.loginViewModel.registerId(this.activationParameters.registerId);
                this.operatorId = this.activationParameters.operatorId;
                this.loginViewModel.password(this.activationParameters.password);
                this.loginViewModel.skipConnectivityOperation = this.activationParameters.skipConnectivityOperation;

                if (Config.aadEnabled) {
                    var activationParamString: string = this.serializeActivationParameters(this.activationParameters);
                    ApplicationStorage.setItem(ApplicationStorageIDs.ACTIVATION_PAGE_PARAMETERS_KEY, activationParamString);
                }
            }

            this.progressBarValue = ko.observable(0);
            this.progressMessage = ko.observable(StringExtensions.EMPTY);

            this.errorHeaderMessage = ko.observable(StringExtensions.EMPTY);
            this.errorMessage = ko.observable(StringExtensions.EMPTY);
            this.errorDetails = ko.observableArray([]);
            this.clientErrorMessage = ko.observable(StringExtensions.EMPTY);
            this.footerMessage = ko.observable(StringExtensions.EMPTY);
            this.headerDeviceId = ko.observable(StringExtensions.EMPTY);
            this.headerRegisterNumber = ko.observable(StringExtensions.EMPTY);
            this.continueLabel = ko.observable(ViewModelAdapter.getResourceString("string_8074"));

            this.getStartedMessage = ko.observable(ViewModelAdapter.getResourceString(DeviceActivationProcessViewController.getStartedResourceStr));
            this.deviceActivatedMessage = ko.observable(ViewModelAdapter.getResourceString(DeviceActivationProcessViewController.deviceActivatedResourceStr));

            this.loginViewModel.currentActivationOperation.subscribe((newValue: Operations.IDeviceActivationOperation) => {
                if (ObjectExtensions.isNullOrUndefined(newValue)) {
                    return;
                }

                var currentStep: number = this.loginViewModel.currentOperationStep();
                var totalSteps: number = this.loginViewModel.totalOperationSteps();
                var statusName: string = newValue.processingStatusName();

                if (currentStep === totalSteps) {
                    // Finishing up...
                    statusName = ViewModelAdapter.getResourceString("string_8071");
                }

                this.progressBarValue((currentStep - 1) / totalSteps);

                this.progressMessage(StringExtensions.format(
                    ViewModelAdapter.getResourceString("string_8040"),
                    currentStep, totalSteps, statusName));

                this.updateTextTranslations();
            }, this);

            this.controllerState = ko.observable(Model.Entities.DeviceActivationControllerState.Processing);

            this.appTitle = ko.observable(StringExtensions.format(Commerce.ViewModelAdapter.getResourceString(
                DeviceActivationProcessViewController.appTitleResourceStrFormat), Commerce.Config.appName));
        }

        /**
         * Actions when View is displayed.
         */
        public onShown(): void {
            this.loginViewModel.currentActivationOperation(null);
            this.activateDevice();
        }

        /**
         * Activates device.
         */
        public activateDevice(): void {
            this.controllerState(Model.Entities.DeviceActivationControllerState.Processing);

            this.loginViewModel.activateDevice(this.operatorId)
                .done(() => {
                    this.controllerState(Model.Entities.DeviceActivationControllerState.Succeeded);
                })
                .fail((errors: Model.Entities.Error[]) => {
                    if (Config.aadEnabled) {
                        RetailLogger.viewsCloudDeviceActivationViewActivationFailed();
                    }

                    var errorDetails: Model.Entities.IErrorDetails = ErrorHelper.resolveError(errors[0].ErrorCode);
                    var errorMessage: string = ErrorHelper.formatErrorMessage(errors[0]);

                    this.errorHeaderMessage(StringExtensions.format(ViewModelAdapter.getResourceString("string_8039"),
                        this.loginViewModel.currentOperationStep(), this.loginViewModel.totalOperationSteps(),
                        this.loginViewModel.currentActivationOperation().errorStatusName()));

                    this.clientErrorMessage(errorDetails.clientErrorCode);
                    this.errorMessage(errorMessage);

                    var localizedMessageDetails: string[] = errorDetails.messageDetailsResource;

                    if (ArrayExtensions.hasElements(localizedMessageDetails)) {
                        for (var i: number = 0; i < localizedMessageDetails.length; i++) {
                            localizedMessageDetails[i] = ViewModelAdapter.getResourceString(localizedMessageDetails[i]);
                        }
                    }

                    this.errorDetails(localizedMessageDetails);

                    // when previous error is ATTEMPTTOACTIVATEFROMDIFFERENTPHYSICALDEVICE
                    // and user clicks retry, the next activation attempt should enforce activation.
                    var forceActivate: boolean =
                        ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_ATTEMPTTOACTIVATEFROMDIFFERENTPHYSICALDEVICE.serverErrorCode
                        === errorDetails.serverErrorCode;
                    this.setErrorDetailsMessage(forceActivate);
                    this.loginViewModel.forceActivate = forceActivate;

                    this.controllerState(Model.Entities.DeviceActivationControllerState.Error);
                });
        }

        /**
         * Navigates to activation page.
         */
        public navigateToActivationPage(): void {
            ViewModelAdapter.navigate("DeviceActivationView", this.activationParameters);
        }

        /**
         * Navigates to LoginView.
         */
        public launch(): void {
            if (Commerce.Config.aadEnabled) {
                this.indeterminateWaitVisible(true);

                // End session has already been called at the end of the device activation sequence when AAD authentication is enabled,
                // so we just need to log off from the authentication provider.
                Utilities.LogonHelper.logoffAuthenticationProvider().always(() => {
                    this.indeterminateWaitVisible(false);
                    ViewModelAdapter.navigate("LoginView");
                });
            } else {
                ViewModelAdapter.navigate("LoginView");
            }
        }

        private updateTextTranslations(): void {
            var currentLanguage: string = ViewModelAdapter.getCurrentAppLanguage();

            if (!StringExtensions.isNullOrWhitespace(currentLanguage) && currentLanguage !== this.applicationLanguage) {
                this.appTitle(StringExtensions.format(Commerce.ViewModelAdapter.getResourceString(
                    DeviceActivationProcessViewController.appTitleResourceStrFormat), Commerce.Config.appName));
                this.getStartedMessage(ViewModelAdapter.getResourceString(DeviceActivationProcessViewController.getStartedResourceStr));
                this.deviceActivatedMessage(ViewModelAdapter.getResourceString(DeviceActivationProcessViewController.deviceActivatedResourceStr));
            }
        }

        private serializeActivationParameters(activationParameters: Model.Entities.IActivationParameters): string {
            var param: string = activationParameters.serverUrl + ";" +
                activationParameters.deviceId + ";" +
                activationParameters.registerId + ";" +
                activationParameters.operatorId + ";" +
                activationParameters.password + ";" +
                activationParameters.skipConnectivityOperation + ";";

            return param;
        }

        private deserializeActivationParameters(param: string): Model.Entities.IActivationParameters {
            var parameters: string[] = param.split(";");
            var activationParameters: Model.Entities.IActivationParameters = {
                serverUrl: parameters[0],
                deviceId: parameters[1],
                registerId: parameters[2],
                operatorId: parameters[3],
                password: parameters[4],
                skipConnectivityOperation: "true" === parameters[5]
            };

            return activationParameters;
        }

        private setErrorDetailsMessage(isErrorFromActivationAttemptFromDifferentDevice: boolean): void {
            if (isErrorFromActivationAttemptFromDifferentDevice) {
                this.errorHeaderMessage(ViewModelAdapter.getResourceString("string_1443"));
                this.headerDeviceId(StringExtensions.format(
                    ViewModelAdapter.getResourceString("string_1445"), this.loginViewModel.deviceId()));
                this.headerRegisterNumber(StringExtensions.format(
                    ViewModelAdapter.getResourceString("string_1446"), this.loginViewModel.registerId()));
                this.footerMessage(StringExtensions.format(ViewModelAdapter.getResourceString("string_1444"), Config.appName));
                this.continueLabel(ViewModelAdapter.getResourceString("string_1447"));
            } else {
                this.headerDeviceId(StringExtensions.EMPTY);
                this.headerRegisterNumber(StringExtensions.EMPTY);
                this.footerMessage(ViewModelAdapter.getResourceString("string_8072"));
                this.continueLabel(ViewModelAdapter.getResourceString("string_8074"));
            }
        }
    }
}
