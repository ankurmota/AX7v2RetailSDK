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
    namespace Commerce.Runtime.Services
    {
        using System;
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;

        /// <summary>
        /// Represents an implementation of the employee operation service.
        /// </summary>
        public class EmployeeTransactionServiceDemoMode : IRequestHandler
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
                    typeof(EmployeeClockInOutRealtimeRequest),
                    typeof(RegisterEmployeeBreakRealtimeRequest),
                    typeof(GetEmployeeCurrentRegistrationStateRealtimeRequest),
                    typeof(GetEmployeeActivityHistoryRealtimeRequest),
                    typeof(GetManagerActivityHistoryRealtimeRequest),
                    typeof(GetEmployeeStoresFromAddressBookRealtimeRequest)
                };
                }
            }

            /// <summary>
            /// Executes the specified service request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                Type requestType = request.GetType();
                Response response;

                if (requestType == typeof(EmployeeClockInOutRealtimeRequest))
                {
                    response = EmployeeClockInOut();
                }
                else if (requestType == typeof(RegisterEmployeeBreakRealtimeRequest))
                {
                    response = RegisterEmployeeBreak();
                }
                else if (requestType == typeof(GetEmployeeCurrentRegistrationStateRealtimeRequest))
                {
                    response = GetEmployeeCurrentRegistrationState();
                }
                else if (requestType == typeof(GetEmployeeActivityHistoryRealtimeRequest))
                {
                    response = GetEmployeeActivityHistory();
                }
                else if (requestType == typeof(GetManagerActivityHistoryRealtimeRequest))
                {
                    response = GetManagerActivityHistory();
                }
                else if (requestType == typeof(GetEmployeeStoresFromAddressBookRealtimeRequest))
                {
                    response = GetEmployeeStoresFromAddressBook((GetEmployeeStoresFromAddressBookRealtimeRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }

                return response;
            }

            /// <summary>
            /// Executes get employee activity history requests.
            /// </summary>
            /// <returns>The activity response.</returns>
            private static EntityDataServiceResponse<EmployeeActivity> GetEmployeeActivityHistory()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "GetEmployeeActivityHistory is not supported in demo mode.");
            }

            /// <summary>
            /// Executes get manager activity history requests.
            /// </summary>
            /// <returns>The activity response.</returns>
            private static EntityDataServiceResponse<EmployeeActivity> GetManagerActivityHistory()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "GetManagerActivityHistory is not supported in demo mode.");
            }

            /// <summary>
            /// Executes get employee current registration state requests.
            /// </summary>
            /// <returns>The activity response.</returns>
            private static SingleEntityDataServiceResponse<EmployeeActivity> GetEmployeeCurrentRegistrationState()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "GetEmployeeCurrentRegistrationState is not supported in demo mode.");
            }

            /// <summary>
            /// Executes clock in / clock out requests.
            /// </summary>
            /// <returns>The clock in / clock out response.</returns>
            private static SingleEntityDataServiceResponse<DateTimeOffset> EmployeeClockInOut()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "EmployeeClockInOut is not supported in demo mode.");
            }

            /// <summary>
            /// Executes register employee break requests.
            /// </summary>
            /// <returns>The activity response.</returns>
            private static SingleEntityDataServiceResponse<DateTimeOffset> RegisterEmployeeBreak()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "RegisterEmployeeBreak is not supported in demo mode.");
            }

            /// <summary>
            /// Executes get stores by employee request.
            /// </summary>
            /// <param name="request">The service request.</param>
            /// <returns>The response containing accessible stores of this employee.</returns>
            private static EntityDataServiceResponse<OrgUnit> GetEmployeeStoresFromAddressBook(GetEmployeeStoresFromAddressBookRealtimeRequest request)
            {
                var dataRequest = new GetEmployeeStoresFromAddressBookDataRequest(request.QueryResultSettings);
                EntityDataServiceResponse<OrgUnit> response = request.RequestContext.Execute<EntityDataServiceResponse<OrgUnit>>(dataRequest);

                return response;
            }
        }
    }
}
