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
    namespace Retail.SampleConnector.PaymentAcceptWeb.Models
    {
        /// <summary>
        /// The custom styles.
        /// </summary>
        public class CustomStyles
        {
            /// <summary>
            /// Gets the default styles.
            /// </summary>
            public static CustomStyles Default
            {
                get
                {
                    return new CustomStyles()
                    {
                        FontSize = "12px",
                        FontFamily = "\"Segoe UI\"",
                        LabelColor = "black",
                        TextBackgroundColor = "white",
                        TextColor = "black",
                        DisabledTextBackgroundColor = "#E4E4E4", // light grey,
                        ColumnNumber = 2,
                    };
                }
            }
    
            /// <summary>
            /// Gets or sets the font size, e.g. <c>12px</c>.
            /// </summary>
            public string FontSize { get; set; }

            /// <summary>
            /// Gets or sets the font family, e.g. <c>"Segoe UI"</c> or <c>Georgia</c>.
            /// </summary>
            public string FontFamily { get; set; }

            /// <summary>
            /// Gets or sets the color of the label, e.g. <c>black</c> or <c>#000000</c> or <c>rgb(0,0,0)</c>.
            /// </summary>
            public string LabelColor { get; set; }

            /// <summary>
            /// Gets or sets the background color of the textbox or the dropdown box, e.g. <c>white</c> or <c>#FFFFFF</c> or <c>rgb(255,255,255)</c>.
            /// </summary>
            public string TextBackgroundColor { get; set; }

            /// <summary>
            /// Gets or sets the background color of the disabled textbox or the disabled dropdown box, e.g. <c>#E4E4E4</c> (light grey).
            /// </summary>
            public string DisabledTextBackgroundColor { get; set; }

            /// <summary>
            /// Gets or sets the color of the text in textbox or the dropdown box, e.g. <c>black</c> or <c>#000000</c> or <c>rgb(0,0,0)</c>.
            /// </summary>
            public string TextColor { get; set; }
    
            /// <summary>
            /// Gets or sets the number of columns, e.g. 1 or 2.
            /// </summary>
            public int ColumnNumber { get; set; }
        }
    }
}
