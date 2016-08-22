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
    export class StoreOperationsWebApi {
        private static proxy;

        public static GetProxy() {
            this.proxy = new AjaxProxy(msaxValues.msax_RetailOperationsWebApiUrl + '/');
        }

        public static GetCountryRegionInfo(languageId: string, callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.CountryRegionInfo[]> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.CountryRegionInfo[]>(callerContext);

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            var data = {
                'languageId': languageId,
                'queryResultSettings': Core.getDefaultQueryResultSettings()
            };

            this.proxy.SubmitRequest(
                "GetCountryRegionInfo",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        public static GetStateProvinceInfo(countryCode: string, callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.StateProvinceInfo[]> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.StateProvinceInfo[]>(callerContext);

            var data = {
                'countryCode': countryCode,
                'queryResultSettings': Core.getDefaultQueryResultSettings()
            };

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "GetStateProvinceInfo",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        public static GetGiftCardBalance(giftCardNumber: string, callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.GiftCard> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.GiftCard>(callerContext);

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            var data = {
                "giftCardId": giftCardNumber
            };

            this.proxy.SubmitRequest(
                "GetGiftCardInformation",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        public static RetrieveCardPaymentAcceptResult(cardPaymentResultAccessCode: string, callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.CardPaymentAcceptResult> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.CardPaymentAcceptResult>(callerContext);

            var data = {
                "cardPaymentResultAccessCode": cardPaymentResultAccessCode
            };

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "RetrieveCardPaymentAcceptResult",
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