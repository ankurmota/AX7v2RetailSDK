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
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// Workflow that retrieves information based on scan input.
        /// </summary>
        public class GetScanResultRequestHandler : SingleRequestHandler<GetScanResultRequest, GetScanResultResponse>
        {
            /// <summary>
            /// Executes the workflow to get scan result.
            /// </summary>
            /// <param name="request">Instance of <see cref="GetScanResultRequest"/>.</param>
            /// <returns>Instance of <see cref="GetScanResultResponse"/>.</returns>
            protected override GetScanResultResponse Process(GetScanResultRequest request)
            {
                ThrowIf.Null(request, "request");

                var getBarcodeRequest = new GetBarcodeRequest(request.ScanInfo);
                var getBarcodeResponse = this.Context.Runtime.Execute<GetBarcodeResponse>(getBarcodeRequest, this.Context);
                Barcode barcode = getBarcodeResponse.Barcode;
                BarcodeMaskType maskType = barcode == null ? BarcodeMaskType.None : barcode.Mask.MaskType;
                ScanResult result = new ScanResult(request.ScanInfo.ScannedText) { Barcode = barcode, MaskType = maskType };

                switch (maskType)
                {
                    case BarcodeMaskType.Item:
                        {
                            result.Product = this.GetSingleProductByItemId(barcode.ItemBarcode.ItemId, barcode.ItemBarcode.InventoryDimensionId);
                            break;
                        }

                    case BarcodeMaskType.Customer:
                        {
                            result.Customer = this.GetCustomerById(barcode.CustomerId);
                            break;
                        }

                    case BarcodeMaskType.DiscountCode:
                        {
                            // ScanResult.Barcode already contains discount information so no additional lookup required.
                            break;
                        }

                    case BarcodeMaskType.LoyaltyCard:
                        {
                            result.LoyaltyCard = this.GetLoyaltyCardById(barcode.LoyaltyCardNumber);
                            result.Customer = this.GetCustomerByLoyaltyCard(result.LoyaltyCard);
                            break;
                        }

                    case BarcodeMaskType.GiftCard:
                        {
                            var getGiftCardRequest = new GetGiftCardServiceRequest(barcode.GiftCardNumber);
                            var getGiftCardResponse = this.Context.Execute<GetGiftCardServiceResponse>(getGiftCardRequest);
                            result.GiftCard = getGiftCardResponse.GiftCard;
                            break;
                        }

                    case BarcodeMaskType.None:
                        {
                            // If barcode is not found try to find entities by id with following priority: Product -> Customer -> Loyalty card.
                            result.Product = this.GetSingleProductByItemId(request.ScanInfo.ScannedText, null);
                            if (result.Product != null)
                            {
                                result.MaskType = BarcodeMaskType.Item;
                                break;
                            }

                            result.Customer = this.GetCustomerById(request.ScanInfo.ScannedText);
                            if (result.Customer != null)
                            {
                                result.MaskType = BarcodeMaskType.Customer;
                                break;
                            }

                            result.LoyaltyCard = this.GetLoyaltyCardById(request.ScanInfo.ScannedText);
                            if (result.LoyaltyCard != null)
                            {
                                result.MaskType = BarcodeMaskType.LoyaltyCard;
                                result.Customer = this.GetCustomerByLoyaltyCard(result.LoyaltyCard);
                            }

                            break;
                        }

                    default:
                        {
                            throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_UnSupportedType, string.Format(CultureInfo.CurrentUICulture, "The barcode type : {0} is not supported to add to transaction", maskType));
                        }
                }

                return new GetScanResultResponse(result);
            }

            private Customer GetCustomerById(string customerId)
            {
                var getCustomerRequest = new GetCustomersServiceRequest(QueryResultSettings.SingleRecord, customerId, SearchLocation.Local);
                var getCustomerResponse = this.Context.Execute<GetCustomersServiceResponse>(getCustomerRequest);
                return getCustomerResponse.Customers.SingleOrDefault();
            }

            private LoyaltyCard GetLoyaltyCardById(string loyaltyCardId)
            {
                var getLoyaltyCardDataRequest = new GetLoyaltyCardDataRequest(loyaltyCardId);
                var getLoyaltyCardDataResponse = this.Context.Runtime.Execute<SingleEntityDataServiceResponse<LoyaltyCard>>(getLoyaltyCardDataRequest, this.Context);
                return getLoyaltyCardDataResponse.Entity;
            }

            private Customer GetCustomerByLoyaltyCard(LoyaltyCard loyaltyCard)
            {
                Customer customer = null;
                if (loyaltyCard != null && !string.IsNullOrWhiteSpace(loyaltyCard.CustomerAccount))
                {
                    customer = this.GetCustomerById(loyaltyCard.CustomerAccount);
                }

                return customer;
            }

            private SimpleProduct GetSingleProductByItemId(string itemId, string inventDimId)
            {
                if (string.IsNullOrWhiteSpace(itemId))
                {
                    return null;
                }

                var lookupClause = new ProductLookupClause(itemId, inventDimId);

                // Query first record (as opposed to single) to check if more products match criteria.
                var getProductRequest = new GetProductsServiceRequest(
                    this.Context.GetPrincipal().ChannelId,
                    new[] { lookupClause },
                    QueryResultSettings.FirstRecord);
                getProductRequest.SearchLocation = SearchLocation.Local;  // Scanned products must be found locally.
                var products = this.Context.Execute<GetProductsServiceResponse>(getProductRequest).Products;
                
                if (products.Results.IsNullOrEmpty() || products.HasNextPage)
                {
                    // if product is not found or multiple products founds (not exact match) return 'null'
                    return null;
                }

                return products.Results.Single();
            }
        }
    }
}