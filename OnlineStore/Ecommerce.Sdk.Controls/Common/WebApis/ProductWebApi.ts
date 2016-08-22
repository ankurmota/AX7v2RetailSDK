/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="../Helpers/Core.ts" />
/// <reference path="../Helpers/AsyncResult.ts" />

"use strict";

// Web services proxy
// Encapsulates ajax calls to the Ajax services.
module Contoso.Retail.Ecommerce.Sdk.Controls {
    export class ProductWebApi {

        private static proxy;

        public static GetProxy() {
            this.proxy = new AjaxProxy(msaxValues.msax_ProductWebApiUrl + '/');
        }

        public static GetSimpleProducts(productIds: number[], callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.SimpleProduct[]> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.SimpleProduct[]>(callerContext);

            var data = {
                'productIds': productIds,
            };

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "GetSimpleProducts",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return (asyncResult);
        }
    }
}