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
     * The response from the GetTransactionActivity class.
     * It contains the selected cart.
     */
    export interface GetTransactionActivityResponse {
        cart: Model.Entities.Cart;
    }
    

    /**
     * Activity for getting transaction comment.
     */
    export class GetTransactionActivity extends Activity<GetTransactionActivityResponse> {
        /**
         * Initializes a new instance of the GetTransactionActivity class.
         */
        constructor() {
            super(null);
        }
    }
}