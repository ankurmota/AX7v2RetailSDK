/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1403:FileMayOnlyContainASingleNamespace", Justification = "This file requires multiple namespaces to support the Retail Sdk code generation.")]

namespace Contoso
{
    namespace Commerce.Runtime.TransactionService
    {
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
    
        /// <summary>
        /// The customer demo mode transaction service implementation.
        /// </summary>
        public class CustomerTransactionServiceDemoMode : IRequestHandler
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
                        typeof(GetCustomerBalanceRealtimeRequest),
                        typeof(ValidateCustomerAccountPaymentRealtimeRequest),
                        typeof(SearchCustomersRealtimeRequest),
                        typeof(DownloadCustomerRealtimeRequest),
                        typeof(DownloadPartyRealtimeRequest),
                        typeof(NewCustomerRealtimeRequest),
                        typeof(NewCustomerFromDirectoryPartyRealtimeRequest),
                        typeof(SaveCustomerRealtimeRequest),
                        typeof(DeactivateAddressRealtimeRequest),
                        typeof(CreateAddressRealtimeRequest),
                        typeof(UpdateAddressRealtimeRequest),
                        typeof(SendEmailRealtimeRequest),
                        typeof(GetOrderHistoryRealtimeRequest),
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
                if (requestType == typeof(GetCustomerBalanceRealtimeRequest))
                {
                    response = GetCustomerBalance();
                }
                else if (requestType == typeof(ValidateCustomerAccountPaymentRealtimeRequest))
                {
                    response = ValidateCustomerAccountPayment();
                }
                else if (requestType == typeof(SearchCustomersRealtimeRequest))
                {
                    response = SearchCustomers();
                }
                else if (requestType == typeof(DownloadCustomerRealtimeRequest))
                {
                    response = DownloadCustomer();
                }
                else if (requestType == typeof(DownloadPartyRealtimeRequest))
                {
                    response = DownloadParty();
                }
                else if (requestType == typeof(NewCustomerRealtimeRequest))
                {
                    response = NewCustomer();
                }
                else if (requestType == typeof(NewCustomerFromDirectoryPartyRealtimeRequest))
                {
                    response = NewCustomerFromDirParty();
                }
                else if (requestType == typeof(SaveCustomerRealtimeRequest))
                {
                    response = UpdateCustomer();
                }
                else if (requestType == typeof(DeactivateAddressRealtimeRequest))
                {
                    response = DeactivateAddress();
                }
                else if (requestType == typeof(CreateAddressRealtimeRequest))
                {
                    response = CreateAddress();
                }
                else if (requestType == typeof(UpdateAddressRealtimeRequest))
                {
                    response = UpdateAddress();
                }
                else if (requestType == typeof(SendEmailRealtimeRequest))
                {
                    response = SendEmailToCustomer();
                }
                else if (requestType == typeof(GetOrderHistoryRealtimeRequest))
                {
                    response = GetOrderHistory((GetOrderHistoryRealtimeRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request type '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Gets the customer balance from AX.
            /// </summary>
            /// <returns>The service response.</returns>
            private static GetCustomerBalanceRealtimeResponse GetCustomerBalance()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "GetCustomerBalance is not supported in demo mode.");
            }
    
            /// <summary>
            /// Validates Customer Account Payment from AX.
            /// </summary>
            /// <returns>The service response.</returns>
            private static NullResponse ValidateCustomerAccountPayment()
            {
                return new NullResponse();
            }

            /// <summary>
            /// Perform a keyword search in AX for customers.
            /// Throws a not supported exception in DemoMode, as there is no connection to AX.
            /// </summary>
            /// <returns>A service response containing the query results.</returns>
            private static EntityDataServiceResponse<GlobalCustomer> SearchCustomers()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "SearchCustomers (in AX) is not supported in demo mode.");
            }

            /// <summary>
            /// Find and retrieve a customer customer from AX and add them to the DB.
            /// Does nothing in DemoMode, as there is no connection to AX.
            /// </summary>
            /// <returns>An empty response.</returns>
            private static NullResponse DownloadCustomer()
            {
                return new NullResponse();
            }

            /// <summary>
            /// Find and retrieve a customer customer from AX and add them to the DB.
            /// Does nothing in DemoMode, as there is no connection to AX.
            /// </summary>
            /// <returns>An empty response.</returns>
            private static NullResponse DownloadParty()
            {
                return new NullResponse();
            }

            /// <summary>
            /// Creates a new customer in AX.
            /// </summary>
            /// <returns>The service response.</returns>
            private static SaveCustomerRealtimeResponse NewCustomer()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "NewCustomer is not supported in demo mode.");
            }
    
            /// <summary>
            /// Creates a new customer from directory in AX.
            /// </summary>
            /// <returns>The service response.</returns>
            private static SaveCustomerRealtimeResponse NewCustomerFromDirParty()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "NewCustomerFromDirParty is not supported in demo mode.");
            }
    
            /// <summary>
            /// Updates a customer in AX.
            /// </summary>
            /// <returns>The service response.</returns>
            private static SaveCustomerRealtimeResponse UpdateCustomer()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "UpdateCustomer is not supported in demo mode.");
            }
    
            /// <summary>
            /// Deactivates an address in AX.
            /// </summary>
            /// <returns>The service response.</returns>
            private static NullResponse DeactivateAddress()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "DeactivateAddress is not supported in demo mode.");
            }
    
            /// <summary>
            /// Creates a new address in AX.
            /// </summary>
            /// <returns>The service response.</returns>
            private static CreateAddressRealtimeResponse CreateAddress()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "CreateAddress is not supported in demo mode.");
            }
    
            /// <summary>
            /// Updates an address in AX.
            /// </summary>
            /// <returns>The service response.</returns>
            private static UpdateAddressRealtimeResponse UpdateAddress()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "UpdateAddress is not supported in demo mode.");
            }
    
            /// <summary>
            /// Sends an email to the requested customer using the email template defined in AX.
            /// </summary>
            /// <returns>SendCustomerEmailServiceResponse object.</returns>
            private static NullResponse SendEmailToCustomer()
            {
                return new NullResponse();
            }
    
            private static GetOrderHistoryRealtimeResponse GetOrderHistory(GetOrderHistoryRealtimeRequest request)
            {
                ThrowIf.Null(request, "request");
    
                var remoteOrders = new List<SalesOrder>();
                return new GetOrderHistoryRealtimeResponse(remoteOrders.AsPagedResult());
            }
        }
    }
}
