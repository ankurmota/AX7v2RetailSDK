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
        /// Customer search request handler.
        /// </summary>
        public sealed class CustomerSearchRequestHandler : SingleRequestHandler<CustomersSearchRequest, CustomersSearchResponse>
        {
            /// <summary>
            /// Executes the workflow to retrieve customer information.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override CustomersSearchResponse Process(CustomersSearchRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.Criteria, "request.Criteria");
    
                var serviceRequest = new CustomersSearchServiceRequest(
                    request.Criteria,
                    request.QueryResultSettings);
    
                CustomersSearchServiceResponse serviceResponse = this.Context.Execute<CustomersSearchServiceResponse>(serviceRequest);
                
                // If no Customers were found then attempt search by barcode.
                if (serviceResponse.Customers.Results.Count == 0 && !string.IsNullOrWhiteSpace(request.Criteria.Keyword))
                {
                    var scanInfo = new ScanInfo() { ScannedText = request.Criteria.Keyword };
                    var barcodeRequest = new GetBarcodeRequest(scanInfo);
                    GetBarcodeResponse getBarcodeResponse = this.Context.Runtime.Execute<GetBarcodeResponse>(barcodeRequest, this.Context);
                    Barcode barcode = getBarcodeResponse.Barcode;
                    
                    // If barcode was a customer barcode then use result of barcode search to search for customer again.
                    if (barcode != null && barcode.Mask.MaskType == BarcodeMaskType.Customer)
                    {
                        request.Criteria.Keyword = barcode.CustomerId;
                        var customerServiceRequest = new CustomersSearchServiceRequest(
                            request.Criteria,
                            request.QueryResultSettings);
    
                        serviceResponse = this.Context.Execute<CustomersSearchServiceResponse>(customerServiceRequest);
                    }
                }
    
                return new CustomersSearchResponse(serviceResponse.Customers);
            }
        }
    }
}
