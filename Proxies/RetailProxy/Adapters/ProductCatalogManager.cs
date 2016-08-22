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
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Client;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Runtime = Microsoft.Dynamics.Commerce.Runtime;
    
        internal class ProductCatalogManager : IProductCatalogManager
        {
            public Task<ProductCatalog> Create(ProductCatalog entity)
            {
                throw new NotSupportedException();
            }
    
            public Task<ProductCatalog> Read(long recordId)
            {
                throw new NotSupportedException();
            }
    
            public Task<PagedResult<ProductCatalog>> ReadAll(QueryResultSettings queryResultSettings)
            {
                throw new NotSupportedException();
            }
    
            public Task<ProductCatalog> Update(ProductCatalog entity)
            {
                throw new NotSupportedException();
            }
    
            public Task Delete(ProductCatalog entity)
            {
                throw new NotSupportedException();
            }
    
            public Task<PagedResult<ProductCatalog>> GetCatalogs(long channelId, bool activeOnly, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetProductCatalogs(channelId, activeOnly, queryResultSettings));
            }
        }
    }
}
