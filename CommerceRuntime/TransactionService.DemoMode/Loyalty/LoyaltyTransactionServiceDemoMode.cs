/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1403:FileMayOnlyContainASingleNamespace", Justification = "This file requires multiple namespaces to support the Retail Sdk code generation.")]

namespace Contoso
{
    namespace Commerce.Runtime.TransactionService
    {
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
    
        /// <summary>
        /// Loyalty demo mode transaction service.
        /// </summary>
        public class LoyaltyTransactionServiceDemoMode : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(IssueLoyaltyCardRealtimeRequest),
                        typeof(GetLoyaltyCardTransactionsRealtimeRequest),
                        typeof(GetLoyaltyCardRewardPointsStatusRealtimeRequest),
                        typeof(PostLoyaltyCardRewardPointRealtimeRequest),
                    };
                }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
    
                Response response;
                if (requestType == typeof(IssueLoyaltyCardRealtimeRequest))
                {
                    response = IssueLoyaltyCard();
                }
                else if (requestType == typeof(GetLoyaltyCardTransactionsRealtimeRequest))
                {
                    response = GetLoyaltyCard();
                }
                else if (requestType == typeof(GetLoyaltyCardRewardPointsStatusRealtimeRequest))
                {
                    response = GetLoyaltyCardRewardPointsStatus();
                }
                else if (requestType == typeof(PostLoyaltyCardRewardPointRealtimeRequest))
                {
                    response = PostLoyaltyCardRewardPointTransaction();
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request type '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Issues loyalty card from AX.
            /// </summary>
            /// <returns>The issue loyalty card response.</returns>
            private static IssueLoyaltyCardRealtimeResponse IssueLoyaltyCard()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "IssueLoyaltyCard is not supported in demo mode.");
            }
    
            /// <summary>
            /// Gets loyalty card transactions from AX.
            /// </summary>
            /// <returns>The get loyalty card transactions response.</returns>
            private static GetLoyaltyCardTransactionsRealtimeResponse GetLoyaltyCard()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "GetLoyaltyCard is not supported in demo mode.");
            }
    
            /// <summary>
            /// Gets loyalty card reward points status from AX.
            /// </summary>
            /// <returns>The get loyalty card reward points status response.</returns>
            private static EntityDataServiceResponse<LoyaltyCard> GetLoyaltyCardRewardPointsStatus()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "GetLoyaltyCardRewardPointsStatus is not supported in demo mode.");
            }
    
            /// <summary>
            /// Posts loyalty card reward point transaction in AX.
            /// </summary>
            /// <returns>A loyalty card reward point transaction response.</returns>
            private static NullResponse PostLoyaltyCardRewardPointTransaction()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "PostLoyaltyCardRewardPointTransaction is not supported in demo mode.");
            }
        }
    }
}
