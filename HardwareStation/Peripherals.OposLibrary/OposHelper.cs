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
    namespace Commerce.HardwareStation.Peripherals
    {
        using System;
        using System.Globalization;
        using System.IO;
        using System.Text;
        using Interop.OposConstants;
    
        /// <summary>
        /// Implements OPOS related helper functions.
        /// </summary>
        public static class OposHelper
        {
            /// <summary>
            /// Gets the claim time out.
            /// </summary>
            public static int ClaimTimeOut
            {
                get
                {
                    return 10000;
                }
            }
    
            /// <summary>
            /// Gets the esc marker.
            /// </summary>
            public static string EscMarker
            {
                get
                {
                    return "&#x1B;";
                }
            }
    
            /// <summary>
            /// Gets the esc character.
            /// </summary>
            public static string EscCharacter
            {
                get
                {
                    return "\x1B";
                }
            }
    
            /// <summary>
            /// Check the OPOS result code from last operation.
            /// </summary>
            /// <param name="source">Source of result code.</param>
            /// <param name="resultCode">Result code returned from last operation.</param>
            /// <exception cref="IOException">Device IO error.</exception>
            public static void CheckResultCode(object source, int resultCode)
            {
                if (source == null)
                {
                    throw new ArgumentNullException("source");
                }
    
                OPOS_Constants result = (OPOS_Constants)resultCode;
    
                if (result != OPOS_Constants.OPOS_SUCCESS)
                {
                    string message = string.Format(CultureInfo.InvariantCulture, "{0} device failed with error '{1}'.", source.GetType().Name, result);
                    throw new IOException(message);
                }
            }
    
            /// <summary>
            /// Convert string from Unicode to given character set and pack in BCD format.
            /// </summary>
            /// <param name="source">String to be converted.</param>
            /// <param name="characterSet">Target character set.</param>
            /// <returns>String as BCD.</returns>
            public static string ConvertToBCD(string source, int characterSet)
            {
                Encoding sourceEncoding = Encoding.Unicode;
                Encoding targetEncoding = Encoding.GetEncoding(characterSet);
    
                byte[] sourceBytes = sourceEncoding.GetBytes(source);
    
                // Converting those bytes
                byte[] targetBytes = Encoding.Convert(sourceEncoding, targetEncoding, sourceBytes);
    
                return ConvertToBCD(targetBytes);
            }
    
            /// <summary>
            /// Converts bytes directly into BCD format.
            /// </summary>
            /// <param name="rawBytes">Byte array to be converted.</param>
            /// <returns>String as BCD.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "bytes", Justification = "Just internal to peripheral.")]
            public static string ConvertToBCD(byte[] rawBytes)
            {
                if (rawBytes == null)
                {
                    throw new ArgumentNullException("rawBytes");
                }
    
                StringBuilder result = new StringBuilder();
    
                // UPOS Binary conversion accepts each character formatted in 3 bytes padded with zeros.
                foreach (byte b in rawBytes)
                {
                    result.AppendFormat("{0:000}", b);
                }
    
                return result.ToString();
            }
        }
    }
}
