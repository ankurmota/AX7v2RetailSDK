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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Business Intelligence Service class.
        /// </summary>
        public sealed class BusinessIntelligenceService : IRequestHandler
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
                        typeof(GetReportDataServiceRequest),
                    };
                }
            }
    
            /// <summary>
            /// Entry point to business intelligence service. Takes a business intelligence service request and returns the result
            /// of the request execution.
            /// </summary>
            /// <param name="request">The business intelligence service request to execute.</param>
            /// <returns>Result of executing request, or null object for void operations.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
                if (requestType == typeof(GetReportDataServiceRequest))
                {
                    response = GetReportData((GetReportDataServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// The request to get report configuration from data layer.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The report data set.</returns>
            private static GetReportDataServiceResponse GetReportData(GetReportDataServiceRequest request)
            {
                ReportDataSet outputData;
    
                var dataRequest = new GetReportConfigurationDataRequest(request.ReportId, request.QueryResultSettings);
                ReportConfiguration reportDefinition = request.RequestContext.Runtime.Execute<SingleEntityDataServiceResponse<ReportConfiguration>>(dataRequest, request.RequestContext).Entity;
    
                // If reportDefinition is null, throw error.
                ThrowIf.Null(reportDefinition, "This report is either not configured in database or not allowed for this role.");
                reportDefinition.SetReportParameters(request.ReportParameters);
    
                // Get report data based on its type.
                if (reportDefinition.DataSourceType.Equals("OLTP", StringComparison.OrdinalIgnoreCase))
                {
                    var dataReportRequest = new GetOLTPReportDataRequest(reportDefinition, request.QueryResultSettings);
                    outputData = request.RequestContext.Runtime.Execute<SingleEntityDataServiceResponse<ReportDataSet>>(dataReportRequest, request.RequestContext).Entity;
                }
                else
                {
                    throw new NotSupportedException("The report type of the requested report is not supported.");
                }
    
                return new GetReportDataServiceResponse(outputData);
            }
        }
    }
}
