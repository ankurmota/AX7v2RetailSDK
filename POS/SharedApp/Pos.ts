/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path="Core/Navigator.ts"/>
///<reference path="Commerce.Core.d.ts"/>

/* BEGIN SDKSAMPLE_STOREHOURS (do not remove this)
///<reference path='Custom.Extension.d.ts'/>
   END SDKSAMPLE_STOREHOURS (do not remove this) */

/* BEGIN SDKSAMPLE_CROSSLOYALTY (do not remove this)
///<reference path='Custom.Extension.d.ts'/>
   END SDKSAMPLE_CROSSLOYALTY (do not remove this) */

///<reference path="Commerce.ViewModels.d.ts"/>
///<reference path="Pos.config.ts"/>

module Pos {
    "use strict";

    interface IApplicationState {
        valid: boolean;
        shouldExit?: boolean;
        errorMessage?: string;
    }

    import Proxy = Commerce.Proxy;
    import Triggers = Commerce.Triggers;

    WinJS.Application.onactivated = (args: WinJS.Application.IPromiseEvent): void => {

        Commerce.Proxy.Common.XmlHttpRequestHelper.SetupAjaxParameters();
        Commerce.Proxy.Common.XmlHttpRequestHelper.SetupODataParameters();

        Commerce.Host.instance.initializeAsync(initializeConfiguration).done((): void => {

            checkActivationArguments();

            if (Commerce.Host.instance.application.getApplicationType() !== Commerce.Proxy.Entities.ApplicationTypeEnum.CloudPos) {
                var config: any = Commerce.ApplicationStorage.getItem(Commerce.ApplicationStorageIDs.ENVIRONMENT_CONFIGURATION_KEY);

                if (config !== null) {
                    var environmentConfiguration: Commerce.Proxy.Entities.EnvironmentConfiguration = JSON.parse(config);
                    TsLogging.LoggerBase.setInstrumentationKey(environmentConfiguration.ClientAppInsightsInstrumentationKey);
                    TsLogging.LoggerBase.setTenantInfo(environmentConfiguration.EnvironmentId);
                    Commerce.RetailLogger.applicationLoadEnvironmentConfigurationStorageLoadSucceeded(config);
                }
            }

            // Explicitly set the user session to empty guid and user id to empty string.
            TsLogging.LoggerBase.setUserSession(TsLogging.Utils.emptyGuid(), "");

            var appSessionId: string = TsLogging.Utils.generateGuid();
            TsLogging.LoggerBase.setAppSessionId(appSessionId);
            var deviceNumber: any = Commerce.ApplicationStorage.getItem(Commerce.ApplicationStorageIDs.DEVICE_ID_KEY);
            var registerId: any = Commerce.ApplicationStorage.getItem(Commerce.ApplicationStorageIDs.REGISTER_ID_KEY);

            TsLogging.LoggerBase.setDeviceInfo(Commerce.Config.appHardwareId, deviceNumber, registerId);

            Commerce.RetailLogger.appLaunch(
                appSessionId,
                Commerce.Config.isDemoMode,
                Commerce.Config.appHardwareId,
                deviceNumber,
                Commerce.Proxy.Entities.ApplicationTypeEnum[Commerce.Host.instance.application.getApplicationType()],
                Commerce.Config.locatorServiceUrl,
                Commerce.Config.aadLoginUrl,
                Commerce.Config.aadClientId);

            if (!Commerce.Host.instance.application.isApplicationLaunching(args.detail.kind)) {
                return;
            }

            var applyCommonStylesResult: Commerce.IVoidAsyncResult = Commerce.CSSHelpers.loadAxRetailStylesheetAsync();
            var applyThemeResult: Commerce.IVoidAsyncResult = Commerce.CSSHelpers.applyThemeAsync();

            args.setPromise(WinJS.UI.processAll().then(() => {
                var startUpQueue: Commerce.AsyncQueue = new Commerce.AsyncQueue();

                startUpQueue.enqueue(() => {
                    return validateApplicationState().done((result: IApplicationState) => {
                        if (result.shouldExit) {
                            startUpQueue.cancel();
                        }

                        if (!result.valid) {
                            var messageResult: Commerce.VoidAsyncResult = new Commerce.VoidAsyncResult();
                            Commerce.VoidAsyncResult.join([applyCommonStylesResult, applyThemeResult]).always(() => {
                                var errorMessage: string = result.errorMessage;
                                if (!Commerce.StringExtensions.isNullOrWhitespace(errorMessage)) {
                                    messageResult.resolveOrRejectOn(Commerce.ViewModelAdapter.displayMessage(
                                        errorMessage,
                                        Commerce.MessageType.Info,
                                        Commerce.MessageBoxButtons.Default));
                                }
                            });
                            return messageResult;
                        }
                    });
                });

                startUpQueue.enqueue(() => {
                    Commerce.Config.retailServerUrl = Commerce.ApplicationStorage.getItem(Commerce.ApplicationStorageIDs.RETAIL_SERVER_URL);
                    Commerce.Model.Managers.Factory = new Commerce.Model.Managers.RetailServerManagerFactory(
                        Commerce.Config.retailServerUrl,
                        Commerce.ViewModelAdapter.getCurrentAppLanguage());

                    /* BEGIN SDKSAMPLE_STOREHOURS (do not remove this)
                    Commerce.Model.Managers.Factory = new Custom.Managers.ExtendedManagerFactory(
                        Commerce.Config.retailServerUrl,
                        Commerce.ViewModelAdapter.getCurrentAppLanguage());
                    END SDKSAMPLE_STOREHOURS (do not remove this) */

                    /* BEGIN SDKSAMPLE_CROSSLOYALTY (do not remove this)
                    Commerce.Model.Managers.Factory = new Custom.Managers.ExtendedManagerFactory(
                        Commerce.Config.retailServerUrl,
                        Commerce.ViewModelAdapter.getCurrentAppLanguage());
                    END SDKSAMPLE_CROSSLOYALTY (do not remove this) */

                    // Browsers limit the amount of data each domain is allowed to store in local and session storage. The data filters below will be used
                    // to filter out non-essential data in the event that the quota is surpassed.
                    Commerce.ApplicationStorage.registerNonEssentialDataFilter(
                        Commerce.ApplicationStorageIDs.DEVICE_CONFIGURATION_KEY,
                        (originalDeviceConfigurationData: string): string => {
                            // Data filter to remove background images from the device configuration deviceConfiguration.
                            var deviceConfiguration: Proxy.Entities.DeviceConfiguration =
                                new Proxy.Entities.DeviceConfigurationClass(JSON.parse(originalDeviceConfigurationData));
                            deviceConfiguration.LogOnBackgroundPictureAsBase64 = Commerce.StringExtensions.EMPTY;
                            deviceConfiguration.LogOnBackgroundPicturePortraitAsBase64 = Commerce.StringExtensions.EMPTY;
                            deviceConfiguration.BackgroundPictureAsBase64 = Commerce.StringExtensions.EMPTY;
                            return JSON.stringify(deviceConfiguration);
                        });

                    var initializePeripheralsResult: Commerce.IVoidAsyncResult = Commerce.Peripherals.instance.initializeAsync();

                    Commerce.Peripherals.HardwareStation.HardwareStationContext.instance.onError = hardwareStationOnError;

                    Commerce.UI.Tutorial.init();

                    var applicationStartTriggerResult: Commerce.VoidAsyncResult = new Commerce.VoidAsyncResult();
                    var initializationResults: Commerce.IVoidAsyncResult[] = [applyCommonStylesResult, applyThemeResult, initializePeripheralsResult];

                    var initializationSequenceResult: Commerce.IVoidAsyncResult = Commerce.VoidAsyncResult.join(initializationResults).always((): void => {
                        var options: Commerce.Triggers.IApplicationStartTriggerOptions = {};
                        Triggers.TriggerManager.instance.execute(Triggers.NonCancelableTriggerType.ApplicationStart, options).done((): void => {
                            applicationStartTriggerResult.resolve();
                        }).fail((errors: Commerce.Model.Entities.Error[]): void => {
                            applicationStartTriggerResult.reject(errors);
                        });
                    });

                    return Commerce.VoidAsyncResult.join([initializationSequenceResult, applicationStartTriggerResult]).done(() => {
                        onReady(args);
                    }).fail((errors: Commerce.Model.Entities.Error[]) => {
                        onReady(args);
                        Commerce.NotificationHandler.displayClientErrors(errors);
                    });
                });

                startUpQueue.run();
            }));
        });
    };

