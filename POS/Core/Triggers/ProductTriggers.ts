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
     * Provides the base type interface for product trigger options.
     */
    export interface IProductTriggerOptions extends ITriggerOptions {
        cart: Model.Entities.Cart;
    }

    /**
     * Provides the type interface for pre product sale trigger options.
     */
    export interface IPreProductSaleTriggerOptions extends IProductTriggerOptions {
        productSaleDetails: Model.Entities.ProductSaleReturnDetails[];
    }

    /**
     * Provides the type interface to be implemented by pre-triggers for the item sale operation.
     */
    export interface IPreProductSaleTrigger extends ICancelableTrigger {
        execute(options: IPreProductSaleTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the type interface for post product sale trigger options.
     */
    export interface IPostProductSaleTriggerOptions extends IProductTriggerOptions {
        productSaleDetails: Model.Entities.ProductSaleReturnDetails[];
    }

    /**
     * Provides the type interface to be implemented by post-triggers for the item sale operation.
     */
    export interface IPostProductSaleTrigger extends ITrigger {
        execute(options: IPostProductSaleTriggerOptions): IVoidAsyncResult;
    }

    /**
     * Provides the type interface for pre return product trigger options.
     */
    export interface IPreReturnProductTriggerOptions extends IProductTriggerOptions {
        cartLinesForReturn: Model.Entities.CartLine[];
    }

    /**
     * Provides the type interface to be implemented by pre-triggers for the return product operation.
     */
    export interface IPreReturnProductTrigger extends ICancelableTrigger {
        execute(options: IPreReturnProductTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the type interface for post return product trigger options.
     */
    export interface IPostReturnProductTriggerOptions extends IProductTriggerOptions {
        cartLinesForReturn: Model.Entities.CartLine[];
    }

    /**
     * Provides the type interface to be implemented by post-triggers for return product operation. 
     */
    export interface IPostReturnProductTrigger extends ITrigger {
        execute(options: IPostReturnProductTriggerOptions): IVoidAsyncResult;
    }

    /**
     * Provides the type interface for pre set quantity trigger options.
     */
    export interface IPreSetQuantityTriggerOptions extends IProductTriggerOptions {
        operationOptions: Operations.ISetQuantityOperationOptions;
    }

    /**
     * Provides the type interface to be implemented by pre-triggers for set quantity.
     */
    export interface IPreSetQuantityTrigger extends ICancelableTrigger {
        execute(options: IPreSetQuantityTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the type interface for post set quantity trigger options.
     */
    export interface IPostSetQuantityTriggerOptions extends IProductTriggerOptions {
        cartLines: Model.Entities.CartLine[];
    }

    /**
     * Provides the type interface to be implemented by post-triggers for set quantity.
     */
    export interface IPostSetQuantityTrigger extends ITrigger {
        execute(options: IPostSetQuantityTriggerOptions): IVoidAsyncResult;
    }

    /**
     * Provides the type interface for pre price override trigger options.
     */
    export interface IPrePriceOverrideTriggerOptions extends IProductTriggerOptions {
        operationOptions: Operations.IPriceOverrideOperationOptions;
    }

    /**
     * Provides the type interface to be implemented by pre-triggers for price override.
     */
    export interface IPrePriceOverrideTrigger extends ICancelableTrigger {
        execute(options: IPrePriceOverrideTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the type interface for post price override trigger options.
     */
    export interface IPostPriceOverrideTriggerOptions extends IProductTriggerOptions {
        cartLines: Model.Entities.CartLine[];
    }

    /**
     * Provides the type interface to be implemented by post-triggers for price override operation. 
     */
    export interface IPostPriceOverrideTrigger extends ITrigger {
        execute(options: IPostPriceOverrideTriggerOptions): IVoidAsyncResult;
    }

    /**
     * Provides the type interface for pre clear quantity trigger options.
     */
    export interface IPreClearQuantityTriggerOptions extends IProductTriggerOptions {
        cartLines: Proxy.Entities.CartLine[];
    }

    /**
     * Provides the type interface to be implemented by pre-triggers for clear quantity.
     */
    export interface IPreClearQuantityTrigger extends ICancelableTrigger {
        execute(options: IPreClearQuantityTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the type interface for post clear quantity trigger options.
     */
    export interface IPostClearQuantityTriggerOptions extends IProductTriggerOptions {
        cartLines: Model.Entities.CartLine[];
    }

    /**
     * Provides the type interface to be implemented by post-triggers for clear quantity operation. 
     */
    export interface IPostClearQuantityTrigger extends ITrigger {
        execute(options: IPostClearQuantityTriggerOptions): IVoidAsyncResult;
    }

    /**
     * Provides the type interface for pre void product trigger options.
     */
    export interface IPreVoidProductsTriggerOptions extends IProductTriggerOptions {
        cartLines: Proxy.Entities.CartLine[];
    }

    /**
     * Provides the type interface to be implemented by pre-triggers for void products.
     */
    export interface IPreVoidProductsTrigger extends ICancelableTrigger {
        execute(options: IPreVoidProductsTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the type interface for post void product trigger options.
     */
    export interface IPostVoidProductsTriggerOptions extends IProductTriggerOptions {
        cartLines: Model.Entities.CartLine[];
    }

    /**
     * Provides the type interface to be implemented by post-triggers for void products operation. 
     */
    export interface IPostVoidProductsTrigger extends ITrigger {
        execute(options: IPostVoidProductsTriggerOptions): IVoidAsyncResult;
    }
} 