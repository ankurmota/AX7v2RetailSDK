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
     * Magnetic stripe reader type
     */
    export enum MagneticStripeReaderType {
        windowsNative,
        veriFoneE255Compatible
    }

    export class MagneticStripeReader implements IMagneticStripeReader, IInitializable {
        private _verifoneE255CompatibleDeviceIdentifier = "_VID&00010039_PID&5035";
        private _magneticStripeReaders: IMagneticStripeReader[] = [];

        /**
         * Initialize peripherals.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public initializeAsync(): IVoidAsyncResult {
            var asyncResult = new VoidAsyncResult();
            var errors: Model.Entities.Error[] = new Array();
            var self = this;

            // Device selector for Windows natively supported POS hardware through user mode drivers framework
            var msrSelector: string = null;

            // Device selector for bluetooth devices with serial port profile
            var bluetoothSerialPortDeviceSelector: string = null;

            try {
                msrSelector = Windows.Devices.PointOfService.MagneticStripeReader.getDeviceSelector();
            }
            catch (error) {
                RetailLogger.peripheralsMagneticStripeReaderGetDeviceSelectorFailed(error.message);
            }

            try {
                bluetoothSerialPortDeviceSelector = Windows.Devices.Bluetooth.Rfcomm.RfcommDeviceService.getDeviceSelector(Windows.Devices.Bluetooth.Rfcomm.RfcommServiceId.serialPort);
            }
            catch (error) {
                RetailLogger.peripheralsMagneticStripeReaderGetBluetoothDeviceSelectorFailed(error.message);
            }

            if (!msrSelector && !bluetoothSerialPortDeviceSelector) {
                asyncResult.resolve();
            }
            else {
                WinJS.Promise.join([msrSelector == null ? null : Windows.Devices.Enumeration.DeviceInformation.findAllAsync(msrSelector, null),
                    bluetoothSerialPortDeviceSelector == null ? null : Windows.Devices.Enumeration.DeviceInformation.findAllAsync(bluetoothSerialPortDeviceSelector, null)
                ]).done(
                    (args: Windows.Devices.Enumeration.DeviceInformationCollection[]) => {
                        //args[0] is the windows POS device collection
                        //args[1] is the bluetooth serial port device collection
                        var numToProcess = args.length;
                        for (var index: number = 0; index < args.length; index++) {
                            var msrType: Commerce.Peripherals.Facade.MagneticStripeReaderType = (index == 0 ? Commerce.Peripherals.Facade.MagneticStripeReaderType.windowsNative : Commerce.Peripherals.Facade.MagneticStripeReaderType.veriFoneE255Compatible);
                            var deviceInformationCollection: Windows.Devices.Enumeration.DeviceInformationCollection = args[index];
                            var deviceInfomationArray: Windows.Devices.Enumeration.DeviceInformation[] = new Array();
                            if (deviceInformationCollection && deviceInformationCollection.size > 0) {
                                deviceInformationCollection.forEach((deviceInformation) => deviceInfomationArray.push(deviceInformation));
                            }

                            ObjectExtensions.forEachAsync(deviceInfomationArray,
                                (deviceInformation, next) => {
                                    if (deviceInformation.isEnabled) {
                                        if (msrType == Commerce.Peripherals.Facade.MagneticStripeReaderType.windowsNative) {
                                            var msr = new Native.MagneticStripeReader(deviceInformation.id);
                                            msr.initializeAsync()
                                                .done(() => {
                                                    this._magneticStripeReaders.push(msr);
                                                    next();
                                                })
                                                .fail((error) => {
                                                    Array.prototype.push.apply(errors, error);
                                                    next();
                                                });
                                        }
                                        else if (msrType == Commerce.Peripherals.Facade.MagneticStripeReaderType.veriFoneE255Compatible) {
                                            // There could be several Bluetooth serial port profile devices, we are
                                            // interested only in a specific device, so we look for that specific vendor id
                                            // and product id in the device information returned.
                                            // In addition, Windows needs to ask for the consent of the user to use the bluetooth device
                                            // The thread that asks for the service needs to be an UI thread so that the
                                            // consent dialog can be shown. Here we ask for the RfcommDeviceService to trigger
                                            // that consent dialog (needs consent only the first time after a fresh install)
                                            if (deviceInformation.id.indexOf(self._verifoneE255CompatibleDeviceIdentifier) >= 0) {
                                                Windows.Devices.Bluetooth.Rfcomm.RfcommDeviceService.fromIdAsync(deviceInformation.id)
                                                    .done((rfcommService: Windows.Devices.Bluetooth.Rfcomm.RfcommDeviceService) => {
                                                        if (rfcommService) {
                                                            var msr = new Native.VfE255MagneticStripeReader(deviceInformation.id);
                                                            msr.initializeAsync()
                                                                .done(() => {
                                                                    this._magneticStripeReaders.push(msr);
                                                                    next();
                                                                })
                                                                .fail((error) => {
                                                                    Array.prototype.push.apply(errors, error);
                                                                    next();
                                                                })
                                                        }
                                                        else {
                                                            next();
                                                        }
                                                    },
                                                    (error) => {
                                                        RetailLogger.peripheralsMagneticStripeReaderRfCommDeviceServiceNotFound();
                                                        next();
                                                    }
                                                    );
                                            }
                                            else {
                                                next();
                                            }
                                        }
                                    }
                                    else {
                                        next();
                                    }
                                },
                                () => {
                                    if (--numToProcess <= 0) {
                                        // This is the final iteration of the device information collection
                                        if (errors.length == 0) {
                                            asyncResult.resolve();
                                        } else {
                                            asyncResult.reject(errors);
                                        }
                                    }
                                }
                            );
                        }
                    },
                    (error) => {
                        asyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.PERIPHERALS_MSR_NOTFOUND)]);
                    }
                    );
            }
            return asyncResult;
        }

        /**
         * Enable MSR device for scan.
         *
         * @param {(cardInfo: Model.Entities.CardInfo) => void} readerMsgEventHandler The msg handler.
         * @returns {IVoidAsyncResult} The async result.
         */
        enableAsync(readerMsgEventHandler: (cardInfo: Model.Entities.CardInfo) => void): IVoidAsyncResult {
            return VoidAsyncResult.join(this._magneticStripeReaders.map((reader: IMagneticStripeReader) => {
                return reader.enableAsync(readerMsgEventHandler);
            }));
        }

        /**
         * Disable magnetic stripe reader device.
         * @returns {IVoidAsyncResult} The async result.
         */
        public disableAsync(): IVoidAsyncResult {
            return VoidAsyncResult.join(this._magneticStripeReaders.map((reader: IMagneticStripeReader) => {
                return reader.disableAsync();
            }));
        }
    }
}