    function checkActivationArguments(): void {
        if (!Commerce.Helpers.DeviceActivationHelper.isDeviceActivationCompleted()
            && Commerce.Helpers.DeviceActivationHelper.areStoredDeviceTerminalDifferentFromArguments()) {
            Commerce.ApplicationStorage.clear();
        }
    }

    var hardwareStationErrorWorker: Commerce.AsyncWorkerQueue = new Commerce.AsyncWorkerQueue();

    function hardwareStationOnError(hardwareStation: Commerce.Proxy.Entities.IHardwareStation, errors: Commerce.Proxy.Entities.Error[]): void {
        hardwareStationErrorWorker.enqueue(() => {
            var workResult: Commerce.VoidAsyncResult = new Commerce.VoidAsyncResult();
            if (Commerce.Session.instance.getErrorDisplayState(Commerce.ErrorsDisplayedPerSession.HardwareStationGeneralError)) {
                var isPaymentException: boolean = false;
                if (errors !== null && errors.length > 0) {
                    for (var index: number = 0; index < errors.length; index++) {
                        if (errors[index].ErrorCode.indexOf(Commerce.PaymentErrorHelper.PAYMENT_EXCEPTION_NAMESPACE) > -1) {
                            isPaymentException = true;
                            break;
                        }
                    }
                }

                if (!isPaymentException) {
                    new Commerce.Controls.HardwareStationErrorDialog().show({ hardwareStation: hardwareStation, errors: errors })
                        .on(Commerce.DialogResult.OK, (value: boolean) => {
                            new Commerce.Activities.SelectHardwareStationActivity(<Commerce.Activities.SelectHardwareStationActivityContext>{}).execute();
                        }).onAny((result: boolean, dialogResult: Commerce.DialogResult) => {
                            Commerce.Session.instance.setErrorDisplayState(Commerce.ErrorsDisplayedPerSession.HardwareStationGeneralError, !result);
                            workResult.resolve();
                        });
                } else {
                    workResult.resolve();
                }
            } else {
                workResult.resolve();
            }
            return workResult;
        });
    }

