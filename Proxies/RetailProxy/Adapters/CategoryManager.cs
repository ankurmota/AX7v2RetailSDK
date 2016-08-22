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
    
        internal class CategoryManager : ICategoryManager
        {
            public Task<Category> Create(Category entity)
            {
                throw new NotSupportedException();
            }
    
            public Task<Category> Read(long recordId)
            {
                throw new NotSupportedException();
            }
    
            public Task<PagedResult<Category>> ReadAll(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetChannelCategoryHierarchy(queryResultSettings));
            }
    
            public Task<Category> Update(Category entity)
            {
                throw new NotSupportedException();
            }
    
            public Task Delete(Category entity)
            {
                throw new NotSupportedException();
            }
    
            public Task<PagedResult<Category>> GetCategories(long channelId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetChannelCategoryHierarchy(channelId, queryResultSettings));
            }
    
            public Task<PagedResult<Category>> GetChildren(long channelId, long categoryId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetDirectChildCategories(channelId, categoryId, queryResultSettings));
            }
        }
    }
}
