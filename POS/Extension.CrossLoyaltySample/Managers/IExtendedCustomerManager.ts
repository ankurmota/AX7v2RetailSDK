/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Commerce.Core.d.ts'/>

module Custom.Managers {
    "use strict";

    export interface IExtendedCustomerManager extends Commerce.Model.Managers.ICustomerManager {
        getCrossLoyaltyCardAction(LoyaltyCardNumber: string): Commerce.IAsyncResult<number>;
    }

    export class ExtendedCustomerManager extends Commerce.Model.Managers.RetailServer.CustomerManager implements IExtendedCustomerManager {
        private _context: Commerce.Proxy.CommerceContext;

        constructor(commerceContext: Commerce.Proxy.CommerceContext) {
            super(commerceContext);

            this._context = commerceContext;
        }

        /**
         * Gets the cross loyalty discount.
         * @param {string} cross loyalty card number.
         * @returns {IAsyncResult<string>} The async result.
         */
        public getCrossLoyaltyCardAction(loyaltyCardNumber: string): Commerce.IAsyncResult<number> {
            var request: Commerce.Proxy.Common.IDataServiceRequest = this._context.customers().getCrossLoyaltyCardDiscountAction(loyaltyCardNumber);
            return request.execute<number>();
        }
    }
} 