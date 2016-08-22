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
     * The context for the GetSerialNumberActivity class.
     * It contains the product to get the serial number for.
     */
    export interface GetSerialNumberActivityContext extends IActivityContext {
        product: Model.Entities.SimpleProduct;
    }

    /**
     * Activity for getting a serial number for a product.
     */
    export class GetSerialNumberActivity extends Activity<{ serialNumber: string }> {
        /**
         * Initializes a new instance of the GetSerialNumberActivity class.
         *
         * @param {GetSerialNumberActivityContext} context The activity context.
         */
        constructor(public context: GetSerialNumberActivityContext) {
            super(context);
        }
    }
} 