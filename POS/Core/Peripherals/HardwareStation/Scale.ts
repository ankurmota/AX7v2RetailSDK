/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../IAsyncResult.ts'/>
///<reference path='../IScale.ts'/>
///<reference path='HardwareStationContext.ts'/>

module Commerce.Peripherals.HardwareStation {
    "use strict";

    export class Scale implements IScale {

        /**
         * Reads the weight value from scale.
         *
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<number>} The async result.
         */
        read(callerContext?: any): IAsyncResult<number> {
            var scaleRequest: ScaleRequest = this.getScaleRequest();

            if (scaleRequest) {
                return HardwareStationContext.instance.peripheral('Scale').execute<number>('Read', scaleRequest, null, /* suppressGlobalErrorEvent: */true);
            }
            else {
                var asyncResult = new VoidAsyncResult(callerContext);

                asyncResult.resolve();
                return asyncResult;
            }

            return asyncResult;
        }

        private getScaleRequest(): ScaleRequest {

            var scaleRequest: ScaleRequest = null;

            if (ApplicationContext.Instance.hardwareProfile.ScaleDeviceTypeValue != Model.Entities.PeripheralDeviceType.None) {

                scaleRequest = <ScaleRequest> {
                    DeviceName: ApplicationContext.Instance.hardwareProfile.ScaleDeviceName,
                    DeviceType: Model.Entities.PeripheralDeviceType[ApplicationContext.Instance.hardwareProfile.ScaleDeviceTypeValue],
                    Timeout: ApplicationContext.Instance.hardwareProfile.ScaleTimeoutInSeconds
                };
            }

            return scaleRequest;
        }
    }
}