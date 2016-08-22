/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='IPeripherals.ts'/>

module Commerce.Peripherals {
    "use strict";

    /**
     * Common peripherals logic.
     */
    export class PeripheralsBase implements IPeripherals {

        /**
         * Gets the barcdoe scanner.
         */
        public barcodeScanner: IBarcodeScanner;

        /**
         * Gets the cash drawer.
         */
        public cashDrawer: ICashDrawer;

        /**
         * Gets the dual display device.
         */
        public dualDisplay: IDualDisplay;

        /**
         * Gets the line display.
         */
        public lineDisplay: ILineDisplay;

        /**
         * Gets the magnetic strip reader.
         */
        public magneticStripeReader: IMagneticStripeReader;

        /**
         * Gets the proximity device.
         */
        public proximity: IProximity;

        /**
         * Gets the payment terminal device.
         */
        public paymentTerminal: IPaymentTerminalFull;

        /**
         * Gets the pin pad device.
         */
        public pinPad: IPinPad;

        /**
         * Gets the printer device.
         */
        public printer: IPrinter;

        /**
         * Gets the scale device.
         */
        public scale: IScale;

        /**
         * Gets the card tender payment interface for payment using hardware station. (Manual, MSR etc).
         */
        public cardPayment: Commerce.Peripherals.ICardPayment;

        /**
         * Gets the signature capture device.
         */
        public signatureCapture: ISignatureCapture;
        
        /**
         * Initialize peripherals.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public initializeAsync(): IVoidAsyncResult {
            // AddCartUpdateHandler needs element, but element reference is not available for this case. 
            // Since this is executed only once during application start it is safe to add event to 'contentHost' element.
            Commerce.Session.instance.AddCartStateUpdateHandler($("#contenthost").get(0), PeripheralsBase.cartUpdateHandler);

            return VoidAsyncResult.createResolved();
        }

        private static cartUpdateHandler(cartStateType: Commerce.CartStateType, oldCart: Proxy.Entities.Cart): void {

            switch (cartStateType) {
                case Commerce.CartStateType.Started:
                case Commerce.CartStateType.Reloaded:
                    var lockHardwareStation: boolean = Commerce.Peripherals.instance
                        && !Commerce.ApplicationContext.Instance.deviceConfiguration.SelectHardwareStationOnTendering;

                    if (lockHardwareStation) {
                        // lock the hardware station.
                        PaymentHelper.callBeginTransaction();
                    }
                    break;

                case Commerce.CartStateType.Updated:
                    if (Commerce.Peripherals.instance.paymentTerminal) {
                        Commerce.Peripherals.instance.paymentTerminal.displayTransaction(Commerce.Session.instance.cart);
                    }

                    // Update dual display
                    if (Commerce.Peripherals.instance.dualDisplay) {
                        Commerce.Peripherals.instance.dualDisplay.displayTransaction(Commerce.Session.instance.cart);
                    }

                    // Update line display
                    if (Commerce.Peripherals.instance.lineDisplay) {
                        var hasOriginalCart: boolean = !ObjectExtensions.isNullOrUndefined(oldCart);

                        // Update the line display only if the original cart is not null.
                        if (hasOriginalCart) {
                            Peripherals.HardwareStation.LineDisplayHelper.displayLineItems(oldCart, Session.instance.cart);
                        }
                    }
                    break;

                case Commerce.CartStateType.Completed:
                    if (Commerce.Peripherals.instance.paymentTerminal) {
                        Commerce.Peripherals.instance.paymentTerminal.endTransaction();
                    } else if (Commerce.Peripherals.instance.cardPayment) {
                        Commerce.Peripherals.instance.cardPayment.endTransaction();
                    }

                    // When transaction ends, close pinpad device
                    if (Commerce.Peripherals.instance.pinPad) {
                        Commerce.Peripherals.instance.pinPad.closeDevice();
                    }

                    // Update dual display
                    if (Commerce.Peripherals.instance.dualDisplay) {
                        Commerce.Peripherals.instance.dualDisplay.displayTransaction(Commerce.Session.instance.cart);
                    }
                    break;
            };
        }
    }
}