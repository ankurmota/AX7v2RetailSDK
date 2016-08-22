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
        /// Encapsulates tender line operations related to customer account payments.
        /// </summary>
        public class CustomerPaymentService : IOperationRequestHandler
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
                        (int)RetailOperation.PayCustomerAccount
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
    
                // no calculation required.
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
    
                TenderLine tenderLine = request.TenderLine;
                if (tenderLine == null)
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPaymentRequest, "Customer account payment requires tender line.");
                }
    
                if (string.IsNullOrWhiteSpace(tenderLine.CustomerId))
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPaymentRequest, "Customer account payment requires CustomerId to be set on tender line.");
                }
    
                SalesTransaction transaction = request.Transaction;
    
                if (transaction.CartType == CartType.CustomerOrder &&
                    (transaction.CustomerOrderMode == CustomerOrderMode.CustomerOrderCreateOrEdit || transaction.CustomerOrderMode == CustomerOrderMode.Cancellation))
                {
                    throw new PaymentException(
                        PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_CustomerAccountPaymentIsNotAllowedForCustomerOrderDepositAndCancellation,
                        string.Format("Customer account payment cannot be used to pay customer order deposit or cancellation (current mode {0}).", transaction.CustomerOrderMode));
                }
    
                if (string.IsNullOrWhiteSpace(transaction.CustomerId))
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPaymentRequest, "Customer account payment requires CustomerId to be set on cart.");
                }
    
                Customer customerOnTransaction = GetCustomer(request.RequestContext, transaction.CustomerId);
    
                if (customerOnTransaction == null)
                {
                    throw new PaymentException(
                        PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPaymentRequest,
                        string.Format(CultureInfo.InvariantCulture, "Customer with id {0} was not found.", transaction.CustomerId));
                }
    
                ValidateCustomerForPayment(customerOnTransaction);
    
                bool useInvoiceAccount = !string.IsNullOrWhiteSpace(customerOnTransaction.InvoiceAccount);
                string customerAccountToCharge = useInvoiceAccount ? customerOnTransaction.InvoiceAccount : customerOnTransaction.AccountNumber;
                if (!tenderLine.CustomerId.Equals(customerAccountToCharge, StringComparison.OrdinalIgnoreCase))
                {
                    // Someone is trying to pay with unathorized account
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_PaymentUsingUnauthorizedAccount, "Customer account payment requires its own account or matching invoice account on a tender line.");
                }
    
                CheckIfPaymentExceedsBalance(request.RequestContext, customerOnTransaction, useInvoiceAccount, tenderLine);
    
                // Looks like request has successfully validated
                tenderLine.Status = TenderLineStatus.Committed;
                tenderLine.IsVoidable = true;
    
                return new AuthorizePaymentServiceResponse(tenderLine);
            }
    
            /// <summary>
            /// Validates sufficient funds on account.
            /// </summary>
            /// <param name="context">The service request context.</param>
            /// <param name="customer">The customer.</param>
            /// <param name="useInvoiceAccount">Whether to use invoice account.</param>
            /// <param name="tenderLine">The tender line.</param>
            private static void CheckIfPaymentExceedsBalance(RequestContext context, Customer customer, bool useInvoiceAccount, TenderLine tenderLine)
            {
                // Search location set to all in order to retrieve pending transactions anchor from AX
                var localBalanceServiceRequest = new GetCustomerBalanceServiceRequest(
                    customer.AccountNumber,
                    customer.InvoiceAccount,
                    SearchLocation.All);
    
                CustomerBalances pendingAccountBalances = context.Execute<GetCustomerBalanceServiceResponse>(localBalanceServiceRequest).Balance;
    
                // Total amount to verify is the sum of amount on the tender line and pending customer account balance
                // where pending balances = (tendered amounts - any deposits made) which is not yet uploaded to AX.
                decimal amountToVerify = tenderLine.Amount + (useInvoiceAccount ? pendingAccountBalances.InvoiceAccountPendingBalance : pendingAccountBalances.PendingBalance);
    
                var validateCustomerAccountPaymentRealtimeRequest = new ValidateCustomerAccountPaymentRealtimeRequest(tenderLine.CustomerId, amountToVerify, tenderLine.Currency);
                context.Execute<NullResponse>(validateCustomerAccountPaymentRealtimeRequest);
            }
    
            /// <summary>
            /// Loads customer.
            /// </summary>
            /// <param name="context">The service request context.</param>
            /// <param name="accountNumber">The account number.</param>
            /// <returns>The customer, or null if not found.</returns>
            private static Customer GetCustomer(RequestContext context, string accountNumber)
            {
                var getCustomerDataRequest = new GetCustomerDataRequest(accountNumber);
                SingleEntityDataServiceResponse<Customer> getCustomerDataResponse = context.Execute<SingleEntityDataServiceResponse<Customer>>(getCustomerDataRequest);
                Customer customer = getCustomerDataResponse.Entity;
                return customer;
            }
    
            /// <summary>
            ///  Validation of customer payment attributes.
            /// </summary>
            /// <param name="customerToPayWith">The customer account to pay with.</param>
            private static void ValidateCustomerForPayment(Customer customerToPayWith)
            {
                // Account shall be chargeable when paying with own account
                if (customerToPayWith.NonChargeableAccount && string.IsNullOrWhiteSpace(customerToPayWith.InvoiceAccount))
                {
                    throw new PaymentException(
                        PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPaymentRequest,
                        string.Format(CultureInfo.InvariantCulture, "Customer with id {0} is non-chargeable account.", customerToPayWith.AccountNumber));
                }
    
                if (customerToPayWith.Blocked)
                {
                    throw new PaymentException(
                        PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPaymentRequest,
                        string.Format(CultureInfo.InvariantCulture, "Customer with id {0} is blocked.", customerToPayWith.AccountNumber));
                }
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
    
                // For customer account payments, there is no extra step needed.
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
                // Change cannot be given to customer account.
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_ChangeTenderTypeNotSupported, string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported. Verify change tender type settings.", request.GetType()));
            }
        }
    }
}
