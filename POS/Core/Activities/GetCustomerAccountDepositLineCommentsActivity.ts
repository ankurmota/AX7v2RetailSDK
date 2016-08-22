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
     * The context for the GetCustomerAccountDepositLineCommentsActivity class.
     * It contains the customer account deposit lines to get comments for.
     */
    export interface IGetCustomerAccountDepositLineCommentsActivityContext extends IActivityContext {
        customerAccountDepositLines: Model.Entities.CustomerAccountDepositLine[];
    }

    /**
     * The response for the GetCustomerAccountDepositLineCommentsActivity class.
     * It contains the comments for the customer account deposit lines, in the same order.
     */
    export interface IGetCustomerAccountDepositLineCommentsActivityResponse {
        comments: string[];
    }

    /**
     * Activity for getting customer account deposit line comments.
     */
    export class GetCustomerAccountDepositLineCommentsActivity extends Activity<IGetCustomerAccountDepositLineCommentsActivityResponse> {
        /**
         * Initializes a new instance of the GetCustomerAccountDepositLineCommentsActivity class.
         */
        
        constructor(public context: IGetCustomerAccountDepositLineCommentsActivityContext) {
            super(context);
        }
        
    }
}