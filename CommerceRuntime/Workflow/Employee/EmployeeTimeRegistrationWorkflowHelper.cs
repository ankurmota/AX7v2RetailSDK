/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

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
    namespace Commerce.Runtime.Workflow
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Helpers;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// Workflow helper class for employee time registration.
        /// </summary>
        public static class EmployeeTimeRegistrationWorkflowHelper
        {
            /// <summary>
            /// Constant to break for lunch defined in AX.
            /// </summary>
            public const string BreakForLunch = "LunchBrk";

            /// <summary>
            /// Constant to break for work defined in AX.
            /// </summary>
            public const string BreakFromWork = "DailyBrks";

            /// <summary>
            /// Gets the latest activity of the employee.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>The latest employee activity.</returns>
            public static EmployeeActivity GetLatestActivity(RequestContext context)
            {
                ThrowIf.Null(context, "context");

                var request = new GetEmployeeCurrentRegistrationStateRealtimeRequest(
                    context.GetPrincipal().UserId,
                    context.GetTerminal().TerminalId);

                return context.Execute<SingleEntityDataServiceResponse<EmployeeActivity>>(request).Entity;
            }

            /// <summary>
            /// Registers the employee clock in.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>Returns the activity DateTimeOffset in channel local time zone.</returns>
            public static DateTimeOffset RegisterClockIn(RequestContext context)
            {
                return RegisterClockInAndOut(context, true);
            }

            /// <summary>
            /// Registers the employee clock out.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>Returns the activity DateTimeOffset in channel local time zone.</returns>
            public static DateTimeOffset RegisterClockOut(RequestContext context)
            {
                return RegisterClockInAndOut(context, false);
            }

            /// <summary>
            /// Register the time for employee break.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="jobId">The job identifier.</param>
            /// <returns>Returns the activity DateTimeOffset in channel local time zone.</returns>
            public static DateTimeOffset RegisterEmployeeBreak(RequestContext context, string jobId)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.NullOrWhiteSpace(jobId, "jobId");

                var request = new RegisterEmployeeBreakRealtimeRequest(
                    context.GetPrincipal().UserId,
                    context.GetTerminal().TerminalId,
                    jobId);

                return context.Execute<SingleEntityDataServiceResponse<DateTimeOffset>>(request).Entity;
            }

            /// <summary>
            /// Gets the break category name based on given job identifier.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="jobId">The job identifier.</param>
            /// <returns>Returns the break category.</returns>
            public static string GetBreakCategoryByJob(RequestContext context, string jobId)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.NullOrWhiteSpace(jobId, "jobId");

                GetEmployeeBreakCategoriesByJobDataRequest dataRequest = new GetEmployeeBreakCategoriesByJobDataRequest(jobId);
                ReadOnlyCollection<EmployeeActivity> breakCategories = context.Execute<EntityDataServiceResponse<EmployeeActivity>>(dataRequest).PagedEntityCollection.Results;

                if (!breakCategories.Any())
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToFindEmployeeActivityBreakCategory,
                        string.Format("Cannot find the break category for job Id: {0}", jobId));
                }

                return breakCategories.SingleOrDefault().BreakCategory;
            }

            /// <summary>
            /// Gets the break category job identifier for the given activity.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="activity">The activity: <c>LunchBrk</c>, <c>DailyBrks</c>.</param>
            /// <returns>Returns the activity job identifier.</returns>
            public static string GetBreakCategoryJobIdByActivity(RequestContext context, string activity)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.NullOrWhiteSpace(activity, "activity");

                var jobIds = GetBreakCategoryJobIdsByActivities(context, new[] { activity });

                return jobIds.SingleOrDefault();
            }

            /// <summary>
            /// Gets the break category job identifiers for the given activities.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="activities">The activity list: <c>LunchBrk</c>, <c>DailyBrks</c>.</param>
            /// <returns>Returns the activity job identifiers list.</returns>
            public static string[] GetBreakCategoryJobIdsByActivities(RequestContext context, IEnumerable<string> activities)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(activities, "activities");

                GetEmployeeBreakCategoriesByActivityDataRequest dataRequest = new GetEmployeeBreakCategoriesByActivityDataRequest(activities);
                var breakCategories = context.Execute<EntityDataServiceResponse<EmployeeActivity>>(dataRequest).PagedEntityCollection.Results;

                if (!breakCategories.Any())
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToFindEmployeeActivityBreakCategory, "Cannot find the break category specified");
                }

                var jobIds = from activity in breakCategories
                             select activity.JobId;

                return jobIds.ToArray();
            }

            /// <summary>
            /// Gets the employee log book details.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="storeNumber">The store number.</param>
            /// <param name="fromDateTimeOffset">The employee activity date and time offset lower bound.</param>
            /// <param name="toDateTimeOffset">The employee activity date and time offset upper bound.</param>
            /// <param name="pagingInfo">The paging information.</param>
            /// <param name="sortingInfo">The sorting information.</param>
            /// <returns>The results collection of employee activities.</returns>
            public static ReadOnlyCollection<EmployeeActivity> GetEmployeeLogbookDetails(
                RequestContext context,
                string storeNumber,
                DateTimeOffset? fromDateTimeOffset,
                DateTimeOffset? toDateTimeOffset,
                PagingInfo pagingInfo,
                SortingInfo sortingInfo)
            {
                ThrowIf.Null(context, "context");

                // Get the UTC date and time from the channel date and time offset
                var fromUtcDateTime = fromDateTimeOffset.GetUtcDateTime();
                var toUtcDateTime = toDateTimeOffset.GetUtcDateTime();

                // Get the store numbers the user is working at
                var storeIds = GetEmployeeStoreIds(context);
                bool storeNumberUndefined = string.IsNullOrWhiteSpace(storeNumber);

                if ((storeIds == null)
                    || (!storeNumberUndefined && !storeIds.Contains(storeNumber)))
                {
                    // if no store the user is assigned to, nor the input storeNumber is among the store(s) the user is assigned to
                    return null;
                }
                else if (storeNumberUndefined)
                {
                    // if store number is undefined, use the one in the context
                    storeNumber = context.GetOrgUnit().OrgUnitNumber;
                }

                var request = new GetEmployeeActivityHistoryRealtimeRequest(
                    context.GetPrincipal().UserId,
                    storeNumber,
                    fromUtcDateTime,
                    toUtcDateTime,
                    pagingInfo,
                    sortingInfo);

                return context.Execute<EntityDataServiceResponse<EmployeeActivity>>(request).PagedEntityCollection.Results;
            }

            /// <summary>
            /// Gets the manager log book view for all employees for the given store and filter by activity.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <param name="storeNumber">Store number.</param>
            /// <param name="employeeActivityTypes">Employee activity types.</param>
            /// <param name="fromDateTimeOffset">The employee activity date and time offset lower bound.</param>
            /// <param name="toDateTimeOffset">The employee activity date and time offset upper bound.</param>
            /// <param name="pagingInfo">The paging information.</param>
            /// <param name="sortingInfo">The sorting information.</param>
            /// <returns>The results collection of employee activities.</returns>
            public static ReadOnlyCollection<EmployeeActivity> GetManagerLogbookView(
                RequestContext context,
                string storeNumber,
                EmployeeActivityType[] employeeActivityTypes,
                DateTimeOffset? fromDateTimeOffset,
                DateTimeOffset? toDateTimeOffset,
                PagingInfo pagingInfo,
                SortingInfo sortingInfo)
            {
                ThrowIf.Null(context, "context");

                // Get the UTC date and time from the channel date and time offset
                var fromUtcDateTime = fromDateTimeOffset.GetUtcDateTime();
                var toUtcDateTime = toDateTimeOffset.GetUtcDateTime();

                // Get the store numbers the user is working at
                var storeIds = GetEmployeeStoreIds(context);
                bool storeNumberDefined = !string.IsNullOrWhiteSpace(storeNumber);

                if ((storeIds == null)
                    || (storeNumberDefined && !storeIds.Contains(storeNumber)))
                {
                    // if no store the user is assigned to, nor the input storeNumber is among the store(s) the user is assigned to
                    return null;
                }
                else if (storeNumberDefined)
                {
                    // if the input store number is defined, use it directly
                    storeIds = new[] { storeNumber };
                }

                // Get the break activity categories
                var breakActivities = GetBreakActivityJobIds(context, employeeActivityTypes);

                var request = new GetManagerActivityHistoryRealtimeRequest(
                    storeIds,
                    employeeActivityTypes,
                    breakActivities,
                    fromUtcDateTime,
                    toUtcDateTime,
                    pagingInfo,
                    sortingInfo);

                return context.Execute<EntityDataServiceResponse<EmployeeActivity>>(request).PagedEntityCollection.Results;
            }

            /// <summary>
            /// Gets the employee registered stores.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="resultSettings">The query result criteria.</param>
            /// <returns>Returns the collection of store.</returns>
            public static ReadOnlyCollection<OrgUnit> GetEmployeeRegisteredStores(RequestContext context, QueryResultSettings resultSettings)
            {
                ThrowIf.Null(context, "context");

                var request = new GetStoresByEmployeeServiceRequest
                {
                    QueryResultSettings = resultSettings
                };

                ReadOnlyCollection<OrgUnit> stores = context.Execute<EntityDataServiceResponse<OrgUnit>>(request).PagedEntityCollection.Results;

                return stores;
            }

            /// <summary>
            /// Gets the employee registered stores.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>Returns boolean value of time clock enabled.</returns>
            public static bool ValidateTimeRegistrationFunctionalityProfile(RequestContext context)
            {
                ThrowIf.Null(context, "context");

                return context.GetDeviceConfiguration().EnableTimeRegistration;
            }

            /// <summary>
            /// Registers the clock in/ clock out request from the employee.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="isClockIn">Whether this is a clockIn request (true) or clockOut (false).</param>
            /// <returns>Returns the activity DateTimeOffset in channel local time zone.</returns>
            private static DateTimeOffset RegisterClockInAndOut(RequestContext context, bool isClockIn)
            {
                ThrowIf.Null(context, "context");

                var request = new EmployeeClockInOutRealtimeRequest(
                    context.GetPrincipal().UserId,
                    context.GetTerminal().TerminalId,
                    isClockIn);

                return context.Execute<SingleEntityDataServiceResponse<DateTimeOffset>>(request).Entity;
            }

            /// <summary>
            /// Gets the break activities job Ids from the employee activity type list.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="employeeActivityTypes">The employee activity types.</param>
            /// <returns>The list of break activities job Ids.</returns>
            private static string[] GetBreakActivityJobIds(RequestContext context, EmployeeActivityType[] employeeActivityTypes)
            {
                ThrowIf.Null(context, "context");

                var breakCategories = new List<string>();
                string[] breakActivities = null;

                if (employeeActivityTypes != null)
                {
                    for (int i = 0; i != employeeActivityTypes.Length; i++)
                    {
                        switch (employeeActivityTypes[i])
                        {
                            case EmployeeActivityType.BreakFromWork:
                                breakCategories.Add(BreakFromWork);
                                employeeActivityTypes[i] = EmployeeActivityType.BreakFlowStart;
                                break;
                            case EmployeeActivityType.BreakForLunch:
                                breakCategories.Add(BreakForLunch);
                                employeeActivityTypes[i] = EmployeeActivityType.BreakFlowStart;
                                break;
                        }
                    }

                    if (breakCategories.Count > 0)
                    {
                        breakActivities = GetBreakCategoryJobIdsByActivities(context, breakCategories);
                    }
                }

                return breakActivities;
            }

            /// <summary>
            /// Get all store numbers of which the employee works for.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>The store numbers of which the user works for.</returns>
            private static string[] GetEmployeeStoreIds(RequestContext context)
            {
                ThrowIf.Null(context, "context");

                var stores = GetEmployeeRegisteredStores(context, QueryResultSettings.AllRecords);

                if (stores != null)
                {
                    IEnumerable<string> storeIds = from store in stores
                               select store.OrgUnitNumber;
                    return storeIds.ToArray();
                }

                return null;
            }
        }
    }
}
