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
    namespace Retail.Ecommerce.Web.Storefront.Models
    {
        using System;
    
        /// <summary>
        /// CustomLink provides a utility for making fully formed html links for use in navigational hierarchies primarily.
        /// </summary>
        public class CustomLink
        {
            /// <summary>
            ///  Url to route to.
            /// </summary>
            private string url;
    
            /// <summary>
            /// Text to show user.
            /// </summary>
            private string text;
    
            /// <summary>
            /// Html element id.
            /// </summary>
            private string id;
    
            /// <summary>
            /// CSS styles to apply (will be added inline with a style = "..." ).
            /// </summary>
            private string style;
    
            /// <summary>
            /// If you want to add any html before the text, could be good for adding things like images.
            /// </summary>
            private string htmlToAddBefore;
    
            /// <summary>
            /// If you want to add any html after the text, could be good for adding things like images.
            /// </summary>
            private string htmlToAddAfter;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="CustomLink" /> class.
            /// </summary>
            /// <param name="url">Url to route to.</param>
            /// <param name="text">Text to show user.</param>
            /// <param name="id">Html element id.</param>
            /// <param name="style">CSS styles to apply (will be added inline with a style = "..." ).</param>
            /// <param name="htmlToAddBefore">If you want to add any html before the text, could be good for adding things like images.</param>
            /// <param name="htmlToAddAfter">If you want to add any html after the text, could be good for adding things like images.</param>
            public CustomLink(string url, string text, string id = "", string style = "", string htmlToAddBefore = "", string htmlToAddAfter = "")
            {
                this.url = url;
                this.text = text;
                this.id = id;
                this.style = style;
                this.htmlToAddBefore = htmlToAddBefore;
                this.htmlToAddAfter = htmlToAddAfter;
            }
    
            /// <summary>
            /// Custom implementation that returns the hyperlink element based on the link properties provided.
            /// </summary>
            /// <returns>Returns an anchor HREF html element based on the link properties provided.</returns>
            public override string ToString()
            {
                string htmlElement = this.htmlToAddBefore;
                htmlElement += "<a href=\"" + this.url + "\" id=\"" + this.id + "\" style=\"" + this.style + "\">" + this.text + "</a>";
                htmlElement += this.htmlToAddAfter;
    
                return htmlElement;
            }
        }
    }
}
