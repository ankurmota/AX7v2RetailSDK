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
        using Microsoft.Dynamics.Commerce.Runtime.Handlers;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Encapsulates service that implements gift card operations.
        /// </summary>
        public class CreditMemoService : IOperationRequestHandler
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
                        (int)RetailOperation.PayCreditMemo,
                        (int)RetailOperation.IssueCreditMemo
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
                        typeof(CapturePaymentServiceRequest),
                        typeof(GetChangePaymentServiceRequest),
                        typeof(GetCreditMemoServiceRequest)
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
                else if (requestType == typeof(CapturePaymentServiceRequest))
                {
                    response = CapturePayment((CapturePaymentServiceRequest)request);
                }
                else if (requestType == typeof(GetCreditMemoServiceRequest))
                {
                    response = GetCreditMemo((GetCreditMemoServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
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
    
                if (request.TenderLine.Amount > 0)
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPaymentRequest, "Amount cannot be specified when paying with credit memo. Only full credit memo amount can be used by setting amount to zero.");
                }
    
                // for refunds amount is provided by client and doesn't have to be calculated
                if (request.TenderLine.Amount == 0)
                {
                    CreditMemo creditMemo = GetCreditMemo(request.RequestContext, request.TenderLine.CreditMemoId);
                    request.TenderLine.Amount = creditMemo.Balance; // set tender line amount to credit memo
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
    
                if (request.TenderLine.Amount > 0)
                {
                    // Positive amount indicates credit memo payment.
                    // Payment always uses full amount. Actual amount will be returned as part of tender line on response.
                    string orgUnitNumber = request.RequestContext.GetOrgUnit().OrgUnitNumber;
    
                    var lockRequest = new LockCreditMemoRealtimeRequest(
                        request.TenderLine.CreditMemoId,
                        orgUnitNumber,
                        request.RequestContext.GetTerminal().TerminalId);
                    CreditMemo creditMemo = request.RequestContext.Execute<SingleEntityDataServiceResponse<CreditMemo>>(lockRequest).Entity;
    
                    request.TenderLine.Amount = creditMemo.Balance;
                    request.TenderLine.Currency = creditMemo.CurrencyCode;
                    request.TenderLine.Status = TenderLineStatus.PendingCommit;
                    request.TenderLine.IsVoidable = true;
                }
                else
                {
                    // For negative amount issue new credit memo
                    if (!string.IsNullOrWhiteSpace(request.TenderLine.CreditMemoId))
                    {
                        throw new PaymentException(
                            PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPaymentRequest,
                            "Refund to existing credit memo is not allowed. To issue credit memo identifier should be left blank.");
                    }
    
                    string creditMemoId = IssueCreditMemo(request.RequestContext, decimal.Negate(request.TenderLine.Amount), request.TenderLine.Currency, request.Transaction.Id, request.Transaction.ReceiptId);
    
                    request.TenderLine.CreditMemoId = creditMemoId;
                    request.TenderLine.Status = TenderLineStatus.Committed;
                    request.TenderLine.IsVoidable = false;
                }
    
                return new AuthorizePaymentServiceResponse(request.TenderLine);
            }
    
            /// <summary>
            /// Captures the payment.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>A response containing the captured tender line.</returns>
            private static CapturePaymentServiceResponse CapturePayment(CapturePaymentServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (request.TenderLine.Amount <= 0)
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPaymentRequest, "Only positive amount can be paid by credit memo.");
                }
    
                if (string.IsNullOrWhiteSpace(request.TenderLine.CreditMemoId))
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPaymentRequest, "Credit memo identifier is not specified.");
                }
    
                var payCreditMemoRealtimeRequest = new PayCreditMemoRealtimeRequest(
                    request.TenderLine.CreditMemoId,
                    request.RequestContext.GetPrincipal().ChannelId.ToString(CultureInfo.InvariantCulture),
                    request.RequestContext.GetPrincipal().TerminalId.ToString(CultureInfo.InvariantCulture),
                    request.RequestContext.GetPrincipal().UserId,
                    request.Transaction.Id,
                    request.Transaction.ReceiptId,
                    request.TenderLine.Currency,
                    request.TenderLine.Amount);
                request.RequestContext.Execute<NullResponse>(payCreditMemoRealtimeRequest);
    
                request.TenderLine.Status = TenderLineStatus.Committed;
                request.TenderLine.IsVoidable = false;
    
                return new CapturePaymentServiceResponse(request.TenderLine);
            }
    
            /// <summary>
            /// Get the credit memo information.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>A response containing the credit memo.</returns>
            private static GetCreditMemoServiceResponse GetCreditMemo(GetCreditMemoServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                var creditMemo = GetCreditMemo(request.RequestContext, request.CreditMemoId);
    
                return new GetCreditMemoServiceResponse(creditMemo);
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
    
                if (request.TenderLine.Amount <= 0)
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPaymentRequest, "Issued credit memo cannot be voided.");
                }
    
                string orgUnitNumber = request.RequestContext.GetOrgUnit().OrgUnitNumber;
    
                var unlockCreditMemoRealtimeRequest = new UnlockCreditMemoRealtimeRequest(
                    request.TenderLine.CreditMemoId,
                    orgUnitNumber,
                    request.RequestContext.GetTerminal().TerminalId);
                request.RequestContext.Execute<NullResponse>(unlockCreditMemoRealtimeRequest);
    
                request.TenderLine.Status = TenderLineStatus.Voided;
                request.TenderLine.IsVoidable = false;
    
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
    
                if (request.ChangeTenderTypeId == null)
                {
                    throw new ArgumentException("request.TenderType is null", "request");
                }
    
                string creditMemoId = IssueCreditMemo(request.RequestContext, request.ChangeAmount, request.CurrencyCode, request.Transaction.Id, request.Transaction.ReceiptId);
    
                var changeTenderLine = new TenderLine
                {
                    CreditMemoId = creditMemoId,
                    Amount = decimal.Negate(request.ChangeAmount), // change tender line must have negative amount
                    Currency = request.CurrencyCode,
                    TenderLineId = Guid.NewGuid().ToString("N"),
                    TenderTypeId = request.ChangeTenderTypeId,
                    Status = TenderLineStatus.Committed,
                    IsVoidable = false,
                    IsChangeLine = true
                };
    
                return new GetChangePaymentServiceResponse(changeTenderLine);
            }
    
            /// <summary>
            /// Call CDX real time service to issue credit memo.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <param name="value">Value of credit memo to issue.</param>
            /// <param name="currencyCode">Currency code of credit memo.</param>
            /// <param name="transactionId">Transaction identifier.</param>
            /// <param name="receiptId">Receipt identifier.</param>
            /// <returns>Credit memo identifier.</returns>
            private static string IssueCreditMemo(RequestContext context, decimal value, string currencyCode, string transactionId, string receiptId)
            {
                string orgUnitNumber = context.GetOrgUnit().OrgUnitNumber;
    
                var issueCreditMemoRealtimeRequest = new IssueCreditMemoRealtimeRequest(
                    orgUnitNumber,
                    context.GetTerminal().TerminalId,
                    context.GetPrincipal().UserId,
                    transactionId,
                    receiptId ?? string.Empty,
                    currencyCode,
                    value);
    
                string creditMemoId = context.Execute<SingleEntityDataServiceResponse<string>>(issueCreditMemoRealtimeRequest).Entity;
    
                return creditMemoId;
            }
    
            /// <summary>
            /// Call CDX real time service to get credit memo by identifier.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <param name="id">Credit memo identifier.</param>
            /// <returns>Credit memo information.</returns>
            private static CreditMemo GetCreditMemo(RequestContext context, string id)
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new ArgumentException("Credit memo identifier is null or empty.", "id");
                }
    
                var getCreditMemoRealtimeRequest = new GetCreditMemoRealtimeRequest(id);
                CreditMemo creditMemo = context.Execute<SingleEntityDataServiceResponse<CreditMemo>>(getCreditMemoRealtimeRequest).Entity;
    
                return creditMemo;
            }
        }
    }
}
