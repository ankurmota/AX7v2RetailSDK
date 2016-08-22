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
     * The context for the RegisterTimeActivity class.
     * It contains the employeeActivity.
     */
    export interface RegisterTimeActivityContext extends IActivityContext {
        employeeActivity: Model.Entities.EmployeeActivity;
    }

    /**
     * The response for the RegisterTimeActivity class.
     * It contains the employee activity type.
     */
    export interface RegisterTimeActivityResponse {
        employeeActivityType: Model.Entities.EmployeeActivityType;
    }

    /**
     * Activity to register a time.
     */
    export class RegisterTimeActivity extends Activity<RegisterTimeActivityResponse> {

        /**
         * The activity can handle the response, if a handler is provided.
         */
        public responseHandler: (response: RegisterTimeActivityResponse) => IAsyncResult<Date>;

        /**
         * Initializes a new instance of the RegisterTimeActivity class.
         */
        constructor(public context: RegisterTimeActivityContext) {
            super(context);
        }
    }
}  