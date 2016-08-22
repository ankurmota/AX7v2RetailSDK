/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../SharedApp/Commerce.Core.d.ts'/>
///<reference path='UI/SwipeBinding.ts'/>
module Commerce.Host {
    import Application = Commerce.Host.IApplication;
    "use strict";

    /**
     * Application related API for browsers.
     */
    export class WebApplication implements Application {

        private static ACTIVATION_KIND_LAUNCH: string = "Windows.Launch";
        private static configurationFilePath: string = "bldver.json";

        private static DEFAULT_VERSION: IVersion = {
            major: 0,
            minor: 0,
            build: 0,
            revision: 0
        };

        private static IDENTITY: IApplicationIdentity = {
            publisher: StringExtensions.EMPTY,
            version: WebApplication.DEFAULT_VERSION
        };

        /**
         * Initializes an instance.
         * @return {IVoidAsyncResult} The result.
         */
        public initializeAsync(): IVoidAsyncResult {
            if (this.getBrowserType() === BrowserType.Chrome) {
                UI.HorizontalScoll.init();
            }
            return DataHelper.loadJsonAsync(WebApplication.configurationFilePath).done((data: IApplicationIdentity): void => {
                if (typeof data === "object") {
                    WebApplication.IDENTITY = data;

                } else {
                    RetailLogger.genericError(
                        "WebApplication: invalid configuration file. Returned configuration file is not valid JSON.");
                }
            });
        }

        /**
         * Determines whether the activation kind is an application launch kind.
         * @param {any} activationKind Activation kind value.
         * @return {boolean} True if launching otherwise false.
         */
        public isApplicationLaunching(activationKind: any): boolean {
            return activationKind === WebApplication.ACTIVATION_KIND_LAUNCH
        }

        /**
         * Gets the current application identity.
         *
         * @return {IApplicationIdentity} Application identity.
         */
        public getApplicationIdentity(): IApplicationIdentity {
            return WebApplication.IDENTITY;
        }

        /**
         * Gets the current platform type.
         *
         * @return {Commerce.Proxy.Entities.ArchitectureType} x86, ARM, x64, etc.
         */
        public getArchitectureType(): Proxy.Entities.ArchitectureType {
            return Proxy.Entities.ArchitectureType.Unknown;
        }

        /**
         * Gets the current device type.
         * @returns {Commerce.Proxy.Entities.DeviceTypeEnum} The device type.
         */
        public getApplicationType(): Commerce.Proxy.Entities.ApplicationTypeEnum {
            return Proxy.Entities.ApplicationTypeEnum.CloudPos;
        }

        /**
         * Registers the binding to select on swipe for winJS grid elements.
         * @param {HTMLElement} [grid] html element used for winJS grid.
         */
        public registerSwipeBinding(element: HTMLElement): void {
            if (this.getBrowserType() !== Commerce.Host.BrowserType.IE11) {
                var swipeBinding: UI.SwipeBinding = new UI.SwipeBinding(element);
                swipeBinding.bind();
            }
        }

        /**
         * Gets an app specific hardware identifier.
         * @return {string} The app specific hardware identifier as a string.
         */
        public getAppSpecificHardwareId(): string {
            // get the cloud session id or generate a new one if not present
            var cloudSessionId: string = Commerce.ApplicationStorage.getItem(ApplicationStorageIDs.CLOUD_SESSION_ID);
            if (StringExtensions.isNullOrWhitespace(cloudSessionId)) {
                cloudSessionId = Microsoft.Dynamics.Diagnostics.TypeScriptCore.Utils.generateGuid();
                Commerce.ApplicationStorage.setItem(ApplicationStorageIDs.CLOUD_SESSION_ID, cloudSessionId);
            }

            return cloudSessionId;
        }

        /**
         * Identifies browser from the list of supported.
         * @returns {Commerce.Host.BrowserType} browser type.
         */
        public getBrowserType(): Host.BrowserType {
            if (/Trident\/7\./.test(window.navigator.userAgent)) {
                return Host.BrowserType.IE11;
            } else if (/Edge\/\d+/.test(navigator.userAgent)) {
                return Host.BrowserType.Edge;
            } else if (!ObjectExtensions.isNullOrUndefined((<any>window).chrome)) {
                return Host.BrowserType.Chrome;
            }
            else if ((<any>window).callPhantom || (<any>window)._phantom) {
                return Host.BrowserType.Phantom;
            }

            return Commerce.Host.BrowserType.Other;
        }
    }
}
