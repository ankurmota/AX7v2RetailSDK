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
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Services = Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Encapsulates the workflow required to get product availability information.
        /// </summary>
        public sealed class GetStoresProductAvailabilityRequestHandler : SingleRequestHandler<GetStoreProductAvailabilityRequest, GetStoreProductAvailabilityResponse>
        {
            /// <summary>
            /// Executes the workflow for a get nearby stores with availability.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override GetStoreProductAvailabilityResponse Process(GetStoreProductAvailabilityRequest request)
            {
                ThrowIf.Null(request, "request");
    
                string variantId = request.VariantId;
    
                if (request.Items == null || !request.Items.Any())
                {
                    if (string.IsNullOrWhiteSpace(request.ItemId) && string.IsNullOrWhiteSpace(request.Barcode))
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ItemIdBarcodeMissing, "Please specify either an item id or a barcode.");
                    }
    
                    List<string> itemIds = new List<string>(1);
                    if (!string.IsNullOrWhiteSpace(request.Barcode))
                    {
                        GetProductBarcodeDataRequest dataRequest = new GetProductBarcodeDataRequest(request.Barcode)
                        {
                            QueryResultSettings = new QueryResultSettings(new ColumnSet("ITEMID"), PagingInfo.AllRecords)
                        };
                        ItemBarcode itemBarcode = this.Context.Runtime.Execute<GetProductBarcodeDataResponse>(dataRequest, this.Context).Barcode;
    
                        if (itemBarcode == null)
                        {
                            throw new DataValidationException(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_BarcodeNotFound,
                                string.Format("The specified barcode ({0}) was not found.", request.Barcode));
                        }
    
                        itemIds.Add(itemBarcode.ItemId);
                        variantId = itemBarcode.VariantId;
                    }
                    else
                    {
                        itemIds.Add(request.ItemId);
                    }

                    var getItemsRequest = new GetItemsDataRequest(itemIds)
                    {
                        QueryResultSettings = new QueryResultSettings(new ColumnSet("INVENTUNITID"), PagingInfo.AllRecords)
                    };
                    var getItemsResponse = request.RequestContext.Runtime.Execute<GetItemsDataResponse>(getItemsRequest, request.RequestContext);
    
                    ReadOnlyCollection<Item> items = getItemsResponse.Items;
                    if (items == null || items.Count == 0)
                    {
                        throw new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound, 
                            string.Format("No items were found for the specified item identifiers ({0}).", string.Join(", ", itemIds)));
                    }
    
                    if (items.Count > 1)
                    {
                        throw new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_DuplicateObject, 
                            string.Format("More than one item was found for the specified item identifiers ({0}).", string.Join(", ", itemIds)));
                    }
    
                    var getStoresRequest = new GetStoresDataRequest(this.Context.GetPrincipal().ChannelId, request.SearchArea, QueryResultSettings.AllRecords);
                    ReadOnlyCollection<OrgUnitLocation> storeLocations = this.Context.Execute<EntityDataServiceResponse<OrgUnitLocation>>(getStoresRequest).PagedEntityCollection.Results;
                    
                    var productAvailabilityRequest = new Services.GetStoreAvailabilityServiceRequest(items[0].ItemId, variantId ?? string.Empty);
                    var productAvailabilityResponse = this.Context.Execute<Services.GetStoreAvailabilityServiceResponse>(productAvailabilityRequest);
    
                    var storeAvailabilities = GetStoreAvailabilities(productAvailabilityResponse.ItemsAvailability, storeLocations, items[0].InventoryUnitOfMeasure);
                    return new GetStoreProductAvailabilityResponse(storeAvailabilities.AsPagedResult());
                }
                else
                {
                    ThrowIf.Null(request.Items, "request.Items");
    
                    var getStoresRequest = new GetStoresDataRequest(this.Context.GetPrincipal().ChannelId, request.SearchArea, QueryResultSettings.AllRecords);
                    ReadOnlyCollection<OrgUnitLocation> storeLocations = this.Context.Execute<EntityDataServiceResponse<OrgUnitLocation>>(getStoresRequest).PagedEntityCollection.Results;
    
                    var storeAvailabilities = ChannelAvailabilityHelper.GetChannelAvailabiltiy(this.Context, storeLocations, request.Items);
                    return new GetStoreProductAvailabilityResponse(storeAvailabilities.AsPagedResult());
                }
            }
    
            private static List<OrgUnitAvailability> GetStoreAvailabilities(IList<ItemAvailabilityStore> inventoryInfo, ReadOnlyCollection<OrgUnitLocation> stores, string unitOfMeasure)
            {
                List<OrgUnitAvailability> storeAvailabilities = new List<OrgUnitAvailability>();
    
                for (int i = 0; i < inventoryInfo.Count; i++)
                {
                    var storesFound = from c in stores where c.OrgUnitName == inventoryInfo[i].OrgUnitName select c;
    
                    if (!storesFound.Any())
                    {
                        continue;
                    }
    
                    ItemAvailability itemAvailibility = new ItemAvailability
                    {
                        ItemId = inventoryInfo[i].ItemId,
                        InventoryLocationId = inventoryInfo[i].InventoryLocationId,
                        AvailableQuantity = inventoryInfo[i].AvailableQuantity,
                        UnitOfMeasure = unitOfMeasure
                    };
    
                    OrgUnitAvailability storeAvailability = new OrgUnitAvailability(storesFound.First(), new List<ItemAvailability> { itemAvailibility });
                    storeAvailabilities.Add(storeAvailability);
                }
    
                return storeAvailabilities;
            }
        }
    }
}
