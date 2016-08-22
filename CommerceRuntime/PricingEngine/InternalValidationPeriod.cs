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
    namespace Commerce.Runtime.Services.PricingEngine
    {
        using System;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Represents a validation period and encapsulates logic to decide
        ///  if the validation period is currently active.
        /// </summary>
        internal sealed class InternalValidationPeriod
        {
            /// <summary>
            /// The sentinel value for 'no date specified'.
            /// </summary>
            internal static readonly DateTime NoDate = new DateTime(1900, 1, 1);
    
            /// <summary>
            /// Prevents a default instance of the InternalValidationPeriod class from being created.
            /// </summary>
            private InternalValidationPeriod()
            {
            }
    
            /// <summary>
            /// Gets the validation type of the validation period.
            /// </summary>
            public DateValidationType ValidationType { get; private set; }
    
            /// <summary>
            /// Gets or sets the beginning date of the period.
            /// </summary>
            private DateTimeOffset StartDate { get; set; }
    
            /// <summary>
            /// Gets or sets the ending date of the period.
            /// </summary>
            private DateTimeOffset EndDate { get; set; }
    
            private ValidationPeriod Period { get; set; }
    
            /// <summary>
            /// Validate date against validation period.
            /// </summary>
            /// <param name="validationType">Validation type.</param>
            /// <param name="validationPeriod">Validation period.</param>
            /// <param name="startDate">Pricing start date.</param>
            /// <param name="endDate">Pricing end date.</param>
            /// <param name="dateToCheck">Date to check.</param>
            /// <returns>True if it's valid.</returns>
            public static bool ValidateDateAgainstValidationPeriod(DateValidationType validationType, ValidationPeriod validationPeriod, DateTimeOffset startDate, DateTimeOffset endDate, DateTimeOffset dateToCheck)
            {
                InternalValidationPeriod validation;
                bool promoPeriodValid = false;
    
                switch (validationType)
                {
                    case DateValidationType.Advanced:
                        validation = InternalValidationPeriod.CreateAdvanced(validationPeriod);
                        promoPeriodValid = validation.IsActive(dateToCheck);
                        break;
    
                    case DateValidationType.Standard:
                        validation = InternalValidationPeriod.CreateStandard(startDate: startDate, endDate: endDate);
                        promoPeriodValid = validation.IsActive(dateToCheck);
                        break;
    
                    default:
                        throw new ArgumentOutOfRangeException("validationType", "Invalid Discount Validation Type: " + validationType);
                }
    
                return promoPeriodValid;
            }
    
            /// <summary>
            /// Create a simple validation period (start date -> end date).
            /// </summary>
            /// <param name="startDate">Date period becomes active.</param>
            /// <param name="endDate">Date period becomes inactive.</param>
            /// <returns>Validation period.</returns>
            public static InternalValidationPeriod CreateStandard(DateTimeOffset startDate, DateTimeOffset endDate)
            {
                return new InternalValidationPeriod
                {
                    ValidationType = DateValidationType.Standard,
                    StartDate = startDate,
                    EndDate = endDate,
                    Period = null,
                };
            }
    
            /// <summary>
            /// For the given date/time is the validation period instance active.
            /// </summary>
            /// <param name="currentTime">Date/time to check for activity.</param>
            /// <returns>True is validation period instance is active for date/time; False otherwise.</returns>
            public bool IsActive(DateTimeOffset currentTime)
            {
                bool promoPeriodValid = false;
    
                if (this.ValidationType == DateValidationType.Advanced)
                {
                    if (this.Period != null)
                    {
                        promoPeriodValid = IsValidationPeriodActive(this.Period, currentTime);
                    }
                }
                else if (this.ValidationType == DateValidationType.Standard)
                {
                    promoPeriodValid = InternalValidationPeriod.IsDateWithinStartEndDate(currentTime.Date, this.StartDate, this.EndDate);
                }
                else
                {
                    string error = string.Format(CultureInfo.InvariantCulture, "The validation type '{0}' is not supported.", this.ValidationType);
                    throw new NotSupportedException(error);
                }
    
                return promoPeriodValid;
            }
    
            internal static bool IsDateWithinStartEndDate(DateTimeOffset dateToCheck, DateTimeOffset startDate, DateTimeOffset endDate)
            {
                return ((dateToCheck.Date >= startDate.Date) || (startDate.Date == NoDate))
                    && ((dateToCheck.Date <= endDate.Date) || (endDate.Date == NoDate));
            }
    
            internal static InternalValidationPeriod CreateAdvanced(ValidationPeriod period)
            {
                return new InternalValidationPeriod
                {
                    ValidationType = DateValidationType.Advanced,
                    Period = period,
                };
            }
    
            private static bool IsRangeDefinedForDay(ValidationPeriod period, DayOfWeek day)
            {
                return (period.StartingTimeForDay(day) != 0) && (period.EndingTimeForDay(day) != 0);
            }
    
            private static bool IsPeriodActiveForDayAndTime(ValidationPeriod period, DayOfWeek day, TimeSpan time, bool testOnlyAfterMidnight)
            {
                var configuration = new PeriodRangeConfiguration
                {
                    StartTime = period.StartingTimeForDay(day),
                    EndTime = period.EndingTimeForDay(day),
                    EndsTomorrow = period.IsEndTimeAfterMidnightForDay(day),
                    IsActiveOnlyWithinBounds = period.IsTimeBoundedForDay(day),
                };
    
                int currentTime = Convert.ToInt32(time.TotalSeconds);
                return IsTimeActiveForConfiguration(currentTime, configuration, testOnlyAfterMidnight);
            }
    
            /// <summary>
            /// For a given time, and period time-range setup, and whether to restrict our search to after midnight,
            ///  this method tells if the given time is active or inactive within the context of the range.
            /// </summary>
            /// <param name="currentTime">Current time in seconds past midnight.</param>
            /// <param name="configuration">Period time range setup parameters.</param>
            /// <param name="testOnlyAfterMidnight">Whether we only check for activity after midnight.</param>
            /// <returns>Result telling if given time is active in the configuration.</returns>
            private static bool IsTimeActiveForConfiguration(int currentTime, PeriodRangeConfiguration configuration, bool testOnlyAfterMidnight)
            {
                // if time falls between start and end times, return true if set to be active in range
                bool rangeAppliesBeforeMidnight = configuration.StartTime <= currentTime &&
                    ((configuration.EndTime >= currentTime) || configuration.EndsTomorrow);
    
                if (!testOnlyAfterMidnight && rangeAppliesBeforeMidnight)
                {
                    return configuration.IsActiveOnlyWithinBounds;
                }
    
                // if time is before end time for ending times past midnight, return true if set to be active in range
                bool rangeAppliesAfterMidnight = configuration.EndsTomorrow && configuration.EndTime >= currentTime;
    
                if (rangeAppliesAfterMidnight)
                {
                    return configuration.IsActiveOnlyWithinBounds;
                }
    
                return !configuration.IsActiveOnlyWithinBounds;
            }
    
            private static bool IsValidationPeriodActive(ValidationPeriod validationPeriod, DateTimeOffset transDateTime)
            {
                if (validationPeriod == null || string.IsNullOrEmpty(validationPeriod.PeriodId))
                {
                    // If no period Id given, then it is always a valid period
                    return true;
                }
    
                DateTime transDate = transDateTime.Date;
                TimeSpan transTime = transDateTime.TimeOfDay;
    
                // Is the discount valid within the start and end date period?
                if (InternalValidationPeriod.IsDateWithinStartEndDate(transDate, validationPeriod.ValidFrom.Date, validationPeriod.ValidTo.Date))
                {
                    bool answerFound = false;
                    bool isActive = false;
    
                    // does today's configuration tell if period is active?
                    if (IsRangeDefinedForDay(validationPeriod, transDate.DayOfWeek))
                    {
                        isActive = IsPeriodActiveForDayAndTime(validationPeriod, transDate.DayOfWeek, transTime, false);
                        answerFound = true;
                    }
    
                    // if we don't know or got negative result, see if yesterday will activate it (if its range ends after midnight)
                    DayOfWeek yesterday = transDate.AddDays(-1).DayOfWeek;
                    bool lastRangeDefinedAfterMidnight =
                        IsRangeDefinedForDay(validationPeriod, yesterday) && validationPeriod.IsEndTimeAfterMidnightForDay(yesterday);
    
                    if ((!answerFound || isActive == false) && lastRangeDefinedAfterMidnight)
                    {
                        // if yesterday makes it active, set isActive = true
                        isActive = IsPeriodActiveForDayAndTime(validationPeriod, yesterday, transTime, true);
                        answerFound = true;
                    }
    
                    // if we still don't know, try using general configuration
                    if (!answerFound)
                    {
                        var configuration = new PeriodRangeConfiguration
                        {
                            StartTime = validationPeriod.StartingTime,
                            EndTime = validationPeriod.EndingTime,
                            IsActiveOnlyWithinBounds = validationPeriod.IsTimeBounded != 0,
                            EndsTomorrow = validationPeriod.IsEndTimeAfterMidnight != 0
                        };
    
                        if ((validationPeriod.StartingTime != 0) && (validationPeriod.EndingTime != 0))
                        {
                            int currentTime = Convert.ToInt32(transTime.TotalSeconds);
                            isActive = IsTimeActiveForConfiguration(currentTime, configuration, false);
                            answerFound = true;
                        }
                    }
    
                    return answerFound ? isActive : (validationPeriod.IsTimeBounded == 1);
                }
    
                // not within date range, so active if not set to be within date range
                return validationPeriod.IsTimeBounded != 1;
            }
    
            /// <summary>
            /// Represent a time-range configuration for discount validation period
            ///  These ranges have a start and end time, indicator for ending past midnight, and
            ///  flag indicated what finding a time in this range means (i.e. whether being in the range validates/invalidates the time).
            /// </summary>
            private struct PeriodRangeConfiguration
            {
                /// <summary>
                /// The starting time for the period range.
                /// </summary>
                public int StartTime;
    
                /// <summary>
                /// The ending time for the period range.
                /// </summary>
                public int EndTime;
    
                /// <summary>
                /// True if the ending time is after midnight (i.e. tomorrow).
                /// </summary>
                public bool EndsTomorrow;
    
                /// <summary>
                /// True if the range is active within the given bounds. False if active outside the given bounds.
                /// </summary>
                public bool IsActiveOnlyWithinBounds;
            }
        }
    }
}
