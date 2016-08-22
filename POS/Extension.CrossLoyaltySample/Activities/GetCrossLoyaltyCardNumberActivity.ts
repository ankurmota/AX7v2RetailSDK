/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Commerce.Core.d.ts'/>

module Custom.Activities {
    "use strict";

    /**
     * The context for the GetCrossLoyaltyCardNumberActivity class.
     * It contains the cart to get the cross loyalty card number for.
     */
    export interface IGetCrossLoyaltyCardNumberActivityContext extends Commerce.Activities.IActivityContext {
        cart: Commerce.Model.Entities.Cart;
    }

    /**
     * The response for the GetCrossLoyaltyCardNumberActivity class.
     * It contains the cross loyalty card number.
     */
    export interface IGetCrossLoyaltyCardNumberActivityResponse {
        cardNumber: string;
    }

    /**
     * Activity for getting a loyalty card number from the customer
     */
    export class GetCrossLoyaltyCardNumberActivity extends Commerce.Activities.Activity<IGetCrossLoyaltyCardNumberActivityResponse> {
        /**
         * Initializes a new instance of the GetCrossLoyaltyCardNumberActivity class.
         * @param {IGetCrossLoyaltyCardNumberActivityContext} context The activity context.
         */
        
        constructor(public context: IGetCrossLoyaltyCardNumberActivityContext) {
            super(context);
        }
    }
} 