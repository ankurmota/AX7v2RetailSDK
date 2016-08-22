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
    namespace Commerce.Runtime.DataServices.Sqlite
    {
        using System;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Helper class for managing row version.
        /// </summary>
        public static class RowVersionHelper
        {
            /// <summary>
            /// Gets a byte array representing a zero value row version.
            /// </summary>
            public static byte[] Zero
            {
                get
                {
                    byte[] zero = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
                    return zero;
                }
            }
    
            /// <summary>
            /// Increments the row version value of 1.
            /// </summary>
            /// <param name="rowversion">The row version value.</param>
            /// <returns>The updated row version value.</returns>
            public static byte[] Increment(byte[] rowversion)
            {
                if (rowversion == null)
                {
                    rowversion = Zero;
                }
    
                ulong value = ConvertToUnsignedLong(rowversion) + 1;
                return BitConverter.GetBytes(value);
            }
    
            /// <summary>
            /// Compare two byte arrays representing row versions.
            /// </summary>
            /// <param name="a">The first row version.</param>
            /// <param name="b">The second row version.</param>
            /// <returns>Returns a value indicating whether <paramref name="a"/> is equals to <paramref name="b"/> or not.</returns>
            public static bool AreEquals(byte[] a, byte[] b)
            {
                return ConvertToUnsignedLong(a) == ConvertToUnsignedLong(b);
            }
    
            /// <summary>
            /// Converts the row version byte array to an unsigned long.
            /// </summary>
            /// <param name="rowversion">The row version byte array.</param>
            /// <returns>The unsigned long value.</returns>
            private static ulong ConvertToUnsignedLong(byte[] rowversion)
            {
                if (rowversion == null || rowversion.Length != 8)
                {
                    throw new ArgumentException("rowversion must be 8 bytes long.", "rowversion");
                }
    
                return BitConverter.ToUInt64(rowversion, 0);
            }
        }
    }
}
