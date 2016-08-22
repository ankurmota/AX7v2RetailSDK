/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../DataJS.d.ts'/>
///<reference path='../IAsyncResult.ts'/>

module Commerce {
    "use strict";

    export class DataHelper {

        /**
         * Load JSON object asynchronously by URI.
         * @param {string} uri The URI.
         * @param {any} callerContext The caller context.
         * @returns {IAsyncResult<T>} The result.
         */
        public static loadJsonAsync<T>(uri: string): IAsyncResult<T> {
            var result: AsyncResult<T> = new AsyncResult<T>();

            $.getJSON(uri)
                .done((data: T) => {
                    result.resolve(data);
                }).fail((error: { message: string }) => {
                    RetailLogger.genericError(error.message);
                    result.reject([new Model.Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
                });

            return result;
        }

        /**
         * Load data asynchronously by URI as string content.
         * @param {string} uri The URI.
         * @param {any} callerContext The callback context.
         * @returns {IAsyncResult<string>} The result.
         */
        public static loadTextAsync(uri: string, callerContext?: any): IAsyncResult<string> {
            var result: AsyncResult<string> = new AsyncResult<string>(callerContext);

            $.ajax(uri)
                .done((data: string) => {
                    result.resolve(data);
                }).fail((error: { message: string }) => {
                    RetailLogger.genericError(error.message);
                    result.reject([new Model.Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
                });

            return result;
        }
    }
}
