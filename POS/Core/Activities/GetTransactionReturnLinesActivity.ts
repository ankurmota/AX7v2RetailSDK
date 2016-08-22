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
     * The context for the GetTransactionReturnLinesActivity class.
     * It contains the default sales order to return.
     */
    export interface IGetTransactionReturnLinesActivityContext extends IActivityContext {
        /**
         * Default sales order to return.
         */
        salesOrder: Model.Entities.SalesOrder;
    }

    /**
     * The return data for the GetTransactionReturnLinesActivity class.
     * It contains the sales order to return.
     */
    export interface IGetTransactionReturnLinesActivityResponse {
        /**
         * The sales order to return.
         */
        salesOrder: Model.Entities.SalesOrder;

        /**
         * The lines of the sales order to return.
         */
        salesLines: Model.Entities.SalesLine[];
    }

    /**
     * Activity for getting the lines to return from the sales order.
     */
    export class GetTransactionReturnLinesActivity extends Activity<IGetTransactionReturnLinesActivityResponse> {
        /**
         * Initializes a new instance of the GetTransactionReturnLinesActivity class.
         * @param {GetTransactionReturnLinesActivity} context The activity context.
         */
        constructor(context: IGetTransactionReturnLinesActivityContext) {
            super(context);
        }
    }
}