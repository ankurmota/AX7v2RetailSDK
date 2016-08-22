/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../IAsyncResult.ts'/>
///<reference path='IApplication.ts'/>
///<reference path='Globalization/IGlobalization.ts'/>

module Commerce.Host {
    "use strict";

    /**
     * Global instance of a host.
     */
    export var instance: Commerce.Host.IHost;

    /**
     * Host API.
     */
    export interface IHost {

        /**
         * Application related API.
         */
        application: IApplication;

        /**
         * Globalization related API.
         */
        globalization: IGlobalization;

        /**
         * WindowTimersExtension (setImmediate) cross-browser implementation.
         */
        timers: WindowTimersExtension;

        /**
         * The configuration provider.
         */
        configurationProvider: IConfigurationProvider;

        /**
         * The Azure Active Directory adapter.
         */
        azureActiveDirectoryAdapter: IAzureActiveDirectoryAdapter;

        /**
         * Initializes an instance.
         * @param {(configurationProvider: IConfigurationProvider) => void} configurationSetupCallback A callback that is triggered when 
         * the configuration provider has been loaded.
         * @return {IVoidAsyncResult} The result.
         */
        initializeAsync(configurationSetupCallback: (configurationProvider: IConfigurationProvider) => void): IVoidAsyncResult;

        /**
         * Gets whether the application update is required.
         * @return {boolean} True if the update is required otherwise false.
         */
        isApplicationUpdateRequired(): boolean;
    }
}