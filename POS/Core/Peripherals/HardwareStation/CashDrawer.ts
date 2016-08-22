/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../IAsyncResult.ts'/>
///<reference path='../ICashDrawer.ts'/>
///<reference path='HardwareStationContext.ts'/>

module Commerce.Peripherals.HardwareStation {
    "use strict";

    export class CashDrawer implements ICashDrawer {
        /**
         * Check if cash drawer is open.
         *
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<boolean>} The async result.
         */
        isOpenAsync(callerContext?: any): IAsyncResult<boolean> {
            var cashDrawerRequest: PeripheralOpenRequest = this.getCashDrawerProfile();

            if (cashDrawerRequest) {
                return HardwareStationContext.instance.peripheral('CashDrawer').execute<boolean>('IsOpen', cashDrawerRequest);
            }
            else {
                var asyncResult = new AsyncResult<boolean>(callerContext);

                asyncResult.resolve(false);
                return asyncResult;
            }
        }

        /**
         * Opens the cash drawer.
         *
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        openAsync(callerContext?: any): IVoidAsyncResult {
            var cashDrawerRequest: PeripheralOpenRequest = this.getCashDrawerProfile();

            if (cashDrawerRequest) {
                RetailLogger.peripheralsCashDrawerOpening(cashDrawerRequest.DeviceName, cashDrawerRequest.DeviceType);
                return HardwareStationContext.instance.peripheral('CashDrawer').execute<any>('Open', cashDrawerRequest);
            }
            else {
                var asyncResult = new VoidAsyncResult(callerContext);

                asyncResult.resolve();
                return asyncResult;
            }
        }

        private getCashDrawerProfile(): PeripheralOpenRequest {
            
            // verify if cash drawer is on local storage (selected upon opening/resuming shift
            // or during tender for shared shift)
            var cashDrawerName = ApplicationStorage.getItem(ApplicationStorageIDs.CASH_DRAWER_NAME);

            var activeCashDrawers = ApplicationContext.Instance.hardwareProfile.CashDrawers.filter((profile) => {
                return (profile.DeviceName === cashDrawerName) 
                    && ((profile.DeviceTypeValue == Model.Entities.PeripheralType.OPOS)
                    || (profile.DeviceTypeValue == Model.Entities.PeripheralType.Windows)
                    || (profile.DeviceTypeValue == Model.Entities.PeripheralType.Network));
            });

            var cashDrawerRequest: PeripheralOpenRequest = null;

            // If any cash drawer is enabled in AX, then return request.
            if (ArrayExtensions.hasElements(activeCashDrawers)) {

                if (!StringExtensions.isNullOrWhitespace(cashDrawerName)) {
                    var cashDrawerType = parseInt(ApplicationStorage.getItem(ApplicationStorageIDs.CASH_DRAWER_TYPE));

                    cashDrawerRequest = <PeripheralOpenRequest> {
                        DeviceName: cashDrawerName,
                        DeviceType: Model.Entities.PeripheralType[cashDrawerType]
                    };

                    if (cashDrawerType == Model.Entities.PeripheralType.Network) {
                        var drawerConfigurations: Model.Entities.HardwareConfiguration[];
                        var hardwareStation: Model.Entities.IHardwareStation = HardwareStationEndpointStorage.getActiveHardwareStation();
                        if (!ObjectExtensions.isNullOrUndefined(hardwareStation) && hardwareStation.ProfileId) {
                            drawerConfigurations = hardwareStation.HardwareConfigurations.CashDrawerConfigurations;
                        } else {
                            drawerConfigurations = ApplicationContext.Instance.deviceConfiguration.HardwareConfigurations.CashDrawerConfigurations;
                        }

                        if (drawerConfigurations) {
                            var drawerConfiguration: Model.Entities.HardwareConfiguration =
                                ArrayExtensions.firstOrUndefined(
                                    drawerConfigurations,
                                    (d: Model.Entities.HardwareConfiguration) => (StringExtensions.compare(d.DeviceName, cashDrawerName) === 0));

                            if (drawerConfiguration) {

                                var ipConfig = <Model.Entities.CommerceProperty> {
                                    Key: PeripheralConfigKey.IpAddress,
                                    Value: <Model.Entities.CommercePropertyValue> {
                                        StringValue: drawerConfiguration.IPAddress
                                    }
                                };
                                var portConfig = <Model.Entities.CommerceProperty> {
                                    Key: PeripheralConfigKey.Port,
                                    Value: <Model.Entities.CommercePropertyValue> {
                                        IntegerValue: drawerConfiguration.Port
                                    }
                                };
                                cashDrawerRequest.DeviceConfig = <PeripheralConfiguration> {
                                    ExtensionProperties: [ipConfig, portConfig]
                                };
                            }
                        }
                    }
                }
            }

            return cashDrawerRequest;
        }
    }
}
