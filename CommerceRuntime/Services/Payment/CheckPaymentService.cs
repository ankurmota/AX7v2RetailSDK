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
        using Microsoft.Dynamics.Commerce.Runtime.Handlers;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Encapsulates tender line operations related to check payments.
        /// </summary>
        public class CheckPaymentService : IOperationRequestHandler
        {
            /// <summary>
            /// Gets a collection of operation identifiers supported by this request handler.
            /// </summary>
            public IEnumerable<int> SupportedOperationIds
            {
                get
                {
                    return new[]
                    {
                        (int)RetailOperation.PayCheck
                    };
                }
            }
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(CalculatePaymentAmountServiceRequest),
                        typeof(AuthorizePaymentServiceRequest),
                        typeof(VoidPaymentServiceRequest),
                        typeof(GetChangePaymentServiceRequest)
                    };
                }
            }
    
            /// <summary>
            /// Executes the request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Response response;
                Type requestType = request.GetType();
    
                if (requestType == typeof(CalculatePaymentAmountServiceRequest))
                {
                    response = CalculatePaymentAmount((CalculatePaymentAmountServiceRequest)request);
                }
                else if (requestType == typeof(AuthorizePaymentServiceRequest))
                {
                    response = AuthorizePayment((AuthorizePaymentServiceRequest)request);
                }
                else if (requestType == typeof(VoidPaymentServiceRequest))
                {
                    response = CancelPayment((VoidPaymentServiceRequest)request);
                }
                else if (requestType == typeof(GetChangePaymentServiceRequest))
                {
                    response = GetChange((GetChangePaymentServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Calculate amount to do be paid.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>A response containing the updated tender line.</returns>
            private static CalculatePaymentAmountServiceResponse CalculatePaymentAmount(CalculatePaymentAmountServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                return new CalculatePaymentAmountServiceResponse(request.TenderLine);
            }
    
            /// <summary>
            /// Authorizes the payment.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>A response containing the authorized tender line.</returns>
            private static AuthorizePaymentServiceResponse AuthorizePayment(AuthorizePaymentServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                request.TenderLine.Status = TenderLineStatus.Committed;
                request.TenderLine.IsVoidable = true;
    
                // For check payments, there is no extra step needed for authorizing.
                return new AuthorizePaymentServiceResponse(request.TenderLine);
            }
    
            /// <summary>
            /// Cancels the payment.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>A response containing the canceled tender line.</returns>
            private static VoidPaymentServiceResponse CancelPayment(VoidPaymentServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (request.TenderLine == null)
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPaymentRequest, "request.TenderLine is null.");
                }
    
                request.TenderLine.Status = TenderLineStatus.Voided;
                request.TenderLine.IsVoidable = false;
    
                // For check payments, there is no extra step needed for cancelling the check.
                return new VoidPaymentServiceResponse(request.TenderLine);
            }
    
            /// <summary>
            /// Gets the change.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>
            /// A response containing the change tender line.
            /// </returns>
            private static GetChangePaymentServiceResponse GetChange(GetChangePaymentServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (string.IsNullOrWhiteSpace(request.ChangeTenderTypeId))
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPaymentRequest, "request.TenderTypeId is null or empty.");
                }
    
                var changeTenderLine = new TenderLine
                {
                    Amount = decimal.Negate(request.ChangeAmount), // change tender line must have negative amount
                    Currency = request.CurrencyCode,
                    TenderLineId = Guid.NewGuid().ToString("N"),
                    TenderTypeId = request.ChangeTenderTypeId,
                    Status = TenderLineStatus.Committed,
                    IsVoidable = true,
                    IsChangeLine = true
                };
    
                return new GetChangePaymentServiceResponse(changeTenderLine);
            }
        }
    }
}
