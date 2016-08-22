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
     * The context for the GetOrderTypeActivity class.
     * It contains the customer order operation identifier.
     */
    export interface GetOrderTypeActivityContext {
        operationId: Operations.RetailOperation;
    }

    /**
     * The response definition for the GetOrderTypeActivity class.
     */
    export interface GetOrderTypeActivityResponse {
        customerOrderMode: Model.Entities.CustomerOrderMode;
    }

    /**
     * Activity for getting order type on customer order.
     */
    export class GetOrderTypeActivity extends Activity<GetOrderTypeActivityResponse> {
        /**
         * Initializes a new instance of the GetOrderTypeActivity class.
         *
         * @param {GetOrderTypeActivityContext} context The activity context.
         */
        constructor(public context: GetOrderTypeActivityContext) {
            super(context);
        }
    }
}