/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='OperationHandlerBase.ts' />

module Commerce.Operations {
    "use strict";

    /**
     * Options passed to the PairHardwareStation operation.
     */
    export interface IPairHardwareStationOperationOptions {
        hardwareStation: Model.Entities.IHardwareStation;
        pair: boolean;
    }

    /**
     * Handler for the PairHardwareStation operation.
     */
    export class PairHardwareStationOperationHandler extends OperationHandlerBase {
        /**
         * Executes the PairHardwareStation operation.
         *
         * @param {IPairHardwareStationOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IPairHardwareStationOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { hardwareStation: undefined, pair: undefined };

            // If line display is currently active, don't allow the hardware station to be changed.
            if (Peripherals.instance && Peripherals.instance.lineDisplay && Peripherals.instance.lineDisplay.isActive) {
                return VoidAsyncResult.createRejected([
                    new Model.Entities.Error(ErrorTypeEnum.HARDWARESTATION_CHANGE_ERROR_LINE_DISPLAY_ACTIVE)]);
            }

            var asyncQueue = new AsyncQueue();
            if (options.pair) {
                var authenticationManager: Commerce.Model.Managers.IAuthenticationManager = Commerce.Model.Managers.Factory.GetManager(Commerce.Model.Managers.IAuthenticationManagerName, null);
                var tokenResult: Model.Entities.CreateHardwareStationTokenResult;
                var pairingResult: string;

                asyncQueue
                    .enqueue(() => {
                        return authenticationManager.createHardwareStationToken()
                            .done((result) => { tokenResult = result; });
                    }).enqueue(() => {
                        var pairingRequest: Peripherals.HardwareStation.PairingRequest = {
                            DeviceNumber: ApplicationStorage.getItem(ApplicationStorageIDs.DEVICE_ID_KEY),
                            HardwareStationToken: tokenResult.HardwareStationToken
                        };

                        return Peripherals.HardwareStation.HardwareStationContext.instance
                            .security(options.hardwareStation)
                            .execute<string>('Pair', pairingRequest)
                            .done((result) => { pairingResult = result; });
                    }).enqueue(() => {
                        if (!pairingResult) {
                            var error = new Model.Entities.Error(
                                ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_PAIRINGERROR);
                            return VoidAsyncResult.createRejected([error]);
                        }

                        HardwareStationEndpointStorage.setHardwareStationToken(options.hardwareStation.RecordId, options.hardwareStation.Url, tokenResult.PairingKey);
                        return VoidAsyncResult.createResolved();
                    }).enqueue(() => {
                        return null;
                    });

            } else {
                asyncQueue.enqueue(() => {
                    return Peripherals.HardwareStation.HardwareStationContext.instance
                        .security(options.hardwareStation)
                        .execute<any>("Unpair", null)
                        .done(() => {
                            HardwareStationEndpointStorage.clearActiveHardwareStation();
                            HardwareStationEndpointStorage.setHardwareStationToken(options.hardwareStation.RecordId, options.hardwareStation.Url, StringExtensions.EMPTY);
                        });
                });
            }

            return asyncQueue.run();
        }
    }
}
