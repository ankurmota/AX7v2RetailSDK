/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='HardwareStationContext.ts'/>

module Commerce.Peripherals.HardwareStation {
    "use strict";

    /**
     * OPOS magnetic stripe reader through Hardware Station.
     */
    export class MagneticStripeReader extends LongPollingLockPeripheralBase<MagneticCardSwipeInfo, Model.Entities.CardInfo> implements IMagneticStripeReader {

        private static HS_PERIPHERAL_NAME: string = "Msr";
        private static HS_GETMSRSWIPEINFO_ACTION: string = "GetMsrSwipeInfo";
        private static HS_LOCK_TIMEOUT: number = HardwareStationContext.HS_DEFAULT_LOCK_TIMEOUT;

        /**
         * Ctor.
         * @param {number} [pollingTimeoutInSeconds] The timeout for long polling of barcodes.
         */
        constructor(pollingTimeoutInSeconds?: number) {
            super(pollingTimeoutInSeconds);
        }

        /**
         * Enable MSR device for scan.
         * @param {cardInfo: Model.Entities.CardInfo) => void} readerMsgEventHandler The msg handler.
         * @returns {IVoidAsyncResult} The async result.
         */
        public enableAsync(readerMsgEventHandler: (barcode: string) => void): IVoidAsyncResult {

            if (ObjectExtensions.isNullOrUndefined(readerMsgEventHandler)) {
                return VoidAsyncResult.createResolved();
            }

            return this.lockAsync(readerMsgEventHandler);
        }

        /**
         * Disable MSR device for scan.
         * @returns {IVoidAsyncResult} The async result.
         */
        public disableAsync(): IVoidAsyncResult {
            return this.unlockAsync();
        }

        /**
         * Create the request object for lock.
         * @returns {any} The lock request.
         */
        protected createLockRequest(): any {
            return <MsrLockRequest> {
                DeviceName: this.hardwareProfile.MsrDeviceName,
                DeviceType: Model.Entities.PeripheralDeviceType[this.hardwareProfile.MsrDeviceTypeValue],
                Timeout: MagneticStripeReader.HS_LOCK_TIMEOUT
            };
        }

        /**
         * Gets the device type.
         * @returns {Commerce.Model.Entities.PeripheralType} The device type.
         */
        protected get deviceType(): Commerce.Model.Entities.PeripheralType {
            return this.hardwareProfile.MsrDeviceTypeValue;
        }

        /**
         * Gets the peripheral name (DeviceName).
         * @returns {string} The name.
         */
        protected get peripheralName(): string {
            return MagneticStripeReader.HS_PERIPHERAL_NAME;
        }

        /**
         * Gets the ODATA action name to get THSData.
         * @returns {string} The action name.
         */
        protected get getDataActionName(): string {
            return MagneticStripeReader.HS_GETMSRSWIPEINFO_ACTION;
        }

        /**
         * Handles the THSData using the peripheral handler.
         * @returns {string} The actiona name.
         */
        protected handleData(handler: (data: Model.Entities.CardInfo) => void, data: MagneticCardSwipeInfo): void {
            var cardInfo: Model.Entities.CardInfo = this.convertSwipeToCardInfo(data);
            handler(cardInfo);
        }

        private get hardwareProfile(): Model.Entities.HardwareProfile {
            return ApplicationContext.Instance.hardwareProfile;
        }

        private convertSwipeToCardInfo(swipeInfo: MagneticCardSwipeInfo): Model.Entities.CardInfo {
            return {
                CardNumber: swipeInfo.AccountNumber,
                FirstName: swipeInfo.FirstName,
                LastName: swipeInfo.LastName,
                ExpirationMonth: swipeInfo.ExpirationMonth,
                ExpirationYear: swipeInfo.ExpirationYear
            };
        }
    }
}