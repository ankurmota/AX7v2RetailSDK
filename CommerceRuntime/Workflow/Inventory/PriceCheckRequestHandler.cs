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
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Encapsulates the workflow required to get price check information.
        /// </summary>
        public sealed class PriceCheckRequestHandler : SingleRequestHandler<PriceCheckRequest, PriceCheckResponse>
        {
            /// <summary>
            /// Executes the workflow for a get price check for a product.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override PriceCheckResponse Process(PriceCheckRequest request)
            {
                ThrowIf.Null(request, "request");
    
                ItemBarcode itemBarcode = null;
    
                if (string.IsNullOrEmpty(request.Barcode) && string.IsNullOrEmpty(request.ItemId))
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ItemIdBarcodeMissing, "Either an item identifier or barcode is required.");
                }
    
                if (string.IsNullOrEmpty(request.ItemId))
                {
                    GetProductBarcodeDataRequest dataRequest = new GetProductBarcodeDataRequest(request.Barcode);
                    itemBarcode = this.Context.Runtime.Execute<GetProductBarcodeDataResponse>(dataRequest, this.Context).Barcode;
                }
    
                SalesTransaction salesTransaction = new SalesTransaction()
                {
                    Id = Guid.NewGuid().ToString(),
                    CustomerId = request.CustomerAccountNumber,
                };
    
                SalesLine salesLine = new SalesLine()
                {
                    ItemId = request.ItemId,
                    InventoryDimensionId = request.InventoryDimensionId ?? itemBarcode.InventoryDimensionId,
                    SalesOrderUnitOfMeasure = request.UnitOfMeasureSymbol ?? itemBarcode.UnitId,
                    Quantity = 1m,
                    LineId = Guid.NewGuid().ToString()
                };
                salesTransaction.SalesLines.Add(salesLine);
    
                GetIndependentPriceDiscountServiceRequest priceRequest = new GetIndependentPriceDiscountServiceRequest(salesTransaction);
    
                GetPriceServiceResponse pricingServiceResponse = this.Context.Execute<GetPriceServiceResponse>(priceRequest);
    
                SalesLine resultLine = pricingServiceResponse.Transaction.SalesLines[0];
    
                ProductPrice productPrice = GetProductPrice(
                    resultLine.ItemId,
                    resultLine.InventoryDimensionId,
                    resultLine.BasePrice,
                    resultLine.TotalAmount,
                    this.Context.GetChannelConfiguration().Currency);
    
                var productPrices = new List<ProductPrice> { productPrice };
                return new PriceCheckResponse(productPrices.AsPagedResult());
            }
    
            private static ProductPrice GetProductPrice(string itemId, string inventoryDimensionId, decimal basePrice, decimal price, string currencyCode)
            {
                ProductPrice productPrice = new ProductPrice();
    
                productPrice.ItemId = itemId;
                productPrice.InventoryDimensionId = inventoryDimensionId;
                productPrice.BasePrice = basePrice;
                productPrice.CustomerContextualPrice = price;
                productPrice.CurrencyCode = currencyCode;
    
                return productPrice;
            }
        }
    }
}
