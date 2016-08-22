/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */




module Commerce.Triggers {
    "use strict";

    /**
     * Interface that trigger types must implement.
     * Note: this was implemented as an interface rather than a base class because in TypeScript static properties 
     * are not considered in assignment compatibility. Thus, the implementing classes need to have different field names
     * in order for the correct overload signature to be choosen when calling TriggerManager::execute.
     */
    export interface ITriggerType {
        toString(): string;
    }

    /**
     * Class containing the types for non-cancelable triggers.
     */
    export class NonCancelableTriggerType implements ITriggerType {
        private _value: string;

        constructor(value: string) {
            this._value = value;
            Object.freeze(this);
        }

        public toString(): string {
            return this._value;
        }

        /*      Application Triggers        */
        public static ApplicationStart: NonCancelableTriggerType = new NonCancelableTriggerType("ApplicationStart");
        public static ApplicationSuspend: NonCancelableTriggerType = new NonCancelableTriggerType("ApplicationSuspend");
        public static PostLogOff: NonCancelableTriggerType = new NonCancelableTriggerType("PostLogOff");
        public static PostLogOn: NonCancelableTriggerType = new NonCancelableTriggerType("PostLogOn");

        /*      Cash Management Triggers    */
        public static PostTenderDeclaration: NonCancelableTriggerType = new NonCancelableTriggerType("PostTenderDeclaration");

        /*      Customer Triggers           */
        public static PostCustomerAdd: NonCancelableTriggerType = new NonCancelableTriggerType("PostCustomerAdd");
        public static PostCustomerClear: NonCancelableTriggerType = new NonCancelableTriggerType("PostCustomerClear");
        public static PostCustomerSearch: NonCancelableTriggerType = new NonCancelableTriggerType("PostCustomerSearch");

        /*      Discount Triggers           */
        public static PostLineDiscountAmount: NonCancelableTriggerType = new NonCancelableTriggerType("PostLineDiscountAmount");
        public static PostLineDiscountPercent: NonCancelableTriggerType = new NonCancelableTriggerType("PostLineDiscountPercent");
        public static PostTotalDiscountAmount: NonCancelableTriggerType = new NonCancelableTriggerType("PostTotalDiscountAmount");
        public static PostTotalDiscountPercent: NonCancelableTriggerType = new NonCancelableTriggerType("PostTotalDiscountPercent");

        /*      Operation Triggers          */
        public static OperationFailure: NonCancelableTriggerType = new NonCancelableTriggerType("OperationFailure");
        public static PostOperation: NonCancelableTriggerType = new NonCancelableTriggerType("PostOperation");

        /*      Payment Triggers            */
        public static PostPayment: NonCancelableTriggerType = new NonCancelableTriggerType("PostPayment");
        public static PostVoidPayment: NonCancelableTriggerType = new NonCancelableTriggerType("PostVoidPayment");

        /*      Product Triggers            */
        public static PostClearQuantity: NonCancelableTriggerType = new NonCancelableTriggerType("PostClearQuantity");
        public static PostPriceOverride: NonCancelableTriggerType = new NonCancelableTriggerType("PostPriceOverride");
        public static PostProductSale: NonCancelableTriggerType = new NonCancelableTriggerType("PostProductSale");
        public static PostReturnProduct: NonCancelableTriggerType = new NonCancelableTriggerType("PostReturnProduct");
        public static PostSetQuantity: NonCancelableTriggerType = new NonCancelableTriggerType("PostSetQuantity");
        public static PostVoidProducts: NonCancelableTriggerType = new NonCancelableTriggerType("PostVoidProducts");

        /*      Transaction Triggers        */
        public static BeginTransaction: NonCancelableTriggerType = new NonCancelableTriggerType("BeginTransaction");
        public static PostEndTransaction: NonCancelableTriggerType = new NonCancelableTriggerType("PostEndTransaction");
        public static PostRecallTransaction: NonCancelableTriggerType = new NonCancelableTriggerType("PostRecallTransaction");
        public static PostReturnTransaction: NonCancelableTriggerType = new NonCancelableTriggerType("PostReturnTransaction");
        public static PostSuspendTransaction: NonCancelableTriggerType = new NonCancelableTriggerType("PostSuspendTransaction");
        public static PostVoidTransaction: NonCancelableTriggerType = new NonCancelableTriggerType("PostVoidTransaction");
    }

