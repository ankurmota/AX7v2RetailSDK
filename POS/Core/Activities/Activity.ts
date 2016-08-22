/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Activities {
    "use strict";

    /**
     * Represents a null response.
     */
    export interface INullResponse {
    }

    /**
     * The context for the Activity class.
     */
    export interface IActivityContext {
    }

    /**
     * Represents an abstract activity.
     */
    export class Activity<TResponse> {
        /**
         * The activity response.
         */
        public response: TResponse;

        /**
         * The activity can handle the response, if a handler is provided.
         */
        public responseHandler: (response: TResponse) => IVoidAsyncResult;

        /**
         * The activity context.
         */
        public context: IActivityContext;

        /**
         * Initializes a new instance of the Activity class.
         * @param {IActivityContext} The activity context.
         */
        constructor(context: IActivityContext) {
            this.context = context || {};
        }

        /**
         * Function to be extended.
         * @return {IVoidAsyncResult} async result for when the activity is done executing or failed its execution.
         * @remarks The default implementation throws an exception.
         */
        public execute(): IVoidAsyncResult {
            throw "Activity execution not implemented.";
        }
    }
}