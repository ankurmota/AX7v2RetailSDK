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
     * The context for the GetPriceOverrideActivity class.
     * It contains the cart line to get the new price for.
     */
    export interface GetPriceOverrideActivityContext {
        cartLine: Model.Entities.CartLine;
    }

    /**
     * The response for the GetPriceOverrideActivity class.
     * It contains the new price for the cart line passed in.
     */
    export interface GetPriceOverrideActivityResponse {
        newPrice: number;
    }

    /**
     * Activity for getting the price override for selected cart lines.
     */
    export class GetPriceOverrideActivity extends Activity<GetPriceOverrideActivityResponse> {
        /**
         * Initializes a new instance of the GetPriceOverrideActivity class.
         *
         * @param {GetPriceOverrideActivityContext} context The activity context.
         */
        constructor(public context: GetPriceOverrideActivityContext) {
            super(context);
        }
    }
} 