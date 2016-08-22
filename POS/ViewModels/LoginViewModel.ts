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

    import Entities = Proxy.Entities;

    export class LoginViewModel extends ViewModelBase {
        public deviceId: Observable<string>;
        public locationId: Observable<string>;
        public registerId: Observable<string>;
        public serviceUrl: Observable<string>;
        public storeId: Observable<string>;
        public forceActivate: boolean;
        public skipConnectivityOperation: boolean;

        public password: Observable<string>;
        public currentActivationOperation: Observable<Operations.IDeviceActivationOperation>;
        public totalOperationSteps: Observable<number>;
        public currentOperationStep: Observable<number>;

        // Hardware support for extended logon
        // public swipeParser: Commerce.Payments.MSRKeyboardSwipeParser;

        constructor(options?: any) {
            super();

            // prepopulate values from arguments, if values are not defined yet
            if (StringExtensions.isNullOrWhitespace(ApplicationStorage.getItem(ApplicationStorageIDs.DEVICE_ID_KEY))) {
                ApplicationStorage.setItem(
                    ApplicationStorageIDs.DEVICE_ID_KEY,
                    Host.instance.configurationProvider.getArgumentValue(ApplicationArgumentId.DEVICE_NUMBER) || "");
            }

            if (StringExtensions.isNullOrWhitespace(ApplicationStorage.getItem(ApplicationStorageIDs.REGISTER_ID_KEY))) {
                ApplicationStorage.setItem(
                    ApplicationStorageIDs.REGISTER_ID_KEY,
                    Host.instance.configurationProvider.getArgumentValue(ApplicationArgumentId.TERMINAL_NUMBER) || "");
            }

            // Server needs to provide registerNumber if this field is left blank by the user
            this.deviceId = ko.observable(ApplicationStorage.getItem(ApplicationStorageIDs.DEVICE_ID_KEY)
                || Commerce.Config.demoModeDeviceId);

            this.registerId = ko.observable(ApplicationStorage.getItem(ApplicationStorageIDs.REGISTER_ID_KEY)
                || Commerce.Config.demoModeTerminalId);

            this.password = ko.observable(Commerce.Config.demoModePassword);
            this.serviceUrl = ko.observable(Commerce.Config.retailServerUrl);
            this.storeId = ko.observable(ApplicationStorage.getItem(ApplicationStorageIDs.STORE_ID_KEY));
            this.locationId = ko.observable(ApplicationStorage.getItem(ApplicationStorageIDs.STORE_ID_KEY) + ": "
                + ApplicationStorage.getItem(ApplicationStorageIDs.REGISTER_ID_KEY));

            this.currentActivationOperation = ko.observable(null);
            this.currentOperationStep = ko.observable(0);
            this.totalOperationSteps = ko.observable(0);
            this.forceActivate = false;
        }

        /**
         * Cleans and updates the server Url in application storage.
         */
        public updateServerUrl(): void {
            // Clean Uri before update.
            this.serviceUrl(StringExtensions.CleanUri(this.serviceUrl()));

            // Save Url in application context
            ApplicationContext.updateServerUrl(this.serviceUrl());
        }

        /**
         * Activates the device.
         * @return {IVoidAsyncResult} The async result.
         */
        public activateDevice(operatorId: string): IVoidAsyncResult {
            if (this.cannotActivate(this.serviceUrl(), operatorId, this.password())) {
                return VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.DEVICE_ACTIVATION_DETAILS_NOT_SPECIFIED)]);
            }

            // make sure server URL is correct before making any RS calls
            this.updateServerUrl();

            Helpers.DeviceActivationHelper.startDeviceActivation();
            return this.runDeviceActivationSequence(operatorId).done(() => {
                Helpers.DeviceActivationHelper.completeDeviceActivation();
            });
        }

        /**
         * Deactivates the device.
         * @return {IVoidAsyncResult} The async result.
         */
        public deactivateDevice(): IVoidAsyncResult {
            if (!this.canLogOff()) {
                return AsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.DEVICE_DEACTIVATION_INCOMPLETE_TRANSACTION_ERROR)]);
            }

            var options: Operations.IDeactivateDeviceOperationOption = {};
            return this.operationsManager.runOperation(Operations.RetailOperation.DeactivateDevice, options);
        }

        /**
         * Logs on the operator.
         * @param {string} operatorId The identifier of the operator to log on.
         * @param {string} password The password.
         * @param {string} [extendedCredentials] The identifier of the operator to log on.
         * @param {string} [grantType] The identifier of the operator to log on.
         * @return {IVoidAsyncResult} The async result of the logon workflow.
         */
        public operatorLogOn(operatorId: string, password: string, extendedCredentials?: string, grantType?: string): IVoidAsyncResult {
            var asyncQueue: AsyncQueue = new AsyncQueue();

            asyncQueue.enqueue((): IVoidAsyncResult => {
                var options: Triggers.IPreLogOnTriggerOptions = { operatorId: operatorId };
                var preTriggerResult: IAsyncResult<ICancelableResult> =
                    Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PreLogOn, options);

                return asyncQueue.cancelOn(preTriggerResult);
            }).enqueue((): IVoidAsyncResult => {
                return this.operatorLogOnInternal(operatorId, password, extendedCredentials, grantType);
            }).enqueue((): IVoidAsyncResult => {
                var options: Triggers.IPostLogOnTriggerOptions = {
                    employee: Session.instance.CurrentEmployee
                };

                return Triggers.TriggerManager.instance.execute(Triggers.NonCancelableTriggerType.PostLogOn, options);
            });

            return asyncQueue.run();
        }

        /**
         * Logs off the operator.
         * @param {any} callerContext - The callback context
         * @return {IVoidAsyncResult} The async result.
         */
        public logOff(callerContext: any): IVoidAsyncResult {
            var queue: AsyncQueue = new AsyncQueue();

            queue.enqueue(() => {
                // user must always be able to logoff
                return Commerce.Operations.OperationsManager.instance.runOperationWithoutPermissionsCheck(
                    Commerce.Operations.RetailOperation.LogOff,
                    this);
            });

            queue.enqueue(() => {
                var userSessionId: string = TsLogging.LoggerBase.getUserSessionId();
                RetailLogger.logoff(userSessionId);
                TsLogging.LoggerBase.clearUserSession();

                // Revert to use the store language.
                if (!StringExtensions.isNullOrWhitespace(Commerce.ApplicationContext.Instance.deviceConfiguration.CultureName)) {
                    return ViewModelAdapter.setApplicationLanguageAsync(Commerce.ApplicationContext.Instance.deviceConfiguration.CultureName)
                        .done(() => {
                            var serializedStoreCustomUIStrings: string =
                                ApplicationStorage.getItem(ApplicationStorageIDs.CUSTOM_UI_STRINGS_KEY);

                            if (!StringExtensions.isNullOrWhitespace(serializedStoreCustomUIStrings)) {
                                ApplicationContext.Instance.customUIStrings = JSON.parse(serializedStoreCustomUIStrings);
                            }
                        });
                } else {
                    // revert to use the first language from the applications list.
                    return ViewModelAdapter.setApplicationLanguageAsync(Commerce.ViewModelAdapter.getDefaultUILanguage()).done(() => {
                        this.channelManager.getCustomUIStrings(Commerce.ViewModelAdapter.getDefaultUILanguage())
                            .done((customStrings: Proxy.Entities.LocalizedString[]) => {
                                ApplicationContext.Instance.customUIStrings = customStrings;
                            });
                    });
                }
            });

            return queue.run();
        }

        /**
         * Unlocks the terminal.
         * @param {string} operatorID The operator id.
         * @param {string} [extendedCredentials] The identifier of the operator to log on.
         * @param {string} [grantType] The identifier of the operator to log on.
         * @return {IVoidAsyncResult} The async result.
         */
        public unlockRegister(operatorID: string, extendedCredentials?: string, grantType?: string): IVoidAsyncResult {
            var asyncQueue: AsyncQueue = new AsyncQueue();

            var isExtendedLogon: boolean = !ObjectExtensions.isNullOrUndefined(extendedCredentials);

            if (!isExtendedLogon) {
                if (StringExtensions.isEmptyOrWhitespace(this.password())) {
                    var retailErrors: Proxy.Entities.Error[] = <Proxy.Entities.Error[]>[
                        new Proxy.Entities.Error(ErrorTypeEnum.OPERATOR_PASSWORD_NOT_SPECIFIED)
                    ];
                    return VoidAsyncResult.createRejected(retailErrors);
                }
            }

            asyncQueue.enqueue(() => {
                return Commerce.Utilities.LogonHelper.resourceOwnedPasswordLogon(
                    operatorID,
                    this.password(),
                        /*operationId:*/ null,
                        /*doNotUpdateSession:*/ true,
                        extendedCredentials,
                        grantType);
            }).enqueue((): IVoidAsyncResult => {
                return this.operatorManager.getCurrentEmployeeAsync()
                    .done((employee: Proxy.Entities.Employee): void => {
                        Commerce.Session.instance.CurrentEmployee = employee;
                        Commerce.Session.instance.isSessionStateValid = true;

                        // Update user picture
                        // This originally happens during load channel configuration at logon, but must be done again when the picture is reset here
                        if (!ObjectExtensions.isNullOrUndefined(Commerce.ApplicationContext.Instance.channelRichMediaBaseURL
                            && (Session.instance.connectionStatus === ConnectionStatusType.Online)
                            && !Core.RegularExpressionValidations.validateUrl(Commerce.Session.instance.picture()))) {

                            Commerce.Session.instance.picture(Commerce.ApplicationContext.Instance.channelRichMediaBaseURL +
                                Commerce.Session.instance.picture());
                        }
                    })
                    .fail((errors: Entities.Error[]) => {
                        Commerce.Utilities.LogonHelper.logoff();
                    });
            });

            return asyncQueue.run();
        }

        /**
         * Reset Password.
         * @param {string} targetUserId The staff identifier whose password has to be changed.
         * @param {string} newPassword The new password.
         * @param {boolean} changePassword If the password should be changed or not.
         * @return {IVoidAsyncResult} The async result.
         */
        public resetPassword(targetUserId: string, newPassword: string, changePassword: boolean): IVoidAsyncResult {
            var options: Operations.IResetPasswordOperationOptions = {
                targetUserId: targetUserId,
                newPassword: newPassword,
                changePassword: changePassword
            };

            return Commerce.Operations.OperationsManager.instance.runOperation(
                Commerce.Operations.RetailOperation.ResetPassword, options);
        }

        /**
         * Initialize offline data sync.
         */
        public initializeOfflineDataSync(): void {
            // Enable offline data sync if offline connection string exists in mPOS config file and no existing offline sync.
            if (Commerce.Utilities.OfflineHelper.isOfflineEnabled()
                && Session.instance.offlineParameters.syncDownloadOfflineData === 0 && Session.instance.offlineParameters.syncUploadOfflineData === 0
                && Session.instance.offlineSyncing() === false) {
                AsyncServiceViewModel.execute();
            }
        }

        /**
         * Checks if errors has token validation exception.
         * @param {Proxy.Entities.Error[]} errors List of error messages.
         * @returns {boolean} True if errors contain token validation error, false otherwise.
         */
        public tokenValidationErrorExists(errors: Proxy.Entities.Error[]): boolean {
            if (ArrayExtensions.hasElements(errors) && !StringExtensions.isNullOrWhitespace(errors[0].ErrorCode)) {
                var errorCode: string = errors[0].ErrorCode.toUpperCase();

                if (errorCode === ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DEVICETOKENVALIDATIONFAILED.serverErrorCode) {
                    return true;
                }
            }

            return false;
        }

        /**
         * Validates whether device can be activated.
         * @param {string} serverUrl The server URL.
         * @param {string} [operatorId] The operator identifier.
         * @param {string} [password] The operator password.
         * @returns {boolean} True if device can be activated, false otherwise.
         */
        public cannotActivate(serverUrl: string, operatorId?: string, password?: string): boolean {
            var serverUrlVisible: boolean = (!Commerce.Config.aadEnabled && Commerce.StringExtensions.isNullOrWhitespace(Commerce.Config.onlineDatabase)
                || (Commerce.Config.aadEnabled && !Commerce.Config.locatorServiceEnabled));

            var validServerUrl: boolean = !serverUrlVisible || !StringExtensions.isNullOrWhitespace(serverUrl);

            return !validServerUrl
                || (!Config.aadEnabled && StringExtensions.isNullOrWhitespace(operatorId))
                || (!Config.aadEnabled && StringExtensions.isNullOrWhitespace(password));
        }

        /**
         * Enforces that user is authenticated.
         * @remarks This call might navigate away from the current view.
         * @returns {IVoidAsyncResult} a promise for the completion of the operation.
         */
        public authenticateUser(): IVoidAsyncResult {
            if (Commerce.Config.aadEnabled) {
                var operationState: Proxy.Entities.IDeviceActivationState = this.createDeviceActivationState();
                var aadAuthenticationOperation: Commerce.Operations.AADLogonOperation = new Commerce.Operations.AADLogonOperation(operationState);
                return aadAuthenticationOperation.operationProcess()();
            }

            return VoidAsyncResult.createResolved();
        }

        /**
         * Checks whether the employee can log off.
         */
        private canLogOff(): boolean {
            return !Session.instance.isCartInProgress;
        }

        /**
         * Internal implementation to logon the operator.
         * @param {string} operatorId The id of the operator to log on.
         * @param {string} password The password.
         * @param {string} [extendedCredentials] The id of the operator to log on.
         * @param {string} [grantType] The id of the operator to log on.
         * @return {IVoidAsyncResult} The async result of the logon attempt.
         */
        private operatorLogOnInternal(operatorId: string, password: string, extendedCredentials?: string, grantType?: string)
                    : IVoidAsyncResult {
            var asyncResult: VoidAsyncResult = new VoidAsyncResult();
            var asyncQueue: AsyncQueue = new AsyncQueue();
            var retailError: Proxy.Entities.Error[];

            var isExtendedLogon: boolean = !ObjectExtensions.isNullOrUndefined(extendedCredentials);

            if (!isExtendedLogon) {
                if ((StringExtensions.isEmptyOrWhitespace(operatorId) || StringExtensions.isEmptyOrWhitespace(password))) {
                    retailError = [new Proxy.Entities.Error(ErrorTypeEnum.OPERATOR_ID_PASSWORD_NOT_SPECIFIED)];
                    this.notifyErrorAsync(retailError, asyncResult);
                    return asyncResult;
                }
            }

            var deviceToken: string = ApplicationStorage.getItem(ApplicationStorageIDs.DEVICE_TOKEN_KEY);
            var deviceId: string = ApplicationStorage.getItem(ApplicationStorageIDs.DEVICE_ID_KEY);

            if (deviceToken == null || deviceId == null) {
                RetailLogger.viewModelLoginRetrieveDeviceInformationFailed();
                retailError = [
                    new Proxy.Entities.Error(ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DEVICETOKENVALIDATIONFAILED.serverErrorCode)
                ];
                this.notifyErrorAsync(retailError, asyncResult);
            } else {

                asyncQueue.enqueue(() => {
                    return Commerce.Utilities.LogonHelper.resourceOwnedPasswordLogon(
                        operatorId,
                        password,
                        /*operationId:*/ null,
                        /*doNotUpdateSession:*/ null,
                        extendedCredentials,
                        grantType);
                }).enqueue((): IVoidAsyncResult => {
                    // starts a new session
                    return this.authenticationManager.startSessionAsync();
                }).enqueue((): IVoidAsyncResult => {
                    return this.operatorManager.getCurrentEmployeeAsync().done((employee: Proxy.Entities.Employee): void => {
                        Commerce.Session.instance.CurrentEmployee = employee;
                        Commerce.Session.instance.isSessionStateValid = true;
                    });
                });

                var appContextLoader: Commerce.ApplicationContextLoader = new Commerce.ApplicationContextLoader();

                asyncQueue.enqueue(() => {
                    return appContextLoader.loadChannelConfiguration()
                        .done(() => {
                            this.onApplicationContextLoad(asyncResult);
                        })
                        .fail(() => {
                            // Switch to online if loading channel configuration failed in offline mode.
                            Commerce.Session.instance.connectionStatus = ConnectionStatusType.Online;
                        });
                });

                // Open line display device and display customer text
                if (Peripherals.instance && Peripherals.instance.lineDisplay) {
                    asyncQueue.enqueue(() => {
                        var result: VoidAsyncResult = new VoidAsyncResult(null);
                        Peripherals.instance.lineDisplay.openDevice().done(() => {
                            Peripherals.HardwareStation.LineDisplayHelper.displayIdleText();
                            result.resolve();
                        }).fail((errors: Proxy.Entities.Error[]) => {
                            Commerce.NotificationHandler.displayClientErrors(errors).done(() => {
                                result.resolve(); // NOTE: line display issues shouldn't prevent logon
                            });
                        });
                        return result;
                    });
                }

                asyncQueue.run().done(() => {
                    var userSessionId: string = TsLogging.Utils.generateGuid();
                    TsLogging.LoggerBase.setUserSession(userSessionId, Commerce.Session.instance.CurrentEmployee.StaffId);
                    RetailLogger.logon(userSessionId);

                    // Update application version if the version has changed and we're in online mode
                    if (Commerce.Session.instance.connectionStatus === ConnectionStatusType.Online) {
                        var previousAppVersion: string = ApplicationStorage.getItem(ApplicationStorageIDs.APPLICATION_VERSION);
                        var currentAppVersion: string = Commerce.ViewModelAdapter.getApplicationVersion();
                        if (previousAppVersion !== currentAppVersion) {
                            // Update retail server
                            this.storeOperationsManager.updateApplicationVersion(currentAppVersion).done(() => {
                                // Update application storage
                                ApplicationStorage.setItem(ApplicationStorageIDs.APPLICATION_VERSION, currentAppVersion);
                            });
                        }
                    }
                    asyncResult.resolve();
                }).fail((error: Proxy.Entities.Error[]) => {
                    // Don't log error if it's an invalid authentication credentials error - this is logged in authentication error handler
                    if (StringExtensions.compare(error[0].ErrorCode,
                        ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDAUTHENTICATIONCREDENTIALS.serverErrorCode, true)) {
                        RetailLogger.viewModelLoginFailed(error[0].ErrorCode, ErrorHelper.formatErrorMessage(error[0]));
                    }
                    this.notifyErrorAsync(error, asyncResult);
                });
            }
            return asyncResult;
        }

        /**
         * Calls the error callback with the given error.
         * @param {Proxy.Entities.Error[]} error The error.
         * @param {VoidAsyncResult} [resultToReject] The async result ot reject after the message was shown.
         * @return {IVoidAsyncResult} The async result of the notify error.
         */
        private notifyErrorAsync(errors: Proxy.Entities.Error[], resultToReject?: VoidAsyncResult): IVoidAsyncResult {
            var error: Proxy.Entities.Error = errors[0];
            var result: IVoidAsyncResult = VoidAsyncResult.createResolved();

            // If the device token is invalid, don't show an error since we handle this case in LoginView by navigating to device activation
            // Also, don't show an error if failed with password expired, as this is handled in LoginView by navigating to change password view
            if (StringExtensions.compare(error.ErrorCode,
                ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DEVICETOKENVALIDATIONFAILED.serverErrorCode, true)
                && StringExtensions.compare(error.ErrorCode,
                    ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_USERPASSWORDEXPIRED.serverErrorCode, true)
                && !ErrorHelper.hasError(errors,
                    Proxy.Entities.SecurityErrors[Proxy.Entities.SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_PasswordRequired])) {
                // Login view error
                result = Commerce.NotificationHandler.displayClientErrors(errors, "string_506");
            }

            if (!ObjectExtensions.isNullOrUndefined(resultToReject)) {
                result.always(() => {
                    resultToReject.reject(errors);
                });
            }

            return result;
        }

        private onApplicationContextLoad(asyncResult: VoidAsyncResult): void {

            var setAppLanguageResult: IVoidAsyncResult;
            var getProductDetailsResult: IVoidAsyncResult;

            // Update UI to use the operator language - store language has already been applied with initial sync.
            if (!StringExtensions.isNullOrWhitespace(Commerce.Session.instance.CurrentEmployee.CultureName) &&
                Session.instance.CurrentEmployee.CultureName !== ApplicationContext.Instance.deviceConfiguration.CultureName) {

                setAppLanguageResult = ViewModelAdapter.setApplicationLanguageAsync(Commerce.Session.instance.CurrentEmployee.CultureName)
                    .done(() => {
                        this.channelManager.getCustomUIStrings(Commerce.Session.instance.CurrentEmployee.CultureName)
                            .done((customStrings: Proxy.Entities.LocalizedString[]) => {
                                ApplicationContext.Instance.customUIStrings = customStrings;
                            });
                    });
            } else {
                setAppLanguageResult = VoidAsyncResult.createResolved();
            }

            // Load the previous transaction/cart if it was not closed/completed
            var cart: Proxy.Entities.Cart = Session.instance.cart;
            if (!StringExtensions.isNullOrWhitespace(cart.Id)) {
                // Since we are showing the product information in the minicart, we need to get this on login.
                var cartLineProductIds: number[] = [];
                cart.CartLines.forEach((cartLine: Proxy.Entities.CartLine) => {
                    cartLineProductIds.push(cartLine.ProductId);
                });

                var channelId: number = Session.instance.productCatalogStore.Context.ChannelId;
                getProductDetailsResult = this.productManager.getByIdsAsync(cartLineProductIds, channelId)
                    .done((products: Proxy.Entities.SimpleProduct[]) => {
                        if (ArrayExtensions.hasElements(products)) {
                            products.forEach((product: Proxy.Entities.SimpleProduct, index: number) => {
                                Session.instance.addToProductsInCartCache(product);
                            });
                        }
                    });
            } else {
                getProductDetailsResult = VoidAsyncResult.createResolved();
            }

            VoidAsyncResult.join([setAppLanguageResult, getProductDetailsResult])
                .done(() => {
                    asyncResult.resolve();
                }).fail((errors: Proxy.Entities.Error[]) => {
                    RetailLogger.viewModelLoginFailed(errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
                    asyncResult.resolve();
                });
        }

        /**
         * Executes device activation sequence.
         * @param {string} operatorId The operator identifier.
         * @returns {IVoidAsyncResult} The async result.
         */
        private runDeviceActivationSequence(operatorId: string): IVoidAsyncResult {
            var stepName: string = ObjectExtensions.isNullOrUndefined(this.currentActivationOperation()) ?
                StringExtensions.EMPTY : this.currentActivationOperation().operationName();

            var operationState: Proxy.Entities.IDeviceActivationState = this.createDeviceActivationState(operatorId);

            // save register id and device id to avoid having the user to retype it
            ApplicationStorage.setItem(ApplicationStorageIDs.REGISTER_ID_KEY, this.registerId());
            ApplicationStorage.setItem(ApplicationStorageIDs.DEVICE_ID_KEY, this.deviceId());

            var activationSequence: Operations.DeviceActivationSequence =
                new Operations.DeviceActivationSequence(operationState, stepName);
            var operations: Operations.IDeviceActivationOperation[] = activationSequence.constructActivationSequence();

            this.totalOperationSteps(operations.length);
            this.currentOperationStep(0);
            var asyncQueue: AsyncQueue = new AsyncQueue();

            for (var i: number = 0; i < operations.length; i++) {
                asyncQueue.enqueue(operations[i].preOperation());
                asyncQueue.enqueue(operations[i].validateState());
                asyncQueue.enqueue(operations[i].operationProcess());
            }

            return asyncQueue.run();
        }

        private createDeviceActivationState(operatorId?: string): Proxy.Entities.IDeviceActivationState {
            var operationState: Proxy.Entities.IDeviceActivationState = {
                serviceUrl: this.serviceUrl(),
                operatorId: operatorId,
                password: this.password(),
                deviceId: this.deviceId(),
                registerId: this.registerId(),
                currentOperation: this.currentActivationOperation,
                currentOperationStep: this.currentOperationStep,
                forceActivate: this.forceActivate,
                skipConnectivityOperation: this.skipConnectivityOperation
            };

            return operationState;
        }
    }
}