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
     * The context for the GetQuotationExpirationDateActivity class.
     */
    export interface GetQuotationExpirationDateActivityContext {
        originalExpirationDate: Date;
    }

    /**
     * Interface definition for GetQuotationExpirationDateActivity activity response.
     */
    export interface GetQuotationExpirationDateActivityResponse {
        expirationDate: Date;
    }

    /**
     * Activity for modifying Shipping Date on customer order.
     */
    export class GetQuotationExpirationDateActivity extends Activity<GetQuotationExpirationDateActivityResponse> {
        /**
         * Initializes a new instance of the GetQuotationExpirationDateActivity class.
         *
         * @param {GetQuotationExpirationDateActivityContext} context The activity context.
         */
        constructor(public context: GetQuotationExpirationDateActivityContext) {
            super(context);
        }
    }
}