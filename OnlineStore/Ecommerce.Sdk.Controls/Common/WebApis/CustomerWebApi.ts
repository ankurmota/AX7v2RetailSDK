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
    export class CustomerWebApi {
        private static proxy;

        public static GetProxy() {
            this.proxy = new AjaxProxy(msaxValues.msax_CustomerWebApiUrl + '/');
        }

        public static GetCustomer(callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.Customer> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.Customer>();

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "GetCustomer",
                null,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        public static IsAuthenticatedSession(): CommerceProxy.IAsyncResult<boolean> {
            var asyncResult = new CommerceProxy.AsyncResult<boolean>();

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "IsAuthenticatedSession",
                null,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        public static GetOrderHistory(orderCountToBeSkipped: number, orderCountToBeRetrieved: number, callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.SalesOrder[]> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.SalesOrder[]>(callerContext);

            var data = {
                "queryResultSettings": Core.getQueryResultSettings(orderCountToBeSkipped, orderCountToBeRetrieved)
            };

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "GetOrderHistory",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        public static GetLoyaltyCards(callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.LoyaltyCard[]> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.LoyaltyCard[]>(callerContext);

            var data = {
                "queryResultSettings": Core.getDefaultQueryResultSettings()
            }

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "GetLoyaltyCards",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }
    }
}
