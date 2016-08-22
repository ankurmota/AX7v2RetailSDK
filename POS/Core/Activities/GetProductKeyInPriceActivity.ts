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
     * The context for the GetProductKeyInPriceActivity class.
     * It contains the product to get the key in price for.
     */
    export interface GetProductKeyInPriceActivityContext extends IActivityContext {
        product: Model.Entities.SimpleProduct;
        minPrice: number; // The minimum value allowed. If NaN, the minimum value will not be checked.
        maxPrice: number; // The maximum value allowed. If NaN, the maximum value will not be checked.
        minPriceInclusive: boolean; // If true, the minimum value is inclusive. If false, the minimum value is exclusive (value must be greater than).
        maxPriceInclusive: boolean; // If true, the maximum value is inclusive. If false, the maximum value is exclusive (value must be less than).
    }

    /**
     * Activity for getting a price for a product.
     */
    export class GetProductKeyInPriceActivity extends Activity<{ keyInPrice: number}> {
        /**
         * Initializes a new instance of the GetProductKeyInPriceActivity class.
         *
         * @param {GetProductKeyInPriceActivityContext} context The activity context.
         */
        constructor(public context: GetProductKeyInPriceActivityContext) {
            super(context);
        }
    }
} 