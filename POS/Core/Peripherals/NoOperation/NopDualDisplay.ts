/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Entities/CommerceTypes.g.ts'/>
///<reference path='../IDualDisplay.ts'/>

module Commerce.Peripherals.NoOperation {
    "use strict";

    export class NopDualDisplay implements IDualDisplay {

        /**
         * Initializes the dual display.
         * @param Model.Entities.DeviceConfiguration} deviceConfiguration The device configuration.
         */
        public initialize(deviceConfiguration: Model.Entities.DeviceConfiguration): void {
            // NOTE: Nop.
        }

        /**
         * Displays a transaction on the dual display
         * If display isn't shown yet, it creates the secondary window; otherwise, the transaction is updated
         * @param {Commerce.Model.Entities.Cart} cart Cart to be displayed.
         */
        public displayTransaction(cart: Model.Entities.Cart): void {
            // NOTE: Nop.
        }
    }
}