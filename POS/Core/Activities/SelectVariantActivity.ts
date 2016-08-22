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
     * The context for the SelectVariantActivity class.
     * @remarks It contains the master product for which the variant product is associated.
     */
    export interface SelectVariantActivityContext {
        product: Model.Entities.SimpleProduct;
    }

    /**
     * The response for the SelectVariantActivityClass.
     * @remarks It contains the selected dimensions for the variant.
     */
    export interface SelectVariantActivityResponse {
        selectedDimensions: Proxy.Entities.ProductDimension[];
    }

    /**
     * Activity for selecting a variant, given a product.
     */
    export class SelectVariantActivity extends Activity<SelectVariantActivityResponse> {
        /**
         * Initializes a new instance of the SelectVariantActivity class.
         *
         * @param {SelectVariantActivityContext} context The activity context.
         */
        constructor(public context: SelectVariantActivityContext) {
            super(context);
        }

        /**
         * Function to be implemented.
         * @return {IAsyncResult<ICancelableResult>} async result for when the activity has completed, been canceled or failed execution.
         * @remarks The default implementation throws an exception.
         */
        public execute(): IAsyncResult<ICancelableResult> {
            return super.execute();
        }
    }
}