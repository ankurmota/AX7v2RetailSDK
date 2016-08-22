/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="../Helpers/Core.ts" />
/// <reference path="../Helpers/EcommerceTypes.ts" />

"use strict";

// Web services proxy
// Encapsulates ajax calls to the Ajax services.
module Contoso.Retail.Ecommerce.Sdk.Controls {

    export class OrgUnitWebApi {
        private static proxy: AjaxProxy;

        public static GetProxy() {
            this.proxy = new AjaxProxy(msaxValues.msax_OrgUnitWebApiUrl + '/');
        }

        public static GetChannelConfiguration(callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.ChannelConfiguration> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.ChannelConfiguration>(callerContext);

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "GetChannelConfiguration",
                null,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        // Calls the GetNearbyStoresWithAvailability method.
        public static GetNearbyStoresWithAvailability(latitude: number, longitude: number, distance: number, itemUnits: CommerceProxy.Entities.ItemUnit[], callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.OrgUnitAvailability[]> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.OrgUnitAvailability[]>(callerContext);

            var data = {
                "latitude": latitude,
                "longitude": longitude,
                "searchRadius": distance,
                "itemUnits": itemUnits,
                "queryResultSettings": Core.getDefaultQueryResultSettings()
            };

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "GetNearbyStoresWithAvailability",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        // Calls the GetNearbyStores method.
        public static GetNearbyStores(latitude: number, longitude: number, distance: number, callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.OrgUnitLocation[]> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.OrgUnitLocation[]>(callerContext);

            var data = {
                "latitude": latitude,
                "longitude": longitude,
                "distance": distance,
                "queryResultSettings": Core.getDefaultQueryResultSettings()
            };

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "GetNearbyStores",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        public static GetDeliveryOptionsInfo(callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.DeliveryOption[]> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.DeliveryOption[]>(callerContext);

            var data = {
                "queryResultSettings": Core.getDefaultQueryResultSettings()
            };

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "GetDeliveryOptionsInfo",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        public static GetTenderTypes(callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.TenderType[]> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.TenderType[]>(callerContext);

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            var data = {
                'queryResultSettings': Core.getDefaultQueryResultSettings()
            };

            this.proxy.SubmitRequest(
                "GetChannelTenderTypes",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        public static GetCardTypes(callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.CardTypeInfo[]> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.CardTypeInfo[]>(callerContext);

            var data = {
                "queryResultSettings": Core.getDefaultQueryResultSettings()
            };

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "GetCardTypes",
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