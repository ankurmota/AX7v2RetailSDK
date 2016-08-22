/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Activities {
    "use strict";

    export class MerchantInformationActivityHelper {
        public static saveMerchantInformationLocalAsync(request: Peripherals.HardwareStation.SavePaymentMerchantInformationRequest,
            hardwareProfileId?: string): IVoidAsyncResult {
            // construct the local/offline hardware station.
            var hardwareStation: Model.Entities.HardwareStation = {
                RecordId: 0,
                HostName: "localhost",
                Description: "Local Hardwarestation",
                Url: Peripherals.HardwareStation.HardwareStationContext.localStation,
                ProfileId: hardwareProfileId,
                IsActive: undefined,
                IsPaired: undefined,
                EftTerminalId: undefined,
                HardwareConfigurations: undefined
            };

            return Peripherals.HardwareStation.HardwareStationContext.instance
                .security(hardwareStation)
                .execute<boolean>("SaveMerchantInformationLocal", request);
        }
    }
}