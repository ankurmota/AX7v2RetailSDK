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
        /// Affiliation data services that contains methods to retrieve the information by calling views.
        /// </summary>
        public class AffiliationDataService : IRequestHandler
        {
            private const string RetailAffiliationsView = "RETAILAFFILIATIONSVIEW";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetAffiliationByAffiliationIdDataRequest)
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
    
                if (requestType == typeof(GetAffiliationByAffiliationIdDataRequest))
                {
                    response = this.GetAffiliationByAffiliationId((GetAffiliationByAffiliationIdDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType().ToString()));
                }
    
                return response;
            }
    
            private AffiliationDataManager GetDataManagerInstance(RequestContext context)
            {
                return new AffiliationDataManager(context);
            }
    
            private SingleEntityDataServiceResponse<Affiliation> GetAffiliationByAffiliationId(GetAffiliationByAffiliationIdDataRequest request)
            {
                AffiliationDataManager affiliationDataManager = this.GetDataManagerInstance(request.RequestContext);
    
                Affiliation affiliation = affiliationDataManager.GetAffiliationByAffiliationId(request.AffiliationId);
    
                return new SingleEntityDataServiceResponse<Affiliation>(affiliation);
            }
        }
    }
}
