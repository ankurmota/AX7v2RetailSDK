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
        using Retail.TransactionServices.ClientProxy;
    
        /// <summary>
        /// Interface for transaction service client factory.
        /// </summary>
        public interface ITransactionServiceClientFactory
        {
            /// <summary>
            /// Gets password hash algorithm in channel configuration.
            /// </summary>
            string PasswordHashAlgorithm
            {
                get;
            }
    
            /// <summary>
            /// Creates the <see cref="RequestInfo" /> based on the transaction service profile from database.
            /// </summary>
            /// <returns>The <see cref="RequestInfo"/> instance.</returns>
            RetailTransactionServiceRequestInfo CreateRequestInfo();
    
            /// <summary>
            /// Creates the transaction service client instance for common services.
            /// </summary>
            /// <returns>The common services client instance.</returns>
            RetailRealTimeServiceContractChannel CreateTransactionServiceClient();
    
            /// <summary>
            /// Request to refresh the transaction service channel factory (load the latest configurations).
            /// </summary>
            /// <remarks>
            /// Refreshing the channel factory resets connection for all the open clients. Use it with caution in multi-threaded environment.
            /// </remarks>
            void Refresh();
        }
    }
}
