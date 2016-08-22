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
     * The context for the GetProductKeyInQuantityActivity class.
     * It contains the product to get the key in quantity for.
     */
    export interface GetProductKeyInQuantityActivityContext extends IActivityContext {
        product: Model.Entities.SimpleProduct;
    }

    /**
     * Activity for getting a quantity for a product.
     */
    export class GetProductKeyInQuantityActivity extends Activity<{ keyInQuantity: number}> {
        /**
         * Initializes a new instance of the GetProductKeyInQuantityActivity class.
         *
         * @param {GetProductKeyInQuantityActivityContext} context The activity context.
         */
        constructor(public context: GetProductKeyInQuantityActivityContext) {
            super(context);
        }
    }
} 