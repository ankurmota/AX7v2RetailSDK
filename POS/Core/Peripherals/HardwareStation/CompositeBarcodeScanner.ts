/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Peripherals.HardwareStation {
    "use strict";

    /**
     * Composition of all available scanners in a current hardware profile.
     */
    export class CompositeBarcodeScanner implements IBarcodeScanner {

        private _usageCount: number;
        private _pollingTimeoutInSeconds: number;
        private _barcodesLazy: Lazy<BarcodeScanner[]>;

        /**
         * Ctor.
         * @param {number} [pollingTimeoutInSeconds] The timeout for long polling of barcodes.
         */
        constructor(pollingTimeoutInSeconds?: number) {
            this._pollingTimeoutInSeconds = pollingTimeoutInSeconds;
            this.initializeLazyCollection();
            this._usageCount = 0;
        }

        /**
         * Enable barcode scanner device.
         * @param {(barcode: string) => void} scannerMsgEventHandler The msg handler.
         * @returns {IVoidAsyncResult} The async result.
         */
        public enableAsync(scannerMsgEventHandler: (barcode: string) => void): IVoidAsyncResult {
            this._usageCount++;

            return VoidAsyncResult.join(this._barcodesLazy.value.map((scanner: BarcodeScanner) => {
                return scanner.enableAsync(scannerMsgEventHandler);
            }));
        }

        /**
         * Disable barcode scanner device for scan.
         * @returns {IVoidAsyncResult} The async result.
         */
        public disableAsync(): IVoidAsyncResult {
            var scanners: BarcodeScanner[] = this._barcodesLazy.value;

            // Reset collection when usage reaches '0'. This is required to take latest hardware profile.
            if (--this._usageCount === 0) {
                this.initializeLazyCollection();
            }

            return VoidAsyncResult.join(scanners.map((scanner: BarcodeScanner) => {
                return scanner.disableAsync();
            }));
        }

         private initializeLazyCollection(): void {
            this._barcodesLazy = new Lazy<BarcodeScanner[]>(() => {
                return this.initializeBarcodeScanners(this._pollingTimeoutInSeconds);
            });
        }

        private initializeBarcodeScanners(pollingTimeoutInSeconds: number): BarcodeScanner[] {

            var scanners: BarcodeScanner[] = [];
            var scannerProfiles: Proxy.Entities.HardwareProfileScanner[] = ApplicationContext.Instance.hardwareProfile.Scanners;

            if (ObjectExtensions.isNullOrUndefined(scannerProfiles)) {
                RetailLogger.peripheralsCompositeBarcodeScannerObjectNotDefined("HardwareProfile.Scanners");
                return scanners;
            }

            scannerProfiles.forEach((profile: Proxy.Entities.HardwareProfileScanner) => {
                if (!ObjectExtensions.isNullOrUndefined(profile) && profile.DeviceTypeValue !== Proxy.Entities.PeripheralType.None) {
                    scanners.push(new BarcodeScanner(profile, pollingTimeoutInSeconds));
                }
            });

            return scanners;
        }
    }
}
