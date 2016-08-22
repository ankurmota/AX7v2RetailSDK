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
    namespace Commerce.Runtime.TransactionService
    {
        using System;
        using System.Collections;
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Helpers;
    
        /// <summary>
        /// Transaction Service Commerce Runtime Client API.
        /// </summary>
        public sealed partial class TransactionServiceClient
        {
            /// <summary>
            /// Clock in method name of transaction service.
            /// </summary>
            public const string ClockInMethodName = "ClockIn";
    
            /// <summary>
            /// Clock out method name of transaction service.
            /// </summary>
            public const string ClockOutMethodName = "ClockOut";
    
            // Transaction service method names.
            private const string GetWorkerHistoryMethodName = "GetWorkerHistoryUtc";
            private const string GetManagerHistoryMethodName = "GetManagerHistoryUtc";
            private const string GetWorkerCurrentTimeRegistrationStateMethodName = "GetWorkerCurrentTimeRegistrationState";
            private const string StartBreakMethodName = "StartBreak";
    
            /// <summary>
            /// Call this method to register clock in/ out for the employee.
            /// </summary>
            /// <param name="staffId">Enter the staff identifier.</param>
            /// <param name="terminalId">Enter the terminal identifier.</param>
            /// <param name="methodName">Enter the TX service method name "clockIn"/ "clockOut".</param>
            /// <returns>Returns the current clock in / clock out date time offset.</returns>
            public DateTimeOffset? RegisterPunchInOut(string staffId, string terminalId, string methodName)
            {
                ThrowIf.NullOrWhiteSpace(staffId, "staffId");
                ThrowIf.NullOrWhiteSpace(terminalId, "terminalId");
                ThrowIf.NullOrWhiteSpace(methodName, "methodName");
    
                var data = this.InvokeMethod(methodName, staffId, terminalId);
    
                // Parse response data
                DateTime? activityDateTime = Convert.ToDateTime(data[0].ToString());
    
                return activityDateTime.ToUtcDateTimeOffset();
            }
    
            /// <summary>
            /// Gets employee current registration state.
            /// </summary>
            /// <param name="staffId">Enter the staff identifier.</param>
            /// <param name="terminalId">Enter the terminal identifier.</param>
            /// <returns>Returns the latest activity performed by employee.</returns>
            public EmployeeActivity GetEmployeeCurrentRegistrationState(string staffId, string terminalId)
            {
                ThrowIf.NullOrWhiteSpace(staffId, "staffId");
                ThrowIf.NullOrWhiteSpace(terminalId, "terminalId");
    
                var data = this.InvokeMethod(GetWorkerCurrentTimeRegistrationStateMethodName, staffId, terminalId);
    
                var employeeActivity = new EmployeeActivity();
    
                // Parse response data
                DateTime? activityDateTime = Convert.ToDateTime(data[0].ToString());
    
                employeeActivity.ActivityDateTimeOffset = activityDateTime.ToUtcDateTimeOffset();
                employeeActivity.EmployeeActivityType = (EmployeeActivityType)data[1];
                employeeActivity.Activity = ((EmployeeActivityType)data[1]).ToString();
                employeeActivity.JobId = (string)data[2];
    
                return employeeActivity;
            }
    
            /// <summary>
            /// Register for break time (BreakFromWork / BreakForLunch).
            /// </summary>
            /// <param name="staffId">Enter the staff identifier.</param>
            /// <param name="terminalId">Enter the terminal identifier.</param>
            /// <param name="jobId">Enter the job identifier.</param>
            /// <returns>Returns the current break out date time offset.</returns>
            public DateTimeOffset? RegisterStartBreak(string staffId, string terminalId, string jobId)
            {
                ThrowIf.NullOrWhiteSpace(staffId, "staffId");
                ThrowIf.NullOrWhiteSpace(terminalId, "terminalId");
                ThrowIf.NullOrWhiteSpace(jobId, "jobId");
    
                var data = this.InvokeMethod(StartBreakMethodName, staffId, terminalId, jobId);
    
                // Parse response data
                DateTime? activityDateTime = Convert.ToDateTime(data[0].ToString());
    
                return activityDateTime.ToUtcDateTimeOffset();
            }
    
            /// <summary>
            /// Gets all employee activities for the given store, activity types and date range.
            /// </summary>
            /// <param name="storeIds">Enter the store numbers.</param>
            /// <param name="activityTypes">Enter the activity types.</param>
            /// <param name="breakActivities">Enter the break activities.</param>
            /// <param name="fromUtcDateTime">Enter the from date/time.</param>
            /// <param name="toUtcDateTime">Enter the to date/time.</param>
            /// <param name="pagingInfo">Enter the paging information.</param>
            /// <param name="sortingInfo">Enter the sorting information.</param>
            /// <returns>The paged result of employee activities.</returns>
            public PagedResult<EmployeeActivity> GetManagerActivityHistory(string[] storeIds, EmployeeActivityType[] activityTypes, string[] breakActivities, DateTimeOffset? fromUtcDateTime, DateTimeOffset? toUtcDateTime, PagingInfo pagingInfo, SortingInfo sortingInfo)
            {
                ThrowIf.Null(pagingInfo, "pagingInfo");
                ThrowIf.Null(sortingInfo, "sortingInfo");
    
                ValidateDateTimeOffset(fromUtcDateTime);
    
                // Set datetime search parameters (same to EPOS, ViewTimeClockEntriesViewModel.cs)
                DateTime convertedFromUtc = fromUtcDateTime.HasValue ?
                    fromUtcDateTime.Value.UtcDateTime : DateTime.Today.AddDays(-90).ToUniversalTime();  // Previous 90 days
                DateTime convertedToUtc = toUtcDateTime.HasValue ?
                    toUtcDateTime.Value.UtcDateTime : DateTime.Today.AddDays(1).ToUniversalTime();      // To the end of of the current day
    
                // Creates the CSV search parameters
                int[] activityTypeList = Array.ConvertAll(activityTypes, value => (int)value);
                string storeIdsCsvStr = (storeIds == null) ? string.Empty : string.Join(",", storeIds);
                string employeeActivityTypesCsvStr = (activityTypeList == null) ? string.Empty : string.Join(",", activityTypeList);
                string breakActivityJobIdsCsvStr = (breakActivities == null) ? string.Empty : string.Join(",", breakActivities);
    
                // Retrieve the first sort column
                var sortColumn = new SortColumn(EmployeeActivity.DateTimeColumn, true);
                using (IEnumerator<SortColumn> enumerator = sortingInfo.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        sortColumn = enumerator.Current;
                    }
                }
    
                var parameters = new object[]
                {
                    string.Empty,               // Personnel number of the worker.
                    storeIdsCsvStr,
                    employeeActivityTypesCsvStr,
                    convertedFromUtc,
                    convertedToUtc,
                    breakActivityJobIdsCsvStr,
                    pagingInfo.NumberOfRecordsToFetch,
                    pagingInfo.Skip,
                    sortColumn.ColumnName,
                    (byte)(sortColumn.IsDescending ? SortOrderType.Descending : SortOrderType.Ascending)
                };
    
                var data = this.InvokeMethodAllowNullResponse(
                    GetManagerHistoryMethodName,
                    parameters);
    
                var employeeActivities = new List<EmployeeActivity>();
    
                if (data != null)
                {
                    // Parse response data
                    foreach (var employeeActivityDataRow in data)
                    {
                        var employeeActivityData = (object[])employeeActivityDataRow;
    
                        var employeeActivity = new EmployeeActivity();
                        DateTime? activityDateTime = Convert.ToDateTime(employeeActivityData[3]);
    
                        employeeActivity.StaffName = (string)employeeActivityData[0];
                        employeeActivity.StaffId = (string)employeeActivityData[1];
                        employeeActivity.ActivityDateTimeOffset = activityDateTime.ToUtcDateTimeOffset();
                        employeeActivity.Activity = ((EmployeeActivityType)employeeActivityData[5]).ToString();
                        employeeActivity.StoreNumber = (string)employeeActivityData[6];
    
                        employeeActivities.Add(employeeActivity);
                    }
                }
    
                return new PagedResult<EmployeeActivity>(employeeActivities.AsReadOnly(), pagingInfo);
            }
    
            /// <summary>
            /// Gets the employee activities for the given store and staff.
            /// </summary>
            /// <param name="staffId">Enter the staff identifier.</param>
            /// <param name="storeId">Enter the store identifier.</param>
            /// <param name="fromUtcDateTime">Enter the from UTC date/time.</param>
            /// <param name="toUtcDateTime">Enter the to UTC date/time.</param>
            /// <param name="pagingInfo">Enter the paging information.</param>
            /// <param name="sortingInfo">Enter the sorting information.</param>
            /// <returns>The paged result of employee activities.</returns>
            public PagedResult<EmployeeActivity> GetEmployeeActivityHistory(string staffId, string storeId, DateTimeOffset? fromUtcDateTime, DateTimeOffset? toUtcDateTime, PagingInfo pagingInfo, SortingInfo sortingInfo)
            {
                ThrowIf.NullOrWhiteSpace(staffId, "staffId");
                ThrowIf.NullOrWhiteSpace(storeId, "storeId");
                ThrowIf.Null(pagingInfo, "pagingInfo");
                ThrowIf.Null(sortingInfo, "sortingInfo");
    
                ValidateDateTimeOffset(fromUtcDateTime);
    
                // Set datetime search parameters (same to EPOS, LogbookForm.cs, with 30 minutes tolerance added)
                DateTime convertedFromUtc = fromUtcDateTime.HasValue ?
                    fromUtcDateTime.Value.UtcDateTime : DateTime.UtcNow.AddDays(1).AddMinutes(-30);
                DateTime convertedToUtc = toUtcDateTime.HasValue ?
                    toUtcDateTime.Value.UtcDateTime : DateTime.UtcNow.AddMinutes(30);
    
                // Retrieve the first sort column
                var sortColumn = new SortColumn(EmployeeActivity.DateTimeColumn, true);
                using (IEnumerator<SortColumn> enumerator = sortingInfo.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        sortColumn = enumerator.Current;
                    }
                }
    
                var parameters = new object[]
                {
                    staffId,
                    storeId,
                    convertedFromUtc,
                    convertedToUtc,
                    pagingInfo.Top,
                    pagingInfo.Skip,
                    sortColumn.ColumnName,
                    (byte)(sortColumn.IsDescending ? SortOrderType.Descending : SortOrderType.Ascending)
                };
    
                var data = this.InvokeMethodAllowNullResponse(GetWorkerHistoryMethodName, parameters);
    
                var employeeActivities = new List<EmployeeActivity>();
    
                if (data != null)
                {
                    // Parse response data
                    foreach (var employeeActivityDataRow in data)
                    {
                        var employeeActivityData = (object[])employeeActivityDataRow;
    
                        var employeeActivity = new EmployeeActivity();
                        DateTime? activityDateTime = Convert.ToDateTime(employeeActivityData[3]);
    
                        employeeActivity.Activity = (string)employeeActivityData[2];
                        employeeActivity.ActivityDateTimeOffset = activityDateTime.ToUtcDateTimeOffset();
                        employeeActivity.StoreNumber = (string)employeeActivityData[4];
    
                        employeeActivities.Add(employeeActivity);
                    }
                }
    
                return new PagedResult<EmployeeActivity>(employeeActivities.AsReadOnly(), pagingInfo);
            }
        }
    }
}
