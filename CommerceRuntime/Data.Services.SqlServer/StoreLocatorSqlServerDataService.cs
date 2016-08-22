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
    namespace Commerce.Runtime.DataServices.SqlServer
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        ///  The SQL server data request handler for store locator.
        /// </summary>
        public class StoreLocatorSqlServerDataService : IRequestHandler
        {
            private const string GetNearbyStoresFunctionName = "GETNEARBYSTORESFUNCTION(@bi_ChannelId, @f_Latitude, @f_Longitude, @f_SearchDistance, @f_UnitConversion)";
            private const string OrgUnitContactsView = "ORGUNITCONTACTSVIEW";
            private const string OrgUnitAddressView = "ORGUNITADDRESSVIEW";
            private const string ChannelIdVariable = "@bi_channelId";
            private const string LongitudeVariable = "@f_Longitude";
            private const string LatitudeVariable = "@f_Latitude";
            private const string SearchDistanceVariable = "@f_SearchDistance";
            private const string UnitConversionVariable = "@f_UnitConversion";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[] 
                    {
                        typeof(GetStoresDataRequest),
                        typeof(GetOrgUnitContactsDataRequest),
                        typeof(GetOrgUnitAddressDataRequest)
                    };
                }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
    
                if (requestType == typeof(GetStoresDataRequest))
                {
                    response = this.GetStores((GetStoresDataRequest)request);
                }
                else if (requestType == typeof(GetOrgUnitContactsDataRequest))
                {
                    response = this.GetOrgUnitContacts((GetOrgUnitContactsDataRequest)request);
                }
                else if (requestType == typeof(GetOrgUnitAddressDataRequest))
                {
                    response = this.GetOrgUnitAddress((GetOrgUnitAddressDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Gets the collection of stores within the specified search area.
            /// </summary>
            /// <param name="request">The get stores data request.</param>
            /// <returns>
            /// A collection of stores.
            /// </returns>
            private EntityDataServiceResponse<OrgUnitLocation> GetStores(GetStoresDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.SearchArea, "request.SearchArea");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                QueryResultSettings settings = request.QueryResultSettings;
                SearchArea searchArea = request.SearchArea;
    
                if (settings.ColumnSet != null && settings.ColumnSet.Count > 0 && !settings.ColumnSet.Contains(OrgUnitLocation.DistanceColumn))
                {
                    settings.ColumnSet.Add(OrgUnitLocation.DistanceColumn);
                }
    
                var query = new SqlPagedQuery(settings)
                {
                    From = GetNearbyStoresFunctionName,
                    OrderBy = searchArea.IsUnbounded ? "NAME ASC" : "DISTANCE ASC"
                };
    
                query.Parameters[ChannelIdVariable] = request.ChannelId;
                query.Parameters[LongitudeVariable] = searchArea.Longitude;
                query.Parameters[LatitudeVariable] = searchArea.Latitude;
                query.Parameters[SearchDistanceVariable] = searchArea.Radius;
                query.Parameters[UnitConversionVariable] = searchArea.GetUnitConversion();
    
                PagedResult<OrgUnitLocation> storeLocationsRecords;
    
                using (var sqlServerdatabaseContext = new SqlServerDatabaseContext(request))
                {
                    storeLocationsRecords = sqlServerdatabaseContext.ReadEntity<OrgUnitLocation>(query);
                }
    
                storeLocationsRecords = this.ParseLocations(storeLocationsRecords.Results, request.RequestContext);
    
                return new EntityDataServiceResponse<OrgUnitLocation>(storeLocationsRecords);
            }

            private EntityDataServiceResponse<OrgUnitContact> GetOrgUnitContacts(GetOrgUnitContactsDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.ChannelIds, "request.ChannelIds");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = OrgUnitContactsView,
                    OrderBy = "CHANNELID"
                };

                PagedResult<OrgUnitContact> results;
                IEnumerable<string> distinctChannelIds = request.ChannelIds.Distinct<long>().Select<long, string>(id => id.ToString());
                using (StringIdTableType channelIdsTable = new StringIdTableType(distinctChannelIds, "CHANNELID"))
                {
                    query.Parameters["@TVP_CHANNELID"] = channelIdsTable;
                    using (var sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                    {
                        results = sqlServerDatabaseContext.ReadEntity<OrgUnitContact>(query);
                    }
                }

                return new EntityDataServiceResponse<OrgUnitContact>(results);
            }

            private EntityDataServiceResponse<OrgUnitAddress> GetOrgUnitAddress(GetOrgUnitAddressDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.ChannelIds, "request.ChannelIds");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = OrgUnitAddressView,
                    OrderBy = "CHANNELID"
                };

                PagedResult<OrgUnitAddress> results;
                IEnumerable<string> distinctChannelIds = request.ChannelIds.Distinct<long>().Select<long, string>(id => id.ToString());
                using (StringIdTableType channelIdsTable = new StringIdTableType(distinctChannelIds, "CHANNELID"))
                {
                    query.Parameters["@TVP_CHANNELID"] = channelIdsTable;
                    using (var sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                    {
                        results = sqlServerDatabaseContext.ReadEntity<OrgUnitAddress>(query);
                    }
                }

                return new EntityDataServiceResponse<OrgUnitAddress>(results);
            }

            /// <summary>
            /// Parse list of locations and extract store location and contact information.
            /// </summary>
            /// <param name="storeLocationsRecords">Parse collection of the <see cref="OrgUnitLocation"/>.</param>
            /// <param name="context">The request context.</param>
            /// <returns>Collection of the <see cref="OrgUnitLocation"/>.</returns>
            private PagedResult<OrgUnitLocation> ParseLocations(IEnumerable<OrgUnitLocation> storeLocationsRecords, RequestContext context)
            {
                Dictionary<long, OrgUnitLocation> storeLocationResults = new Dictionary<long, OrgUnitLocation>();
    
                if (storeLocationsRecords != null && storeLocationsRecords.Any())
                {
                    // go over list of locations and extract store location and contact information.
                    foreach (OrgUnitLocation currentStoreLocation in storeLocationsRecords)
                    {
                        if (!storeLocationResults.ContainsKey(currentStoreLocation.PostalAddressId))
                        {
                            storeLocationResults.Add(currentStoreLocation.PostalAddressId, currentStoreLocation);
                        }
                    }

                    IEnumerable<OrgUnitLocation> result = storeLocationResults.Values;
                    var channelIds = result.Select<OrgUnitLocation, long>(orgUnitLocation => orgUnitLocation.ChannelId);
                    var getOrgUnitContactsDataRequest = new GetOrgUnitContactsDataRequest(channelIds);
                    IEnumerable<OrgUnitContact> contacts = context.Execute<EntityDataServiceResponse<OrgUnitContact>>(getOrgUnitContactsDataRequest).PagedEntityCollection.Results;

                    // map the OrgUnitContacts to OrgUnitLocation
                    result = result.GroupJoin<OrgUnitLocation, OrgUnitContact, long, OrgUnitLocation>(
                        contacts,
                        orgUnitLocation => orgUnitLocation.ChannelId,
                        orgUnitContact => orgUnitContact.ChannelId,
                        (orgUnitLocation, orgUnitContacts) =>
                        {
                            foreach (OrgUnitContact storeContact in orgUnitContacts)
                            {
                                if (storeContact.Locator != null || storeContact.Description != null)
                                {
                                    orgUnitLocation.Contacts.Add(storeContact);
                                }
                            }

                            return orgUnitLocation;
                        });
                }
    
                return storeLocationResults.Values.ToArray().AsPagedResult();
            }
        }
    }
}
