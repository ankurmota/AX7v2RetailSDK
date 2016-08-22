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
    namespace Commerce.RetailProxy.Adapters
    {
        using System;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        internal sealed class HttpUtility
        {
            public static HttpValueCollection ParseQueryString(string query)
            {
                if (query == null)
                {
                    throw new ArgumentNullException("query");
                }
    
                if ((query.Length > 0) && (query[0] == '?'))
                {
                    query = query.Substring(1);
                }
    
                return new HttpValueCollection(query, true);
            }
        }
    }
}
