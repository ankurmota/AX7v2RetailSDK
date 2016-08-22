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
    namespace Commerce.Runtime.DataServices.Common
    {
        using System;
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Offline provision data service class.
        /// </summary>
        public sealed class OfflineProvisioningDataService : IRequestHandler
        {
            private const string OfflineLatestDatabaseChunkViewName = "OFFLINELATESTDATABASEPARTITIONSVIEW";
            private const string OfflineDatabaseChunkViewName = "OFFLINEDATABASEPARTITIONSVIEW";
    
            private static readonly Type[] SupportedRequestTypesArray = new Type[]
            {
                typeof(GetOfflineDatabaseChunksDataRequest),
            };
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get { return SupportedRequestTypesArray; }
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
    
                Response response;
    
                if (request is GetOfflineDatabaseChunksDataRequest)
                {
                    response = this.GetOfflineDatabaseChunks((GetOfflineDatabaseChunksDataRequest)request);
                }
                else
                {
                    string message = string.Format("Request type '{0}' is not supported", request.GetType().FullName);
                    throw new NotSupportedException(message);
                }
    
                return response;
            }
    
            private EntityDataServiceResponse<OfflineDatabaseChunk> GetOfflineDatabaseChunks(GetOfflineDatabaseChunksDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                PagedResult<OfflineDatabaseChunk> result = null;
                if (request.RecId != null)
                {
                    List<OfflineDatabaseChunk> offlineDatabaseChunks = new List<OfflineDatabaseChunk>();
                    offlineDatabaseChunks.Add(this.GetOfflineDatabaseChunkByRecordId((long)request.RecId, request.QueryResultSettings, request.RequestContext));
                    result = offlineDatabaseChunks.AsPagedResult(request.QueryResultSettings);
                }
                else if (request.DatabaseType != null)
                {
                    result = this.GetLatestOfflineDatabaseChunks((OfflineDatabaseType)request.DatabaseType, request.QueryResultSettings, request.RequestContext);
                }
    
                return new EntityDataServiceResponse<OfflineDatabaseChunk>(result);
            }
    
            private OfflineDatabaseChunk GetOfflineDatabaseChunkByRecordId(long recordId, QueryResultSettings settings, RequestContext requestContext)
            {
                var query = new SqlPagedQuery(settings)
                {
                    From = OfflineDatabaseChunkViewName,
                    Where = "RECID = @RecordId"
                };
    
                query.Parameters["@RecordId"] = recordId;
    
                OfflineDatabaseChunk chunk = null;
                using (DatabaseContext databaseContext = new DatabaseContext(requestContext))
                {
                    chunk = databaseContext.ReadEntity<OfflineDatabaseChunk>(query).SingleOrDefault();
                }
    
                return chunk;
            }
    
            private PagedResult<OfflineDatabaseChunk> GetLatestOfflineDatabaseChunks(OfflineDatabaseType databaseType, QueryResultSettings settings, RequestContext requestContext)
            {
                var query = new SqlPagedQuery(settings)
                {
                    From = OfflineLatestDatabaseChunkViewName,
                    Where = "DATABASETYPE = @databaseType AND CHANNELID = @channelId",
                    OrderBy = "NUMBER"
                };
    
                query.Parameters["@databaseType"] = (int)databaseType;
                query.Parameters["@channelId"] = requestContext.GetPrincipal().ChannelId;
    
                PagedResult<OfflineDatabaseChunk> chunks;
    
                using (DatabaseContext databaseContext = new DatabaseContext(requestContext))
                {
                    chunks = databaseContext.ReadEntity<OfflineDatabaseChunk>(query);
                }
    
                return chunks;
            }
        }
    }
}
