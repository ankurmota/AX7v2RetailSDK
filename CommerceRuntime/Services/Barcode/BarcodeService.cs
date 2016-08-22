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
    namespace Commerce.Runtime.Services
    {
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Represents an implementation of the BarCode service.
        /// </summary>
        public class BarcodeService : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(ProcessMaskSegmentsServiceRequest),
                        typeof(GetBarcodeTypeServiceRequest),
                        typeof(CalculateQuantityFromPriceServiceRequest)
                    };
                }
            }
    
            /// <summary>
            /// Executes the specified service serviceRequest.
            /// </summary>
            /// <param name="request">The type of serviceRequest.</param>
            /// <returns>The response.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
    
                if (requestType == typeof(ProcessMaskSegmentsServiceRequest))
                {
                    response = ProcessMaskSegments((ProcessMaskSegmentsServiceRequest)request);
                }
                else if (requestType == typeof(GetBarcodeTypeServiceRequest))
                {
                    response = GetBarcodeType((GetBarcodeTypeServiceRequest)request);
                }
                else if (requestType == typeof(CalculateQuantityFromPriceServiceRequest))
                {
                    response = CalculateQuantityFromPrice((CalculateQuantityFromPriceServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            private static CalculateQuantityFromPriceServiceResponse CalculateQuantityFromPrice(CalculateQuantityFromPriceServiceRequest request)
            {
                decimal quantity = 0;
    
                if (request.BarcodePrice > 0)
                {
                    if (request.DefaultProductPrice != request.BarcodePrice && request.DefaultProductPrice != decimal.Zero)
                    {
                        quantity = request.BarcodePrice / request.DefaultProductPrice;
    
                        var roundingRequest = new GetRoundQuantityServiceRequest(quantity, request.UnitOfMeasure);
                        var response = request.RequestContext.Execute<GetRoundQuantityServiceResponse>(roundingRequest);
    
                        quantity = response.RoundedValue;
                    }
                }
    
                return new CalculateQuantityFromPriceServiceResponse(quantity);
            }
    
            private static GetBarcodeTypeServiceResponse GetBarcodeType(GetBarcodeTypeServiceRequest serviceRequest)
            {
                if (string.IsNullOrEmpty(serviceRequest.BarcodeId))
                {
                    throw new ArgumentNullException(serviceRequest.BarcodeId);
                }
    
                string barcodePrefix = serviceRequest.BarcodeId.Substring(0, 1);
    
                GetBarcodeMaskDataRequest getBarcodeMaskDataRequest = new GetBarcodeMaskDataRequest(barcodePrefix, QueryResultSettings.AllRecords);
                IEnumerable<BarcodeMask> barcodeMasks = serviceRequest.RequestContext.Runtime.Execute<EntityDataServiceResponse<BarcodeMask>>(getBarcodeMaskDataRequest, serviceRequest.RequestContext).PagedEntityCollection.Results;
    
                BarcodeMask barcodeMask = null;
    
                bool found = false;
    
                foreach (var bcmask in barcodeMasks.Where(mask => !string.IsNullOrEmpty(mask.Prefix) && serviceRequest.BarcodeId.Length >= mask.Prefix.Length))
                {
                    barcodeMask = bcmask;
                    barcodePrefix = serviceRequest.BarcodeId.Substring(0, bcmask.Prefix.Length);
    
                    if (bcmask.Prefix == barcodePrefix)
                    {
                        if (serviceRequest.BarcodeId.Length == bcmask.Mask.Length)
                        {
                            found = true;
                            break;
                        }
                    }
                }
    
                if (found == false)
                {
                    barcodeMask = null;
                }
    
                return new GetBarcodeTypeServiceResponse(barcodeMask);
            }
    
            private static ProcessMaskSegmentsServiceResponse ProcessMaskSegments(ProcessMaskSegmentsServiceRequest serviceRequest)
            {
                GetBarcodeMaskSegmentDataRequest getBarcodeMaskSegmentDataRequest = new GetBarcodeMaskSegmentDataRequest(serviceRequest.BarcodeMask.MaskId, QueryResultSettings.AllRecords);
                IEnumerable<BarcodeMaskSegment> barCodeMaskSegments = serviceRequest.RequestContext.Runtime.Execute<EntityDataServiceResponse<BarcodeMaskSegment>>(getBarcodeMaskSegmentDataRequest, serviceRequest.RequestContext).PagedEntityCollection.Results;
    
                int position = serviceRequest.BarcodeMask.Prefix.Length;

                Barcode barcodeInfo = serviceRequest.Barcode;
    
                foreach (BarcodeMaskSegment segment in barCodeMaskSegments)
                {
                    var segmentType = (BarcodeSegmentType)segment.MaskType;
    
                    switch (segmentType)
                    {
                        case BarcodeSegmentType.Item:
                            {
                                LoadItemInfo(serviceRequest, barcodeInfo, position, segment);
                                break;
                            }
    
                        case BarcodeSegmentType.AnyNumber:
                        case BarcodeSegmentType.CheckDigit: // Check Digit is not implemented yet functionality in RetailServer.
                            {
                                break;
                            }
    
                        case BarcodeSegmentType.Price:
                            {
                                LoadPriceInfo(serviceRequest, barcodeInfo, position, segment);
    
                                if (barcodeInfo.BarcodePrice != null && barcodeInfo.ItemBarcode.ItemId != null)
                                {
                                    ProductPrice productPrice = GetItemPrice(serviceRequest.RequestContext, barcodeInfo.ItemBarcode.ItemId, barcodeInfo.ItemBarcode.InventoryDimensionId, barcodeInfo.ItemBarcode.UnitId, string.Empty);
                                    decimal defaultProductPrice = productPrice.AdjustedPrice;
    
                                    var calculateQuantityRequest = new CalculateQuantityFromPriceServiceRequest(barcodeInfo.BarcodePrice.Value, defaultProductPrice, barcodeInfo.ItemBarcode.UnitId);
                                    CalculateQuantityFromPriceServiceResponse calculateQuantityResponse = serviceRequest.RequestContext.Execute<CalculateQuantityFromPriceServiceResponse>(calculateQuantityRequest);
    
                                    barcodeInfo.BarcodeQuantity = calculateQuantityResponse.BarcodeQuantity;
                                }
    
                                break;
                            }
    
                        case BarcodeSegmentType.Quantity:
                            {
                                LoadQuantityInfo(serviceRequest, barcodeInfo, position, segment);
                                break;
                            }
    
                        case BarcodeSegmentType.DiscountCode:
                            {
                                barcodeInfo.DiscountCode = serviceRequest.Barcode.BarcodeId.Substring(position, segment.Length).TrimStart('0');
                                break;
                            }
    
                        case BarcodeSegmentType.GiftCard:
                            {
                                barcodeInfo.GiftCardNumber = serviceRequest.BarcodeMask.Prefix + serviceRequest.Barcode.BarcodeId.Substring(position, segment.Length);
                                break;
                            }
    
                        case BarcodeSegmentType.LoyaltyCard:
                            {
                                barcodeInfo.LoyaltyCardNumber = serviceRequest.BarcodeMask.Prefix + serviceRequest.Barcode.BarcodeId.Substring(position, segment.Length);
                                break;
                            }
    
                        case BarcodeSegmentType.SizeDigit:
                        case BarcodeSegmentType.ColorDigit:
                        case BarcodeSegmentType.StyleDigit:
                            {
                                // Not used.
                                break;
                            }
    
                        case BarcodeSegmentType.EANLicenseCode:
                            {
                                barcodeInfo.EANLicenseId = serviceRequest.Barcode.BarcodeId.Substring(position, segment.Length);
                                break;
                            }
    
                        case BarcodeSegmentType.Employee:
                            {
                                barcodeInfo.EmployeeId = serviceRequest.Barcode.BarcodeId.Substring(position, segment.Length);
                                break;
                            }
    
                        case BarcodeSegmentType.Salesperson:
                            {
                                barcodeInfo.SalespersonId = serviceRequest.Barcode.BarcodeId.Substring(position, segment.Length);
                                break;
                            }
    
                        case BarcodeSegmentType.Customer:
                            {
                                barcodeInfo.CustomerId = serviceRequest.Barcode.BarcodeId.Substring(position, segment.Length);
                                break;
                            }
    
                        case BarcodeSegmentType.DataEntry:
                            {
                                barcodeInfo.DataEntry = serviceRequest.Barcode.BarcodeId.Substring(position, segment.Length);
                                break;
                            }
    
                        default:
                            {
                                break;
                            }
                    }
    
                    position = position + segment.Length;
                }
    
                return new ProcessMaskSegmentsServiceResponse(barcodeInfo);
            }
    
            private static void LoadQuantityInfo(ProcessMaskSegmentsServiceRequest serviceRequest, Barcode barcode, int position, BarcodeMaskSegment segment)
            {
                string strBarCodeQuantity = serviceRequest.Barcode.BarcodeId.Substring(position, segment.Length);
    
                decimal barCodeQuantity;
    
                if (decimal.TryParse(strBarCodeQuantity, out barCodeQuantity))
                {
                    barCodeQuantity = barCodeQuantity / (decimal)Math.Pow(10, segment.Decimals);
                }
                else
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidFormat, 
                        string.Format("Cannot Parse to decimal, Invalid format {0}", strBarCodeQuantity));
                }

                if (barCodeQuantity != 0)
                {
                    barcode.BarcodeQuantity = barCodeQuantity;
                    barcode.Decimals = segment.Decimals;
                }
            }
    
            private static void LoadPriceInfo(ProcessMaskSegmentsServiceRequest serviceRequest, Barcode barcode, int position, BarcodeMaskSegment segment)
            {
                string strBarCodePrice = serviceRequest.Barcode.BarcodeId.Substring(position, segment.Length);
    
                decimal barCodePrice;
    
                if (decimal.TryParse(strBarCodePrice, out barCodePrice))
                {
                    barCodePrice = barCodePrice / (decimal)Math.Pow(10, segment.Decimals);
                }
                else
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidFormat,
                        string.Format("Cannot Parse to decimal, Invalid format {0}", strBarCodePrice));
                }

                barcode.BarcodePrice = barCodePrice == 0 ? (decimal?)null : barCodePrice;
                barcode.Decimals = segment.Decimals;
            }
    
            private static void LoadItemInfo(ProcessMaskSegmentsServiceRequest serviceRequest, Barcode barcode, int position, BarcodeMaskSegment segment)
            {
                if (barcode.ItemBarcode != null && !string.IsNullOrWhiteSpace(barcode.ItemBarcode.ItemId))
                {
                    // Skip item barcode lookup if it is already set.
                    return;
                }

                string barcodeText = serviceRequest.Barcode.BarcodeId.Substring(0, position + segment.Length);
                barcodeText += '%';
    
                GetProductBarcodeDataRequest dataRequest = new GetProductBarcodeDataRequest(barcodeText);
                ItemBarcode itemBarcode = serviceRequest.RequestContext.Runtime.Execute<GetProductBarcodeDataResponse>(dataRequest, serviceRequest.RequestContext).Barcode;
    
                if (itemBarcode == null)
                {
                    barcodeText = barcodeText.Substring(0, barcodeText.Length - 1);
                    barcodeText += Convert.ToString(CalculateCheckDigit(barcodeText), CultureInfo.CurrentCulture);
    
                    dataRequest = new GetProductBarcodeDataRequest(barcodeText);
                    itemBarcode = serviceRequest.RequestContext.Runtime.Execute<GetProductBarcodeDataResponse>(dataRequest, serviceRequest.RequestContext).Barcode;
                }

                if (itemBarcode != null)
                {
                    barcode.ItemBarcode = itemBarcode;
                    barcode.Mask.MaskType = BarcodeMaskType.Item;
                }
            }
    
            private static ProductPrice GetItemPrice(RequestContext requestContext, string itemId, string inventDimId, string unitOfMeasure, string customerAcctNumber)
            {
                SalesTransaction salesTransaction = new SalesTransaction
                {
                    Id = Guid.NewGuid().ToString(),
                    CustomerId = customerAcctNumber,
                };
    
                SalesLine salesLine = new SalesLine()
                {
                    LineId = Guid.NewGuid().ToString("N"),
                    ItemId = itemId,
                    InventoryDimensionId = inventDimId,
                    SalesOrderUnitOfMeasure = unitOfMeasure,
                    Quantity = 1m,
                };
                salesTransaction.SalesLines.Add(salesLine);
    
                GetIndependentPriceDiscountServiceRequest priceRequest = new GetIndependentPriceDiscountServiceRequest(salesTransaction);
    
                GetPriceServiceResponse pricingServiceResponse = requestContext.Execute<GetPriceServiceResponse>(priceRequest);
    
                SalesLine resultLine = pricingServiceResponse.Transaction.SalesLines[0];
    
                ProductPrice productPrice = GetProductPrice(
                    resultLine.ItemId,
                    resultLine.InventoryDimensionId,
                    resultLine.BasePrice,
                    resultLine.AgreementPrice,
                    resultLine.AdjustedPrice,
                    requestContext.GetChannelConfiguration().Currency);
    
                return productPrice;
            }
    
            private static ProductPrice GetProductPrice(string itemId, string inventoryDimensionId, decimal basePrice, decimal tradeAgreementPrice, decimal adjustedPrice, string currencyCode)
            {
                ProductPrice productPrice = new ProductPrice();
    
                productPrice.ItemId = itemId;
                productPrice.InventoryDimensionId = inventoryDimensionId;
                productPrice.BasePrice = basePrice;
                productPrice.TradeAgreementPrice = tradeAgreementPrice;
                productPrice.AdjustedPrice = adjustedPrice;
                productPrice.CurrencyCode = currencyCode;
    
                return productPrice;
            }
    
            /// <summary>
            /// Calculates the check digit for a barcode, without a check digit using the Universal Product Code (UPC) algorithm.
            /// </summary>
            /// <remarks>If the barcode contains non-digits then -1 is returned.</remarks>
            /// <param name="barcode">Barcode without a check digit.</param>
            /// <returns>The calculated check digit.</returns>
            private static int CalculateCheckDigit(string barcode)
            {
                int even = 0;
                int odd = 0;
                int total = 0;
                int checkDigit = 0;
    
                for (int i = 0; i < barcode.Length; i++)
                {
                    int temp;
    
                    if (int.TryParse(barcode.Substring(barcode.Length - 1 - i, 1), out temp))
                    {
                        if (((i + 1) % 2) == 0)
                        {
                            even += temp;
                        }
                        else
                        {
                            odd += temp;
                        }
                    }
                    else
                    {   // Not valid as it contians non-numeric data
                        return -1;
                    }
                }
    
                total = (odd * 3) + even;
                checkDigit = 10 - (total % 10);
    
                return checkDigit;
            }
        }
    }
}
