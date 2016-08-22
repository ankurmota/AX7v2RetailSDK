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

        internal enum ActionExecutionPattern
        {
            /// <summary>
            /// The value indicating the action should only be executed towards online context.
            /// </summary>
            Online = 1,
    
            /// <summary>
            /// The value indicating the action should be executed towards online context first, when failed, switch to offline.
            /// </summary>
            SeamlessOnlineOffline,
    
            /// <summary>
            /// The value indicating the action should only be executed towards offline context.
            /// </summary>
            Offline,
    
            /// <summary>
            /// The value indicating the action should be executed towards both online and offline.
            /// </summary>
            OnlineAndOffline
        }
    }
}
