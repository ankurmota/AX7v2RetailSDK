/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../SharedApp/Commerce.Core.d.ts'/>

module Commerce.Host {
    "use strict";

    /**
     * Browser host implementation.
     */
    export class WebHost implements IHost {

        private _configurationProvider: WebConfigurationProvider;
        private _azureActiveDirectoryAdapter: WebAADAuthenticationAdapter;
        private _application: WebApplication;
        private _globalization: WebGlobalization;

        /**
         * Initializes a new instance of the WebHost.
         */
        constructor() {
            this._configurationProvider = new WebConfigurationProvider();
            this._azureActiveDirectoryAdapter = new WebAADAuthenticationAdapter();
            this._application = new WebApplication();
            this._globalization = new WebGlobalization();
        }

        /**
         * Initializes an instance.
         * @param {(configurationProvider: IConfigurationProvider) => void} configurationSetupCallback A callback that is triggered when 
         * the configuration provider has been loaded.
         * @return {IVoidAsyncResult} The result.
         */
        public initializeAsync(configurationSetupCallback: (configurationProvider: IConfigurationProvider) => void): IVoidAsyncResult {
            var result: VoidAsyncResult = new VoidAsyncResult(this);

            this.fixIELocalStorageSyncIssue();

            var configurationInitialization: AsyncQueue = new AsyncQueue();
            configurationInitialization.enqueue((): IVoidAsyncResult => {
                // Make sure the application is initializes before any configuration 
                // initialization happens as some of the configuration initialization
                // might depend on the application initialize.
                return this._application.initializeAsync();
            }).enqueue((): IVoidAsyncResult => {
                return this._configurationProvider.initialize();
            }).enqueue((): IVoidAsyncResult => {
                // the elements bellow are initialized here
                // as they depend on the configuration provider
                if (configurationSetupCallback) {
                    configurationSetupCallback(this.configurationProvider);
                }

                this._azureActiveDirectoryAdapter.initialize();
                return VoidAsyncResult.createResolved();
            });

            // even if one of the requests fails, we proceed with app initialization
            VoidAsyncResult.join([
                configurationInitialization.run(),
                this._globalization.initializeAsync()
            ]).done((): void => result.resolve())
                .fail((errors: Model.Entities.Error[]): void => result.resolve());

            window.addEventListener("beforeunload", this._beforeUnloadHandler.bind(this));
            window.addEventListener("hashchange", this.hashChangeHandler.bind(this));

            return result;
        }

        /**
         * Application related API.
         */
        public get application(): IApplication {
            return this._application;
        }

        /**
         * Globalization related API.
         */
        public get globalization(): IGlobalization {
            return this._globalization;
        }

        /**
         * WindowTimersExtension (setImmediate) cross-browser implementation.
         */
        public get timers(): WindowTimersExtension {
            return Commerce.Host.CrossBrowser.WindowTimersExtensionFactory.get();
        }

        /**
         * The configuration provider.
         */
        public get configurationProvider(): IConfigurationProvider {
            return this._configurationProvider;
        }

        /**
         * The Azure Active Directory adapter.
         */
        public get azureActiveDirectoryAdapter(): IAzureActiveDirectoryAdapter {
            return this._azureActiveDirectoryAdapter;
        }

        /**
         * Gets whether the application update is required.
         * @return {boolean} True if the update is required otherwise false.
         */
        public isApplicationUpdateRequired(): boolean {
            return false;
        }

        private fixIELocalStorageSyncIssue(): void {
            // NOTE: the dummy subscription is required to make IE11 synchronize localStorage between tabs properly.
            
            window.addEventListener("storage", (event: StorageEvent) => { }, false);
            
        }

        private _beforeUnloadHandler(e: BeforeUnloadEvent): string {
            if (Commerce.Session.instance.isLoggedOn && !Helpers.DeviceActivationHelper.isInDeviceActivationProcess()) {

                // Are you sure you want to exit? This will terminate any active operations.
                var message: string = Commerce.ViewModelAdapter.getResourceString("string_9000");
                e.returnValue = message;

                return message;
            }

            return undefined;
        }

        private hashChangeHandler(): void {
            // if hash is changed we reload the app to pick up new arguments and their values
            document.location.reload();
        }
    }

    Commerce.Host.instance = new WebHost();
}