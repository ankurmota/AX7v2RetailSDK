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
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
        using System.Threading.Tasks;

        /// <summary>
        /// Provides helper methods for conversion of hex string sequences.
        /// </summary>
        internal static class HexStringConverter
        {
            /// <summary>
            /// Converts to hex string to a byte array.
            /// </summary>
            /// <param name="hexString">The hex string to convert.</param>
            /// <param name="nullTerminated">If set to true, then stops parsing hex after the first 0x00 found (will not include the null to the resulting byte array).</param>
            /// <returns>The byte array containing converted hex string.</returns>
            /// <remarks>
            /// The method is not case sensitive. Works with both upper and lower case hex symbols.
            /// </remarks>
            public static byte[] ToByteArray(string hexString, bool nullTerminated = false)
            {
                if (hexString == null)
                {
                    throw new ArgumentNullException("hexString");
                }

                int length = hexString.Length;

                IList<byte> byteArray = new List<byte>(length / 2);

                for (var i = 0; i < length; i += 2)
                {
                    if (i + 1 == length)
                    {
                        throw new FormatException("Unexpected end of the hex string.");
                    }

                    var ch = (byte)((HexVal(hexString[i]) << 4) | HexVal(hexString[i + 1]));

                    // Some devices have trailing zeros.
                    if (ch == 0 && nullTerminated)
                    {
                        break;
                    }

                    byteArray.Add(ch);
                }

                return byteArray.ToArray();
            }

            private static int HexVal(char hex)
            {
                if (hex >= '0' && hex <= '9')
                {
                    return hex - '0';
                }
                else if (hex >= 'A' && hex <= 'F')
                {
                    return hex - 'A' + 10;
                }
                else if (hex >= 'a' && hex <= 'f')
                {
                    return hex - 'a' + 10;
                }
                else
                {
                    throw new FormatException("Invalid char in the hex string.");
                }
            }
        }
    }
}