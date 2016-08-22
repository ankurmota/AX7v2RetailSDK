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
    namespace Retail.Ecommerce.Sdk.Core.OperationsHandlers
    {
        using System;
        using System.Collections.Generic;
        using System.Threading.Tasks;
        using Commerce.RetailProxy;

        /// <summary>
        /// Handler for org unit operations.
        /// </summary>
        public class OrgUnitOperationsHandler : OperationsHandlerBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="OrgUnitOperationsHandler"/> class.
            /// </summary>
            /// <param name="ecommerceContext">The ecommerce context.</param>
            public OrgUnitOperationsHandler(EcommerceContext ecommerceContext) : base(ecommerceContext)
            {
            }

            /// <summary>
            /// Get nearby stores.
            /// </summary>
            /// <param name="latitude">The latitude of the location to search for stores.</param>
            /// <param name="longitude">The longitude of the location to search for stores.</param>
            /// <param name="distance">Distance to search in miles.</param>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>Response instance.</returns>
            /// <exception cref="ArgumentException">Thrown when one of the input parameters is invalid.</exception>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async calls.")]
            public virtual async Task<PagedResult<OrgUnitLocation>> GetNearbyStores(decimal latitude, decimal longitude, int distance, QueryResultSettings queryResultSettings)
            {
                SearchArea searchArea = new SearchArea();
                searchArea.Latitude = latitude;
                searchArea.Longitude = longitude;
                searchArea.Radius = (distance == 0) ? 200 : distance; /* If the client does not specify the radius for search it is defaulted to 200 miles */

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                IOrgUnitManager orgUnitManager = managerFactory.GetManager<IOrgUnitManager>();

                PagedResult<OrgUnitLocation> orgUnitLocations =
                    await orgUnitManager.GetOrgUnitLocationsByArea(searchArea, queryResultSettings);

                return orgUnitLocations;
            }

            /// <summary>
            /// Get stores nearby with product availability.
            /// </summary>
            /// <param name="latitude">The latitude of the location to search for stores.</param>
            /// <param name="longitude">The longitude of the location to search for stores.</param>
            /// <param name="searchRadius">The search radius in miles.</param>
            /// <param name="itemUnits">The item units.</param>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>Response instance.</returns>
            /// <exception cref="ArgumentException">Thrown when one of the input parameters is invalid.</exception>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async calls.")]
            public virtual async Task<PagedResult<OrgUnitAvailability>> GetNearbyStoresWithAvailability(decimal latitude, decimal longitude, double searchRadius, IEnumerable<ItemUnit> itemUnits, QueryResultSettings queryResultSettings)
            {
                if (itemUnits == null)
                {
                    throw new ArgumentNullException(nameof(itemUnits));
                }

                SearchArea searchArea = new SearchArea();
                searchArea.Latitude = latitude;
                searchArea.Longitude = longitude;
                searchArea.Radius = (searchRadius > 0) ? searchRadius : 200;

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                IOrgUnitManager orgUnitManager = managerFactory.GetManager<IOrgUnitManager>();

                PagedResult<OrgUnitAvailability> orgUnitAvailabilities =
                    await orgUnitManager.GetAvailableInventoryNearby(itemUnits, searchArea, queryResultSettings);

                return orgUnitAvailabilities;
            }

            /// <summary>
            /// Gets the channel configuration.
            /// </summary>
            /// <returns>The channel configuration.</returns>
            public virtual async Task<ChannelConfiguration> GetChannelConfiguration()
            {
                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                IOrgUnitManager orgUnitManager = managerFactory.GetManager<IOrgUnitManager>();

                ChannelConfiguration channelConfiguration = await orgUnitManager.GetOrgUnitConfiguration();
                return channelConfiguration;
            }

            /// <summary>
            /// Gets channel tender types.
            /// </summary>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>Channel tender types.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async calls.")]
            public virtual async Task<PagedResult<TenderType>> GetTenderTypes(QueryResultSettings queryResultSettings)
            {
                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                IStoreOperationsManager storeOperationsManager = managerFactory.GetManager<IStoreOperationsManager>();
                PagedResult<TenderType> tenderTypes = await storeOperationsManager.GetTenderTypes(queryResultSettings);

                return tenderTypes;
            }

            /// <summary>
            /// Get the card types.
            /// </summary>
            /// <param name="queryResultSettings">The queryResultSettings.</param>
            /// <returns>A response containing collection of card type info.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async calls.")]
            public virtual async Task<PagedResult<CardTypeInfo>> GetCardTypes(QueryResultSettings queryResultSettings)
            {
                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                IStoreOperationsManager storeOperationsManager = managerFactory.GetManager<IStoreOperationsManager>();
                PagedResult<CardTypeInfo> cardTypeInfoCollection = await storeOperationsManager.GetCardTypes(queryResultSettings);
                return cardTypeInfoCollection;
            }

            /// <summary>
            /// Gets information for all the delivery modes that are supported for the legal entity.
            /// </summary>
            /// <param name="queryResultSettings">Accepts queryResultSettings.</param>
            /// <returns>The information for all the available delivery options.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async calls.")]
            public virtual async Task<PagedResult<DeliveryOption>> GetDeliveryOptionsInfo(QueryResultSettings queryResultSettings)
            {
                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                IStoreOperationsManager storeOperationsManager = managerFactory.GetManager<IStoreOperationsManager>();
                PagedResult<DeliveryOption> deliveryOptions = await storeOperationsManager.GetDeliveryOptions(queryResultSettings);
                return deliveryOptions;
            }

            /// <summary>
            /// Gets the navigation hierarchy categories.
            /// </summary>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>Navigation hierarchy categories.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async calls.")]
            public virtual async Task<PagedResult<Category>> GetNavigationalHierarchyCategories(QueryResultSettings queryResultSettings)
            {
                long channelId = await Utilities.GetChannelId(this.EcommerceContext);

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICategoryManager manager = managerFactory.GetManager<ICategoryManager>();
                PagedResult<Category> categories = await manager.GetCategories(channelId, queryResultSettings);

                return categories;
            }
        }
    }
}