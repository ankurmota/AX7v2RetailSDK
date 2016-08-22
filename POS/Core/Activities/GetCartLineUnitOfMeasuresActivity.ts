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
     * The context for the GetCartLineUnitOfMeasuresActivity class.
     * It contains the cart lines to get unit of measures for.
     */
    export interface GetCartLineUnitOfMeasuresActivityContext extends IActivityContext {
        /**
         * The array of cart lines with the applicable unit of measure options from which to select a new unit of measure.
         */
        cartLinesWithUnitOfMeasureOptions: {
            cartLine: Proxy.Entities.CartLine;
            unitOfMeasureOptions: Proxy.Entities.UnitOfMeasure[];
        }[];
    }

    /**
     * The response for the GetCartLineUnitOfMeasuresActivity class.
     * It contains the unit of measures for the cart lines, in the same order.
     */
    export interface GetCartLineUnitOfMeasuresActivityResponse {
        /**
         * The array of unit of measures for the cart lines, mapped one to one in the order provided by the context.
         */
        selectedUnitsOfMeasure: Proxy.Entities.UnitOfMeasure[];
    }
    

    /**
     * Activity for getting cart line unit of measures.
     */
    export class GetCartLineUnitOfMeasuresActivity extends Activity<GetCartLineUnitOfMeasuresActivityResponse> {
        /**
         * Initializes a new instance of the GetCartLineUnitOfMeasuresActivity class.
         * @param {GetCartLineUnitOfMeasuresActivityContext} context The activity context.
         */
        
        constructor(public context: GetCartLineUnitOfMeasuresActivityContext) {
            super(context);
        }
        
    }
}