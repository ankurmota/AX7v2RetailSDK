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
     * The context for the GetInvoiceCommentActivity class.
     * It contains the cart to get comments for.
     */
    export interface GetInvoiceCommentActivityContext extends IActivityContext {
        cart: Model.Entities.Cart;
    }

    /**
     * Activity for getting Invoice comment.
     */
    export class GetInvoiceCommentActivity extends Activity<string> {
        /**
         * Initializes a new instance of the GetInvoiceCommentActivity class.
         */
        constructor(public context: GetInvoiceCommentActivityContext) {
            super(context);
        }
    }
}