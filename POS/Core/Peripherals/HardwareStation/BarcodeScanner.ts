/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='HardwareStationContext.ts'/>
///<reference path='LongPollingLockPeripheralBase.ts'/>

module Commerce.Peripherals.HardwareStation {
    "use strict";

    /**
     * OPOS barcode scanner through Hardware Station.
     */
    export class BarcodeScanner extends LongPollingLockPeripheralBase<string[], string> implements IBarcodeScanner {

        private static HS_PERIPHERAL_NAME: string = "BarcodeScanner";
        private static HS_GETBARCODES_ACTION: string = "GetBarcodes";
        private static HS_LOCK_TIMEOUT: number = HardwareStationContext.HS_DEFAULT_LOCK_TIMEOUT;

        private _profile: Model.Entities.HardwareProfileScanner;

        /**
         * Ctor.
         * @param {Model.Entities.HardwareProfileScanner} profile The hardware profile for scanner.
         * @param {number} [pollingTimeoutInSeconds] The timeout for long polling of barcodes.
         */
        constructor(profile: Model.Entities.HardwareProfileScanner, pollingTimeoutInSeconds?: number) {
            super(pollingTimeoutInSeconds);
            this._profile = profile;
        }

        /**
         * Enable barcode scanner device.
         * @param {(barcode: string) => void} scannerMsgEventHandler The msg handler.
         * @returns {IVoidAsyncResult} The async result.
         */
        public enableAsync(scannerMsgEventHandler: (barcode: string) => void): IVoidAsyncResult {

            if (ObjectExtensions.isNullOrUndefined(scannerMsgEventHandler)) {
                return VoidAsyncResult.createResolved();
            }

            return this.lockAsync(scannerMsgEventHandler);
        }

        /**
         * Disable barcode scanner device for scan.
         * @returns {IVoidAsyncResult} The async result.
         */
        public disableAsync(): IVoidAsyncResult {
            return this.unlockAsync();
        }

        protected createLockRequest(): any {
            var lockRequest: BarcodeScannerLockRequest = {
                DeviceName: this._profile.DeviceName,
                DeviceType: Model.Entities.PeripheralDeviceType[this._profile.DeviceTypeValue],
                Timeout: BarcodeScanner.HS_LOCK_TIMEOUT
            };

            return lockRequest;
        }

        protected get deviceType(): Commerce.Model.Entities.PeripheralType {
            return this._profile.DeviceTypeValue;
        }

        protected get peripheralName(): string {
            return BarcodeScanner.HS_PERIPHERAL_NAME;
        }

        protected get getDataActionName(): string {
            return BarcodeScanner.HS_GETBARCODES_ACTION;
        }

        protected handleData(handler: (barcode: string) => void, hsData: string[]): void {
            if (ArrayExtensions.hasElements(hsData)) {
                hsData.forEach((barcode: string) => {
                    handler(barcode);
                });
            }
        }
    }
}