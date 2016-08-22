var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Diagnostics;
        (function (Diagnostics) {
            var TypeScriptCore;
            (function (TypeScriptCore) {
                "use strict";
                var AppInsightsSink = (function () {
                    function AppInsightsSink(appInsightsInstrumentationKey, applicationName, applicationVersion) {
                        this.appSessionId = TypeScriptCore.Utils.emptyGuid();
                        this.userSessionId = TypeScriptCore.Utils.emptyGuid();
                        this.tenantId = TypeScriptCore.Utils.emptyGuid();
                        this.application = applicationName;
                        this.applicationVersion = applicationVersion;
                        var defaultAppInsightsConfig = Microsoft.ApplicationInsights.Initialization.getDefaultConfig();
                        defaultAppInsightsConfig.instrumentationKey = appInsightsInstrumentationKey;
                        this.appInsightsProxy = new Microsoft.ApplicationInsights.AppInsights(defaultAppInsightsConfig);
                        this.appInsightsProxy.context.application.ver = applicationVersion;
                    }
                    AppInsightsSink.prototype.setSessionInfo = function (appSessionId, userSessionId, deviceId, deviceNumber, terminalId, userId, tenantId, offlineAvailability, offlineCurrentMode) {
                        this.appSessionId = appSessionId;
                        this.userSessionId = userSessionId;
                        this.deviceId = deviceId;
                        this.deviceNumber = deviceNumber;
                        this.terminalId = terminalId;
                        this.userId = userId;
                        this.tenantId = tenantId;
                        this.offlineAvailability = offlineAvailability;
                        this.offlineCurrentMode = offlineCurrentMode;
                        this.appInsightsProxy.context.user.id = userId;
                    };
                    AppInsightsSink.prototype.setInstrumentationKey = function (instrumentationKey) {
                    };
                    AppInsightsSink.prototype.writeEvent = function (event) {
                        var payload;
                        if (event.Payload) {
                            payload = event.Payload;
                        }
                        else {
                            payload = {};
                        }
                        payload.Application = this.application;
                        payload.AppSessionId = this.appSessionId;
                        payload.DeviceId = this.deviceId;
                        payload.DeviceNumber = this.deviceNumber;
                        payload.TerminalId = this.terminalId;
                        payload.UserId = this.userId;
                        payload.UserSessionId = this.userSessionId;
                        payload.TenantId = this.tenantId;
                        payload.OfflineAvailability = this.offlineAvailability;
                        payload.OfflineCurrentMode = this.offlineCurrentMode;
                        if (event.Type === TypeScriptCore.EventType.Custom) {
                            payload.EventSeverity = event.StaticMetadata.LevelName;
                            this.appInsightsProxy.trackEvent(event.StaticMetadata.Name, payload, null);
                        }
                        else if (event.Type === TypeScriptCore.EventType.PageView) {
                            this.appInsightsProxy.trackPageView(event.PageViewMetadata.PageName, "", payload, null);
                        }
                    };
                    return AppInsightsSink;
                })();
                TypeScriptCore.AppInsightsSink = AppInsightsSink;
            })(TypeScriptCore = Diagnostics.TypeScriptCore || (Diagnostics.TypeScriptCore = {}));
        })(Diagnostics = Dynamics.Diagnostics || (Dynamics.Diagnostics = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Diagnostics;
        (function (Diagnostics) {
            var TypeScriptCore;
            (function (TypeScriptCore) {
                "use strict";
                var DebuggingConsoleSink = (function () {
                    function DebuggingConsoleSink() {
                    }
                    DebuggingConsoleSink.prototype.setSessionInfo = function (appSessionId, userSessionId, deviceId, deviceNumber, terminalId, userId, tenantId, offlineAvailability, offlineCurrentMode) {
                    };
                    DebuggingConsoleSink.prototype.setInstrumentationKey = function (instrumentationKey) {
                    };
                    DebuggingConsoleSink.prototype.writeEvent = function (event) {
                        var args = [];
                        for (var name in event.Payload) {
                            args.push(event.Payload[name]);
                        }
                        if (event.Type === Microsoft.Dynamics.Diagnostics.TypeScriptCore.EventType.Custom) {
                            var message = event.StaticMetadata.Message.toString();
                            args.forEach(function (arg, index) {
                                var param = arg.toString().replace(/\$/gi, '$$$$');
                                var regexp = new RegExp('\\{' + index + '\\}', 'gi');
                                message = message.replace(regexp, param);
                            });
                            var level = event.StaticMetadata.Level;
                            switch (level) {
                                case Microsoft.Dynamics.Diagnostics.TypeScriptCore.EventLevel.Critical:
                                case Microsoft.Dynamics.Diagnostics.TypeScriptCore.EventLevel.Error:
                                    console.error(message);
                                    break;
                                case Microsoft.Dynamics.Diagnostics.TypeScriptCore.EventLevel.Warning:
                                    console.warn(message);
                                    break;
                                case Microsoft.Dynamics.Diagnostics.TypeScriptCore.EventLevel.LogAlways:
                                case Microsoft.Dynamics.Diagnostics.TypeScriptCore.EventLevel.Informational:
                                case Microsoft.Dynamics.Diagnostics.TypeScriptCore.EventLevel.Verbose:
                                    console.info(message);
                                    break;
                            }
                        }
                        else if (event.Type === Microsoft.Dynamics.Diagnostics.TypeScriptCore.EventType.PageView) {
                            console.info("Page Loaded: " + event.PageViewMetadata.PageName);
                        }
                    };
                    return DebuggingConsoleSink;
                })();
                TypeScriptCore.DebuggingConsoleSink = DebuggingConsoleSink;
            })(TypeScriptCore = Diagnostics.TypeScriptCore || (Diagnostics.TypeScriptCore = {}));
        })(Diagnostics = Dynamics.Diagnostics || (Dynamics.Diagnostics = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Diagnostics;
        (function (Diagnostics) {
            var TypeScriptCore;
            (function (TypeScriptCore) {
                "use strict";
                var WindowsLoggingRequest = (function () {
                    function WindowsLoggingRequest() {
                    }
                    WindowsLoggingRequest.prototype.writeEvent = function (event) {
                        var request = JSON.stringify(event);
                        window.setImmediate(function () {
                            Microsoft.Dynamics.Commerce.ClientBroker.Logger.logAsync(request)
                                .done(function (response) { }, function (error) {
                                console.error("Logging request to native logging broker failed due to error sending the request to the broker.  Error: " + error);
                            });
                        });
                    };
                    WindowsLoggingRequest.prototype.setSessionInfo = function (appSessionId, userSessionId, deviceId, deviceNumber, terminalId, userId, tenantId, offlineAvailability, offlineCurrentMode, screenResolution) {
                        window.setImmediate(function () {
                            Microsoft.Dynamics.Commerce.ClientBroker.Logger.setSessionInfoAsync(appSessionId, userSessionId, deviceId, deviceNumber, terminalId, userId, tenantId, offlineAvailability, offlineCurrentMode, screenResolution)
                                .done(function (response) { }, function (error) {
                                console.error("Setting the session info through the native logging broker failed due to error sending the request to the broker.  Error: " + error);
                            });
                        });
                    };
                    WindowsLoggingRequest.prototype.setInstrumentationKey = function (instrumentationKey) {
                        window.setImmediate(function () {
                            Microsoft.Dynamics.Commerce.ClientBroker.Logger.setInstrumentationKeyAsync(instrumentationKey)
                                .done(function (response) {
                                console.info("Called the API to set the instrumentation key on the client broker. Response: " + response);
                            }, function (error) {
                                console.error("Setting the instrumentation key through the native logging broker failed due to error sending the request to the broker.  Error: " + error);
                            });
                        });
                    };
                    return WindowsLoggingRequest;
                })();
                TypeScriptCore.WindowsLoggingRequest = WindowsLoggingRequest;
            })(TypeScriptCore = Diagnostics.TypeScriptCore || (Diagnostics.TypeScriptCore = {}));
        })(Diagnostics = Dynamics.Diagnostics || (Dynamics.Diagnostics = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
var Commerce;
(function (Commerce) {
    "use strict";
    function AccessControlData() {
        var args = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            args[_i - 0] = arguments[_i];
        }
    }
    Commerce.AccessControlData = AccessControlData;
    function CustomerContent() {
        var args = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            args[_i - 0] = arguments[_i];
        }
    }
    Commerce.CustomerContent = CustomerContent;
    function EndUserIdentifiableInformation() {
        var args = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            args[_i - 0] = arguments[_i];
        }
    }
    Commerce.EndUserIdentifiableInformation = EndUserIdentifiableInformation;
    function OrganizationIdentifiableInformation() {
        var args = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            args[_i - 0] = arguments[_i];
        }
    }
    Commerce.OrganizationIdentifiableInformation = OrganizationIdentifiableInformation;
    function AccountData() {
        var args = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            args[_i - 0] = arguments[_i];
        }
    }
    Commerce.AccountData = AccountData;
    function SystemData() {
        var args = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            args[_i - 0] = arguments[_i];
        }
    }
    Commerce.SystemData = SystemData;
})(Commerce || (Commerce = {}));
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Diagnostics;
        (function (Diagnostics) {
            var TypeScriptCore;
            (function (TypeScriptCore) {
                var PayloadAnnotator = (function () {
                    function PayloadAnnotator(func) {
                        this.payload = {};
                        var names = Utils.getParameterNames(func);
                        var values = func.arguments;
                        for (var i = 0; i < names.length; i++) {
                            var name = names[i];
                            var value = values[i];
                            if (value == undefined) {
                                this.payload[name] = "undefined";
                            }
                            else {
                                if (PayloadAnnotator.isAllowedType(value) == true) {
                                    this.payload[name] = value;
                                }
                                else {
                                    throw ("Type validation failed for parameter " + name);
                                }
                            }
                        }
                    }
                    PayloadAnnotator.isAllowedType = function (variable) {
                        for (var i = 0; i < PayloadAnnotator.allowedTypes.length; i++) {
                            if (typeof variable == PayloadAnnotator.allowedTypes[i]) {
                                return true;
                            }
                        }
                        return false;
                    };
                    PayloadAnnotator.prototype.annotate = function (event) {
                        event.Payload = this.payload;
                    };
                    PayloadAnnotator.allowedTypes = ['string', 'number', 'boolean'];
                    return PayloadAnnotator;
                })();
                TypeScriptCore.PayloadAnnotator = PayloadAnnotator;
                (function (EventType) {
                    EventType[EventType["None"] = 0] = "None";
                    EventType[EventType["Custom"] = 1] = "Custom";
                    EventType[EventType["PageView"] = 2] = "PageView";
                })(TypeScriptCore.EventType || (TypeScriptCore.EventType = {}));
                var EventType = TypeScriptCore.EventType;
                (function (EventLevel) {
                    EventLevel[EventLevel["LogAlways"] = 0] = "LogAlways";
                    EventLevel[EventLevel["Critical"] = 1] = "Critical";
                    EventLevel[EventLevel["Error"] = 2] = "Error";
                    EventLevel[EventLevel["Warning"] = 3] = "Warning";
                    EventLevel[EventLevel["Informational"] = 4] = "Informational";
                    EventLevel[EventLevel["Verbose"] = 5] = "Verbose";
                })(TypeScriptCore.EventLevel || (TypeScriptCore.EventLevel = {}));
                var EventLevel = TypeScriptCore.EventLevel;
                ;
                (function (EventChannel) {
                    EventChannel[EventChannel["Admin"] = 16] = "Admin";
                    EventChannel[EventChannel["Operational"] = 17] = "Operational";
                    EventChannel[EventChannel["Analytic"] = 18] = "Analytic";
                    EventChannel[EventChannel["Debug"] = 19] = "Debug";
                })(TypeScriptCore.EventChannel || (TypeScriptCore.EventChannel = {}));
                var EventChannel = TypeScriptCore.EventChannel;
                ;
                var PageViewMetadata = (function () {
                    function PageViewMetadata() {
                    }
                    return PageViewMetadata;
                })();
                TypeScriptCore.PageViewMetadata = PageViewMetadata;
                var EventStaticMetadata = (function () {
                    function EventStaticMetadata() {
                    }
                    return EventStaticMetadata;
                })();
                TypeScriptCore.EventStaticMetadata = EventStaticMetadata;
                var EventCoreFields = (function () {
                    function EventCoreFields() {
                    }
                    return EventCoreFields;
                })();
                TypeScriptCore.EventCoreFields = EventCoreFields;
                var Event = (function () {
                    function Event(type, appSessionId, userSessionId, deviceId, deviceNumber, terminalId, userId, tenantId, offlineAvailability, offlineCurrentMode, screenResolution) {
                        this.CoreFields = new EventCoreFields();
                        this.Type = type;
                        this.CoreFields.ClientTimestamp = Date.now();
                        this.CoreFields.AppSessionId = appSessionId;
                        this.CoreFields.UserSessionId = userSessionId;
                        this.CoreFields.DeviceId = deviceId;
                        this.CoreFields.DeviceNumber = deviceNumber;
                        this.CoreFields.TerminalId = terminalId;
                        this.CoreFields.UserId = userId;
                        this.CoreFields.TenantId = tenantId;
                        this.CoreFields.OfflineAvailability = offlineAvailability;
                        this.CoreFields.OfflineCurrentMode = offlineCurrentMode;
                        this.CoreFields.ScreenResolution = screenResolution;
                        if (this.Type == EventType.Custom) {
                            this.StaticMetadata = new EventStaticMetadata();
                        }
                        else if (this.Type == EventType.PageView) {
                            this.PageViewMetadata = new PageViewMetadata();
                        }
                    }
                    return Event;
                })();
                TypeScriptCore.Event = Event;
                var Utils = (function () {
                    function Utils() {
                    }
                    Utils.getParameterNames = function (func) {
                        var stripComments = /((\/\/.*$)|(\/\*[\s\S]*?\*\/))/mg;
                        var argNames = /([^\s,]+)/g;
                        var funcStr = func.toString().replace(stripComments, '');
                        var result = funcStr.slice(funcStr.indexOf('(') + 1, funcStr.indexOf(')')).match(argNames);
                        if (result === null) {
                            result = [];
                        }
                        return result;
                    };
                    Utils.generateGuid = function () {
                        function guidPart() {
                            return Math.floor((1 + Math.random()) * 0x10000).toString(16).substring(1);
                        }
                        return guidPart() + guidPart() + '-' + guidPart() + '-' + guidPart() + '-' + guidPart() + '-' + guidPart() + guidPart() + guidPart();
                    };
                    Utils.emptyGuid = function () {
                        return "00000000-0000-0000-0000-000000000000";
                    };
                    return Utils;
                })();
                TypeScriptCore.Utils = Utils;
                var LoggerBase = (function () {
                    function LoggerBase() {
                    }
                    LoggerBase.addAnnotator = function (annotator) {
                        LoggerBase.annotators.push(annotator);
                    };
                    LoggerBase.addLoggingSink = function (loggingSink) {
                        LoggerBase.loggingSinks.push(loggingSink);
                    };
                    LoggerBase.setEmergencySink = function (sink) {
                        LoggerBase.emergencySink = sink;
                    };
                    LoggerBase.setAppSessionId = function (id) {
                        LoggerBase.appSessionId = id;
                        this.refreshSessionInfo();
                    };
                    LoggerBase.setUserSession = function (userSessionId, userId) {
                        LoggerBase.userSessionId = userSessionId;
                        LoggerBase.userId = userId;
                        this.refreshSessionInfo();
                    };
                    LoggerBase.setDeviceOfflineInfo = function (offlineMode, isOffline) {
                        LoggerBase.offlineAvailability = offlineMode;
                        LoggerBase.offlineCurrentMode = isOffline;
                        this.refreshSessionInfo();
                    };
                    LoggerBase.setDeviceInfo = function (deviceId, deviceNumber, terminalId) {
                        LoggerBase.deviceId = deviceId;
                        LoggerBase.deviceNumber = deviceNumber;
                        LoggerBase.terminalId = terminalId;
                        this.refreshSessionInfo();
                    };
                    LoggerBase.setTenantInfo = function (tenantId) {
                        LoggerBase.tenantId = tenantId;
                        this.refreshSessionInfo();
                    };
                    LoggerBase.clearUserSession = function () {
                        LoggerBase.userSessionId = Utils.emptyGuid();
                        LoggerBase.userId = "";
                        this.refreshSessionInfo();
                    };
                    LoggerBase.getAppSessionId = function () {
                        return LoggerBase.appSessionId;
                    };
                    LoggerBase.getUserSessionId = function () {
                        return LoggerBase.userSessionId;
                    };
                    LoggerBase.getScreenResolution = function () {
                        return window.screen.width + "x" + window.screen.height;
                    };
                    LoggerBase.refreshSessionInfo = function () {
                        for (var i = 0; i < LoggerBase.loggingSinks.length; i++) {
                            LoggerBase.loggingSinks[i].setSessionInfo(LoggerBase.appSessionId, LoggerBase.userSessionId, LoggerBase.deviceId, LoggerBase.deviceNumber, LoggerBase.terminalId, LoggerBase.userId, LoggerBase.tenantId, LoggerBase.offlineAvailability, LoggerBase.offlineCurrentMode, LoggerBase.getScreenResolution());
                        }
                    };
                    LoggerBase.setInstrumentationKey = function (instrumentationKey) {
                        for (var i = 0; i < LoggerBase.loggingSinks.length; i++) {
                            LoggerBase.loggingSinks[i].setInstrumentationKey(instrumentationKey);
                        }
                    };
                    LoggerBase.writeEvent = function (name, eventId, version, channel, level, keywords, task, opCode, message) {
                        var event = new Event(EventType.Custom, LoggerBase.appSessionId, LoggerBase.userSessionId, LoggerBase.deviceId, LoggerBase.deviceNumber, LoggerBase.terminalId, LoggerBase.userId, LoggerBase.tenantId, LoggerBase.offlineAvailability, LoggerBase.offlineCurrentMode, LoggerBase.getScreenResolution());
                        event.StaticMetadata.Name = name;
                        event.StaticMetadata.Id = eventId;
                        event.StaticMetadata.Version = version;
                        event.StaticMetadata.Level = level;
                        event.StaticMetadata.LevelName = EventLevel[level];
                        event.StaticMetadata.Channel = channel;
                        event.StaticMetadata.ChannelName = EventChannel[channel];
                        event.StaticMetadata.Keywords = keywords;
                        event.StaticMetadata.Task = task;
                        event.StaticMetadata.OpCode = opCode;
                        event.StaticMetadata.Message = message;
                        var payload = new PayloadAnnotator(arguments.callee.caller);
                        payload.annotate(event);
                        this.dispatchEvent(event);
                    };
                    LoggerBase.writePageViewEvent = function (pageName) {
                        var event = new Event(EventType.PageView, LoggerBase.appSessionId, LoggerBase.userSessionId, LoggerBase.deviceId, LoggerBase.deviceNumber, LoggerBase.terminalId, LoggerBase.userId, LoggerBase.tenantId, LoggerBase.offlineAvailability, LoggerBase.offlineCurrentMode, LoggerBase.getScreenResolution());
                        event.PageViewMetadata.PageName = pageName;
                        this.dispatchEvent(event);
                    };
                    LoggerBase.dispatchEvent = function (event) {
                        try {
                            for (var i = 0; i < LoggerBase.annotators.length; i++) {
                                LoggerBase.annotators[i].annotate(event);
                            }
                            for (var i = 0; i < LoggerBase.loggingSinks.length; i++) {
                                LoggerBase.loggingSinks[i].writeEvent(event);
                            }
                        }
                        catch (Error) {
                            try {
                                if (typeof LoggerBase.emergencySink !== 'undefined') {
                                    LoggerBase.emergencySink.handleError(Error);
                                }
                            }
                            catch (Error) {
                            }
                        }
                    };
                    LoggerBase.appSessionId = Utils.emptyGuid();
                    LoggerBase.userSessionId = Utils.emptyGuid();
                    LoggerBase.deviceId = "";
                    LoggerBase.deviceNumber = "";
                    LoggerBase.terminalId = "";
                    LoggerBase.userId = "";
                    LoggerBase.tenantId = "";
                    LoggerBase.annotators = [];
                    LoggerBase.loggingSinks = [];
                    return LoggerBase;
                })();
                TypeScriptCore.LoggerBase = LoggerBase;
            })(TypeScriptCore = Diagnostics.TypeScriptCore || (Diagnostics.TypeScriptCore = {}));
        })(Diagnostics = Dynamics.Diagnostics || (Dynamics.Diagnostics = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
//# sourceMappingURL=Diagnostics.TypeScriptCore.js.map
// SIG // Begin signature block
// SIG // MIIdwgYJKoZIhvcNAQcCoIIdszCCHa8CAQExCzAJBgUr
// SIG // DgMCGgUAMGcGCisGAQQBgjcCAQSgWTBXMDIGCisGAQQB
// SIG // gjcCAR4wJAIBAQQQEODJBs441BGiowAQS9NQkAIBAAIB
// SIG // AAIBAAIBAAIBADAhMAkGBSsOAwIaBQAEFFNOO6CSd7y9
// SIG // ZhhCPm5cPj+aJWKgoIIYZDCCBMMwggOroAMCAQICEzMA
// SIG // AACb4HQ3yz1NjS4AAAAAAJswDQYJKoZIhvcNAQEFBQAw
// SIG // dzELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0
// SIG // b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1p
// SIG // Y3Jvc29mdCBDb3Jwb3JhdGlvbjEhMB8GA1UEAxMYTWlj
// SIG // cm9zb2Z0IFRpbWUtU3RhbXAgUENBMB4XDTE2MDMzMDE5
// SIG // MjEyOVoXDTE3MDYzMDE5MjEyOVowgbMxCzAJBgNVBAYT
// SIG // AlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQH
// SIG // EwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29y
// SIG // cG9yYXRpb24xDTALBgNVBAsTBE1PUFIxJzAlBgNVBAsT
// SIG // Hm5DaXBoZXIgRFNFIEVTTjo3MjhELUM0NUYtRjlFQjEl
// SIG // MCMGA1UEAxMcTWljcm9zb2Z0IFRpbWUtU3RhbXAgU2Vy
// SIG // dmljZTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoC
// SIG // ggEBAI2j4s+Bi9fLvwOiYPY7beLUGLA3BdWNNpwOc85N
// SIG // f6IQsnxDeywYV7ysp6aGfXmhtd4yZvmO/CDNq3N3z3ed
// SIG // b2Cca3jzxa2pvVtMK1WqUoBBQ0FmmaXwMGiGug8hch/D
// SIG // dT+SdsEA15ksqFk/wWKRbQn2ztMiui0An2bLU9HKVjpY
// SIG // TCGyhaOYZYzHiUpFWHurU0CfjGqyBcX+HuL/CqGootvL
// SIG // IY18lTDeMReKDelfzEJwyqQVFG6ED8LC/WwCTJOxTLbO
// SIG // tuzitc2aGhD1SOVXEHfqgd1fhEIycETJyryw+/dIOdhg
// SIG // dUmts79odC6UDhy+wXBydBAOzNtrUB8x6jT6bD0CAwEA
// SIG // AaOCAQkwggEFMB0GA1UdDgQWBBSWlbGeE1O6WCFGNOJ8
// SIG // xzlKbCDwdzAfBgNVHSMEGDAWgBQjNPjZUkZwCu1A+3b7
// SIG // syuwwzWzDzBUBgNVHR8ETTBLMEmgR6BFhkNodHRwOi8v
// SIG // Y3JsLm1pY3Jvc29mdC5jb20vcGtpL2NybC9wcm9kdWN0
// SIG // cy9NaWNyb3NvZnRUaW1lU3RhbXBQQ0EuY3JsMFgGCCsG
// SIG // AQUFBwEBBEwwSjBIBggrBgEFBQcwAoY8aHR0cDovL3d3
// SIG // dy5taWNyb3NvZnQuY29tL3BraS9jZXJ0cy9NaWNyb3Nv
// SIG // ZnRUaW1lU3RhbXBQQ0EuY3J0MBMGA1UdJQQMMAoGCCsG
// SIG // AQUFBwMIMA0GCSqGSIb3DQEBBQUAA4IBAQAhHbNT6TtG
// SIG // gaH6KhPjWiAkunalO7Z3yJFyBNbq/tKbIi+TCKKwbu8C
// SIG // pblWXv1l9o0Sfeon3j+guC4zMteWWj/DdDnJD6m2utr+
// SIG // EGjPiP2PIN6ysdZdKJMnt8IHpEclZbtS1XFNKWnoC1DH
// SIG // jJWWoF6sNzkC1V7zVCh5cdsXw0P8zWor+Q85QER8LGjI
// SIG // 0oHomSKrIFbm5O8khptmVk474u64ZPfln8p1Cu58lp9Z
// SIG // 4aygt9ZpvUIm0vWlh1IB7Cl++wW05tiXfBOAcTVfkybn
// SIG // 5F90lXF8A421H3X1orZhPe7EbIleZAR/KUts1EjqSkpM
// SIG // 54JutTq/VyYRyHiA1YDNDrtkMIIGBzCCA++gAwIBAgIK
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
// SIG // m3ZbCoBIa/15n8G9bW1qyVJzEw16UM0xggTKMIIExgIB
// SIG // ATCBlTB+MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2Fz
// SIG // aGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UE
// SIG // ChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSgwJgYDVQQD
// SIG // Ex9NaWNyb3NvZnQgQ29kZSBTaWduaW5nIFBDQSAyMDEx
// SIG // AhMzAAAAZEeElIbbQRk4AAAAAABkMAkGBSsOAwIaBQCg
// SIG // gd4wGQYJKoZIhvcNAQkDMQwGCisGAQQBgjcCAQQwHAYK
// SIG // KwYBBAGCNwIBCzEOMAwGCisGAQQBgjcCARUwIwYJKoZI
// SIG // hvcNAQkEMRYEFGfAHf22Cm9EZ9Vc1IaEMb0wMpwXMH4G
// SIG // CisGAQQBgjcCAQwxcDBuoDyAOgBEAGkAYQBnAG4AbwBz
// SIG // AHQAaQBjAHMALgBUAHkAcABlAFMAYwByAGkAcAB0AEMA
// SIG // bwByAGUALgBqAHOhLoAsaHR0cDovL3d3dy5NaWNyb3Nv
// SIG // ZnQuY29tL01pY3Jvc29mdER5bmFtaWNzLyAwDQYJKoZI
// SIG // hvcNAQEBBQAEggEAglC8UZWp2MozV6wuYQc7uHqhmEms
// SIG // jXYZv18BK3U94EL5iYz6wrq4mvKpbOa6DCzoHRPPD46B
// SIG // FoXKtndVYTuWrVbCDcyhRNSoR1IYYnVypRpqb4gUyqiN
// SIG // FP5uUUoONjHnpTFSgN1NtT/YdxHUq0nbbICe9/HoS6SK
// SIG // FfM5aZ/6QWO9EMx2Lq0oHIjkb3eVOEvUTp2JIvqphwHT
// SIG // DCdHZG2aRTXU0KjOTyRGcSqedHRwvccCRvMSiWod5Jd4
// SIG // f/gKZJUoBH0+IhIX7pS0PN5LunmjU4QyxPglwk/MfpG3
// SIG // bD6Pzp83GExT43hD2ys3CYj8sHK7JqRf8PfXzrWnpAUo
// SIG // LjLwUqGCAigwggIkBgkqhkiG9w0BCQYxggIVMIICEQIB
// SIG // ATCBjjB3MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2Fz
// SIG // aGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UE
// SIG // ChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSEwHwYDVQQD
// SIG // ExhNaWNyb3NvZnQgVGltZS1TdGFtcCBQQ0ECEzMAAACb
// SIG // 4HQ3yz1NjS4AAAAAAJswCQYFKw4DAhoFAKBdMBgGCSqG
// SIG // SIb3DQEJAzELBgkqhkiG9w0BBwEwHAYJKoZIhvcNAQkF
// SIG // MQ8XDTE2MDcyMTIxMDY1N1owIwYJKoZIhvcNAQkEMRYE
// SIG // FKgk/jEo3ZMeYronslXZ6qeuwhlYMA0GCSqGSIb3DQEB
// SIG // BQUABIIBACTO7XxJaUffVOwJbWYfHy5SfTYtjUMdPE7h
// SIG // LJHj+MEV+pDIdr5NdSUnbapVedq4m+l6j7O1EHj2FNbp
// SIG // 5TY46iJyvxOxTLbxs1vDopbGABOIR9GgR2UVkDcW02a0
// SIG // f51b3Lx1hj0uw7CDWY4h3iATZw4z221Ze1I39w1FNkh3
// SIG // g1dP2yqvLCiEEJOYAq3yKkyK7GfVP1woq70gxg/BzSf3
// SIG // CWZ1ZGhO2azqutZq1BUr706MQCxSigUJKt2PiiYOohlp
// SIG // hzZKwKsNgnwYFFS9iMBYe4k0NynOwGcgYdVfCl+cn1ws
// SIG // TEnkdfi4zvGe4hOrJ2qyhvgFnGipmdd5H+glB5LlDUY=
// SIG // End signature block
