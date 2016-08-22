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
     * The context for the GetReasonCodeLinesActivity class.
     * It contains the reason codes to get reason code lines out of.
     * Optionally includes the context of the reason code lines, i.e., the cart, cart line or tender line.
     */
    export interface GetReasonCodeLinesActivityContext {
        reasonCodes: Proxy.Entities.ReasonCode[];
        cart?: Proxy.Entities.Cart;
        cartLine?: Proxy.Entities.CartLine;
        tenderLine?: Proxy.Entities.TenderLine;
        affiliationLine?: Proxy.Entities.AffiliationLoyaltyTier;
        nonSalesTransaction?: Proxy.Entities.NonSalesTransaction;
        dropAndDeclareTransaction?: Proxy.Entities.DropAndDeclareTransaction;
    }

    /**
     * Activity for getting reason code lines out of reason codes.
     */
    export class GetReasonCodeLinesActivity extends Activity<{ reasonCodeLines: Model.Entities.ReasonCodeLine[] }> {
        /**
         * Initializes a new instance of the GetReasonCodeLinesActivity class.
         *
         * @param {GetReasonCodeLinesActivityContext} context The activity context.
         */
        constructor(public context: GetReasonCodeLinesActivityContext) {
            super(context);
        }
    }
} 