    function validateApplicationState(): Commerce.IAsyncResult<IApplicationState> {
        var state: IApplicationState = {
            valid: true
        };

        if (!Commerce.ApplicationStorage.isLocalStorageSupported()) {
            state.errorMessage = Commerce.ViewModelAdapter.getResourceString(Commerce.ErrorTypeEnum.LOCAL_STORAGE_IS_NOT_AVAILABLE);
            Commerce.RetailLogger.applicationLocalStorageNotAvailable(state.errorMessage);
            state.valid = false;
            state.shouldExit = true;
        }

        if (Commerce.Host.instance.isApplicationUpdateRequired()) {
            state.errorMessage = Commerce.ViewModelAdapter.getResourceString(Commerce.ErrorTypeEnum.APPLICATION_UPDATE_REQUIRED);
            Commerce.RetailLogger.applicationUpdateIsRequired();
            state.valid = false;
            state.shouldExit = false;
        }

        var queue: Commerce.AsyncQueue = new Commerce.AsyncQueue();
        var ignoreUnsupportedBrowserValue: string = Commerce.ApplicationStorage.getItem(Commerce.ApplicationStorageIDs.IGNORE_UNSUPPORTED_BROWSER_ERROR);
        var ignoreUnsupportedBrowser: boolean = !Commerce.StringExtensions.isNullOrWhitespace(ignoreUnsupportedBrowserValue)
            && ignoreUnsupportedBrowserValue === true.toString();

        if (Commerce.Host.instance.application.getApplicationType() === Commerce.Proxy.Entities.ApplicationTypeEnum.CloudPos
            && !ignoreUnsupportedBrowser && state.valid && !state.shouldExit) {
            if (Commerce.ObjectExtensions.isNullOrUndefined(ignoreUnsupportedBrowserValue)) {

                var browserType: Commerce.Host.BrowserType = Commerce.Host.instance.application.getBrowserType();
                var isBrowserSupported: boolean = browserType === Commerce.Host.BrowserType.IE11
                    || browserType === Commerce.Host.BrowserType.Chrome
                    || browserType === Commerce.Host.BrowserType.Edge
                    || browserType === Commerce.Host.BrowserType.Phantom;
            // If the browser is not supported and the user hasn't been promted before during this session..
                if (!isBrowserSupported) {
                    queue.enqueue(() => {
                        return Commerce.ViewModelAdapter.displayMessage(Commerce.ViewModelAdapter
                            .getResourceString(Commerce.ErrorTypeEnum.BROWSER_IS_NOT_SUPPORTED),
                            Commerce.MessageType.Info,
                            Commerce.MessageBoxButtons.YesNo,
                            Commerce.ViewModelAdapter.getResourceString("string_29851")).done((result: Commerce.DialogResult) => {
                            if (result === Commerce.DialogResult.No) {
                                state.valid = false;
                                state.shouldExit = true;
                            }

                            Commerce.ApplicationStorage.setItem(Commerce.ApplicationStorageIDs.IGNORE_UNSUPPORTED_BROWSER_ERROR, state.valid.toString());
                        });
                    });
                }
            } else {
                state.valid = false;
                state.shouldExit = true;
            }
        }

        if (Commerce.Helpers.DeviceActivationHelper.isDeviceActivationCompleted()
            && Commerce.Helpers.DeviceActivationHelper.areStoredDeviceTerminalDifferentFromArguments()) {
            var pattern: string = Commerce.ViewModelAdapter.getResourceString(Commerce.ErrorTypeEnum.ACCESS_WRONG_DEVICE_TERMINAL);
            state.errorMessage = Commerce.StringExtensions.format(pattern,
                Commerce.Helpers.DeviceActivationHelper.argumentDeviceNumber,
                Commerce.Helpers.DeviceActivationHelper.argumentRegisterNumber,
                Commerce.Helpers.DeviceActivationHelper.storedDeviceNumber,
                Commerce.Helpers.DeviceActivationHelper.storedRegisterNumber);
            Commerce.RetailLogger.accessWrongDeviceTerminal();
            state.valid = false;
            state.shouldExit = false;
        }

        return queue.run().map<IApplicationState>(() => state);
    }

