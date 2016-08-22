/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */


module Commerce.Peripherals {
    "use strict";

    export interface IProximity {

        /**
         * Creates a subscription for the specified message type.
         *
         * @param {string} messageType The type of message.
         * @param {any} messageReceivedHandler The message handler.
         * @return {number} subscriptionId.
         */
        subscribeForMessage(messageType: string, messageReceivedHandler) : number;

        /**
         * Unsubscribe the message.
         *
         * @param {number} subscriptionId This subscription identifier.
         */
        unsubscribeForMessage(subscriptionId : number);
    }
}
