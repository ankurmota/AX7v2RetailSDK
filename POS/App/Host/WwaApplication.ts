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
     * WWA Application implementation (Windows Store App, WinRT).
     */
    export class WwaApplication implements IApplication {

        constructor() {
        }

        /**
         * Determines whether the activation kind is an application launch kind.
         * @param {any} activationKind Activation kind value.
         * @return {boolean} True if launching otherwise false.
         */
        public isApplicationLaunching(activationKind: any): boolean {
            return activationKind === Windows.ApplicationModel.Activation.ActivationKind.launch;
        }

        /**
         * Gets the current application identity.
         *
         * @return {IApplicationIdentity} Application identity. 
         */
        public getApplicationIdentity(): IApplicationIdentity {
            return <IApplicationIdentity> {
                version: Windows.ApplicationModel.Package.current.id.version,
                publisher: (<any>Windows.ApplicationModel.Package.current).publisherDisplayName
            };
        }

        /**
         * Gets a current architecture type.
         *
         * @return {Commerce.Proxy.Entities.ArchitectureType} x86, ARM, x64, etc.
         */
        public getArchitectureType(): Proxy.Entities.ArchitectureType {
            var architecture: Proxy.Entities.ArchitectureType = Commerce.Proxy.Entities.ArchitectureType.Unknown;
            var packageId = Windows.ApplicationModel.Package.current.id;

            if (packageId) {
                switch (packageId.architecture) {
                    case Windows.System.ProcessorArchitecture.x86:
                        architecture = Commerce.Proxy.Entities.ArchitectureType.X86;
                        break;

                    case Windows.System.ProcessorArchitecture.x64:
                        architecture = Commerce.Proxy.Entities.ArchitectureType.X64;
                        break;

                    case Windows.System.ProcessorArchitecture.arm:
                        architecture = Commerce.Proxy.Entities.ArchitectureType.ARM;
                        break;
                }
            }

            return architecture;
        }

        /**
         * Gets the current application type.
         * @returns {Commerce.Proxy.Entities.ApplicationTypeEnum} The application type.
         */
        public getApplicationType(): Commerce.Proxy.Entities.ApplicationTypeEnum {
            return Proxy.Entities.ApplicationTypeEnum.MposForWindows;
        }

        /**
         * Registers the binding to select on swipe for winJS grid elements.
         *
         * @param {HTMLElement} [grid] html element used for winJS grid.
         */
        public registerSwipeBinding(element: HTMLElement): void {
            //NOTE: mPOS uses native winjs implementation for swipe select.
        }

        /**
         * Gets an app specific hardware identifier.
         * @return {string} The app specific hardware identifier as a string.
         */
        public getAppSpecificHardwareId(): string {
            var ashwidBuffer: Windows.Storage.Streams.IBuffer = Windows.System.Profile.HardwareIdentification.getPackageSpecificToken(/*nonce:*/null).id;
            var ashwidBytes: number[] = new Array(ashwidBuffer.length);
            var dataReader: Windows.Storage.Streams.DataReader;
            try {
                dataReader = Windows.Storage.Streams.DataReader.fromBuffer(ashwidBuffer);
                dataReader.readBytes(ashwidBytes);
            } finally {
                if (!ObjectExtensions.isNullOrUndefined(dataReader)) {
                    dataReader.close();
                }
            }
            return ashwidBytes.toString();
        }
	
	    /**
         * Identifies current browser.
         * For mPOS it is always IE
         * @returns {Commerce.Host.BrowserType} browser type
         */
        public getBrowserType(): Commerce.Host.BrowserType {
            return Commerce.Host.BrowserType.IE11;
        }
    }
}