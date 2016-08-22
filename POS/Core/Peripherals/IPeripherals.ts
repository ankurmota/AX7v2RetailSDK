/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='IBarcodeScanner.ts'/>
///<reference path='ICashDrawer.ts'/>
///<reference path='ILineDisplay.ts'/>
///<reference path='IMagneticStripeReader.ts'/>
///<reference path='IPaymentTerminal.ts'/>
///<reference path='IProximity.ts'/>
///<reference path='IPrinter.ts'/>

module Commerce.Peripherals {
    "use strict";

    /**
     * Instance of devices.
     */
    export var instance: Commerce.Peripherals.IPeripherals;

    export interface IPeripherals extends IInitializable {

        barcodeScanner: IBarcodeScanner;

        cashDrawer: ICashDrawer;
        
        dualDisplay: IDualDisplay;

        lineDisplay: ILineDisplay;

        magneticStripeReader: IMagneticStripeReader;

        paymentTerminal: IPaymentTerminalFull;

        pinPad: IPinPad;

        proximity: IProximity;

        printer: IPrinter;

        scale: IScale;

        signatureCapture: ISignatureCapture;

        cardPayment: ICardPayment;
    }
}
