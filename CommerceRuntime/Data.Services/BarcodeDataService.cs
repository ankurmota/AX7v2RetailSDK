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
    namespace Commerce.Runtime.DataServices.Common
    {
        using System;
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// The barcode common data request handler.
        /// </summary>
        public class BarcodeDataService : IRequestHandler
        {
            private const string BarcodeMaskSegmentsView = "BARCODEMASKSEGMENTSVIEW";
            private const string BarcodeMasksView = "BARCODEMASKSVIEW";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetBarcodeMaskSegmentDataRequest),
                        typeof(GetBarcodeMaskDataRequest),
                    };
                }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
    
                if (requestType == typeof(GetBarcodeMaskSegmentDataRequest))
                {
                    response = this.GetBarcodeMaskSegment((GetBarcodeMaskSegmentDataRequest)request);
                }
                else if (requestType == typeof(GetBarcodeMaskDataRequest))
                {
                    response = this.GetBarcodeMask((GetBarcodeMaskDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Gets the barcode mask segment using the specified mask identifier.
            /// </summary>
            /// <param name="request">The get barcode mask segment data request.</param>
            /// <returns>
            /// A entity data service response.
            /// </returns>
            private EntityDataServiceResponse<BarcodeMaskSegment> GetBarcodeMaskSegment(GetBarcodeMaskSegmentDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.MaskId, "request.MaskId");
    
                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = BarcodeMaskSegmentsView,
                    Where = "MASKID = @MaskId AND DATAAREAID = @DataAreaId",
                };
    
                query.Parameters["@MaskId"] = request.MaskId;
                query.Parameters["@DataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
    
                PagedResult<BarcodeMaskSegment> results;
                using (var databaseContext = new DatabaseContext(request.RequestContext))
                {
                    results = databaseContext.ReadEntity<BarcodeMaskSegment>(query);
                }
    
                return new EntityDataServiceResponse<BarcodeMaskSegment>(results);
            }
    
            /// <summary>
            /// Gets the barcode mask segment using the specified mask identifier.
            /// </summary>
            /// <param name="request">The get barcode mask segment data request.</param>
            /// <returns>
            /// A entity data service response.
            /// </returns>
            private EntityDataServiceResponse<BarcodeMask> GetBarcodeMask(GetBarcodeMaskDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.Prefix, "request.Prefix");
    
                PagedResult<BarcodeMask> results;
    
                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = BarcodeMasksView,
                    OrderBy = "LENGTH DESC, MASK",
                    Where = "PREFIX LIKE @Prefix AND DATAAREAID = @DataAreaId",
                };
    
                query.Parameters["@Prefix"] = string.Format("{0}%", request.Prefix);
                query.Parameters["@DataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
    
                using (var databaseContext = new DatabaseContext(request.RequestContext))
                {
                    results = databaseContext.ReadEntity<BarcodeMask>(query);
                }
    
                return new EntityDataServiceResponse<BarcodeMask>(results);
            }
        }
    }
}
