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
     * The context for the GetLoyaltyCardActivity class.
     */
    export interface GetLoyaltyCardActivityContext extends IActivityContext {
        defaultLoyaltyCardId: string;  // The default loyalty card id
    }

    /**
     * The response for the GetLoyaltyCardActivityResponse class.
     * It contains the loyaltyCardId
     */
    export interface GetLoyaltyCardActivityResponse {
        loyaltyCardId: string;
    }

    /**
     * Activity for getting loyalty card.
     */
    export class GetLoyaltyCardActivity extends Activity<GetLoyaltyCardActivityResponse> {
        /**
         * The activity can handle the response, if a handler is provided.
         */
        public responseHandler: (response: GetLoyaltyCardActivityResponse) => IVoidAsyncResult;

        /**
         * Initializes a new instance of the GetLoyaltyCardActivity class.
         */
        constructor(public context: GetLoyaltyCardActivityContext) {
            super(context);
        }
    }
}