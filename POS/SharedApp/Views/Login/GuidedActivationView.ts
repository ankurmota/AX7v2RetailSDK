/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.ViewControllers {
    "use strict";

    import Entities = Proxy.Entities;

    export class GuidedActivationViewController extends ViewControllerBase {

        // View models
        public viewModel: Commerce.ViewModels.LoginViewModel;
        public activationViewModel: ViewModels.ActivationViewModel;
        public storeOperationsViewModel: Commerce.ViewModels.StoreOperationsViewModel;

        // Values
        public operatorId: string;
        public appTitle: Observable<string>;
        public selectedStore: Observable<Model.Entities.OrgUnit>;
        public selectedRegisterAndDevice: Observable<Model.Entities.TerminalInfo>;
        public selectedDevice: Observable<Model.Entities.Device>;
        public isAutoDeviceIdChecked: Observable<boolean>;
        public newDeviceId: Observable<string>;

        public storeOptions: ObservableArray<Model.Entities.OrgUnit>;
        public registerAndDeviceOptions: ObservableArray<Model.Entities.TerminalInfo>;
        public deviceOptions: ObservableArray<Model.Entities.Device>;
        public databaseStatusStr: Observable<string>;
        public rtsStatusStr: Observable<string>;
        public databaseIconStatus: Observable<string>;
        public rtsIconStatus: Observable<string>;
        public isCheckInProgress: Observable<boolean>;

        // Visibility booleans
        public showStores: Observable<boolean>;
        public showRegistersAndDevices: Observable<boolean>;
        public showRetrievingStores: Observable<boolean>;
        public showRetrievingRegistersAndDevices: Observable<boolean>;
        public addNewDeviceSelected: Observable<boolean>;
        public showHealthCheckStatus: Observable<boolean>;
        public showUserAuthenticating: Observable<boolean>;

        public disableActivateButton: Computed<boolean>;
        public disableNextButton: Computed<boolean>;
        public disableHealthCheckButton: Computed<boolean>;
        public showNextButton: Computed<boolean>;
        public showStartButton: Computed<boolean>;
        public showSelectedStore: Computed<boolean>;
        public showDeviceSelector: Computed<boolean>;
        public showDeviceCreator: Computed<boolean>;

        // Error control properties
        public errorHeaderMessage: Observable<string>;
        public errorMessage: Observable<string>;
        public errorDetails: ObservableArray<string>;
        public clientErrorMessage: Observable<string>;
        public footerMessage: Observable<string>;
        public previousLabel: Observable<string>;
        public continueLabel: Observable<string>;

        public showErrorControl: Observable<boolean>;

        // Private variables
        private showServerUrl: Computed<boolean>;
        private retryActivation: boolean;
        private _errorsToShow: Model.Entities.Error[];
        private skipConnectivityOperation: boolean;

        private commonHeaderData: Commerce.Controls.CommonHeaderData;

        /**
         * Create an instance of GuidedActivationViewController.
         * @constructor
         */
        constructor(options?: Model.Entities.IActivationParameters) {
            super(true);

            // Initialize values
            this.viewModel = new Commerce.ViewModels.LoginViewModel();
            this.storeOperationsViewModel = new Commerce.ViewModels.StoreOperationsViewModel();
            this.activationViewModel = new Commerce.ViewModels.ActivationViewModel();
            this.retryActivation = false;
            this.operatorId = StringExtensions.EMPTY;

            this.storeOptions = ko.observableArray([]);
            this.registerAndDeviceOptions = ko.observableArray([]);
            this.deviceOptions = ko.observableArray([]);
            this.selectedStore = ko.observable(null);
            this.selectedRegisterAndDevice = ko.observable(null);
            this.selectedDevice = ko.observable(null);
            this.newDeviceId = ko.observable(StringExtensions.EMPTY);
            this.isAutoDeviceIdChecked = ko.observable(false);
            this.skipConnectivityOperation = false;

            // Initialize visibility booleans
            this.showRetrievingStores = ko.observable(false);
            this.showStores = ko.observable(false);
            this.showRetrievingRegistersAndDevices = ko.observable(false);
            this.showRegistersAndDevices = ko.observable(false);
            this.showHealthCheckStatus = ko.observable(false);
            this.addNewDeviceSelected = ko.observable(false);
            this.showUserAuthenticating = ko.observable(false);

            // Initialize error control variables
            this.errorHeaderMessage = ko.observable(StringExtensions.EMPTY);
            this.errorMessage = ko.observable(StringExtensions.EMPTY);
            this.errorDetails = ko.observableArray([]);
            this.clientErrorMessage = ko.observable(StringExtensions.EMPTY);
            this.databaseStatusStr = ko.observable(StringExtensions.EMPTY);
            this.rtsStatusStr = ko.observable(StringExtensions.EMPTY);
            this.rtsIconStatus = ko.observable(StringExtensions.EMPTY);
            this.databaseIconStatus = ko.observable(StringExtensions.EMPTY);
            this.footerMessage = ko.observable(ViewModelAdapter.getResourceString("string_8072"));  // After you have resolved the issue, press Retry.
            this.previousLabel = ko.observable(ViewModelAdapter.getResourceString("string_1449"));  // Start over
            this.continueLabel = ko.observable(ViewModelAdapter.getResourceString("string_8074"));  // Retry
            this.isCheckInProgress = ko.observable(false);

            this.showErrorControl = ko.observable(false);

            if (!ObjectExtensions.isNullOrUndefined(options)) {
                this.retryActivation = true;
                this.viewModel.serviceUrl(options.serverUrl);
                this.viewModel.deviceId(options.deviceId);
                this.viewModel.registerId(options.registerId);
                this.operatorId = options.operatorId;
                this.viewModel.password(options.password);
                this._errorsToShow = options.errors;
            }

            this.activationViewModel.dbConnectivityStatus.subscribe((newValue: Entities.HealthCheckConnectivityStatus) => {
                this.setConnectionStatusMessage(this.databaseIconStatus, "string_8090", this.databaseStatusStr, newValue);
            });

            this.activationViewModel.rtsConnectivityStatus.subscribe((newValue: Entities.HealthCheckConnectivityStatus) => {
                this.setConnectionStatusMessage(this.rtsIconStatus, "string_8091", this.rtsStatusStr, newValue);
            });

            // Define computed variables
            this.showServerUrl = ko.computed(() => {
                var hideServerUrl: boolean = this.showRetrievingRegistersAndDevices()
                    || this.showRegistersAndDevices()
                    || this.showUserAuthenticating();

                return !hideServerUrl;
            }, this);
            this.showSelectedStore = ko.computed(() => {
                return this.showRetrievingRegistersAndDevices() || this.showRegistersAndDevices();
            }, this);
            this.showDeviceSelector = ko.computed(() => {
                return !ObjectExtensions.isNullOrUndefined(this.selectedRegisterAndDevice())
                    && StringExtensions.isNullOrWhitespace(this.selectedRegisterAndDevice().DeviceNumber)
                    && !this.showDeviceCreator();
            }, this);
            this.showDeviceCreator = ko.computed(() => {
                return this.addNewDeviceSelected()
                    && !ObjectExtensions.isNullOrUndefined(this.selectedRegisterAndDevice())
                    && StringExtensions.isNullOrWhitespace(this.selectedRegisterAndDevice().DeviceNumber);
            }, this);

            this.showNextButton = ko.computed(() => {
                return (!this.showRetrievingStores() || this.showStores())
                    && !this.showRetrievingRegistersAndDevices()
                    && !this.showRegistersAndDevices()
                    && !this.showUserAuthenticating();
            }, this);
            this.showStartButton = ko.computed(() => {
                return this.showRetrievingStores() || this.showStores();
            }, this);

            this.disableNextButton = ko.computed(() => {
                return (!this.showStartButton() && !Core.RegularExpressionValidations.validateUrl(this.viewModel.serviceUrl()))
                    || (this.showStores() && Commerce.ObjectExtensions.isNullOrUndefined(this.selectedStore()))
                    || (this.showRegistersAndDevices() && Commerce.ObjectExtensions.isNullOrUndefined(this.selectedRegisterAndDevice()));
            }, this);
            this.disableActivateButton = ko.computed(() => {
                return (this.showRegistersAndDevices() && ObjectExtensions.isNullOrUndefined(this.selectedRegisterAndDevice()))
                    || (this.showDeviceSelector() && ObjectExtensions.isNullOrUndefined(this.selectedDevice()))
                    || (this.showDeviceCreator() && (StringExtensions.isNullOrWhitespace(this.newDeviceId()) && !this.isAutoDeviceIdChecked()));
            }, this);
            this.disableHealthCheckButton = ko.computed(() => {
                return !Core.RegularExpressionValidations.validateUrl(this.viewModel.serviceUrl());
            });

            this.appTitle = ko.observable(StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_8000"),
                Commerce.Config.appName));

            if (Commerce.Config.persistentRetailServerEnabled) {
                this.viewModel.serviceUrl(Commerce.Config.persistentRetailServerUrl);
            }

            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.commonHeaderData.toggleAllElementsVisibility(false);
            this.commonHeaderData.viewCommonHeader(true);
        }

        /*
         * Gets the activation status string associated with the status number.
         * @param {number} statusNumber The activation status value.
         * @return {string} The activation status string.
         */
        public static getActivationStatusStrings(statusNumber: number): string {
            var statusString: string = StringExtensions.EMPTY;

            switch (statusNumber) {
                case Model.Entities.DeviceActivationStatus.Pending:
                    statusString = Commerce.ViewModelAdapter.getResourceString("string_1468"); // Pending
                    break;
                case Model.Entities.DeviceActivationStatus.Activated:
                    statusString = Commerce.ViewModelAdapter.getResourceString("string_1469"); // Activated
                    break;
                case Model.Entities.DeviceActivationStatus.Deactivated:
                    statusString = Commerce.ViewModelAdapter.getResourceString("string_1470"); // Deactivated
                    break;
                case Model.Entities.DeviceActivationStatus.Disabled:
                    statusString = Commerce.ViewModelAdapter.getResourceString("string_1471"); // Disabled
                    break;
                case Model.Entities.DeviceActivationStatus.None:
                    statusString = Commerce.ViewModelAdapter.getResourceString("string_1472"); // No associated devices
                    break;
            }

            return statusString;
        }

        /**
         * Called when view is shown.
         */
        public onShown(): void {
            if (!Config.isDemoMode && !this.retryActivation) {
                this.operatorId = StringExtensions.EMPTY;
                this.viewModel.password(StringExtensions.EMPTY);
            }
            if (ArrayExtensions.hasElements(this._errorsToShow)) {
                NotificationHandler.displayClientErrors(this._errorsToShow).always(() => {
                    this.authenticateUser();
                });
            } else {
                this.authenticateUser();
            }
        }

        /**    
         * Try to activate device.
         */
        public activateDeviceHandler(): void {
            // Set activation parameters
            this.viewModel.registerId(this.selectedRegisterAndDevice().TerminalId);
            if (!StringExtensions.isNullOrWhitespace(this.selectedRegisterAndDevice().DeviceNumber)) {
                // Device Id specified via association with the register
                this.viewModel.deviceId(this.selectedRegisterAndDevice().DeviceNumber);
            } else if (!ObjectExtensions.isNullOrUndefined(this.selectedDevice()) && !this.showDeviceCreator()) {
                // Device Id specified via device selector
                this.viewModel.deviceId(this.selectedDevice().DeviceNumber);
            } else if (!this.isAutoDeviceIdChecked() && !StringExtensions.isNullOrWhitespace(this.newDeviceId())) {
                // Device Id specified via add new device
                this.viewModel.deviceId(this.newDeviceId());
            } else {
                // Device Id will be autogenerated - empty string triggers the generation
                this.viewModel.deviceId(StringExtensions.EMPTY);
            }

            if (this.canActivate()) {
                this.retryActivation = false;
                var parameters: Model.Entities.IActivationParameters = {
                    serverUrl: this.viewModel.serviceUrl(),
                    deviceId: this.viewModel.deviceId(),
                    registerId: this.viewModel.registerId(),
                    operatorId: this.operatorId,
                    password: this.viewModel.password(),
                    skipConnectivityOperation: this.skipConnectivityOperation
                };

                this.authenticateUser().done((): void => {
                    ViewModelAdapter.navigate(Helpers.DeviceActivationHelper.DEVICE_ACTIVATION_PROCESS_VIEW_NAME, parameters);
                });
            }
        }

        /**
         * Ping the health check url.
         */
        public pingHealthCheck(): void {
            this.showHealthCheckStatus(true);
            this.activationViewModel.pingHealthCheck(this.viewModel.serviceUrl());
        }

        /**    
         * Try to complete the next step in the guided activation process.
         */
        public nextHandler(): void {
            this.showErrorControl(false);
            this.resetHealthCheckStatus();

            if (ObjectExtensions.isNullOrUndefined(Commerce.Session.instance.CurrentEmployee)) {
                // AAD failed - retry
                this.authenticateUser();
            } else if ((!this.showStartButton() || this.showRetrievingStores()) && !StringExtensions.isNullOrWhitespace(this.viewModel.serviceUrl())) {
                // Set waiting state
                this.showRetrievingStores(true);

                // Update server Url in application storage
                this.viewModel.updateServerUrl();

                var asyncQueue: AsyncQueue = new AsyncQueue();

                asyncQueue.enqueue(() => {
                    return Operations.CheckServerConnectivityOperation.operationProcessImpl(this.viewModel.serviceUrl());
                }).enqueue(() => {
                    // Retrieve stores for the Url
                    return this.storeOperationsViewModel.getEmployeeStoresAsync()
                        .done((stores: Model.Entities.OrgUnit[]) => {
                            this.storeOptions(stores);
                            this.showStores(true);
                            this.showRetrievingStores(false);

                            this.skipConnectivityOperation = true;
                        });
                });

                asyncQueue.run().fail((errors: Model.Entities.Error[]) => {
                    this.displayErrorControl(errors, Commerce.ViewModelAdapter.getResourceString("string_1465"));   // Retrieving the stores failed.
                });

            } else if (this.showStores() && !ObjectExtensions.isNullOrUndefined(this.selectedStore())) {
                // Set waiting state
                this.showRetrievingRegistersAndDevices(true);

                // Retrieve terminal info for this store
                this.viewModel.channelManager.getTerminalInfoAsync(this.selectedStore().OrgUnitNumber, Commerce.Host.instance.application.getApplicationType())
                    .done((terminals: Model.Entities.TerminalInfo[]) => {
                        this.registerAndDeviceOptions(terminals);

                        // Retrieve devices list for this store
                        this.viewModel.storeOperationsManager.getAvailableDevices(Commerce.Host.instance.application.getApplicationType())
                            .done((devices: Model.Entities.Device[]) => {
                                this.deviceOptions(devices);
                                this.showRegistersAndDevices(true);
                                this.showRetrievingRegistersAndDevices(false);
                            }).fail((errors: Model.Entities.Error[]) => {
                                // Retrieving the available devices failed.
                                this.displayErrorControl(errors, Commerce.ViewModelAdapter.getResourceString("string_1467"));
                            });
                    }).fail((errors: Model.Entities.Error[]) => {
                        // Retrieving the registers and devices failed.
                        this.displayErrorControl(errors, Commerce.ViewModelAdapter.getResourceString("string_1466"));
                    });
            }
        }

        /**    
         * Start the guided activation process over.
         */
        public startOverHandler(): void {
            ViewModelAdapter.navigate(Helpers.DeviceActivationHelper.GUIDED_ACTIVATION_VIEW_NAME);
        }

        /**   
         * Add new device handler.
         */
        public addNewDeviceHandler(): void {
            this.addNewDeviceSelected(true);
        }

        /**   
         * Navigate to manual activation view.
         */
        public navigateToManualActivation(): void {
            // Update server Url in application storage before navigating
            this.viewModel.updateServerUrl();

            Commerce.ViewModelAdapter.navigate(Helpers.DeviceActivationHelper.DEVICE_ACTIVATION_VIEW_NAME);
        }

        /*
         * Gets the selected store's name for display.
         * @return {string} The selected store's display name.
         */
        public getSelectedStoreDisplayName(): string {
            var storeName: string = StringExtensions.EMPTY;

            if (!ObjectExtensions.isNullOrUndefined(this.selectedStore())) {
                storeName = this.selectedStore().OrgUnitName;
            }

            return storeName;
        }

        /*
         * Determines if we can try to activate the device.
         * @return {boolean} True if we can activate, false otherwise.
         */
        private canActivate(): boolean {
            return !this.viewModel.cannotActivate(this.viewModel.serviceUrl(),
                this.operatorId, this.viewModel.password());
        }

        /**
         * Makes sure that the user is authenticated.
         * @return {IVoidAsyncResult} The async result.
         */
        private authenticateUser(): IVoidAsyncResult {
            this.showUserAuthenticating(true);
            return this.viewModel.authenticateUser().fail((errors: Model.Entities.Error[]) => {
                this.displayErrorControl(errors, Commerce.ViewModelAdapter.getResourceString("string_8044"));   // Logging in anonymously failed.
            }).always(() => {
                this.showUserAuthenticating(false);
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

        /**
         * Set the localized message of health check connection status.
         * @param {Observable<string>} observableIcon Icon that should be used for the status. 
         * @param {string} connectionTypeResource The string resource of connection type.
         * @param {Observable<string>} observable  The observable that shows the message.
         * @param {Entities.HealthCheckConnectivityStatus} newStatus The updated status.
         */
        private setConnectionStatusMessage(observableIcon: Observable<string>, connectionTypeResource: string,
            observable: Observable<string>, newStatus: Entities.HealthCheckConnectivityStatus): void {

            var connectionStatus: string;
            var iconClass: string = StringExtensions.EMPTY;
            var cssProgressClassName: string = "iconProgressInnerLoop";
            var cssSucceededClassName: string = "iconFieldCorrect";
            var cssErrorClassName: string = "iconError";
            var cssUnknownClassName: string = "iconWarning";
            this.isCheckInProgress(false);

            switch (newStatus) {
                case Entities.HealthCheckConnectivityStatus.Connecting:
                    // The status for both icons changed in one request. 
                    // We can use one observable to indicate that both statuses are in connecting state.
                    this.isCheckInProgress(true);
                    connectionStatus = ViewModelAdapter.getResourceString("string_8094"); // Connecting...
                    iconClass = cssProgressClassName;
                    break;
                case Entities.HealthCheckConnectivityStatus.Succeeded:
                    connectionStatus = ViewModelAdapter.getResourceString("string_8092"); // Succeeded
                    iconClass = cssSucceededClassName;
                    break;
                case Entities.HealthCheckConnectivityStatus.Failed:
                    connectionStatus = ViewModelAdapter.getResourceString("string_8093"); // Failed
                    iconClass = cssErrorClassName;
                    break;
                case Entities.HealthCheckConnectivityStatus.Unknown:
                    connectionStatus = ViewModelAdapter.getResourceString("string_8096"); // Unable to retrieve data
                    iconClass = cssUnknownClassName;
                    break;
                default:
                    connectionStatus = StringExtensions.EMPTY;
                    break;
            }

            var template: string = ViewModelAdapter.getResourceString("string_8088");
            var localizedMessage: string = StringExtensions.format(
                template, ViewModelAdapter.getResourceString(connectionTypeResource), connectionStatus);

            // update the connection status observable
            observable(localizedMessage);
            observableIcon(iconClass);
        }

        private resetHealthCheckStatus(): void {
            this.showHealthCheckStatus(false);
            this.activationViewModel.setAllHealthCheckStatuses(Entities.HealthCheckConnectivityStatus.None);
        }
    }
}