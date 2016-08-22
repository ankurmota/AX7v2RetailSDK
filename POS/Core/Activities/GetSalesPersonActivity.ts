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
     * The context for the GetSalesPersonActivity class.
     * It contains sales person identifier.
     */
    export interface GetSalesPersonActivityContext {
        salesPersons: Model.Entities.Employee[];
        originalSalesPersonId: string;
    }

    /**
     * Activity for getting the sales person identifier.
     */
    export class GetSalesPersonActivity extends Activity<{ salesPersonId: string }> {
        /**
         * Initializes a new instance of the GetSalesPersonActivity class.
         *
         * @param {GetCancellationChargeActivityContext} context The activity context.
         */
        constructor(public context: GetSalesPersonActivityContext) {
            super(context);
        }
    }
}