    function initializeConfiguration(configurationProvider: Commerce.Host.IConfigurationProvider): void {
        Commerce.attachLoggingSink(new Microsoft.Dynamics.Diagnostics.TypeScriptCore.DebuggingConsoleSink());

        // retrieve logging related configuration properties.
        if (configurationProvider.getValue<string>("IsNativeLoggingEnabled") === "true") {
            Commerce.attachLoggingSink(new Microsoft.Dynamics.Diagnostics.TypeScriptCore.WindowsLoggingRequest());
        }

        if (configurationProvider.getValue<string>("IsAppInsightsLoggingEnabled") === "true") {
            var appInsightsInstrumentationKey: string = configurationProvider.getValue<string>("AppInsightsInstrumentationKey");
            var appInsightsApplicatioName: string = configurationProvider.getValue<string>("AppInsightsApplicationName");
            var environmentId: string = configurationProvider.getValue<string>("EnvironmentId");

            Commerce.Config.appName = appInsightsApplicatioName;
            TsLogging.LoggerBase.setTenantInfo(environmentId);

            Commerce.attachLoggingSink(
                new Microsoft.Dynamics.Diagnostics.TypeScriptCore.AppInsightsSink(
                    appInsightsInstrumentationKey,
                    appInsightsApplicatioName,
                    Commerce.ViewModelAdapter.getApplicationVersion()));
        }

        if (configurationProvider.getValue<string>("IsDemoModeEnabled") === "true") {
            Commerce.Config.isDemoMode = true;
            Commerce.Config.demoModeDeviceId = configurationProvider.getValue<string>("DemoModeDeviceId");
            Commerce.Config.demoModeTerminalId = configurationProvider.getValue<string>("DemoModeTerminalId");
            Commerce.Config.demoModeStaffId = configurationProvider.getValue<string>("DemoModeStaffId");
            Commerce.Config.demoModePassword = configurationProvider.getValue<string>("DemoModePassword");
        }

        // retrieve connection strings from config.
        Commerce.Config.onlineDatabase = configurationProvider.getValue<string>("OnlineConnectionString");
        Commerce.Config.offlineDatabase = configurationProvider.getValue<string>("OfflineConnectionString");

        // configuration for AAD & locator service
        Commerce.Config.locatorServiceUrl = configurationProvider.getValue<string>("LocatorServiceUrl");
        Commerce.Config.locatorServiceEnabled = !Commerce.StringExtensions.isNullOrWhitespace(Commerce.Config.locatorServiceUrl);
        Commerce.Config.aadLoginUrl = configurationProvider.getValue<string>("AADLoginUrl");
        Commerce.Config.aadClientId = configurationProvider.getValue<string>("AADClientId");
        Commerce.Config.aadRetailServerResourceId = configurationProvider.getValue<string>("AADRetailServerResourceId");
        Commerce.Config.aadEnabled = !Commerce.StringExtensions.isNullOrWhitespace(Commerce.Config.aadLoginUrl);

        Commerce.Config.persistentRetailServerUrl = configurationProvider.getValue<string>("RetailServerUrl");
        Commerce.Config.persistentRetailServerEnabled = !Commerce.StringExtensions.isNullOrWhitespace(Commerce.Config.persistentRetailServerUrl);

        var sqlCommandTimeout: number = Number(configurationProvider.getValue<string>("SqlCommandTimeout"));

        if (sqlCommandTimeout >= 0) {
            Commerce.Config.sqlCommandTimeout = sqlCommandTimeout;
        }

        var commerceAudience: string = configurationProvider.getValue<string>("CommerceAuthenticationAudience");
        if (!Commerce.StringExtensions.isNullOrWhitespace(commerceAudience)) {
            Commerce.Config.commerceAuthenticationAudience = commerceAudience;
        }

        // retrieve default offline intervals from config.
        var defaultOfflineDownloadInterval: number = Number(configurationProvider.getValue<string>("DefaultOfflineDownloadInterval"));
        var defaultOfflineUploadInterval: number = Number(configurationProvider.getValue<string>("DefaultOfflineUploadInterval"));

        if (defaultOfflineDownloadInterval > 0) {
            Commerce.Config.defaultOfflineDownloadIntervalInMilliseconds = defaultOfflineDownloadInterval * 60000;
        }

        if (defaultOfflineUploadInterval > 0) {
            Commerce.Config.defaultOfflineUploadIntervalInMilliseconds = defaultOfflineUploadInterval * 60000;
        }

        var connectionTimeOut: number = Number(configurationProvider.getValue<string>("ConnectionTimeout"));

        if (connectionTimeOut > 0) {
            Commerce.Config.connectionTimeout = connectionTimeOut;
        }

        Commerce.Config.appHardwareId = Commerce.Host.instance.application.getAppSpecificHardwareId();

        // initialize the data source
        Commerce.NumberSequence.Enabled = true;
    }

