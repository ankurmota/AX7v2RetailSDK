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
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Client;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Runtime = Microsoft.Dynamics.Commerce.Runtime;
    
        internal class CommerceListManager : ICommerceListManager
        {
            public Task<CommerceList> Create(CommerceList entity)
            {
                return Task.Run(() => Runtime.Client.CommerceListManager.Create(CommerceRuntimeManager.Runtime).CreateCommerceList(entity));
            }
    
            public Task<CommerceList> Read(long id)
            {
                return Task.Run(() => Runtime.Client.CommerceListManager.Create(CommerceRuntimeManager.Runtime).GetCommerceList(id));
            }
    
            public Task<PagedResult<CommerceList>> ReadAll(QueryResultSettings queryResultSettings)
            {
                throw new NotSupportedException();
            }
    
            public Task<CommerceList> Update(CommerceList entity)
            {
                return Task.Run(() => Runtime.Client.CommerceListManager.Create(CommerceRuntimeManager.Runtime).UpdateCommerceListProperties(entity));
            }
    
            public Task Delete(CommerceList entity)
            {
                return Task.Run(() => Runtime.Client.CommerceListManager.Create(CommerceRuntimeManager.Runtime).DeleteCommerceList(entity.Id));
            }
    
            public Task<PagedResult<CommerceList>> GetByCustomer(string customerId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.CommerceListManager.Create(CommerceRuntimeManager.Runtime).GetCommerceListsByCustomer(customerId, favoriteFilter: false, publicFilter: false));
            }
    
            public Task<CommerceList> AddLines(long id, IEnumerable<CommerceListLine> commerceListLines)
            {
                return Task.Run(() => Runtime.Client.CommerceListManager.Create(CommerceRuntimeManager.Runtime).AddToCommerceList(id, commerceListLines));
            }
    
            public Task<CommerceList> UpdateLines(long id, IEnumerable<CommerceListLine> commerceListLines)
            {
                return Task.Run(() => Runtime.Client.CommerceListManager.Create(CommerceRuntimeManager.Runtime).UpdateCommerceListLines(id, commerceListLines));
            }
    
            public Task<CommerceList> RemoveLines(long id, IEnumerable<CommerceListLine> commerceListLines)
            {
                return Task.Run(() => Runtime.Client.CommerceListManager.Create(CommerceRuntimeManager.Runtime).RemoveFromCommerceList(id, commerceListLines));
            }
    
            public Task<CommerceList> MoveLines(IEnumerable<CommerceListLine> commerceListLines, long destinationId)
            {
                return Task.Run(() => Runtime.Client.CommerceListManager.Create(CommerceRuntimeManager.Runtime).MoveCommerceListLines(commerceListLines, destinationId));
            }
    
            public Task<CommerceList> CopyLines(IEnumerable<CommerceListLine> commerceListLines, long destinationId)
            {
                return Task.Run(() => Runtime.Client.CommerceListManager.Create(CommerceRuntimeManager.Runtime).CopyCommerceListLines(commerceListLines, destinationId));
            }
    
            public Task<CommerceList> AddContributors(long id, IEnumerable<CommerceListContributor> commerceListContributors)
            {
                return Task.Run(() => Runtime.Client.CommerceListManager.Create(CommerceRuntimeManager.Runtime).AddContributors(id, commerceListContributors));
            }
    
            public Task<CommerceList> RemoveContributors(long id, IEnumerable<CommerceListContributor> commerceListContributors)
            {
                return Task.Run(() => Runtime.Client.CommerceListManager.Create(CommerceRuntimeManager.Runtime).RemoveContributors(id, commerceListContributors));
            }
    
            public Task<CommerceList> CreateInvitations(long id, IEnumerable<CommerceListInvitation> commerceListInvitations)
            {
                return Task.Run(() => Runtime.Client.CommerceListManager.Create(CommerceRuntimeManager.Runtime).CreateInvitations(id, commerceListInvitations));
            }
    
            public Task AcceptInvitation(string invitationToken, string customerId)
            {
                return Task.Run(() => Runtime.Client.CommerceListManager.Create(CommerceRuntimeManager.Runtime).AcceptInvitation(invitationToken, customerId));
            }
        }
    }
}
