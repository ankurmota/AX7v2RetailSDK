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
        using System.Collections;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Helper class for dealing with AxContainer objects.
        /// </summary>
        internal static class AxContainerHelper
        {
            /// <summary>
            /// The fixed date time format.
            /// </summary>
            private const string FixedDateTimeFormat = "yyyy-MM-ddThh:mm:ss";
    
            /// <summary>
            /// The fixed date format.
            /// </summary>
            private const string FixedDateFormat = "yyyy-MM-dd";
    
            /// <summary>
            /// Parse the date from an AX-sent date string.
            /// </summary>
            /// <param name="dateString">The date string.</param>
            /// <param name="defaultDate">The default date.</param>
            /// <param name="dateTimeStyle">The date time style.</param>
            /// <returns>The date/time.</returns>
            internal static DateTime ParseDateString(string dateString, DateTime defaultDate, DateTimeStyles dateTimeStyle = DateTimeStyles.AssumeLocal)
            {
                DateTimeFormatInfo info = new DateTimeFormatInfo();
                DateTime result = defaultDate;
    
                if (string.IsNullOrWhiteSpace(dateString) ||
                    (!DateTime.TryParseExact(dateString, FixedDateFormat, info, dateTimeStyle, out result) &&
                    !DateTime.TryParseExact(dateString, FixedDateTimeFormat, info, dateTimeStyle, out result)))
                {
                    return defaultDate;
                }
    
                return result;
            }
    
            /// <summary>
            /// Convert DateTime object as a string (includes time) in the AX supported format.
            /// </summary>
            /// <param name="dateTime">The date time.</param>
            /// <returns>The date/time as string.</returns>
            internal static string DateTimeToString(DateTime dateTime)
            {
                return dateTime.ToString(FixedDateTimeFormat, CultureInfo.CurrentCulture);
            }
    
            /// <summary>
            /// Convert DateTime object as a string (includes date only) in the AX supported format.
            /// </summary>
            /// <param name="dateTime">The date time.</param>
            /// <returns>The date as a string.</returns>
            internal static string DateToString(DateTime dateTime)
            {
                return dateTime.ToString(FixedDateFormat, CultureInfo.CurrentCulture);
            }
    
            internal static decimal DecimalAtIndex(IList list, int index)
            {
                return Convert.ToDecimal(list[index], CultureInfo.CurrentCulture);
            }
    
            internal static long LongAtIndex(IList list, int index)
            {
                return Convert.ToInt64(list[index], CultureInfo.CurrentCulture);
            }
    
            internal static bool BooleanAtIndex(IList list, int index)
            {
                return Convert.ToBoolean(list[index], CultureInfo.CurrentCulture);
            }
    
            internal static string StringAtIndex(IList list, int index)
            {
                return Convert.ToString(list[index], CultureInfo.CurrentCulture);
            }
    
            /// <summary>
            /// Extracts a date time value from the specified index.
            /// Returns a UTC value if non UTC value is passed in.
            /// </summary>
            /// <param name="list">The collection.</param>
            /// <param name="index">The index.</param>
            /// <param name="dateTimeKind">Kind of the date time.</param>
            /// <returns>The date/time.</returns>
            internal static DateTime DateTimeAtIndex(IList list, int index, DateTimeKind dateTimeKind)
            {
                DateTime dt = Convert.ToDateTime(list[index], CultureInfo.CurrentCulture);
    
                DateTime.SpecifyKind(dt, dateTimeKind);
    
                // Convert to UTC if the value is explicitly Local, otherwise assume UTC.
                return (dt.Kind == DateTimeKind.Local) ? dt.ToUniversalTime() : dt;
            }
        }
    }
}
