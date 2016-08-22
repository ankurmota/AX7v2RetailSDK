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
    namespace Commerce.HardwareStation.RamblerSample
    {
        using System;
        using System.Text;

        /// <summary>
        /// Provides helper methods for conversion of ASCII strings.
        /// </summary>
        internal static class AsciiStringConverter
        {
            /// <summary>
            /// Converts 8-bit ASCII array to string.
            /// </summary>
            /// <param name="asciiArray">The 8-bit ASCII array.</param>
            /// <param name="offset">The offset of beginning of the string.</param>
            /// <param name="length">The length of the string.</param>
            /// <returns>The converted string.</returns>
            public static string Ascii8BitToString(byte[] asciiArray, int offset, int length)
            {
                if (asciiArray == null)
                {
                    throw new ArgumentNullException("asciiArray");
                }

                if (offset < 0 || offset > asciiArray.Length)
                {
                    throw new ArgumentOutOfRangeException("offset", "The offset is out data boundaries.");
                }

                if (length < 0 || offset + length > asciiArray.Length)
                {
                    throw new ArgumentOutOfRangeException("length", "The length is out data boundaries.");
                }

                var sb = new StringBuilder(length);

                for (var i = offset; i < offset + length; i++)
                {
                    sb.Append((char)asciiArray[i]);
                }

                return sb.ToString();
            }
        }
    }
}