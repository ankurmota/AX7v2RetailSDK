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
    namespace Commerce.Runtime.TransactionService
    {
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Transaction Service Commerce Runtime Client APIs.
        /// </summary>
        public sealed partial class TransactionServiceClient
        {
            // Transaction service method names.
            private const string UpdateChannelPublishingStatusMethodName = "UpdateChannelPublishingStatus";
    
            /// <summary>
            /// Updates the publishing status and message for the given channel.
            /// </summary>
            /// <param name="channelId">The channel identifier.</param>
            /// <param name="publishingStatus">The channel publishing status.</param>
            /// <param name="publishingStatusMessage">The channel publishing status message.</param>
            public void UpdateChannelPublishingStatus(long channelId, OnlineChannelPublishStatusType publishingStatus, string publishingStatusMessage)
            {
                this.InvokeMethodNoDataReturn(
                    UpdateChannelPublishingStatusMethodName,
                    new object[] { channelId, (int)publishingStatus, publishingStatusMessage });
            }
        }
    }
}
