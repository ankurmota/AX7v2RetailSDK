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
    namespace Commerce.Runtime.DataServices.SqlServer
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Shifts data service class.
        /// </summary>
        public class NumberSequenceSqlServerDataService : IRequestHandler
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
                        typeof(GetLatestNumberSequenceDataRequest),
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
    
                if (requestType == typeof(GetLatestNumberSequenceDataRequest))
                {
                    response = this.GetLatestNumberSequenceValue((GetLatestNumberSequenceDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType().ToString()));
                }
    
                return response;
            }
    
            private GetLatestNumberSequenceDataResponse GetLatestNumberSequenceValue(GetLatestNumberSequenceDataRequest request)
            {
                Shift numberSequenceForShift = null;
                SalesTransaction numberSequenceForSalesTransaction = null;
                IEnumerable<SalesTransaction> numberSequenceForReceipts = null;
    
                var sequenceSeedDataManager = new NumberSequenceSeedDataManager(request.RequestContext);
                var numberSequenceValue = sequenceSeedDataManager.GetLatestNumberSequenceData(request.TerminalId);
    
                if (numberSequenceValue != null)
                {
                    if (numberSequenceValue.Item1 != null && numberSequenceValue.Item1.Any())
                    {
                        numberSequenceForShift = numberSequenceValue.Item1.Single();
                    }
    
                    if (numberSequenceValue.Item2 != null && numberSequenceValue.Item2.Any())
                    {
                        numberSequenceForSalesTransaction = numberSequenceValue.Item2.Single();
                    }
    
                    if (numberSequenceValue.Item3 != null && numberSequenceValue.Item3.Any())
                    {
                        numberSequenceForReceipts = numberSequenceValue.Item3;
                    }
                }
    
                return new GetLatestNumberSequenceDataResponse(numberSequenceForShift, numberSequenceForSalesTransaction, numberSequenceForReceipts);
            }
        }
    }
}
