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
    namespace Commerce.Runtime.Services.Security
    {
        using Microsoft.Dynamics.Commerce.Runtime;

        /// <summary>
        /// Security context class.
        /// </summary>
        internal static class Secret
        {
            /// <summary>
            /// Security context value.
            /// </summary>
            public const string SecurityContext = "F3252730538F430c98ED738E60C16A3DEB9919639E8D43318A018EC407815D3A";
    
            /// <summary>
            /// Security Registry key value.
            /// </summary>
            public const string SecurityRegKey = "Data2";
        }
    }
}
