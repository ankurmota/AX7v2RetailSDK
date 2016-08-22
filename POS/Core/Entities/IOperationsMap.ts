/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Operations {
    "use strict";

    export import RetailOperation = Proxy.Entities.RetailOperation;

    /**
     * Provides a typed interface for an operation async result,
     * which can be canceled and/or provide data.
     */
    export interface IOperationResult extends ICancelableResult {
        data?: any;
    }

    /**
     * Provides a typed interface for operation options.
     */
    export interface IOperationOptions {
    }

    /**
     * Provides the handler execution function and returns whether it was canceled,
     * and optional data as the result of the operation.
     */
    export interface IOperationHandler {
        /**
         * Executes the operation.
         *
         * @param {IOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        execute(options: IOperationOptions): IAsyncResult<IOperationResult>;
    }

    /**
     * This interface enforces a typed validator for operation options data.
     */
    export interface IValidator<T> {
        /**
         * Optional function used to get the data to be validated from the given operation options.
         */
        dataAccessor?: (options: IOperationOptions) => T;

        /**
         * Validator functions that takes the optional data to be validated.
         */
        validatorFunctions: Array<(data?: T) => Model.Entities.Error[]>;
    }

    /**
     * This interface enforces a type on the requirements for an operation implementation.
     * The id and handler properties are always needed when constructing new objects.
     */
    export interface IOperation {
        /**
         * The operation identifier.
         */
        id: Model.Entities.RetailOperation;

        /**
         * The optional pre handler is executed asynchronously and when it is done, if it was not canceled,
         * the handler is executed right after.
         * It takes the operation arguments as input.
         */
        preHandler?: <T extends ICancelableResult>(options: IOperationOptions) => IAsyncResult<T>;

        /**
         * The handler containing the execute function that handles the operation specified by identifier.
         */
        handler: IOperationHandler;

        /**
         * The optional post handler is executed after the handler is done executing.
         * It takes the handler result, if any, as input.
         */
        postHandler?: (handlerResult: any) => IVoidAsyncResult;

        /**
         * Collection of validators applied to the arguments of the handler.
         */
        validators?: IValidator<any>[];
    }
}