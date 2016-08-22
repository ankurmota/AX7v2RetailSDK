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
    namespace Retail.Ecommerce.Web.Storefront.Controllers
    {
        using System.Collections.Generic;
        using System.Threading.Tasks;
        using System.Web.Mvc;
        using Commerce.RetailProxy;
        using Retail.Ecommerce.Sdk.Core;
        using Retail.Ecommerce.Sdk.Core.OperationsHandlers;

        /// <summary>
        /// Org Unit Controller.
        /// </summary>
        public class OrgUnitController : WebApiControllerBase
        {
            /// <summary>
            /// Gets the nearby stores with availability.
            /// </summary>
            /// <param name="latitude">The latitude.</param>
            /// <param name="longitude">The longitude.</param>
            /// <param name="searchRadius">The search radius.</param>
            /// <param name="itemUnits">The item units.</param>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>Response containing stores with product availability.</returns>
            public async Task<ActionResult> GetNearbyStoresWithAvailability(decimal latitude, decimal longitude, double searchRadius, IEnumerable<ItemUnit> itemUnits, QueryResultSettings queryResultSettings)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                OrgUnitOperationsHandler orgUnitOperationsHandler = new OrgUnitOperationsHandler(ecommerceContext);
                PagedResult<OrgUnitAvailability> orgUnitAvailabilities = await orgUnitOperationsHandler.GetNearbyStoresWithAvailability(latitude, longitude, searchRadius, itemUnits, queryResultSettings);

                return this.Json(orgUnitAvailabilities.Results);
            }

            /// <summary>
            /// Gets the nearby stores.
            /// </summary>
            /// <param name="latitude">The latitude.</param>
            /// <param name="longitude">The longitude.</param>
            /// <param name="distance">The distance.</param>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>Response containing nearby stores.</returns>
            public async Task<ActionResult> GetNearbyStores(decimal latitude, decimal longitude, int distance, QueryResultSettings queryResultSettings)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                OrgUnitOperationsHandler orgUnitOperationsHandler = new OrgUnitOperationsHandler(ecommerceContext);
                PagedResult<OrgUnitLocation> orgUnitLocations = await orgUnitOperationsHandler.GetNearbyStores(latitude, longitude, distance, queryResultSettings);

                return this.Json(orgUnitLocations.Results);
            }

            /// <summary>
            /// Gets the channel configuration.
            /// </summary>
            /// <returns>Response containing the channel configuration.</returns>
            public async Task<ActionResult> GetChannelConfiguration()
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                OrgUnitOperationsHandler orgUnitOperationsHandler = new OrgUnitOperationsHandler(ecommerceContext);
                ChannelConfiguration channelConfiguration = await orgUnitOperationsHandler.GetChannelConfiguration();

                return this.Json(channelConfiguration);
            }
            
            /// <summary>
            /// Gets the channel tender types.
            /// </summary>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>Tender types response.</returns>
            public async Task<ActionResult> GetChannelTenderTypes(QueryResultSettings queryResultSettings)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                OrgUnitOperationsHandler orgUnitOperationsHandler = new OrgUnitOperationsHandler(ecommerceContext);
                PagedResult<TenderType> tenderTypes = await orgUnitOperationsHandler.GetTenderTypes(queryResultSettings);

                return this.Json(tenderTypes.Results);
            }

            /// <summary>
            /// Gets the card types.
            /// </summary>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>A response containing collection of card types supported by channel..</returns>
            public async Task<ActionResult> GetCardTypes(QueryResultSettings queryResultSettings)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                OrgUnitOperationsHandler orgUnitOperationsHandler = new OrgUnitOperationsHandler(ecommerceContext);
                PagedResult<CardTypeInfo> cardTypeInfoCollection = await orgUnitOperationsHandler.GetCardTypes(queryResultSettings);

                return this.Json(cardTypeInfoCollection.Results);
            }

            /// <summary>
            /// Gets the delivery options information.
            /// </summary>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>Response containing delivery options available for the channel.</returns>
            public async Task<ActionResult> GetDeliveryOptionsInfo(QueryResultSettings queryResultSettings)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                OrgUnitOperationsHandler orgUnitOperationsHandler = new OrgUnitOperationsHandler(ecommerceContext);
                PagedResult<DeliveryOption> deliveryOptions = await orgUnitOperationsHandler.GetDeliveryOptionsInfo(queryResultSettings);

                return this.Json(deliveryOptions.Results);
            }
        }
    }
}