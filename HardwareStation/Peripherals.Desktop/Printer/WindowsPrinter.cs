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
        using System.Drawing.Printing;
        using System.Globalization;
        using System.IO;
        using System.Linq;
        using System.Text.RegularExpressions;
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Class implements windows based printer driver for hardware station.
        /// </summary>
        [Export(PeripheralType.Windows, typeof(IPrinter))]
        public class WindowsPrinter : IPrinter
        {
            private const string BarCodeRegEx = "<B: (.+?)>";
            private const string NormalTextMarker = "|1C";
            private const string BoldTextMarker = "|2C";
    
            private List<TextPart> parts;
            private int printLine;
            private Barcode barCode = new BarcodeCode39();
            private DefaultLogo defaultLogo = new DefaultLogo();
    
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
            /// Gets or sets the printer name.
            /// </summary>
            protected string PrinterName { get; set; }

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
                this.PrinterName = peripheralName;
            }
    
            /// <summary>
            /// Closes the peripheral.
            /// </summary>
            public void Close()
            {
            }
    
            /// <summary>
            /// Print the data on printer.
            /// </summary>
            /// <param name="header">The header.</param>
            /// <param name="lines">The lines.</param>
            /// <param name="footer">The footer.</param>
            public void Print(string header, string lines, string footer)
            {
                string textToPrint = header + lines + footer;
    
                if (!string.IsNullOrWhiteSpace(textToPrint))
                {
                    using (PrintDocument printDoc = new PrintDocument())
                    {
                        printDoc.PrinterSettings.PrinterName = this.PrinterName;
                        string subString = textToPrint.Replace(EscMarker, string.Empty);
                        var printText = subString.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
    
                        this.parts = new List<TextPart>();
                        foreach (var line in printText)
                        {
                            var lineParts = TextLogoParser.Parse(line);
                            if (null != lineParts)
                            {
                                this.parts.AddRange(lineParts);
                            }
                        }
    
                        this.printLine = 0;
    
                        printDoc.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.PrintPageHandler);
    
    #if DEBUG
                        if ("Microsoft XPS Document Writer" == this.PrinterName)
                        {
                            printDoc.PrinterSettings.PrintFileName = Path.Combine(Path.GetTempPath(), "HardwareStation_Print_Result.xps");
                            printDoc.PrinterSettings.PrintToFile = true;
                            NetTracer.Information(string.Format(CultureInfo.InvariantCulture, "Look for XPS file here: {0}", printDoc.PrinterSettings.PrintFileName));
                        }
    #endif
                        printDoc.Print();
                    }
                }
            }
    
            private static bool DrawBitmapImage(PrintPageEventArgs e, byte[] defaultLogoBytes, float contentWidth, ref float y)
            {
                using (MemoryStream ms = new MemoryStream(defaultLogoBytes))
                {
                    var image = Image.FromStream(ms);
    
                    if (y + image.Height >= e.MarginBounds.Height)
                    {
                        // No more room - advance to next page
                        e.HasMorePages = true;
                        return false;
                    }
    
                    float center = ((contentWidth - image.Width) / 2.0f) + e.MarginBounds.Left;
                    if (center < 0)
                    {
                        center = 0;
                    }
    
                    float top = e.MarginBounds.Top + y;
    
                    e.Graphics.DrawImage(image, center, top);
    
                    y += image.Height;
    
                    return true;
                }
            }
    
            /// <summary>
            /// Prints the page.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="PrintPageEventArgs" /> instance containing the event data.</param>
            private void PrintPageHandler(object sender, PrintPageEventArgs e)
            {
                const int LineHeight = 10;
    
                const string TextFontName = "Courier New";
                const int TextFontSize = 7;
    
                e.HasMorePages = false;
                using (Font textFont = new Font(TextFontName, TextFontSize, FontStyle.Regular))
                using (Font textFontBold = new Font(TextFontName, TextFontSize, FontStyle.Bold))
                {
                    float y = 0;
                    float dpiXRatio = e.Graphics.DpiX / 96f; // 96dpi = 100%
                    float dpiYRatio = e.Graphics.DpiY / 96f; // 96dpi = 100%
    
                    // This calculation isn't exactly the width of the rendered text.
                    // All the calculations occurring in the rendering code of PrintTextLine.  It almost needs to run that code and use e.Graphics.MeasureString()
                    // the first time to get the true contentWidth, then re-run the same logic using the true contentWidth for rendering.
                    //
                    // For now, the rendering is close, but it's not 'exact' center due to the mismatch in estimated vs. true size
                    float contentWidth = this.parts.Where(x => x.TextType == TextType.Text).Select(p => p.Value).Max(str => str.Replace(NormalTextMarker, string.Empty).Replace(BoldTextMarker, string.Empty).Length) * dpiXRatio; // Line with max length = content width
    
                    for (; this.printLine < this.parts.Count; this.printLine++)
                    {
                        var part = this.parts[this.printLine];
    
                        if (part.TextType == TextType.Text)
                        {
                            if (!this.PrintTextLine(e, LineHeight, textFont, textFontBold, dpiYRatio, contentWidth, dpiXRatio, part.Value, ref y))
                            {
                                return;
                            }
                        }
                        else if (part.TextType == TextType.LegacyLogo)
                        {
                            byte[] defaultLogoBytes = this.defaultLogo.GetBytes();
                            if (!DrawBitmapImage(e, defaultLogoBytes, contentWidth, ref y))
                            {
                                return;
                            }
                        }
                        else if (part.TextType == TextType.LogoWithBytes)
                        {
                            byte[] image = TextLogoParser.GetLogoImageBytes(part.Value);
                            if (!DrawBitmapImage(e, image, contentWidth, ref y))
                            {
                                return;
                            }
                        }
                    }
                }
            }
    
            private bool PrintTextLine(PrintPageEventArgs e, int lineHeight, Font textFont, Font textFontBold, float dpiYRatio, float contentWidth, float dpiXRatio, string line, ref float y)
            {
                float x = 0;
                string temp;
                string subString;
                int index, index2;
    
                if (y + lineHeight >= e.MarginBounds.Height)
                {
                    // No more room - advance to next page
                    e.HasMorePages = true;
                    return false;
                }
    
                index = line.IndexOf(BoldTextMarker, StringComparison.Ordinal);
    
                if (index >= 0)
                {
                    // Text line printing with bold Text in it.
                    subString = line;
    
                    while (subString.Length > 0)
                    {
                        index2 = subString.IndexOf(BoldTextMarker, StringComparison.Ordinal);
    
                        if (index2 >= 0)
                        {
                            temp = subString.Substring(0, index2)
                                            .Replace(NormalTextMarker, string.Empty)
                                            .Replace(BoldTextMarker, string.Empty);
                            e.Graphics.DrawString(temp, textFont, Brushes.Black, x + e.MarginBounds.Left, y + e.MarginBounds.Top);
                            x = x + (temp.Length * 6);
    
                            index2 = index2 + 3;
                            subString = subString.Substring(index2, subString.Length - index2);
                            index2 = subString.IndexOf(NormalTextMarker, StringComparison.Ordinal);
    
                            temp = subString.Substring(0, index2)
                                            .Replace(NormalTextMarker, string.Empty)
                                            .Replace(BoldTextMarker, string.Empty);
                            e.Graphics.DrawString(temp, textFontBold, Brushes.Black, x + e.MarginBounds.Left, y + e.MarginBounds.Top);
                            x = x + (temp.Length * 6);
    
                            subString = subString.Substring(index2, subString.Length - index2);
                        }
                        else
                        {
                            subString = subString.Replace(NormalTextMarker, string.Empty).Replace(BoldTextMarker, string.Empty);
                            e.Graphics.DrawString(subString, textFont, Brushes.Black, x + e.MarginBounds.Left, y + e.MarginBounds.Top);
                            subString = string.Empty;
                        }
                    }
                }
                else
                {
                    // Text line printing with no bold Text in it.
                    subString = line.Replace(NormalTextMarker, string.Empty);
    
                    Match barCodeMarkerMatch = Regex.Match(subString, BarCodeRegEx, RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
                    if (barCodeMarkerMatch.Success)
                    {
                        // Get the receiptId
                        subString = barCodeMarkerMatch.Groups[1].ToString();
    
                        using (Image barcodeImage = this.barCode.Create(subString, e.Graphics.DpiX, e.Graphics.DpiY))
                        {
                            if (barcodeImage != null)
                            {
                                float barcodeHeight = barcodeImage.Height / dpiYRatio;
    
                                if (y + barcodeHeight >= e.MarginBounds.Height)
                                {
                                    // No more room - advance to next page
                                    e.HasMorePages = true;
                                    return true;
                                }
    
                                // Render barcode in the center of receipt.
                                e.Graphics.DrawImage(barcodeImage, ((contentWidth - (barcodeImage.Width / dpiXRatio)) / 2) + e.MarginBounds.Left, y + e.MarginBounds.Top);
                                y += barcodeHeight;
                            }
                        }
                    }
                    else
                    {
                        e.Graphics.DrawString(subString, textFont, Brushes.Black, e.MarginBounds.Left, y + e.MarginBounds.Top);
                    }
                }
    
                y = y + lineHeight;
                return true;
            }
        }
    }
}