    function setupDeveloperMode(): void {
        // debug settings. CSSHelpers.isCSSDeveloperMode() might return either true or false 
        // during Debug mode, but we want to bind the CTRL + D keypress handler even if isCSSDeveloperMode() is false.
        if (Commerce.Config.isDebugMode) {
            if (Commerce.CSSHelpers.isCSSDeveloperMode()) {
                // uf developer mode is set then set all developer mode defaults.
                Commerce.CSSHelpers.loadCSSDeveloperModePresets();
            }

            // hot key for developer settings menu
            $(window).keydown((event: any): void => {
                // ctrl + D
                if (event.keyCode === 68 && event.ctrlKey) {
                    WinJS.UI.SettingsFlyout.showSettings("developerModeSettingsFlyout", "Views/Controls/DeveloperMode.html");
                }
            });
        }
    }

    function navigateToStartPage(args: WinJS.Application.IPromiseEvent): void {
        args.setPromise(WinJS.UI.processAll()
            .then((): void => {
                // to track the user activity so that we can navigate back to login page after 30 minutes
                Commerce.UserActivityTracker.setUpUserActivity();
                Commerce.ViewModelAdapter.navigateToLoginPage(true);
            }));
    }

    function onReady(args: WinJS.Application.IPromiseEvent): void {
        if (checkWindowIsSafe()) {
            registerAuthenticationProviders();
            setupDeveloperMode();
            navigateToStartPage(args);
        }
    }

