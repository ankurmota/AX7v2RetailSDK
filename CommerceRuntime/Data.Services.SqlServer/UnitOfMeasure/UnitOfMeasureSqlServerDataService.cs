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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// The request handler for unit of measure data requests.
        /// </summary>
        public sealed class UnitOfMeasureSqlServerDataService : IRequestHandler
        {
            private const string GetUnitOfMeasureConversionsFunctionName = "GETUNITOFMEASURECONVERSIONS(@nvc_DataAreaId, @tvp_ItemUnitConversions)";
            private const string DataAreaIdVariableName = "@nvc_DataAreaId";
            private const string ItemUnitConversionsTableType = "@tvp_ItemUnitConversions";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[] { typeof(GetUnitOfMeasureConversionDataRequest) };
                }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                ThrowIf.Null(request, "request");
    
                Response response;
    
                if (request is GetUnitOfMeasureConversionDataRequest)
                {
                    response = GetUnitOfMeasureConversion((GetUnitOfMeasureConversionDataRequest)request);
                }
                else
                {
                    string message = string.Format("Request type '{0}' is not supported", request.GetType().FullName);
                    throw new NotSupportedException(message);
                }
    
                return response;
            }
    
            private static GetUnitOfMeasureConversionDataResponse GetUnitOfMeasureConversion(GetUnitOfMeasureConversionDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.ItemUnitConversions, "request.ItemUnitConversions");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = GetUnitOfMeasureConversionsFunctionName,
                    OrderBy = "ITEMID, FROMUNITID, TOUNITID",
                };
    
                PagedResult<UnitOfMeasureConversion> unitOfMeasureConversions;
    
                using (ItemUnitConversionTableType type = new ItemUnitConversionTableType(request.ItemUnitConversions))
                {
                    query.Parameters[DataAreaIdVariableName] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                    query.Parameters[ItemUnitConversionsTableType] = type.DataTable;
    
                    using (var sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                    {
                        unitOfMeasureConversions = sqlServerDatabaseContext.ReadEntity<UnitOfMeasureConversion>(query);
                    }
                }
    
                return new GetUnitOfMeasureConversionDataResponse(unitOfMeasureConversions);
            }
        }
    }
}
