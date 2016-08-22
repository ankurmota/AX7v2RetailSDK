/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../SharedApp/Peripherals/Facade/Printer.ts'/>
///<reference path='Native/BarcodeScanner.ts'/>
///<reference path='Facade/MagneticStripeReader.ts'/>
///<reference path='Native/DualDisplay.ts'/>
///<reference path='Native/MagneticStripeReader.ts'/>
///<reference path='Native/Printer.ts'/>
///<reference path='Native/Proximity.ts'/>

module Commerce.Peripherals {
    "use strict";

    import HardwareProfile = Model.Entities.HardwareProfile;

    export class WindowsPeripherals extends PeripheralsBase {

        private _initializablePeripherals: IInitializable[] = [];
        private static _instance: WindowsPeripherals = null;

        /**
         * Ctor.
         */
        constructor() {
            super();
            var nativeBarcodeScanner = new Native.BarcodeScanner(StringExtensions.EMPTY);
            var nativeMagneticStripeReaderFacade = new Facade.MagneticStripeReader();

            this.barcodeScanner = new CompositeBarcodeScanner([
                nativeBarcodeScanner,
                new HardwareStation.CompositeBarcodeScanner(),
                new KeyboardBarcodeScanParser()
            ]);
            this.cashDrawer = new HardwareStation.CashDrawer();
            this.dualDisplay = new Native.DualDisplay();
            this.lineDisplay = new HardwareStation.LineDisplay();
            this.magneticStripeReader = new CompositeMagneticStripeReader([
                nativeMagneticStripeReaderFacade,
                new HardwareStation.MagneticStripeReader(),
                new MSRKeyboardSwipeParser()
            ]);
            this.paymentTerminal = new HardwareStation.PaymentTerminal();
            this.pinPad = new HardwareStation.PinPad();
            this.proximity = new Native.Proximity();
            this.printer = this.createPrinter();
            this.scale = new HardwareStation.Scale();
            this.signatureCapture = new HardwareStation.SignatureCapture();
            this.cardPayment = new HardwareStation.CardPayment();
            this._initializablePeripherals.push(nativeBarcodeScanner);
            this._initializablePeripherals.push(nativeMagneticStripeReaderFacade);
        }

        /**
         * Gets the instance of windows peripheral.
         */
        public static get instance(): WindowsPeripherals {
            if (ObjectExtensions.isNullOrUndefined(WindowsPeripherals._instance)) {
                WindowsPeripherals._instance = new WindowsPeripherals();
            }

            return WindowsPeripherals._instance;
        }

        /**
         * Initialize peripherals.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public initializeAsync(): IVoidAsyncResult {

            var asyncResult = new VoidAsyncResult();
            var errors: Model.Entities.Error[] = [];

            if (Windows.Devices.PointOfService) {

                // PointOfService APIs takes few milleseonds to initialize on startup. Wait for 500 milliseconds before perpherals initialization.
                setTimeout(() => {
                    ObjectExtensions.forEachAsync(this._initializablePeripherals,
                        (peripheral: IInitializable, next) => {
                            peripheral.initializeAsync()
                                .done(() => next())
                                .fail((error) => {
                                    Array.prototype.push.apply(errors, error);
                                    next();
                                });
                        },
                        () => {
                            if (!ArrayExtensions.hasElements(errors)) {
                                asyncResult.resolve();
                            } else {
                                asyncResult.reject(errors);
                            }
                        }
                        );
                }, 500);
            }
            else {
                asyncResult.resolve();
            }
            return VoidAsyncResult.join([super.initializeAsync(), asyncResult]);
        }

        private createPrinter(): IPrinter {
            var printerFacade: Facade.Printer = new Facade.Printer();
            var hardwareStationPrinter: IPrinter = new HardwareStation.Printer();

            printerFacade.registerPrinter(Model.Entities.PeripheralType.OPOS, hardwareStationPrinter);
            printerFacade.registerPrinter(Model.Entities.PeripheralType.Windows, hardwareStationPrinter);
            printerFacade.registerPrinter(Model.Entities.PeripheralType.Device, new Native.Printer());
            printerFacade.registerPrinter(Model.Entities.PeripheralType.Network, hardwareStationPrinter);

            return printerFacade;
        }
    }

    Commerce.Peripherals.instance = WindowsPeripherals.instance;
}
