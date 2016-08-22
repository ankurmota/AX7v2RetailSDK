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
        using System.Linq;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Client;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Runtime = Microsoft.Dynamics.Commerce.Runtime;

        internal class OrgUnitManager : IOrgUnitManager
        {
            public Task<OrgUnit> Create(OrgUnit entity)
            {
                throw new NotSupportedException();
            }

            public Task<OrgUnit> Read(string orgUnitNumber)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetStoreByStoreNumber(orgUnitNumber));
            }

            public Task<PagedResult<OrgUnit>> ReadAll(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetAllStores(queryResultSettings));
            }

            public Task<OrgUnit> Update(OrgUnit entity)
            {
                throw new NotSupportedException();
            }

            public Task Delete(OrgUnit entity)
            {
                throw new NotSupportedException();
            }

            public Task<TillLayout> GetTillLayout()
            {
                return Task.Run(() => LayoutManager.Create(CommerceRuntimeManager.Runtime).GetTillLayout());
            }

            public Task<PagedResult<OrgUnit>> Search(SearchStoreCriteria storeSearchCriteria, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).SearchStores(storeSearchCriteria, queryResultSettings));
            }

            public Task<PagedResult<OrgUnitLocation>> GetOrgUnitLocationsByArea(SearchArea searchArea, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => StoreLocatorManager.Create(CommerceRuntimeManager.Runtime).GetStoreLocations(queryResultSettings, searchArea));
            }

            public Task<ChannelConfiguration> GetOrgUnitConfiguration()
            {
                return Task.Run(() => ChannelManager.Create(CommerceRuntimeManager.Runtime).GetChannelConfiguration());
            }

            public Task<PagedResult<OrgUnitAvailability>> GetAvailableInventory(string itemId, string variantId, string barcode, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => InventoryManager.Create(CommerceRuntimeManager.Runtime).GetAvailableInventory(itemId, variantId, barcode, queryResultSettings));
            }

            public Task<PagedResult<OrgUnitAvailability>> GetAvailableInventoryNearby(IEnumerable<ItemUnit> itemIds, SearchArea searchArea, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => InventoryManager.Create(CommerceRuntimeManager.Runtime).GetStoreAvailabilities(queryResultSettings, itemIds, searchArea));
            }

            public Task<PagedResult<TerminalInfo>> GetTerminalInfo(string orgUnitNumber, int deviceType, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ChannelManager.Create(CommerceRuntimeManager.Runtime).GetTerminalInfo(orgUnitNumber, deviceType, queryResultSettings));
            }
        }
    }
}