    function registerAuthenticationProviders(): void {
        var authenticationManager: Commerce.Authentication.AuthenticationProviderManager = Commerce.Authentication.AuthenticationProviderManager.instance;

        // Register Azure Active Directory for user authentication
        var addProvider: Commerce.Authentication.Providers.AzureActiveDirectoryUserAuthenticationProvider =
            new Commerce.Authentication.Providers.AzureActiveDirectoryUserAuthenticationProvider();
        authenticationManager.registerImplicitGrantProvider(
            addProvider,
            Commerce.Authentication.AuthenticationProviderResourceType.USER);

        // Register provider for locator service
        authenticationManager.registerImplicitGrantProvider(
            addProvider,
            Commerce.Authentication.AuthenticationProviderResourceType.LOCATOR_SERVICE);

        // Register Commerce Authentication for user authentication
        authenticationManager.registerResourceOwnerPasswordGrantProvider(
            new Commerce.Authentication.Providers.CommerceUserAuthenticationProvider(),
            Commerce.Authentication.AuthenticationProviderResourceType.USER);

        // Register provider for device
        authenticationManager.registerResourceOwnerPasswordGrantProvider(
            new Commerce.Authentication.Providers.DeviceAuthenticationProvider(),
            Commerce.Authentication.AuthenticationProviderResourceType.DEVICE);
    }

    function checkWindowIsSafe(): boolean {
        // only 2 situations are allowed:
        // 1) no frames at all (self === top)
        // 2) only one frame (parent === top) that shares same origin as parent / top (self origin === parent origin)
        var isWindowAllowed: boolean = window.self === window.top
            || (window.self.parent === window.top && (<any>window).self.location.origin === (<any>window).self.parent.location.origin);

        // if we are being framed, let user know
        if (!isWindowAllowed) {
            // we can't rely on the application to show the structured dialog, so we use alert for informing the user
            // about the problem
            alert(Commerce.ViewModelAdapter.getResourceString("string_29044"));
            var errorDetails: string = Commerce.StringExtensions.format(
                "Potential clickjacking attempt detected. Source origin: {0}.",
                (<any>top.location).origin);
            Commerce.RetailLogger.cloudPosBrowserNotSupported(navigator.userAgent, errorDetails);
        }

        return isWindowAllowed;
    }

    function setHighchartsGlobalOptions(): void {
        // NOTE: reset Highcharts urls to prevent it from accessing external resources in runtime.
        // The urls are for obsolete browsers (VML, Android 2+) which is out of scope for the product.
        Highcharts.setOptions({
            global: {
                VMLRadialGradientURL: null,
                canvasToolsURL: null
            }
        });
    }

    WinJS.Application.onerror = function (error: any): boolean {
        var errorMessage: string = "";
        var errorUrl: string = "";
        var stackTrace: string = "";
        var errorString: string = "";
        console.log(error);

        if (typeof (error.detail) === "object") {
            if (!Commerce.ObjectExtensions.isNullOrUndefined(error.detail.errorMessage)) {
                errorMessage = error.detail.errorMessage;
                errorUrl = error.detail.errorUrl || "";
            }

            if (!Commerce.ObjectExtensions.isNullOrUndefined(error.detail.error)) {
                var innerError: any = error.detail.error;
                stackTrace = Commerce.ObjectExtensions.isNullOrUndefined(innerError.stack) ? "" : innerError.stack;
                errorMessage = Commerce.ObjectExtensions.isNullOrUndefined(innerError.message) ? "" : innerError.message;
            }
        } else {
            errorMessage = error.detail || "";
        }

        try {
            errorString = JSON.stringify(error);
        } catch (ex) {
            // ignore errors, this was the last possible way to get all the info. 
            // rethrow will cause app be unresponsive.
            errorString = "";
        }

        Commerce.NotificationHandler.displayErrorMessage("string_29000"); // "Application error"

        Commerce.RetailLogger.appUnhandledError(errorMessage, stackTrace, errorUrl, errorString);
        return true;
    };

    // Execute the application suspend triggers when the application is suspended.
    WinJS.Application.oncheckpoint = function (args: any): void {
        var promise: WinJS.Promise<{}> = new WinJS.Promise(function (complete: any, error: any, progress: any): void {
            var options: Commerce.Triggers.IApplicationSuspendTriggerOptions = {};
            Commerce.Triggers.TriggerManager.instance.execute(Commerce.Triggers.NonCancelableTriggerType.ApplicationSuspend, options).always((): void => {
                complete();
            });
        });

        args.setPromise(promise);
    };

    setHighchartsGlobalOptions();
    WinJS.Application.start();
}

declare module Microsoft.Dynamics.Commerce.ClientBroker {
    class AppConfiguration {
        public static read(key: string): string;
    }
}