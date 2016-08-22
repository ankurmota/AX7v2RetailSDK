/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

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
    namespace Commerce.Runtime.Workflow
    {
        using System;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Handles request to begin read changed products.
        /// </summary>
        public class BeginReadChangedProductsRequestHandler : SingleRequestHandler<BeginReadChangedProductsRequest, BeginReadChangedProductsResponse>
        {
            /// <summary>
            /// Executes the workflow to retrieve changed products.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override BeginReadChangedProductsResponse Process(BeginReadChangedProductsRequest request)
            {
                ThrowIf.Null(request, "request");
    
                long channelId = this.Context.GetPrincipal().ChannelId;
                if (request.SearchCriteria.Context.IsRemoteLookup(channelId))
                {
                    string message = string.Format(
                        CultureInfo.InvariantCulture,
                        "The specified context (Channel={0}, Catalog={1}) is not supported when retrieving changed products.",
                        request.SearchCriteria.Context.ChannelId,
                        request.SearchCriteria.Context.CatalogId);
    
                    throw new NotSupportedException(message);
                }
    
                BeginReadChangedProductsDataRequest dataRequest = new BeginReadChangedProductsDataRequest(request.SearchCriteria);
                ReadChangedProductsSession session = this.Context.Runtime.Execute<SingleEntityDataServiceResponse<ReadChangedProductsSession>>(dataRequest, this.Context).Entity;
    
                BeginReadChangedProductsResponse response = new BeginReadChangedProductsResponse(session);
                return response;
            }
        }
    }
}
