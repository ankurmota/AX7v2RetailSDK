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
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Encapsulates the workflow required to retrieve an item.
        /// </summary>
        /// <remarks>
        /// If both ItemId and RecordId have been specified, ItemId takes precedence.
        /// </remarks>
        public sealed class GetItemByIdRequestHandler : SingleRequestHandler<GetItemByIdRequest, GetItemByIdResponse>
        {
            /// <summary>
            /// Executes the workflow to retrieve an item.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override GetItemByIdResponse Process(GetItemByIdRequest request)
            {
                ThrowIf.Null(request, "request");
    
                List<Item> items = new List<Item>();
                if (request.ItemIds.Any())
                {
                    var getItemsRequest = new GetItemsDataRequest(request.ItemIds)
                    {
                        QueryResultSettings = new QueryResultSettings(request.ColumnSet, PagingInfo.AllRecords)
                    };

                    var getItemsResponse = request.RequestContext.Runtime.Execute<GetItemsDataResponse>(getItemsRequest, request.RequestContext);
    
                    items.AddRange(getItemsResponse.Items);
                }
    
                if (request.ProductIds.Any())
                {
                    GetItemsDataRequest dataRequest = new GetItemsDataRequest(request.ProductIds)
                    {
                        QueryResultSettings = request.QueryResultSettings
                    };

                    GetItemsDataResponse dataResponse = this.Context.Runtime.Execute<GetItemsDataResponse>(dataRequest, this.Context);
    
                    items.AddRange(dataResponse.Items);
                }
    
                var response = new GetItemByIdResponse(items.AsPagedResult());
                return response;
            }
        }
    }
}
