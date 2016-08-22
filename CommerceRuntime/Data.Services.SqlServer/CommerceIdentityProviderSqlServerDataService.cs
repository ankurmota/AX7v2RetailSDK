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
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Handles the request related to registered commerce identity providers.
        /// </summary>
        public class CommerceIdentityProviderSqlServerDataService : IRequestHandler
        {
            // Stored procedure names
            private const string GetIdentityProviderSprocName = "GETIDENTITYPROVIDER";
            private const string GetRelyingPartiesByIdentityProviderSprocName = "GETRELYINGPARTIESBYIDENTITYPROVIDER";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new Type[]
                    {
                        typeof(GetCommerceIdentityProviderByIssuerDataRequest),
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
    
                Type requestedType = request.GetType();
                Response response;
    
                if (requestedType == typeof(GetCommerceIdentityProviderByIssuerDataRequest))
                {
                    response = this.GetCommerceIdentityProviderByIssuer((GetCommerceIdentityProviderByIssuerDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Gets the customer account by external identity.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<CommerceIdentityProvider> GetCommerceIdentityProviderByIssuer(GetCommerceIdentityProviderByIssuerDataRequest request)
            {
                CommerceIdentityProviderL2CacheDataStoreAccessor levelL2CacheDataAccessor = this.GetCommerceIdentityProviderL2CacheDataStoreAccessor(request.RequestContext);
                bool foundInCache;
                bool updateCache;
    
                CommerceIdentityProvider commerceIdentityProvider = DataManager.GetDataFromCache(() => levelL2CacheDataAccessor.GetCommerceIdentityProviderByIssuer(request.CommerceIdentityProviderIssuer), out foundInCache, out updateCache);
    
                if (!foundInCache)
                {
                    var getIdentityProviderParameters = new ParameterSet();
                    PagedResult<CommerceRelyingParty> identityProviderRelyingParties;
                    getIdentityProviderParameters["@nvc_Issuer"] = request.CommerceIdentityProviderIssuer;
                    using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                    {
                        commerceIdentityProvider = sqlServerDatabaseContext.ExecuteStoredProcedure<CommerceIdentityProvider>(GetIdentityProviderSprocName, getIdentityProviderParameters).Results.FirstOrDefault();
    
                        if (commerceIdentityProvider != null)
                        {
                            var getRelyingPartiesParameters = new ParameterSet();
                            getRelyingPartiesParameters["@bi_ProviderId"] = commerceIdentityProvider.RecordId;
                            identityProviderRelyingParties = sqlServerDatabaseContext.ExecuteStoredProcedure<CommerceRelyingParty>(GetRelyingPartiesByIdentityProviderSprocName, getRelyingPartiesParameters);
    
                            commerceIdentityProvider.CommerceRelyingParties = identityProviderRelyingParties.Results.ToList();
                        }
                    }
                }
    
                if (updateCache && commerceIdentityProvider != null)
                {
                    levelL2CacheDataAccessor.CacheCommerceIdentityProviderByIssuer(request.CommerceIdentityProviderIssuer, commerceIdentityProvider);
                }
    
                return new SingleEntityDataServiceResponse<CommerceIdentityProvider>(commerceIdentityProvider);
            }
    
            /// <summary>
            /// Gets the cache accessor for the commerce identity provider data service requests.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>An instance of the <see cref="CommerceIdentityProviderL2CacheDataStoreAccessor"/> class.</returns>
            private CommerceIdentityProviderL2CacheDataStoreAccessor GetCommerceIdentityProviderL2CacheDataStoreAccessor(RequestContext context)
            {
                DataStoreManager.InstantiateDataStoreManager(context);
                return new CommerceIdentityProviderL2CacheDataStoreAccessor(DataStoreManager.DataStores[DataStoreType.L2Cache], context);
            }
        }
    }
}
