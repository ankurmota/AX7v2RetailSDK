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
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Client;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        internal class SalesOrderManager : ISalesOrderManager
        {
            public Task<SalesOrder> Create(SalesOrder entity)
            {
                return Task.Run(() =>
                {
                    OrderManager.Create(CommerceRuntimeManager.Runtime).UploadOrder(entity);
                    return entity;
                });
            }
    
            public Task<SalesOrder> Read(string id)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetOrderByTransactionId(id));
            }
    
            public Task<PagedResult<SalesOrder>> ReadAll(QueryResultSettings queryResultSettings)
            {
                throw new NotSupportedException();
            }
    
            public Task<SalesOrder> Update(SalesOrder entity)
            {
                throw new NotSupportedException();
            }
    
            public Task Delete(SalesOrder entity)
            {
                throw new NotSupportedException();
            }
    
            public Task<PagedResult<SalesOrder>> Search(SalesOrderSearchCriteria salesOrderSearchCriteria, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).SearchOrders(queryResultSettings, salesOrderSearchCriteria));
            }
    
            public Task<PagedResult<Receipt>> GetReceipts(string id, ReceiptRetrievalCriteria receiptRetrievalCriteria, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetReceipts(id, receiptRetrievalCriteria, queryResultSettings));
            }
    
            public Task<PagedResult<SalesOrder>> GetByReceiptId(string receiptId, string orderStoreNumber, string orderTerminalId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetOrdersByReceiptId(queryResultSettings, receiptId, orderStoreNumber, orderTerminalId));
            }
    
            public Task<PagedResult<SalesInvoice>> GetInvoicesBySalesId(string salesId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).GetInvoicesBySalesId(salesId));
            }
    
            public Task CreatePickingList(string salesId)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).CreatePickingList(salesId));
            }
    
            public Task CreatePackingSlip(string salesId)
            {
                return Task.Run(() => OrderManager.Create(CommerceRuntimeManager.Runtime).CreatePackingSlip(salesId));
            }
        }
    }
}
