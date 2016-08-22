/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

namespace Contoso
{
    namespace Commerce.RetailProxy.Adapters
    {
        using System;
        using System.Collections.ObjectModel;
        using System.Linq;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Client;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        internal class PurchaseOrderManager : IPurchaseOrderManager
        {
            public Task<PurchaseOrder> Create(PurchaseOrder entity)
            {
                throw new NotSupportedException();
            }
    
            public Task<PurchaseOrder> Read(string orderId)
            {
                return Task.Run(() => InventoryManager.Create(CommerceRuntimeManager.Runtime).GetPurchaseOrder(orderId).FirstOrDefault());
            }
    
            public Task<PagedResult<PurchaseOrder>> ReadAll(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => InventoryManager.Create(CommerceRuntimeManager.Runtime).GetPurchaseOrder(null));
            }
    
            public Task<PurchaseOrder> Update(PurchaseOrder entity)
            {
                return Task.Run(() =>
                {
                    InventoryManager.Create(CommerceRuntimeManager.Runtime).SavePickReceiveCount(false, entity, null);
                    return entity;
                });
            }
    
            public Task Delete(PurchaseOrder entity)
            {
                throw new NotSupportedException();
            }
    
            public Task Commit(string orderId)
            {
                return Task.Run(() => InventoryManager.Create(CommerceRuntimeManager.Runtime).CommitPurchaseOrder(orderId));
            }
        }
    }
}
