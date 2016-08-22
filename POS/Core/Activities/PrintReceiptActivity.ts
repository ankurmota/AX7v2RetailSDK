/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='Activity.ts'/>

module Commerce.Activities {
    "use strict";

    /**
     * The context for the PrintReceiptActivity class.
     * It contains the receipts to print.
     */
    export interface PrintReceiptActivityContext {
        receipts: Model.Entities.Receipt[];
        notifyOnNoPrintableReceipts: boolean;
    }

    /**
     * Activity for asking user printing options and sending it to printer.
     */
    export class PrintReceiptActivity extends Activity<INullResponse> {
        /**
         * Initializes a new instance of the PrintReceiptActivity class.
         *
         * @param {PrintReceiptActivityContext} context The activity context.
         */
        constructor(public context: PrintReceiptActivityContext) {
            super(context);
        }
    }
}