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
     * The context for the GetCartLineCommentsActivity class.
     * It contains the cart lines to get comments for.
     */
    export interface GetCartLineCommentsActivityContext extends IActivityContext {
        cartLines: Model.Entities.CartLine[];
    }

    /**
     * The response for the GetCartLineCommentsActivity class.
     * It contains the comments for the cart lines, in the same order.
     */
    export interface GetCartLineCommentsActivityResponse {
        comments: string[];
    }

    /**
     * Activity for getting cart line comments.
     */
    export class GetCartLineCommentsActivity extends Activity<GetCartLineCommentsActivityResponse> {
        /**
         * Initializes a new instance of the GetCartLineCommentsActivity class.
         */
        constructor(public context: GetCartLineCommentsActivityContext) {
            super(context);
        }
    }
}