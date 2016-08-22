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
     * The context for the GetIncomeExpenseLineActivity class.
     * It contains the mode in which the UI is launched.
     */
    export interface GetIncomeExpenseLineActivityContext {
        accountType: Model.Entities.IncomeExpenseAccountType;
    }

    /**
     * Activity for getting an income expense line.
     */
    export class GetIncomeExpenseLineActivity extends Activity<Model.Entities.IncomeExpenseLine> {
        /**
         * Initializes a new instance of the GetIncomeExpenseLineActivity class.
         *
         * @param {GetIncomeExpenseLineActivityContext} context The activity context.
         */
        constructor(public context: GetIncomeExpenseLineActivityContext) {
            super(context);
        }
    }
}