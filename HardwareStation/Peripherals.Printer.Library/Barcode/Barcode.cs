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
        using System.Drawing;
    
        /// <summary>
        /// An abstract base class the provides functionality for various barcode encoder classes.
        /// </summary>
        public abstract class Barcode
        {
            /// <summary>
            /// The default dpi for screen.
            /// </summary>
            protected const float DefaultDpi = 96f;
    
            /// <summary>
            /// The default print font name.
            /// </summary>
            protected const string TextFontName = "Courier New";
    
            /// <summary>
            /// The default print font size.
            /// </summary>
            protected const int TextFontSize = 10;
    
            /// <summary>
            /// Gets the font name.
            /// </summary>
            public abstract string FontName { get; }
    
            /// <summary>
            /// Gets the font size.
            /// </summary>
            public abstract int FontSize { get; }
    
            /// <summary>
            /// Encode the text.
            /// </summary>
            /// <param name="text">The text to encode.</param>
            /// <returns>Encoded string.</returns>
            public abstract string Encode(string text);
    
            /// <summary>
            /// Creates a barcode image.
            /// </summary>
            /// <param name="text">The barcode text.</param>
            /// <param name="dpiX">Horizontal resolution.</param>
            /// <param name="dpiY">Vertical resolution.</param>
            /// <returns>
            /// Barcode image. Null if barcode is not created.
            /// </returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Barcode exceptions should not stop printing."), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing returned object")]
            public virtual Image Create(string text, float dpiX = DefaultDpi, float dpiY = DefaultDpi)
            {
                Bitmap barcodeImage = null;
    
                using (Font barcodeFont = new Font(this.FontName, this.FontSize))
                {
                    // If font installed.
                    if (barcodeFont.Name.Equals(barcodeFont.OriginalFontName, StringComparison.Ordinal))
                    {
                        // Text font
                        using (Font barcodeTextFont = new Font(TextFontName, TextFontSize))
                        {
                            try
                            {
                                text = this.Encode(text);
    
                                SizeF barcodeSizeF = GetTextSizeF(text, barcodeFont, dpiX, dpiY);
                                float barcodeTextHeight = barcodeTextFont.GetHeight(dpiY);
    
                                barcodeImage = new Bitmap((int)barcodeSizeF.Width, (int)(barcodeSizeF.Height + barcodeTextHeight));
                                barcodeImage.SetResolution(dpiX, dpiY);
    
                                using (Graphics graphic = Graphics.FromImage(barcodeImage))
                                {
                                    // Calculate left/right margin for drawing barcode considering dpi being used.
                                    float margin = (dpiX / DefaultDpi) * 5;
    
                                    // Draw barcode
                                    graphic.DrawString(text, barcodeFont, Brushes.Black, margin, margin);
    
                                    // Draw text below barcode in center
                                    RectangleF textRect = new RectangleF(0, barcodeSizeF.Height, barcodeSizeF.Width, barcodeTextHeight);
    
                                    using (StringFormat textFormat = new StringFormat(StringFormatFlags.NoClip) { Alignment = StringAlignment.Center })
                                    {
                                        graphic.DrawString(text, barcodeTextFont, Brushes.Black, textRect, textFormat);
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                if (barcodeImage != null)
                                {
                                    barcodeImage.Dispose();
                                }
                            }
                        }
                    }
                }
    
                return barcodeImage;
            }
    
            /// <summary>
            /// Gets the sizeF of a text with given font.
            /// </summary>
            /// <param name="text">The text to size.</param>
            /// <param name="font">The font for the text.</param>
            /// <param name="dpiX">X dpi for target.</param>
            /// <param name="dpiY">Y dpi for target.</param>
            /// <returns>
            /// Size float.
            /// </returns>
            private static SizeF GetTextSizeF(string text, Font font, float dpiX, float dpiY)
            {
                SizeF sizeF;
    
                // Create temporary graphics and calculate the height/width
                using (Bitmap bitmap = new Bitmap(1, 1))
                {
                    bitmap.SetResolution(dpiX, dpiY);
                    using (Graphics graphic = Graphics.FromImage(bitmap))
                    {
                        sizeF = graphic.MeasureString(text, font);
                    }
                }
    
                return sizeF;
            }
        }
    }
}
