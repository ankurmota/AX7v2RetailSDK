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
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// The totaling service is responsible for calculating the totals on the sales transaction and sales lines.
        /// </summary>
        public sealed class TotalingService : IRequestHandler
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
                        typeof(CalculateTotalsServiceRequest),
                        typeof(CalculateAmountPaidAndDueServiceRequest),
                        typeof(CalculateDepositServiceRequest),
                        typeof(GetCustomerOrderCalculationModesServiceRequest),
                    };
                }
            }
    
            /// <summary>
            /// Executes the specified request for the totaling service.
            /// </summary>
            /// <param name="request">The request object.</param>
            /// <returns>The response object.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Response response;
                Type requestType = request.GetType();
    
                if (requestType == typeof(CalculateTotalsServiceRequest))
                {
                    response = CalculateTotals((CalculateTotalsServiceRequest)request);
                }
                else if (requestType == typeof(CalculateAmountPaidAndDueServiceRequest))
                {
                    response = CalculateAmountPaidAndDue((CalculateAmountPaidAndDueServiceRequest)request);
                }
                else if (requestType == typeof(CalculateDepositServiceRequest))
                {
                    response = CalculateDeposit((CalculateDepositServiceRequest)request);
                }
                else if (requestType == typeof(GetCustomerOrderCalculationModesServiceRequest))
                {
                    response = CalculationModesHelper.GetCalculationModes((GetCustomerOrderCalculationModesServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Calculates the totals on the sales transaction.
            /// </summary>
            /// <param name="calculateTotalsRequest">The request.</param>
            /// <returns>The service response.</returns>
            private static Response CalculateTotals(CalculateTotalsServiceRequest calculateTotalsRequest)
            {
                SalesTransactionTotaler.CalculateTotals(calculateTotalsRequest.RequestContext, calculateTotalsRequest.Transaction);
                return new CalculateTotalsServiceResponse(calculateTotalsRequest.Transaction);
            }
    
            /// <summary>
            /// Calculates the amount paid and due on the sales transaction.
            /// </summary>
            /// <param name="calculateAmountPaidAndDueRequest">The request.</param>
            /// <returns>The service response.</returns>
            private static Response CalculateAmountPaidAndDue(CalculateAmountPaidAndDueServiceRequest calculateAmountPaidAndDueRequest)
            {
                SalesTransactionTotaler.CalculateAmountPaidAndDue(calculateAmountPaidAndDueRequest.RequestContext, calculateAmountPaidAndDueRequest.Transaction);
                return new CalculateAmountPaidAndDueServiceResponse(calculateAmountPaidAndDueRequest.Transaction);
            }
    
            private static Response CalculateDeposit(CalculateDepositServiceRequest calculateDepositRequest)
            {
                SalesTransactionTotaler.CalculateDeposit(calculateDepositRequest.RequestContext, calculateDepositRequest.Transaction);
                return new CalculateDepositServiceResponse(calculateDepositRequest.Transaction);
            }
        }
    }
}
