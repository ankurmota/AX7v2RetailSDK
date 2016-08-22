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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Get discount codes request handler.
        /// </summary>
        public sealed class GetDiscountCodesRequestHandler : SingleRequestHandler<GetDiscountCodesRequest, GetDiscountCodesResponse>
        {
            /// <summary>
            /// Gets the discount codes from pricing services.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override GetDiscountCodesResponse Process(GetDiscountCodesRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                // Create service request.
                var serviceRequest = new GetDiscountCodesServiceRequest(
                    request.OfferId, request.DiscountCode, request.Keyword, request.ActiveDate, request.QueryResultSettings);
    
                // Execute service request.
                var serviceResponse = this.Context.Execute<GetDiscountCodesServiceResponse>(serviceRequest);
                
                // If no discount codes were found then attempt the search again using Barcode
                if (serviceResponse.DiscountCodes != null && serviceResponse.DiscountCodes.Results.Count == 0)
                {
                    var scanInfo = new ScanInfo() { ScannedText = request.Keyword };
                    var barcodeRequest = new GetBarcodeRequest(scanInfo);
                    GetBarcodeResponse getBarcodeResponse = this.Context.Runtime.Execute<GetBarcodeResponse>(barcodeRequest, this.Context);
                    Barcode barcode = getBarcodeResponse.Barcode;
                    if (barcode != null && barcode.Mask.MaskType == BarcodeMaskType.DiscountCode)
                    {
                        var getDiscountCodesByBarcodeServiceRequest = new GetDiscountCodesServiceRequest(
                            request.OfferId, barcode.DiscountCode, null, request.ActiveDate, request.QueryResultSettings);
                        serviceResponse = this.Context.Execute<GetDiscountCodesServiceResponse>(getDiscountCodesByBarcodeServiceRequest);
                    }
                }
    
                // Convert service response to response.
                return new GetDiscountCodesResponse(serviceResponse.DiscountCodes);
            }
        }
    }
}
