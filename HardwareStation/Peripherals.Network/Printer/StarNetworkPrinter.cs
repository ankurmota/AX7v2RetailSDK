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
        using System.Composition;
        using System.Drawing;
        using System.IO;
        using System.Net.Http;
        using System.Text;
        using System.Text.RegularExpressions;
        using Microsoft.Dynamics.Commerce.HardwareStation;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// Star printers which enabled WebPRNT interface, e.g. TSP650II.
        /// </summary>
        [Export(PeripheralType.Network, typeof(IPrinter))]
        public sealed class StarNetworkPrinter : IPrinter
        {
            private const string StarWebUrlFormat = "http://{0}/StarWebPRNT/SendMessage";
            private const string StarWebRequestFormat = "<StarWebPrint xmlns=\"http://www.star-m.jp\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"><Request>{0}</Request></StarWebPrint>";
            private const string DataFormat = "<root><initialization/>{0}{1}{2}<text>\x0a</text><cutpaper feed=\"true\"/></root>";
            private const string LogoFormat = "<alignment position=\"center\"/><bitimage width=\"{0}\" height=\"{1}\">{2}</bitimage><alignment position=\"left\"/>";
            private const string BarcodeFormat = "<alignment position=\"center\"/><barcode symbology=\"Code128\" height=\"80\">{0}</barcode><alignment position=\"left\"/>";
            private const string TextFormat = "<text emphasis=\"{1}\" font=\"font_b\"{2}>{0}</text>";
            private const string CodePageFormat = " codepage=\"cp{0}\"";
            private const string NewLine = "<text>\x0a</text>";

            private const string Seperator = "&#x1B;";
            private const string Bold = "|2C";
            private const string CommandCharacter = "|";
            private const int CommandLength = 3;
            private const string BarCodeRegEx = "<B: (.+?)>";
            private const int BitsPerByte = 8;

            // Single bit bitmask for all 8 bits.
            private static readonly byte[] MonochromePixelBit = { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };
            private string ip = null;
            private int port = 0;
            private int codePage = 0;
            private string codePageAttribute = null;
            private DefaultLogo defaultLogo = new DefaultLogo();

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
                ThrowIf.NullOrWhiteSpace(peripheralName, "peripheralName");
                ThrowIf.Null(peripheralConfig, "peripheralConfig");

                IDictionary<string, object> configurations = peripheralConfig.ExtensionProperties.ToObjectDictionary();
                string ip = configurations[PeripheralConfigKey.IpAddress] as string;
                if (string.IsNullOrWhiteSpace(ip))
                {
                    throw new ArgumentException(string.Format("Peripheral configuration parameter is missing: {0}.", PeripheralConfigKey.IpAddress));
                }

                int? port = configurations[PeripheralConfigKey.Port] as int?;
                if (port == null)
                {
                    throw new ArgumentException(string.Format("Peripheral configuration parameter is missing: {0}.", PeripheralConfigKey.Port));
                }

                this.ip = ip.Trim();
                this.port = (int)port;

                // Get code page attribute
                this.codePage = characterSet;
                if (this.codePage > 0)
                {
                    this.codePageAttribute = string.Format(CodePageFormat, this.codePage);
                }
                else
                {
                    this.codePageAttribute = string.Empty; // Use printer setting
                }
            }

            /// <summary>
            /// Closes the peripheral.
            /// </summary>
            public void Close()
            {
                this.ip = null;
                this.port = 0;
            }

            /// <summary>
            /// Print the data on printer.
            /// </summary>
            /// <param name="header">The header.</param>
            /// <param name="lines">The lines.</param>
            /// <param name="footer">The footer.</param>
            public void Print(string header, string lines, string footer)
            {
                string headerXml = this.ConvertToXml(header);
                string bodyXml = this.ConvertToXml(lines);
                string footerXml = this.ConvertToXml(footer);
                string data = string.Format(DataFormat, headerXml, bodyXml, footerXml);
                var content = new StringContent(string.Format(StarWebRequestFormat, data.Replace("<", "&lt;").Replace(">", "&gt;")), Encoding.UTF8, "text/xml");

                string host;
                if (this.port == 0)
                {
                    host = this.ip;
                }
                else
                {
                    host = string.Format("{0}:{1}", this.ip, this.port);
                }

                var printerUri = new Uri(string.Format(StarWebUrlFormat, host));

                using (var httpClient = new HttpClient())
                {
                    httpClient.PostAsync(printerUri, content).Wait();
                }
            }

            /// <summary>
            ///  Converts the OPOS formatted receipt into an xml Star WebPRNT request.
            /// </summary>
            /// <param name="data">OPOS receipt data.</param>
            /// <returns>Star WebPRNT request.</returns>
            private string ConvertToXml(string data)
            {
                if (string.IsNullOrWhiteSpace(data))
                {
                    return data;
                }

                StringBuilder receipt = new StringBuilder();
                var lines = data.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                foreach (var line in lines)
                {
                    var textValues = line.Split(new[] { Seperator }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var textValue in textValues)
                    {
                        bool bold = textValue.StartsWith(Bold, StringComparison.Ordinal);
                        string textToPrint = textValue.StartsWith(CommandCharacter, StringComparison.Ordinal) ? textValue.Substring(CommandLength) : textValue;

                        var fields = TextLogoParser.Parse(textToPrint);
                        foreach (var field in fields)
                        {
                            switch (field.TextType)
                            {
                                case TextType.LegacyLogo:
                                    this.AppendLegacyLogo(receipt);
                                    break;
                                case TextType.LogoWithBytes:
                                    this.AppendLogo(receipt, field.Value);
                                    break;
                                case TextType.Text:
                                    this.AppendText(receipt, field.Value, bold);
                                    break;
                            }
                        }
                    }

                    receipt.Append(NewLine);
                }

                return receipt.ToString();
            }

            /// <summary>
            /// Appends the legacy logo to the receipt.
            /// </summary>
            /// <param name="receipt">The receipt content builder.</param>
            private void AppendLegacyLogo(StringBuilder receipt)
            {
                byte[] defaultLogoBytes = this.defaultLogo.GetBytes();
                this.AppendLogoBytes(receipt, defaultLogoBytes);
            }

            /// <summary>
            /// Appends the logo embedded in a logo tag via base64 encoding.
            /// </summary>
            /// <param name="receipt">The receipt content builder.</param>
            /// <param name="logoText">The logo text.</param>
            private void AppendLogo(StringBuilder receipt, string logoText)
            {
                byte[] image = TextLogoParser.GetLogoImageBytes(logoText);
                this.AppendLogoBytes(receipt, image);
            }

            /// <summary>
            /// Appends the raw bytes of a BMP-formatted image.
            /// </summary>
            /// <param name="receipt">The receipt content builder.</param>
            /// <param name="image">Image bytes to print.</param>
            private void AppendLogoBytes(StringBuilder receipt, byte[] image)
            {
                if (image != null)
                {
                    receipt.Append(NewLine);
                    using (var stream = new MemoryStream(image))
                    {
                        // Convert the image to monochrome (1 bit per pixel).
                        var bitmap = new Bitmap(stream);
                        int pixelCount = bitmap.Width * bitmap.Height;
                        int monochromeSize = (int)Math.Ceiling((decimal)pixelCount / (decimal)BitsPerByte);
                        var monochromePixels = new byte[monochromeSize];

                        // For each pixel, check whether the color is dark enough. If yes, flip the bit for that pixel.
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            for (int y = 0; y < bitmap.Height; y++)
                            {
                                Color pixel = bitmap.GetPixel(x, y);
                                int average = (pixel.R + pixel.B + pixel.G) / 3;
                                if (average < 128)
                                {
                                    var pixelIndex = (y * bitmap.Width) + x;
                                    var pixelLocation = pixelIndex / BitsPerByte;
                                    var pixelOffset = pixelIndex % BitsPerByte;

                                    monochromePixels[pixelLocation] |= MonochromePixelBit[pixelOffset];
                                }
                            }
                        }

                        receipt.AppendFormat(LogoFormat, bitmap.Width, bitmap.Height, Convert.ToBase64String(monochromePixels));
                    }

                    receipt.Append(NewLine);
                }
            }

            /// <summary>
            /// Appends the text to the receipt.
            /// </summary>
            /// <param name="receipt">The receipt content builder.</param>
            /// <param name="text">The text to print.</param>
            /// <param name="isBold">The value indicating whether the text should be bold.</param>
            private void AppendText(StringBuilder receipt, string text, bool isBold)
            {
                Match barCodeMarkerMatch = Regex.Match(text, BarCodeRegEx, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                bool printBarcode = false;
                string receiptId = string.Empty;

                if (barCodeMarkerMatch.Success)
                {
                    printBarcode = true;

                    // Get the barcode value
                    receiptId = barCodeMarkerMatch.Groups[1].ToString();

                    // Delete the barcode marker from the printed string
                    text = text.Remove(barCodeMarkerMatch.Index, barCodeMarkerMatch.Length);
                }
                
                // Print text
                receipt.AppendFormat(TextFormat, text, isBold, this.codePageAttribute);

                // Print barcode
                if (printBarcode == true)
                {
                    receipt.Append(NewLine);
                    receipt.AppendFormat(BarcodeFormat, receiptId);
                    receipt.Append(NewLine);
                }
            }
        }
    }
}
