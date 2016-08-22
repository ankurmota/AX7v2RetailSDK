/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../SharedApp/Commerce.Core.d.ts'/>
///<reference path='../../SharedApp/Peripherals/Facade/Printer.ts'/>

module Commerce.Peripherals {
    "use strict";

    export class WebPeripherals extends PeripheralsBase {

        private static _instance: WebPeripherals = null;

        /**
         * Ctor.
         */
        constructor() {
            super();
            this.barcodeScanner = new CompositeBarcodeScanner([
                new HardwareStation.CompositeBarcodeScanner(),
                new KeyboardBarcodeScanParser()
            ]);
            this.cashDrawer = new HardwareStation.CashDrawer();
            this.dualDisplay = new NoOperation.NopDualDisplay();
            this.lineDisplay = new HardwareStation.LineDisplay();
            this.magneticStripeReader = new CompositeMagneticStripeReader([
                new MSRKeyboardSwipeParser(),
                new HardwareStation.MagneticStripeReader()
            ]);
            this.paymentTerminal = new HardwareStation.PaymentTerminal();
            this.pinPad = new HardwareStation.PinPad();
            this.proximity = new NoOperation.NopProximity();
            this.printer = this.createPrinter();
            this.scale = new HardwareStation.Scale();
            this.signatureCapture = new HardwareStation.SignatureCapture();
            this.cardPayment = new HardwareStation.CardPayment();
        }

        /**
         * Gets the instance of windows peripheral.
         */
        public static get instance(): WebPeripherals {
            if (ObjectExtensions.isNullOrUndefined(WebPeripherals._instance)) {
                WebPeripherals._instance = new WebPeripherals();
            }

            return WebPeripherals._instance;
        }

        /**
         * Initialize peripherals.
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        public initializeAsync(callerContext?: any): IVoidAsyncResult {
            return super.initializeAsync();
        }

        private createPrinter(): IPrinter {
            var printerFacade: Facade.Printer = new Facade.Printer();
            var hardwareStationPrinter: IPrinter = new HardwareStation.Printer();

            printerFacade.registerPrinter(Model.Entities.PeripheralType.OPOS, hardwareStationPrinter);
            printerFacade.registerPrinter(Model.Entities.PeripheralType.Windows, hardwareStationPrinter);

            return printerFacade;
        }
    }

    Commerce.Peripherals.instance = WebPeripherals.instance;
}
