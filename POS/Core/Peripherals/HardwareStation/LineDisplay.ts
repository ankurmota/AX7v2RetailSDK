/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../IAsyncResult.ts'/>
///<reference path='../ILineDisplay.ts'/>
///<reference path='HardwareStationContext.ts'/>

module Commerce.Peripherals.HardwareStation {
    "use strict";

    export class LineDisplay implements ILineDisplay {

        private static HS_LOCK_TIMEOUT: number = HardwareStationContext.HS_DEFAULT_LOCK_TIMEOUT;

        public lineLength: number;
        public isActive: boolean = false;

        private _lockToken: string;

        /**
         * Opens the line display device for use.
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        public openDevice(callerContext?: any): IVoidAsyncResult {
            var asyncResult: VoidAsyncResult = new VoidAsyncResult(callerContext);
            var self: LineDisplay = this;
            var hardwareProfile: Model.Entities.HardwareProfile = Commerce.ApplicationContext.Instance.hardwareProfile;
            self.isActive = false;
            self._lockToken = null;

            if (hardwareProfile.LineDisplayDeviceTypeValue !== Model.Entities.PeripheralDeviceType.None) {

                var lineDisplayLockRequest: LineDisplayLockRequest = {
                    DeviceName: hardwareProfile.LineDisplayDeviceName,
                    DeviceType: Commerce.Model.Entities.PeripheralDeviceType[hardwareProfile.LineDisplayDeviceTypeValue],
                    CharacterSet: hardwareProfile.LineDisplayCharacterSet,
                    BinaryConversion: hardwareProfile.LineDisplayBinaryConversion,
                    Timeout: LineDisplay.HS_LOCK_TIMEOUT // 8 hours
                };

                HardwareStationContext.instance.peripheral("LineDisplay").execute<LineDisplayLockResponse>("Lock", lineDisplayLockRequest)
                    .done((result: LineDisplayLockResponse) => {
                        self._lockToken = result.Token;
                        self.lineLength = result.Columns;
                        self.isActive = true;
                        asyncResult.resolve();
                    })
                    .fail((error: Model.Entities.Error[]) => asyncResult.reject(error));
            } else {
                asyncResult.resolve();
            }

            return asyncResult;
        }

        /**
         * Closes the line display device.
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        public closeDevice(callerContext?: any): IVoidAsyncResult {
            var asyncResult: VoidAsyncResult = new VoidAsyncResult(callerContext);
            var self: LineDisplay = this;

            if (self.isActive) {
                var unlockRequest: UnlockRequest = {
                    Token: self._lockToken
                };

                HardwareStationContext.instance.peripheral("LineDisplay").execute<void>("Unlock", unlockRequest)
                    .done(() => {
                        self._lockToken = null;
                        self.isActive = false;
                        asyncResult.resolve();
                    })
                    .fail((error: Model.Entities.Error[]) => asyncResult.reject(error));
            } else {
                asyncResult.resolve();
            }

            return asyncResult;
        }

        /**
         * Displays text on line display.
         * @param {string[]} lines The lines to be displayed.
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        public displayLines(lines: string[], callerContext?: any): IVoidAsyncResult {
            var asyncResult: VoidAsyncResult = new VoidAsyncResult(callerContext);
            var lineDisplayRequest: LineDisplayRequest = this.getLineDisplayRequest(lines);

            if (this.isActive && lineDisplayRequest) {
                HardwareStationContext.instance.peripheral("LineDisplay").execute<void>("DisplayText", lineDisplayRequest)
                    .done(() => {
                        asyncResult.resolve();
                    })
                    .fail((error: Model.Entities.Error[]) => asyncResult.reject(error));
            } else {
                asyncResult.resolve();
            }

            return asyncResult;
        }

        /**
         * Creates the line display request with the lines to be displayed.
         * @param {string[]} lines The lines to be displayed.
         * @return {LineDisplayRequest} The line display request.
         */
        private getLineDisplayRequest(lines: string[]): LineDisplayRequest {
            var lineDisplayRequest: LineDisplayRequest = null;
            var hardwareProfile: Model.Entities.HardwareProfile = Commerce.ApplicationContext.Instance.hardwareProfile;

            if (hardwareProfile.LineDisplayDeviceTypeValue !== Model.Entities.PeripheralDeviceType.None) {

                lineDisplayRequest = <LineDisplayRequest> {
                    DeviceName: hardwareProfile.LineDisplayDeviceName,
                    DeviceType: Model.Entities.PeripheralDeviceType[hardwareProfile.LineDisplayDeviceTypeValue],
                    Token: this._lockToken,
                    Lines: lines,
                    Clear: true
                };
            }

            return lineDisplayRequest;
        }
    }
}