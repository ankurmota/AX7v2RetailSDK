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
        using System.Collections.Generic;
        using System.Composition;
        using System.Text.RegularExpressions;
        using Interop.OposConstants;
        using Interop.OposPOSPrinter;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;

        /// <summary>
        /// Class implements OPOS based printer driver for hardware station.
        /// </summary>
        [Export(PeripheralType.Opos, typeof(IPrinter))]
        public class OposPrinter : IPrinter
        {
            private const string BarCodeRegEx = "<B: (.+?)>";
            private IOPOSPOSPrinter oposPrinter;
            private DefaultLogo defaultLogo = new DefaultLogo();
            private bool binaryConversionEnabled;

            /// <summary>
            /// Opens a peripheral.
            /// </summary>
            /// <param name="peripheralName">Name of the peripheral.</param>
            /// <param name="peripheralConfig">Configuration parameters of the peripheral.</param>
            public void Open(string peripheralName, PeripheralConfiguration peripheralConfig)
            {
                this.Open(peripheralName, 0, false, peripheralConfig);
            }

            /// <summary>
            /// Opens a peripheral.
            /// </summary>
            /// <param name="peripheralName">Name of the peripheral.</param>
            /// <param name="characterSet">The character set.</param>
            /// <param name="binaryConversion">If set to <c>true</c> [binary conversion].</param>
            /// <param name="peripheralConfig">Configuration parameters of the peripheral.</param>
            public void Open(string peripheralName, int characterSet, bool binaryConversion, PeripheralConfiguration peripheralConfig)
            {
                this.oposPrinter = new OPOSPOSPrinter();

                // Open
                this.oposPrinter.Open(peripheralName);
                OposHelper.CheckResultCode(this, this.oposPrinter.ResultCode);

                // Claim
                this.oposPrinter.ClaimDevice(OposHelper.ClaimTimeOut);
                OposHelper.CheckResultCode(this, this.oposPrinter.ResultCode);

                // Enable/Configure
                this.oposPrinter.DeviceEnabled = true;
                this.oposPrinter.AsyncMode = false;
                this.oposPrinter.CharacterSet = characterSet;
                this.oposPrinter.RecLineChars = 56;
                this.oposPrinter.SlpLineChars = 60;
                this.binaryConversionEnabled = binaryConversion;
            }

            /// <summary>
            /// Closes the peripheral.
            /// </summary>
            public void Close()
            {
                if (this.oposPrinter != null)
                {
                    this.oposPrinter.ReleaseDevice();
                    this.oposPrinter.Close();
                }
            }

            /// <summary>
            /// Print the data on printer.
            /// </summary>
            /// <param name="header">The header.</param>
            /// <param name="lines">The lines.</param>
            /// <param name="footer">The footer.</param>
            public void Print(string header, string lines, string footer)
            {
                string textToPrint = (header + lines + footer).Replace(OposHelper.EscMarker, OposHelper.EscCharacter);
                var parts = TextLogoParser.Parse(textToPrint);

                foreach (var part in parts)
                {
                    if (part.TextType == TextType.LegacyLogo)
                    {
                        this.PrintLegacyLogo();
                    }

                    if (part.TextType == TextType.LogoWithBytes)
                    {
                        this.PrintLogo(part.Value);
                    }

                    if (part.TextType == TextType.Text)
                    {
                        this.PrintText(part.Value);
                    }
                }

                this.oposPrinter.CutPaper(100);
            }

            /// <summary>
            /// Prints the legacy logo.
            /// </summary>
            private void PrintLegacyLogo()
            {
                byte[] defaultLogoBytes = this.defaultLogo.GetBytes();
                this.PrintLogoBytes(defaultLogoBytes);
            }

            /// <summary>
            /// Prints the logo embedded in a logo tag via base64 encoding.
            /// </summary>
            /// <param name="logoText">The logo text.</param>
            private void PrintLogo(string logoText)
            {
                byte[] image = TextLogoParser.GetLogoImageBytes(logoText);
                this.PrintLogoBytes(image);
            }

            /// <summary>
            /// Method to print the raw bytes of a BMP-formatted image.
            /// </summary>
            /// <param name="image">Image bytes to print.</param>
            private void PrintLogoBytes(byte[] image)
            {
                if (this.oposPrinter.CapRecBitmap)
                {
                    if (null != image && image.Length > 0)
                    {
                        int conversion = this.oposPrinter.BinaryConversion; // save current conversion mode
                        this.oposPrinter.BinaryConversion = 2; // OposBcDecimal

                        this.oposPrinter.PrintMemoryBitmap(
                            (int)OPOSPOSPrinterConstants.PTR_S_RECEIPT,
                            OposHelper.ConvertToBCD(image),
                            (int)OPOSPOSPrinterConstants.PTR_BMT_BMP,
                            (int)OPOSPOSPrinterConstants.PTR_BM_ASIS,
                            (int)OPOSPOSPrinterConstants.PTR_BM_CENTER);
                        this.oposPrinter.BinaryConversion = conversion; // restore previous conversion mode
                    }
                }
            }

            /// <summary>
            /// Print the text to the Printer.
            /// </summary>
            /// <param name="textToPrint">The text to print on the receipt.</param>
            private void PrintText(string textToPrint)
            {
                Match barCodeMarkerMatch = Regex.Match(textToPrint, BarCodeRegEx, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                bool printBarcode = false;
                string receiptId = string.Empty;

                if (barCodeMarkerMatch.Success)
                {
                    printBarcode = true;

                    // Get the receiptId
                    receiptId = barCodeMarkerMatch.Groups[1].ToString();

                    // Delete the barcode marker from the printed string
                    textToPrint = textToPrint.Remove(barCodeMarkerMatch.Index, barCodeMarkerMatch.Length);
                }

                // replace ESC with Char(27) and add a CRLF to the end
                textToPrint = textToPrint.Replace("ESC", ((char)27).ToString());

                if (this.binaryConversionEnabled == true)
                {
                    this.oposPrinter.BinaryConversion = 2; // OposBcDecimal
                    textToPrint = OposHelper.ConvertToBCD(textToPrint + "\r\n\r\n\r\n", this.oposPrinter.CharacterSet);
                }

                this.oposPrinter.PrintNormal((int)OPOSPOSPrinterConstants.PTR_S_RECEIPT, textToPrint);
                this.oposPrinter.BinaryConversion = 0; // OposBcNone

                // Check if we should print the receipt id as a barcode on the receipt
                if (printBarcode == true)
                {
                    this.oposPrinter.PrintBarCode(
                        (int)OPOSPOSPrinterConstants.PTR_S_RECEIPT,
                        receiptId,
                        (int)OPOSPOSPrinterConstants.PTR_BCS_Code128,
                        80,
                        80,
                        (int)OPOSPOSPrinterConstants.PTR_BC_CENTER,
                        (int)OPOSPOSPrinterConstants.PTR_BC_TEXT_BELOW);
                    this.oposPrinter.PrintNormal((int)OPOSPOSPrinterConstants.PTR_S_RECEIPT, "\r\n\r\n\r\n\r\n");
                }
            }
        }
    }
}
