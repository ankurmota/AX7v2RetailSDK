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
     * The context for the GetShippingChargeActivity class.
     * It contains the Shipping charge amount and delivery method description.
     */
    export interface GetShippingChargeActivityContext {
        originalShippingCharge: number;
        deliveryMethodDescription: string;
    }

    /**
     * Interface definition for Get shipping charge activity response.
     */
    export interface GetShippingChargeActivityResponse {
        shippingChargeAmount: number;
    }

    /**
     * Activity for modifying Shipping charge on customer order.
     */
    export class GetShippingChargeActivity extends Activity<GetShippingChargeActivityResponse> {
        /**
         * Initializes a new instance of the GetShippingChargeActivity class.
         *
         * @param {GetShippingChargeActivityContext} context The activity context.
         */
        constructor(public context: GetShippingChargeActivityContext) {
            super(context);
        }
    }
}