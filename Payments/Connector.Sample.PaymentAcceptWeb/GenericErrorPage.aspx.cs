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
    namespace Retail.SampleConnector.PaymentAcceptWeb
    {
        using System;
    
        /// <summary>
        /// The error page.
        /// </summary>
        public partial class GenericErrorPage : System.Web.UI.Page
        {
            /// <summary>
            /// Loads the content of the page.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The event data.</param>
            protected void Page_Load(object sender, EventArgs e)
            {
            }
    
            /// <summary>
            /// Initializes the page.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The event data.</param>
            protected void Page_Init(object sender, EventArgs e)
            {
                this.ViewStateUserKey = Session.SessionID;
            }
        }
    }
}
