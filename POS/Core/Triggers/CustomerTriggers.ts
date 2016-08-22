/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ITrigger.ts' />

module Commerce.Triggers {
    "use strict";

    /**
     * Provides the base type interface for customer trigger options.
     */
    export interface ICustomerTriggerOptions extends ITriggerOptions {
        cart: Model.Entities.Cart;
    }

    /**
     * Provides the type interface for the pre customer add trigger options.
     */
    export interface IPreCustomerAddTriggerOptions extends ICustomerTriggerOptions {
    }

    /**
     * Provides the type interface to be implemented by pre-triggers for the CustomerAdd operation.
     */
    export interface IPreCustomerAddTrigger extends ICancelableTrigger {
        execute(options: IPreCustomerAddTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the type interface for the post customer add trigger options.
     */
    export interface IPostCustomerAddTriggerOptions extends ICustomerTriggerOptions {
        customer: Model.Entities.Customer;
    }

    /**
     * Provides the type interface to be implemented by post-triggers for the CustomerAdd operation.
     */
    export interface IPostCustomerAddTrigger extends ITrigger {
        execute(options: IPostCustomerAddTriggerOptions): IVoidAsyncResult;
    }

    /**
     * Provides the type interface for the customer clear trigger options.
     */
    export interface ICustomerClearTriggerOptions extends ICustomerTriggerOptions {
    }

    /**
     * Provides the type interface to be implemented by pre-triggers for the CustomerClear operation.
     */
    export interface IPreCustomerClearTrigger extends ICancelableTrigger {
        execute(options: ICustomerClearTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the type interface to be implemented by post-triggers for the CustomerClear operation.
     */
    export interface IPostCustomerClearTrigger extends ITrigger {
        execute(options: ICustomerClearTriggerOptions): IVoidAsyncResult;
    }

    /**
     * Provides the type interface for the pre customer set trigger options.
     */
    export interface IPreCustomerSetTriggerOptions extends ICustomerTriggerOptions {
        customerId: string;
    }

    /**
     * Provides the type interface to be implemented by pre-triggers for the CustomerSet operation.
     */
    export interface IPreCustomerSetTrigger extends ICancelableTrigger {
        execute(options: IPreCustomerSetTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the type interface for the pre customer search trigger options.
     */
    export interface IPreCustomerSearchTriggerOptions extends ICustomerTriggerOptions {
        searchText: string;
    }

    /**
     * Provides the type interface to be implemented by pre-triggers for customer search.
     */
    export interface IPreCustomerSearchTrigger extends ICancelableTrigger {
        execute(options: IPreCustomerSearchTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the type interface for the post customer search trigger options.
     */
    export interface IPostCustomerSearchTriggerOptions extends ICustomerTriggerOptions {
        customers: Proxy.Entities.Customer[];
    }

    /**
     * Provides the type interface to be implemented by post-triggers for customer search.
     */
    export interface IPostCustomerSearchTrigger extends ITrigger {
        execute(options: IPostCustomerSearchTriggerOptions): IVoidAsyncResult;
    }
}