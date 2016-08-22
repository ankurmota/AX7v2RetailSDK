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
        using System.Resources;
    
        /// <summary>
        /// ResourceHelper provides utility methods for accessing resource files.
        /// </summary>
        public static class ResourceHelper
        {
            /// <summary>
            /// Way of getting to the resource files.
            /// </summary>
            private static ResourceManager manager = new ResourceManager(typeof(Resources));
    
            /// <summary>
            /// This wrapper makes it a bit easier to get things from the resource file, especially when strings are generated dynamically from a DB. 
            /// </summary>
            /// <param name="str">Resource string to look up.</param>
            /// <returns>Localized String.</returns>
            public static string GetLocalString(string str)
            {
                // we take out any spaces, this allows a more robust lookup if strings returned from controllers have spaces assuming they were 
                // saved in the resource file as the full string minus the spaces. For example:
                // "TV and Video" is the string returned from the controller, if the string is saved in the resource file as "tvandvideo", it will then find it
                // the lookup is case insensitive as well
                if (str != null)
                {
                    string lookupStr = str.Replace(" ", string.Empty);
    
                    manager.IgnoreCase = true;
    
                    string strReturned = manager.GetString(lookupStr);
    
                    if (string.IsNullOrEmpty(strReturned))
                    {
                        return str;
                    }
                    else
                    {
                        return strReturned;
                    }
                }
    
                return str;
            }
        }
    }
}
