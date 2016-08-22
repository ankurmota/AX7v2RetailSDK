/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../IAsyncResult.ts'/>

module Commerce.Peripherals {
    "use strict";

    /**
     * Interface definition for credit card and debit card payment using hardware station (Manual, MSR etc).
     */
    export interface ICardPayment {

        /**
         * Lock token.
         */
        lockToken: string;

        /**
         * Begins a transaction on the hardware station.
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        beginTransaction(callerContext?: any): IVoidAsyncResult;

        /**
         * Ends a transaction on the hardware station.
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        endTransaction(callerContext?: any): IVoidAsyncResult;

        /**
         * Authorizes payment given the tender information.
         * @param {number} amount The amount to get authorization for.
         * @param {Commerce.Peripherals.HardwareStation.TenderInfo} tenderInfo The tender information.
         * @param {Commerce.Peripherals.HardwareStation.ExtensionTransaction} [extensionTransactionProperties] \
         *   The transaction properties parameters for the command.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>} The async result.
         */
        authorizePayment(amount: number, tenderInfo: Commerce.Peripherals.HardwareStation.TenderInfo,
            extensionTransactionProperties: Commerce.Peripherals.HardwareStation.ExtensionTransaction, callerContext?: any):
            IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>;

        /**
         * Voids the payment.
         * @param {number} amount The amount to void payment for.
         * @param {string} paymentPropertiesXml The payment properties blob.
         * @param {Commerce.Peripherals.HardwareStation.ExtensionTransaction} [extensionTransactionProperties] \
         *   The transaction properties parameters for the command.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>} The async result.
         */
        voidPayment(amount: number, paymentPropertiesXml: string,
            extensionTransactionProperties: Commerce.Peripherals.HardwareStation.ExtensionTransaction, callerContext?: any):
            IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>;

        /**
         * Captures the payment.
         * @param {number} amount The amount to capture payment for.
         * @param {string} paymentPropertiesXml The payment properties blob.
         * @param {Commerce.Peripherals.HardwareStation.ExtensionTransaction} [extensionTransactionProperties] \
         *   The transaction properties parameters for the command.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>} The async result.
         */
        capturePayment(amount: number, paymentPropertiesXml: string,
            extensionTransactionProperties: Commerce.Peripherals.HardwareStation.ExtensionTransaction, callerContext?: any):
            IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>;

        /**
         * Refunds the payment given the tender information.
         * @param {number} amount The amount to get refund for.
         * @param {Commerce.Peripherals.HardwareStation.TenderInfo} tenderInfo The tender information.
         * @param {Commerce.Peripherals.HardwareStation.ExtensionTransaction} [extensionTransactionProperties] \
         *   The transaction properties parameters for the command.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>} The async result.
         */
        refundPayment(amount: number, tenderInfo: Commerce.Peripherals.HardwareStation.TenderInfo,
            extensionTransactionProperties: Commerce.Peripherals.HardwareStation.ExtensionTransaction, callerContext?: any):
            IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>;

        /**
         * Gets the card token given the tender information.
         * @param {Commerce.Peripherals.HardwareStation.TenderInfo} tenderInfo The tender information.
         * @param {Commerce.Peripherals.HardwareStation.ExtensionTransaction} [extensionTransactionProperties] \
         *   The transaction properties parameters for the command.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>} The async result.
         */
        fetchToken(tenderInfo: Commerce.Peripherals.HardwareStation.TenderInfo,
            extensionTransactionProperties: Commerce.Peripherals.HardwareStation.ExtensionTransaction, callerContext?: any):
            IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>;

        /**
         * Extensibility execute method.
         * @param {string} task The task to execute.
         * @param {Commerce.Peripherals.HardwareStation.ExtensionTransaction} [extensionTransactionProperties] \
         *   The transaction properties parameters for the command.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Commerce.Peripherals.HardwareStation.ExtensionTransaction>} The async result.
         */
        executeTask(task: string, extensionTransactionProperties: Commerce.Peripherals.HardwareStation.ExtensionTransaction, callerContext?: any):
            IAsyncResult<Commerce.Peripherals.HardwareStation.ExtensionTransaction>;

    }
}