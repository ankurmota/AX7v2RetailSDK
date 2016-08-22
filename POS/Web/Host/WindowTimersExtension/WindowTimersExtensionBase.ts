/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../../SharedApp/Commerce.Core.d.ts'/>

module Commerce.Host.CrossBrowser {
    "use strict";

    /**
     * Base abstract class for WindowTimersExtension interface implementation.
     */
    export class WindowTimersExtensionBase implements WindowTimersExtension {

        private _nextHandleId: number = 1;
        private _handlers: Dictionary<Function> = new Dictionary<Function>();
        private _isHandlerRunning: boolean = false;

        /**
         * When implemented in child class, requests that a function be called when current or 
         * pending tasks are complete, such as events or screen updates.
         * @param {Function} [handler] The function to be called.
         * @param {any[]} [...args] Arguments to be passed to the function.
         * @return {number} A handle to the request.
         * @remarks This method is used to break up long running operations and run a callback function immediately.
         */
        public setImmediate(handler: Function, ...args: any[]): number {
            throw new Error("SetImmediateHandlerBase is an abstract class. No implementation exists.");
        }

        /**
         * Requests that a function be called when current or pending tasks are complete, such as events or screen updates.
         * @param {Function} [handler] The function to be called.
         * @param {any[]} [...args] Arguments to be passed to the function.
         * @return {number} A handle to the request.
         * @deprecated This method is deprecated and may be removed in future versions. Please use setImmediate() instead.
         */
        public msSetImmediate(handler: Function, ...args: any[]): number {
            return this.setImmediate(handler, args);
        }

        /**
         * Cancels a function request created with setImmediate.
         * @param {number} [handle] A handle to an immediate callback request, which is the value returned by setImmediate.
         */
        public clearImmediate(handle: number): void {
            this._handlers.removeItem(handle);
        }

        /**
         * Cancels a function request created with setImmediate.
         * @param {number} [handle] A handle to an immediate callback request, which is the value returned by setImmediate.
         * @deprecated This method is deprecated and may be removed in future versions. Please use clearImmediate() instead.
         */
        public msClearImmediate(handle: number): void {
            this.clearImmediate(handle);
        }

        /**
         * Adds a function request to the pending calls list.
         * @param {IArguments} [args] Function arguments.
         * @todo [v-olkuly] Change access modifier to 'protected' after upgrading TS to v.1.3+
         */
        public addHandler(args: IArguments): number {
            var handleId: number = this._nextHandleId++;
            this._handlers[handleId] = this.partialApply.apply(this, args);

            return handleId;
        }

        /**
         * Executes handler.
         * @param {number} [handle] A handle to an immediate callback request, which is the value returned by setImmediate.
         * @todo [v-olkuly] Change access modifier to protected after upgrading TS to v.1.3+
         */
        public executeHandler(handle: number): void {
            // If currently we are running a task, we will need to delay this invocation.
            if (this._isHandlerRunning) {
                var appliedHandler: () => void = this.partialApply(this.executeHandler, handle);

                window.setTimeout(appliedHandler, 0);
            } else {
                var handler: Function = this._handlers[handle];

                if (ObjectExtensions.isFunction(handler)) {
                    this._isHandlerRunning = true;

                    try {
                        handler();
                    } finally {
                        this.clearImmediate(handle);
                        this._isHandlerRunning = false;
                    }
                }
            }
        }

        /**
         * Performs partial application by converting function that accepts the same arguments as setImmediate, 
         * to the function that requires no arguments.
         * @param {Function} [handler] The function to be partially applied.
         * @param {any[]} [...args] Arguments to be passed to the function.
         * @todo [v-olkuly] Change access modifier to protected after upgrading TS to v.1.3+
         */
        public partialApply(handler: Function, ...args: any[]): () => void {
            if (ObjectExtensions.isNullOrUndefined(handler)) {
                throw new Error("handler is null or undefined");
            } else if (!ObjectExtensions.isFunction(handler)) {
                throw new Error("setImmediate(string) is not allowed due to security policy!");
            }

            return () => { handler.apply(this, args); };
        }
    }
}