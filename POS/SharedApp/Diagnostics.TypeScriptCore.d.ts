/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

declare module Microsoft.Dynamics.Diagnostics.TypeScriptCore {
    class AppInsightsSink implements Microsoft.Dynamics.Diagnostics.TypeScriptCore.ILoggingSink {
        private appInsightsProxy;
        constructor(appInsightsInstrumentationKey: string, applicationName: string, applicationVersion: string);
        private application;
        private applicationVersion;
        private appSessionId;
        private userSessionId;
        private deviceId;
        private deviceNumber;
        private terminalId;
        private userId;
        private tenantId;
        private offlineAvailability;
        private offlineCurrentMode;
        setSessionInfo(appSessionId: string, userSessionId: string, deviceId: string, deviceNumber: string, terminalId: string, userId: string, tenantId: string, offlineAvailability: string, offlineCurrentMode: string): void;
        setInstrumentationKey(instrumentationKey: string): void;
        writeEvent(event: Microsoft.Dynamics.Diagnostics.TypeScriptCore.Event): void;
    }
}
declare module Microsoft.Dynamics.Diagnostics.TypeScriptCore {
    class DebuggingConsoleSink implements Microsoft.Dynamics.Diagnostics.TypeScriptCore.ILoggingSink {
        setSessionInfo(appSessionId: string, userSessionId: string, deviceId: string, deviceNumber: string, terminalId: string, userId: string, tenantId: string, offlineAvailability: string, offlineCurrentMode: string): void;
        setInstrumentationKey(instrumentationKey: string): void;
        writeEvent(event: Microsoft.Dynamics.Diagnostics.TypeScriptCore.Event): void;
    }
}
declare module Microsoft.Dynamics.Diagnostics.TypeScriptCore {
    class WindowsLoggingRequest implements Microsoft.Dynamics.Diagnostics.TypeScriptCore.ILoggingSink {
        writeEvent(event: Microsoft.Dynamics.Diagnostics.TypeScriptCore.Event): void;
        setSessionInfo(appSessionId: string, userSessionId: string, deviceId: string, deviceNumber: string, terminalId: string, userId: string, tenantId: string, offlineAvailability: string, offlineCurrentMode: string, screenResolution: string): void;
        setInstrumentationKey(instrumentationKey: string): void;
    }
}
declare module Microsoft.Dynamics.Commerce.ClientBroker {
    class Logger {
        static logAsync(string: any): any;
        static setSessionInfoAsync(appSessionId: string, userSessionId: string, deviceId: string, deviceNumber: string, terminalId: string, userId: string, tenantId: string, offlineAvailability: string, offlineCurrentMode: string, screenResolution: string): any;
        static setInstrumentationKeyAsync(instrumentationKey: string): any;
    }
}
declare module Commerce {
    function AccessControlData(...args: any[]): void;
    function CustomerContent(...args: any[]): void;
    function EndUserIdentifiableInformation(...args: any[]): void;
    function OrganizationIdentifiableInformation(...args: any[]): void;
    function AccountData(...args: any[]): void;
    function SystemData(...args: any[]): void;
}
declare module Microsoft.Dynamics.Diagnostics.TypeScriptCore {
    interface IEventAnnotator {
        annotate(event: Event): void;
    }
    class PayloadAnnotator implements IEventAnnotator {
        private payload;
        private static allowedTypes;
        static isAllowedType(variable: any): boolean;
        constructor(func: Function);
        annotate(event: Event): void;
    }
    interface ILoggingSink {
        writeEvent(event: Event): void;
        setSessionInfo(appSessionId: string, userSessionId: string, deviceId: string, deviceNumber: string, terminalId: string, userId: string, tenantId: string, offlineAvailability: string, offlineCurrentMode: string, screenResolution: string): void;
        setInstrumentationKey(instrumentationKey: string): void;
    }
    interface IEmergencySink {
        handleError(error: any): void;
    }
    enum EventType {
        None = 0,
        Custom = 1,
        PageView = 2,
    }
    enum EventLevel {
        LogAlways = 0,
        Critical = 1,
        Error = 2,
        Warning = 3,
        Informational = 4,
        Verbose = 5,
    }
    enum EventChannel {
        Admin = 16,
        Operational = 17,
        Analytic = 18,
        Debug = 19,
    }
    class PageViewMetadata {
        PageName: string;
    }
    class EventStaticMetadata {
        Name: string;
        Id: number;
        Version: number;
        Level: EventLevel;
        LevelName: string;
        Channel: EventChannel;
        ChannelName: string;
        Keywords: string[];
        Task: string;
        OpCode: string;
        Message: string;
    }
    class EventCoreFields {
        ClientTimestamp: number;
        AppSessionId: string;
        UserSessionId: string;
        DeviceId: string;
        DeviceNumber: string;
        TerminalId: string;
        UserId: string;
        TenantId: string;
        OfflineAvailability: string;
        OfflineCurrentMode: string;
        ScreenResolution: string;
    }
    class Event {
        Type: EventType;
        PageViewMetadata: PageViewMetadata;
        StaticMetadata: EventStaticMetadata;
        CoreFields: EventCoreFields;
        Payload: {
            [name: string]: any;
        };
        constructor(type: EventType, appSessionId: string, userSessionId: string, deviceId: string, deviceNumber: string, terminalId: string, userId: string, tenantId: string, offlineAvailability: string, offlineCurrentMode: string, screenResolution: string);
    }
    class Utils {
        static getParameterNames(func: Function): string[];
        static generateGuid(): string;
        static emptyGuid(): string;
    }
    class LoggerBase {
        private static appSessionId;
        private static userSessionId;
        private static deviceId;
        private static deviceNumber;
        private static terminalId;
        private static userId;
        private static tenantId;
        private static offlineAvailability;
        private static offlineCurrentMode;
        private static annotators;
        private static loggingSinks;
        private static emergencySink;
        static addAnnotator(annotator: IEventAnnotator): void;
        static addLoggingSink(loggingSink: ILoggingSink): void;
        static setEmergencySink(sink: IEmergencySink): void;
        static setAppSessionId(id: string): void;
        static setUserSession(userSessionId: string, userId: string): void;
        static setDeviceOfflineInfo(offlineMode: string, isOffline: string): void;
        static setDeviceInfo(deviceId: string, deviceNumber: string, terminalId: string): void;
        static setTenantInfo(tenantId: string): void;
        static clearUserSession(): void;
        static getAppSessionId(): string;
        static getUserSessionId(): string;
        static getScreenResolution(): string;
        private static refreshSessionInfo();
        static setInstrumentationKey(instrumentationKey: string): void;
        static writeEvent(name: string, eventId: number, version: number, channel: EventChannel, level: EventLevel, keywords: string[], task: string, opCode: string, message: string): void;
        static writePageViewEvent(pageName: string): void;
        private static dispatchEvent(event);
    }
}
