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
    namespace Retail.Connector.FunctionalTests
    {
        using System;
        using Microsoft.VisualStudio.TestTools.UnitTesting;
    
        /// <summary>
        /// Extensions for TestContext class.
        /// </summary>
        public static class TestContextExtensions
        {
            /// <summary>
            /// Gets the string value of the named column.
            /// </summary>
            /// <param name="testContext">The current object.</param>
            /// <param name="columnName">The column name.</param>
            /// <returns>The value.</returns>
            public static string GetStringValue(this TestContext testContext, string columnName)
            {
                if (testContext == null)
                {
                    throw new ArgumentNullException("testContext");
                }
    
                if (columnName == null)
                {
                    throw new ArgumentNullException("columnName");
                }
    
                return testContext.DataRow[columnName] as string;
            }
    
            /// <summary>
            /// Gets the integer value of the named column.
            /// </summary>
            /// <param name="testContext">The current object.</param>
            /// <param name="columnName">The column name.</param>
            /// <returns>The value.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "integer", Justification = "The word integer is included in the method name by design.")]
            public static int GetIntegerValue(this TestContext testContext, string columnName)
            {
                if (testContext == null)
                {
                    throw new ArgumentNullException("testContext");
                }
    
                string stringValue = testContext.GetStringValue(columnName);
                return int.Parse(stringValue);
            }
    
            /// <summary>
            /// Gets the decimal value of the named column.
            /// </summary>
            /// <param name="testContext">The current object.</param>
            /// <param name="columnName">The column name.</param>
            /// <returns>The value.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "integer", Justification = "The word decimal is included in the method name by design.")]
            public static decimal GetDecimalValue(this TestContext testContext, string columnName)
            {
                if (testContext == null)
                {
                    throw new ArgumentNullException("testContext");
                }
    
                string stringValue = testContext.GetStringValue(columnName);
                return decimal.Parse(stringValue);
            }
    
            /// <summary>
            /// Gets the date time value of the named column.
            /// </summary>
            /// <param name="testContext">The current object.</param>
            /// <param name="columnName">The column name.</param>
            /// <returns>The value.</returns>
            public static DateTime GetDateTimeValue(this TestContext testContext, string columnName)
            {
                if (testContext == null)
                {
                    throw new ArgumentNullException("testContext");
                }
    
                string stringValue = testContext.GetStringValue(columnName);
                return DateTime.Parse(stringValue);
            }
    
            /// <summary>
            /// Tries to get the string value of the named column.
            /// </summary>
            /// <param name="testContext">The current object.</param>
            /// <param name="columnName">The column name.</param>
            /// <param name="value">The value.</param>
            /// <returns>The value indicating whether the column value is retrieved.</returns>
            public static bool TryGetStringValue(this TestContext testContext, string columnName, out string value)
            {
                if (testContext == null)
                {
                    throw new ArgumentNullException("testContext");
                }
    
                if (columnName == null)
                {
                    throw new ArgumentNullException("columnName");
                }
    
                object obj = null;
                try
                {
                    obj = testContext.DataRow[columnName];
                }
                catch (ArgumentException)
                {
                    obj = null;
                }
    
                if (obj != null && obj is string)
                {
                    value = testContext.DataRow[columnName] as string;
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            }
    
            /// <summary>
            /// Tries to get the integer value of the named column.
            /// </summary>
            /// <param name="testContext">The current object.</param>
            /// <param name="columnName">The column name.</param>
            /// <param name="value">The value.</param>
            /// <returns>The value indicating whether the column value is retrieved.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "integer", Justification = "The word integer is included in the method name by design.")]
            public static bool TryGetIntegerValue(this TestContext testContext, string columnName, out int value)
            {
                if (testContext == null)
                {
                    throw new ArgumentNullException("testContext");
                }
    
                string stringValue;
                if (testContext.TryGetStringValue(columnName, out stringValue))
                {
                    return int.TryParse(stringValue, out value);
                }
                else
                {
                    value = 0;
                    return false;
                }
            }
    
            /// <summary>
            /// Tries to get the decimal value of the named column.
            /// </summary>
            /// <param name="testContext">The current object.</param>
            /// <param name="columnName">The column name.</param>
            /// <param name="value">The value.</param>
            /// <returns>The value indicating whether the column value is retrieved.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "integer", Justification = "The word decimal is included in the method name by design.")]
            public static bool TryGetDecimalValue(this TestContext testContext, string columnName, out decimal value)
            {
                if (testContext == null)
                {
                    throw new ArgumentNullException("testContext");
                }
    
                string stringValue;
                if (testContext.TryGetStringValue(columnName, out stringValue))
                {
                    return decimal.TryParse(stringValue, out value);
                }
                else
                {
                    value = 0m;
                    return false;
                }
            }
    
            /// <summary>
            /// Tries to get the DateTime value of the named column.
            /// </summary>
            /// <param name="testContext">The current object.</param>
            /// <param name="columnName">The column name.</param>
            /// <param name="value">The value.</param>
            /// <returns>The value indicating whether the column value is retrieved.</returns>
            public static bool TryGetDateTimeValue(this TestContext testContext, string columnName, out DateTime value)
            {
                if (testContext == null)
                {
                    throw new ArgumentNullException("testContext");
                }
    
                string stringValue;
                if (testContext.TryGetStringValue(columnName, out stringValue))
                {
                    return DateTime.TryParse(stringValue, out value);
                }
                else
                {
                    value = DateTime.MinValue;
                    return false;
                }
            }
        }   
    }
}
