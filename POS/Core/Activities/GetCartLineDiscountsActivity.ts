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
     * The context for the GetCartLineDiscountsActivity class.
     * It contains the cart lines to get discounts for and whether it is percent (otherwise it is by amount).
     */
    export interface GetCartLineDiscountsActivityContext extends IActivityContext {
        cartLines: Model.Entities.CartLine[];
        isPercent: boolean;
    }

    /**
     * The response for the GetCartLineDiscountsActivity class.
     * It contains the discounts for the cart lines, in the same order.
     */
    export interface GetCartLineDiscountsActivityResponse {
        discounts: number[];
    }

    /**
     * Activity for getting cart line discounts.
     */
    export class GetCartLineDiscountsActivity extends Activity<GetCartLineDiscountsActivityResponse> {
        /**
         * Initializes a new instance of the GetCartLineDiscountsActivity class.
         */
        constructor(public context: GetCartLineDiscountsActivityContext) {
            super(context);
        }
    }
}