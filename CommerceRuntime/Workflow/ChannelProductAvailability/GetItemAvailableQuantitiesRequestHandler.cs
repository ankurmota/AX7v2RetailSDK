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
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Encapsulates the workflow required to get item available quantities.
        /// </summary>
        public sealed class GetItemAvailableQuantitiesRequestHandler : SingleRequestHandler<GetItemAvailableQuantitiesRequest, GetItemAvailableQuantitiesResponse>
        {
            /// <summary>
            /// Executes the workflow for getting item available quantities.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override GetItemAvailableQuantitiesResponse Process(GetItemAvailableQuantitiesRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.ItemUnits, "request.ItemUnits");
                
                // Gets all distinct items.
                IEnumerable<ItemVariantInventoryDimension> distinctItems = request.ItemUnits.Select(itemUnit => itemUnit.GetItem()).Distinct();
    
                // Get item available quantities.
                var quantityRequest = new GetItemAvailableQuantitiesByItemsServiceRequest(request.QueryResultSettings, distinctItems, request.CustomerAccountNumber);
                var quantityResponse = this.Context.Execute<GetItemAvailableQuantitiesByItemsServiceResponse>(quantityRequest);
    
                // Converts to requested unit of measure if possible.
                var convertedItemAvailableQuantities = ChannelAvailabilityHelper.ConvertUnitOfMeasure(this.Context, quantityResponse.ItemAvailableQuantities.Results, request.ItemUnits);
    
                // Get item available quantities.
                return new GetItemAvailableQuantitiesResponse(convertedItemAvailableQuantities.AsPagedResult(quantityResponse.ItemAvailableQuantities.TotalCount));
            }
        }
    }
}
