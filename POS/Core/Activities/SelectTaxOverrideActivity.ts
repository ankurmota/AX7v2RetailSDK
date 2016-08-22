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
     * The context for the SelectTaxOverrideActivity class.
     * It contains the list of tax overrides where the tax override is selected from.
     */
    export interface SelectTaxOverrideActivityContext {
        overrideType: Model.Entities.TaxOverrideBy;
        taxOverrides: Model.Entities.TaxOverride[];
    }

    /**
     * The response for the SelectTaxOverrideActivity class.
     */
    export interface SelectTaxOverrideActivityResponse {
        taxOverride: Model.Entities.TaxOverride;
    }

    /**
     * Activity for selecting a TaxOverride, given a product.
     */
    export class SelectTaxOverrideActivity extends Activity<SelectTaxOverrideActivityResponse> {
        /**
         * Initializes a new instance of the SelectTaxOverrideActivity class.
         *
         * @param {SelectTaxOverrideActivityContext} context The activity context.
         */
        constructor(public context: SelectTaxOverrideActivityContext) {
            super(context);
        }
    }
}