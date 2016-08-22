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
     * The context for the GetCartLineQuantitiesActivity class.
     * It contains the cart lines to get quantities for.
     */
    export interface GetCartLineQuantitiesActivityContext extends IActivityContext {
        /**
         * The array of cart lines to get quantities for.
         */
        cartLines: Model.Entities.CartLine[];
    }

    /**
     * The response for the GetCartLineQuantitiesActivity class.
     * It contains the quantities for the cart lines, in the same order.
     */
    export interface GetCartLineQuantitiesActivityResponse {
        /**
         * The array of quantities for the cart lines, mapped one to one in the order provided by the context.
         */
        quantities: number[];
    }
    

    /**
     * Activity for getting cart line quantities.
     */
    export class GetCartLineQuantitiesActivity extends Activity<GetCartLineQuantitiesActivityResponse> {
        /**
         * Initializes a new instance of the GetCartLineQuantitiesActivity class.
         * @param {GetCartLineQuantitiesActivityContext} context The activity context.
         */
        
        constructor(public context: GetCartLineQuantitiesActivityContext) {
            super(context);
        }
        
    }
}