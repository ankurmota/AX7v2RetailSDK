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
     * The context for the GetTransactionDiscountActivity class.
     * It contains the cart to get discounts for and whether it is percent (otherwise it is by amount).
     */
    export interface GetTransactionDiscountActivityContext extends IActivityContext {
        cart: Model.Entities.Cart;
        isPercent: boolean;
    }

    /**
     * The response for the GetTransactionDiscountActivityResponse class.
     * It contains the discount for the cart.
     */
    export interface GetTransactionDiscountActivityResponse {
        discount: number;
    }

    /**
     * Activity for getting transaction discount.
     */
    export class GetTransactionDiscountActivity extends Activity<GetTransactionDiscountActivityResponse> {
        /**
         * Initializes a new instance of the GetTransactionDiscountActivity class.
         */
        constructor(public context: GetTransactionDiscountActivityContext) {
            super(context);
        }
    }
}