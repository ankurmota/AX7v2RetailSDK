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

    export interface IPaymentTerminal {

        /**
         * Lock token.
         */
        lockToken: string;

        /**
         * Check if the device is available.
         */
        isActive: boolean;

        /**
         * Begins a transaction on the payment terminal device.
         *
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        beginTransaction(callerContext?: any): IVoidAsyncResult;

        /**
         * Ends a transaction on the payment terminal device.
         *
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        endTransaction(callerContext?: any): IVoidAsyncResult;

        /**
         * Display transaction on the payment terminal device.
         *
         * @param {Model.Entities.Cart} cart The cart to display.
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        displayTransaction(cart: Model.Entities.Cart, callerContext?: any): IVoidAsyncResult;

        /**
         * Gets the tender information from from payment terminal.
         *
         * @param {number} amount The amount to request.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<CardInfo>} The async result.
         */
        getTender(amount: number, callerContext?: any): IAsyncResult<Model.Entities.CardInfo>;

        /**
         * Gets the tender information from from payment terminal.
         *
         * @param {TenderLine} tenderLine The tender line to request.
         * @param {merchantProperties} merchantProperties The merchantProperties to make the payment request.
         * @param {paymentConnectorName} paymentConnectorName The connector name to make the payment request.
         * @param {paymentType} paymentType The paymentType to make the payment request.
         * @param {Commerce.Model.Entities.CardType} cardTypeValue The card type.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<TenderInfo>} The async result.
         */
        makePayment(tenderLine: Model.Entities.TenderLine, merchantProperties: string, paymentConnectorName: string, paymentType: Commerce.Peripherals.HardwareStation.PaymentType, cardTypeValue: Commerce.Model.Entities.CardType, callerContext?: any): IAsyncResult<Model.Entities.TenderLine>;

        /**
         * Get the tender approval from the payment terminal device.
         *
         * @param {number} amount The amount to request.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Model.Entities.CardInfo>} The async result.
         */
        getTenderApproval(amount: number, callerContext?: any): IAsyncResult<Model.Entities.CardInfo>;

        /**
         * Authorizes payment and gets the authorized payment information from the payment terminal device.
         *
         * @param {number} amount The amount to get authorization for.
         * @param {string} voiceAuthorization The voice authorization code.
         * @param {string} isManualEntry Set to true if card number is entered manually in the device (point device). It is not related to manual entry of card number in Payment view.
         * @param {Commerce.Peripherals.HardwareStation.ExtensionTransaction} [extensionTransactionProperties] The transaction properties parameters for the command.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>} The async result.
         */
        authorizePayment(amount: number, voiceAuthorization: string, isManualEntry: boolean,
            extensionTransactionProperties: Commerce.Peripherals.HardwareStation.ExtensionTransaction, callerContext?: any): IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>;

        /**
         * Voids the payment using the payment terminal device.
         *
         * @param {number} amount The amount to void payment for.
         * @param {string} paymentPropertiesXml The payment properties blob.
         * @param {Commerce.Peripherals.HardwareStation.ExtensionTransaction} [extensionTransactionProperties] The transaction properties parameters for the command.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>} The async result.
         */
        voidPayment(amount: number, paymentPropertiesXml: string,
            extensionTransactionProperties: Commerce.Peripherals.HardwareStation.ExtensionTransaction, callerContext?: any): IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>;

        /**
         * Captures the payment using the payment terminal device.
         *
         * @param {number} amount The amount to capture payment for.
         * @param {string} paymentPropertiesXml The payment properties blob.
         * @param {Commerce.Peripherals.HardwareStation.ExtensionTransaction} [extensionTransactionProperties] The transaction properties parameters for the command.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>} The async result.
         */
        capturePayment(amount: number, paymentPropertiesXml: string,
            extensionTransactionProperties: Commerce.Peripherals.HardwareStation.ExtensionTransaction, callerContext?: any): IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>;

        /**
         * Refunds the payment using the payment terminal device.
         *
         * @param {number} amount The amount to get refund for.
         * @param {string} isManualEntry Set to true if card number is entered manually in the device (point device). It is not related to manual entry of card number in Payment view.
         * @param {Commerce.Peripherals.HardwareStation.ExtensionTransaction} [extensionTransactionProperties] The transaction properties parameters for the command.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>} The async result.
         */
        refundPayment(amount: number, isManualEntry: boolean,
            extensionTransactionProperties: Commerce.Peripherals.HardwareStation.ExtensionTransaction, callerContext?: any): IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>;

        /**
         * Fetches card token using the payment terminal device.
         *
         * @param {boolean} isManualEntry The value indicating whether the card number is entered manually on the device. 
         * @param {PeripheralConfiguration[]} [extensionTransactionProperties] The transaction properties parameters for the command.
         * @param {any} [callerContext] The callback context.
         * @return {IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>} The async result.
         */
        fetchToken(isManualEntry: boolean,
            extensionTransactionProperties: Commerce.Peripherals.HardwareStation.PeripheralConfiguration[], callerContext?: any): IAsyncResult<Commerce.Peripherals.HardwareStation.PaymentInfo>;

        /**
         * Cancel the current operation.
         *
         * @param {any} [callerContext] The callback context.
         * @return {IVoidAsyncResult} The async result.
         */
        cancelOperation(callerContext?: any): IVoidAsyncResult;
    }
}