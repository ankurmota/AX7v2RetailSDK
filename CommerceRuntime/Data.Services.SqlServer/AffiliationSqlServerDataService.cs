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
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Affiliation SQL server data service class.
        /// </summary>
        public class AffiliationSqlServerDataService : IRequestHandler
        {
            private const string GetAffiliationsByAffiliationTypeSprocName = "GETAFFILIATIONSBYAFFILIATIONTYPE";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get { return new[] { typeof(GetAffiliationsDataRequest) }; }
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
    
                if (requestType == typeof(GetAffiliationsDataRequest))
                {
                    response = this.GetAffiliations((GetAffiliationsDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Gets affiliations.
            /// </summary>
            /// <param name="request">The request with affiliation type and query result settings.</param>
            /// <returns>The data service response with the collection of affiliations.</returns>
            public EntityDataServiceResponse<Affiliation> GetAffiliations(GetAffiliationsDataRequest request)
            {
                ThrowIf.Null(request, "request");
    
                ThrowIf.Null(request.QueryResultSettings, "settings");
    
                ParameterSet parameters = new ParameterSet();
                parameters["@i_affiliationType"] = (int)request.AffiliationType;
                parameters["@nvc_Locale"] = request.RequestContext.LanguageId;
    
                PagedResult<Affiliation> affiliations;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    affiliations = sqlServerDatabaseContext.ExecuteStoredProcedure<Affiliation>(GetAffiliationsByAffiliationTypeSprocName, parameters);
                }
    
                return new EntityDataServiceResponse<Affiliation>(affiliations);
            }
        }
    }
}
