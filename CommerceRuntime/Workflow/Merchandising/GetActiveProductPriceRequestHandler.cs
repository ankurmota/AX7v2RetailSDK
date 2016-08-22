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
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Retrieves the price of an item.
        /// </summary>
        public sealed class GetActiveProductPriceRequestHandler : SingleRequestHandler<GetActiveProductPriceRequest, GetActiveProductPriceResponse>
        {
            /// <summary>
            /// Executes the workflow to retrieve active product prices for given product ids.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override GetActiveProductPriceResponse Process(GetActiveProductPriceRequest request)
            {
                ThrowIf.Null(request, "request");

                var validateCustomerAccountRequest = new GetValidatedCustomerAccountNumberServiceRequest(request.CustomerAccountNumber, throwOnValidationFailure: true);
                var validateCustomerAccountResponse = this.Context.Execute<GetValidatedCustomerAccountNumberServiceResponse>(validateCustomerAccountRequest);
                if (validateCustomerAccountResponse.IsCustomerAccountNumberInContextDifferent)
                {
                    request.CustomerAccountNumber = validateCustomerAccountResponse.ValidatedAccountNumber;
                }

                bool downloadedProductsFilter = false;
                if (request.Context.ChannelId != this.Context.GetPrincipal().ChannelId)
                {
                    downloadedProductsFilter = true;
                }

                var settings = new QueryResultSettings(PagingInfo.CreateWithExactCount(request.ProductIds.Count(), 0));
                var productsRequest = new GetProductsDataRequest(request.ProductIds, settings, downloadedProductsFilter);
                var products = this.Context.Execute<EntityDataServiceResponse<SimpleProduct>>(productsRequest).PagedEntityCollection.Results;
                var activePrices = new List<ProductPrice>(products.Count);

                // package sales lines to calculate
                var salesLines = new List<SalesLine>(products.Count);
                foreach (var product in products)
                {
                    salesLines.Add(new SalesLine
                    {
                        ItemId = product.ItemId,
                        InventoryDimensionId = product.InventoryDimensionId,
                        SalesOrderUnitOfMeasure = product.DefaultUnitOfMeasure,
                        LineId = System.Guid.NewGuid().ToString("N"),
                        Quantity = 1,
                        ProductId = product.RecordId,
                        CatalogId = request.Context.CatalogId.GetValueOrDefault()
                    });
                }
    
                // set the catalogIds on the sales lines
                if (request.Context.CatalogId != null)
                {
                    if (request.Context.CatalogId.Value > 0)
                    {
                        // If a specific catalogId is set on the context, add it to the catalogIds on the sales lines.
                        foreach (var sl in salesLines)
                        {
                            sl.CatalogIds.Add(request.Context.CatalogId.Value);
                        }
                    }
                    else
                    {
                        // If catalogId is 0, add all active catalogs to the catalogIds on the sales lines.
                        foreach (var sl in salesLines)
                        {
                            var productCatalogAssociationRequest = new GetProductCatalogAssociationsDataRequest(salesLines.Select(p => p.ProductId))
                                {
                                    QueryResultSettings = QueryResultSettings.AllRecords
                                };
                            var productCatalogs = request.RequestContext.Runtime.Execute<GetProductCatalogAssociationsDataResponse>(
                                productCatalogAssociationRequest,
                                request.RequestContext).CatalogAssociations;
    
                            sl.CatalogIds.UnionWith(productCatalogs.Where(pc => pc.ProductRecordId == sl.ProductId).Select(pc => pc.CatalogRecordId));
                        }
                    }
                }
    
                Customer customer = null;
                if (!string.IsNullOrWhiteSpace(request.CustomerAccountNumber))
                {
                    var getCustomerDataRequest = new GetCustomerDataRequest(request.CustomerAccountNumber);
                    SingleEntityDataServiceResponse<Customer> getCustomerDataResponse = this.Context.Runtime.Execute<SingleEntityDataServiceResponse<Customer>>(getCustomerDataRequest, this.Context);
                    customer = getCustomerDataResponse.Entity;
                }
    
                string priceGroup = customer != null ? customer.PriceGroup : string.Empty;
    
                // calculate prices for sales lines
                var itemPriceServiceRequest = new GetPricesServiceRequest(salesLines, request.DateWhenActive, request.CustomerAccountNumber, priceGroup, PricingCalculationMode.Independent, request.AffiliationLoyaltyTiers);
                var itemPriceServiceResponse = this.Context.Execute<GetPricesServiceResponse>(itemPriceServiceRequest);
                var salesLineDictionary = itemPriceServiceResponse.SalesLines.Results.ToDictionary(sl => sl.ProductId);
    
                foreach (var product in products)
                {
                    SalesLine salesLine;
                    if (!salesLineDictionary.TryGetValue(product.RecordId, out salesLine))
                    {
                        salesLine = new SalesLine();
                    }

                    ProductPrice activePrice = GetActiveProductPriceRequestHandler.ActivePriceFromSalesLine(product.RecordId, salesLine);
                    activePrice.ProductId = product.RecordId;
                    activePrice.ValidFrom = request.DateWhenActive;
                    activePrice.CurrencyCode = itemPriceServiceResponse.CurrencyCode;
                    activePrice.ChannelId = request.Context.ChannelId.GetValueOrDefault();
                    activePrice.CatalogId = request.Context.CatalogId.GetValueOrDefault();
    
                    activePrices.Add(activePrice);
                }
    
                return new GetActiveProductPriceResponse(activePrices.AsPagedResult());
            }
    
            private static ProductPrice ActivePriceFromSalesLine(long productId, SalesLine salesLine)
            {
                return new ProductPrice
                {
                    UnitOfMeasure = salesLine.SalesOrderUnitOfMeasure,
                    ItemId = salesLine.ItemId,
                    InventoryDimensionId = salesLine.InventoryDimensionId,
                    BasePrice = salesLine.BasePrice,
                    TradeAgreementPrice = salesLine.AgreementPrice,
                    AdjustedPrice = salesLine.AdjustedPrice,
                    ProductId = productId            
                };
            }
        }
    }
}
