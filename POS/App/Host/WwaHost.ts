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
     * WWA host implementation (Windows Store App, WinRT).
     */
    export class WwaHost implements IHost {

        private _configurationProvider: WwaConfigurationProvider;
        private _azureActiveDirectoryAdapter: WwaAADAuthenticationAdapter;
        private _application: WwaApplication;
        private _globalization: WwaGlobalization;

        /**
         * Initializes a new instance of the WwaHost.
         */
        constructor() {
            this._configurationProvider = new WwaConfigurationProvider();
            this._application = new WwaApplication();
            this._globalization = new WwaGlobalization();
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
            return <WindowTimersExtension>window;
        }

        /**
         * The configuration provider.
         */
        public get configurationProvider(): IConfigurationProvider {
            return this._configurationProvider;
        }

        /**
         * The authentication provider.
         */
        public get azureActiveDirectoryAdapter(): IAzureActiveDirectoryAdapter {
            return this._azureActiveDirectoryAdapter;
        }

        /**
         * Initializes an instance.
         * @param {(configurationProvider: IConfigurationProvider) => void} configurationSetupCallback A callback that is triggered when 
         * the configuration provider has been loaded.
         * @return {IVoidAsyncResult} The result.
         */
        public initializeAsync(configurationSetupCallback: (configurationProvider: IConfigurationProvider) => void): IVoidAsyncResult {
            // the elements bellow are initialized here
            // as they depend on the configuration provider
            if (configurationSetupCallback) {
                configurationSetupCallback(this.configurationProvider);
            }

            this.initializeCharms();
            this.subscribeEnableHardwareOnResume();
            this._azureActiveDirectoryAdapter = new WwaAADAuthenticationAdapter();
            return VoidAsyncResult.createResolved();
        }

        /**
         * Gets whether the application update is required.
         * @return {boolean} True if the update is required otherwise false.
         */
        public isApplicationUpdateRequired(): boolean {
            var version: IVersion = this.application.getApplicationIdentity().version;
            var currentVersionString: string = StringExtensions.format("{0}.{1}.{2}.{3}", version.major, version.minor, version.build, version.revision);
            var expectedVersionString: string = this.configurationProvider.getValue<string>("InstalledAppVersion");
            return !StringExtensions.isNullOrWhitespace(expectedVersionString) && (currentVersionString !== expectedVersionString);
        }

        private subscribeEnableHardwareOnResume(): void {
            Windows.UI.WebUI.WebUIApplication.onresuming = (): void => {
                Commerce.Peripherals.instance.barcodeScanner.enableAsync(null);
                Commerce.Peripherals.instance.magneticStripeReader.enableAsync(null);
            };
        }

        private initializeCharms(): void {
            WinJS.Application.onsettings = (args: CustomEvent): void => {

                var flyoutCommands: { [key: string]: { href: string; title: string } } = {};
                var applicationCommands: Windows.Foundation.Collections.IVector<Windows.UI.ApplicationSettings.SettingsCommand>
                    = args.detail.e.request.applicationCommands;

                applicationCommands.append(new Windows.UI.ApplicationSettings.SettingsCommand(
                    "settings",
                    Commerce.ViewModelAdapter.getResourceString("string_7401"),
                    () => Commerce.ViewModelAdapter.navigate("SettingsView")
                ));

                args.detail.applicationcommands = flyoutCommands;
                WinJS.UI.SettingsFlyout.populateSettings(args);

                applicationCommands.append(this.createHelpSettingCommand());
            };
        }

        private createHelpSettingCommand(): Windows.UI.ApplicationSettings.SettingsCommand {
            return new Windows.UI.ApplicationSettings.SettingsCommand(
                "help",
                Commerce.ViewModelAdapter.getResourceString("string_7"),
                () => {
                    Windows.System.Launcher.launchUriAsync(
                        new Windows.Foundation.Uri("http://go.microsoft.com/fwlink/?LinkId=327785")
                        );
                });
        }
    }

    Commerce.Host.instance = new WwaHost();
}