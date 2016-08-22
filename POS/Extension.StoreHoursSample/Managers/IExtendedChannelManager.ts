/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

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

    export interface IExtendedChannelManager extends Commerce.Model.Managers.IChannelManager {
        getStoreDayHoursAsync(storeId: string): Commerce.IAsyncResult<Commerce.Proxy.Entities.StoreDayHours[]>;
    }

    export class ExtendedChannelManager extends Commerce.Model.Managers.RetailServer.ChannelManager implements IExtendedChannelManager {
        private _context: Commerce.Proxy.CommerceContext;

        constructor(commerceContext: Commerce.Proxy.CommerceContext) {
            super(commerceContext);

            this._context = commerceContext;
        }

        public getStoreDayHoursAsync(storeId: string): Commerce.IAsyncResult<Commerce.Proxy.Entities.StoreDayHours[]> {
            Commerce.RetailLogger.extendedInformational("ChannelManager.getStoreDayHoursAsync()");

            var request: Commerce.Proxy.Common.IDataServiceRequest = this._context.storeHours().getStoreDaysByStore(storeId);
            return request.execute<Commerce.Proxy.Entities.StoreDayHours[]>();
        }
    }
} 