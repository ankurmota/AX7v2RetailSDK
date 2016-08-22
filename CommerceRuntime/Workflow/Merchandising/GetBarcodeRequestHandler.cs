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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// Workflow class helps to retrieve the Barcode details.
        /// </summary>
        public class GetBarcodeRequestHandler : SingleRequestHandler<GetBarcodeRequest, GetBarcodeResponse>
        {
            /// <summary>
            /// Barcode workflow handler to process the incoming workflow requests.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override GetBarcodeResponse Process(GetBarcodeRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.ScanInfo, "request.ScanInfo");
                ThrowIf.NullOrWhiteSpace(request.ScanInfo.ScannedText, "request.ScanInfo.ScanDataLabel");

                string barcodeId = request.ScanInfo.ScannedText;
                ItemBarcode itemBarcode = GetItemBarcode(request.RequestContext, barcodeId);
                BarcodeMask barcodeMask = GetBarcodeTypeFromMask(request.RequestContext, barcodeId);

                Barcode barcode = new Barcode();
                if (itemBarcode == null)
                {
                    if (barcodeMask == null)
                    {
                        return new GetBarcodeResponse();
                    }
                }
                else
                {
                    barcode.ItemBarcode = itemBarcode;
                }

                if (barcodeMask != null)
                {
                    barcode.Mask = barcodeMask;
                }
                else
                {
                    barcode.Mask.MaskType = BarcodeMaskType.Item;
                }

                barcode.BarcodeId = barcodeId;
                barcode.TimeStarted = DateTime.UtcNow;
                barcode.EntryMethodType = request.ScanInfo.EntryMethodType;

                if (barcodeMask != null)
                {
                    barcode = ProcessBarcodeMask(request.RequestContext, barcode, barcodeMask);
                }

                return new GetBarcodeResponse(barcode);
            }

            private static BarcodeMask GetBarcodeTypeFromMask(RequestContext context, string barcodeId)
            {
                var serviceRequest = new GetBarcodeTypeServiceRequest(barcodeId);
                var serviceResponse = context.Execute<GetBarcodeTypeServiceResponse>(serviceRequest);
                return serviceResponse.BarcodeMask;
            }

            private static Barcode ProcessBarcodeMask(RequestContext context, Barcode barcode, BarcodeMask barcodeMask)
            {
                // If given Barcode prefix matches with configured in AX, then barcode should fall under anyone of Internal types defined.
                if (barcodeMask != null)
                {
                    switch (barcodeMask.MaskType)
                    {
                        case BarcodeMaskType.Item:
                        case BarcodeMaskType.Customer:
                        case BarcodeMaskType.DataEntry:
                        case BarcodeMaskType.Employee:
                        case BarcodeMaskType.Salesperson:
                        case BarcodeMaskType.DiscountCode:
                        case BarcodeMaskType.GiftCard:
                        case BarcodeMaskType.LoyaltyCard:
                            {
                                // Get the Barcode details by processing the masked segments.
                                var serviceRequest = new ProcessMaskSegmentsServiceRequest(barcode, barcodeMask);
                                var serviceResponse = context.Execute<ProcessMaskSegmentsServiceResponse>(serviceRequest);
                                return serviceResponse.Barcode;
                            }

                        case BarcodeMaskType.Coupon:
                            break;

                        default:
                            throw new DataValidationException(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_UnSupportedType, 
                                string.Format("Process Barcode: Unsupported barcode type {0}", barcodeMask.MaskType));
                    }
                }

                return barcode;
            }

            private static ItemBarcode GetItemBarcode(RequestContext context, string barcodeId)
            {
                GetProductBarcodeDataRequest productBarcodeRequest = new GetProductBarcodeDataRequest(barcodeId);
                GetProductBarcodeDataResponse productBarcodeResponse = context.Execute<GetProductBarcodeDataResponse>(productBarcodeRequest);
                return productBarcodeResponse.Barcode;
            }
        }
    }
}