    /**
     * Class containing the types for cancelable triggers.
     */
    export class CancelableTriggerType implements ITriggerType {
        private value: string;

        constructor(value: string) {
            this.value = value;
            Object.freeze(this);
        }

        public toString(): string {
            return this.value;
        }

        /*      Application Triggers        */
        public static PreLogOn: CancelableTriggerType = new CancelableTriggerType("PreLogOn");

        /*      Cash Management Triggers    */
        public static PreTenderDeclaration: CancelableTriggerType = new CancelableTriggerType("PreTenderDeclaration");

        /*      Customer Triggers           */
        public static PreCustomerAdd: CancelableTriggerType = new CancelableTriggerType("PreCustomerAdd");
        public static PreCustomerClear: CancelableTriggerType = new CancelableTriggerType("PreCustomerClear");
        public static PreCustomerSearch: CancelableTriggerType = new CancelableTriggerType("PreCustomerSearch");
        public static PreCustomerSet: CancelableTriggerType = new CancelableTriggerType("PreCustomerSet");

        /*      Discount Triggers           */
        public static PreLineDiscountAmount: CancelableTriggerType = new CancelableTriggerType("PreLineDiscountAmount");
        public static PreLineDiscountPercent: CancelableTriggerType = new CancelableTriggerType("PreLineDiscountPercent");
        public static PreTotalDiscountAmount: CancelableTriggerType = new CancelableTriggerType("PreTotalDiscountAmount");
        public static PreTotalDiscountPercent: CancelableTriggerType = new CancelableTriggerType("PreTotalDiscountPercent");

        /*      Operation Triggers          */
        public static PreOperation: CancelableTriggerType = new CancelableTriggerType("PreOperation");

        /*      Payment Triggers            */
        public static PreAddTenderLine: CancelableTriggerType = new CancelableTriggerType("PreAddTenderLine");
        public static PrePayment: CancelableTriggerType = new CancelableTriggerType("PrePayment");
        public static PreVoidPayment: CancelableTriggerType = new CancelableTriggerType("PreVoidPayment");

        /*      Printing Triggers           */
        public static PrePrintReceiptCopy: CancelableTriggerType = new CancelableTriggerType("PrePrintReceiptCopy");

        /*      Product Triggers            */
        public static PreClearQuantity: CancelableTriggerType = new CancelableTriggerType("PreClearQuantity");
        public static PrePriceOverride: CancelableTriggerType = new CancelableTriggerType("PrePriceOverride");
        public static PreProductSale: CancelableTriggerType = new CancelableTriggerType("PreProductSale");
        public static PreReturnProduct: CancelableTriggerType = new CancelableTriggerType("PreReturnProduct");
        public static PreSetQuantity: CancelableTriggerType = new CancelableTriggerType("PreSetQuantity");
        public static PreVoidProducts: CancelableTriggerType = new CancelableTriggerType("PreVoidProducts");

        /*      Transaction Triggers        */
        public static PreConfirmReturnTransaction: CancelableTriggerType = new CancelableTriggerType("PreConfirmReturnTransaction");
        public static PreEndTransaction: CancelableTriggerType = new CancelableTriggerType("PreEndTransaction");
        public static PreRecallTransaction: CancelableTriggerType = new CancelableTriggerType("PreRecallTransaction");
        public static PreReturnTransaction: CancelableTriggerType = new CancelableTriggerType("PreReturnTransaction");
        public static PreSuspendTransaction: CancelableTriggerType = new CancelableTriggerType("PreSuspendTransaction");
        public static PreVoidTransaction: CancelableTriggerType = new CancelableTriggerType("PreVoidTransaction");
    }
}


