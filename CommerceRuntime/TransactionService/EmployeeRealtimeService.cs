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
        using System.Linq;
        using Commerce.Runtime.TransactionService;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;

        /// <summary>
        /// Represents an implementation of the employee operation service.
        /// </summary>
        public class EmployeeRealtimeService : IRequestHandler
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
                    response = EmployeeClockInOut((EmployeeClockInOutRealtimeRequest)request);
                }
                else if (requestType == typeof(RegisterEmployeeBreakRealtimeRequest))
                {
                    response = RegisterEmployeeBreak((RegisterEmployeeBreakRealtimeRequest)request);
                }
                else if (requestType == typeof(GetEmployeeCurrentRegistrationStateRealtimeRequest))
                {
                    response = GetEmployeeCurrentRegistrationState((GetEmployeeCurrentRegistrationStateRealtimeRequest)request);
                }
                else if (requestType == typeof(GetEmployeeActivityHistoryRealtimeRequest))
                {
                    response = GetEmployeeActivityHistory((GetEmployeeActivityHistoryRealtimeRequest)request);
                }
                else if (requestType == typeof(GetManagerActivityHistoryRealtimeRequest))
                {
                    response = GetManagerActivityHistory((GetManagerActivityHistoryRealtimeRequest)request);
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
            /// <param name="request">The service request.</param>
            /// <returns>The activity response.</returns>
            private static EntityDataServiceResponse<EmployeeActivity> GetEmployeeActivityHistory(GetEmployeeActivityHistoryRealtimeRequest request)
            {
                var transactionService = new TransactionServiceClient(request.RequestContext);

                var activities = transactionService.GetEmployeeActivityHistory(
                    request.UserId,
                    request.StoreNumber,
                    request.FromUtcDateTime,
                    request.ToUtcDateTime,
                    request.PagingInfo,
                    request.SortingInfo);

                // Convert the UTC date time offset field of each employee activity to channel time zone
                foreach (EmployeeActivity activity in activities.Results.Where(activity => activity.ActivityDateTimeOffset.HasValue))
                {
                    activity.ActivityDateTimeOffset = request.RequestContext.ConvertDateTimeToChannelDate(activity.ActivityDateTimeOffset.Value);
                }

                return new EntityDataServiceResponse<EmployeeActivity>(activities);
            }

            /// <summary>
            /// Executes get manager activity history requests.
            /// </summary>
            /// <param name="request">The service request.</param>
            /// <returns>The activity response.</returns>
            private static EntityDataServiceResponse<EmployeeActivity> GetManagerActivityHistory(GetManagerActivityHistoryRealtimeRequest request)
            {
                var transactionService = new TransactionServiceClient(request.RequestContext);

                var activities = transactionService.GetManagerActivityHistory(
                    request.StoreIds,
                    request.EmployeeActivityTypes,
                    request.BreakActivities,
                    request.FromUtcDateTime,
                    request.ToUtcDateTime,
                    request.PagingInfo,
                    request.SortingInfo);

                // Convert the UTC date time offset field of each employee activity to channel time zone
                foreach (EmployeeActivity activity in activities.Results.Where(activity => activity.ActivityDateTimeOffset.HasValue))
                {
                    activity.ActivityDateTimeOffset = request.RequestContext.ConvertDateTimeToChannelDate(activity.ActivityDateTimeOffset.Value);
                }

                return new EntityDataServiceResponse<EmployeeActivity>(activities);
            }

            /// <summary>
            /// Executes get employee current registration state requests.
            /// </summary>
            /// <param name="request">The service request.</param>
            /// <returns>The activity response.</returns>
            private static SingleEntityDataServiceResponse<EmployeeActivity> GetEmployeeCurrentRegistrationState(GetEmployeeCurrentRegistrationStateRealtimeRequest request)
            {
                var transactionService = new TransactionServiceClient(request.RequestContext);

                var activity = transactionService.GetEmployeeCurrentRegistrationState(request.UserId, request.TerminalId);

                // Convert the UTC date time offset of employee activity to channel date time offset
                if (activity.ActivityDateTimeOffset.HasValue)
                {
                    activity.ActivityDateTimeOffset = request.RequestContext.ConvertDateTimeToChannelDate(activity.ActivityDateTimeOffset.Value);
                }

                return new SingleEntityDataServiceResponse<EmployeeActivity>(activity);
            }

            /// <summary>
            /// Executes clock in / clock out requests.
            /// </summary>
            /// <param name="request">The service request.</param>
            /// <returns>The clock in / clock out response.</returns>
            private static SingleEntityDataServiceResponse<DateTimeOffset> EmployeeClockInOut(EmployeeClockInOutRealtimeRequest request)
            {
                var transactionService = new TransactionServiceClient(request.RequestContext);

                string methodName = request.IsClockIn
                                    ? TransactionServiceClient.ClockInMethodName
                                    : TransactionServiceClient.ClockOutMethodName;

                DateTimeOffset? currentActivityDateTimeOffset = transactionService.RegisterPunchInOut(request.UserId, request.TerminalId, methodName);

                // Convert the UTC date time offset to channel date time offset
                if (currentActivityDateTimeOffset.HasValue)
                {
                    currentActivityDateTimeOffset = request.RequestContext.ConvertDateTimeToChannelDate(currentActivityDateTimeOffset.Value);
                }

                return new SingleEntityDataServiceResponse<DateTimeOffset>(currentActivityDateTimeOffset.GetValueOrDefault());
            }

            /// <summary>
            /// Executes register employee break requests.
            /// </summary>
            /// <param name="request">The service request.</param>
            /// <returns>The activity response.</returns>
            private static SingleEntityDataServiceResponse<DateTimeOffset> RegisterEmployeeBreak(RegisterEmployeeBreakRealtimeRequest request)
            {
                var transactionService = new TransactionServiceClient(request.RequestContext);

                DateTimeOffset? currentActivityDateTimeOffset = transactionService.RegisterStartBreak(request.UserId, request.TerminalId, request.JobId);

                // Convert the UTC date time offset to channel date time offset
                if (currentActivityDateTimeOffset.HasValue)
                {
                    currentActivityDateTimeOffset = request.RequestContext.ConvertDateTimeToChannelDate(currentActivityDateTimeOffset.Value);
                }

                return new SingleEntityDataServiceResponse<DateTimeOffset>(currentActivityDateTimeOffset.GetValueOrDefault());
            }

            /// <summary>
            /// Executes get stores by employee request.
            /// </summary>
            /// <param name="request">The service request.</param>
            /// <returns>The response containing accessible stores of this employee.</returns>
            private static EntityDataServiceResponse<OrgUnit> GetEmployeeStoresFromAddressBook(GetEmployeeStoresFromAddressBookRealtimeRequest request)
            {
                var transactionService = new TransactionServiceClient(request.RequestContext);
                PagedResult<OrgUnit> orgUnits = transactionService.GetEmployeeStoresFromAddressBook(request.RequestContext.GetPrincipal().UserId, request.QueryResultSettings);
                return new EntityDataServiceResponse<OrgUnit>(orgUnits);
            }
        }
    }
}
