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
     * The context for the GetCartLineWeightActivity class.
     * It contains the cart line to get the weight for.
     */
    export interface GetCartLineWeightActivityContext extends IActivityContext {
        /**
         * The cart line to get weight for.
         */
        cartLine: Model.Entities.CartLine;
    }

    /**
     * The response for the GetCartLineWeightActivity class.
     * It contains the weight for the cart line.
     */
    export interface GetCartLineWeightActivityResponse {
        /**
         * The weight for the cart line.
         */
        weight: number;
    }
    

    /**
     * Activity for getting the weight for a cart line.
     */
    export class GetCartLineWeightActivity extends Activity<GetCartLineWeightActivityResponse> {
        /**
         * Initializes a new instance of the GetCartLineWeightActivity class.
         * @param {GetCartLineWeightActivityContext} context The activity context.
         */
        
        constructor(public context: GetCartLineWeightActivityContext) {
            super(context);
        }
        
    }
} 