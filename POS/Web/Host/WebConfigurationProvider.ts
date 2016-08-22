/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Host {
    "use strict";

    /**
     * Provides a way to access the configuration for the application.
     */
    export class WebConfigurationProvider implements IConfigurationProvider {
        private static configurationFilePath: string = "config.json";

        /**
         * Keeps the configuration object.
         */
        private configuration: Object;

        /**
         * Collection of application arguments.
         */
        private arguments: { [argumentName: string]: string };

        /**
         * Initializes the configuration provider.
         */
        public initialize(): IVoidAsyncResult {
            this.configuration = {};

            this.readArguments();

            return DataHelper.loadJsonAsync(WebConfigurationProvider.configurationFilePath).done((data: Object): void => {
                if (typeof data === "object") {
                    this.configuration = data;
                } else {
                    RetailLogger.genericError(
                        "ConfigurationProvider: invalid configuration file. Returned configuration file is not valid JSON.");
                }
            });
        }

        /**
         * Reads a value from the configuration.
         * @param {string} key The configuration key to be read.
         * @returns {string} the configuration value read.
         */
        public getValue<T>(key: string): T {
            return <T>this.configuration[key];
        }

        /**
         * Gets an argument pass to the application by its key.
         * @param {string} key The argument name to be read.
         * @returns {string} the argument value read.
         */
        public getArgumentValue(key: string): string {
            return this.arguments[key.toLowerCase()];
        }

        /**
         * Read input arguments and populate "this.arguments".
         */
        private readArguments(): void {
            var argumentString: string = window.location.hash || "";

            // remove potential # at the begining
            if (argumentString.charAt(0) === "#") {
                argumentString = argumentString.substring(1);
            }

            this.arguments = UrlHelper.parseArguments(argumentString);
        }
    }
}