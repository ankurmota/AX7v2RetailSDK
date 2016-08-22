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
        using System.Collections.Generic;
        using System.Diagnostics;
        using System.Globalization;
        using System.Linq;
        using System.Text;
        using Microsoft.Dynamics.Commerce.HardwareStation;

        /// <summary>
        /// Encapsulates the barcode Code 39 string encoding.
        /// </summary>
        public sealed class BarcodeCode39 : Barcode
        {
            private const char StartEndMarker = '*';
            private const string BarcodeFontName = "BC C39 2 to 1 Narrow";
            private const int BarcodeFontSize = 30;
            private readonly List<int> supportedCharacters = new List<int>(100);
    
            /// <summary>
            /// Initializes a new instance of the <see cref="BarcodeCode39" /> class.
            /// </summary>
            public BarcodeCode39()
            {
                this.supportedCharacters.AddRange(Enumerable.Range(32, 60).Where(n => (
                        n == 32 // Space
                        || (n >= 36 && n <= 38) // $%&
                        || (n >= 43 && n <= 57) // +,./ 0 - 9
                        || (n >= 65 && n <= 90)))); // A - Z
            }
    
            /// <summary>
            /// Gets the font name.
            /// </summary>
            public override string FontName
            {
                get
                {
                    return BarcodeFontName;
                }
            }
    
            /// <summary>
            /// Gets the font size.
            /// </summary>
            public override int FontSize
            {
                get
                {
                    return BarcodeFontSize;
                }
            }
    
            /// <summary>
            /// Encodes the text to for Code39.
            /// </summary>
            /// <param name="text">The text to encode.</param>
            /// <returns>
            /// Encoded string.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">Exception thrown when 'text' is null.</exception>
            public override string Encode(string text)
            {
                ThrowIf.Null(text, "text");
    
                StringBuilder result = new StringBuilder(text.Length + 2);
    
                // Add start character
                result.Append(StartEndMarker);
    
                foreach (char ch in text)
                {
                    if (this.supportedCharacters.BinarySearch(ch) >= 0)
                    {
                        result.Append(ch);
                    }
                }
    
                // Replace Space with comma
                result.Replace(' ', ',');
    
                // Add stop character
                result.Append(StartEndMarker);
    
                return result.ToString();
            }
        }
    }
}
