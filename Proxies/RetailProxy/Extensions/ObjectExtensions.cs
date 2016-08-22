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
    namespace Commerce.RetailProxy
    {
        using Microsoft.Dynamics.Commerce.Runtime;
        using Newtonsoft.Json;
    
        /// <summary>
        /// Encapsulates functionality used to extend the <see cref="System.Object"/> class.
        /// </summary>
        internal static class ObjectExtensions
        {
            /// <summary>
            /// Serializes the object to JSON.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns>The JSON string.</returns>
            public static string SerializeToJsonObject(this object source)
            {
                return JsonConvert.SerializeObject(
                    source,
                    new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.None,
                        NullValueHandling = NullValueHandling.Ignore
                    });
            }
        }
    }
}
