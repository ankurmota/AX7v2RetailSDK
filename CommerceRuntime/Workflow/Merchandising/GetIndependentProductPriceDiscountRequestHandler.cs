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
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Retrieves the prices and calculated discount amount for the products.
        /// </summary>
        public sealed class GetIndependentProductPriceDiscountRequestHandler : SingleRequestHandler<GetIndependentProductPriceDiscountRequest, GetIndependentProductPriceDiscountResponse>
        {
            /// <summary>
            /// Executes the workflow to retrieve the prices and calculated discount amount for the given product identifiers.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override GetIndependentProductPriceDiscountResponse Process(GetIndependentProductPriceDiscountRequest request)
            {
                ThrowIf.Null(request, "request");

                ProductSearchCriteria productSearchCriteria = new ProductSearchCriteria(
                    request.Context.ChannelId.GetValueOrDefault(), 
                    request.Context.CatalogId.GetValueOrDefault())
                    {
                        DataLevel = CommerceEntityDataLevel.Standard,
                        Ids = request.ProductIds.ToList()
                    };

                ProductSearchResultContainer productSearchResult = request.RequestContext.Runtime.Execute<ProductSearchServiceResponse>(
                    new ProductSearchServiceRequest(productSearchCriteria, request.QueryResultSettings), request.RequestContext).ProductSearchResult;
    
                List<ProductPrice> productPrices = new List<ProductPrice>(request.ProductIds.Count());
    
                // Create sales line for every product id there is in the request.
                List<SalesLine> salesLines = new List<SalesLine>(request.ProductIds.Count());
                foreach (var product in productSearchResult.Results)
                {
                    if (product.IsMasterProduct)
                    {
                        foreach (var variant in product.GetVariants())
                        {
                            if (request.ProductIds.Contains(variant.DistinctProductVariantId))
                            {
                                salesLines.Add(new SalesLine
                                {
                                    ItemId = product.ItemId,
                                    Variant = variant,
                                    InventoryDimensionId = variant.InventoryDimensionId,
                                    SalesOrderUnitOfMeasure = product.Rules.DefaultUnitOfMeasure,
                                    LineId = System.Guid.NewGuid().ToString("N"),
                                    Quantity = 1,
                                    ProductId = variant.DistinctProductVariantId,
                                    CatalogId = request.Context.CatalogId.GetValueOrDefault()
                                });
                            }
                        }
                    }
                    else
                    {
                        salesLines.Add(new SalesLine
                        {
                            ItemId = product.ItemId,
                            SalesOrderUnitOfMeasure = product.Rules.DefaultUnitOfMeasure,
                            LineId = System.Guid.NewGuid().ToString("N"),
                            Quantity = 1,
                            ProductId = product.RecordId,
                            CatalogId = request.Context.CatalogId.GetValueOrDefault()
                        });
                    }
                }
    
                // Set the catalog ids on the sales lines.
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
                        GetProductCatalogAssociationsDataRequest productCatalogAssociationRequest = new GetProductCatalogAssociationsDataRequest(salesLines.Select(p => p.ProductId))
                            {
                                QueryResultSettings = QueryResultSettings.AllRecords
                            };
                        ReadOnlyCollection<ProductCatalogAssociation> productCatalogs = request.RequestContext.Runtime.Execute<GetProductCatalogAssociationsDataResponse>(
                            productCatalogAssociationRequest,
                            request.RequestContext).CatalogAssociations;
    
                        // If catalogId is 0, add all independent catalogs to the catalogIds on the sales lines.
                        foreach (var sl in salesLines)
                        {
                            sl.CatalogIds.UnionWith(productCatalogs.Where(pc => pc.ProductRecordId == sl.ProductId).Select(pc => pc.CatalogRecordId));
                        }
                    }
                }
    
                Collection<SalesAffiliationLoyaltyTier> affiliations = null;
                if (request.AffiliationLoyaltyTiers != null)
                {
                    affiliations = new Collection<SalesAffiliationLoyaltyTier>((from alt in request.AffiliationLoyaltyTiers
                                   select new SalesAffiliationLoyaltyTier
                                   {
                                       AffiliationId = alt.AffiliationId,
                                       AffiliationType = alt.AffiliationType,
                                       LoyaltyTierId = alt.LoyaltyTierId,
                                       ReasonCodeLines = alt.ReasonCodeLines,
                                       CustomerId = alt.CustomerId,
                                       ChannelId = request.Context.ChannelId.GetValueOrDefault()
                                   }).ToList());
                }

                SalesTransaction transaction = new SalesTransaction
                {
                    SalesLines = new Collection<SalesLine>(salesLines),
                    CustomerId = request.CustomerAccountNumber,
                    AffiliationLoyaltyTierLines = affiliations
                };

                // Calculate prices and discounts for sales lines
                GetIndependentPriceDiscountServiceRequest itemPriceDiscountServiceRequest = new GetIndependentPriceDiscountServiceRequest(transaction);
                GetPriceServiceResponse itemPriceServiceResponse = this.Context.Execute<GetPriceServiceResponse>(itemPriceDiscountServiceRequest);
                Dictionary<long, SalesLine> salesLineDictionary = itemPriceServiceResponse.Transaction.SalesLines.ToDictionary(sl => sl.ProductId);
    
                foreach (long productId in request.ProductIds)
                {
                    SalesLine salesLine;
    
                    if (!salesLineDictionary.TryGetValue(productId, out salesLine))
                    {
                        salesLine = new SalesLine();
                    }
    
                    ProductPrice productPrice = new ProductPrice
                    {
                        UnitOfMeasure = salesLine.SalesOrderUnitOfMeasure,
                        ItemId = salesLine.ItemId,
                        InventoryDimensionId = salesLine.InventoryDimensionId,
                        BasePrice = salesLine.BasePrice,
                        TradeAgreementPrice = salesLine.AgreementPrice,
                        AdjustedPrice = salesLine.AdjustedPrice,
                        DiscountAmount = salesLine.DiscountAmount,    
                        ProductId = productId,
                        ChannelId = request.Context.ChannelId.GetValueOrDefault(),
                        CatalogId = request.Context.CatalogId.GetValueOrDefault()
                    };
    
                    productPrices.Add(productPrice);
                }
    
                return new GetIndependentProductPriceDiscountResponse(productPrices.AsPagedResult());
            }
        }
    }
}
