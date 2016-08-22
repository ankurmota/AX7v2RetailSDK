/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='Extensions/ObjectExtensions.ts'/>
///<reference path='RetailLogger.ts'/>
///<reference path='Extensions/StringExtensions.ts'/>

module Commerce.Proxy {
    "use strict";

    /**
    * The tracer class allows information to be logged on the device for Information, Warning or Error
    * The same method call can be used for each type
    * Example:
    *         Commerce.Proxy.Tracer.Information("Message {0} {1}", ["Arg1", "Arg2"]);     // With parameters to be formatted
    *         Commerce.Proxy.Tracer.Information("Message");                               // Without parameters
    *
    * Example of different tracer logs
    *   Dynamics-Information: <<Formatted message>> <<number of milliseconds since 1970/01/01>> 
    *   Dynamics-Warning: <<Formatted message>> <<number of milliseconds since 1970/01/01>>
    *   Dynamics-Error: <<Formatted message>> <<number of milliseconds since 1970/01/01>>
    */

    export class Tracer {
        private static TracerDynamics: string = "Dynamics-";
        private static TracerDynamicsInformation: string = "Information: ";
        private static TracerDynamicsWarning: string = "Warning: ";
        private static TracerDynamicsError: string = "Error: ";
        private static TracerDynamicsException: string = "Exception: ";

        private static TracingOn: boolean = true;

        private static Counters: any[] = new Array();
        private static Timers: any[] = new Array();

        private static init(): boolean {
            Commerce.Proxy.attachLoggingSink(new Microsoft.Dynamics.Diagnostics.TypeScriptCore.DebuggingConsoleSink());
            return true;
        }

        private static _initialized: boolean = Tracer.init();

        /**
        * Traces an informational message in the specified format.
        */
        public static Information(format: string, ...args: any[]): void {
            if (Tracer.TracingOn) {
                var newArgs = [Tracer.TracerDynamicsInformation, format].concat(args);
                RetailLogger.genericInfo(Tracer.GetMessage.apply(null, newArgs));
            }
        }

        /**
        * Traces a warning message in the specified format.
        */
        public static Warning(format: string, ...args: any[]): void {
            if (Tracer.TracingOn) {
                var newArgs = [Tracer.TracerDynamicsWarning, format].concat(args);
                RetailLogger.genericWarning(Tracer.GetMessage.apply(null, newArgs));
            }
        }

        /**
        * Traces an error message in the specified format.
        */
        public static Error(format: string, ...args: any[]): void {
            if (Tracer.TracingOn) {
                var newArgs = [Tracer.TracerDynamicsError, format].concat(args);
                RetailLogger.genericError(Tracer.GetMessage.apply(null, newArgs));
            }
        }

        /**
        * Traces an error message in the specified format.
        */
        public static Errors(errors: ProxyError[], format: string, ...args: any[]): void {
            if (Tracer.TracingOn && errors != null && errors.length > 0) {
                var errorMessage: string = '';
                var errorCode: string = '';
                var errorString: string = '';

                errors.forEach((error) => {
                    errorMessage = error.ErrorMessage || StringExtensions.EMPTY;
                    errorCode = error.ErrorCode || StringExtensions.EMPTY;
                    errorString = JSON.stringify(error);
                    Tracer.Error(format + ' ' + StringExtensions.format(" \n ErrorMessage: {0}; \n ErrorCode: {1}; \n ErrorObject: '{2}'", errorMessage, errorCode, errorString), args);
                });
            }
        }

        public static StartCounter(uniqueFunctionName: string): void {
            var counter;
            if (Tracer.Counters[uniqueFunctionName] == undefined) {
                counter = new PerformanceCounters();
                counter.Name = uniqueFunctionName;
            }
            else {
                counter = Tracer.Counters[uniqueFunctionName];
            }
            counter.Count += 1;
            Tracer.Counters[uniqueFunctionName] = counter;
            var now = new Date().getTime();
            Tracer.Timers.push(now);
        }

        public static EndCounter(uniqueFunctionName: string): void {
            var now = new Date().getTime();
            var startTime = Tracer.Timers.pop();
            var counter = Tracer.Counters[uniqueFunctionName];
            counter.TotalTime += (now - startTime);
        }

        public static DumpCounters() {
            for (var item in Tracer.Counters) {
                Tracer.Information("Method = {0} Number of calls = {1} TotalTime = {2}mS Avg = {3}mS", Tracer.Counters[item].Name, Tracer.Counters[item].Count, Tracer.Counters[item].TotalTime, (Tracer.Counters[item].TotalTime / Tracer.Counters[item].Count) );
            }
        }

        private static GetMessage(type: string, format: string, ...args: any[]) {
            var now = "" + new Date().getTime() + "mS";
            if (Tracer.TracingOn) {
                if (ObjectExtensions.isNullOrUndefined(args)) {
                    return Tracer.TracerDynamics + type + "\t" + format + "\t" + now;
                }
                else {
                    var newArgs = [format];
                    return Tracer.TracerDynamics + type + "\t" + StringExtensions.format.apply(null, newArgs.concat(Array.prototype.slice.call(args))) + "\t" + now;
                }
            }
        }
    }

    class PerformanceCounters {
        public Name: string;
        public Count: number = 0;
        public TotalTime: number = 0;
    }
}
