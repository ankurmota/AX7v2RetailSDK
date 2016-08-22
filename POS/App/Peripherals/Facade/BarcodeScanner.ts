/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */



module Commerce.Peripherals.Facade {
    "use strict";

    /**
     * scanner type
     */
    export enum BarcodeScannerType {
        windowsNative,
        veriFoneE255Compatible
    }

    export class BarcodeScanner implements IBarcodeScanner, IInitializable {
        private static VERIFONE_E255_COMPATIBLE_DEVICE_IDENTIFIER: string = "_VID&00010039_PID&5035";
        private _barcodeScanners: IBarcodeScanner[] = [];

        /**
         * Initialize peripherals.
         * @return {IVoidAsyncResult} The async result.
         */
        public initializeAsync(): IVoidAsyncResult {
            var asyncResult: VoidAsyncResult = new VoidAsyncResult();
            var errors: Model.Entities.Error[] = new Array();

            // Device selector for Windows natively supported POS hardware through user mode drivers framework
            var barcodeDeviceSelector: string = null;

            // Device selector for bluetooth devices with serial port profile
            var bluetoothSerialPortDeviceSelector: string = null;

            try {
                barcodeDeviceSelector = Windows.Devices.PointOfService.BarcodeScanner.getDeviceSelector();
            } catch (error) {
                RetailLogger.peripheralsBarcodeScannerGetDeviceSelectorFailed(error.message);
            }

            try {
                bluetoothSerialPortDeviceSelector = Windows.Devices.Bluetooth.Rfcomm.RfcommDeviceService.getDeviceSelector(Windows.Devices.Bluetooth.Rfcomm.RfcommServiceId.serialPort);
            } catch (error) {
                RetailLogger.peripheralsBarcodeScannerGetBluetoothDeviceSelectorFailed(error.message);
            }

            if (!barcodeDeviceSelector && !bluetoothSerialPortDeviceSelector) {
                asyncResult.resolve();
            } else {
                WinJS.Promise.join([barcodeDeviceSelector == null ? null : Windows.Devices.Enumeration.DeviceInformation.findAllAsync(barcodeDeviceSelector, null),
                    bluetoothSerialPortDeviceSelector == null ? null : Windows.Devices.Enumeration.DeviceInformation.findAllAsync(bluetoothSerialPortDeviceSelector, null)
                ]).done(
                    (args: Windows.Devices.Enumeration.DeviceInformationCollection[]) => {
                        // args[0] is the windows POS device collection
                        // args[1] is the bluetooth serial port device collection
                        var numToProcess: number = args.length;
                        for (var i: number = 0; i < args.length; i++) {
                            var scannerType: Commerce.Peripherals.Facade.BarcodeScannerType = (i === 0 ? Commerce.Peripherals.Facade.BarcodeScannerType.windowsNative : Commerce.Peripherals.Facade.BarcodeScannerType.veriFoneE255Compatible);
                            var deviceInformationCollection: Windows.Devices.Enumeration.DeviceInformationCollection = args[i];
                            var deviceInfomationArray: Windows.Devices.Enumeration.DeviceInformation[] = new Array();
                            if (deviceInformationCollection && deviceInformationCollection.size > 0) {
                                deviceInformationCollection.forEach((deviceInformation: Windows.Devices.Enumeration.DeviceInformation) => deviceInfomationArray.push(deviceInformation));
                            }

                            ObjectExtensions.forEachAsync(deviceInfomationArray,
                                (deviceInformation: Windows.Devices.Enumeration.DeviceInformation, next: () => void) => {
                                    if (deviceInformation.isEnabled) {
                                        if (scannerType === Commerce.Peripherals.Facade.BarcodeScannerType.windowsNative) {
                                            var barcodeScanner: Native.BarcodeScanner = new Native.BarcodeScanner(deviceInformation.id);
                                            barcodeScanner.initializeAsync()
                                                .done(() => {
                                                    this._barcodeScanners.push(barcodeScanner);
                                                    next();
                                                })
                                                .fail((error: Model.Entities.Error[]) => {
                                                    Array.prototype.push.apply(errors, error);
                                                    next();
                                                });
                                        } else if (scannerType === Commerce.Peripherals.Facade.BarcodeScannerType.veriFoneE255Compatible) {
                                            // There could be several Bluetooth serial port profile devices, we are
                                            // interested only in a specific device, so we look for that specific vendor id
                                            // and product id in the device information returned.
                                            // In addition, Windows needs to ask for the consent of the user to use the bluetooth device
                                            // The thread that asks for the service needs to be an UI thread so that the
                                            // consent dialog can be shown. Here we ask for the RfcommDeviceService to trigger
                                            // that consent dialog (needs consent only the first time after a fresh install)
                                            if (deviceInformation.id.indexOf(BarcodeScanner.VERIFONE_E255_COMPATIBLE_DEVICE_IDENTIFIER) >= 0) {
                                                Windows.Devices.Bluetooth.Rfcomm.RfcommDeviceService.fromIdAsync(deviceInformation.id)
                                                    .done((rfcommService: Windows.Devices.Bluetooth.Rfcomm.RfcommDeviceService) => {
                                                        if (rfcommService) {
                                                            var barcodeScanner: Native.VfE255BarcodeScanner = new Native.VfE255BarcodeScanner(deviceInformation.id);
                                                            barcodeScanner.initializeAsync()
                                                                .done(() => {
                                                                    this._barcodeScanners.push(barcodeScanner);
                                                                    next();
                                                                })
                                                                .fail((error: Model.Entities.Error[]) => {
                                                                    Array.prototype.push.apply(errors, error);
                                                                    next();
                                                                });
                                                        } else {
                                                            next();
                                                        }
                                                    },
                                                    (error: Model.Entities.Error[]) => {
                                                        RetailLogger.peripheralsBarcodeScannerRfCommDeviceServiceNotFound();
                                                        next();
                                                    }
                                                    );
                                            } else {
                                                next();
                                            }
                                        }
                                    } else {
                                        next();
                                    }
                                },
                                () => {
                                    if (--numToProcess <= 0) {
                                        // This is the final iteration of the device information collection
                                        if (errors.length === 0) {
                                            asyncResult.resolve();
                                        } else {
                                            asyncResult.reject(errors);
                                        }
                                    }
                                }
                            );
                        }
                    },
                    (error: Model.Entities.Error[]) => {
                        asyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.PERIPHERALS_BARCODE_SCANNER_NOTFOUND)]);
                    }
                );
            }
            return asyncResult;
        }

        /**
         * Enable barcode scanner device.
         * @param {(barcode: string) => void} scannerMsgEventHandler The msg handler.
         * @returns {IVoidAsyncResult} The async result.
         */
        enableAsync(scannerMsgEventHandler: (barcode: string) => void): IVoidAsyncResult {
            return VoidAsyncResult.join(this._barcodeScanners.map((barcodeScanner: IBarcodeScanner) => {
                return barcodeScanner.enableAsync(scannerMsgEventHandler);
            }));
        }

        /**
         * Disable barcode scanner device for scan.
         * @returns {IVoidAsyncResult} The async result.
         */
        public disableAsync(): IVoidAsyncResult {
            return VoidAsyncResult.join(this._barcodeScanners.map((barcodeScanner: IBarcodeScanner) => {
                return barcodeScanner.disableAsync();
            }));
        }
    }
}
