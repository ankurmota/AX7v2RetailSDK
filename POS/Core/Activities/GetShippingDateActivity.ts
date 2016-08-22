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
     * The context for the GetShippingDateActivity class.
     * It contains the Shipping Date and delivery method description.
     */
    export interface GetShippingDateActivityContext {
        originalShippingDate: Date;
        deliveryMethodDescription: string;
    }

    /**
     * Interface definition for Get shipping date activity response.
     */
    export interface GetShippingDateActivityResponse {
        requestedShippingDate: Date;
    }

    /**
     * Activity for modifying Shipping Date on customer order.
     */
    export class GetShippingDateActivity extends Activity<GetShippingDateActivityResponse> {
        /**
         * Initializes a new instance of the GetShippingDateActivity class.
         *
         * @param {GetShippingDateActivityContext} context The activity context.
         */
        constructor(public context: GetShippingDateActivityContext) {
            super(context);
        }
    }
}