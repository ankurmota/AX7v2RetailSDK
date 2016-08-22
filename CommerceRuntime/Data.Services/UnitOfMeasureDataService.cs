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
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// The unit of measure common data request handler.
        /// </summary>
        public class UnitOfMeasureDataService : IRequestHandler
        {
            private const string GetUnitsOfMeasureFunctionName = "GETUNITSOFMEASURE(@LanguageId)";
            private const string SymbolColumnName = "SYMBOL";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[] { typeof(GetUnitsOfMeasureDataRequest) };
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
    
                if (requestType == typeof(GetUnitsOfMeasureDataRequest))
                {
                    response = this.GetUnitsOfMeasure((GetUnitsOfMeasureDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Retrieves the units of measure for the given unit identifiers.
            /// If no unit identifiers are provided then all the supported units of measure are retrieved.
            /// </summary>
            /// <param name="request">The units of measure request.</param>
            /// <returns>
            /// A unit of measure for the symbol.
            /// </returns>
            private EntityDataServiceResponse<UnitOfMeasure> GetUnitsOfMeasure(GetUnitsOfMeasureDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.UnitIds, "request.UnitIds");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                PagedResult<UnitOfMeasure> results;
    
                // Default query to retrieve all units of measure.
                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = GetUnitsOfMeasureFunctionName,
                    Aliased = true,
                    OrderBy = SymbolColumnName,
                };
    
                query.Parameters["@LanguageId"] = request.RequestContext.LanguageId;
    
                // Update query when only one unit of measure is retrieved.
                if (request.UnitIds.Count() == 1)
                {
                    query.Where = string.Format("{0} = @UnitId", SymbolColumnName);
                    query.Parameters["@UnitId"] = request.UnitIds.FirstOrDefault();
                }
    
                // Update query when multiple units of measure are retrieved.
                if (request.UnitIds.HasMultiple())
                {
                    IEnumerable<string> distinctUnitOfMeasure = request.UnitIds.Distinct(StringComparer.OrdinalIgnoreCase);
    
                    using (StringIdTableType type = new StringIdTableType(distinctUnitOfMeasure, SymbolColumnName))
                    {
                        query.Parameters["@TVP_UNITIDTABLETYPE"] = type;
    
                        // Query execution for retrieving multiple units.
                        results = this.ExecuteQuery(query, request.RequestContext);
                    }
                }
                else
                {
                    // Query execution for retrieving single or all the units.
                    results = this.ExecuteQuery(query, request.RequestContext);
                }
    
                return new EntityDataServiceResponse<UnitOfMeasure>(results);
            }
    
            private PagedResult<UnitOfMeasure> ExecuteQuery(SqlPagedQuery query, RequestContext context)
            {
                PagedResult<UnitOfMeasure> results;
    
                using (var databaseContext = new DatabaseContext(context))
                {
                    results = databaseContext.ReadEntity<UnitOfMeasure>(query);
                }
    
                return results;
            }
        }
    }
}
