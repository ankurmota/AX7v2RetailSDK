/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/Peripherals.ts'/>
///<reference path='../IAsyncResult.ts'/>

module Commerce.Peripherals {
    "use strict";

    export interface IPaymentTerminalExtension {

        /**
         * Extensibility execute method.
         * @param {string} task The task to execute.
         * @param {Commerce.Peripherals.HardwareStation.ExtensionTransaction} [extensionTransactionProperties] /
         *  The transaction properties parameters for the command.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Commerce.Peripherals.HardwareStation.ExtensionTransaction>} The async result.
         */
        executeTask(task: string, extensionTransactionProperties: Commerce.Peripherals.HardwareStation.ExtensionTransaction, callerContext?: any):
            IAsyncResult<Commerce.Peripherals.HardwareStation.ExtensionTransaction>;

    }
}