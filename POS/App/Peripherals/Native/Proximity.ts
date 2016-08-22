/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../../SharedApp/Commerce.Core.d.ts'/>

module Commerce.Peripherals.Native {
    "use strict";

    export class Proximity implements IProximity {

        private _proximityDevice: Windows.Networking.Proximity.ProximityDevice;

        /**
         * Initialize the ProximityDevice.
         */
        constructor() {

            try {
                this._proximityDevice = Windows.Networking.Proximity.ProximityDevice.getDefault();
            }
            catch (error) {
                RetailLogger.peripheralsProximityOpenDeviceFailed(error.message);
            }

            if (this._proximityDevice == null) {
                RetailLogger.peripheralsProximityNotAvailable();
            }
        }

        /**
        * Creates a subscription for the specified message type.
        *
        * @param {string} messageType The type of message.
        * @param {any} messageReceivedHandler The message handler.
        * @return {number} subscriptionId.
        */
        public subscribeForMessage(messageType: string, messageReceivedHandler): number {

            var subscriptionId: number = -1;

            if (this._proximityDevice != null) {
                subscriptionId = this._proximityDevice.subscribeForMessage(messageType, messageReceivedHandler);
            }

            return subscriptionId;
        }

        /**
        * Unsubscribe the message.
        *
        * @param {number} subscriptionId This subscription identifier.
        */
        public unsubscribeForMessage(subscriptionId: number) {

            if (this._proximityDevice != null) {
                this._proximityDevice.stopSubscribingForMessage(subscriptionId);
            }
        }
    }
}
