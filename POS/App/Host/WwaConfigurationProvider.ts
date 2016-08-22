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
    export class WwaConfigurationProvider implements IConfigurationProvider {

        /**
         * Reads a value from the configuration.
         * @param {string} key The configuration key to be read.
         * @returns {string} the configuration value read.
         */
        public getValue<T>(key: string): T {
            var value: T;
            try {
                value = <T><Object>Microsoft.Dynamics.Commerce.ClientBroker.AppConfiguration.read(key);
            } catch (error) {
                RetailLogger.genericWarning("Could not access configuration information due to error: " + error);
            }

            return value;
        }

        /**
         * Gets an argument pass to the application by its key.
         * @param {string} key The argument name to be read.
         * @returns {string} the argument value read.
         */
        public getArgumentValue(key: string): string {
            return this.getValue<string>(key);
        }
    }
}