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
     * The context for the GetCashDrawerActivity class.
     */
    export interface IGetCashDrawerActivityContext extends IActivityContext {
        availableCashDrawers: Proxy.Entities.HardwareProfileCashDrawer[];
    }

    /**
     * The response for the GetCashDrawerActivity class.
     */
    export interface IGetCashDrawerActivityResponse {
        /**
         * The selected cash drawer.
         */
        cashDrawer: Proxy.Entities.HardwareProfileCashDrawer;
    }

    /**
     * Activity for getting cash drawer selection.
     */
    export class GetCashDrawerActivity extends Activity<IGetCashDrawerActivityResponse> {
        /**
         * Initializes a new instance of the GetCashDrawerActivity class.
         */
        
        constructor(public context: IGetCashDrawerActivityContext) {
        
            super(context);
        }
    }
}