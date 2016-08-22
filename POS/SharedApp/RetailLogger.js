///<reference path='Diagnostics.TypeScriptCore.d.ts'/>
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") return Reflect.decorate(decorators, target, key, desc);
    switch (arguments.length) {
        case 2: return decorators.reduceRight(function(o, d) { return (d && d(o)) || o; }, target);
        case 3: return decorators.reduceRight(function(o, d) { return (d && d(target, key)), void 0; }, void 0);
        case 4: return decorators.reduceRight(function(o, d) { return (d && d(target, key, o)) || o; }, desc);
    }
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var __param = (this && this.__param) || function (paramIndex, decorator) {
    return function (target, key) { decorator(target, key, paramIndex); }
};
var TsLogging = Microsoft.Dynamics.Diagnostics.TypeScriptCore;
var Commerce;
(function (Commerce) {
    function attachLoggingSink(sink) {
        Microsoft.Dynamics.Diagnostics.TypeScriptCore.LoggerBase.addLoggingSink(sink);
    }
    Commerce.attachLoggingSink = attachLoggingSink;
    var RetailLogger = (function () {
        function RetailLogger() {
        }
        RetailLogger.writePageViewEvent = function (pageName) {
            TsLogging.LoggerBase.writePageViewEvent(pageName);
        };
        RetailLogger.genericError = function (message) {
            TsLogging.LoggerBase.writeEvent("GenericError", 40000, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "{0}");
        };
        RetailLogger.genericWarning = function (message) {
            TsLogging.LoggerBase.writeEvent("GenericWarning", 40001, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "{0}");
        };
        RetailLogger.genericInfo = function (message) {
            TsLogging.LoggerBase.writeEvent("GenericInfo", 40002, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Verbose, [], "", "", "{0}");
        };
        RetailLogger.extendedCritical = function (message, parameter1, parameter2, parameter3, parameter4, parameter5, parmater6) {
            if (parameter1 === void 0) { parameter1 = ""; }
            if (parameter2 === void 0) { parameter2 = ""; }
            if (parameter3 === void 0) { parameter3 = ""; }
            if (parameter4 === void 0) { parameter4 = ""; }
            if (parameter5 === void 0) { parameter5 = ""; }
            if (parmater6 === void 0) { parmater6 = ""; }
            TsLogging.LoggerBase.writeEvent("ExtendedCritical", 40005, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Critical, [], "", "", "{0}");
        };
        RetailLogger.extendedError = function (message, parameter1, parameter2, parameter3, parameter4, parameter5, parmater6) {
            if (parameter1 === void 0) { parameter1 = ""; }
            if (parameter2 === void 0) { parameter2 = ""; }
            if (parameter3 === void 0) { parameter3 = ""; }
            if (parameter4 === void 0) { parameter4 = ""; }
            if (parameter5 === void 0) { parameter5 = ""; }
            if (parmater6 === void 0) { parmater6 = ""; }
            TsLogging.LoggerBase.writeEvent("ExtendedError", 40006, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "{0}");
        };
        RetailLogger.extendedWarning = function (message, parameter1, parameter2, parameter3, parameter4, parameter5, parmater6) {
            if (parameter1 === void 0) { parameter1 = ""; }
            if (parameter2 === void 0) { parameter2 = ""; }
            if (parameter3 === void 0) { parameter3 = ""; }
            if (parameter4 === void 0) { parameter4 = ""; }
            if (parameter5 === void 0) { parameter5 = ""; }
            if (parmater6 === void 0) { parmater6 = ""; }
            TsLogging.LoggerBase.writeEvent("ExtendedWarning", 40007, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "{0}");
        };
        RetailLogger.extendedInformational = function (message, parameter1, parameter2, parameter3, parameter4, parameter5, parmater6) {
            if (parameter1 === void 0) { parameter1 = ""; }
            if (parameter2 === void 0) { parameter2 = ""; }
            if (parameter3 === void 0) { parameter3 = ""; }
            if (parameter4 === void 0) { parameter4 = ""; }
            if (parameter5 === void 0) { parameter5 = ""; }
            if (parmater6 === void 0) { parmater6 = ""; }
            TsLogging.LoggerBase.writeEvent("ExtendedInformational", 40008, 1, TsLogging.EventChannel.Admin, TsLogging.EventLevel.Informational, [], "", "", "{0}");
        };
        RetailLogger.extendedVerbose = function (message, parameter1, parameter2, parameter3, parameter4, parameter5, parmater6) {
            if (parameter1 === void 0) { parameter1 = ""; }
            if (parameter2 === void 0) { parameter2 = ""; }
            if (parameter3 === void 0) { parameter3 = ""; }
            if (parameter4 === void 0) { parameter4 = ""; }
            if (parameter5 === void 0) { parameter5 = ""; }
            if (parmater6 === void 0) { parmater6 = ""; }
            TsLogging.LoggerBase.writeEvent("ExtendedVerbose", 40009, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Verbose, [], "", "", "{0}");
        };
        RetailLogger.appLaunch = function (appSessionId, isDemoMode, deviceId, deviceNumber, applicationType, locatorServiceUrl, aadLoginUrl, aadClientId) {
            TsLogging.LoggerBase.writeEvent("AppLaunch", 40010, 1, TsLogging.EventChannel.Analytic, TsLogging.EventLevel.LogAlways, [], "", "", "The application has been launched.");
        };
        RetailLogger.appUnhandledError = function (errorMessage, stackTrace, errorUrl, errorJson) {
            TsLogging.LoggerBase.writeEvent("AppUnhandledError", 40011, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Unhandled App error: {0}; \n StackTrace: {1}; ErrorUrl: '{2}'; ErrorObject: '{3}'");
        };
        RetailLogger.logon = function (userSessionId) {
            TsLogging.LoggerBase.writeEvent("Logon", 40012, 1, TsLogging.EventChannel.Analytic, TsLogging.EventLevel.LogAlways, [], "", "", "User logged in.");
        };
        RetailLogger.logoff = function (userSessionId) {
            TsLogging.LoggerBase.writeEvent("Logoff", 40013, 1, TsLogging.EventChannel.Analytic, TsLogging.EventLevel.LogAlways, [], "", "", "User logged out.");
        };
        RetailLogger.userMessageDisplay = function (messageType, messageTitle, message) {
            TsLogging.LoggerBase.writeEvent("UserMessageDisplay", 40020, 1, TsLogging.EventChannel.Admin, TsLogging.EventLevel.Informational, [], "", "", "Message displayed to the user: title: '{1}'.");
        };
        RetailLogger.errorMessageDisplay = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("ErrorMessageDisplay", 40021, 1, TsLogging.EventChannel.Admin, TsLogging.EventLevel.Error, [], "", "", "Error displayed to the user: error message '{1}'.");
        };
        RetailLogger.applicationContextSetupLanguagesInvalidLanguage = function (languageId) {
            TsLogging.LoggerBase.writeEvent("ApplicationContextSetupLanguagesInvalidLanguage", 40050, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "ApplicationContext SetupLanguages Invalid language ID = '{0}'.");
        };
        RetailLogger.applicationStylesheetsLoadFailed = function (uri, errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("ApplicationStylesheetsLoadFailed", 40051, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "Application stylesheet has failed to load: uri = '{0}', error message '{2}'.");
        };
        RetailLogger.applicationLocalStorageNotAvailable = function (errorMessage) {
            TsLogging.LoggerBase.writeEvent("ApplicationLocalStorageNotAvailable", 40052, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "Application Local storage is not available: error message '{0}'.");
        };
        RetailLogger.applicationContextInvalidCatalogImageFormat = function () {
            TsLogging.LoggerBase.writeEvent("ApplicationContextInvalidCatalogImageFormat", 40053, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "ApplicationContext Invalid Catalog image format.");
        };
        RetailLogger.applicationContextApplicationContextEntitySetInvalid = function (entitySetId) {
            TsLogging.LoggerBase.writeEvent("ApplicationContextApplicationContextEntitySetInvalid", 40054, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "A method with ApplicationContextEntitySet ID '{0}' was reported.");
        };
        RetailLogger.applicationContextApplicationContextEntitySetMultipleTimes = function (entitySetId) {
            TsLogging.LoggerBase.writeEvent("ApplicationContextApplicationContextEntitySetMultipleTimes", 40055, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "A method with ApplicationContextEntitySet ID '{0}' was reported multiple times.");
        };
        RetailLogger.applicationContextApplicationContextEntitySetNoMethodNumber = function () {
            TsLogging.LoggerBase.writeEvent("ApplicationContextApplicationContextEntitySetNoMethodNumber", 40056, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "The value ApplicationContextEntitySet.All does not represent the number of methods that can be run");
        };
        RetailLogger.applicationFailedToParseError = function (error) {
            TsLogging.LoggerBase.writeEvent("ApplicationFailedToParseError", 40057, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Failed parse error message, error = '{0}'.");
        };
        RetailLogger.applicationFailedToParseErrorInvalidJson = function (error) {
            TsLogging.LoggerBase.writeEvent("ApplicationFailedToParseErrorInvalidJson", 40058, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Invalid JSON format from server, it was not possible to parse error message, error = '{0}'.");
        };
        RetailLogger.applicationGlobalizationResourcesEmpty = function () {
            TsLogging.LoggerBase.writeEvent("ApplicationGlobalizationResourcesEmpty", 40059, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Application Globalization Resources are empty.");
        };
        RetailLogger.applicationGlobalizationResourcesLoadFailed = function (languageTag, errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("ApplicationGlobalizationResourcesLoadFailed", 40060, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Application WebGlobalization Can not load resources for the '{0}' culture: error message = '{2}'.");
        };
        RetailLogger.applicationContextLoadCategoriesFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("ApplicationContextLoadCategoriesFailed", 40061, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Application Load categories failed: error message = '{1}'.");
        };
        RetailLogger.applicationLoadChannelConfigurationFailed = function (component, errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("ApplicationLoadChannelConfigurationFailed", 40062, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Application Load channel configuration failed: component = '{0}', error message = '{2}'.");
        };
        RetailLogger.applicationContextSetupDebitCashbackLimitFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("ApplicationContextSetupDebitCashbackLimitFailed", 40063, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "ApplicationContext Error when getting debit cashback limit: error message = '{1}'.");
        };
        RetailLogger.applicationContextSetupCardTypesFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("ApplicationContextSetupCardTypesFailed", 40064, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "ApplicationContext Error when getting list of card types: error message = '{1}'.");
        };
        RetailLogger.applicationContextSetupReturnOrderReasonCodesFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("ApplicationContextSetupReturnOrderReasonCodesFailed", 40065, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "ApplicationContext Error when getting list of return order reason codes: error message = '{1}'.");
        };
        RetailLogger.applicationContextSetupCustomerTypesFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("ApplicationContextSetupCustomerTypesFailed", 40066, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "ApplicationContext Error when getting list of customer types: error message = '{1}'.");
        };
        RetailLogger.applicationContextSetupCustomerGroupsFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("ApplicationContextSetupCustomerGroupsFailed", 40067, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "ApplicationContext Error when getting list of customer groups: error message = '{1}'.");
        };
        RetailLogger.applicationContextSetupHardwareStationProfileFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("ApplicationContextSetupHardwareStationProfileFailed", 40068, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "ApplicationContext Error when getting list of hardware station profiles: error message = '{1}'.");
        };
        RetailLogger.applicationContextSetupLanguagesFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("ApplicationContextSetupLanguagesFailed", 40069, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "ApplicationContext Error when getting list of available languages: error message = '{1}'.");
        };
        RetailLogger.applicationContextSetupReceiptOptionsFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("ApplicationContextSetupReceiptOptionsFailed", 40070, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "ApplicationContext Error when getting list of receipt options: error message = '{1}'.");
        };
        RetailLogger.applicationContextSetupCashDeclarationsFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("ApplicationContextSetupCashDeclarationsFailed", 40071, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "ApplicationContext Error when getting list of cash declarations: error message = '{1}'.");
        };
        RetailLogger.applicationGlobalizationResourcesLoading = function (languageTag) {
            TsLogging.LoggerBase.writeEvent("applicationGlobalizationResourcesLoading", 40072, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Verbose, [], "", "", "Application WebGlobalization loadResourcesAsync Loading string resources for the language tag '{0}'.");
        };
        RetailLogger.applicationGlobalizationResourcesLanguageResolved = function (languageTag) {
            TsLogging.LoggerBase.writeEvent("applicationGlobalizationResourcesLanguageResolved", 40073, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Verbose, [], "", "", "Application WebGlobalization loadResourcesAsync Requested language tag has been resolved to '{0}'.");
        };
        RetailLogger.applicationLoadEnvironmentConfigurationStorageLoadSucceeded = function (config) {
            TsLogging.LoggerBase.writeEvent("applicationLoadEnvironmentConfigurationStorageLoadSucceeded", 40074, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "The environment configuration was successfully loaded from the local storage and set on the app. Config: '{0}'.");
        };
        RetailLogger.applicationLoadEnvironmentConfigurationServerLoadSucceeded = function (config) {
            TsLogging.LoggerBase.writeEvent("applicationLoadEnvironmentConfigurationServerLoadSucceeded", 40075, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "The environment configuration was successfully loaded from the server and set on the app. Config: '{0}'.");
        };
        RetailLogger.applicationLoadEnvironmentConfigurationServerLoadFailed = function (errorMessage) {
            TsLogging.LoggerBase.writeEvent("applicationLoadEnvironmentConfigurationServerLoadFailed", 40076, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "There was an error loading the environment configuration from the server. Error: '{0}'.");
        };
        RetailLogger.applicationUpdateIsRequired = function () {
            TsLogging.LoggerBase.writeEvent("applicationUpdateIsRequired", 40077, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "Application update is required.");
        };
        RetailLogger.accessWrongDeviceTerminal = function () {
            TsLogging.LoggerBase.writeEvent("accessWrongDeviceTerminal", 40078, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "Access to the wrong device/terminal while the different one is being currently activated.");
        };
        RetailLogger.modelManagersRetailServerRequestStarted = function (requestId, requestUrl) {
            TsLogging.LoggerBase.writeEvent("ModelManagersRetailServerRequestStarted", 40100, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Verbose, [], "", "", "The Retail Server Request with request id '{0}' and request url '{1}' started.");
        };
        RetailLogger.modelManagersRetailServerRequestError = function (requestId, requestUrl, error) {
            TsLogging.LoggerBase.writeEvent("ModelManagersRetailServerRequestError", 40101, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "The Retail Server Request with request id '{0}' and request url '{1}' failed.  Error: {2}.");
        };
        RetailLogger.modelManagersRetailServerRequestFinished = function (requestId, requestUrl) {
            TsLogging.LoggerBase.writeEvent("ModelManagersRetailServerRequestFinished", 40102, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Verbose, [], "", "", "The Retail Server Request with request id '{0}' and request url '{1}' succeeded.");
        };
        RetailLogger.modelManagersCommerceRuntimeRequestStarted = function (requestId, requestUrl) {
            TsLogging.LoggerBase.writeEvent("ModelManagersCommerceRuntimeRequestStarted", 40103, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The Commerce Runtime Request with request id '{0}' and request url '{1}' started.");
        };
        RetailLogger.modelManagersCommerceRuntimeRequestError = function (requestId, requestUrl, error) {
            TsLogging.LoggerBase.writeEvent("ModelManagersCommerceRuntimeRequestError", 40104, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "The Commerce Runtime Request with request id '{0}' and request url '{1}' failed.  Error: {2}.");
        };
        RetailLogger.modelManagersCommerceRuntimeRequestFinished = function (requestId, requestUrl) {
            TsLogging.LoggerBase.writeEvent("ModelManagersCommerceRuntimeRequestFinished", 40105, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The Commerce Runtime Request with request id '{0}' and request url '{1}' succeeded.");
        };
        RetailLogger.modelManagersCheckDownloadCompleteRequestError = function (statusText) {
            TsLogging.LoggerBase.writeEvent("ModelManagersCheckDownloadCompleteRequestError", 40106, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "The request to check if offline data is downloaded to offline database failed with error: {0}.");
        };
        RetailLogger.modelManagersLocatorServiceRequestStarted = function (request, locatorServiceActivityId) {
            TsLogging.LoggerBase.writeEvent("modelManagersLocatorServiceRequestStarted", 40107, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "The request to discover retail server url has started. Request: {0}, Locator service activity id: {2}.");
        };
        RetailLogger.modelManagersLocatorServiceRequestException = function (errorMessage, locatorServiceActivityId) {
            TsLogging.LoggerBase.writeEvent("modelManagersLocatorServiceRequestException", 40108, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "The request to discover retail server url has failed. Error: {0}. Locator service activity id: {1}.");
        };
        RetailLogger.modelManagersLocatorServiceRequestFinished = function (locatorServiceActivityId) {
            TsLogging.LoggerBase.writeEvent("modelManagersLocatorServiceRequestFinished", 40109, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "The request to discover retail server url has finished. Locator service activity id: {0}.");
        };
        RetailLogger.modelManagersServerResponseStatusCode = function (statusCode) {
            TsLogging.LoggerBase.writeEvent("modelManagersServerResponseStatusCode", 40110, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "Server response status code is '{0}'.");
        };
        RetailLogger.modelManagersChainedRequestFactorySwitchingToOnline = function () {
            TsLogging.LoggerBase.writeEvent("modelManagersChainedRequestFactorySwitchingToOnline", 40111, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "ChannelRequestFactory Switching connection state to online.");
        };
        RetailLogger.modelManagersChainedRequestFactorySwitchingToOffline = function () {
            TsLogging.LoggerBase.writeEvent("modelManagersChainedRequestFactorySwitchingToOffline", 40112, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "ChannelRequestFactory Switching connection state to offline.");
        };
        RetailLogger.modelManagersChainedRequestFactoryShiftTransferFailed = function (currentState, errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("modelManagersChainedRequestFactoryShiftTransferFailed", 40113, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "ChannelRequestFactory Shift transfer failed during connection switch, current state = '{0}', error message = '{2}'.");
        };
        RetailLogger.modelManagersChainedRequestFactoryCartTransferToOfflineFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("modelManagersChainedRequestFactoryCartTransferToOfflineFailed", 40114, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "ChannelRequestFactory Cart transfer to offline failed, error message = '{1}'.");
        };
        RetailLogger.modelManagersODataExecuteBatchSuccess = function (batchRequestUri) {
            TsLogging.LoggerBase.writeEvent("modelManagersODataExecuteBatchSuccess", 40115, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "ODataRequest Request '{0}' success.");
        };
        RetailLogger.modelManagersODataExecuteBatchFailed = function (batchRequestUri, errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("modelManagersODataExecuteBatchFailed", 40116, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "ODataRequest Request '{0}' failed, error message = '{2}'.");
        };
        RetailLogger.modelManagersRetailServerManagerFactoryCreate = function (platform) {
            TsLogging.LoggerBase.writeEvent("modelManagersRetailServerManagerFactoryCreate", 40117, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Verbose, [], "", "", "Creating Data Service Request Factory for '{0}' platform.");
        };
        RetailLogger.modelManagersRetailServerManagerFactoryCreateChained = function () {
            TsLogging.LoggerBase.writeEvent("modelManagersRetailServerManagerFactoryCreateChained", 40118, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Verbose, [], "", "", "RetailServerManagerFactory Creating chained data service factory.");
        };
        RetailLogger.modelManagersRetailServerManagerFactoryCreateOnline = function () {
            TsLogging.LoggerBase.writeEvent("modelManagersRetailServerManagerFactoryCreateOnline", 40119, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Verbose, [], "", "", "RetailServerManagerFactory Creating online data service factory.");
        };
        RetailLogger.modelManagersCartManagerAddTenderLineToCartFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("modelManagersCartManagerAddTenderLineToCartFailed", 40120, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "Add tender line to cart failed, recovering: failed, error message = '{1}'.");
        };
        RetailLogger.modelManagersCartManagerFailedToOverridePriceNoCartLinesProvided = function () {
            TsLogging.LoggerBase.writeEvent("modelManagersCartManagerFailedToOverridePriceNoCartLinesProvided", 40121, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "CartManager Attempted to override price when no cart lines were provided.");
        };
        RetailLogger.modelManagersCartManagerFailedToOverridePriceNoCartLineOrPriceProvided = function () {
            TsLogging.LoggerBase.writeEvent("modelManagersCartManagerFailedToOverridePriceNoCartLineOrPriceProvided", 40122, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "CartManager Attempted to override price when a cart line, cart line id, or price was not provided.");
        };
        RetailLogger.modelManagersCustomerManagerCustomerValidationFailed = function () {
            TsLogging.LoggerBase.writeEvent("modelManagersCustomerManagerCustomerValidationFailed", 40123, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "CustomerManager Update customer validation failed.");
        };
        RetailLogger.modelManagersCustomerManagerCustomerIsNull = function () {
            TsLogging.LoggerBase.writeEvent("modelManagersCustomerManagerCustomerIsNull", 40124, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "CustomerManager Customer id is null, empty or undefined.");
        };
        RetailLogger.modelManagersRetailServerOdataRequestResponse = function (clientRequest, serverResponse) {
            TsLogging.LoggerBase.writeEvent("modelManagersRetailServerOdataRequestResponse", 40125, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Call to Retail Server succeeded.\n\nRequest:\n'{0}'\n\nResponse:\n'{1}'");
        };
        RetailLogger.modelManagersRetailServerOdataRequestErrorResponse = function (clientRequest, serverResponse) {
            TsLogging.LoggerBase.writeEvent("modelManagersRetailServerOdataRequestErrorResponse", 40126, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Call to Retail Server failed.\n\nRequest:\n'{0}'\n\nResponse:\n'{1}'");
        };
        RetailLogger.modelManagersChainedRequestFactoryShiftTransferToOnlineCreateFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("modelManagersChainedRequestFactoryShiftTransferToOnlineCreateFailed", 40127, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "ChannelRequestFactory Shift transfer from offline to online failed when creating or updating shift in online channel, error message = '{1}'.");
        };
        RetailLogger.modelManagersChainedRequestFactoryShiftTransferToOnlineDeleteFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("modelManagersChainedRequestFactoryShiftTransferToOnlineDeleteFailed", 40128, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "ChannelRequestFactory Shift transfer from offline to online failed when deleting shift in offline channel, error message = '{1}'.");
        };
        RetailLogger.coreCannotMapResourceMessage = function (resourceId) {
            TsLogging.LoggerBase.writeEvent("ResourceStringMappingNotFound", 40200, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Cannot localize message from resource identifier: '{0}'.");
        };
        RetailLogger.coreCannotMapErrorCode = function (errorCode) {
            TsLogging.LoggerBase.writeEvent("ErrorCodeMappingNotFound", 40201, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Cannot map to error entity from error code: '{0}'.");
        };
        RetailLogger.corePropertyMissingInDeviceActivationSequence = function (propertyName, operationName) {
            TsLogging.LoggerBase.writeEvent("MissingPropertyOnActivationSequence", 40202, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Operation '{1}' on device activation sequence is missing property '{0}'.");
        };
        RetailLogger.coreRetailOperationStarted = function (correlationId, operationName, operationId) {
            TsLogging.LoggerBase.writeEvent("RetailOperationStarted", 40203, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Running operation {1} ({2}).");
        };
        RetailLogger.coreOperationManagerRevertToSelf = function () {
            TsLogging.LoggerBase.writeEvent("OperationManagerRevertToSelf", 40204, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Executing revert to self.");
        };
        RetailLogger.coreRetailOperationManagerOverride = function (operationName, operationId) {
            TsLogging.LoggerBase.writeEvent("RetailOperationManagerOverride", 40205, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Running manager override for operation {0} ({1}).");
        };
        RetailLogger.coreRetailOperationHandlerNotFound = function (operationName, operationId) {
            TsLogging.LoggerBase.writeEvent("RetailOperationHandlerNotFound", 40206, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Operation handler not found for operation {0} ({1}).");
        };
        RetailLogger.coreRetailOperationCompleted = function (correlationId, operationName, operationId) {
            TsLogging.LoggerBase.writeEvent("RetailOperationCompleted", 40207, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Operation execution completed for operation {1} ({2}).");
        };
        RetailLogger.coreRetailOperationCanceled = function (correlationId, operationName, operationId) {
            TsLogging.LoggerBase.writeEvent("RetailOperationCanceled", 40208, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Operation execution canceled for operation {1} ({2}).");
        };
        RetailLogger.coreRetailOperationFailed = function (correlationId, operationName, operationId, errorMessage) {
            TsLogging.LoggerBase.writeEvent("RetailOperationFailed", 40209, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Operation execution failed for operation {1} ({2}) with error: {3}.");
        };
        RetailLogger.coreTriggerExecutionStarted = function (triggerName) {
            TsLogging.LoggerBase.writeEvent("TriggerExecutionStarted", 40210, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Trigger execution started for trigger {0}.");
        };
        RetailLogger.coreTriggerExecutionCompleted = function (triggerName) {
            TsLogging.LoggerBase.writeEvent("TriggerExecutionCompleted", 40211, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Trigger execution completed for trigger {0}.");
        };
        RetailLogger.coreTriggerExecutionCanceled = function (triggerName) {
            TsLogging.LoggerBase.writeEvent("TriggerExecutionCanceled", 40212, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Trigger execution canceled for trigger {0}.");
        };
        RetailLogger.coreTriggerExecutionFailed = function (triggerName, errorMessage) {
            TsLogging.LoggerBase.writeEvent("TriggerExecutionFailed", 40213, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Trigger execution failed for trigger {0} with error: {1}.");
        };
        RetailLogger.coreLogOriginalUnauthorizedRetailServerResponse = function (errorResourceId, errorMessage) {
            TsLogging.LoggerBase.writeEvent("LogOriginalUnauthorizedRetailServerResponse", 40214, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Unauthorized response with error: {1}. ErrorResourceId: {0}");
        };
        RetailLogger.coreLogXmlHttpRequestError = function (requestUrl, message) {
            TsLogging.LoggerBase.writeEvent("coreLogXmlHttpRequestError", 40215, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Warning, [], "", "", "Response error when sending request to URL {0}, Error message: '{1}'");
        };
        RetailLogger.coreHelpersProductPropertiesGetTranslation = function (translationKey) {
            TsLogging.LoggerBase.writeEvent("coreHelpersProductPropertiesGetTranslation", 40216, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Verbose, [], "", "", "ProductPropertiesHelper Getting translation value: {0}.");
        };
        RetailLogger.coreHelpersProductPropertiesPropertyNotExist = function (propertyName) {
            TsLogging.LoggerBase.writeEvent("coreHelpersProductPropertiesPropertyNotExist", 40217, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "ProductPropertiesHelper Property {0} is either not exists or not yet implemented.");
        };
        RetailLogger.coreHelpersProductPropertiesUnitOfMeasureNotExist = function (unitOfMeasureSymbol) {
            TsLogging.LoggerBase.writeEvent("coreHelpersProductPropertiesUnitOfMeasureNotExist", 40218, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "ProductPropertiesHelper GetUnitOfMeasures() The unit of measure object does not exist for the unit of measure '{0}'.");
        };
        RetailLogger.coreHelpersProductPropertiesVariantPropertyNotExist = function (propertyName) {
            TsLogging.LoggerBase.writeEvent("coreHelpersProductPropertiesVariantPropertyNotExist", 40219, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "ProductPropertiesHelper Property {0} is either not exists or not yet implemented.");
        };
        RetailLogger.coreHelpersProductPropertiesTranslationPropertyNotFound = function (translationKey) {
            TsLogging.LoggerBase.writeEvent("coreHelpersProductPropertiesTranslationPropertyNotFound", 40220, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "ProductPropertiesHelper Could not get translation property {0}.");
        };
        RetailLogger.coreHelpersProductPropertiesVariantNotFound = function (variantId, productId) {
            TsLogging.LoggerBase.writeEvent("coreHelpersProductPropertiesVariantNotFound", 40221, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "ProductPropertiesHelper Failed to find Variant {0} from product record Id: {1}.");
        };
        RetailLogger.coreHelpersProductPropertiesInputParameterProductIsUndefined = function (propertyName) {
            TsLogging.LoggerBase.writeEvent("coreHelpersProductPropertiesInputParameterProductIsUndefined", 40222, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "ProductPropertiesHelper - ProductPropertyFormatter: Input parameter product is undefined. Property name: {0}.");
        };
        RetailLogger.coreHelpersProductPropertiesInputParameterPropertyNameIsInvalid = function (itemId, propertyName) {
            TsLogging.LoggerBase.writeEvent("coreHelpersProductPropertiesInputParameterPropertyNameIsInvalid", 40223, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "ProductPropertiesHelper ProductPropertyFormatter: Input parameter propertyName is invalid. Item Id: {0}, Property name: {1}.");
        };
        RetailLogger.coreHelpersProductPropertiesProductNotHaveVariant = function (productId, variantId) {
            TsLogging.LoggerBase.writeEvent("coreHelpersProductPropertiesProductNotHaveVariant", 40224, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "ProductPropertiesHelper Product with Item Id {0} does not have the requested variant {1}.");
        };
        RetailLogger.coreHelpersProductPropertiesProductNotHaveProperty = function (productId, propertyName) {
            TsLogging.LoggerBase.writeEvent("coreHelpersProductPropertiesProductNotHaveProperty", 40225, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "ProductPropertiesHelper ProductPropertyFormatter: Product with Item Id {0} does not have the requested property {1}.");
        };
        RetailLogger.coreHelpersUnrecognizedAddressComponent = function (addressComponent) {
            TsLogging.LoggerBase.writeEvent("coreHelpersUnrecognizedAddressComponent", 40226, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "AddressHelper Unrecognized address component: {0}.");
        };
        RetailLogger.coreHelpersInvalidManualDiscountType = function (manualDiscountType) {
            TsLogging.LoggerBase.writeEvent("coreHelpersInvalidManualDiscountType", 40228, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "DiscountsHelper Invalid manual discount type: '{0}'.");
        };
        RetailLogger.coreHelpersInvalidCustomerDiscountType = function (customerDiscountType) {
            TsLogging.LoggerBase.writeEvent("coreHelpersInvalidCustomerDiscountType", 40229, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "DiscountsHelper Invalid customer discount type: '{0}'.");
        };
        RetailLogger.coreHelpersInvalidDiscountLineType = function (discountLineType) {
            TsLogging.LoggerBase.writeEvent("coreHelpersInvalidDiscountLineType", 40230, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "DiscountsHelper Invalid discount line type: '{0}'.");
        };
        RetailLogger.coreHelpersUnknownAddressType = function (addressType) {
            TsLogging.LoggerBase.writeEvent("coreHelpersUnknownAddressType", 40231, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "AddressTypeEnumHelper getDescription() Unknown address type '{0}'.");
        };
        RetailLogger.coreTenderTypeMapOperationHasNoTenderType = function (operationId) {
            TsLogging.LoggerBase.writeEvent("coreTenderTypeMapOperationHasNoTenderType", 40232, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "TenderTypeMap Operation id {0} has no tender type mapped to it.");
        };
        RetailLogger.coreTenderTypeMapMultipleTendersOfSameType = function () {
            TsLogging.LoggerBase.writeEvent("coreTenderTypeMapMultipleTendersOfSameType", 40233, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "TenderTypeMap Multiple tenders of same type.");
        };
        RetailLogger.coreBindingHandlersLoadImageFailed = function (src) {
            TsLogging.LoggerBase.writeEvent("coreBindingHandlersLoadImageFailed", 40234, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "BindingHandlers SetDefaultImage() Error loading image for '{0}'.");
        };
        RetailLogger.coreFormattersCartLineWrongInputParameters = function (propertyName, data) {
            TsLogging.LoggerBase.writeEvent("coreFormattersCartLineWrongInputParameters", 40235, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "Formatters.CartLineProperty Wrong input paramters: {0} data: {1}.");
        };
        RetailLogger.coreTenderTypeMapTenderTypeNotFound = function (tenderTypeId) {
            TsLogging.LoggerBase.writeEvent("coreTenderTypeMapTenderTypeNotFound", 40236, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "TenderTypeMap did not locate tender type. Tender type does not exist or job was not run. TenderTypeId: {0}");
        };
        RetailLogger.coreApplicationStorageSetItemFailure = function (storageKey, errorMessage) {
            TsLogging.LoggerBase.writeEvent("coreApplicationStorageSetItemFailure", 40237, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "The attempt to save key '{0}' in application storage failed. Error message: {1}");
        };
        RetailLogger.coreApplicationStorageSetItemFailureRecoveryUnsuccessful = function (storageKey, errorMessage) {
            TsLogging.LoggerBase.writeEvent("coreApplicationStorageSetItemFailureRecoveryUnsuccessful", 40238, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "The attempt to recover from the application storage failure for key '{0}' was unsucessful. Error message: {1}.");
        };
        RetailLogger.coreLogUserAuthenticationRetailServerResponse = function (errorResourceId, errorMessage) {
            TsLogging.LoggerBase.writeEvent("coreLogUserAuthenticationRetailServerResponse", 40239, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "Unauthorized response with error: {1}. ErrorResourceId: {0}");
        };
        RetailLogger.coreRetailCheckOpenDrawerStatusExecutionStart = function () {
            TsLogging.LoggerBase.writeEvent("coreRetailCheckOpenDrawerStatusStart", 40240, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Check Open Cash Drawer Status Execution Started.");
        };
        RetailLogger.coreRetailCheckOpenDrawerStatusExecutionCompleted = function () {
            TsLogging.LoggerBase.writeEvent("coreRetailCheckOpenDrawerStatusExecutionCompleted", 40241, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Check Open Cash Drawer Status Execution Completed.");
        };
        RetailLogger.coreRetailCheckOpenDrawerStatusExecutionFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("coreRetailCheckOpenDrawerStatusExecutionFailed", 40242, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Check Open Cash Drawer Status Execution Failed: error message = '{1}'.");
        };
        RetailLogger.operationLogOffComplete = function () {
            TsLogging.LoggerBase.writeEvent("operationLogOffComplete", 40300, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "Logoff operation handler success.");
        };
        RetailLogger.operationLogOffFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("operationLogOffFailed", 40301, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Post logoff operation(s) failed: error message = '{1}'.");
        };
        RetailLogger.operationAddGiftCard = function (giftCardId, amount, currency, lineDescription) {
            TsLogging.LoggerBase.writeEvent("operationAddGiftCard", 40302, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Verbose, [], "", "", "AddGiftCard Gift card Id: {0}, amount: {1}, currency: {2}, description: {3}.");
        };
        RetailLogger.operationIssueGiftCard = function (giftCardId, amount, currency, lineDescription) {
            TsLogging.LoggerBase.writeEvent("operationIssueGiftCard", 40303, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Verbose, [], "", "", "IssueGiftCard Gift card Id: {0}, amount: {1}, currency: {2}, description: {3}.");
        };
        RetailLogger.operationCloseShift = function () {
            TsLogging.LoggerBase.writeEvent("operationCloseShift", 40304, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "CloseShift Close shift operation succeeded.");
        };
        RetailLogger.operationLocateServerUrl = function (url) {
            TsLogging.LoggerBase.writeEvent("operationLocateServerUrl", 40305, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Verbose, [], "", "", "UpdateServerUrl Getting Retail Server URL from locator service '{0}'.");
        };
        RetailLogger.operationUpdateServerUrl = function (url) {
            TsLogging.LoggerBase.writeEvent("operationUpdateServerUrl", 40306, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Verbose, [], "", "", "UpdateServerUrl Retail Server URL from locator service '{0}'.");
        };
        RetailLogger.operationDeviceActivationUnhandledError = function (error) {
            TsLogging.LoggerBase.writeEvent("operationDeviceActivationUnhandledError", 40307, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "DeviceActivationOperation Unhandled error = '{0}'.");
        };
        RetailLogger.operationTimeClockNotEnabled = function () {
            TsLogging.LoggerBase.writeEvent("operationTimeClockNotEnabled", 40308, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "TimeClock Time clock not enabled.");
        };
        RetailLogger.operationPickingAndReceivingGetAllOrdersFailed = function () {
            TsLogging.LoggerBase.writeEvent("operationPickingAndReceivingGetAllOrdersFailed", 40309, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "PickingAndReceivingOperationHandler Failed to get all purchase orders.");
        };
        RetailLogger.operationPickingAndReceivingUpdatePurchaseOrderFailed = function (orderId) {
            TsLogging.LoggerBase.writeEvent("operationPickingAndReceivingUpdatePurchaseOrderFailed", 40310, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "PickingAndReceivingOperationHandler Failed to update purchase order {0}.");
        };
        RetailLogger.operationPickingAndReceivingCommitPurchaseOrderFailed = function (journalId) {
            TsLogging.LoggerBase.writeEvent("operationPickingAndReceivingCommitPurchaseOrderFailed", 40311, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "PickingAndReceivingOperationHandler Failed to commit purchase order {0}.");
        };
        RetailLogger.operationPickingAndReceivingGetPurchaseOrderFailed = function (journalId) {
            TsLogging.LoggerBase.writeEvent("operationPickingAndReceivingGetPurchaseOrderFailed", 40312, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "PickingAndReceivingOperationHandler Failed to get purchase order {0}.");
        };
        RetailLogger.operationPickingAndReceivingUpdateTransderOrderFailed = function (orderId) {
            TsLogging.LoggerBase.writeEvent("operationPickingAndReceivingUpdateTransderOrderFailed", 40313, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "PickingAndReceivingOperationHandler Failed to update transfer order {0}.");
        };
        RetailLogger.operationPickingAndReceivingCommitTransferOrderFailed = function (journalId) {
            TsLogging.LoggerBase.writeEvent("operationPickingAndReceivingCommitTransferOrderFailed", 40314, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "PickingAndReceivingOperationHandler Failed to commit transfer order {0}.");
        };
        RetailLogger.operationPickingAndReceivingGetTransferOrderFailed = function (journalId) {
            TsLogging.LoggerBase.writeEvent("operationPickingAndReceivingGetTransferOrderFailed", 40315, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "PickingAndReceivingOperationHandler Failed to get transfer order {0}.");
        };
        RetailLogger.operationPickingAndReceivingUpdatePickingListFailed = function (orderId) {
            TsLogging.LoggerBase.writeEvent("operationPickingAndReceivingUpdatePickingListFailed", 40316, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "PickingAndReceivingOperationHandler Failed to update picking list {0}.");
        };
        RetailLogger.operationPickingAndReceivingCommitPickingListFailed = function (journalId) {
            TsLogging.LoggerBase.writeEvent("operationPickingAndReceivingCommitPickingListFailed", 40317, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "PickingAndReceivingOperationHandler Failed to commit picking list {0}.");
        };
        RetailLogger.operationPickingAndReceivingGetPickingListFailed = function (journalId) {
            TsLogging.LoggerBase.writeEvent("operationPickingAndReceivingGetPickingListFailed", 40318, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "PickingAndReceivingOperationHandler Failed to get picking list {0}.");
        };
        RetailLogger.retailServerRequestRedirection = function (redirectionUrl) {
            TsLogging.LoggerBase.writeEvent("retailServerRequestRedirection", 40319, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "Redirection response from server. Redirecting to: {0}.");
        };
        RetailLogger.operationItemSaleCreateCartLinesStarted = function (correlationId) {
            TsLogging.LoggerBase.writeEvent("operationItemSaleCreateCartLinesStarted", 40320, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Creating cart lines started.");
        };
        RetailLogger.operationItemSaleCreateCartLinesFinished = function (correlationId, successful) {
            TsLogging.LoggerBase.writeEvent("operationItemSaleCreateCartLinesFinished", 40321, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Creating cart lines " + successful ? "finished successfully." : "failed.");
        };
        RetailLogger.operationItemSaleGetProductSaleDetailsStarted = function (correlationId) {
            TsLogging.LoggerBase.writeEvent("operationItemSaleGetProductSaleDetailsStarted", 40322, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Retrieving product details started.");
        };
        RetailLogger.operationItemSaleGetProductSaleDetailsFinished = function (correlationId, successful) {
            TsLogging.LoggerBase.writeEvent("operationItemSaleGetProductSaleDetailsFinished", 40323, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Retrieving product details " + successful ? "finished successfully." : "failed.");
        };
        RetailLogger.operationBlindCloseSharedShiftFailedOnRevertToSelfDuringCancellation = function (shiftId, staffId) {
            TsLogging.LoggerBase.writeEvent("operationBlindCloseSharedShiftFailedOnRevertToSelfDuringCancellation", 40324, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "The staff '" + staffId + "' fails to cancel the blind close shared shift operation for shift '" + shiftId + "' when reverting back from the elevated user.");
        };
        RetailLogger.peripheralsCashDrawerOpening = function (deviceName, deviceType) {
            TsLogging.LoggerBase.writeEvent("peripheralsCashDrawerOpening", 40400, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "CashDrawer Opening cash drawer. Name: {0}, Type: {1}.");
        };
        RetailLogger.peripheralsMSRKeyboardSwipeParserLog = function (message) {
            TsLogging.LoggerBase.writeEvent("peripheralsMSRKeyboardSwipeParserLog", 40401, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "{0}");
        };
        RetailLogger.peripheralsCompositeBarcodeScannerObjectNotDefined = function (objectName) {
            TsLogging.LoggerBase.writeEvent("peripheralsCompositeBarcodeScannerObjectNotDefined", 40402, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "CompositeBarcodeScanner _barcodesLazy {0} is not defined.");
        };
        RetailLogger.peripheralsHardwareStationContextActionRequestSucceeded = function (actionRequest) {
            TsLogging.LoggerBase.writeEvent("peripheralsHardwareStationContextActionRequestSucceeded", 40403, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "HardwareStationContext {0} success.");
        };
        RetailLogger.peripheralsHardwareStationContextActionRequestFailed = function (actionRequest, errors) {
            TsLogging.LoggerBase.writeEvent("peripheralsHardwareStationContextActionRequestFailed", 40404, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "HardwareStationContext {0} failed. Errors: {1}.");
        };
        RetailLogger.peripheralsUnsupportedPrinterType = function (printerType) {
            TsLogging.LoggerBase.writeEvent("peripheralsUnsupportedPrinterType", 40405, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "Peripherals Facade Printer has an unsupported printer type '{0}'.");
        };
        RetailLogger.peripheralsBarcodeScannerGetDeviceSelectorFailed = function (errorMessage) {
            TsLogging.LoggerBase.writeEvent("peripheralsBarcodeScannerGetDeviceSelectorFailed", 40406, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "BarcodeScanner Facade Could not get a barcode device selector from  Windows.Devices.PointOfService.BarcodeScanner - Error is: '{0}'.");
        };
        RetailLogger.peripheralsBarcodeScannerGetBluetoothDeviceSelectorFailed = function (errorMessage) {
            TsLogging.LoggerBase.writeEvent("peripheralsBarcodeScannerGetBluetoothDeviceSelectorFailed", 40407, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "BarcodeScanner Facade Could not get a bluetooth serial port device selector from Windows.Devices.Bluetooth.Rfcomm.RfcommDeviceService - Error is: '{0}'.");
        };
        RetailLogger.peripheralsBarcodeScannerRfCommDeviceServiceNotFound = function () {
            TsLogging.LoggerBase.writeEvent("peripheralsBarcodeScannerRfCommDeviceServiceNotFound", 40408, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "BarcodeScanner Facade Could not find RfCommDeviceService... this could be a problem where the device use consent is not presented or device is not turned on etc.");
        };
        RetailLogger.peripheralsBarcodeScannerEnableFailed = function (errorMessage) {
            TsLogging.LoggerBase.writeEvent("peripheralsBarcodeScannerEnableFailed", 40409, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "BarcodeScanner Native Error enabling barcode scanner.");
        };
        RetailLogger.peripheralsMagneticStripeReaderGetDeviceSelectorFailed = function (errorMessage) {
            TsLogging.LoggerBase.writeEvent("peripheralsMagneticStripeReaderGetDeviceSelectorFailed", 40410, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "MagneticStripeReader Facade Could not get a msr device selector from  Windows.Devices.PointOfService.MagneticStripeReader - Error is: '{0}'.");
        };
        RetailLogger.peripheralsMagneticStripeReaderGetBluetoothDeviceSelectorFailed = function (errorMessage) {
            TsLogging.LoggerBase.writeEvent("peripheralsMagneticStripeReaderGetBluetoothDeviceSelectorFailed", 40411, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "MagneticStripeReader Facade Could not get a bluetooth serial port device selector from Windows.Devices.Bluetooth.Rfcomm.RfcommDeviceService - Error is: '{0}'.");
        };
        RetailLogger.peripheralsMagneticStripeReaderRfCommDeviceServiceNotFound = function () {
            TsLogging.LoggerBase.writeEvent("peripheralsMagneticStripeReaderRfCommDeviceServiceNotFound", 40412, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "MagneticStripeReader Facade Could not find RfCommDeviceService... this could be a problem where the device use consent is not presented or device is not turned on etc.");
        };
        RetailLogger.peripheralsMagneticStripeReaderInitializeFailed = function (errorMessage) {
            TsLogging.LoggerBase.writeEvent("peripheralsMagneticStripeReaderInitializeFailed", 40413, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "MagneticStripeReader Native Device failed to initialize due to error: '{0}'.");
        };
        RetailLogger.peripheralsMagneticStripeReaderEnableFailed = function (errorMessage) {
            TsLogging.LoggerBase.writeEvent("peripheralsMagneticStripeReaderEnableFailed", 40414, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "MagneticStripeReader Native Error enabling magnetic stripe reader: '{0}'.");
        };
        RetailLogger.peripheralsProximityOpenDeviceFailed = function (errorMessage) {
            TsLogging.LoggerBase.writeEvent("peripheralsProximityOpenDeviceFailed", 40418, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "Peripherals Proximity Error opening device: '{0}'.");
        };
        RetailLogger.peripheralsProximityNotAvailable = function () {
            TsLogging.LoggerBase.writeEvent("peripheralsProximityNotAvailable", 40419, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "Peripherals Proximity Device is not available.");
        };
        RetailLogger.peripheralsLongPollingLockGetDataError = function (errors) {
            TsLogging.LoggerBase.writeEvent("peripheralsLongPoolingLockGetDataError", 40421, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Peripherals LongPollingLock Failed to get data.: errors = '{0}'.");
        };
        RetailLogger.peripheralsLongPollingLockGetDataUnhandledError = function (message) {
            TsLogging.LoggerBase.writeEvent("peripheralsLongPoolingLockGetDataUnhandledError", 40422, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "Peripherals LongPollingLock {0}");
        };
        RetailLogger.peripheralsNetworkPaymentTerminalIsNotSupported = function () {
            TsLogging.LoggerBase.writeEvent("peripheralsNetworkPaymentTerminalIsNotSupported", 40423, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "Network payment terminal is not supported for the application.");
        };
        RetailLogger.librariesWinJsListViewShown = function (elementId) {
            TsLogging.LoggerBase.writeEvent("LibrariesWinJsListViewShown", 40500, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The list view is shown. Element ID: '{0}'.");
        };
        RetailLogger.librariesWinJsListViewItemClick = function (elementId) {
            TsLogging.LoggerBase.writeEvent("LibrariesWinJsListViewItemClick", 40501, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "An item in the list view has been clicked. Element ID: '{0}'.");
        };
        RetailLogger.librariesNumpadEnterKey = function (textFieldValue) {
            TsLogging.LoggerBase.writeEvent("LibrariesNumpadEnterKey", 40600, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Numpad enter key was pressed.  Text field value: '{0}'.");
        };
        RetailLogger.librariesAuthenticationProviderLoginStarted = function (requestId, details) {
            TsLogging.LoggerBase.writeEvent("librariesAuthenticationProviderLoginStarted", 40610, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Login is starting. {1} RequestId: {0}");
        };
        RetailLogger.librariesAuthenticationProviderLoginFinished = function (requestId, details) {
            TsLogging.LoggerBase.writeEvent("librariesAuthenticationProviderLoginFinished", 40611, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Login has finished. {1} RequestId: {0}");
        };
        RetailLogger.librariesAuthenticationProviderAcquireTokenStarted = function (requestId, resourceId) {
            TsLogging.LoggerBase.writeEvent("librariesAuthenticationProviderAcquireTokenStarted", 40612, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Starting: acquireToken for resource '{1}'. RequestId: {0}");
        };
        RetailLogger.librariesAuthenticationProviderAcquireTokenFinished = function (requestId) {
            TsLogging.LoggerBase.writeEvent("librariesAuthenticationProviderAcquireTokenFinished", 40613, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Finished: acquireToken. RequestId: {0}");
        };
        RetailLogger.helpersActivityHelperAddCartLinesStarted = function (correlationId) {
            TsLogging.LoggerBase.writeEvent("HelpersActivityHelperAddCartLineStarted", 40700, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Items added to cart started.");
        };
        RetailLogger.helpersActivityHelperAddCartLinesFinished = function (correlationId) {
            TsLogging.LoggerBase.writeEvent("HelpersActivityHelperAddCartLineFinished", 40701, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Items added to cart finished.");
        };
        RetailLogger.viewsLoginLoginViewSignInStarted = function () {
            TsLogging.LoggerBase.writeEvent("ViewsLoginLoginViewSignInStarted", 40800, 1, TsLogging.EventChannel.Admin, TsLogging.EventLevel.Informational, [], "", "", "Logon started.");
        };
        RetailLogger.viewsLoginLoginViewSignInFinished = function () {
            TsLogging.LoggerBase.writeEvent("ViewsLoginLoginViewSignInFinished", 40804, 1, TsLogging.EventChannel.Admin, TsLogging.EventLevel.Informational, [], "", "", "Logon finished.");
        };
        RetailLogger.viewsCloudDeviceActivationViewActivationFailed = function () {
            TsLogging.LoggerBase.writeEvent("ViewsCloudDeviceActivationViewActivationFailed", 40810, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "The device activation operation failed.");
        };
        RetailLogger.viewsHomeTileClick = function (action) {
            TsLogging.LoggerBase.writeEvent("ViewsHomeTileClick", 40820, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "One of the tiles on the Home Page was clicked. Action: '{0}'.");
        };
        RetailLogger.viewsOrderPaymentViewPaymentInitiated = function () {
            TsLogging.LoggerBase.writeEvent("ViewsOrderPaymentViewPaymentInitiated", 40850, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "Payment has been initiated from the 'Payment View'.");
        };
        RetailLogger.viewsCustomerDetailsLoaded = function () {
            TsLogging.LoggerBase.writeEvent("ViewsCustomerDetailsLoaded", 40890, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The Customer Details page has fully loaded.");
        };
        RetailLogger.viewsCustomerDetailsError = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("viewsCustomerDetailsError", 40891, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "CustomerDetailsViewController: error message = '{1}'.");
        };
        RetailLogger.viewsCustomerDetailsAddCustomerFailed = function () {
            TsLogging.LoggerBase.writeEvent("viewsCustomerDetailsAddCustomerFailed", 40892, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "CustomerDetailsViewController addCustomerToCart() failed.");
        };
        RetailLogger.viewsMerchandisingSearchViewSearchClick = function (searchTerm) {
            TsLogging.LoggerBase.writeEvent("ViewsMerchandisingSearchViewSearchClick", 40900, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "A search has been initiated from the search view. Search term: '{0}'.");
        };
        RetailLogger.viewsMerchandisingSearchViewProductButtonClick = function (searchTerm) {
            TsLogging.LoggerBase.writeEvent("ViewsMerchandisingSearchViewProductButtonClick", 40901, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The produt button has been clicked in the search view. Search term: '{0}'.");
        };
        RetailLogger.viewsMerchandisingSearchViewCustomerButtonClick = function (searchTerm) {
            TsLogging.LoggerBase.writeEvent("ViewsMerchandisingSearchViewCustomerButtonClick", 40902, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The customer button has been clicked in the search view. Search term: '{0}'.");
        };
        RetailLogger.viewsMerchandisingSearchViewAddToCartClick = function (numberOfItems) {
            TsLogging.LoggerBase.writeEvent("ViewsMerchandisingSearchViewAddToCartClick", 40903, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The 'Add to Sale' button was clicked on the Search View Page. Number of items: '{0}'.");
        };
        RetailLogger.viewsMerchandisingSearchViewQuickSaleClick = function (numberOfItems) {
            TsLogging.LoggerBase.writeEvent("ViewsMerchandisingSearchViewQuickSaleClick", 40904, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The 'Sell Now' button was clicked on the Search View Page. Number of items: '{0}'.");
        };
        RetailLogger.viewsMerchandisingSearchViewInvalidProductOperation = function () {
            TsLogging.LoggerBase.writeEvent("viewsMerchandisingSearchViewInvalidProductOperation", 40907, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "SearchViewController productOperationHandler() Invalid product operation.");
        };
        RetailLogger.viewsMerchandisingSearchViewInvalidCustomerOperation = function () {
            TsLogging.LoggerBase.writeEvent("viewsMerchandisingSearchViewInvalidCustomerOperation", 40908, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "SearchViewController customerOperationHandler() Invalid product operation.");
        };
        RetailLogger.viewsMerchandisingProductDetailsLoadStarted = function () {
            TsLogging.LoggerBase.writeEvent("ViewsMerchandisingProductDetailsLoadStarted", 40910, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The loading of the Product Details page has started.");
        };
        RetailLogger.viewsMerchandisingProductDetailsAddItem = function () {
            TsLogging.LoggerBase.writeEvent("ViewsMerchandisingProductDetailsAddItem", 40911, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The 'Add Item' button on the Product Details page has been clicked.");
        };
        RetailLogger.viewsMerchandisingProductDetailsQuickSale = function () {
            TsLogging.LoggerBase.writeEvent("ViewsMerchandisingProductDetailsQuickSale", 40912, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The 'Quick Sale' button on the Product Details page has been clicked.");
        };
        RetailLogger.viewsMerchandisingProductDetailsLoaded = function () {
            TsLogging.LoggerBase.writeEvent("ViewsMerchandisingProductDetailsLoaded", 40913, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The Product Details page has fully loaded.");
        };
        RetailLogger.viewsMerchandisingProductDetailsKitVariantNotFound = function (kitVariantId, productId) {
            TsLogging.LoggerBase.writeEvent("viewsMerchandisingProductDetailsKitVariantNotFound", 40914, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "ProductDetailsViewController Kit Variant {0} not found as part of the variant information in product id {1}.");
        };
        RetailLogger.viewsMerchandisingPriceCheckViewGetPriceFinished = function () {
            TsLogging.LoggerBase.writeEvent("ViewsMerchandisingPriceCheckViewGetPriceFinished", 40930, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The price for a specific item was successfully retrieved.");
        };
        RetailLogger.viewsMerchandisingCatalogsCatalogClicked = function (catalogId, catalogName) {
            TsLogging.LoggerBase.writeEvent("ViewsMerchandisingCatalogsCatalogClicked", 40936, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "A catalog was clicked in the Catalogs Page. Catalog ID: '{0}'. Catalog Name: '{1}'.");
        };
        RetailLogger.viewsMerchandisingCategoriesViewLoaded = function () {
            TsLogging.LoggerBase.writeEvent("viewsMerchandisingCategoriesViewLoaded", 40940, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The Categories Page was successfully loaded.");
        };
        RetailLogger.viewsCartCartViewPayQuickCash = function () {
            TsLogging.LoggerBase.writeEvent("ViewsCartCartViewPayQuickCash", 44000, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Checking out with quick cash.");
        };
        RetailLogger.viewsCartCartViewShowPrintDialogFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("viewsCartCartViewShowPrintDialogFailed", 44001, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "CartViewController showPrintDialog() failed: error message = '{1}'.");
        };
        RetailLogger.viewsCartShowJournalViewLoaded = function () {
            TsLogging.LoggerBase.writeEvent("ViewsCartShowJournalViewLoaded", 44050, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The Show Journal View has successfully loaded.");
        };
        RetailLogger.viewsCartShowJournalViewRetrieveProductFailed = function (productId) {
            TsLogging.LoggerBase.writeEvent("viewsCartShowJournalViewRetrieveProductFailed", 44051, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "ShowJournalViewController Failed to retrieve product with Id {0}.");
        };
        RetailLogger.viewsControlsCommonHeaderSearch = function (searchTerm) {
            TsLogging.LoggerBase.writeEvent("ViewsControlsCommonHeaderSearch", 44100, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "A search has been initiated from the nav bar. Search term: '{0}'.");
        };
        RetailLogger.viewsControlsCommonHeaderFilterIconClick = function () {
            TsLogging.LoggerBase.writeEvent("ViewsControlsCommonHeaderFilterIconClick", 44101, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The filter icon in the nav bar was clicked.");
        };
        RetailLogger.viewsControlsCommonHeaderCategoryInTreeClicked = function (categoryName) {
            TsLogging.LoggerBase.writeEvent("ViewsControlsCommonHeaderCategoryInTreeClicked", 44105, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "One of the categories in the Category Tree of the nav bar was clicked. Category Name: '{0}'.");
        };
        RetailLogger.viewsControlsModalDialogRendered = function () {
            TsLogging.LoggerBase.writeEvent("ViewsControlsModalDialogRendered", 44110, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The Modal Dialog has been successfully rendered.");
        };
        RetailLogger.viewsControlsRefinersApplyFilters = function () {
            TsLogging.LoggerBase.writeEvent("ViewsControlsRefinersApplyFilters", 44116, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The 'Apply Filters' button on the filter nav bar was clicked.");
        };
        RetailLogger.viewsControlsRefinersTypeNotSupported = function (refiner) {
            TsLogging.LoggerBase.writeEvent("viewsControlsRefinersTypeNotSupported", 44118, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "RefinerControl Refiner Type Value not supported: data: {0}.");
        };
        RetailLogger.viewsControlsRefinersDisplayTemplateNotSupported = function (refiner) {
            TsLogging.LoggerBase.writeEvent("viewsControlsRefinersDisplayTemplateNotSupported", 44119, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "RefinerControl Refiner DisplayTemplate not supported: data: {0}.");
        };
        RetailLogger.viewsControlsRefinersWrongInputParameters = function (refiner) {
            TsLogging.LoggerBase.writeEvent("viewsControlsRefinersWrongInputParameters", 44120, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "RefinerControl Wrong input paramters: data: {0}.");
        };
        RetailLogger.viewsControlsPrintReceiptShown = function () {
            TsLogging.LoggerBase.writeEvent("ViewsControlsPrintReceiptShown", 44130, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The Print Receipt dialog is shown.");
        };
        RetailLogger.viewsControlsPrintReceiptSkipped = function () {
            TsLogging.LoggerBase.writeEvent("ViewsControlsPrintReceiptSkipped", 44131, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The Print Receipt dialog was not shown and skipped to print directly.");
        };
        RetailLogger.viewsControlsPrintReceiptPrinted = function () {
            TsLogging.LoggerBase.writeEvent("ViewsControlsPrintReceiptPrinted", 44132, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The receipts were printed.");
        };
        RetailLogger.viewsControlsCashbackDialogOnShowingParametersUndefined = function () {
            TsLogging.LoggerBase.writeEvent("viewsControlsCashbackDialogOnShowingParametersUndefined", 44140, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "CashbackDialog onShowing failed. Parameter options is null or undefined.");
        };
        RetailLogger.viewsControlsOrderCheckoutDialogOnShowingParametersUndefined = function () {
            TsLogging.LoggerBase.writeEvent("viewsControlsOrderCheckoutDialogOnShowingParametersUndefined", 44160, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "OrderCheckoutDialog onShowing failed. Parameter options is null or undefined.");
        };
        RetailLogger.viewsMerchandisingAllStoresViewConstructorArgumentUndefined = function (argument) {
            TsLogging.LoggerBase.writeEvent("viewsMerchandisingAllStoresViewConstructorArgumentUndefined", 44180, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "AllStoresView constructor: Argument '{0}' is null or undefined.");
        };
        RetailLogger.viewsMerchandisingPickingAndReceivingDetailsViewLoadJournalFailed = function (journalId) {
            TsLogging.LoggerBase.writeEvent("viewsMerchandisingPickingAndReceivingDetailsViewLoadJournalFailed", 44190, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "PickingAndReceivingDetailsView Failed to load journal with journal Id {0}.");
        };
        RetailLogger.viewsCustomerAddEditViewAddCustomerFailed = function () {
            TsLogging.LoggerBase.writeEvent("viewsCustomerAddEditViewAddCustomerFailed", 44200, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "CustomerAddEditView addCustomerToCart() failed.");
        };
        RetailLogger.viewsCustomerAddEditViewAddUpdateNewCustomerFailed = function () {
            TsLogging.LoggerBase.writeEvent("viewsCustomerAddEditViewAddUpdateNewCustomerFailed", 44201, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "CustomerAddEditView addUpdateNewCustomer() failed.");
        };
        RetailLogger.viewsCustomerPickUpInStoreViewBingMapsFaild = function (message) {
            TsLogging.LoggerBase.writeEvent("viewsCustomerPickUpInStoreViewBingMapsFaild", 44210, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "PickUpInStoreView Bing maps control Failed: {0}.");
        };
        RetailLogger.viewsCustomerPickUpInStoreViewBingMapsFailedToInitialize = function (message) {
            TsLogging.LoggerBase.writeEvent("viewsCustomerPickUpInStoreViewBingMapsFailedToInitialize", 44211, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "PickUpInStoreView Bing maps control failed to initialize: {0}.");
        };
        RetailLogger.viewsCustomerAddressAddEditViewDownloadTaxGroupsFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("viewsCustomerAddressAddEditViewDownloadTaxGroupsFailed", 44220, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "AddressAddEditView Failed when downloading sales tax groups: error message = '{1}'.");
        };
        RetailLogger.viewsDailyOperationsCashManagementViewOperationFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("viewsDailyOperationsCashManagementViewOperationFailed", 44230, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "CashManagementView operationSuccessCallback() failed: error message = '{1}'.");
        };
        RetailLogger.viewsControlsKnockoutCustomerCardDataPropertyRequired = function () {
            TsLogging.LoggerBase.writeEvent("viewsControlsKnockoutCustomerCardDataPropertyRequired", 44240, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "knockout.customerCard the data property is required.");
        };
        RetailLogger.viewsControlsKnockoutParallaxBackgroundElementRequired = function () {
            TsLogging.LoggerBase.writeEvent("viewsControlsKnockoutParallaxBackgroundElementRequired", 44241, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "knockout.parallax A parallax background element class, id, or tagname is required.");
        };
        RetailLogger.viewsTutorialVideoDialogVideoElementThrowsError = function (errorMessage) {
            TsLogging.LoggerBase.writeEvent("viewsTutorialVideoDialogVideoElementThrowsError", 44260, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Video element throws an error. Details: {0}.");
        };
        RetailLogger.viewModelCartAddProductsToCart = function () {
            TsLogging.LoggerBase.writeEvent("ViewModelCartAddProductsToCart", 44300, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The cart has been updated with a set of products.");
        };
        RetailLogger.viewModelCartVoidProductsStarted = function () {
            TsLogging.LoggerBase.writeEvent("viewModelCartVoidProductsStarted", 44310, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Product void has started.");
        };
        RetailLogger.viewModelCartVoidProductsFinished = function (success) {
            TsLogging.LoggerBase.writeEvent("viewModelCartVoidProductsFinished", 44311, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Product void has finished. Success: {0}.");
        };
        RetailLogger.viewModelCartGetProductDetailsFailed = function () {
            TsLogging.LoggerBase.writeEvent("viewModelCartGetProductDetailsFailed", 44312, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "CartViewModel getProductDetailsAsync() failed.");
        };
        RetailLogger.viewModelGetCustomerBalanceFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("viewModelGetCustomerBalanceFailed", 44313, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "CartViewModel getCustomerBalanceAsync() failed: error message = '{1}'.");
        };
        RetailLogger.viewModelGetCustomerLoyaltyCardsFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("viewModelGetCustomerLoyaltyCardsFailed", 44314, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "CartViewModel getCustomerLoyaltyCardsAsync() failed: error message = '{1}'.");
        };
        RetailLogger.viewModelUnsupportedBarcodeMaskType = function (maskType) {
            TsLogging.LoggerBase.writeEvent("viewModelUnsupportedBarcodeMaskType", 44315, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "CartViewModel a bar code with unsupported mask type, {0}, was scanned.");
        };
        RetailLogger.viewModelCartProcessScanResultStarted = function (correlationId, barcodeMaskType) {
            TsLogging.LoggerBase.writeEvent("viewModelCartProcessItemScanStarted", 44316, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "CartViewModel processing scan result with barcode mask type {1} started.");
        };
        RetailLogger.viewModelCartProcessScanResultFinished = function (correlationId, successful) {
            TsLogging.LoggerBase.writeEvent("viewModelCartProcessItemScanFinished", 44317, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "CartViewModel processing scan result " + successful ? "finished successfully." : "failed.");
        };
        RetailLogger.viewModelLoginDeviceActivationFailed = function (deviceId, terminalId, errorResourceIds) {
            TsLogging.LoggerBase.writeEvent("ViewModelLoginDeviceActivationFailed", 44400, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "The device activation call failed for device {0}, terminal {1} due to {2}.");
        };
        RetailLogger.viewModelLoginRetrieveUserAuthenticationTokenFailed = function (tokenResourceId, errorDetails) {
            TsLogging.LoggerBase.writeEvent("ViewModelLoginRetrieveUserAuthenticationTokenFailed", 44401, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Retrieving user authentication token for resource identifier '{0}' failed with error: {1}.");
        };
        RetailLogger.viewModelLoginRetailServerDiscoveryFailed = function (locatorServiceUrl, tenantId, errorDetails) {
            TsLogging.LoggerBase.writeEvent("ViewModelLoginRetailServerDiscoveryFailed", 44402, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Could not resolve retail server url from locator service at '{0}' for tenantId: '{1}'. Error was: '{2}'.");
        };
        RetailLogger.viewModelLoginRetrieveDeviceInformationFailed = function () {
            TsLogging.LoggerBase.writeEvent("ViewModelLoginRetrieveDeviceInformationFailed", 44403, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "The device token and/or device identifier could not be found in persistent store during logon attempt.");
        };
        RetailLogger.viewModelLoginFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("viewModelLoginFailed", 44404, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "LoginViewModel Logon failed: error message = '{1}'.");
        };
        RetailLogger.viewModelDeleteExpiredSessionFailed = function (statusText) {
            TsLogging.LoggerBase.writeEvent("ViewModelDeleteExpiredSessionFailed", 44500, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "The request to delete expired session records failed with error: {0}.");
        };
        RetailLogger.viewModelRetrieveBlobStorageUriFailed = function () {
            TsLogging.LoggerBase.writeEvent("ViewModelRetrieveBlobStorageUriFailed", 44501, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Failed to retrieve download session url from blob storage. Some download sessions are failed to download.");
        };
        RetailLogger.viewModelGetTerminalIdFailed = function () {
            TsLogging.LoggerBase.writeEvent("ViewModelGetTerminalIdFailed", 44502, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Failed to get terminalId.");
        };
        RetailLogger.viewModelGetTerminalDataStoreNameFailed = function (terminalId) {
            TsLogging.LoggerBase.writeEvent("ViewModelGetTerminalDataStoreNameFailed", 44503, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "AsyncServiceViewModel.GetTerminalDataStoreNameAsync(), Failed to get data store name by terminalId: {0}.");
        };
        RetailLogger.viewModelGetDownloadIntervalFailed = function (dataStoreName) {
            TsLogging.LoggerBase.writeEvent("ViewModelGetDownloadIntervalFailed", 44504, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "AsyncServiceViewModel.GetDownloadIntervalAsync(), Failed to get download interval from data store name: {0 }.");
        };
        RetailLogger.viewModelCheckInitialSyncFailed = function (statusText) {
            TsLogging.LoggerBase.writeEvent("ViewModelCheckInitialSyncFailed", 44505, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "CheckInitialDataSyncResponseMessage: {0}.");
        };
        RetailLogger.viewModelGetDownloadSessionsFailed = function (dataStoreName) {
            TsLogging.LoggerBase.writeEvent("ViewModelGetDownloadSessionsFailed", 44506, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "AsyncServiceViewModel.GetDownloadSessionsAsync(), Failed to get download sessions from data store name: {0}.");
        };
        RetailLogger.viewModelDownloadFileFailed = function (statusText) {
            TsLogging.LoggerBase.writeEvent("ViewModelDownloadFileFailed", 44507, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "DownloadFileResponseMessage: {0}.");
        };
        RetailLogger.viewModelDownloadFileBrokerRequestFailed = function (errorMessage) {
            TsLogging.LoggerBase.writeEvent("ViewModelDownloadFileBrokerRequestFailed", 44508, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Microsoft.Dynamics.Commerce.ClientBroker.DownloadFileRequest.executeAsync() Failed. Errors: {0}");
        };
        RetailLogger.viewModelApplyToOfflineDbFailed = function (statusText) {
            TsLogging.LoggerBase.writeEvent("ViewModelApplyToOfflineDbFailed", 44509, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "AsyncClientResponseMessage: {0}");
        };
        RetailLogger.viewModelApplyToOfflineDbBrokerRequestFailed = function (statusText) {
            TsLogging.LoggerBase.writeEvent("ViewModelApplyToOfflineDbBrokerRequestFailed", 44510, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Microsoft.Dynamics.Commerce.ClientBroker.ApplyToOfflineDatabaseRequest.executeAsync(() Failed. Errors: {0}");
        };
        RetailLogger.viewModelUpdateDownloadSessionStatusBrokerRequestFailed = function (statusText) {
            TsLogging.LoggerBase.writeEvent("ViewModelUpdateDownloadSessionStatusBrokerRequestFailed", 44511, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "AsyncClientResponseMessage: {0}");
        };
        RetailLogger.viewModelUpdateDownloadSessionStatusFailed = function () {
            TsLogging.LoggerBase.writeEvent("ViewModelUpdateDownloadSessionStatusFailed", 44512, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "AsyncServiceViewModel.UpdateDownloadSessionAsync(), Failed to update download session status");
        };
        RetailLogger.viewModelGetUploadIntervalFailed = function (dataStoreName) {
            TsLogging.LoggerBase.writeEvent("ViewModelGetUploadIntervalFailed", 44513, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "AsyncServiceViewModel.GetUploadIntervalAsync(), Failed to get upload interval for data store name: {0}");
        };
        RetailLogger.viewModelGetUploadJobDefinitionsFailed = function (dataStoreName) {
            TsLogging.LoggerBase.writeEvent("ViewModelGetUploadJobDefinitionsFailed", 44514, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "AsyncServiceViewModel.GetUploadJobDefinitionsAsync(), Failed to get upload job definitions for data store name: {0}");
        };
        RetailLogger.viewModelLoadUploadTransactionsFailed = function (statusText) {
            TsLogging.LoggerBase.writeEvent("ViewModelLoadUploadTransactionsFailed", 44515, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "LoadUploadTransactionResponseMessage: {0}");
        };
        RetailLogger.viewModelSyncOfflineTransactionsFailed = function () {
            TsLogging.LoggerBase.writeEvent("ViewModelSyncOfflineTransactionsFailed", 44516, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "AsyncServiceViewModel.SyncOfflineTransactionAsync(), Failed to post offline transaction data.");
        };
        RetailLogger.viewModelPurgeOfflineTransactionsFailed = function (statusText) {
            TsLogging.LoggerBase.writeEvent("ViewModelPurgeOfflineTransactionsFailed", 44517, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "AsyncClientResponseMessage: {0}.");
        };
        RetailLogger.viewModelGetDownloadLinkFailed = function (dataStoreName, downloadSessionId) {
            TsLogging.LoggerBase.writeEvent("ViewModelGetDownloadLinkFailed", 44518, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "AsyncServiceViewModel.GetDownloadLinkAsync(), Failed to get download link from data store name: {0} and download session id: {1}.");
        };
        RetailLogger.viewModelGetOfflineSyncStatsFailed = function (statusText) {
            TsLogging.LoggerBase.writeEvent("ViewModelGetOfflineSyncStatsFailed", 44519, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Failed to load offline sync records from offline database. Error message: {0}.");
        };
        RetailLogger.viewModelProductDetailsComponentsNotInKit = function () {
            TsLogging.LoggerBase.writeEvent("viewModelProductDetailsComponentsNotInKit", 44600, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Verbose, [], "", "", "ProductDetailsViewModel Components are not defined for this kit.");
        };
        RetailLogger.viewModelProductDetailsKitVariantNotFound = function (kitVariantId, productId) {
            TsLogging.LoggerBase.writeEvent("viewModelProductDetailsKitVariantNotFound", 44601, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "ProductDetailsViewModel Kit Variant {0} not found as part of the variant information in product id {1}.");
        };
        RetailLogger.viewModelKitDisassemblyRetrievedKitProduct = function () {
            TsLogging.LoggerBase.writeEvent("viewModelKitDisassemblyRetrievedKitProduct", 44700, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Verbose, [], "", "", "KitDisassemblyViewModel Retrieved kit product.");
        };
        RetailLogger.viewModelKitDisassemblyKitDisassemblyBlocked = function () {
            TsLogging.LoggerBase.writeEvent("viewModelKitDisassemblyKitDisassemblyBlocked", 44701, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "KitDisassemblyViewModel Kit is blocked from being disassembled at a register.");
        };
        RetailLogger.viewModelAddressAddEditGetAddressFromZipCodeFailed = function () {
            TsLogging.LoggerBase.writeEvent("viewModelAddressAddEditGetAddressFromZipCodeFailed", 44800, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "AddressAddEditViewModel getAddressFromZipCodeAsync() failed.");
        };
        RetailLogger.viewModelGetAffiliationsFailed = function () {
            TsLogging.LoggerBase.writeEvent("viewModelGetAffiliationsFailed", 44900, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "AffiliationsViewModel getAffiliationsAsync() failed.");
        };
        RetailLogger.viewModelPriceCheckContextEntitySetNone = function () {
            TsLogging.LoggerBase.writeEvent("viewModelPriceCheckContextEntitySetNone", 45000, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "PriceCheckViewModel actionOnContextEntitySetCompletion() A method with PriceCheckContextEntitySet ID 'None' was reported.");
        };
        RetailLogger.viewModelPriceCheckContextEntitySetMultipleTimes = function (entitySetId) {
            TsLogging.LoggerBase.writeEvent("viewModelPriceCheckContextEntitySetMultipleTimes", 45001, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "PriceCheckViewModel actionOnContextEntitySetCompletion() A method with PriceCheckContextEntitySet ID {0} was reported multiple times.");
        };
        RetailLogger.viewModelPriceCheckContextEntitySetNoMethod = function () {
            TsLogging.LoggerBase.writeEvent("viewModelPriceCheckContextEntitySetNoMethod", 45002, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "PriceCheckViewModel actionOnContextEntitySetCompletion() The value PriceCheckContextEntitySet.All does not represent the number of methods that can be run.");
        };
        RetailLogger.viewModelPriceCheckGetProductPriceFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("viewModelPriceCheckGetProductPriceFailed", 45003, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "PriceCheckViewModel load.getProductPrice() failed when getting product price: error message = '{1}'.");
        };
        RetailLogger.viewModelPriceCheckGetCustomerFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("viewModelPriceCheckGetCustomerFailed", 45004, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "PriceCheckViewModel load.getCustomer() failed when getting customer: error message = '{1}'.");
        };
        RetailLogger.viewModelPriceCheckGetStoreDetailsFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("viewModelPriceCheckGetStoreDetailsFailed", 45005, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "PriceCheckViewModel load.getStoreDetailsAsync() failed when getting store: error message = '{1}'.");
        };
        RetailLogger.viewModelPriceCheckGetActivePriceFailed = function (errorCode, errorMessage) {
            TsLogging.LoggerBase.writeEvent("viewModelPriceCheckGetActivePriceFailed", 45006, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "PriceCheckViewModel load.getProductPrice() failed when getting product price: error code = '{0}', error message = '{1}'.");
        };
        RetailLogger.viewModelPaymentCardSwipeNotSupported = function (operationId) {
            TsLogging.LoggerBase.writeEvent("viewModelPaymentCardSwipeNotSupported", 45100, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "PaymentViewModel Card swipe is not supported for payment operation id {0}.");
        };
        RetailLogger.viewModelCustomerAddEditUnknownCustomerType = function (customerType) {
            TsLogging.LoggerBase.writeEvent("viewModelCustomerAddEditUnknownCustomerType", 45200, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "CustomerAddEditViewModel Unknown CustomerType: {0}.");
        };
        RetailLogger.viewModelStockCountDetailsSearchProductsByItemsFailed = function () {
            TsLogging.LoggerBase.writeEvent("viewModelStockCountDetailsSearchProductsByItemsFailed", 45300, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "StockCountDetailsViewModel Failed to do operation searchProductsByItemIdsAsync.");
        };
        RetailLogger.viewModelStoreOperationsGetCurrenciesForStoreFailed = function () {
            TsLogging.LoggerBase.writeEvent("viewModelStoreOperationsGetCurrenciesForStoreFailed", 45400, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "StoreOperationsViewModel getCurrenciesForCurrentStoreAsync() failed.");
        };
        RetailLogger.viewModelPickingAndReceivingDetailsSearchProductsByIdFailed = function () {
            TsLogging.LoggerBase.writeEvent("viewModelPickingAndReceivingDetailsSearchProductsByIdFailed", 45500, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "PickingAndReceivingDetailsViewModel Failed to do operation searchProductsByItemIdsAsync.");
        };
        RetailLogger.cloudPosBrowserNotSupported = function (userAgentDetails, errorDetails) {
            TsLogging.LoggerBase.writeEvent("CloudPosBrowserNotSupported", 45600, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "The browser being used ({0}) is not supported. {1}.");
        };
        RetailLogger.coreOperationValidatorsNoCartOnCartValidator = function (src) {
            TsLogging.LoggerBase.writeEvent("operationValidatorFailed", 45800, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "OperationValidators validator {0} failed - Cart is undefined.");
        };
        RetailLogger.viewModelProductSearchViewModelSearchProductsByTextFailed = function (searchText, refinerValues, error) {
            TsLogging.LoggerBase.writeEvent("viewModelProductSearchViewModelSearchProductsByTextFailed", 46000, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Search by text failed. Search text: '{0}', Refiner values: '{1}', Error: {2}.");
        };
        RetailLogger.viewModelProductSearchViewModelGetRefinersByTextFailed = function (searchText, error) {
            TsLogging.LoggerBase.writeEvent("viewModelProductSearchViewModelGetRefinersByTextFailed", 46001, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Get refiners by text failed. Search text: '{0}' Error: {1}.");
        };
        RetailLogger.viewModelProductSearchViewModelGetRefinerValuesByTextFailed = function (searchText, refinerId, refinerSourceValue, error) {
            TsLogging.LoggerBase.writeEvent("viewModelProductSearchViewModelGetRefinerValuesByTextFailed", 46002, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Get refiner values by text failed. Search text: '{0}', Refiner Id: '{1}', Refiner source value: '{2}', Error: {3}.");
        };
        RetailLogger.viewModelProductsViewModelAddItemsToCart = function (itemDetails, isQuickSale) {
            TsLogging.LoggerBase.writeEvent("viewModelProductsViewModelAddItemsToCart", 46050, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The 'Add Item' action in the Products view model has been initiated. Item details: {0}, Is quick sale: '{1}'");
        };
        RetailLogger.viewsModelProductsViewModelSearchProductsByCategoryFailed = function (categoryId, refinerValues, error) {
            TsLogging.LoggerBase.writeEvent("viewsModelProductsViewModelSearchProductsByCategoryFailed", 46051, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Search by category failed. Category Id: '{0}', Refiner values: '{1}', Error: {2}.");
        };
        RetailLogger.viewsModelProductsViewModelGetRefinersByCategoryFailed = function (categoryId, error) {
            TsLogging.LoggerBase.writeEvent("viewsModelProductsViewModelGetRefinersByCategoryFailed", 46052, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Get refiners by category failed. Category Id: '{0}', Error: {1}.");
        };
        RetailLogger.viewsModelProductsViewModelGetRefinerValuesByCategoryFailed = function (categoryId, refinerId, refinerSourceValue, error) {
            TsLogging.LoggerBase.writeEvent("viewsModelProductsViewModelGetRefinerValuesByCategoryFailed", 46053, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Get refiner values by category failed. Category Id: '{0}', Refiner Id: '{1}', Refiner source value: '{2}', Error: {3}.");
        };
        RetailLogger.viewModelProductsViewModelGetProductDetailsFailed = function (productSearchCriteria, error) {
            TsLogging.LoggerBase.writeEvent("viewModelProductsViewModelGetProductDetailsFailed", 46054, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get product details by search criteria failed. Search criteria: {0}, Error: '{1}'");
        };
        RetailLogger.viewModelSearchViewModelAddCustomerToCartFailed = function (customerAccountNumber, error) {
            TsLogging.LoggerBase.writeEvent("viewModelSearchViewModelAddCustomerToCartFailed", 46100, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Add customer to cart failed. Customer account number: {0}, Error: {1}");
        };
        RetailLogger.viewModelSearchViewModelGetProductDetailsFailed = function (productSearchCriteria, error) {
            TsLogging.LoggerBase.writeEvent("viewModelSearchViewModelGetProductDetailsFailed", 46101, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get product details by search criteria failed. Search criteria: {0}, Error: '{1}'");
        };
        RetailLogger.taskRecorderContinueRecording = function (sessionId, sessionName) {
            TsLogging.LoggerBase.writeEvent("TaskRecorderContinueRecording", 46150, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "The session ID:{0}, Name:{1} - RecordingViewModel continueRecording()");
        };
        RetailLogger.taskRecorderPauseRecording = function (sessionId, sessionName) {
            TsLogging.LoggerBase.writeEvent("TaskRecorderPauseRecording", 46151, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "The session ID:{0}, Name:{1} - RecordingViewModel pauseRecording()");
        };
        RetailLogger.taskRecorderStopRecording = function (sessionId, sessionName) {
            TsLogging.LoggerBase.writeEvent("TaskRecorderStopRecording", 46152, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "The session ID:{0}, Name:{1} - RecordingViewModel stopRecording()");
        };
        RetailLogger.taskRecorderEndTask = function (sessionId, sessionName) {
            TsLogging.LoggerBase.writeEvent("TaskRecorderEndTask", 46153, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "The session ID:{0}, Name:{1} - RecordingViewModel endTask()");
        };
        RetailLogger.taskRecorderHandleAction = function (actionText) {
            TsLogging.LoggerBase.writeEvent("TaskRecorderHandleAction", 46154, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "{0} - TaskRecorderEventListener handleAction()");
        };
        RetailLogger.taskRecorderScreenshotsUploadingFailed = function (errors) {
            TsLogging.LoggerBase.writeEvent("TaskRecorderScreenshotsUploadingFailed", 46155, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Screenshots uploading to Azure failed. Errors: {0} .");
        };
        RetailLogger.taskRecorderDownloadFile = function (sourceUrl, destinationPath) {
            TsLogging.LoggerBase.writeEvent("TaskRecorderDownloadFile", 46156, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "Source url: {0} , Destination path: {1} . - TaskRecorderDownloadHelper downloadFile()");
        };
        RetailLogger.taskRecorderShowSaveDialog = function (suggestedFileName, fileTypeChoice) {
            TsLogging.LoggerBase.writeEvent("TaskRecorderShowSaveDialog", 46157, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "Suggested file name: {0} , File type choice: {1} . - TaskRecorderDialogHelper showSaveDialog()");
        };
        RetailLogger.taskRecorderSavingFileFailed = function (errors) {
            TsLogging.LoggerBase.writeEvent("TaskRecorderSavingFileFailed", 46158, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Saving file failed. Errors: {0} .");
        };
        RetailLogger.taskRecorderSavingFileFinished = function (url) {
            TsLogging.LoggerBase.writeEvent("TaskRecorderSaveFileFinished", 46159, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "Saving file was finished. URL: {0} . - TaskRecorderManager saveFile()");
        };
        RetailLogger.taskRecorderSavingFileCanceled = function (url) {
            TsLogging.LoggerBase.writeEvent("TaskRecorderSavingFileCanceled", 46160, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "Saving file was canceled. URL: {0} . - TaskRecorderManager saveFile()");
        };
        RetailLogger.taskRecorderFileWasSaved = function (fileName) {
            TsLogging.LoggerBase.writeEvent("TaskRecorderFileWasSaved", 46161, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "File {0} was saved. - TaskRecorderDownloadHelper downloadFile()");
        };
        RetailLogger.taskRecorderSaveXMLFailed = function (sessionId, errors) {
            TsLogging.LoggerBase.writeEvent("taskRecorderSaveXMLFailed", 46162, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Some error occurred in exporting the XML file. Session id: {0}. Error details: {1}. - CompleteRecordingViewModel saveRecording()");
        };
        RetailLogger.taskRecorderSaveTrainingDocumentFailed = function (sessionId, errors) {
            TsLogging.LoggerBase.writeEvent("taskRecorderSaveTrainingDocumentFailed", 46163, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Some error occurred in exporting the word document. Session id: {0}. Error details: {1}. - CompleteRecordingViewModel saveWordDocument()");
        };
        RetailLogger.taskRecorderDeleteFolderFromLocalStorageFailed = function (folder, errors) {
            TsLogging.LoggerBase.writeEvent("taskRecorderDeleteFolderFromLocalStorageFailed", 46164, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Delete folder '{0}' from local storage failed. Errors: {1}.");
        };
        RetailLogger.taskRecorderSaveBpmPackageFailed = function (sessionId, errors) {
            TsLogging.LoggerBase.writeEvent("taskRecorderSaveBpmPackageFailed", 46165, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Some error occurred in exporting the BPM package. Session id: {0}. Error details: {1}. - CompleteRecordingViewModel saveBpmPackage()");
        };
        RetailLogger.taskRecorderSaveSessionAsRecordingBundleFailed = function (sessionId) {
            TsLogging.LoggerBase.writeEvent("taskRecorderSaveSessionAsRecordingBundleFailed", 46166, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Some error occurred in saving session as recording bundle. Session id: {0}. - CompleteRecordingViewModel saveToThisPC()");
        };
        RetailLogger.viewsAsyncImageControlInvalidDefaultImage = function () {
            TsLogging.LoggerBase.writeEvent("viewsAsyncImageControlInvalidDefaultImage", 46300, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Invalid default image set. It is either null or undefined.");
        };
        Object.defineProperty(RetailLogger, "genericError",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "genericError", Object.getOwnPropertyDescriptor(RetailLogger, "genericError")));
        Object.defineProperty(RetailLogger, "genericWarning",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "genericWarning", Object.getOwnPropertyDescriptor(RetailLogger, "genericWarning")));
        Object.defineProperty(RetailLogger, "genericInfo",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "genericInfo", Object.getOwnPropertyDescriptor(RetailLogger, "genericInfo")));
        Object.defineProperty(RetailLogger, "extendedCritical",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.CustomerContent),
                __param(2, Commerce.CustomerContent),
                __param(3, Commerce.CustomerContent),
                __param(4, Commerce.CustomerContent),
                __param(5, Commerce.CustomerContent),
                __param(6, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String, String, String, String, String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "extendedCritical", Object.getOwnPropertyDescriptor(RetailLogger, "extendedCritical")));
        Object.defineProperty(RetailLogger, "extendedError",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.CustomerContent),
                __param(2, Commerce.CustomerContent),
                __param(3, Commerce.CustomerContent),
                __param(4, Commerce.CustomerContent),
                __param(5, Commerce.CustomerContent),
                __param(6, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String, String, String, String, String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "extendedError", Object.getOwnPropertyDescriptor(RetailLogger, "extendedError")));
        Object.defineProperty(RetailLogger, "extendedWarning",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.CustomerContent),
                __param(2, Commerce.CustomerContent),
                __param(3, Commerce.CustomerContent),
                __param(4, Commerce.CustomerContent),
                __param(5, Commerce.CustomerContent),
                __param(6, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String, String, String, String, String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "extendedWarning", Object.getOwnPropertyDescriptor(RetailLogger, "extendedWarning")));
        Object.defineProperty(RetailLogger, "extendedInformational",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.CustomerContent),
                __param(2, Commerce.CustomerContent),
                __param(3, Commerce.CustomerContent),
                __param(4, Commerce.CustomerContent),
                __param(5, Commerce.CustomerContent),
                __param(6, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String, String, String, String, String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "extendedInformational", Object.getOwnPropertyDescriptor(RetailLogger, "extendedInformational")));
        Object.defineProperty(RetailLogger, "extendedVerbose",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.CustomerContent),
                __param(2, Commerce.CustomerContent),
                __param(3, Commerce.CustomerContent),
                __param(4, Commerce.CustomerContent),
                __param(5, Commerce.CustomerContent),
                __param(6, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String, String, String, String, String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "extendedVerbose", Object.getOwnPropertyDescriptor(RetailLogger, "extendedVerbose")));
        Object.defineProperty(RetailLogger, "appLaunch",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.SystemData),
                __param(2, Commerce.CustomerContent),
                __param(3, Commerce.CustomerContent),
                __param(4, Commerce.SystemData),
                __param(5, Commerce.AccessControlData),
                __param(6, Commerce.AccessControlData),
                __param(7, Commerce.AccessControlData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, Boolean, String, String, String, String, String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "appLaunch", Object.getOwnPropertyDescriptor(RetailLogger, "appLaunch")));
        Object.defineProperty(RetailLogger, "appUnhandledError",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.CustomerContent),
                __param(2, Commerce.CustomerContent),
                __param(3, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String, String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "appUnhandledError", Object.getOwnPropertyDescriptor(RetailLogger, "appUnhandledError")));
        Object.defineProperty(RetailLogger, "logon",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "logon", Object.getOwnPropertyDescriptor(RetailLogger, "logon")));
        Object.defineProperty(RetailLogger, "logoff",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "logoff", Object.getOwnPropertyDescriptor(RetailLogger, "logoff")));
        Object.defineProperty(RetailLogger, "userMessageDisplay",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent),
                __param(2, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "userMessageDisplay", Object.getOwnPropertyDescriptor(RetailLogger, "userMessageDisplay")));
        Object.defineProperty(RetailLogger, "errorMessageDisplay",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "errorMessageDisplay", Object.getOwnPropertyDescriptor(RetailLogger, "errorMessageDisplay")));
        Object.defineProperty(RetailLogger, "applicationContextSetupLanguagesInvalidLanguage",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationContextSetupLanguagesInvalidLanguage", Object.getOwnPropertyDescriptor(RetailLogger, "applicationContextSetupLanguagesInvalidLanguage")));
        Object.defineProperty(RetailLogger, "applicationStylesheetsLoadFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.SystemData),
                __param(2, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationStylesheetsLoadFailed", Object.getOwnPropertyDescriptor(RetailLogger, "applicationStylesheetsLoadFailed")));
        Object.defineProperty(RetailLogger, "applicationLocalStorageNotAvailable",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationLocalStorageNotAvailable", Object.getOwnPropertyDescriptor(RetailLogger, "applicationLocalStorageNotAvailable")));
        Object.defineProperty(RetailLogger, "applicationContextApplicationContextEntitySetInvalid",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationContextApplicationContextEntitySetInvalid", Object.getOwnPropertyDescriptor(RetailLogger, "applicationContextApplicationContextEntitySetInvalid")));
        Object.defineProperty(RetailLogger, "applicationContextApplicationContextEntitySetMultipleTimes",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationContextApplicationContextEntitySetMultipleTimes", Object.getOwnPropertyDescriptor(RetailLogger, "applicationContextApplicationContextEntitySetMultipleTimes")));
        Object.defineProperty(RetailLogger, "applicationFailedToParseError",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationFailedToParseError", Object.getOwnPropertyDescriptor(RetailLogger, "applicationFailedToParseError")));
        Object.defineProperty(RetailLogger, "applicationFailedToParseErrorInvalidJson",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationFailedToParseErrorInvalidJson", Object.getOwnPropertyDescriptor(RetailLogger, "applicationFailedToParseErrorInvalidJson")));
        Object.defineProperty(RetailLogger, "applicationGlobalizationResourcesLoadFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.SystemData),
                __param(2, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationGlobalizationResourcesLoadFailed", Object.getOwnPropertyDescriptor(RetailLogger, "applicationGlobalizationResourcesLoadFailed")));
        Object.defineProperty(RetailLogger, "applicationContextLoadCategoriesFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationContextLoadCategoriesFailed", Object.getOwnPropertyDescriptor(RetailLogger, "applicationContextLoadCategoriesFailed")));
        Object.defineProperty(RetailLogger, "applicationLoadChannelConfigurationFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.SystemData),
                __param(2, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationLoadChannelConfigurationFailed", Object.getOwnPropertyDescriptor(RetailLogger, "applicationLoadChannelConfigurationFailed")));
        Object.defineProperty(RetailLogger, "applicationContextSetupDebitCashbackLimitFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationContextSetupDebitCashbackLimitFailed", Object.getOwnPropertyDescriptor(RetailLogger, "applicationContextSetupDebitCashbackLimitFailed")));
        Object.defineProperty(RetailLogger, "applicationContextSetupCardTypesFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationContextSetupCardTypesFailed", Object.getOwnPropertyDescriptor(RetailLogger, "applicationContextSetupCardTypesFailed")));
        Object.defineProperty(RetailLogger, "applicationContextSetupReturnOrderReasonCodesFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationContextSetupReturnOrderReasonCodesFailed", Object.getOwnPropertyDescriptor(RetailLogger, "applicationContextSetupReturnOrderReasonCodesFailed")));
        Object.defineProperty(RetailLogger, "applicationContextSetupCustomerTypesFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationContextSetupCustomerTypesFailed", Object.getOwnPropertyDescriptor(RetailLogger, "applicationContextSetupCustomerTypesFailed")));
        Object.defineProperty(RetailLogger, "applicationContextSetupCustomerGroupsFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationContextSetupCustomerGroupsFailed", Object.getOwnPropertyDescriptor(RetailLogger, "applicationContextSetupCustomerGroupsFailed")));
        Object.defineProperty(RetailLogger, "applicationContextSetupHardwareStationProfileFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationContextSetupHardwareStationProfileFailed", Object.getOwnPropertyDescriptor(RetailLogger, "applicationContextSetupHardwareStationProfileFailed")));
        Object.defineProperty(RetailLogger, "applicationContextSetupLanguagesFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationContextSetupLanguagesFailed", Object.getOwnPropertyDescriptor(RetailLogger, "applicationContextSetupLanguagesFailed")));
        Object.defineProperty(RetailLogger, "applicationContextSetupReceiptOptionsFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationContextSetupReceiptOptionsFailed", Object.getOwnPropertyDescriptor(RetailLogger, "applicationContextSetupReceiptOptionsFailed")));
        Object.defineProperty(RetailLogger, "applicationContextSetupCashDeclarationsFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationContextSetupCashDeclarationsFailed", Object.getOwnPropertyDescriptor(RetailLogger, "applicationContextSetupCashDeclarationsFailed")));
        Object.defineProperty(RetailLogger, "applicationGlobalizationResourcesLoading",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationGlobalizationResourcesLoading", Object.getOwnPropertyDescriptor(RetailLogger, "applicationGlobalizationResourcesLoading")));
        Object.defineProperty(RetailLogger, "applicationGlobalizationResourcesLanguageResolved",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationGlobalizationResourcesLanguageResolved", Object.getOwnPropertyDescriptor(RetailLogger, "applicationGlobalizationResourcesLanguageResolved")));
        Object.defineProperty(RetailLogger, "applicationLoadEnvironmentConfigurationStorageLoadSucceeded",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationLoadEnvironmentConfigurationStorageLoadSucceeded", Object.getOwnPropertyDescriptor(RetailLogger, "applicationLoadEnvironmentConfigurationStorageLoadSucceeded")));
        Object.defineProperty(RetailLogger, "applicationLoadEnvironmentConfigurationServerLoadSucceeded",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationLoadEnvironmentConfigurationServerLoadSucceeded", Object.getOwnPropertyDescriptor(RetailLogger, "applicationLoadEnvironmentConfigurationServerLoadSucceeded")));
        Object.defineProperty(RetailLogger, "applicationLoadEnvironmentConfigurationServerLoadFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "applicationLoadEnvironmentConfigurationServerLoadFailed", Object.getOwnPropertyDescriptor(RetailLogger, "applicationLoadEnvironmentConfigurationServerLoadFailed")));
        Object.defineProperty(RetailLogger, "modelManagersRetailServerRequestStarted",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "modelManagersRetailServerRequestStarted", Object.getOwnPropertyDescriptor(RetailLogger, "modelManagersRetailServerRequestStarted")));
        Object.defineProperty(RetailLogger, "modelManagersRetailServerRequestError",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent),
                __param(2, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "modelManagersRetailServerRequestError", Object.getOwnPropertyDescriptor(RetailLogger, "modelManagersRetailServerRequestError")));
        Object.defineProperty(RetailLogger, "modelManagersRetailServerRequestFinished",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "modelManagersRetailServerRequestFinished", Object.getOwnPropertyDescriptor(RetailLogger, "modelManagersRetailServerRequestFinished")));
        Object.defineProperty(RetailLogger, "modelManagersCommerceRuntimeRequestStarted",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "modelManagersCommerceRuntimeRequestStarted", Object.getOwnPropertyDescriptor(RetailLogger, "modelManagersCommerceRuntimeRequestStarted")));
        Object.defineProperty(RetailLogger, "modelManagersCommerceRuntimeRequestError",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent),
                __param(2, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "modelManagersCommerceRuntimeRequestError", Object.getOwnPropertyDescriptor(RetailLogger, "modelManagersCommerceRuntimeRequestError")));
        Object.defineProperty(RetailLogger, "modelManagersCommerceRuntimeRequestFinished",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "modelManagersCommerceRuntimeRequestFinished", Object.getOwnPropertyDescriptor(RetailLogger, "modelManagersCommerceRuntimeRequestFinished")));
        Object.defineProperty(RetailLogger, "modelManagersCheckDownloadCompleteRequestError",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "modelManagersCheckDownloadCompleteRequestError", Object.getOwnPropertyDescriptor(RetailLogger, "modelManagersCheckDownloadCompleteRequestError")));
        Object.defineProperty(RetailLogger, "modelManagersLocatorServiceRequestStarted",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "modelManagersLocatorServiceRequestStarted", Object.getOwnPropertyDescriptor(RetailLogger, "modelManagersLocatorServiceRequestStarted")));
        Object.defineProperty(RetailLogger, "modelManagersLocatorServiceRequestException",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "modelManagersLocatorServiceRequestException", Object.getOwnPropertyDescriptor(RetailLogger, "modelManagersLocatorServiceRequestException")));
        Object.defineProperty(RetailLogger, "modelManagersLocatorServiceRequestFinished",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "modelManagersLocatorServiceRequestFinished", Object.getOwnPropertyDescriptor(RetailLogger, "modelManagersLocatorServiceRequestFinished")));
        Object.defineProperty(RetailLogger, "modelManagersServerResponseStatusCode",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [Number]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "modelManagersServerResponseStatusCode", Object.getOwnPropertyDescriptor(RetailLogger, "modelManagersServerResponseStatusCode")));
        Object.defineProperty(RetailLogger, "modelManagersChainedRequestFactoryShiftTransferFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.SystemData),
                __param(2, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "modelManagersChainedRequestFactoryShiftTransferFailed", Object.getOwnPropertyDescriptor(RetailLogger, "modelManagersChainedRequestFactoryShiftTransferFailed")));
        Object.defineProperty(RetailLogger, "modelManagersChainedRequestFactoryCartTransferToOfflineFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "modelManagersChainedRequestFactoryCartTransferToOfflineFailed", Object.getOwnPropertyDescriptor(RetailLogger, "modelManagersChainedRequestFactoryCartTransferToOfflineFailed")));
        Object.defineProperty(RetailLogger, "modelManagersODataExecuteBatchSuccess",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "modelManagersODataExecuteBatchSuccess", Object.getOwnPropertyDescriptor(RetailLogger, "modelManagersODataExecuteBatchSuccess")));
        Object.defineProperty(RetailLogger, "modelManagersODataExecuteBatchFailed",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.SystemData),
                __param(2, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "modelManagersODataExecuteBatchFailed", Object.getOwnPropertyDescriptor(RetailLogger, "modelManagersODataExecuteBatchFailed")));
        Object.defineProperty(RetailLogger, "modelManagersRetailServerManagerFactoryCreate",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "modelManagersRetailServerManagerFactoryCreate", Object.getOwnPropertyDescriptor(RetailLogger, "modelManagersRetailServerManagerFactoryCreate")));
        Object.defineProperty(RetailLogger, "modelManagersCartManagerAddTenderLineToCartFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "modelManagersCartManagerAddTenderLineToCartFailed", Object.getOwnPropertyDescriptor(RetailLogger, "modelManagersCartManagerAddTenderLineToCartFailed")));
        Object.defineProperty(RetailLogger, "modelManagersRetailServerOdataRequestResponse",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "modelManagersRetailServerOdataRequestResponse", Object.getOwnPropertyDescriptor(RetailLogger, "modelManagersRetailServerOdataRequestResponse")));
        Object.defineProperty(RetailLogger, "modelManagersRetailServerOdataRequestErrorResponse",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "modelManagersRetailServerOdataRequestErrorResponse", Object.getOwnPropertyDescriptor(RetailLogger, "modelManagersRetailServerOdataRequestErrorResponse")));
        Object.defineProperty(RetailLogger, "modelManagersChainedRequestFactoryShiftTransferToOnlineCreateFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "modelManagersChainedRequestFactoryShiftTransferToOnlineCreateFailed", Object.getOwnPropertyDescriptor(RetailLogger, "modelManagersChainedRequestFactoryShiftTransferToOnlineCreateFailed")));
        Object.defineProperty(RetailLogger, "modelManagersChainedRequestFactoryShiftTransferToOnlineDeleteFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "modelManagersChainedRequestFactoryShiftTransferToOnlineDeleteFailed", Object.getOwnPropertyDescriptor(RetailLogger, "modelManagersChainedRequestFactoryShiftTransferToOnlineDeleteFailed")));
        Object.defineProperty(RetailLogger, "coreCannotMapResourceMessage",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreCannotMapResourceMessage", Object.getOwnPropertyDescriptor(RetailLogger, "coreCannotMapResourceMessage")));
        Object.defineProperty(RetailLogger, "coreCannotMapErrorCode",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreCannotMapErrorCode", Object.getOwnPropertyDescriptor(RetailLogger, "coreCannotMapErrorCode")));
        Object.defineProperty(RetailLogger, "corePropertyMissingInDeviceActivationSequence",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "corePropertyMissingInDeviceActivationSequence", Object.getOwnPropertyDescriptor(RetailLogger, "corePropertyMissingInDeviceActivationSequence")));
        Object.defineProperty(RetailLogger, "coreRetailOperationStarted",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.SystemData),
                __param(2, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String, Number]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreRetailOperationStarted", Object.getOwnPropertyDescriptor(RetailLogger, "coreRetailOperationStarted")));
        Object.defineProperty(RetailLogger, "coreRetailOperationManagerOverride",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, Number]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreRetailOperationManagerOverride", Object.getOwnPropertyDescriptor(RetailLogger, "coreRetailOperationManagerOverride")));
        Object.defineProperty(RetailLogger, "coreRetailOperationHandlerNotFound",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, Number]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreRetailOperationHandlerNotFound", Object.getOwnPropertyDescriptor(RetailLogger, "coreRetailOperationHandlerNotFound")));
        Object.defineProperty(RetailLogger, "coreRetailOperationCompleted",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.SystemData),
                __param(2, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String, Number]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreRetailOperationCompleted", Object.getOwnPropertyDescriptor(RetailLogger, "coreRetailOperationCompleted")));
        Object.defineProperty(RetailLogger, "coreRetailOperationCanceled",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.SystemData),
                __param(2, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String, Number]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreRetailOperationCanceled", Object.getOwnPropertyDescriptor(RetailLogger, "coreRetailOperationCanceled")));
        Object.defineProperty(RetailLogger, "coreRetailOperationFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.SystemData),
                __param(2, Commerce.SystemData),
                __param(3, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String, Number, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreRetailOperationFailed", Object.getOwnPropertyDescriptor(RetailLogger, "coreRetailOperationFailed")));
        Object.defineProperty(RetailLogger, "coreTriggerExecutionStarted",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreTriggerExecutionStarted", Object.getOwnPropertyDescriptor(RetailLogger, "coreTriggerExecutionStarted")));
        Object.defineProperty(RetailLogger, "coreTriggerExecutionCompleted",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreTriggerExecutionCompleted", Object.getOwnPropertyDescriptor(RetailLogger, "coreTriggerExecutionCompleted")));
        Object.defineProperty(RetailLogger, "coreTriggerExecutionCanceled",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreTriggerExecutionCanceled", Object.getOwnPropertyDescriptor(RetailLogger, "coreTriggerExecutionCanceled")));
        Object.defineProperty(RetailLogger, "coreTriggerExecutionFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreTriggerExecutionFailed", Object.getOwnPropertyDescriptor(RetailLogger, "coreTriggerExecutionFailed")));
        Object.defineProperty(RetailLogger, "coreLogOriginalUnauthorizedRetailServerResponse",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreLogOriginalUnauthorizedRetailServerResponse", Object.getOwnPropertyDescriptor(RetailLogger, "coreLogOriginalUnauthorizedRetailServerResponse")));
        Object.defineProperty(RetailLogger, "coreLogXmlHttpRequestError",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreLogXmlHttpRequestError", Object.getOwnPropertyDescriptor(RetailLogger, "coreLogXmlHttpRequestError")));
        Object.defineProperty(RetailLogger, "coreHelpersProductPropertiesGetTranslation",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreHelpersProductPropertiesGetTranslation", Object.getOwnPropertyDescriptor(RetailLogger, "coreHelpersProductPropertiesGetTranslation")));
        Object.defineProperty(RetailLogger, "coreHelpersProductPropertiesPropertyNotExist",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreHelpersProductPropertiesPropertyNotExist", Object.getOwnPropertyDescriptor(RetailLogger, "coreHelpersProductPropertiesPropertyNotExist")));
        Object.defineProperty(RetailLogger, "coreHelpersProductPropertiesUnitOfMeasureNotExist",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreHelpersProductPropertiesUnitOfMeasureNotExist", Object.getOwnPropertyDescriptor(RetailLogger, "coreHelpersProductPropertiesUnitOfMeasureNotExist")));
        Object.defineProperty(RetailLogger, "coreHelpersProductPropertiesVariantPropertyNotExist",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreHelpersProductPropertiesVariantPropertyNotExist", Object.getOwnPropertyDescriptor(RetailLogger, "coreHelpersProductPropertiesVariantPropertyNotExist")));
        Object.defineProperty(RetailLogger, "coreHelpersProductPropertiesTranslationPropertyNotFound",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreHelpersProductPropertiesTranslationPropertyNotFound", Object.getOwnPropertyDescriptor(RetailLogger, "coreHelpersProductPropertiesTranslationPropertyNotFound")));
        Object.defineProperty(RetailLogger, "coreHelpersProductPropertiesVariantNotFound",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [Number, Number]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreHelpersProductPropertiesVariantNotFound", Object.getOwnPropertyDescriptor(RetailLogger, "coreHelpersProductPropertiesVariantNotFound")));
        Object.defineProperty(RetailLogger, "coreHelpersProductPropertiesInputParameterProductIsUndefined",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreHelpersProductPropertiesInputParameterProductIsUndefined", Object.getOwnPropertyDescriptor(RetailLogger, "coreHelpersProductPropertiesInputParameterProductIsUndefined")));
        Object.defineProperty(RetailLogger, "coreHelpersProductPropertiesInputParameterPropertyNameIsInvalid",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreHelpersProductPropertiesInputParameterPropertyNameIsInvalid", Object.getOwnPropertyDescriptor(RetailLogger, "coreHelpersProductPropertiesInputParameterPropertyNameIsInvalid")));
        Object.defineProperty(RetailLogger, "coreHelpersProductPropertiesProductNotHaveVariant",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, Number]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreHelpersProductPropertiesProductNotHaveVariant", Object.getOwnPropertyDescriptor(RetailLogger, "coreHelpersProductPropertiesProductNotHaveVariant")));
        Object.defineProperty(RetailLogger, "coreHelpersProductPropertiesProductNotHaveProperty",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreHelpersProductPropertiesProductNotHaveProperty", Object.getOwnPropertyDescriptor(RetailLogger, "coreHelpersProductPropertiesProductNotHaveProperty")));
        Object.defineProperty(RetailLogger, "coreHelpersUnrecognizedAddressComponent",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [Number]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreHelpersUnrecognizedAddressComponent", Object.getOwnPropertyDescriptor(RetailLogger, "coreHelpersUnrecognizedAddressComponent")));
        Object.defineProperty(RetailLogger, "coreHelpersInvalidManualDiscountType",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [Number]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreHelpersInvalidManualDiscountType", Object.getOwnPropertyDescriptor(RetailLogger, "coreHelpersInvalidManualDiscountType")));
        Object.defineProperty(RetailLogger, "coreHelpersInvalidCustomerDiscountType",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [Number]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreHelpersInvalidCustomerDiscountType", Object.getOwnPropertyDescriptor(RetailLogger, "coreHelpersInvalidCustomerDiscountType")));
        Object.defineProperty(RetailLogger, "coreHelpersInvalidDiscountLineType",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [Number]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreHelpersInvalidDiscountLineType", Object.getOwnPropertyDescriptor(RetailLogger, "coreHelpersInvalidDiscountLineType")));
        Object.defineProperty(RetailLogger, "coreHelpersUnknownAddressType",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [Number]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreHelpersUnknownAddressType", Object.getOwnPropertyDescriptor(RetailLogger, "coreHelpersUnknownAddressType")));
        Object.defineProperty(RetailLogger, "coreTenderTypeMapOperationHasNoTenderType",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [Number]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreTenderTypeMapOperationHasNoTenderType", Object.getOwnPropertyDescriptor(RetailLogger, "coreTenderTypeMapOperationHasNoTenderType")));
        Object.defineProperty(RetailLogger, "coreBindingHandlersLoadImageFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreBindingHandlersLoadImageFailed", Object.getOwnPropertyDescriptor(RetailLogger, "coreBindingHandlersLoadImageFailed")));
        Object.defineProperty(RetailLogger, "coreFormattersCartLineWrongInputParameters",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreFormattersCartLineWrongInputParameters", Object.getOwnPropertyDescriptor(RetailLogger, "coreFormattersCartLineWrongInputParameters")));
        Object.defineProperty(RetailLogger, "coreTenderTypeMapTenderTypeNotFound",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreTenderTypeMapTenderTypeNotFound", Object.getOwnPropertyDescriptor(RetailLogger, "coreTenderTypeMapTenderTypeNotFound")));
        Object.defineProperty(RetailLogger, "coreApplicationStorageSetItemFailure",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreApplicationStorageSetItemFailure", Object.getOwnPropertyDescriptor(RetailLogger, "coreApplicationStorageSetItemFailure")));
        Object.defineProperty(RetailLogger, "coreApplicationStorageSetItemFailureRecoveryUnsuccessful",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreApplicationStorageSetItemFailureRecoveryUnsuccessful", Object.getOwnPropertyDescriptor(RetailLogger, "coreApplicationStorageSetItemFailureRecoveryUnsuccessful")));
        Object.defineProperty(RetailLogger, "coreLogUserAuthenticationRetailServerResponse",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreLogUserAuthenticationRetailServerResponse", Object.getOwnPropertyDescriptor(RetailLogger, "coreLogUserAuthenticationRetailServerResponse")));
        Object.defineProperty(RetailLogger, "coreRetailCheckOpenDrawerStatusExecutionFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreRetailCheckOpenDrawerStatusExecutionFailed", Object.getOwnPropertyDescriptor(RetailLogger, "coreRetailCheckOpenDrawerStatusExecutionFailed")));
        Object.defineProperty(RetailLogger, "operationLogOffFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "operationLogOffFailed", Object.getOwnPropertyDescriptor(RetailLogger, "operationLogOffFailed")));
        Object.defineProperty(RetailLogger, "operationAddGiftCard",
            __decorate([
                __param(0, Commerce.AccountData),
                __param(1, Commerce.AccountData),
                __param(2, Commerce.AccountData),
                __param(3, Commerce.AccountData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, Number, String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "operationAddGiftCard", Object.getOwnPropertyDescriptor(RetailLogger, "operationAddGiftCard")));
        Object.defineProperty(RetailLogger, "operationIssueGiftCard",
            __decorate([
                __param(0, Commerce.AccountData),
                __param(1, Commerce.AccountData),
                __param(2, Commerce.AccountData),
                __param(3, Commerce.AccountData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, Number, String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "operationIssueGiftCard", Object.getOwnPropertyDescriptor(RetailLogger, "operationIssueGiftCard")));
        Object.defineProperty(RetailLogger, "operationLocateServerUrl",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "operationLocateServerUrl", Object.getOwnPropertyDescriptor(RetailLogger, "operationLocateServerUrl")));
        Object.defineProperty(RetailLogger, "operationUpdateServerUrl",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "operationUpdateServerUrl", Object.getOwnPropertyDescriptor(RetailLogger, "operationUpdateServerUrl")));
        Object.defineProperty(RetailLogger, "operationDeviceActivationUnhandledError",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "operationDeviceActivationUnhandledError", Object.getOwnPropertyDescriptor(RetailLogger, "operationDeviceActivationUnhandledError")));
        Object.defineProperty(RetailLogger, "operationPickingAndReceivingUpdatePurchaseOrderFailed",
            __decorate([
                __param(0, Commerce.AccountData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "operationPickingAndReceivingUpdatePurchaseOrderFailed", Object.getOwnPropertyDescriptor(RetailLogger, "operationPickingAndReceivingUpdatePurchaseOrderFailed")));
        Object.defineProperty(RetailLogger, "operationPickingAndReceivingCommitPurchaseOrderFailed",
            __decorate([
                __param(0, Commerce.AccountData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "operationPickingAndReceivingCommitPurchaseOrderFailed", Object.getOwnPropertyDescriptor(RetailLogger, "operationPickingAndReceivingCommitPurchaseOrderFailed")));
        Object.defineProperty(RetailLogger, "operationPickingAndReceivingGetPurchaseOrderFailed",
            __decorate([
                __param(0, Commerce.AccountData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "operationPickingAndReceivingGetPurchaseOrderFailed", Object.getOwnPropertyDescriptor(RetailLogger, "operationPickingAndReceivingGetPurchaseOrderFailed")));
        Object.defineProperty(RetailLogger, "operationPickingAndReceivingUpdateTransderOrderFailed",
            __decorate([
                __param(0, Commerce.AccountData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "operationPickingAndReceivingUpdateTransderOrderFailed", Object.getOwnPropertyDescriptor(RetailLogger, "operationPickingAndReceivingUpdateTransderOrderFailed")));
        Object.defineProperty(RetailLogger, "operationPickingAndReceivingCommitTransferOrderFailed",
            __decorate([
                __param(0, Commerce.AccountData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "operationPickingAndReceivingCommitTransferOrderFailed", Object.getOwnPropertyDescriptor(RetailLogger, "operationPickingAndReceivingCommitTransferOrderFailed")));
        Object.defineProperty(RetailLogger, "operationPickingAndReceivingGetTransferOrderFailed",
            __decorate([
                __param(0, Commerce.AccountData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "operationPickingAndReceivingGetTransferOrderFailed", Object.getOwnPropertyDescriptor(RetailLogger, "operationPickingAndReceivingGetTransferOrderFailed")));
        Object.defineProperty(RetailLogger, "operationPickingAndReceivingUpdatePickingListFailed",
            __decorate([
                __param(0, Commerce.AccountData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "operationPickingAndReceivingUpdatePickingListFailed", Object.getOwnPropertyDescriptor(RetailLogger, "operationPickingAndReceivingUpdatePickingListFailed")));
        Object.defineProperty(RetailLogger, "operationPickingAndReceivingCommitPickingListFailed",
            __decorate([
                __param(0, Commerce.AccountData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "operationPickingAndReceivingCommitPickingListFailed", Object.getOwnPropertyDescriptor(RetailLogger, "operationPickingAndReceivingCommitPickingListFailed")));
        Object.defineProperty(RetailLogger, "operationPickingAndReceivingGetPickingListFailed",
            __decorate([
                __param(0, Commerce.AccountData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "operationPickingAndReceivingGetPickingListFailed", Object.getOwnPropertyDescriptor(RetailLogger, "operationPickingAndReceivingGetPickingListFailed")));
        Object.defineProperty(RetailLogger, "retailServerRequestRedirection",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "retailServerRequestRedirection", Object.getOwnPropertyDescriptor(RetailLogger, "retailServerRequestRedirection")));
        Object.defineProperty(RetailLogger, "operationItemSaleCreateCartLinesStarted",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "operationItemSaleCreateCartLinesStarted", Object.getOwnPropertyDescriptor(RetailLogger, "operationItemSaleCreateCartLinesStarted")));
        Object.defineProperty(RetailLogger, "operationItemSaleCreateCartLinesFinished",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, Boolean]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "operationItemSaleCreateCartLinesFinished", Object.getOwnPropertyDescriptor(RetailLogger, "operationItemSaleCreateCartLinesFinished")));
        Object.defineProperty(RetailLogger, "operationItemSaleGetProductSaleDetailsStarted",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "operationItemSaleGetProductSaleDetailsStarted", Object.getOwnPropertyDescriptor(RetailLogger, "operationItemSaleGetProductSaleDetailsStarted")));
        Object.defineProperty(RetailLogger, "operationItemSaleGetProductSaleDetailsFinished",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, Boolean]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "operationItemSaleGetProductSaleDetailsFinished", Object.getOwnPropertyDescriptor(RetailLogger, "operationItemSaleGetProductSaleDetailsFinished")));
        Object.defineProperty(RetailLogger, "operationBlindCloseSharedShiftFailedOnRevertToSelfDuringCancellation",
            __decorate([
                __param(0, Commerce.AccountData),
                __param(1, Commerce.EndUserIdentifiableInformation), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [Number, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "operationBlindCloseSharedShiftFailedOnRevertToSelfDuringCancellation", Object.getOwnPropertyDescriptor(RetailLogger, "operationBlindCloseSharedShiftFailedOnRevertToSelfDuringCancellation")));
        Object.defineProperty(RetailLogger, "peripheralsCashDrawerOpening",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "peripheralsCashDrawerOpening", Object.getOwnPropertyDescriptor(RetailLogger, "peripheralsCashDrawerOpening")));
        Object.defineProperty(RetailLogger, "peripheralsMSRKeyboardSwipeParserLog",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "peripheralsMSRKeyboardSwipeParserLog", Object.getOwnPropertyDescriptor(RetailLogger, "peripheralsMSRKeyboardSwipeParserLog")));
        Object.defineProperty(RetailLogger, "peripheralsCompositeBarcodeScannerObjectNotDefined",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "peripheralsCompositeBarcodeScannerObjectNotDefined", Object.getOwnPropertyDescriptor(RetailLogger, "peripheralsCompositeBarcodeScannerObjectNotDefined")));
        Object.defineProperty(RetailLogger, "peripheralsHardwareStationContextActionRequestSucceeded",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "peripheralsHardwareStationContextActionRequestSucceeded", Object.getOwnPropertyDescriptor(RetailLogger, "peripheralsHardwareStationContextActionRequestSucceeded")));
        Object.defineProperty(RetailLogger, "peripheralsHardwareStationContextActionRequestFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "peripheralsHardwareStationContextActionRequestFailed", Object.getOwnPropertyDescriptor(RetailLogger, "peripheralsHardwareStationContextActionRequestFailed")));
        Object.defineProperty(RetailLogger, "peripheralsUnsupportedPrinterType",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "peripheralsUnsupportedPrinterType", Object.getOwnPropertyDescriptor(RetailLogger, "peripheralsUnsupportedPrinterType")));
        Object.defineProperty(RetailLogger, "peripheralsBarcodeScannerGetDeviceSelectorFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "peripheralsBarcodeScannerGetDeviceSelectorFailed", Object.getOwnPropertyDescriptor(RetailLogger, "peripheralsBarcodeScannerGetDeviceSelectorFailed")));
        Object.defineProperty(RetailLogger, "peripheralsBarcodeScannerGetBluetoothDeviceSelectorFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "peripheralsBarcodeScannerGetBluetoothDeviceSelectorFailed", Object.getOwnPropertyDescriptor(RetailLogger, "peripheralsBarcodeScannerGetBluetoothDeviceSelectorFailed")));
        Object.defineProperty(RetailLogger, "peripheralsBarcodeScannerEnableFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "peripheralsBarcodeScannerEnableFailed", Object.getOwnPropertyDescriptor(RetailLogger, "peripheralsBarcodeScannerEnableFailed")));
        Object.defineProperty(RetailLogger, "peripheralsMagneticStripeReaderGetDeviceSelectorFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "peripheralsMagneticStripeReaderGetDeviceSelectorFailed", Object.getOwnPropertyDescriptor(RetailLogger, "peripheralsMagneticStripeReaderGetDeviceSelectorFailed")));
        Object.defineProperty(RetailLogger, "peripheralsMagneticStripeReaderGetBluetoothDeviceSelectorFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "peripheralsMagneticStripeReaderGetBluetoothDeviceSelectorFailed", Object.getOwnPropertyDescriptor(RetailLogger, "peripheralsMagneticStripeReaderGetBluetoothDeviceSelectorFailed")));
        Object.defineProperty(RetailLogger, "peripheralsMagneticStripeReaderInitializeFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "peripheralsMagneticStripeReaderInitializeFailed", Object.getOwnPropertyDescriptor(RetailLogger, "peripheralsMagneticStripeReaderInitializeFailed")));
        Object.defineProperty(RetailLogger, "peripheralsMagneticStripeReaderEnableFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "peripheralsMagneticStripeReaderEnableFailed", Object.getOwnPropertyDescriptor(RetailLogger, "peripheralsMagneticStripeReaderEnableFailed")));
        Object.defineProperty(RetailLogger, "peripheralsProximityOpenDeviceFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "peripheralsProximityOpenDeviceFailed", Object.getOwnPropertyDescriptor(RetailLogger, "peripheralsProximityOpenDeviceFailed")));
        Object.defineProperty(RetailLogger, "peripheralsLongPollingLockGetDataError",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "peripheralsLongPollingLockGetDataError", Object.getOwnPropertyDescriptor(RetailLogger, "peripheralsLongPollingLockGetDataError")));
        Object.defineProperty(RetailLogger, "peripheralsLongPollingLockGetDataUnhandledError",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "peripheralsLongPollingLockGetDataUnhandledError", Object.getOwnPropertyDescriptor(RetailLogger, "peripheralsLongPollingLockGetDataUnhandledError")));
        Object.defineProperty(RetailLogger, "librariesWinJsListViewShown",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "librariesWinJsListViewShown", Object.getOwnPropertyDescriptor(RetailLogger, "librariesWinJsListViewShown")));
        Object.defineProperty(RetailLogger, "librariesWinJsListViewItemClick",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "librariesWinJsListViewItemClick", Object.getOwnPropertyDescriptor(RetailLogger, "librariesWinJsListViewItemClick")));
        Object.defineProperty(RetailLogger, "librariesNumpadEnterKey",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "librariesNumpadEnterKey", Object.getOwnPropertyDescriptor(RetailLogger, "librariesNumpadEnterKey")));
        Object.defineProperty(RetailLogger, "librariesAuthenticationProviderLoginStarted",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "librariesAuthenticationProviderLoginStarted", Object.getOwnPropertyDescriptor(RetailLogger, "librariesAuthenticationProviderLoginStarted")));
        Object.defineProperty(RetailLogger, "librariesAuthenticationProviderLoginFinished",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "librariesAuthenticationProviderLoginFinished", Object.getOwnPropertyDescriptor(RetailLogger, "librariesAuthenticationProviderLoginFinished")));
        Object.defineProperty(RetailLogger, "librariesAuthenticationProviderAcquireTokenStarted",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "librariesAuthenticationProviderAcquireTokenStarted", Object.getOwnPropertyDescriptor(RetailLogger, "librariesAuthenticationProviderAcquireTokenStarted")));
        Object.defineProperty(RetailLogger, "librariesAuthenticationProviderAcquireTokenFinished",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "librariesAuthenticationProviderAcquireTokenFinished", Object.getOwnPropertyDescriptor(RetailLogger, "librariesAuthenticationProviderAcquireTokenFinished")));
        Object.defineProperty(RetailLogger, "helpersActivityHelperAddCartLinesStarted",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "helpersActivityHelperAddCartLinesStarted", Object.getOwnPropertyDescriptor(RetailLogger, "helpersActivityHelperAddCartLinesStarted")));
        Object.defineProperty(RetailLogger, "helpersActivityHelperAddCartLinesFinished",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "helpersActivityHelperAddCartLinesFinished", Object.getOwnPropertyDescriptor(RetailLogger, "helpersActivityHelperAddCartLinesFinished")));
        Object.defineProperty(RetailLogger, "viewsHomeTileClick",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsHomeTileClick", Object.getOwnPropertyDescriptor(RetailLogger, "viewsHomeTileClick")));
        Object.defineProperty(RetailLogger, "viewsCustomerDetailsError",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsCustomerDetailsError", Object.getOwnPropertyDescriptor(RetailLogger, "viewsCustomerDetailsError")));
        Object.defineProperty(RetailLogger, "viewsMerchandisingSearchViewSearchClick",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsMerchandisingSearchViewSearchClick", Object.getOwnPropertyDescriptor(RetailLogger, "viewsMerchandisingSearchViewSearchClick")));
        Object.defineProperty(RetailLogger, "viewsMerchandisingSearchViewProductButtonClick",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsMerchandisingSearchViewProductButtonClick", Object.getOwnPropertyDescriptor(RetailLogger, "viewsMerchandisingSearchViewProductButtonClick")));
        Object.defineProperty(RetailLogger, "viewsMerchandisingSearchViewCustomerButtonClick",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsMerchandisingSearchViewCustomerButtonClick", Object.getOwnPropertyDescriptor(RetailLogger, "viewsMerchandisingSearchViewCustomerButtonClick")));
        Object.defineProperty(RetailLogger, "viewsMerchandisingSearchViewAddToCartClick",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [Number]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsMerchandisingSearchViewAddToCartClick", Object.getOwnPropertyDescriptor(RetailLogger, "viewsMerchandisingSearchViewAddToCartClick")));
        Object.defineProperty(RetailLogger, "viewsMerchandisingSearchViewQuickSaleClick",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [Number]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsMerchandisingSearchViewQuickSaleClick", Object.getOwnPropertyDescriptor(RetailLogger, "viewsMerchandisingSearchViewQuickSaleClick")));
        Object.defineProperty(RetailLogger, "viewsMerchandisingProductDetailsKitVariantNotFound",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [Number, Number]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsMerchandisingProductDetailsKitVariantNotFound", Object.getOwnPropertyDescriptor(RetailLogger, "viewsMerchandisingProductDetailsKitVariantNotFound")));
        Object.defineProperty(RetailLogger, "viewsMerchandisingCatalogsCatalogClicked",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsMerchandisingCatalogsCatalogClicked", Object.getOwnPropertyDescriptor(RetailLogger, "viewsMerchandisingCatalogsCatalogClicked")));
        Object.defineProperty(RetailLogger, "viewsCartCartViewShowPrintDialogFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsCartCartViewShowPrintDialogFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewsCartCartViewShowPrintDialogFailed")));
        Object.defineProperty(RetailLogger, "viewsCartShowJournalViewRetrieveProductFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [Number]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsCartShowJournalViewRetrieveProductFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewsCartShowJournalViewRetrieveProductFailed")));
        Object.defineProperty(RetailLogger, "viewsControlsCommonHeaderSearch",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsControlsCommonHeaderSearch", Object.getOwnPropertyDescriptor(RetailLogger, "viewsControlsCommonHeaderSearch")));
        Object.defineProperty(RetailLogger, "viewsControlsCommonHeaderCategoryInTreeClicked",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsControlsCommonHeaderCategoryInTreeClicked", Object.getOwnPropertyDescriptor(RetailLogger, "viewsControlsCommonHeaderCategoryInTreeClicked")));
        Object.defineProperty(RetailLogger, "viewsControlsRefinersTypeNotSupported",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsControlsRefinersTypeNotSupported", Object.getOwnPropertyDescriptor(RetailLogger, "viewsControlsRefinersTypeNotSupported")));
        Object.defineProperty(RetailLogger, "viewsControlsRefinersDisplayTemplateNotSupported",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsControlsRefinersDisplayTemplateNotSupported", Object.getOwnPropertyDescriptor(RetailLogger, "viewsControlsRefinersDisplayTemplateNotSupported")));
        Object.defineProperty(RetailLogger, "viewsControlsRefinersWrongInputParameters",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsControlsRefinersWrongInputParameters", Object.getOwnPropertyDescriptor(RetailLogger, "viewsControlsRefinersWrongInputParameters")));
        Object.defineProperty(RetailLogger, "viewsMerchandisingAllStoresViewConstructorArgumentUndefined",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsMerchandisingAllStoresViewConstructorArgumentUndefined", Object.getOwnPropertyDescriptor(RetailLogger, "viewsMerchandisingAllStoresViewConstructorArgumentUndefined")));
        Object.defineProperty(RetailLogger, "viewsMerchandisingPickingAndReceivingDetailsViewLoadJournalFailed",
            __decorate([
                __param(0, Commerce.AccountData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsMerchandisingPickingAndReceivingDetailsViewLoadJournalFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewsMerchandisingPickingAndReceivingDetailsViewLoadJournalFailed")));
        Object.defineProperty(RetailLogger, "viewsCustomerPickUpInStoreViewBingMapsFaild",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsCustomerPickUpInStoreViewBingMapsFaild", Object.getOwnPropertyDescriptor(RetailLogger, "viewsCustomerPickUpInStoreViewBingMapsFaild")));
        Object.defineProperty(RetailLogger, "viewsCustomerPickUpInStoreViewBingMapsFailedToInitialize",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsCustomerPickUpInStoreViewBingMapsFailedToInitialize", Object.getOwnPropertyDescriptor(RetailLogger, "viewsCustomerPickUpInStoreViewBingMapsFailedToInitialize")));
        Object.defineProperty(RetailLogger, "viewsCustomerAddressAddEditViewDownloadTaxGroupsFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsCustomerAddressAddEditViewDownloadTaxGroupsFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewsCustomerAddressAddEditViewDownloadTaxGroupsFailed")));
        Object.defineProperty(RetailLogger, "viewsDailyOperationsCashManagementViewOperationFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsDailyOperationsCashManagementViewOperationFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewsDailyOperationsCashManagementViewOperationFailed")));
        Object.defineProperty(RetailLogger, "viewsTutorialVideoDialogVideoElementThrowsError",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsTutorialVideoDialogVideoElementThrowsError", Object.getOwnPropertyDescriptor(RetailLogger, "viewsTutorialVideoDialogVideoElementThrowsError")));
        Object.defineProperty(RetailLogger, "viewModelCartVoidProductsFinished",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [Boolean]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelCartVoidProductsFinished", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelCartVoidProductsFinished")));
        Object.defineProperty(RetailLogger, "viewModelGetCustomerBalanceFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelGetCustomerBalanceFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelGetCustomerBalanceFailed")));
        Object.defineProperty(RetailLogger, "viewModelGetCustomerLoyaltyCardsFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelGetCustomerLoyaltyCardsFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelGetCustomerLoyaltyCardsFailed")));
        Object.defineProperty(RetailLogger, "viewModelUnsupportedBarcodeMaskType",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelUnsupportedBarcodeMaskType", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelUnsupportedBarcodeMaskType")));
        Object.defineProperty(RetailLogger, "viewModelCartProcessScanResultStarted",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelCartProcessScanResultStarted", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelCartProcessScanResultStarted")));
        Object.defineProperty(RetailLogger, "viewModelCartProcessScanResultFinished",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, Boolean]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelCartProcessScanResultFinished", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelCartProcessScanResultFinished")));
        Object.defineProperty(RetailLogger, "viewModelLoginDeviceActivationFailed",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.CustomerContent),
                __param(2, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelLoginDeviceActivationFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelLoginDeviceActivationFailed")));
        Object.defineProperty(RetailLogger, "viewModelLoginRetrieveUserAuthenticationTokenFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelLoginRetrieveUserAuthenticationTokenFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelLoginRetrieveUserAuthenticationTokenFailed")));
        Object.defineProperty(RetailLogger, "viewModelLoginRetailServerDiscoveryFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent),
                __param(2, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelLoginRetailServerDiscoveryFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelLoginRetailServerDiscoveryFailed")));
        Object.defineProperty(RetailLogger, "viewModelLoginFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelLoginFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelLoginFailed")));
        Object.defineProperty(RetailLogger, "viewModelDeleteExpiredSessionFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelDeleteExpiredSessionFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelDeleteExpiredSessionFailed")));
        Object.defineProperty(RetailLogger, "viewModelGetTerminalDataStoreNameFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelGetTerminalDataStoreNameFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelGetTerminalDataStoreNameFailed")));
        Object.defineProperty(RetailLogger, "viewModelGetDownloadIntervalFailed",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelGetDownloadIntervalFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelGetDownloadIntervalFailed")));
        Object.defineProperty(RetailLogger, "viewModelCheckInitialSyncFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelCheckInitialSyncFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelCheckInitialSyncFailed")));
        Object.defineProperty(RetailLogger, "viewModelGetDownloadSessionsFailed",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelGetDownloadSessionsFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelGetDownloadSessionsFailed")));
        Object.defineProperty(RetailLogger, "viewModelDownloadFileFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelDownloadFileFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelDownloadFileFailed")));
        Object.defineProperty(RetailLogger, "viewModelDownloadFileBrokerRequestFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelDownloadFileBrokerRequestFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelDownloadFileBrokerRequestFailed")));
        Object.defineProperty(RetailLogger, "viewModelApplyToOfflineDbFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelApplyToOfflineDbFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelApplyToOfflineDbFailed")));
        Object.defineProperty(RetailLogger, "viewModelApplyToOfflineDbBrokerRequestFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelApplyToOfflineDbBrokerRequestFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelApplyToOfflineDbBrokerRequestFailed")));
        Object.defineProperty(RetailLogger, "viewModelUpdateDownloadSessionStatusBrokerRequestFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelUpdateDownloadSessionStatusBrokerRequestFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelUpdateDownloadSessionStatusBrokerRequestFailed")));
        Object.defineProperty(RetailLogger, "viewModelGetUploadIntervalFailed",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelGetUploadIntervalFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelGetUploadIntervalFailed")));
        Object.defineProperty(RetailLogger, "viewModelGetUploadJobDefinitionsFailed",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelGetUploadJobDefinitionsFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelGetUploadJobDefinitionsFailed")));
        Object.defineProperty(RetailLogger, "viewModelLoadUploadTransactionsFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelLoadUploadTransactionsFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelLoadUploadTransactionsFailed")));
        Object.defineProperty(RetailLogger, "viewModelPurgeOfflineTransactionsFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelPurgeOfflineTransactionsFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelPurgeOfflineTransactionsFailed")));
        Object.defineProperty(RetailLogger, "viewModelGetDownloadLinkFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, Number]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelGetDownloadLinkFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelGetDownloadLinkFailed")));
        Object.defineProperty(RetailLogger, "viewModelGetOfflineSyncStatsFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelGetOfflineSyncStatsFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelGetOfflineSyncStatsFailed")));
        Object.defineProperty(RetailLogger, "viewModelProductDetailsKitVariantNotFound",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [Number, Number]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelProductDetailsKitVariantNotFound", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelProductDetailsKitVariantNotFound")));
        Object.defineProperty(RetailLogger, "viewModelPriceCheckContextEntitySetMultipleTimes",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelPriceCheckContextEntitySetMultipleTimes", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelPriceCheckContextEntitySetMultipleTimes")));
        Object.defineProperty(RetailLogger, "viewModelPriceCheckGetProductPriceFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelPriceCheckGetProductPriceFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelPriceCheckGetProductPriceFailed")));
        Object.defineProperty(RetailLogger, "viewModelPriceCheckGetCustomerFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelPriceCheckGetCustomerFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelPriceCheckGetCustomerFailed")));
        Object.defineProperty(RetailLogger, "viewModelPriceCheckGetStoreDetailsFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelPriceCheckGetStoreDetailsFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelPriceCheckGetStoreDetailsFailed")));
        Object.defineProperty(RetailLogger, "viewModelPriceCheckGetActivePriceFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelPriceCheckGetActivePriceFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelPriceCheckGetActivePriceFailed")));
        Object.defineProperty(RetailLogger, "viewModelPaymentCardSwipeNotSupported",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [Number]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelPaymentCardSwipeNotSupported", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelPaymentCardSwipeNotSupported")));
        Object.defineProperty(RetailLogger, "viewModelCustomerAddEditUnknownCustomerType",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelCustomerAddEditUnknownCustomerType", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelCustomerAddEditUnknownCustomerType")));
        Object.defineProperty(RetailLogger, "cloudPosBrowserNotSupported",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "cloudPosBrowserNotSupported", Object.getOwnPropertyDescriptor(RetailLogger, "cloudPosBrowserNotSupported")));
        Object.defineProperty(RetailLogger, "coreOperationValidatorsNoCartOnCartValidator",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "coreOperationValidatorsNoCartOnCartValidator", Object.getOwnPropertyDescriptor(RetailLogger, "coreOperationValidatorsNoCartOnCartValidator")));
        Object.defineProperty(RetailLogger, "viewModelProductSearchViewModelSearchProductsByTextFailed",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.CustomerContent),
                __param(2, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelProductSearchViewModelSearchProductsByTextFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelProductSearchViewModelSearchProductsByTextFailed")));
        Object.defineProperty(RetailLogger, "viewModelProductSearchViewModelGetRefinersByTextFailed",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelProductSearchViewModelGetRefinersByTextFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelProductSearchViewModelGetRefinersByTextFailed")));
        Object.defineProperty(RetailLogger, "viewModelProductSearchViewModelGetRefinerValuesByTextFailed",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.SystemData),
                __param(2, Commerce.SystemData),
                __param(3, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, Number, Number, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelProductSearchViewModelGetRefinerValuesByTextFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelProductSearchViewModelGetRefinerValuesByTextFailed")));
        Object.defineProperty(RetailLogger, "viewModelProductsViewModelAddItemsToCart",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, Boolean]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelProductsViewModelAddItemsToCart", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelProductsViewModelAddItemsToCart")));
        Object.defineProperty(RetailLogger, "viewsModelProductsViewModelSearchProductsByCategoryFailed",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.CustomerContent),
                __param(2, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [Number, String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsModelProductsViewModelSearchProductsByCategoryFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewsModelProductsViewModelSearchProductsByCategoryFailed")));
        Object.defineProperty(RetailLogger, "viewsModelProductsViewModelGetRefinersByCategoryFailed",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [Number, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsModelProductsViewModelGetRefinersByCategoryFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewsModelProductsViewModelGetRefinersByCategoryFailed")));
        Object.defineProperty(RetailLogger, "viewsModelProductsViewModelGetRefinerValuesByCategoryFailed",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.CustomerContent),
                __param(2, Commerce.SystemData),
                __param(3, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [Number, Number, Number, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewsModelProductsViewModelGetRefinerValuesByCategoryFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewsModelProductsViewModelGetRefinerValuesByCategoryFailed")));
        Object.defineProperty(RetailLogger, "viewModelProductsViewModelGetProductDetailsFailed",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelProductsViewModelGetProductDetailsFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelProductsViewModelGetProductDetailsFailed")));
        Object.defineProperty(RetailLogger, "viewModelSearchViewModelAddCustomerToCartFailed",
            __decorate([
                __param(0, Commerce.AccountData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelSearchViewModelAddCustomerToCartFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelSearchViewModelAddCustomerToCartFailed")));
        Object.defineProperty(RetailLogger, "viewModelSearchViewModelGetProductDetailsFailed",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "viewModelSearchViewModelGetProductDetailsFailed", Object.getOwnPropertyDescriptor(RetailLogger, "viewModelSearchViewModelGetProductDetailsFailed")));
        Object.defineProperty(RetailLogger, "taskRecorderContinueRecording",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "taskRecorderContinueRecording", Object.getOwnPropertyDescriptor(RetailLogger, "taskRecorderContinueRecording")));
        Object.defineProperty(RetailLogger, "taskRecorderPauseRecording",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "taskRecorderPauseRecording", Object.getOwnPropertyDescriptor(RetailLogger, "taskRecorderPauseRecording")));
        Object.defineProperty(RetailLogger, "taskRecorderStopRecording",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "taskRecorderStopRecording", Object.getOwnPropertyDescriptor(RetailLogger, "taskRecorderStopRecording")));
        Object.defineProperty(RetailLogger, "taskRecorderEndTask",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "taskRecorderEndTask", Object.getOwnPropertyDescriptor(RetailLogger, "taskRecorderEndTask")));
        Object.defineProperty(RetailLogger, "taskRecorderHandleAction",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "taskRecorderHandleAction", Object.getOwnPropertyDescriptor(RetailLogger, "taskRecorderHandleAction")));
        Object.defineProperty(RetailLogger, "taskRecorderScreenshotsUploadingFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "taskRecorderScreenshotsUploadingFailed", Object.getOwnPropertyDescriptor(RetailLogger, "taskRecorderScreenshotsUploadingFailed")));
        Object.defineProperty(RetailLogger, "taskRecorderDownloadFile",
            __decorate([
                __param(0, Commerce.CustomerContent),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "taskRecorderDownloadFile", Object.getOwnPropertyDescriptor(RetailLogger, "taskRecorderDownloadFile")));
        Object.defineProperty(RetailLogger, "taskRecorderShowSaveDialog",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "taskRecorderShowSaveDialog", Object.getOwnPropertyDescriptor(RetailLogger, "taskRecorderShowSaveDialog")));
        Object.defineProperty(RetailLogger, "taskRecorderSavingFileFailed",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "taskRecorderSavingFileFailed", Object.getOwnPropertyDescriptor(RetailLogger, "taskRecorderSavingFileFailed")));
        Object.defineProperty(RetailLogger, "taskRecorderSavingFileFinished",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "taskRecorderSavingFileFinished", Object.getOwnPropertyDescriptor(RetailLogger, "taskRecorderSavingFileFinished")));
        Object.defineProperty(RetailLogger, "taskRecorderSavingFileCanceled",
            __decorate([
                __param(0, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "taskRecorderSavingFileCanceled", Object.getOwnPropertyDescriptor(RetailLogger, "taskRecorderSavingFileCanceled")));
        Object.defineProperty(RetailLogger, "taskRecorderFileWasSaved",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "taskRecorderFileWasSaved", Object.getOwnPropertyDescriptor(RetailLogger, "taskRecorderFileWasSaved")));
        Object.defineProperty(RetailLogger, "taskRecorderSaveXMLFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "taskRecorderSaveXMLFailed", Object.getOwnPropertyDescriptor(RetailLogger, "taskRecorderSaveXMLFailed")));
        Object.defineProperty(RetailLogger, "taskRecorderSaveTrainingDocumentFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "taskRecorderSaveTrainingDocumentFailed", Object.getOwnPropertyDescriptor(RetailLogger, "taskRecorderSaveTrainingDocumentFailed")));
        Object.defineProperty(RetailLogger, "taskRecorderDeleteFolderFromLocalStorageFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "taskRecorderDeleteFolderFromLocalStorageFailed", Object.getOwnPropertyDescriptor(RetailLogger, "taskRecorderDeleteFolderFromLocalStorageFailed")));
        Object.defineProperty(RetailLogger, "taskRecorderSaveBpmPackageFailed",
            __decorate([
                __param(0, Commerce.SystemData),
                __param(1, Commerce.CustomerContent), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String, String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "taskRecorderSaveBpmPackageFailed", Object.getOwnPropertyDescriptor(RetailLogger, "taskRecorderSaveBpmPackageFailed")));
        Object.defineProperty(RetailLogger, "taskRecorderSaveSessionAsRecordingBundleFailed",
            __decorate([
                __param(0, Commerce.SystemData), 
                __metadata('design:type', Function), 
                __metadata('design:paramtypes', [String]), 
                __metadata('design:returntype', void 0)
            ], RetailLogger, "taskRecorderSaveSessionAsRecordingBundleFailed", Object.getOwnPropertyDescriptor(RetailLogger, "taskRecorderSaveSessionAsRecordingBundleFailed")));
        return RetailLogger;
    })();
    Commerce.RetailLogger = RetailLogger;
})(Commerce || (Commerce = {}));
//# sourceMappingURL=e:/bt/129158/source/frameworks/retailrain/components/apps/platform/pos/retaillogger/RetailLogger.js.map
// SIG // Begin signature block
// SIG // MIIdpgYJKoZIhvcNAQcCoIIdlzCCHZMCAQExCzAJBgUr
// SIG // DgMCGgUAMGcGCisGAQQBgjcCAQSgWTBXMDIGCisGAQQB
// SIG // gjcCAR4wJAIBAQQQEODJBs441BGiowAQS9NQkAIBAAIB
// SIG // AAIBAAIBAAIBADAhMAkGBSsOAwIaBQAEFKRWdhWbSTqi
// SIG // IuQkMkDVbPZ7YEINoIIYZDCCBMMwggOroAMCAQICEzMA
// SIG // AACc7v4UValdNVAAAAAAAJwwDQYJKoZIhvcNAQEFBQAw
// SIG // dzELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0
// SIG // b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1p
// SIG // Y3Jvc29mdCBDb3Jwb3JhdGlvbjEhMB8GA1UEAxMYTWlj
// SIG // cm9zb2Z0IFRpbWUtU3RhbXAgUENBMB4XDTE2MDMzMDE5
// SIG // MjEzMFoXDTE3MDYzMDE5MjEzMFowgbMxCzAJBgNVBAYT
// SIG // AlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQH
// SIG // EwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29y
// SIG // cG9yYXRpb24xDTALBgNVBAsTBE1PUFIxJzAlBgNVBAsT
// SIG // Hm5DaXBoZXIgRFNFIEVTTjo1ODQ3LUY3NjEtNEY3MDEl
// SIG // MCMGA1UEAxMcTWljcm9zb2Z0IFRpbWUtU3RhbXAgU2Vy
// SIG // dmljZTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoC
// SIG // ggEBAMwlhsl+iHoEj/vklU9epTLAab6xrU1GWdPtri0X
// SIG // lXXCMHd2091EB93Uff8GMa0sSf786tMU1N48+M230myS
// SIG // iD2LhwqTOH+Wtrc7v555A64ftHgB3Tc7LuyveruJiWU7
// SIG // iGI15VE7d64pXCwmFZs4K9MbvbPBtBKuu76g8rl7jG2p
// SIG // 8o7lEj/f2zhzZtxVW0XTnLCg2y34ziccn4ieu78n2xHP
// SIG // emwVbpUZv+hTb1+ewejzeMMwiURNM4oQLKdHRDqDccaW
// SIG // dOU+iQbhgUshhWzdmlwnrRfbPvS0ezij1zAE4GnvjMtG
// SIG // xRLA8t7CfM/J1FW7ktvNOThFdvqZVRFYbMQsiYkCAwEA
// SIG // AaOCAQkwggEFMB0GA1UdDgQWBBQ9XziJKANTiL5XmMZp
// SIG // /vYFXJZLLjAfBgNVHSMEGDAWgBQjNPjZUkZwCu1A+3b7
// SIG // syuwwzWzDzBUBgNVHR8ETTBLMEmgR6BFhkNodHRwOi8v
// SIG // Y3JsLm1pY3Jvc29mdC5jb20vcGtpL2NybC9wcm9kdWN0
// SIG // cy9NaWNyb3NvZnRUaW1lU3RhbXBQQ0EuY3JsMFgGCCsG
// SIG // AQUFBwEBBEwwSjBIBggrBgEFBQcwAoY8aHR0cDovL3d3
// SIG // dy5taWNyb3NvZnQuY29tL3BraS9jZXJ0cy9NaWNyb3Nv
// SIG // ZnRUaW1lU3RhbXBQQ0EuY3J0MBMGA1UdJQQMMAoGCCsG
// SIG // AQUFBwMIMA0GCSqGSIb3DQEBBQUAA4IBAQBW9mryWArT
// SIG // QwTRt58bLNWamRLKYRBK7V4/jFUv0R3jt027EwgUYa/L
// SIG // EWspXTacTuw6feQf/Ov68BRuktDg4eLL7sMBFl+oSuK7
// SIG // 4rT4+rVGDt3ZL4likaHyLofibFnlxCHa9893BvwIQrq8
// SIG // OOyT+j2l5f7tesai2vrhS7krO3Le7H+DoJM+bvZc9/9K
// SIG // +WyVFpHqY9wXqNLTBX0rql19kWdzw3WNHzkui86g8mw1
// SIG // T4ez07TsJEHqKzpEAv/8j5vIJsr+h+Hp19UdUcDPtExi
// SIG // XXJKoIcLFLYxTLZ2axLwxuFSwOqwzpSNPG8sWnYUGupP
// SIG // TBbE37m8UOHC2xm7iFh+XejuMIIGBzCCA++gAwIBAgIK
// SIG // YRZoNAAAAAAAHDANBgkqhkiG9w0BAQUFADBfMRMwEQYK
// SIG // CZImiZPyLGQBGRYDY29tMRkwFwYKCZImiZPyLGQBGRYJ
// SIG // bWljcm9zb2Z0MS0wKwYDVQQDEyRNaWNyb3NvZnQgUm9v
// SIG // dCBDZXJ0aWZpY2F0ZSBBdXRob3JpdHkwHhcNMDcwNDAz
// SIG // MTI1MzA5WhcNMjEwNDAzMTMwMzA5WjB3MQswCQYDVQQG
// SIG // EwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UE
// SIG // BxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENv
// SIG // cnBvcmF0aW9uMSEwHwYDVQQDExhNaWNyb3NvZnQgVGlt
// SIG // ZS1TdGFtcCBQQ0EwggEiMA0GCSqGSIb3DQEBAQUAA4IB
// SIG // DwAwggEKAoIBAQCfoWyx39tIkip8ay4Z4b3i48WZUSNQ
// SIG // rc7dGE4kD+7Rp9FMrXQwIBHrB9VUlRVJlBtCkq6YXDAm
// SIG // 2gBr6Hu97IkHD/cOBJjwicwfyzMkh53y9GccLPx754gd
// SIG // 6udOo6HBI1PKjfpFzwnQXq/QsEIEovmmbJNn1yjcRlOw
// SIG // htDlKEYuJ6yGT1VSDOQDLPtqkJAwbofzWTCd+n7Wl7Po
// SIG // IZd++NIT8wi3U21StEWQn0gASkdmEScpZqiX5NMGgUqi
// SIG // +YSnEUcUCYKfhO1VeP4Bmh1QCIUAEDBG7bfeI0a7xC1U
// SIG // n68eeEExd8yb3zuDk6FhArUdDbH895uyAc4iS1T/+QXD
// SIG // wiALAgMBAAGjggGrMIIBpzAPBgNVHRMBAf8EBTADAQH/
// SIG // MB0GA1UdDgQWBBQjNPjZUkZwCu1A+3b7syuwwzWzDzAL
// SIG // BgNVHQ8EBAMCAYYwEAYJKwYBBAGCNxUBBAMCAQAwgZgG
// SIG // A1UdIwSBkDCBjYAUDqyCYEBWJ5flJRP8KuEKU5VZ5KSh
// SIG // Y6RhMF8xEzARBgoJkiaJk/IsZAEZFgNjb20xGTAXBgoJ
// SIG // kiaJk/IsZAEZFgltaWNyb3NvZnQxLTArBgNVBAMTJE1p
// SIG // Y3Jvc29mdCBSb290IENlcnRpZmljYXRlIEF1dGhvcml0
// SIG // eYIQea0WoUqgpa1Mc1j0BxMuZTBQBgNVHR8ESTBHMEWg
// SIG // Q6BBhj9odHRwOi8vY3JsLm1pY3Jvc29mdC5jb20vcGtp
// SIG // L2NybC9wcm9kdWN0cy9taWNyb3NvZnRyb290Y2VydC5j
// SIG // cmwwVAYIKwYBBQUHAQEESDBGMEQGCCsGAQUFBzAChjho
// SIG // dHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpL2NlcnRz
// SIG // L01pY3Jvc29mdFJvb3RDZXJ0LmNydDATBgNVHSUEDDAK
// SIG // BggrBgEFBQcDCDANBgkqhkiG9w0BAQUFAAOCAgEAEJeK
// SIG // w1wDRDbd6bStd9vOeVFNAbEudHFbbQwTq86+e4+4LtQS
// SIG // ooxtYrhXAstOIBNQmd16QOJXu69YmhzhHQGGrLt48ovQ
// SIG // 7DsB7uK+jwoFyI1I4vBTFd1Pq5Lk541q1YDB5pTyBi+F
// SIG // A+mRKiQicPv2/OR4mS4N9wficLwYTp2OawpylbihOZxn
// SIG // LcVRDupiXD8WmIsgP+IHGjL5zDFKdjE9K3ILyOpwPf+F
// SIG // ChPfwgphjvDXuBfrTot/xTUrXqO/67x9C0J71FNyIe4w
// SIG // yrt4ZVxbARcKFA7S2hSY9Ty5ZlizLS/n+YWGzFFW6J1w
// SIG // lGysOUzU9nm/qhh6YinvopspNAZ3GmLJPR5tH4LwC8cs
// SIG // u89Ds+X57H2146SodDW4TsVxIxImdgs8UoxxWkZDFLyz
// SIG // s7BNZ8ifQv+AeSGAnhUwZuhCEl4ayJ4iIdBD6Svpu/RI
// SIG // zCzU2DKATCYqSCRfWupW76bemZ3KOm+9gSd0BhHudiG/
// SIG // m4LBJ1S2sWo9iaF2YbRuoROmv6pH8BJv/YoybLL+31HI
// SIG // jCPJZr2dHYcSZAI9La9Zj7jkIeW1sMpjtHhUBdRBLlCs
// SIG // lLCleKuzoJZ1GtmShxN1Ii8yqAhuoFuMJb+g74TKIdbr
// SIG // Hk/Jmu5J4PcBZW+JC33Iacjmbuqnl84xKf8OxVtc2E0b
// SIG // odj6L54/LlUWa8kTo/0wggYQMIID+KADAgECAhMzAAAA
// SIG // ZEeElIbbQRk4AAAAAABkMA0GCSqGSIb3DQEBCwUAMH4x
// SIG // CzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9u
// SIG // MRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNy
// SIG // b3NvZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jv
// SIG // c29mdCBDb2RlIFNpZ25pbmcgUENBIDIwMTEwHhcNMTUx
// SIG // MDI4MjAzMTQ2WhcNMTcwMTI4MjAzMTQ2WjCBgzELMAkG
// SIG // A1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAO
// SIG // BgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29m
// SIG // dCBDb3Jwb3JhdGlvbjENMAsGA1UECxMETU9QUjEeMBwG
// SIG // A1UEAxMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMIIBIjAN
// SIG // BgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAky7a2OY+
// SIG // mNkbD2RfTahYTRQ793qE/DwRMTrvicJKLUGlSF3dEp7v
// SIG // q2YoNNV9KlV7TE2K8sDxstNSFYu2swi4i1AL3X/7agmg
// SIG // 3GcExPHfvHUYIEC+eCyZVt3u9S7dPkL5Wh8wrgEUirCC
// SIG // tVGg4m1l/vcYCo0wbU06p8XzNi3uXyygkgCxHEziy/f/
// SIG // JCV/14/A3ZduzrIXtsccRKckyn6B5uYxuRbZXT7RaO6+
// SIG // zUjQhiyu3A4hwcCKw+4bk1kT9sY7gHIYiFP7q78wPqB3
// SIG // vVKIv3rY6LCTraEbjNR+phBQEL7hyBxk+ocu+8RHZhbA
// SIG // hHs2r1+6hURsAg8t4LAOG6I+JQIDAQABo4IBfzCCAXsw
// SIG // HwYDVR0lBBgwFgYIKwYBBQUHAwMGCisGAQQBgjdMCAEw
// SIG // HQYDVR0OBBYEFFhWcQTwvbsz9YNozOeARvdXr9IiMFEG
// SIG // A1UdEQRKMEikRjBEMQ0wCwYDVQQLEwRNT1BSMTMwMQYD
// SIG // VQQFEyozMTY0Mis0OWU4YzNmMy0yMzU5LTQ3ZjYtYTNi
// SIG // ZS02YzhjNDc1MWM0YjYwHwYDVR0jBBgwFoAUSG5k5VAF
// SIG // 04KqFzc3IrVtqMp1ApUwVAYDVR0fBE0wSzBJoEegRYZD
// SIG // aHR0cDovL3d3dy5taWNyb3NvZnQuY29tL3BraW9wcy9j
// SIG // cmwvTWljQ29kU2lnUENBMjAxMV8yMDExLTA3LTA4LmNy
// SIG // bDBhBggrBgEFBQcBAQRVMFMwUQYIKwYBBQUHMAKGRWh0
// SIG // dHA6Ly93d3cubWljcm9zb2Z0LmNvbS9wa2lvcHMvY2Vy
// SIG // dHMvTWljQ29kU2lnUENBMjAxMV8yMDExLTA3LTA4LmNy
// SIG // dDAMBgNVHRMBAf8EAjAAMA0GCSqGSIb3DQEBCwUAA4IC
// SIG // AQCI4gxkQx3dXK6MO4UktZ1A1r1mrFtXNdn06DrARZkQ
// SIG // Tdu0kOTLdlGBCfCzk0309RLkvUgnFKpvLddrg9TGp3n8
// SIG // 0yUbRsp2AogyrlBU+gP5ggHFi7NjGEpj5bH+FDsMw9Py
// SIG // gLg8JelgsvBVudw1SgUt625nY7w1vrwk+cDd58TvAyJQ
// SIG // FAW1zJ+0ySgB9lu2vwg0NKetOyL7dxe3KoRLaztUcqXo
// SIG // YW5CkI+Mv3m8HOeqlhyfFTYxPB5YXyQJPKQJYh8zC9b9
// SIG // 0JXLT7raM7mQ94ygDuFmlaiZ+QSUR3XVupdEngrmZgUB
// SIG // 5jX13M+Pl2Vv7PPFU3xlo3Uhj1wtupNC81epoxGhJ0tR
// SIG // uLdEajD/dCZ0xIniesRXCKSC4HCL3BMnSwVXtIoj/QFy
// SIG // mFYwD5+sAZuvRSgkKyD1rDA7MPcEI2i/Bh5OMAo9App4
// SIG // sR0Gp049oSkXNhvRi/au7QG6NJBTSBbNBGJG8Qp+5QTh
// SIG // KoQUk8mj0ugr4yWRsA9JTbmqVw7u9suB5OKYBMUN4hL/
// SIG // yI+aFVsE/KJInvnxSzXJ1YHka45ADYMKAMl+fLdIqm3n
// SIG // x6rIN0RkoDAbvTAAXGehUCsIod049A1T3IJyUJXt3OsT
// SIG // d3WabhIBXICYfxMg10naaWcyUePgW3+VwP0XLKu4O1+8
// SIG // ZeGyaDSi33GnzmmyYacX3BTqMDCCB3owggVioAMCAQIC
// SIG // CmEOkNIAAAAAAAMwDQYJKoZIhvcNAQELBQAwgYgxCzAJ
// SIG // BgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAw
// SIG // DgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3Nv
// SIG // ZnQgQ29ycG9yYXRpb24xMjAwBgNVBAMTKU1pY3Jvc29m
// SIG // dCBSb290IENlcnRpZmljYXRlIEF1dGhvcml0eSAyMDEx
// SIG // MB4XDTExMDcwODIwNTkwOVoXDTI2MDcwODIxMDkwOVow
// SIG // fjELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0
// SIG // b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1p
// SIG // Y3Jvc29mdCBDb3Jwb3JhdGlvbjEoMCYGA1UEAxMfTWlj
// SIG // cm9zb2Z0IENvZGUgU2lnbmluZyBQQ0EgMjAxMTCCAiIw
// SIG // DQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBAKvw+nIQ
// SIG // HC6t2G6qghBNNLrytlghn0IbKmvpWlCquAY4GgRJun/D
// SIG // DB7dN2vGEtgL8DjCmQawyDnVARQxQtOJDXlkh36UYCRs
// SIG // r55JnOloXtLfm1OyCizDr9mpK656Ca/XllnKYBoF6WZ2
// SIG // 6DJSJhIv56sIUM+zRLdd2MQuA3WraPPLbfM6XKEW9Ea6
// SIG // 4DhkrG5kNXimoGMPLdNAk/jj3gcN1Vx5pUkp5w2+oBN3
// SIG // vpQ97/vjK1oQH01WKKJ6cuASOrdJXtjt7UORg9l7snuG
// SIG // G9k+sYxd6IlPhBryoS9Z5JA7La4zWMW3Pv4y07MDPbGy
// SIG // r5I4ftKdgCz1TlaRITUlwzluZH9TupwPrRkjhMv0ugOG
// SIG // jfdf8NBSv4yUh7zAIXQlXxgotswnKDglmDlKNs98sZKu
// SIG // HCOnqWbsYR9q4ShJnV+I4iVd0yFLPlLEtVc/JAPw0Xpb
// SIG // L9Uj43BdD1FGd7P4AOG8rAKCX9vAFbO9G9RVS+c5oQ/p
// SIG // I0m8GLhEfEXkwcNyeuBy5yTfv0aZxe/CHFfbg43sTUkw
// SIG // p6uO3+xbn6/83bBm4sGXgXvt1u1L50kppxMopqd9Z4Dm
// SIG // imJ4X7IvhNdXnFy/dygo8e1twyiPLI9AN0/B4YVEicQJ
// SIG // TMXUpUMvdJX3bvh4IFgsE11glZo+TzOE2rCIF96eTvSW
// SIG // sLxGoGyY0uDWiIwLAgMBAAGjggHtMIIB6TAQBgkrBgEE
// SIG // AYI3FQEEAwIBADAdBgNVHQ4EFgQUSG5k5VAF04KqFzc3
// SIG // IrVtqMp1ApUwGQYJKwYBBAGCNxQCBAweCgBTAHUAYgBD
// SIG // AEEwCwYDVR0PBAQDAgGGMA8GA1UdEwEB/wQFMAMBAf8w
// SIG // HwYDVR0jBBgwFoAUci06AjGQQ7kUBU7h6qfHMdEjiTQw
// SIG // WgYDVR0fBFMwUTBPoE2gS4ZJaHR0cDovL2NybC5taWNy
// SIG // b3NvZnQuY29tL3BraS9jcmwvcHJvZHVjdHMvTWljUm9v
// SIG // Q2VyQXV0MjAxMV8yMDExXzAzXzIyLmNybDBeBggrBgEF
// SIG // BQcBAQRSMFAwTgYIKwYBBQUHMAKGQmh0dHA6Ly93d3cu
// SIG // bWljcm9zb2Z0LmNvbS9wa2kvY2VydHMvTWljUm9vQ2Vy
// SIG // QXV0MjAxMV8yMDExXzAzXzIyLmNydDCBnwYDVR0gBIGX
// SIG // MIGUMIGRBgkrBgEEAYI3LgMwgYMwPwYIKwYBBQUHAgEW
// SIG // M2h0dHA6Ly93d3cubWljcm9zb2Z0LmNvbS9wa2lvcHMv
// SIG // ZG9jcy9wcmltYXJ5Y3BzLmh0bTBABggrBgEFBQcCAjA0
// SIG // HjIgHQBMAGUAZwBhAGwAXwBwAG8AbABpAGMAeQBfAHMA
// SIG // dABhAHQAZQBtAGUAbgB0AC4gHTANBgkqhkiG9w0BAQsF
// SIG // AAOCAgEAZ/KGpZjgVHkaLtPYdGcimwuWEeFjkplCln3S
// SIG // eQyQwWVfLiw++MNy0W2D/r4/6ArKO79HqaPzadtjvyI1
// SIG // pZddZYSQfYtGUFXYDJJ80hpLHPM8QotS0LD9a+M+By4p
// SIG // m+Y9G6XUtR13lDni6WTJRD14eiPzE32mkHSDjfTLJgJG
// SIG // KsKKELukqQUMm+1o+mgulaAqPyprWEljHwlpblqYluSD
// SIG // 9MCP80Yr3vw70L01724lruWvJ+3Q3fMOr5kol5hNDj0L
// SIG // 8giJ1h/DMhji8MUtzluetEk5CsYKwsatruWy2dsViFFF
// SIG // WDgycScaf7H0J/jeLDogaZiyWYlobm+nt3TDQAUGpgEq
// SIG // KD6CPxNNZgvAs0314Y9/HG8VfUWnduVAKmWjw11SYobD
// SIG // HWM2l4bf2vP48hahmifhzaWX0O5dY0HjWwechz4GdwbR
// SIG // BrF1HxS+YWG18NzGGwS+30HHDiju3mUv7Jf2oVyW2ADW
// SIG // oUa9WfOXpQlLSBCZgB/QACnFsZulP0V3HjXG0qKin3p6
// SIG // IvpIlR+r+0cjgPWe+L9rt0uX4ut1eBrs6jeZeRhL/9az
// SIG // I2h15q/6/IvrC4DqaTuv/DDtBEyO3991bWORPdGdVk5P
// SIG // v4BXIqF4ETIheu9BCrE/+6jMpF3BoYibV3FWTkhFwELJ
// SIG // m3ZbCoBIa/15n8G9bW1qyVJzEw16UM0xggSuMIIEqgIB
// SIG // ATCBlTB+MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2Fz
// SIG // aGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UE
// SIG // ChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSgwJgYDVQQD
// SIG // Ex9NaWNyb3NvZnQgQ29kZSBTaWduaW5nIFBDQSAyMDEx
// SIG // AhMzAAAAZEeElIbbQRk4AAAAAABkMAkGBSsOAwIaBQCg
// SIG // gcIwGQYJKoZIhvcNAQkDMQwGCisGAQQBgjcCAQQwHAYK
// SIG // KwYBBAGCNwIBCzEOMAwGCisGAQQBgjcCARUwIwYJKoZI
// SIG // hvcNAQkEMRYEFDuZsPpFf0bFNJ+RJZCO41Exr5oWMGIG
// SIG // CisGAQQBgjcCAQwxVDBSoCCAHgBSAGUAdABhAGkAbABM
// SIG // AG8AZwBnAGUAcgAuAGoAc6EugCxodHRwOi8vd3d3Lk1p
// SIG // Y3Jvc29mdC5jb20vTWljcm9zb2Z0RHluYW1pY3MvIDAN
// SIG // BgkqhkiG9w0BAQEFAASCAQAutI+WuaPhpClIrOqYe/Kq
// SIG // 01dpQK37SdOlyou4fpUgiOY+hHwYzrcr/hvqyvAUSO4z
// SIG // YcGiBTMQtjyf99Sxp+Tz2tmOd7sD3KCgdke+FnBfIPd2
// SIG // mrxOHUr6B69TK733SnTVNmRUhKMcbYdS/EA15kyfkTMa
// SIG // k12/kcPUwoODoono+KJD9Xhqg6W6Hw7QyuRtxEPkwKgP
// SIG // eEb9mXdjx/tf2aTLoGJIW7QwYkE+Uovxm/eKn47Gt1QN
// SIG // X7D2AKn2KkU2auh1rFMdbsXMrJnTXR58SeA9+fAgxxfw
// SIG // rKgTd6cEE/RImrm+pG5Ia7aYWN1+ekBsCOboCRhXZWmy
// SIG // oeVK64zVgmCPoYICKDCCAiQGCSqGSIb3DQEJBjGCAhUw
// SIG // ggIRAgEBMIGOMHcxCzAJBgNVBAYTAlVTMRMwEQYDVQQI
// SIG // EwpXYXNoaW5ndG9uMRAwDgYDVQQHEwdSZWRtb25kMR4w
// SIG // HAYDVQQKExVNaWNyb3NvZnQgQ29ycG9yYXRpb24xITAf
// SIG // BgNVBAMTGE1pY3Jvc29mdCBUaW1lLVN0YW1wIFBDQQIT
// SIG // MwAAAJzu/hRVqV01UAAAAAAAnDAJBgUrDgMCGgUAoF0w
// SIG // GAYJKoZIhvcNAQkDMQsGCSqGSIb3DQEHATAcBgkqhkiG
// SIG // 9w0BCQUxDxcNMTYwNzIxMjEwNjUyWjAjBgkqhkiG9w0B
// SIG // CQQxFgQUIBx0iEkFbjNjmopVOEK18qdPkZswDQYJKoZI
// SIG // hvcNAQEFBQAEggEAfHLd1FoToOztO0MVON2rRZxxDwCl
// SIG // wNbUI/oLmqcyfjnYETxTbP5nMcQv5Hr1YZJI+8D+7r2T
// SIG // GjdV4T5tF2kVXLlSlwa4+WfXy2XMW71vNW/xeaLm8aEm
// SIG // j4GOaHI/3LsU9DaAqE39l0YdP4BeSj2khZshcRugDdma
// SIG // XuXPJYNmBYkkIas5j4eE8IxVS0XsSrJ9DMNHb9N7S2SH
// SIG // g/ZMT0Lrp9/dRsxEYS4LLR+2Ik0dvKrP99n8OytdJas6
// SIG // v1f3DcDu9KB7R9qJmlx5WCHyVSiklUNU3o/XJaa2Hpgb
// SIG // 5DMV38SGTwe9UgTm9+Y0kbwFhvt0AMIaOB7YAMm48YYp
// SIG // IUoJZA==
// SIG // End signature block
